using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using VAdvantage.Classes;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Process;
using VAdvantage.ProcessEngine;
using VAdvantage.Utility;

namespace VA012.Models
{
    #region Import Data
    //    /// <summary>
    //    /// Import Bank Statement.
    //    /// </summary>
    //    public class BankStatementDataImport
    //    {
    //        #region Variables
    //        int _AD_Org_ID = 0;
    //        int _C_BankAccount_ID = 0, Count = 0;
    //        int _c_payment_id = 0, _C_Currency_ID = 0;
    //        decimal? _payAmt = 0;
    //        string _FileName = string.Empty, _doctype = string.Empty;
    //        string _message = string.Empty, _date = string.Empty;
    //        MBankStatement _BnkStatm = null;
    //        MBankStatementLine _BnkStmtLine = null;
    //        DataSet PyDS = null;
    //        StringBuilder query = null, _Filenames = new StringBuilder();
    //        string _Extension = string.Empty, _FileLocation = string.Empty;
    //        #endregion
    //        public StatementResponse ImportStatement(Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno)
    //        {
    //            StatementResponse _obj = new StatementResponse();

    //            #region Period StartDate and End Date
    //            DateTime? _startdate = null;
    //            DateTime? _enddate = null;
    //            string _sqlDate = @"SELECT STARTDATE
    //                                FROM C_PERIOD
    //                                WHERE C_YEAR_ID =
    //                                  (SELECT MAX(C_YEAR_ID) AS C_YEAR_ID
    //                                  FROM C_YEAR
    //                                  WHERE C_CALENDAR_ID =
    //                                    (SELECT C_CALENDAR_ID FROM AD_CLIENTINFO WHERE AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
    //                                    )
    //                                  )
    //                                AND PERIODNO=1";
    //            _startdate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sqlDate));
    //            _sqlDate = @"SELECT ENDDATE
    //                                FROM C_PERIOD
    //                                WHERE C_YEAR_ID =
    //                                  (SELECT MAX(C_YEAR_ID) AS C_YEAR_ID
    //                                  FROM C_YEAR
    //                                  WHERE C_CALENDAR_ID =
    //                                    (SELECT C_CALENDAR_ID FROM AD_CLIENTINFO WHERE AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
    //                                    )
    //                                  )
    //                                AND PERIODNO=12";
    //            _enddate = Util.GetValueOfDateTime(DB.ExecuteScalar(_sqlDate));

    //            #endregion


    //            int _existingStatementID = 0;
    //            string _statementDocStatus = "";
    //            int pageno = 1;
    //            int lineno = 10;


    //            DataSet _ds = new DataSet();
    //            //_ds = DB.ExecuteDataset("SELECT C_BANKSTATEMENT_ID,DOCSTATUS FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND NAME='" + _statementno + "'  AND TO_CHAR(STATEMENTDATE,'YYYY')=TO_CHAR(sysdate,'YYYY') ", null);
    //            _ds = DB.ExecuteDataset("SELECT C_BANKSTATEMENT_ID,DOCSTATUS FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND NAME='" + _statementno + "' AND STATEMENTDATE BETWEEN " + GlobalVariable.TO_DATE(_startdate, true) + " AND " + GlobalVariable.TO_DATE(_enddate, true), null);
    //            if (_ds != null)
    //            {
    //                if (_ds.Tables[0].Rows.Count > 0)
    //                {
    //                    _existingStatementID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BANKSTATEMENT_ID"]);
    //                    _statementDocStatus = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCSTATUS"]);
    //                    if (_statementDocStatus == "CO")
    //                    {

    //                        _obj._error = "VA012_StatementAlreadyExist";
    //                        return _obj;
    //                    }
    //                    #region Get Page And Line
    //                    string _sql = @"SELECT MAX(BSL.VA012_PAGE) AS PAGE
    //                    FROM C_BANKSTATEMENTLINE BSL
    //                    INNER JOIN C_BANKSTATEMENT BS
    //                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + _existingStatementID;
    //                    pageno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
    //                    if (pageno <= 0)
    //                    {
    //                        pageno = 1;
    //                    }

    //                    //            _sql = @"SELECT MAX(BSL.LINE)+10  AS LINE
    //                    //                    FROM C_BANKSTATEMENTLINE BSL
    //                    //                    INNER JOIN C_BANKSTATEMENT BS
    //                    //                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.NAME ='" + _statementno + "' AND BSL.VA012_PAGE='" + pageno + "' AND TO_CHAR(BS.STATEMENTDATE,'YYYY')=TO_CHAR(sysdate,'YYYY')  ";
    //                    _sql = @"SELECT MAX(BSL.LINE)+10  AS LINE
    //                    FROM C_BANKSTATEMENTLINE BSL
    //                    INNER JOIN C_BANKSTATEMENT BS
    //                    ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID WHERE BS.C_BANKSTATEMENT_ID =" + _existingStatementID + "' AND BSL.VA012_PAGE='" + pageno + "'";

    //                    lineno = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
    //                    if (lineno <= 0)
    //                    {
    //                        lineno = 10;
    //                    }
    //                    #endregion


    //                }

    //            }



    //            _AD_Org_ID = Util.GetValueOfInt(ctx.GetAD_Org_ID());
    //            _C_BankAccount_ID = _bankaccount;
    //            string _accountType = Util.GetValueOfString(DB.ExecuteScalar("Select BankAccountType from C_BankAccount Where C_BankAccount_ID=" + _C_BankAccount_ID));


    //            // SaveAttachment();
    //            int _stementID = 0;
    //            _Filenames.Append(FileName + ",");
    //            // Number Of New Files Saved In Our System
    //            if (_Filenames.ToString() != "")
    //            {
    //                _Filenames.Remove(_Filenames.Length - 1, 1);
    //            }
    //            else
    //            {
    //                _obj._error = "VA012_AttachmentsAllreadyInSystem";
    //                return _obj;
    //            }

    //            // New Files To Update In Our System
    //            _message = _Filenames.ToString();
    //            string[] _filenamesall = _message.Split(',');
    //            for (int K = 0; K < _filenamesall.Length; K++)
    //            {
    //                _FileLocation = _filenamesall[K].ToString();
    //                string[] _FileNameExten = _FileLocation.Split('.');
    //                _FileName = _FileNameExten[0].ToString();
    //                _Extension = "." + _FileNameExten[1].ToString();
    //                // _FileNameExten = null;
    //                //_FileNameExten = _FileName.Split('-');
    //                // _date = _FileNameExten[1].ToString();

    //                if ((_Extension.ToUpper() == ".CSV") || (_Extension.ToUpper() == ".XLSX"))
    //                {
    //                    // DataSet ds = ImportFromCSV(HostingEnvironment.ApplicationPhysicalPath + @"\Attachment\" +_FileLocation, false);
    //                    DataSet ds = ImportFromCSV(_path, false);
    //                    if (File.Exists(_path))
    //                    {
    //                        FileInfo fileToDelete = new FileInfo(_path);
    //                        fileToDelete.Delete();
    //                        //DirectoryInfo myDirInfo = new DirectoryInfo(filepath.Substring(0, filepath.LastIndexOf("\\")));
    //                        //foreach (FileInfo file in myDirInfo.GetFiles())
    //                        //{
    //                        //    file.Delete();
    //                        //}
    //                        //Directory.Delete(filepath.Substring(0, filepath.LastIndexOf("\\")));
    //                    }
    //                    if (ds != null)
    //                    {
    //                        #region Pattern For ICICI Bank E-Statements
    //                        DataTable dt = ds.Tables[0];
    //                        if (dt.Rows.Count > 0)
    //                        {
    //                            for (int i = 0; i < dt.Rows.Count - 2; i++)
    //                            {

    //                                //For 1 to 3 lines of CSV which contains Nothing
    //                                if (i <= 3)
    //                                    continue;

    //                                #region For First Line Which Contains B/F (Balance Forward)

    //                                if (i == 4)
    //                                {
    //                                    _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
    //                                    if (_C_Currency_ID != _bankAccountCurrency)
    //                                    {
    //                                        _obj._error = "VA012_DiffAccountAndStatementCurrency";
    //                                        return _obj;
    //                                    }
    //                                    if (_existingStatementID <= 0)
    //                                    {
    //                                        _BnkStatm = new MBankStatement(Env.GetCtx(), 0, null);
    //                                        _BnkStatm.SetAD_Client_ID(ctx.GetAD_Client_ID());
    //                                        _BnkStatm.SetAD_Org_ID(_AD_Org_ID);
    //                                        _BnkStatm.SetC_BankAccount_ID(_C_BankAccount_ID);
    //                                        _BnkStatm.SetName(_statementno);
    //                                        _BnkStatm.SetStatementDate(Convert.ToDateTime(dt.Rows[i][1]));
    //                                        _BnkStatm.SetBeginningBalance(Convert.ToDecimal(dt.Rows[i][9]));
    //                                        if (!_BnkStatm.Save())
    //                                        {

    //                                            _obj._error = "VA012_BankStatementHeaderNotSaved";
    //                                            //_obj._error = "VA012_Header Not Saved Of Bank Statement";
    //                                            return _obj;
    //                                        }
    //                                        else
    //                                        {
    //                                            _stementID = _BnkStatm.Get_ID();
    //                                        }
    //                                    }
    //                                    else
    //                                    {
    //                                        _BnkStatm = new MBankStatement(Env.GetCtx(), _existingStatementID, null);

    //                                    }
    //                                }
    //                                #endregion

    //                                #region Rest All Other Entries Which Contains Data
    //                                else
    //                                {
    //                                    // If Check Number Exists
    //                                    if ((Convert.ToString(dt.Rows[i][0]) != string.Empty) && (Convert.ToString(dt.Rows[i][4]) != string.Empty) && (Convert.ToString(dt.Rows[i][6]) != string.Empty) && ((Convert.ToString(dt.Rows[i][7]) != string.Empty) || (Convert.ToString(dt.Rows[i][8]) != string.Empty)))
    //                                    {
    //                                        _BnkStmtLine = new MBankStatementLine(_BnkStatm);

    //                                        _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
    //                                        _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
    //                                        _BnkStmtLine.SetVA012_Page(pageno);
    //                                        _BnkStmtLine.SetLine(lineno);
    //                                        lineno = lineno + 10;
    //                                        _BnkStmtLine.SetStatementLineDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
    //                                        _BnkStmtLine.SetDateAcct(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
    //                                        _BnkStmtLine.SetValutaDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
    //                                        _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][3]));// Set Transaction Remarks
    //                                        _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][2]));// Set Transaction Purticular
    //                                        _BnkStmtLine.SetEftCheckNo(Convert.ToString(dt.Rows[i][4]));// Set Check Number
    //                                        _BnkStmtLine.SetMemo(Convert.ToString(dt.Rows[i][10]));// Set Deposite Branch
    //                                        _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
    //                                        if (_C_Currency_ID > 0)
    //                                        {
    //                                            _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
    //                                        }
    //                                        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                        {
    //                                            _payAmt = Convert.ToDecimal(dt.Rows[i][7]);
    //                                        }
    //                                        else
    //                                        {
    //                                            _payAmt = Convert.ToDecimal(dt.Rows[i][8]);
    //                                        }
    //                                        if (_accountType == "C")
    //                                        {

    //                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                                _BnkStmtLine.SetTrxAmt(_payAmt);

