﻿using System;
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
using System.Threading;
using System.Globalization;
using VAdvantage.Logging;
using Microsoft.Win32;

namespace VA012.Models
{
    public class VA012_ENBD
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
        string _serverCulture = "";
        string _serverCultureAC = "";


        private static VLogger _log = VLogger.GetVLogger(typeof(VA012_ENBD).FullName);

        #endregion
        public StatementResponse ImportStatement(Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges)
        {
            StatementResponse _obj = new StatementResponse();
            string _branchName = "";
            string _IBAN = "";
            string abc = "";
            string _datasetvalues = "";

            try
            {
                
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                _log.Log(Level.INFO," Culture Name --> " + cultureInfo.Name);


               
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



                int _stementID = 0;
                _Filenames.Append(FileName + ",");

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

                    if ((_Extension.ToUpper() == ".CSV") || (_Extension.ToUpper() == ".XLSX") || (_Extension.ToUpper() == ".XLS"))
                    {

                        DataSet ds = ExcelImport.ImportFromCSV(_path, false, 1);

                        if (File.Exists(_path))
                        {
                            FileInfo fileToDelete = new FileInfo(_path);
                            //fileToDelete.Delete();
                        }
                        if (ds != null)
                        {                           
                            DataTable dt = ds.Tables[0];
                            string _date = "";
                            string[] _dateList = new string[3];
                            if (dt.Rows.Count > 0)
                            {
                                #region ENBD Format
                                if (dt.Columns[0].ColumnName == "F1" && Convert.ToString(dt.Rows[0][0]) == "Account Statement")
                                {
                                    ////Change Culture to Server Culture
                                    //Thread.CurrentThread.CurrentCulture.ClearCachedData();
                                    // _serverCulture = Thread.CurrentThread.CurrentCulture.Name;
                                    // _serverCultureAC = "";
                                    //if (_serverCulture != "en-US")
                                    //{
                                    //    Thread.CurrentThread.CurrentCulture = new CultureInfo(_serverCulture);
                                    //}
                                    /////
                                    // _serverCultureAC = Thread.CurrentThread.CurrentCulture.Name;


                                    string accountCurrency = Convert.ToString(dt.Rows[5][0]);
                                    if (string.IsNullOrEmpty(accountCurrency))
                                    {
                                        accountCurrency = Convert.ToString(dt.Rows[4][0]);
                                    }

                                    //_C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= 'AED'"));
                                    accountCurrency = accountCurrency.Substring(accountCurrency.IndexOf(':') + 1).Trim();

                                    _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("SELECT C_CURRENCY_ID FROM C_CURRENCY WHERE LOWER(DESCRIPTION) LIKE LOWER('" + accountCurrency + "') OR LOWER(ISO_CODE) LIKE LOWER('" + accountCurrency + "')"));

                                    if (_C_Currency_ID != _bankAccountCurrency)
                                    {
                                        _obj._error = "VA012_DiffAccountAndStatementCurrency";
                                        return _obj;
                                    }


                                    _branchName = "";
                                    _IBAN = "";
                                    for (int i = 0; i < dt.Rows.Count; i++)
                                    {
                                        //if (Convert.ToString(dt.Rows[0][0]) != "Account Statement")
                                        //{
                                        //    break;
                                        //}

                                        if (i <= 8)
                                        {
                                            if (i == 3)
                                            {
                                                _IBAN = Convert.ToString(dt.Rows[i][0]);
                                                _IBAN = _IBAN.Substring(_IBAN.LastIndexOf(':') + 1).Trim();
                                            }
                                            if (i == 7)
                                            {
                                                _branchName = Convert.ToString(dt.Rows[i][0]);
                                                _branchName = _branchName.Substring(_branchName.LastIndexOf(':') + 1).Trim();
                                            }
                                            continue;
                                        }
                                        #region Header

                                        if (i == 9)
                                        {
                                            if (_existingStatementID <= 0)
                                            {
                                                _BnkStatm = new MBankStatement(Env.GetCtx(), 0, null);
                                                _BnkStatm.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                                _BnkStatm.SetAD_Org_ID(_AD_Org_ID);
                                                _BnkStatm.SetC_BankAccount_ID(_C_BankAccount_ID);
                                                _BnkStatm.SetName(_statementno);
                                                _BnkStatm.SetStatementDate(DateTime.Now);
                                                // _BnkStatm.SetBeginningBalance(Convert.ToDecimal(dt.Rows[i][9]));
                                                if (!_BnkStatm.Save())
                                                {

                                                    _obj._error = "VA012_BankStatementHeaderNotSaved";
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
                                            _date = "";
                                            if ((Convert.ToString(dt.Rows[i][0]) != string.Empty) && (Convert.ToString(dt.Rows[i][1]) != string.Empty))
                                            {
                                                bool isDiffCulture = false;

                                                if (dt.Rows[i][0].ToString().Contains('.'))
                                                {
                                                    isDiffCulture = true;
                                                    _dateList = dt.Rows[i][0].ToString().Split('.');
                                                    if (_dateList.Length == 3)
                                                    {
                                                        _date = _dateList[1].ToString() + "/" + _dateList[0].ToString() + "/" + _dateList[2].ToString();// MM/DD/YYYY
                                                    }
                                                }
                                                else
                                                {
                                                    _date = dt.Rows[i][0].ToString();
                                                }

                                                //_datasetvalues = "DataTableRpw==> --0--" + dt.Rows[i][0].ToString() + "--1--" + dt.Rows[i][1].ToString() + "--2--" + dt.Rows[i][2].ToString() + "--3--" + dt.Rows[i][3].ToString() + "--4--" + dt.Rows[i][4].ToString();


                                                _BnkStmtLine = new MBankStatementLine(_BnkStatm);
                                                _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                                _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                                _BnkStmtLine.SetVA012_Page(pageno);
                                                _BnkStmtLine.SetLine(lineno);
                                                lineno = lineno + 10;
                                                // abc = "onessss";
                                                _BnkStmtLine.SetStatementLineDate(DateTime.Parse(_date));// Set Transaction Date

                                                _BnkStmtLine.SetDateAcct(DateTime.Parse(_date));// Set Transaction Date
                                                _BnkStmtLine.SetValutaDate(DateTime.Parse(_date));// Set Transaction Date
                                                // abc = "one";
                                                _BnkStmtLine.SetReferenceNo(_IBAN);// Set Transaction Remarks
                                                _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][1]));// Set Transaction Purticular
                                                _BnkStmtLine.SetMemo(_branchName);// Set Deposite Branch
                                                //_C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
                                                // abc = "two";
                                                if (_C_Currency_ID > 0)
                                                    _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
                                                if ((Convert.ToString(dt.Rows[i][2]) != string.Empty) && (Convert.ToString(dt.Rows[i][2]) != "0"))
                                                {
                                                    // abc = "three";
                                                    //  _payAmt = Convert.ToDecimal(dt.Rows[i][2]);
                                                    _payAmt = GetAmount(dt.Rows[i][2].ToString(), isDiffCulture);                                                   
                                                    // abc = "four";
                                                }
                                                else
                                                {
                                                    // abc = "five";
                                                    // abc = dt.Rows[i][3].ToString();
                                                    // _payAmt = Convert.ToDecimal(dt.Rows[i][3]);
                                                    _payAmt = GetAmount(dt.Rows[i][3].ToString(), isDiffCulture);
                                                    //  abc = "six";
                                                }

                                                if (_accountType == "C")
                                                {

                                                    if ((Convert.ToString(dt.Rows[i][2]) != string.Empty) && (Convert.ToString(dt.Rows[i][2]) != "0"))
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
                                                    if ((Convert.ToString(dt.Rows[i][2]) != string.Empty) && (Convert.ToString(dt.Rows[i][2]) != "0"))
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
                                                //  abc = "seven";
                                                if (!_BnkStmtLine.Save())
                                                {

                                                }
                                            }
                                            else
                                            {

                                            }

                                        }
                                        #endregion
                                    }
                                    ////Revert culture back to en-US
                                    //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                                    ////
                                }
                                #endregion ENBD Format

                                #region HSBC Format
                                else if (dt.Columns[0].ColumnName == "Account Number")
                                {

                                    _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= 'AED'"));
                                    if (_C_Currency_ID != _bankAccountCurrency)
                                    {
                                        _obj._error = "VA012_DiffAccountAndStatementCurrency";
                                        return _obj;
                                    }

                                    for (int i = -1; i < dt.Rows.Count; i++)
                                    {

                                        #region Header

                                        if (i == -1)
                                        {
                                            if (_existingStatementID <= 0)
                                            {
                                                _BnkStatm = new MBankStatement(Env.GetCtx(), 0, null);
                                                _BnkStatm.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                                _BnkStatm.SetAD_Org_ID(_AD_Org_ID);
                                                _BnkStatm.SetC_BankAccount_ID(_C_BankAccount_ID);
                                                _BnkStatm.SetName(_statementno);
                                                _BnkStatm.SetStatementDate(DateTime.Now);
                                                // _BnkStatm.SetBeginningBalance(Convert.ToDecimal(dt.Rows[i][9]));
                                                if (!_BnkStatm.Save())
                                                {

                                                    _obj._error = "VA012_BankStatementHeaderNotSaved";
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

                                            if ((Convert.ToString(dt.Rows[i][0]) != string.Empty) && (Convert.ToString(dt.Rows[i][1]) != string.Empty))
                                            {
                                                _BnkStmtLine = new MBankStatementLine(_BnkStatm);
                                                _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                                _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                                _BnkStmtLine.SetVA012_Page(pageno);
                                                _BnkStmtLine.SetLine(lineno);
                                                lineno = lineno + 10;
                                                _BnkStmtLine.SetStatementLineDate(DateTime.Parse(dt.Rows[i][1].ToString()));// Set Transaction Date
                                                _BnkStmtLine.SetDateAcct(DateTime.Parse(dt.Rows[i][1].ToString()));// Set Transaction Date
                                                _BnkStmtLine.SetValutaDate(DateTime.Parse(dt.Rows[i][1].ToString()));// Set Transaction Date
                                                // _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][3]));// Set Transaction Remarks
                                                _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][2]));// Set Transaction Purticular
                                                // _BnkStmtLine.SetMemo(Convert.ToString(dt.Rows[i][10]));// Set Deposite Branch
                                                //_C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][6]) + "'"));
                                                if (_C_Currency_ID > 0)
                                                    _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
                                                if ((Convert.ToString(dt.Rows[i][3]) != string.Empty) && (Convert.ToString(dt.Rows[i][3]) != "0"))
                                                {
                                                    _payAmt = Convert.ToDecimal(dt.Rows[i][3]);
                                                }
                                                else
                                                {
                                                    _payAmt = Convert.ToDecimal(dt.Rows[i][4]);
                                                }

                                                if (_accountType == "C")
                                                {

                                                    if ((Convert.ToString(dt.Rows[i][3]) != string.Empty) && (Convert.ToString(dt.Rows[i][3]) != "0"))
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
                                                    if ((Convert.ToString(dt.Rows[i][3]) != string.Empty) && (Convert.ToString(dt.Rows[i][3]) != "0"))
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
                                                if (!_BnkStmtLine.Save())
                                                {

                                                }
                                            }
                                            else
                                            {

                                            }

                                        }
                                        #endregion
                                    }
                                }
                                #endregion HSBC Format
                                else
                                {
                                    _obj._error = "VA012_ErrorInFileFormat";
                                    return _obj;
                                }
                            }
                            else
                            {
                                _obj._error = "VA012_NoRecordsInExcel";
                                return _obj;
                            }

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
            catch (Exception e)
            {
                //_obj._error = "VA012_ErrorInFileFormat" + "|" + e.Message + "|" + _branchName + "|" + _IBAN + "|" + abc + "|" + _serverCulture + "|" + _serverCultureAC + "||||" + _datasetvalues;
                _obj._error = "VA012_ErrorInFileFormat";
                return _obj;
            }
        }

       
        public DateTime? GetDate(string _date)
        {
            string[] _dateArray = _date.Split('/');
            DateTime? _returnDate = new DateTime(Convert.ToInt32(_dateArray[2]), Convert.ToInt32(_dateArray[1]), Convert.ToInt32(_dateArray[0]));
            return _returnDate;
        }
        public Decimal GetAmount(string _amt)
        {
            Decimal _number = 0;
            int divNum = 0;

            CultureInfo cultureInfo = CultureInfo.CurrentCulture;

           // _number = Convert.ToDecimal(_amt, CultureInfo.InvariantCulture);

            _log.Log(Level.INFO, " _amt --> " + _amt);
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();

            _number = Convert.ToDecimal(_amt, System.Globalization.CultureInfo.GetCultureInfo("no"));

            //if (Decimal.TryParse(_amt, out _number))
            //{

            //}
            //else
            //{
            //    info.NumberDecimalSeparator = ",";
            //    info.NumberGroupSeparator = ".";
            //    _number = Convert.ToDecimal(_amt, info);                             
            //}

            //RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
          //  key.GetValue("PreferredUILanguagesPending");

            //_amt.ToString(CultureInfo.CreateSpecificCulture("en-IN"));
            

            //if (_amt.Contains(".") && !_amt.Contains(","))
            //{
            //    divNum = _amt.Length - _amt.IndexOf(".") - 1;

            //    _number = _number / Convert.ToInt32((Math.Pow(10, divNum)));
            //}
            _log.Log(Level.INFO, " _number --> " + _number);
            return _number;
        }

        private Decimal GetAmount(string _amt, bool isDiffCulture)
        {
            Decimal _number = 0;
                                                
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();

            if (isDiffCulture)
            {
                try
                {
                    info.NumberDecimalSeparator = ",";
                    info.NumberGroupSeparator = ".";
                    _number = Convert.ToDecimal(_amt, info);
                }
                catch
                {
                    Decimal.TryParse(_amt, out _number);
                }
            }
            else
            {
                Decimal.TryParse(_amt, out _number);
            }
         
            return _number;
        }
    }
}