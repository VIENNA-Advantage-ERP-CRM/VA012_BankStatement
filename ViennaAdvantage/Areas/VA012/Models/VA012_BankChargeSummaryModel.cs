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
using System.Dynamic;
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
        /// <returns>Dynamic Object</returns>
        /// <author>VIS103</author>
        public dynamic GetFinancialYearDetail(Ctx ctx)
        {
            DataSet ds = null;
            dynamic data = new ExpandoObject();
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
                sql.Append("SELECT MIN(StartDate) as StartDate,MAX(EndDate) AS EndDate,C_Year_ID FROM C_PERIOD WHERE ISACTIVE='Y' AND C_YEAR_ID=" + Year_ID + " GROUP BY C_Year_ID");
                ds = DB.ExecuteDataset(sql.ToString(), null, null);
                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
                {
                    data.StartDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[0]["StartDate"]);
                    data.EndDate = Util.GetValueOfDateTime(ds.Tables[0].Rows[0]["EndDate"]);
                    data.C_Year_ID = Util.GetValueOfInt(ds.Tables[0].Rows[0]["C_Year_ID"]);
                }
            }
            return data;
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
        /// <returns>return list of labels and bank statement data</returns>
        public VA012_BankChargeData GetBankChargeData(Ctx ctx, int C_BankAccount_ID, int C_Charge_ID, DateTime yrStartDate, DateTime yrEndDate, int Year_ID)
        {
            string[] labels = null;
            decimal[] chargeAmt = null;
            string[] currency = null;
            VA012_BankChargeData bankData = new VA012_BankChargeData();
            //Get Bank Charge and Periods data
            sql.Append(@"WITH BANKSTATEMENTDATA AS (
                        SELECT
                            CASE
                            WHEN (C_BANKACCOUNT.C_CURRENCY_ID != C_BANKSTATEMENTLINE.C_CURRENCY_ID) THEN
                            ROUND(
                                COALESCE(
                                    (CURRENCYCONVERT(
                                        C_BANKSTATEMENTLINE.CHARGEAMT, C_BANKSTATEMENTLINE.C_CURRENCY_ID, C_BANKACCOUNT.C_CURRENCY_ID, C_BANKSTATEMENTLINE.
                                        STATEMENTLINEDATE, C_BANKSTATEMENTLINE.C_CONVERSIONTYPE_ID, C_BANKSTATEMENTLINE.AD_CLIENT_ID, C_BANKSTATEMENTLINE.
                                        AD_ORG_ID
                                    )), 0
                                ), C_CURRENCY.STDPRECISION
                            )
                            ELSE
                            C_BANKSTATEMENTLINE.CHARGEAMT
                            END AS CHARGEAMT,
                            C_BANKSTATEMENTLINE.STATEMENTLINEDATE,C_CURRENCY.ISO_CODE,C_CURRENCY.CurSymbol
                        FROM
                            C_BANKSTATEMENTLINE
                            INNER JOIN C_BANKSTATEMENT ON ( C_BANKSTATEMENT.C_BANKSTATEMENT_ID = C_BANKSTATEMENTLINE.C_BANKSTATEMENT_ID )
                            INNER JOIN C_CHARGE ON ( C_BANKSTATEMENTLINE.C_CHARGE_ID = C_CHARGE.C_CHARGE_ID )
                            INNER JOIN C_BANKACCOUNT ON ( C_BANKACCOUNT.C_BANKACCOUNT_ID = C_BANKSTATEMENT.C_BANKACCOUNT_ID )
                            INNER JOIN C_CURRENCY ON ( C_CURRENCY.C_CURRENCY_ID = C_BANKACCOUNT.C_CURRENCY_ID )
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
                                    )) AS CHARGEAMT,BANKSTATEMENTDATA.ISO_CODE,BANKSTATEMENTDATA.CurSymbol,
                                    PERIODDATA.NAME
                                FROM
                                    (
                                        SELECT
                                            NAME,
                                            StartDate,EndDate
                                        FROM
                                            C_PERIOD
                                        WHERE
                                            ISACTIVE = 'Y'
                                            AND C_YEAR_ID = " + Year_ID + @"
                                    ) PERIODDATA
                                    LEFT JOIN BANKSTATEMENTDATA ON (1=1 AND BANKSTATEMENTDATA.STATEMENTLINEDATE BETWEEN PERIODDATA.StartDate AND PERIODDATA.ENDDate)
                                GROUP BY
                                    PERIODDATA.NAME,PERIODDATA.STARTDATE,BANKSTATEMENTDATA.ISO_CODE,BANKSTATEMENTDATA.CurSymbol
                                ORDER BY
                                    PERIODDATA.STARTDATE");
            DataSet ds = DB.ExecuteDataset(sql.ToString(), null, null);
            if (ds != null && ds.Tables[0].Rows.Count > 0)
            {
                chargeAmt = new decimal[ds.Tables[0].Rows.Count];
                labels = new string[ds.Tables[0].Rows.Count];
                currency = new string[ds.Tables[0].Rows.Count];
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    chargeAmt[i] =Util.GetValueOfDecimal(ds.Tables[0].Rows[i]["CHARGEAMT"]);
                    labels[i]=Util.GetValueOfString(ds.Tables[0].Rows[i]["NAME"]);
                    currency[i] = Util.GetValueOfString(ds.Tables[0].Rows[i]["ISO_CODE"]);
                }
                bankData.bankChargeData=chargeAmt;
                bankData.labels = labels;
                bankData.currency = currency;
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
            public string [] currency { get; set; }
        }
    }
}