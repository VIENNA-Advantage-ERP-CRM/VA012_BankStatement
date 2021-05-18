using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VAdvantage.Utility;
using VAdvantage.ProcessEngine;
using System.IO;
using System.Data;
using System.Data.OleDb;
using System.Web.Hosting;
using VAdvantage.Model;
using VAdvantage.DataBase;
using System.Net;
using System.Web;
using VAdvantage.Logging;

namespace VA012.Models
{
    #region Import Data
    /// <summary>
    /// Import Bank Statement.
    /// </summary>
    public class BankStatementDataImport
    {
        #region Variables
        int _AD_Org_ID = 0;
        int _C_BankAccount_ID = 0, Count = 0;
        int _c_payment_id = 0, _C_Currency_ID = 0;
        decimal? _payAmt = 0;
        string _FileName = string.Empty, _doctype = string.Empty;
        string _message = string.Empty, _date = string.Empty;
        MBankStatement _BnkStatm = null;
        MBankStatementLine _BnkStmtLine = null;
        DataSet PyDS = null;
        StringBuilder query = null, _Filenames = new StringBuilder();
        string _Extension = string.Empty, _FileLocation = string.Empty;
        #endregion
        public StatementResponse ImportStatement(Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges, DateTime? statementDate)
        {
            StatementResponse _obj = new StatementResponse();

            #region Period StartDate and End Date
            DateTime? _startdate = null;
            DateTime? _enddate = null;
            string _sqlDate = @"SELECT STARTDATE
                                FROM C_PERIOD
                                WHERE C_YEAR_ID =
                                  (SELECT (Y.C_YEAR_ID) AS C_YEAR_ID
                                        FROM C_YEAR Y
                                        INNER JOIN C_PERIOD P
                                        ON P.C_YEAR_ID        = Y.C_YEAR_ID
                                        WHERE Y.C_CALENDAR_ID =
                                          (SELECT C_CALENDAR_ID FROM AD_CLIENTINFO WHERE AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
                                          )
                                        AND TRUNC(SYSDATE) BETWEEN P.STARTDATE AND P.ENDDATE
                                        AND P.ISACTIVE = 'Y'
                                        AND Y.ISACTIVE ='Y'
                                  )
                                AND PERIODNO=1";
            _startdate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sqlDate));
            _sqlDate = @"SELECT ENDDATE
                                FROM C_PERIOD
                                WHERE C_YEAR_ID =
                                  (SELECT (Y.C_YEAR_ID) AS C_YEAR_ID
                                            FROM C_YEAR Y
                                            INNER JOIN C_PERIOD P
                                            ON P.C_YEAR_ID        = Y.C_YEAR_ID
                                            WHERE Y.C_CALENDAR_ID =
                                              (SELECT C_CALENDAR_ID FROM AD_CLIENTINFO WHERE AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
                                              )
                                            AND TRUNC(SYSDATE) BETWEEN P.STARTDATE AND P.ENDDATE
                                            AND P.ISACTIVE = 'Y'
                                            AND Y.ISACTIVE ='Y'
                                  )
                                AND PERIODNO=12";
            _enddate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sqlDate));

            #endregion


            int _existingStatementID = 0;
            string _statementDocStatus = "";
            int pageno = 1;
            int lineno = 10;


            DataSet _ds = new DataSet();
            //_ds = DB.ExecuteDataset("SELECT C_BANKSTATEMENT_ID,DOCSTATUS FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND NAME='" + _statementno + "'  AND TO_CHAR(STATEMENTDATE,'YYYY')=TO_CHAR(sysdate,'YYYY') ", null);
            _ds = DB.ExecuteDataset("SELECT C_BANKSTATEMENT_ID,DOCSTATUS FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND NAME='" + _statementno + "' AND STATEMENTDATE BETWEEN " + GlobalVariable.TO_DATE(_startdate, true) + " AND " + GlobalVariable.TO_DATE(_enddate, true), null);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    _existingStatementID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BANKSTATEMENT_ID"]);
                    _statementDocStatus = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCSTATUS"]);
                    if (_statementDocStatus == "CO")
                    {

                        _obj._error = "VA012_StatementAlreadyExist";
                        return _obj;
                    }
                    #region Get Page And Line
                    string _sql = @"SELECT MAX(BSL.VA012_PAGE) AS PAGE
                    FROM C_BANKSTATEMENTLINE BSL
                    INNER JOIN C_BANKSTATEMENT BS
                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + _existingStatementID;
                    pageno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (pageno <= 0)
                    {
                        pageno = 1;
                    }

                    //            _sql = @"SELECT MAX(BSL.LINE)+10  AS LINE
                    //                    FROM C_BANKSTATEMENTLINE BSL
                    //                    INNER JOIN C_BANKSTATEMENT BS
                    //                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.NAME ='" + _statementno + "' AND BSL.VA012_PAGE='" + pageno + "' AND TO_CHAR(BS.STATEMENTDATE,'YYYY')=TO_CHAR(sysdate,'YYYY')  ";
                    _sql = @"SELECT MAX(BSL.LINE)+10  AS LINE
                    FROM C_BANKSTATEMENTLINE BSL
                    INNER JOIN C_BANKSTATEMENT BS
                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + _existingStatementID + " AND BSL.VA012_PAGE='" + pageno + "'";

                    lineno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                    if (lineno <= 0)
                    {
                        lineno = 10;
                    }
                    #endregion


                }

            }



            _AD_Org_ID = Util.GetValueOfInt(ctx.GetAD_Org_ID());
            _C_BankAccount_ID = _bankaccount;
            string _accountType = Util.GetValueOfString(DB.ExecuteScalar("Select BankAccountType from C_BankAccount Where C_BankAccount_ID=" + _C_BankAccount_ID));


            // SaveAttachment();
            int _stementID = 0;
            _Filenames.Append(FileName + ",");
            // Number Of New Files Saved In Our System
            if (_Filenames.ToString() != "")
            {
                _Filenames.Remove(_Filenames.Length - 1, 1);
            }
            else
            {
                _obj._error = "VA012_AttachmentsAllreadyInSystem";
                return _obj;
            }

            // New Files To Update In Our System
            _message = _Filenames.ToString();
            string[] _filenamesall = _message.Split(',');
            for (int K = 0; K < _filenamesall.Length; K++)
            {
                _FileLocation = _filenamesall[K].ToString();
                string[] _FileNameExten = _FileLocation.Split('.');
                _FileName = _FileNameExten[0].ToString();
                _Extension = "." + _FileNameExten[1].ToString();
                // _FileNameExten = null;
                //_FileNameExten = _FileName.Split('-');
                // _date = _FileNameExten[1].ToString();

                if ((_Extension.ToUpper() == ".CSV") || (_Extension.ToUpper() == ".XLSX") || (_Extension.ToUpper() == ".XLS"))
                {
                    // DataSet ds = ImportFromCSV(HostingEnvironment.ApplicationPhysicalPath + @"\Attachment\" +_FileLocation, false);
                    DataSet ds = ExcelImport.ImportFromCSV(_path, false);
                    if (File.Exists(_path))
                    {
                        FileInfo fileToDelete = new FileInfo(_path);
                        fileToDelete.Delete();
                        //DirectoryInfo myDirInfo = new DirectoryInfo(filepath.Substring(0, filepath.LastIndexOf("\\")));
                        //foreach (FileInfo file in myDirInfo.GetFiles())
                        //{
                        //    file.Delete();
                        //}
                        //Directory.Delete(filepath.Substring(0, filepath.LastIndexOf("\\")));
                    }
                    if (ds != null)
                    {
                        #region Pattern For ICICI Bank E-Statements
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            for (int i = 0; i < dt.Rows.Count - 2; i++)
                            {

                                //For 1 to 3 lines of CSV which contains Nothing
                                if (i <= 3)
                                    continue;

                                #region For First Line Which Contains B/F (Balance Forward)

                                if (i == 4)
                                {
                                    _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
                                    if (_C_Currency_ID != _bankAccountCurrency)
                                    {
                                        _obj._error = "VA012_DiffAccountAndStatementCurrency";
                                        return _obj;
                                    }
                                    if (_existingStatementID <= 0)
                                    {
                                        _BnkStatm = new MBankStatement(Env.GetCtx(), 0, null);
                                        _BnkStatm.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                        _BnkStatm.SetAD_Org_ID(_AD_Org_ID);
                                        _BnkStatm.SetC_BankAccount_ID(_C_BankAccount_ID);
                                        _BnkStatm.SetName(_statementno);
                                        _BnkStatm.SetStatementDate(statementDate);
                                        _BnkStatm.SetBeginningBalance(Convert.ToDecimal(dt.Rows[i][9]));
                                        if (!_BnkStatm.Save())
                                        {
                                            //Used ValueNamePair to get error
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            //some times getting the error pp also
                                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                            if (string.IsNullOrEmpty(error))
                                            {
                                                error = pp != null ? pp.GetName() : "";
                                            }
                                            _obj._error = !string.IsNullOrEmpty(error) ? error : "VA012_BankStatementHeaderNotSaved";
                                            //_obj._error = "VA012_Header Not Saved Of Bank Statement";
                                            return _obj;
                                        }
                                        else
                                        {
                                            _stementID = _BnkStatm.Get_ID();
                                        }
                                    }
                                    else
                                    {
                                        _BnkStatm = new MBankStatement(Env.GetCtx(), _existingStatementID, null);

                                    }
                                }
                                #endregion

                                #region Rest All Other Entries Which Contains Data
                                else
                                {
                                    // If Check Number Exists
                                    if ((Convert.ToString(dt.Rows[i][0]) != string.Empty) && (Convert.ToString(dt.Rows[i][4]) != string.Empty) && (Convert.ToString(dt.Rows[i][6]) != string.Empty) && ((Convert.ToString(dt.Rows[i][7]) != string.Empty) || (Convert.ToString(dt.Rows[i][8]) != string.Empty)))
                                    {
                                        _BnkStmtLine = new MBankStatementLine(_BnkStatm);

                                        _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                        _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                        _BnkStmtLine.SetVA012_Page(pageno);
                                        _BnkStmtLine.SetLine(lineno);
                                        lineno = lineno + 10;
                                        _BnkStmtLine.SetStatementLineDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
                                        _BnkStmtLine.SetDateAcct(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
                                        _BnkStmtLine.SetValutaDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
                                        _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][3]));// Set Transaction Remarks
                                        _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][2]));// Set Transaction Purticular
                                        _BnkStmtLine.SetEftCheckNo(Convert.ToString(dt.Rows[i][4]));// Set Check Number
                                        _BnkStmtLine.SetMemo(Convert.ToString(dt.Rows[i][10]));// Set Deposite Branch
                                        _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
                                        if (_C_Currency_ID > 0)
                                        {
                                            _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
                                        }
                                        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                        {
                                            _payAmt = Convert.ToDecimal(dt.Rows[i][7]);
                                        }
                                        else
                                        {
                                            _payAmt = Convert.ToDecimal(dt.Rows[i][8]);
                                        }
                                        if (_accountType == "C")
                                        {

                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                            {
                                                _BnkStmtLine.SetStmtAmt(_payAmt);
                                                _BnkStmtLine.SetTrxAmt(_payAmt);

                                            }
                                            else
                                            {
                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                            }
                                        }
                                        else
                                        {
                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                            {
                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                            }
                                            else
                                            {
                                                _BnkStmtLine.SetStmtAmt(_payAmt);
                                                _BnkStmtLine.SetTrxAmt(_payAmt);
                                            }
                                        }

                                        //Set TrxNo Value if exists in Excel sheet
                                        if (!string.IsNullOrEmpty(Util.GetValueOfString(dt.Rows[i]["TrxNo"])))
                                        {
                                            _BnkStmtLine.Set_Value("TrxNo", Util.GetValueOfString(dt.Rows[i]["TrxNo"]));
                                        }
                                        //PyDS = DB.ExecuteDataset("SELECT cp.c_payment_id as c_payment_id,  cd.name as doctype,cp.c_invoice_id as c_invoice_id,cp.c_bpartner_id as c_bpartner_id FROM c_payment cp inner join c_doctype cd on cd.c_doctype_id= cp.c_doctype_id WHERE cp.c_bankaccount_id=" + _C_BankAccount_ID + " AND cp.c_currency_id     = " + _C_Currency_ID + " AND cp.checkno           ='" + Convert.ToString(dt.Rows[i][4]) + "' AND cp.payamt =" + _payAmt + "");
                                        //if (PyDS != null)
                                        //{
                                        //    if (PyDS.Tables[0].Rows.Count > 0)
                                        //    {
                                        //        _c_payment_id = Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_payment_id"]);
                                        //        _doctype = Convert.ToString(PyDS.Tables[0].Rows[0]["doctype"]);
                                        //        _BnkStmtLine.SetC_BPartner_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_bpartner_id"]));
                                        //        _BnkStmtLine.SetC_Invoice_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_invoice_id"]));
                                        //        if (_c_payment_id != null && _c_payment_id != 0)
                                        //        {
                                        //            if (_doctype == "AP Payment")
                                        //            {
                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
                                        //                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                        //                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                        //            }
                                        //            else if (_doctype == "AR Receipt")
                                        //            {
                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
                                        //                _BnkStmtLine.SetStmtAmt(_payAmt);
                                        //                _BnkStmtLine.SetTrxAmt(_payAmt);
                                        //            }

                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                        //        {
                                        //            _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                        //            _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                        //        }
                                        //        else
                                        //        {
                                        //            _BnkStmtLine.SetStmtAmt(_payAmt);
                                        //            _BnkStmtLine.SetTrxAmt(_payAmt);
                                        //        }
                                        //    }
                                        //}
                                        if (!_BnkStmtLine.Save())
                                        {
                                            //Used ValueNamePair to get error
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            //some times getting the error pp also
                                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                            if (string.IsNullOrEmpty(error))
                                            {
                                                error = pp != null ? pp.GetName() : "";
                                            }
                                            _obj._error = !string.IsNullOrEmpty(error) ? error : "VA012_StatementLineNotSaved";
                                            //_obj._error = "VA012_Statement Line Not Saved";
                                            return _obj;
                                        }
                                    }
                                    // If Check Number Doesn't Exists
                                    else
                                    {
                                        _BnkStmtLine = new MBankStatementLine(_BnkStatm);
                                        _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                        _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                        _BnkStmtLine.SetVA012_Page(pageno);
                                        _BnkStmtLine.SetLine(lineno);
                                        lineno = lineno + 10;
                                        _BnkStmtLine.SetStatementLineDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
                                        _BnkStmtLine.SetDateAcct(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
                                        _BnkStmtLine.SetValutaDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
                                        _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][3]));// Set Transaction Remarks
                                        _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][2]));// Set Transaction Purticular
                                        _BnkStmtLine.SetMemo(Convert.ToString(dt.Rows[i][10]));// Set Deposite Branch
                                        _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
                                        if (_C_Currency_ID > 0)
                                            _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
                                        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                        {
                                            _payAmt = Convert.ToDecimal(dt.Rows[i][7]);
                                        }
                                        else
                                        {
                                            _payAmt = Convert.ToDecimal(dt.Rows[i][8]);
                                        }

                                        if (_accountType == "C")
                                        {

                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                            {
                                                _BnkStmtLine.SetStmtAmt(_payAmt);
                                                _BnkStmtLine.SetTrxAmt(_payAmt);

                                            }
                                            else
                                            {
                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                            }
                                        }
                                        else
                                        {
                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                            {
                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                            }
                                            else
                                            {
                                                _BnkStmtLine.SetStmtAmt(_payAmt);
                                                _BnkStmtLine.SetTrxAmt(_payAmt);
                                            }
                                        }

                                        //Set TrxNo Value if exists in Excel sheet
                                        if (!string.IsNullOrEmpty(Util.GetValueOfString(dt.Rows[i]["TrxNo"])))
                                        {
                                            _BnkStmtLine.Set_Value("TrxNo", Util.GetValueOfString(dt.Rows[i]["TrxNo"]));
                                        }
                                        //PyDS = DB.ExecuteDataset("SELECT cp.c_payment_id as c_payment_id,  cd.name as doctype,cp.c_invoice_id as c_invoice_id,cp.c_bpartner_id as c_bpartner_id FROM c_payment cp inner join c_doctype cd on cd.c_doctype_id= cp.c_doctype_id WHERE cp.c_bankaccount_id=" + _C_BankAccount_ID + " AND cp.c_currency_id     = " + _C_Currency_ID + "  AND cp.payamt =" + _payAmt + "");
                                        //if (PyDS != null)
                                        //{
                                        //    if (PyDS.Tables[0].Rows.Count > 0)
                                        //    {
                                        //        _c_payment_id = Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_payment_id"]);
                                        //        _doctype = Convert.ToString(PyDS.Tables[0].Rows[0]["doctype"]);
                                        //        _BnkStmtLine.SetC_BPartner_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_bpartner_id"]));
                                        //        _BnkStmtLine.SetC_Invoice_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_invoice_id"]));
                                        //        if (_c_payment_id != null && _c_payment_id != 0)
                                        //        {
                                        //            if (_doctype == "AP Payment")
                                        //            {
                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
                                        //                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                        //                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                        //            }
                                        //            else if (_doctype == "AR Receipt")
                                        //            {
                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
                                        //                _BnkStmtLine.SetStmtAmt(_payAmt);
                                        //                _BnkStmtLine.SetTrxAmt(_payAmt);
                                        //            }

                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
                                        //        {
                                        //            _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                        //            _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
                                        //        }
                                        //        else
                                        //        {
                                        //            _BnkStmtLine.SetStmtAmt(_payAmt);
                                        //            _BnkStmtLine.SetTrxAmt(_payAmt);
                                        //        }
                                        //    }
                                        //}
                                        if (!_BnkStmtLine.Save())
                                        {
                                            //Used ValueNamePair to get error
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            //some times getting the error pp also
                                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                            if (string.IsNullOrEmpty(error))
                                            {
                                                error = pp != null ? pp.GetName() : "";
                                            }
                                            _obj._error = !string.IsNullOrEmpty(error) ? error : "VA012_StatementLineNotSaved";
                                            return _obj;
                                        }
                                    }

                                }
                                #endregion

                                _BnkStatm.SetEndingBalance(_BnkStatm.GetBeginningBalance() + _BnkStatm.GetStatementDifference());
                                if (!_BnkStatm.Save())
                                {
                                    //Used ValueNamePair to get error
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    //some times getting the error pp also
                                    string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        error = pp != null ? pp.GetName() : "";
                                    }
                                    _obj._error = !string.IsNullOrEmpty(error) ? error : "VA012_BeginningBalanceNotUpdated";
                                    // _obj._error = "Beginning Balance Not Updated";
                                    return _obj;
                                }
                            }
                        }
                        else
                        {
                            _obj._error = "VA012_NoRecordsInExcel";
                            return _obj;
                        }
                        #endregion
                    }
                    else
                    {
                        _obj._error = "VA012_NoRecordsInExcel";
                        return _obj;
                    }
                }
                else
                {
                    _obj._error = "VA012_FormatNotSupported";
                    return _obj;
                }
            }
            _obj._statementID = _stementID.ToString();
            return _obj;
        }
        //#region  Function to Import data from CSV File
        //public DataSet ImportFromCSV(string _FileLocation, bool _HasHeader)
        //{
        //    string HDR = _HasHeader ? "Yes" : "No";
        //    string strConn = string.Empty;

        //    if (_FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower() == ".xlsx")
        //        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
        //    else
        //        strConn = string.Format(
        //                @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
        //                    Path.GetDirectoryName(_FileLocation));

        //    DataSet output = new DataSet();

        //    try
        //    {
        //        if (_FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower() == ".xlsx")
        //        {
        //            using (OleDbConnection oledbconn = new OleDbConnection(strConn))
        //            {
        //                oledbconn.Open();

        //                DataTable schemaTable = oledbconn.GetOleDbSchemaTable(
        //               OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        //                foreach (DataRow schemaRow in schemaTable.Rows)
        //                {
        //                    string sheet = schemaRow["TABLE_NAME"].ToString();

        //                    if (!sheet.EndsWith("_"))
        //                    {
        //                        try
        //                        {
        //                            OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", oledbconn);
        //                            cmd.CommandType = CommandType.Text;

        //                            DataTable outputTable = new DataTable(sheet);
        //                            output.Tables.Add(outputTable);
        //                            new OleDbDataAdapter(cmd).Fill(outputTable);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            return null;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            using (OleDbConnection conn = new OleDbConnection(strConn))
        //            {
        //                conn.Open();
        //                DataTable schemaTable = conn.GetOleDbSchemaTable(
        //                    OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
        //                foreach (DataRow schemaRow in schemaTable.Rows)
        //                {
        //                    string sheet = schemaRow["TABLE_NAME"].ToString();

        //                    if (!sheet.EndsWith("_"))
        //                    {
        //                        try
        //                        {
        //                            OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
        //                            cmd.CommandType = CommandType.Text;
        //                            DataTable outputTable = new DataTable(sheet);
        //                            output.Tables.Add(outputTable);
        //                            new OleDbDataAdapter(cmd).Fill(outputTable);
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            return null;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _message = ex.Message;
        //        return output;
        //    }
        //    return output;
        //}
        //#endregion
    }
    ///// <summary>
    ///// Bank Statement Import Response Properties.
    ///// </summary>
    //public class StatementResponse
    //{
    //    public string _path { get; set; }
    //    public string _filename { get; set; }
    //    public string _error { get; set; }
    //    public string _statementID { get; set; }
    //    public string _orgfilename { get; set; }
    //}
    #endregion Import Data
}