    //                                            }
    //                                            else
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                            }
    //                                        }
    //                                        else
    //                                        {
    //                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                            }
    //                                            else
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                                _BnkStmtLine.SetTrxAmt(_payAmt);
    //                                            }
    //                                        }
    //                                        //PyDS = DB.ExecuteDataset("SELECT cp.c_payment_id as c_payment_id,  cd.name as doctype,cp.c_invoice_id as c_invoice_id,cp.c_bpartner_id as c_bpartner_id FROM c_payment cp inner join c_doctype cd on cd.c_doctype_id= cp.c_doctype_id WHERE cp.c_bankaccount_id=" + _C_BankAccount_ID + " AND cp.c_currency_id     = " + _C_Currency_ID + " AND cp.checkno           ='" + Convert.ToString(dt.Rows[i][4]) + "' AND cp.payamt =" + _payAmt + "");
    //                                        //if (PyDS != null)
    //                                        //{
    //                                        //    if (PyDS.Tables[0].Rows.Count > 0)
    //                                        //    {
    //                                        //        _c_payment_id = Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_payment_id"]);
    //                                        //        _doctype = Convert.ToString(PyDS.Tables[0].Rows[0]["doctype"]);
    //                                        //        _BnkStmtLine.SetC_BPartner_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_bpartner_id"]));
    //                                        //        _BnkStmtLine.SetC_Invoice_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_invoice_id"]));
    //                                        //        if (_c_payment_id != null && _c_payment_id != 0)
    //                                        //        {
    //                                        //            if (_doctype == "AP Payment")
    //                                        //            {
    //                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
    //                                        //                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //            }
    //                                        //            else if (_doctype == "AR Receipt")
    //                                        //            {
    //                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
    //                                        //                _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                        //                _BnkStmtLine.SetTrxAmt(_payAmt);
    //                                        //            }

    //                                        //        }
    //                                        //    }
    //                                        //    else
    //                                        //    {
    //                                        //        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                        //        {
    //                                        //            _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //            _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //        }
    //                                        //        else
    //                                        //        {
    //                                        //            _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                        //            _BnkStmtLine.SetTrxAmt(_payAmt);
    //                                        //        }
    //                                        //    }
    //                                        //}
    //                                        if (!_BnkStmtLine.Save())
    //                                        {
    //                                            _obj._error = "VA012_StatementLineNotSaved";
    //                                            //_obj._error = "VA012_Statement Line Not Saved";
    //                                            return _obj;
    //                                        }
    //                                    }
    //                                    // If Check Number Doesn't Exists
    //                                    else
    //                                    {
    //                                        _BnkStmtLine = new MBankStatementLine(_BnkStatm);
    //                                        _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
    //                                        _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
    //                                        _BnkStmtLine.SetVA012_Page(pageno);
    //                                        _BnkStmtLine.SetLine(lineno);
    //                                        lineno = lineno + 10;
    //                                        _BnkStmtLine.SetStatementLineDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
    //                                        _BnkStmtLine.SetDateAcct(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
    //                                        _BnkStmtLine.SetValutaDate(Convert.ToDateTime(dt.Rows[i][1]));// Set Transaction Date
    //                                        _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][3]));// Set Transaction Remarks
    //                                        _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][2]));// Set Transaction Purticular
    //                                        _BnkStmtLine.SetMemo(Convert.ToString(dt.Rows[i][10]));// Set Deposite Branch
    //                                        _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
    //                                        if (_C_Currency_ID > 0)
    //                                            _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
    //                                        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                        {
    //                                            _payAmt = Convert.ToDecimal(dt.Rows[i][7]);
    //                                        }
    //                                        else
    //                                        {
    //                                            _payAmt = Convert.ToDecimal(dt.Rows[i][8]);
    //                                        }

    //                                        if (_accountType == "C")
    //                                        {

    //                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                                _BnkStmtLine.SetTrxAmt(_payAmt);

    //                                            }
    //                                            else
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                            }
    //                                        }
    //                                        else
    //                                        {
    //                                            if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                            }
    //                                            else
    //                                            {
    //                                                _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                                _BnkStmtLine.SetTrxAmt(_payAmt);
    //                                            }
    //                                        }


    //                                        //PyDS = DB.ExecuteDataset("SELECT cp.c_payment_id as c_payment_id,  cd.name as doctype,cp.c_invoice_id as c_invoice_id,cp.c_bpartner_id as c_bpartner_id FROM c_payment cp inner join c_doctype cd on cd.c_doctype_id= cp.c_doctype_id WHERE cp.c_bankaccount_id=" + _C_BankAccount_ID + " AND cp.c_currency_id     = " + _C_Currency_ID + "  AND cp.payamt =" + _payAmt + "");
    //                                        //if (PyDS != null)
    //                                        //{
    //                                        //    if (PyDS.Tables[0].Rows.Count > 0)
    //                                        //    {
    //                                        //        _c_payment_id = Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_payment_id"]);
    //                                        //        _doctype = Convert.ToString(PyDS.Tables[0].Rows[0]["doctype"]);
    //                                        //        _BnkStmtLine.SetC_BPartner_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_bpartner_id"]));
    //                                        //        _BnkStmtLine.SetC_Invoice_ID(Convert.ToInt32(PyDS.Tables[0].Rows[0]["c_invoice_id"]));
    //                                        //        if (_c_payment_id != null && _c_payment_id != 0)
    //                                        //        {
    //                                        //            if (_doctype == "AP Payment")
    //                                        //            {
    //                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
    //                                        //                _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //                _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //            }
    //                                        //            else if (_doctype == "AR Receipt")
    //                                        //            {
    //                                        //                _BnkStmtLine.SetC_Payment_ID(_c_payment_id);
    //                                        //                _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                        //                _BnkStmtLine.SetTrxAmt(_payAmt);
    //                                        //            }

    //                                        //        }
    //                                        //    }
    //                                        //    else
    //                                        //    {
    //                                        //        if ((Convert.ToString(dt.Rows[i][7]) != string.Empty) && (Convert.ToString(dt.Rows[i][7]) != "0"))
    //                                        //        {
    //                                        //            _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //            _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _payAmt));
    //                                        //        }
    //                                        //        else
    //                                        //        {
    //                                        //            _BnkStmtLine.SetStmtAmt(_payAmt);
    //                                        //            _BnkStmtLine.SetTrxAmt(_payAmt);
    //                                        //        }
    //                                        //    }
    //                                        //}
    //                                        if (!_BnkStmtLine.Save())
    //                                        {

    //                                        }
    //                                    }

    //                                }
    //                                #endregion

    //                                _BnkStatm.SetEndingBalance(_BnkStatm.GetBeginningBalance() + _BnkStatm.GetStatementDifference());
    //                                if (!_BnkStatm.Save())
    //                                {
    //                                    _obj._error = "VA012_BeginningBalanceNotUpdated";
    //                                    // _obj._error = "Beginning Balance Not Updated";
    //                                    return _obj;
    //                                }
    //                            }
    //                        }
    //                        else
    //                        {
    //                            _obj._error = "VA012_NoRecordsInExcel";
    //                            return _obj;
    //                        }
    //                        #endregion
    //                    }
    //                    else
    //                    {
    //                        _obj._error = "VA012_NoRecordsInExcel";
    //                        return _obj;
    //                    }
    //                }
    //            }
    //            _obj._statementID = _stementID.ToString();
    //            return _obj;
    //        }
    //        #region  Function to Import data from CSV File
    //        public DataSet ImportFromCSV(string _FileLocation, bool _HasHeader)
    //        {
    //            string HDR = _HasHeader ? "Yes" : "No";
    //            string strConn = string.Empty;

    //            if (_FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower() == ".xlsx")
    //                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
    //            else
    //                strConn = string.Format(
    //                        @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
    //                            Path.GetDirectoryName(_FileLocation));

    //            DataSet output = new DataSet();

    //            try
    //            {
    //                if (_FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower() == ".xlsx")
    //                {
    //                    using (OleDbConnection oledbconn = new OleDbConnection(strConn))
    //                    {
    //                        oledbconn.Open();

    //                        DataTable schemaTable = oledbconn.GetOleDbSchemaTable(
    //                       OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
    //                        foreach (DataRow schemaRow in schemaTable.Rows)
    //                        {
    //                            string sheet = schemaRow["TABLE_NAME"].ToString();

    //                            if (!sheet.EndsWith("_"))
    //                            {
    //                                try
    //                                {
    //                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", oledbconn);
    //                                    cmd.CommandType = CommandType.Text;

    //                                    DataTable outputTable = new DataTable(sheet);
    //                                    output.Tables.Add(outputTable);
    //                                    new OleDbDataAdapter(cmd).Fill(outputTable);
    //                                }
    //                                catch (Exception ex)
    //                                {
    //                                    return null;
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //                else
    //                {
    //                    using (OleDbConnection conn = new OleDbConnection(strConn))
    //                    {
    //                        conn.Open();
    //                        DataTable schemaTable = conn.GetOleDbSchemaTable(
    //                            OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
    //                        foreach (DataRow schemaRow in schemaTable.Rows)
    //                        {
    //                            string sheet = schemaRow["TABLE_NAME"].ToString();

    //                            if (!sheet.EndsWith("_"))
    //                            {
    //                                try
    //                                {
    //                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", conn);
    //                                    cmd.CommandType = CommandType.Text;
    //                                    DataTable outputTable = new DataTable(sheet);
    //                                    output.Tables.Add(outputTable);
    //                                    new OleDbDataAdapter(cmd).Fill(outputTable);
    //                                }
    //                                catch (Exception ex)
    //                                {
    //                                    return null;
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                _message = ex.Message;
    //                return output;
    //            }
    //            return output;
    //        }
    //        #endregion
    //    }
    #endregion

    /// <summary>
    /// Bank Statement Import Response Properties.
    /// </summary>
    public class StatementResponse
    {
        public string _path { get; set; }
        public string _filename { get; set; }
        public string _error { get; set; }
        public string _statementID { get; set; }
        public string _orgfilename { get; set; }
    }
    //    #endregion Import Data


    /// <summary>
    /// Bank Statement Operations.
    /// </summary>
    public class StatementOperations
    {
        //bool _isPaymentFromCash = false;
        /// <summary>
        /// Create or Update the Payment. BankStatement and BankStatement Line
        /// using references of Invoice or Order or CashJournalLine
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_formData">List of StatementProp class properties</param>
        /// <returns>Success or Error Message (string type)</returns>
        public string InsertData(Ctx ctx, List<StatementProp> _formData)
        {
            int _existingStatementID = 0;
            int _existingAccountID = 0;
            string _statementDocStatus = "";
            int _statementPaymentID = 0;
            int _statementChargeID = 0;
            int _statementCashLineID = 0;
            string _isMatchingConfirmed = "N";
            DataSet _ds = new DataSet();
            string _qryStmt = "";

            //Using transaction to handle the Exception while Saving the data
            Trx trx = Trx.GetTrx(Trx.CreateTrxName("STrx"));

            if (Util.GetValueOfInt(_formData[0]._bankStatementLineID) > 0)
            {
                _qryStmt = @"SELECT BS.C_BANKSTATEMENT_ID,BS.C_BANKACCOUNT_ID,
                            BS.DOCSTATUS,
                            NVL(BSL.C_PAYMENT_ID,0) AS C_PAYMENT_ID,
                            NVL(BSL.C_CHARGE_ID,0)  AS C_CHARGE_ID,
                            NVL(BSL.C_CASHLINE_ID,0) AS C_CASHLINE_ID,
                            BSL.VA012_ISMATCHINGCONFIRMED
                        FROM C_BANKSTATEMENT BS
                        INNER JOIN C_BANKSTATEMENTLINE BSL
                        ON BS.C_BANKSTATEMENT_ID=BSL.C_BANKSTATEMENT_ID
                        WHERE BS.ISACTIVE       ='Y' 
                        AND BSL.C_BANKSTATEMENTLINE_ID=" + _formData[0]._bankStatementLineID + 
                        " AND BS.AD_Client_ID = " + ctx.GetAD_Client_ID();
            }
            else
            {
                //_qryStmt = "SELECT C_BANKSTATEMENT_ID,DOCSTATUS,0 AS C_PAYMENT_ID, 0 AS C_CHARGE_ID, 'N' AS VA012_ISMATCHINGCONFIRMED FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND NAME='" + _formData[0]._txtStatementNo + "' AND TO_CHAR(BS.STATEMENTDATE,'YYYY')=TO_CHAR(sysdate,'YYYY')";
                //not required start and end date filters
                _qryStmt = @"SELECT C_BANKSTATEMENT_ID,C_BANKACCOUNT_ID,DOCSTATUS,0 AS C_PAYMENT_ID,
                            0 AS C_CHARGE_ID, 0 AS C_CASHLINE_ID, 'N' AS VA012_ISMATCHINGCONFIRMED 
                            FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' AND AD_Client_ID = " + ctx.GetAD_Client_ID() + " AND NAME='" + _formData[0]._txtStatementNo + "'";/*  AND STATEMENTDATE BETWEEN " + GlobalVariable.TO_DATE(_startdate, true) + " AND " + GlobalVariable.TO_DATE(_enddate, true)*/
            }
            //used transaction trx
            _ds = DB.ExecuteDataset(_qryStmt, null, trx);
            if (_ds != null)
            {
                if (_ds.Tables[0].Rows.Count > 0)
                {
                    _existingStatementID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BANKSTATEMENT_ID"]);
                    _existingAccountID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BANKACCOUNT_ID"]);
                    _statementDocStatus = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCSTATUS"]);
                    _statementPaymentID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_PAYMENT_ID"]);
                    _statementChargeID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_CHARGE_ID"]);
                    _statementCashLineID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_CASHLINE_ID"]);
                    _isMatchingConfirmed = Util.GetValueOfString(_ds.Tables[0].Rows[0]["VA012_ISMATCHINGCONFIRMED"]);

                    if (_formData[0]._bankStatementLineID > 0)
                    {
                        if (_statementDocStatus == "CO" || _statementDocStatus == "CL" || _statementDocStatus == "RE" || _statementDocStatus == "VO")
                        {
                            //closing transaction
                            trx.Rollback();
                            //close the transaction
                            trx.Close();
                            //clear the object
                            trx = null;
                            return "VA012_CompletedRecordCantUpdate";
                        }

                    }
                    else
                    {
                        //Statement should check the status Drafted as well as Inprogress
                        if (_statementDocStatus.Equals(X_C_BankStatement.DOCSTATUS_Drafted) || _statementDocStatus.Equals(X_C_BankStatement.DOCSTATUS_InProgress))
                        {
                            if (_existingAccountID > 0 && _existingAccountID != _formData[0]._cmbBankAccount)
                            {
                                //closing transaction
                                trx.Rollback();
                                //close the transaction
                                trx.Close();
                                //clear the object
                                trx = null;
                                return "VA012_StatementAlreadyExistDiffAcc";
                            }
                        }
                        else if (_statementDocStatus == "CO" || _statementDocStatus == "CL" || _statementDocStatus == "RE" || _statementDocStatus == "VO")
                        {
                            //closing transaction
                            trx.Rollback();
                            //close the transaction
                            trx.Close();
                            //clear the object
                            trx = null;
                            return "VA012_StatementAlreadyExist";
                        }
                    }
                }
            }
            //Transaction should not allow when Statement Date is lesser than previous Statement Date of BankStatement
            if (_existingStatementID == 0)
            {
                //Compare the Statement date with previous Bankstatement records statement Date which is not consider the records in Status as void or Reversed 
                _qryStmt = Util.GetValueOfString(DB.ExecuteScalar(@"SELECT COUNT(C_BankStatement_ID) FROM C_BankStatement WHERE IsActive = 'Y' AND DocStatus NOT IN ('VO', 'RE') AND StatementDate > "
                    + GlobalVariable.TO_DATE(_formData[0]._dtStatementDate, true) + " AND C_BankAccount_ID = " + _formData[0]._cmbBankAccount, null, trx));
                if (Util.GetValueOfInt(_qryStmt) > 0)
                {
                    //closing transaction
                    trx.Rollback();
                    //close the transaction
                    trx.Close();
                    //clear the object
                    trx = null;
                    return "VIS_BankStatementDate";
                }
            }
            //get the Org_ID from bank account
            _formData[0]._bankAcctOrg_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Org_ID FROM C_BankAccount WHERE IsActive ='Y' AND AD_Client_ID=" + ctx.GetAD_Client_ID() + " AND C_BankAccount_ID=" + _formData[0]._cmbBankAccount));

            string schedulePaymentResult = "";
            string orderPaymentResult = "";
            string chargePaymentResult = "";
            //not in Use
            //string cashPaymentResult = "";

            int paymentID = 0;
            //Used string.IsNullOrEmpty() method to check weather the property have value  or not
            if (_formData[0]._ctrlPayment <= 0 && !string.IsNullOrEmpty(_formData[0]._scheduleList))
            {
                //added trx parameter to handle the transaction
                schedulePaymentResult = CreatePaymentFromSchedule(ctx, _formData, trx);
                if (int.TryParse(schedulePaymentResult, out paymentID))
                {
                }
                else
                {
                    //close the transaction
                    trx.Close();
                    //clear the object
                    trx = null;
                    return schedulePaymentResult;
                }
            }
            if (_formData[0]._ctrlPayment <= 0 && _formData[0]._ctrlOrder > 0)
            {
                //added trx parameter to handle the transaction
                orderPaymentResult = CreatePaymentFromOrder(ctx, _formData, trx);
                if (int.TryParse(orderPaymentResult, out paymentID))
                {
                }
                else
                {
                    //close the transaction
                    trx.Close();
                    //clear the object
                    trx = null;
                    return orderPaymentResult;
                }
            }
            //not create Payment with Charge if Transaction is Prepy Order
            if (_formData[0]._cmbCharge > 0 && _formData[0]._ctrlBusinessPartner > 0 && _formData[0]._ctrlPayment <= 0 && paymentID == 0 && _formData[0]._ctrlOrder <= 0)
            {
                //added trx parameter to handle the transaction
                chargePaymentResult = CreatePaymentFromCharge(ctx, _formData, trx);
                if (int.TryParse(chargePaymentResult, out paymentID))
                {
                }
                else
                {
                    //close the transaction
                    trx.Close();
                    //clear the object
                    trx = null;
                    return chargePaymentResult;
                }
            }

            if (_formData[0]._cmbContraType == "CB" && _formData[0]._cmbCashBook > 0)
            {

                //cashPaymentResult = CreatePaymentFromCash(ctx, _formData);
                //if (int.TryParse(cashPaymentResult, out paymentID))
                //{

                //}
                //else
                //{
                //    return cashPaymentResult;
                //}
            }
                MBankStatement _bankStatement = null;
                /*change by pratap*/
                MPayment paymentrecord = null;
                MDocType documentType = null;
                /*change by pratap*/
                MBankStatementLine _bankStatementLine = null;
            if (_existingStatementID <= 0)
            {
                //used transaction trx to handle the Errors
                _bankStatement = new MBankStatement(ctx, 0, trx);
                _bankStatement.SetAD_Client_ID(ctx.GetAD_Client_ID());
                //_bankStatement.SetAD_Org_ID(ctx.GetAD_Org_ID());
                //set Org_ID based on BankAccount
                _bankStatement.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                _bankStatement.SetC_BankAccount_ID(_formData[0]._cmbBankAccount);
                _bankStatement.SetName(_formData[0]._txtStatementNo);
                _bankStatement.SetStatementDate(_formData[0]._dtStatementDate);

                decimal CurrentBalance = Util.GetValueOfDecimal(DB.ExecuteScalar("SELECT CurrentBalance FROM C_BankAccount WHERE C_BankAccount_ID=" + _formData[0]._cmbBankAccount));
                _bankStatement.SetBeginningBalance(CurrentBalance);


                if (!_bankStatement.Save())
                {
                    //used transaction trx
                    trx.Rollback();
                    //close the transaction
                    trx.Close();
                    //set object value as null
                    trx = null;
                    ValueNamePair pp = VLogger.RetrieveError();
                    string error = pp != null ? pp.GetValue() : "";
                    if (string.IsNullOrEmpty(error))
                    {
                        error = pp != null ? pp.GetName() : "";
                    }
                    return !string.IsNullOrEmpty(error) ? error : "VA012_ErrorSavingBankStatement";
                }
            }
            else
            {
                //used transaction trx
                _bankStatement = new MBankStatement(ctx, _existingStatementID, trx);
            }
            if (_formData[0]._bankStatementLineID > 0)
            {
                //used transaction trx
                _bankStatementLine = new MBankStatementLine(ctx, _formData[0]._bankStatementLineID, trx);
            }
            else
            {
                _bankStatementLine = new MBankStatementLine(_bankStatement);
            }

            if (_statementPaymentID > 0 || _statementCashLineID > 0)
            {
                _bankStatementLine.SetDescription(HttpUtility.HtmlDecode(_formData[0]._txtDescription));

                //string trxNumOnPmt = Util.GetValueOfString(DB.ExecuteScalar("SELECT CP.TRXNO, CP.VA034_DEPOSITSLIPNO FROM C_PAYMENT CP LEFT JOIN VA009_PAYMENTMETHOD PM ON PM.VA009_PAYMENTMETHOD_ID = CP.VA009_PAYMENTMETHOD_ID WHERE (PM.VA009_PAYMENTBASETYPE = 'K' OR PM.VA009_PAYMENTBASETYPE = 'C') AND CP.C_PAYMENT_ID= " + _formData[0]._ctrlPayment));

                int _CountVA034 = Env.IsModuleInstalled("VA034_") ? 1 : 0;

                string trxNumOnPmt = "";

                if (_CountVA034 > 0)
                {
                    trxNumOnPmt = Util.GetValueOfString(DB.ExecuteScalar("SELECT CP.TRXNO, CP.VA034_DEPOSITSLIPNO FROM C_PAYMENT CP LEFT JOIN VA009_PAYMENTMETHOD PM ON PM.VA009_PAYMENTMETHOD_ID = CP.VA009_PAYMENTMETHOD_ID WHERE (PM.VA009_PAYMENTBASETYPE = 'K' OR PM.VA009_PAYMENTBASETYPE = 'C') AND CP.C_PAYMENT_ID= " + _formData[0]._ctrlPayment));
                }
                else
                {
                    trxNumOnPmt = Util.GetValueOfString(DB.ExecuteScalar("SELECT CP.TRXNO FROM C_PAYMENT CP LEFT JOIN VA009_PAYMENTMETHOD PM ON PM.VA009_PAYMENTMETHOD_ID = CP.VA009_PAYMENTMETHOD_ID WHERE (PM.VA009_PAYMENTBASETYPE = 'K' OR PM.VA009_PAYMENTBASETYPE = 'C') AND CP.C_PAYMENT_ID= " + _formData[0]._ctrlPayment));
                }

                if (string.IsNullOrEmpty(trxNumOnPmt))
                {
                    _bankStatementLine.SetVA012_VoucherNo(HttpUtility.HtmlDecode(_formData[0]._txtVoucherNo));
                }

                if (string.IsNullOrEmpty((string)_bankStatementLine.Get_Value("TRXNO")) && !string.IsNullOrEmpty(trxNumOnPmt))
                {
                    _bankStatementLine.Set_Value("TrxNo", HttpUtility.HtmlDecode(_formData[0]._txtVoucherNo));
                }
            }
            else
            {
                #region New Record                

                //string queryPmt = "SELECT CP.TRXNO, CP.VA034_DEPOSITSLIPNO FROM C_PAYMENT CP LEFT JOIN VA009_PAYMENTMETHOD PM ON PM.VA009_PAYMENTMETHOD_ID = CP.VA009_PAYMENTMETHOD_ID WHERE (PM.VA009_PAYMENTBASETYPE = 'K' OR PM.VA009_PAYMENTBASETYPE = 'C') AND CP.C_PAYMENT_ID= " + _formData[0]._ctrlPayment;
                //DataSet dsPmt = DB.ExecuteDataset(queryPmt, null);
                //if (dsPmt != null && dsPmt.Tables[0] != null && dsPmt.Tables[0].Rows.Count > 0)
                //{
                //}

                int _CountVA034 = Env.IsModuleInstalled("VA034_") ? 1 : 0;

                string trxNumOnPmt = "";

                if (_CountVA034 > 0)
                {
                    trxNumOnPmt = Util.GetValueOfString(DB.ExecuteScalar("SELECT CP.TRXNO, CP.VA034_DEPOSITSLIPNO FROM C_PAYMENT CP LEFT JOIN VA009_PAYMENTMETHOD PM ON PM.VA009_PAYMENTMETHOD_ID = CP.VA009_PAYMENTMETHOD_ID WHERE (PM.VA009_PAYMENTBASETYPE = 'K' OR PM.VA009_PAYMENTBASETYPE = 'C') AND CP.C_PAYMENT_ID= " + _formData[0]._ctrlPayment));
                }
                else
                {
                    trxNumOnPmt = Util.GetValueOfString(DB.ExecuteScalar("SELECT CP.TRXNO FROM C_PAYMENT CP LEFT JOIN VA009_PAYMENTMETHOD PM ON PM.VA009_PAYMENTMETHOD_ID = CP.VA009_PAYMENTMETHOD_ID WHERE (PM.VA009_PAYMENTBASETYPE = 'K' OR PM.VA009_PAYMENTBASETYPE = 'C') AND CP.C_PAYMENT_ID= " + _formData[0]._ctrlPayment));
                }

                if (_formData[0]._bankStatementLineID <= 0)
                {
                    //Set Line Organization with Bank Organization reference
                    _bankStatementLine.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                    _bankStatementLine.SetStatementLineDate(_formData[0]._dtStatementDate);
                    _bankStatementLine.SetDateAcct(_formData[0]._dtStatementDate);
                    _bankStatementLine.SetValutaDate(_formData[0]._dtStatementDate);
                }
                _bankStatementLine.SetDescription(HttpUtility.HtmlDecode(_formData[0]._txtDescription));

                if (string.IsNullOrEmpty(trxNumOnPmt))
                {
                    _bankStatementLine.SetVA012_VoucherNo(HttpUtility.HtmlDecode(_formData[0]._txtVoucherNo));
                }

                if (string.IsNullOrEmpty((string)_bankStatementLine.Get_Value("TRXNO")) && !string.IsNullOrEmpty(trxNumOnPmt))
                {
                    _bankStatementLine.Set_Value("TrxNo", HttpUtility.HtmlDecode(_formData[0]._txtVoucherNo));
                }
                _bankStatementLine.SetLine(_formData[0]._txtStatementLine);

                _bankStatementLine.SetVA012_Page(_formData[0]._txtStatementPage);
                _bankStatementLine.SetC_Currency_ID(_formData[0]._cmbCurrency);
                //added new field and Set the ConversionType selected on the form
                _bankStatementLine.Set_Value("C_ConversionType_ID", _formData[0]._txtConversionType);
                //TaxAmount will update on Line when DifferenceType is charge not always
                //_bankStatementLine.SetTaxAmt(Util.GetValueOfDecimal(_formData[0]._txtTaxAmount));

                /*chnage by pratap*/
                decimal differenceAmount = 0;
                //if (_formData[0]._txtDifference != 0)
                //{
                //    if (Math.Abs(_formData[0]._txtTrxAmt) > Math.Abs(_formData[0]._txtAmount))
                //    {

                //        differenceAmount = Math.Abs(_formData[0]._txtDifference) * -1;

                //    }
                //    else if (Math.Abs(_formData[0]._txtTrxAmt) < Math.Abs(_formData[0]._txtAmount))
                //    {
                //        differenceAmount = Math.Abs(_formData[0]._txtDifference);
                //    }
                //}
                if (_formData[0]._ctrlPayment > 0)
                {
                    //used transaction trx
                    paymentrecord = new MPayment(ctx, _formData[0]._ctrlPayment, trx);
                }
                else if (paymentID > 0)
                {
                    //used transaction trx
                    paymentrecord = new MPayment(ctx, paymentID, trx);
                }
                if (paymentrecord != null)
                {

                    //int _CountVA034 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA034_' AND IsActive='Y'"));

                    if (_CountVA034 > 0)
                    {
                        paymentrecord.SetVA034_DepositSlipNo(HttpUtility.HtmlDecode(_formData[0]._txtVoucherNo));
                        //paymentrecord.Save();
                        //Used condition to check payment Saved or not
                        if (!paymentrecord.Save()) {
                            //Used transaction 
                            trx.Rollback();
                            //close the transaction
                            trx.Close();
                            //clear the object
                            trx = null;
                            //Used ValueNamePair class to get the Error
                            ValueNamePair pp = VLogger.RetrieveError();
                            string error = pp != null ? pp.GetValue() : "";
                            if (string.IsNullOrEmpty(error))
                            {
                                error = pp != null ? pp.GetName() : "";
                                if (string.IsNullOrEmpty(error))
                                {
                                    error = pp != null ? pp.ToString() : "";
                                }
                            }
                            return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotSaved";
                        }
                    }


                    decimal _paymentAmt = 0;
                    if (paymentrecord.GetC_Currency_ID() != _formData[0]._cmbCurrency)
                    {
                        //Conversion according to the Statement Date and BankCurrency
                        //Conversion will be based on ConversionRate selected on form to consider.
                        _paymentAmt = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(paymentrecord.GetPayAmt()), paymentrecord.GetC_Currency_ID(), _formData[0]._cmbCurrency, _formData[0]._dtStatementDate, _formData[0]._txtConversionType, ctx.GetAD_Client_ID(), _formData[0]._bankAcctOrg_ID);
                    }
                    else
                    {
                        _paymentAmt = Util.GetValueOfDecimal(paymentrecord.GetPayAmt());
                    }
                    ////


                    documentType = new MDocType(ctx, paymentrecord.GetC_DocType_ID(), null);
                    if (documentType.GetDocBaseType() == "ARR")
                    {

                        _bankStatementLine.SetTrxAmt(_paymentAmt);

                        /////
                        if (_bankStatementLine.GetTrxAmt() > 0)
                        {
                            differenceAmount = Math.Abs(_formData[0]._txtTrxAmt) - _bankStatementLine.GetTrxAmt();
                        }
                        else
                        {
                            differenceAmount = (Math.Abs(_formData[0]._txtTrxAmt) * -1) - _bankStatementLine.GetTrxAmt();
                        }
                        /////

                        if (_formData[0]._cmbVoucherMatch == "M" && _formData[0]._cmbDifferenceType == "CH" && differenceAmount != 0)
                        {
                            if (_bankStatementLine.GetStmtAmt() == 0)
                            {
                                _bankStatementLine.SetStmtAmt(_paymentAmt + differenceAmount);
                            }
                        }
                        else
                        {
                            if (_bankStatementLine.GetStmtAmt() == 0)
                            {
                                _bankStatementLine.SetStmtAmt(_paymentAmt);
                            }
                        }
                    }
                    else if (documentType.GetDocBaseType() == "APP")
                    {
                        if (_paymentAmt < 0)
                        {


                            //_bankStatementLine.SetStmtAmt(Decimal.Negate(Util.GetValueOfDecimal(paymentrecord.GetPayAmt())));
                            _bankStatementLine.SetTrxAmt(Decimal.Negate(_paymentAmt));
                            /////
                            if (_bankStatementLine.GetTrxAmt() > 0)
                            {
                                differenceAmount = Math.Abs(_formData[0]._txtTrxAmt) - _bankStatementLine.GetTrxAmt();
                            }
                            else
                            {
                                differenceAmount = (Math.Abs(_formData[0]._txtTrxAmt) * -1) - _bankStatementLine.GetTrxAmt();
                            }
                            /////
                            if (_formData[0]._cmbVoucherMatch == "M" && _formData[0]._cmbDifferenceType == "CH" && differenceAmount != 0)
                            {
                                if (_bankStatementLine.GetStmtAmt() == 0)
                                {
                                    _bankStatementLine.SetStmtAmt(Decimal.Negate(_paymentAmt) + differenceAmount);
                                }
                            }
                            else
                            {
                                if (_bankStatementLine.GetStmtAmt() == 0)
                                {
                                    _bankStatementLine.SetStmtAmt(Decimal.Negate(_paymentAmt));
                                }
                            }

                        }
                        else
                        {

                            // _bankStatementLine.SetStmtAmt(Decimal.Negate(Util.GetValueOfDecimal(paymentrecord.GetPayAmt())));
                            _bankStatementLine.SetTrxAmt(Decimal.Negate(_paymentAmt));
                            /////
                            if (_bankStatementLine.GetTrxAmt() > 0)
                            {
                                differenceAmount = Math.Abs(_formData[0]._txtTrxAmt) - _bankStatementLine.GetTrxAmt();
                            }
                            else
                            {
                                differenceAmount = (Math.Abs(_formData[0]._txtTrxAmt) * -1) - _bankStatementLine.GetTrxAmt();
                            }
                            /////
                            if (_formData[0]._cmbVoucherMatch == "M" && _formData[0]._cmbDifferenceType == "CH" && differenceAmount != 0)
                            {
                                if (_bankStatementLine.GetStmtAmt() == 0)
                                {
                                    _bankStatementLine.SetStmtAmt(Decimal.Negate(_paymentAmt) + differenceAmount);
                                }
                            }
                            else
                            {
                                if (_bankStatementLine.GetStmtAmt() == 0)
                                {
                                    _bankStatementLine.SetStmtAmt(Decimal.Negate(_paymentAmt));
                                }
                            }
                        }
                    }

                    else
                    {
                        //_bankStatementLine.SetStmtAmt(Util.GetValueOfDecimal(_formData[0]._txtAmount));
                        _bankStatementLine.SetTrxAmt(Util.GetValueOfDecimal(_formData[0]._txtAmount));
                        /////
                        if (_bankStatementLine.GetTrxAmt() > 0)
                        {
                            differenceAmount = Math.Abs(_formData[0]._txtTrxAmt) - _bankStatementLine.GetTrxAmt();
                        }
                        else
                        {
                            differenceAmount = (Math.Abs(_formData[0]._txtTrxAmt) * -1) - _bankStatementLine.GetTrxAmt();
                        }
                        /////
                        if (_formData[0]._cmbVoucherMatch == "M" && _formData[0]._cmbDifferenceType == "CH" && differenceAmount != 0)
                        {
                            if (_bankStatementLine.GetStmtAmt() == 0)
                            {
                                _bankStatementLine.SetStmtAmt(Util.GetValueOfDecimal(_formData[0]._txtTrxAmt) + differenceAmount);
                            }
                        }
                        else
                        {
                            if (_bankStatementLine.GetStmtAmt() == 0)
                            {
                                _bankStatementLine.SetStmtAmt(Util.GetValueOfDecimal(_formData[0]._txtTrxAmt));
                            }
                        }
                    }
                }
                else
                {
                    _bankStatementLine.SetStmtAmt(Util.GetValueOfDecimal(_formData[0]._txtAmount));
                    //Used string.Equal() method
                    if (Util.GetValueOfString(_formData[0]._cmbVoucherMatch).Equals("C") && Util.GetValueOfString(_formData[0]._cmbContraType).Equals("CB"))
                    {
                        //Contra Amount as TrxAmt
                        _bankStatementLine.SetTrxAmt(_formData[0]._txtTrxAmt);
                        _bankStatementLine.SetC_CashLine_ID(Util.GetValueOfInt(_formData[0]._ctrlCashLine));

                    }
                    //Used string.Equal() method
                    if (Util.GetValueOfString(_formData[0]._cmbVoucherMatch).Equals("V") || (Util.GetValueOfString(_formData[0]._cmbVoucherMatch).Equals("C") && Util.GetValueOfString(_formData[0]._cmbContraType).Equals("BB")))
                    {
                        _bankStatementLine.SetTrxAmt(0);
                    }
                    else
                    {
                        //Other than Cash to bank Type set txtAmount as TrxAmt
                        if (!_formData[0]._cmbContraType.Equals("CB"))
                        {
                            _bankStatementLine.SetTrxAmt(Util.GetValueOfDecimal(_formData[0]._txtAmount));
                        }
                    }
                }
                //Charge Amount will Update on Line when VoucherMatch Type is not Voucher
                if (_formData[0]._cmbCharge > 0 && _formData[0]._cmbVoucherMatch != "V")
                {
                    _bankStatementLine.SetChargeAmt(_bankStatementLine.GetStmtAmt() - _bankStatementLine.GetTrxAmt());
                    _bankStatementLine.SetC_Charge_ID(_formData[0]._cmbCharge);
                    if (_formData[0]._cmbTaxRate > 0)
                    {
                        _bankStatementLine.SetC_Tax_ID(_formData[0]._cmbTaxRate);
                    }
                    if (_bankStatementLine.GetChargeAmt() > 0)
                    {
                        _bankStatementLine.SetTaxAmt(Math.Abs(_formData[0]._txtTaxAmount));
                        _bankStatementLine.Set_Value("SurchargeAmt", Math.Abs(_formData[0]._surChargeAmt));
                    }
                    else
                    {
                        _bankStatementLine.SetTaxAmt(Decimal.Negate(Math.Abs(_formData[0]._txtTaxAmount)));
                        _bankStatementLine.Set_Value("SurchargeAmt", Decimal.Negate(Math.Abs(_formData[0]._surChargeAmt)));
                    }
                }
                else
                {
                    _bankStatementLine.SetC_Charge_ID(0);
                }
                /*chnage by pratap*/

                //_bankStatementLine.SetVA012_IsUseNextTime(_formData[0]._chkUseNextTime);
                _bankStatementLine.SetVA012_VoucherType(_formData[0]._cmbVoucherMatch);
                if (_formData[0]._cmbContraType != "0" && _formData[0]._cmbContraType != "")
                {
                    _bankStatementLine.SetVA012_ContraType(_formData[0]._cmbContraType);
                }
                if (_formData[0]._cmbDifferenceType != "0" && _formData[0]._cmbDifferenceType != "")
                {
                    _bankStatementLine.SetVA012_DifferenceType(_formData[0]._cmbDifferenceType);
                }

                if (_formData[0]._cmbCashBook > 0)
                {
                    _bankStatementLine.SetC_CashBook_ID(_formData[0]._cmbCashBook);

                }

                else
                {
                    _bankStatementLine.SetC_CashBook_ID(0);
                }
                _bankStatementLine.SetEftCheckNo(_formData[0]._txtCheckNo);
                if (_formData[0]._ctrlBusinessPartner > 0)
                {
                    _bankStatementLine.SetC_BPartner_ID(_formData[0]._ctrlBusinessPartner);

                }
                else
                {
                    _bankStatementLine.SetC_BPartner_ID(0);
                }
                //If Invoice is have then set Invoice from Payment
                //_bankStatementLine.SetC_Invoice_ID(_formData[0]._ctrlInvoice);
                //when drag the record at that time _formData[0]._ctrlInvoice is null 
                //not need to used condtion not null of _formData[0]._ctrlInvoice
                if (paymentrecord != null)
                {
                    _bankStatementLine.SetC_Invoice_ID(paymentrecord.GetC_Invoice_ID());
                }
                else
                {
                    _bankStatementLine.SetC_Invoice_ID(0);
                }

                if (_formData[0]._ctrlOrder > 0)
                {
                    _bankStatementLine.SetC_Order_ID(_formData[0]._ctrlOrder);
                }
                else
                {
                    _bankStatementLine.SetC_Order_ID(0);
                }

                if (_formData[0]._ctrlPayment > 0)
                {
                    _bankStatementLine.SetC_Payment_ID(_formData[0]._ctrlPayment);
                }
                else if (paymentID > 0)
                {
                    _bankStatementLine.SetC_Payment_ID(paymentID);
                }
                else
                {
                    _bankStatementLine.SetC_Payment_ID(0);
                }

                if (_formData[0]._cmbCharge > 0 || paymentID > 0 || _formData[0]._ctrlPayment > 0 || _formData[0]._ctrlCashLine > 0)
                {
                    _bankStatementLine.SetVA012_IsMatchingConfirmed(true);
                }
                // TaxRate will update on Line when DifferenceType as charge on BankStatement form
                //if (_formData[0]._cmbTaxRate > 0)
                //{
                //    _bankStatementLine.SetC_Tax_ID(_formData[0]._cmbTaxRate);
                //}
                //else
                //{
                //    _bankStatementLine.SetC_Tax_ID(0);
                //}
                #endregion New Record
            }
            if (!_bankStatementLine.Save())
            {
                //Used transaction 
                trx.Rollback();
                //close the transaction
                trx.Close();
                //clear the object
                trx = null;
                //Used ValueNamePair class to get the Error
                ValueNamePair pp = VLogger.RetrieveError();
                string error = pp != null ? pp.GetValue() : "";
                if (string.IsNullOrEmpty(error))
                {
                    error = pp != null ? pp.GetName() : "";
                    if (string.IsNullOrEmpty(error))
                    {
                        error = pp != null ? pp.ToString() : "";
                    }
                }
                return !string.IsNullOrEmpty(error) ? error : "VA012_ErrorSavingBankStatementLine";
            }
            else
            {
                //Commmented code StatementDate shouldn't update according to Line AcctDate - requirement changed
                #region Update StatementDate on header
                //when the statement is existed then set the Statement Date as latest Date which is AcctDate in StatementLine
                //if (_existingStatementID > 0)
                //{
                //    DateTime? acctDate = Util.GetValueOfDateTime(DB.ExecuteScalar("SELECT MAX(DateAcct) FROM C_BankStatementLine WHERE IsActive='Y' AND C_BankStatement_ID=" + _existingStatementID, null, null));
                //    if (acctDate != null)
                //    {
                //        if (_formData[0]._dtStatementDate > acctDate)
                //        {
                //            _bankStatement.SetStatementDate(_formData[0]._dtStatementDate);
                //        }
                //        else
                //        {
                //            _bankStatement.SetStatementDate(acctDate);
                //        }

                //        if (!_bankStatement.Save())
                //        {
                //            //Used transaction 
                //            trx.Rollback();
                //            //close the transaction
                //            trx.Close();
                //            //clear the object
                //            trx = null;
                //            ValueNamePair pp = VLogger.RetrieveError();
                //            string error = pp != null ? pp.GetValue() : "";
                //            if (string.IsNullOrEmpty(error))
                //            {
                //                error = pp != null ? pp.GetName() : "";
                //            }
                //            return !string.IsNullOrEmpty(error) ? error : "VA012_ErrorSavingBankStatement";
                //        }
                //    }
                //}
                #endregion

                if (_formData[0]._chkUseNextTime && _formData[0]._bankStatementLineID <= 0)
                {
                    //if Invoice variable hold one Id then it returns Value otherwise set as Zero
                    if (!int.TryParse(_formData[0]._ctrlInvoice, out paymentID))
                        paymentID = 0;

                    string _sql = @"INSERT
                                INTO VA012_TEMPSTATEMENT
                                  (
                                    DESCRIPTION ,
                                    C_INVOICE_ID ,
                                    TAXAMT ,
                                    AMOUNT ,
                                    C_BPARTNER_ID ,
                                    C_PAYMENT_ID ,
                                    C_CURRENCY_ID ,
                                    AD_ORG_ID ,
                                    VA012_PAGE ,
                                    LINE ,
                                    AD_CLIENT_ID ,
                                    C_CHARGE_ID ,
                                    VA012_STATEMENTNO ,
                                    C_TAX_ID ,
                                    STATEMENTDATE ,
                                    C_BANKSTATEMENTLINE_ID
                                  )
                                  VALUES
                                  ( '"
                               + _formData[0]._txtDescription + "'," +
                                //_formData[0]._ctrlInvoice + "," +
                                paymentID + "," +
                                _formData[0]._txtTaxAmount + "," +
                                _formData[0]._txtAmount + "," +
                                _formData[0]._ctrlBusinessPartner + "," +
                                _formData[0]._ctrlPayment + "," +
                                _formData[0]._cmbCurrency + "," +
                                ctx.GetAD_Org_ID() + "," +
                                 _formData[0]._txtStatementPage + "," +
                                 _formData[0]._txtStatementLine + "," +
                                ctx.GetAD_Client_ID() + "," +
                                _formData[0]._cmbCharge + ",'" +
                                 _formData[0]._txtStatementNo + "'," +
                                 _formData[0]._cmbTaxRate + "," +
                                 GlobalVariable.TO_DATE(_formData[0]._dtStatementDate, true) + "," +
                                _bankStatementLine.GetC_BankStatementLine_ID() +
                              " )";
                    int _count = DB.ExecuteQuery(_sql);
                }
            }
            //closed the transaction after commit
            trx.Commit();
            //close the transaction
            trx.Close();
            //clear the object
            trx = null;
            return "Success";
        }

        /// <summary>
        /// Complete the Payment Record when cal this function to complete the record
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="Record_ID">C_Payment_ID</param>
        /// <param name="Process_ID">AD_Process_ID</param>
        /// <param name="DocAction">Documnet Action</param>
        /// <returns>return message</returns>
        public string CompletePayment(Ctx ctx, int Record_ID, int Process_ID, string DocAction)
        {
            string result = "";
            MRole role = MRole.Get(ctx, ctx.GetAD_Role_ID());
            if (Util.GetValueOfBool(role.GetProcessAccess(Process_ID)))
            {
                DB.ExecuteQuery("UPDATE C_Payment SET DocAction = '" + DocAction + "' WHERE C_Payment_ID = " + Record_ID);

                MProcess proc = new MProcess(ctx, Process_ID, null);
                MPInstance pin = new MPInstance(proc, Record_ID);
                if (!pin.Save())
                {
                    ValueNamePair vnp = VLogger.RetrieveError();
                    string errorMsg = "";
                    if (vnp != null)
                    {
                        errorMsg = vnp.GetName();
                        if (errorMsg == "")
                            errorMsg = vnp.GetValue();
                    }
                    if (errorMsg == "")
                        result = "DocNotCompleted";

                    return result;
                }

                MPInstancePara para = new MPInstancePara(pin, 20);
                para.setParameter("DocAction", DocAction);
                if (!para.Save())
                {
                    //String msg = "No DocAction Parameter added"; // not translated
                }
                VAdvantage.ProcessEngine.ProcessInfo pi = new VAdvantage.ProcessEngine.ProcessInfo("WF", Process_ID);
                pi.SetAD_User_ID(ctx.GetAD_User_ID());
                pi.SetAD_Client_ID(ctx.GetAD_Client_ID());
                pi.SetAD_PInstance_ID(pin.GetAD_PInstance_ID());
                pi.SetRecord_ID(Record_ID);
                pi.SetTable_ID(335); //AD_Table_ID=335 for C_Payment

                ProcessCtl worker = new ProcessCtl(ctx, null, pi, null);
                worker.Run();

                if (pi.IsError())
                {
                    ValueNamePair vnp = VLogger.RetrieveError();
                    string errorMsg = "";
                    if (vnp != null)
                    {
                        errorMsg = vnp.GetName();
                        if (errorMsg == "")
                            errorMsg = vnp.GetValue();
                    }

                    if (errorMsg == "")
                        errorMsg = pi.GetSummary();

                    if (errorMsg == "")
                        errorMsg = "DocNotCompleted";
                    result = errorMsg;
                    return result;
                }
                else
                    result = "";
            }
            else
            {
                result = "NoAccess";
                return result;
            }
            return result;
        }

        /// <summary>
        /// Get List 
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="currency">C_Currency_ID</param>
        /// <param name="conversionType">C_ConverstionType_ID</param>
        /// <param name="stmtDate">statement date</param>
        /// <param name="schedules">C_InvoicePaySchedule_ID's</param>
        /// <param name="_accountId">C_BankAccount_ID</param>
        /// <param name="orderId">C_Order_ID</param>
        /// <param name="paymentId">C_Payment_ID</param>
        /// <param name="cashLineId">C_CashLine_ID</param>
        /// <returns>List</returns>
        public Dictionary<String, Object> GetConvertedAmount(Ctx ctx, int currency, int conversionType, DateTime? stmtDate, string schedules, int _accountId, int orderId, int paymentId, int cashLineId)
        {
            decimal _convertedAmt = 0;
            DataSet _ds = null;
            string[] ids = null;
            string _query = null;
            Dictionary<String, Object> _list = new Dictionary<String, Object>();
            //corrected Query to fetch the Organization ID
            int _org_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Org_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + _accountId, null, null));

            //string _query = "SELECT pay.C_InvoicePaySchedule_ID, pay.DueAmt, inv.C_Currency_ID  FROM C_Invoice inv INNER JOIN C_InvoicePaySchedule pay ON inv.C_Invoice_ID=pay.C_Invoice_ID WHERE" +
            //    " pay.IsActive = 'Y' AND C_InvoicePaySchedule_ID IN (" + schedules + ")";
            if (!string.IsNullOrEmpty(schedules))
            {
                //Corrected Query passed Variable inplace of Hardcode value
                _query = @"SELECT CASE
                                    WHEN(inv.C_CURRENCY_ID != BCURR.C_CURRENCY_ID)
                                    THEN
                                    CASE
                                     WHEN(DT.DOCBASETYPE = 'ARI' OR DT.DOCBASETYPE = 'APC')
                                     THEN CURRENCYCONVERT(PAY.DueAmt, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(stmtDate, true) + "," + conversionType + @", INV.AD_Client_ID, " + _org_ID + @")
                                     ELSE CURRENCYCONVERT(PAY.DueAmt*-1, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(stmtDate, true) + @"
                                            ," + conversionType + @", INV.AD_Client_ID, " + _org_ID + @") END
                                    ELSE
                                    CASE
                                    WHEN(DT.DOCBASETYPE = 'ARI' OR DT.DOCBASETYPE = 'APC')
                                    THEN ROUND(PAY.DUEAMT, NVL(BCURR.StdPrecision,2))
                                    ELSE ROUND(PAY.DUEAMT*-1,NVL(BCURR.StdPrecision, 2))
                                    END
                                    END AS DueAmt,
                                    PAY.C_INVOICE_ID,PAY.C_INVOICEPAYSCHEDULE_ID, INV.DocumentNo
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID = INV.C_INVOICE_ID
                            INNER JOIN C_Doctype dt
                            ON dt.C_Doctype_id = INV.c_doctype_id
                            LEFT JOIN C_CURRENCY BCURR
                            ON " + currency + @" = BCURR.C_CURRENCY_ID
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + schedules + ")";
            }
            else if (orderId != 0)
            {

                _query = @"SELECT 
                    CASE 
                        WHEN(ORD.C_CURRENCY_ID!=" + currency + @")
                        THEN CURRENCYCONVERT(ORD.GrandTotal, ORD.C_CURRENCY_ID," + currency + ", " + GlobalVariable.TO_DATE(stmtDate, true) + "," + conversionType + "," + ctx.GetAD_Client_ID() + ", " + _org_ID + @") 
                        ELSE ORD.GrandTotal END DueAmt, ORD.DocumentNo
                        FROM C_ORDER ORD
                        INNER JOIN C_OrderLine ol ON ORD.C_ORDER_ID=ol.C_ORDER_ID
                        INNER JOIN C_DOCTYPE DT ON ORD.C_DocTypeTarget_ID=dt.C_DocType_ID WHERE ORD.IsActive='Y' AND ORD.C_ORDER_ID=" + orderId;
            }
            //added for to Get the Converted PaymentAmount
            else if (paymentId != 0)
            {
                _query = @"SELECT
                        CASE
                        WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                        THEN
                            CASE
                                WHEN (DT.DOCBASETYPE='ARR')
                                THEN CURRENCYCONVERT(PAY.PAYAMT, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(stmtDate, true) + "," + conversionType + @", PAY.AD_Client_ID, PAY.AD_Org_ID) 
                                WHEN (DT.DOCBASETYPE='APP')
                                THEN CURRENCYCONVERT(PAY.PAYAMT * -1, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(stmtDate, true) + "," + conversionType + @", PAY.AD_Client_ID, PAY.AD_Org_ID) 
                            END
                        ELSE
                            CASE
                            WHEN (DT.DOCBASETYPE='ARR')
                            THEN PAY.PAYAMT
                            WHEN (DT.DOCBASETYPE='APP')
                            THEN PAY.PAYAMT*-1
                            END
                        END AS DueAmt, PAY.DocumentNo 
                    FROM C_PAYMENT PAY
                    INNER JOIN C_DOCTYPE DT
                    ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                    INNER JOIN C_BANKACCOUNT AC
                    ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID

                    LEFT JOIN C_BANKSTATEMENTLINE BSL 
                    ON PAY.C_PAYMENT_ID =BSL.C_PAYMENT_ID 

                    LEFT JOIN C_CURRENCY BCURR
                    ON " + currency + @" =BCURR.C_CURRENCY_ID
                   WHERE PAY.C_PAYMENT_ID=" + paymentId;
            }
            //added for to Get the Converted Cash Jounrnal Line Amount
            else if (cashLineId != 0)
            {
                _query = @"SELECT  CASE
                                       WHEN(csl.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                            THEN CURRENCYCONVERT(csl.amount, csl.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(stmtDate, true) +
                                                 ", " + conversionType + @", cs.AD_Client_ID, cs.AD_Org_ID) 
                                            ELSE ROUND(csl.amount,NVL(BCURR.StdPrecision,2))
                                 END AS DueAmt, csl.Line AS DocumentNo
                           FROM C_Cashline csl
                           inner join C_Cash cs on cs.C_Cash_id=csl.C_Cash_id
                           LEFT JOIN C_CURRENCY BCURR
                           ON " + currency + @" =BCURR.C_CURRENCY_ID
                           WHERE csl.C_cashline_ID=" + cashLineId;

            }

            _ds = DB.ExecuteDataset(_query, null, null);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                decimal amount = 0;
                ids = new string[_ds.Tables[0].Rows.Count];
                for (int  i=0; _ds.Tables[0].Rows.Count>i; i++) {
                    //amount = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["DueAmt"]), Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"]), currency, stmtDate, conversionType, ctx.GetAD_Client_ID(), _org_ID);
                    //Incase of cashLine It should change the sign
                    if (cashLineId != 0)
                    {
                        amount = Decimal.Negate(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["DueAmt"]));
                    }
                    else
                    {
                        amount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["DueAmt"]);
                    }
                    if (amount != 0)
                    {
                        _convertedAmt += amount;
                    }
                    else {

                        ids[i] = Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocumentNo"]);
                    }
                }
                
            }
            _list["_convertedAmt"] = _convertedAmt;
            _list["schedule_Ids"] = ids;
            return _list;
        }

        /// <summary>
        /// Get Currency & Conversion Type while checking 
        /// Invoice Transaction Conditions
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="schedules">C_InvoicePaySchedule's</param>
        /// <returns>List</returns>
        public Dictionary<String, Object> GetCurrencyandConversionType(Ctx ctx, string schedules)
        {
            int _conversionType_Id = 0;
            int _currency_Id = 0;
            Dictionary<String, Object> _list = new Dictionary<String, Object>();

            DataSet _ds = DB.ExecuteDataset(@"SELECT inv.C_ConversionType_ID,inv.C_Currency_ID FROM C_Invoice inv 
                                    INNER JOIN C_InvoicePaySchedule sch ON inv.C_Invoice_ID=sch.C_Invoice_ID WHERE C_InvoicePaySchedule_ID IN (" + schedules + ") Order by inv.DateAcct DESC", null, null);

            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                _currency_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]);
                _conversionType_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                for (int i = 0; _ds.Tables[0].Rows.Count > i; i++) {
                    if (_currency_Id!= Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"])) {
                        _currency_Id = 0;
                        _conversionType_Id = 0;
                        break;
                    }
                }
            }
            _list["currencyId"] = _currency_Id;
            _list["conversionTypeId"] = _conversionType_Id;

            return _list;
        }

        /// <summary>
        /// Calculate Surcharge Tax and Tax
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="tax_ID">C_Tax_ID</param>
        /// <param name="chargeAmt">Charge Amount</param>
        /// <returns>List of Values</returns>
        public Dictionary<String, Object> CalculateSurcharge(Ctx ctx, int tax_ID, decimal chargeAmt, int _stdPrecision)
        {
            Dictionary<String, Object> retval = new Dictionary<String, Object>();

            Boolean IsTaxIncluded = true;

            //End Assign parameter value
            MTax tax = new MTax(ctx, tax_ID, null);
            Decimal surchargeAmt = Env.ZERO;
            Decimal TaxAmt = Env.ZERO;
            if (tax.Get_ColumnIndex("Surcharge_Tax_ID") > 0 && tax.GetSurcharge_Tax_ID() > 0)
            {
                TaxAmt = tax.CalculateSurcharge(chargeAmt, IsTaxIncluded, _stdPrecision, out surchargeAmt);
            }
            else 
            {
                TaxAmt = tax.CalculateTax(chargeAmt, IsTaxIncluded, _stdPrecision);
            }
            retval["TaxAmt"] = TaxAmt;
            retval["SurchargeAmt"] = surchargeAmt;
            return retval;
        }

        /// <summary>
        /// Get the Converted amount based on Conversion Rate
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="recordID"></param>
        /// <param name="bnkAct_Id">C_BankAccount_ID</param>
        /// <param name="transcType">Transaction Type</param>
        /// <param name="stmtDate">Statement Date</param>
        /// <returns>List</returns>
        public List<InvoicePaySchedule> GetConvtAmount(Ctx ctx, string recordID, int bnkAct_Id, string transcType, DateTime? stmtDate)
        {
            List<InvoicePaySchedule> payList = new List<InvoicePaySchedule>();
            InvoicePaySchedule list = null;
            string _sql = null;
            DataSet _ds = null;
            int bnkCurrency_ID = 0;
            int bnkOrg_ID = 0;
            //get Account Currency ID
            _ds = DB.ExecuteDataset("SELECT C_CURRENCY_ID, AD_Org_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + bnkAct_Id);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                bnkCurrency_ID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]);
                bnkOrg_ID = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["AD_Org_ID"]);
            }

            string recordIds = string.Join(",", recordID.Split(','));

            if (transcType.Equals("IS"))
            {
                _sql = @"SELECT INV.C_Currency_ID,INV.C_ConversionType_ID,PAY.DueAmt,DT.DocBaseType FROM C_INVOICEPAYSCHEDULE PAY
                        INNER JOIN C_Invoice INV ON PAY.C_Invoice_ID=INV.C_Invoice_ID 
		                INNER JOIN C_DOCTYPE DT ON DT.C_DOCTYPE_ID=INV.C_DOCTYPE_ID
			            WHERE PAY.IsActive='Y' AND  PAY.C_INVOICEPAYSCHEDULE_ID IN (" + recordIds + ")";

                _ds = DB.ExecuteDataset(_sql, null, null);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {

                    for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                    {
                        list = new InvoicePaySchedule();
                        //Conversion is based On Bank Org not Context Organziation
                        if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_APINVOICE) || Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_ARCREDITMEMO))
                        {
                            list.DueAmount = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["DueAmt"]), Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"]), bnkCurrency_ID, stmtDate, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ConversionType_ID"]), ctx.GetAD_Client_ID(), bnkOrg_ID) * -1;
                        }
                        else if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_ARINVOICE) || Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_APCREDITMEMO))
                        {
                            list.DueAmount = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["DueAmt"]), Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"]), bnkCurrency_ID, stmtDate, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ConversionType_ID"]), ctx.GetAD_Client_ID(), bnkOrg_ID);
                        }
                        payList.Add(list);
                    }
                }
            }
            else if (transcType.Equals("PY"))
            {
                _sql = @"SELECT PAY.C_Currency_ID,PAY.C_ConversionType_ID,PAY.PaymentAmount,DT.DocBaseType FROM C_Payment PAY 
		                INNER JOIN C_DOCTYPE DT ON DT.C_DOCTYPE_ID=PAY.C_DOCTYPE_ID
			            WHERE PAY.IsActive='Y' AND PAY.C_Payment_ID IN (" + recordIds + ")";

                _ds = DB.ExecuteDataset(_sql, null, null);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {

                    for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                    {
                        list = new InvoicePaySchedule();
                        if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_APPAYMENT))
                        {
                            list.DueAmount = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["PaymentAmount"]), Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"]), bnkCurrency_ID, stmtDate, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ConversionType_ID"]), ctx.GetAD_Client_ID(), bnkOrg_ID) * -1;
                        }
                        else if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocBaseType"]).Equals(MDocBaseType.DOCBASETYPE_ARRECEIPT))
                        {
                            list.DueAmount = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["PaymentAmount"]), Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"]), bnkCurrency_ID, stmtDate, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ConversionType_ID"]), ctx.GetAD_Client_ID(), bnkOrg_ID);
                        }
                        payList.Add(list);
                    }
                }
            }
            //Converted Amount Incase of Cash Journal Line
            else if (transcType.Equals("CO"))
            {
                _sql = @"SELECT C_CashLine_ID, CASE
                            WHEN(csl.C_Currency_ID!=bcurr.C_Currency_ID)
                            THEN CURRENCYCONVERT(csl.Amount * -1, csl.C_Currency_ID, bcurr.C_Currency_ID, " + GlobalVariable.TO_DATE(stmtDate, true) + @", csl.C_ConversionType_ID, cs.AD_Client_ID, " + bnkOrg_ID + @") 
                            ELSE ROUND(csl.Amount * -1 ,NVL(bcurr.StdPrecision,2))
                            END AS Amount FROM C_CashLine csl
                            INNER JOIN C_Cash cs ON csl.C_Cash_ID = cs.C_Cash_ID 
                            LEFT JOIN C_Currency bcurr ON " + bnkCurrency_ID + @" = bcurr.C_Currency_ID WHERE csl.C_CashLine_ID IN  (" + recordIds + ")";

                _ds = DB.ExecuteDataset(_sql, null, null);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                    {
                        list = new InvoicePaySchedule();
                        list.DueAmount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["Amount"]);
                        payList.Add(list);
                    }
                }
            }
            //Converted Amount Incase of Prepay Order
            else if (transcType.Equals("PO"))
            {
                _sql = @"SELECT 
                    CASE 
                        WHEN(ORD.C_CURRENCY_ID!=" + bnkCurrency_ID + @")
                        THEN CURRENCYCONVERT(ORD.GrandTotal, ORD.C_CURRENCY_ID," + bnkCurrency_ID+", " + GlobalVariable.TO_DATE(stmtDate, true) + @", ORD.C_ConversionType_ID," + ctx.GetAD_Client_ID() + ", " + bnkOrg_ID + @") 
                        ELSE ORD.GrandTotal END GrandTotal
                        FROM C_ORDER ORD
                        INNER JOIN C_OrderLine ol ON ORD.C_ORDER_ID=ol.C_ORDER_ID
                        INNER JOIN C_DOCTYPE DT ON ORD.C_DocTypeTarget_ID=dt.C_DocType_ID WHERE ORD.IsActive='Y' AND ORD.C_ORDER_ID IN (" + recordIds + ")";

                _ds = DB.ExecuteDataset(_sql, null, null);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                    {
                        list = new InvoicePaySchedule();
                        list.DueAmount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["GrandTotal"]);
                        //MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["GrandTotal"]), Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_Currency_ID"]), bnkCurrency_ID, stmtDate, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_ConversionType_ID"]), ctx.GetAD_Client_ID(), bnkOrg_ID);
                        payList.Add(list);
                    }
                }
            }

            return payList;
        }

        /// <summary>
        /// Get Bank statement date
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="bankAcct">C_BankAccount_ID</param>
        /// <returns>Bank Statement Date</returns>
        public string GetStatementDate(Ctx ctx, int bankAcct)
        {
            string bnkStmtDate = Util.GetValueOfString(DB.ExecuteScalar("SELECT StatementDate FROM C_BankStatement WHERE IsActive='Y' AND DocStatus='DR' AND C_BankAccount_ID=" + bankAcct, null, null));
            if (!string.IsNullOrEmpty(bnkStmtDate))
                return bnkStmtDate;
            return "";
        }

        /// <summary>
        /// Get Conciled or Unconciled Statements
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="cmbBankAccount">C_BankAccout_ID</param>
        /// <param name="txtSearch">Search Text</param>
        /// <param name="currencyID">C_Currency_ID</param>
        /// <param name="_searchRequest">Search Request</param>
        /// <returns></returns>
        public List<ConcileStatement> LoadConciledOrUnConciledStatements(Ctx ctx, int cmbBankAccount, string txtSearch, int currencyID, bool _searchRequest)
        {
            string _sqlCon = @"SELECT NVL(ROUND(SUM( 
             CASE 
             WHEN (BSL.C_PAYMENT_ID IS NOT NULL OR BSL.C_CHARGE_ID IS NOT NULL OR BSL.C_CASHLINE_ID IS NOT NULL) 
             THEN ( 
             CASE 
             WHEN ( BSL.C_CURRENCY_ID! =BCURR.C_CURRENCY_ID) 
             THEN BSL.StmtAmt*( 
              CASE 
             WHEN CCR.MULTIPLYRATE IS NOT NULL 
             THEN CCR.MULTIPLYRATE 
              ELSE CCR1.DIVIDERATE 
              END) 
             ELSE BSL.StmtAmt 
             END) 
             END),NVL(CURR.StdPrecision,2)),0) AS RECONCILED, 
             NVL(ROUND(SUM( 
             CASE 
             WHEN (BSL.C_PAYMENT_ID IS NULL AND BSL.C_CHARGE_ID IS NULL AND  BSL.C_CASHLINE_ID IS NULL) 
             THEN ( 
             CASE 
             WHEN ( BSL.C_CURRENCY_ID! = BCURR.C_CURRENCY_ID) 
             THEN BSL.StmtAmt*( 
             CASE  
              WHEN CCR.MULTIPLYRATE IS NOT NULL 
             THEN CCR.MULTIPLYRATE 
              ELSE CCR1.DIVIDERATE 
              END) 
             ELSE BSL.StmtAmt 
             END) 
             END),NVL(CURR.StdPrecision,2)),0) AS UNRECONCILED,BCURR.ISO_CODE AS BASECURRENCY 
              FROM C_BANKSTATEMENT BS
               INNER JOIN C_BANKSTATEMENTLINE BSL
              ON BS.C_BANKSTATEMENT_ID=BSL.C_BANKSTATEMENT_ID
              LEFT JOIN C_BPARTNER BP
              ON BSL.C_BPARTNER_ID     =BP.C_BPARTNER_ID
              LEFT JOIN C_CURRENCY CURR 
              ON BSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID 
             
             INNER JOIN AD_CLIENTINFO CINFO  
             ON CINFO.AD_CLIENT_ID =BSL.AD_CLIENT_ID 
             INNER JOIN C_ACCTSCHEMA AC 
             ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID
             LEFT JOIN C_CURRENCY BCURR
             ON " + currencyID + @" =BCURR.C_CURRENCY_ID
             LEFT JOIN C_CONVERSION_RATE CCR 
             ON (CCR.C_CURRENCY_ID   =BSL.C_CURRENCY_ID 
             AND CCR.ISACTIVE        ='Y' 
             AND CCR.C_CURRENCY_TO_ID=" + currencyID + @"AND CCR.AD_CLIENT_ID    =BSL.AD_CLIENT_ID
             AND CCR.AD_ORG_ID      IN (BSL.AD_ORG_ID,0)
             AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)
             
             LEFT JOIN C_CONVERSION_RATE CCR1
             ON (CCR1.C_CURRENCY_ID   =" + currencyID + @" AND CCR1.C_CURRENCY_TO_ID=BSL.C_CURRENCY_ID
             AND CCR1.ISACTIVE        ='Y'
             AND CCR1.AD_CLIENT_ID    =BSL.AD_CLIENT_ID 
             AND CCR1.AD_ORG_ID      IN (BSL.AD_ORG_ID,0) 
             AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO) 
             WHERE BS.ISACTIVE='Y' AND BS.C_BANKACCOUNT_ID= " + cmbBankAccount + " AND BS.DOCSTATUS !='VO' AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();

            if (cmbBankAccount > 0)
            {
                _sqlCon += " AND BS.AD_ORG_ID = (SELECT AD_Org_ID FROM C_BankAccount WHERE IsActive = 'Y' AND C_BankAccount_ID = " + cmbBankAccount + ")";
            }

            if (_searchRequest)
            {

                _sqlCon += " AND (UPPER(BP.NAME) LIKE UPPER('%" + txtSearch + "%')"
                    + " OR UPPER(BSL.DESCRIPTION) LIKE UPPER('%" + txtSearch + "%')"
                    + " OR UPPER(BS.NAME) LIKE UPPER('%" + txtSearch + "%')"
                    + " OR UPPER(BSL.StmtAmt) LIKE UPPER('%" + txtSearch + "%')"
                    + " OR UPPER(BSL.TRXNO) LIKE UPPER('%" + txtSearch + "%')"
                    + " OR UPPER(BSL.TrxAmt) LIKE UPPER('%" + txtSearch + "%'))";
            }
            _sqlCon += " GROUP BY BCURR.ISO_CODE ,NVL(CURR.StdPrecision,2)";

            DataSet _ds = DB.ExecuteDataset(_sqlCon, null, null);
            List<ConcileStatement> _statements = new List<ConcileStatement>();
            ConcileStatement conOrunconcile = new ConcileStatement();

            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    conOrunconcile.basecurrency = Util.GetValueOfString(_ds.Tables[0].Rows[i]["BASECURRENCY"]);
                    conOrunconcile.reconciled = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["RECONCILED"]);
                    conOrunconcile.unreconciled = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["UNRECONCILED"]);
                    _statements.Add(conOrunconcile);
                }
            }
            return _statements;

        }

        /// <summary>
        /// Get Statement Line Data
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_bankStatementLineID">C_BankStatementLine_ID</param>
        /// <param name="_trxType">Tranaction Type</param>
        /// <param name="payment_ID">C_Payment_ID or C_InvoiceSchedule_ID or C_Order_ID</param>
        /// <returns>List</returns>

        public StatementProp GetStatementLine(Ctx ctx, int _bankStatementLineID, string _trxType, int payment_ID)
        {

            StatementProp statementDetail = new StatementProp();
            MBankStatementLine _bankStatementLine = new MBankStatementLine(ctx, _bankStatementLineID, null);

            MCharge chrg = null;
            statementDetail._txtStatementLine = _bankStatementLine.GetLine();
            statementDetail._txtAmount = _bankStatementLine.GetStmtAmt();
            //statement Date should be Line StatementDate
            statementDetail._dtStatementDate = _bankStatementLine.GetStatementLineDate();

            if (_bankStatementLine.GetC_Charge_ID() > 0)
            {
                chrg = new MCharge(ctx, _bankStatementLine.GetC_Charge_ID(), null);
                statementDetail._txtTrxAmt = _bankStatementLine.GetTrxAmt();

                if (statementDetail._txtAmount != statementDetail._txtTrxAmt)
                {
                    statementDetail._cmbDifferenceType = "CH";
                }

                //if (_bankStatementLine.GetC_Payment_ID() > 0)
                //{
                //    statementDetail._txtDifference = _bankStatementLine.GetChargeAmt();
                //}
                //statementDetail._cmbDifferenceType = _bankStatementLine.GetVA012_DifferenceType();
            }
            else if (_bankStatementLine.GetC_Payment_ID() > 0 || (payment_ID != 0 && _trxType.Equals("PY")))
            {
                MPayment _pay = new MPayment(ctx, _bankStatementLine.GetC_Payment_ID() != 0 ? _bankStatementLine.GetC_Payment_ID() : payment_ID, null);
                //decimal _trxamt = _pay.GetPayAmt() + _pay.GetOverUnderAmt() + _pay.GetDiscountAmt() + _pay.GetWriteOffAmt();
                //pratap
                // decimal _trxamt = _bankStatementLine.GetTrxAmt();
                decimal _trxamt = 0;
                string _baseType = Util.GetValueOfString(DB.ExecuteScalar("SELECT DOcBaseType FROM C_DocType WHERE IsActive='Y' AND C_DocType_ID=" + _pay.GetC_DocType_ID(), null, null));
                if (_pay.GetC_Currency_ID() != _bankStatementLine.GetC_Currency_ID())
                {
                    _trxamt = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_pay.GetPayAmt()), _pay.GetC_Currency_ID(), _bankStatementLine.GetC_Currency_ID(), statementDetail._dtStatementDate, _pay.GetC_ConversionType_ID(), ctx.GetAD_Client_ID(), _bankStatementLine.GetAD_Org_ID());
                    if (_baseType != null && _baseType.Equals(MDocBaseType.DOCBASETYPE_APPAYMENT))
                    {
                        _trxamt = Decimal.Negate(_trxamt);
                    }
                }
                else
                {
                    if (_baseType != null && _baseType.Equals(MDocBaseType.DOCBASETYPE_APPAYMENT))
                    {
                        _trxamt = Decimal.Negate(_pay.GetPayAmt());
                    }
                    else
                    {
                        _trxamt = _pay.GetPayAmt();
                    }
                }
                statementDetail._txtTrxAmt = _trxamt;
                //get Currency_ID and C_ConversionType_ID
                statementDetail._txtCurrency = _pay.GetC_Currency_ID();
                statementDetail._txtConversionType = _pay.GetC_ConversionType_ID();
                //if (_bankStatementLine.GetStmtAmt() > 0)
                //{
                //    statementDetail._txtTrxAmt = Math.Abs(_trxamt);
                //}
                //else
                //{
                //    statementDetail._txtTrxAmt = Math.Abs(_trxamt) * -1;
                //}
                // statementDetail._txtDifference = _pay.GetOverUnderAmt() + _pay.GetDiscountAmt() + _pay.GetWriteOffAmt();

                //commented by pratap 1/12/15
                //string _differenceType = "";
                //if (_pay.GetOverUnderAmt() != 0)
                //{
                //    statementDetail._txtDifference = _pay.GetOverUnderAmt();
                //    _differenceType = "OU";
                //}
                //else if (_pay.GetDiscountAmt() != 0)
                //{
                //    statementDetail._txtDifference = _pay.GetDiscountAmt();
                //    _differenceType = "DA";
                //}
                //else if (_pay.GetWriteOffAmt() != 0)
                //{
                //    statementDetail._txtDifference = _pay.GetWriteOffAmt();
                //    _differenceType = "WO";
                //}
                //statementDetail._cmbDifferenceType = _differenceType;
                //
                //#region GetPaymentSchedules

                //statementDetail._getSchedules = GetPaymentSchedules(ctx, _bankStatementLine.GetC_Payment_ID());

                //#endregion
            }
            else if (_bankStatementLine.GetC_CashLine_ID() == 0 && payment_ID == 0)
            {
                statementDetail._txtTrxAmt = _bankStatementLine.GetTrxAmt();
            }
            //Get TrxAmount when Drag Invoice Schedule on to the Line
            else if (payment_ID != 0 && _trxType.Equals("IS"))
            {
                DataSet data = DB.ExecuteDataset(@"SELECT CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                  CASE
                                    WHEN (dt.DOCBASETYPE IN ('ARI','APC'))
                                     THEN CURRENCYCONVERT(pay.DueAmt, inv.C_Currency_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statementDetail._dtStatementDate, true) + @", inv.C_ConversionType_ID, inv.AD_Client_ID, "+ _bankStatementLine.GetAD_Org_ID() + @")
                                    WHEN (dt.DOCBASETYPE IN ('API','ARC'))
                                     THEN CURRENCYCONVERT(pay.DueAmt*-1, inv.C_Currency_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statementDetail._dtStatementDate, true) + @", inv.C_ConversionType_ID, inv.AD_Client_ID, " + _bankStatementLine.GetAD_Org_ID() + @")
                                  END
                                ELSE
                                  CASE
                                    WHEN (dt.DOCBASETYPE IN ('ARI','APC'))
                                     THEN ROUND(pay.DUEAMT,NVL(BCURR.StdPrecision,2))
                                    WHEN (dt.DOCBASETYPE IN ('API','ARC'))
                                     THEN ROUND(pay.DUEAMT,NVL(BCURR.StdPrecision,2))*-1
                                  END
                                END AS DueAmt, inv.C_Currency_ID, inv.C_ConversionType_ID
                                FROM C_InvoicePaySchedule pay
                                INNER JOIN C_Invoice inv ON pay.C_Invoice_ID = inv.C_Invoice_ID
                                INNER JOIN C_DocType dt ON dt.C_DocType_ID = inv.C_DocType_ID
                                LEFT JOIN C_CURRENCY BCURR ON " + _bankStatementLine.GetC_Currency_ID() + @" =BCURR.C_CURRENCY_ID WHERE pay.IsActive='Y' AND pay.C_InvoicePaySchedule_ID =" + payment_ID, null, null);

                if (data != null && data.Tables[0].Rows.Count > 0)
                {
                    statementDetail._txtTrxAmt = Util.GetValueOfDecimal(data.Tables[0].Rows[0]["DueAmt"]);
                    //get Currency_ID and C_ConversionType_ID
                    statementDetail._txtCurrency = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_Currency_ID"]);
                    statementDetail._txtConversionType = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_ConversionType_ID"]);
                }
            }
            //Get TrxAmount when Drag Prepay Order on to the Line
            else if (payment_ID != 0 && _trxType.Equals("PO"))
            {

                DataSet data = DB.ExecuteDataset(@"SELECT CASE
                        WHEN(ord.C_Currency_ID != bcurr.C_Currency_ID)
                          THEN CURRENCYCONVERT(ord.GrandTotal, ord.C_Currency_ID, bcurr.C_Currency_ID, " + GlobalVariable.TO_DATE(statementDetail._dtStatementDate, true) + @", ord.C_ConversionType_ID, ord.AD_Client_ID, " + _bankStatementLine.GetAD_Org_ID() + @")
                        ELSE ROUND(ord.GrandTotal, NVL(bcurr.StdPrecision,2))
                        END AS GrandTotal, ord.C_Currency_ID, ord.C_ConversionType_ID 
                        FROM C_Order ord LEFT JOIN C_Currency bcurr ON " + _bankStatementLine.GetC_Currency_ID() + @" = bcurr.C_Currency_ID WHERE ord.IsActive='Y' AND ord.C_Order_ID =" + payment_ID, null, null);

                if (data != null && data.Tables[0].Rows.Count > 0)
                {
                    statementDetail._txtTrxAmt = Util.GetValueOfDecimal(data.Tables[0].Rows[0]["GrandTotal"]);
                    //get Currency_ID and C_ConversionType_ID
                    statementDetail._txtCurrency = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_Currency_ID"]);
                    statementDetail._txtConversionType = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_ConversionType_ID"]);
                }
            }

            //Get TrxAmount when Drag Prepay Order on to the Line
            else if (payment_ID != 0 && _trxType.Equals("CO"))
            {

                DataSet data = DB.ExecuteDataset(@"SELECT C_CashLine_ID, CASE
                            WHEN(csl.C_Currency_ID!=bcurr.C_Currency_ID)
                            THEN CURRENCYCONVERT(csl.Amount * -1, csl.C_Currency_ID, bcurr.C_Currency_ID, " + GlobalVariable.TO_DATE(statementDetail._dtStatementDate, true) + @", csl.C_ConversionType_ID, cs.AD_Client_ID, " + _bankStatementLine.GetAD_Org_ID() + @") 
                            ELSE ROUND(csl.Amount * -1 ,NVL(bcurr.StdPrecision,2))
                            END AS Amount, csl.C_Currency_ID, csl.C_ConversionType_ID FROM C_CashLine csl
                            INNER JOIN C_Cash cs ON csl.C_Cash_ID = cs.C_Cash_ID 
                            LEFT JOIN C_Currency bcurr ON " + _bankStatementLine.GetC_Currency_ID() + @" = bcurr.C_Currency_ID WHERE csl.IsActive='Y' AND csl.C_CashLine_ID =" + payment_ID, null, null);

                if (data != null && data.Tables[0].Rows.Count > 0)
                {
                    statementDetail._txtTrxAmt= Util.GetValueOfDecimal(data.Tables[0].Rows[0]["Amount"]);
                    statementDetail._ctrlCashLine = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_CashLine_ID"]);
                    //get Currency_ID
                    statementDetail._txtCurrency = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_Currency_ID"]);
                    //Get C_ConversionType_ID
                    statementDetail._txtConversionType = Util.GetValueOfInt(data.Tables[0].Rows[0]["C_ConversionType_ID"]);
                }
            }

            statementDetail._cmbCurrency = _bankStatementLine.GetC_Currency_ID();
            statementDetail._txtDescription = _bankStatementLine.GetDescription();
            statementDetail._txtVoucherNo = _bankStatementLine.GetVA012_VoucherNo();
            statementDetail._txtCheckNo = _bankStatementLine.GetEftCheckNo();
            statementDetail._cmbCharge = _bankStatementLine.GetC_Charge_ID();

            if (_bankStatementLine.GetC_Charge_ID() > 0)
            {
                statementDetail._txtCharge = chrg.GetName();
            }
            else
            {
                statementDetail._txtCharge = "";
            }
            statementDetail._ctrlPayment = _bankStatementLine.GetC_Payment_ID();
            statementDetail._ctrlOrder = _bankStatementLine.GetC_Order_ID();
            statementDetail._ctrlInvoice = Util.GetValueOfString(_bankStatementLine.GetC_Invoice_ID());//Converting into string Type
            statementDetail._ctrlBusinessPartner = _bankStatementLine.GetC_BPartner_ID();
            statementDetail._bankStatementLineID = _bankStatementLineID;
            statementDetail._txtStatementPage = _bankStatementLine.GetVA012_Page();
            statementDetail._cmbTaxRate = _bankStatementLine.GetC_Tax_ID();
            statementDetail._txtTaxAmount = _bankStatementLine.GetTaxAmt();
            //statementDetail._cmbPaymentMethod = _bankStatementLine.GetVA009_PaymentMethod_ID();
            //statementDetail._chkUseNextTime = _bankStatementLine.IsVA012_IsUseNextTime();
            statementDetail._cmbVoucherMatch = _bankStatementLine.GetVA012_VoucherType();
            statementDetail._cmbContraType = _bankStatementLine.GetVA012_ContraType();

            statementDetail._cmbCashBook = _bankStatementLine.GetC_CashBook_ID();
            MBankStatement _bankStatement = new MBankStatement(ctx, _bankStatementLine.GetC_BankStatement_ID(), null);
            statementDetail._txtStatementNo = _bankStatement.GetName();
            //When bankStatementLine have C_CashLine_ID then the value set to _ctrlCashLine
            if (payment_ID == 0 && !_trxType.Equals("CO"))
            {
                statementDetail._ctrlCashLine = _bankStatementLine.GetC_CashLine_ID();
            }
            statementDetail._trxno = (String)_bankStatementLine.Get_Value("TrxNo");
            if (string.IsNullOrEmpty(statementDetail._txtVoucherNo) && !string.IsNullOrEmpty(statementDetail._trxno))
            {
                statementDetail._txtVoucherNo = statementDetail._trxno;
            }
            statementDetail._txtDifference = Math.Abs(statementDetail._txtTrxAmt) - Math.Abs(statementDetail._txtAmount);
            //either Payment or Prepay Order or Contra by default diffrenceType select as Charge
            if (statementDetail._txtDifference != 0 && payment_ID != 0 && (_trxType.Equals("PY") || _trxType.Equals("PO") || _trxType.Equals("CO")))
            {
                statementDetail._cmbDifferenceType = "CH";
            }
            return statementDetail;

        }
        //public List<GetScheduleProp> GetPaymentSchedules(Ctx ctx, int _paymentID)
        //{
        //    List<GetScheduleProp> _obj = new List<GetScheduleProp>();

        //    return _obj;

        //}
        /// <summary>
        /// Get PaymentAmt and Transaction Amount
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_dragPaymentID">C_Payment_ID</param>
        /// <param name="_dragStatementID">C_BankStatementLine_ID</param>
        /// <param name="statementDate">Statement Date</param>
        /// <returns>list of class object</returns>
        public PaymentResponse MatchByDrag(Ctx ctx, int _dragPaymentID, int _dragStatementID, DateTime? statementDate)
        {
            int _count = 0;
            string _qry = "";
            int _statementBP = 0;
            decimal _statementAmt = 0;
            int _paymentBP = 0;
            decimal _paymentAmt = 0;
            decimal _trxAmt = 0;
            string _authCode = "";
            string _stateDesc = "";
            DateTime? _stateLineDate = null;
            DateTime? _acctDate = null;
            DataSet _ds = new DataSet();
            DataSet _ds1 = new DataSet();
            bool _status = false;
            PaymentResponse _obj = new PaymentResponse();

            _count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) AS COUNT FROM C_BANKSTATEMENTLINE BSL INNER JOIN C_BANKSTATEMENT BS ON BS.C_BANKSTATEMENT_ID = BSL.C_BANKSTATEMENT_ID  WHERE BS.DOCSTATUS!='VO' AND BSL.C_PAYMENT_ID=" + _dragPaymentID));
            if (_count > 0)
            {
                _obj._status = "VA012_PaymentAlreadyMatchedOthrStatement";
                return _obj;

            }
            _count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_PAYMENT_ID FROM C_BANKSTATEMENTLINE WHERE C_BANKSTATEMENTLINE_ID=" + _dragStatementID));
            if (_count > 0)
            {
                _obj._status = "VA012_StatementAlreadyMatchedOthrPayment";
                return _obj;
            }

            _qry = @" SELECT C_BPARTNER_ID, STMTAMT, TRXAMT, TRXNO AS DESCRIPTION, StatementLineDate  
                    FROM C_BANKSTATEMENTLINE
                    WHERE C_BANKSTATEMENTLINE_id      =" + _dragStatementID;
            _ds = DB.ExecuteDataset(_qry);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                _statementBP = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPARTNER_ID"]);
                _statementAmt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["TRXAMT"]);
                _stateDesc = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DESCRIPTION"]);
                //Get the StatementLine Date 
                _stateLineDate = Util.GetValueOfDateTime(_ds.Tables[0].Rows[0]["StatementLineDate"]);
                _ds.Dispose();
            }
            _qry = @" SELECT PAY.C_BPARTNER_ID,
                        CASE
                        WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                        THEN
                            CASE
                            WHEN (DT.DOCBASETYPE='ARR')
                            THEN CURRENCYCONVERT(PAY.PAYAMT, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(_stateLineDate, true) + @", PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) 
                            WHEN (DT.DOCBASETYPE='APP')
                            THEN CURRENCYCONVERT(PAY.PAYAMT * -1, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(_stateLineDate, true) + @", PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) 
                            END
                        ELSE
                            CASE
                            WHEN (DT.DOCBASETYPE='ARR')
                            THEN PAY.PAYAMT
                            WHEN (DT.DOCBASETYPE='APP')
                            THEN PAY.PAYAMT*-1
                            END
                        END AS AMOUNT,
                        CASE
                        WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                        THEN CURRENCYCONVERT(PAY.PAYAMT, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(_stateLineDate, true) + @", PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) 
                        ELSE ROUND(PAY.PAYAMT,NVL(BCURR.StdPrecision,2))
                      END AS TRXAMOUNT,
                        PAY.TrxNo, PAY.DateAcct
                    FROM C_PAYMENT PAY
                    INNER JOIN C_DOCTYPE DT
                    ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                    INNER JOIN C_BANKACCOUNT AC
                    ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                    LEFT JOIN C_CURRENCY BCURR
                    ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID
                    WHERE C_PAYMENT_ID=" + _dragPaymentID;

            _ds1 = DB.ExecuteDataset(_qry);
            if (_ds1 != null && _ds1.Tables[0].Rows.Count > 0)
            {
                _paymentBP = Util.GetValueOfInt(_ds1.Tables[0].Rows[0]["C_BPARTNER_ID"]);
                _paymentAmt = Util.GetValueOfDecimal(_ds1.Tables[0].Rows[0]["AMOUNT"]);
                _trxAmt = Util.GetValueOfDecimal(_ds1.Tables[0].Rows[0]["TRXAMOUNT"]);
                _authCode = Util.GetValueOfString(_ds1.Tables[0].Rows[0]["TrxNo"]);
                //Get the Payment AcctDate 
                _acctDate = Util.GetValueOfDateTime(_ds1.Tables[0].Rows[0]["DateAcct"]);
                _ds1.Dispose();
            }
            //StatementLine Date can't be less than the AcctDate of Payment when Drag the payment on to the Line
            if (_stateLineDate < _acctDate)
            {
                _obj._status = "VA012_StmtDateCantlessTrxDate";
                return _obj;
            }

            if ((_authCode == "" || _authCode == null) || (!string.IsNullOrEmpty(_authCode) && string.IsNullOrEmpty(_stateDesc)))
            {
                if (_statementAmt == 0)
                {
                    _status = true;
                    _obj._amount = _paymentAmt;
                    _obj._trxamount = _trxAmt;

                }
                //both statement amount and payment must be same sign
                if (_statementBP == 0 && ((_statementAmt > 0 && _paymentAmt > 0) || (_statementAmt < 0 && _paymentAmt < 0)))
                {
                    _status = true;
                    _obj._status = "Success";
                    return _obj;
                }
                // no need to match the statement amount with Payment amount but match with sign
                else if (_statementBP > 0 && ((_statementAmt > 0 && _paymentAmt > 0) || (_statementAmt < 0 && _paymentAmt < 0)) && _statementBP == _paymentBP)
                {
                    _status = true;
                    _obj._status = "Success";
                    return _obj;
                }
                //added condition to check the Conversion is Present or not
                else if (_statementBP > 0 && _statementBP == _paymentBP && _paymentAmt == 0)
                {
                    _obj._status = "VA012_ConversionRateNotFound";
                    return _obj;
                }
                else
                {
                    _obj._status = "VA012_StatementPaymentNotMatched";
                    return _obj;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_authCode) && !string.IsNullOrEmpty(_stateDesc))
                {
                    if (_authCode.ToLower().Trim().Contains(_stateDesc.ToLower().Trim()))
                    {
                        if (_authCode.ToLower().Trim().Equals(_stateDesc.ToLower().Trim())
                            || Regex.Matches((_authCode.ToLower().Trim().Replace(_stateDesc.ToLower().Trim(), "")), @"[a-zA-Z]").Count > 0)
                        {
                            if (_statementAmt == 0)
                            {
                                _status = true;
                                _obj._amount = _paymentAmt;
                                _obj._trxamount = _trxAmt;
                                return _obj;
                            }
                            else
                            {
                                _status = true;
                                _obj._status = "Success";
                                return _obj;
                            }
                        }
                        else
                        {
                            _obj._status = "VA012_StatementPaymentNotMatched";
                            return _obj;
                        }
                    }
                    else
                    {
                        _obj._status = "VA012_StatementPaymentNotMatched";
                        return _obj;
                    }
                }
                else
                {
                    _obj._status = "VA012_StatementPaymentNotMatched";
                    return _obj;
                }
            }
        }

        /// <summary>
        /// Update the Statement Line(unmatch line with Payment, Charge or Cash Journal Line)
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_statementLinesList">C_BankStatementLine_ID's</param>
        /// <returns>object of class returns string message</returns>
        public List<UnMatchResponse> UnmatchStatement(Ctx ctx, string _statementLinesList)
        {

            //int _count = 0;
            //try
            //{
            //    _count = DB.ExecuteQuery("UPDATE C_BANKSTATEMENTLINE SET C_PAYMENT_ID=NULL WHERE C_BANKSTATEMENTLINE_ID IN(" + _statementLinesList + ")");
            //}
            //catch (Exception e)
            //{
            //    return e.Message;
            //}
            //return "Success";
            //Using transaction to handle the Exception while Saving the data
            Trx trx = Trx.GetTrx(Trx.CreateTrxName("SUTrx"));

            int status = 1;
            List<UnMatchResponse> _lstObj = new List<UnMatchResponse>();
            UnMatchResponse _unObj = new UnMatchResponse();
            try
            {
                string[] _statementLinesListArray = _statementLinesList.Split(',');
                if (_statementLinesListArray.Length > 0)
                {
                    for (int i = 0; i < _statementLinesListArray.Length; i++)
                    {
                        //replaced null with Transation
                        MBankStatementLine _obj = new MBankStatementLine(ctx, Util.GetValueOfInt(_statementLinesListArray[i]), trx);
                        if (_obj.GetC_Payment_ID() <= 0)
                        {
                            if (_obj.GetC_Charge_ID() > 0)
                            {

                            }
                            else if (_obj.GetC_CashLine_ID() > 0)
                            {

                            }
                            else
                            {
                                continue;
                            }
                        }
                        //replaced null with Transation
                        MBankStatement _objSt = new MBankStatement(ctx, _obj.GetC_BankStatement_ID(), trx);
                        if (_objSt.GetDocStatus() == "CO")
                        {
                            status = 0;
                            if (_unObj._statementNo == null)
                            {
                                _unObj._statementNo = _objSt.GetName() + "/" + _obj.GetLine();
                            }
                            else
                            {
                                _unObj._statementNo = _unObj._statementNo + ", " + _objSt.GetName() + "/" + _obj.GetLine();
                            }

                        }
                        else
                        {
                            if (_obj.GetC_Payment_ID() > 0)
                            {
                                _obj.SetC_Payment_ID(0);
                                //When Unmatch the payment and charge is selected then UnMatch the Charge_ID from Bank Statement line
                                //_obj.SetC_Charge_ID(0);
                            }
                            if (_obj.GetC_Charge_ID() > 0)
                            {
                                _obj.SetC_Charge_ID(0);
                                _obj.SetC_Tax_ID(0);
                                _obj.SetTaxAmt(0);
                                _obj.SetChargeAmt(0);
                                //When Unmatch the Bank StatementLine then reset the Surcharge Amount
                                _obj.Set_Value("SurchargeAmt", 0);
                                _obj.SetTrxAmt(_obj.GetStmtAmt());
                            }
                            if (_obj.GetC_CashLine_ID() > 0)
                            {
                                _obj.SetC_CashLine_ID(0);
                            }
                            //Clear ConversionType
                            _obj.Set_Value("C_ConversionType_ID", 0);

                            _obj.SetVA012_IsMatchingConfirmed(false);
                            if (!_obj.Save())
                            {
                                status = 0;
                                trx.Rollback();
                                //Used ValueNamePair to get Error
                                ValueNamePair pp = VLogger.RetrieveError();
                                //some times getting the error pp also
                                string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                if (string.IsNullOrEmpty(error))
                                {
                                    error = pp != null ? pp.GetName() : "";
                                }

                                _unObj._statementNoNotUpdate = !string.IsNullOrEmpty(error) ? error : _objSt.GetName() + "/" + _obj.GetLine();

                                _lstObj.Add(_unObj);
                                trx.Close();
                                trx = null;
                                return _lstObj;
                                //Used ValueNamePair to get Error
                                //if (_unObj._statementNoNotUpdate == null)
                                //{
                                //    _unObj._statementNoNotUpdate = _objSt.GetName() + "/" + _obj.GetLine();
                                //}
                                //else
                                //{
                                //    _unObj._statementNoNotUpdate = _unObj._statementNoNotUpdate + ", " + _objSt.GetName() + "/" + _obj.GetLine();
                                //}

                            }
                            else
                            {
                                //Commit the transaction
                                trx.Commit();
                                if (_unObj._statementOk == null)
                                {
                                    _unObj._statementOk = _objSt.GetName() + "/" + _obj.GetLine();
                                }
                                else
                                {
                                    _unObj._statementOk = _unObj._statementOk + ", " + _objSt.GetName() + "/" + _obj.GetLine();
                                }

                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                status = 0;
                _unObj._error = e.Message;
                _lstObj.Add(_unObj);
                //Roll back the Transaction
                trx.Rollback();
                trx.Close();
                trx = null;
                return _lstObj;
            }
            if (status == 1)
            {
                _unObj._status = "Success";
                _lstObj.Add(_unObj);
            }
            else
            {
                _lstObj.Add(_unObj);
            }
            //Close the transaction
            trx.Close();
            trx = null;
            return _lstObj;
        }
        public List<ProcessResponse> ProcessStatement(Ctx ctx, string _statementLinesList, int _accountID)
        {
            DataSet _ds = new DataSet();
            int status = 1;
            List<ProcessResponse> _lstObj = new List<ProcessResponse>();
            ProcessResponse _obj = new ProcessResponse();

            //            string _sql = @"SELECT DISTINCT BSLOUT.C_BANKSTATEMENT_ID,
            //                          (SELECT COUNT(BSL.VA012_ISMATCHINGCONFIRMED)
            //                          FROM C_BANKSTATEMENTLINE BSL
            //                          WHERE BSL.C_BANKSTATEMENT_ID     =BSLOUT.C_BANKSTATEMENT_ID
            //                          AND BSL.VA012_ISMATCHINGCONFIRMED='N'
            //                          ) AS UNCONFIRMED
            //                        FROM C_BANKSTATEMENTLINE BSLOUT WHERE C_BANKSTATEMENTLINE_ID IN(" + _statementLinesList + ")";
            string _sql = @"SELECT DISTINCT BSLOUT.C_BANKSTATEMENT_ID,
                              (SELECT COUNT(BSL.VA012_ISMATCHINGCONFIRMED)
                              FROM C_BANKSTATEMENTLINE BSL
                              WHERE BSL.C_BANKSTATEMENT_ID     =BSLOUT.C_BANKSTATEMENT_ID
                              AND BSL.VA012_ISMATCHINGCONFIRMED='N'
                              ) AS UNCONFIRMED,
                              (SELECT COUNT(BSL.VA012_ISMATCHINGCONFIRMED)
                              FROM C_BANKSTATEMENTLINE BSL
                              WHERE BSL.C_BANKSTATEMENT_ID     =BSLOUT.C_BANKSTATEMENT_ID
                              AND BSL.VA012_ISMATCHINGCONFIRMED='Y'
                              ) AS CONFIRMED
                            FROM C_BANKSTATEMENT BSOUT
                            INNER JOIN C_BANKSTATEMENTLINE BSLOUT
                            ON BSLOUT.C_BANKSTATEMENT_ID=BSOUT.C_BANKSTATEMENT_ID
                            WHERE BSOUT.ISACTIVE        ='Y'
                            AND BSOUT.DOCSTATUS NOT    IN ('CO','CL','VO')
                            AND BSOUT.C_BANKACCOUNT_ID =" + _accountID;
            try
            {
                _ds = DB.ExecuteDataset(_sql);
                if (_ds != null)
                {
                    if (_ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                        {
                            MBankStatement _bankStatement = new MBankStatement(ctx, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BANKSTATEMENT_ID"]), null);
                            if (Util.GetValueOfInt(_ds.Tables[0].Rows[i]["UNCONFIRMED"]) > 0)
                            {
                                status = 0;
                                if (_obj._statementUnmatchedLines == null)
                                {
                                    _obj._statementUnmatchedLines = _bankStatement.GetName();
                                }
                                else
                                {
                                    _obj._statementUnmatchedLines = _obj._statementUnmatchedLines + ", " + _bankStatement.GetName();
                                }

                            }
                            else if (Util.GetValueOfInt(_ds.Tables[0].Rows[i]["CONFIRMED"]) > 0)
                            {
                                if (_bankStatement.CompleteIt() == "CO")
                                {
                                    _bankStatement.SetProcessed(true);
                                    _bankStatement.SetDocAction(MBankStatement.DOCACTION_Close);
                                    _bankStatement.SetDocStatus("CO");
                                    _bankStatement.Save();

                                    if (_obj._statementProcessed == null)
                                    {
                                        _obj._statementProcessed = _bankStatement.GetName();
                                    }
                                    else
                                    {
                                        _obj._statementProcessed = _obj._statementProcessed + ", " + _bankStatement.GetName();
                                    }

                                }
                                else
                                {
                                    status = 0;
                                    if (_obj._statementNotProcessed == null)
                                    {
                                        _obj._statementNotProcessed = _bankStatement.GetName();
                                    }
                                    else
                                    {
                                        _obj._statementNotProcessed = _obj._statementNotProcessed + ", " + _bankStatement.GetName();
                                    }
                                }

                            }
                        }
                    }
                    _ds.Dispose();
                }

            }
            catch (Exception e)
            {
                if (_ds != null)
                {
                    _ds.Dispose();
                }
                _obj._error = e.Message;
                _lstObj.Add(_obj);
                return _lstObj;
            }
            if (status == 1)
            {
                _obj._status = "Success";
                _lstObj.Add(_obj);
            }
            else
            {
                _lstObj.Add(_obj);
            }
            return _lstObj;
        }
        public List<ProcessResponse> DeleteStatement(Ctx ctx, string _statementLinesList, int _statementLineID)
        {
            int status = 1;
            string _no = "";
            int _page = 0;
            int _line = 0;
            string[] _statementLines = null;
            string _statementHeader = "";
            string _sql = "";
            DataSet _ds = new DataSet();
            List<ProcessResponse> _lstObj = new List<ProcessResponse>();
            ProcessResponse _obj = new ProcessResponse();
            if (_statementLinesList != "" && _statementLinesList != null)
            {
                _statementLines = _statementLinesList.Split(',');

            }
            else
            {
                _statementLines = new string[1];
                _statementLines[0] = _statementLineID.ToString();
            }

            try
            {
                if (_statementLines.Length > 0)
                {


                    for (int i = 0; i < _statementLines.Length; i++)
                    {
                        MBankStatementLine _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(Util.GetValueOfInt(_statementLines[i])), null);
                        MBankStatement _bankStatement = new MBankStatement(ctx, _bankStatementLine.GetC_BankStatement_ID(), null);
                        _no = "";
                        _page = 0;
                        _line = 0;
                        _no = _bankStatement.GetName();
                        _page = _bankStatementLine.GetVA012_Page();
                        _line = _bankStatementLine.GetLine();

                        if (_statementHeader == "")
                        {
                            _statementHeader = Util.GetValueOfString(_bankStatementLine.GetC_BankStatement_ID());
                        }
                        else
                        {
                            _statementHeader = _statementHeader + "," + Util.GetValueOfString(_bankStatementLine.GetC_BankStatement_ID());
                        }

                        if (_bankStatementLine.Delete(true))
                        {

                            if (_obj._statementProcessed == null)
                            {
                                _obj._statementProcessed = _no + "/" + _page + "/" + _line;
                            }
                            else
                            {
                                _obj._statementProcessed = _obj._statementProcessed + ", " + _no + "/" + _page + "/" + _line;
                            }

                        }
                        else
                        {
                            status = 0;
                            if (_obj._statementNotProcessed == null)
                            {
                                _obj._statementNotProcessed = _no + "/" + _page + "/" + _line;
                            }
                            else
                            {
                                _obj._statementNotProcessed = _obj._statementNotProcessed + ", " + _no + "/" + _page + "/" + _line;
                            }
                        }


                    }

                    //delete header

                    if (_statementHeader != "")
                    {


                        _sql = @"SELECT C_BANKSTATEMENT_ID
                            FROM
                              (SELECT BS.C_BANKSTATEMENT_ID ,
                                COUNT(BSL.C_BANKSTATEMENTLINE_ID) AS LINECOUNT
                              FROM C_BANKSTATEMENT BS
                              LEFT JOIN C_BANKSTATEMENTLINE BSL
                              ON BS.C_BANKSTATEMENT_ID     = BSL.C_BANKSTATEMENT_ID
                              WHERE BS.C_BANKSTATEMENT_ID IN (" + _statementHeader +
                            @")
                              GROUP BY BS.C_BANKSTATEMENT_ID
                              )
                            WHERE LINECOUNT=0";

                        _ds = DB.ExecuteDataset(_sql);
                        if (_ds != null)
                        {
                            if (_ds.Tables[0].Rows.Count > 0)
                            {
                                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                                {
                                    MBankStatement _bankStatement = new MBankStatement(ctx, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BANKSTATEMENT_ID"]), null);
                                    _bankStatement.Delete(true);
                                }
                            }
                        }
                    }

                    ////

                }

            }
            catch (Exception e)
            {
                _obj._error = e.Message;
                _lstObj.Add(_obj);
                return _lstObj;
            }
            if (status == 1)
            {
                _obj._status = "Success";
                _lstObj.Add(_obj);
            }
            else
            {
                _lstObj.Add(_obj);
            }
            return _lstObj;
        }

        #region matching statement
        //        public List<MatchResponse> MatchStatement(Ctx ctx, string _matchingBaseItemList, string _cmbMatchingCriteria, int _BankAccount)
        //        {
        //            List<MatchResponse> _lstObj = new List<MatchResponse>();
        //            MatchResponse _obj = new MatchResponse();
        //            string[] _BaseItemList = _matchingBaseItemList.Split(',');
        //            StringBuilder _sql = new StringBuilder();
        //            DataSet _dsPayments = new DataSet();
        //            DataSet _dsStatements = new DataSet();
        //            List<int> _bankStatementLineID = new List<int>();
        //            List<int> _paymentID = new List<int>();
        //            try
        //            {
        //                #region Payments
        //                _sql.Clear();
        //                _sql.Append(@"SELECT CP.C_PAYMENT_ID,
        //                              CP.C_INVOICE_ID,
        //                              CP.C_BPARTNER_ID,
        //                              CP.PAYAMT AS PAYMENTAMOUNT,
        //                              TRUNC(CP.DATETRX) AS PAYMENTDATE,
        //                              CP.CHECKNO,
        //                              0 AS VA009_PAYMENTMETHOD_ID,
        //                              CP.C_ORDER_ID,
        //                            CP.DOCUMENTNO AS PAYMENTNO
        //                            FROM C_PAYMENT CP
        //                            LEFT JOIN C_BANKSTATEMENTLINE BSL
        //                            ON cp.C_PAYMENT_ID      =BSl.C_PAYMENT_ID ");

        //                _sql.Append(@" WHERE CP.ISACTIVE='Y' AND CP.ISRECONCILED ='N' AND BSL.C_PAYMENT_ID IS NULL 
        //                                 AND CP.C_BANKACCOUNT_ID=" + _BankAccount
        //                             + " AND CP.AD_CLIENT_ID=" + ctx.GetAD_Client_ID()
        //                             + " AND CP.AD_ORG_ID=" + ctx.GetAD_Org_ID());
        //                _dsPayments = DB.ExecuteDataset(_sql.ToString());
        //                _sql.Clear();
        //                #endregion Payments
        //                #region Statement Lines
        //                _sql.Clear();
        //                _sql.Append(@"SELECT BSL.C_PAYMENT_ID,
        //                              BSL.C_INVOICE_ID,
        //                              BSL.C_BANKSTATEMENTLINE_ID,
        //                              BSL.C_BPARTNER_ID,
        //                              ABS(BSL.TRXAMT) AS PAYMENTAMOUNT, 
        //                              TRUNC(BSL.STATEMENTLINEDATE) AS PAYMENTDATE,
        //                              ''                  AS CHECKNO,
        //                              0                   AS VA009_PAYMENTMETHOD_ID,
        //                              INV.C_ORDER_ID,
        //                              BS.NAME AS STATEMENTNO,
        //                              BSL.LINE AS STATEMENTLINENO
        //                            FROM C_BANKSTATEMENTLINE BSL
        //                            INNER JOIN C_BANKSTATEMENT BS
        //                            ON BSL.C_BANKSTATEMENT_ID=BS.C_BANKSTATEMENT_ID
        //                            LEFT JOIN C_PAYMENT CP
        //                            ON BSL.C_PAYMENT_ID =CP.C_PAYMENT_ID
        //                            LEFT JOIN C_INVOICE INV
        //                            ON BSL.C_INVOICE_ID     =INV.C_INVOICE_ID");

        //                _sql.Append(@" WHERE BSL.ISACTIVE='Y' AND BSL.C_PAYMENT_ID IS NULL
        //                                 AND BS.C_BANKACCOUNT_ID=" + _BankAccount
        //                             + " AND BSL.AD_CLIENT_ID=" + ctx.GetAD_Client_ID()
        //                             + " AND BSL.AD_ORG_ID=" + ctx.GetAD_Org_ID());
        //                _dsStatements = DB.ExecuteDataset(_sql.ToString());
        //                _sql.Clear();
        //                #endregion Statement Lines

        //                #region Match All / Match One
        //                if (_cmbMatchingCriteria == "AL" || _cmbMatchingCriteria == "AO")
        //                {
        //                    if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
        //                    {
        //                        DataRow[] _drStatements = null;
        //                        string _condition = "";
        //                        for (int i = 0; i < _dsPayments.Tables[0].Rows.Count; i++)
        //                        {
        //                            _condition = "";

        //                            _condition = GetCondition(i, _dsPayments, _BaseItemList, _cmbMatchingCriteria);

        //                            _drStatements = _dsStatements.Tables[0].Select(_condition);
        //                            if (_drStatements.Length > 0)
        //                            {
        //                                for (int k = 0; k < _drStatements.Length; k++)
        //                                {
        //                                    _obj = new MatchResponse();
        //                                    if (_bankStatementLineID.Contains(Util.GetValueOfInt(_drStatements[k]["C_BANKSTATEMENTLINE_ID"])))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    if (_paymentID.Contains(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_PAYMENT_ID"])))
        //                                    {
        //                                        continue;
        //                                    }
        //                                    MBankStatementLine _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_drStatements[k]["C_BANKSTATEMENTLINE_ID"]), null);
        //                                    _bankStatementLine.SetC_Payment_ID(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_PAYMENT_ID"]));
        //                                    _bankStatementLine.SetVA012_IsMatchingConfirmed(true);
        //                                    if (!_bankStatementLine.Save())
        //                                    {
        //                                        _obj._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_drStatements[k]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_drStatements[0]["STATEMENTLINENO"]);
        //                                        _lstObj.Add(_obj);
        //                                    }
        //                                    else
        //                                    {
        //                                        _bankStatementLineID.Add(Util.GetValueOfInt(_drStatements[k]["C_BANKSTATEMENTLINE_ID"]));
        //                                        _paymentID.Add(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_PAYMENT_ID"]));
        //                                        _obj._statementNo = Util.GetValueOfString(_drStatements[k]["STATEMENTNO"]);
        //                                        _obj._statementLine = Util.GetValueOfString(_drStatements[k]["STATEMENTLINENO"]);
        //                                        _obj._paymentNo = Util.GetValueOfString(_dsPayments.Tables[0].Rows[i]["PAYMENTNO"]);
        //                                        _lstObj.Add(_obj);
        //                                    }
        //                                }
        //                            }

        //                        }
        //                    }

        //                }
        //                #endregion Match All / Match One

        //                #region Match Two
        //                else if (_cmbMatchingCriteria == "AT")
        //                {
        //                    Dictionary<int, string[]> _combinationList = new Dictionary<int, string[]>();
        //                    _combinationList = GetAllCombinations(_BaseItemList);
        //                    if (_combinationList.Count > 0)
        //                    {
        //                        for (int j = 0; j < _combinationList.Count; j++)
        //                        {

        //                            //////
        //                            if (_dsPayments != null && _dsPayments.Tables[0].Rows.Count > 0)
        //                            {
        //                                DataRow[] _drStatements = null;
        //                                string _condition = "";
        //                                for (int i = 0; i < _dsPayments.Tables[0].Rows.Count; i++)
        //                                {
        //                                    _condition = "";
        //                                    _condition = GetCondition(i, _dsPayments, _combinationList[j], _cmbMatchingCriteria);

        //                                    _drStatements = _dsStatements.Tables[0].Select(_condition);
        //                                    if (_drStatements.Length > 0)
        //                                    {
        //                                        for (int k = 0; k < _drStatements.Length; k++)
        //                                        {
        //                                            _obj = new MatchResponse();
        //                                            if (_bankStatementLineID.Contains(Util.GetValueOfInt(_drStatements[k]["C_BANKSTATEMENTLINE_ID"])))
        //                                            {
        //                                                continue;
        //                                            }
        //                                            if (_paymentID.Contains(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_PAYMENT_ID"])))
        //                                            {
        //                                                continue;
        //                                            }
        //                                            MBankStatementLine _bankStatementLine = new MBankStatementLine(ctx, Util.GetValueOfInt(_drStatements[k]["C_BANKSTATEMENTLINE_ID"]), null);
        //                                            _bankStatementLine.SetC_Payment_ID(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_PAYMENT_ID"]));
        //                                            _bankStatementLine.SetVA012_IsMatchingConfirmed(true);
        //                                            if (!_bankStatementLine.Save())
        //                                            {
        //                                                _obj._error = Msg.GetMsg(ctx, "VA012_ErrorSaving") + " : " + Util.GetValueOfString(_drStatements[k]["STATEMENTNO"]) + ">" + Util.GetValueOfString(_drStatements[0]["STATEMENTLINENO"]);
        //                                                _lstObj.Add(_obj);
        //                                            }
        //                                            else
        //                                            {
        //                                                _bankStatementLineID.Add(Util.GetValueOfInt(_drStatements[k]["C_BANKSTATEMENTLINE_ID"]));
        //                                                _paymentID.Add(Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_PAYMENT_ID"]));
        //                                                _obj._statementNo = Util.GetValueOfString(_drStatements[k]["STATEMENTNO"]);
        //                                                _obj._statementLine = Util.GetValueOfString(_drStatements[k]["STATEMENTLINENO"]);
        //                                                _obj._paymentNo = Util.GetValueOfString(_dsPayments.Tables[0].Rows[i]["PAYMENTNO"]);
        //                                                _lstObj.Add(_obj);
        //                                            }
        //                                        }
        //                                    }

        //                                }
        //                            }

        //                            //////

        //                        }
        //                    }


        //                }
        //                #endregion Match Two

        //                if (_dsPayments != null)
        //                {
        //                    _dsPayments.Dispose();
        //                }
        //                if (_dsStatements != null)
        //                {
        //                    _dsStatements.Dispose();
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //                if (_dsPayments != null)
        //                {
        //                    _dsPayments.Dispose();
        //                }
        //                if (_dsStatements != null)
        //                {
        //                    _dsStatements.Dispose();
        //                }
        //                _obj._error = e.Message;
        //                _lstObj.Add(_obj);
        //                return _lstObj;
        //            }
        //            return _lstObj;
        //        }
        //        private static Dictionary<int, string[]> GetAllCombinations(string[] _BaseItemList)
        //        {
        //            string[] _combinationArray = null;
        //            Dictionary<int, string[]> _combinationList = new Dictionary<int, string[]>();
        //            var combinations = Combinations(0, 2, _BaseItemList);
        //            int _count = 0;
        //            foreach (var item in combinations)
        //            {
        //                _combinationArray = item.Split(',');
        //                _combinationList.Add(_count, _combinationArray);
        //                _count++;
        //            }
        //            return _combinationList;
        //        }
        //        private static IEnumerable<string> Combinations(int start, int level, string[] _BaseItemList)
        //        {
        //            for (int i = start; i < _BaseItemList.Length; i++)
        //            {
        //                if (level == 1)
        //                {
        //                    yield return _BaseItemList[i];
        //                }
        //                else
        //                {
        //                    foreach (string combination in Combinations(i + 1, level - 1, _BaseItemList))
        //                    {
        //                        yield return String.Format("{0},{1}", _BaseItemList[i], combination);
        //                    }
        //                }
        //            }
        //        }
        //        public string GetCondition(int i, DataSet _dsPayments, string[] _BaseItemList, string _cmbMatchingCriteria)
        //        {
        //            StringBuilder _condition = new StringBuilder();
        //            for (int j = 0; j < _BaseItemList.Length; j++)
        //            {
        //                // ["BP", "PA", "PD", "CN", "PM", "OR", "IN"]
        //                if (j != 0)
        //                {
        //                    if (_cmbMatchingCriteria == "AL" || _cmbMatchingCriteria == "AT")
        //                    {
        //                        _condition.Append(" AND ");
        //                    }
        //                    else if (_cmbMatchingCriteria == "AO")
        //                    {
        //                        _condition.Append(" OR ");
        //                    }
        //                }
        //                if (_BaseItemList[j] == "IN")       //Invoice //Done
        //                {
        //                    _condition.Append(" C_INVOICE_ID=" + Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_INVOICE_ID"]));
        //                }
        //                else if (_BaseItemList[j] == "BP")  //Business Partner //Done
        //                {
        //                    _condition.Append(" C_BPARTNER_ID=" + Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_BPARTNER_ID"]));
        //                }
        //                else if (_BaseItemList[j] == "PA")  //Payment Amount //Done
        //                {
        //                    _condition.Append(" PAYMENTAMOUNT=" + Util.GetValueOfDecimal(_dsPayments.Tables[0].Rows[i]["PAYMENTAMOUNT"]));
        //                }
        //                else if (_BaseItemList[j] == "PD")  //Payment Date //Done
        //                {
        //                    _condition.Append(" PAYMENTDATE='#" + Util.GetValueOfDateTime(_dsPayments.Tables[0].Rows[i]["PAYMENTDATE"]).Value.ToShortDateString() + "#'");
        //                }
        //                #region Don't Delete
        //                //else if (_BaseItemList[j] == "CN")  //Cheque No // Field not exist on Statement
        //                //{
        //                //    _condition.Append(" CHECKNO='" + Util.GetValueOfString(_dsPayments.Tables[0].Rows[i]["CHECKNO"]) + "'");
        //                //}
        //                //else if (_BaseItemList[j] == "PM")  //Payment Method // Field not exist on Statement
        //                //{
        //                //    _condition.Append(" VA009_PAYMENTMETHOD_ID=" + Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["VA009_PAYMENTMETHOD_ID"]));
        //                //}
        //                //else if (_BaseItemList[j] == "OR") //Order // Done // Get against Invoice
        //                //{
        //                //    _condition.Append(" C_ORDER_ID=" + Util.GetValueOfInt(_dsPayments.Tables[0].Rows[i]["C_ORDER_ID"]));
        //                //}
        //                #endregion Don't Delete

        //            }
        //            return _condition.ToString();
        //        }
        #endregion matching statement


        //
        public int LoadStatementsPages(Ctx ctx, int _cmbBankAccount, int _statementPageNo, int _PAGESIZE, bool _SEARCHREQUEST, string _txtSearch)
        {
            string _sql = "";

            _sql = "SELECT COUNT(*) AS Records "
               + " FROM C_BANKSTATEMENT BS "
               + " INNER JOIN C_BANKSTATEMENTLINE BSL "
               + " ON BS.C_BANKSTATEMENT_ID=BSL.C_BANKSTATEMENT_ID "
               + " LEFT JOIN C_INVOICE INV "
               + " ON BSL.C_INVOICE_ID = INV.C_INVOICE_ID "
               + "  LEFT JOIN C_BPARTNER BP "
               + "  ON BSL.C_BPARTNER_ID =BP.C_BPARTNER_ID "
               + " LEFT JOIN C_CHARGE CHRG "
               + " ON BSL.C_CHARGE_ID=CHRG.C_CHARGE_ID "
              //+ " LEFT JOIN AD_IMAGE IMG "
              //+ " ON BP.PIC=IMG.AD_IMAGE_ID "
              + "  LEFT JOIN C_BP_GROUP BPG "
              + " ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID "
              + "  LEFT JOIN C_CURRENCY CURR "
              + "  ON BSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID "
                + " INNER JOIN AD_CLIENTINFO CINFO  "
                + " ON CINFO.AD_CLIENT_ID =BSL.AD_CLIENT_ID  "
                + " INNER JOIN C_ACCTSCHEMA AC  "
                + " ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID  "
                + " LEFT JOIN C_CURRENCY BCURR  "
                + " ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID  "

               + " WHERE BS.ISACTIVE='Y' AND BS.C_BANKACCOUNT_ID= " + _cmbBankAccount + " AND BS.DOCSTATUS !='VO' AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();


            if (ctx.GetAD_Org_ID() != 0)
            {
                _sql += " AND BS.AD_ORG_ID=" + ctx.GetAD_Org_ID();
            }

            if (_SEARCHREQUEST)
            {
                _sql += " AND (UPPER(BP.NAME) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BSL.DESCRIPTION) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BS.NAME) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BSL.StmtAmt) LIKE UPPER('%" + _txtSearch + "%'))";
            }


            #region Check Total Pages Count
            int _totalPageCount = 0;
            int _totalRecordCount = 0;
            _totalRecordCount = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
            _totalPageCount = Util.GetValueOfInt(Math.Ceiling((decimal)_totalRecordCount / _PAGESIZE));
            #endregion Check Total Pages Count
            return _totalPageCount;
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
        /// <returns>Count</returns>
        public int LoadPaymentsPages(Ctx ctx, int _accountID, int _paymentPageNo, int _PAGESIZE, int _paymentMethodID, string _transactionType)
        {
            int _totalPageCount = 0;
            int _totalRecordCount = 0;
            string _sql = "";
            //fetch the record count according to the BankAccount_Organization
            int _bankOrg_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Org_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + _accountID));

            if (_transactionType == "PY")
            {
                _sql = " SELECT COUNT(*) AS Records"
                   + " FROM C_PAYMENT PAY "
                   + " LEFT JOIN C_BPARTNER BP "
                   + " ON PAY.C_BPARTNER_ID =BP.C_BPARTNER_ID "

                   + " LEFT JOIN C_BANKSTATEMENTLINE BSL "
                   + " ON PAY.C_PAYMENT_ID =BSL.C_PAYMENT_ID "

                   //+ " LEFT JOIN AD_IMAGE IMG "
                   //+ " ON BP.PIC=IMG.AD_IMAGE_ID "
                   + " LEFT JOIN C_BP_GROUP BPG "
                   + " ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID "
                   + " LEFT JOIN C_CURRENCY CURR "
                   + " ON PAY.C_CURRENCY_ID =CURR.C_CURRENCY_ID "
                     + " INNER JOIN C_BANKACCOUNT AC  "
                      + " ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID  "

                   + " LEFT JOIN VA009_PAYMENTMETHOD PM  "
                   + " ON (PM.VA009_PAYMENTMETHOD_ID   =PAY.VA009_PAYMENTMETHOD_ID ) "

                   + " INNER JOIN C_DOCTYPE DT "
                   + " ON DT.C_DOCTYPE_ID            =PAY.C_DOCTYPE_ID "

                   + " WHERE PAY.ISACTIVE   ='Y' AND PAY.DOCSTATUS IN ('CO','CL') AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE       IS NULL) AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID()
               + " AND PAY.ISRECONCILED ='N' ";


                _sql += " AND PAY.C_BANKACCOUNT_ID= " + _accountID;

                //filter the records based on Bank Organization not the context Org
                if (_bankOrg_ID != 0)
                {
                    _sql += " AND PAY.AD_ORG_ID=" + _bankOrg_ID;
                }
                if (_paymentMethodID > 0)
                {
                    _sql += " AND PAY.VA009_PAYMENTMETHOD_ID= " + _paymentMethodID;
                }
            }
            else if (_transactionType == "IS")
            {
                _sql = @" SELECT 
                              COUNT(*) AS Records
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON pay.C_INVOICE_id=inv.C_INVOICE_id
                            LEFT JOIN C_BPARTNER BP
                            ON inv.C_BPARTNER_ID =BP.C_BPARTNER_ID
                            --LEFT JOIN AD_IMAGE IMG
                            --ON BP.PIC=IMG.AD_IMAGE_ID
                            LEFT JOIN C_BP_GROUP BPG
                            ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID
                            LEFT JOIN C_CURRENCY CURR
                            ON inv.C_CURRENCY_ID =CURR.C_CURRENCY_ID

                            INNER JOIN VA009_PAYMENTMETHOD PM  
                            ON (PM.VA009_PAYMENTMETHOD_ID   =PAY.VA009_PAYMENTMETHOD_ID )
                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID            =INV.C_DOCTYPE_ID
                            WHERE  pay.VA009_IsPaid='N'
                            AND PAY.ISACTIVE      ='Y' AND INV.DOCSTATUS IN ('CO','CL') AND PM.VA009_PAYMENTBASETYPE!='B' AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                //filter the records based on Bank Organization not the context Org
                if (_bankOrg_ID != 0)
                {
                    _sql += " AND PAY.AD_ORG_ID=" + _bankOrg_ID;
                }
                if (_paymentMethodID > 0)
                {
                    _sql += " AND PAY.VA009_PAYMENTMETHOD_ID= " + _paymentMethodID;
                }
                //                //Check Schedule already mapped to payment
                //                _sql += @" AND PAY.C_INVOICEPAYSCHEDULE_ID NOT IN (SELECT NVL(C_INVOICEPAYSCHEDULE_ID,0)
                //                            FROM
                //                              (SELECT PAY1.C_INVOICEPAYSCHEDULE_ID
                //                              FROM C_PAYMENT PAY1
                //                              WHERE PAY1.DOCSTATUS NOT IN ('VO','RE')
                //                              UNION
                //                              SELECT PA1.C_INVOICEPAYSCHEDULE_ID
                //                              FROM C_PAYMENTALLOCATE PA1
                //                              INNER JOIN C_PAYMENT PAY1
                //                              ON PAY1.C_PAYMENT_ID      =PA1.C_PAYMENT_ID
                //                              WHERE PAY1.DOCSTATUS NOT IN ('VO','RE')
                //                              )
                //                            )";
                //                ////

            }
            else if (_transactionType == "PO")
            {
                _sql = @" SELECT COUNT(*) AS Records
                        FROM C_ORDER ORD
                        LEFT JOIN C_DOCTYPE DT
                        ON ORD.C_DocTypeTarget_ID=dt.C_DocType_ID
                        LEFT JOIN C_BPARTNER BP
                        ON ord.C_BPARTNER_ID =BP.C_BPARTNER_ID
                        --LEFT JOIN AD_IMAGE IMG
                        --ON BP.PIC=IMG.AD_IMAGE_ID
                        LEFT JOIN C_BP_GROUP BPG
                        ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID
                        LEFT JOIN C_CURRENCY CURR
                        ON ord.C_CURRENCY_ID =CURR.C_CURRENCY_ID
                       

                        INNER JOIN VA009_PAYMENTMETHOD PM  
                        ON (PM.VA009_PAYMENTMETHOD_ID   =ORD.VA009_PAYMENTMETHOD_ID )


                        WHERE dt.DocSubTypeSO='PR'
                        AND ORD.DOCSTATUS    ='WP'
                            AND ORD.ISACTIVE      ='Y' AND PM.VA009_PAYMENTBASETYPE!='B' AND ORD.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                //filter the records based on Bank Organization not the context Org
                if (_bankOrg_ID != 0)
                {
                    _sql += " AND ORD.AD_ORG_ID=" + _bankOrg_ID;
                }
                if (_paymentMethodID > 0)
                {
                    _sql += " AND ORD.VA009_PAYMENTMETHOD_ID= " + _paymentMethodID;
                }

            }
            else if (_transactionType == "CO")
            {
                _sql = @" SELECT COUNT(*) AS Records
                        FROM C_CASH CS
                        INNER JOIN C_CASHLINE CSL
                        ON CS.C_CASH_ID=CSL.C_CASH_ID
                        LEFT JOIN C_BPARTNER BP
                        ON CSL.C_BPARTNER_ID =BP.C_BPARTNER_ID
                        --LEFT JOIN AD_IMAGE IMG
                        --ON BP.PIC=IMG.AD_IMAGE_ID
                        LEFT JOIN C_BP_GROUP BPG
                        ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID
                        LEFT JOIN C_CURRENCY CURR
                        ON CSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID
                     WHERE CS.ISACTIVE   ='Y' AND CS.DOCSTATUS IN ('CO','CL') AND CS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
                    AND CSL.VA012_ISRECONCILED ='N' AND CSL.C_BANKACCOUNT_ID= " + _accountID;
                //filter the records based on Bank Organization not the context Org
                if (_bankOrg_ID != 0)
                {
                    _sql += " AND CS.AD_ORG_ID=" + _bankOrg_ID;
                }
                _sql += " ORDER BY CS.NAME";
            }
            _totalRecordCount = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
            _totalPageCount = Util.GetValueOfInt(Math.Ceiling((decimal)_totalRecordCount / _PAGESIZE));
            return _totalPageCount;
        }
        //

        /// <summary>
        /// to get the data based on selected parameters from payment window
        /// </summary>
        /// <param name="ctx">context</param>
        /// <param name="_accountID">Bank Account ID</param>
        /// <param name="_paymentPageNo">Payment Page Number</param>
        /// <param name="_PAGESIZE">Page Size</param>
        /// <param name="_paymentMethodID">Payment Method ID</param>
        /// <param name="_transactionType">Transaction Type</param>
        /// <param name="statementDate">Statement Date</param>
        /// <returns>List of Payment Records</returns>
        public List<PaymentProp> LoadPayments(Ctx ctx, int _accountID, int _paymentPageNo, int _PAGESIZE, int _paymentMethodID, string _transactionType, DateTime? statementDate)
        {
            //int _accountCurrencyID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CURRENCY_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + _accountID));
            int bankCurr_ID = 0;
            int bankOrg_ID = 0;
            DataSet ds = DB.ExecuteDataset("SELECT C_CURRENCY_ID,AD_Org_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + _accountID);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                bankCurr_ID = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Currency_ID"]);
                bankOrg_ID = Util.GetValueOfInt(ds.Tables[0].Rows[0]["AD_Org_ID"]);
            }
            //multiply rate 


            //
            string _sql = "";
            int _CountVA034 = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(AD_MODULEINFO_ID) FROM AD_MODULEINFO WHERE PREFIX='VA034_' AND IsActive='Y'"));
            if (_transactionType == "PY")
            {



                _sql = " SELECT PAY.C_PAYMENT_ID, "
                             + " CURR.ISO_CODE AS CURRENCY, "
                            + "  PAY.DOCUMENTNO    AS PAYMENTNO, ";

                if (_CountVA034 > 0)
                    _sql += "PAY.VA034_DepositSlipNo    AS DepositSlipNo, ";


                _sql += "  PAY.C_BPARTNER_ID , "
                            + "  BP.NAME AS BUSINESSPARTNER, "

                             + "  CASE "
                            + "  WHEN (DT.DOCBASETYPE='ARR') "
                           + "  THEN ROUND(PAY.PAYAMT,NVL(BCURR.StdPrecision,2)) "
                           + "  WHEN (DT.DOCBASETYPE='APP') "
                           + "  THEN ROUND(PAY.PAYAMT,NVL(BCURR.StdPrecision,2))*-1 "
                          + " END      AS PAYMENTAMOUNT, "
                   //+ " BPG.NAME AS BPGROUP, "
                   //+ " IMG.AD_IMAGE_ID , "
                   + " BCURR.ISO_CODE AS BASECURRENCY,  "
                                      + "  CASE  "
                      + "  WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)  "
                       + " THEN  "
                         + " CASE "
                            + "  WHEN (DT.DOCBASETYPE='ARR') "
                            + "  THEN CURRENCYCONVERT(PAY.PAYAMT, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, PAY.DateAcct, PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) "
                            + "   WHEN (DT.DOCBASETYPE='APP') "
                            + "  THEN CURRENCYCONVERT(PAY.PAYAMT * -1, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, PAY.DateAcct, PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) "
                            + "  END "
                       + " ELSE "
                        + "  CASE "
                          + "  WHEN (DT.DOCBASETYPE='ARR') "
                        + "  THEN ROUND(PAY.PAYAMT,NVL(BCURR.StdPrecision,2)) "
                          + "  WHEN (DT.DOCBASETYPE='APP') "
                          + "  THEN ROUND(PAY.PAYAMT,NVL(BCURR.StdPrecision,2))*-1 "
                         + " END "
                     + " END AS CONVERTEDAMOUNT, "
                   + " CASE "
                   + " WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID) "
                   + " THEN 'Y' "
                   + " ELSE 'N' "
                   + " END AS ISCONVERTED, "
                   + " BSL.C_BANKSTATEMENTLINE_ID, "
                    + "  CASE WHEN dt.docbasetype IN ('APP')   THEN 'Payment'   WHEN dt.docbasetype IN ('ARR')   THEN 'Receipt'   END AS PaymentType ,"
                    + " BS.DocStatus AS DocStatus ,"
                   + " PAY.TrxNo , PM.VA009_Name, pay.DateAcct "
                    + " FROM C_PAYMENT PAY "
                   + " LEFT JOIN C_BPARTNER BP "
                   + " ON PAY.C_BPARTNER_ID =BP.C_BPARTNER_ID "

                   + " LEFT JOIN C_BANKSTATEMENTLINE BSL "
                   + " ON (PAY.C_PAYMENT_ID =BSL.C_PAYMENT_ID AND 'VO' <> (SELECT NVL(DocStatus, 'XX') FROM C_BANKSTATEMENT BST WHERE BST.C_BANKSTATEMENT_ID = BSL.C_BANKSTATEMENT_ID)) "

                   + " LEFT JOIN C_BANKSTATEMENT BS "
                   + " ON (BS.C_BANKSTATEMENT_ID =BSL.C_BANKSTATEMENT_ID) "


                   + " LEFT JOIN C_BP_GROUP BPG "
                   + " ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID "
                   + " LEFT JOIN C_CURRENCY CURR "
                   + " ON PAY.C_CURRENCY_ID =CURR.C_CURRENCY_ID "

                     + " INNER JOIN C_BANKACCOUNT AC  "
                      + " ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID  "
                   + " LEFT JOIN C_CURRENCY BCURR  "
                   + " ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID  "

                   + " LEFT JOIN VA009_PAYMENTMETHOD PM  "
                   + " ON (PM.VA009_PAYMENTMETHOD_ID   =PAY.VA009_PAYMENTMETHOD_ID ) "

                   + " INNER JOIN C_DOCTYPE DT "
                   + " ON DT.C_DOCTYPE_ID            =PAY.C_DOCTYPE_ID "

                   + " WHERE PAY.ISACTIVE   ='Y' AND PAY.DOCSTATUS IN ('CO','CL') AND (PM.VA009_PAYMENTBASETYPE !='B' OR PM.VA009_PAYMENTBASETYPE IS NULL) AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID()
               + " AND PAY.ISRECONCILED ='N' ";


                _sql += " AND PAY.C_BANKACCOUNT_ID= " + _accountID;
                //if ( _accountID > 0)
                //{
                //    _sql += " AND PAY.C_BANKACCOUNT_ID= " + _accountID;
                //}

                if (bankOrg_ID != 0)
                {
                    _sql += " AND PAY.AD_ORG_ID=" + bankOrg_ID;
                }
                if (_paymentMethodID > 0)
                {
                    _sql += " AND PAY.VA009_PAYMENTMETHOD_ID= " + _paymentMethodID;
                }
                //append statement date if it is not null
                if (statementDate != null)
                {
                    _sql += " AND PAY.dateacct <= " + GlobalVariable.TO_DATE(statementDate, true);
                }

                _sql += " ORDER BY PAY.DOCUMENTNO";



            }
            else if (_transactionType == "IS")
            {
                _sql = @" SELECT 
                              PAY.C_INVOICEPAYSCHEDULE_id AS C_PAYMENT_ID,
                              CURR.ISO_CODE               AS CURRENCY,
                              inv.DOCUMENTNO              AS PAYMENTNO,
                              inv.C_BPARTNER_ID ,
                              BP.NAME    AS BUSINESSPARTNER,
                                CASE
                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))*-1
                                  END      AS PAYMENTAMOUNT,
                              --BPG.NAME   AS BPGROUP,
                                INV.InvoiceReference AS InvoiceRef,
                              --IMG.AD_IMAGE_ID ,
                              BCURR.ISO_CODE AS BASECURRENCY,
                               CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                  CASE
                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                                    THEN CURRENCYCONVERT(PAY.DueAmt, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, INV.DateAcct, INV.C_ConversionType_ID, INV.AD_Client_ID, INV.AD_Org_ID)
                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                                    THEN CURRENCYCONVERT(PAY.DueAmt*-1, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, INV.DateAcct, INV.C_ConversionType_ID, INV.AD_Client_ID, INV.AD_Org_ID)
                                  END
                                ELSE
                                  CASE
                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))*-1
                                  END
                              END AS CONVERTEDAMOUNT,
                              CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN 'Y'
                                ELSE 'N'
                              END AS ISCONVERTED,
                        0 as C_BANKSTATEMENTLINE_ID, CASE WHEN dt.docbasetype IN ('API' , 'ARC')
                             THEN 'Payment'
                            WHEN dt.docbasetype IN ('ARI' , 'APC')
                            THEN 'Receipt'
                             END AS PaymentType,
                             'CO' AS DocStatus ,
                            ' ' as TrxNo , PM.VA009_Name, INV.DateAcct, PAY.DueDate
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON pay.C_INVOICE_id=inv.C_INVOICE_id
                            LEFT JOIN C_BPARTNER BP
                            ON inv.C_BPARTNER_ID =BP.C_BPARTNER_ID
                           /* --LEFT JOIN AD_IMAGE IMG
                            -- ON BP.PIC=IMG.AD_IMAGE_ID*/
                            LEFT JOIN C_BP_GROUP BPG
                            ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID
                            LEFT JOIN C_CURRENCY CURR
                            ON inv.C_CURRENCY_ID =CURR.C_CURRENCY_ID
                            /*--INNER JOIN AD_CLIENTINFO CINFO
                            --ON CINFO.AD_CLIENT_ID =PAY.AD_CLIENT_ID
                            --INNER JOIN C_ACCTSCHEMA AC
                            --ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID
                            --LEFT JOIN C_CURRENCY BCURR
                            --ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID*/
                            LEFT JOIN C_CURRENCY BCURR
                            ON " + bankCurr_ID + @" =BCURR.C_CURRENCY_ID

                            INNER JOIN VA009_PAYMENTMETHOD PM  
                            ON (PM.VA009_PAYMENTMETHOD_ID   =PAY.VA009_PAYMENTMETHOD_ID )
                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID            =INV.C_DOCTYPE_ID
                            WHERE  pay.VA009_IsPaid='N'
                            AND PAY.ISACTIVE      ='Y' AND INV.DOCSTATUS IN ('CO','CL') AND PM.VA009_PAYMENTBASETYPE!='B' AND PAY.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();



                if (bankOrg_ID != 0)
                {
                    _sql += " AND PAY.AD_ORG_ID=" + bankOrg_ID;
                }

                if (_paymentMethodID > 0)
                {
                    _sql += " AND PAY.VA009_PAYMENTMETHOD_ID= " + _paymentMethodID;
                }
                //append statement date if it is not null
                if (statementDate != null)
                {
                    _sql += " AND INV.dateacct <= " + GlobalVariable.TO_DATE(statementDate, true);
                }
                //                //Check Schedule already mapped to payment
                //                _sql += @" AND PAY.C_INVOICEPAYSCHEDULE_ID NOT IN (SELECT NVL(C_INVOICEPAYSCHEDULE_ID,0)
                //                            FROM
                //                              (SELECT PAY1.C_INVOICEPAYSCHEDULE_ID
                //                              FROM C_PAYMENT PAY1
                //                              WHERE PAY1.DOCSTATUS NOT IN ('VO','RE')
                //                              UNION
                //                              SELECT PA1.C_INVOICEPAYSCHEDULE_ID
                //                              FROM C_PAYMENTALLOCATE PA1
                //                              INNER JOIN C_PAYMENT PAY1
                //                              ON PAY1.C_PAYMENT_ID      =PA1.C_PAYMENT_ID
                //                              WHERE PAY1.DOCSTATUS NOT IN ('VO','RE')
                //                              )
                //                            )";
                //                ////
                _sql += " ORDER BY inv.DOCUMENTNO ";

            }
            else if (_transactionType == "PO")
            {
                _sql = @" SELECT ord.C_order_id AS C_PAYMENT_ID,
                          CURR.ISO_CODE       AS CURRENCY,
                          ord.DOCUMENTNO      AS PAYMENTNO,
                          ord.C_BPARTNER_ID ,
                          BP.NAME        AS BUSINESSPARTNER,
                          ROUND(ord.GrandTotal,NVL(BCURR.StdPrecision,2)) AS PAYMENTAMOUNT,
                          --BPG.NAME       AS BPGROUP,
                          --IMG.AD_IMAGE_ID ,
                          BCURR.ISO_CODE AS BASECURRENCY,
                          CASE
                            WHEN(ord.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                             THEN CURRENCYCONVERT(ord.GrandTotal, ord.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, ord.DateAcct, ord.C_ConversionType_ID, ord.AD_Client_ID, ord.AD_Org_ID) 
                            ELSE ROUND(ord.GrandTotal,NVL(BCURR.StdPrecision,2))
                          END AS CONVERTEDAMOUNT,
                          CASE
                            WHEN(ord.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                            THEN 'Y'
                            ELSE 'N'
                          END AS ISCONVERTED,
                        0 as C_BANKSTATEMENTLINE_ID,'Receipt' AS PaymentType,
                        'CO' AS DocStatus ,
                        ' ' as TrxNo , PM.VA009_Name, ORD.DateAcct
                        FROM C_ORDER ORD
                        LEFT JOIN C_DOCTYPE DT
                        ON ORD.C_DocTypeTarget_ID=dt.C_DocType_ID
                        LEFT JOIN C_BPARTNER BP
                        ON ord.C_BPARTNER_ID =BP.C_BPARTNER_ID
                        --LEFT JOIN AD_IMAGE IMG
                        --ON BP.PIC=IMG.AD_IMAGE_ID
                        LEFT JOIN C_BP_GROUP BPG
                        ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID
                        LEFT JOIN C_CURRENCY CURR
                        ON ord.C_CURRENCY_ID =CURR.C_CURRENCY_ID
                        LEFT JOIN C_CURRENCY BCURR
                        ON " + bankCurr_ID + @" =BCURR.C_CURRENCY_ID
                     
                        INNER JOIN VA009_PAYMENTMETHOD PM  
                        ON (PM.VA009_PAYMENTMETHOD_ID   =ORD.VA009_PAYMENTMETHOD_ID )


                        WHERE dt.DocSubTypeSO='PR'
                        AND ORD.DOCSTATUS    ='WP'
                            AND ORD.ISACTIVE      ='Y' AND PM.VA009_PAYMENTBASETYPE!='B' AND ORD.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
                if (bankOrg_ID != 0)
                {
                    _sql += " AND ORD.AD_ORG_ID=" + bankOrg_ID;
                }

                if (_paymentMethodID > 0)
                {
                    _sql += " AND ORD.VA009_PAYMENTMETHOD_ID= " + _paymentMethodID;
                }
                //append statement date if it is not null
                if (statementDate != null)
                {
                    _sql += " AND ORD.dateacct <= " + GlobalVariable.TO_DATE(statementDate, true);
                }
                _sql += " ORDER BY ord.DOCUMENTNO";

            }
            else if (_transactionType == "CO")
            {
                _sql = @" SELECT CSL.C_CASHLINE_ID AS C_PAYMENT_ID,
                            CURR.ISO_CODE          AS CURRENCY,
                            CS.DOCUMENTNO          AS PAYMENTNO,
                            csl.C_BPARTNER_ID ,
                              CASE
                            WHEN csl.C_BPARTNER_ID IS NOT NULL
                            THEN BP.NAME
                            ELSE cs.NAME
                          END                                         AS BUSINESSPARTNER,
                            ROUND(CSL.AMOUNT * -1,NVL(BCURR.StdPrecision,2)) AS PAYMENTAMOUNT,
                            --BPG.NAME                                    AS BPGROUP,
                            --IMG.AD_IMAGE_ID ,
                            BCURR.ISO_CODE AS BASECURRENCY,
                            CASE
                            WHEN(CSL.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                            THEN CURRENCYCONVERT(CSL.AMOUNT * -1, CSL.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, CS.DateAcct, CSL.C_ConversionType_ID, CS.AD_Client_ID, CS.AD_Org_ID) 
                            ELSE ROUND(CSL.AMOUNT * -1 ,NVL(BCURR.StdPrecision,2))
                            END AS CONVERTEDAMOUNT,
                            CASE
                            WHEN(csl.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                            THEN 'Y'
                            ELSE 'N'
                            END AS ISCONVERTED,
                            BSL.C_BANKSTATEMENTLINE_ID,
                            CASE csl.VSS_PAYMENTTYPE
                            WHEN 'P'
                            THEN 'Receipt'
                            WHEN 'R'
                            THEN 'Payment'
                            END AS PaymentType,
                            BS.DocStatus AS DocStatus ,
                              ' '  as TrxNo , '' as VA009_Name, CS.DateAcct
                        FROM C_CASH CS
                        INNER JOIN C_CASHLINE CSL
                        ON CS.C_CASH_ID=CSL.C_CASH_ID
                        INNER JOIN c_charge chrg
                        ON chrg.c_charge_id=csl.c_charge_id
                        LEFT JOIN C_BPARTNER BP
                        ON CSL.C_BPARTNER_ID =BP.C_BPARTNER_ID

                         LEFT JOIN C_BANKSTATEMENTLINE BSL 
                         ON (CSL.C_CASHLINE_ID =BSL.C_CASHLINE_ID AND 'VO' <> (SELECT NVL(DocStatus, 'XX') FROM C_BANKSTATEMENT BST WHERE BST.C_BANKSTATEMENT_ID = BSL.C_BANKSTATEMENT_ID))

                         LEFT JOIN C_BANKSTATEMENT BS 
                         ON (BS.C_BANKSTATEMENT_ID =BSL.C_BANKSTATEMENT_ID)

                        --LEFT JOIN AD_IMAGE IMG
                        --ON BP.PIC=IMG.AD_IMAGE_ID
                        LEFT JOIN C_BP_GROUP BPG
                        ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID
                        LEFT JOIN C_CURRENCY CURR
                        ON CSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID
                        LEFT JOIN C_CURRENCY BCURR
                        ON " + bankCurr_ID + @" =BCURR.C_CURRENCY_ID
                       
                     WHERE CS.ISACTIVE   ='Y' 
                        AND CSL.CashType           ='C'
                        AND chrg.dtd001_chargetype ='CON'                        
                        AND CS.DOCSTATUS IN ('CO','CL') AND CS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + @"
                    AND CSL.VA012_ISRECONCILED ='N' AND CSL.C_BANKACCOUNT_ID= " + _accountID;

                if (bankOrg_ID != 0)
                {
                    _sql += " AND CS.AD_ORG_ID=" + bankOrg_ID;
                }
                //append statement date if it is not null
                if (statementDate != null)
                {
                    _sql += " AND CS.dateacct <= " + GlobalVariable.TO_DATE(statementDate, true);
                }
                _sql += " ORDER BY CS.DOCUMENTNO";
            }

            List<PaymentProp> _payments = new List<PaymentProp>();
            PaymentProp _payment = new PaymentProp();
            DataSet _ds = new DataSet();
            try
            {
                _ds = VIS.DBase.DB.ExecuteDatasetPaging(_sql, _paymentPageNo, _PAGESIZE);
                if (_ds != null)
                {
                    for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                    {
                        _payment = new PaymentProp();
                        _payment.c_bankstatementline_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]);
                        _payment.c_payment_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_PAYMENT_ID"]);
                        _payment.currency = Util.GetValueOfString(_ds.Tables[0].Rows[i]["CURRENCY"]);
                        _payment.paymentno = Util.GetValueOfString(_ds.Tables[0].Rows[i]["PAYMENTNO"]);
                        _payment.c_bpartner_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BPARTNER_ID"]);
                        _payment.businesspartner = Util.GetValueOfString(_ds.Tables[0].Rows[i]["BUSINESSPARTNER"]);
                        _payment.paymentamount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["PAYMENTAMOUNT"]);
                        if (_transactionType.Equals("IS"))
                        {
                            _payment.bpgroup = Util.GetValueOfString(_ds.Tables[0].Rows[i]["InvoiceRef"]);
                        }
                        _payment.basecurrency = Util.GetValueOfString(_ds.Tables[0].Rows[i]["BASECURRENCY"]);
                        _payment.convertedamount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["CONVERTEDAMOUNT"]);
                        _payment.isconverted = Util.GetValueOfString(_ds.Tables[0].Rows[i]["ISCONVERTED"]);
                        /* change by pratap */
                        _payment.paymenttype = Util.GetValueOfString(_ds.Tables[0].Rows[i]["PaymentType"]);
                        /* end change by pratap */
                        //added new column to show data on Bank Statement Form
                        _payment.PaymentMethod = Util.GetValueOfString(_ds.Tables[0].Rows[i]["VA009_Name"]);
                        //_payment.DateAcct = Util.GetValueOfString(Convert.ToDateTime(_ds.Tables[0].Rows[i]["DateAcct"]).ToShortDateString());
                        _payment.DateAcct = Util.GetValueOfDateTime(_ds.Tables[0].Rows[i]["DateAcct"]);
                        //end
                        /* change by pratap */
                        _payment.docstatus = Util.GetValueOfString(_ds.Tables[0].Rows[i]["DocStatus"]);
                        /* end change by pratap */

                        if (_CountVA034 > 0 && string.IsNullOrEmpty((_ds.Tables[0].Rows[i]["TrxNo"]).ToString()))
                        {
                            _payment.depositslipno = Util.GetValueOfString(_ds.Tables[0].Rows[i]["DepositSlipNo"]);
                        }

                        _payment.authcode = Util.GetValueOfString(_ds.Tables[0].Rows[i]["TrxNo"]);

                        //for Invoice Schedule get DueDate and Due Amount
                        if (Util.GetValueOfString(_transactionType).Equals("IS"))
                        {
                            _payment.DueDate = Util.GetValueOfDateTime(_ds.Tables[0].Rows[i]["DueDate"]);
                            _payment.DueAmt = DisplayType.GetNumberFormat(DisplayType.Amount).GetFormatAmount(_ds.Tables[0].Rows[i]["PAYMENTAMOUNT"], ctx.GetContext("#ClientLanguage"));
                        }

                        //if (_ds.Tables[0].Rows[i]["AD_IMAGE_ID"] != DBNull.Value && _ds.Tables[0].Rows[i]["AD_IMAGE_ID"] != null && Util.GetValueOfInt(_ds.Tables[0].Rows[i]["AD_IMAGE_ID"]) > 0)
                        //{
                        //    MImage _image = new MImage(ctx, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["AD_IMAGE_ID"]), null);
                        //    _payment.imageurl = _image.GetThumbnailURL(46, 46);
                        //    //_payment.binarydata = Convert.ToBase64String(_image.GetThumbnailByte(46, 46));

                        //    if (_payment.imageurl == "FileDoesn'tExist" || _payment.imageurl == "NoRecordFound")
                        //    {
                        //        _payment.imageurl = "";
                        //    }                            
                        //}
                        //else
                        //{
                        //    _payment.imageurl = "";
                        //}
                        _payments.Add(_payment);
                    }

                }
                if (_ds != null)
                {
                    _ds.Dispose();
                }

            }
            catch (Exception e)
            {
                if (_ds != null)
                {
                    _ds.Dispose();
                }
            }
            return _payments;
        }
        public List<StatementLineProp> LoadStatements(Ctx ctx, int _cmbBankAccount, int _statementPageNo, int _PAGESIZE, bool _SEARCHREQUEST, string _txtSearch)
        {
            string _sql = "";


            _sql = "SELECT BS.NAME    AS STATEMENTNO, "
                                + " CASE  "
                    + " WHEN (BSL.C_BPARTNER_ID IS NOT NULL  "
                    + " AND BSL.C_BPARTNER_ID    >0)  "
                     + " THEN BP.NAME  "
                    + " WHEN (BSL.DESCRIPTION IS NOT NULL)  "
                    + " THEN BSL.DESCRIPTION  "
                    //+ " WHEN (BSL.C_CHARGE_ID IS NOT NULL  "
                    //+ " AND BSL.C_CHARGE_ID    >0)  "
                    //+ " THEN CHRG.NAME  "

                    + " WHEN (BSL.C_PAYMENT_ID IS NOT NULL  "
                    + " AND BSL.C_PAYMENT_ID    >0)  "
                    + " THEN PAY.DOCUMENTNO  "

                    + " WHEN (BSL.C_CASHLINE_ID IS NOT NULL  "
                    + " AND BSL.C_CASHLINE_ID    >0)  "
                    + " THEN CAST('" + Msg.GetMsg(ctx, "VA012_CashToBank") + "' AS NVARCHAR2(50))  "
                    + " ELSE CAST('  -  ' AS NVARCHAR2(50))"

                  + " END AS DESCRIPTION,  "
               //   + " CASE "
               //   + " WHEN BPG.NAME IS NOT NULL "
               //   + " THEN BPG.NAME "
               //  + " ELSE CAST(' ' AS NVARCHAR2(50)) "
               //+ " END AS BPGROUP, "
               + " ROUND(BSL.StmtAmt,NVL(CURR.StdPrecision,2))      AS STMTAMT, "
                + " ROUND(BSL.TRXAMT,NVL(CURR.StdPrecision,2)) + ROUND(BSL.ChargeAmt,NVL(CURR.StdPrecision,2))       AS TRXAMOUNT, "
               + " CURR.ISO_CODE   AS CURRENCY, "
               + " BSL.C_BANKSTATEMENTLINE_ID, "
               + " BSL.C_PAYMENT_ID, "
               //+ " IMG.AD_IMAGE_ID, "
               + " BSL.LINE, "
                + " BSL.VA012_PAGE AS PAGE,"
                + " BSL.VA012_ISUSENEXTTIME, "
                + " BSL.C_CHARGE_ID ,"
                + " BCURR.ISO_CODE AS BASECURRENCY,  "
                + " CASE  "
                + " WHEN(BSL.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID) "
                + " THEN ROUND(BSL.StmtAmt*( "
                  + " CASE "
                   + "  WHEN CCR.MULTIPLYRATE IS NOT NULL "
                   + "  THEN CCR.MULTIPLYRATE "
                    + " ELSE CCR1.DIVIDERATE "
                 + "  END),NVL(CURR.StdPrecision,2)) "
                + " ELSE ROUND(BSL.StmtAmt,NVL(CURR.StdPrecision,2))  "
                + " END AS CONVERTEDAMOUNT, "
                + " CASE "
                + " WHEN(BSL.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID) "
                + " THEN 'Y' "
                + " ELSE 'N' "
                + " END AS ISCONVERTED, "
                + "  INV.DOCUMENTNO AS INVOICENO,BS.C_BANKSTATEMENT_ID,BS.DOCSTATUS,BSL.C_CASHLINE_ID, BSL.TRXNO, BSL.StatementLineDate "
               + " FROM C_BANKSTATEMENT BS "
               + " INNER JOIN C_BANKSTATEMENTLINE BSL "
               + " ON BS.C_BANKSTATEMENT_ID=BSL.C_BANKSTATEMENT_ID "
               + " LEFT JOIN C_INVOICE INV "
               + " ON BSL.C_INVOICE_ID = INV.C_INVOICE_ID "
               + "  LEFT JOIN C_BPARTNER BP "
               + "  ON BSL.C_BPARTNER_ID =BP.C_BPARTNER_ID "
               + " LEFT JOIN C_CHARGE CHRG "
               + " ON BSL.C_CHARGE_ID=CHRG.C_CHARGE_ID "

                + " LEFT JOIN C_PAYMENT PAY "
               + " ON BSL.C_PAYMENT_ID=PAY.C_PAYMENT_ID "

              //+ " LEFT JOIN AD_IMAGE IMG "
              //+ " ON BP.PIC=IMG.AD_IMAGE_ID "
              + "  LEFT JOIN C_BP_GROUP BPG "
              + " ON BP.C_BP_GROUP_ID=BPG.C_BP_GROUP_ID "
              + "  LEFT JOIN C_CURRENCY CURR "
              + "  ON BSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID "
                + " INNER JOIN AD_CLIENTINFO CINFO  "
                + " ON CINFO.AD_CLIENT_ID =BSL.AD_CLIENT_ID  "
                + " INNER JOIN C_ACCTSCHEMA AC  "
                + " ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID  "
                + " LEFT JOIN C_CURRENCY BCURR  "
                + " ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID  "
                + " LEFT JOIN C_CONVERSION_RATE CCR  "
                + " ON (CCR.C_CURRENCY_ID   =BSL.C_CURRENCY_ID  "
                + " AND CCR.ISACTIVE        ='Y'  "
                + " AND CCR.C_CURRENCY_TO_ID=AC.C_CURRENCY_ID  "
                 + "  AND CCR.AD_CLIENT_ID    =BSL.AD_CLIENT_ID "
                 + "  AND CCR.AD_ORG_ID      IN (BSL.AD_ORG_ID,0)  "
                + " AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)  "

                + " LEFT JOIN C_CONVERSION_RATE CCR1 "
                + " ON (CCR1.C_CURRENCY_ID   =AC.C_CURRENCY_ID "
                + " AND CCR1.C_CURRENCY_TO_ID=BSL.C_CURRENCY_ID "
                + " AND CCR1.ISACTIVE        ='Y' "
                  + "  AND CCR1.AD_CLIENT_ID    =BSL.AD_CLIENT_ID "
                 + "  AND CCR1.AD_ORG_ID      IN (BSL.AD_ORG_ID,0)  "
                + " AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO) "

              + " WHERE BS.ISACTIVE='Y' AND BS.C_BANKACCOUNT_ID= " + _cmbBankAccount + " AND BS.DOCSTATUS NOT IN ( 'VO','CO') AND BS.AD_CLIENT_ID=" + ctx.GetAD_Client_ID();
            // + "  WHERE BS.C_BANKSTATEMENT_ID IN (" + _statementID + ")";

            int org_Id = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Org_ID FROM C_BankAccount WHERE IsActive='Y' AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + " AND C_BankAccount_ID=" + _cmbBankAccount, null, null));
            if (org_Id > 0)
            {
                _sql += " AND BS.AD_ORG_ID=" + org_Id;
            }

            if (_SEARCHREQUEST)
            {
                _sql += " AND (UPPER(BP.NAME) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BSL.DESCRIPTION) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BS.NAME) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BSL.StmtAmt) LIKE UPPER('%" + _txtSearch + "%')"
                        + " OR UPPER(BSL.TrxAmt) LIKE UPPER('%" + _txtSearch + "%') "
                        + " OR UPPER(BSL.TRXNO) LIKE UPPER('%" + _txtSearch + "%'))";
            }

            //_sql += " ORDER BY BSL.StatementLineDate DESC, TO_NUMBER(REGEXP_SUBSTR(BS.NAME, '\\d+')) DESC , BSL.VA012_PAGE DESC , BSL.LINE DESC";
            _sql += " ORDER BY ( CASE  WHEN BS.DOCSTATUS='DR' THEN 1 ELSE 0 END) DESC, TO_NUMBER(REGEXP_SUBSTR(BS.NAME, '\\d+'), '999999999999') DESC , BSL.VA012_PAGE DESC , BSL.LINE DESC";
            List<StatementLineProp> _statements = new List<StatementLineProp>();
            StatementLineProp _statement = new StatementLineProp();
            DataSet _ds = new DataSet();
            try
            {


                _ds = VIS.DBase.DB.ExecuteDatasetPaging(_sql, _statementPageNo, _PAGESIZE);

                //_ds = DB.ExecuteDataset(_sql);
                if (_ds != null)
                {
                    for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                    {
                        _statement = new StatementLineProp();
                        _statement.usenexttime = Util.GetValueOfString(_ds.Tables[0].Rows[i]["VA012_ISUSENEXTTIME"]) == "Y" ? true : false;
                        _statement.c_charge_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                        _statement.page = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["PAGE"]);
                        _statement.line = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["LINE"]);
                        _statement.c_payment_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_PAYMENT_ID"]);
                        _statement.currency = Util.GetValueOfString(_ds.Tables[0].Rows[i]["CURRENCY"]);
                        _statement.statementno = Util.GetValueOfString(_ds.Tables[0].Rows[i]["STATEMENTNO"]);
                        _statement.c_bankstatementline_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]);
                        _statement.c_bankstatement_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BANKSTATEMENT_ID"]);
                        _statement.description = Util.GetValueOfString(_ds.Tables[0].Rows[i]["DESCRIPTION"]);
                        _statement.trxamount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["TRXAMOUNT"]);
                        _statement.STMTAMT = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["STMTAMT"]);
                        //if charge id is there and Statement amount is Equal to Charge Amount then make it Matching Confirmed.
                        if (Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CHARGE_ID"]) > 0)
                        {
                            if (Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["TRXAMOUNT"]).Equals(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["STMTAMT"])))
                            {
                                DB.ExecuteScalar(" UPDATE C_BANKSTATEMENTLINE SET VA012_ISMATCHINGCONFIRMED='Y' WHERE C_BANKSTATEMENTLINE_ID = " + Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_BANKSTATEMENTLINE_ID"]));
                            }
                        }
                        //not required 
                        //_statement.bpgroup = Util.GetValueOfString(_ds.Tables[0].Rows[i]["BPGROUP"]);
                        _statement.docstatus = Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCSTATUS"]);
                        _statement.basecurrency = Util.GetValueOfString(_ds.Tables[0].Rows[i]["BASECURRENCY"]);
                        _statement.convertedamount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["CONVERTEDAMOUNT"]);
                        _statement.isconverted = Util.GetValueOfString(_ds.Tables[0].Rows[i]["ISCONVERTED"]);
                        _statement.invoiceno = Util.GetValueOfString(_ds.Tables[0].Rows[i]["INVOICENO"]);
                        _statement.c_cashline_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_CASHLINE_ID"]);
                        _statement.trxno = Util.GetValueOfString(_ds.Tables[0].Rows[i]["TRXNO"]);
                        //get Statement Line Date from the Statement Line
                        _statement.stmtLineDate = Util.GetValueOfDateTime(_ds.Tables[0].Rows[i]["StatementLineDate"]);
                        //if (_ds.Tables[0].Rows[i]["AD_IMAGE_ID"] != DBNull.Value && _ds.Tables[0].Rows[i]["AD_IMAGE_ID"] != null && Util.GetValueOfInt(_ds.Tables[0].Rows[i]["AD_IMAGE_ID"]) > 0)
                        //{
                        //    MImage _image = new MImage(ctx, Util.GetValueOfInt(_ds.Tables[0].Rows[i]["AD_IMAGE_ID"]), null);
                        //    _statement.imageurl = _image.GetThumbnailURL(46, 46);                            

                        //    if (_statement.imageurl == "FileDoesn'tExist" || _statement.imageurl == "NoRecordFound")
                        //    {
                        //        _statement.imageurl = "";
                        //    }                           
                        //}
                        //else
                        //{
                        //    _statement.imageurl = "";
                        //}
                        _statements.Add(_statement);
                    }

                }

                if (_ds != null)
                {
                    _ds.Dispose();
                }

            }
            catch
            {
                if (_ds != null)
                {
                    _ds.Dispose();
                }
            }
            return _statements;
        }
        //public string CreatePayment(Ctx ctx, int _bankStatementLineID)
        //{
        //    CreatePaymentFromStatement _obj = new CreatePaymentFromStatement();
        //    return _obj.CreatePayment(new MBankStatementLine(ctx, _bankStatementLineID, null), ctx);
        //    //VAdvantage.Process.BankStatementPayment _obj = new VAdvantage.Process.BankStatementPayment();
        //    ////return _obj.CreatePayment(new MBankStatementLine(ctx, _bankStatementLineID, null));
        //}
        public List<ChargeProp> GetCharge(Ctx ctx, string searchText)
        {
            List<ChargeProp> _lstcharge = new List<ChargeProp>();
            //var _sql = "SELECT NAME,C_CHARGE_ID FROM C_CHARGE WHERE ISACTIVE='Y' AND AD_CLIENT_ID=" + ctx.GetAD_Client_ID() + " AND AD_ORG_ID=" + ctx.GetAD_Org_ID() + " AND UPPER(Name) like UPPER('%" + searchText + "%')";
            var _sql = "SELECT Name,C_Charge_ID FROM C_Charge WHERE IsActive='Y' AND UPPER(Name) LIKE UPPER('%" + searchText + "%')";
            _sql = MRole.GetDefault(ctx).AddAccessSQL(_sql, "C_Charge", MRole.SQL_FULLYQUALIFIED, MRole.SQL_RO);
            DataSet ds = DB.ExecuteDataset(_sql);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    ChargeProp _chrg = new ChargeProp();
                    _chrg.chargeID = Util.GetValueOfInt(ds.Tables[0].Rows[i]["C_CHARGE_ID"]);
                    _chrg.name = Util.GetValueOfString(ds.Tables[0].Rows[i]["NAME"]);
                    _lstcharge.Add(_chrg);
                }
            }
            return _lstcharge;
        }

        /// <summary>
        /// Get DocType_ID
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_amount">Amount</param>
        /// <param name="org_Id">AD_Org_ID</param>
        /// <returns></returns>
        public int GetDocTypeID(Ctx ctx, decimal _amount, int org_Id)
        {
            string _docBaseType = string.Empty;
            if (_amount > 0)
            {
                _docBaseType = "ARR";
            }
            else if (_amount < 0)
            {
                _docBaseType = "APP";
            }
            //Used Order By clause and DESC command to get the C_DocType_ID for non zero AD_Org_ID
            return Util.GetValueOfInt(DB.ExecuteScalar("Select  dt.c_doctype_id  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='" + _docBaseType + "' AND dt.IsActive = 'Y' AND (DT.ad_org_id = " + org_Id + " or  DT.ad_org_id = 0) AND DT.AD_Client_ID = " + ctx.GetAD_Client_ID() + " ORDER BY DT.AD_Org_ID DESC"));
        }

        /// <summary>
        /// Get DocType_ID
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="docBaseType">C_DocBaseType</param>
        /// <param name="org_Id">AD_Org_ID</param>
        /// <returns></returns>
        /*Created by pratap */
        public int GetDocTypeID(Ctx ctx, string docBaseType, int org_Id)
        {
            if (docBaseType == "API" || docBaseType == "APC")
            {
                docBaseType = "APP";
            }
            else if (docBaseType == "ARI" || docBaseType == "ARC")
            {
                docBaseType = "ARR";
            }
            //Used Order By clause and DESC command to get the C_DocType_ID for non zero AD_Org_ID
            return Util.GetValueOfInt(DB.ExecuteScalar("Select  MIN(dt.c_doctype_id)  From C_doctype DT inner join c_docbasetype DBT On dt.docbasetype=dbt.docbasetype where dbt.docbasetype='" + docBaseType + "' AND dt.IsActive = 'Y' AND (DT.ad_org_id = " + org_Id + " or  DT.ad_org_id = 0) AND DT.AD_Client_ID = " + ctx.GetAD_Client_ID() + " ORDER BY DT.AD_Org_ID DESC"));
        }
        public int GetCurrencyType()
        {
            return Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CONVERSIONTYPE_ID FROM C_CONVERSIONTYPE WHERE VALUE='S'"));
        }

        /// <summary>
        /// Create the Payment aganist InvoicePaySchedule's
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_formData">List fo Form Data</param>
        /// <param name="_trx">Transaction</param>
        /// <returns>either DocNo or Error Msg(string type Value)</returns>
        public string CreatePaymentFromSchedule(Ctx ctx, List<StatementProp> _formData, Trx _trx)
        {
            //Get Transaction
            //Trx trx = Trx.GetTrx("Payment_" + DateTime.Now.ToString("yyMMddHHmmssff"));
            string ex = "";
            string docno = "";
            string processMsg = "";

            string _sql = "";
            DataSet _ds = new DataSet();
            try
            {

                _sql = @"SELECT COUNT(*)
                            FROM
                              (SELECT PAY.C_INVOICEPAYSCHEDULE_ID FROM C_PAYMENT PAY WHERE PAY.DOCSTATUS NOT IN ('VO','RE')
                              UNION
                              SELECT PA.C_INVOICEPAYSCHEDULE_ID FROM C_PAYMENTALLOCATE PA INNER JOIN C_PAYMENT PAY ON PAY.C_PAYMENT_ID      =PA.C_PAYMENT_ID WHERE PAY.DOCSTATUS NOT IN ('VO','RE')
                              )t
                            WHERE C_INVOICEPAYSCHEDULE_ID IN(" + _formData[0]._scheduleList + ")";

                if (Util.GetValueOfInt(DB.ExecuteScalar(_sql)) > 0)
                {
                    //closing transaction
                    _trx.Rollback();
                    return "VA012_PaymentAlreadyCreated";
                }
                _sql = "";
                //_formData[0]._txtConversionType - C_ConversionType_ID Selected on the form.
                //_formData[0]._txtCurrency - C_Currency_ID selected on the form.
                _sql = @"SELECT CASE
                                    WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                    THEN CURRENCYCONVERT(PAY.DueAmt, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(_formData[0]._dtStatementDate, true) +
                                            @", "+ _formData[0]._txtConversionType + @", INV.AD_Client_ID, "+ _formData[0]._bankAcctOrg_ID + @")
                                    ELSE ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2)) END AS AMOUNT,
                                 CASE
                                    WHEN(INV.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                    THEN CURRENCYCONVERT(inv.GrandTotal, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(_formData[0]._dtStatementDate, true) +
                                            @", " + _formData[0]._txtConversionType + @", INV.AD_Client_ID, " + _formData[0]._bankAcctOrg_ID + @")
                                    ELSE ROUND(inv.GrandTotal,NVL(BCURR.STDPRECISION,2)) END AS GrandTotal,

                                    PAY.C_INVOICE_ID,PAY.VA009_PAYMENTMETHOD_ID,PAY.C_INVOICEPAYSCHEDULE_ID,
                               PAY.AD_ORG_ID,PAY.AD_CLIENT_ID, dt.DOCBASETYPE, INV.C_BPartner_Location_ID
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                            INNER JOIN C_Doctype dt
                            ON dt.C_Doctype_id = INV.c_doctype_id
                            LEFT JOIN C_CURRENCY BCURR
                            ON " + _formData[0]._txtCurrency + @" =BCURR.C_CURRENCY_ID 
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _formData[0]._scheduleList + ")";
                // Trx trx = Trx.Get("VA012_PaymentCreate" + System.DateTime.Now.Ticks);
                _ds = DB.ExecuteDataset(_sql.ToString(), null, _trx);
                if (_ds != null)
                {
                    //variables
                    decimal _txtAmount = 0;
                    decimal _txtTrxAmt = 0;
                    decimal _txtDifference = 0;

                    //_formData[0]._txtCurrency - selected Currency on the form either Invoice or Bank Currency
                    //_formData[0]._cmbCurrency - Bank Currency
                    //_formData[0]._txtConversionType - COnversionType selected on the form.
                    //get the Converted Amt from Bank Currency to Selected Currency and ConversionType
                    //to create the Payment based on Selected Currency and ConversionType on the form.
                    if (_formData[0]._txtCurrency != _formData[0]._cmbCurrency)
                    {
                        _txtAmount = MConversionRate.Convert(ctx, _formData[0]._txtAmount, _formData[0]._cmbCurrency, _formData[0]._txtCurrency, _formData[0]._dtStatementDate, _formData[0]._txtConversionType, ctx.GetAD_Client_ID(), _formData[0]._bankAcctOrg_ID);
                        _txtTrxAmt = MConversionRate.Convert(ctx, _formData[0]._txtTrxAmt, _formData[0]._cmbCurrency, _formData[0]._txtCurrency, _formData[0]._dtStatementDate, _formData[0]._txtConversionType, ctx.GetAD_Client_ID(), _formData[0]._bankAcctOrg_ID);
                        _txtDifference = MConversionRate.Convert(ctx, _formData[0]._txtDifference, _formData[0]._cmbCurrency, _formData[0]._txtCurrency, _formData[0]._dtStatementDate, _formData[0]._txtConversionType, ctx.GetAD_Client_ID(), _formData[0]._bankAcctOrg_ID);
                    }
                    else
                    {
                        _txtAmount = _formData[0]._txtAmount;
                        _txtTrxAmt = _formData[0]._txtTrxAmt;
                        _txtDifference = _formData[0]._txtDifference;
                    }
                    #region under Payment
                    if (_ds.Tables[0].Rows.Count == 1)
                    {
                        decimal differenceAmount = 0;
                        MPayment _pay = new MPayment(ctx, 0, _trx);
                        /*chnage by pratap*/
                        //int C_Doctype_ID = GetDocTypeID(ctx, _formData[0]._txtAmount);
                        int C_Doctype_ID = GetDocTypeID(ctx, Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]), _formData[0]._bankAcctOrg_ID);
                        /*end change by pratap*/
                        _pay.SetC_DocType_ID(C_Doctype_ID);
                        //Payment AcctDate & Trx date should be StatementLine AcctDate
                        //_pay.SetDateAcct(System.DateTime.Now);
                        _pay.SetDateAcct(_formData[0]._dtStatementDate);
                        //_pay.SetDateTrx(System.DateTime.Now);
                        _pay.SetDateTrx(_formData[0]._dtStatementDate);
                        _pay.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                        _pay.SetC_BankAccount_ID(Util.GetValueOfInt(_formData[0]._cmbBankAccount));
                        _pay.SetC_BPartner_ID(Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner));
                        //added BPartner_Location_ID from Invoice Reference
                        _pay.SetC_BPartner_Location_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPartner_Location_ID"]));
                        _pay.SetC_Currency_ID(Util.GetValueOfInt(_formData[0]._txtCurrency));  //Set the Currency which selected on new form
                        _pay.SetC_ConversionType_ID(_formData[0]._txtConversionType); //Set the Currency ConversionType which selected on new form
                        /*chnage by pratap*/
                        //_pay.SetPayAmt(Math.Abs(_formData[0]._txtAmount));

                        if (Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]) == "API" || Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]) == "ARI")
                        {
                            _pay.SetPayAmt(Math.Abs(_txtAmount));
                        }
                        else
                        {
                            _pay.SetPayAmt(-1 * Math.Abs(_txtAmount));
                        }
                        /*end change by pratap*/

                        //uncomment this
                        _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[0]["VA009_PAYMENTMETHOD_ID"]));




                        #region OverUnder

                        if (Util.GetValueOfDecimal(_pay.GetPayAmt()) >= 0)
                        {
                            if (Math.Abs(_txtTrxAmt) > _pay.GetPayAmt())
                            {
                                differenceAmount = Math.Abs(_txtDifference);
                            }
                            else if (Math.Abs(_txtTrxAmt) < _pay.GetPayAmt())
                            {
                                differenceAmount = Math.Abs(_txtDifference) * -1;
                            }
                        }
                        else
                        {
                            if (Math.Abs(_txtTrxAmt) > Math.Abs(Util.GetValueOfDecimal(_pay.GetPayAmt())))
                            {
                                differenceAmount = Math.Abs(_txtDifference) * -1;
                            }
                            else if (Math.Abs(_txtTrxAmt) < Math.Abs(Util.GetValueOfDecimal(_pay.GetPayAmt())))
                            {
                                differenceAmount = Math.Abs(_txtDifference);

                            }

                        }
                        if (differenceAmount != 0 && _formData[0]._cmbVoucherMatch == "M")
                        {
                            //DiscountAmount
                            if (_formData[0]._cmbDifferenceType == "DA")
                            {
                                _pay.SetDiscountAmt(differenceAmount);
                            }
                            //OverUnderPayment
                            else if (_formData[0]._cmbDifferenceType == "OU")
                            {
                                _pay.SetOverUnderAmt(differenceAmount);
                            }
                            //WriteoffAmount
                            else if (_formData[0]._cmbDifferenceType == "WO")
                            {
                                _pay.SetWriteOffAmt(differenceAmount);
                            }
                        }
                        #endregion OverUnder


                        _pay.SetC_Invoice_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_INVOICE_ID"]));
                        _pay.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_INVOICEPAYSCHEDULE_ID"]));




                        if (!_pay.Save())
                        {
                            _trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            //some times getting the error pp also
                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                            if (string.IsNullOrEmpty(error))
                            {
                                error = pp != null ? pp.GetName() : "";
                            }
                            return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotSaved";
                        }
                        else
                        {
                            //Commit Transaction
                            _trx.Commit();
                            string _msg = CompletePayment(ctx, _pay.GetC_Payment_ID(), 149, MPayment.DOCACTION_Complete);
                            if (string.IsNullOrEmpty(_msg))
                            {
                                return _pay.GetC_Payment_ID().ToString();
                            }
                            else {
                                //if Payment is not Completed then delete the record
                                if (!_pay.Delete(true, _trx))
                                {
                                    _trx.Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    //some times getting the error pp also
                                    string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        error = pp != null ? pp.GetName() : "";
                                    }
                                    return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotDeleted";
                                }
                                else
                                {
                                    //commit the transaction
                                    _trx.Commit();
                                    return _msg;
                                }
                            }
                        }
                    }
                    else if (_ds.Tables[0].Rows.Count > 1)
                    {
                        //int _paymentMethodID = 0;
                        //_paymentMethodID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT VA009_PAYMENTMETHOD_ID FROM C_BPARTNER WHERE C_BPARTNER_ID=" + Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner)));
                        MPayment _pay = new MPayment(ctx, 0, _trx);


                        //int C_Doctype_ID = GetDocTypeID(ctx, _formData[0]._txtAmount);
                        /*chnage by pratap*/
                        int C_Doctype_ID = GetDocTypeID(ctx, Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]), _formData[0]._bankAcctOrg_ID);
                        /*end change by pratap*/

                        _pay.SetC_DocType_ID(C_Doctype_ID);
                        //Payment AcctDate & Trx Date should be StatementLine AcctDate
                        //_pay.SetDateAcct(System.DateTime.Now);
                        _pay.SetDateAcct(_formData[0]._dtStatementDate);
                        //_pay.SetDateTrx(System.DateTime.Now);
                        _pay.SetDateTrx(_formData[0]._dtStatementDate);
                        //set the Organization from the backaccount
                        _pay.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                        _pay.SetC_BankAccount_ID(Util.GetValueOfInt(_formData[0]._cmbBankAccount));
                        _pay.SetC_BPartner_ID(Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner));
                        //added BPartner_Location_ID from Invoice Reference
                        _pay.SetC_BPartner_Location_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPartner_Location_ID"]));
                        _pay.SetC_Currency_ID(Util.GetValueOfInt(_formData[0]._txtCurrency)); //Set the Currency which selected on new form
                        _pay.SetC_ConversionType_ID(_formData[0]._txtConversionType); //Set the Currency ConversionType which selected on new form
                        //_pay.SetPayAmt(Math.Abs(_formData[0]._txtAmount));
                        //get C_PaymentMethod_ID from Invoice
                        _pay.SetVA009_PaymentMethod_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[0]["VA009_PAYMENTMETHOD_ID"]));



                        if (!_pay.Save())
                        {
                            //closing transaction
                            _trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            string error = pp != null ? pp.GetValue() : "";
                            if (string.IsNullOrEmpty(error))
                            {
                                error = pp != null ? pp.GetName() : "";
                            }
                            return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotSaved";
                        }
                        else
                        {
                            bool _status = true;
                            decimal differenceAmount = 0;
                            for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                            {
                                differenceAmount = 0;
                                MPaymentAllocate PayAlocate = new MPaymentAllocate(ctx, 0, _trx);
                                PayAlocate.SetC_Payment_ID(_pay.GetC_Payment_ID());
                                //PayAlocate.SetC_Invoice_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]));
                                //PayAlocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICEPAYSCHEDULE_ID"]));



                                //PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["GrandTotal"]));
                                //PayAlocate.SetAD_Org_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["AD_ORG_ID"]));
                                PayAlocate.SetAD_Client_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["AD_CLIENT_ID"]));
                                //set Organization with the reference of Bank Account
                                PayAlocate.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                                //PayAlocate.SetWriteOffAmt(0);
                                //PayAlocate.SetOverUnderAmt(0);


                                #region OverUnder
                                if (_txtDifference != 0 && _formData[0]._cmbVoucherMatch == "M" && _formData[0]._cmbDifferenceType != "CH" && Math.Abs(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"])) > Math.Abs(_txtDifference) && _status)
                                {


                                    //pratap
                                    //PayAlocate.SetAmount(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                    if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]) == "API" || Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]) == "ARI")
                                    {
                                        PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                        //PayAlocate.SetAmount(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]) + _formData[0]._txtDifference);
                                        PayAlocate.SetAmount(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                    }
                                    else
                                    {
                                        PayAlocate.SetInvoiceAmt(-1 * Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                        //PayAlocate.SetAmount((-1 * Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"])) + _formData[0]._txtDifference);
                                        PayAlocate.SetAmount((-1 * Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"])));
                                    }

                                    if (PayAlocate.GetAmount() > 0)
                                    {

                                        differenceAmount = Math.Abs(_txtDifference);
                                        PayAlocate.SetAmount(PayAlocate.GetAmount() - differenceAmount);
                                    }
                                    else
                                    {
                                        if (_formData[0]._cmbDifferenceType == "OU")
                                        {
                                            differenceAmount = Decimal.Negate(_txtDifference);
                                            //differenceAmount = _formData[0]._txtDifference;
                                            PayAlocate.SetAmount(PayAlocate.GetAmount() + differenceAmount);
                                        }
                                        else
                                        {
                                            differenceAmount = Decimal.Negate(_txtDifference);
                                            PayAlocate.SetAmount(PayAlocate.GetAmount() + differenceAmount);
                                        }

                                    }


                                    //differenceAmount = _formData[0]._txtDifference;
                                    //end pratap

                                    //DiscountAmount
                                    if (_formData[0]._cmbDifferenceType == "DA")
                                    {
                                        PayAlocate.SetDiscountAmt(differenceAmount);
                                    }
                                    //WriteoffAmount
                                    else if (_formData[0]._cmbDifferenceType == "WO")
                                    {
                                        PayAlocate.SetWriteOffAmt(differenceAmount);
                                    }
                                    //OverUnderPayment
                                    else if (_formData[0]._cmbDifferenceType == "OU")
                                    {
                                        PayAlocate.SetOverUnderAmt(differenceAmount);

                                    }


                                    _status = false;
                                }
                                #endregion OverUnder
                                else
                                {
                                    if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]) == "API" || Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]) == "ARI")
                                    {
                                        PayAlocate.SetInvoiceAmt(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                        PayAlocate.SetAmount(Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                    }
                                    else
                                    {
                                        PayAlocate.SetInvoiceAmt(-1 * Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                        PayAlocate.SetAmount(-1 * Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]));
                                    }
                                }



                                PayAlocate.SetC_Invoice_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]));
                                PayAlocate.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICEPAYSCHEDULE_ID"]));



                                if (!PayAlocate.Save())
                                {
                                    _trx.Rollback();
                                    ValueNamePair pp = VLogger.RetrieveError();
                                    string error = pp != null ? pp.GetValue() : "";
                                    if (string.IsNullOrEmpty(error))
                                    {
                                        error = pp != null ? pp.GetName() : "";
                                    }
                                    return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotSaved";
                                }
                            }
                            //fetch the Updated data from new Instance 
                            //_pay = new MPayment(ctx, _pay.GetC_Payment_ID(), _trx);
                            //Committed Transaction
                            _trx.Commit();
                            //Complete Action using worker
                            string _msg = CompletePayment(ctx, _pay.GetC_Payment_ID(), 149, MPayment.DOCACTION_Complete);
                            if (string.IsNullOrEmpty(_msg))
                            {
                                return _pay.GetC_Payment_ID().ToString();
                            }
                            else
                            {
                                //if Payment is not Completed then delete the record
                                //get the PaymentAllocate records which is under the Payment
                                MPaymentAllocate[] pAllocs = MPaymentAllocate.Get(_pay);
                                if (pAllocs.Length > 0)
                                {
                                    for (int j = 0; j < pAllocs.Length; j++)
                                    {
                                        if (!pAllocs[j].Delete(true, _trx))
                                        {
                                            _trx.Rollback();
                                            ValueNamePair pp = VLogger.RetrieveError();
                                            //some times getting the error pp also
                                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                            if (string.IsNullOrEmpty(error))
                                            {
                                                error = pp != null ? pp.GetName() : "";
                                            }
                                            return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotDeleted";
                                        }

                                    }
                                    if (!_pay.Delete(true, _trx))
                                    {
                                        _trx.Rollback();
                                        ValueNamePair pp = VLogger.RetrieveError();
                                        //some times getting the error pp also
                                        string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                                        if (string.IsNullOrEmpty(error))
                                        {
                                            error = pp != null ? pp.GetName() : "";
                                        }
                                        return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotDeleted";
                                    }
                                    else
                                    {
                                        //commit the transaction
                                        _trx.Commit();
                                        return _msg;
                                    }
                                }
                            }
                        }
                    }
                    #endregion under Payment
                    //}
                }

                else
                {
                    _ds.Dispose();
                    //closing transaction
                    _trx.Rollback();
                    return "VA012_NoDataFound";
                }

            }
            catch (Exception e)
            {
                if (_ds != null)
                {
                    _ds.Dispose();
                }
                //closing transaction
                _trx.Rollback();
                return e.Message;
            }
            return "";
        }
        public int CreateViewAllocation(Ctx ctx, List<StatementProp> _formData, DataSet _ds, int _paymentID)
        {
            MPayment _pay = new MPayment(ctx, _paymentID, null);
            MAllocationHdr alloc = new MAllocationHdr(ctx, true,	//	manual
                   _pay.GetDateTrx(), _pay.GetC_Currency_ID(), ctx.GetContext("#AD_User_Name"), null);
            alloc.SetAD_Org_ID(ctx.GetAD_Org_ID());
            if (!alloc.Save())
            {
            }
            else
            {
                decimal _toAllocateAmount = _pay.GetPayAmt();
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    decimal overUnderAmt = 0;
                    decimal _amount = 0;


                    if (Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]) == "API" || Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]) == "ARI")
                    {
                        _amount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]);
                    }
                    else
                    {
                        _amount = -1 * Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["AMOUNT"]);
                    }
                    if (Math.Abs(_toAllocateAmount) - Math.Abs(_amount) > Math.Abs(_amount))
                    {
                        _toAllocateAmount = _toAllocateAmount - _amount;
                        overUnderAmt = 0;
                        MAllocationLine aLine = new MAllocationLine(ctx, 0, null);
                        aLine.SetAmount(_amount);
                        aLine.SetC_AllocationHdr_ID(alloc.GetC_AllocationHdr_ID());
                        aLine.SetDiscountAmt(Env.ZERO);
                        aLine.SetWriteOffAmt(Env.ZERO);
                        aLine.SetOverUnderAmt(Env.ZERO);
                        aLine.SetC_BPartner_ID(Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner));

                        aLine.SetC_Invoice_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]));

                        MInvoice inv = new MInvoice(ctx, aLine.GetC_Invoice_ID(), null);
                        if (Util.GetValueOfInt(inv.GetC_Order_ID()) > 0)
                        {
                            aLine.SetC_Order_ID(Util.GetValueOfInt(inv.GetC_Order_ID()));
                        }
                        aLine.SetC_Payment_ID(_paymentID);
                        aLine.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICEPAYSCHEDULE_ID"]));
                        if (!aLine.Save())
                        {
                        }
                    }
                    else
                    {

                        overUnderAmt = Math.Abs(_amount) - Math.Abs(_toAllocateAmount);
                        MAllocationLine aLine = new MAllocationLine(ctx, 0, null);
                        aLine.SetAmount(_amount);
                        aLine.SetC_AllocationHdr_ID(alloc.GetC_AllocationHdr_ID());
                        aLine.SetDiscountAmt(Env.ZERO);
                        aLine.SetWriteOffAmt(Env.ZERO);
                        aLine.SetOverUnderAmt(overUnderAmt);
                        aLine.SetC_BPartner_ID(Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner));

                        aLine.SetC_Invoice_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICE_ID"]));

                        MInvoice inv = new MInvoice(ctx, aLine.GetC_Invoice_ID(), null);
                        if (Util.GetValueOfInt(inv.GetC_Order_ID()) > 0)
                        {
                            aLine.SetC_Order_ID(Util.GetValueOfInt(inv.GetC_Order_ID()));
                        }
                        aLine.SetC_Payment_ID(_paymentID);
                        aLine.SetC_InvoicePaySchedule_ID(Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICEPAYSCHEDULE_ID"]));
                        if (!aLine.Save())
                        {
                        }
                    }
                }
                if (alloc.CompleteIt() == "CO")
                {
                    alloc.SetProcessed(true);
                    alloc.SetDocAction("CL");
                    alloc.SetDocStatus("CO");
                    alloc.Save();
                    _ds.Dispose();

                    return alloc.GetC_AllocationHdr_ID();
                }
            }
            return 0;
        }

        /// <summary>
        /// Create Payment against PrePayOrder
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_formData">List of StatementProp class</param>
        /// <param name="_trx">Transaction</param>
        /// <returns>DocumentNo or Error message (string type)</returns>
        public string CreatePaymentFromOrder(Ctx ctx, List<StatementProp> _formData, Trx _trx)
        {
            int _paymentMethodID = 0;
            decimal _txtAmount = 0;
            try
            {
                if (Util.GetValueOfInt(_formData[0]._ctrlOrder) > 0)
                {
                    _paymentMethodID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT VA009_PAYMENTMETHOD_ID FROM C_ORDER WHERE C_ORDER_ID=" + _formData[0]._ctrlOrder));
                }
                if (_paymentMethodID <= 0)
                {
                    _paymentMethodID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT VA009_PAYMENTMETHOD_ID FROM C_BPARTNER WHERE C_BPARTNER_ID=" + Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner)));
                }

                //get Bpartner Location
                int _bPartnerLocation_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_BPartner_Location_ID FROM C_BPartner_Location WHERE IsActive='Y' AND C_BPartner_ID=" + _formData[0]._ctrlBusinessPartner));

                if (_formData[0]._cmbCurrency != _formData[0]._txtCurrency)
                {
                    _txtAmount = MConversionRate.Convert(ctx, _formData[0]._txtAmount, _formData[0]._cmbCurrency, _formData[0]._txtCurrency, _formData[0]._dtStatementDate, _formData[0]._txtConversionType, ctx.GetAD_Client_ID(), _formData[0]._bankAcctOrg_ID);
                }
                else {
                    _txtAmount = _formData[0]._txtAmount;
                }

                MPayment _pay = new MPayment(ctx, 0, _trx);
                //passed third parameter to pass AD_Org_ID
                int C_Doctype_ID = GetDocTypeID(ctx, _txtAmount, _formData[0]._bankAcctOrg_ID);//Get DocType using Amount
                _pay.SetC_DocType_ID(C_Doctype_ID);
                //Payment AcctDate & Trx Date should be StatementLine AcctDate
                //_pay.SetDateAcct(System.DateTime.Now);
                _pay.SetDateAcct(_formData[0]._dtStatementDate);
                //_pay.SetDateTrx(System.DateTime.Now);
                _pay.SetDateTrx(_formData[0]._dtStatementDate);
                //set Org_ID based on BankAccount
                _pay.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                _pay.SetC_BankAccount_ID(Util.GetValueOfInt(_formData[0]._cmbBankAccount));
                _pay.SetC_BPartner_ID(Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner));
                //added BPartner_Location_ID from BPartner
                _pay.SetC_BPartner_Location_ID(_bPartnerLocation_ID);
                _pay.SetC_Currency_ID(_formData[0]._txtCurrency);// set Currency selected on form.
                _pay.SetC_ConversionType_ID(_formData[0]._txtConversionType);//set conversionType selected on form.
                _pay.SetPayAmt(Math.Abs(_txtAmount));//Set Amount which is converted based on Currency and ConversionRate
                _pay.SetVA009_PaymentMethod_ID(_paymentMethodID);
                _pay.SetC_Order_ID(_formData[0]._ctrlOrder);

                if (!_pay.Save())
                {
                    _trx.Rollback();
                    ValueNamePair pp = VLogger.RetrieveError();
                    string error = pp != null ? pp.GetValue() : "";
                    if (string.IsNullOrEmpty(error))
                    {
                        error = pp != null ? pp.GetName() : "";
                    }
                    return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotSaved";
                }
                else
                {
                    //Committing transaction
                    _trx.Commit();
                    //Complete Action using worker
                    string _msg = CompletePayment(ctx, _pay.GetC_Payment_ID(), 149, MPayment.DOCACTION_Complete);
                    if (string.IsNullOrEmpty(_msg))
                    {
                        return _pay.GetC_Payment_ID().ToString();
                    }
                    else
                    {
                        //delete the Payment if not record is not completed!
                        if (!_pay.Delete(true, _trx))
                        {
                            _trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            //some times getting the error pp also
                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                            if (string.IsNullOrEmpty(error))
                            {
                                error = pp != null ? pp.GetName() : "";
                            }
                            return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotDeleted";
                        }
                        else
                        {
                            //commit the transaction
                            _trx.Commit();
                            return _msg;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _trx.Rollback();
                return e.Message;
            }
        }

        /// <summary>
        /// Create Payment against PrePayOrder
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_formData">List of StatementProp class</param>
        /// <param name="_trx">Transaction</param>
        /// <returns>DocumentNo or Error message (string type)</returns>
        public string CreatePaymentFromCharge(Ctx ctx, List<StatementProp> _formData, Trx _trx)
        {
            int _paymentMethodID = 0;
            try
            {
                //based on txtAmout get the PaymentMethod with respective column
                string payMethod_ID = _formData[0]._txtAmount >= 0 ? "VA009_PAYMENTMETHOD_ID" : "VA009_PO_PaymentMethod_ID";
                _paymentMethodID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT " + payMethod_ID + " FROM C_BPARTNER WHERE C_BPARTNER_ID=" + Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner)));
                //if PaymentMethod_ID is zero it will return a message not do the Payment
                if (_paymentMethodID == 0)
                {
                    return "VA012_NotfoundPayMethodOnBPartner";
                }
                //get Bpartner Location
                int _bPartnerLocation_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_BPartner_Location_ID FROM C_BPartner_Location WHERE IsActive='Y' AND C_BPartner_ID=" + _formData[0]._ctrlBusinessPartner));

                MPayment _pay = new MPayment(ctx, 0, _trx);
                //passed third parameter to pass AD_Org_ID
                int C_Doctype_ID = GetDocTypeID(ctx, _formData[0]._txtAmount, _formData[0]._bankAcctOrg_ID);
                _pay.SetDescription(_formData[0]._txtVoucherNo);
                _pay.SetC_DocType_ID(C_Doctype_ID);
                //Payment AcctDate & Trx Date should be StatementLine AcctDate
                //_pay.SetDateAcct(System.DateTime.Now);
                _pay.SetDateAcct(_formData[0]._dtStatementDate);
                //_pay.SetDateTrx(System.DateTime.Now);
                _pay.SetDateTrx(_formData[0]._dtStatementDate);
                //set Org_ID based on BankAccount
                _pay.SetAD_Org_ID(_formData[0]._bankAcctOrg_ID);
                _pay.SetC_BankAccount_ID(Util.GetValueOfInt(_formData[0]._cmbBankAccount));
                _pay.SetC_BPartner_ID(Util.GetValueOfInt(_formData[0]._ctrlBusinessPartner));
                //added BPartner_Location_ID from BPartner
                _pay.SetC_BPartner_Location_ID(_bPartnerLocation_ID);
                _pay.SetC_Currency_ID(Util.GetValueOfInt(_formData[0]._txtCurrency)); //Set the Currency selected on new form
                _pay.SetC_ConversionType_ID(_formData[0]._txtConversionType); //Set the Currency ConversionType selected on new form
                _pay.SetPayAmt(Math.Abs(_formData[0]._txtAmount));
                //uncomment this
                _pay.SetVA009_PaymentMethod_ID(_paymentMethodID);

                //remove this
                //_pay.SetVA009_PaymentMethod_ID(1000006);

                _pay.SetC_Charge_ID(Util.GetValueOfInt(_formData[0]._cmbCharge));
                _pay.SetC_Tax_ID(Util.GetValueOfInt(_formData[0]._cmbTaxRate));
                _pay.SetTaxAmount(Math.Abs(_formData[0]._txtTaxAmount));
                //if selected TaxRate have Surcharge then will insert SurChargeAmt
                if (_formData[0]._cmbCharge > 0 && _formData[0]._cmbTaxRate > 0 && _formData[0]._txtTaxAmount != 0)
                {
                    _pay.Set_Value("SurchargeAmt", Math.Abs(_formData[0]._surChargeAmt));
                }

                if (!_pay.Save())
                {
                    _trx.Rollback();
                    ValueNamePair pp = VLogger.RetrieveError();
                    string error = pp != null ? pp.GetValue() : "";
                    if (string.IsNullOrEmpty(error))
                    {
                        error = pp != null ? pp.GetName() : "";
                    }
                    return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotSaved";
                }
                else
                {
                    //Committing transaction
                    _trx.Commit();
                    //Implemented Complete Action using worker
                    string _msg = CompletePayment(ctx, _pay.GetC_Payment_ID(), 149, MPayment.DOCACTION_Complete);
                    if (string.IsNullOrEmpty(_msg))
                    {
                        return _pay.GetC_Payment_ID().ToString();
                    }
                    else
                    {
                        //Delete the Payment if Payment is not Completed!
                        if (!_pay.Delete(true, _trx))
                        {
                            _trx.Rollback();
                            ValueNamePair pp = VLogger.RetrieveError();
                            //some times getting the error pp also
                            string error = pp != null ? pp.ToString() == null ? pp.GetValue() : pp.ToString() : "";
                            if (string.IsNullOrEmpty(error))
                            {
                                error = pp != null ? pp.GetName() : "";
                            }
                            return !string.IsNullOrEmpty(error) ? error : "VA012_PaymentNotDeleted";
                        }
                        else
                        {
                            //commit the transaction
                            _trx.Commit();
                            return _msg;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //RollBack the Transaction
                _trx.Rollback();
                return e.Message;
            }
        }
        public string CreatePaymentFromCash(Ctx ctx, List<StatementProp> _formData)
        {

            try
            {
                int _fromCurr = 0;
                int _toCurr = 0;
                _fromCurr = _formData[0]._cmbCurrency;
                _toCurr = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CURRENCY_ID FROM C_CASHBOOK WHERE C_CASHBOOK_ID=" + _formData[0]._cmbCashBook));
                decimal _amt = Decimal.Negate(_formData[0]._txtAmount);
                _amt = MConversionRate.Convert(ctx, _amt, _fromCurr, _toCurr, ctx.GetAD_Client_ID(), ctx.GetAD_Org_ID());
                int C_doctype_ID = Util.GetValueOfInt(DB.ExecuteScalar("select c_doctype_id from C_DocType where docbasetype='CMC' and name ='Cash Journal' and ad_client_id=" + ctx.GetAD_Client_ID(), null, null));
                MCashBook _cashbook = new MCashBook(ctx, Util.GetValueOfInt(_formData[0]._cmbCashBook), null);
                MCash _cash = new MCash(ctx, 0, null);
                _cash.SetC_CashBook_ID(Util.GetValueOfInt(_formData[0]._cmbCashBook));
                _cash.SetName(DateTime.Now.ToShortDateString());
                _cash.SetC_DocType_ID(C_doctype_ID);
                _cash.SetStatementDate(System.DateTime.Now);
                _cash.SetDateAcct(System.DateTime.Now);
                _cash.SetBeginningBalance(_cashbook.GetCompletedBalance());
                if (!_cash.Save())
                {
                    return "VA012_CashJournalNotSaved";
                }
                else
                {
                    MCashLine _cashLine = new MCashLine(ctx, 0, null);
                    _cashLine.SetC_Cash_ID(_cash.GetC_Cash_ID());
                    _cashLine.SetCashType("C");
                    _cashLine.SetDescription(_formData[0]._txtDescription);
                    _cashLine.SetC_Charge_ID(_formData[0]._cmbCharge);
                    _cashLine.SetAmount(_amt);
                    _cashLine.SetC_Tax_ID(_formData[0]._cmbTaxRate);
                    if (_amt > 0)
                    {
                        _cashLine.SetTaxAmt(MConversionRate.Convert(ctx, Math.Abs(_formData[0]._txtTaxAmount), _fromCurr, _toCurr, ctx.GetAD_Client_ID(), ctx.GetAD_Org_ID()));
                    }
                    else
                    {
                        _cashLine.SetTaxAmt(MConversionRate.Convert(ctx, Math.Abs(_formData[0]._txtTaxAmount), _fromCurr, _toCurr, ctx.GetAD_Client_ID(), ctx.GetAD_Org_ID()) * -1);
                    }
                    _cashLine.SetC_Currency_ID(_formData[0]._cmbCurrency);
                    _cashLine.SetC_BankAccount_ID(_formData[0]._cmbBankAccount);
                    //_cashLine.SetTransferType(_formData[0]._cmbTransferType);
                    // if (_formData[0]._cmbTransferType == "CK")

                    if (_formData[0]._txtCheckNo != null || _formData[0]._txtCheckNo != "")
                    {
                        _cashLine.SetCheckNo(_formData[0]._txtCheckNo);
                        _cashLine.SetCheckDate(System.DateTime.Now);
                    }
                    if (_amt > 0)
                    {
                        _cashLine.SetVSS_PAYMENTTYPE("R");
                    }
                    else
                    {
                        _cashLine.SetVSS_PAYMENTTYPE("P");
                    }

                    if (!_cashLine.Save())
                    {
                        return "VA012_CashJournalLineNotSaved";
                    }
                    else
                    {

                        if (_cash.CompleteIt() == "CO")
                        {
                            _cash.SetProcessed(true);
                            _cash.SetDocAction("CL");
                            _cash.SetDocStatus("CO");
                            _cash.Save();

                            //                            #region Get Payment ID
                            //                            string _sql = @"SELECT C_PAYMENT_ID
                            //                                        FROM C_PAYMENT PAY
                            //                                        WHERE PAY.DOCUMENTNO    ='" + _cash.GetName() + @"'
                            //                                        AND PAY.C_BANKACCOUNT_ID=" + _cashLine.GetC_BankAccount_ID() + @"
                            //                                        AND PAY.AD_CLIENT_ID    =" + _cashLine.GetAD_Client_ID() + @"
                            //                                        AND PAY.AD_ORG_ID       =" + _cashLine.GetAD_Org_ID() + @"
                            //                                        AND PAY.TRXTYPE         ='X'
                            //                                        AND PAY.PAYAMT          = " + Decimal.Negate(_cashLine.GetAmount()) + @"
                            //                                        AND PAY.CREATED          >= " + GlobalVariable.TO_DATE(_cashLine.GetCreated(), false);
                            //                            int _pyID = 0;
                            //                            _pyID = Util.GetValueOfInt(DB.ExecuteScalar(_sql));
                            //                            if (_pyID > 0)
                            //                            {
                            //                                _isPaymentFromCash = true;
                            //                                return _pyID.ToString();
                            //                            }
                            //                            else
                            //                            {
                            //                                //return "VA012_PaymentNotCreated";

                            //                            }
                            //                            #endregion Get Payment ID
                            return "0";

                        }
                        else
                        {
                            return "VA012_CashJournalNotProcessed";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Get Payment Amount and Tranaction Amounts
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="_dragSourceID">C_Payment_ID</param>
        /// <param name="_dragDestinationID">C_Payment_ID or Zero</param>
        /// <param name="_amount">Amount</param>
        /// <param name="statmtDate">Statement Date</param>
        /// <param name="accountID">C_BankAccount_ID</param>
        /// <returns>List of Amount</returns>

        public PaymentResponse CheckPaymentCondition(Ctx ctx, int _dragSourceID, int _dragDestinationID, decimal _amount, DateTime? statmtDate, int accountID)
        {
            PaymentResponse _obj = new PaymentResponse();
            string _sql = "";
            decimal _payAmt = 0;
            decimal _trxAmt = 0;
            string _authCode = "";
            //defined new Variables
            int _currency_Id = 0;
            int _conversionType_Id = 0;

            int _accountCurrencyID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CURRENCY_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + accountID));

            if (_dragDestinationID == 0)
            {
                int _count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) AS COUNT FROM C_BANKSTATEMENTLINE BSL INNER JOIN C_BANKSTATEMENT BS ON BS.C_BANKSTATEMENT_ID = BSL.C_BANKSTATEMENT_ID  WHERE BS.DOCSTATUS!='VO' AND BSL.C_PAYMENT_ID IN(" + _dragSourceID + ")"));
                if (_count > 0)
                {
                    _obj._status = "VA012_PaymentAlreadyMatchedOthrStatement";
                    return _obj;

                }

                _sql = @"SELECT
                        CASE
                        WHEN(PAY.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                        THEN
                            CASE
                                WHEN (DT.DOCBASETYPE='ARR')
                                THEN CURRENCYCONVERT(PAY.PAYAMT, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statmtDate, true) + @", PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) 
                                WHEN (DT.DOCBASETYPE='APP')
                                THEN CURRENCYCONVERT(PAY.PAYAMT * -1, PAY.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statmtDate, true) + @", PAY.C_ConversionType_ID, PAY.AD_Client_ID, PAY.AD_Org_ID) 
                            END
                        ELSE
                            CASE
                            WHEN (DT.DOCBASETYPE='ARR')
                            THEN PAY.PAYAMT
                            WHEN (DT.DOCBASETYPE='APP')
                            THEN PAY.PAYAMT*-1
                            END
                        END AS AMOUNT, PAY.C_Currency_ID, PAY.C_ConversionType_ID
                    FROM C_PAYMENT PAY
                    INNER JOIN C_DOCTYPE DT
                    ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                    INNER JOIN C_BANKACCOUNT AC
                    ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID

                    LEFT JOIN C_BANKSTATEMENTLINE BSL 
                    ON PAY.C_PAYMENT_ID =BSL.C_PAYMENT_ID 

                    LEFT JOIN C_CURRENCY BCURR
                    ON " + _accountCurrencyID + @" =BCURR.C_CURRENCY_ID
                   WHERE PAY.C_PAYMENT_ID=" + _dragSourceID;
                DataSet _ds = DB.ExecuteDataset(_sql);
                //get Currency and ConversionRateType also with PaymentAmount
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    _currency_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]);
                    _conversionType_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                    _payAmt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["AMOUNT"]);

                }
                //string.IsNullOrEmpty() method
                if (string.IsNullOrEmpty(_authCode))
                {
                    if (_amount == 0)
                    {
                        _obj._amount = _payAmt;
                        _obj._trxamount = _payAmt;// _trxAmt;
                    }
                    else if ((_payAmt > 0 && _amount < 0) || (_payAmt < 0 && _amount > 0))
                    {
                        //_obj._status = "VA012_StatementPaymentNotEq";
                        _obj._status = "VA012_StatementPaymentNotMatched";
                        return _obj;
                    }
                }
                else
                {
                    if (_amount == 0)
                    {
                        _obj._amount = _payAmt;
                        _obj._trxamount = _payAmt;// _trxAmt;
                    }

                }
            }
            else if (_dragDestinationID > 0)
            {
                return MatchByDrag(ctx, _dragSourceID, _dragDestinationID, statmtDate);
            }
            //Set the property Values of Currency and ConversionType
            _obj._conversionType_Id = _conversionType_Id;
            _obj._currency_Id = _currency_Id;
            _obj._status = "Success";
            return _obj;
        }
        public string CheckScheduleCondition(Ctx ctx, int _dragSourceID, int _dragDestinationID, string _listToCheck, decimal _amount, int _currencyId, int _formBPartnerID)
        {
            decimal _amt = 0;
            int _bpID = 0;
            //for new record
            if (_dragDestinationID == 0)
            {
                _amt = _amount;
                return ScheduleCondition(_dragSourceID, _listToCheck, _amt, _currencyId, _formBPartnerID);
            }
            //for existing record
            else if (_dragDestinationID > 0)
            {
                //Compare the StatementLine Date with DueDate of the Schedule and StatementLineDate Can't less than the DueDate
                if (int.TryParse(_listToCheck, out int _scheduleId))
                {
                    DateTime? _stmtDate = Util.GetValueOfDateTime(DB.ExecuteScalar("SELECT StatementLineDate FROM C_BANKSTATEMENTLINE WHERE C_BANKSTATEMENTLINE_id =" + _dragDestinationID));
                    DateTime? _dueDate = Util.GetValueOfDateTime(DB.ExecuteScalar("SELECT DueDate FROM C_InvoicePaySchedule WHERE C_InvoicePaySchedule_ID =" + _scheduleId));
                    if (_stmtDate < _dueDate)
                    {
                        return "VA012_StmtDateCantlessTrxDate";
                    }
                }
                try
                {
                    _amt = Util.GetValueOfDecimal(DB.ExecuteScalar("SELECT StmtAmt FROM C_BANKSTATEMENTLINE WHERE C_BANKSTATEMENTLINE_id =" + _dragDestinationID));
                    _bpID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_BPARTNER_ID FROM C_BANKSTATEMENTLINE WHERE C_BANKSTATEMENTLINE_id =" + _dragDestinationID));
                    if (_bpID == 0)
                    {
                        _bpID = _formBPartnerID;
                    }
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                //return ScheduleCondition(_dragSourceID, _listToCheck, _amt, _currencyId, _formBPartnerID);
                return ScheduleCondition(_dragSourceID, _listToCheck, _amt, _currencyId, _bpID);
            }
            return "";
        }

        public string CheckScheduleTotal(Ctx ctx, string _listToCheck, decimal _amt, int _currencyId, int _destinationID)
        {
            string _sql = "";
            decimal _existingAmount = 0;
            decimal _amount = 0;
            //for new record
            if (_destinationID == 0)
            {
                _amount = _amt;
            }
            //for existing record
            else if (_destinationID > 0)
            {
                try
                {
                    _amount = Util.GetValueOfDecimal(DB.ExecuteScalar("SELECT StmtAmt FROM C_BANKSTATEMENTLINE WHERE C_BANKSTATEMENTLINE_id =" + _destinationID));
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
            if (_listToCheck != "" && _listToCheck != null)
            {
                _sql = @"SELECT SUM(CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                     ROUND(PAY.DueAmt       *(
                                      CASE
                                        WHEN CCR.MULTIPLYRATE IS NOT NULL
                                        THEN CCR.MULTIPLYRATE
                                        ELSE CCR1.DIVIDERATE
                                      END),NVL(BCURR.StdPrecision,2))
                                ELSE
                                     ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                              END) AS AMOUNT
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                            LEFT JOIN C_CURRENCY BCURR
                            ON " + _currencyId + @" =BCURR.C_CURRENCY_ID

                            LEFT JOIN C_CONVERSION_RATE CCR
                            ON (CCR.C_CURRENCY_ID =inv.C_CURRENCY_ID
                            AND CCR.C_CURRENCY_TO_ID=" + _currencyId + @"
                            AND CCR.ISACTIVE      ='Y'
                            AND CCR.AD_CLIENT_ID    =inv.AD_CLIENT_ID
                            AND CCR.AD_ORG_ID      IN (inv.AD_ORG_ID,0)
                            AND CCR.C_ConversionType_ID = inv.C_ConversionType_ID
                            AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)

                            LEFT JOIN C_CONVERSION_RATE CCR1
                            ON (CCR1.C_CURRENCY_ID   =" + _currencyId + @"
                            AND CCR1.C_CURRENCY_TO_ID=inv.C_CURRENCY_ID
                            AND CCR1.ISACTIVE        ='Y'
                            AND CCR1.AD_CLIENT_ID    =inv.AD_CLIENT_ID
                            AND CCR1.AD_ORG_ID      IN (inv.AD_ORG_ID,0)
                            AND CCR1.C_ConversionType_ID = inv.C_ConversionType_ID
                            AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO) 

                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID 
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _listToCheck + ")";
                _existingAmount = Util.GetValueOfDecimal(Util.GetValueOfDecimal(DB.ExecuteScalar(_sql)));

                if (Math.Abs(_amount) < _existingAmount)
                {
                    return "VA012_StatementAmountLessThnSchedules";
                }
                else if (Math.Abs(_amount) > _existingAmount)
                {
                    return "VA012_ScheduleAmountLessThnStatement";
                }
                else if (Math.Abs(_amount) == _existingAmount)
                {
                    return "Success";
                }
                else
                {
                    return "VA012_Error";
                }
            }
            else
            {
                return "VA012_NoScheduleSelected";
            }

        }

        /// <summary>
        /// Check the Schedule Condition
        /// </summary>
        /// <param name="_dragSourceID">C_InvoicePaySchedule_ID</param>
        /// <param name="_listToCheck">List of Schedules</param>
        /// <param name="_amount">Amount on the form</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <returns>success or Error (string type Value)</returns>
        private static string ScheduleCondition(int _dragSourceID, string _listToCheck, decimal _amount, int _currencyId, int _formBPartnerID)
        {
            string[] listToCheck = null;
            string _sql = "";
            decimal _existingAmount = 0;
            DataSet _ds = new DataSet();
            DataSet _dsExisting = new DataSet();
            string _inputDocBaseType = "";
            string _existingDocBaseType = "";
            string _existingDocBaseType2 = "";
            if (_listToCheck != "")
            {
                listToCheck = _listToCheck.Split(',');

            }
            //            _sql = @"SELECT inv.C_BPARTNER_ID ,
            //                              CASE
            //                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
            //                                THEN
            //                                  CASE
            //                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
            //                                    THEN PAY.DueAmt       *CCR.MULTIPLYRATE
            //                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
            //                                    THEN (PAY.DueAmt      *CCR.MULTIPLYRATE) *-1
            //                                  END
            //                                ELSE
            //                                  CASE
            //                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
            //                                    THEN PAY.DUEAMT
            //                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
            //                                    THEN PAY.DUEAMT       *-1
            //                                  END
            //                              END AS AMOUNT,DT.DOCBASETYPE
            //                            FROM C_INVOICEPAYSCHEDULE PAY
            //                            INNER JOIN C_INVOICE INV
            //                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
            //                            LEFT JOIN C_CURRENCY BCURR
            //                            ON " + _currencyId + @" =BCURR.C_CURRENCY_ID
            //                            LEFT JOIN C_CONVERSION_RATE CCR
            //                            ON (CCR.C_CURRENCY_ID =inv.C_CURRENCY_ID
            //                            AND CCR.ISACTIVE      ='Y'
            //                            AND CCR.C_CURRENCY_TO_ID=" + _currencyId + @"
            //                            AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)
            //                            INNER JOIN C_DOCTYPE DT
            //                            ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID 
            //                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID=" + _dragSourceID;
            _sql = @"SELECT inv.C_BPARTNER_ID ,
                              CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                     ROUND(PAY.DueAmt       *(
                                  CASE
                                    WHEN CCR.MULTIPLYRATE IS NOT NULL
                                    THEN CCR.MULTIPLYRATE
                                    ELSE CCR1.DIVIDERATE
                                  END),NVL(BCURR.StdPrecision,2))
                                ELSE
                                     ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                              END AS AMOUNT,DT.DOCBASETYPE, INV.C_Currency_ID
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                            LEFT JOIN C_CURRENCY BCURR
                            ON " + _currencyId + @" =BCURR.C_CURRENCY_ID

                            LEFT JOIN C_CONVERSION_RATE CCR
                            ON (CCR.C_CURRENCY_ID =inv.C_CURRENCY_ID
                            AND CCR.C_CURRENCY_TO_ID=" + _currencyId + @"
                            AND CCR.ISACTIVE      ='Y'
                            AND CCR.AD_CLIENT_ID    =inv.AD_CLIENT_ID
                            AND CCR.AD_ORG_ID      IN (inv.AD_ORG_ID,0)
                            AND CCR.C_ConversionType_ID = inv.C_ConversionType_ID
                            AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)

                            LEFT JOIN C_CONVERSION_RATE CCR1
                            ON (CCR1.C_CURRENCY_ID   =" + _currencyId + @"
                            AND CCR1.C_CURRENCY_TO_ID=inv.C_CURRENCY_ID
                            AND CCR1.ISACTIVE        ='Y'
                            AND CCR1.AD_CLIENT_ID    =inv.AD_CLIENT_ID
                            AND CCR1.AD_ORG_ID      IN (inv.AD_ORG_ID,0)
                            AND CCR1.C_ConversionType_ID = inv.C_ConversionType_ID
                            AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO) 

                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID 
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID=" + _dragSourceID;
            _ds = DB.ExecuteDataset(_sql);

            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                _inputDocBaseType = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]);


                if (_formBPartnerID > 0 && Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPARTNER_ID"]) != _formBPartnerID)
                {
                    return "VA012_BPNotSame";
                }
                if (_amount < 0 && (_inputDocBaseType == "ARI" || _inputDocBaseType == "APC"))
                {
                    //return "VA012_SelectAPI_ARC";
                }
                else if (_amount > 0 && (_inputDocBaseType == "API" || _inputDocBaseType == "ARC"))
                {
                    //return "VA012_SelectARI_APC";
                }
                else if (_amount == 0)
                {
                    // return "VA012_StatementAmountZero";
                }

                if (Math.Abs(_amount) < Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["AMOUNT"]))
                {
                    //return "VA012_StatementAmountLessThnSchedules";
                }
                else if (_listToCheck != "" && listToCheck.Length > 0)
                {
                    _sql = @"SELECT UNIQUE DT.DOCBASETYPE
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID 
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _listToCheck + ")";
                    _dsExisting = DB.ExecuteDataset(_sql);

                    if (_dsExisting != null && _dsExisting.Tables[0].Rows.Count > 0)
                    {

                        // change by pratap
                        if (_dsExisting.Tables[0].Rows.Count == 2)
                        {
                            _existingDocBaseType = Util.GetValueOfString(_dsExisting.Tables[0].Rows[0]["DOCBASETYPE"]);
                            _existingDocBaseType2 = Util.GetValueOfString(_dsExisting.Tables[0].Rows[1]["DOCBASETYPE"]);
                        }
                        else
                        {
                            _existingDocBaseType = Util.GetValueOfString(_dsExisting.Tables[0].Rows[0]["DOCBASETYPE"]);
                        }


                        if (_dsExisting.Tables[0].Rows.Count == 2)
                        {
                            if ((_inputDocBaseType == "ARI" || _inputDocBaseType == "ARC") && ((_existingDocBaseType == "API" && _existingDocBaseType2 == "APC") || (_existingDocBaseType2 == "API" && _existingDocBaseType == "APC")))
                            {
                                return "VA012_SelectAPI_APC";
                            }
                            else if ((_inputDocBaseType == "API" || _inputDocBaseType == "APC") && ((_existingDocBaseType == "ARI" && _existingDocBaseType2 == "ARC") || (_existingDocBaseType2 == "ARI" && _existingDocBaseType == "ARC")))
                            {
                                return "VA012_SelectARI_ARC";
                            }
                        }
                        else
                        {
                            if ((_inputDocBaseType == "ARI" || _inputDocBaseType == "ARC") && (_existingDocBaseType == "API" || _existingDocBaseType == "APC"))
                            {
                                return "VA012_SelectAPI_APC";
                            }
                            else if ((_inputDocBaseType == "API" || _inputDocBaseType == "APC") && (_existingDocBaseType == "ARI" || _existingDocBaseType == "ARC"))
                            {
                                return "VA012_SelectARI_ARC";
                            }
                        }


                        //_existingDocBaseType = Util.GetValueOfString(_dsExisting.Tables[0].Rows[0]["DOCBASETYPE"]);

                        //if ((_inputDocBaseType == "ARI" || _inputDocBaseType == "APC") && (_existingDocBaseType == "API" && _existingDocBaseType == "ARC"))
                        //{
                        //    return "VA012_SelectAPI_ARC";
                        //}
                        //else if ((_inputDocBaseType == "API" || _inputDocBaseType == "ARC") && (_existingDocBaseType == "ARI" && _existingDocBaseType == "APC"))
                        //{
                        //    return "VA012_SelectARI_APC";
                        //}
                        //end pratap
                    }
                    //used new query to get the C_Currency_ID
                    //replaced UNIQUE to DISTINCT to Excute query in Postgre as well
                    _sql = @"SELECT DISTINCT INV.C_Currency_ID
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _listToCheck + ")";
                    _dsExisting = DB.ExecuteDataset(_sql);

                    if (_dsExisting != null && _dsExisting.Tables[0].Rows.Count > 0)
                    {

                        if (Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]) != Util.GetValueOfInt(_dsExisting.Tables[0].Rows[0]["C_Currency_ID"]))
                        {
                            return "VA012_SelectSameCurrencySchedules";
                        }
                    }
                    //                    _sql = @"SELECT SUM(CASE
                    //                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                    //                                THEN
                    //                                  CASE
                    //                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                    //                                    THEN PAY.DueAmt       *CCR.MULTIPLYRATE
                    //                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                    //                                    THEN (PAY.DueAmt      *CCR.MULTIPLYRATE) *-1
                    //                                  END
                    //                                ELSE
                    //                                  CASE
                    //                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                    //                                    THEN PAY.DUEAMT
                    //                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                    //                                    THEN PAY.DUEAMT       *-1
                    //                                  END
                    //                              END) AS AMOUNT
                    //                            FROM C_INVOICEPAYSCHEDULE PAY
                    //                            INNER JOIN C_INVOICE INV
                    //                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                    //                            LEFT JOIN C_CURRENCY BCURR
                    //                            ON " + _currencyId + @" =BCURR.C_CURRENCY_ID
                    //                            LEFT JOIN C_CONVERSION_RATE CCR
                    //                            ON (CCR.C_CURRENCY_ID =inv.C_CURRENCY_ID
                    //                            AND CCR.ISACTIVE      ='Y'
                    //                            AND CCR.C_CURRENCY_TO_ID=" + _currencyId + @"
                    //                            AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)
                    //                            INNER JOIN C_DOCTYPE DT
                    //                            ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID 
                    //                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _listToCheck + ")";


                    _sql = @"SELECT SUM(CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                     ROUND(PAY.DueAmt       *(
                                      CASE
                                        WHEN CCR.MULTIPLYRATE IS NOT NULL
                                        THEN CCR.MULTIPLYRATE
                                        ELSE CCR1.DIVIDERATE
                                      END),NVL(BCURR.StdPrecision,2))
                                ELSE
                                     ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                              END) AS AMOUNT
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV
                            ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID
                            LEFT JOIN C_CURRENCY BCURR
                            ON " + _currencyId + @" =BCURR.C_CURRENCY_ID

                            LEFT JOIN C_CONVERSION_RATE CCR
                            ON (CCR.C_CURRENCY_ID =inv.C_CURRENCY_ID
                            AND CCR.C_CURRENCY_TO_ID=" + _currencyId + @"
                            AND CCR.ISACTIVE      ='Y'
                            AND CCR.AD_CLIENT_ID    =inv.AD_CLIENT_ID
                            AND CCR.AD_ORG_ID      IN (inv.AD_ORG_ID,0)
                            AND CCR.C_ConversionType_ID = inv.C_ConversionType_ID
                            AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)

                            LEFT JOIN C_CONVERSION_RATE CCR1
                            ON (CCR1.C_CURRENCY_ID   =" + _currencyId + @"
                            AND CCR1.C_CURRENCY_TO_ID=inv.C_CURRENCY_ID
                            AND CCR1.ISACTIVE        ='Y'
                            AND CCR1.AD_CLIENT_ID    =inv.AD_CLIENT_ID
                            AND CCR1.AD_ORG_ID      IN (inv.AD_ORG_ID,0)
                            AND CCR1.C_ConversionType_ID = inv.C_ConversionType_ID
                            AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO) 

                            INNER JOIN C_DOCTYPE DT
                            ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID 
                            WHERE PAY.C_INVOICEPAYSCHEDULE_ID IN(" + _listToCheck + ")";
                    _existingAmount = Util.GetValueOfDecimal(Util.GetValueOfDecimal(DB.ExecuteScalar(_sql)));

                    if (Math.Abs(_amount) < _existingAmount + Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["AMOUNT"]))
                    {
                        //pratap
                        //return "VA012_StatementAmountLessThnSchedules";
                    }
                }
            }
            return "Success";
        }

        /// <summary>
        /// Check PrepayCondition and return string object either success or Error Value
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_dragSourceID">C_Order_ID</param>
        /// <param name="_dragDestinationID">C_BankStatementLine_ID</param>
        /// <param name="_listToCheck">PrepayOrder List</param>
        /// <param name="_amount">Amount</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <param name="statementDate">Statement Date</param>
        /// <returns>return string object</returns>
        public PrepayResponse CheckPrepayCondition(Ctx ctx, int _dragSourceID, int _dragDestinationID, string _listToCheck, decimal _amount, int _currencyId, int _formBPartnerID, DateTime? statementDate)
        {
            PrepayResponse _obj = new PrepayResponse();
            string _sql = "";
            decimal _payAmt = 0;
            int _count = 0;
            //for new record
            if (_dragDestinationID == 0)
            {
                return CheckFormPrepayCondition(ctx, _dragSourceID, _amount, _currencyId, _formBPartnerID, statementDate);

            }

            //for existing record
            else if (_dragDestinationID > 0)
            {
                string _qry = "";
                int _statementBP = 0;
                decimal _statementAmt = 0;
                int _orderBP = 0;
                decimal _orderAmt = 0;
                decimal _unConvtOrderAmt = 0;
                DateTime? _stateLineDate = null;
                DateTime? _acctDate = null;
                DataSet _ds = new DataSet();
                DataSet _ds1 = new DataSet();
                _qry = @" SELECT C_BPARTNER_ID, StmtAmt as TRXAMT, StatementLineDate 
                    FROM C_BANKSTATEMENTLINE
                    WHERE C_BANKSTATEMENTLINE_id      =" + _dragDestinationID;
                _ds = DB.ExecuteDataset(_qry);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    _statementBP = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPARTNER_ID"]);
                    _statementAmt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["TRXAMT"]);
                    //Get the StatementLine Date 
                    _stateLineDate = Util.GetValueOfDateTime(_ds.Tables[0].Rows[0]["StatementLineDate"]);
                    _ds.Dispose();
                }

                _qry = @" SELECT ord.C_BPARTNER_ID,
                                        CASE
                                            WHEN(ord.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                            THEN CURRENCYCONVERT(ord.GrandTotal, ord.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, "
                                                + GlobalVariable.TO_DATE(_stateLineDate, true) + @", ord.C_ConversionType_ID, ord.AD_Client_ID, ord.AD_Org_ID) 
                                            ELSE ROUND(ord.GrandTotal,NVL(BCURR.StdPrecision,2))
                                          END AS AMOUNT, ORD.DateAcct, ord.GrandTotal
                                        FROM C_ORDER ORD
                                        LEFT JOIN C_CURRENCY BCURR
                                        ON " + _currencyId + @" =BCURR.C_CURRENCY_ID                
                                        WHERE ORD.C_ORDER_ID=" + _dragSourceID;
                _ds1 = DB.ExecuteDataset(_qry);
                if (_ds1 != null && _ds1.Tables[0].Rows.Count > 0)
                {
                    _orderBP = Util.GetValueOfInt(_ds1.Tables[0].Rows[0]["C_BPARTNER_ID"]);
                    _orderAmt = Util.GetValueOfDecimal(_ds1.Tables[0].Rows[0]["AMOUNT"]);
                    //actual Amount (UnconvertedAmt if Currencies are not same)
                    _unConvtOrderAmt = Util.GetValueOfDecimal(_ds1.Tables[0].Rows[0]["GrandTotal"]);
                    //Get the Order AcctDate 
                    _acctDate = Util.GetValueOfDateTime(_ds1.Tables[0].Rows[0]["DateAcct"]);
                    _ds1.Dispose();
                }

                //StatementLine Date can't be less than the AcctDate of Order when Drag the PrePayOrder on to the Line
                if (_stateLineDate < _acctDate)
                {
                    _obj._status = "VA012_StmtDateCantlessTrxDate";
                    return _obj;
                }
                //_orderAmt is zero and _unConvtOrderAmt is have non zero amount then it means no ConversionRate found return the message
                if (_orderAmt == 0 && _unConvtOrderAmt != 0)
                {
                    _obj._status = "VA012_ConversionRateNotFound";
                    return _obj;
                }

                //both must be same sign statementAmt & orderAmt
                //if (_statementBP == 0 && _statementAmt <= _orderAmt)
                if (_statementBP == 0 && ((_statementAmt <= 0 && _orderAmt <= 0) || (_statementAmt >= 0 && _orderAmt >= 0)))
                {
                    _obj._status = "Success";
                    return _obj;
                }
                //no need to match the statementAmt with orderAmt
                //else if (_statementBP > 0 && (_statementAmt <= _orderAmt && _statementBP == _orderBP))
                else if (_statementBP > 0 && _statementBP == _orderBP)
                {

                    _obj._status = "Success";
                    return _obj;
                }
                else
                {
                    _obj._status = "VA012_StatementOrderNotMatched";
                    return _obj;

                }
            }
            _obj._status = "Success";
            return _obj;

        }

        /// <summary>
        /// Check the Conditions and get List
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_dragSourceID">C_CashJournal_ID</param>
        /// <param name="_dragDestinationID">C_BankStatementLine_ID</param>
        /// <param name="_amount">Amount on the form</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <param name="statementDat">StatementDate selected on the form</param>
        /// <returns>Class Object which is having list of values</returns>
        public ContraResponse CheckContraCondition(Ctx ctx, int _dragSourceID, int _dragDestinationID, decimal _amount, int _currencyId, int _formBPartnerID, DateTime? statementDat)
        {
            ContraResponse _obj = new ContraResponse();
            string _sql = "";
            decimal _payAmt = 0;
            decimal _unConvtpayAmt = 0;
            int _currency_Id = 0;
            int _conversionType_Id = 0;
            if (_dragDestinationID == 0)
            {
                int _count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT COUNT(*) AS COUNT FROM C_BANKSTATEMENTLINE BSL INNER JOIN C_BANKSTATEMENT BS ON BS.C_BANKSTATEMENT_ID = BSL.C_BANKSTATEMENT_ID  WHERE BS.DOCSTATUS!='VO' AND BSL.C_CASHLINE_ID=" + _dragSourceID));
                if (_count > 0)
                {
                    _obj._status = "VA012_CashLineAlreadyMatchedOthrStmt";
                    return _obj;

                }

                _sql = @"SELECT  CASE
                                            WHEN(csl.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                            THEN CURRENCYCONVERT(csl.amount, csl.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statementDat, true) +
                                            @", csl.C_ConversionType_ID, cs.AD_Client_ID, cs.AD_Org_ID) 
                                            ELSE ROUND(csl.amount,NVL(BCURR.StdPrecision,2))
                                          END AS AMOUNT, csl.C_Currency_ID, csl.C_ConversionType_ID, csl.amount AS actualAmt
                                        FROM C_Cashline csl
                                        inner join C_Cash cs on cs.C_Cash_id=csl.C_Cash_id
                                        LEFT JOIN C_CURRENCY BCURR
                                        ON " + _currencyId + @" =BCURR.C_CURRENCY_ID
                                        WHERE csl.C_cashline_ID=" + _dragSourceID;
                DataSet _ds = DB.ExecuteDataset(_sql, null, null);
                //_payAmt = Decimal.Negate(Util.GetValueOfDecimal(DB.ExecuteScalar(_sql)));
                //get the payAmt 
                //get Currency_ID which is bind on CashLine
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    _payAmt = Decimal.Negate(Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["AMOUNT"]));
                    //UnConverted Amount or Actual amount
                    _unConvtpayAmt = Decimal.Negate(Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["actualAmt"]));
                    _currency_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]);
                    //ConversionType also want on the form when select CashLine as per requirement
                    _conversionType_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                }
                //if payAmt is getting zero means it does have ConversionRate for that selected StatementDate when Currencies are not same and _unConvtpayAmt is actual Amount
                if (_payAmt == 0 && _unConvtpayAmt != 0)
                {
                    _obj._status = "VA012_ConversionRateNotFound";
                    return _obj;
                }
                if (_amount == 0)
                {
                    _obj._amount = _payAmt;
                }
                else if (_payAmt == _amount)
                {
                    _obj._amount = _payAmt;
                }
                //sign should be same
                else if (!(_payAmt <= 0 && _amount <= 0) && !(_payAmt >= 0 && _amount >= 0))
                {
                    _obj._status = "VA012_StatementCashLineNotMatched";
                    return _obj;
                }
                else if ((_payAmt <= 0 && _amount <= 0) || (_payAmt >= 0 && _amount >= 0))
                {
                    _obj._amount = _amount;
                }
            }
            else if (_dragDestinationID > 0)
            {
                return MatchContraByDrag(ctx, _dragSourceID, _dragDestinationID, _currencyId, statementDat);
            }
            //set the Property _currency_Id with new value
            _obj._currency_Id = _currency_Id;
            //set the ConversionType
            _obj._conversionType_Id = _conversionType_Id;
            _obj._status = "Success";
            return _obj;


        }

        /// <summary>
        /// Get the Contra values if success otherwise get error Value
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_dragPaymentID">C_CashLine_ID</param>
        /// <param name="_dragStatementID">C_BankStatementLine_ID</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="statementDat">StatementDate</param>
        /// <returns>Object of the ContraResponse class</returns>
        public ContraResponse MatchContraByDrag(Ctx ctx, int _dragPaymentID, int _dragStatementID, int _currencyId, DateTime? statementDat)
        {
            int _count = 0;
            string _qry = "";
            int _statementBP = 0;
            decimal _statementAmt = 0;
            int _paymentBP = 0;
            decimal _paymentAmt = 0;
            decimal _unConvtpaymentAmt = 0;
            DateTime? _stateLineDate = null;
            DateTime? _acctDate = null;
            DataSet _ds = new DataSet();
            DataSet _ds1 = new DataSet();
            bool _status = false;
            int _currency_Id = 0;
            int _conversionType_Id = 0;
            ContraResponse _obj = new ContraResponse();


            _count = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CASHLINE_ID FROM C_BANKSTATEMENTLINE WHERE C_BANKSTATEMENTLINE_ID=" + _dragStatementID));
            if (_count > 0)
            {
                _obj._status = "VA012_StatementAlreadyMatchedOthrCshln";
                return _obj;
            }

            _qry = @" SELECT C_BPARTNER_ID, StmtAmt as TRXAMT, StatementLineDate 
                    FROM C_BANKSTATEMENTLINE
                    WHERE C_BANKSTATEMENTLINE_id      =" + _dragStatementID;
            _ds = DB.ExecuteDataset(_qry);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                _statementBP = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPARTNER_ID"]);
                _statementAmt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["TRXAMT"]);
                //Get the StatementLine Date 
                _stateLineDate = Util.GetValueOfDateTime(_ds.Tables[0].Rows[0]["StatementLineDate"]);
                _ds.Dispose();
            }
            _qry = @" SELECT csl.C_BPARTNER_ID,
                                        CASE
                                            WHEN(csl.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                            THEN CURRENCYCONVERT(csl.amount, csl.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(_stateLineDate, true) +
                                            @", csl.C_ConversionType_ID, cs.AD_Client_ID, cs.AD_Org_ID) 
                                            ELSE ROUND(csl.amount,NVL(BCURR.StdPrecision,2))
                                          END AS AMOUNT, csl.C_Currency_ID, csl.C_ConversionType_ID, cs.DateAcct, csl.amount as actualAmt
                                        FROM C_Cashline csl
                                        inner join C_Cash cs on cs.C_Cash_id=csl.C_Cash_id
                                        LEFT JOIN C_CURRENCY BCURR
                                        ON " + _currencyId + @" =BCURR.C_CURRENCY_ID
                                    WHERE csl.C_cashline_ID=" + _dragPaymentID;

            _ds1 = DB.ExecuteDataset(_qry);
            if (_ds1 != null && _ds1.Tables[0].Rows.Count > 0)
            {
                _paymentBP = Util.GetValueOfInt(_ds1.Tables[0].Rows[0]["C_BPARTNER_ID"]);
                _paymentAmt = Decimal.Negate(Util.GetValueOfDecimal(_ds1.Tables[0].Rows[0]["AMOUNT"]));
                //unConvertedAmount
                _unConvtpaymentAmt = Decimal.Negate(Util.GetValueOfDecimal(_ds1.Tables[0].Rows[0]["actualAmt"]));
                _currency_Id = Util.GetValueOfInt(_ds1.Tables[0].Rows[0]["C_Currency_ID"]);//get the C_Currency_ID from CashJournalLine
                 //ConversionType also want on the form when select CashLine as per requirement
                _conversionType_Id = Util.GetValueOfInt(_ds1.Tables[0].Rows[0]["C_ConversionType_ID"]);
                //Get the Cash AcctDate 
                _acctDate = Util.GetValueOfDateTime(_ds1.Tables[0].Rows[0]["DateAcct"]);
                _ds1.Dispose();
            }

            //StatementLine Date can't be less than the AcctDate of Cash Journal when Drag the Cash Journal on to the Line
            if (_stateLineDate < _acctDate)
            {
                _obj._status = "VA012_StmtDateCantlessTrxDate";
                return _obj;
            }
            //_paymentAmt is zero and _unConvtpaymentAmt is have non zero amount then it means no ConversionRate found return the message
            if (_paymentAmt == 0 && _unConvtpaymentAmt != 0)
            {
                _obj._status = "VA012_ConversionRateNotFound";
                return _obj;
            }

            if (_statementAmt == 0)
            {
                _status = true;
                _obj._amount = _paymentAmt;

            }
            //StatementAmt and CashLineAmt should be Equal sign
            if (_statementBP == 0 && ((_statementAmt <= 0 && _paymentAmt <= 0) || (_statementAmt >= 0 && _paymentAmt >= 0)))
            {
                _status = true;
                _obj._amount = _paymentAmt;
                _obj._status = "Success";
                _obj._currency_Id = _currency_Id;
                //set the ConversionType
                _obj._conversionType_Id = _conversionType_Id;
                return _obj;
            }
            // _statementAmt & _paymentAmt should be same sign but not same amount
            else if (_statementBP > 0 && ((_statementAmt <= 0 && _paymentAmt <= 0) || (_statementAmt >= 0 && _paymentAmt >= 0)) && _statementBP == _paymentBP)
            {
                _status = true;
                _obj._amount = _paymentAmt;
                _obj._status = "Success";
                _obj._currency_Id = _currency_Id;
                //set the ConversionType
                _obj._conversionType_Id = _conversionType_Id;
                return _obj;
            }
            else
            {
                _obj._status = "VA012_StatementCashLineNotMatched";
                return _obj;
            }


        }

        /// <summary>
        /// Check the Invoice Condition 
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_invoiceID">C_Invoice_ID's</param>
        /// <param name="_amount">Amount present on the form</param>
        /// <returns>message success or Error (string type value)</returns>
        public string CheckInvoiceCondition(Ctx ctx, string _invoiceID, decimal _amount)
        {
            string _sql = "";
            string _docBaseType = "";
            if (!string.IsNullOrEmpty(_invoiceID))
            {
                _sql = @"SELECT DT.DOCBASETYPE
                        FROM C_INVOICE INV
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =INV.C_DOCTYPE_ID
                        WHERE INV.C_INVOICE_ID IN(" + _invoiceID + ")";
                //_docBaseType = Util.GetValueOfString(DB.ExecuteScalar(_sql));
                DataSet _ds = DB.ExecuteDataset(_sql, null, null);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    _docBaseType = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]);
                    for (int i = 0; _ds.Tables[0].Rows.Count > i; i++)
                    {
                        string _nxtdocBaseType = Util.GetValueOfString(_ds.Tables[0].Rows[i]["DOCBASETYPE"]);
                        if (_amount < 0 && (_docBaseType.Equals(MDocBaseType.DOCBASETYPE_ARINVOICE) || _docBaseType.Equals(MDocBaseType.DOCBASETYPE_APCREDITMEMO)))
                        {
                            return "VA012_SelectAPI";
                        }
                        else if (_amount > 0 && (_docBaseType.Equals(MDocBaseType.DOCBASETYPE_APINVOICE) || _docBaseType.Equals(MDocBaseType.DOCBASETYPE_ARCREDITMEMO)))
                        {
                            return "VA012_SelectARI";
                        }
                        if (!_nxtdocBaseType.Equals(_docBaseType) && (_docBaseType.Equals(MDocBaseType.DOCBASETYPE_ARINVOICE) || _docBaseType.Equals(MDocBaseType.DOCBASETYPE_APCREDITMEMO)))
                        {
                            return "VA012_SelectAPI";
                        }
                        else if (!_nxtdocBaseType.Equals(_docBaseType) && (_docBaseType.Equals(MDocBaseType.DOCBASETYPE_APINVOICE) || _docBaseType.Equals(MDocBaseType.DOCBASETYPE_ARCREDITMEMO)))
                        {
                            return "VA012_SelectARI";
                        }
                    }
                }
                //if (_amount < 0 && (_docBaseType == "ARI" || _docBaseType == "APC"))
                //{
                //    return "VA012_SelectAPI";
                //}
                //else if (_amount > 0 && (_docBaseType == "API" || _docBaseType == "ARC"))
                //{
                //    return "VA012_SelectARI";
                //}
                //else if (_amount == 0)
                //{
                //    return "VA012_StatementAmountZero";
                //}
            }
            return "Success";
        }
        public PaymentResponse CheckFormPaymentCondition(Ctx ctx, int _paymentID, decimal _amount)
        {
            PaymentResponse _obj = new PaymentResponse();
            #region commented
            //            string _sql = "";
            //            string _docBaseType = "";
            //            if (_paymentID > 0)
            //            {
            //                _sql = @"SELECT DT.DOCBASETYPE
            //                        FROM C_PAYMENT PAY
            //                        INNER JOIN C_DOCTYPE DT
            //                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
            //                        WHERE PAY.C_PAYMENT_ID=" + _paymentID;
            //                _docBaseType = Util.GetValueOfString(DB.ExecuteScalar(_sql));
            //                if (_amount < 0 && _docBaseType == "ARR")
            //                {
            //                    return "VA012_SelectAPP";
            //                }
            //                else if (_amount > 0 && _docBaseType == "APP" )
            //                {
            //                    return "VA012_SelectARR";
            //                }
            //                else if (_amount == 0)
            //                {
            //                    return "VA012_StatementAmountZero";
            //                }
            //            }
            #endregion commented

            string _sql = "";
            string _docBaseType = "";
            decimal _paymentAmount = 0;
            DataSet _ds = new DataSet();

            if (_paymentID > 0)
            {
                _sql = @"SELECT DT.DOCBASETYPE,
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
                                  END),NVL(BCURR.StdPrecision,2))
                                WHEN (DT.DOCBASETYPE='APP')
                                THEN ROUND((PAY.PAYAMT*(
                              CASE
                                WHEN CCR.MULTIPLYRATE IS NOT NULL
                                THEN CCR.MULTIPLYRATE
                                ELSE CCR1.DIVIDERATE
                              END)),NVL(BCURR.StdPrecision,2))*-1
                              END
                            ELSE
                              CASE
                                WHEN (DT.DOCBASETYPE='ARR')
                                THEN PAY.PAYAMT
                                WHEN (DT.DOCBASETYPE='APP')
                                THEN PAY.PAYAMT*-1
                              END
                          END AS AMOUNT
                        FROM C_PAYMENT PAY
                        INNER JOIN C_DOCTYPE DT
                        ON DT.C_DOCTYPE_ID =PAY.C_DOCTYPE_ID
                        INNER JOIN C_BANKACCOUNT AC
                        ON AC.C_BANKACCOUNT_ID =PAY.C_BANKACCOUNT_ID
                        LEFT JOIN C_CURRENCY BCURR
                        ON AC.C_CURRENCY_ID =BCURR.C_CURRENCY_ID
                        LEFT JOIN C_CONVERSION_RATE CCR
                        ON (CCR.C_CURRENCY_ID   =PAY.C_CURRENCY_ID
                        AND CCR.ISACTIVE        ='Y'
                        AND CCR.C_CURRENCY_TO_ID=AC.C_CURRENCY_ID
                            AND CCR.AD_CLIENT_ID    =pay.AD_CLIENT_ID
                            AND CCR.AD_ORG_ID      IN (pay.AD_ORG_ID,0)
                         AND CCR.C_ConversionType_ID      IN (pay.C_ConversionType_ID,0)
                        AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)
                    LEFT JOIN C_CONVERSION_RATE CCR1
                    ON (CCR1.C_CURRENCY_ID   =AC.C_CURRENCY_ID
                    AND CCR1.C_CURRENCY_TO_ID=PAY.C_CURRENCY_ID
                    AND CCR1.ISACTIVE        ='Y'
                           AND CCR1.AD_CLIENT_ID    =pay.AD_CLIENT_ID
                            AND CCR1.AD_ORG_ID      IN (pay.AD_ORG_ID,0)
                    AND CCR1.C_ConversionType_ID      IN (pay.C_ConversionType_ID,0)
                    AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO)

                        WHERE PAY.C_PAYMENT_ID=" + _paymentID;
                _ds = DB.ExecuteDataset(_sql);
                if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                {
                    _docBaseType = Util.GetValueOfString(_ds.Tables[0].Rows[0]["DOCBASETYPE"]);
                    _paymentAmount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["AMOUNT"]);
                    if (_amount < 0 && _docBaseType == "ARR")
                    {
                        _obj._status = "VA012_SelectAPP";
                        return _obj;

                    }
                    else if (_amount > 0 && _docBaseType == "APP")
                    {
                        _obj._status = "VA012_SelectARR";
                        return _obj;
                    }
                    else if (_amount == 0)
                    {
                        _obj._amount = _paymentAmount;
                    }
                    else if (_paymentAmount != _amount)
                    {
                        _obj._status = "VA012_StmtPayAmtNotEqual";
                        return _obj;
                    }
                }
                else
                {
                    _obj._status = "VA012_Error";
                    return _obj;

                }


            }
            _obj._status = "Success";
            return _obj;
        }

        /// <summary>
        /// Check Prepay Order Condition
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="_orderID">C_Order_ID</param>
        /// <param name="_amount">Amount present on the form</param>
        /// <param name="_currencyId">C_Currency_ID</param>
        /// <param name="_formBPartnerID">C_BPartner_ID</param>
        /// <param name="statementDate">Statement Date</param>
        /// <returns>object which is having class propety Values</returns>
        public PrepayResponse CheckFormPrepayCondition(Ctx ctx, int _orderID, decimal _amount, int _currencyId, int _formBPartnerID, DateTime? statementDate)
        {
            PrepayResponse _obj = new PrepayResponse();
            string _sql = "";
            decimal _payAmt = 0;
            decimal _unConvtpayAmt = 0;
            int currency_Id = 0;
            int conversionType = 0;
            DataSet _ds = new DataSet();
            if (_orderID > 0)
            {
                if (_amount < 0)
                {
                    _obj._status = "VA012_StatementAmountNegative";
                    return _obj;
                }
                //else if (_amount == 0)
                //{
                //    return "VA012_StatementAmountZero";
                //}
                else
                {

                    _sql = @"SELECT
                          CASE
                            WHEN(ord.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                            THEN CURRENCYCONVERT(ord.GrandTotal, ord.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, "
                                        + GlobalVariable.TO_DATE(statementDate, true) + @", ord.C_ConversionType_ID, ord.AD_Client_ID, ord.AD_Org_ID) 
                            ELSE ROUND(ord.GrandTotal,NVL(BCURR.StdPrecision,2))
                          END AS AMOUNT,ORD.C_BPARTNER_ID, ORD.C_Currency_ID, ORD.C_ConversionType_ID, ord.GrandTotal
                        FROM C_ORDER ORD
                        LEFT JOIN C_CURRENCY BCURR
                        ON " + _currencyId + @" =BCURR.C_CURRENCY_ID
                        WHERE ORD.C_ORDER_ID=" + _orderID;
                    //_payAmt = Util.GetValueOfDecimal(DB.ExecuteScalar(_sql));
                    _ds = DB.ExecuteDataset(_sql);
                    if (_ds != null && _ds.Tables[0].Rows.Count > 0)
                    {
                        //get the C_Currency_ID and C_ConversionType_ID from the Order table
                        currency_Id = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]);
                        conversionType = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                        _payAmt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["AMOUNT"]);
                        //Actual amount
                        _unConvtpayAmt = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["GrandTotal"]);

                        //_orderAmt is zero and _unConvtOrderAmt is have non zero amount then it means no ConversionRate found return the message
                        if (_payAmt == 0 && _unConvtpayAmt != 0)
                        {
                            _obj._status = "VA012_ConversionRateNotFound";
                            return _obj;
                        }

                        if (_amount == 0)
                        {
                            _obj._amount = _payAmt;

                        }
                        //if (_amount > _payAmt)
                        //{
                        //    _obj._status = "VA012_StatementAmountBigger";
                        //    return _obj;

                        //}

                        if (_formBPartnerID > 0)
                        {
                            if (_formBPartnerID != Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_BPARTNER_ID"]))
                            {
                                _obj._status = "VA012_BPNotSame";
                                return _obj;
                            }

                        }
                    }

                }

            }
            //set the property Values
            _obj._currency_Id = currency_Id;
            _obj._conversionType_Id = conversionType;
            _obj._status = "Success";
            return _obj;
        }

        /// <summary>
        /// to get the list of Matching Base Data from Refrence key
        /// </summary>
        /// <param name="ct">Context</param>
        /// <returns>list of Matching Base Name and Value</returns>
        public List<MatchBase> getMatchBaseData(Ctx ct)
        {
            List<MatchBase> matchingBase = new List<MatchBase>();
            MatchBase lst = null;
            bool isBase = false;
            if (Env.IsBaseLanguage(ct, "AD_Ref_List"))
                isBase = true;
            StringBuilder sql = new StringBuilder();
            if (isBase)
                sql.Append(" SELECT rl.name, rl.value FROM AD_Ref_List rl");
            else
            {
                sql.Append("SELECT trl.name, rl.value  FROM AD_Ref_List rl ")
               .Append("  INNER JOIN AD_Ref_List_Trl trl ON rl.AD_Ref_List_ID = trl.AD_Ref_List_ID ");
            }
            //Excluded the Invoice and Order Values
            sql.Append(@" WHERE rl.isactive = 'Y' AND rl.ad_reference_id IN (SELECT AD_Reference_ID FROM AD_Reference
                        WHERE upper(NAME) LIKE upper('VA012_MatchingBase'))  AND rl.value NOT IN ('OR','IN')");
            if (!isBase)
                sql.Append(" AND trl.AD_Language='" + Env.GetAD_Language(ct) + "'");

            DataSet ds = DB.ExecuteDataset(sql.ToString(), null, null);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lst = new MatchBase();
                    lst.Value = Util.GetValueOfString(ds.Tables[0].Rows[i]["value"]);
                    lst.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["name"]);
                    matchingBase.Add(lst);
                }
            }
            return matchingBase;
        }

        /// <summary>
        /// Get the list of Bank's
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <returns>List of Bank's</returns>
        public List<MatchBase> GetBank(Ctx ctx)
        {
            List<MatchBase> bankList = new List<MatchBase>();
            MatchBase list = null;
            //added Client_ID to get Bank's with respect to Client
            DataSet ds = DB.ExecuteDataset("SELECT NAME,C_BANK_ID FROM C_Bank WHERE ISACTIVE='Y' AND IsOwnBank='Y' AND AD_Client_ID=" + ctx.GetAD_Client_ID(), null, null);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    list = new MatchBase();
                    list.Value = Util.GetValueOfString(ds.Tables[0].Rows[i]["C_BANK_ID"]);
                    list.Name = Util.GetValueOfString(ds.Tables[0].Rows[i]["NAME"]);
                    bankList.Add(list);
                }
            }
            return bankList;
        }

        /// <summary>
        /// Get InvoicePaySchedule Details
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="seltdInvoice">C_Invoice_ID's</param>
        /// <param name="accountID">C_BankAccount_ID</param>
        /// <param name="statemtDate">Statement Date</param>
        /// <returns>get the list of InvoicePaySchedule</returns>
        public List<InvoicePaySchedule> GetInvPaySchedule(Ctx ctx, string seltdInvoice, int accountID, DateTime? statemtDate)
        {
            List<InvoicePaySchedule> payList = new List<InvoicePaySchedule>();
            InvoicePaySchedule list = null;
            //get Account Currency ID
            int _accountCurrencyID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CURRENCY_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + accountID));
            string _sql = @"SELECT PAY.C_INVOICEPAYSCHEDULE_ID,
                                CASE
                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))*-1
                                  END      AS DUEAMT,
                               CASE
                                WHEN(inv.C_CURRENCY_ID!=BCURR.C_CURRENCY_ID)
                                THEN
                                  CASE
                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                                     THEN CURRENCYCONVERT(PAY.DueAmt, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statemtDate, true) + @", inv.C_ConversionType_ID, inv.AD_Client_ID, inv.AD_Org_ID) 
                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                                    THEN CURRENCYCONVERT(PAY.DueAmt * -1, inv.C_CURRENCY_ID, BCURR.C_CURRENCY_ID, " + GlobalVariable.TO_DATE(statemtDate, true) + @", inv.C_ConversionType_ID, inv.AD_Client_ID, inv.AD_Org_ID) 
                                  END
                                ELSE
                                  CASE
                                    WHEN (DT.DOCBASETYPE IN ('ARI','APC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))
                                    WHEN (DT.DOCBASETYPE IN ('API','ARC'))
                                    THEN ROUND(PAY.DUEAMT,NVL(BCURR.StdPrecision,2))*-1
                                  END
                              END AS CONVERTEDAMOUNT,PAY.DueDate, PAY.DueAmt
                            FROM C_INVOICEPAYSCHEDULE PAY
                            INNER JOIN C_INVOICE INV ON pay.C_INVOICE_id=inv.C_INVOICE_id
                            LEFT JOIN C_CURRENCY CURR ON INV.C_CURRENCY_ID =CURR.C_CURRENCY_ID
                            LEFT JOIN C_CURRENCY BCURR ON " + _accountCurrencyID + @" =BCURR.C_CURRENCY_ID 
                            INNER JOIN VA009_PAYMENTMETHOD PM ON (PM.VA009_PAYMENTMETHOD_ID=PAY.VA009_PAYMENTMETHOD_ID )
                            INNER JOIN C_DOCTYPE DT ON DT.C_DOCTYPE_ID=INV.C_DOCTYPE_ID
                            WHERE  pay.VA009_IsPaid='N' AND PAY.ISACTIVE='Y' AND INV.DOCSTATUS IN ('CO','CL') AND PM.VA009_PAYMENTBASETYPE!='B' AND PAY.C_INVOICE_ID IN(" + seltdInvoice + ")";

            DataSet _ds = DB.ExecuteDataset(_sql, null, null);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < _ds.Tables[0].Rows.Count; i++)
                {
                    list = new InvoicePaySchedule();

                    list.DueDate = Util.GetValueOfDateTime(_ds.Tables[0].Rows[i]["DueDate"]);
                    list.c_invoicepayschedule_id = Util.GetValueOfInt(_ds.Tables[0].Rows[i]["C_INVOICEPAYSCHEDULE_ID"]);
                    list.DueAmount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["CONVERTEDAMOUNT"]);
                    list.Amount = Util.GetValueOfDecimal(_ds.Tables[0].Rows[i]["DueAmt"]);
                    list.DueAmt = DisplayType.GetNumberFormat(DisplayType.Amount).GetFormatAmount(list.Amount, ctx.GetContext("#ClientLanguage"));
                    payList.Add(list);
                }
            }
            return payList;
        }
    }

    public class ConcileStatement
    {
        public string basecurrency { get; internal set; }
        public decimal reconciled { get; internal set; }
        public decimal unreconciled { get; internal set; }
    }

    public class InvoicePaySchedule
    {
        public string DueAmt { get; internal set; }
        public DateTime? DueDate { get; internal set; }
        public int c_invoicepayschedule_id { get; internal set; }
        public decimal DueAmount { get; internal set; }
        public decimal Amount { get; internal set; }
    }

    public class MatchBase
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }
    public class PrepayResponse
    {
        public decimal _amount { get; set; }
        public string _status { get; set; }
        public int _conversionType_Id { get; set; }
        public int _currency_Id { get; set; }
    }
    public class ContraResponse
    {
        public decimal _amount { get; set; }
        public string _status { get; set; }
        public int _currency_Id { get; set; }
        public int _conversionType_Id { get; internal set; }
    }
    public class PaymentResponse
    {
        public decimal _amount { get; set; }
        public decimal _trxamount { get; set; }
        public string _status { get; set; }
        public int _currency_Id { get; set; }
        public int _conversionType_Id { get; set; }
    }

    public class ProcessResponse
    {
        public string _statementUnmatchedLines { get; set; }
        public string _statementNotProcessed { get; set; }
        public string _statementProcessed { get; set; }
        public string _error { get; set; }
        public string _status { get; set; }

    }
    public class MatchResponse
    {
        public string _paymentNo { get; set; }
        public string _statementNo { get; set; }
        public string _statementLine { get; set; }
        public string _error { get; set; }
        public string _warning { get; set; }
        public string _paymentOrCash { get; set; }

    }

    public class MatchStatementGridResponse
    {

        public string _paymentNo { get; set; }
        public string _statementNo { get; set; }
        public string _statementLine { get; set; }
        public string _error { get; set; }
        public string _warning { get; set; }
        public string _paymentOrCash { get; set; }

        public string _trxDate { get; set; }
        public string _trxNo { get; set; }
        public string _salesAmt { get; set; }
        public string _netAmt { get; set; }
        public string _difference { get; set; }
        public string _taxAmt { get; set; }
        public string _chargeType { get; set; }
        public string _taxRate { get; set; }
    }

    public class UnMatchResponse
    {
        public string _statementOk { get; set; }
        public string _statementNo { get; set; }
        public string _statementNoNotUpdate { get; set; }
        public string _error { get; set; }
        public string _status { get; set; }

    }
    /// <summary>
    /// Bank Statement Operations Properties.
    /// </summary>
    public class StatementProp
    {
        //pratap
        public string _scheduleAmount { get; set; }
        //

        public string _scheduleList { get; set; }
        public string _cmbTransactionType { get; set; }
        public string _txtCharge { get; set; }
        public string _txtStatementNo { get; set; }
        public int _txtStatementPage { get; set; }
        public int _txtStatementLine { get; set; }
        public DateTime? _dtStatementDate { get; set; }
        public int _cmbPaymentMethod { get; set; }
        public string _cmbVoucherMatch { get; set; }
        public decimal _txtAmount { get; set; }
        public decimal _txtTrxAmt { get; set; }
        public string _txtDescription { get; set; }
        public string _txtVoucherNo { get; set; }
        public int _cmbCharge { get; set; }
        public int _cmbTaxRate { get; set; }
        public decimal _txtTaxAmount { get; set; }
        public int _ctrlPayment { get; set; }
        public int _ctrlOrder { get; set; }
        public string _ctrlInvoice { get; set; }
        public int _ctrlCashLine { get; set; }
        public int _ctrlBusinessPartner { get; set; }
        public bool _chkUseNextTime { get; set; }
        public int _cmbBank { get; set; }
        public int _cmbBankAccount { get; set; }
        public int _cmbCurrency { get; set; }
        public int _bankStatementLineID { get; set; }

        public string _cmbContraType { get; set; }
        public int _cmbCashBook { get; set; }
        public string _cmbTransferType { get; set; }
        public string _txtCheckNo { get; set; }

        public string _cmbDifferenceType { get; set; }
        public decimal _txtDifference { get; set; }
        public string _trxno { get; set; }
        public int _bankAcctOrg_ID { get; internal set; }
        public decimal _surChargeAmt { get; set; }
        public int _txtCurrency { get; set; }
        public int _txtConversionType { get; set; }
        // public List<GetScheduleProp> _getSchedules { get; set; }
    }
    public class PaymentProp
    {
        //pratap
        public string paymenttype { get; set; }
        // end pratap
        public int c_bankstatementline_id { get; set; }
        public int c_payment_id { get; set; }
        public string currency { get; set; }
        public string paymentno { get; set; }
        public int c_bpartner_id { get; set; }
        public string businesspartner { get; set; }
        public decimal paymentamount { get; set; }
        public string bpgroup { get; set; }
        public string binarydata { get; set; }
        public string imageurl { get; set; }
        public string ad_image_id { get; set; }
        public string basecurrency { get; set; }
        public decimal convertedamount { get; set; }
        public string isconverted { get; set; }
        public string depositslipno { get; set; }
        public string authcode { get; set; }
        // added new prop docStatus // Lokesh 6/6/2019
        public string docstatus { get; set; }

        public string PaymentMethod { get; set; }
        public DateTime? DateAcct { get; set; }
        public DateTime? DueDate { get; internal set; }
        public string DueAmt { get; internal set; }
    }
    public class StatementLineProp
    {
        public int page { get; set; }
        public int line { get; set; }
        public string invoiceno { get; set; }
        public bool usenexttime { get; set; }

        public decimal STMTAMT { get; set; }
        public int c_charge_id { get; set; }
        public int c_payment_id { get; set; }
        public int c_bankstatementline_id { get; set; }
        public int c_bankstatement_id { get; set; }
        public string currency { get; set; }
        public decimal trxamount { get; set; }
        public string statementno { get; set; }
        public string bpgroup { get; set; }
        public int c_bpartner_id { get; set; }
        public string description { get; set; }
        public string binarydata { get; set; }
        public string imageurl { get; set; }
        public string ad_image_id { get; set; }
        public string basecurrency { get; set; }
        public decimal convertedamount { get; set; }
        public string isconverted { get; set; }
        public string docstatus { get; set; }
        public int c_cashline_id { get; set; }
        public string trxno { get; set; }
        public DateTime? stmtLineDate { get; internal set; }
    }
    public class ChargeProp
    {
        public int chargeID { get; set; }
        public string name { get; set; }
    }
    //public class GetScheduleProp
    //{
    //    public string uid { get; set; }
    //    public string paymentdata { get; set; }
    //    // _scheduleList //uid
    //    //    _scheduleDataList //paymentdata
    //    //   _scheduleAmount 
    //}

}
