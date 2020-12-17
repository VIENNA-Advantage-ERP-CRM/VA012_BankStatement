/********************************************************
 * Module Name    : Bank Statement
 * Purpose        : Save Check No on Statement Line
 * Class Used     : Controller
 * Chronological Development
 * Neha Thakur     25-10-2019
 ******************************************************/using System;
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
    public class VA012_ENBDChkNo
    {

        #region Variables
        //Variable declaration
        int _AD_Org_ID = 0;
        int _C_BankAccount_ID = 0;
        //int Count = 0;
        int _c_bpartner_id = 0;
        //to get charge id from Statement
        int _C_Charge_ID = 0;
        int _C_Currency_ID = 0;
        decimal? _payAmt = 0;
        string _FileName = string.Empty, _doctype = string.Empty;
        string _message = string.Empty, _date = string.Empty;
        MBankStatement _BnkStatm = null;
        MBankStatementLine _BnkStmtLine = null;
        //DataSet PyDS = null;
        //StringBuilder query = null;
        StringBuilder _Filenames = new StringBuilder();
        string _Extension = string.Empty, _FileLocation = string.Empty;
        //string _serverCulture = "";
        //string _serverCultureAC = "";
        private static VLogger _log = VLogger.GetVLogger(typeof(VA012_ENBDChkNo).FullName);
        #endregion
        /// <summary>
        /// Used to import XLSX,CSV,XLS files and Set values on Bank Statement line according to imported file.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="FileName"></param>
        /// <param name="_path"></param>
        /// <param name="_bankaccount"></param>
        /// <param name="_bankAccountCurrency"></param>
        /// <param name="_statementno"></param>
        /// <param name="_statementCharges"></param>
        /// <returns>StatementResponse Object</returns>
        public VA012.Models.StatementResponse ImportStatement(Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges)
        {
            VA012.Models.StatementResponse _obj = new VA012.Models.StatementResponse();
            string _branchName = "";
            string _IBAN = "";
            //string abc = "";
            //string _datasetvalues = "";
            try
            {
                CultureInfo cultureInfo = CultureInfo.CurrentCulture;
                _log.Log(Level.INFO, " Culture Name --> " + cultureInfo.Name);
                #region Period StartDate and End Date
                DateTime? _startdate = null;
                DateTime? _enddate = null;
                //Get Start Date and End Date (Finacial Year start and end date) from Login Client (Client Info tab where calender ID is located)
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
                            lineno = 10;
                        #endregion
                    }
                }
                _AD_Org_ID = Util.GetValueOfInt(ctx.GetAD_Org_ID());
                _C_BankAccount_ID = _bankaccount;
                _AD_Org_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT AD_Org_ID FROM C_BankAccount WHERE C_BankAccount_ID=" + _C_BankAccount_ID));
                string _accountType = Util.GetValueOfString(DB.ExecuteScalar("Select BankAccountType from C_BankAccount Where C_BankAccount_ID=" + _C_BankAccount_ID));
                int _stementID = 0;
                _Filenames.Append(FileName + ",");
                if (_Filenames.ToString() != "")
                    _Filenames.Remove(_Filenames.Length - 1, 1);
                else
                {
                    _obj._error = "VA012_AttachmentsAllreadyInSystem";
                    return _obj;
                }
                _message = _Filenames.ToString();
                string[] _filenamesall = _message.Split(',');
                for (int K = 0; K < _filenamesall.Length; K++)
                {
                    _FileLocation = _filenamesall[K].ToString();
                    string[] _FileNameExten = _FileLocation.Split('.');
                    _FileName = _FileNameExten[0].ToString();
                    _Extension = "." + _FileNameExten[1].ToString();
                    //Import file
                    if ((_Extension.ToUpper() == ".CSV") || (_Extension.ToUpper() == ".XLSX") || (_Extension.ToUpper() == ".XLS"))
                    {
                        DataSet ds = VA012.Models.ExcelImport.ImportFromCSV(_path, false, 1);
                        if (File.Exists(_path))
                        {
                            FileInfo fileToDelete = new FileInfo(_path);
                        }
                        if (ds != null)
                        {
                            DataTable dt = ds.Tables[0];
                            string _date = "";
                            string[] _dateList = new string[3];
                            if (dt.Rows.Count > 0)
                            {
                                #region ENBD Format

                                string accountCurrency = Util.GetValueOfString(dt.Rows[2][0]);
                                if (accountCurrency != null && accountCurrency != "")
                                    accountCurrency = accountCurrency.Substring(accountCurrency.IndexOf(':') + 1).Trim();

                                _C_Currency_ID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_CURRENCY_ID FROM C_CURRENCY WHERE LOWER(DESCRIPTION) LIKE LOWER('" + accountCurrency + "') OR LOWER(ISO_CODE) LIKE LOWER('" + accountCurrency + "')"));

                                if (_C_Currency_ID != _bankAccountCurrency)
                                {
                                    _obj._error = "VA012_DiffAccountAndStatementCurrency";
                                    return _obj;
                                }
                                _branchName = "";
                                _IBAN = "";
                                for (int i = 0; i < dt.Rows.Count; i++)
                                {
                                    if (i <= 2)
                                    {
                                        if (i == 1)
                                        {
                                            _IBAN = Util.GetValueOfString(dt.Rows[i][0]);
                                            _IBAN = _IBAN.Substring(_IBAN.LastIndexOf(':') + 1).Trim();
                                        }
                                        continue;
                                    }
                                    #region Header
                                    if (i == 3)
                                    {
                                        if (_existingStatementID <= 0)
                                        {
                                            _BnkStatm = new MBankStatement(Env.GetCtx(), 0, null);
                                            _BnkStatm.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                            _BnkStatm.SetAD_Org_ID(_AD_Org_ID);
                                            _BnkStatm.SetC_BankAccount_ID(_C_BankAccount_ID);
                                            _BnkStatm.SetName(_statementno);
                                            _BnkStatm.SetStatementDate(DateTime.Now);
                                            if (!_BnkStatm.Save())
                                            {
                                                _obj._error = "VA012_BankStatementHeaderNotSaved";
                                                return _obj;
                                            }
                                            else
                                                _stementID = _BnkStatm.Get_ID();
                                        }
                                        else
                                            _BnkStatm = new MBankStatement(Env.GetCtx(), _existingStatementID, null);
                                    }
                                    #endregion
                                    #region Rest All Other Entries Which Contains Data
                                    else
                                    {
                                        _date = "";
                                        if ((Util.GetValueOfString(dt.Rows[i][0]) != string.Empty) && (Util.GetValueOfString(dt.Rows[i][1]) != string.Empty))
                                        {
                                            bool isDiffCulture = false;
                                            if (dt.Rows[i][0].ToString().Contains('.'))
                                            {
                                                isDiffCulture = true;
                                                _dateList = dt.Rows[i][0].ToString().Split('.');
                                                if (_dateList.Length == 3)
                                                    _date = _dateList[1].ToString() + "/" + _dateList[0].ToString() + "/" + _dateList[2].ToString();// MM/DD/YYYY
                                            }
                                            else
                                                _date = dt.Rows[i][0].ToString();
                                            _BnkStmtLine = new MBankStatementLine(_BnkStatm);
                                            _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                            _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                            _BnkStmtLine.SetVA012_Page(pageno);
                                            _BnkStmtLine.SetLine(lineno);
                                            lineno = lineno + 10;
                                            _BnkStmtLine.SetStatementLineDate(DateTime.Parse(_date));// Set Transaction Date
                                            _BnkStmtLine.SetDateAcct(DateTime.Parse(_date));// Set Transaction Date
                                            _BnkStmtLine.SetValutaDate(DateTime.Parse(_date));// Set Transaction Date
                                            _BnkStmtLine.SetReferenceNo(_IBAN);// Set Transaction Remarks
                                            _BnkStmtLine.SetDescription(Util.GetValueOfString(dt.Rows[i][1]));// Set Transaction Purticular
                                            _BnkStmtLine.SetMemo(_branchName);// Set Deposite Branch
                                            if (Util.GetValueOfString(dt.Rows[i][2]) != "")
                                                _BnkStmtLine.SetEftCheckNo(Util.GetValueOfString(dt.Rows[i][2]));

                                            if (Util.GetValueOfString(dt.Rows[i][6]) != "")
                                            {
                                                _c_bpartner_id = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT C_BPartner_ID FROM C_BPartner
                                                WHERE AD_Client_ID = " + ctx.GetAD_Client_ID() +
                                                @" AND ( LOWER(Value)= LOWER(" + GetConvertedValue(Util.GetValueOfString(dt.Rows[i][6])) + @") 
                                                   OR LOWER(Name)= LOWER(" + GetConvertedValue(Util.GetValueOfString(dt.Rows[i][6])) + "))"));
                                                _BnkStmtLine.SetC_BPartner_ID(_c_bpartner_id);
                                            }
                                            //set charge id if charge value is available in Bank Statement 7 column
                                            if (Util.GetValueOfString(dt.Rows[i][7]) != "")
                                            {
                                                _C_Charge_ID = Util.GetValueOfInt(DB.ExecuteScalar(@"SELECT C_Charge_ID FROM C_Charge
                                                    WHERE AD_Client_ID IN (0,  " + ctx.GetAD_Client_ID() +
                                                    @") AND LOWER(Value)= LOWER(" + GetConvertedValue(Util.GetValueOfString(dt.Rows[i][7])) + ")"));
                                                _BnkStmtLine.SetC_Charge_ID(_C_Charge_ID);
                                                //If chanrge id is available then set charge amount and statement amount on bank statement line suggested by Ashish.
                                                if (_C_Currency_ID > 0)
                                                    _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
                                                if ((Util.GetValueOfString(dt.Rows[i][3]) != string.Empty) && (Util.GetValueOfString(dt.Rows[i][3]) != "0"))
                                                {
                                                    _payAmt = GetAmount(dt.Rows[i][3].ToString(), isDiffCulture);
                                                }
                                                else
                                                {
                                                    _payAmt = GetAmount(dt.Rows[i][4].ToString(), isDiffCulture);
                                                }

                                                if ((Util.GetValueOfString(dt.Rows[i][3]) != string.Empty) && (Util.GetValueOfString(dt.Rows[i][3]) != "0"))
                                                {
                                                    if (_payAmt != 0)
                                                    {
                                                        _BnkStmtLine.SetStmtAmt(Util.GetValueOfDecimal("-" + _payAmt));
                                                        _BnkStmtLine.SetChargeAmt(Util.GetValueOfDecimal("-" + _payAmt));
                                                        _BnkStmtLine.SetTrxAmt(Util.GetValueOfDecimal(0));
                                                    }
                                                    else
                                                    {
                                                        _BnkStmtLine.SetStmtAmt(0);
                                                        _BnkStmtLine.SetChargeAmt(0);
                                                        _BnkStmtLine.SetTrxAmt(Util.GetValueOfDecimal(0));
                                                    }
                                                }
                                                else
                                                {
                                                    _BnkStmtLine.SetStmtAmt(_payAmt);
                                                    _BnkStmtLine.SetChargeAmt(_payAmt);
                                                    _BnkStmtLine.SetTrxAmt(Util.GetValueOfDecimal(0));
                                                }
                                            }
                                            else
                                            {

                                                if (_C_Currency_ID > 0)
                                                    _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type
                                                if ((Util.GetValueOfString(dt.Rows[i][3]) != string.Empty) && (Util.GetValueOfString(dt.Rows[i][3]) != "0"))
                                                {
                                                    _payAmt = GetAmount(dt.Rows[i][3].ToString(), isDiffCulture);
                                                }
                                                else
                                                {
                                                    _payAmt = GetAmount(dt.Rows[i][4].ToString(), isDiffCulture);
                                                }

                                                if ((Util.GetValueOfString(dt.Rows[i][3]) != string.Empty) && (Util.GetValueOfString(dt.Rows[i][3]) != "0"))
                                                {
                                                    if (_payAmt != 0)
                                                    {
                                                        _BnkStmtLine.SetStmtAmt(Util.GetValueOfDecimal("-" + _payAmt));
                                                        _BnkStmtLine.SetTrxAmt(Util.GetValueOfDecimal("-" + _payAmt));
                                                    }
                                                    else
                                                    {
                                                        _BnkStmtLine.SetStmtAmt(0);
                                                        _BnkStmtLine.SetTrxAmt(0);
                                                    }
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
                                #endregion ENBD Format

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
                _obj._error = "VA012_ErrorInFileFormat";
                return _obj;
            }
        }
        /// <summary>
        /// Get Date from string date
        /// </summary>
        /// <param name="_date"></param>
        /// <returns>DateTime</returns>
        public DateTime? GetDate(string _date)
        {
            string[] _dateArray = _date.Split('/');
            DateTime? _returnDate = new DateTime(Util.GetValueOfInt(_dateArray[2]), Util.GetValueOfInt(_dateArray[1]), Util.GetValueOfInt(_dateArray[0]));
            return _returnDate;
        }

        /// <summary>
        /// This function is used to handle the Single QUOTE and &ampersand in where clause
        /// </summary>
        /// <param name="value">value</param>
        /// <returns>converted value</returns>
        private String GetConvertedValue(String value)
        {
            value = DB.TO_STRING(value);
            //value = value.Replace("&", "&'||'");
            return value;
        }

        /// <summary>
        /// Used to fetching amount in decimal format
        /// </summary>
        /// <param name="_amt"></param>
        /// <returns>Decimal</returns>
        public Decimal GetAmount(string _amt)
        {
            Decimal _number = 0;
            //int divNum = 0;
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            _log.Log(Level.INFO, " _amt --> " + _amt);
            System.Globalization.NumberFormatInfo info = new System.Globalization.NumberFormatInfo();
            _number = Convert.ToDecimal(_amt, System.Globalization.CultureInfo.GetCultureInfo("no"));
            _log.Log(Level.INFO, " _number --> " + _number);
            return _number;
        }
        /// <summary>
        /// Used to fetching amount If isDiffCulture is true then amount will formatted depending upon culture.
        /// </summary>
        /// <param name="_amt"></param>
        /// <param name="isDiffCulture"></param>
        /// <returns>Decimal</returns>
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