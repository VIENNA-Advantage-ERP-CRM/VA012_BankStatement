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
        /// Get Account Date 
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="fields">Window Tab Field Values</param>
        /// <returns>DateTime</returns>
        public DateTime? GetAcctDate(Ctx ctx, string fields)
        {
            string[] paramValue = fields.ToString().Split(',');
            //Assign parameter value
            int cashLine_Id = Util.GetValueOfInt(paramValue[0].ToString());
            return Convert.ToDateTime(DB.ExecuteScalar("SELECT c.DateAcct FROM C_Cash c INNER JOIN C_CashLine cl ON c.C_Cash_ID=cl.C_Cash_ID WHERE c.IsActive='Y' AND cl.C_CashLine_ID=" + cashLine_Id, null, null));
        }
    }
}