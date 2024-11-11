/*******************************************************
       * Module Name    : VAS
       * Purpose        : Bank charge Summary-Month wise widget Model
       * Chronological  : Development
       * Created Date   : 7 Nov, 2024
       * Created by     : VIS103
******************************************************/
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using VAdvantage.DataBase;
using VAdvantage.Utility;

namespace VA012.Models
{
    public class VA012_BankChargeSummaryModel
    {
        StringBuilder sql = new StringBuilder("");
        /// <summary>
        /// This function is used to get the Financial Year Details
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="ErrorMessage">Error Message if any</param>
        /// <returns>DataSet</returns>
        /// <author>VIS103</author>
        public DataSet GetFinancialYearDetail(Ctx ctx, out string ErrorMessage)
        {
            ErrorMessage = "";
            DataSet ds = null;
            sql.Append(@"SELECT cy.c_year_id
                         FROM C_Calendar cc
                         INNER JOIN AD_ClientInfo ci ON (ci.C_Calendar_ID=cc.C_Calendar_ID)
                         INNER JOIN C_Year cy ON (cy.C_Calendar_ID=cc.C_Calendar_ID)
                         INNER JOIN C_Period cp  ON (cy.C_Year_ID = cp.C_Year_ID)
                         WHERE cy.IsActive = 'Y'
                         AND cp.IsActive = 'Y'
                         AND ci.IsActive='Y'
                         AND TRUNC(SYSDATE) BETWEEN cp.StartDate AND cp.EndDate AND cc.AD_Client_ID=" + ctx.GetAD_Client_ID());
            int Year_ID = Util.GetValueOfInt(DB.ExecuteScalar(sql.ToString(), null, null));
            if (Year_ID > 0)
            {
                sql.Clear();
                sql.Append("SELECT NAME,STARTDATE,ENDDATE,PERIODNO,C_YEAR_ID FROM C_PERIOD WHERE ISACTIVE='Y' AND C_YEAR_ID=" + Year_ID + " ORDER BY PERIODNO");
                ds = DB.ExecuteDataset(sql.ToString(), null, null);
            }
            else
            {
                ErrorMessage = Msg.GetMsg(ctx, "VA012_CalendarNotFound");
            }
            return ds;
        }
        /// <summary>
        /// Get Bank Satement line data against charge
        /// </summary>
        /// <param name="ctx">Context</param>
        /// <param name="C_BankAccount_ID">Selected Bank Account</param>
        /// <param name="C_Charge_ID">Selected Charge Parameter if any</param>
        /// <param name="yrStartDate">Year Start Date</param>
        /// <param name="yrEndDate">Year End Date</param>
        /// <param name="Year_ID">Year ID</param>
        /// <param name="errorMessage">out paramter to return error message</param>
        /// <returns>return list of labels and bank statement data</returns>
        public VA012_BankChargeData GetBankChargeData(Ctx ctx, int C_BankAccount_ID, int C_Charge_ID, DateTime yrStartDate, DateTime yrEndDate, int Year_ID, out string errorMessage)
        {
            errorMessage = "";
            string[] labels = null;
            decimal[] chargeAmt = null;
            VA012_BankChargeData bankData = new VA012_BankChargeData();
            //Get Bank Charge and Periods data
            sql.Append(@"WITH BANKSTATEMENTDATA AS (
                        SELECT
                            C_BANKSTATEMENTLINE.CHARGEAMT,
                            TO_CHAR(
                                STATEMENTLINEDATE, 'mm'
                            ) AS PERIODNO
                        FROM
                            C_BANKSTATEMENTLINE
                            INNER JOIN C_BANKSTATEMENT ON ( C_BANKSTATEMENT.C_BANKSTATEMENT_ID = C_BANKSTATEMENTLINE.C_BANKSTATEMENT_ID )
                            INNER JOIN C_CHARGE ON ( C_BANKSTATEMENTLINE.C_CHARGE_ID = C_CHARGE.C_CHARGE_ID )
                        WHERE
                            C_BANKSTATEMENTLINE.ISACTIVE = 'Y'
                            AND C_BANKSTATEMENT.ISACTIVE = 'Y'
                            AND C_CHARGE.ISACTIVE = 'Y'
                            AND STATEMENTLINEDATE BETWEEN " + GlobalVariable.TO_DATE(yrStartDate, true) + " AND " + GlobalVariable.TO_DATE(yrEndDate, true)
                           + @" AND C_BANKSTATEMENTLINE.C_CHARGE_ID > 0 AND C_BANKSTATEMENT.DocStatus IN ('CO','CL')");
            if (C_BankAccount_ID > 0)
            {
                sql.Append(@" AND C_BANKSTATEMENT.C_BANKACCOUNT_ID=" + C_BankAccount_ID);
            }
            if (C_Charge_ID > 0)
            {
                sql.Append(@" AND C_BANKSTATEMENTLINE.C_Charge_ID=" + C_Charge_ID);
            }
            sql.Append(@")
                                SELECT
                                    SUM(COALESCE(
                                        BANKSTATEMENTDATA.CHARGEAMT, 0
                                    )) AS CHARGEAMT,
                                    PERIODDATA.PERIODNO,
                                    PERIODDATA.NAME
                                FROM
                                    (
                                        SELECT
                                            NAME,
                                            PERIODNO
                                        FROM
                                            C_PERIOD
                                        WHERE
                                            ISACTIVE = 'Y'
                                            AND C_YEAR_ID = " + Year_ID + @"
                                    ) PERIODDATA
                                    LEFT JOIN BANKSTATEMENTDATA ON (PERIODDATA.PERIODNO = BANKSTATEMENTDATA.PERIODNO)
                                GROUP BY
                                    PERIODDATA.PERIODNO,
                                    PERIODDATA.NAME
                                ORDER BY
                                    PERIODDATA.PERIODNO");
            DataSet ds = DB.ExecuteDataset(sql.ToString(), null, null);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                chargeAmt = new decimal[ds.Tables[0].Rows.Count];
                labels = new string[ds.Tables[0].Rows.Count];
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    chargeAmt[i] =Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["CHARGEAMT"]);
                    labels[i]=Util.GetValueOfString(ds.Tables[0].Rows[i]["NAME"]);
                }
                bankData.bankChargeData=chargeAmt;
                bankData.labels = labels;
            }
            else
            {
                errorMessage = Msg.GetMsg(ctx, "VA012_NoDataFound");
            }
            return bankData;
        }
        /// <summary>
        /// Properties declaration
        /// </summary>
        public class VA012_BankChargeData
        {
            public decimal[] bankChargeData { get; set; }

            public string[] labels { get; set; }
        }
    }
}