/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto reconciliation - run (match, save, reconcile ready lines), counts, paged results and decline
 * Chronological  : Development
 * Design         : VA012 Auto Reconciliation Solution Approach v4
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    public class AutoReconcileModel
    {
        public const int PageSize = 50;
        private const int InListChunk = 500;
        /// <summary>The document types, in the order an all-types run tries them.</summary>
        private static readonly string[] DocTypes = { "PY", "IS", "PO", "CO" };
        /// <summary>VA012_DocType of a run over every type (no type chosen).</summary>
        public const string AllTypes = "AL";
        private static readonly VLogger log = VLogger.GetVLogger(typeof(AutoReconcileModel).FullName);

        /// <summary>
        /// Runs the standard formula on the open lines of one statement and stores the result as a match run with one
        /// suggestion (S) or no-match row (N) per line. A run without filters first deletes this statement's earlier
        /// suggested and no-match rows with their documents, which frees their reservations. With a payment method or
        /// partner filter only lines that get a new suggestion are touched. Everything happens in one transaction.
        /// docType AL (no type chosen) tries every type in turn, payments first, then invoice schedules, prepay orders and
        /// contra; a line one type matches is not tried by the next, a line without a match goes on to it.
        /// </summary>
        public Dictionary<string, object> Run(Ctx ctx, int bankAccountId, int statementId, string docType, int payMethodId, int bpId)
        {
            var result = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(docType)) docType = AllTypes;
            if (docType != AllTypes && Array.IndexOf(DocTypes, docType) < 0) { result["error"] = Msg.GetMsg(ctx, "VA012_ARChooseType"); return result; }
            if (docType == "CO") { payMethodId = 0; bpId = 0; }                 // the filters do not apply to contra
            bool filtered = payMethodId > 0 || bpId > 0;
            string[] passes = docType == AllTypes ? DocTypes : new[] { docType };
            Stopwatch watch = Stopwatch.StartNew();
            long loadMs = 0, matchMs = 0;
            int docCount = 0;
            Trx trx = Trx.GetTrx(Trx.CreateTrxName("VA012AR"));
            try
            {
                var loader = new AutoReconcileCandidates(ctx, trx);
                ArRunInfo info = loader.GetRunInfo(bankAccountId, statementId);
                ReleaseUnmatched(ctx, info.StatementId, trx);
                if (!filtered) DeleteReplaced(ctx, info.StatementId, null, trx);
                List<ArLine> lines = loader.LoadLines(info);
                List<string> declined = loader.LoadDeclinedPairs(info);
                loadMs = watch.ElapsedMilliseconds;

                var results = new List<ArResult>();
                var noMatch = new Dictionary<int, ArResult>();                     // line -> its no-match result so far
                List<ArLine> open = lines;
                foreach (string pass in passes)
                {
                    if (open.Count == 0) break;
                    long passStart = watch.ElapsedMilliseconds;
                    bool contra = pass == "CO";
                    List<ArDoc> docs = loader.LoadDocs(info, pass, contra ? 0 : payMethodId, contra ? 0 : bpId, open);
                    docCount += docs.Count;
                    long loaded = watch.ElapsedMilliseconds;
                    loadMs += loaded - passStart;
                    foreach (ArResult r in new AutoReconcileMatcher(pass, docs, declined, info.Settings).Match(open))
                    {
                        r.DocType = pass;
                        if (r.Status == "S") { results.Add(r); noMatch.Remove(r.Line.LineId); continue; }
                        // keep the first no-match that has candidates to show; a later type only replaces one without
                        ArResult earlier;
                        if (!noMatch.TryGetValue(r.Line.LineId, out earlier) || (earlier.Candidates.Count == 0 && r.Candidates.Count > 0))
                            noMatch[r.Line.LineId] = r;
                    }
                    matchMs += watch.ElapsedMilliseconds - loaded;
                    var matched = new HashSet<int>(results.Select(r => r.Line.LineId));
                    open = open.Where(l => !matched.Contains(l.LineId)).ToList();
                }
                results.AddRange(noMatch.Values);
                if (passes.Length > 1)
                    foreach (ArResult r in noMatch.Values) r.Reason = AllTypesReason(ctx, r.Reason);
                if (filtered)
                {
                    results = results.Where(r => r.Status == "S").ToList();
                    DeleteReplaced(ctx, info.StatementId, results.Select(r => r.Line.LineId).ToList(), trx);
                }

                int runId = SaveRun(ctx, info, docType, payMethodId, bpId, trx);
                new SuggestionWriter(ctx, info, runId, trx).WriteAll(results);
                if (!trx.Commit()) throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                long saveMs = watch.ElapsedMilliseconds - loadMs - matchMs;

                // automatic reconciliation: each ready line is saved through the form's own save, one line at a time
                int reconciled = 0, notReconciled = 0;
                List<ArResult> ready = results.Where(r => r.Status == "S" && r.IsAuto && r.SuggestionId > 0).ToList();
                if (ready.Count > 0)
                {
                    var posters = new Dictionary<string, AutoReconcilePoster>();       // one per document type of the run
                    foreach (ArResult r in ready)
                    {
                        AutoReconcilePoster poster;
                        if (!posters.TryGetValue(r.DocType, out poster)) posters[r.DocType] = poster = new AutoReconcilePoster(ctx, info, r.DocType);
                        string why;
                        try { why = poster.Post(r); }
                        catch (Exception pe) { log.Log(Level.SEVERE, "VA012 auto reconcile line " + r.Line.LineId, pe); why = pe.Message; }
                        if (why == null) { SetStatus(r.SuggestionId, "C", null); reconciled++; }
                        else { SetStatus(r.SuggestionId, "S", why); notReconciled++; }
                    }
                }
                long postMs = watch.ElapsedMilliseconds - loadMs - matchMs - saveMs;

                result["runId"] = runId;
                result["counts"] = GetCounts(ctx, info.StatementId);            // every line of the statement, not only this run's
                result["reconciled"] = reconciled;
                result["notReconciled"] = notReconciled;
                result["dateWindow"] = info.Settings.DateWindow;
                result["docType"] = docType;
                result["defaults"] = new Dictionary<string, int> { { "chargeId", info.Settings.ChargeId }, { "taxId", info.Settings.TaxId } };
                result["hasRules"] = info.Settings.Rules.Any(x => passes.Any(p => x.AppliesTo.Contains(p)));
                result["lineCount"] = lines.Count;
                result["docCount"] = docCount;
                result["timings"] = new Dictionary<string, long> { { "load", loadMs }, { "match", matchMs }, { "save", saveMs }, { "post", postMs } };
                log.Info("VA012 auto reconcile run " + runId + " (" + docType + "): " + lines.Count + " lines, " + docCount + " documents, " + reconciled
                    + " reconciled, load " + loadMs + " ms, match " + matchMs + " ms, save " + saveMs + " ms, post " + postMs + " ms");
            }
            catch (Exception e)
            {
                trx.Rollback();
                log.Log(Level.SEVERE, "VA012 auto reconcile", e);
                string msg = Msg.GetMsg(ctx, e.Message);
                result["error"] = string.IsNullOrEmpty(msg) ? e.Message : msg;
            }
            finally
            {
                trx.Close();
            }
            return result;
        }

        /// <summary>Where one statement line stands: its current auto reconcile row (0 when it has none) and its status.</summary>
        private sealed class LineState
        {
            public int LineId;
            public int SuggestionId;
            /// <summary>
            /// A accepted, C reconciled automatically, M reconciled outside auto reconcile (the form or Match Statement),
            /// S suggested, N no match, D declined, O open and not run yet.
            /// </summary>
            public string Status;
            public bool IsAuto;
        }

        /// <summary>
        /// Every active line of the statement with its status, in line order (two queries). A line with a payment or cash
        /// line, or confirmed, is reconciled (as a run reads it), with the row that reconciled it when auto reconcile did.
        /// An open line shows its live suggestion, else its latest no-match or declined row, else that no run covered it.
        /// </summary>
        private static List<LineState> LoadLineStates(Ctx ctx, int statementId)
        {
            string ssql = "SELECT MS.VA012_MatchSuggestion_ID, MS.C_BankStatementLine_ID, MS.VA012_MatchStatus, MS.VA012_IsAuto"
                + " FROM VA012_MatchSuggestion MS INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)"
                + " WHERE MS.IsActive = 'Y' AND BSL.C_BankStatement_ID = @stmt AND MS.VA012_MatchStatus IN ('S', 'A', 'C', 'N', 'D')";
            ssql = MRole.GetDefault(ctx).AddAccessSQL(ssql, "MS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            var byLine = new Dictionary<int, List<DataRow>>();
            DataSet sds = DB.ExecuteDataset(ssql, new[] { new SqlParameter("@stmt", statementId) }, null);
            if (sds != null && sds.Tables.Count > 0)
                foreach (DataRow r in sds.Tables[0].Rows)
                {
                    int lineId = Util.GetValueOfInt(r["C_BankStatementLine_ID"]);
                    List<DataRow> list;
                    if (!byLine.TryGetValue(lineId, out list)) byLine[lineId] = list = new List<DataRow>();
                    list.Add(r);
                }

            string lsql = "SELECT BSL.C_BankStatementLine_ID, BSL.C_Payment_ID, BSL.C_CashLine_ID, BSL.VA012_IsMatchingConfirmed"
                + " FROM C_BankStatementLine BSL WHERE BSL.IsActive = 'Y' AND BSL.C_BankStatement_ID = @stmt";
            lsql = MRole.GetDefault(ctx).AddAccessSQL(lsql, "BSL", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY BSL.Line, BSL.C_BankStatementLine_ID";
            DataSet lds = DB.ExecuteDataset(lsql, new[] { new SqlParameter("@stmt", statementId) }, null);
            var states = new List<LineState>();
            if (lds == null || lds.Tables.Count == 0) return states;
            foreach (DataRow r in lds.Tables[0].Rows)
            {
                int lineId = Util.GetValueOfInt(r["C_BankStatementLine_ID"]);
                bool reconciled = Util.GetValueOfInt(r["C_Payment_ID"]) > 0 || Util.GetValueOfInt(r["C_CashLine_ID"]) > 0
                    || Util.GetValueOfString(r["VA012_IsMatchingConfirmed"]) == "Y";
                List<DataRow> list;
                byLine.TryGetValue(lineId, out list);
                List<DataRow> latest = (list ?? new List<DataRow>()).OrderByDescending(x => Util.GetValueOfInt(x["VA012_MatchSuggestion_ID"])).ToList();
                Func<string, DataRow> pick = codes => latest.FirstOrDefault(x => codes.Contains(Util.GetValueOfString(x["VA012_MatchStatus"])));
                DataRow row = reconciled ? pick("AC") : (pick("S") ?? pick("ND"));
                states.Add(new LineState
                {
                    LineId = lineId,
                    SuggestionId = row == null ? 0 : Util.GetValueOfInt(row["VA012_MatchSuggestion_ID"]),
                    Status = row != null ? Util.GetValueOfString(row["VA012_MatchStatus"]) : reconciled ? "M" : "O",
                    IsAuto = row != null && Util.GetValueOfString(row["VA012_IsAuto"]) == "Y"
                });
            }
            return states;
        }

        /// <summary>
        /// filter: "" all, "C" reconciled (A, C, M), "S" suggested, "AU" suggested and ready for automatic reconciliation,
        /// "N" no match and declined (both wait for a manual match: a document dragged onto the line), "D", "O".
        /// </summary>
        private static bool InFilter(LineState s, string filter)
        {
            switch (filter ?? "")
            {
                case "": return true;
                case "C": return s.Status == "A" || s.Status == "C" || s.Status == "M";
                case "AU": return s.Status == "S" && s.IsAuto;
                case "N": return s.Status == "N" || s.Status == "D";
                default: return s.Status == filter;
            }
        }

        /// <summary>Counts per status of every line of the statement, for the toolbar.</summary>
        public Dictionary<string, int> GetCounts(Ctx ctx, int statementId)
        {
            List<LineState> states = LoadLineStates(ctx, statementId);
            var counts = new Dictionary<string, int> { { "total", states.Count }, { "reconciled", 0 }, { "suggested", 0 }, { "auto", 0 }, { "noMatch", 0 }, { "declined", 0 }, { "open", 0 } };
            foreach (LineState s in states)
            {
                counts[InFilter(s, "C") ? "reconciled" : s.Status == "S" ? "suggested" : s.Status == "D" ? "declined" : s.Status == "N" ? "noMatch" : "open"]++;
                if (InFilter(s, "AU")) counts["auto"]++;
            }
            counts["noMatch"] += counts["declined"];                              // No match lists the declined lines too
            return counts;
        }

        /// <summary>What the page needs when a statement is chosen: the counts, and the date window and default charge and tax of the account's rules header.</summary>
        public Dictionary<string, object> GetStatementView(Ctx ctx, int bankAccountId, int statementId)
        {
            var result = new Dictionary<string, object>();
            try
            {
                ArRunInfo info = new AutoReconcileCandidates(ctx, null).GetRunInfo(bankAccountId, statementId);
                result["counts"] = GetCounts(ctx, info.StatementId);
                result["dateWindow"] = info.Settings.DateWindow;
                result["defaults"] = new Dictionary<string, int> { { "chargeId", info.Settings.ChargeId }, { "taxId", info.Settings.TaxId } };
                result["statementDate"] = FormatDate(info.StatementDate);          // the document list loads as the form's does for this date
            }
            catch (Exception e)
            {
                log.Log(Level.SEVERE, "VA012 auto reconcile statement", e);
                string msg = Msg.GetMsg(ctx, e.Message);
                result["error"] = string.IsNullOrEmpty(msg) ? e.Message : msg;
            }
            return result;
        }

        /// <summary>
        /// One page of the statement's lines with their status, in line order: the line states (two light queries), then for
        /// the page only the lines' auto reconcile rows with their documents, and the other lines with what reconciled them.
        /// </summary>
        public List<Dictionary<string, object>> GetPage(Ctx ctx, int statementId, string filter, int pageNo)
        {
            var rows = new List<Dictionary<string, object>>();
            List<LineState> page = LoadLineStates(ctx, statementId).Where(s => InFilter(s, filter))
                .Skip((Math.Max(1, pageNo) - 1) * PageSize).Take(PageSize).ToList();
            if (page.Count == 0) return rows;
            Dictionary<int, Dictionary<string, object>> bySuggestion = LoadSuggestionRows(ctx, statementId, page.Where(s => s.SuggestionId > 0).Select(s => s.SuggestionId).ToList());
            Dictionary<int, Dictionary<string, object>> byLine = LoadPlainLineRows(ctx, page.Where(s => s.SuggestionId == 0).ToList());
            foreach (LineState s in page)
            {
                Dictionary<string, object> row;
                if (s.SuggestionId > 0 ? bySuggestion.TryGetValue(s.SuggestionId, out row) : byLine.TryGetValue(s.LineId, out row)) rows.Add(row);
            }
            return rows;
        }

        /// <summary>The statement line's own fields, the same for every row of the list.</summary>
        private static Dictionary<string, object> LineFields(DataRow r)
        {
            return new Dictionary<string, object>
            {
                { "lineId", Util.GetValueOfInt(r["C_BankStatementLine_ID"]) },
                { "lineNo", Util.GetValueOfInt(r["Line"]) },
                { "date", FormatDate(Util.GetValueOfDateTime(r["StatementLineDate"])) },
                { "amount", Util.GetValueOfDecimal(r["StmtAmt"]) },
                { "narration", string.Join(" ", new[] { Util.GetValueOfString(r["Description"]), Util.GetValueOfString(r["Memo"]) }.Where(s => !string.IsNullOrWhiteSpace(s))) },
                { "trxNo", Util.GetValueOfString(r["TrxNo"]) },
                { "chequeNo", Util.GetValueOfString(r["EftCheckNo"]) },
                { "bpName", Util.GetValueOfString(r["BPName"]) },
                { "payMethod", Util.GetValueOfString(r["PayMethod"]) }
            };
        }

        private const string LineColumns = "BSL.C_BankStatementLine_ID, BSL.Line, BSL.StatementLineDate, BSL.StmtAmt, BSL.Description, BSL.Memo,"
            + " BSL.TrxNo, BSL.EftCheckNo, BP.Name AS BPName, PM.VA009_Name AS PayMethod";
        private const string LineJoins = " LEFT OUTER JOIN C_BPartner BP ON (BP.C_BPartner_ID = BSL.C_BPartner_ID)"
            + " LEFT OUTER JOIN VA009_PaymentMethod PM ON (PM.VA009_PaymentMethod_ID = BSL.VA009_PaymentMethod_ID)";

        /// <summary>
        /// Lines that have no auto reconcile row: reconciled in the form or by Match Statement (M), with the payment, cash
        /// line or charge they were reconciled with, or open and not run yet (O). Their key is minus the line id.
        /// </summary>
        private static Dictionary<int, Dictionary<string, object>> LoadPlainLineRows(Ctx ctx, List<LineState> states)
        {
            var rows = new Dictionary<int, Dictionary<string, object>>();
            if (states.Count == 0) return rows;
            List<int> ids = states.Select(s => s.LineId).ToList();
            Dictionary<int, string> status = states.ToDictionary(s => s.LineId, s => s.Status);
            string sql = "SELECT " + LineColumns + ", PAY.DocumentNo AS PayDocNo, PAY.DateAcct AS PayDate, PAY.PayAmt, PAY.IsReceipt, PAY.CheckNo AS PayCheckNo,"
                + " PAY.TrxNo AS PayTrxNo, BPP.Name AS PayBPName, CS.DocumentNo AS CashDocNo, CS.DateAcct AS CashDate, CL.Amount AS CashAmt, BPC.Name AS CashBPName,"
                + " CH.Name AS ChargeName"
                + " FROM C_BankStatementLine BSL" + LineJoins
                + " LEFT OUTER JOIN C_Payment PAY ON (PAY.C_Payment_ID = BSL.C_Payment_ID)"
                + " LEFT OUTER JOIN C_BPartner BPP ON (BPP.C_BPartner_ID = PAY.C_BPartner_ID)"
                + " LEFT OUTER JOIN C_CashLine CL ON (CL.C_CashLine_ID = BSL.C_CashLine_ID)"
                + " LEFT OUTER JOIN C_Cash CS ON (CS.C_Cash_ID = CL.C_Cash_ID)"
                + " LEFT OUTER JOIN C_BPartner BPC ON (BPC.C_BPartner_ID = CL.C_BPartner_ID)"
                + " LEFT OUTER JOIN C_Charge CH ON (CH.C_Charge_ID = BSL.C_Charge_ID)"
                + " WHERE BSL.IsActive = 'Y' AND BSL.C_BankStatementLine_ID IN (" + InList(ids, "l") + ")";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "BSL", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, ids.Select((id, i) => new SqlParameter("@l" + i.ToString(CultureInfo.InvariantCulture), id)).ToArray(), null);
            if (ds == null || ds.Tables.Count == 0) return rows;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                Dictionary<string, object> row = LineFields(r);
                int lineId = (int)row["lineId"];
                string st;
                if (!status.TryGetValue(lineId, out st)) continue;
                var docs = new List<Dictionary<string, object>>();
                if (st == "M")
                {
                    // what the line was reconciled with; a payment's amount carries the statement's sign (out = minus)
                    if (!string.IsNullOrEmpty(Util.GetValueOfString(r["PayDocNo"])))
                        docs.Add(PlainDoc("PY", Util.GetValueOfString(r["PayDocNo"]), Util.GetValueOfString(r["PayBPName"]),
                            Util.GetValueOfDecimal(r["PayAmt"]) * (Util.GetValueOfString(r["IsReceipt"]) == "N" ? -1 : 1),
                            Util.GetValueOfDateTime(r["PayDate"]), Util.GetValueOfString(r["PayCheckNo"]), Util.GetValueOfString(r["PayTrxNo"])));
                    else if (!string.IsNullOrEmpty(Util.GetValueOfString(r["CashDocNo"])))
                        docs.Add(PlainDoc("CO", Util.GetValueOfString(r["CashDocNo"]), Util.GetValueOfString(r["CashBPName"]),
                            Util.GetValueOfDecimal(r["CashAmt"]), Util.GetValueOfDateTime(r["CashDate"]), "", ""));
                    else if (!string.IsNullOrEmpty(Util.GetValueOfString(r["ChargeName"])))
                        docs.Add(PlainDoc("", Util.GetValueOfString(r["ChargeName"]), "", Util.GetValueOfDecimal(r["StmtAmt"]),
                            Util.GetValueOfDateTime(r["StatementLineDate"]), "", ""));
                }
                row["id"] = -lineId;
                row["status"] = st;
                row["isAuto"] = false;
                row["matchCase"] = "";
                row["reason"] = "";
                row["docType"] = "";
                row["docs"] = docs;
                rows[lineId] = row;
            }
            return rows;
        }

        private static Dictionary<string, object> PlainDoc(string kind, string docNo, string bpName, decimal amount, DateTime? date, string chequeNo, string trxNo)
        {
            return new Dictionary<string, object>
            {
                { "docRowId", 0 }, { "kind", kind }, { "docNo", docNo }, { "bpName", bpName }, { "amount", amount }, { "date", FormatDate(date) },
                { "trxNo", trxNo }, { "chequeNo", chequeNo }, { "chequeDate", "" }, { "reserved", true }, { "currency", "" }, { "foreign", false },
                { "conversionTypeId", 0 }, { "payMethodId", 0 }
            };
        }

        /// <summary>The lines' auto reconcile rows with their documents (two queries), keyed by suggestion.</summary>
        private static Dictionary<int, Dictionary<string, object>> LoadSuggestionRows(Ctx ctx, int statementId, List<int> suggestionIds)
        {
            var byId = new Dictionary<int, Dictionary<string, object>>();
            if (suggestionIds.Count == 0) return byId;
            string sql = "SELECT MS.VA012_MatchSuggestion_ID, MS.VA012_MatchCase, MS.VA012_MatchStatus, MS.VA012_IsAuto, MS.VA012_Reason, MS.VA012_DifferenceType,"
                + " MS.VA009_PaymentMethod_ID, MS.C_ConversionType_ID, MS.VA012_ChequeNo, MS.VA012_SignalSource, " + LineColumns + ", MR.Name AS RuleName,"
                + " RN.VA012_DocType AS RunDocType"
                + " FROM VA012_MatchSuggestion MS"
                + " INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)" + LineJoins
                + " LEFT OUTER JOIN VA012_MatchRule MR ON (MR.VA012_MatchRule_ID = MS.VA012_MatchRule_ID)"
                + " LEFT OUTER JOIN VA012_MatchRun RN ON (RN.VA012_MatchRun_ID = MS.VA012_MatchRun_ID)"
                + " WHERE MS.IsActive = 'Y' AND MS.VA012_MatchSuggestion_ID IN (" + InList(suggestionIds, "s") + ")";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, suggestionIds.Select((id, i) => new SqlParameter("@s" + i.ToString(CultureInfo.InvariantCulture), id)).ToArray(), null);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return byId;

            foreach (DataRow r in ds.Tables[0].Rows)
            {
                int id = Util.GetValueOfInt(r["VA012_MatchSuggestion_ID"]);
                Dictionary<string, object> row = LineFields(r);
                row["id"] = id;
                row["matchCase"] = Util.GetValueOfString(r["VA012_MatchCase"]);
                row["status"] = Util.GetValueOfString(r["VA012_MatchStatus"]);
                row["isAuto"] = Util.GetValueOfString(r["VA012_IsAuto"]) == "Y";
                row["reason"] = Util.GetValueOfString(r["VA012_Reason"]);
                row["signal"] = Util.GetValueOfString(r["VA012_SignalSource"]);
                row["ruleName"] = Util.GetValueOfString(r["RuleName"]);
                row["docType"] = Util.GetValueOfString(r["RunDocType"]);
                row["differenceType"] = Util.GetValueOfString(r["VA012_DifferenceType"]);
                row["payMethodId"] = Util.GetValueOfInt(r["VA009_PaymentMethod_ID"]);
                row["conversionTypeId"] = Util.GetValueOfInt(r["C_ConversionType_ID"]);
                row["enteredCheque"] = Util.GetValueOfString(r["VA012_ChequeNo"]);
                row["docs"] = new List<Dictionary<string, object>>();
                byId[id] = row;
            }
            int bankCurrencyId = Util.GetValueOfInt(DB.ExecuteScalar("SELECT BA.C_Currency_ID FROM C_BankStatement BS"
                + " INNER JOIN C_BankAccount BA ON (BA.C_BankAccount_ID = BS.C_BankAccount_ID) WHERE BS.C_BankStatement_ID = @stmt",
                new[] { new SqlParameter("@stmt", statementId) }, null));
            foreach (DataRow r in LoadDocsFor(ctx, byId.Keys.ToList()))
            {
                Dictionary<string, object> row;
                if (!byId.TryGetValue(Util.GetValueOfInt(r["VA012_MatchSuggestion_ID"]), out row)) continue;
                int currencyId = Util.GetValueOfInt(r["CurrencyId"]);
                string kind = Util.GetValueOfInt(r["C_Payment_ID"]) > 0 ? "PY" : Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]) > 0 ? "IS"
                    : Util.GetValueOfInt(r["C_Order_ID"]) > 0 ? "PO" : "CO";
                ((List<Dictionary<string, object>>)row["docs"]).Add(new Dictionary<string, object>
                {
                    { "docRowId", Util.GetValueOfInt(r["VA012_MatchSuggestionDoc_ID"]) },
                    { "kind", kind },
                    { "docNo", Util.GetValueOfString(r["DocumentNo"]) },
                    { "bpName", Util.GetValueOfString(r["BPName"]) },
                    { "amount", Util.GetValueOfDecimal(r["VA012_Amount"]) },
                    { "date", FormatDate(Util.GetValueOfDateTime(r["DocDate"])) },
                    { "trxNo", Util.GetValueOfString(r["PayTrxNo"]) },
                    { "chequeNo", Util.GetValueOfString(r["ChequeNo"]) },
                    { "chequeDate", FormatDate(Util.GetValueOfDateTime(r["PayChequeDate"])) },
                    { "reserved", Util.GetValueOfString(r["VA012_IsReserved"]) == "Y" },
                    { "currency", Util.GetValueOfString(r["CurrencyCode"]) },
                    { "foreign", currencyId > 0 && bankCurrencyId > 0 && currencyId != bankCurrencyId },
                    { "conversionTypeId", Util.GetValueOfInt(r["ConversionTypeId"]) },
                    { "payMethodId", Util.GetValueOfInt(r["DocPayMethodId"]) }
                });
            }
            return byId;
        }

        /// <summary>
        /// Declines suggested lines: they become D, the alternatives shown next to the suggestion are deleted, the declined
        /// document is freed and the next run skips the pair. A tie has no suggested document: its candidates are what the
        /// user declined, so they stay.
        /// </summary>
        public Dictionary<string, object> Decline(Ctx ctx, string suggestionIds)
        {
            var result = new Dictionary<string, object>();
            List<int> ids = (suggestionIds ?? "").Split(',').Select(s => Util.GetValueOfInt(s.Trim())).Where(i => i > 0).Distinct().ToList();
            if (ids.Count == 0) { result["declined"] = 0; return result; }
            Trx trx = Trx.GetTrx(Trx.CreateTrxName("VA012ARD"));
            try
            {
                int declined = 0;
                for (int start = 0; start < ids.Count; start += InListChunk)
                {
                    List<int> chunk = ids.Skip(start).Take(InListChunk).ToList();
                    string inList = InList(chunk, "s");
                    if (DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestionDoc WHERE COALESCE(VA012_IsReserved, 'N') = 'N'"
                        + " AND VA012_MatchSuggestion_ID IN (SELECT MS.VA012_MatchSuggestion_ID FROM VA012_MatchSuggestion MS"
                        + " WHERE MS.VA012_MatchStatus = 'S' AND MS.AD_Client_ID = @client AND MS.VA012_MatchSuggestion_ID IN (" + inList + ")"
                        + " AND EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSR WHERE MSR.VA012_MatchSuggestion_ID = MS.VA012_MatchSuggestion_ID"
                        + " AND MSR.VA012_IsReserved = 'Y'))",
                        InParams(ctx, chunk, "s"), trx) < 0)
                        throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                    if (DB.ExecuteQuery("UPDATE VA012_MatchSuggestionDoc SET VA012_IsReserved = 'N', VA012_ReserveKey = NULL"
                        + " WHERE VA012_MatchSuggestion_ID IN (SELECT MS.VA012_MatchSuggestion_ID FROM VA012_MatchSuggestion MS"
                        + " WHERE MS.VA012_MatchStatus = 'S' AND MS.AD_Client_ID = @client AND MS.VA012_MatchSuggestion_ID IN (" + inList + "))",
                        InParams(ctx, chunk, "s"), trx) < 0)
                        throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                    int n = DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'D', VA012_LiveLine = NULL"
                        + " WHERE VA012_MatchStatus = 'S' AND AD_Client_ID = @client AND VA012_MatchSuggestion_ID IN (" + inList + ")",
                        InParams(ctx, chunk, "s"), trx);
                    if (n < 0) throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                    declined += n;
                }
                if (!trx.Commit()) throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                result["declined"] = declined;
            }
            catch (Exception e)
            {
                trx.Rollback();
                log.Log(Level.SEVERE, "VA012 auto reconcile decline", e);
                result["error"] = e.Message;
            }
            finally
            {
                trx.Close();
            }
            return result;
        }

        /// <summary>
        /// Drag and drop on the page: a document dropped on a line no run matched (N) makes the line suggested with that
        /// document (case DR), so Accept reconciles it as for any suggestion, with the same checks and difference handling.
        /// Dropped on a line matched this way, an invoice schedule joins the line's schedules and a prepay order its orders,
        /// as the form allows; any other document replaces the line's dragged document. The document is
        /// read with the run's query for its type: open, not held by another line, amount in the bank's currency.
        /// Returns the line's row as the page shows it.
        /// </summary>
        public Dictionary<string, object> AssignDocument(Ctx ctx, int suggestionId, string kind, int docId)
        {
            var result = new Dictionary<string, object>();
            Trx trx = null;
            try
            {
                if (Array.IndexOf(DocTypes, kind) < 0 || docId <= 0) throw new Exception(Msg.GetMsg(ctx, "VA012_ARChooseType"));
                string sql = "SELECT MS.VA012_MatchStatus, MS.VA012_MatchCase, MS.C_BankStatementLine_ID, MR.C_BankAccount_ID, MR.C_BankStatement_ID,"
                    + " BSL.StmtAmt, BSL.C_BPartner_ID, BSL.StatementLineDate, BSL.C_Payment_ID, BSL.C_CashLine_ID, BSL.VA012_IsMatchingConfirmed"
                    + " FROM VA012_MatchSuggestion MS"
                    + " INNER JOIN VA012_MatchRun MR ON (MR.VA012_MatchRun_ID = MS.VA012_MatchRun_ID)"
                    + " INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)"
                    + " WHERE MS.IsActive = 'Y' AND MS.VA012_MatchSuggestion_ID = @id";
                sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@id", suggestionId) }, null);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) throw new Exception(Text(ctx, "VA012_ARNotFound", "The record was not found."));
                DataRow s = ds.Tables[0].Rows[0];
                string status = Util.GetValueOfString(s["VA012_MatchStatus"]);
                bool dragged = status == "S" && Util.GetValueOfString(s["VA012_MatchCase"]) == "DR";
                bool declined = status == "D";                                      // a declined line is matched by hand the same way
                if (status != "N" && !declined && !dragged) throw new Exception(Text(ctx, "VA012_ARDropOnNoMatch", "Drop a document on a line without a match."));
                if (Util.GetValueOfInt(s["C_Payment_ID"]) > 0 || Util.GetValueOfInt(s["C_CashLine_ID"]) > 0 || Util.GetValueOfString(s["VA012_IsMatchingConfirmed"]) == "Y")
                    throw new Exception(Text(ctx, "VA012_ARLineReconciled", "The statement line was reconciled in the form meanwhile."));
                int lineId = Util.GetValueOfInt(s["C_BankStatementLine_ID"]);
                int statementId = Util.GetValueOfInt(s["C_BankStatement_ID"]);
                ArRunInfo info = new AutoReconcileCandidates(ctx, null).GetRunInfo(Util.GetValueOfInt(s["C_BankAccount_ID"]), statementId);

                // the line's document rows
                DataSet rds = DB.ExecuteDataset("SELECT MSD.VA012_MatchSuggestionDoc_ID, MSD.VA012_IsReserved, MSD.VA012_Amount, MSD.C_Payment_ID,"
                    + " MSD.C_InvoicePaySchedule_ID, MSD.C_Order_ID, MSD.C_CashLine_ID FROM VA012_MatchSuggestionDoc MSD"
                    + " WHERE MSD.IsActive = 'Y' AND MSD.VA012_MatchSuggestion_ID = @id", new[] { new SqlParameter("@id", suggestionId) }, null);
                List<DataRow> rows = rds == null || rds.Tables.Count == 0 ? new List<DataRow>() : rds.Tables[0].Rows.Cast<DataRow>().ToList();
                Func<DataRow, string> kindOf = r => Util.GetValueOfInt(r["C_Payment_ID"]) > 0 ? "PY" : Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]) > 0 ? "IS"
                    : Util.GetValueOfInt(r["C_Order_ID"]) > 0 ? "PO" : "CO";
                Func<DataRow, int> idOf = r => Util.GetValueOfInt(r[kindOf(r) == "PY" ? "C_Payment_ID" : kindOf(r) == "IS" ? "C_InvoicePaySchedule_ID" : kindOf(r) == "PO" ? "C_Order_ID" : "C_CashLine_ID"]);
                List<DataRow> reserved = rows.Where(r => Util.GetValueOfString(r["VA012_IsReserved"]) == "Y").ToList();
                if (!reserved.Any(r => kindOf(r) == kind && idOf(r) == docId))
                {
                    ArDoc doc = new AutoReconcileCandidates(ctx, null).LoadDoc(info, kind, docId);
                    if (doc == null) throw new Exception(NotAvailable(ctx, kind + "_" + docId.ToString(CultureInfo.InvariantCulture), statementId));
                    // several invoice schedules or several prepay orders on one line, as the form allows; else the new one replaces
                    bool add = dragged && (kind == "IS" || kind == "PO") && reserved.Count > 0 && reserved.All(r => kindOf(r) == kind);
                    // the form's own checks for this document on this line: refused here what the form refuses on a drop
                    string refused = DropCheck(ctx, info, doc, lineId, Util.GetValueOfDecimal(s["StmtAmt"]), Util.GetValueOfInt(s["C_BPartner_ID"]),
                        Util.GetValueOfDateTime(s["StatementLineDate"]) ?? info.StatementDate, add ? reserved.Select(idOf).ToList() : new List<int>());
                    if (refused != null) throw new Exception(refused);
                    var docs = (add ? reserved.Select(r => new ArDoc { Kind = kind, Amount = Util.GetValueOfDecimal(r["VA012_Amount"]) }) : Enumerable.Empty<ArDoc>()).ToList();
                    docs.Add(doc);

                    int chargeId, taxId;
                    DiffDefaults(info, Util.GetValueOfDecimal(s["StmtAmt"]), docs, out chargeId, out taxId);

                    trx = Trx.GetTrx(Trx.CreateTrxName("VA012ARG"));
                    // a replaced document was dragged there too (only a dragged line has one): it goes, as with the ×
                    if (!add && DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestionDoc WHERE VA012_MatchSuggestion_ID = @s AND VA012_IsReserved = 'Y'",
                        new[] { new SqlParameter("@s", suggestionId) }, trx) < 0)
                        throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                    DataRow existing = rows.FirstOrDefault(r => kindOf(r) == kind && idOf(r) == docId);
                    // a declined line: the declined document(s) leave the line (dropped again, the document stays and is reserved)
                    if (declined)
                    {
                        if (DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestionDoc WHERE VA012_MatchSuggestion_ID = @s AND COALESCE(VA012_IsReserved, 'N') = 'N'"
                            + (existing != null ? " AND VA012_MatchSuggestionDoc_ID <> @keep" : ""),
                            existing != null
                                ? new[] { new SqlParameter("@s", suggestionId), new SqlParameter("@keep", Util.GetValueOfInt(existing["VA012_MatchSuggestionDoc_ID"])) }
                                : new[] { new SqlParameter("@s", suggestionId) }, trx) < 0)
                            throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                    }
                    if (existing != null)
                    {
                        if (DB.ExecuteQuery("UPDATE VA012_MatchSuggestionDoc SET VA012_IsReserved = 'Y', VA012_ReserveKey = @key, VA012_Amount = @amt WHERE VA012_MatchSuggestionDoc_ID = @d",
                            new[] { new SqlParameter("@key", doc.ReserveKey), new SqlParameter("@amt", doc.Amount), new SqlParameter("@d", Util.GetValueOfInt(existing["VA012_MatchSuggestionDoc_ID"])) }, trx) <= 0)
                            throw new Exception(Text(ctx, "VA012_ARReservedElsewhere", "This document is reserved for another line."));
                    }
                    else
                    {
                        var batch = new RowBatch("VA012_MatchSuggestionDoc", "VA012_MatchSuggestion_ID", "C_Payment_ID", "C_InvoicePaySchedule_ID",
                            "C_Order_ID", "C_CashLine_ID", "VA012_Amount", "VA012_IsReserved", "VA012_ReserveKey");
                        batch.Add(suggestionId, kind == "PY" ? IdOrNull(docId) : null, kind == "IS" ? IdOrNull(docId) : null, kind == "PO" ? IdOrNull(docId) : null,
                            kind == "CO" ? IdOrNull(docId) : null, doc.Amount, true, doc.ReserveKey);
                        batch.Write(ctx, info.OrgId, ReserveIds(ctx, "VA012_MatchSuggestionDoc", 1), trx);
                    }
                    if (DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'S', VA012_MatchCase = 'DR', VA012_LiveLine = @line, VA012_IsAuto = 'N',"
                        + " VA012_Reason = NULL, VA012_MatchRule_ID = NULL, VA012_DifferenceType = NULL, VA009_PaymentMethod_ID = @pm, C_ConversionType_ID = NULL,"
                        + " C_Charge_ID = @charge, C_Tax_ID = @tax WHERE VA012_MatchSuggestion_ID = @s",
                        new[] { new SqlParameter("@line", lineId), new SqlParameter("@pm", IdOrNull(doc.PayMethodId) ?? DBNull.Value),
                            new SqlParameter("@charge", IdOrNull(chargeId) ?? DBNull.Value), new SqlParameter("@tax", IdOrNull(taxId) ?? DBNull.Value),
                            new SqlParameter("@s", suggestionId) }, trx) <= 0)
                        throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                    if (!trx.Commit()) throw new Exception(Text(ctx, "VA012_ARReservedElsewhere", "This document is reserved for another line."));
                }
                // dropped again on the line it is on: as the form says
                else throw new Exception(Text(ctx, "VA012_AlreadySelected", "This document is already on the line."));
                Dictionary<string, object> row;
                LoadSuggestionRows(ctx, statementId, new List<int> { suggestionId }).TryGetValue(suggestionId, out row);
                result["row"] = row;
            }
            catch (Exception e)
            {
                if (trx != null) trx.Rollback();
                log.Log(Level.WARNING, "VA012 auto reconcile assign document", e);
                // a unique key refusal (the document or the line is held elsewhere) comes as a database error
                string msg = e.Message ?? "";
                result["error"] = msg.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0 || msg.IndexOf("ORA-00001", StringComparison.Ordinal) >= 0
                    ? Text(ctx, "VA012_ARReservedElsewhere", "This document is reserved for another line.") : msg;
            }
            finally
            {
                if (trx != null) trx.Close();
            }
            return result;
        }

        /// <summary>
        /// The × on a dragged document: the document leaves the line and is free again. A line left without a document goes
        /// back to No match, saying so; with documents left (several schedules or orders) it stays suggested, its default
        /// charge and tax worked out again for the new difference. Returns the line's row as the page shows it.
        /// </summary>
        public Dictionary<string, object> UnassignDocument(Ctx ctx, int suggestionId, int docRowId)
        {
            var result = new Dictionary<string, object>();
            Trx trx = null;
            try
            {
                string sql = "SELECT MS.VA012_MatchStatus, MS.VA012_MatchCase, MR.C_BankAccount_ID, MR.C_BankStatement_ID, BSL.StmtAmt"
                    + " FROM VA012_MatchSuggestion MS"
                    + " INNER JOIN VA012_MatchRun MR ON (MR.VA012_MatchRun_ID = MS.VA012_MatchRun_ID)"
                    + " INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)"
                    + " WHERE MS.IsActive = 'Y' AND MS.VA012_MatchSuggestion_ID = @id";
                sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@id", suggestionId) }, null);
                if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) throw new Exception(Text(ctx, "VA012_ARNotFound", "The record was not found."));
                DataRow s = ds.Tables[0].Rows[0];
                if (Util.GetValueOfString(s["VA012_MatchStatus"]) != "S" || Util.GetValueOfString(s["VA012_MatchCase"]) != "DR")
                    throw new Exception(Text(ctx, "VA012_ARNotDragged", "Only a document dragged onto the line can be removed here."));
                int statementId = Util.GetValueOfInt(s["C_BankStatement_ID"]);

                trx = Trx.GetTrx(Trx.CreateTrxName("VA012ARU"));
                if (DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestionDoc WHERE VA012_MatchSuggestionDoc_ID = @d AND VA012_MatchSuggestion_ID = @s AND VA012_IsReserved = 'Y'",
                    new[] { new SqlParameter("@d", docRowId), new SqlParameter("@s", suggestionId) }, trx) < 0)
                    throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                DataSet rds = DB.ExecuteDataset("SELECT MSD.VA012_Amount, MSD.C_Payment_ID, MSD.C_InvoicePaySchedule_ID, MSD.C_Order_ID FROM VA012_MatchSuggestionDoc MSD"
                    + " WHERE MSD.VA012_MatchSuggestion_ID = @s AND MSD.VA012_IsReserved = 'Y'", new[] { new SqlParameter("@s", suggestionId) }, trx);
                List<ArDoc> left = (rds == null || rds.Tables.Count == 0 ? Enumerable.Empty<DataRow>() : rds.Tables[0].Rows.Cast<DataRow>())
                    .Select(r => new ArDoc
                    {
                        Kind = Util.GetValueOfInt(r["C_Payment_ID"]) > 0 ? "PY" : Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]) > 0 ? "IS" : Util.GetValueOfInt(r["C_Order_ID"]) > 0 ? "PO" : "CO",
                        Amount = Util.GetValueOfDecimal(r["VA012_Amount"])
                    }).ToList();
                int n;
                if (left.Count == 0)
                    n = DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'N', VA012_MatchCase = NULL, VA012_LiveLine = NULL, VA012_Reason = @reason,"
                        + " VA009_PaymentMethod_ID = NULL, C_ConversionType_ID = NULL, VA012_ChequeNo = NULL, VA012_DifferenceType = NULL, C_Charge_ID = NULL, C_Tax_ID = NULL"
                        + " WHERE VA012_MatchSuggestion_ID = @s",
                        new[] { new SqlParameter("@reason", Text(ctx, "VA012_ARDocRemoved", "Document removed. Drag another document onto the line.")), new SqlParameter("@s", suggestionId) }, trx);
                else
                {
                    int chargeId, taxId;
                    ArRunInfo info = new AutoReconcileCandidates(ctx, null).GetRunInfo(Util.GetValueOfInt(s["C_BankAccount_ID"]), statementId);
                    DiffDefaults(info, Util.GetValueOfDecimal(s["StmtAmt"]), left, out chargeId, out taxId);
                    n = DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET C_Charge_ID = @charge, C_Tax_ID = @tax WHERE VA012_MatchSuggestion_ID = @s",
                        new[] { new SqlParameter("@charge", IdOrNull(chargeId) ?? DBNull.Value), new SqlParameter("@tax", IdOrNull(taxId) ?? DBNull.Value),
                            new SqlParameter("@s", suggestionId) }, trx);
                }
                if (n <= 0 || !trx.Commit()) throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
                Dictionary<string, object> row;
                LoadSuggestionRows(ctx, statementId, new List<int> { suggestionId }).TryGetValue(suggestionId, out row);
                result["row"] = row;
            }
            catch (Exception e)
            {
                if (trx != null) trx.Rollback();
                log.Log(Level.WARNING, "VA012 auto reconcile unassign document", e);
                result["error"] = e.Message;
            }
            finally
            {
                if (trx != null) trx.Close();
            }
            return result;
        }

        /// <summary>
        /// Why a dropped document cannot be had: the statement line that holds it (its line no., and its statement when
        /// that is another one), and how to free it; or, held by none, that it is no longer open.
        /// </summary>
        private static string NotAvailable(Ctx ctx, string reserveKey, int statementId)
        {
            string sql = "SELECT BSL.Line, BS.C_BankStatement_ID, BS.Name, MS.VA012_MatchStatus FROM VA012_MatchSuggestionDoc MSD"
                + " INNER JOIN VA012_MatchSuggestion MS ON (MS.VA012_MatchSuggestion_ID = MSD.VA012_MatchSuggestion_ID)"
                + " INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)"
                + " INNER JOIN C_BankStatement BS ON (BS.C_BankStatement_ID = BSL.C_BankStatement_ID)"
                + " WHERE MSD.IsActive = 'Y' AND MSD.VA012_ReserveKey = @key";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MSD", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@key", reserveKey) }, null);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                return Text(ctx, "VA012_ARDocNoLongerOpenHere", "This document is no longer open: it was reconciled, paid or put on a statement line meanwhile.");
            DataRow r = ds.Tables[0].Rows[0];
            string holder = Text(ctx, "VA012_ARHeldByLine", "Statement line") + " " + Util.GetValueOfInt(r["Line"]).ToString(CultureInfo.InvariantCulture);
            if (Util.GetValueOfInt(r["C_BankStatement_ID"]) != statementId)
                holder += " " + Text(ctx, "VA012_AROfStatement", "of statement") + " " + Util.GetValueOfString(r["Name"]);
            return holder + " " + (Util.GetValueOfString(r["VA012_MatchStatus"]) == "S"
                ? Text(ctx, "VA012_ARHoldsSuggested", "holds this document as its suggestion. Decline it there, or take the document off with ×, to use it here.")
                : Text(ctx, "VA012_ARHoldsReconciled", "was reconciled with this document. If that line was unmatched in the form, run auto reconciliation on its statement to free the document."));
        }

        /// <summary>
        /// The checks the Bank Statement form runs when a document is dropped on an existing line, through the form's own
        /// methods with the line as the destination, so the page refuses the same drops with the same messages:
        /// payment (MatchByDrag): partner other than the line's, the other direction (in or out), line date before the
        /// payment's, the line's Trx No not in the payment's, no conversion rate; invoice schedule (CheckScheduleCondition):
        /// partner other than the line's, the other direction, schedules of the other side or another currency than those
        /// already on the line; prepay order (CheckPrepayCondition): partner, direction, date, conversion rate, and, as the
        /// form's page checks, an order larger than the line; contra (MatchContraByDrag): partner, direction, date,
        /// conversion rate. A document whose amount cannot be converted to the bank's currency is refused for every type.
        /// Null when the drop is allowed, else the message.
        /// </summary>
        private static string DropCheck(Ctx ctx, ArRunInfo info, ArDoc doc, int lineId, decimal lineAmount, int lineBpId, DateTime lineDate, List<int> heldIds)
        {
            if (doc.Amount == 0) return Text(ctx, "VA012_ConversionRateNotFound", "Conversion rate not found.");
            var ops = new StatementOperations();
            string held = string.Join(",", heldIds.Select(id => id.ToString(CultureInfo.InvariantCulture)));
            string status;
            switch (doc.Kind)
            {
                case "PY":
                    status = ops.CheckPaymentCondition(ctx, doc.Id, lineId, lineAmount, info.StatementDate, info.BankAccountId)._status;
                    break;
                case "IS":
                    status = ops.CheckScheduleCondition(ctx, doc.Id, lineId, held, lineAmount, info.BankCurrencyId, lineBpId);
                    break;
                case "PO":
                    status = ops.CheckPrepayCondition(ctx, doc.Id, lineId, held, lineAmount, info.BankCurrencyId, lineBpId, lineDate)._status;
                    if (status == "Success" && lineAmount != 0 && Math.Abs(doc.Amount) > Math.Abs(lineAmount)) status = "VA012_StatementAmtShouldBeMore";
                    break;
                default:
                    status = ops.CheckContraCondition(ctx, doc.Id, lineId, lineAmount, info.BankCurrencyId, lineBpId, info.StatementDate)._status;
                    break;
            }
            if (status == "Success") return null;
            if (string.IsNullOrEmpty(status)) status = "VA012_StatementPaymentNotMatched";
            return Text(ctx, status, status);
        }

        /// <summary>
        /// What a run stores for a difference: the header's charge and tax where the line and its documents differ for a
        /// payment or prepay order, or the line pays more than its invoice schedules; none otherwise.
        /// </summary>
        private static void DiffDefaults(ArRunInfo info, decimal lineAmount, List<ArDoc> docs, out int chargeId, out int taxId)
        {
            chargeId = 0;
            taxId = 0;
            if (docs.Count == 0 || !info.Settings.HasDefaultChargeAndTax) return;
            var r = new ArResult { Line = new ArLine { Amount = lineAmount }, Docs = docs };
            string kind = docs[0].Kind;
            if (lineAmount - docs.Sum(d => d.Amount) != 0 && (kind == "PY" || kind == "PO" || (kind == "IS" && AutoReconcileMatcher.StatementExceedsDocs(r))))
            {
                chargeId = info.Settings.ChargeId;
                taxId = info.Settings.TaxId;
            }
        }

        private static string Text(Ctx ctx, string key, string fallback)
        {
            string t = Msg.GetMsg(ctx, key);
            return string.IsNullOrEmpty(t) || t == key || t == "[" + key + "]" ? fallback : t;
        }

        // ------------------------------------------------------------------ helpers
        private static int SaveRun(Ctx ctx, ArRunInfo info, string docType, int payMethodId, int bpId, Trx trx)
        {
            var run = new RowBatch("VA012_MatchRun", "C_BankAccount_ID", "C_BankStatement_ID", "VA012_MatchHeader_ID", "VA012_DocType",
                "VA009_PaymentMethod_ID", "C_BPartner_ID");
            run.Add(info.BankAccountId, info.StatementId, IdOrNull(info.HeaderId), docType, IdOrNull(payMethodId), IdOrNull(bpId));
            List<int> ids = ReserveIds(ctx, "VA012_MatchRun", 1);
            run.Write(ctx, info.OrgId, ids, trx);
            return ids[0];
        }

        /// <summary>A line no type matched: say so, keeping the reason of the type whose candidates are shown.</summary>
        private static string AllTypesReason(Ctx ctx, string reason)
        {
            string none = Msg.GetMsg(ctx, "VA012_ARNoTypeMatched");
            if (string.IsNullOrEmpty(none) || none == "VA012_ARNoTypeMatched" || none == "[VA012_ARNoTypeMatched]")
                none = "No payment, invoice schedule, prepay order or contra line matched";
            return string.IsNullOrEmpty(reason) ? none : none + " · " + reason;
        }

        private static object IdOrNull(int id)
        {
            return id > 0 ? (object)id : null;
        }

        private static bool? _nativeSequence;

        /// <summary>System setting SYSTEM_NATIVE_SEQUENCE (keys from database sequences), read once per application start.</summary>
        private static bool IsNativeSequence()
        {
            if (_nativeSequence == null)
                _nativeSequence = Util.GetValueOfString(DB.ExecuteScalar("SELECT Value FROM AD_SysConfig WHERE IsActive = 'Y' AND AD_Client_ID = 0"
                    + " AND Name = 'SYSTEM_NATIVE_SEQUENCE'", null, null)) == "Y";
            return _nativeSequence.Value;
        }

        /// <summary>
        /// Keys for count new rows of a table from AD_Sequence in one call: the sequence moves on by count, so first to
        /// first + count - 1 are ours. A database (native) sequence hands out one number per call.
        /// </summary>
        private static List<int> ReserveIds(Ctx ctx, string table, int count)
        {
            var ids = new List<int>(count);
            if (count <= 0) return ids;
            if (IsNativeSequence())
            {
                for (int i = 0; i < count; i++)
                {
                    int id = DB.GetNextID(ctx.GetAD_Client_ID(), table, null);
                    if (id <= 0) throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving") + " " + table);
                    ids.Add(id);
                }
                return ids;
            }
            int first = DB.GetNextID(ctx.GetAD_Client_ID(), table, null, count);
            if (first <= 0) throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving") + " " + table);
            for (int i = 0; i < count; i++) ids.Add(first + i);
            return ids;
        }

        /// <summary>
        /// Rows of one of the run's own work tables (VA012_MatchRun, VA012_MatchSuggestion, VA012_MatchSuggestionDoc),
        /// written set-based in the caller's transaction instead of one PO save per row (decision 2026-10-27): a PO save
        /// costs a sequence round trip, and for a role that logs changes one more per column. These tables have no model
        /// class and no document number. Values are bound; NULL and the timestamp are literals. PostgreSQL takes one
        /// multi-row INSERT, Oracle one PL/SQL block of INSERTs: one round trip per chunk either way.
        /// </summary>
        private sealed class RowBatch
        {
            private const int MaxParams = 1000;
            private readonly string _table;
            private readonly string[] _columns;
            private readonly List<object[]> _rows = new List<object[]>();

            public RowBatch(string table, params string[] columns)
            {
                _table = table;
                _columns = columns;
            }

            public int Count { get { return _rows.Count; } }

            /// <summary>One row: values in the order of the columns; null writes NULL, a bool Y or N.</summary>
            public void Add(params object[] values)
            {
                _rows.Add(values);
            }

            /// <summary>Inserts the rows with the keys from ReserveIds, in the order they were added.</summary>
            public void Write(Ctx ctx, int orgId, List<int> ids, Trx trx)
            {
                if (_rows.Count == 0) return;
                string head = "INSERT INTO " + _table + " (" + _table + "_ID, AD_Client_ID, AD_Org_ID, IsActive, Created, CreatedBy, Updated, UpdatedBy, "
                    + string.Join(", ", _columns) + ")";
                string now = GlobalVariable.TO_DATE(DateTime.Now, false);
                bool postgre = DB.IsPostgreSQL();
                int chunk = Math.Max(1, MaxParams / (_columns.Length + 5));
                bool verify = false;
                for (int start = 0; start < _rows.Count; start += chunk)
                {
                    int end = Math.Min(_rows.Count, start + chunk);
                    var p = new List<SqlParameter>();
                    var values = new List<string>();
                    for (int i = start; i < end; i++)
                    {
                        var v = new StringBuilder("(");
                        v.Append(Bind(p, ids[i])).Append(", ").Append(Bind(p, ctx.GetAD_Client_ID())).Append(", ").Append(Bind(p, orgId))
                            .Append(", 'Y', ").Append(now).Append(", ").Append(Bind(p, ctx.GetAD_User_ID()))       // CreatedBy
                            .Append(", ").Append(now).Append(", ").Append(Bind(p, ctx.GetAD_User_ID()));            // UpdatedBy
                        foreach (object value in _rows[i]) v.Append(", ").Append(Bind(p, value));
                        values.Add(v.Append(")").ToString());
                    }
                    int rows = end - start;
                    string sql = postgre || rows == 1
                        ? head + " VALUES " + string.Join(", ", values)
                        : "BEGIN " + string.Join(" ", values.Select(x => head + " VALUES " + x + ";")) + " END;";
                    int no = DB.ExecuteQuery(sql, p.ToArray(), trx);
                    // a PL/SQL block reports no row count: those chunks are counted once at the end
                    if (!postgre && rows > 1) verify = true;
                    else if (no != rows) throw new Exception(SaveError(ctx, _table));
                }
                if (verify)
                {
                    int written = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(1) FROM " + _table + " WHERE IsActive = 'Y' AND " + _table + "_ID BETWEEN @lo AND @hi",
                        new[] { new SqlParameter("@lo", ids.Take(_rows.Count).Min()), new SqlParameter("@hi", ids.Take(_rows.Count).Max()) }, trx));
                    if (written < _rows.Count) throw new Exception(SaveError(ctx, _table));
                }
            }

            private static string Bind(List<SqlParameter> p, object value)
            {
                if (value == null) return "NULL";
                if (value is bool) value = (bool)value ? "Y" : "N";
                string name = "@b" + p.Count.ToString(CultureInfo.InvariantCulture);
                p.Add(new SqlParameter(name, value));
                return name;
            }
        }

        /// <summary>
        /// Reconciled rows (A, C) whose line was unmatched in the form since: they become released (R) and free their keys,
        /// so the line can be matched again and its document offered to other lines.
        /// </summary>
        private static void ReleaseUnmatched(Ctx ctx, int statementId, Trx trx)
        {
            const string unmatchedLines = "SELECT BSL.C_BankStatementLine_ID FROM C_BankStatementLine BSL WHERE BSL.C_BankStatement_ID = @stmt"
                + " AND BSL.C_Payment_ID IS NULL AND BSL.C_CashLine_ID IS NULL";
            if (DB.ExecuteQuery("UPDATE VA012_MatchSuggestionDoc SET VA012_IsReserved = 'N', VA012_ReserveKey = NULL"
                + " WHERE VA012_ReserveKey IS NOT NULL AND VA012_MatchSuggestion_ID IN (SELECT MS.VA012_MatchSuggestion_ID FROM VA012_MatchSuggestion MS"
                + " WHERE MS.VA012_MatchStatus IN ('A', 'C') AND MS.C_BankStatementLine_ID IN (" + unmatchedLines + "))",
                new[] { new SqlParameter("@stmt", statementId) }, trx) < 0
                || DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'R', VA012_LiveLine = NULL"
                + " WHERE VA012_MatchStatus IN ('A', 'C') AND C_BankStatementLine_ID IN (" + unmatchedLines + ")",
                new[] { new SqlParameter("@stmt", statementId) }, trx) < 0)
                throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
        }

        /// <summary>After posting: C when the line was reconciled, else it stays suggested (not automatic) with the reason.</summary>
        private static void SetStatus(int suggestionId, string status, string reason)
        {
            if (status == "C")
            {
                DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'C' WHERE VA012_MatchSuggestion_ID = @id",
                    new[] { new SqlParameter("@id", suggestionId) }, null);
                DeleteCandidates(suggestionId);
            }
            else
                DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_IsAuto = 'N', VA012_Reason = @reason WHERE VA012_MatchSuggestion_ID = @id",
                    new[] { new SqlParameter("@reason", Truncate(reason ?? "", 255)), new SqlParameter("@id", suggestionId) }, null);
        }

        private static string Truncate(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max);
        }

        /// <summary>
        /// The candidate rows (not reserved) of a suggestion that was reconciled or accepted: nobody looks at them again.
        /// The reserved documents stay as the record of what the line was reconciled with.
        /// </summary>
        internal static void DeleteCandidates(int suggestionId)
        {
            DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestionDoc WHERE VA012_MatchSuggestion_ID = @id AND COALESCE(VA012_IsReserved, 'N') = 'N'",
                new[] { new SqlParameter("@id", suggestionId) }, null);
        }

        /// <summary>
        /// Deletes what a new run replaces: the earlier suggested and no-match rows of the lines with their documents, which
        /// frees their reservations (status X only remains from earlier builds and goes the same way), then the statement's
        /// runs that have no rows left. Accepted, reconciled, declined and released rows stay as the line's history.
        /// </summary>
        private static void DeleteReplaced(Ctx ctx, int statementId, List<int> onlyLines, Trx trx)
        {
            if (onlyLines != null && onlyLines.Count == 0) return;
            var batches = new List<List<int>>();
            if (onlyLines == null) batches.Add(null);
            else for (int start = 0; start < onlyLines.Count; start += InListChunk) batches.Add(onlyLines.Skip(start).Take(InListChunk).ToList());
            foreach (List<int> batch in batches)
            {
                string lineFilter = batch == null
                    ? "IN (SELECT BSL.C_BankStatementLine_ID FROM C_BankStatementLine BSL WHERE BSL.C_BankStatement_ID = @stmt)"
                    : "IN (" + InList(batch, "l") + ")";
                Func<SqlParameter[]> param = () => batch == null
                    ? new[] { new SqlParameter("@stmt", statementId) }
                    : batch.Select((id, i) => new SqlParameter("@l" + i.ToString(CultureInfo.InvariantCulture), id)).ToArray();
                if (DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestionDoc WHERE VA012_MatchSuggestion_ID IN (SELECT MS.VA012_MatchSuggestion_ID FROM VA012_MatchSuggestion MS"
                    + " WHERE MS.VA012_MatchStatus IN ('S', 'N', 'X') AND MS.C_BankStatementLine_ID " + lineFilter + ")", param(), trx) < 0
                    || DB.ExecuteQuery("DELETE FROM VA012_MatchSuggestion"
                    + " WHERE VA012_MatchStatus IN ('S', 'N', 'X') AND C_BankStatementLine_ID " + lineFilter, param(), trx) < 0)
                    throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
            }
            if (DB.ExecuteQuery("DELETE FROM VA012_MatchRun WHERE C_BankStatement_ID = @stmt"
                + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestion MS WHERE MS.VA012_MatchRun_ID = VA012_MatchRun.VA012_MatchRun_ID)",
                new[] { new SqlParameter("@stmt", statementId) }, trx) < 0)
                throw new Exception(Msg.GetMsg(ctx, "VA012_ErrorSaving"));
        }

        private static IEnumerable<DataRow> LoadDocsFor(Ctx ctx, List<int> suggestionIds)
        {
            string sql = "SELECT MSD.VA012_MatchSuggestionDoc_ID, MSD.VA012_MatchSuggestion_ID, MSD.VA012_Amount, MSD.VA012_IsReserved, MSD.C_Payment_ID, MSD.C_InvoicePaySchedule_ID,"
                + " MSD.C_Order_ID, MSD.C_CashLine_ID, COALESCE(PAY.DocumentNo, INV.DocumentNo, ORD.DocumentNo, CS.DocumentNo) AS DocumentNo,"
                + " COALESCE(BPP.Name, BPI.Name, BPO.Name, BPC.Name) AS BPName, COALESCE(PAY.DateAcct, IPS.DueDate, ORD.DateOrdered, CS.DateAcct) AS DocDate,"
                + " PAY.TrxNo AS PayTrxNo, COALESCE(PAY.CheckNo, CL.CheckNo) AS ChequeNo, PAY.CheckDate AS PayChequeDate,"
                + " COALESCE(PAY.C_Currency_ID, INV.C_Currency_ID, ORD.C_Currency_ID, CL.C_Currency_ID) AS CurrencyId,"
                + " COALESCE(CUP.ISO_Code, CUI.ISO_Code, CUO.ISO_Code, CUC.ISO_Code) AS CurrencyCode,"
                + " COALESCE(PAY.C_ConversionType_ID, INV.C_ConversionType_ID, ORD.C_ConversionType_ID, CL.C_ConversionType_ID) AS ConversionTypeId,"
                + " COALESCE(PAY.VA009_PaymentMethod_ID, IPS.VA009_PaymentMethod_ID, ORD.VA009_PaymentMethod_ID) AS DocPayMethodId"
                + " FROM VA012_MatchSuggestionDoc MSD"
                // one partner and currency join per document kind: a function with commas in a JOIN breaks the access SQL parser
                + " LEFT OUTER JOIN C_Payment PAY ON (PAY.C_Payment_ID = MSD.C_Payment_ID)"
                + " LEFT OUTER JOIN C_BPartner BPP ON (BPP.C_BPartner_ID = PAY.C_BPartner_ID)"
                + " LEFT OUTER JOIN C_Currency CUP ON (CUP.C_Currency_ID = PAY.C_Currency_ID)"
                + " LEFT OUTER JOIN C_InvoicePaySchedule IPS ON (IPS.C_InvoicePaySchedule_ID = MSD.C_InvoicePaySchedule_ID)"
                + " LEFT OUTER JOIN C_Invoice INV ON (INV.C_Invoice_ID = IPS.C_Invoice_ID)"
                + " LEFT OUTER JOIN C_BPartner BPI ON (BPI.C_BPartner_ID = INV.C_BPartner_ID)"
                + " LEFT OUTER JOIN C_Currency CUI ON (CUI.C_Currency_ID = INV.C_Currency_ID)"
                + " LEFT OUTER JOIN C_Order ORD ON (ORD.C_Order_ID = MSD.C_Order_ID)"
                + " LEFT OUTER JOIN C_BPartner BPO ON (BPO.C_BPartner_ID = ORD.C_BPartner_ID)"
                + " LEFT OUTER JOIN C_Currency CUO ON (CUO.C_Currency_ID = ORD.C_Currency_ID)"
                + " LEFT OUTER JOIN C_CashLine CL ON (CL.C_CashLine_ID = MSD.C_CashLine_ID)"
                + " LEFT OUTER JOIN C_Cash CS ON (CS.C_Cash_ID = CL.C_Cash_ID)"
                + " LEFT OUTER JOIN C_BPartner BPC ON (BPC.C_BPartner_ID = CL.C_BPartner_ID)"
                + " LEFT OUTER JOIN C_Currency CUC ON (CUC.C_Currency_ID = CL.C_Currency_ID)"
                + " WHERE MSD.IsActive = 'Y' AND MSD.VA012_MatchSuggestion_ID IN (" + InList(suggestionIds, "s") + ")";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MSD", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO)
                + " ORDER BY MSD.VA012_MatchSuggestion_ID, MSD.VA012_MatchSuggestionDoc_ID";
            DataSet ds = DB.ExecuteDataset(sql, suggestionIds.Select((id, i) => new SqlParameter("@s" + i.ToString(CultureInfo.InvariantCulture), id)).ToArray(), null);
            return ds == null || ds.Tables.Count == 0 ? Enumerable.Empty<DataRow>() : ds.Tables[0].Rows.Cast<DataRow>();
        }

        private static string InList(List<int> ids, string prefix)
        {
            return string.Join(", ", ids.Select((id, i) => "@" + prefix + i.ToString(CultureInfo.InvariantCulture)));
        }

        private static SqlParameter[] InParams(Ctx ctx, List<int> ids, string prefix)
        {
            var p = new List<SqlParameter> { new SqlParameter("@client", ctx.GetAD_Client_ID()) };
            p.AddRange(ids.Select((id, i) => new SqlParameter("@" + prefix + i.ToString(CultureInfo.InvariantCulture), id)));
            return p.ToArray();
        }

        private static string FormatDate(DateTime? value)
        {
            return value == null ? "" : value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string SaveError(Ctx ctx, string table)
        {
            ValueNamePair pp = VLogger.RetrieveError();
            string detail = pp == null ? "" : (pp.GetName() ?? pp.GetValue() ?? "");
            return Msg.GetMsg(ctx, "VA012_ErrorSaving") + " " + table + (detail.Length > 0 ? ": " + detail : "");
        }

        /// <summary>
        /// Writes the run's suggestions and their documents set-based: one key reservation and a few batched INSERTs per
        /// table, whatever the number of lines. Sets each result's SuggestionId.
        /// </summary>
        private class SuggestionWriter
        {
            private readonly Ctx _ctx;
            private readonly ArRunInfo _info;
            private readonly int _runId;
            private readonly Trx _trx;

            public SuggestionWriter(Ctx ctx, ArRunInfo info, int runId, Trx trx)
            {
                _ctx = ctx; _info = info; _runId = runId; _trx = trx;
            }

            public void WriteAll(List<ArResult> results)
            {
                if (results.Count == 0) return;
                var suggestions = new RowBatch("VA012_MatchSuggestion", "VA012_MatchRun_ID", "C_BankStatementLine_ID", "VA012_MatchStatus",
                    "VA012_IsAuto", "VA012_MatchCase", "VA012_SignalSource", "VA012_Reason", "VA012_LiveLine", "VA012_MatchRule_ID",
                    "VA009_PaymentMethod_ID", "C_ConversionType_ID", "VA012_DifferenceType", "C_Charge_ID", "C_Tax_ID");
                var docs = new RowBatch("VA012_MatchSuggestionDoc", "VA012_MatchSuggestion_ID", "C_Payment_ID", "C_InvoicePaySchedule_ID",
                    "C_Order_ID", "C_CashLine_ID", "VA012_Amount", "VA012_IsReserved", "VA012_ReserveKey");
                List<int> ids = ReserveIds(_ctx, "VA012_MatchSuggestion", results.Count);
                for (int i = 0; i < results.Count; i++)
                {
                    ArResult r = results[i];
                    r.SuggestionId = ids[i];
                    ArRule rule = r.Rule;
                    int method = 0, conversionType = 0, chargeId = 0, taxId = 0;
                    string diffType = null;
                    if (r.Status == "S" && r.Docs.Count > 0)
                    {
                        method = rule != null && rule.PayMethodId > 0 ? rule.PayMethodId : r.Docs[0].PayMethodId;
                        if (rule != null) conversionType = rule.ConversionTypeId;
                        bool statementMore = AutoReconcileMatcher.StatementExceedsDocs(r);
                        if (r.Difference != 0 && r.Docs[0].Kind == "IS" && !statementMore && rule != null && !string.IsNullOrEmpty(rule.DifferenceType))
                            diffType = rule.DifferenceType;
                        if (r.Difference != 0 && (r.Docs[0].Kind == "PY" || r.Docs[0].Kind == "PO" || (r.Docs[0].Kind == "IS" && statementMore))
                            && _info.Settings.HasDefaultChargeAndTax && (rule == null || rule.DiffFromHeader))
                        {
                            chargeId = _info.Settings.ChargeId;
                            taxId = _info.Settings.TaxId;
                        }
                    }
                    suggestions.Add(_runId, r.Line.LineId, r.Status, r.IsAuto, r.MatchCase,
                        r.SignalSource == null ? null : Truncate(r.SignalSource, 60), r.Reason == null ? null : Truncate(r.Reason, 255),
                        r.Status == "S" ? IdOrNull(r.Line.LineId) : null, IdOrNull(rule != null ? rule.Id : 0),
                        IdOrNull(method), IdOrNull(conversionType), diffType, IdOrNull(chargeId), IdOrNull(taxId));
                    foreach (ArDoc d in r.Docs) AddDoc(docs, ids[i], d, true);
                    foreach (ArDoc d in r.Candidates) AddDoc(docs, ids[i], d, false);
                }
                suggestions.Write(_ctx, _info.OrgId, ids, _trx);
                docs.Write(_ctx, _info.OrgId, ReserveIds(_ctx, "VA012_MatchSuggestionDoc", docs.Count), _trx);
            }

            private static void AddDoc(RowBatch docs, int suggestionId, ArDoc d, bool reserved)
            {
                docs.Add(suggestionId, d.Kind == "PY" ? IdOrNull(d.Id) : null, d.Kind == "IS" ? IdOrNull(d.Id) : null, d.Kind == "PO" ? IdOrNull(d.Id) : null,
                    d.Kind != "PY" && d.Kind != "IS" && d.Kind != "PO" ? IdOrNull(d.Id) : null, d.Amount, reserved, reserved ? d.ReserveKey : null);
            }
        }
    }
}
