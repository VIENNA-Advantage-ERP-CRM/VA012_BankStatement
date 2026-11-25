/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto reconciliation - loads the run's lines and candidate documents (one query per kind)
 * Chronological  : Development
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using VAdvantage.Classes;
using VAdvantage.DataBase;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    /// <summary>What a run needs to know about the bank account, the statement and the rules header.</summary>
    public class ArRunInfo
    {
        public int BankAccountId;
        public int StatementId;
        public int ClientId;
        public int OrgId;
        public int BankCurrencyId;
        public int BankPrecision;
        public DateTime StatementDate;
        public int HeaderId;
        public ArSettings Settings = new ArSettings();
    }

    /// <summary>
    /// Set-based loading for one run: every query runs once per run, never once per line. Documents already reserved by
    /// a live suggestion, or already on a statement line, are left out in SQL.
    /// </summary>
    public class AutoReconcileCandidates
    {
        private const int InListChunk = 500;
        private readonly Ctx _ctx;
        private readonly Trx _trx;

        public AutoReconcileCandidates(Ctx ctx, Trx trx)
        {
            _ctx = ctx;
            _trx = trx;
        }

        /// <summary>Bank account, statement and rules header of the run.</summary>
        public ArRunInfo GetRunInfo(int bankAccountId, int statementId)
        {
            string sql = "SELECT BA.AD_Client_ID, BA.AD_Org_ID, BA.C_Currency_ID, CU.StdPrecision, BA.VA012_MatchHeader_ID, BS.StatementDate"
                + " FROM C_BankStatement BS INNER JOIN C_BankAccount BA ON (BA.C_BankAccount_ID = BS.C_BankAccount_ID)"
                + " INNER JOIN C_Currency CU ON (CU.C_Currency_ID = BA.C_Currency_ID)"
                + " WHERE BS.IsActive = 'Y' AND BS.C_BankStatement_ID = @stmt AND BS.C_BankAccount_ID = @acct";
            sql = MRole.GetDefault(_ctx).AddAccessSQL(sql, "BS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@stmt", statementId), new SqlParameter("@acct", bankAccountId) }, _trx);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0)
                throw new Exception("VA012_ARSelectStatement");
            DataRow r = ds.Tables[0].Rows[0];
            var info = new ArRunInfo
            {
                BankAccountId = bankAccountId,
                StatementId = statementId,
                ClientId = Util.GetValueOfInt(r["AD_Client_ID"]),
                OrgId = Util.GetValueOfInt(r["AD_Org_ID"]),
                BankCurrencyId = Util.GetValueOfInt(r["C_Currency_ID"]),
                BankPrecision = Util.GetValueOfInt(r["StdPrecision"]),
                HeaderId = Util.GetValueOfInt(r["VA012_MatchHeader_ID"]),
                StatementDate = Util.GetValueOfDateTime(r["StatementDate"]) ?? DateTime.Today
            };
            if (info.HeaderId > 0)
            {
                string hsql = "SELECT MH.VA012_AutoMatch, MH.VA012_DateWindow, MH.VA012_AmountTolerance, MH.C_Charge_ID, MH.C_Tax_ID"
                    + " FROM VA012_MatchHeader MH WHERE MH.IsActive = 'Y' AND MH.VA012_MatchHeader_ID = @hdr";
                hsql = MRole.GetDefault(_ctx).AddAccessSQL(hsql, "MH", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
                DataSet hds = DB.ExecuteDataset(hsql, new[] { new SqlParameter("@hdr", info.HeaderId) }, _trx);
                if (hds != null && hds.Tables.Count > 0 && hds.Tables[0].Rows.Count > 0)
                {
                    DataRow h = hds.Tables[0].Rows[0];
                    info.Settings.AutoMatch = Util.GetValueOfString(h["VA012_AutoMatch"]) != "N";
                    if (h["VA012_DateWindow"] != DBNull.Value) info.Settings.DateWindow = Util.GetValueOfInt(h["VA012_DateWindow"]);
                    if (h["VA012_AmountTolerance"] != DBNull.Value) info.Settings.Tolerance = Util.GetValueOfDecimal(h["VA012_AmountTolerance"]);
                    info.Settings.ChargeId = Util.GetValueOfInt(h["C_Charge_ID"]);
                    info.Settings.TaxId = Util.GetValueOfInt(h["C_Tax_ID"]);
                    info.Settings.HasDefaultChargeAndTax = info.Settings.ChargeId > 0 && info.Settings.TaxId > 0;
                    info.Settings.Rules = LoadRules(info.HeaderId);
                }
                else info.HeaderId = 0;                                       // an inactive header counts as no header
            }
            info.Settings.ChequeMethodIds = LoadChequeMethods();
            return info;
        }

        /// <summary>
        /// The document types a rule applies to: one Yes-No column per type, so the rule window can show them as checkboxes.
        /// (VA012_AppliesTo, the earlier comma-separated text, is no longer read; the dictionary script copied it into these.)
        /// </summary>
        public static readonly string[][] AppliesToColumns =
        {
            new[] { "PY", "VA012_IsPayment" }, new[] { "IS", "VA012_IsInvSchedule" }, new[] { "PO", "VA012_IsPrepayOrder" }, new[] { "CO", "VA012_IsContra" }
        };

        public static string AppliesToSelect(string alias)
        {
            return string.Join(", ", AppliesToColumns.Select(c => alias + "." + c[1]));
        }

        public static List<string> AppliesToCodes(DataRow r)
        {
            return AppliesToColumns.Where(c => Util.GetValueOfString(r[c[1]]) == "Y").Select(c => c[0]).ToList();
        }

        /// <summary>The header's active rules with their conditions and keys (three queries).</summary>
        private List<ArRule> LoadRules(int headerId)
        {
            var rules = new Dictionary<int, ArRule>();
            var p = new[] { new SqlParameter("@hdr", headerId) };
            string sql = "SELECT MR.VA012_MatchRule_ID, MR.SeqNo, MR.Name, " + AppliesToSelect("MR") + ", MR.VA012_DateWindow, MR.VA012_AmountTolerance,"
                + " MR.VA012_Control, MR.VA012_ControlCap, MR.VA012_DifferenceType, MR.VA012_OnDiff, MR.VA009_PaymentMethod_ID,"
                + " MR.C_ConversionType_ID, MR.VA012_CheckSource, MR.VA012_IsMultiDoc, MR.VA012_MinDocs, MR.VA012_OnMissing"
                + " FROM VA012_MatchRule MR WHERE MR.IsActive = 'Y' AND MR.VA012_MatchHeader_ID = @hdr";
            sql = MRole.GetDefault(_ctx).AddAccessSQL(sql, "MR", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, p, _trx);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return new List<ArRule>();
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                var rule = new ArRule
                {
                    Id = Util.GetValueOfInt(r["VA012_MatchRule_ID"]),
                    SeqNo = Util.GetValueOfInt(r["SeqNo"]),
                    Name = Util.GetValueOfString(r["Name"]),
                    DateWindow = r["VA012_DateWindow"] == DBNull.Value ? (int?)null : Util.GetValueOfInt(r["VA012_DateWindow"]),
                    Tolerance = r["VA012_AmountTolerance"] == DBNull.Value ? (decimal?)null : Util.GetValueOfDecimal(r["VA012_AmountTolerance"]),
                    AutoConfirm = Util.GetValueOfString(r["VA012_Control"]) == "A",
                    ControlCap = Util.GetValueOfDecimal(r["VA012_ControlCap"]),
                    DifferenceType = Util.GetValueOfString(r["VA012_DifferenceType"]),
                    DiffFromHeader = Util.GetValueOfString(r["VA012_OnDiff"]) == "H",
                    PayMethodId = Util.GetValueOfInt(r["VA009_PaymentMethod_ID"]),
                    ConversionTypeId = Util.GetValueOfInt(r["C_ConversionType_ID"]),
                    CheckSource = string.IsNullOrEmpty(Util.GetValueOfString(r["VA012_CheckSource"])) ? "S" : Util.GetValueOfString(r["VA012_CheckSource"]),
                    MultiDoc = Util.GetValueOfString(r["VA012_IsMultiDoc"]) != "N",
                    MinDocs = Math.Max(2, Util.GetValueOfInt(r["VA012_MinDocs"])),
                    SkipOnMissing = Util.GetValueOfString(r["VA012_OnMissing"]) == "S"
                };
                rule.AppliesTo.UnionWith(AppliesToCodes(r));
                rules[rule.Id] = rule;
            }

            string csql = "SELECT MRC.VA012_MatchRule_ID, MRC.VA012_CondField, MRC.VA012_CondOperator, MRC.VA012_Value, MRC.VA012_Value2"
                + " FROM VA012_MatchRuleCond MRC INNER JOIN VA012_MatchRule MR ON (MR.VA012_MatchRule_ID = MRC.VA012_MatchRule_ID)"
                + " WHERE MRC.IsActive = 'Y' AND MR.VA012_MatchHeader_ID = @hdr";
            csql = MRole.GetDefault(_ctx).AddAccessSQL(csql, "MRC", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY MRC.SeqNo, MRC.VA012_MatchRuleCond_ID";
            DataSet cds = DB.ExecuteDataset(csql, new[] { new SqlParameter("@hdr", headerId) }, _trx);
            if (cds != null && cds.Tables.Count > 0)
                foreach (DataRow r in cds.Tables[0].Rows)
                {
                    ArRule rule;
                    if (rules.TryGetValue(Util.GetValueOfInt(r["VA012_MatchRule_ID"]), out rule))
                        rule.Conditions.Add(new ArRuleCond
                        {
                            Field = Util.GetValueOfString(r["VA012_CondField"]),
                            Op = Util.GetValueOfString(r["VA012_CondOperator"]),
                            Value = Util.GetValueOfString(r["VA012_Value"]),
                            Value2 = Util.GetValueOfString(r["VA012_Value2"])
                        });
                }

            string ksql = "SELECT MRK.VA012_MatchRule_ID, MRK.VA012_MatchKey, MRK.VA012_IsRequired"
                + " FROM VA012_MatchRuleKey MRK INNER JOIN VA012_MatchRule MR ON (MR.VA012_MatchRule_ID = MRK.VA012_MatchRule_ID)"
                + " WHERE MRK.IsActive = 'Y' AND MR.VA012_MatchHeader_ID = @hdr";
            ksql = MRole.GetDefault(_ctx).AddAccessSQL(ksql, "MRK", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet kds = DB.ExecuteDataset(ksql, new[] { new SqlParameter("@hdr", headerId) }, _trx);
            if (kds != null && kds.Tables.Count > 0)
                foreach (DataRow r in kds.Tables[0].Rows)
                {
                    ArRule rule;
                    string key = Util.GetValueOfString(r["VA012_MatchKey"]);
                    if (key.Length > 0 && rules.TryGetValue(Util.GetValueOfInt(r["VA012_MatchRule_ID"]), out rule))
                        rule.Keys[key] = Util.GetValueOfString(r["VA012_IsRequired"]) == "Y";
                }
            return rules.Values.ToList();
        }

        /// <summary>Payment methods whose base type is cheque (S).</summary>
        private HashSet<int> LoadChequeMethods()
        {
            string sql = "SELECT PM.VA009_PaymentMethod_ID FROM VA009_PaymentMethod PM WHERE PM.IsActive = 'Y' AND PM.VA009_PaymentBaseType = 'S'";
            sql = MRole.GetDefault(_ctx).AddAccessSQL(sql, "PM", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, null, _trx);
            var ids = new HashSet<int>();
            if (ds != null && ds.Tables.Count > 0)
                foreach (DataRow r in ds.Tables[0].Rows) ids.Add(Util.GetValueOfInt(r["VA009_PaymentMethod_ID"]));
            return ids;
        }

        /// <summary>Open lines of the statement, as Match Statement selects them, without accepted or auto-matched suggestions.</summary>
        public List<ArLine> LoadLines(ArRunInfo info)
        {
            string sql = "SELECT BSL.C_BankStatementLine_ID, BSL.Line, BSL.StatementLineDate, BSL.StmtAmt, BSL.TrxNo, BSL.EftCheckNo,"
                + " BSL.C_BPartner_ID, BSL.Description, BSL.Memo, BSL.ReferenceNo"
                + " FROM C_BankStatementLine BSL"
                + " WHERE BSL.IsActive = 'Y' AND BSL.C_BankStatement_ID = @stmt AND COALESCE(BSL.VA012_IsMatchingConfirmed, 'N') = 'N'"
                + " AND BSL.C_Payment_ID IS NULL AND BSL.C_CashLine_ID IS NULL"
                + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestion MS WHERE MS.C_BankStatementLine_ID = BSL.C_BankStatementLine_ID"
                + " AND MS.VA012_MatchStatus IN ('A', 'C'))";
            sql = MRole.GetDefault(_ctx).AddAccessSQL(sql, "BSL", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY BSL.Line";
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@stmt", info.StatementId) }, _trx);
            var lines = new List<ArLine>();
            if (ds == null || ds.Tables.Count == 0) return lines;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                lines.Add(new ArLine
                {
                    LineId = Util.GetValueOfInt(r["C_BankStatementLine_ID"]),
                    LineNo = Util.GetValueOfInt(r["Line"]),
                    LineDate = Util.GetValueOfDateTime(r["StatementLineDate"]) ?? info.StatementDate,
                    Amount = Util.GetValueOfDecimal(r["StmtAmt"]),
                    TrxNo = Util.GetValueOfString(r["TrxNo"]),
                    ChequeNo = Util.GetValueOfString(r["EftCheckNo"]),
                    BPartnerId = Util.GetValueOfInt(r["C_BPartner_ID"]),
                    Narration = string.Join(" ", new[] { Util.GetValueOfString(r["Description"]), Util.GetValueOfString(r["Memo"]), Util.GetValueOfString(r["ReferenceNo"]) }
                        .Where(s => !string.IsNullOrWhiteSpace(s)))
                });
            }
            return lines;
        }

        /// <summary>"lineId|reserveKey" pairs the user declined on this statement, so a run does not suggest them again.</summary>
        public List<string> LoadDeclinedPairs(ArRunInfo info)
        {
            string sql = "SELECT MS.C_BankStatementLine_ID, MSD.C_Payment_ID, MSD.C_InvoicePaySchedule_ID, MSD.C_Order_ID, MSD.C_CashLine_ID"
                + " FROM VA012_MatchSuggestion MS"
                + " INNER JOIN VA012_MatchSuggestionDoc MSD ON (MSD.VA012_MatchSuggestion_ID = MS.VA012_MatchSuggestion_ID)"
                + " INNER JOIN C_BankStatementLine BSL ON (BSL.C_BankStatementLine_ID = MS.C_BankStatementLine_ID)"
                + " WHERE MS.IsActive = 'Y' AND MS.VA012_MatchStatus = 'D' AND BSL.C_BankStatement_ID = @stmt";
            sql = MRole.GetDefault(_ctx).AddAccessSQL(sql, "MS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@stmt", info.StatementId) }, _trx);
            var pairs = new List<string>();
            if (ds == null || ds.Tables.Count == 0) return pairs;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                string line = Util.GetValueOfInt(r["C_BankStatementLine_ID"]).ToString(CultureInfo.InvariantCulture) + "|";
                if (Util.GetValueOfInt(r["C_Payment_ID"]) > 0) pairs.Add(line + "PY_" + Util.GetValueOfInt(r["C_Payment_ID"]));
                if (Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]) > 0) pairs.Add(line + "IS_" + Util.GetValueOfInt(r["C_InvoicePaySchedule_ID"]));
                if (Util.GetValueOfInt(r["C_Order_ID"]) > 0) pairs.Add(line + "PO_" + Util.GetValueOfInt(r["C_Order_ID"]));
                if (Util.GetValueOfInt(r["C_CashLine_ID"]) > 0) pairs.Add(line + "CO_" + Util.GetValueOfInt(r["C_CashLine_ID"]));
            }
            return pairs;
        }

        /// <summary>Open, unreserved documents of the run's type, optionally narrowed by payment method and partner.</summary>
        public List<ArDoc> LoadDocs(ArRunInfo info, string docType, int payMethodId, int bpId, List<ArLine> lines)
        {
            switch (docType)
            {
                case "PY": return LoadPayments(info, payMethodId, bpId);
                case "IS": return LoadSchedules(info, payMethodId, bpId, lines);
                case "PO": return LoadPrepayOrders(info, payMethodId, bpId);
                case "CO": return LoadContra(info, bpId);
                default: throw new ArgumentException("docType");
            }
        }

        /// <summary>
        /// One document a user dragged onto a line, read with the run's own query for its type (bank-currency amount with the
        /// statement's sign, withholding for schedules); null when it is not open for this statement or another line holds it.
        /// </summary>
        public ArDoc LoadDoc(ArRunInfo info, string kind, int docId)
        {
            if (docId <= 0) return null;
            List<ArDoc> docs;
            switch (kind)
            {
                case "PY": docs = LoadPayments(info, 0, 0, docId); break;
                case "IS":
                    var p = new List<SqlParameter> { new SqlParameter("@cur1", info.BankCurrencyId), new SqlParameter("@cur2", info.BankCurrencyId) };
                    var sql = new StringBuilder(ScheduleSelect(info));
                    AddFilters(sql, p, "IPS.AD_Org_ID", info.OrgId, null, 0, null, 0);
                    sql.Append(" AND IPS.C_InvoicePaySchedule_ID = @docid");
                    p.Add(new SqlParameter("@docid", docId));
                    docs = ReadDocs("IS", sql.ToString(), "IPS", p);
                    break;
                case "PO": docs = LoadPrepayOrders(info, 0, 0, docId); break;
                case "CO": docs = LoadContra(info, 0, docId); break;
                default: return null;
            }
            return docs.FirstOrDefault();
        }

        private List<ArDoc> LoadPayments(ArRunInfo info, int payMethodId, int bpId, int docId = 0)
        {
            var p = new List<SqlParameter>();
            var sql = new StringBuilder("SELECT PAY.C_Payment_ID AS DocId, PAY.DocumentNo, PAY.C_BPartner_ID, BP.Name AS BPName, BP.Value AS BPValue,"
                + " (CASE WHEN DT.DocBaseType = 'APP' THEN -1 ELSE 1 END) * (CASE WHEN PAY.C_Currency_ID <> @cur1"
                + " THEN CURRENCYCONVERT(PAY.PayAmt, PAY.C_Currency_ID, @cur2, " + Date(info.StatementDate) + ", PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID)"
                + " ELSE PAY.PayAmt END) AS Amount,"
                + " PAY.DateAcct AS DocDate, PAY.TrxNo, PAY.CheckNo AS ChequeNo, PAY.VA009_PaymentMethod_ID AS PayMethodId"
                + " FROM C_Payment PAY"
                + " INNER JOIN C_DocType DT ON (DT.C_DocType_ID = PAY.C_DocType_ID)"
                + " LEFT OUTER JOIN C_BPartner BP ON (BP.C_BPartner_ID = PAY.C_BPartner_ID)"
                + " LEFT OUTER JOIN VA009_PaymentMethod PM ON (PM.VA009_PaymentMethod_ID = PAY.VA009_PaymentMethod_ID)"
                + " WHERE PAY.IsActive = 'Y' AND PAY.DocStatus IN ('CO', 'CL') AND PAY.IsReconciled = 'N' AND PAY.C_BankAccount_ID = @acct"
                + " AND (PM.VA009_PaymentBaseType <> 'B' OR PM.VA009_PaymentBaseType IS NULL)"
                + " AND PAY.DateAcct <= " + Date(info.StatementDate)
                + " AND NOT EXISTS (SELECT 1 FROM C_BankStatementLine BSLP INNER JOIN C_BankStatement BSP ON (BSP.C_BankStatement_ID = BSLP.C_BankStatement_ID)"
                + " WHERE BSLP.C_Payment_ID = PAY.C_Payment_ID AND BSP.DocStatus NOT IN ('VO', 'RE'))"
                + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSD WHERE MSD.VA012_ReserveKey = 'PY_' || PAY.C_Payment_ID)");
            p.Add(new SqlParameter("@cur1", info.BankCurrencyId));
            p.Add(new SqlParameter("@cur2", info.BankCurrencyId));
            p.Add(new SqlParameter("@acct", info.BankAccountId));
            AddFilters(sql, p, "PAY.AD_Org_ID", info.OrgId, "PAY.VA009_PaymentMethod_ID", payMethodId, "PAY.C_BPartner_ID", bpId);
            if (docId > 0) { sql.Append(" AND PAY.C_Payment_ID = @docid"); p.Add(new SqlParameter("@docid", docId)); }
            return ReadDocs("PY", sql.ToString(), "PAY", p);
        }

        /// <summary>Open, unreserved invoice schedules: FROM and WHERE, without the org, method and partner filters.</summary>
        private static string ScheduleOpen(ArRunInfo info)
        {
            return " FROM C_InvoicePaySchedule IPS"
                + " INNER JOIN C_Invoice INV ON (INV.C_Invoice_ID = IPS.C_Invoice_ID)"
                + " INNER JOIN C_DocType DT ON (DT.C_DocType_ID = INV.C_DocType_ID)"
                + " INNER JOIN C_BPartner BP ON (BP.C_BPartner_ID = INV.C_BPartner_ID)"
                + " INNER JOIN VA009_PaymentMethod PM ON (PM.VA009_PaymentMethod_ID = IPS.VA009_PaymentMethod_ID)"
                + " LEFT OUTER JOIN C_Withholding WH ON (WH.C_Withholding_ID = BP.AP_WithholdingTax_ID)"
                + " LEFT OUTER JOIN C_Withholding WHAR ON (WHAR.C_Withholding_ID = BP.C_Withholding_ID)"
                + " WHERE IPS.IsActive = 'Y' AND IPS.VA009_IsPaid = 'N' AND INV.DocStatus IN ('CO', 'CL') AND PM.VA009_PaymentBaseType <> 'B'"
                + " AND IPS.VA009_ExecutionStatus NOT IN ('Y', 'J') AND INV.DateAcct <= " + Date(info.StatementDate)
                + " AND NOT EXISTS (SELECT 1 FROM C_Payment PD WHERE PD.C_InvoicePaySchedule_ID = IPS.C_InvoicePaySchedule_ID"
                + " AND PD.DocStatus NOT IN ('CO', 'CL', 'RE', 'VO'))"
                + " AND NOT EXISTS (SELECT 1 FROM C_PaymentAllocate PAL INNER JOIN C_Payment PA2 ON (PA2.C_Payment_ID = PAL.C_Payment_ID)"
                + " WHERE PAL.C_InvoicePaySchedule_ID = IPS.C_InvoicePaySchedule_ID AND PA2.DocStatus NOT IN ('CO', 'CL', 'RE', 'VO'))"
                + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSD WHERE MSD.VA012_ReserveKey = 'IS_' || IPS.C_InvoicePaySchedule_ID)";
        }

        /// <summary>The schedule columns a run compares: amount in the bank's currency with the sign and less withholding (binds @cur1, @cur2).</summary>
        private static string ScheduleSelect(ArRunInfo info)
        {
            return "SELECT IPS.C_InvoicePaySchedule_ID AS DocId, INV.DocumentNo, INV.C_BPartner_ID, BP.Name AS BPName, BP.Value AS BPValue,"
                + " (CASE WHEN DT.DocBaseType IN ('API', 'ARC') THEN -1 ELSE 1 END)"
                + " * (CASE WHEN INV.C_Currency_ID <> @cur1"
                + " THEN CURRENCYCONVERT(IPS.DueAmt, INV.C_Currency_ID, @cur2, " + Date(info.StatementDate) + ", INV.C_ConversionType_ID, INV.AD_Client_ID, INV.AD_Org_ID)"
                + " ELSE IPS.DueAmt END)"
                + " * (1 - (CASE WHEN (COALESCE(BP.AP_WithholdingTax_ID, 0) > 0 AND BP.IsApplicableonAPPayment = 'Y' AND DT.DocBaseType IN ('API', 'APC'))"
                + " THEN COALESCE(WH.PayPercentage, 0)"
                + " WHEN (COALESCE(BP.C_Withholding_ID, 0) > 0 AND BP.IsApplicableonARReceipt = 'Y' AND DT.DocBaseType IN ('ARI', 'ARC'))"
                + " THEN COALESCE(WHAR.PayPercentage, 0) ELSE 0 END) / 100) AS Amount,"
                + " IPS.DueDate AS DocDate, NULL AS TrxNo, NULL AS ChequeNo, IPS.VA009_PaymentMethod_ID AS PayMethodId"
                + ScheduleOpen(info);
        }

        private List<ArDoc> LoadSchedules(ArRunInfo info, int payMethodId, int bpId, List<ArLine> lines)
        {
            string open = ScheduleOpen(info);

            // 1. partners with open schedules (id, name, search key), to read partners from the narrations
            var pp = new List<SqlParameter>();
            var psql = new StringBuilder("SELECT DISTINCT BP.C_BPartner_ID, BP.Name, BP.Value" + open);
            AddFilters(psql, pp, "IPS.AD_Org_ID", info.OrgId, "IPS.VA009_PaymentMethod_ID", payMethodId, "INV.C_BPartner_ID", bpId);
            string partnerSql = MRole.GetDefault(_ctx).AddAccessSQL(psql.ToString(), "IPS", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet pds = DB.ExecuteDataset(partnerSql, pp.ToArray(), _trx);
            var partnerKeys = AutoReconcileMatcher.BuildPartnerKeys(pds == null || pds.Tables.Count == 0 ? Enumerable.Empty<DataRow>() : pds.Tables[0].Rows.Cast<DataRow>(),
                r => Util.GetValueOfInt(r["C_BPartner_ID"]), r => Util.GetValueOfString(r["Name"]), r => Util.GetValueOfString(r["Value"]));

            // 2. schedules of those partners, or named in a narration
            var partners = new HashSet<int>(lines.Where(l => l.BPartnerId > 0).Select(l => l.BPartnerId));
            foreach (ArLine l in lines) partners.UnionWith(AutoReconcileMatcher.PartnersStartingWith(l.Narration, partnerKeys));
            var tokens = new HashSet<string>(lines.SelectMany(l => AutoReconcileMatcher.Tokens(l.Narration))
                .Select(t => t.ToUpperInvariant()).Where(t => t.Length >= 4), StringComparer.Ordinal);

            string select = ScheduleSelect(info);
            Action<StringBuilder, List<SqlParameter>> filters = (s, prm) =>
                AddFilters(s, prm, "IPS.AD_Org_ID", info.OrgId, "IPS.VA009_PaymentMethod_ID", payMethodId, "INV.C_BPartner_ID", bpId);
            var docs = new Dictionary<int, ArDoc>();

            if (partners.Count > 0 || tokens.Count > 0)
            {
                var p = new List<SqlParameter> { new SqlParameter("@cur1", info.BankCurrencyId), new SqlParameter("@cur2", info.BankCurrencyId) };
                var sql = new StringBuilder(select);
                filters(sql, p);
                sql.Append(" AND (1 = 0");
                AppendInList(sql, p, "INV.C_BPartner_ID", partners.Cast<object>().ToList(), "bp");
                AppendInList(sql, p, "UPPER(INV.DocumentNo)", tokens.Cast<object>().ToList(), "dn");
                sql.Append(")");
                foreach (ArDoc d in ReadDocs("IS", sql.ToString(), "IPS", p)) docs[d.Id] = d;
            }

            // 3. an invoice-schedule rule that matches on Amount: also schedules near a line amount, whatever the partner
            decimal? tolerance = AmountRuleTolerance(info, "IS");
            if (tolerance.HasValue && lines.Count > 0)
                foreach (ArDoc d in LoadByAmount("IS", select, "IPS", filters, info, lines, tolerance.Value)) docs[d.Id] = d;
            return docs.Values.ToList();
        }

        /// <summary>The largest amount tolerance of the active rules of this type that match on Amount; null when there is none.</summary>
        private static decimal? AmountRuleTolerance(ArRunInfo info, string docType)
        {
            decimal? max = null;
            foreach (ArRule rule in info.Settings.Rules)
            {
                if (!rule.AppliesTo.Contains(docType) || !rule.Keys.ContainsKey("AM")) continue;
                decimal t = rule.Tolerance ?? info.Settings.Tolerance;
                if (max == null || t > max.Value) max = t;
            }
            return max;
        }

        /// <summary>
        /// Open documents whose amount (bank currency, as the select computes it) is within the tolerance of a line amount.
        /// One query loads the open documents with their amount, computed once each; the line amounts become merged ranges
        /// and the documents are kept by binary search, so the cost does not grow with the number of lines.
        /// </summary>
        private List<ArDoc> LoadByAmount(string kind, string select, string mainAlias, Action<StringBuilder, List<SqlParameter>> filters,
            ArRunInfo info, List<ArLine> lines, decimal tolerance)
        {
            var ranges = new List<KeyValuePair<decimal, decimal>>();
            foreach (decimal a in lines.Select(l => l.Amount).Distinct().OrderBy(a => a))
            {
                decimal low = a - tolerance, high = a + tolerance;
                if (ranges.Count > 0 && low <= ranges[ranges.Count - 1].Value)
                    ranges[ranges.Count - 1] = new KeyValuePair<decimal, decimal>(ranges[ranges.Count - 1].Key, Math.Max(high, ranges[ranges.Count - 1].Value));
                else ranges.Add(new KeyValuePair<decimal, decimal>(low, high));
            }
            if (ranges.Count == 0) return new List<ArDoc>();

            var p = new List<SqlParameter> { new SqlParameter("@cur1", info.BankCurrencyId), new SqlParameter("@cur2", info.BankCurrencyId) };
            var sql = new StringBuilder(select);
            filters(sql, p);
            return ReadDocs(kind, sql.ToString(), mainAlias, p).Where(d => InRanges(d.Amount, ranges)).ToList();
        }

        /// <summary>Whether the amount falls in one of the sorted, non-overlapping ranges.</summary>
        private static bool InRanges(decimal amount, List<KeyValuePair<decimal, decimal>> ranges)
        {
            int lo = 0, hi = ranges.Count - 1;
            while (lo <= hi)
            {
                int mid = (lo + hi) / 2;
                if (amount < ranges[mid].Key) hi = mid - 1;
                else if (amount > ranges[mid].Value) lo = mid + 1;
                else return true;
            }
            return false;
        }

        private List<ArDoc> LoadPrepayOrders(ArRunInfo info, int payMethodId, int bpId, int docId = 0)
        {
            var p = new List<SqlParameter>();
            var sql = new StringBuilder("SELECT ORD.C_Order_ID AS DocId, ORD.DocumentNo, ORD.C_BPartner_ID, BP.Name AS BPName, BP.Value AS BPValue,"
                + " (CASE WHEN ORD.C_Currency_ID <> @cur1"
                + " THEN CURRENCYCONVERT(ORD.GrandTotal, ORD.C_Currency_ID, @cur2, " + Date(info.StatementDate) + ", ORD.C_ConversionType_ID, ORD.AD_Client_ID, ORD.AD_Org_ID)"
                + " ELSE ORD.GrandTotal END) AS Amount,"
                + " ORD.DateOrdered AS DocDate, NULL AS TrxNo, NULL AS ChequeNo, ORD.VA009_PaymentMethod_ID AS PayMethodId"
                + " FROM C_Order ORD"
                + " INNER JOIN C_DocType DT ON (DT.C_DocType_ID = ORD.C_DocTypeTarget_ID)"
                + " LEFT OUTER JOIN C_BPartner BP ON (BP.C_BPartner_ID = ORD.C_BPartner_ID)"
                + " INNER JOIN VA009_PaymentMethod PM ON (PM.VA009_PaymentMethod_ID = ORD.VA009_PaymentMethod_ID)"
                + " WHERE ORD.IsActive = 'Y' AND DT.DocSubTypeSO = 'PR' AND ORD.DocStatus = 'WP' AND PM.VA009_PaymentBaseType <> 'B'"
                + " AND ORD.DateAcct <= " + Date(info.StatementDate)
                + " AND NOT EXISTS (SELECT 1 FROM C_Payment PD WHERE PD.C_Order_ID = ORD.C_Order_ID AND PD.DocStatus NOT IN ('CO', 'CL', 'RE', 'VO'))"
                + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSD WHERE MSD.VA012_ReserveKey = 'PO_' || ORD.C_Order_ID)");
            p.Add(new SqlParameter("@cur1", info.BankCurrencyId));
            p.Add(new SqlParameter("@cur2", info.BankCurrencyId));
            AddFilters(sql, p, "ORD.AD_Org_ID", info.OrgId, "ORD.VA009_PaymentMethod_ID", payMethodId, "ORD.C_BPartner_ID", bpId);
            if (docId > 0) { sql.Append(" AND ORD.C_Order_ID = @docid"); p.Add(new SqlParameter("@docid", docId)); }
            return ReadDocs("PO", sql.ToString(), "ORD", p);
        }

        private List<ArDoc> LoadContra(ArRunInfo info, int bpId, int docId = 0)
        {
            var p = new List<SqlParameter>();
            var sql = new StringBuilder("SELECT CL.C_CashLine_ID AS DocId, CS.DocumentNo, CL.C_BPartner_ID, BP.Name AS BPName, BP.Value AS BPValue,"
                + " (CASE WHEN CL.C_Currency_ID <> @cur1"
                + " THEN CURRENCYCONVERT(CL.Amount * -1, CL.C_Currency_ID, @cur2, " + Date(info.StatementDate) + ", CL.C_ConversionType_ID, CS.AD_Client_ID, CS.AD_Org_ID)"
                + " ELSE CL.Amount * -1 END) AS Amount,"
                + " CS.DateAcct AS DocDate, NULL AS TrxNo, CL.CheckNo AS ChequeNo, 0 AS PayMethodId"
                + " FROM C_CashLine CL"
                + " INNER JOIN C_Cash CS ON (CS.C_Cash_ID = CL.C_Cash_ID)"
                + " INNER JOIN C_Charge CH ON (CH.C_Charge_ID = CL.C_Charge_ID)"
                + " LEFT OUTER JOIN C_BPartner BP ON (BP.C_BPartner_ID = CL.C_BPartner_ID)"
                + " WHERE CS.IsActive = 'Y' AND CL.IsActive = 'Y' AND CL.CashType = 'C' AND CH.DTD001_ChargeType = 'CON'"
                + " AND CS.DocStatus IN ('CO', 'CL') AND COALESCE(CL.VA012_IsReconciled, 'N') = 'N' AND CL.C_BankAccount_ID = @acct"
                + " AND CS.DateAcct <= " + Date(info.StatementDate)
                + " AND NOT EXISTS (SELECT 1 FROM C_BankStatementLine BSLC INNER JOIN C_BankStatement BSC ON (BSC.C_BankStatement_ID = BSLC.C_BankStatement_ID)"
                + " WHERE BSLC.C_CashLine_ID = CL.C_CashLine_ID AND BSC.DocStatus NOT IN ('VO', 'RE'))"
                + " AND NOT EXISTS (SELECT 1 FROM VA012_MatchSuggestionDoc MSD WHERE MSD.VA012_ReserveKey = 'CO_' || CL.C_CashLine_ID)");
            p.Add(new SqlParameter("@cur1", info.BankCurrencyId));
            p.Add(new SqlParameter("@cur2", info.BankCurrencyId));
            p.Add(new SqlParameter("@acct", info.BankAccountId));
            AddFilters(sql, p, "CS.AD_Org_ID", info.OrgId, null, 0, "CL.C_BPartner_ID", bpId);
            if (docId > 0) { sql.Append(" AND CL.C_CashLine_ID = @docid"); p.Add(new SqlParameter("@docid", docId)); }
            return ReadDocs("CO", sql.ToString(), "CL", p);
        }

        // ------------------------------------------------------------------ helpers
        private List<ArDoc> ReadDocs(string kind, string sql, string mainAlias, List<SqlParameter> p)
        {
            sql = MRole.GetDefault(_ctx).AddAccessSQL(sql, mainAlias, MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, p.ToArray(), _trx);
            var docs = new List<ArDoc>();
            if (ds == null || ds.Tables.Count == 0) return docs;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                docs.Add(new ArDoc
                {
                    Kind = kind,
                    Id = Util.GetValueOfInt(r["DocId"]),
                    DocumentNo = Util.GetValueOfString(r["DocumentNo"]),
                    BPartnerId = Util.GetValueOfInt(r["C_BPartner_ID"]),
                    BPartnerName = Util.GetValueOfString(r["BPName"]),
                    BPartnerValue = Util.GetValueOfString(r["BPValue"]),
                    Amount = Math.Round(Util.GetValueOfDecimal(r["Amount"]), 2, MidpointRounding.AwayFromZero),
                    DocDate = Util.GetValueOfDateTime(r["DocDate"]) ?? DateTime.MinValue,
                    TrxNo = Util.GetValueOfString(r["TrxNo"]),
                    ChequeNo = Util.GetValueOfString(r["ChequeNo"]),
                    PayMethodId = Util.GetValueOfInt(r["PayMethodId"])
                });
            }
            return docs;
        }

        private static void AddFilters(StringBuilder sql, List<SqlParameter> p, string orgColumn, int orgId,
            string payMethodColumn, int payMethodId, string bpColumn, int bpId)
        {
            if (orgId > 0) { sql.Append(" AND ").Append(orgColumn).Append(" = @org"); p.Add(new SqlParameter("@org", orgId)); }
            if (payMethodColumn != null && payMethodId > 0) { sql.Append(" AND ").Append(payMethodColumn).Append(" = @pm"); p.Add(new SqlParameter("@pm", payMethodId)); }
            if (bpId > 0) { sql.Append(" AND ").Append(bpColumn).Append(" = @bpf"); p.Add(new SqlParameter("@bpf", bpId)); }
        }

        /// <summary>Appends "OR column IN (...)" in chunks, one bind parameter per value.</summary>
        private static void AppendInList(StringBuilder sql, List<SqlParameter> p, string column, List<object> values, string prefix)
        {
            for (int start = 0; start < values.Count; start += InListChunk)
            {
                sql.Append(" OR ").Append(column).Append(" IN (");
                int end = Math.Min(start + InListChunk, values.Count);
                for (int i = start; i < end; i++)
                {
                    string name = "@" + prefix + i.ToString(CultureInfo.InvariantCulture);
                    if (i > start) sql.Append(", ");
                    sql.Append(name);
                    p.Add(new SqlParameter(name, values[i]));
                }
                sql.Append(")");
            }
        }

        private static string Date(DateTime value)
        {
            return GlobalVariable.TO_DATE(value, true);
        }
    }
}
