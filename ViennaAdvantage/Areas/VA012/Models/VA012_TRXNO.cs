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
using System.Threading;
using System.Globalization;

namespace VA012.Models
{
    public class VA012_TRXNO
    {
        #region Variables
        int _AD_Org_ID = 0;
        int _C_BankAccount_ID = 0, Count = 0;
        int _c_payment_id = 0, _C_Currency_ID = 0;
        decimal? _payAmt = 0;
        decimal? _trxAmt = 0;
        string _FileName = string.Empty, _doctype = string.Empty;
        string _message = string.Empty, _date = string.Empty;
        MBankStatement _BnkStatm = null;
        MBankStatementLine _BnkStmtLine = null;
        DataSet PyDS = null;
        StringBuilder query = null, _Filenames = new StringBuilder();
        string _Extension = string.Empty, _FileLocation = string.Empty;
        #endregion        

        public StatementResponse ImportStatement(Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges)
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

                if ((_Extension.ToUpper() == ".CSV") || (_Extension.ToUpper() == ".XLSX") || (_Extension.ToUpper() == ".XLS"))
                {
                    DataSet ds = ExcelImport.ImportFromCSV(_path, false);

                    if (File.Exists(_path))
                    {
                        FileInfo fileToDelete = new FileInfo(_path);
                        fileToDelete.Delete();                       
                    }

                    if (ds != null && ds.Tables.Count>0)
                    {
                        #region [NEW FORMAT]

                        DataTable dt = ds.Tables[0];

                        if (dt.Rows.Count > 0)
                        {
                            try
                            {

                                if (!dt.Columns.Contains("MERCHANT_ID")
                                    || !dt.Columns.Contains("CHAIN_ID")
                                    || !dt.Columns.Contains("AUTHORIZATION_CODE"))
                                {
                                    _obj._error = "VA012_ErrorInFileFormat";
                                    return _obj;
                                }


                                //int chargeID = Util.GetValueOfInt(DB.ExecuteScalar("SELECT C_Charge_ID FROM C_Charge WHERE DTD001_ChargeType = 'OTH' AND AD_CLIENT_ID =" + ctx.GetAD_Client_ID()));
                                int chargeID = Util.GetValueOfInt(_statementCharges);
                                for (int i = -1; i < dt.Rows.Count; i++)
                                {
                                    #region Header

                                    if (i == -1)
                                    {
                                        _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i + 1][8]).ToString().Trim() + "'"));
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
                                            _BnkStatm.SetStatementDate(DateTime.Now);

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
                                        if (!string.IsNullOrEmpty(dt.Rows[i][1].ToString().Trim()) && !string.IsNullOrEmpty(dt.Rows[i][2].ToString().Trim())) //if it is not a sum of rows
                                        {
                                            _BnkStmtLine = new MBankStatementLine(_BnkStatm);
                                            _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                            _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                            _BnkStmtLine.SetVA012_Page(pageno);
                                            _BnkStmtLine.SetLine(lineno);
                                            lineno = lineno + 10;
                                            _BnkStmtLine.SetStatementLineDate(Convert.ToDateTime(DateTime.ParseExact(dt.Rows[i][9].ToString().Trim(), "dd/MM/yyyy", null)));// Set Transaction Date
                                            _BnkStmtLine.SetDateAcct(Convert.ToDateTime(DateTime.ParseExact(dt.Rows[i][9].ToString().Trim(), "dd/MM/yyyy", null)));// Set Transaction Date
                                            _BnkStmtLine.SetValutaDate(Convert.ToDateTime(DateTime.ParseExact(dt.Rows[i][9].ToString().Trim(), "dd/MM/yyyy", null)));// Set Transaction Date
                                            _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][19]).Trim());// Set Reference No.
                                            _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][19]).Trim());// Set Reference No.
                                            _BnkStmtLine.SetMemo(Convert.ToString(dt.Rows[i][3]).Trim() + " " + Convert.ToString(dt.Rows[i][4]).Trim());// Set Merchant name and Location                                    

                                            // _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= '" + (dt.Rows[i][8].ToString().Trim()) + "'"));
                                            if (_C_Currency_ID > 0)
                                                _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);// Set Currency Type

                                            if ((Convert.ToString(dt.Rows[i][17]).Trim() != string.Empty) && (Convert.ToString(dt.Rows[i][17]).Trim() != "0"))
                                            {
                                                _payAmt = Convert.ToDecimal(dt.Rows[i][17].ToString().Trim());
                                            }

                                            if ((Convert.ToString(dt.Rows[i][14]).Trim() != string.Empty) && (Convert.ToString(dt.Rows[i][14]).Trim() != "0"))
                                            {
                                                _trxAmt = Convert.ToDecimal(dt.Rows[i][14].ToString().Trim());
                                            }

                                            if (_accountType == "C")
                                            {

                                                if ((Convert.ToString(dt.Rows[i][14]).Trim() != string.Empty) && (Convert.ToString(dt.Rows[i][14]).Trim() != "0"))
                                                {
                                                    _BnkStmtLine.SetStmtAmt(_payAmt);
                                                    _BnkStmtLine.SetTrxAmt(_trxAmt);
                                                }
                                                else
                                                {
                                                    _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                                    _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _trxAmt));
                                                }
                                            }
                                            else
                                            {
                                                if ((Convert.ToString(dt.Rows[i][14]).Trim() != string.Empty) && (Convert.ToString(dt.Rows[i][15]).Trim() != "0"))
                                                {
                                                    _BnkStmtLine.SetStmtAmt(Convert.ToDecimal("-" + _payAmt));
                                                    _BnkStmtLine.SetTrxAmt(Convert.ToDecimal("-" + _trxAmt));
                                                }
                                                else
                                                {
                                                    _BnkStmtLine.SetStmtAmt(_payAmt);
                                                    _BnkStmtLine.SetTrxAmt(_trxAmt);
                                                }
                                            }

                                            //if charges present 
                                            if ((Convert.ToString(dt.Rows[i][15]).Trim() != string.Empty) && (Convert.ToString(dt.Rows[i][15]).Trim() != "0"))
                                            {
                                                _BnkStmtLine.SetChargeAmt(Convert.ToDecimal(dt.Rows[i][15].ToString().Trim()));
                                                _BnkStmtLine.SetC_Charge_ID(chargeID);

                                                var _sql = "SELECT C_TAX_ID FROM C_TAX WHERE EXPORT_ID IS NOT NULL AND ISDEFAULT = 'Y' AND IsActive='Y'";
                                              // _sql = MRole.GetDefault(ctx).AddAccessSQL(_sql, "C_Tax", true, false);
                                               int _C_Tax_ID = Convert.ToInt32(DB.ExecuteScalar(_sql));
                                               
                                                if (_C_Tax_ID > 0)
                                                {
                                                    _BnkStmtLine.SetC_Tax_ID(_C_Tax_ID);
                                                    var Rate = Util.GetValueOfDecimal(DB.ExecuteScalar("Select Rate From C_Tax Where C_Tax_ID=" + _C_Tax_ID + " AND IsActive='Y'"));
                                                    if (Rate > 0)
                                                    {
                                                        var TaxAmt = Math.Round(Util.GetValueOfDecimal(_BnkStmtLine.GetChargeAmt() - (_BnkStmtLine.GetChargeAmt() / ((Rate / 100) + 1))), 2);
                                                        _BnkStmtLine.SetTaxAmt(TaxAmt);
                                                    }
                                                    else
                                                    {
                                                        _BnkStmtLine.SetTaxAmt(0);
                                                    }
                                                }
                                            }

                                            _BnkStmtLine.Set_Value("TrxNo", Convert.ToString(dt.Rows[i][12]).Trim());

                                            if (!_BnkStmtLine.Save())
                                            {

                                            }
                                        }
                                    }
                                    #endregion

                                    _BnkStatm.SetEndingBalance(_BnkStatm.GetBeginningBalance() + _BnkStatm.GetStatementDifference());
                                    if (!_BnkStatm.Save())
                                    {
                                        _obj._error = "VA012_BeginningBalanceNotUpdated";
                                        return _obj;
                                    }
                                }
                            }
                            catch (Exception ex)
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
                        #endregion [NEW FORMAT]
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

        public DateTime? GetDate(string _date)
        {
            string[] _dateArray = _date.Split('/');
            DateTime? _returnDate = new DateTime(Convert.ToInt32(_dateArray[2]), Convert.ToInt32(_dateArray[1]), Convert.ToInt32(_dateArray[0]));
            return _returnDate;
        }
    }
}