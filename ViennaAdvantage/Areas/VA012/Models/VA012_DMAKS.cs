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

namespace VA012.Models
{
    public class VA012_DMAKS
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
        public StatementResponse ImportStatement(Ctx ctx, string FileName, string _path, int _bankaccount, int _bankAccountCurrency, string _statementno, string _statementCharges)
        {
            StatementResponse _obj = new StatementResponse();
            try
            {
                
                _C_Currency_ID = Convert.ToInt32(DB.ExecuteScalar("Select C_Currency_ID from C_Currency Where iso_code= 'INR'"));
                if (_C_Currency_ID != _bankAccountCurrency)
                {
                    _obj._error = "VA012_DiffAccountAndStatementCurrency";
                    return _obj;
                }
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


                _message = _Filenames.ToString();
                string[] _filenamesall = _message.Split(',');
                for (int K = 0; K < _filenamesall.Length; K++)
                {
                    _FileLocation = _filenamesall[K].ToString();
                    string[] _FileNameExten = _FileLocation.Split('.');
                    _FileName = _FileNameExten[0].ToString();
                    _Extension = "." + _FileNameExten[1].ToString();


                    //if ((_Extension.ToUpper() == ".CSV") || (_Extension.ToUpper() == ".XLSX") || (_Extension.ToUpper() == ".XLS") || (_Extension.ToUpper() == ".TXT"))
                        if (_Extension.ToUpper() == ".TXT")
                    {
                        
                        DataSet ds = ExcelImport.ImportFromCSV(_path, false);
                        if (File.Exists(_path))
                        {
                            FileInfo fileToDelete = new FileInfo(_path);
                            fileToDelete.Delete();
                        }
                        if (ds != null)
                        {
                            DataTable dt = ds.Tables[0];
                            StringBuilder _qry = new StringBuilder();
                            Boolean _inward = true;
                            int _bpID = 0;
                            _qry.Clear();
                            if (dt.Rows.Count > 0)
                            {
                                //for (int i = 0; i < dt.Rows.Count - 2; i++)
                                for (int i = 0; i < dt.Rows.Count ; i++)
                                {
                                    if (i == 0)
                                    {
                                        #region Statement Header
                                        if (_existingStatementID <= 0)
                                        {
                                            _BnkStatm = new MBankStatement(Env.GetCtx(), 0, null);
                                            _BnkStatm.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                            _BnkStatm.SetAD_Org_ID(_AD_Org_ID);
                                            _BnkStatm.SetC_BankAccount_ID(_C_BankAccount_ID);
                                            _BnkStatm.SetName(_statementno);
                                            _BnkStatm.SetStatementDate(System.DateTime.Now);
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
                                        #endregion
                                    }

                                        #region Statement Line
                                        _BnkStmtLine = new MBankStatementLine(_BnkStatm);
                                        #region Check Inward Or Outward
                                        _qry.Clear();
                                        _qry.Append(@"SELECT COUNT(VA017_INWARDPAYMENTPREFIX) AS INWARD
                                                FROM VA017_ICICI_PAYMENT
                                                WHERE ISACTIVE                         ='Y'
                                                AND C_BANKACCOUNT_ID                   =" + _C_BankAccount_ID
                                                   + @" AND TRIM(UPPER(VA017_INWARDPAYMENTPREFIX))=TRIM(UPPER('" + Convert.ToString(dt.Rows[i][0]) + "'))");
                                        if (Util.GetValueOfInt(DB.ExecuteScalar(_qry.ToString())) > 0)
                                        {
                                            _inward = true;
                                        }
                                        else
                                        {
                                            _qry.Clear();
                                            _qry.Append(@"SELECT COUNT(VA017_OUTWARDPAYMENTPREFIX) AS OUTWARD
                                                FROM VA017_ICICI_PAYMENT
                                                WHERE ISACTIVE                         ='Y'
                                                AND C_BANKACCOUNT_ID                   =" + _C_BankAccount_ID
                                                    + @" AND TRIM(UPPER(VA017_OUTWARDPAYMENTPREFIX))=TRIM(UPPER('" + Convert.ToString(dt.Rows[i][0]) + "'))");
                                            if (Util.GetValueOfInt(DB.ExecuteScalar(_qry.ToString())) > 0)
                                            {
                                                _inward = false;
                                            }
                                            else
                                            {
                                                _obj._error = "VA012_SetInwardOutwardPrefix";
                                                return _obj;
                                            }
                                        }
                                        #endregion Check Inward Or Outward

                                        _BnkStmtLine.SetAD_Client_ID(ctx.GetAD_Client_ID());
                                        _BnkStmtLine.SetAD_Org_ID(ctx.GetAD_Org_ID());
                                        _BnkStmtLine.SetVA012_Page(pageno);
                                        _BnkStmtLine.SetLine(lineno);
                                        lineno = lineno + 10;
                                        _BnkStmtLine.SetReferenceNo(Convert.ToString(dt.Rows[i][1]));

                                        ////
                                        _qry.Clear();
                                        _qry.Append("SELECT C_BPARTNER_ID FROM C_BPARTNER WHERE C_BPARTNER_ID=" + Util.GetValueOfInt(dt.Rows[i][1]));
                                        _bpID = Util.GetValueOfInt(DB.ExecuteScalar(_qry.ToString()));
                                        if (_bpID > 0)
                                        {
                                            _BnkStmtLine.SetC_BPartner_ID(_bpID);
                                        }
                                        ////

                                        if (_inward)
                                        {
                                            _BnkStmtLine.SetStmtAmt(Math.Abs(Convert.ToDecimal(dt.Rows[i][2])));
                                            _BnkStmtLine.SetTrxAmt(Math.Abs(Convert.ToDecimal(dt.Rows[i][2])));
                                        }
                                        else
                                        {
                                            _BnkStmtLine.SetStmtAmt(Decimal.Negate(Math.Abs(Convert.ToDecimal(dt.Rows[i][2]))));
                                            _BnkStmtLine.SetTrxAmt(Decimal.Negate(Math.Abs(Convert.ToDecimal(dt.Rows[i][2]))));
                                        }
                                       
                                        _BnkStmtLine.SetStatementLineDate(GetDate(dt.Rows[i][3].ToString()));
                                        _BnkStmtLine.SetDateAcct(GetDate(dt.Rows[i][3].ToString()));
                                        _BnkStmtLine.SetEftTrxID(Convert.ToString(dt.Rows[i][4]));
                                        _BnkStmtLine.SetEftReference(Convert.ToString(dt.Rows[i][5]));
                                        _BnkStmtLine.SetEftPayeeAccount(Convert.ToString(dt.Rows[i][6]));
                                        _BnkStmtLine.SetDescription(Convert.ToString(dt.Rows[i][7]));
                                        _BnkStmtLine.SetEftMemo(Convert.ToString(dt.Rows[i][8]));
                                        _BnkStmtLine.SetEftTrxType(Convert.ToString(dt.Rows[i][9]));
                                        _BnkStmtLine.SetEftPayee(Convert.ToString(dt.Rows[i][10]));

                                        if (_C_Currency_ID > 0)
                                        {
                                            _BnkStmtLine.SetC_Currency_ID(_C_Currency_ID);
                                        }

                                        if (dt.Columns.Count > 11)
                                        {
                                            _BnkStmtLine.SetEftCheckNo(Convert.ToString(dt.Rows[i][11]));
                                        }
                                        if (!_BnkStmtLine.Save())
                                        {

                                        }
                                        #endregion
                                    
                                }
                                _BnkStatm.SetEndingBalance(_BnkStatm.GetBeginningBalance() + _BnkStatm.GetStatementDifference());
                                if (!_BnkStatm.Save())
                                {
                                    _obj._error = "VA012_EndingBalanceNotUpdated";
                                    return _obj;
                                }
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
                _obj._error = "VA012_ErrorInFileFormat"; ;
                return _obj;
            }
        }
        //#region  Function to Import data from CSV File
        //public DataSet ImportFromCSV(string _FileLocation, bool _HasHeader)
        //{
        //    string HDR = _HasHeader ? "Yes" : "No";
        //    string strConn = string.Empty;
        //    string _fileExtension = _FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower();

        //    if (_fileExtension == ".xlsx")
        //    {
        //        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
        //    }
        //    else if (_fileExtension == ".xls")
        //    {
        //        strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=0\"";
        //    }
        //    else
        //    {
        //        strConn = string.Format(
        //                @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
        //                    Path.GetDirectoryName(_FileLocation));
        //    }
        //    DataSet output = new DataSet();

        //    try
        //    {
        //        if (_fileExtension == ".xlsx")
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

        public DateTime? GetDate(string _date)
        {
            string[] _dateArray =_date.Split('/');
            DateTime? _returnDate = new DateTime(Convert.ToInt32(_dateArray[2]), Convert.ToInt32(_dateArray[1]), Convert.ToInt32(_dateArray[0]));
            return _returnDate;
        }
    }


}