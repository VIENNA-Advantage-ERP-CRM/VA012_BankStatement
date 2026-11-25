/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto reconciliation - reconciles a matched line through the Bank Statement form's own save
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
using System.Web;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    /// <summary>What the user chose for a suggested line on Accept; empty values fall back to the rule and the header.</summary>
    public class ArPostChoices
    {
        public string DifferenceType;
        public int ChargeId;
        public int TaxId;
        /// <summary>Payment method and conversion type for the payment created for an invoice schedule or prepay order.</summary>
        public int PayMethodId;
        public int ConversionTypeId;
        /// <summary>A cheque number the user typed for the new payment (overrides the cheque book).</summary>
        public string ChequeNo;
        /// <summary>Cheque date the user chose for the line (payment) or the new payment; null takes the default.</summary>
        public DateTime? ChequeDate;
    }

    /// <summary>
    /// Reconciles one matched line the way a user does on the Bank Statement form: the form data is built as the form
    /// builds it after the document is dropped on the line (GetStatementLine plus the document), the form's own drag
    /// checks run first, and StatementOperations.InsertData saves it (creating the payment for invoice schedules and
    /// prepay orders). So an automatic reconciliation posts exactly what a manual match posts.
    /// </summary>
    public class AutoReconcilePoster
    {
        private static readonly VLogger log = VLogger.GetVLogger(typeof(AutoReconcilePoster).FullName);
        private readonly Ctx _ctx;
        private readonly ArRunInfo _info;
        private readonly string _docType;
        private readonly int _precision;
        private readonly StatementOperations _ops = new StatementOperations();

        public AutoReconcilePoster(Ctx ctx, ArRunInfo info, string docType)
        {
            _ctx = ctx;
            _info = info;
            _docType = docType;
            _precision = info.BankPrecision > 0 ? info.BankPrecision : 2;
        }

        /// <summary>Posts the line. Returns null when it is reconciled, otherwise why it was not (the line stays suggested).</summary>
        public string Post(ArResult r, ArPostChoices choices = null)
        {
            Stopwatch watch = Stopwatch.StartNew();
            long checkMs = 0, lineMs = 0;
            string result = null;
            try
            {
                result = PostLine(r, choices, watch, ref checkMs, ref lineMs);
                return result;
            }
            finally
            {
                // where the time goes: the form's drag checks, the form data, and the form's save (payment and workflow)
                log.Info("VA012 auto reconcile post line " + r.Line.LineId + ": check " + checkMs + " ms, form data " + lineMs + " ms, save "
                    + Math.Max(0, watch.ElapsedMilliseconds - checkMs - lineMs) + " ms" + (result == null ? "" : " (not reconciled)"));
            }
        }

        private string PostLine(ArResult r, ArPostChoices choices, Stopwatch watch, ref long checkMs, ref long lineMs)
        {
            if (r.Docs.Count == 0) return Text("VA012_ARChooseDocument", "Choose a document first");
            ArDoc first = r.Docs[0];
            int lineId = r.Line.LineId;
            ArRule rule = r.Rule;

            string check = DragCheck(r, first);
            checkMs = watch.ElapsedMilliseconds;
            if (check != null) return check;

            StatementProp sp = _ops.GetStatementLine(_ctx, lineId, _docType, first.Id);
            lineMs = watch.ElapsedMilliseconds - checkMs;
            // sp._errorMsg is not checked: GetStatementLine looks for a cheque book for every AP schedule with the schedule's
            // own method, and the form ignores that result too. The cheque book is checked when the payment is created
            // (CreatePaymentFromSchedule), and only for a cheque method, with the method finally used.
            if (sp._reconciled) return Text("VA012_StatementAlreadyMatchedOthrPayment", "The line is already reconciled");

            sp._bankStatementLineID = lineId;
            sp._cmbBankAccount = _info.BankAccountId;
            sp._cmbTransactionType = _docType;
            if (sp._cmbCurrency <= 0) sp._cmbCurrency = _info.BankCurrencyId;
            // the form sends these HTML-encoded and InsertData decodes them
            sp._txtDescription = HttpUtility.HtmlEncode(sp._txtDescription ?? "");
            sp._txtVoucherNo = HttpUtility.HtmlEncode(sp._txtVoucherNo ?? "");
            sp._chkUseNextTime = false;
            sp._ctrlBusinessPartner = r.Line.BPartnerId > 0 ? r.Line.BPartnerId : first.BPartnerId;

            switch (_docType)
            {
                case "PY":
                    sp._cmbVoucherMatch = "M";
                    sp._ctrlPayment = first.Id;
                    ApplyPaymentCheque(sp, first.Id, r.Line, choices != null ? choices.ChequeDate : null);
                    break;
                case "IS":
                    sp._cmbVoucherMatch = "M";
                    sp._ctrlPayment = 0;
                    sp._scheduleList = string.Join(",", r.Docs.Select(d => d.Id.ToString(CultureInfo.InvariantCulture)));
                    sp._txtTrxAmt = r.Docs.Sum(d => d.Amount);
                    ApplyNewPaymentSettings(sp, r, rule, choices);
                    break;
                case "PO":
                    sp._cmbVoucherMatch = "M";
                    sp._ctrlPayment = 0;
                    sp._ctrlOrder = first.Id;
                    ApplyNewPaymentSettings(sp, r, rule, choices);
                    break;
                case "CO":
                    sp._cmbVoucherMatch = "C";
                    sp._cmbContraType = "CB";
                    sp._ctrlCashLine = first.Id;
                    break;
            }

            // a cheque method on a new payment: the cheque date is the line date unless the line has its own
            if ((_docType == "IS" || _docType == "PO") && _info.Settings.ChequeMethodIds.Contains(sp._txtPaymentMethod))
            {
                if (choices != null && choices.ChequeDate.HasValue) sp._txtCheckDate = choices.ChequeDate;   // picked in the detail row (Accept)
                else if (sp._txtCheckDate == null) sp._txtCheckDate = r.Line.LineDate;
            }

            // difference: none, a charge with tax (payment, prepay order) or the rule's difference type (invoice schedule)
            sp._txtDifference = Math.Abs(sp._txtTrxAmt) - Math.Abs(sp._txtAmount);
            sp._cmbDifferenceType = "";
            sp._cmbCharge = 0;
            sp._cmbTaxRate = 0;
            sp._txtTaxAmount = 0;
            sp._surChargeAmt = 0;
            if (sp._txtDifference != 0)
            {
                // invoice schedules as the form: invoices more than the statement -> discount, write-off or over/under;
                // statement more than the invoices -> charge and tax (the payment is created for the invoice amount)
                bool statementMore = Math.Abs(r.Line.Amount) > Math.Abs(sp._txtTrxAmt);
                if (_docType == "IS" && !statementMore)
                {
                    string diffType = choices != null && !string.IsNullOrEmpty(choices.DifferenceType) ? choices.DifferenceType
                        : rule != null ? rule.DifferenceType : null;
                    if (string.IsNullOrEmpty(diffType)) return Text("VA012_ARNeedsDiffType", "Needs difference type");
                    sp._cmbDifferenceType = diffType;
                }
                else if (_docType == "PY" || _docType == "PO" || _docType == "IS")
                {
                    int chargeId = choices != null && choices.ChargeId > 0 ? choices.ChargeId : _info.Settings.ChargeId;
                    int taxId = choices != null && choices.TaxId > 0 ? choices.TaxId : _info.Settings.TaxId;
                    if (chargeId <= 0 || taxId <= 0) return Text("VA012_ARNeedsChargeTax", "Needs charge and tax");
                    sp._cmbDifferenceType = "CH";
                    sp._cmbCharge = chargeId;
                    sp._cmbTaxRate = taxId;
                    // InsertData books ChargeAmt = StmtAmt - TrxAmt; the tax is taken out of that difference
                    Dictionary<string, object> tax = _ops.CalculateSurcharge(_ctx, taxId, Math.Abs(r.Line.Amount - sp._txtTrxAmt), _precision);
                    sp._txtTaxAmount = Util.GetValueOfDecimal(tax["TaxAmt"]);
                    sp._surChargeAmt = Util.GetValueOfDecimal(tax["SurchargeAmt"]);
                }
                else return Text("VA012_ARContraEqual", "Contra amounts must be equal");
            }

            string saved = _ops.InsertData(_ctx, new List<StatementProp> { sp }, _precision);
            if (saved != "Success") return Text(saved, saved);

            if (_docType == "IS" || _docType == "PO") WriteTrxNoOnNewPayment(lineId, r.Line.TrxNo);
            return null;
        }

        /// <summary>
        /// A cheque payment: the line takes the payment's cheque number and cheque date, as Match Statement does
        /// (MatchBankStatement.cs). InsertData gives the line the payment's tender type, and for a cheque (K) the line's
        /// EFT Effective Date is mandatory; without it the save is refused with "EFT Effective Date".
        /// </summary>
        private static void ApplyPaymentCheque(StatementProp sp, int paymentId, ArLine line, DateTime? chosenDate)
        {
            DataSet ds = DB.ExecuteDataset("SELECT PAY.CheckNo, PAY.CheckDate, PAY.TenderType FROM C_Payment PAY WHERE PAY.C_Payment_ID = @pay",
                new[] { new SqlParameter("@pay", paymentId) }, null);
            if (ds == null || ds.Tables.Count == 0 || ds.Tables[0].Rows.Count == 0) return;
            DataRow p = ds.Tables[0].Rows[0];
            string chequeNo = Util.GetValueOfString(p["CheckNo"]);
            if (Util.GetValueOfString(p["TenderType"]) != X_C_Payment.TENDERTYPE_Check && string.IsNullOrWhiteSpace(chequeNo)) return;
            if (string.IsNullOrWhiteSpace(sp._txtCheckNum) && !string.IsNullOrWhiteSpace(chequeNo)) sp._txtCheckNum = chequeNo.Trim();
            if (chosenDate.HasValue) sp._txtCheckDate = chosenDate;                    // picked in the detail row (Accept)
            else if (!sp._txtCheckDate.HasValue) sp._txtCheckDate = Util.GetValueOfDateTime(p["CheckDate"]) ?? line.LineDate;
        }

        /// <summary>
        /// Payment method, conversion type and cheque number for the payment InsertData creates: the user's choice (Set
        /// method / type), else the rule's, else the document's.
        /// </summary>
        private static void ApplyNewPaymentSettings(StatementProp sp, ArResult r, ArRule rule, ArPostChoices choices)
        {
            int documentMethod = sp._txtPaymentMethod;
            if (choices != null && choices.PayMethodId > 0) sp._txtPaymentMethod = choices.PayMethodId;
            else if (rule != null && rule.PayMethodId > 0) sp._txtPaymentMethod = rule.PayMethodId;
            else if (sp._txtPaymentMethod <= 0 && r.Docs[0].PayMethodId > 0) sp._txtPaymentMethod = r.Docs[0].PayMethodId;
            if (choices != null && choices.ConversionTypeId > 0) sp._txtConversionType = choices.ConversionTypeId;
            else if (rule != null && rule.ConversionTypeId > 0) sp._txtConversionType = rule.ConversionTypeId;
            // another method than the document's: a cheque number GetStatementLine took from the old method's cheque book
            // does not apply; keep only the statement's own cheque number
            if (sp._txtPaymentMethod != documentMethod) sp._txtCheckNum = r.Line.ChequeNo ?? "";
            if (rule != null && rule.CheckSource == "B")
            {
                sp._txtCheckNum = "";                                              // cheque book only: the next number from the book
                sp._OverrideAutoCheck = false;
            }
            else sp._OverrideAutoCheck = !string.IsNullOrEmpty(r.Line.ChequeNo);   // the statement's cheque number wins
            if (choices != null && !string.IsNullOrWhiteSpace(choices.ChequeNo))
            {
                sp._txtCheckNum = choices.ChequeNo.Trim();                         // typed by the user: wins over the book
                sp._OverrideAutoCheck = true;
            }
        }

        /// <summary>
        /// The checks the form runs when a document is dropped on the line. Payment: the line date may not be before the
        /// payment's accounting date and a partner already on the line must be the payment's (MatchByDrag). Invoice
        /// schedule, prepay order and contra: the form's own check methods.
        /// </summary>
        private string DragCheck(ArResult r, ArDoc first)
        {
            int lineId = r.Line.LineId;
            switch (_docType)
            {
                case "PY":
                    if (r.Line.LineDate.Date < first.DocDate.Date) return Text("VA012_StmtDateCantlessTrxDate", "The statement line date is before the payment's accounting date");
                    if (r.Line.BPartnerId > 0 && first.BPartnerId > 0 && r.Line.BPartnerId != first.BPartnerId)
                        return Text("VA012_StatementPaymentNotMatched", "The payment's partner differs from the line's");
                    return null;
                case "IS":
                    var checkedIds = new List<int>();
                    foreach (ArDoc d in r.Docs)
                    {
                        string status = _ops.CheckScheduleCondition(_ctx, d.Id, lineId, string.Join(",", checkedIds), r.Line.Amount, _info.BankCurrencyId, r.Line.BPartnerId);
                        if (status != "Success") return Text(status, status);
                        checkedIds.Add(d.Id);
                    }
                    return null;
                case "PO":
                    PrepayResponse prepay = _ops.CheckPrepayCondition(_ctx, first.Id, lineId, "", r.Line.Amount, _info.BankCurrencyId, r.Line.BPartnerId, r.Line.LineDate);
                    return prepay._status == "Success" ? null : Text(prepay._status, prepay._status);
                case "CO":
                    ContraResponse contra = _ops.MatchContraByDrag(_ctx, first.Id, lineId, _info.BankCurrencyId, r.Line.LineDate);
                    return contra._status == "Success" ? null : Text(contra._status, contra._status);
            }
            return null;
        }

        /// <summary>The line's transaction number goes to the payment created for it, when that payment has none.</summary>
        private static void WriteTrxNoOnNewPayment(int lineId, string trxNo)
        {
            if (string.IsNullOrWhiteSpace(trxNo)) return;
            int paymentId = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_Payment_ID FROM C_BankStatementLine WHERE C_BankStatementLine_ID = @line",
                new[] { new SqlParameter("@line", lineId) }, null));
            if (paymentId <= 0) return;
            DB.ExecuteQuery("UPDATE C_Payment SET TrxNo = @trx WHERE C_Payment_ID = @pay AND (TrxNo IS NULL OR TrxNo = '')",
                new[] { new SqlParameter("@trx", trxNo.Trim()), new SqlParameter("@pay", paymentId) }, null);
        }

        private string Text(string key, string fallback)
        {
            if (string.IsNullOrEmpty(key)) return fallback;
            string t = Msg.GetMsg(_ctx, key);
            return string.IsNullOrEmpty(t) || t == key ? (fallback ?? key) : t;
        }
    }
}
