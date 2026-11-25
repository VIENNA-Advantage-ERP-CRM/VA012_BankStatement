/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto reconciliation - Accept and Accept selected for suggested lines
 * Chronological  : Development
 * Design         : VA012 Auto Reconciliation Solution Approach v4 (Accept, Accept selected)
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    /// <summary>One suggested line to accept, with the user's choices from the detail row.</summary>
    public class ArAcceptItem
    {
        /// <summary>VA012_MatchSuggestion_ID</summary>
        public int id { get; set; }
        /// <summary>VA012_MatchSuggestionDoc_ID of another candidate the user chose; 0 keeps the suggested document(s).</summary>
        public int docRowId { get; set; }
        /// <summary>DA, WO or OU for an invoice-schedule difference.</summary>
        public string differenceType { get; set; }
        public int chargeId { get; set; }
        public int taxId { get; set; }
        /// <summary>Cheque date the user chose (yyyy-MM-dd); empty takes the payment's cheque date, else the line date.</summary>
        public string chequeDate { get; set; }
    }

    /// <summary>Set method / type for one suggested line; 0 leaves that field as it is.</summary>
    public class ArPaymentItem
    {
        public int id { get; set; }
        public int payMethodId { get; set; }
        public int conversionTypeId { get; set; }
        /// <summary>Cheque number typed for the new payment; null leaves it, "" clears it.</summary>
        public string chequeNo { get; set; }
    }

    /// <summary>
    /// Accepts suggested lines one at a time. Each line is checked again first: still suggested, the statement line still
    /// open, the document still open (a document or line matched in the form meanwhile releases the suggestion with the
    /// reason). A different candidate the user chose takes over the reservation. The line is then posted through the same
    /// form save as automatic reconciliation (AutoReconcilePoster) and becomes accepted (A). One line that cannot be
    /// accepted never blocks the others.
    /// </summary>
    public class AutoReconcileAccept
    {
        private static readonly VLogger log = VLogger.GetVLogger(typeof(AutoReconcileAccept).FullName);

        public Dictionary<string, object> Accept(Ctx ctx, List<ArAcceptItem> items)
        {
            var accepted = new List<int>();
            var skipped = new List<Dictionary<string, object>>();
            var runs = new Dictionary<int, ArRunInfo>();
            // the rechecks read through one connection; a statement without a transaction opens a connection of its own.
            // Read committed: each check still sees what an earlier line of this call saved.
            Trx read = Trx.GetTrx(Trx.CreateTrxName("VA012ARA"));
            try
            {
                foreach (ArAcceptItem item in (items ?? new List<ArAcceptItem>()).Where(i => i != null && i.id > 0))
                {
                    Stopwatch watch = Stopwatch.StartNew();
                    int lineNo = 0;
                    string why;
                    try { why = AcceptOne(ctx, item, runs, read, out lineNo); }
                    catch (Exception e)
                    {
                        log.Log(Level.SEVERE, "VA012 auto reconcile accept " + item.id, e);
                        why = e.Message;
                    }
                    if (why == null) accepted.Add(item.id);
                    else
                    {
                        // S: still suggested (details missing, cannot post yet); R: released because the line or document moved on
                        string status = Util.GetValueOfString(DB.ExecuteScalar("SELECT VA012_MatchStatus FROM VA012_MatchSuggestion WHERE VA012_MatchSuggestion_ID = @id",
                            new[] { new SqlParameter("@id", item.id) }, read));
                        skipped.Add(new Dictionary<string, object> { { "id", item.id }, { "lineNo", lineNo }, { "reason", why }, { "status", status } });
                    }
                    log.Info("VA012 auto reconcile accept " + item.id + " (line " + lineNo + "): " + watch.ElapsedMilliseconds + " ms" + (why == null ? "" : " (not accepted)"));
                }
            }
            finally
            {
                read.Rollback();
                read.Close();
            }
            return new Dictionary<string, object> { { "accepted", accepted }, { "skipped", skipped } };
        }

        /// <summary>
        /// Set method / type: saves the payment method and conversion type for the payment Accept will create. Only where
        /// the field is open: suggested lines of invoice-schedule and prepay-order runs (a payment keeps its own method and
        /// type, a contra line its cash line's type). The method must be one the form offers (active, not Cash or On Credit).
        /// </summary>
        public Dictionary<string, object> SetPayment(Ctx ctx, List<ArPaymentItem> items)
        {
            int updated = 0, skipped = 0;
            foreach (ArPaymentItem item in (items ?? new List<ArPaymentItem>()).Where(i => i != null && i.id > 0))
            {
                if (item.payMethodId > 0 && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(PM.VA009_PaymentMethod_ID) FROM VA009_PaymentMethod PM WHERE PM.VA009_PaymentMethod_ID = @m"
                    + " AND PM.IsActive = 'Y' AND PM.VA009_PaymentBaseType NOT IN ('B', 'P') AND PM.AD_Client_ID IN (0, @client)",
                    new[] { new SqlParameter("@m", item.payMethodId), new SqlParameter("@client", ctx.GetAD_Client_ID()) }, null)) == 0)
                    item.payMethodId = 0;
                if (item.conversionTypeId > 0 && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(CT.C_ConversionType_ID) FROM C_ConversionType CT WHERE CT.C_ConversionType_ID = @c"
                    + " AND CT.IsActive = 'Y' AND CT.AD_Client_ID IN (0, @client)",
                    new[] { new SqlParameter("@c", item.conversionTypeId), new SqlParameter("@client", ctx.GetAD_Client_ID()) }, null)) == 0)
                    item.conversionTypeId = 0;
                if (item.payMethodId <= 0 && item.conversionTypeId <= 0 && item.chequeNo == null) { skipped++; continue; }

                // the open-field rule in SQL: a suggested line of this tenant whose own (reserved) document is an invoice schedule
                // or a prepay order, whatever the run's type (all types, or a document dragged onto the line), or a tie of an
                // invoice-schedule or prepay-order run, which has no reserved document yet
                const string open = " WHERE VA012_MatchSuggestion_ID = @id AND VA012_MatchStatus = 'S' AND AD_Client_ID = @client"
                    + " AND (EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSD WHERE MSD.VA012_MatchSuggestion_ID = VA012_MatchSuggestion.VA012_MatchSuggestion_ID"
                    + " AND MSD.VA012_IsReserved = 'Y' AND (MSD.C_InvoicePaySchedule_ID IS NOT NULL OR MSD.C_Order_ID IS NOT NULL))"
                    + " OR (VA012_MatchRun_ID IN (SELECT MR.VA012_MatchRun_ID FROM VA012_MatchRun MR WHERE MR.VA012_DocType IN ('IS', 'PO'))"
                    + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSR WHERE MSR.VA012_MatchSuggestion_ID = VA012_MatchSuggestion.VA012_MatchSuggestion_ID"
                    + " AND MSR.VA012_IsReserved = 'Y')))";
                int n = 0;
                if (item.payMethodId > 0)
                    n = Math.Max(n, DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA009_PaymentMethod_ID = @m" + open,
                        new[] { new SqlParameter("@m", item.payMethodId), new SqlParameter("@id", item.id), new SqlParameter("@client", ctx.GetAD_Client_ID()) }, null));
                if (item.conversionTypeId > 0)
                    n = Math.Max(n, DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET C_ConversionType_ID = @c" + open,
                        new[] { new SqlParameter("@c", item.conversionTypeId), new SqlParameter("@id", item.id), new SqlParameter("@client", ctx.GetAD_Client_ID()) }, null));
                if (item.chequeNo != null)
                {
                    string chq = item.chequeNo.Trim();
                    if (chq.Length > 30) chq = chq.Substring(0, 30);
                    n = Math.Max(n, DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_ChequeNo = @q" + open,
                        new[] { new SqlParameter("@q", chq.Length > 0 ? (object)chq : DBNull.Value), new SqlParameter("@id", item.id), new SqlParameter("@client", ctx.GetAD_Client_ID()) }, null));
                }
                if (n > 0) updated++; else skipped++;
            }
            return new Dictionary<string, object> { { "updated", updated }, { "skipped", skipped } };
        }

        /// <summary>
        /// The next number of the bank account's cheque book for a cheque method, as the form takes it
        /// (C_BankAccountDoc: payment rule cheque, account with cheque number auto control, numbers left, by priority);
        /// empty when there is no such book. The number is only shown: it is assigned when the payment is created.
        /// </summary>
        public Dictionary<string, object> ChequeBookNext(Ctx ctx, int bankAccountId, int payMethodId)
        {
            string sql = "SELECT BAD.CurrentNext FROM C_BankAccountDoc BAD INNER JOIN C_BankAccount BA ON (BA.C_BankAccount_ID = BAD.C_BankAccount_ID)"
                + " WHERE BAD.IsActive = 'Y' AND BAD.PaymentRule = 'S' AND BA.ChkNoAutoControl = 'Y' AND BAD.EndChkNumber <> (BAD.CurrentNext - 1)"
                + " AND BAD.VA009_PaymentMethod_ID = @m AND BAD.C_BankAccount_ID = @acct";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "BAD", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY BAD.Priority";
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@m", payMethodId), new SqlParameter("@acct", bankAccountId) }, null);
            int next = ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0 ? Util.GetValueOfInt(ds.Tables[0].Rows[0]["CurrentNext"]) : 0;
            return new Dictionary<string, object> { { "next", next > 0 ? next.ToString(CultureInfo.InvariantCulture) : "" } };
        }

        private string AcceptOne(Ctx ctx, ArAcceptItem item, Dictionary<int, ArRunInfo> runs, Trx read, out int lineNo)
        {
            lineNo = 0;
            string sql = "SELECT MS.VA012_MatchStatus, MS.VA012_MatchRule_ID, MS.VA012_DifferenceType, MS.VA012_MatchRun_ID, MR.VA012_DocType,"
                + " MS.VA009_PaymentMethod_ID, MS.C_ConversionType_ID, MS.VA012_ChequeNo,"
                + " MR.C_BankAccount_ID, MR.C_BankStatement_ID, BSL.C_BankStatementLine_ID, BSL.Line, BSL.StatementLineDate, BSL.StmtAmt, BSL.TrxNo,"
                + " BSL.EftCheckNo, BSL.C_BPartner_ID, BSL.C_Payment_ID, BSL.C_CashLine_ID, BSL.VA012_IsMatchingConfirmed"
                + " FROM VA012_MatchSuggestion MS"
                + " INNER JOIN VA012_MatchRun MR ON (MR.VA012_MatchRun_ID = MS.VA012_MatchRun_ID)"
                + " INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)"
                + " WHERE MS.IsActive = 'Y' AND MS.VA012_MatchSuggestion_ID = @id";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@id", item.id) }, read);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return Text(ctx, "VA012_ARNotFound", "The record was not found.");
            DataRow s = ds.Tables[0].Rows[0];
            lineNo = Util.GetValueOfInt(s["Line"]);
            if (Util.GetValueOfString(s["VA012_MatchStatus"]) != "S") return Text(ctx, "VA012_ARNotSuggested", "This line is no longer suggested.");

            var line = new ArLine
            {
                LineId = Util.GetValueOfInt(s["C_BankStatementLine_ID"]),
                LineNo = lineNo,
                LineDate = Util.GetValueOfDateTime(s["StatementLineDate"]) ?? DateTime.Today,
                Amount = Util.GetValueOfDecimal(s["StmtAmt"]),
                TrxNo = Util.GetValueOfString(s["TrxNo"]),
                ChequeNo = Util.GetValueOfString(s["EftCheckNo"]),
                BPartnerId = Util.GetValueOfInt(s["C_BPartner_ID"])
            };
            if (Util.GetValueOfInt(s["C_Payment_ID"]) > 0 || Util.GetValueOfInt(s["C_CashLine_ID"]) > 0 || Util.GetValueOfString(s["VA012_IsMatchingConfirmed"]) == "Y")
                return Release(ctx, item.id, Text(ctx, "VA012_ARLineReconciled", "The statement line was reconciled in the form meanwhile."));

            string docType = Util.GetValueOfString(s["VA012_DocType"]);
            int runId = Util.GetValueOfInt(s["VA012_MatchRun_ID"]);
            ArRunInfo info;
            if (!runs.TryGetValue(runId, out info))
            {
                info = new AutoReconcileCandidates(ctx, read).GetRunInfo(Util.GetValueOfInt(s["C_BankAccount_ID"]), Util.GetValueOfInt(s["C_BankStatement_ID"]));
                runs[runId] = info;
            }

            // the document(s): the user's other candidate, or the reserved ones
            List<DataRow> rows = DocRows(item.id, read);
            List<DataRow> chosen = item.docRowId > 0
                ? rows.Where(r => Util.GetValueOfInt(r["VA012_MatchSuggestionDoc_ID"]) == item.docRowId).ToList()
                : rows.Where(r => Util.GetValueOfString(r["VA012_IsReserved"]) == "Y").ToList();
            if (chosen.Count == 0) return Text(ctx, "VA012_ARChooseDocument", "Choose a document first.");

            var docs = new List<ArDoc>();
            foreach (DataRow r in chosen)
            {
                ArDoc d = ReadDoc(r, read);
                string notOpen = CheckOpen(ctx, d, line.LineId, read);
                if (notOpen != null) return Release(ctx, item.id, notOpen);
                docs.Add(d);
            }
            if (item.docRowId > 0 && Util.GetValueOfString(chosen[0]["VA012_IsReserved"]) != "Y")
            {
                string moved = MoveReservation(ctx, item.id, item.docRowId, docs[0].ReserveKey);
                if (moved != null) return moved;
            }

            var result = new ArResult
            {
                Line = line,
                Status = "S",
                Docs = docs,
                Difference = line.Amount - docs.Sum(d => d.Amount),
                Rule = info.Settings.Rules.FirstOrDefault(r => r.Id == Util.GetValueOfInt(s["VA012_MatchRule_ID"]))
            };
            var choices = new ArPostChoices
            {
                DifferenceType = !string.IsNullOrEmpty(item.differenceType) ? item.differenceType : Util.GetValueOfString(s["VA012_DifferenceType"]),
                ChargeId = item.chargeId,
                TaxId = item.taxId,
                // Set method / type, saved on the suggestion (the run stores the rule's or the document's method there)
                PayMethodId = Util.GetValueOfInt(s["VA009_PaymentMethod_ID"]),
                ConversionTypeId = Util.GetValueOfInt(s["C_ConversionType_ID"]),
                ChequeNo = Util.GetValueOfString(s["VA012_ChequeNo"]),
                ChequeDate = ParseDay(item.chequeDate)
            };
            // the line's own document decides: in an all-types run, and for a document dragged onto the line, it can differ from the run's type
            if (docType != docs[0].Kind) docType = docs[0].Kind;
            string why = new AutoReconcilePoster(ctx, info, docType).Post(result, choices);
            if (why != null) return why;

            DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'A', VA012_Reason = NULL WHERE VA012_MatchSuggestion_ID = @id",
                new[] { new SqlParameter("@id", item.id) }, null);
            AutoReconcileModel.DeleteCandidates(item.id);
            return null;
        }

        private static List<DataRow> DocRows(int suggestionId, Trx read)
        {
            DataSet ds = DB.ExecuteDataset("SELECT MSD.VA012_MatchSuggestionDoc_ID, MSD.VA012_IsReserved, MSD.VA012_Amount, MSD.C_Payment_ID,"
                + " MSD.C_InvoicePaySchedule_ID, MSD.C_Order_ID, MSD.C_CashLine_ID FROM VA012_MatchSuggestionDoc MSD"
                + " WHERE MSD.IsActive = 'Y' AND MSD.VA012_MatchSuggestion_ID = @id ORDER BY MSD.VA012_MatchSuggestionDoc_ID",
                new[] { new SqlParameter("@id", suggestionId) }, read);
            return ds == null || ds.Tables.Count == 0 ? new List<DataRow>() : ds.Tables[0].Rows.Cast<DataRow>().ToList();
        }

        /// <summary>The document as the poster needs it: kind, id, amount compared, partner, date and payment method.</summary>
        private static ArDoc ReadDoc(DataRow r, Trx read)
        {
            var d = new ArDoc { Amount = Util.GetValueOfDecimal(r["VA012_Amount"]) };
            string sql;
            if (Util.GetValueOfInt(r["C_Payment_ID"]) > 0)
            {
                d.Kind = "PY"; d.Id = Util.GetValueOfInt(r["C_Payment_ID"]);
                sql = "SELECT PAY.DocumentNo, PAY.C_BPartner_ID, PAY.DateAcct AS DocDate, PAY.VA009_PaymentMethod_ID AS PayMethodId FROM C_Payment PAY WHERE PAY.C_Payment_ID = @id";
            }
            else if (Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]) > 0)
            {
                d.Kind = "IS"; d.Id = Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]);
                sql = "SELECT INV.DocumentNo, INV.C_BPartner_ID, IPS.DueDate AS DocDate, IPS.VA009_PaymentMethod_ID AS PayMethodId"
                    + " FROM C_InvoicePaySchedule IPS INNER JOIN C_Invoice INV ON (INV.C_Invoice_ID = IPS.C_Invoice_ID) WHERE IPS.C_InvoicePaySchedule_ID = @id";
            }
            else if (Util.GetValueOfInt(r["C_Order_ID"]) > 0)
            {
                d.Kind = "PO"; d.Id = Util.GetValueOfInt(r["C_Order_ID"]);
                sql = "SELECT ORD.DocumentNo, ORD.C_BPartner_ID, ORD.DateOrdered AS DocDate, ORD.VA009_PaymentMethod_ID AS PayMethodId FROM C_Order ORD WHERE ORD.C_Order_ID = @id";
            }
            else
            {
                d.Kind = "CO"; d.Id = Util.GetValueOfInt(r["C_CashLine_ID"]);
                sql = "SELECT CS.DocumentNo, CL.C_BPartner_ID, CS.DateAcct AS DocDate, 0 AS PayMethodId"
                    + " FROM C_CashLine CL INNER JOIN C_Cash CS ON (CS.C_Cash_ID = CL.C_Cash_ID) WHERE CL.C_CashLine_ID = @id";
            }
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@id", d.Id) }, read);
            if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                DataRow x = ds.Tables[0].Rows[0];
                d.DocumentNo = Util.GetValueOfString(x["DocumentNo"]);
                d.BPartnerId = Util.GetValueOfInt(x["C_BPartner_ID"]);
                d.DocDate = Util.GetValueOfDateTime(x["DocDate"]) ?? DateTime.MinValue;
                d.PayMethodId = Util.GetValueOfInt(x["PayMethodId"]);
            }
            return d;
        }

        /// <summary>Null when the document can still be matched; otherwise why not (matched or paid elsewhere meanwhile).</summary>
        private static string CheckOpen(Ctx ctx, ArDoc d, int lineId, Trx read)
        {
            string sql;
            switch (d.Kind)
            {
                case "PY":
                    sql = "SELECT COUNT(PAY.C_Payment_ID) FROM C_Payment PAY WHERE PAY.C_Payment_ID = @id AND PAY.DocStatus IN ('CO', 'CL') AND PAY.IsReconciled = 'N'"
                        + " AND NOT EXISTS (SELECT 1 FROM C_BankStatementLine BSLP INNER JOIN C_BankStatement BSP ON (BSP.C_BankStatement_ID = BSLP.C_BankStatement_ID)"
                        + " WHERE BSLP.C_Payment_ID = PAY.C_Payment_ID AND BSP.DocStatus NOT IN ('VO', 'RE') AND BSLP.C_BankStatementLine_ID <> @line)";
                    break;
                case "IS":
                    sql = "SELECT COUNT(IPS.C_InvoicePaySchedule_ID) FROM C_InvoicePaySchedule IPS WHERE IPS.C_InvoicePaySchedule_ID = @id AND IPS.VA009_IsPaid = 'N'"
                        + " AND @line > 0 AND NOT EXISTS (SELECT 1 FROM C_Payment PD WHERE PD.C_InvoicePaySchedule_ID = IPS.C_InvoicePaySchedule_ID AND PD.DocStatus NOT IN ('VO', 'RE'))"
                        + " AND NOT EXISTS (SELECT 1 FROM C_PaymentAllocate PAL INNER JOIN C_Payment PA2 ON (PA2.C_Payment_ID = PAL.C_Payment_ID)"
                        + " WHERE PAL.C_InvoicePaySchedule_ID = IPS.C_InvoicePaySchedule_ID AND PA2.DocStatus NOT IN ('VO', 'RE'))";
                    break;
                case "PO":
                    sql = "SELECT COUNT(ORD.C_Order_ID) FROM C_Order ORD WHERE ORD.C_Order_ID = @id AND ORD.DocStatus = 'WP' AND @line > 0"
                        + " AND NOT EXISTS (SELECT 1 FROM C_Payment PD WHERE PD.C_Order_ID = ORD.C_Order_ID AND PD.DocStatus NOT IN ('VO', 'RE'))";
                    break;
                default:
                    sql = "SELECT COUNT(CL.C_CashLine_ID) FROM C_CashLine CL WHERE CL.C_CashLine_ID = @id AND COALESCE(CL.VA012_IsReconciled, 'N') = 'N'"
                        + " AND NOT EXISTS (SELECT 1 FROM C_BankStatementLine BSLC INNER JOIN C_BankStatement BSC ON (BSC.C_BankStatement_ID = BSLC.C_BankStatement_ID)"
                        + " WHERE BSLC.C_CashLine_ID = CL.C_CashLine_ID AND BSC.DocStatus NOT IN ('VO', 'RE') AND BSLC.C_BankStatementLine_ID <> @line)";
                    break;
            }
            int open = Util.GetValueOfInt(DB.ExecuteScalar(sql, new[] { new SqlParameter("@id", d.Id), new SqlParameter("@line", lineId) }, read));
            return open > 0 ? null
                : (d.DocumentNo ?? d.Id.ToString(CultureInfo.InvariantCulture)) + " " + Text(ctx, "VA012_ARDocNoLongerOpen", "was matched or paid elsewhere meanwhile.");
        }

        /// <summary>The chosen candidate takes over the reservation; the unique key refuses it when another line holds it.</summary>
        private static string MoveReservation(Ctx ctx, int suggestionId, int docRowId, string reserveKey)
        {
            Trx trx = Trx.GetTrx(Trx.CreateTrxName("VA012ARM"));
            try
            {
                if (DB.ExecuteQuery("UPDATE VA012_MatchSuggestionDoc SET VA012_IsReserved = 'N', VA012_ReserveKey = NULL WHERE VA012_MatchSuggestion_ID = @s",
                        new[] { new SqlParameter("@s", suggestionId) }, trx) < 0
                    || DB.ExecuteQuery("UPDATE VA012_MatchSuggestionDoc SET VA012_IsReserved = 'Y', VA012_ReserveKey = @key WHERE VA012_MatchSuggestionDoc_ID = @d",
                        new[] { new SqlParameter("@key", reserveKey), new SqlParameter("@d", docRowId) }, trx) < 0)
                {
                    trx.Rollback();
                    return Text(ctx, "VA012_ARReservedElsewhere", "This document is reserved for another line.");
                }
                trx.Commit();
                return null;
            }
            catch (Exception e)
            {
                trx.Rollback();
                log.Log(Level.WARNING, "VA012 auto reconcile move reservation", e);
                return Text(ctx, "VA012_ARReservedElsewhere", "This document is reserved for another line.");
            }
            finally { trx.Close(); }
        }

        /// <summary>Releases the suggestion (R) and its reservations, with the reason.</summary>
        private static string Release(Ctx ctx, int suggestionId, string reason)
        {
            DB.ExecuteQuery("UPDATE VA012_MatchSuggestionDoc SET VA012_IsReserved = 'N', VA012_ReserveKey = NULL WHERE VA012_MatchSuggestion_ID = @s",
                new[] { new SqlParameter("@s", suggestionId) }, null);
            DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchStatus = 'R', VA012_LiveLine = NULL, VA012_Reason = @reason WHERE VA012_MatchSuggestion_ID = @s",
                new[] { new SqlParameter("@reason", reason.Length > 255 ? reason.Substring(0, 255) : reason), new SqlParameter("@s", suggestionId) }, null);
            return reason;
        }

        /// <summary>A date sent by the dialog as yyyy-MM-dd; null when empty or not a date.</summary>
        private static DateTime? ParseDay(string value)
        {
            DateTime d;
            return !string.IsNullOrWhiteSpace(value) && DateTime.TryParseExact(value.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out d)
                ? d : (DateTime?)null;
        }

        private static string Text(Ctx ctx, string key, string fallback)
        {
            string t = Msg.GetMsg(ctx, key);
            return string.IsNullOrEmpty(t) || t == key || t == "[" + key + "]" ? fallback : t;
        }
    }
}
