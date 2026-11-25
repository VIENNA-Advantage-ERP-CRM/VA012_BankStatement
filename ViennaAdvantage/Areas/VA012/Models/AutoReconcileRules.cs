/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto reconciliation - rules setup: the bank account's rules header and its rules
 * Chronological  : Development
 * Design         : VA012 Auto Reconciliation Solution Approach v4, section 5
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    /// <summary>Header settings as the dialog sends them.</summary>
    public class ArHeaderDto
    {
        public string name { get; set; }
        public bool active { get; set; }
        public bool autoMatch { get; set; }
        public int dateWindow { get; set; }
        public decimal tolerance { get; set; }
        public int chargeId { get; set; }
        public int taxId { get; set; }
    }

    public class ArCondDto
    {
        public string field { get; set; }
        public string op { get; set; }
        public string value { get; set; }
        public string value2 { get; set; }
    }

    public class ArKeyDto
    {
        public string key { get; set; }
        public bool required { get; set; }
    }

    /// <summary>A rule with its conditions and keys, as the dialog sends and receives it.</summary>
    public class ArRuleDto
    {
        public int id { get; set; }
        public int seqNo { get; set; }
        public string name { get; set; }
        public bool active { get; set; }
        public List<string> appliesTo { get; set; }
        public int? dateWindow { get; set; }
        public decimal? tolerance { get; set; }
        public string control { get; set; }
        public decimal? controlCap { get; set; }
        public string differenceType { get; set; }
        public string onDiff { get; set; }
        public int payMethodId { get; set; }
        public int conversionTypeId { get; set; }
        public string checkSource { get; set; }
        public bool multiDoc { get; set; }
        public int minDocs { get; set; }
        public string onMissing { get; set; }
        public List<ArCondDto> conds { get; set; }
        public List<ArKeyDto> keys { get; set; }
    }

    /// <summary>
    /// Rules setup for the Auto Reconcile dialog. A header can serve several bank accounts: linking only sets
    /// C_BankAccount.VA012_MatchHeader_ID, copying creates a new header with its own copy of the rules.
    /// Headers and rules are created in organisation * so a header can be linked to any account of the tenant.
    /// </summary>
    public class AutoReconcileRules
    {
        private static readonly VLogger log = VLogger.GetVLogger(typeof(AutoReconcileRules).FullName);
        private static readonly string[] DocTypes = { "PY", "IS", "PO", "CO" };
        private static readonly string[] CondFields = { "NA", "TR", "CH", "DI", "AM" };
        private static readonly string[] CondOps = { "CT", "EQ", "SW", "BT", "NE", "IS" };
        private static readonly string[] MatchKeys = { "TR", "CH", "BP", "AM", "DN" };

        /// <summary>Everything the Rules tab shows for a bank account, plus the lookups its editor needs.</summary>
        public Dictionary<string, object> GetSetup(Ctx ctx, int bankAccountId)
        {
            var result = new Dictionary<string, object>();
            int headerId = HeaderOf(ctx, bankAccountId);
            result["header"] = headerId > 0 ? LoadHeader(ctx, headerId) : null;
            result["rules"] = headerId > 0 ? LoadRules(ctx, headerId) : new List<ArRuleDto>();
            result["headers"] = OtherHeaders(ctx);
            result["conversionTypes"] = Rows(ctx, "SELECT CT.C_ConversionType_ID AS id, CT.Name AS name FROM C_ConversionType CT WHERE CT.IsActive = 'Y'",
                "CT", null, " ORDER BY CT.Name");
            result["taxes"] = Rows(ctx, "SELECT TX.C_Tax_ID AS id, TX.Name AS name FROM C_Tax TX WHERE TX.IsActive = 'Y' AND TX.IsSurcharge = 'N'"
                + " AND COALESCE(TX.Parent_Tax_ID, 0) = 0", "TX", null, " ORDER BY TX.Name");
            // the difference types of the statement line (VA012_DifferenceType); CH (charge) is not a rule choice
            result["differenceTypes"] = Rows(ctx, "SELECT RL.Value AS id, RL.Name AS name FROM AD_Ref_List RL WHERE RL.IsActive = 'Y' AND RL.Value <> 'CH'"
                + " AND RL.AD_Reference_ID = (SELECT COL.AD_Reference_Value_ID FROM AD_Column COL INNER JOIN AD_Table TB ON (TB.AD_Table_ID = COL.AD_Table_ID)"
                + " WHERE TB.TableName = 'C_BankStatementLine' AND COL.ColumnName = 'VA012_DifferenceType')", null, null, " ORDER BY RL.Name");
            return result;
        }

        /// <summary>A new header with the system defaults, linked to the bank account.</summary>
        public Dictionary<string, object> CreateHeader(Ctx ctx, int bankAccountId, string name)
        {
            return InTrx(ctx, "VA012ARH", trx =>
            {
                PO h = NewPO(ctx, "VA012_MatchHeader", trx);
                h.Set_Value("Name", Clip(string.IsNullOrWhiteSpace(name) ? "Rules" : name.Trim(), 60));
                h.Set_Value("VA012_AutoMatch", true);
                h.Set_Value("VA012_DateWindow", 3);
                h.Set_Value("VA012_AmountTolerance", 1m);
                Save(ctx, h, "VA012_MatchHeader");
                Link(ctx, bankAccountId, h.Get_ID(), trx);
                return h.Get_ID();
            });
        }

        /// <summary>Links an existing header to the bank account (0 unlinks). No data is copied.</summary>
        public Dictionary<string, object> LinkHeader(Ctx ctx, int bankAccountId, int headerId)
        {
            return InTrx(ctx, "VA012ARL", trx =>
            {
                if (headerId > 0 && !BelongsToTenant(ctx, "VA012_MatchHeader", headerId, trx)) throw new Exception("VA012_ARNotFound");
                Link(ctx, bankAccountId, headerId, trx);
                return headerId;
            });
        }

        /// <summary>A new header with its own copy of another header's settings and rules, linked to the bank account.</summary>
        public Dictionary<string, object> CopyHeader(Ctx ctx, int bankAccountId, int fromHeaderId, string name)
        {
            return InTrx(ctx, "VA012ARC", trx =>
            {
                if (!BelongsToTenant(ctx, "VA012_MatchHeader", fromHeaderId, trx)) throw new Exception("VA012_ARNotFound");
                Dictionary<string, object> from = LoadHeader(ctx, fromHeaderId);
                PO h = NewPO(ctx, "VA012_MatchHeader", trx);
                h.Set_Value("Name", Clip(string.IsNullOrWhiteSpace(name) ? from["name"] + " (copy)" : name.Trim(), 60));
                h.Set_Value("VA012_AutoMatch", (bool)from["autoMatch"]);
                h.Set_Value("VA012_DateWindow", (int)from["dateWindow"]);
                h.Set_Value("VA012_AmountTolerance", (decimal)from["tolerance"]);
                if ((int)from["chargeId"] > 0) h.Set_Value("C_Charge_ID", (int)from["chargeId"]);
                if ((int)from["taxId"] > 0) h.Set_Value("C_Tax_ID", (int)from["taxId"]);
                Save(ctx, h, "VA012_MatchHeader");
                foreach (ArRuleDto rule in LoadRules(ctx, fromHeaderId))
                {
                    rule.id = 0;
                    WriteRule(ctx, h.Get_ID(), rule, trx);
                }
                Link(ctx, bankAccountId, h.Get_ID(), trx);
                return h.Get_ID();
            });
        }

        public Dictionary<string, object> SaveHeader(Ctx ctx, int headerId, ArHeaderDto dto)
        {
            return InTrx(ctx, "VA012ARS", trx =>
            {
                if (dto == null || string.IsNullOrWhiteSpace(dto.name)) throw new Exception("VA012_ARNameRequired");
                PO h = ExistingPO(ctx, "VA012_MatchHeader", headerId, trx);
                h.Set_Value("Name", Clip(dto.name.Trim(), 60));
                h.Set_Value("IsActive", dto.active);
                h.Set_Value("VA012_AutoMatch", dto.autoMatch);
                h.Set_Value("VA012_DateWindow", Math.Max(0, dto.dateWindow));
                h.Set_Value("VA012_AmountTolerance", Math.Max(0m, dto.tolerance));
                h.Set_Value("C_Charge_ID", dto.chargeId > 0 ? (object)dto.chargeId : null);
                h.Set_Value("C_Tax_ID", dto.taxId > 0 ? (object)dto.taxId : null);
                Save(ctx, h, "VA012_MatchHeader");
                return headerId;
            });
        }

        public Dictionary<string, object> SaveRule(Ctx ctx, int headerId, ArRuleDto dto)
        {
            return InTrx(ctx, "VA012ARR", trx =>
            {
                if (!BelongsToTenant(ctx, "VA012_MatchHeader", headerId, trx)) throw new Exception("VA012_ARNotFound");
                Validate(dto);
                return WriteRule(ctx, headerId, dto, trx);
            });
        }

        public Dictionary<string, object> DeleteRule(Ctx ctx, int ruleId)
        {
            return InTrx(ctx, "VA012ARX", trx =>
            {
                PO rule = ExistingPO(ctx, "VA012_MatchRule", ruleId, trx);
                DeleteChildren(ctx, ruleId, trx);
                // suggestions keep their history: they only lose the link to the rule
                if (DB.ExecuteQuery("UPDATE VA012_MatchSuggestion SET VA012_MatchRule_ID = NULL WHERE VA012_MatchRule_ID = @rule",
                    new[] { new SqlParameter("@rule", ruleId) }, trx) < 0)
                    throw new Exception("VA012_ErrorSaving");
                if (!rule.Delete(true)) throw new Exception(SaveError(ctx, "VA012_MatchRule"));
                return ruleId;
            });
        }

        public Dictionary<string, object> SetRuleActive(Ctx ctx, int ruleId, bool active)
        {
            return InTrx(ctx, "VA012ARA", trx =>
            {
                PO rule = ExistingPO(ctx, "VA012_MatchRule", ruleId, trx);
                rule.Set_Value("IsActive", active);
                Save(ctx, rule, "VA012_MatchRule");
                return ruleId;
            });
        }

        // ------------------------------------------------------------------ reading
        private int HeaderOf(Ctx ctx, int bankAccountId)
        {
            string sql = "SELECT BA.VA012_MatchHeader_ID FROM C_BankAccount BA WHERE BA.C_BankAccount_ID = @acct";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "BA", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            return Util.GetValueOfInt(DB.ExecuteScalar(sql, new[] { new SqlParameter("@acct", bankAccountId) }, null));
        }

        private Dictionary<string, object> LoadHeader(Ctx ctx, int headerId)
        {
            string sql = "SELECT MH.VA012_MatchHeader_ID, MH.Name, MH.IsActive, MH.VA012_AutoMatch, MH.VA012_DateWindow, MH.VA012_AmountTolerance,"
                + " MH.C_Charge_ID, MH.C_Tax_ID, CH.Name AS ChargeName FROM VA012_MatchHeader MH"
                + " LEFT OUTER JOIN C_Charge CH ON (CH.C_Charge_ID = MH.C_Charge_ID) WHERE MH.VA012_MatchHeader_ID = @hdr";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MH", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@hdr", headerId) }, null);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return null;
            DataRow r = ds.Tables[0].Rows[0];
            return new Dictionary<string, object>
            {
                { "id", headerId },
                { "name", Util.GetValueOfString(r["Name"]) },
                { "active", Util.GetValueOfString(r["IsActive"]) == "Y" },
                { "autoMatch", Util.GetValueOfString(r["VA012_AutoMatch"]) != "N" },
                { "dateWindow", r["VA012_DateWindow"] == DBNull.Value ? 3 : Util.GetValueOfInt(r["VA012_DateWindow"]) },
                { "tolerance", r["VA012_AmountTolerance"] == DBNull.Value ? 1m : Util.GetValueOfDecimal(r["VA012_AmountTolerance"]) },
                { "chargeId", Util.GetValueOfInt(r["C_Charge_ID"]) },
                { "chargeName", Util.GetValueOfString(r["ChargeName"]) },
                { "taxId", Util.GetValueOfInt(r["C_Tax_ID"]) },
                { "accounts", LinkedAccounts(ctx, headerId) }
            };
        }

        private List<string> LinkedAccounts(Ctx ctx, int headerId)
        {
            string sql = "SELECT BA.AccountNo FROM C_BankAccount BA WHERE BA.IsActive = 'Y' AND BA.VA012_MatchHeader_ID = @hdr";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "BA", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY BA.AccountNo";
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@hdr", headerId) }, null);
            return ds == null || ds.Tables.Count == 0 ? new List<string>()
                : ds.Tables[0].Rows.Cast<DataRow>().Select(r => Util.GetValueOfString(r["AccountNo"])).ToList();
        }

        /// <summary>Active headers of the tenant with the accounts they serve, for Link and Copy.</summary>
        private List<Dictionary<string, object>> OtherHeaders(Ctx ctx)
        {
            string sql = "SELECT MH.VA012_MatchHeader_ID, MH.Name FROM VA012_MatchHeader MH WHERE MH.IsActive = 'Y'";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MH", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY MH.Name";
            DataSet ds = DB.ExecuteDataset(sql, null, null);
            var list = new List<Dictionary<string, object>>();
            if (ds == null || ds.Tables.Count == 0) return list;
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                int id = Util.GetValueOfInt(r["VA012_MatchHeader_ID"]);
                list.Add(new Dictionary<string, object> { { "id", id }, { "name", Util.GetValueOfString(r["Name"]) }, { "accounts", LinkedAccounts(ctx, id) } });
            }
            return list;
        }

        private List<ArRuleDto> LoadRules(Ctx ctx, int headerId)
        {
            var rules = new Dictionary<int, ArRuleDto>();
            var order = new List<int>();
            string sql = "SELECT MR.VA012_MatchRule_ID, MR.SeqNo, MR.Name, MR.IsActive, " + AutoReconcileCandidates.AppliesToSelect("MR") + ", MR.VA012_DateWindow, MR.VA012_AmountTolerance,"
                + " MR.VA012_Control, MR.VA012_ControlCap, MR.VA012_DifferenceType, MR.VA012_OnDiff, MR.VA009_PaymentMethod_ID, MR.C_ConversionType_ID,"
                + " MR.VA012_CheckSource, MR.VA012_IsMultiDoc, MR.VA012_MinDocs, MR.VA012_OnMissing FROM VA012_MatchRule MR WHERE MR.VA012_MatchHeader_ID = @hdr";
            sql = MRole.GetDefault(ctx).AddAccessSQL(sql, "MR", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY MR.SeqNo, MR.VA012_MatchRule_ID";
            DataSet ds = DB.ExecuteDataset(sql, new[] { new SqlParameter("@hdr", headerId) }, null);
            if (ds == null || ds.Tables.Count == 0) return new List<ArRuleDto>();
            foreach (DataRow r in ds.Tables[0].Rows)
            {
                var dto = new ArRuleDto
                {
                    id = Util.GetValueOfInt(r["VA012_MatchRule_ID"]),
                    seqNo = Util.GetValueOfInt(r["SeqNo"]),
                    name = Util.GetValueOfString(r["Name"]),
                    active = Util.GetValueOfString(r["IsActive"]) == "Y",
                    appliesTo = AutoReconcileCandidates.AppliesToCodes(r),
                    dateWindow = r["VA012_DateWindow"] == DBNull.Value ? (int?)null : Util.GetValueOfInt(r["VA012_DateWindow"]),
                    tolerance = r["VA012_AmountTolerance"] == DBNull.Value ? (decimal?)null : Util.GetValueOfDecimal(r["VA012_AmountTolerance"]),
                    control = Util.GetValueOfString(r["VA012_Control"]) == "A" ? "A" : "S",
                    controlCap = r["VA012_ControlCap"] == DBNull.Value ? (decimal?)null : Util.GetValueOfDecimal(r["VA012_ControlCap"]),
                    differenceType = Util.GetValueOfString(r["VA012_DifferenceType"]),
                    onDiff = Util.GetValueOfString(r["VA012_OnDiff"]) == "H" ? "H" : "A",
                    payMethodId = Util.GetValueOfInt(r["VA009_PaymentMethod_ID"]),
                    conversionTypeId = Util.GetValueOfInt(r["C_ConversionType_ID"]),
                    checkSource = string.IsNullOrEmpty(Util.GetValueOfString(r["VA012_CheckSource"])) ? "S" : Util.GetValueOfString(r["VA012_CheckSource"]),
                    multiDoc = Util.GetValueOfString(r["VA012_IsMultiDoc"]) != "N",
                    minDocs = Math.Max(2, Util.GetValueOfInt(r["VA012_MinDocs"])),
                    onMissing = Util.GetValueOfString(r["VA012_OnMissing"]) == "S" ? "S" : "W",
                    conds = new List<ArCondDto>(),
                    keys = new List<ArKeyDto>()
                };
                rules[dto.id] = dto;
                order.Add(dto.id);
            }
            if (rules.Count == 0) return new List<ArRuleDto>();

            string csql = "SELECT MRC.VA012_MatchRule_ID, MRC.VA012_CondField, MRC.VA012_CondOperator, MRC.VA012_Value, MRC.VA012_Value2"
                + " FROM VA012_MatchRuleCond MRC INNER JOIN VA012_MatchRule MR ON (MR.VA012_MatchRule_ID = MRC.VA012_MatchRule_ID)"
                + " WHERE MRC.IsActive = 'Y' AND MR.VA012_MatchHeader_ID = @hdr";
            csql = MRole.GetDefault(ctx).AddAccessSQL(csql, "MRC", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO) + " ORDER BY MRC.SeqNo, MRC.VA012_MatchRuleCond_ID";
            DataSet cds = DB.ExecuteDataset(csql, new[] { new SqlParameter("@hdr", headerId) }, null);
            if (cds != null && cds.Tables.Count > 0)
                foreach (DataRow r in cds.Tables[0].Rows)
                {
                    ArRuleDto dto;
                    if (rules.TryGetValue(Util.GetValueOfInt(r["VA012_MatchRule_ID"]), out dto))
                        dto.conds.Add(new ArCondDto
                        {
                            field = Util.GetValueOfString(r["VA012_CondField"]),
                            op = Util.GetValueOfString(r["VA012_CondOperator"]),
                            value = Util.GetValueOfString(r["VA012_Value"]),
                            value2 = Util.GetValueOfString(r["VA012_Value2"])
                        });
                }

            string ksql = "SELECT MRK.VA012_MatchRule_ID, MRK.VA012_MatchKey, MRK.VA012_IsRequired"
                + " FROM VA012_MatchRuleKey MRK INNER JOIN VA012_MatchRule MR ON (MR.VA012_MatchRule_ID = MRK.VA012_MatchRule_ID)"
                + " WHERE MRK.IsActive = 'Y' AND MR.VA012_MatchHeader_ID = @hdr";
            ksql = MRole.GetDefault(ctx).AddAccessSQL(ksql, "MRK", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet kds = DB.ExecuteDataset(ksql, new[] { new SqlParameter("@hdr", headerId) }, null);
            if (kds != null && kds.Tables.Count > 0)
                foreach (DataRow r in kds.Tables[0].Rows)
                {
                    ArRuleDto dto;
                    if (rules.TryGetValue(Util.GetValueOfInt(r["VA012_MatchRule_ID"]), out dto))
                        dto.keys.Add(new ArKeyDto { key = Util.GetValueOfString(r["VA012_MatchKey"]), required = Util.GetValueOfString(r["VA012_IsRequired"]) == "Y" });
                }
            return order.Select(id => rules[id]).ToList();
        }

        private List<Dictionary<string, object>> Rows(Ctx ctx, string sql, string alias, SqlParameter[] p, string orderBy)
        {
            if (alias != null) sql = MRole.GetDefault(ctx).AddAccessSQL(sql, alias, MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(sql + orderBy, p, null);
            var list = new List<Dictionary<string, object>>();
            if (ds == null || ds.Tables.Count == 0) return list;
            foreach (DataRow r in ds.Tables[0].Rows)
                list.Add(new Dictionary<string, object> { { "id", r["id"] is string ? (object)Util.GetValueOfString(r["id"]) : Util.GetValueOfInt(r["id"]) }, { "name", Util.GetValueOfString(r["name"]) } });
            return list;
        }

        // ------------------------------------------------------------------ writing
        private static void Validate(ArRuleDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.name)) throw new Exception("VA012_ARNameRequired");
            if (dto.appliesTo == null || !dto.appliesTo.Any(t => DocTypes.Contains(t))) throw new Exception("VA012_ARChooseType");
            if (dto.keys == null || !dto.keys.Any(k => MatchKeys.Contains(k.key))) throw new Exception("VA012_ARChooseKey");
            foreach (ArCondDto c in dto.conds ?? new List<ArCondDto>())
            {
                if (!CondFields.Contains(c.field) || !CondOps.Contains(c.op)) throw new Exception("VA012_ARBadCondition");
                if (c.op == "BT" && (string.IsNullOrWhiteSpace(c.value) || string.IsNullOrWhiteSpace(c.value2))) throw new Exception("VA012_ARBetweenNeedsTwo");
                if (c.op != "NE" && c.op != "BT" && string.IsNullOrWhiteSpace(c.value)) throw new Exception("VA012_ARConditionValue");
            }
        }

        /// <summary>Inserts or updates the rule and replaces its conditions and keys.</summary>
        private static int WriteRule(Ctx ctx, int headerId, ArRuleDto dto, Trx trx)
        {
            PO rule = dto.id > 0 ? ExistingPO(ctx, "VA012_MatchRule", dto.id, trx) : NewPO(ctx, "VA012_MatchRule", trx);
            if (dto.id <= 0) rule.Set_ValueNoCheck("VA012_MatchHeader_ID", headerId);   // parent link: not updateable
            rule.Set_Value("Name", Clip(dto.name.Trim(), 60));
            rule.Set_Value("SeqNo", dto.seqNo);
            rule.Set_Value("IsActive", dto.active);
            foreach (string[] c in AutoReconcileCandidates.AppliesToColumns) rule.Set_Value(c[1], dto.appliesTo.Contains(c[0]));
            rule.Set_Value("VA012_DateWindow", dto.dateWindow.HasValue ? (object)Math.Max(0, dto.dateWindow.Value) : null);
            rule.Set_Value("VA012_AmountTolerance", dto.tolerance.HasValue ? (object)Math.Max(0m, dto.tolerance.Value) : null);
            rule.Set_Value("VA012_Control", dto.control == "A" ? "A" : "S");
            rule.Set_Value("VA012_ControlCap", dto.control == "A" && dto.controlCap.HasValue && dto.controlCap.Value > 0 ? (object)dto.controlCap.Value : null);
            rule.Set_Value("VA012_DifferenceType", string.IsNullOrEmpty(dto.differenceType) ? null : dto.differenceType);
            rule.Set_Value("VA012_OnDiff", dto.onDiff == "H" ? "H" : "A");
            rule.Set_Value("VA009_PaymentMethod_ID", dto.payMethodId > 0 ? (object)dto.payMethodId : null);
            rule.Set_Value("C_ConversionType_ID", dto.conversionTypeId > 0 ? (object)dto.conversionTypeId : null);
            rule.Set_Value("VA012_CheckSource", dto.checkSource == "B" || dto.checkSource == "M" ? dto.checkSource : "S");
            rule.Set_Value("VA012_IsMultiDoc", dto.multiDoc);
            rule.Set_Value("VA012_MinDocs", Math.Max(2, dto.minDocs));
            rule.Set_Value("VA012_OnMissing", dto.onMissing == "S" ? "S" : "W");
            Save(ctx, rule, "VA012_MatchRule");
            int ruleId = rule.Get_ID();

            if (dto.id > 0) DeleteChildren(ctx, ruleId, trx);
            int seq = 10;
            foreach (ArCondDto c in dto.conds ?? new List<ArCondDto>())
            {
                PO cond = NewPO(ctx, "VA012_MatchRuleCond", trx);
                cond.Set_ValueNoCheck("VA012_MatchRule_ID", ruleId);
                cond.Set_Value("SeqNo", seq);
                cond.Set_Value("VA012_CondField", c.field);
                cond.Set_Value("VA012_CondOperator", c.op);
                cond.Set_Value("VA012_Value", string.IsNullOrWhiteSpace(c.value) ? null : Clip(c.value.Trim(), 255));
                cond.Set_Value("VA012_Value2", string.IsNullOrWhiteSpace(c.value2) ? null : Clip(c.value2.Trim(), 60));
                Save(ctx, cond, "VA012_MatchRuleCond");
                seq += 10;
            }
            foreach (ArKeyDto k in dto.keys.Where(k => MatchKeys.Contains(k.key)).GroupBy(k => k.key).Select(g => g.First()))
            {
                PO key = NewPO(ctx, "VA012_MatchRuleKey", trx);
                key.Set_ValueNoCheck("VA012_MatchRule_ID", ruleId);
                key.Set_Value("VA012_MatchKey", k.key);
                key.Set_Value("VA012_IsRequired", k.required);
                Save(ctx, key, "VA012_MatchRuleKey");
            }
            return ruleId;
        }

        private static void DeleteChildren(Ctx ctx, int ruleId, Trx trx)
        {
            foreach (string table in new[] { "VA012_MatchRuleCond", "VA012_MatchRuleKey" })
            {
                DataSet ds = DB.ExecuteDataset("SELECT " + table + "_ID FROM " + table + " WHERE VA012_MatchRule_ID = @rule",
                    new[] { new SqlParameter("@rule", ruleId) }, trx);
                if (ds == null || ds.Tables.Count == 0) continue;
                MTable t = MTable.Get(ctx, table);
                foreach (DataRow r in ds.Tables[0].Rows)
                {
                    PO child = t.GetPO(ctx, Util.GetValueOfInt(r[0]), trx);
                    if (child != null && child.Get_ID() > 0 && !child.Delete(true)) throw new Exception(SaveError(ctx, table));
                }
            }
        }

        private static void Link(Ctx ctx, int bankAccountId, int headerId, Trx trx)
        {
            var account = new MBankAccount(ctx, bankAccountId, trx);
            if (account.Get_ID() <= 0 || account.GetAD_Client_ID() != ctx.GetAD_Client_ID()) throw new Exception("VA012_SelectBankAccountFirst");
            account.Set_Value("VA012_MatchHeader_ID", headerId > 0 ? (object)headerId : null);
            if (!account.Save()) throw new Exception(SaveError(ctx, "C_BankAccount"));
        }

        private static bool BelongsToTenant(Ctx ctx, string table, int id, Trx trx)
        {
            return id > 0 && Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM " + table + " WHERE " + table + "_ID = @id AND AD_Client_ID = @client",
                new[] { new SqlParameter("@id", id), new SqlParameter("@client", ctx.GetAD_Client_ID()) }, trx)) > 0;
        }

        private static PO NewPO(Ctx ctx, string table, Trx trx)
        {
            PO po = MTable.Get(ctx, table).GetPO(ctx, 0, trx);
            po.SetAD_Org_ID(0);
            return po;
        }

        private static PO ExistingPO(Ctx ctx, string table, int id, Trx trx)
        {
            if (!BelongsToTenant(ctx, table, id, trx)) throw new Exception("VA012_ARNotFound");
            PO po = MTable.Get(ctx, table).GetPO(ctx, id, trx);
            if (po == null || po.Get_ID() != id) throw new Exception("VA012_ARNotFound");
            return po;
        }

        private static void Save(Ctx ctx, PO po, string table)
        {
            if (!po.Save()) throw new Exception(SaveError(ctx, table));
        }

        private static string SaveError(Ctx ctx, string table)
        {
            ValueNamePair pp = VLogger.RetrieveError();
            string detail = pp == null ? "" : (pp.GetName() ?? pp.GetValue() ?? "");
            return Msg.GetMsg(ctx, "VA012_ErrorSaving") + " " + table + (detail.Length > 0 ? ": " + detail : "");
        }

        private static string Clip(string value, int max)
        {
            return value.Length <= max ? value : value.Substring(0, max);
        }

        /// <summary>Runs one change in a transaction; returns {id} or {error} with the message translated.</summary>
        private static Dictionary<string, object> InTrx(Ctx ctx, string name, Func<Trx, int> work)
        {
            var result = new Dictionary<string, object>();
            Trx trx = Trx.GetTrx(Trx.CreateTrxName(name));
            try
            {
                int id = work(trx);
                if (!trx.Commit()) throw new Exception("VA012_ErrorSaving");
                result["id"] = id;
            }
            catch (Exception e)
            {
                trx.Rollback();
                log.Log(Level.SEVERE, "VA012 auto reconcile rules", e);
                string msg = Msg.GetMsg(ctx, e.Message);
                result["error"] = string.IsNullOrEmpty(msg) ? e.Message : msg;
            }
            finally
            {
                trx.Close();
            }
            return result;
        }
    }
}
