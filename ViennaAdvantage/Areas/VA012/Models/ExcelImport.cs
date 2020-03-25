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

    public class ExcelImport
    {
        #region  Function to Import data from CSV File
        public static DataSet ImportFromCSV(string _FileLocation, bool _HasHeader, int _IMEX = 0)
        {
          //  Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            string HDR = _HasHeader ? "Yes" : "No";
            string strConn = string.Empty;
            string _fileExtension = _FileLocation.Substring(_FileLocation.LastIndexOf('.')).ToLower();

            if (_fileExtension == ".xlsx" || _fileExtension == ".xls")
            {
                strConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + _FileLocation + ";Extended Properties=\"Excel 12.0;HDR=" + HDR + ";IMEX=" + _IMEX + "\"";
            }
            else
            {
                strConn = string.Format(
                        @"Provider=Microsoft.Jet.OleDb.4.0; Data Source={0};Extended Properties=""Text;HDR=YES;FMT=Delimited""",
                            Path.GetDirectoryName(_FileLocation));
            }
            DataSet output = new DataSet();
            try
            {
                if (_fileExtension == ".xlsx" || _fileExtension == ".xls" || _fileExtension == ".csv")
                {
                    using (OleDbConnection oledbconn = new OleDbConnection(strConn))
                    {
                        oledbconn.Open();
                        System.Data.DataTable schemaTable = null;


                        if (_fileExtension == ".csv")
                        {
                            schemaTable = oledbconn.GetOleDbSchemaTable(
                            OleDbSchemaGuid.Tables, new object[] { null, null, _FileLocation.Substring(_FileLocation.LastIndexOf('\\') + 1).Replace('.', '#'), "TABLE" });
                        }
                        else
                        {
                            schemaTable = oledbconn.GetOleDbSchemaTable(
                         OleDbSchemaGuid.Tables, new object[] { null, null, null, "TABLE" });
                        }

                        foreach (DataRow schemaRow in schemaTable.Rows)
                        {
                            string sheet = schemaRow["TABLE_NAME"].ToString();

                            if (!sheet.EndsWith("_"))
                            {
                                try
                                {
                                    OleDbCommand cmd = new OleDbCommand("SELECT * FROM [" + sheet + "]", oledbconn);
                                    cmd.CommandType = CommandType.Text;
                                    System.Data.DataTable outputTable = new System.Data.DataTable(sheet);
                                    output.Tables.Add(outputTable);
                                    new OleDbDataAdapter(cmd).Fill(outputTable);
                                }
                                catch (Exception ex)
                                {
                                    return null;
                                }
                            }
                        }
                    }
                }
                else if (_fileExtension == ".txt")
                {
                    DataTable dt = new DataTable();

                    dt.Columns.AddRange(new DataColumn[11] { new DataColumn("CustomerCode"), new DataColumn("VendorCode"), new DataColumn("Amount"), new DataColumn("TrxDate"), new DataColumn("UTRNo"), new DataColumn("IFSCCode"), new DataColumn("CreditAccountNo"), new DataColumn("VendorName"), new DataColumn("Mode"), new DataColumn("PaymentMode"), new DataColumn("BeneficiaryBank") });
                    StringBuilder sb = new StringBuilder();
                    List<string> list = new List<string>();
                    using (StreamReader sr = new StreamReader(_FileLocation))
                    {
                        while (sr.Peek() >= 0)
                        {
                            list.Add(sr.ReadLine());
                        }
                    }
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] == "" || list[i] == " " || list[i] == null)
                        {
                            continue;
                        }
                        string[] strlist = list[i].Split('|');
                        dt.Rows.Add(strlist[0], strlist[1], strlist[2], strlist[3], strlist[4], strlist[5], strlist[6], strlist[7], strlist[8], strlist[9], strlist[10]);
                    }
                    output.Tables.Add(dt);
                }
            }
            catch (Exception ex)
            {
                return output;
            }
            return output;
        }
        #endregion

    }


}