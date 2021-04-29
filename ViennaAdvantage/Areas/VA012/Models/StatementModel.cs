using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using VAdvantage.Classes;
using VAdvantage.DataBase;
using VAdvantage.Logging;
using VAdvantage.Model;
using VAdvantage.Utility;

namespace VA012.Models
{
    public class StatementModel
    {
        /// <summary>
        /// Get Account Date, Amount and ConversionType
        /// to set the values on to the Statement Line when User select the CashLine
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">Window Tab Field Values</param>
        /// <returns>List of DateAcct and Line amount</returns>
        public Dictionary<string, object> GetAcctDate(Ctx ctx, string fields)
        {
            Dictionary<string, object> _list = new Dictionary<string, object>();
            string[] paramValue = fields.ToString().Split(',');
            //Assign parameter value
            int cashLine_Id = Util.GetValueOfInt(paramValue[0].ToString());
            int currency_Id= Util.GetValueOfInt(paramValue[1].ToString());
            DateTime? stmtDate = Convert.ToDateTime(paramValue[2].ToString());
            int org_Id = Util.GetValueOfInt(paramValue[3].ToString());
            DataSet _ds = DB.ExecuteDataset("SELECT c.DateAcct, cl.Amount, cl.C_ConversionType_ID, cl.C_Currency_ID FROM C_Cash c INNER JOIN C_CashLine cl ON c.C_Cash_ID=cl.C_Cash_ID WHERE c.IsActive='Y' AND cl.C_CashLine_ID=" + cashLine_Id, null, null);
            if (_ds != null && _ds.Tables[0].Rows.Count > 0)
            {
                _list["DateAcct"] = Convert.ToDateTime(_ds.Tables[0].Rows[0]["DateAcct"]);
                _list["C_ConversionType_ID"] = Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]);
                //Amount is Converting according to the Currency Conversion
                if (currency_Id != Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]))
                {
                    _list["Amount"] = MConversionRate.Convert(ctx, Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["Amount"]), Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_Currency_ID"]), currency_Id, stmtDate, Util.GetValueOfInt(_ds.Tables[0].Rows[0]["C_ConversionType_ID"]), ctx.GetAD_Client_ID(), org_Id);
                }
                else
                {
                    _list["Amount"] = Util.GetValueOfDecimal(_ds.Tables[0].Rows[0]["Amount"]);
                }
            }
            return _list;
        }
    }
}