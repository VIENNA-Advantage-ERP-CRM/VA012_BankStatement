using System;
using System.Collections.Generic;
using System.Linq;
using VAdvantage.DataBase;
using VAdvantage.Utility;

namespace VA012.Models
{
    /// <summary>
    /// Bulk-operation support for C_BankStatement header totals.
    /// MBankStatementLine.AfterSave recalculates StatementDifference / EndingBalance of the header on every
    /// single line save. For loops that save many lines, wrap the loop in <see cref="Suspend"/> and call
    /// <see cref="Recalculate(int, Trx)"/> once afterwards.
    /// The flag is honoured by MBankStatementLine once ModelLibrary contains the matching check
    /// (ctx.GetContext(FlagKey) == "Y"); until then it is ignored and Recalculate is just an idempotent no-op update.
    /// </summary>
    public static class StatementHeaderSync
    {
        public const string FlagKey = "VA012_SkipBSHeaderUpdate";

        private sealed class Scope : IDisposable
        {
            private readonly Ctx _ctx;
            private readonly string _previous;

            public Scope(Ctx ctx)
            {
                _ctx = ctx;
                _previous = ctx.GetContext(FlagKey);
                ctx.SetContext(FlagKey, "Y");
            }

            public void Dispose()
            {
                _ctx.SetContext(FlagKey, string.IsNullOrEmpty(_previous) ? "N" : _previous);
            }
        }

        /// <summary>Tell MBankStatementLine to skip its per-save header UPDATEs until the returned scope is disposed.</summary>
        public static IDisposable Suspend(Ctx ctx)
        {
            return new Scope(ctx);
        }

        /// <summary>Recalculate StatementDifference and EndingBalance of one statement header.</summary>
        public static void Recalculate(int bankStatementID, Trx trx = null)
        {
            if (bankStatementID <= 0)
            {
                return;
            }
            DB.ExecuteQuery("UPDATE C_BankStatement bs"
                + " SET StatementDifference=(SELECT COALESCE( SUM(StmtAmt), 0 ) FROM C_BankStatementLine bsl "
                + "WHERE bsl.C_BankStatement_ID=bs.C_BankStatement_ID AND bsl.IsActive='Y') "
                + "WHERE C_BankStatement_ID=" + bankStatementID, null, trx);
            DB.ExecuteQuery("UPDATE C_BankStatement bs"
                + " SET EndingBalance=BeginningBalance+StatementDifference "
                + "WHERE C_BankStatement_ID=" + bankStatementID, null, trx);
        }

        /// <summary>Sum of active line amounts of a statement (what the header UPDATE would store as StatementDifference).</summary>
        public static decimal SumLines(int bankStatementID)
        {
            return Util.GetValueOfDecimal(DB.ExecuteScalar("SELECT COALESCE( SUM(StmtAmt), 0 ) FROM C_BankStatementLine "
                + "WHERE C_BankStatement_ID=" + bankStatementID + " AND IsActive='Y'"));
        }

        /// <summary>
        /// Importers append to the open (not completed) statement of a bank account; after an import,
        /// recalculate the header of every such statement once, on success and on error paths alike.
        /// </summary>
        public static void RecalculateOpenStatements(Ctx ctx, int bankAccountID)
        {
            System.Data.DataSet ds = DB.ExecuteDataset("SELECT C_BANKSTATEMENT_ID FROM C_BANKSTATEMENT WHERE ISACTIVE='Y' "
                + "AND DocStatus NOT IN('RE','VO','CO','CL') AND AD_Client_ID=" + ctx.GetAD_Client_ID()
                + " AND C_BANKACCOUNT_ID=" + bankAccountID);
            if (ds == null || ds.Tables.Count == 0)
            {
                return;
            }
            foreach (System.Data.DataRow row in ds.Tables[0].Rows)
            {
                Recalculate(Util.GetValueOfInt(row["C_BANKSTATEMENT_ID"]));
            }
            ds.Dispose();
        }
        public static void Recalculate(IEnumerable<int> bankStatementIDs, Trx trx = null)
        {
            foreach (int id in bankStatementIDs.Distinct())
            {
                Recalculate(id, trx);
            }
        }
    }
}