﻿using System;
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
        /// <summary>
        /// Import Statement and Get C_BankStatement_ID
        /// </summary>
        /// <param name="_path">File Path</param>
        /// <param name="_filename">File Name</param>
        /// <param name="_bankaccount">C_BankAccount_ID</param>
        /// <param name="_bankAccountCurrency">C_Currency_ID</param>
        /// <param name="_statementno">Statement No</param>
        /// <param name="_statementClassName">Statement Class Name</param>
        /// <param name="_statementCharges">Statement charges</param>
        /// <param name="statementDate">Statement Date</param>
        /// <param name="IsStatementDateAsAccountDate">Statement Date As Account Date</param>
        /// <returns>class Object that returns C_BankStatement_ID or Error Message in the JSON format</returns>
        public JsonResult ImportStatement(string _path, string _filename, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementClassName, string _statementCharges, DateTime? statementDate, bool IsStatementDateAsAccountDate)
        {
            Ctx ctx = Session["ctx"] as Ctx;
            string _className = "";
            string _sql = "";

            _statementClassName = _statementClassName.Substring(_statementClassName.LastIndexOf('_') + 1);
            //changes done for log entry
            _sql = @"SELECT SC.NAME,VA012_BANKSTATEMENTCLASS_ID
                        FROM VA012_BankStatementClass BSC
                        INNER JOIN VA012_StatementClass SC
                        ON (BSC.VA012_STATEMENTCLASS_ID=SC.VA012_STATEMENTCLASS_ID)
                        WHERE BSC.ISACTIVE='Y' AND BSC.VA012_BANKSTATEMENTCLASS_ID=" + _statementClassName;
            _className = Util.GetValueOfString(DB.ExecuteScalar(_sql));


            if (_className == null || _className == "" || _className == string.Empty)
            {
                BankStatementDataImport obj = new BankStatementDataImport();
                return Json(obj.ImportStatement(ctx, _filename, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges, statementDate, IsStatementDateAsAccountDate), JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(ExecuteClass(_className, ctx, _filename, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges, statementDate, IsStatementDateAsAccountDate), JsonRequestBehavior.AllowGet);

            }

        }

        /// <summary>
        /// Import Statement and Get C_BankStatement_ID
        /// </summary>
        /// <param name="_className">Statement Class Name</param>
        /// <param name="ctx">Context</param>
        /// <param name="FileName">File Name</param>
        /// <param name="_path">File Path</param>
        /// <param name="_bankaccount">C_BankAccount_ID</param>
        /// <param name="_bankAccountCurrency">C_Currency_ID</param>
        /// <param name="_statementno">Statement Name</param>
        /// <param name="_statementCharges">Statement Charges</param>
        /// <param name="statementDate">Statement Date</param>
        /// <param name="IsStatementDateAsAccountDate">Statement Date As Account Date</param>
        /// <returns>class Object that returns C_BankStatement_ID or Error Message</returns>
        private static StatementResponse ExecuteClass(string _className, Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges, DateTime? statementDate, bool IsStatementDateAsAccountDate)
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
                        object[] obj;
                        //_className is Equal to VA012_ENBDChkNo then pass the extra parameter statementDate in obj object array
                        if (_className.Equals("VA012.Models.VA012_ENBDChkNo"))
                        {
                            obj = new object[] { ctx, FileName, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges, statementDate, IsStatementDateAsAccountDate };
                        }
                        else
                        {
                            obj = new object[] { ctx, FileName, _path, _bankaccount, _bankAccountCurrency, _statementno, _statementCharges };
                        }
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
        /// <summary>
        /// This function is used to insert data in banking journal from ban statement form
        /// </summary>
        /// <param name="_formData">This takse the data from form</param>
        /// <param name="stdprecision">Standard precision according to bank</param>
        /// <returns> This will return success or if currency not found than aive error message</returns>
        public JsonResult InsertData(List<StatementProp> _formData, int stdprecision)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.InsertData(ctx, _formData, stdprecision));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Max Statement PageNo, LineNo and StatementName
        /// </summary>
        /// <param name="_bankAccount">C_BankAccount_ID</param>
        /// <param name="_origin">origin differentiate weather it is Existing Statement or New Statement</param>
        /// <param name="_pageNo">PageNo</param>
        /// <returns>return JSON string which contains PageNo,LineNo and StatementNo</returns>
        public JsonResult MaxStatement(int _bankAccount, string _origin, int _pageNo)
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
                    _sql = "SELECT C_BANKSTATEMENT_ID FROM C_BankStatement WHERE ISACTIVE='Y' AND DOCSTATUS IN ('DR','IP') AND  C_BANKACCOUNT_ID=" + _bankAccount + "  AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    statementID = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (statementID > 0)
                    {
                        //when the DocStatus in InProgress also should allow  to check the statementNo
                        _sql = "SELECT NAME AS STATEMENTNO FROM C_BankStatement WHERE ISACTIVE='Y' AND DOCSTATUS IN ('DR','IP') AND  C_BANKACCOUNT_ID=" + _bankAccount + " AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                        statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                    }
                    else
                    {
                        //based on BankAcct fetch the Statement Name
                        _sql = "SELECT NVL(MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'), '999999999999'))+1,0) AS STATEMENTNO FROM C_BankStatement WHERE ISACTIVE='Y' AND  C_BANKACCOUNT_ID=" + _bankAccount + " AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
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
                    //Get the Max LineNo based on PageNo
                    string pageNo;
                    if (_pageNo > 0)
                    {
                        pageNo = _pageNo.ToString();
                    }
                    else
                    {
                        pageNo = "SELECT MAX(BL.VA012_PAGE) AS PAGE FROM C_BankStatementLine BL WHERE BL.C_BANKSTATEMENT_ID = " + statementID;
                    }

                    _sql = @"SELECT MAX(BSL.VA012_PAGE) AS PAGE, MAX(BSL.LINE)+10  AS LINE FROM C_BankStatementLine BSL
                    WHERE BSL.VA012_PAGE=(" + pageNo + @") 
                    AND BSL.C_BANKSTATEMENT_ID =" + statementID + "  AND BSL.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    //pageno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    //No need of PageNo condition to fetch the LineNo so get both PageNo and LineNo in One dB Query
                    DataSet _data = DB.ExecuteDataset(_sql, null, null);
                    if (_data != null && _data.Tables[0].Rows.Count > 0)
                    {
                        pageno = Util.GetValueOfInt(_data.Tables[0].Rows[0]["PAGE"]);
                        lineno = Util.GetValueOfInt(_data.Tables[0].Rows[0]["LINE"]);
                    }
                    if (pageno <= 0)
                    {
                        pageno = 1;
                    }

                    //No need of PageNo condition to fetch the LineNo
                    //_sql = @"SELECT MAX(BSL.LINE)+10  AS LINE
                    //FROM C_BANKSTATEMENTLINE BSL
                    //INNER JOIN C_BANKSTATEMENT BS
                    //ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + statementID + " AND BSL.VA012_PAGE='" + pageno + "'  AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                    //lineno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (lineno <= 0)
                    {
                        lineno = 10;
                    }
                }
            }
            else
            {
                //not required start and end date's
                _sql = "SELECT MAX(TO_NUMBER(REGEXP_SUBSTR(NAME, '\\d+'), '999999999999'))+1 AS STATEMENTNO FROM C_BankStatement WHERE ISACTIVE='Y'   AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                statementNo = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                pageno = 1;
                lineno = 10;
            }
            list = "{\"statementNo\":\"" + statementNo + "\",\"pageno\":\"" + pageno + "\",\"lineno\":\"" + lineno + "\"}";


            retJSON = JsonConvert.SerializeObject(list);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Invoice and BPartner
        /// </summary>
        /// <param name="_paymentID">C_Payment_ID or C_Invoice_ID's or C_Order_ID</param>
        /// <param name="_cmbTransactionType">Current Transaction Type</param>
        /// <returns>Object in JSON format</returns>
        public JsonResult SetInvoiceAndBPartner(string _paymentID, string _cmbTransactionType)
        {
            string retJSON = "";
            string _sendBackData = "";
            int count = 0;
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            string _sql = "";
            if (_cmbTransactionType == "PY")
            {
                //Count of VA034_ module Info
                count = Env.IsModuleInstalled("VA034_") ? 1 : 0;
                if (count > 0)
                    _sql = "SELECT C_BPARTNER_ID,C_INVOICE_ID,VA034_DepositSlipNo FROM C_Payment WHERE C_PAYMENT_ID IN(" + _paymentID + ")";
                else
                    _sql = "SELECT C_BPARTNER_ID,C_INVOICE_ID FROM C_Payment WHERE C_PAYMENT_ID IN(" + _paymentID + ")";
            }
            else if (_cmbTransactionType == "IS")
            {
                _sql = "SELECT INV.C_BPARTNER_ID,  NULL AS C_INVOICE_ID FROM C_InvoicePaySchedule PAY INNER JOIN C_Invoice INV ON (PAY.C_INVOICE_ID=INV.C_INVOICE_ID) WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _paymentID + ")";

            }
            else if (_cmbTransactionType == "PO")
            {
                _sql = "SELECT C_BPARTNER_ID, null AS C_INVOICE_ID FROM C_Order WHERE C_ORDER_ID IN(" + _paymentID + ")";

            }


            _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    if (count > 0)
                        _sendBackData = "{\"_bPartnerID\":\"" + _ds.Tables[0].Rows[0]["C_BPARTNER_ID"] + "\",\"_invoiceID\":\"" + _ds.Tables[0].Rows[0]["C_INVOICE_ID"] + "\",\"VA034_DepositSlipNo\":\"" + _ds.Tables[0].Rows[0]["VA034_DepositSlipNo"] + "\",\"Count\":\"" + count + "\"}";
                    else
                        _sendBackData = "{\"_bPartnerID\":\"" + _ds.Tables[0].Rows[0]["C_BPARTNER_ID"] + "\",\"_invoiceID\":\"" + _ds.Tables[0].Rows[0]["C_INVOICE_ID"] + "\",\"Count\":\"" + count + "\"}";

                }
                _ds.Dispose();
            }
            retJSON = JsonConvert.SerializeObject(_sendBackData);
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get C_BPARTNER_ID
        /// </summary>
        /// <param name="_invoiceID">C_Invoice_ID's</param>
        /// <returns>C_BPartner_ID</returns>
        public JsonResult SetBPartner(string _invoiceID)
        {
            string retJSON = "";
            string _sendBackData = "";
            Ctx ctx = Session["ctx"] as Ctx;
            DataSet _ds = new DataSet();
            string _sql = "SELECT C_BPARTNER_ID FROM C_Invoice WHERE C_INVOICE_ID IN (" + _invoiceID + ")";

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
                foreach (DataColumn column in _ds.Tables[0].Columns)
                {
                    column.ColumnName = column.ColumnName.ToUpper();
                }

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
                    foreach (DataColumn column in _ds1.Tables[0].Columns)
                    {
                        column.ColumnName = column.ColumnName.ToUpper();
                    }

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
               + " FROM AD_ClientInfo CINFO "
               + " INNER JOIN C_AcctSchema AC "
               + " ON (AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID) "
               + " LEFT JOIN C_Currency BCURR "
               + " ON (AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID) "
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
        /// <returns>List</returns>
        public JsonResult GetStatementLine(int _bankStatementLineID, string trxType, int payment_ID)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetStatementLine(ctx, _bankStatementLineID, trxType, payment_ID));
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

        /// <summary>
        /// Match the Statement Lines
        /// </summary>
        /// <param name="_matchingBaseItemList">Matching Item List</param>
        /// <param name="_cmbMatchingCriteria">Matching Criteria</param>
        /// <param name="_BankAccount">C_BankAccount</param>
        /// <param name="_StatementNo">StatementNo</param>
        /// <param name="_BankCharges">Bank Charges</param>
        /// <param name="_TaxRate">C_TaxRate_ID</param>
        /// <returns>returns Matched Lines result</returns>
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


        /// <summary>
        /// /Get the Count of Payments or Invoices or Orders or CashJournal Lines 
        /// based on conditions
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_accountID">C_BankAccount_ID</param>
        /// <param name="_paymentPageNo">Currenct PageNo</param>
        /// <param name="_PAGESIZE">Size of the Page</param>
        /// <param name="_paymentMethodID">C_PaymentMethod_ID</param>
        /// <param name="_transactionType">Current Transation Type</param>
        /// <param name="businessPartnerId">Business partner id</param>
        /// <returns>Count</returns>
        public JsonResult LoadPaymentsPages(int _accountID, int _paymentPageNo, int _PAGESIZE, int _paymentMethodID, string _transactionType, int? businessPartnerId, string txtSearch = "")
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadPaymentsPages(ctx, _accountID, _paymentPageNo, _PAGESIZE, _paymentMethodID, _transactionType, businessPartnerId, txtSearch));
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
        /// <param name="businessPartnerId">Business PartnerId</param>
        /// <returns>List of Payments</returns>
        public JsonResult LoadPayments(int _accountID, int _paymentPageNo, int _PAGESIZE, int _paymentMethodID, string _transactionType, DateTime? statementDate, int? businessPartnerId, string txtSearch = "")
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                int CountVA034 = Env.IsModuleInstalled("VA034_") ? 1 : 0;
                TempData["CountVA034"] = CountVA034;
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadPayments(ctx, _accountID, _paymentPageNo, _PAGESIZE, _paymentMethodID, _transactionType, statementDate, businessPartnerId, txtSearch));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        public JsonResult LoadStatements(int _cmbBankAccount, int _statementPageNo, int _PAGESIZE, bool _SEARCHREQUEST, string _txtSearch,int RecOrUnRecComboVal)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.LoadStatements(ctx, _cmbBankAccount, _statementPageNo, _PAGESIZE, _SEARCHREQUEST, _txtSearch, RecOrUnRecComboVal));

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
        /// <summary>
        /// Get the Charge Data
        /// </summary>
        /// <param name="searchText">Search text</param>
        /// <param name="voucherType">Voucher Type</param>
        /// <param name="bankAcct">C_BankAccount_ID</param>
        /// <returns>Charge Data</returns>
        public JsonResult GetCharge(string searchText, string voucherType, int bankAcct)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations obj = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(obj.GetCharge(ctx, searchText, voucherType, bankAcct));
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

        /// <summary>
        ///  Check Invoice condition
        /// </summary>
        /// <param name="_invoiceID">C_Invoice_ID's</param>
        /// <param name="_amount">Amount bind on form</param>
        /// <returns>string Value in JSON format</returns>
        public JsonResult CheckInvoiceCondition(string _invoiceID, decimal _amount)
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

        /// <summary>
        /// Check Form Prepay Condition
        /// </summary>
        /// <param name="_orderID">C_Order_ID</param>
        /// <param name="_amount">Amount</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <returns>List in JSON format</returns>
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
        public JsonResult GetInvPaySchedule(string seltdInvoice, int accountID, DateTime? statemtDate)
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

        /// <summary>
        /// Get Currency and ConversionType
        /// </summary>
        /// <param name="_invoiceSchedules">C_InvoicePaySchedule_ID's</param>
        /// <returns>List</returns>
        public JsonResult GetCurrencyandConversionType(string _invoiceSchedules)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetCurrencyandConversionType(ctx, _invoiceSchedules));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Converted Amount when change the Conversion Type
        /// </summary>
        /// <param name="currency">C_Currency_ID</param>
        /// <param name="conversionType">C_ConversionType_ID</param>
        /// <param name="stmtDate">Statement Date</param>
        /// <param name="_schedules">C_InvoicePaySchedule_ID</param>
        /// <param name="_accountId">C_BankAccount_ID</param>
        /// <param name="orderId">C_Order_ID</param>
        /// <param name="paymentId">C_Payment_ID</param>
        /// <param name="cashLineId">C_CashLine_ID</param>
        /// <returns>Converted Amount</returns>
        public JsonResult GetConvertedAmount(int currency, int conversionType, DateTime? stmtDate, string _schedules, int _accountId, int orderId, int paymentId, int cashLineId)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetConvertedAmount(ctx, currency, conversionType, stmtDate, _schedules, _accountId, orderId, paymentId, cashLineId));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the Payment Methods
        /// </summary>
        /// <param name="BankOrgId">Bank Account Organization Id</param>
        /// <returns>List Of payment methods</returns>
        public JsonResult GetPaymentMethods(int BankOrgId)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetPaymentMethods(ctx, BankOrgId));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the Tax Rates
        /// </summary>
        /// <returns>List Of Tax Rates</returns>
        public JsonResult LoadTaxRate()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.LoadTaxRate(ctx));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Payment Base Type
        /// </summary>
        /// <returns>return PaymentBaseType</returns>
        public JsonResult GetPaymentBaseType(string _whereClause)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetPaymentBaseType(ctx, _whereClause));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get AutoCheckNo and Payment Method
        /// </summary>
        /// <param name="bnkAct_Id">C_BankAccount_ID</param>
        /// <param name="_PayMethod">VA009_PaymentMethod_ID</param>
        /// <param name="_InvSchdleList">C_InvoicePaySchedule_ID's</param>
        /// <param name="voucherMatch">check voucher/match type</param>
        /// <returns>return object that contains AutoCheckNo, PaymentMethod and status</returns>
        public JsonResult GetAutoCheckNo(int bnkAct_Id, int _PayMethod, int[] _InvSchdleList, string voucherMatch)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetAutoCheckNo(ctx, bnkAct_Id, _PayMethod, _InvSchdleList, voucherMatch));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get AD_Column_ID for VA009_PaymentMethod
        /// </summary>
        /// <returns>AD_Column_ID</returns>
        public JsonResult GetAD_Column_IDForPayMethod()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetAD_Column_IDForPayMethod(ctx));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Charge Data
        /// </summary>
        /// <param name="voucherType">Voucher Type</param>
        /// <param name="bankAcct">C_BankAccount_ID</param>
        /// <returns>charge Data</returns>
        public JsonResult GetChargeData(string voucherType, int bankAcct)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetChargeData(ctx, voucherType, bankAcct));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Check module prefix
        /// Author:Rakesh (VA230)
        /// </summary>
        /// <param name="prefix">module prefix</param>
        /// <returns>1/0</returns>
        public JsonResult GetModulePrefix(string prefix)
        {
            int CountVA034 = Env.IsModuleInstalled(prefix) ? 1 : 0;
            return Json(JsonConvert.SerializeObject(CountVA034), JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Author: VA230
        /// Get List of Bank Accounts
        /// </summary>
        /// <param name="bankId">bank id</param>
        /// <returns>List of Bank Accounts</returns>
        public JsonResult GetBankAccount(int bankId)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetBankAccount(ctx, bankId));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Author: VA323
        /// Get List of Bank Charge
        /// </summary>
        /// <returns>List of Bank charge</returns>
        public JsonResult GetBankCharge()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetBankCharge(ctx));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Author: VA323
        /// Get Window for zoom
        /// </summary>
        /// <param name="WindowName">Zoom for window</param>
        /// <returns>window id</returns>
        public JsonResult GetWindowforzoom(string WindowName)
        {
            string retJSON = "";
            int window_Id = 0;
            /* VIS_427 Split the window name element 0 will have new screen name and 
            element 1 will contain old screen name*/
            string[] windowArr = WindowName.Split(',');
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                string sql = @"SELECT AD_Window_ID FROM AD_Window WHERE Name ='" + windowArr[0] + "'";
                window_Id = Util.GetValueOfInt(DB.ExecuteScalar(sql));
                if (window_Id == 0)
                {
                    sql = "SELECT AD_Window_ID FROM AD_Window WHERE Name ='" + windowArr[1] + "'";
                    window_Id = Util.GetValueOfInt(DB.ExecuteScalar(sql));
                }
                retJSON = JsonConvert.SerializeObject(window_Id);
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Author: VA323
        /// Get Window for zoom
        /// </summary>
        /// <param name="_ctrlPayment">_ctrlPayment</param>
        /// <returns>window id</returns>
        public JsonResult GetTrxNoforVoucher(int _ctrlPayment)
        {
            string retJSON = "";
            string res = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                string sql = @"select trxno from C_Payment where C_Payment_ID=" + _ctrlPayment;
                res = DB.ExecuteScalar(sql).ToString();
                retJSON = JsonConvert.SerializeObject(res);
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get the Statement No
        /// </summary>
        /// <param name="_cmbBankAccount">bank Account id</param>
        /// <returns>List Of Statement No</returns>
        public JsonResult LoadStatementNo(int _cmbBankAccount)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetStatementNolist(ctx, _cmbBankAccount));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Bank Account Classes
        /// </summary>
        /// <param name="_cmbBankAccount">bank Account id</param>
        /// <returns>List Bank Account Classes</returns>
        public JsonResult GetBankAccountClasses(int _cmbBankAccount)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.LoadListofStatementclass(ctx, _cmbBankAccount));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Cash Book
        /// </summary>
        /// <returns>List of Cashbook</returns>
        public JsonResult LoadCashbook()
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetCashBook(ctx));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Get Payment Methods
        /// </summary>
        /// <param name="AD_Client">AD_Client id</param>
        /// <param name="AD_Org">AD_Org id</param>
        /// <returns>List of Payment Method</returns>
        public JsonResult LoadPaymentMethod(int AD_Client, int AD_Org)
        {
            string retJSON = "";
            if (Session["ctx"] != null)
            {
                Ctx ctx = Session["ctx"] as Ctx;
                StatementOperations _model = new StatementOperations();
                retJSON = JsonConvert.SerializeObject(_model.GetPaymentMethodList(ctx, AD_Client, AD_Org));
            }
            return Json(retJSON, JsonRequestBehavior.AllowGet);
        }
    }
}
