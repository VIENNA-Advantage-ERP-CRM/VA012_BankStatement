using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using VA012.Models;
using VAdvantage.DataBase;
using VAdvantage.Utility;
using System.Reflection;


namespace VA012.Controllers
{
    public class BankStatementController : Controller
    {
        //
        // GET: /VA012/BankStatement/
        public ActionResult Index(int windowno)
        {
            ViewBag.WindowNumber = windowno;
            return PartialView();
        }
        public JsonResult ImportStatement(string _path, string _filename, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementClassName, string _statementCharges, DateTime? statementDate)
        {
            Ctx ctx = Session["ctx"] as Ctx;
            string _className = "";
            string _sql = "";

            _statementClassName = _statementClassName.Substring(_statementClassName.LastIndexOf('_') + 1);

            _sql = @"SELECT SC.NAME,VA012_BANKSTATEMENTCLASS_ID
                        FROM VA012_BANKSTATEMENTCLASS BSC
                        INNER JOIN VA012_STATEMENTCLASS SC
                        ON BSC.VA012_STATEMENTCLASS_ID=SC.VA012_STATEMENTCLASS_ID
                        WHERE BSC.ISACTIVE='Y' AND BSC.VA012_BANKSTATEMENTCLASS_ID=" + _statementClassName;
            _className = Util.GetValueOfString(DB.ExecuteScalar(_sql));



            if (_className == null || _className == "" || _className == string.Empty)
            {
                BankStatementDataImport obj = new BankStatementDataImport();
                return Json(obj.ImportStatement(ctx, _filename, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges, statementDate), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(ExecuteClass(_className, ctx, _filename, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges), JsonRequestBehavior.AllowGet);

            }

        }

        private static StatementResponse ExecuteClass(string _className, Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges)
        {
            StatementResponse _obj = new StatementResponse();
            MethodInfo methodInfo = null;
            string _moduleVersion = "";
            Assembly asm = null;
            Type type = null;
            string Prefix = "";
            string[] dotSplit = _className.Split('.');
            string methodName = dotSplit[dotSplit.Length - 1];
            int startindex = _className.LastIndexOf('.');
            string Class = dotSplit[dotSplit.Length - 2];
            int CharcterIndex = Class.IndexOf("_");
            if (CharcterIndex > 0)
            {
                Prefix = Class.Substring(0, CharcterIndex) + "_";
            }

            _className = _className.Remove(startindex, methodName.Length + 1);

            if (Class.Contains("VA012_"))
                type = Type.GetType(_className);
            else
                type = ModuleTypeConatiner.GetClassType(_className, Class);

            if (type != null)
            {
                if (type.IsClass)
                {
                    if (type.GetMethod(methodName) != null)
                    {
                        object _classobj = Activator.CreateInstance(type);
                        MethodInfo method = type.GetMethod(methodName);
                        object[] obj = new object[] { ctx, FileName, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges };
                        return (StatementResponse)method.Invoke(_classobj, obj);
                    }
                    else
                    {
                        _obj._error = "VA012_MethdNotFound";
                        return _obj;
                    }
                }
            }
            _obj._error = "VA012_Error";
            return _obj;

            //Tuple<String, String, string> aInfo = null;
            //if (Env.HasModulePrefix(Prefix, out aInfo))
            //{

            //    if (methodInfo == null || Env.GetModuleVersion(Prefix) != _moduleVersion)
            //    {
            //        _moduleVersion = Env.GetModuleVersion(Prefix);
            //        asm = System.Reflection.Assembly.Load(aInfo.Item1);
            //        type = asm.GetType(_className);
            //        if (type != null)
            //        {
            //            methodInfo = type.GetMethod(methodName);
            //        }
            //    }
            //}
            //else
            //{
            //    return Msg.GetMsg(ctx, "VA012_PlzInstallModule");
            //}
            //if (methodInfo != null)
            //{
            //    object result = "";
            //    object classInstance = Activator.CreateInstance(type, null);
            //    object[] parametersArray = new object[] { ctx, FileName, _path, _bankaccount, _bankAccountCurrency, _statementno };
            //    result = methodInfo.Invoke(classInstance, parametersArray);
            //    return result.ToString();
            //}
            //else
            //{
            //    return Msg.GetMsg(ctx, "VA012_MethdnotFound");
            //}
        }
        public JsonResult InsertData(List<StatementProp> _formData)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.InsertData(ctx, _formData));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MaxStatement(int _bankAccount, string _origin)
        {
            string retJSON = "";
            string _sql = "";
            string list = "";
            string statementNo = "0";
            int statementID = 0;
            int pageno = 0;
            int lineno = 0;
            Ctx ctx = Session["ctx"] as Ctx;
            //not required
            #region Period StartDate and End Date
            //DateTime? _startdate = null;
            //DateTime? _enddate = null;
            //string _sqlDate = @"SELECT STARTDATE
            //                    FROM C_PERIOD
            //                    WHERE C_YEAR_ID =
            //                      (SELECT (Y.C_YEAR_ID) AS C_YEAR_ID
            //                        FROM C_YEAR Y
            //                        INNER JOIN C_PERIOD P
            //                        ON P.C_YEAR_ID        = Y.C_YEAR_ID
            //                        WHERE Y.C_CALENDAR_ID =
            //                          (SELECT C_CALENDAR_ID FROM AD_CLIENTINFO WHERE AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
            //                          )
            //                        AND TRUNC(SYSDATE) BETWEEN P.STARTDATE AND P.ENDDATE
            //                        AND P.ISACTIVE = 'Y'
            //                        AND Y.ISACTIVE ='Y'
            //                      )
            //                    AND PERIODNO=1";
            //_startdate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sqlDate));
            //_sqlDate = @"SELECT ENDDATE
            //                    FROM C_PERIOD
            //                    WHERE C_YEAR_ID =
            //                      (SELECT (Y.C_YEAR_ID) AS C_YEAR_ID
            //                            FROM C_YEAR Y
            //                            INNER JOIN C_PERIOD P
            //                            ON P.C_YEAR_ID        = Y.C_YEAR_ID
            //                            WHERE Y.C_CALENDAR_ID =
            //                              (SELECT C_CALENDAR_ID FROM AD_CLIENTINFO WHERE AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
            //                              )
            //                            AND TRUNC(SYSDATE) BETWEEN P.STARTDATE AND P.ENDDATE
            //                            AND P.ISACTIVE = 'Y'
            //                            AND Y.ISACTIVE ='Y'
            //                      )
            //                    AND PERIODNO=12";
            //_enddate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sqlDate));

            #endregion

            if (_origin == "LO")
            {
                if (_bankAccount > 0)
                {
                    //changed logic as per requirement that first get the Statement Name and if name is null then fetch name from regular expression
                    //when the DocStatus in InProgress also should allow to check the StatementID
                    _sql = "SELECT C_BANKSTATEMENT_ID FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND DOCSTATUS IN ('DR','IP') AND  C_BANKACCOUNT_ID=" + _bankAccount + "  AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    statementID = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (statementID > 0)
                    {
                        //when the DocStatus in InProgress also should allow  to check the statementNo
                        _sql = "SELECT NAME AS STATEMENTNO FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND DOCSTATUS IN ('DR','IP') AND  C_BANKACCOUNT_ID=" + _bankAccount + " AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                        statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                    }
                    else
                    {
                        //based on BankAcct fetch the Statement Name
                        _sql = "SELECT NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'), '999999999999'))+1,0) AS STATEMENTNO FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND  C_BANKACCOUNT_ID=" + _bankAccount + " AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                        statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                    }

                    ////_sql = "SELECT MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'))) AS STATEMENTNO FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND DOCSTATUS='DR' AND TO_CHAR(STATEMENTDATE,'YYYY')=TO_CHAR(sysdate,'YYYY') AND C_BANKACCOUNT_ID=" + _bankAccount + " AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    //_sql = "SELECT NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'), '999999999999')),0) AS STATEMENTNO FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND DOCSTATUS='DR' AND  C_BANKACCOUNT_ID=" + _bankAccount + " AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + " AND STATEMENTDATE BETWEEN " + GlobalVariable.TO_DATE(_startdate, true) + " AND " + GlobalVariable.TO_DATE(_enddate, true);
                    //statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));

                    //_sql = "SELECT MAX(C_BANKSTATEMENT_ID) AS C_BANKSTATEMENT_ID FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND DOCSTATUS='DR' AND  C_BANKACCOUNT_ID=" + _bankAccount + " AND NAME='" + statementNo + "'  AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + " AND STATEMENTDATE BETWEEN " + GlobalVariable.TO_DATE(_startdate, true) + " AND " + GlobalVariable.TO_DATE(_enddate, true);
                    //statementID = Util.GetValueOfInt(DB.ExecuteScalar(_sql));

                    //if (statementID <= 0)
                    //{
                    //    _sql = "SELECT NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'), '999999999999'))+1,0) AS STATEMENTNO FROM C_BANKSTATEMENT WHERE ISACTIVE='Y'  AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + " AND STATEMENTDATE BETWEEN " + GlobalVariable.TO_DATE(_startdate, true) + " AND " + GlobalVariable.TO_DATE(_enddate, true);
                    //    statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                    //}


                    _sql = @"SELECT MAX(BSL.VA012_PAGE) AS PAGE
                    FROM C_BANKSTATEMENTLINE BSL
                    INNER JOIN C_BANKSTATEMENT BS
                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + statementID + "  AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    pageno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (pageno <= 0)
                    {
                        pageno = 1;
                    }


                    _sql = @"SELECT MAX(BSL.LINE)+10  AS LINE
                    FROM C_BANKSTATEMENTLINE BSL
                    INNER JOIN C_BANKSTATEMENT BS
                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + statementID + " AND BSL.VA012_PAGE='" + pageno + "'  AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    lineno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (lineno <= 0)
                    {
                        lineno = 10;
                    }
                }
            }
            else
            {
                //not required start and end date's
                _sql = "SELECT MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'), '999999999999'))+1 AS STATEMENTNO FROM C_BANKSTATEMENT WHERE ISACTIVE='Y'   AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                pageno = 1;
                lineno = 10;
            }
            list = "{\"statementNo\":\"" + statementNo + "\",\"pageno\":\"" + pageno + "\",\"lineno\":\"" + lineno + "\"}";


            retJSON = JsonConvert.SerializeObject(list);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SetInvoiceAndBPartner(int _paymentID, string _cmbTransactionType)
        {
            string retJSON = "";
            string _sendBackData = "";
            int count = 0;
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            string _sql = "";
            if (_cmbTransactionType == "PY")
            {
                count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix='VA034_' AND IsActive='Y'"));
                if (count > 0)
                    _sql = "SELECT C_BPARTNER_ID,C_INVOICE_ID,VA034_DepositSlipNo FROM C_PAYMENT WHERE C_PAYMENT_ID=" + _paymentID;
                else
                    _sql = "SELECT C_BPARTNER_ID,C_INVOICE_ID FROM C_PAYMENT WHERE C_PAYMENT_ID=" + _paymentID;
            }
            else if (_cmbTransactionType == "IS")
            {
                _sql = "SELECT INV.C_BPARTNER_ID,  NULL AS C_INVOICE_ID FROM C_INVOICEPAYSCHEDULE PAY INNER JOIN C_INVOICE INV ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID WHERE PAY.C_INVOICEPAYSCHEDULE_ID=" + _paymentID;

            }
            else if (_cmbTransactionType == "PO")
            {
                _sql = "SELECT C_BPARTNER_ID, null AS C_INVOICE_ID FROM C_ORDER WHERE C_ORDER_ID=" + _paymentID;

            }


            _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    if (count > 0)
                        _sendBackData = "{\"_bPartnerID\":\"" + _ds.Tables[0].Rows[0]["C_BPARTNER_ID"] + "\",\"_invoiceID\":\"" + _ds.Tables[0].Rows[0]["C_INVOICE_ID"] + "\",\"VA034_DepositSlipNo\":\"" + _ds.Tables[0].Rows[0]["VA034_DepositSlipNo"] + "\"}";
                    else
                        _sendBackData = "{\"_bPartnerID\":\"" + _ds.Tables[0].Rows[0]["C_BPARTNER_ID"] + "\",\"_invoiceID\":\"" + _ds.Tables[0].Rows[0]["C_INVOICE_ID"] + "\"}";
                }
                _ds.Dispose();
            }
            retJSON = JsonConvert.SerializeObject(_sendBackData);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SetBPartner(int _invoiceID)
        {
            string retJSON = "";
            string _sendBackData = "";
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            string _sql = "SELECT C_BPARTNER_ID FROM C_INVOICE WHERE C_INVOICE_ID =" + _invoiceID;

            _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    _sendBackData = _ds.Tables[0].Rows[0]["C_BPARTNER_ID"].ToString();
                }
                _ds.Dispose();
            }
            retJSON = JsonConvert.SerializeObject(_sendBackData);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getOverUnderPayment(int _paymentID)
        {
            string retJSON = "";
            string _sendBackData = "";
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            decimal _difference = 0;
            decimal _payamt = 0;
            string _differenceType = "0";
            string _sql = "";
            _sql = "SELECT OverUnderAmt,DiscountAmt,WriteOffAmt,PayAmt FROM C_PAYMENT WHERE C_PAYMENT_ID=" + _paymentID;
            _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    _payamt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["PayAmt"]);
                    if (Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["OverUnderAmt"]) != 0)
                    {
                        _difference = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["OverUnderAmt"]);
                        _differenceType = "OU";
                    }
                    else if (Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["DiscountAmt"]) != 0)
                    {
                        _difference = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["DiscountAmt"]);
                        _differenceType = "DA";
                    }
                    else if (Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["WriteOffAmt"]) != 0)
                    {
                        _difference = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["WriteOffAmt"]);
                        _differenceType = "WO";
                    }

                    _sendBackData = "{\"_payamt\":\"" + _payamt + "\",\"_difference\":\"" + _difference + "\",\"_differenceType\":\"" + _differenceType + "\"}";
                }
                _ds.Dispose();
            }
            retJSON = JsonConvert.SerializeObject(_sendBackData);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }


        public JsonResult SetBankAndAccount()
        {
            string retJSON = "";
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            string _sql = @" SELECT t.C_BANKACCOUNT_ID,
                              t.C_BANK_id
                            FROM
                              (SELECT bs.C_BANKACCOUNT_ID,
                                ba.C_BANK_id,
                                bs.created
                              FROM C_BANKSTATEMENT bs
                              INNER JOIN C_BANKACCOUNT BA
                              ON bs.C_BANKACCOUNT_ID =ba.C_BANKACCOUNT_ID
                              INNER JOIN C_BANK B
                              ON B.C_BANK_ID   =BA.C_BANK_ID
                              WHERE B.AD_ORG_ID=" + ctx.GetAD_Org_ID() +
                              @" ORDER BY BS.CREATED DESC
                              ) t
                            WHERE rownum=1";

            _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                retJSON = JsonConvert.SerializeObject(_ds);
                _ds.Dispose();
            }
            else
            {
                DataSet _ds1 = new DataSet();
                string _sql1 = @"SELECT t.C_BANKACCOUNT_ID,
                                    t.C_BANK_id
                                FROM
                                    (SELECT BA.C_BANKACCOUNT_ID,
                                    BA.C_BANK_ID
                                    FROM C_BANKACCOUNT BA
                                    INNER JOIN C_BANK B
                                    ON B.C_BANK_ID   =BA.C_BANK_ID
                                    WHERE BA.ISACTIVE='Y'
                                    AND BA.AD_ORG_ID =" + ctx.GetAD_Org_ID() +
                              @" ) t
                                WHERE rownum=1";

                _ds1 = DB.ExecuteDataset(_sql1, null, null);
                if (_ds1 != null && _ds1.Tables[0].Rows.Count > 0)
                {
                    retJSON = JsonConvert.SerializeObject(_ds1);
                    _ds1.Dispose();
                }

            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }


        public JsonResult GetBaseCurrency()
        {
            string retJSON = "";
            string _currency = "";
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            string _sql = " SELECT BCURR.ISO_CODE AS BASECURRENCY,BCURR.C_CURRENCY_ID"
               + " FROM AD_CLIENTINFO CINFO "
               + " INNER JOIN C_ACCTSCHEMA AC "
               + " ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID "
               + " LEFT JOIN C_CURRENCY BCURR "
               + " ON AC.C_CURRENCY_ID      =BCURR.C_CURRENCY_ID "
               + " WHERE CINFO.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();

            _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    _currency = "{\"_code\":\"" + _ds.Tables[0].Rows[0]["BASECURRENCY"] + "\",\"_id\":\"" + _ds.Tables[0].Rows[0]["C_CURRENCY_ID"] + "\"}";
                }
                _ds.Dispose();
            }
            retJSON = JsonConvert.SerializeObject(_currency);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Bank Statement Line Data
        /// </summary>
        /// <param name="_bankStatementLineID">C_BankStatementLine_ID</param>
        /// <param name="trxType">Transaction Type</param>
        /// <param name="payment_ID">Payment ID</param>
        /// <param name="_statementDt">Statement Date</param>
        /// <returns>List</returns>
        public JsonResult GetStatementLine(int _bankStatementLineID, string trxType, int payment_ID, DateTime? _statementDt)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetStatementLine(ctx, _bankStatementLineID, trxType, payment_ID, _statementDt));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult UnmatchStatement(string _statementLinesList)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.UnmatchStatement(ctx, _statementLinesList));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult ProcessStatement(string _statementLinesList, int _accountID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.ProcessStatement(ctx, _statementLinesList, _accountID));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteStatement(string _statementLinesList, int _statementLineID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.DeleteStatement(ctx, _statementLinesList, _statementLineID));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }


        public JsonResult MatchStatement(string _matchingBaseItemList, string _cmbMatchingCriteria, int _BankAccount, int _StatementNo)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                MatchBankStatement obj = new MatchBankStatement();
                retJSON = JsonConvert.SerializeObject(obj.MatchStatement(ctx, _matchingBaseItemList, _cmbMatchingCriteria, _BankAccount, _StatementNo));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult MatchStatementGridData(string _matchingBaseItemList, string _cmbMatchingCriteria, int _BankAccount, int _StatementNo, int _BankCharges, int _TaxRate)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                MatchBankStatement obj = new MatchBankStatement();
                retJSON = JsonConvert.SerializeObject(obj.MatchStatementGridData(ctx, _matchingBaseItemList, _cmbMatchingCriteria, _BankAccount, _StatementNo, _BankCharges, _TaxRate));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }



        public JsonResult LoadPaymentsPages(int _accountID, int _paymentPageNo, int _PAGESIZE, int _paymentMethodID, string _transactionType)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadPaymentsPages(ctx, _accountID, _paymentPageNo, _PAGESIZE, _paymentMethodID, _transactionType));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadStatementsPages(int _cmbBankAccount, int _statementPageNo, int _PAGESIZE, bool _SEARCHREQUEST, string _txtSearch)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadStatementsPages(ctx, _cmbBankAccount, _statementPageNo, _PAGESIZE, _SEARCHREQUEST, _txtSearch));

            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// To load payments against selected account
        /// </summary>
        /// <param name="_accountID">Bank Account ID</param>
        /// <param name="_paymentPageNo">Page Number</param>
        /// <param name="_PAGESIZE">Page Size</param>
        /// <param name="_paymentMethodID">Payment Method ID</param>
        /// <param name="_transactionType">Transaction Type</param>
        /// <param name="statementDate">Statement Date</param>
        /// <returns>List of Payments</returns>
        public JsonResult LoadPayments(int _accountID, int _paymentPageNo, int _PAGESIZE, int _paymentMethodID, string _transactionType, DateTime? statementDate)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadPayments(ctx, _accountID, _paymentPageNo, _PAGESIZE, _paymentMethodID, _transactionType, statementDate));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadStatements(int _cmbBankAccount, int _statementPageNo, int _PAGESIZE, bool _SEARCHREQUEST, string _txtSearch)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadStatements(ctx, _cmbBankAccount, _statementPageNo, _PAGESIZE, _SEARCHREQUEST, _txtSearch));

            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult MatchByDrag(int _dragPaymentID, int _dragStatementID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.MatchByDrag(ctx, _dragPaymentID, _dragStatementID, DateTime.Now));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        //public JsonResult CreatePayment(int _bankStatementLineID)
        //{
        //    string retJSON = "";
        //    if (Session["ctx"] != null)
        //    {
        //        Ctx ctx = Session["ctx"] as Ctx;
        //        StatementOperations obj = new StatementOperations();
        //        retJSON = JsonConvert.SerializeObject(obj.CreatePayment(ctx, _bankStatementLineID));
        //    }
        //    return Json(retJSON, JsonRequestBehavior.AllowGet);
        //}
        public JsonResult GetCharge(string searchText)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetCharge(ctx, searchText));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the List of Amount and Message
        /// </summary>
        /// <param name="_dragSourceID">C_Order_ID</param>
        /// <param name="_dragDestinationID">C_Order_ID</param>
        /// <param name="_listToCheck">true or false to check list</param>
        /// <param name="_amount">Amount</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <param name="stateDate">Statement Date</param>
        /// <returns>List</returns>
        public JsonResult CheckPrepayCondition(int _dragSourceID, int _dragDestinationID, string _listToCheck, decimal _amount, int _currencyId, int _formBPartnerID, DateTime? stateDate)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckPrepayCondition(ctx, _dragSourceID, _dragDestinationID, _listToCheck, _amount, _currencyId, _formBPartnerID, stateDate));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the Amount
        /// </summary>
        /// <param name="_dragSourceID">C_CashLine_ID</param>
        /// <param name="_dragDestinationID">C_CashLine_ID</param>
        /// <param name="_amount">Amount</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <param name="stateDate">Statement Date</param>
        /// <returns>List</returns>
        public JsonResult CheckContraCondition(int _dragSourceID, int _dragDestinationID, decimal _amount, int _currencyId, int _formBPartnerID, DateTime? stateDate)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckContraCondition(ctx, _dragSourceID, _dragDestinationID, _amount, _currencyId, _formBPartnerID, stateDate));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Payment Amounts
        /// </summary>
        /// <param name="_dragSourceID">C_Payment_ID</param>
        /// <param name="_dragDestinationID">C_Payment_ID or Zero</param>
        /// <param name="_listToCheck">true or false</param>
        /// <param name="_amount">Amount</param>
        /// <param name="statmtDate">Statement Date</param>
        /// <param name="accountID">C_BankAccount_ID</param>
        /// <returns>List</returns>
        public JsonResult CheckPaymentCondition(int _dragSourceID, int _dragDestinationID, string _listToCheck, decimal _amount, DateTime? statmtDate, int accountID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckPaymentCondition(ctx, _dragSourceID, _dragDestinationID, _amount, statmtDate, accountID));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CheckScheduleCondition(int _dragSourceID, int _dragDestinationID, string _listToCheck, decimal _amount, int _currencyId, int _formBPartnerID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckScheduleCondition(ctx, _dragSourceID, _dragDestinationID, _listToCheck, _amount, _currencyId, _formBPartnerID));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckScheduleTotal(string _listToCheck, decimal _amount, int _currencyId, int _destinationID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckScheduleTotal(ctx, _listToCheck, _amount, _currencyId, _destinationID));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckInvoiceCondition(int _invoiceID, decimal _amount)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckInvoiceCondition(ctx, _invoiceID, _amount));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckFormPaymentCondition(int _paymentID, decimal _amount)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckFormPaymentCondition(ctx, _paymentID, _amount));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckFormPrepayCondition(int _orderID, decimal _amount, int _currencyId, int _formBPartnerID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.CheckFormPrepayCondition(ctx, _orderID, _amount, _currencyId, _formBPartnerID, DateTime.Now));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// To get the data for "Matching Base" Based on List
        /// </summary>
        /// <returns>List of Matching Base ID and Name</returns>
        public JsonResult getMatchRecData()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.getMatchBaseData(ctx));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get List of Bank's
        /// </summary>
        /// <returns>List of Banks</returns>
        public JsonResult GetBank()
        {
            Ctx ctx = Session["ctx"] as Ctx;
            StatementOperations obj = new StatementOperations();
            return Json(JsonConvert.SerializeObject(obj.GetBank(ctx)), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Invoice PaySchedule list
        /// </summary>
        /// <param name="seltdInvoice">C_Invoice_ID</param>
        /// <param name="accountID">C_BankAccount_ID</param>
        /// <param name="statemtDate">Statement Date</param>
        /// <returns>Invoice PaySchedule list</returns>
        public JsonResult GetInvPaySchedule(int seltdInvoice, int accountID, DateTime? statemtDate)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetInvPaySchedule(ctx, seltdInvoice, accountID, statemtDate));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Total conciled or Unconciled Amount
        /// </summary>
        /// <param name="_cmbBankAccount">C_BankAccount_ID</param>
        /// <param name="_txtSearch">Search Text</param>
        /// <param name="_currencyID">C_Currency_ID</param>
        /// <param name="_searchRequest">Search Request</param>
        /// <returns>List</returns>
        public JsonResult LoadConciledOrUnConciledStatements(int _cmbBankAccount, string _txtSearch, int _currencyID, bool _searchRequest)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadConciledOrUnConciledStatements(ctx, _cmbBankAccount, _txtSearch, _currencyID, _searchRequest));

            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Statement Date from Bank Statement window
        /// </summary>
        /// <param name="bankAcct">C_BankAccount_ID</param>
        /// <returns>Statement Date</returns>
        public JsonResult GetStatementDate(int bankAcct)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetStatementDate(ctx, bankAcct));

            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Amount and Date
        /// </summary>
        /// <param name="recordID">Record ID</param>
        /// <param name="bnkAct_Id">C_BankAccount_ID</param>
        /// <param name="transcType">Tansaction Type</param>
        /// <param name="stmtDate">Statement Date</param>
        /// <returns>LIst</returns>
        public JsonResult GetConvtAmount(string recordID, int bnkAct_Id, string transcType, DateTime? stmtDate)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetConvtAmount(ctx, recordID, bnkAct_Id, transcType, stmtDate));

            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Calculate Surcharge & Tax
        /// </summary>
        /// <param name="_tax_ID">C_Tax_ID</param>
        /// <param name="_chargeAmt">Charge Amount</param>
        /// <param name="_stdPrecision">Standard Precision</param>
        /// <returns>Tax Amount and Surcharge Amount</returns>
        public JsonResult CalculateSurcharge(int _tax_ID, decimal _chargeAmt, int _stdPrecision)
        {

            string retJSON = "";
            if (Session["ctx"] != null)
            {
                VAdvantage.Utility.Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations tax = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(tax.CalculateSurcharge(ctx, _tax_ID, _chargeAmt, _stdPrecision));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}