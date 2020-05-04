using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    public class MatchBankStatement
    {
        public List<MatchResponse> MatchStatement(Ctx ctx, string _matchingBaseItemList, string _cmbMatchingCriteria, int _BankAccount, int _StatementNo)
        {
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA012/MatchLog")))
            {
                Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA012/MatchLog"));
            }
            string _path = System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA012/MatchLog/MatchLog.txt");
            LogEntry(_path, " ");
            LogEntry(_path, "== Start :" + DateTime.Now.ToString() + " ==");
            List<MatchResponse> _lstObj = new List<MatchResponse>();
            MatchResponse _obj = new MatchResponse();
            string[] _BaseItemList = _matchingBaseItemList.Split(',');
            StringBuilder _sql = new StringBuilder();
            DataSet _dsPayments = new DataSet();
            DataSet _dsCashLine = new DataSet();
            DataSet _dsStatements = new DataSet();
            try
            {
                #region Statement Lines
                _sql.Clear();
                _sql.Append(@"SELECT BSL.C_BANKSTATEMENTLINE_ID,
                              BS.NAME  AS STATEMENTNO,
                              BSL.LINE AS STATEMENTLINENO,
                              BSL.C_BPARTNER_ID,
                              ROUND(BSL.TRXAMT,NVL(CURR.STDPRECISION,2)) AS PAYMENTAMOUNT,
                              UPPER(BSL.EFTCHECKNO)  AS CHECKNO,
                              BSL.C_CHARGE_ID,
                              BSL.C_INVOICE_ID,
                              BSL.C_ORDER_ID,
                              UPPER(BSL.DESCRIPTION) AS DESCRIPTION,
                              UPPER(BSL.REFERENCENO) AS REFERENCENO,
                              UPPER(BSL.MEMO) AS MEMO,
                              BSL.TRXNO,
                              ROUND(BSL.TRXAMT,NVL(CURR.STDPRECISION,2)) AS TRXAMT
                            FROM C_BANKSTATEMENTLINE BSL
                            INNER JOIN C_BANKSTATEMENT BS
                            ON BSL.C_BANKSTATEMENT_ID        =BS.C_BANKSTATEMENT_ID
                            LEFT JOIN C_CURRENCY CURR 
                            ON BSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID
                            WHERE BSL.ISACTIVE               ='Y'
                            AND BSL.VA012_ISMATCHINGCONFIRMED='N'
                            AND BSL.C_PAYMENT_ID            IS NULL                            
                            AND BSL.C_CASHLINE_ID           IS NULL  ");//Contra Check

                _sql.Append(" AND BS.C_BANKACCOUNT_ID=" + _BankAccount
                          + " AND BS.C_BANKSTATEMENT_ID=" + _StatementNo
                          + " AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());


                if (ctx.GetAD_Org_ID() != 0)
                {
                    _sql.Append(@" AND BS.AD_ORG_ID=" + ctx.GetAD_Org_ID());
                }

                _dsStatements = DB.ExecuteDataset(_sql.ToString());
                _sql.Clear();
                #endregion Statement Lines

                if (_dsStatements != null && _dsStatements.Tables[0].Rows.Count > 0)
                {
                    #region Get Matching Criteria
                    int _matchingCriteria = 0;
                    int _matchingCount = 0;
                    if (_cmbMatchingCriteria == "AT")
                    {
                        _matchingCriteria = 2;
                    }
                    else if (_cmbMatchingCriteria == "AR")
                    {
                        _matchingCriteria = 3;
                    }
                    else if (_cmbMatchingCriteria == "AL")
                    {
                        _matchingCriteria = _BaseItemList.Length;
                    }
                    #endregion Get Matching Criteria

                    int _businessPartnerID = 0;
                    decimal _paymentAmount = 0;
                    string _checkNo = "";
                    int _chargeID = 0;
                    int _invoiceID = 0;
                    int _orderID = 0;
                    string _authCode = "";
                    StringBuilder _condition = new StringBuilder();
                    StringBuilder _conditionTemp = new StringBuilder();
                    string _checkUseNextTime = "";
                    bool _checkOk = false;
                    for (int i = 0; i < _dsStatements.Tables[0].Rows.Count; i++)
                    {
                        _checkOk = false;
                        #region Payment Match Case
                        #region Get all Base values form Statement
                        _obj = new MatchResponse();
                        _businessPartnerID = 0;
                        _paymentAmount = 0;
                        _checkNo = "";
                        _chargeID = 0;
                        _invoiceID = 0;
                        _orderID = 0;
                        _matchingCount = 0;
                        _authCode = "";
                        _checkUseNextTime = "";
                        _condition.Clear();

                        #region Authentication Code
                        if (_BaseItemList.Contains("AC") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            // _authCode = AuthenticationCode(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString());
                            _authCode = AuthenticationCode(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["TRXNO"].ToString().Trim(), Convert.ToDecimal(_dsStatements.Tables[0].Rows[i]["TRXAMT"]));
                            if (_authCode != "" && _authCode != null)
                            {
                                //_conditionTemp.Append(_condition).Append(" AND TRIM(PAY.TrxNo) ='" + _authCode + "'");

                                _conditionTemp.Append(_condition).Append(" AND TRIM(UPPER(PAY.TrxNo)) LIKE UPPER('" + _authCode + "')");
                                _conditionTemp.Append(_condition).Append(" AND LENGTH(TRIM(TRANSLATE(REPLACE(PAY.TRXNO, '" + _authCode + "', ''), 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ', ' '))) IS NULL");
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND UPPER(PAY.TrxNo)='" + _authCode + "'");
                                    _matchingCount++;
                                }

                            }

                        }
                        #endregion Authentication Code
                        #region Business Partner
                        if (_BaseItemList.Contains("BP") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BPARTNER_ID"]) > 0)
                            {
                                _businessPartnerID = Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                            }
                            else
                            {
                                _businessPartnerID = BusinessPartner(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            }

                            if (_businessPartnerID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_BPARTNER_ID=" + _businessPartnerID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_BPARTNER_ID=" + _businessPartnerID);
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Business Partner
                        #region Payment Amount
                        if (_BaseItemList.Contains("PA") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            _paymentAmount = Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["PAYMENTAMOUNT"]);
                            if (_paymentAmount != 0)
                            {
                                _conditionTemp.Append(_condition).Append(@" AND ( CASE
                                                    WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                                    THEN
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END),NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND((PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END)),NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                    ELSE
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                  END)=" + _paymentAmount);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(@" AND ( CASE
                                                    WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                                    THEN
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END),NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND((PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END)),NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                    ELSE
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                  END)=" + _paymentAmount);
                                    _matchingCount++;
                                }
                            }
                        }
                        #endregion Payment Amount
                        #region Check Number
                        if (_BaseItemList.Contains("CN") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            _checkNo = CheckNumber(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["CHECKNO"].ToString());
                            if (_checkNo != "" && _checkNo != null)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.CHECKNO='" + _checkNo + "' ");
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND UPPER(PAY.CHECKNO)='" + _checkNo + "' ");
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Check Number
                        #region Charge
                        if (_BaseItemList.Contains("CH") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            _chargeID = Charge(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            if (_chargeID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_CHARGE_ID=" + _chargeID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_CHARGE_ID=" + _chargeID);
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Charge
                        #region Invoice
                        if (_BaseItemList.Contains("IN") && _matchingCriteria > _matchingCount)
                        {
                            if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_INVOICE_ID"]) > 0)
                            {
                                _invoiceID = Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_INVOICE_ID"]);
                            }
                            else
                            {
                                _invoiceID = Invoice(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            }
                            if (_invoiceID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_INVOICE_ID=" + _invoiceID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_INVOICE_ID=" + _invoiceID);
                                    _matchingCount++;
                                }

                            }

                        }
                        #endregion Invoice
                        #region Order
                        if (_BaseItemList.Contains("OR") && _matchingCriteria > _matchingCount)
                        {
                            if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_ORDER_ID"]) > 0)
                            {
                                _orderID = Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_ORDER_ID"]);
                            }
                            else
                            {
                                _orderID = Invoice(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            }
                            if (_orderID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_ORDER_ID=" + _orderID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_ORDER_ID=" + _orderID);
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Order


                        #endregion Get all Base values form Statement

                        if (_dsPayments != null && _dsPayments.Tables.Count > 0 && _dsPayments.Tables[0].Rows.Count > 0 && _matchingCriteria == _matchingCount)
                        {
                            MBankStatementLine _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]), null);
                            _bankStatementLine.SetC_Payment_ID(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[0]["C_PAYMENT_ID"]));
                            _bankStatementLine.SetVA012_IsMatchingConfirmed(true);

                            if (_businessPartnerID > 0)
                            {
                                _bankStatementLine.SetC_BPartner_ID(_businessPartnerID);
                            }
                            if (_checkNo != "")
                            {
                                _bankStatementLine.SetEftCheckNo(_checkNo);
                            }
                            if (_chargeID > 0)
                            {
                                _bankStatementLine.SetC_Charge_ID(_chargeID);
                                _bankStatementLine.SetChargeAmt(_bankStatementLine.GetStmtAmt());
                                _bankStatementLine.SetTrxAmt(0);







                            }
                            if (_invoiceID > 0)
                            {
                                _bankStatementLine.SetC_Invoice_ID(_invoiceID);
                            }
                            if (_orderID > 0)
                            {
                                _bankStatementLine.SetC_Order_ID(_orderID);
                            }

                            if (!_bankStatementLine.Save())
                            {
                                ValueNamePair vnp = null;
                                vnp = VLogger.RetrieveError();
                                _obj._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]) + " #" + vnp.Key + ":" + vnp.Name;
                                _lstObj.Add(_obj);
                                LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _obj._error);

                            }
                            else
                            {
                                _obj._statementNo = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]);
                                _obj._statementLine = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]);
                                _obj._paymentNo = Util.GetValueOfString(_dsPayments.Tables[0].Rows[0]["PAYMENTNO"]);
                                _obj._paymentOrCash = "P";
                                _checkUseNextTime = CheckUseNextTime(ctx, _bankStatementLine);
                                if (_checkUseNextTime != "" && _checkUseNextTime != null)
                                {
                                    _obj._warning = _checkUseNextTime;
                                }
                                _lstObj.Add(_obj);
                                LogEntry(_path, Msg.GetMsg(ctx, "VA012_StatementNumber") + ": " + _obj._statementNo + "," + Msg.GetMsg(ctx, "VA012_StatementLine") + ": " + _obj._statementLine + "," + Msg.GetMsg(ctx, "VA012_MatchedToPayment") + ": " + _obj._paymentNo + " " + _obj._warning);
                                _checkOk = true;
                            }
                        }
                        #endregion Payment Match Case


                        if (!_checkOk)
                        {
                            #region Contra Match Case
                            if (_cmbMatchingCriteria == "AL" && _matchingCriteria > 3)
                            {

                            }
                            else if (_BaseItemList.Contains("PA") || _BaseItemList.Contains("CN") || _BaseItemList.Contains("CH"))
                            {

                                #region Get all Base values form Statement
                                _obj = new MatchResponse();
                                _paymentAmount = 0;
                                _checkNo = "";
                                _chargeID = 0;
                                _matchingCount = 0;
                                _checkUseNextTime = "";
                                _condition.Clear();
                                #region Payment Amount
                                if (_BaseItemList.Contains("PA") && _matchingCriteria > _matchingCount)
                                {
                                    _conditionTemp.Clear();
                                    _paymentAmount = Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["PAYMENTAMOUNT"]);
                                    if (_paymentAmount != 0)
                                    {
                                        _conditionTemp.Append(_condition).Append(@" AND (ROUND(CSL.AMOUNT,NVL(BCURR.StdPrecision,2))*-1) =" + _paymentAmount);
                                        _dsCashLine = CheckCashLineExist(ctx, _BankAccount, _conditionTemp);
                                        if (_dsCashLine != null && _dsCashLine.Tables[0].Rows.Count > 0)
                                        {
                                            _condition.Append(@" AND (ROUND(CSL.AMOUNT,NVL(BCURR.StdPrecision,2))*-1) =" + _paymentAmount);
                                            _matchingCount++;
                                        }
                                    }
                                }
                                #endregion Payment Amount
                                #region Check Number
                                if (_BaseItemList.Contains("CN") && _matchingCriteria > _matchingCount)
                                {
                                    _conditionTemp.Clear();
                                    _checkNo = CheckNumber_Contra(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["CHECKNO"].ToString());
                                    if (_checkNo != "" && _checkNo != null)
                                    {
                                        _conditionTemp.Append(_condition).Append(" AND CSL.CHECKNO='" + _checkNo + "' ");
                                        _dsCashLine = CheckCashLineExist(ctx, _BankAccount, _conditionTemp);
                                        if (_dsCashLine != null && _dsCashLine.Tables[0].Rows.Count > 0)
                                        {
                                            _condition.Append(" AND UPPER(CSL.CHECKNO)='" + _checkNo + "' ");
                                            _matchingCount++;
                                        }

                                    }
                                }
                                #endregion Check Number
                                #region Charge
                                if (_BaseItemList.Contains("CH") && _matchingCriteria > _matchingCount)
                                {
                                    _conditionTemp.Clear();
                                    _chargeID = Charge_Contra(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                                    if (_chargeID > 0)
                                    {
                                        _conditionTemp.Append(_condition).Append(" AND CSL.C_CHARGE_ID=" + _chargeID);
                                        _dsCashLine = CheckCashLineExist(ctx, _BankAccount, _conditionTemp);
                                        if (_dsCashLine != null && _dsCashLine.Tables[0].Rows.Count > 0)
                                        {
                                            _condition.Append(" AND CSL.C_CHARGE_ID=" + _chargeID);
                                            _matchingCount++;
                                        }

                                    }
                                }
                                #endregion Charge
                                #endregion Get all Base values form Statement

                                if (_dsCashLine != null && _dsCashLine.Tables.Count > 0 && _dsCashLine.Tables[0].Rows.Count > 0 && _matchingCriteria == _matchingCount)
                                {
                                    MBankStatementLine _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]), null);
                                    _bankStatementLine.SetC_CashLine_ID(Util.GetValueOfInt(_dsCashLine.Tables[0].Rows[0]["C_CASHLINE_ID"]));
                                    _bankStatementLine.SetVA012_IsMatchingConfirmed(true);


                                    if (_checkNo != "")
                                    {
                                        _bankStatementLine.SetEftCheckNo(_checkNo);
                                    }
                                    if (!_bankStatementLine.Save())
                                    {
                                        ValueNamePair vnp = null;
                                        vnp = VLogger.RetrieveError();
                                        _obj._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]) + " #" + vnp.Key + ":" + vnp.Name;
                                        _lstObj.Add(_obj);
                                        LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _obj._error);

                                    }
                                    else
                                    {
                                        _obj._statementNo = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]);
                                        _obj._statementLine = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]);
                                        _obj._paymentNo = Util.GetValueOfString(_dsCashLine.Tables[0].Rows[0]["CASHNO"]);
                                        _obj._paymentOrCash = "C";
                                        _checkUseNextTime = CheckUseNextTime(ctx, _bankStatementLine);
                                        if (_checkUseNextTime != "" && _checkUseNextTime != null)
                                        {
                                            _obj._warning = _checkUseNextTime;
                                        }
                                        _lstObj.Add(_obj);
                                        LogEntry(_path, Msg.GetMsg(ctx, "VA012_StatementNumber") + ": " + _obj._statementNo + "," + Msg.GetMsg(ctx, "VA012_StatementLine") + ": " + _obj._statementLine + "," + Msg.GetMsg(ctx, "VA012_MatchedToCashLine") + ": " + _obj._paymentNo + " " + _obj._warning);

                                    }
                                }
                            }
                            #endregion Contra Match Case
                        }

                    }
                }

                if (_dsPayments != null)
                {
                    _dsPayments.Dispose();
                }
                if (_dsStatements != null)
                {
                    _dsStatements.Dispose();
                }
                if (_dsCashLine != null)
                {
                    _dsCashLine.Dispose();
                }
            }
            catch (Exception e)
            {
                if (_dsPayments != null)
                {
                    _dsPayments.Dispose();
                }
                if (_dsStatements != null)
                {
                    _dsStatements.Dispose();
                }
                if (_dsCashLine != null)
                {
                    _dsCashLine.Dispose();
                }
                _obj._error = e.Message;
                _lstObj.Add(_obj);
                LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _obj._error);
                LogEntry(_path, "== End :" + DateTime.Now.ToString() + " ==");
                return _lstObj;
            }
            LogEntry(_path, "== End :" + DateTime.Now.ToString() + " ==");
            return _lstObj;
        }

        private static string CheckUseNextTime(Ctx ctx, MBankStatementLine _bankStatementLine)
        {

            DataSet _dsIn = new DataSet();
            string _sqlIn = @"SELECT TBSL.C_BANKSTATEMENTLINE_ID,
                                TBSL.VA012_STATEMENTNO,
                                TBSL.VA012_PAGE,
                                TBSL.LINE
                            FROM VA012_TEMPSTATEMENT tbsl
                            WHERE TBSL.AMOUNT=" + _bankStatementLine.GetStmtAmt()
                              + " AND TBSL.C_CHARGE_ID=" + _bankStatementLine.GetC_Charge_ID()
                              + " AND TBSL.C_TAX_ID=" + _bankStatementLine.GetC_Tax_ID()
                              + " AND TBSL.C_CURRENCY_ID=" + _bankStatementLine.GetC_Currency_ID()
                              + " AND TBSL.C_BANKSTATEMENTLINE_ID!=" + _bankStatementLine.GetC_BankStatementLine_ID()
                              + " AND TBSL.STATEMENTDATE BETWEEN TBSL.STATEMENTDATE-30 AND TBSL.STATEMENTDATE";
            _dsIn = DB.ExecuteDataset(_sqlIn);
            if (_dsIn != null)
            {
                if (_dsIn.Tables[0].Rows.Count > 0)
                {
                    return Msg.GetMsg(ctx, "VA012_SimilarStatementExist") + ":" + _dsIn.Tables[0].Rows[0]["VA012_STATEMENTNO"] + "/" + _dsIn.Tables[0].Rows[0]["VA012_PAGE"] + "/" + _dsIn.Tables[0].Rows[0]["LINE"];
                }
                _dsIn.Dispose();
            }
            return "";
        }
        public static void LogEntry(string _path, string _str)
        {

            FileStream fs1E = new FileStream(_path, FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter sw1E = new StreamWriter(fs1E);
            sw1E.BaseStream.Seek(0, SeekOrigin.End);
            sw1E.WriteLine(_str);
            sw1E.Flush();
            sw1E.Close();
        }
        private string AuthenticationCode(Ctx ctx, int _BankAccount, string _description)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT UPPER(PAY.TrxNo) 
                        AS TrxNo
                        FROM C_PAYMENT PAY
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN VA009_PAYMENTMETHOD PM
                        ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        LEFT JOIN C_BPARTNER BP
                        ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                        WHERE PAY.ISACTIVE           ='Y'
                        AND PAY.ISRECONCILED         ='N'
                        AND PAY.DOCSTATUS           IN ('CO','CL')
                        AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL) ");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());
            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }

            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                string[] _bpSplit = null;
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_description.Contains(_ds.Tables[0].Rows[i]["TrxNo"].ToString()))
                    {
                        return Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]);
                    }
                    _bpSplit = _ds.Tables[0].Rows[i]["TrxNo"].ToString().Split(' ');
                    if (_bpSplit.Length > 1)
                    {
                        for (int k = 0; k < _bpSplit.Length; k++)
                        {
                            if (_description.Contains(_bpSplit[k].ToString()))
                            {
                                return Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]);
                            }
                        }
                    }
                }
            }
            return "";
        }
        private string AuthenticationCode(Ctx ctx, int _BankAccount, string _TrxNo, decimal _TrxAmt)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT UPPER(PAY.TrxNo) 
                        AS TrxNo,
                        PAY.PAYAMT
                        FROM C_PAYMENT PAY
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN VA009_PAYMENTMETHOD PM
                        ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        LEFT JOIN C_BPARTNER BP
                        ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                        WHERE PAY.ISACTIVE           ='Y'
                        AND PAY.ISRECONCILED         ='N'
                        AND PAY.DOCSTATUS           IN ('CO','CL')
                        AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL) ");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());
            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }

            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                string[] _bpSplit = null;
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    // Commented, as per Ashish sir, match only Transaction Number not pay amount and set balance amount if any in Charge Amount Against Charge
                    // Lokesh Chauhan
                    //if (_TrxNo.Trim().Equals(_ds.Tables[0].Rows[i]["TrxNo"].ToString().Trim())
                    //    && _TrxAmt == Convert.ToDecimal(_ds.Tables[0].Rows[i]["PAYAMT"]))
                    if (_TrxNo.Trim().Equals(Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]).Trim()))
                    {
                        return Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]).Trim();
                    }
                    _bpSplit = Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]).Trim().Split(',');
                    if (_bpSplit.Length > 1)
                    {
                        for (int k = 0; k < _bpSplit.Length; k++)
                        {
                            // Commented, as per Ashish sir, match only Transaction Number not pay amount and set balance amount if any in Charge Amount Against Charge
                            // Lokesh Chauhan
                            //if (_TrxNo.Equals(_bpSplit[k].ToString().Trim()) && _TrxAmt == Convert.ToDecimal(_ds.Tables[0].Rows[i]["PAYAMT"]))
                            if (_TrxNo.Equals(Util.GetValueOfString(_bpSplit[k]).Trim()))
                            {
                                return Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]).Trim();
                            }
                        }
                    }
                }
            }
            return "";
        }
        private static int BusinessPartner(Ctx ctx, int _BankAccount, string _description, string _referenceNo, string _memo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT PAY.C_BPARTNER_ID,
                          UPPER(BP.NAME) AS BUSINESSPARTNER
                        FROM C_PAYMENT PAY
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN VA009_PAYMENTMETHOD PM
                        ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        LEFT JOIN C_BPARTNER BP
                        ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                        WHERE PAY.ISACTIVE           ='Y'
                        AND PAY.ISRECONCILED         ='N'
                        AND PAY.DOCSTATUS           IN ('CO','CL')
                        AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL) ");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());
            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }

            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                string[] _bpSplit = null;
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {

                    if (_description.Contains(_ds.Tables[0].Rows[i]["BUSINESSPARTNER"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                    }
                    if (_referenceNo.Contains(_ds.Tables[0].Rows[i]["BUSINESSPARTNER"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                    }
                    if (_memo.Contains(_ds.Tables[0].Rows[i]["BUSINESSPARTNER"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                    }
                    _bpSplit = _ds.Tables[0].Rows[i]["BUSINESSPARTNER"].ToString().Split(' ');
                    if (_bpSplit.Length > 1)
                    {
                        for (int k = 0; k < _bpSplit.Length; k++)
                        {
                            if (_description.Contains(_bpSplit[k].ToString()))
                            {
                                return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                            }
                            if (_referenceNo.Contains(_bpSplit[k].ToString()))
                            {
                                return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                            }
                            if (_memo.Contains(_bpSplit[k].ToString()))
                            {
                                return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                            }
                        }
                    }
                }
            }
            return 0;

        }
        private static string CheckNumber(Ctx ctx, int _BankAccount, string _checkNo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT UPPER(PAY.CHECKNO) AS CHECKNO
                            FROM C_PAYMENT PAY
                            INNER JOIN C_BANKACCOUNT AC
                            ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                            LEFT JOIN VA009_PAYMENTMETHOD PM
                            ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                            LEFT JOIN C_BPARTNER BP
                            ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                            WHERE PAY.ISACTIVE           ='Y'
                            AND PAY.ISRECONCILED         ='N'
                            AND PAY.DOCSTATUS           IN ('CO','CL')
                            AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL)
                            AND PAY.CHECKNO      IS NOT NULL");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());

            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }
            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_checkNo.Contains(_ds.Tables[0].Rows[i]["CHECKNO"].ToString()))
                    {
                        return Util.GetValueOfString(_ds.Tables[0].Rows[i]["CHECKNO"]);
                    }
                }
            }
            return "";
        }
        private static int Charge(Ctx ctx, int _BankAccount, string _description, string _referenceNo, string _memo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT PAY.C_CHARGE_ID,
                          UPPER(CH.NAME) AS CHARGE
                        FROM C_PAYMENT PAY
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN VA009_PAYMENTMETHOD PM
                        ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        LEFT JOIN C_CHARGE CH
                        ON PAY.C_CHARGE_ID =CH.C_CHARGE_ID
                        LEFT JOIN C_BPARTNER BP
                        ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                        WHERE PAY.ISACTIVE           ='Y'
                        AND PAY.ISRECONCILED         ='N'
                        AND PAY.DOCSTATUS           IN ('CO','CL')
                        AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL)
                        AND PAY.C_CHARGE_ID         IS NOT NULL");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());
            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }

            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_description.Contains(_ds.Tables[0].Rows[i]["CHARGE"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    }
                    if (_referenceNo.Contains(_ds.Tables[0].Rows[i]["CHARGE"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    }
                    if (_memo.Contains(_ds.Tables[0].Rows[i]["CHARGE"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    }
                }
            }
            return 0;

        }
        private static int Invoice(Ctx ctx, int _BankAccount, string _description, string _referenceNo, string _memo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT PAY.C_INVOICE_ID,
                          UPPER(INV.DOCUMENTNO) AS INVOICENO
                        FROM C_PAYMENT PAY
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN VA009_PAYMENTMETHOD PM
                        ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        LEFT JOIN C_INVOICE INV
                        ON PAY.C_INVOICE_ID =INV.C_INVOICE_ID
                        LEFT JOIN C_BPARTNER BP
                        ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                        WHERE PAY.ISACTIVE           ='Y'
                        AND PAY.ISRECONCILED         ='N'
                        AND PAY.DOCSTATUS           IN ('CO','CL')
                        AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL)
                        AND PAY.C_INVOICE_ID         IS NOT NULL");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());

            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }
            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_description.Contains(_ds.Tables[0].Rows[i]["INVOICENO"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]);
                    }
                    if (_referenceNo.Contains(_ds.Tables[0].Rows[i]["INVOICENO"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]);
                    }
                    if (_memo.Contains(_ds.Tables[0].Rows[i]["INVOICENO"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]);
                    }
                }
            }
            return 0;
        }
        private static int Order(Ctx ctx, int _BankAccount, string _description, string _referenceNo, string _memo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT PAY.C_ORDER_ID,
                          UPPER(ORD.DOCUMENTNO) AS ORDERNO
                        FROM C_PAYMENT PAY
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN VA009_PAYMENTMETHOD PM
                        ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        LEFT JOIN C_ORDER ORD
                        ON PAY.C_ORDER_ID =ORD.C_ORDER_ID
                        LEFT JOIN C_BPARTNER BP
                        ON PAY.C_BPARTNER_ID         =BP.C_BPARTNER_ID
                        WHERE PAY.ISACTIVE           ='Y'
                        AND PAY.ISRECONCILED         ='N'
                        AND PAY.DOCSTATUS           IN ('CO','CL')
                        AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL)
                        AND PAY.C_ORDER_ID          IS NOT NULL");
            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());
            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }
            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_description.Contains(_ds.Tables[0].Rows[i]["ORDERNO"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ORDER_ID"]);
                    }
                    if (_referenceNo.Contains(_ds.Tables[0].Rows[i]["ORDERNO"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ORDER_ID"]);
                    }
                    if (_memo.Contains(_ds.Tables[0].Rows[i]["ORDERNO"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ORDER_ID"]);
                    }
                }
            }
            return 0;
        }
        private static DataSet CheckPaymentExist(Ctx ctx, int _BankAccount, StringBuilder _condition)
        {
            #region Payments
            DataSet _ds = new DataSet();
            StringBuilder _sql = new StringBuilder();
            _sql.Clear();
            _sql.Append(@"SELECT PAY.C_PAYMENT_ID,
                              PAY.DOCUMENTNO AS PAYMENTNO,
                              PAY.C_BPARTNER_ID,
                              BP.NAME AS BUSINESSPARTNER,
                              CASE
                                WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                  CASE
                                    WHEN (DT.DOCBASETYPE='ARR')
                                    THEN ROUND(PAY.PAYAMT*(
                                      CASE
                                        WHEN CCR.MULTIPLYRATE IS NOT NULL
                                        THEN CCR.MULTIPLYRATE
                                        ELSE CCR1.DIVIDERATE
                                      END),NVL(BCURR.STDPRECISION,2))
                                    WHEN (DT.DOCBASETYPE='APP')
                                    THEN ROUND((PAY.PAYAMT*(
                                      CASE
                                        WHEN CCR.MULTIPLYRATE IS NOT NULL
                                        THEN CCR.MULTIPLYRATE
                                        ELSE CCR1.DIVIDERATE
                                      END)),NVL(BCURR.STDPRECISION,2))*-1
                                  END
                                ELSE
                                  CASE
                                    WHEN (DT.DOCBASETYPE='ARR')
                                    THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))
                                    WHEN (DT.DOCBASETYPE='APP')
                                    THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))*-1
                                  END
                              END AS PAYMENTAMOUNT,
                              PAY.CHECKNO,
                              PAY.C_CHARGE_ID,
                              CH.NAME AS CHARGE,
                              PAY.C_INVOICE_ID,
                              INV.DOCUMENTNO AS INVOICENO,
                              PAY.C_ORDER_ID,
                              ORD.DOCUMENTNO AS ORDERNO
                            FROM C_PAYMENT PAY
                            INNER JOIN C_BANKACCOUNT AC
                            ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                            LEFT JOIN VA009_PAYMENTMETHOD PM
                            ON PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID
                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                            LEFT JOIN C_CURRENCY BCURR
                            ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID
                            LEFT JOIN C_BPARTNER BP
                            ON PAY.C_BPARTNER_ID =BP.C_BPARTNER_ID
                            LEFT JOIN C_CHARGE CH
                            ON PAY.C_CHARGE_ID =CH.C_CHARGE_ID
                            LEFT JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID =INV.C_INVOICE_ID
                            LEFT JOIN C_ORDER ORD
                            ON PAY.C_ORDER_ID =ORD.C_ORDER_ID
                            LEFT JOIN C_CONVERSION_RATE CCR
                            ON (CCR.C_CURRENCY_ID   =PAY.C_CURRENCY_ID
                            AND CCR.ISACTIVE        ='Y'
                            AND CCR.C_CURRENCY_TO_ID=AC.C_CURRENCY_ID
                            AND CCR.AD_CLIENT_ID    =PAY.AD_CLIENT_ID
                            AND CCR.AD_ORG_ID      IN (PAY.AD_ORG_ID,0)
                            AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)
                            LEFT JOIN C_CONVERSION_RATE CCR1
                            ON (CCR1.C_CURRENCY_ID   =AC.C_CURRENCY_ID
                            AND CCR1.C_CURRENCY_TO_ID=PAY.C_CURRENCY_ID
                            AND CCR1.ISACTIVE        ='Y'
                            AND CCR1.AD_CLIENT_ID    =PAY.AD_CLIENT_ID
                            AND CCR1.AD_ORG_ID      IN (PAY.AD_ORG_ID,0)
                            AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO)
                            WHERE PAY.ISACTIVE           ='Y'
                            AND PAY.ISRECONCILED         ='N'
                            AND PAY.DOCSTATUS           IN ('CO','CL')
                            AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL) ");

            _sql.Append(@" AND PAY.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());
            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND PAY.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }
            _sql.Append(_condition);
            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            return _ds;
            #endregion Payments
        }
        private static string CheckNumber_Contra(Ctx ctx, int _BankAccount, string _checkNo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();
            _sql.Append(@"SELECT DISTINCT UPPER(CSL.CHECKNO) AS CHECKNO
                            FROM C_CASH CS
                        INNER JOIN C_CASHLINE CSL
                        ON CS.C_CASH_ID=CSL.C_CASH_ID
                        INNER JOIN C_CHARGE chrg
                        ON chrg.c_charge_id=csl.c_charge_id
                        WHERE CS.ISACTIVE           ='Y'
                        AND CSL.CashType           ='C'
                        AND chrg.dtd001_chargetype ='CON' 
                        AND CSL.VA012_ISRECONCILED  ='N'
                        AND CS.DOCSTATUS           IN ('CO','CL')
                        AND CSL.CHECKNO      IS NOT NULL");
            _sql.Append(@" AND CSL.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND CS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());

            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND CS.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }
            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_checkNo.Contains(_ds.Tables[0].Rows[i]["CHECKNO"].ToString()))
                    {
                        return Util.GetValueOfString(_ds.Tables[0].Rows[i]["CHECKNO"]);
                    }
                }
            }
            return "";
        }
        private static int Charge_Contra(Ctx ctx, int _BankAccount, string _description, string _referenceNo, string _memo)
        {
            StringBuilder _sql = new StringBuilder();
            DataSet _ds = new DataSet();
            _sql.Clear();

            _sql.Append(@"SELECT DISTINCT CSL.C_CHARGE_ID,
                          UPPER(chrg.NAME) AS CHARGE
                        FROM C_CASH CS
                        INNER JOIN C_CASHLINE CSL
                        ON CS.C_CASH_ID=CSL.C_CASH_ID
                        INNER JOIN C_CHARGE chrg
                        ON chrg.c_charge_id=csl.c_charge_id
                        WHERE CS.ISACTIVE           ='Y'
                        AND CSL.CashType           ='C'
                        AND chrg.dtd001_chargetype ='CON' 
                        AND CSL.VA012_ISRECONCILED  ='N'
                        AND CS.DOCSTATUS           IN ('CO','CL')");
            _sql.Append(@" AND CSL.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND CS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());

            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND CS.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }

            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    if (_description.Contains(_ds.Tables[0].Rows[i]["CHARGE"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    }
                    if (_referenceNo.Contains(_ds.Tables[0].Rows[i]["CHARGE"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    }
                    if (_memo.Contains(_ds.Tables[0].Rows[i]["CHARGE"].ToString()))
                    {
                        return Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    }
                }
            }
            return 0;

        }
        private static DataSet CheckCashLineExist(Ctx ctx, int _BankAccount, StringBuilder _condition)
        {
            #region Cash Lines
            DataSet _ds = new DataSet();
            StringBuilder _sql = new StringBuilder();
            _sql.Clear();
            _sql.Append(@" SELECT CSL.C_CASHLINE_ID,
                              CS.DOCUMENTNO ||'_'|| CSL.LINE AS CASHNO,
                              (ROUND(CSL.AMOUNT,NVL(BCURR.StdPrecision,2))*-1) AS PAYMENTAMOUNT,
                              CSL.CHECKNO,
                              CSL.C_CHARGE_ID,
                              chrg.NAME AS CHARGE
                        FROM C_CASH CS
                        INNER JOIN C_CASHLINE CSL
                        ON CS.C_CASH_ID=CSL.C_CASH_ID
                        INNER JOIN C_CHARGE chrg
                        ON chrg.c_charge_id=csl.c_charge_id
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =CSL.C_BANKACCOUNT_ID
                        LEFT JOIN C_CURRENCY BCURR
                        ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID
                        WHERE CS.ISACTIVE           ='Y'
                        AND CSL.CashType           ='C'
                        AND chrg.dtd001_chargetype ='CON' 
                        AND CSL.VA012_ISRECONCILED  ='N'
                        AND CS.DOCSTATUS           IN ('CO','CL')");
            _sql.Append(@" AND CSL.C_BANKACCOUNT_ID=" + _BankAccount
                         + " AND CS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());

            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql.Append(@" AND CS.AD_ORG_ID=" + ctx.GetAD_Org_ID());
            }

            _sql.Append(_condition);
            _ds = DB.ExecuteDataset(_sql.ToString());
            _sql.Clear();
            return _ds;
            #endregion CashLines
        }


        #region[Match Statement Grid]
        public List<MatchStatementGridResponse> MatchStatementGridData(Ctx ctx, string _matchingBaseItemList, string _cmbMatchingCriteria, int _BankAccount, int _StatementNo, int _SetBankCharges, int _SetTaxRate)
        {
            if (!Directory.Exists(System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA012/MatchLog")))
            {
                Directory.CreateDirectory(System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA012/MatchLog"));
            }
            string _path = System.Web.HttpContext.Current.Server.MapPath("~/Areas/VA012/MatchLog/MatchLog.txt");
            LogEntry(_path, " ");
            LogEntry(_path, "== Start :" + DateTime.Now.ToString() + " ==");

            List<MatchStatementGridResponse> _lstObjResponse = new List<MatchStatementGridResponse>();
            MatchStatementGridResponse _objResponse = new MatchStatementGridResponse();

            string[] _BaseItemList = _matchingBaseItemList.Split(',');
            StringBuilder _sql = new StringBuilder();
            DataSet _dsPayments = new DataSet();
            DataSet _dsCashLine = new DataSet();
            DataSet _dsStatements = new DataSet();
            //Declared Varriable globally because it is using in the loop.
            MBankStatementLine _bankStatementLine = null;
            try
            {
                #region Statement Lines
                _sql.Clear();
                _sql.Append(@"SELECT BSL.C_BANKSTATEMENTLINE_ID,
                              BS.NAME  AS STATEMENTNO,
                              BSL.LINE AS STATEMENTLINENO,
                              BSL.C_BPARTNER_ID,
                              CASE
                                When ROUND(BSL.TRXAMT,NVL(CURR.STDPRECISION,2)) = 0
                                THEN '0.00'
                                ELSE To_Char(ROUND(BSL.TRXAMT,NVL(CURR.STDPRECISION,2)),'FM99999999999.00')
                              END  AS PAYMENTAMOUNT,                               
                              UPPER(BSL.EFTCHECKNO)  AS CHECKNO,
                              BSL.C_CHARGE_ID,
                              BSL.C_INVOICE_ID,
                              BSL.C_ORDER_ID,
                              UPPER(BSL.DESCRIPTION) AS DESCRIPTION,
                              UPPER(BSL.REFERENCENO) AS REFERENCENO,
                              UPPER(BSL.MEMO) AS MEMO,
                              BSL.TRXNO,
                              CASE
                                When ROUND(BSL.TRXAMT,NVL(CURR.STDPRECISION,2)) = 0
                                THEN '0.00'
                                ELSE To_Char(ROUND(BSL.TRXAMT,NVL(CURR.STDPRECISION,2)),'FM99999999999.00')
                              END  AS TRXAMT, 

                              CASE
                                When ROUND(BSL.TAXAMT,NVL(CURR.STDPRECISION,2)) = 0
                                THEN '0.00'
                                ELSE   To_Char(ROUND(BSL.TAXAMT,NVL(CURR.STDPRECISION,2)),'FM99999999999.00')
                              END  AS TAXAMOUNT, 

                              CASE
                                When ROUND(BSL.STMTAMT,NVL(CURR.STDPRECISION,2)) = 0
                                THEN '0.00'
                                ELSE To_Char(ROUND(BSL.STMTAMT,NVL(CURR.STDPRECISION,2)),'FM99999999999.00')
                              END  AS NETAMOUNT, 

                              CASE
                                When ROUND(BSL.CHARGEAMT,NVL(CURR.STDPRECISION,2)) = 0
                                THEN '0.00'
                                ELSE To_Char(ROUND(BSL.CHARGEAMT,NVL(CURR.STDPRECISION,2)),'FM99999999999.00')
                              END  AS CHARGEAMOUNT,    
                                                                                                                  
                              TRUNC(BSL.STATEMENTLINEDATE) AS TRANSACTIONDATE,
                              CG.NAME AS CHARGETYPE,
                              CT.NAME As Tax,BSL.C_Currency_ID,BSL.C_Tax_ID
                            FROM C_BANKSTATEMENTLINE BSL
                            INNER JOIN C_BANKSTATEMENT BS
                            ON BSL.C_BANKSTATEMENT_ID = BS.C_BANKSTATEMENT_ID
                            LEFT JOIN C_CURRENCY CURR 
                            ON BSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID
                            LEFT JOIN C_CHARGE CG
                            ON CG.C_CHARGE_ID = BSL.C_CHARGE_ID
                            LEFT JOIN C_TAX CT
                            on CT.C_TAX_ID = BSL.C_TAX_ID
                            WHERE BSL.ISACTIVE               ='Y'
                            AND BSL.VA012_ISMATCHINGCONFIRMED='N'
                            AND BSL.C_PAYMENT_ID            IS NULL                            
                            AND BSL.C_CASHLINE_ID           IS NULL  ");//Contra Check

                _sql.Append(" AND BS.C_BANKACCOUNT_ID=" + _BankAccount
                          + " AND BS.C_BANKSTATEMENT_ID=" + _StatementNo
                          + " AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID());


                if (ctx.GetAD_Org_ID() != 0)
                {
                    _sql.Append(@" AND BS.AD_ORG_ID=" + ctx.GetAD_Org_ID());
                }

                _dsStatements = DB.ExecuteDataset(_sql.ToString());
                //_sql.Clear();
                #endregion Statement Lines

                if (_dsStatements != null && _dsStatements.Tables[0].Rows.Count > 0)
                {
                    #region Get Matching Criteria
                    int _matchingCriteria = 0;
                    int _matchingCount = 0;
                    if (_cmbMatchingCriteria == "AT")
                    {
                        _matchingCriteria = 2;
                    }
                    else if (_cmbMatchingCriteria == "AR")
                    {
                        _matchingCriteria = 3;
                    }
                    else if (_cmbMatchingCriteria == "AL")
                    {
                        _matchingCriteria = _BaseItemList.Length;
                    }
                    #endregion Get Matching Criteria

                    int _businessPartnerID = 0;
                    decimal _paymentAmount = 0;
                    string _checkNo = "";
                    int _chargeID = 0;
                    int _invoiceID = 0;
                    int _orderID = 0;
                    string _authCode = "";
                    StringBuilder _condition = new StringBuilder();
                    StringBuilder _conditionTemp = new StringBuilder();
                    string _checkUseNextTime = "";
                    bool _checkOk = false;
                    for (int i = 0; i < _dsStatements.Tables[0].Rows.Count; i++)
                    {
                        _checkOk = false;
                        #region Payment Match Case
                        #region Get all Base values form Statement
                        _objResponse = new MatchStatementGridResponse();
                        _businessPartnerID = 0;
                        _paymentAmount = 0;
                        _checkNo = "";
                        _chargeID = 0;
                        _invoiceID = 0;
                        _orderID = 0;
                        _matchingCount = 0;
                        _authCode = "";
                        _checkUseNextTime = "";
                        _condition.Clear();

                        #region Authentication Code
                        if (_BaseItemList.Contains("AC") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            // _authCode = AuthenticationCode(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString());
                            _authCode = AuthenticationCode(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["TRXNO"].ToString().Trim(), Convert.ToDecimal(_dsStatements.Tables[0].Rows[i]["TRXAMT"]));
                            if (_authCode != "" && _authCode != null)
                            {
                                //_conditionTemp.Append(_condition).Append(" AND TRIM(PAY.TrxNo) ='" + _authCode + "'");

                                _conditionTemp.Append(_condition).Append(" AND TRIM(UPPER(PAY.TrxNo)) LIKE UPPER('" + _authCode + "')");
                                _conditionTemp.Append(_condition).Append(" AND LENGTH(TRIM(TRANSLATE(REPLACE(PAY.TRXNO, '" + _authCode + "', ''), 'abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ', ' '))) IS NULL");
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND UPPER(PAY.TrxNo)='" + _authCode + "'");
                                    _matchingCount++;
                                }

                            }

                        }
                        #endregion Authentication Code
                        #region Business Partner
                        if (_BaseItemList.Contains("BP") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BPARTNER_ID"]) > 0)
                            {
                                _businessPartnerID = Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                            }
                            else
                            {
                                _businessPartnerID = BusinessPartner(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            }

                            if (_businessPartnerID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_BPARTNER_ID=" + _businessPartnerID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_BPARTNER_ID=" + _businessPartnerID);
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Business Partner
                        #region Payment Amount
                        if (_BaseItemList.Contains("PA") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            _paymentAmount = Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["PAYMENTAMOUNT"]);
                            if (_paymentAmount != 0)
                            {
                                _conditionTemp.Append(_condition).Append(@" AND ( CASE
                                                    WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                                    THEN
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END),NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND((PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END)),NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                    ELSE
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                  END)=" + _paymentAmount);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(@" AND ( CASE
                                                    WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                                    THEN
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END),NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND((PAY.PAYAMT*(
                                                          CASE
                                                            WHEN CCR.MULTIPLYRATE IS NOT NULL
                                                            THEN CCR.MULTIPLYRATE
                                                            ELSE CCR1.DIVIDERATE
                                                          END)),NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                    ELSE
                                                      CASE
                                                        WHEN (DT.DOCBASETYPE='ARR')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))
                                                        WHEN (DT.DOCBASETYPE='APP')
                                                        THEN ROUND(PAY.PAYAMT,NVL(BCURR.STDPRECISION,2))*-1
                                                      END
                                                  END)=" + _paymentAmount);
                                    _matchingCount++;
                                }
                            }
                        }
                        #endregion Payment Amount
                        #region Check Number
                        if (_BaseItemList.Contains("CN") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            _checkNo = CheckNumber(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["CHECKNO"].ToString());
                            if (_checkNo != "" && _checkNo != null)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.CHECKNO='" + _checkNo + "' ");
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND UPPER(PAY.CHECKNO)='" + _checkNo + "' ");
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Check Number
                        #region Charge
                        if (_BaseItemList.Contains("CH") && _matchingCriteria > _matchingCount)
                        {
                            _conditionTemp.Clear();
                            _chargeID = Charge(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            if (_chargeID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_CHARGE_ID=" + _chargeID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_CHARGE_ID=" + _chargeID);
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Charge
                        #region Invoice
                        if (_BaseItemList.Contains("IN") && _matchingCriteria > _matchingCount)
                        {
                            if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_INVOICE_ID"]) > 0)
                            {
                                _invoiceID = Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_INVOICE_ID"]);
                            }
                            else
                            {
                                _invoiceID = Invoice(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            }
                            if (_invoiceID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_INVOICE_ID=" + _invoiceID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_INVOICE_ID=" + _invoiceID);
                                    _matchingCount++;
                                }

                            }

                        }
                        #endregion Invoice
                        #region Order
                        if (_BaseItemList.Contains("OR") && _matchingCriteria > _matchingCount)
                        {
                            if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_ORDER_ID"]) > 0)
                            {
                                _orderID = Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_ORDER_ID"]);
                            }
                            else
                            {
                                _orderID = Invoice(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                            }
                            if (_orderID > 0)
                            {
                                _conditionTemp.Append(_condition).Append(" AND PAY.C_ORDER_ID=" + _orderID);
                                _dsPayments = CheckPaymentExist(ctx, _BankAccount, _conditionTemp);
                                if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
                                {
                                    _condition.Append(" AND PAY.C_ORDER_ID=" + _orderID);
                                    _matchingCount++;
                                }

                            }
                        }
                        #endregion Order


                        #endregion Get all Base values form Statement

                        if (_dsPayments != null && _dsPayments.Tables.Count > 0 && _dsPayments.Tables[0].Rows.Count > 0 && _matchingCriteria == _matchingCount)
                        {
                            _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]), null);
                            _bankStatementLine.SetC_Payment_ID(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[0]["C_PAYMENT_ID"]));
                            _bankStatementLine.SetVA012_IsMatchingConfirmed(true);

                            if (_businessPartnerID > 0)
                            {
                                _bankStatementLine.SetC_BPartner_ID(_businessPartnerID);
                            }
                            if (_checkNo != "")
                            {
                                _bankStatementLine.SetEftCheckNo(_checkNo);
                            }

                            if (_invoiceID > 0)
                            {
                                _bankStatementLine.SetC_Invoice_ID(_invoiceID);
                            }
                            if (_orderID > 0)
                            {
                                _bankStatementLine.SetC_Order_ID(_orderID);
                            }

                            //When Charges Present
                            decimal TaxAmt = 0;
                            string chargeType = "";
                            string taxRate = "";
                            Decimal? _ChgAmt = 0;
                            if (_chargeID > 0)
                            {
                                _bankStatementLine.SetC_Charge_ID(_chargeID);
                                _bankStatementLine.SetChargeAmt(_bankStatementLine.GetStmtAmt());
                                _bankStatementLine.SetTrxAmt(0);

                                //Calculate Surcharge on Bank Statement Line according to Charge Amount and Standard Precision
                                MTax tax = new MTax(ctx, _SetTaxRate, null);

                                //TO set Tax Rate suggested  by Ashish
                                _bankStatementLine.SetC_Tax_ID(tax.GetC_Tax_ID());

                                Decimal surchargeAmt, TaxAmount = Env.ZERO;
                                MCurrency currency = MCurrency.Get(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_Currency_ID"]));
                                int StdPrecision = Util.GetValueOfInt(currency.GetStdPrecision().ToString());
                                if (tax.Get_ColumnIndex("Surcharge_Tax_ID") > 0 && tax.GetSurcharge_Tax_ID() > 0)
                                {
                                    TaxAmount = tax.CalculateSurcharge(_bankStatementLine.GetChargeAmt(), true, StdPrecision, out surchargeAmt);
                                    _bankStatementLine.Set_Value("SurchargeAmt", surchargeAmt);
                                }
                            }
                            else
                            {
                                // Commented as per Ashish sir check difference between payment amount and Bank Statement line amount
                                // if there is difference then only set charge
                                //if ((Convert.ToString(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]).Trim() != string.Empty) && (Convert.ToString(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]).Trim() != "0"))
                                if (_bankStatementLine.GetTrxAmt() != Util.GetValueOfDecimal(_dsPayments.Tables[0].Rows[0]["PAYMENTAMOUNT"]))
                                {
                                    _ChgAmt = Util.GetValueOfDecimal(_bankStatementLine.GetTrxAmt()) - Util.GetValueOfDecimal(_dsPayments.Tables[0].Rows[0]["PAYMENTAMOUNT"]);

                                    _bankStatementLine.SetTrxAmt(Util.GetValueOfDecimal(_dsPayments.Tables[0].Rows[0]["PAYMENTAMOUNT"]));

                                    _bankStatementLine.SetChargeAmt(_ChgAmt);
                                    //Calculate Surcharge on Bank Statement Line according to Charge Amount and Standard Precision
                                    MTax tax = new MTax(ctx, _SetTaxRate, null);
                                    Decimal surchargeAmt, TaxAmount = Env.ZERO;
                                    MCurrency currency = MCurrency.Get(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_Currency_ID"]));
                                    int StdPrecision = Util.GetValueOfInt(currency.GetStdPrecision().ToString());
                                    if (tax.Get_ColumnIndex("Surcharge_Tax_ID") > 0 && tax.GetSurcharge_Tax_ID() > 0)
                                    {
                                        TaxAmount = tax.CalculateSurcharge(Util.GetValueOfDecimal(_ChgAmt), true, StdPrecision, out surchargeAmt);
                                        _bankStatementLine.Set_Value("SurchargeAmt", surchargeAmt);
                                    }
                                    if (_SetBankCharges > 0)
                                    {
                                        _bankStatementLine.SetC_Charge_ID(_SetBankCharges);
                                    }

                                    if (_SetTaxRate > 0)
                                    {
                                        _bankStatementLine.SetC_Tax_ID(_SetTaxRate);
                                        var Rate = Util.GetValueOfDecimal(DB.ExecuteScalar("Select Rate From C_Tax Where C_Tax_ID=" + _SetTaxRate + " AND IsActive='Y'"));
                                        if (Rate > 0)
                                        {
                                            TaxAmt = Math.Round(Util.GetValueOfDecimal(_ChgAmt - (_ChgAmt / ((Rate / 100) + 1))), 2);
                                            _bankStatementLine.SetTaxAmt(TaxAmt);
                                        }
                                        else
                                        {
                                            _bankStatementLine.SetTaxAmt(0);
                                        }
                                        taxRate = Util.GetValueOfString(DB.ExecuteScalar("Select Name From C_Tax Where C_Tax_ID=" + _SetTaxRate + " AND IsActive='Y'"));
                                    }
                                    chargeType = Util.GetValueOfString(DB.ExecuteScalar("Select Name From C_CHARGE Where C_CHARGE_ID=" + _SetBankCharges + " AND IsActive='Y'"));
                                }
                            }

                            if (!_bankStatementLine.Save())
                            {
                                ValueNamePair vnp = null;
                                vnp = VLogger.RetrieveError();
                                _objResponse._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]) + " #" + vnp.Key + ":" + vnp.Name;
                                _lstObjResponse.Add(_objResponse);
                                LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _objResponse._error);

                            }
                            else
                            {
                                _objResponse._statementNo = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]);
                                _objResponse._statementLine = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]);
                                _objResponse._paymentNo = Util.GetValueOfString(_dsPayments.Tables[0].Rows[0]["PAYMENTNO"]);
                                _objResponse._paymentOrCash = "P";

                                DateTime date = Convert.ToDateTime(_dsStatements.Tables[0].Rows[i]["TRANSACTIONDATE"]);
                                _objResponse._trxDate = date.ToShortDateString();
                                _objResponse._trxNo = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["TRXNO"]);
                                _objResponse._salesAmt = Util.GetValueOfString(_bankStatementLine.GetTrxAmt());
                                _objResponse._netAmt = Util.GetValueOfString(_bankStatementLine.GetStmtAmt());
                                _objResponse._difference = Util.GetValueOfString(_ChgAmt);

                                if (_SetBankCharges > 0)
                                {
                                    _objResponse._chargeType = chargeType;
                                }
                                else
                                {
                                    _objResponse._chargeType = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["CHARGETYPE"]);
                                }

                                if (_SetTaxRate > 0)
                                {
                                    _objResponse._taxRate = taxRate;
                                    _objResponse._taxAmt = Util.GetValueOfString(TaxAmt); //Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["TAXAMOUNT"]);
                                }
                                else
                                {
                                    _objResponse._taxRate = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["Tax"]);
                                    _objResponse._taxAmt = Util.GetValueOfString(_bankStatementLine.GetTaxAmt());
                                }

                                _checkUseNextTime = CheckUseNextTime(ctx, _bankStatementLine);
                                if (_checkUseNextTime != "" && _checkUseNextTime != null)
                                {
                                    _objResponse._warning = _checkUseNextTime;
                                }
                                _lstObjResponse.Add(_objResponse);
                                LogEntry(_path, Msg.GetMsg(ctx, "VA012_StatementNumber") + ": " + _objResponse._statementNo + "," + Msg.GetMsg(ctx, "VA012_StatementLine") + ": " + _objResponse._statementLine + "," + Msg.GetMsg(ctx, "VA012_MatchedToPayment") + ": " + _objResponse._paymentNo + " " + _objResponse._warning);
                                _checkOk = true;
                            }
                        }
                        #endregion Payment Match Case

                        if (!_checkOk)
                        {
                            #region Contra Match Case
                            if (_cmbMatchingCriteria == "AL" && _matchingCriteria > 3)
                            {

                            }
                            else if (_BaseItemList.Contains("PA") || _BaseItemList.Contains("CN") || _BaseItemList.Contains("CH"))
                            {

                                #region Get all Base values form Statement
                                _objResponse = new MatchStatementGridResponse();
                                _paymentAmount = 0;
                                _checkNo = "";
                                _chargeID = 0;
                                _matchingCount = 0;
                                _checkUseNextTime = "";
                                _condition.Clear();
                                #region Payment Amount
                                if (_BaseItemList.Contains("PA") && _matchingCriteria > _matchingCount)
                                {
                                    _conditionTemp.Clear();
                                    _paymentAmount = Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["PAYMENTAMOUNT"]);
                                    if (_paymentAmount != 0)
                                    {
                                        _conditionTemp.Append(_condition).Append(@" AND (ROUND(CSL.AMOUNT,NVL(BCURR.StdPrecision,2))*-1) =" + _paymentAmount);
                                        _dsCashLine = CheckCashLineExist(ctx, _BankAccount, _conditionTemp);
                                        if (_dsCashLine != null && _dsCashLine.Tables[0].Rows.Count > 0)
                                        {
                                            _condition.Append(@" AND (ROUND(CSL.AMOUNT,NVL(BCURR.StdPrecision,2))*-1) =" + _paymentAmount);
                                            _matchingCount++;
                                        }
                                    }
                                }
                                #endregion Payment Amount
                                #region Check Number
                                if (_BaseItemList.Contains("CN") && _matchingCriteria > _matchingCount)
                                {
                                    _conditionTemp.Clear();
                                    _checkNo = CheckNumber_Contra(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["CHECKNO"].ToString());
                                    if (_checkNo != "" && _checkNo != null)
                                    {
                                        _conditionTemp.Append(_condition).Append(" AND CSL.CHECKNO='" + _checkNo + "' ");
                                        _dsCashLine = CheckCashLineExist(ctx, _BankAccount, _conditionTemp);
                                        if (_dsCashLine != null && _dsCashLine.Tables[0].Rows.Count > 0)
                                        {
                                            _condition.Append(" AND UPPER(CSL.CHECKNO)='" + _checkNo + "' ");
                                            _matchingCount++;
                                        }

                                    }
                                }
                                #endregion Check Number
                                #region Charge
                                if (_BaseItemList.Contains("CH") && _matchingCriteria > _matchingCount)
                                {
                                    _conditionTemp.Clear();
                                    _chargeID = Charge_Contra(ctx, _BankAccount, _dsStatements.Tables[0].Rows[i]["DESCRIPTION"].ToString(), _dsStatements.Tables[0].Rows[i]["REFERENCENO"].ToString(), _dsStatements.Tables[0].Rows[i]["MEMO"].ToString());
                                    if (_chargeID > 0)
                                    {
                                        _conditionTemp.Append(_condition).Append(" AND CSL.C_CHARGE_ID=" + _chargeID);
                                        _dsCashLine = CheckCashLineExist(ctx, _BankAccount, _conditionTemp);
                                        if (_dsCashLine != null && _dsCashLine.Tables[0].Rows.Count > 0)
                                        {
                                            _condition.Append(" AND CSL.C_CHARGE_ID=" + _chargeID);
                                            _matchingCount++;
                                        }

                                    }
                                }
                                #endregion Charge
                                #endregion Get all Base values form Statement

                                if (_dsCashLine != null && _dsCashLine.Tables.Count > 0 && _dsCashLine.Tables[0].Rows.Count > 0 && _matchingCriteria == _matchingCount)
                                {
                                    _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]), null);
                                    _bankStatementLine.SetC_CashLine_ID(Util.GetValueOfInt(_dsCashLine.Tables[0].Rows[0]["C_CASHLINE_ID"]));
                                    _bankStatementLine.SetVA012_IsMatchingConfirmed(true);


                                    if (_checkNo != "")
                                    {
                                        _bankStatementLine.SetEftCheckNo(_checkNo);
                                    }

                                    decimal TaxAmt = 0;
                                    string chargeType = "";
                                    string taxRate = "";
                                    if ((Convert.ToString(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]).Trim() != string.Empty) && (Convert.ToString(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]).Trim() != "0"))
                                    {

                                        if (_SetBankCharges > 0)
                                        {
                                            _bankStatementLine.SetC_Charge_ID(_SetBankCharges);
                                        }

                                        if (_SetTaxRate > 0)
                                        {
                                            _bankStatementLine.SetC_Tax_ID(_SetTaxRate);
                                            var Rate = Util.GetValueOfDecimal(DB.ExecuteScalar("Select Rate From C_Tax Where C_Tax_ID=" + _SetTaxRate + " AND IsActive='Y'"));
                                            if (Rate > 0)
                                            {
                                                TaxAmt = Math.Round(Util.GetValueOfDecimal(_bankStatementLine.GetChargeAmt() - (_bankStatementLine.GetChargeAmt() / ((Rate / 100) + 1))), 2);
                                                _bankStatementLine.SetTaxAmt(TaxAmt);
                                            }
                                            else
                                            {
                                                _bankStatementLine.SetTaxAmt(0);
                                            }
                                            taxRate = Util.GetValueOfString(DB.ExecuteScalar("Select Name From C_Tax Where C_Tax_ID=" + _SetTaxRate + " AND IsActive='Y'"));
                                        }
                                        chargeType = Util.GetValueOfString(DB.ExecuteScalar("Select Name From C_CHARGE Where C_CHARGE_ID=" + _SetBankCharges + " AND IsActive='Y'"));
                                    }


                                    if (!_bankStatementLine.Save())
                                    {
                                        ValueNamePair vnp = null;
                                        vnp = VLogger.RetrieveError();
                                        _objResponse._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]) + " #" + vnp.Key + ":" + vnp.Name;
                                        _lstObjResponse.Add(_objResponse);
                                        LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _objResponse._error);
                                    }
                                    else
                                    {
                                        _objResponse._statementNo = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]);
                                        _objResponse._statementLine = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]);
                                        _objResponse._paymentNo = Util.GetValueOfString(_dsPayments.Tables[0].Rows[0]["PAYMENTNO"]);
                                        _objResponse._paymentOrCash = "P";

                                        DateTime date = Convert.ToDateTime(_dsStatements.Tables[0].Rows[i]["TRANSACTIONDATE"]);
                                        _objResponse._trxDate = date.ToShortDateString();
                                        _objResponse._trxNo = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["TRXNO"]);
                                        _objResponse._salesAmt = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["TRXAMT"]);
                                        _objResponse._netAmt = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["NETAMOUNT"]);
                                        _objResponse._difference = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]);

                                        if (_SetBankCharges > 0)
                                        {
                                            _objResponse._chargeType = chargeType;
                                        }
                                        else
                                        {
                                            _objResponse._chargeType = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["CHARGETYPE"]);
                                        }

                                        if (_SetTaxRate > 0)
                                        {
                                            _objResponse._taxRate = taxRate;
                                            _objResponse._taxAmt = Util.GetValueOfString(TaxAmt); //Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["TAXAMOUNT"]);
                                        }
                                        else
                                        {
                                            _objResponse._taxRate = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["Tax"]);
                                            _objResponse._taxAmt = Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["TAXAMOUNT"]);
                                        }

                                        //_objResponse._taxAmt = Util.GetValueOfString(TaxAmt);
                                        //_objResponse._chargeType = chargeType;
                                        //_objResponse._taxRate = taxRate;

                                        _checkUseNextTime = CheckUseNextTime(ctx, _bankStatementLine);
                                        if (_checkUseNextTime != "" && _checkUseNextTime != null)
                                        {
                                            _objResponse._warning = _checkUseNextTime;
                                        }
                                        _lstObjResponse.Add(_objResponse);
                                        LogEntry(_path, Msg.GetMsg(ctx, "VA012_StatementNumber") + ": " + _objResponse._statementNo + "," + Msg.GetMsg(ctx, "VA012_StatementLine") + ": " + _objResponse._statementLine + "," + Msg.GetMsg(ctx, "VA012_MatchedToCashLine") + ": " + _objResponse._paymentNo + " " + _objResponse._warning);
                                    }
                                }
                            }
                            #endregion Contra Match Case
                        }

                        //Set TaxRate ID if charge amount = Statement amount suggested by Ashish
                        if (Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_CHARGE_ID"]) > 0) 
                        {
                            if (Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]).Equals( Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["NETAMOUNT"])))
                            {
                                decimal TaxAmt = 0;
                                _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_dsStatements.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]), null);
                                if (_SetTaxRate > 0)
                                {
                                    _bankStatementLine.SetC_Tax_ID(_SetTaxRate);
                                    var Rate = Util.GetValueOfDecimal(DB.ExecuteScalar("Select Rate From C_Tax Where C_Tax_ID=" + _SetTaxRate + " AND IsActive='Y'"));
                                    if (Rate > 0)
                                    {
                                        TaxAmt = Math.Round(Util.GetValueOfDecimal(Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]) - (Util.GetValueOfDecimal(_dsStatements.Tables[0].Rows[i]["CHARGEAMOUNT"]) / ((Rate / 100) + 1))), 2);
                                        _bankStatementLine.SetTaxAmt(TaxAmt);
                                    }
                                    else
                                    {
                                        _bankStatementLine.SetTaxAmt(0);
                                    }
                                }
                                if (!_bankStatementLine.Save())
                                {
                                    ValueNamePair vnp = null;
                                    vnp = VLogger.RetrieveError();
                                    _objResponse._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_dsStatements.Tables[0].Rows[i]["STATEMENTLINENO"]) + " #" + vnp.Key + ":" + vnp.Name;
                                    _lstObjResponse.Add(_objResponse);
                                    LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _objResponse._error);
                                }
                            }
                        }
                    }
                }

                if (_dsPayments != null)
                {
                    _dsPayments.Dispose();
                }
                if (_dsStatements != null)
                {
                    _dsStatements.Dispose();
                }
                if (_dsCashLine != null)
                {
                    _dsCashLine.Dispose();
                }
            }
            catch (Exception e)
            {
                if (_dsPayments != null)
                {
                    _dsPayments.Dispose();
                }
                if (_dsStatements != null)
                {
                    _dsStatements.Dispose();
                }
                if (_dsCashLine != null)
                {
                    _dsCashLine.Dispose();
                }
                _objResponse._error = e.Message;
                _lstObjResponse.Add(_objResponse);
                LogEntry(_path, Msg.GetMsg(ctx, "VA012_Error") + ": " + _objResponse._error);
                LogEntry(_path, "== End :" + DateTime.Now.ToString() + " ==");
                return _lstObjResponse;
            }
            LogEntry(_path, "== End :" + DateTime.Now.ToString() + " ==");
            return _lstObjResponse;
        }
        #endregion
    }
}