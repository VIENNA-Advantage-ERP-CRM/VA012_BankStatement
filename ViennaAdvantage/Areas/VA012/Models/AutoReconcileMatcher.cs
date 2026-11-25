/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto reconciliation - standard formula (in memory, no database access)
 * Chronological  : Development
 * Design         : VA012 Auto Reconciliation Solution Approach v4
 ******************************************************/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VA012.Models
{
    /// <summary>
    /// An open bank statement line as the matcher sees it. Amount is the signed statement amount
    /// (credit positive) in the bank account's currency.
    /// </summary>
    public class ArLine
    {
        public int LineId;
        public int LineNo;
        public DateTime LineDate;
        public decimal Amount;
        public string TrxNo;
        public string ChequeNo;
        public int BPartnerId;
        public string Narration;
    }

    /// <summary>
    /// A candidate document: payment (PY), invoice schedule (IS), prepay order (PO) or contra cash line (CO).
    /// Amount is signed like the statement amount and converted to the bank currency; for invoice schedules it is
    /// the due amount less withholding. DocDate is DateAcct (PY, CO), the due date (IS) or the order date (PO).
    /// </summary>
    public class ArDoc
    {
        public string Kind;
        public int Id;
        public string DocumentNo;
        public int BPartnerId;
        public string BPartnerName;
        public string BPartnerValue;
        public decimal Amount;
        public DateTime DocDate;
        public string TrxNo;
        public string ChequeNo;
        public int PayMethodId;

        /// <summary>Key of the unique reservation index (VA012_ReserveKey).</summary>
        public string ReserveKey { get { return Kind + "_" + Id.ToString(CultureInfo.InvariantCulture); } }
    }

    /// <summary>
    /// A "When" condition of a rule. Field: NA narration, TR trx no, CH cheque no, DI direction, AM amount.
    /// Op: CT contains, EQ equals, SW starts with, BT between, NE is not empty, IS is (direction C or D).
    /// Several values may be given comma-separated; any one of them satisfies CT, EQ and SW.
    /// </summary>
    public class ArRuleCond
    {
        public string Field;
        public string Op;
        public string Value;
        public string Value2;
    }

    /// <summary>An active rule of the bank account's rules header, as loaded for one run.</summary>
    public class ArRule
    {
        public int Id;
        public string Name;
        public int SeqNo;
        public HashSet<string> AppliesTo = new HashSet<string>(StringComparer.Ordinal);
        public int? DateWindow;
        public decimal? Tolerance;
        public bool AutoConfirm;
        /// <summary>Auto-confirm only up to this absolute amount; 0 = no cap.</summary>
        public decimal ControlCap;
        /// <summary>DA, WO or OU for an invoice-schedule difference; empty = ask.</summary>
        public string DifferenceType;
        /// <summary>VA012_OnDiff = H: a payment or prepay order difference takes the header's charge and tax rate.</summary>
        public bool DiffFromHeader;
        public int PayMethodId;
        public int ConversionTypeId;
        /// <summary>S statement then cheque book, B cheque book only, M ask.</summary>
        public string CheckSource = "S";
        public bool MultiDoc = true;
        public int MinDocs = 2;
        public bool SkipOnMissing;
        public List<ArRuleCond> Conditions = new List<ArRuleCond>();
        /// <summary>Match-on keys TR, CH, BP, AM, DN; true = Required, false = Preferred.</summary>
        public Dictionary<string, bool> Keys = new Dictionary<string, bool>(StringComparer.Ordinal);
    }

    /// <summary>Settings of one run: system defaults, overridden by the bank account's rules header.</summary>
    public class ArSettings
    {
        public int DateWindow = 3;
        public decimal Tolerance = 1m;
        public int Case4MaxDays = 30;
        public int MinDocs = 2;
        public bool AutoMatch = true;
        public bool HasDefaultChargeAndTax;
        public int ChargeId;
        public int TaxId;
        /// <summary>Active rules of the header (empty without a header).</summary>
        public List<ArRule> Rules = new List<ArRule>();
        /// <summary>Payment methods with VA009_PaymentBaseType = 'S' (cheque).</summary>
        public HashSet<int> ChequeMethodIds = new HashSet<int>();
    }

    /// <summary>The outcome for one line: a suggestion (S) with its documents, or no match (N) with a reason.</summary>
    public class ArResult
    {
        public ArLine Line;
        public string Status = "N";
        public string MatchCase;
        public bool IsAuto;
        public decimal Difference;
        public string SignalSource;
        public string Reason;
        /// <summary>The rule that decided, or null for the standard formula.</summary>
        public ArRule Rule;
        /// <summary>A rule or a standard case found several documents it cannot separate: they are Candidates and the user chooses.</summary>
        public bool Tie;
        public int SuggestionId;
        /// <summary>The document type this result was matched for (PY, IS, PO, CO); differs per line in an all-types run.</summary>
        public string DocType;
        public List<ArDoc> Docs = new List<ArDoc>();
        public List<ArDoc> Candidates = new List<ArDoc>();
    }

    /// <summary>
    /// Rules first, then the standard formula. The header's active rules for the run's type are tried by priority; a
    /// rule decides a line when its conditions hold and it finds a document (or a tie). Otherwise the standard formula
    /// checks, in order: Case 1 (reference and amount; a document number in the narration counts as Case 1 too),
    /// Case 3 (partner, amount, date window), Case 2 (accounting date and amount; payment and contra runs only),
    /// Case 4 (partner and amount). The first check that finds exactly one document decides. When none does, the first
    /// check that found several documents makes the line a suggestion without a document: the user chooses one.
    /// Lines are processed in statement order and every chosen document is reserved for that line.
    /// </summary>
    public class AutoReconcileMatcher
    {
        private const int MinTokenLength = 4;
        private const int MinPartnerWordLength = 5;
        private const int MaxPartnerWords = 4;                                    // words joined when reading a partner name
        private static readonly HashSet<string> PartnerStopWords = new HashSet<string>(StringComparer.Ordinal)
        {
            "NEFT", "RTGS", "IMPS", "TRANSFER", "PAYMENT", "PAYMENTS", "RECEIPT", "CHEQUE", "CHECK", "DEPOSIT", "WITHDRAWAL",
            "BRANCH", "ADVANCE", "CREDIT", "DEBIT", "SETTLEMENT", "INVOICE", "ORDER", "SALARY", "CHARGES", "INTEREST",
            "REFUND", "ONLINE", "FUNDS", "TRANSACTION", "ACCOUNT", "REVERSAL", "CLEARING", "INWARD", "OUTWARD"
        };

        private readonly string _docType;
        private readonly ArSettings _settings;
        private readonly HashSet<string> _declined;
        private readonly HashSet<string> _reserved = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ArDoc>> _byRef = new Dictionary<string, List<ArDoc>>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<ArDoc>> _byDocNo = new Dictionary<string, List<ArDoc>>(StringComparer.Ordinal);
        private readonly Dictionary<int, List<ArDoc>> _byPartner = new Dictionary<int, List<ArDoc>>();
        private readonly Dictionary<DateTime, List<ArDoc>> _byDate = new Dictionary<DateTime, List<ArDoc>>();
        private readonly List<KeyValuePair<string, int>> _partnerKeys = new List<KeyValuePair<string, int>>();
        private readonly HashSet<string> _docNoPrefixes = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<ArRule> _rules;
        private readonly List<ArDoc> _byAmount;                                    // sorted by Amount, for range lookups

        /// <param name="docType">PY, IS, PO or CO</param>
        /// <param name="docs">open, unreserved documents of that type</param>
        /// <param name="declinedPairs">"lineId|reserveKey" pairs the user declined before</param>
        /// <param name="settings">run settings</param>
        public AutoReconcileMatcher(string docType, IEnumerable<ArDoc> docs, IEnumerable<string> declinedPairs, ArSettings settings)
        {
            _docType = docType;
            _settings = settings ?? new ArSettings();
            _declined = new HashSet<string>(declinedPairs ?? Enumerable.Empty<string>(), StringComparer.Ordinal);
            List<ArDoc> all = docs.ToList();
            _byAmount = all.OrderBy(d => d.Amount).ToList();
            _rules = (_settings.Rules ?? new List<ArRule>()).Where(r => r.AppliesTo.Contains(docType) && r.Keys.Count > 0)
                .OrderBy(r => r.SeqNo).ThenBy(r => r.Id).ToList();
            var partnerKeys = new Dictionary<string, HashSet<int>>(StringComparer.Ordinal);
            foreach (ArDoc d in all)
            {
                Add(_byRef, NormRef(d.TrxNo), d);
                Add(_byRef, NormRef(d.ChequeNo), d);
                string docNo = NormRef(d.DocumentNo);
                Add(_byDocNo, docNo, d);
                if (docNo != null)
                {
                    string prefix = LetterPrefix(d.DocumentNo);
                    if (prefix.Length > 0) _docNoPrefixes.Add(prefix);
                }
                if (d.BPartnerId > 0)
                {
                    List<ArDoc> list;
                    if (!_byPartner.TryGetValue(d.BPartnerId, out list)) { list = new List<ArDoc>(); _byPartner[d.BPartnerId] = list; }
                    list.Add(d);
                    foreach (string key in new[] { LettersAndDigits(d.BPartnerName), LettersAndDigits(d.BPartnerValue) })
                    {
                        if (key.Length < MinPartnerWordLength) continue;
                        HashSet<int> ids;
                        if (!partnerKeys.TryGetValue(key, out ids)) { ids = new HashSet<int>(); partnerKeys[key] = ids; }
                        ids.Add(d.BPartnerId);
                    }
                }
                List<ArDoc> byDate;
                if (!_byDate.TryGetValue(d.DocDate.Date, out byDate)) { byDate = new List<ArDoc>(); _byDate[d.DocDate.Date] = byDate; }
                byDate.Add(d);
            }
            foreach (var kv in partnerKeys)
                foreach (int id in kv.Value)
                    _partnerKeys.Add(new KeyValuePair<string, int>(kv.Key, id));
            _partnerKeys.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        }

        /// <summary>Matches the lines in statement order.</summary>
        public List<ArResult> Match(IEnumerable<ArLine> lines)
        {
            var results = new List<ArResult>();
            foreach (ArLine line in lines.OrderBy(l => l.LineNo).ThenBy(l => l.LineId))
            {
                ArResult r = MatchLine(line);
                if (r.Status == "S")
                    foreach (ArDoc d in r.Docs) _reserved.Add(d.ReserveKey);
                results.Add(r);
            }
            return results;
        }

        private ArResult MatchLine(ArLine line)
        {
            var ctx = new LineContext(line, this);

            // ---- Rules of the bank account's header, by priority; a line no rule decides goes on to the standard cases
            foreach (ArRule rule in _rules)
            {
                ArResult byRule = TryRule(rule, line, ctx);
                if (byRule != null) return byRule;
            }

            var result = new ArResult { Line = line };
            List<ArDoc> ambiguous = null;
            string reason = null;
            Choice choice = null;       // the first case that found several equal documents: the user chooses among them

            // ---- Case 1: transaction or cheque number (field first, then narration)
            string source;
            List<ArDoc> refHits = ReferenceHits(ctx, out source);
            if (refHits.Count > 0)
            {
                List<ArDoc> withAmount = refHits.Where(ctx.AmountFits).ToList();
                if (withAmount.Count == 1)
                    return Suggest(result, "C1", withAmount[0], source, true);
                if (withAmount.Count == 0 && refHits.Count == 1)
                    return Suggest(result, "C1", refHits[0], source, false);
                ambiguous = refHits;
                reason = refHits.Count + " documents have this reference";
                choice = new Choice { Case = "C1", Docs = withAmount.Count > 1 ? withAmount : refHits, Reason = reason, Source = source };
            }

            // ---- Case 1 from the narration: invoice, order or payment document number
            List<ArDoc> docNoHits = ctx.DocumentNumberHits();
            if (docNoHits.Count > 0)
            {
                if (_docType == "IS" && ctx.PartnerId > 0)
                {
                    List<ArDoc> ofPartner = docNoHits.Where(d => d.BPartnerId == ctx.PartnerId).ToList();
                    if (ofPartner.Count >= Math.Max(2, _settings.MinDocs))
                    {
                        decimal total = ofPartner.Sum(d => d.Amount);
                        bool missing = ctx.NamedNumbersNotFound(ofPartner) > 0;
                        if (Math.Abs(total - line.Amount) <= _settings.Tolerance)
                            return SuggestSeveral(result, ofPartner, missing);
                        if (ambiguous == null)
                        {
                            ambiguous = ofPartner;
                            reason = "Total does not tie: invoices found " + Amount(total) + " against " + Amount(line.Amount);
                        }
                    }
                }
                List<ArDoc> single = docNoHits.Where(ctx.AmountFits).ToList();
                if (single.Count == 1)
                {
                    ArDoc d = single[0];
                    bool partnerAgrees = ctx.PartnerId > 0 && d.BPartnerId == ctx.PartnerId;
                    return Suggest(result, "CN", d, "narration", _docType != "PY" && partnerAgrees);
                }
                if (single.Count == 0 && docNoHits.Count == 1)
                    return Suggest(result, "CN", docNoHits[0], "narration", false);
                if (ambiguous == null && docNoHits.Count > 1)
                {
                    ambiguous = docNoHits;
                    reason = docNoHits.Count + " documents are named in the narration";
                }
            }

            // ---- Case 3: partner and amount, document date within the window
            if (ctx.PartnerId > 0)
            {
                List<ArDoc> c3 = ctx.PartnerDocs().Where(d => ctx.AmountFits(d) && Math.Abs((d.DocDate.Date - line.LineDate.Date).TotalDays) <= _settings.DateWindow).ToList();
                if (c3.Count == 1)
                    return Suggest(result, "C3", c3[0], ctx.PartnerSource, true);
                if (c3.Count > 1)
                {
                    string why = c3.Count + " documents fit: same partner and amount, within " + _settings.DateWindow + " days";
                    if (ambiguous == null) { ambiguous = c3; reason = why; }
                    if (choice == null) choice = new Choice { Case = "C3", Docs = c3, Reason = why, Source = ctx.PartnerSource };
                }
            }

            // ---- Case 2: accounting date and amount (payment and contra runs); a different partner on the line rejects
            if (_docType == "PY" || _docType == "CO")
            {
                List<ArDoc> sameDay;
                if (_byDate.TryGetValue(line.LineDate.Date, out sameDay))
                {
                    List<ArDoc> c2 = sameDay.Where(d => ctx.Available(d) && ctx.AmountFits(d)
                                                         && (line.BPartnerId <= 0 || d.BPartnerId <= 0 || d.BPartnerId == line.BPartnerId)).ToList();
                    if (c2.Count == 1)
                        return Suggest(result, "C2", c2[0], "date", false);
                    if (c2.Count > 1)
                    {
                        string why = c2.Count + " documents fit: same date and amount";
                        if (ambiguous == null) { ambiguous = c2; reason = why; }
                        if (choice == null) choice = new Choice { Case = "C2", Docs = c2, Reason = why, Source = "date" };
                    }
                }
            }

            // ---- Case 4: partner and amount, no date check (automatic only within 30 days)
            if (ctx.PartnerId > 0)
            {
                List<ArDoc> c4 = ctx.PartnerDocs().Where(ctx.AmountFits).ToList();
                if (c4.Count == 1)
                {
                    bool recent = Math.Abs((c4[0].DocDate.Date - line.LineDate.Date).TotalDays) <= _settings.Case4MaxDays;
                    return Suggest(result, "C4", c4[0], ctx.PartnerSource, recent);
                }
                if (c4.Count > 1)
                {
                    string why = c4.Count + " documents fit: same partner and amount";
                    if (ambiguous == null) { ambiguous = c4; reason = why; }
                    if (choice == null) choice = new Choice { Case = "C4", Docs = c4, Reason = why, Source = ctx.PartnerSource };
                }
            }

            // ---- Several documents fit one case equally: suggested with no document chosen; the user picks one of the
            // candidates (never automatic). Invoices named together in a narration are not a choice: they stay No match.
            if (choice != null)
            {
                result.Status = "S";
                result.Tie = true;
                result.MatchCase = choice.Case;
                result.SignalSource = choice.Source;
                result.Reason = choice.Reason + ": choose one";
                result.Candidates = Closest(choice.Docs, line).Take(10).ToList();
                return result;
            }

            // ---- No match
            result.Status = "N";
            result.Reason = reason ?? ctx.RejectionReason ?? (ctx.PartnerId > 0 ? "No open document of this partner has this amount" : "No open document fits");
            if (ambiguous != null)
                result.Candidates = Closest(ambiguous, line).Take(10).ToList();
            return result;
        }

        /// <summary>Documents ordered by the smallest amount difference, then the nearest date.</summary>
        private static IEnumerable<ArDoc> Closest(IEnumerable<ArDoc> docs, ArLine line)
        {
            return docs.OrderBy(d => Math.Abs(d.Amount - line.Amount))
                .ThenBy(d => Math.Abs((d.DocDate.Date - line.LineDate.Date).TotalDays)).ThenBy(d => d.Id);
        }

        /// <summary>The first standard case that found several documents it cannot separate.</summary>
        private class Choice
        {
            public string Case;
            public string Reason;
            public string Source;
            public List<ArDoc> Docs;
        }

        private ArResult Suggest(ArResult r, string matchCase, ArDoc doc, string source, bool canBeAuto)
        {
            r.Status = "S";
            r.MatchCase = matchCase;
            r.Docs.Add(doc);
            r.Difference = r.Line.Amount - doc.Amount;
            r.SignalSource = source;
            SetAuto(r, canBeAuto, doc);
            return r;
        }

        private ArResult SuggestSeveral(ArResult r, List<ArDoc> docs, bool numberMissing)
        {
            r.Status = "S";
            r.MatchCase = "CN";
            r.Docs.AddRange(docs);
            r.Difference = r.Line.Amount - docs.Sum(d => d.Amount);
            r.SignalSource = "narration";
            if (numberMissing) { r.IsAuto = false; r.Reason = "A number in the narration is not an open invoice of this partner"; }
            else SetAuto(r, true, docs.FirstOrDefault(d => d.PayMethodId <= 0) ?? docs[0]);
            return r;
        }

        /// <summary>Decides automatic reconciliation; a line that could be automatic but is held back says why.</summary>
        private void SetAuto(ArResult r, bool canBeAuto, ArDoc doc)
        {
            string blocked = canBeAuto ? AutoBlockedBy(r, doc) : null;
            r.IsAuto = canBeAuto && blocked == null;
            if (blocked != null && _settings.AutoMatch) r.Reason = blocked;
            else if (r.Difference != 0 && Math.Abs(r.Difference) > Tolerance(r.Rule))
                r.Reason = "Amount differs by " + Amount(r.Difference);
        }

        /// <summary>
        /// Why a match may not reconcile automatically, or null when it may: the master switch is off, the rule's amount
        /// cap is exceeded, there is a difference without a default (charge and tax for payments and prepay orders, a
        /// difference type for invoice schedules, none allowed for contra), or the new payment has no payment method or
        /// needs a cheque number to be asked for.
        /// </summary>
        private string AutoBlockedBy(ArResult r, ArDoc doc)
        {
            if (!_settings.AutoMatch) return "Auto Reconciliation is off";
            ArRule rule = r.Rule;
            if (rule != null && rule.ControlCap > 0 && Math.Abs(r.Line.Amount) > rule.ControlCap)
                return "Above the rule's limit of " + Amount(rule.ControlCap);
            if (r.Difference != 0)
            {
                if (_docType == "CO") return "Contra amounts must be equal";
                // invoice schedules as the form: invoices more than the statement -> discount, write-off or over/under;
                // statement more than the invoices -> a charge with tax rate, like payments and prepay orders
                if (_docType == "IS" && !StatementExceedsDocs(r))
                {
                    if (rule == null || string.IsNullOrEmpty(rule.DifferenceType)) return "Needs difference type";
                }
                else if (!_settings.HasDefaultChargeAndTax || (rule != null && !rule.DiffFromHeader))
                    return "Needs charge and tax";
            }
            if (_docType == "IS" || _docType == "PO")
            {
                int method = rule != null && rule.PayMethodId > 0 ? rule.PayMethodId : doc.PayMethodId;
                if (method <= 0) return "Needs payment method";
                if (rule != null && rule.CheckSource == "M" && _settings.ChequeMethodIds.Contains(method)) return "Needs cheque number";
            }
            return null;
        }

        /// <summary>The statement amount is larger than the matched documents (the rest is posted to a charge).</summary>
        public static bool StatementExceedsDocs(ArResult r)
        {
            return Math.Abs(r.Line.Amount) > Math.Abs(r.Docs.Sum(d => d.Amount));
        }

        private decimal Tolerance(ArRule rule)
        {
            if (_docType == "CO") return 0m;
            return rule != null && rule.Tolerance.HasValue ? rule.Tolerance.Value : _settings.Tolerance;
        }

        // ------------------------------------------------------------------ rules
        private enum KeyOutcome { Match, Missing, Differs }

        /// <summary>
        /// One rule on one line. Null when its conditions do not hold or it finds no document. A document qualifies when
        /// every Required key matches and a Trx no. or cheque no. key does not contradict it; the documents that match
        /// the most Preferred keys win. One winner is a match, several are a tie the user resolves.
        /// </summary>
        private ArResult TryRule(ArRule rule, ArLine line, LineContext ctx)
        {
            if (!rule.Conditions.All(c => ConditionHolds(c, line))) return null;
            decimal tolerance = Tolerance(rule);
            int window = rule.DateWindow ?? _settings.DateWindow;

            // several invoices named in the narration, limited to the line's partner (invoice schedules)
            if (_docType == "IS" && rule.MultiDoc && ctx.PartnerId > 0)
            {
                List<ArDoc> named = ctx.DocumentNumberHits().Where(d => d.BPartnerId == ctx.PartnerId).ToList();
                if (named.Count >= Math.Max(2, rule.MinDocs) && Math.Abs(named.Sum(d => d.Amount) - line.Amount) <= tolerance)
                {
                    bool missing = ctx.NamedNumbersNotFound(named) > 0;
                    if (!(missing && rule.SkipOnMissing))
                    {
                        var several = new ArResult { Line = line, Rule = rule };
                        SuggestSeveral(several, named, missing);
                        several.MatchCase = null;
                        if (!rule.AutoConfirm && several.IsAuto) several.IsAuto = false;
                        return several;
                    }
                }
            }

            var scored = new List<KeyValuePair<ArDoc, int>>();
            foreach (ArDoc d in RuleCandidates(rule, line, ctx, tolerance))
            {
                if (Math.Abs((d.DocDate.Date - line.LineDate.Date).TotalDays) > window) continue;
                int preferred = 0, matched = 0;
                bool rejected = false;
                foreach (KeyValuePair<string, bool> key in rule.Keys)
                {
                    KeyOutcome o = EvaluateKey(key.Key, d, line, ctx, tolerance);
                    if (o == KeyOutcome.Match) { matched++; if (!key.Value) preferred++; continue; }
                    if (key.Value || (o == KeyOutcome.Differs && (key.Key == "TR" || key.Key == "CH"))) { rejected = true; break; }
                }
                if (!rejected && matched > 0) scored.Add(new KeyValuePair<ArDoc, int>(d, preferred));
            }
            if (scored.Count == 0) return null;

            // best first: most Preferred keys, then the smallest amount difference, then the nearest date
            int best = scored.Max(s => s.Value);
            Func<ArDoc, decimal> amountGap = d => Math.Abs(d.Amount - line.Amount);
            Func<ArDoc, double> dayGap = d => Math.Abs((d.DocDate.Date - line.LineDate.Date).TotalDays);
            List<ArDoc> ranked = scored.Where(s => s.Value == best).Select(s => s.Key)
                .OrderBy(amountGap).ThenBy(dayGap).ThenBy(d => d.Id).ToList();
            var r = new ArResult { Line = line, Rule = rule, Status = "S", SignalSource = "rule" };
            if (ranked.Count == 1)
            {
                r.Docs.Add(ranked[0]);
                r.Difference = line.Amount - ranked[0].Amount;
                SetAuto(r, rule.AutoConfirm, ranked[0]);
                return r;
            }

            ArDoc first = ranked[0], second = ranked[1];
            bool separated = amountGap(first) < amountGap(second) || (amountGap(first) == amountGap(second) && dayGap(first) < dayGap(second));
            if (!separated)
            {
                // equally close on amount and date: a real tie, the user chooses
                r.Tie = true;
                r.Candidates = ranked.Take(10).ToList();
                r.Reason = ranked.Count + " documents fit rule " + rule.Name + " equally well: choose one";
                return r;
            }

            // the closest of several becomes the suggestion; the next ones stay visible as alternatives (not reserved).
            // It reconciles automatically only when it alone has the exact amount and the rule auto-confirms.
            r.Docs.Add(first);
            r.Difference = line.Amount - first.Amount;
            r.Candidates = ranked.Skip(1).Take(5).ToList();
            SetAuto(r, rule.AutoConfirm && amountGap(first) == 0 && amountGap(second) != 0, first);
            if (!r.IsAuto)
            {
                string closest = "Closest of " + ranked.Count + " documents that fit rule " + rule.Name;
                r.Reason = string.IsNullOrEmpty(r.Reason) ? closest : r.Reason + " · " + closest;
            }
            return r;
        }

        /// <summary>Documents that match at least one of the rule's keys, found through the indexes.</summary>
        private IEnumerable<ArDoc> RuleCandidates(ArRule rule, ArLine line, LineContext ctx, decimal tolerance)
        {
            var found = new HashSet<ArDoc>();
            if (rule.Keys.ContainsKey("TR") || rule.Keys.ContainsKey("CH"))
            {
                foreach (string value in new[] { ctx.TrxField, ctx.ChequeField }.Concat(ctx.RefTokens))
                {
                    List<ArDoc> list;
                    if (value != null && _byRef.TryGetValue(value, out list)) found.UnionWith(list);
                }
            }
            if (rule.Keys.ContainsKey("DN")) found.UnionWith(ctx.DocumentNumberHits());
            if (rule.Keys.ContainsKey("BP") && ctx.PartnerId > 0)
            {
                List<ArDoc> list;
                if (_byPartner.TryGetValue(ctx.PartnerId, out list)) found.UnionWith(list);
            }
            if (rule.Keys.ContainsKey("AM")) found.UnionWith(AmountRange(line.Amount - tolerance, line.Amount + tolerance));
            return found.Where(ctx.Available);
        }

        private IEnumerable<ArDoc> AmountRange(decimal low, decimal high)
        {
            int lo = 0, hi = _byAmount.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (_byAmount[mid].Amount < low) lo = mid + 1; else hi = mid;
            }
            for (int i = lo; i < _byAmount.Count && _byAmount[i].Amount <= high; i++) yield return _byAmount[i];
        }

        private KeyOutcome EvaluateKey(string key, ArDoc d, ArLine line, LineContext ctx, decimal tolerance)
        {
            switch (key)
            {
                case "TR": return ReferenceOutcome(ctx.TrxField, NormRef(d.TrxNo), ctx);
                case "CH": return ReferenceOutcome(ctx.ChequeField, NormRef(d.ChequeNo), ctx);
                case "BP":
                    if (ctx.PartnerId <= 0 || d.BPartnerId <= 0) return KeyOutcome.Missing;
                    if (d.BPartnerId == ctx.PartnerId) return KeyOutcome.Match;
                    return line.BPartnerId > 0 ? KeyOutcome.Differs : KeyOutcome.Missing;   // a partner read from the narration never rejects
                case "AM": return Math.Abs(d.Amount - line.Amount) <= tolerance ? KeyOutcome.Match : KeyOutcome.Differs;
                case "DN":
                    string no = NormRef(d.DocumentNo);
                    return no != null && ctx.RefTokens.Contains(no) ? KeyOutcome.Match : KeyOutcome.Missing;
                default: return KeyOutcome.Missing;
            }
        }

        /// <summary>Field first, then the narration: equal to the field or found in the narration is a match.</summary>
        private static KeyOutcome ReferenceOutcome(string field, string docValue, LineContext ctx)
        {
            if (docValue == null) return KeyOutcome.Missing;
            if (docValue == field || ctx.RefTokens.Contains(docValue)) return KeyOutcome.Match;
            return field != null ? KeyOutcome.Differs : KeyOutcome.Missing;
        }

        /// <summary>A rule's When condition on the line.</summary>
        public static bool ConditionHolds(ArRuleCond c, ArLine line)
        {
            string op = string.IsNullOrEmpty(c.Op) ? "CT" : c.Op;
            switch (c.Field)
            {
                case "DI":
                    string want = (c.Value ?? "").Trim().ToUpperInvariant();
                    if (want.StartsWith("C", StringComparison.Ordinal)) return line.Amount >= 0;
                    if (want.StartsWith("D", StringComparison.Ordinal)) return line.Amount < 0;
                    return true;
                case "AM": return AmountCondition(op, Math.Abs(line.Amount), c.Value, c.Value2);
                case "TR": return TextCondition(op, line.TrxNo, c.Value, c.Value2, true);
                case "CH": return TextCondition(op, line.ChequeNo, c.Value, c.Value2, true);
                default: return TextCondition(op, line.Narration, c.Value, c.Value2, false);
            }
        }

        private static bool TextCondition(string op, string text, string value, string value2, bool reference)
        {
            if (op == "NE") return !string.IsNullOrWhiteSpace(text);
            if (string.IsNullOrWhiteSpace(text)) return false;
            Func<string, string> norm = s => reference ? NormRef(s) : (string.IsNullOrWhiteSpace(s) ? null : s.Trim().ToUpperInvariant());
            string subject = norm(text);
            if (subject == null) return false;
            if (op == "BT") return Between(subject, norm(value), norm(value2));
            List<string> values = (value ?? "").Split(',').Select(norm).Where(v => v != null).ToList();
            if (values.Count == 0) return false;
            switch (op)
            {
                case "EQ": return values.Any(v => subject == v);
                case "SW": return values.Any(v => subject.StartsWith(v, StringComparison.Ordinal));
                default: return values.Any(v => subject.IndexOf(v, StringComparison.Ordinal) >= 0);
            }
        }

        /// <summary>Numeric comparison when all three are numbers (cheque numbers), text comparison otherwise.</summary>
        private static bool Between(string subject, string low, string high)
        {
            if (low == null || high == null) return false;
            decimal s, l, h;
            if (decimal.TryParse(subject, NumberStyles.Number, CultureInfo.InvariantCulture, out s)
                && decimal.TryParse(low, NumberStyles.Number, CultureInfo.InvariantCulture, out l)
                && decimal.TryParse(high, NumberStyles.Number, CultureInfo.InvariantCulture, out h))
                return s >= l && s <= h;
            return string.CompareOrdinal(subject, low) >= 0 && string.CompareOrdinal(subject, high) <= 0;
        }

        private static bool AmountCondition(string op, decimal amount, string value, string value2)
        {
            if (op == "NE") return amount != 0;
            decimal v1, v2;
            bool has1 = decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out v1);
            switch (op)
            {
                case "EQ": return has1 && Math.Abs(amount - Math.Abs(v1)) < 0.005m;
                case "BT": return has1 && decimal.TryParse(value2, NumberStyles.Number, CultureInfo.InvariantCulture, out v2)
                                  && amount >= Math.Min(Math.Abs(v1), Math.Abs(v2)) && amount <= Math.Max(Math.Abs(v1), Math.Abs(v2));
                default:
                    string text = amount.ToString("0.##", CultureInfo.InvariantCulture);
                    return !string.IsNullOrWhiteSpace(value) && (op == "SW" ? text.StartsWith(value.Trim(), StringComparison.Ordinal) : text.Contains(value.Trim()));
            }
        }

        private List<ArDoc> ReferenceHits(LineContext ctx, out string source)
        {
            source = "field";
            var hits = new List<ArDoc>();
            foreach (string field in new[] { ctx.TrxField, ctx.ChequeField })
            {
                if (field == null) continue;
                List<ArDoc> list;
                if (_byRef.TryGetValue(field, out list))
                    foreach (ArDoc d in list) if (ctx.Available(d) && !hits.Contains(d)) hits.Add(d);
            }
            if (hits.Count > 0) return hits;
            source = "narration";
            foreach (string token in ctx.RefTokens)
            {
                List<ArDoc> list;
                if (_byRef.TryGetValue(token, out list))
                    foreach (ArDoc d in list) if (ctx.Available(d) && !hits.Contains(d)) hits.Add(d);
            }
            return hits;
        }

        // ------------------------------------------------------------------ per-line state
        private class LineContext
        {
            private readonly ArLine _line;
            private readonly AutoReconcileMatcher _m;
            private readonly decimal _tolerance;
            public readonly string TrxField;
            public readonly string ChequeField;
            public readonly HashSet<string> RefTokens;
            public readonly int PartnerId;
            public readonly string PartnerSource;
            public string RejectionReason;

            public LineContext(ArLine line, AutoReconcileMatcher m)
            {
                _line = line;
                _m = m;
                _tolerance = m._docType == "CO" ? 0m : m._settings.Tolerance;      // contra allows no difference
                TrxField = NormRef(line.TrxNo);
                ChequeField = NormRef(line.ChequeNo);
                RefTokens = new HashSet<string>(Tokens(line.Narration).Select(NormRef).Where(t => t != null && t.Length >= MinTokenLength), StringComparer.Ordinal);
                if (line.BPartnerId > 0) { PartnerId = line.BPartnerId; PartnerSource = "partner field"; }
                else { PartnerId = m.PartnerFromNarration(line.Narration); PartnerSource = "partner in narration"; }
            }

            public bool AmountFits(ArDoc d) { return Math.Abs(d.Amount - _line.Amount) <= _tolerance; }

            /// <summary>Not reserved, not declined for this line, same direction, and not contradicted by a reference.</summary>
            public bool Available(ArDoc d)
            {
                if (_m._reserved.Contains(d.ReserveKey)) return false;
                if (_m._declined.Contains(_line.LineId.ToString(CultureInfo.InvariantCulture) + "|" + d.ReserveKey)) return false;
                if (Math.Sign(d.Amount) != Math.Sign(_line.Amount)) return false;
                return !RejectedByReference(d);
            }

            /// <summary>A transaction or cheque number in the line's field that differs from the document's rejects it,
            /// unless the document's number appears in the narration.</summary>
            private bool RejectedByReference(ArDoc d)
            {
                string docTrx = NormRef(d.TrxNo), docChq = NormRef(d.ChequeNo);
                if (TrxField != null && docTrx != null && docTrx != TrxField && !RefTokens.Contains(docTrx))
                {
                    RejectionReason = "UTR differs: statement " + _line.TrxNo + ", document " + d.TrxNo;
                    return true;
                }
                if (ChequeField != null && docChq != null && docChq != ChequeField && !RefTokens.Contains(docChq))
                {
                    RejectionReason = "Cheque no. differs: statement " + _line.ChequeNo + ", document " + d.ChequeNo;
                    return true;
                }
                return false;
            }

            public List<ArDoc> PartnerDocs()
            {
                List<ArDoc> list;
                return _m._byPartner.TryGetValue(PartnerId, out list) ? list.Where(Available).ToList() : new List<ArDoc>();
            }

            public List<ArDoc> DocumentNumberHits()
            {
                var hits = new List<ArDoc>();
                foreach (string token in RefTokens)
                {
                    List<ArDoc> list;
                    if (!_m._byDocNo.TryGetValue(token, out list)) continue;
                    if (list.Count != 1) continue;                                 // a token naming several documents counts for nothing
                    if (Available(list[0]) && !hits.Contains(list[0])) hits.Add(list[0]);
                }
                return hits;
            }

            /// <summary>Narration words that look like these documents' numbers (same letter prefix) but were not found.</summary>
            public int NamedNumbersNotFound(List<ArDoc> found)
            {
                var foundKeys = new HashSet<string>(found.Select(d => NormRef(d.DocumentNo)), StringComparer.Ordinal);
                int missing = 0;
                foreach (string raw in Tokens(_line.Narration))
                {
                    string prefix = LetterPrefix(raw);
                    string key = NormRef(raw);
                    if (key == null || key.Length < MinTokenLength || prefix.Length == 0 || !_m._docNoPrefixes.Contains(prefix)) continue;
                    if (key.Length == prefix.Length) continue;                     // the prefix alone, e.g. "INV"
                    if (!foundKeys.Contains(key)) missing++;
                }
                return missing;
            }
        }

        /// <summary>The partner the narration names: a run of words (see PartnerMatches) that exactly one partner fits.</summary>
        private int PartnerFromNarration(string narration)
        {
            int found = 0;
            foreach (HashSet<int> ids in PartnerMatches(narration, _partnerKeys))
            {
                if (ids.Count != 1) continue;                                      // several partners start with it: counts for nothing
                int id = ids.First();
                if (found != 0 && found != id) return 0;                           // two runs point at two partners
                found = id;
            }
            return found;
        }

        /// <summary>Sorted (name or search key without spaces, partner) pairs, for prefix lookups by binary search.</summary>
        public static List<KeyValuePair<string, int>> BuildPartnerKeys<T>(IEnumerable<T> rows, Func<T, int> id, Func<T, string> name, Func<T, string> value)
        {
            var keys = new List<KeyValuePair<string, int>>();
            foreach (T row in rows)
            {
                int pid = id(row);
                if (pid <= 0) continue;
                foreach (string key in new[] { LettersAndDigits(name(row)), LettersAndDigits(value(row)) })
                    if (key.Length >= MinPartnerWordLength) keys.Add(new KeyValuePair<string, int>(key, pid));
            }
            keys.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
            return keys;
        }

        /// <summary>Every partner a run of narration words may name (used to narrow what is loaded).</summary>
        public static HashSet<int> PartnersStartingWith(string narration, List<KeyValuePair<string, int>> sortedKeys)
        {
            var ids = new HashSet<int>();
            foreach (HashSet<int> m in PartnerMatches(narration, sortedKeys)) ids.UnionWith(m);
            return ids;
        }

        /// <summary>
        /// The partners a narration names. At each word, the longest run of up to four consecutive words that, joined without
        /// spaces, has at least 5 letters and starts some partner's name or search key: "BRS Infotek" gives "BRSINFOTEK",
        /// so a name whose first word is short is found too. A longer run is more specific (fewer names start with it), so
        /// the first run that fits any partner is taken and its words are used up. A run never starts with a bank word
        /// (NEFT, IMPS, ...). Yields the partners each run fits.
        /// </summary>
        private static IEnumerable<HashSet<int>> PartnerMatches(string narration, List<KeyValuePair<string, int>> sortedKeys)
        {
            List<string> words = Words(narration).ToList();
            for (int i = 0; i < words.Count; )
            {
                int used = 1;
                if (!PartnerStopWords.Contains(words[i]))
                {
                    for (int n = Math.Min(MaxPartnerWords, words.Count - i); n >= 1; n--)
                    {
                        string run = string.Concat(words.GetRange(i, n));
                        if (run.Length < MinPartnerWordLength) continue;
                        HashSet<int> ids = KeysStartingWith(run, sortedKeys);
                        if (ids.Count == 0) continue;
                        yield return ids;
                        used = n;
                        break;
                    }
                }
                i += used;
            }
        }

        private static HashSet<int> KeysStartingWith(string word, List<KeyValuePair<string, int>> sortedKeys)
        {
            var ids = new HashSet<int>();
            int lo = 0, hi = sortedKeys.Count;
            while (lo < hi)
            {
                int mid = (lo + hi) / 2;
                if (string.CompareOrdinal(sortedKeys[mid].Key, word) < 0) lo = mid + 1; else hi = mid;
            }
            for (int i = lo; i < sortedKeys.Count && sortedKeys[i].Key.StartsWith(word, StringComparison.Ordinal); i++)
                ids.Add(sortedKeys[i].Value);
            return ids;
        }

        // ------------------------------------------------------------------ text helpers
        private static readonly char[] TokenSeparators = { ' ', '\t', '\r', '\n', '/', ':', ',', '.', ';', '(', ')', '|', '\\', '#', '*' };

        /// <summary>Uppercase; spaces and dashes removed; leading zeros removed. Null when nothing is left.</summary>
        public static string NormRef(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var sb = new StringBuilder(value.Length);
            foreach (char c in value.ToUpperInvariant())
                if (c != ' ' && c != '-' && c != '\t') sb.Append(c);
            string s = sb.ToString().TrimStart('0');
            return s.Length == 0 ? null : s;
        }

        /// <summary>Narration tokens: whole words split on separators, plus their dash-separated parts.</summary>
        public static IEnumerable<string> Tokens(string narration)
        {
            if (string.IsNullOrWhiteSpace(narration)) yield break;
            foreach (string t in narration.Split(TokenSeparators, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return t;
                if (t.IndexOf('-') > 0)
                    foreach (string part in t.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)) yield return part;
            }
        }

        private static IEnumerable<string> Words(string narration)
        {
            if (string.IsNullOrWhiteSpace(narration)) yield break;
            var sb = new StringBuilder();
            foreach (char c in narration.ToUpperInvariant())
            {
                if (c >= 'A' && c <= 'Z') { sb.Append(c); continue; }
                if (sb.Length > 0) { yield return sb.ToString(); sb.Clear(); }
            }
            if (sb.Length > 0) yield return sb.ToString();
        }

        private static string LettersAndDigits(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var sb = new StringBuilder(value.Length);
            foreach (char c in value.ToUpperInvariant())
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')) sb.Append(c);
            return sb.ToString();
        }

        private static string LetterPrefix(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var sb = new StringBuilder();
            foreach (char c in value.ToUpperInvariant())
            {
                if (c >= 'A' && c <= 'Z') sb.Append(c);
                else if (c == '-' || c == ' ') continue;
                else break;
            }
            return sb.ToString();
        }

        private static string Amount(decimal value)
        {
            return value.ToString("N2", CultureInfo.InvariantCulture);
        }

        private static void Add<TKey>(Dictionary<TKey, List<ArDoc>> index, TKey key, ArDoc doc)
        {
            if (key == null) return;
            List<ArDoc> list;
            if (!index.TryGetValue(key, out list)) { list = new List<ArDoc>(); index[key] = list; }
            if (!list.Contains(doc)) list.Add(doc);
        }
    }
}
