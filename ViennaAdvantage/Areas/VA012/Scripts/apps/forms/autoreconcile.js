; VA012 = window.VA012 || {};
/********************************************************
 * Module Name    : VA012
 * Purpose        : Auto Reconcile page (a VIS.CFrame tab of its own), opened from the Bank Statement form.
 *                  Tab 1 Rules: the bank account's optional rules header and its rules.
 *                  Tab 2 Auto reconcile: runs rules and the standard formula on the open statement, reconciles the
 *                  lines that qualify through the form's own save, shows the result page by page, declines suggestions.
 * UI spec        : auto-reconcile-dialog-only.html (VA012 Auto Reconciliation Solution Approach v4)
 * Messages       : VA012_AutoReconcile, VA012_AR* (seeded by Database/AutoReconcile/create-dictionary.ps1);
 *                  every text has an English fallback, so the page works before they are seeded.
 ******************************************************/
(function (VA012, $) {
    var PAGE_SIZE = 50;

    function msg(key, fallback) {
        var t = VIS.Msg.getMsg(key);
        return (!t || t === key || t === "[" + key + "]") ? fallback : t;
    }
    function esc(s) { return VIS.Utility.encodeText(s == null ? "" : String(s)).replace(/"/g, "&quot;"); }
    function amt(n) {
        return Number(n || 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    }
    function day(iso) {
        if (!iso) return "";
        var p = iso.split("-");
        return new Date(+p[0], +p[1] - 1, +p[2]).toLocaleDateString();
    }
    function normRef(s) {
        return (s || "").toUpperCase().replace(/[\s-]/g, "").replace(/^0+/, "");
    }
    /* a reference the narration names, read as the matcher reads it: whole words and their dash-separated parts */
    function narrationHas(narration, value) {
        var v = normRef(value);
        if (!v) return false;
        return (narration || "").split(/[\s\/:,.;()|\\#*]+/).some(function (t) {
            return t && (normRef(t) === v || (t.indexOf("-") > 0 && t.split("-").some(function (p) { return normRef(p) === v; })));
        });
    }
    /* a partner the narration names: the partner's name, letters and digits only, inside the narration's */
    function narrationNames(narration, name) {
        var n = (name || "").toUpperCase().replace(/[^A-Z0-9]/g, "");
        return n.length >= 5 && (narration || "").toUpperCase().replace(/[^A-Z0-9]/g, "").indexOf(n) > -1;
    }
    function parse(data) {
        return typeof data === "string" ? JSON.parse(data) : data;
    }
    function options(list, selected, emptyText) {
        return (emptyText != null ? '<option value="">' + esc(emptyText) + '</option>' : "")
            + (list || []).map(function (o) {
                return '<option value="' + esc(o.id) + '"' + (String(o.id) === String(selected) ? " selected" : "") + '>' + esc(o.name) + '</option>';
            }).join("");
    }
    function num(v) { var n = parseFloat(v); return isNaN(n) ? null : n; }

    /* codes of the dictionary lists; labels in plain words */
    var TYPES = [["PY", "Payment"], ["IS", "Invoice schedule"], ["PO", "Prepay order"], ["CO", "Contra"]];
    var FIELDS = [["NA", "Narration"], ["TR", "Trx No / UTR"], ["CH", "Cheque No"], ["DI", "Direction"], ["AM", "Amount"]];
    var OPS = { CT: "contains", EQ: "equals", SW: "starts with", BT: "between", NE: "is not empty", IS: "is" };
    function opsFor(f) {
        return f === "DI" ? ["IS"] : f === "CH" ? ["EQ", "BT", "SW", "NE"] : f === "AM" ? ["EQ", "BT", "NE"] : ["CT", "EQ", "SW", "NE"];
    }
    var KEYS = [["TR", "Trx No / UTR"], ["CH", "Cheque no."], ["BP", "Business partner"], ["AM", "Amount"], ["DN", "Document no. in narration"]];
    var CHECK_SOURCES = [["S", "Statement first, then cheque book"], ["B", "Cheque book only"], ["M", "Ask"]];
    var AUTO_TIP = "On: lines matched on UTR or cheque no. and amount, on partner and amount, or on a document no. in the narration "
        + "with the same amount and partner, and lines matched by a rule set to auto-confirm, are reconciled automatically. "
        + "Off: nothing is reconciled automatically; every match is suggested and a user accepts it.";

    /**
     * opts: { windowNo, bankAccountId, bankOrgId, accountName, statementId (optional, preselected), onChanged, onClosed }
     */
    VA012.AutoReconcile = function (opts) {
        var windowNo = opts.windowNo;
        var url = VIS.Application.contextUrl + "VA012/BankStatement/";
        var $root, $tabs, $rulesView, $runView;
        var $stmt, $chips, $pm, $bpHolder, $run, $hint, $bar, $msg, $results, $body, $selAll, $work, $dp, $dpList;
        var $hdrBox, $rulesArea, $ruleList, $editor, $rulesMsg;
        var bpControl = null, chargeControl = null, frame = null, xhr = null, $busy = null, busyCount = 0;
        var docType = "";
        // server work in progress (Reconcile, Accept): closing the page then asks first; closeThen runs once the page is closed
        var serverWork = 0, closeConfirmed = false, closeThen = null;
        var setup = null, payMethods = [], editing = 0, draft = null;
        /* the list shows every line of the chosen statement (statementId) with its status, before and after a run */
        var st = {
            statementId: 0, filter: "", page: 0, loading: false, done: true, counts: null, dateWindow: 3, rows: {}, order: [], expanded: {}, selected: {},
            defaults: {}, choices: {}, chargeCtrls: {}, payOpen: false, bookNext: {}, taxCalc: {}
        };

        // [filter, label, count key]; "Not run" only shows while the statement has lines no run covered. No Declined chip:
        // declined lines are listed (and counted) under No match, where they are matched by hand
        var FILTERS = [
            ["", msg("VA012_ARAllLines", "All"), "total"],
            ["C", msg("VA012_ARReconciled", "Reconciled"), "reconciled"],
            ["S", msg("VA012_ARSuggested", "Suggested"), "suggested"],
            ["N", msg("VA012_ARNoMatch", "No match"), "noMatch"],
            ["O", msg("VA012_ARNotRun", "Not run"), "open"]
        ];
        function typeName(code) { for (var i = 0; i < TYPES.length; i++) if (TYPES[i][0] === code) return TYPES[i][1]; return code; }

        /* ------------------------------------------------------------------ shell */
        function build() {
            $root = $(
                '<div class="va012-ar">'
                + '<div class="va012-ar-tabs" role="tablist">'
                + '<button type="button" role="tab" class="va012-ar-tab" data-view="rules" aria-selected="false">' + esc(msg("VA012_ARRules", "1 · Rules")) + ' <span class="va012-ar-pill gray va012-ar-rcount">0</span></button>'
                + '<button type="button" role="tab" class="va012-ar-tab" data-view="run" aria-selected="true">' + esc(msg("VA012_ARRunTab", "2 · Auto reconcile")) + '</button>'
                + '</div>'
                + '<div class="va012-ar-view va012-ar-rulesview" hidden>'
                + '<div class="va012-ar-msg va012-ar-rulesmsg" role="status" hidden></div>'
                + '<div class="va012-ar-hdrbox"></div>'
                + '<div class="va012-ar-rulesarea" hidden><h3 class="va012-ar-rulestitle"></h3>'
                + '<div class="va012-ar-split"><div class="va012-ar-listcol"><div class="va012-ar-rulelist"></div>'
                + '<button type="button" class="va012-ar-btn va012-ar-newrule">' + esc(msg("VA012_ARNewRule", "+ New rule")) + '</button></div>'
                + '<div class="va012-ar-editor"></div></div></div>'
                + '</div>'
                + '<div class="va012-ar-view va012-ar-runview"></div>'
                + '<div class="va012-ar-busy" hidden><div class="vis-apanel-busy"></div></div>'
                + '</div>');
            $busy = $root.find(".va012-ar-busy");
            $tabs = $root.find(".va012-ar-tabs");
            $rulesView = $root.find(".va012-ar-rulesview");
            $runView = $root.find(".va012-ar-runview");
            $rulesMsg = $root.find(".va012-ar-rulesmsg");
            $hdrBox = $root.find(".va012-ar-hdrbox");
            $rulesArea = $root.find(".va012-ar-rulesarea");
            $ruleList = $root.find(".va012-ar-rulelist");
            $editor = $root.find(".va012-ar-editor");
            buildRunView();
            bindRulesEvents();
            $tabs.on("click", "button", function () { showView($(this).data("view")); });
            loadSetup();
            loadPaymentMethods();
        }

        /* lines were reconciled: the Bank Statement form reloads its statement lines, totals and documents */
        function formChanged() {
            if (typeof opts.onChanged !== "function") return;
            try { opts.onChanged(); } catch (e) { if (window.console) console.log(e); }
        }

        /* the framework's busy indicator over the page while a call is running (calls may overlap); it stays at least
           400 ms so a quick save on the Rules tab is still visible */
        var busySince = 0, busyTimer = null;
        function busy(on) {
            busyCount = Math.max(0, busyCount + (on ? 1 : -1));
            if (!$busy) return;
            if (busyCount > 0) {
                if (busyTimer) { clearTimeout(busyTimer); busyTimer = null; }
                if ($busy.prop("hidden")) { busySince = Date.now(); $busy.prop("hidden", false); }
                return;
            }
            var wait = Math.max(0, 400 - (Date.now() - busySince));
            busyTimer = setTimeout(function () { busyTimer = null; if (busyCount === 0) $busy.prop("hidden", true); }, wait);
        }

        function showView(view) {
            $tabs.find("button").each(function () {
                var selected = $(this).data("view") === view;
                $(this).attr("aria-selected", selected ? "true" : "false");
                plainTab(this, selected);
            });
            $rulesView.prop("hidden", view !== "rules");
            $runView.prop("hidden", view !== "run");
        }

        /* plain text tabs: inline !important wins over the theme's focus border and shadow on buttons */
        function plainTab(el, selected) {
            var s = el.style;
            s.setProperty("border-top", "0", "important");
            s.setProperty("border-left", "0", "important");
            s.setProperty("border-right", "0", "important");
            s.setProperty("border-bottom", "2px solid " + (selected ? "rgba(var(--v-c-primary), 1)" : "transparent"), "important");
            s.setProperty("border-radius", "0", "important");
            s.setProperty("box-shadow", "none", "important");
            s.setProperty("outline", "none", "important");
            s.setProperty("background", "transparent", "important");
        }

        function post(action, data, done) {
            busy(true);
            $.ajax({
                url: url + action, type: "POST", data: data,
                success: function (d) {
                    var res = parse(d) || {};
                    if (res.error) { rulesMessage("bad", res.error); return; }
                    if (done) done(res);
                },
                error: function () { rulesMessage("bad", msg("VA012_ErrorSaving", "Something went wrong.")); },
                complete: function () { busy(false); }
            });
        }

        var rulesMsgTimer = null;
        function rulesMessage(kind, text) {
            clearTimeout(rulesMsgTimer);
            $rulesMsg.stop(true, true);
            if (!text) { $rulesMsg.hide().attr("hidden", true).html(""); return; }
            $rulesMsg.removeAttr("hidden").attr("class", "va012-ar-msg va012-ar-rulesmsg " + kind).text(text).show();
            // a confirmation ("Saved.") goes away by itself; a problem stays until the next action
            if (kind === "ok") rulesMsgTimer = setTimeout(function () { $rulesMsg.fadeOut(400, function () { rulesMessage(); }); }, 3000);
        }

        function loadSetup(selectRuleId) {
            busy(true);
            $.ajax({
                url: url + "GetAutoReconcileSetup", type: "GET", data: { bankAccountId: opts.bankAccountId },
                success: function (d) { setupLoaded(parse(d), selectRuleId); },
                error: function () { rulesMessage("bad", msg("VA012_ErrorSaving", "Something went wrong.")); },
                complete: function () { busy(false); }
            });
        }

        function setupLoaded(data, selectRuleId) {
            setup = data || { header: null, rules: [], headers: [] };
            var rules = setup.rules || [];
            editing = selectRuleId != null ? selectRuleId : (rules.length ? rules[0].id : 0);
            var current = rules.filter(function (r) { return r.id === editing; })[0];
            draft = current ? JSON.parse(JSON.stringify(current)) : blankRule();
            if (!current) editing = 0;
            renderRulesView();
            updateRunHint();
        }

        function loadPaymentMethods() {
            VIS.dataContext.getJSONData(url + "GetPaymentMethods", { BankOrgId: VIS.Utility.Util.getValueOfInt(opts.bankOrgId) }, function (ds) {
                payMethods = (ds || []).map(function (m) { return { id: VIS.Utility.Util.getValueOfInt(m.chargeID), name: m.name, base: m.PaymentBaseType }; });
                payMethods.forEach(function (m) { $pm.append('<option value="' + m.id + '">' + esc(m.name) + '</option>'); });
                if (setup) renderEditor();
            });
        }

        /* ------------------------------------------------------------------ tab 1: header */
        function activeRules() {
            if (!setup || !setup.header || !setup.header.active) return [];
            return (setup.rules || []).filter(function (r) { return r.active; });
        }

        function renderRulesView() {
            var n = activeRules().length;
            $tabs.find(".va012-ar-rcount").text(n).attr("class", "va012-ar-pill va012-ar-rcount " + (n ? "ok" : "gray"));
            renderHeader();
            renderRuleList();
            renderEditor();
        }

        function renderHeader() {
            var h = setup.header, acct = opts.accountName || "";
            if (chargeControl) { if (chargeControl.disposeComponent) chargeControl.disposeComponent(); chargeControl = null; }
            if (!h) {
                var others = setup.headers || [];
                var hopts = others.map(function (x) {
                    return '<option value="' + x.id + '">' + esc(x.name) + '</option>';
                }).join("");
                $hdrBox.html(
                    '<div class="va012-ar-empty va012-ar-setup"><span>' + esc(msg("VA012_ARNoHeader", "Rules are optional. Standard matching works without them. Set up a rules header for this bank account to add rules.")) + '</span>'
                    + (others.length
                        ? '<div class="va012-ar-cards">'
                        + '<div class="va012-ar-card"><h4>' + esc(msg("VA012_ARLinkHeader", "Link an existing header")) + '</h4>'
                        + '<p class="va012-ar-muted">' + esc(msg("VA012_ARLinkNote", "No new data. This account uses the same header and rules, so a change to a rule applies to every linked account.")) + '</p>'
                        + '<select class="va012-ar-linkfrom">' + hopts + '</select>'
                        + '<button type="button" class="va012-ar-btn primary va012-ar-linkhdr">' + esc(msg("VA012_ARLink", "Link")) + '</button></div>'
                        + '<div class="va012-ar-card"><h4>' + esc(msg("VA012_ARCopyHeader", "Copy a header")) + '</h4>'
                        + '<p class="va012-ar-muted">' + esc(msg("VA012_ARCopyNote", "Creates a new header with its own copy of the rules, under a name you give. Later changes stay separate.")) + '</p>'
                        + '<select class="va012-ar-copyfrom">' + hopts + '</select>'
                        + '<input type="text" class="va012-ar-copyname" value="' + esc(acct + " rules") + '" aria-label="' + esc(msg("VA012_ARNewHeaderName", "New header name")) + '">'
                        + '<button type="button" class="va012-ar-btn va012-ar-copyhdr">' + esc(msg("VA012_ARCopy", "Copy header and rules")) + '</button></div>'
                        + '</div>'
                        : '')
                    + '<div class="va012-ar-newhdr"><input type="text" class="va012-ar-newname" value="' + esc(acct + " rules") + '" aria-label="' + esc(msg("VA012_ARNewHeaderName", "New header name")) + '">'
                    + '<button type="button" class="va012-ar-btn' + (others.length ? '' : ' primary') + ' va012-ar-mkhdr">'
                    + esc(others.length ? msg("VA012_ARCreateBlank", "Or create a blank header") : msg("VA012_ARCreateHeader", "Create rules header")) + '</button></div>'
                    + '</div>');
                $rulesArea.prop("hidden", true);
                return;
            }
            $rulesArea.prop("hidden", false);
            $hdrBox.html(
                '<div class="va012-ar-hdr">'
                + '<div class="va012-ar-params">'
                + '<div class="va012-ar-fld va012-ar-grow"><label>' + esc(msg("VA012_ARHeaderName", "Rules header")) + ' <sup>*</sup></label><input type="text" class="va012-ar-hname" value="' + esc(h.name) + '"></div>'
                + '<label class="va012-ar-check"><input type="checkbox" class="va012-ar-hactive"' + (h.active ? " checked" : "") + '> ' + esc(msg("VA012_ARActive", "Active")) + '</label>'
                + '<label class="va012-ar-check" title="' + esc(msg("VA012_ARAutoMatchTip", AUTO_TIP)) + '"><input type="checkbox" class="va012-ar-hauto"' + (h.autoMatch ? " checked" : "") + '> '
                + esc(msg("VA012_ARAutoMatch", "Auto Reconciliation")) + '</label>'
                + '</div>'
                + '<div class="va012-ar-params">'
                + '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARDateWindow", "Date window ± days")) + '</label><input type="number" min="0" class="va012-ar-hwin" value="' + esc(h.dateWindow) + '"></div>'
                + '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARTolerance", "Amount tolerance")) + '</label><input type="number" min="0" step="0.01" class="va012-ar-htol" value="' + esc(h.tolerance) + '"></div>'
                + '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARDefaultCharge", "Default charge")) + '</label><div class="va012-ar-chg"></div></div>'
                + '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARDefaultTax", "Default tax rate")) + '</label><select class="va012-ar-htax">' + options(setup.taxes, h.taxId, msg("VA012_ARAskEachTime", "Ask each time")) + '</select></div>'
                + '<div class="va012-ar-fld va012-ar-acts"><button type="button" class="va012-ar-btn primary va012-ar-savehdr">' + esc(msg("VA012_ARSaveHeader", "Save header")) + '</button>'
                + '<button type="button" class="va012-ar-btn va012-ar-unlink">' + esc(msg("VA012_ARUnlink", "Unlink from this account")) + '</button></div>'
                + '</div></div>');
            // default charge: the form's own charge search lookup
            var chargeLookup = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), windowNo, 3787, VIS.DisplayType.Search);
            chargeControl = new VIS.Controls.VTextBoxButton("C_Charge_ID", false, false, true, VIS.DisplayType.Search, chargeLookup);
            $hdrBox.find(".va012-ar-chg").append(chargeControl.getControl().addClass("va012-ar-bptext").attr("placeholder", msg("VA012_ARAskEachTime", "Ask each time")))
                .append(chargeControl.getBtn(0).addClass("va012-ar-bpbtn"));
            if (h.chargeId > 0) chargeControl.setValue(h.chargeId);
            $root.find(".va012-ar-rulestitle").text(msg("VA012_ARRulesUnder", "Rules under") + " “" + h.name + "” (" + (setup.rules || []).length + ")");
        }

        /* ------------------------------------------------------------------ tab 1: rules */
        function renderRuleList() {
            if (!setup.header) { $ruleList.empty(); return; }
            var rules = (setup.rules || []).slice().sort(function (a, b) { return a.seqNo - b.seqNo; });
            if (!rules.length) {
                $ruleList.html('<div class="va012-ar-muted va012-ar-pad">' + esc(msg("VA012_ARNoRules", "No rules under this header yet. Add the first one.")) + '</div>');
                return;
            }
            $ruleList.html('<table><thead><tr><th>#</th><th>' + esc(msg("VA012_ARRule", "Rule")) + '</th><th>' + esc(msg("VA012_ARAppliesTo", "Applies to")) + '</th><th>'
                + esc(msg("VA012_ARControl", "Control")) + '</th><th>' + esc(msg("VA012_AROn", "On")) + '</th></tr></thead><tbody>'
                + rules.map(function (r) {
                    return '<tr tabindex="0" class="va012-ar-rulerow' + (r.id === editing ? " sel" : "") + '" data-rid="' + r.id + '">'
                        + '<td>' + r.seqNo + '</td><td>' + esc(r.name)
                        + (r.multiDoc && r.appliesTo.indexOf("IS") > -1 ? ' <span class="va012-ar-pill gray">' + esc(msg("VA012_ARSeveralDocs", "Several docs")) + '</span>' : '')
                        // set up in the rules window without a key or a document type: the run skips it until that is added
                        + (!r.keys || !r.keys.length ? ' <span class="va012-ar-pill warn" title="' + esc(msg("VA012_ARSkippedTip", "The run skips this rule until it has one.")) + '">' + esc(msg("VA012_ARNoMatchKey", "No match key")) + '</span>' : '')
                        + (!r.appliesTo.length ? ' <span class="va012-ar-pill warn" title="' + esc(msg("VA012_ARSkippedTip", "The run skips this rule until it has one.")) + '">' + esc(msg("VA012_ARNoDocType", "No document type")) + '</span>' : '') + '</td>'
                        + '<td>' + r.appliesTo.map(function (t) { return '<span class="va012-ar-pill gray">' + esc(typeName(t)) + '</span>'; }).join(" ") + '</td>'
                        + '<td><span class="va012-ar-pill ' + (r.control === "A" ? "ok" : "warn") + '">' + esc(r.control === "A" ? msg("VA012_ARAutoConfirm", "Auto-confirm") : msg("VA012_ARSuggestOnly", "Suggest only")) + '</span></td>'
                        + '<td><input type="checkbox" data-on="' + r.id + '"' + (r.active ? " checked" : "") + ' aria-label="' + esc(r.name) + '"></td></tr>';
                }).join("") + '</tbody></table>');
        }

        function blankRule() {
            var max = 0;
            ((setup && setup.rules) || []).forEach(function (r) { if (r.seqNo > max) max = r.seqNo; });
            return {
                id: 0, seqNo: max + 10, name: "", active: true, appliesTo: ["PY"], dateWindow: null, tolerance: null, control: "S", controlCap: null,
                differenceType: "", onDiff: "A", payMethodId: 0, conversionTypeId: 0, checkSource: "S", multiDoc: true, minDocs: 2, onMissing: "W",
                conds: [], keys: [{ key: "AM", required: false }]
            };
        }

        function keyOf(k) { return (draft.keys || []).filter(function (x) { return x.key === k; })[0]; }

        function renderEditor() {
            if (!setup || !setup.header || !draft) { $editor.empty(); return; }
            var d = draft, has = function (t) { return d.appliesTo.indexOf(t) > -1; };
            var hasIS = has("IS"), newPayment = hasIS || has("PO"), hasCharge = has("PY") || has("PO") || hasIS;
            var conds = (d.conds || []).map(function (c, i) {
                var ops = opsFor(c.field);
                if (ops.indexOf(c.op) < 0) c.op = ops[0];
                var value;
                if (c.field === "DI") value = '<select data-ci="' + i + '" data-cf="value"><option value="C"' + (c.value !== "D" ? " selected" : "") + '>' + esc(msg("VA012_ARCredit", "Credit"))
                    + '</option><option value="D"' + (c.value === "D" ? " selected" : "") + '>' + esc(msg("VA012_ARDebit", "Debit")) + '</option></select>';
                else if (c.op === "BT") value = '<input type="text" data-ci="' + i + '" data-cf="value" placeholder="' + esc(msg("VA012_ARFrom", "From")) + '" value="' + esc(c.value) + '">'
                    + '<span class="va012-ar-muted">' + esc(msg("VA012_ARTo", "to")) + '</span><input type="text" data-ci="' + i + '" data-cf="value2" placeholder="' + esc(msg("VA012_ARTo", "To")) + '" value="' + esc(c.value2) + '">';
                else value = '<input type="text" data-ci="' + i + '" data-cf="value" value="' + esc(c.value) + '"' + (c.op === "NE" ? " disabled" : "")
                    + (c.field === "NA" ? ' placeholder="' + esc(msg("VA012_ARCommaHint", "one or more, comma-separated")) + '"' : '') + '>';
                return '<div class="va012-ar-cond"><select data-ci="' + i + '" data-cf="field">' + FIELDS.map(function (f) { return '<option value="' + f[0] + '"' + (c.field === f[0] ? " selected" : "") + '>' + esc(f[1]) + '</option>'; }).join("") + '</select>'
                    + '<select data-ci="' + i + '" data-cf="op">' + ops.map(function (o) { return '<option value="' + o + '"' + (c.op === o ? " selected" : "") + '>' + esc(OPS[o]) + '</option>'; }).join("") + '</select>'
                    + value + '<button type="button" class="va012-ar-sbtn" data-rmcond="' + i + '">' + esc(msg("VA012_ARRemove", "Remove")) + '</button></div>';
            }).join("");
            var keys = KEYS.map(function (k) {
                var on = keyOf(k[0]);
                return '<span class="va012-ar-key"><label class="va012-ar-opt"><input type="checkbox" data-key="' + k[0] + '"' + (on ? " checked" : "") + '> ' + esc(k[1]) + '</label>'
                    + (on ? '<select data-keyreq="' + k[0] + '"><option value="R"' + (on.required ? " selected" : "") + '>' + esc(msg("VA012_ARRequired", "Required")) + '</option><option value="P"'
                        + (on.required ? "" : " selected") + '>' + esc(msg("VA012_ARPreferred", "Preferred")) + '</option></select>' : '') + '</span>';
            }).join("");
            var applies = TYPES.map(function (t) { return '<label class="va012-ar-opt"><input type="checkbox" data-applies="' + t[0] + '"' + (has(t[0]) ? " checked" : "") + '> ' + esc(t[1]) + '</label>'; }).join("");
            var thenText = d.multiDoc && hasIS ? msg("VA012_ARThenSeveral", "Reserve every invoice found in the narration when their total equals the line amount, and create one payment")
                : newPayment ? msg("VA012_ARThenCreate", "Match, and create the payment") : msg("VA012_ARThenMatch", "Match the document");

            $editor.html(
                row(msg("VA012_ARRule", "Rule"), '<input type="text" data-f="name" class="va012-ar-wide" value="' + esc(d.name) + '">', true)
                + row(msg("VA012_ARAppliesTo", "Applies to"), applies, true)
                + row(msg("VA012_ARPriority", "Priority"), '<input type="number" data-f="seqNo" value="' + esc(d.seqNo) + '"><span class="va012-ar-muted">' + esc(msg("VA012_ARLowerFirst", "lower runs first")) + '</span>', true)
                + row(msg("VA012_ARWhen", "When"), conds + '<button type="button" class="va012-ar-sbtn va012-ar-addcond">' + esc(msg("VA012_ARAddCondition", "+ Add condition")) + '</button>'
                    + (d.conds && d.conds.length ? '' : '<span class="va012-ar-muted">' + esc(msg("VA012_ARNoCondition", "no condition: the rule is tried on every line")) + '</span>'))
                + row(msg("VA012_ARMatchOn", "Match on"), keys + '<span class="va012-ar-muted va012-ar-full">' + esc(msg("VA012_ARKeysNote",
                    "Required: the candidate is rejected if the value is missing or differs. Preferred: used to choose between candidates; a missing value does not reject. A Trx no. or cheque no. that is present but different always rejects the candidate.")) + '</span>', true)
                + (hasIS ? row(msg("VA012_ARDocuments", "Documents"), '<label class="va012-ar-opt"><input type="checkbox" data-f="multiDoc"' + (d.multiDoc ? " checked" : "") + '> ' + esc(msg("VA012_ARAllowSeveral", "Allow several documents for one line")) + '</label>'
                    + (d.multiDoc ? '<label>' + esc(msg("VA012_ARMinInvoices", "Minimum invoices")) + '</label><input type="number" min="2" data-f="minDocs" value="' + esc(d.minDocs || 2) + '">'
                        // on its own line, so the choice is readable
                        + '<div class="va012-ar-sub"><label>' + esc(msg("VA012_ARIfNotFound", "If a number is not found")) + '</label>'
                        + '<select data-f="onMissing"><option value="W"' + (d.onMissing !== "S" ? " selected" : "") + '>' + esc(msg("VA012_ARWarnSuggest", "Warn and suggest (never auto-confirm)"))
                        + '</option><option value="S"' + (d.onMissing === "S" ? " selected" : "") + '>' + esc(msg("VA012_ARDoNotMatch", "Do not match")) + '</option></select>'
                        + '<span class="va012-ar-muted">' + esc(msg("VA012_ARNotFoundNote", "a number in the narration that is not an open invoice of this partner")) + '</span></div>' : '')) : '')
                + row(msg("VA012_ARAllow", "Allow"), '<label>' + esc(msg("VA012_ARDateWindow", "Date window ± days")) + '</label><input type="number" min="0" data-f="dateWindow" value="' + esc(d.dateWindow == null ? "" : d.dateWindow) + '" placeholder="' + esc(setup.header.dateWindow) + '">'
                    + '<label>' + esc(msg("VA012_ARTolerance", "Amount tolerance")) + '</label><input type="number" min="0" step="0.01" data-f="tolerance" value="' + esc(d.tolerance == null ? "" : d.tolerance) + '" placeholder="' + esc(setup.header.tolerance) + '">'
                    + '<span class="va012-ar-muted">' + esc(msg("VA012_AREmptyHeader", "empty: the header's")) + '</span>')
                + row(msg("VA012_ARThen", "Then"), '<span>' + esc(thenText) + '</span>')
                + (hasCharge ? row(msg("VA012_ARAmountDiffers", "Amount differs"), '<select data-f="onDiff"><option value="A"' + (d.onDiff !== "H" ? " selected" : "") + '>' + esc(msg("VA012_ARAskChargeTax", "Ask for charge and tax rate"))
                    + '</option><option value="H"' + (d.onDiff === "H" ? " selected" : "") + '>' + esc(msg("VA012_ARUseHeaderCharge", "Use the header's charge and tax rate")) + '</option></select><span class="va012-ar-muted">' + esc(msg("VA012_ARChargeWhen", "payment and prepay order; invoice schedule when the statement amount is more than the invoices")) + '</span>') : '')
                + (hasIS ? row(msg("VA012_ARDifferenceType", "Difference type"), '<select data-f="differenceType">' + options(setup.differenceTypes, d.differenceType, msg("VA012_ARAskEachTime", "Ask each time")) + '</select><span class="va012-ar-muted">' + esc(msg("VA012_ARDiffTypeWhen", "invoice schedule when the invoices are more than the statement amount")) + '</span>') : '')
                + (newPayment ? row(msg("VA012_ARPayment", "Payment"), '<label>' + esc(msg("VA012_ARMethod", "Method")) + '</label><select data-f="payMethodId">' + options(payMethods, d.payMethodId || "", msg("VA012_ARFromDocument", "From document")) + '</select>'
                    + '<label>' + esc(msg("VA012_ARConversionType", "Conversion type")) + '</label><select data-f="conversionTypeId">' + options(setup.conversionTypes, d.conversionTypeId || "", msg("VA012_ARFromDocument", "From document")) + '</select>') : '')
                + (newPayment ? row(msg("VA012_ARChequeNo", "Cheque no."), '<select data-f="checkSource">' + CHECK_SOURCES.map(function (c) { return '<option value="' + c[0] + '"' + (d.checkSource === c[0] ? " selected" : "") + '>' + esc(c[1]) + '</option>'; }).join("")
                    + '</select><span class="va012-ar-muted">' + esc(msg("VA012_ARChequeNote", "for a new payment with a cheque method")) + '</span>') : '')
                + row(msg("VA012_ARControl", "Control"), '<label class="va012-ar-opt"><input type="radio" name="va012-ar-ctl-' + windowNo + '" data-f="control" value="A"' + (d.control === "A" ? " checked" : "") + '> ' + esc(msg("VA012_ARAutoConfirm", "Auto-confirm")) + '</label>'
                    + '<label class="va012-ar-opt"><input type="radio" name="va012-ar-ctl-' + windowNo + '" data-f="control" value="S"' + (d.control !== "A" ? " checked" : "") + '> ' + esc(msg("VA012_ARSuggestOnly", "Suggest only")) + '</label>'
                    + '<label>' + esc(msg("VA012_ARUpTo", "up to")) + '</label><input type="number" min="0" step="0.01" data-f="controlCap" value="' + esc(d.controlCap == null ? "" : d.controlCap) + '" placeholder="' + esc(msg("VA012_ARNoCap", "no cap")) + '"' + (d.control === "A" ? "" : " disabled") + '>', true)
                + '<div class="va012-ar-acts"><button type="button" class="va012-ar-btn primary va012-ar-saverule">' + esc(editing ? msg("VA012_ARSaveRule", "Save rule") : msg("VA012_ARAddRule", "Add rule")) + '</button>'
                + (editing ? '<button type="button" class="va012-ar-btn va012-ar-delrule">' + esc(msg("VA012_ARDelete", "Delete")) + '</button>' : '') + '</div>');

            // required: a red asterisk after the label, as on the run parameters
            function row(label, ctl, required) {
                return '<div class="va012-ar-erow"><span class="va012-ar-k">' + esc(label) + (required ? ' <sup>*</sup>' : '') + '</span><div class="va012-ar-ctl">' + ctl + '</div></div>';
            }
        }

        function bindRulesEvents() {
            // header set-up
            $hdrBox.on("click", ".va012-ar-mkhdr", function () {
                post("CreateAutoReconcileHeader", { bankAccountId: opts.bankAccountId, name: $hdrBox.find(".va012-ar-newname").val() }, function () { rulesMessage(); loadSetup(); });
            });
            $hdrBox.on("click", ".va012-ar-linkhdr", function () {
                post("LinkAutoReconcileHeader", { bankAccountId: opts.bankAccountId, headerId: $hdrBox.find(".va012-ar-linkfrom").val() }, function () { rulesMessage(); loadSetup(); });
            });
            $hdrBox.on("click", ".va012-ar-copyhdr", function () {
                post("CopyAutoReconcileHeader", { bankAccountId: opts.bankAccountId, fromHeaderId: $hdrBox.find(".va012-ar-copyfrom").val(), name: $hdrBox.find(".va012-ar-copyname").val() },
                    function () { rulesMessage(); loadSetup(); });
            });
            $hdrBox.on("click", ".va012-ar-unlink", function () {
                VIS.ADialog.confirm("VA012_ARUnlinkConfirm", true, "", "Confirm", function (ok) {
                    if (ok) post("LinkAutoReconcileHeader", { bankAccountId: opts.bankAccountId, headerId: 0 }, function () { rulesMessage(); loadSetup(); });
                });
            });
            $hdrBox.on("click", ".va012-ar-savehdr", function () {
                var header = {
                    name: $hdrBox.find(".va012-ar-hname").val(),
                    active: $hdrBox.find(".va012-ar-hactive").is(":checked"),
                    autoMatch: $hdrBox.find(".va012-ar-hauto").is(":checked"),
                    dateWindow: VIS.Utility.Util.getValueOfInt($hdrBox.find(".va012-ar-hwin").val()),
                    tolerance: num($hdrBox.find(".va012-ar-htol").val()) || 0,
                    chargeId: VIS.Utility.Util.getValueOfInt(chargeControl ? chargeControl.getValue() : 0),
                    taxId: VIS.Utility.Util.getValueOfInt($hdrBox.find(".va012-ar-htax").val())
                };
                post("SaveAutoReconcileHeader", { headerId: setup.header.id, header: JSON.stringify(header) }, function () {
                    rulesMessage("ok", msg("VA012_ARSaved", "Saved."));
                    loadSetup(editing);
                });
            });

            // rule list
            $ruleList.on("click keydown", "tr.va012-ar-rulerow", function (e) {
                if (e.type === "keydown" && e.key !== "Enter" && e.key !== " ") return;
                if ($(e.target).is("input")) return;
                var id = $(this).data("rid");
                var r = (setup.rules || []).filter(function (x) { return x.id === id; })[0];
                if (!r) return;
                editing = id; draft = JSON.parse(JSON.stringify(r));
                rulesMessage(); renderRuleList(); renderEditor();
            });
            $ruleList.on("change", "input[data-on]", function () {
                var id = $(this).data("on"), on = this.checked;
                post("SetAutoReconcileRuleActive", { ruleId: id, active: on }, function () { loadSetup(editing); });
            });
            $rulesArea.on("click", ".va012-ar-newrule", function () {
                editing = 0; draft = blankRule(); rulesMessage(); renderRuleList(); renderEditor();
                $editor.find('[data-f="name"]').trigger("focus");
            });

            // editor
            $editor.on("change input", "[data-f]", function (e) {
                var f = $(this).data("f"), v;
                if (this.type === "checkbox") v = this.checked;
                else if (this.type === "radio") { if (!this.checked) return; v = this.value; }
                else v = $(this).val();
                if (f === "minDocs" || f === "payMethodId" || f === "conversionTypeId") v = VIS.Utility.Util.getValueOfInt(v);
                if (f === "dateWindow" || f === "seqNo") v = v === "" ? null : VIS.Utility.Util.getValueOfInt(v);
                if (f === "tolerance" || f === "controlCap") v = v === "" ? null : num(v);
                draft[f] = v;
                if (e.type === "change" && (f === "control" || f === "multiDoc")) renderEditor();
            });
            $editor.on("change", "[data-applies]", function () {
                var t = $(this).data("applies");
                draft.appliesTo = draft.appliesTo.filter(function (x) { return x !== t; });
                if (this.checked) draft.appliesTo.push(t);
                renderEditor();
            });
            $editor.on("change input", "[data-ci]", function (e) {
                var i = $(this).data("ci"), f = $(this).data("cf");
                draft.conds[i][f] = $(this).val();
                if (f === "field") { draft.conds[i].op = opsFor(draft.conds[i].field)[0]; draft.conds[i].value = draft.conds[i].field === "DI" ? "C" : ""; draft.conds[i].value2 = ""; }
                if (e.type === "change" && (f === "field" || f === "op")) renderEditor();
            });
            $editor.on("click", "[data-rmcond]", function () { draft.conds.splice($(this).data("rmcond"), 1); renderEditor(); });
            $editor.on("click", ".va012-ar-addcond", function () { draft.conds.push({ field: "NA", op: "CT", value: "", value2: "" }); renderEditor(); });
            $editor.on("change", "[data-key]", function () {
                var k = $(this).data("key");
                draft.keys = draft.keys.filter(function (x) { return x.key !== k; });
                if (this.checked) draft.keys.push({ key: k, required: k === "TR" || k === "CH" });
                renderEditor();
            });
            $editor.on("change", "[data-keyreq]", function () { keyOf($(this).data("keyreq")).required = $(this).val() === "R"; });
            $editor.on("click", ".va012-ar-saverule", function () {
                var problem = ruleProblem(draft);
                if (problem) { rulesMessage("bad", problem); return; }
                post("SaveAutoReconcileRule", { headerId: setup.header.id, rule: JSON.stringify(draft) }, function (res) {
                    rulesMessage("ok", msg("VA012_ARSaved", "Saved."));
                    loadSetup(res.id);
                });
            });
            $editor.on("click", ".va012-ar-delrule", function () {
                VIS.ADialog.confirm("VA012_ARDeleteRuleConfirm", true, "", "Confirm", function (ok) {
                    if (ok) post("DeleteAutoReconcileRule", { ruleId: editing }, function () { rulesMessage(); loadSetup(null); });
                });
            });
        }

        function ruleProblem(d) {
            if (!d.name || !d.name.trim()) return msg("VA012_ARNameRequired", "Give the rule a name.");
            if (!d.appliesTo.length) return msg("VA012_ARChooseType", "Choose at least one document type.");
            if (d.seqNo == null) return msg("VA012_ARPriorityRequired", "Enter a priority.");
            if (!d.keys.length) return msg("VA012_ARChooseKey", "Tick at least one Match on key.");
            for (var i = 0; i < d.conds.length; i++) {
                var c = d.conds[i];
                if (c.op === "BT" && (!c.value || !c.value2)) return msg("VA012_ARBetweenNeedsTwo", "A between condition needs From and To.");
                if (c.op !== "NE" && c.op !== "BT" && !c.value) return msg("VA012_ARConditionValue", "Every condition needs a value.");
            }
            return null;
        }

        /* ------------------------------------------------------------------ tab 2: run */
        /* "Matched by": plain words, never case numbers */
        function matchedBy(r) {
            if (r.status === "M") return msg("VA012_ARMatchManual", "In the form");
            if (r.status === "O") return "-";
            if (r.matchCase === "DR") return msg("VA012_ARMatchDragged", "Dragged onto the line");
            if (r.ruleName) return msg("VA012_ARMatchRule", "Rule") + ": " + r.ruleName;
            switch (r.matchCase) {
                case "C1": return r.signal === "narration"
                    ? msg("VA012_ARMatchC1N", "Same UTR or cheque no. (from narration) and amount")
                    : msg("VA012_ARMatchC1", "Same UTR or cheque no. and amount");
                case "CN": return msg("VA012_ARMatchCN", "Invoice or order no. in narration");
                case "C2": return msg("VA012_ARMatchC2", "Same date and amount");
                case "C3": return msg("VA012_ARMatchC3", "Same partner and amount, within") + " " + st.dateWindow + " " + msg("VA012_ARDays", "days");
                case "C4": return msg("VA012_ARMatchC4", "Same partner and amount");
                default: return msg("VA012_ARNoMatch", "No match");
            }
        }
        function isTie(r) { return r.status === "S" && r.docs.length > 0 && r.docs.every(function (d) { return !d.reserved; }); }
        function statusPill(r) {
            if (r.status === "R") return '<span class="va012-ar-pill gray">' + esc(msg("VA012_ARReleased", "Released")) + '</span>';
            if (r.status === "C" || r.status === "A" || r.status === "M") return '<span class="va012-ar-pill ok">' + esc(msg("VA012_ARReconciledAuto", "Reconciled")) + '</span>';
            if (r.status === "O") return '<span class="va012-ar-pill gray">' + esc(msg("VA012_ARNotRun", "Not run")) + '</span>';
            if (r.status === "S" && isTie(r)) return '<span class="va012-ar-pill warn">' + esc(msg("VA012_ARChooseDoc", "Choose a document")) + '</span>';
            if (r.status === "S") return '<span class="va012-ar-pill warn">' + esc(msg("VA012_ARSuggested", "Suggested")) + '</span>';
            if (r.status === "D") return '<span class="va012-ar-pill bad">' + esc(msg("VA012_ARDeclined", "Declined")) + '</span>';
            return '<span class="va012-ar-pill gray">' + esc(msg("VA012_ARNoMatch", "No match")) + '</span>';
        }

        function buildRunView() {
            var typeKeys = { PY: "VA012_ARPayment", IS: "VA012_ARInvoiceSchedule", PO: "VA012_ARPrepayOrder", CO: "VA012_ARContra" };
            var typeButtons = TYPES.map(function (t) {
                return '<button type="button" role="radio" aria-checked="false" data-type="' + t[0] + '">' + esc(msg(typeKeys[t[0]], t[1])) + '</button>';
            }).join("");
            // row 1: statement, the status chips (filter of the list), method, partner and the run button on one grid, controls
            // aligned at the bottom; row 2: the hint. There is no type choice: a run always tries every type.
            $runView.html(
                '<div class="va012-ar-runparams">'
                + '<div class="va012-ar-fld"><label for="VA012_arStmt_' + windowNo + '">' + esc(msg("VA012_StatementNo", "Statement")) + ' <sup>*</sup></label>'
                + '<select id="VA012_arStmt_' + windowNo + '" class="va012-ar-stmt"></select></div>'
                + '<div class="va012-ar-fld va012-ar-chipfld"><div class="va012-ar-chips" role="group" aria-label="' + esc(msg("VA012_ARStatus", "Status")) + '"></div></div>'
                + '<div class="va012-ar-fld"><label for="VA012_arPm_' + windowNo + '">' + esc(msg("VA012_PaymentMethod", "Payment method")) + '</label>'
                + '<select id="VA012_arPm_' + windowNo + '" class="va012-ar-pm"><option value="0">' + esc(msg("VA012_ARAllMethods", "All methods")) + '</option></select></div>'
                + '<div class="va012-ar-fld"><span class="va012-ar-lb">' + esc(msg("VA012_BusinessPartner", "Business partner")) + '</span><div class="va012-ar-bp"></div></div>'
                + '<div class="va012-ar-fld va012-ar-runfld"><button type="button" class="va012-ar-btn primary va012-ar-run">' + esc(msg("VA012_ARReconcileBtn", "Reconcile")) + '</button></div>'
                + '</div>'
                + '<div class="va012-ar-runbar"><div class="va012-ar-hint"></div></div>'
                + '<div class="va012-ar-bar" hidden></div>'
                + '<div class="va012-ar-msg" role="status" hidden></div>'
                + '<div class="va012-ar-work">'
                + '<div class="va012-ar-results">'
                + '<table><thead><tr>'
                + '<th class="va012-ar-chk"><input type="checkbox" class="va012-ar-selall" aria-label="' + esc(msg("VA012_ARSelectAll", "Select all suggested lines")) + '"></th>'
                + '<th>' + esc(msg("VA012_ARStatementLine", "Statement line")) + '</th>'
                + '<th>' + esc(msg("VA012_ARMatchedDoc", "Matched document")) + '</th>'
                + '<th>' + esc(msg("VA012_ARChequeNo", "Cheque no.")) + '</th>'
                + '<th>' + esc(msg("VA012_ARMatchedBy", "Matched by")) + '</th>'
                + '<th>' + esc(msg("VA012_ARStatus", "Status")) + '</th>'
                + '<th></th>'
                + '</tr></thead><tbody></tbody></table>'
                + '<div class="va012-ar-empty"></div>'
                + '</div>'
                // documents of the chosen type, as the form lists them, to drag onto a line without a match
                + '<aside class="va012-ar-docpanel" hidden aria-label="' + esc(msg("VA012_ARDocuments", "Documents")) + '">'
                + '<div class="va012-ar-dphead"><b>' + esc(msg("VA012_ARDocuments", "Documents")) + '</b>'
                + '<button type="button" class="va012-ar-dpclose" aria-label="' + esc(msg("VA012_ARClose", "Close")) + '" title="' + esc(msg("VA012_ARClose", "Close")) + '">×</button></div>'
                + '<div class="va012-ar-seg va012-ar-dptypes" role="radiogroup" aria-label="' + esc(msg("VA012_ARDocuments", "Documents")) + '">' + typeButtons + '</div>'
                + '<input type="search" class="va012-ar-dpsearch" placeholder="' + esc(msg("VA012_ARSearchDocs", "Search document no., partner or amount")) + '">'
                + '<div class="va012-ar-muted va012-ar-dphint"></div>'
                + '<div class="va012-ar-dplist"></div>'
                + '</aside>'
                + '</div>');
            $stmt = $runView.find(".va012-ar-stmt");
            $chips = $runView.find(".va012-ar-runparams .va012-ar-chips");
            $pm = $runView.find(".va012-ar-pm");
            $bpHolder = $runView.find(".va012-ar-bp");
            $run = $runView.find(".va012-ar-run");
            $hint = $runView.find(".va012-ar-hint");
            $bar = $runView.find(".va012-ar-bar");
            $msg = $runView.find(".va012-ar-msg");
            $results = $runView.find(".va012-ar-results");
            $body = $results.find("tbody");
            $selAll = $runView.find(".va012-ar-selall");
            $work = $runView.find(".va012-ar-work");
            $dp = $runView.find(".va012-ar-docpanel");
            $dpList = $dp.find(".va012-ar-dplist");

            var bpValidation = "C_BPartner.IsActive='Y' AND C_BPartner.IsSummary ='N' AND C_BPartner.AD_Org_ID IN(0," + VIS.Utility.Util.getValueOfInt(opts.bankOrgId)
                + ") AND C_BPartner.AD_Client_ID = " + VIS.context.getAD_Client_ID();
            var bpLookup = VIS.MLookupFactory.get(VIS.Env.getCtx(), windowNo, 3499, VIS.DisplayType.Search, "C_BPartner_ID", 0, false, bpValidation);
            bpControl = new VIS.Controls.VTextBoxButton("C_BPartner_ID", false, false, true, VIS.DisplayType.Search, bpLookup);
            $bpHolder.append(bpControl.getControl().addClass("va012-ar-bptext").attr("placeholder", msg("VA012_ARAllPartners", "All partners")))
                .append(bpControl.getBtn(0).addClass("va012-ar-bpbtn"));

            bindRunEvents();
            renderBar();                                                           // the chips show (at 0) before any statement
            loadStatements();
            updateParams();
        }

        function loadStatements() {
            VIS.dataContext.getJSONData(url + "LoadStatementNo", { _cmbBankAccount: opts.bankAccountId }, function (ds) {
                $stmt.html('<option value="0">-</option>');
                (ds || []).forEach(function (s) {
                    var id = VIS.Utility.Util.getValueOfInt(s.Value);
                    $stmt.append('<option value="' + id + '"' + (id === VIS.Utility.Util.getValueOfInt(opts.statementId) ? " selected" : "") + '>' + esc(s.Name) + '</option>');
                });
                updateParams();
                showStatement();
            });
        }

        /* the chosen statement: every line with its status, before any run; the header's settings for the detail rows */
        function showStatement() {
            var statementId = Math.max(0, VIS.Utility.Util.getValueOfInt($stmt.val()));
            st.statementId = statementId;
            st.counts = null;
            st.filter = "";
            st.statementDate = "";
            showMessage("", null);
            closeDocs();
            resetRows();
            renderBar();
            if (!statementId) return;
            busy(true);
            $.ajax({
                url: url + "GetAutoReconcileStatement",
                type: "GET",
                data: { bankAccountId: opts.bankAccountId, statementId: statementId },
                complete: function () { busy(false); },
                success: function (data) {
                    if (statementId !== st.statementId) return;                          // another statement was chosen meanwhile
                    var res = parse(data) || {};
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    st.counts = res.counts;
                    st.dateWindow = res.dateWindow || 3;
                    st.defaults = res.defaults || {};
                    st.statementDate = res.statementDate || "";
                    renderBar();
                    loadPage();
                }
            });
        }

        /* ------------------------------------------------------------------ documents to drag onto a line */
        /* the panel shows only while the No match chip is chosen: the documents of the type chosen in Reconcile with (its own
           type buttons when none is chosen), as the form's left list loads them (BankStatement/LoadPayments: same filters,
           paging and search); a document dropped on a line without a match makes the line suggested with it, the × on it
           takes it off again, and Accept reconciles it as any suggestion */
        var dp = { open: false, type: "PY", page: 0, done: true, loading: false, xhr: null, searchTimer: null };

        /* open on the No match chip, closed on every other */
        function syncDocs() {
            if (st.filter === "N") openDocs(); else closeDocs();
        }

        function openDocs() {
            if (docType) dp.type = docType;
            if (dp.open) return;
            dp.open = true;
            $dp.prop("hidden", false);
            $work.addClass("with-docs");
            $dp.find(".va012-ar-dphint").text(msg("VA012_ARDragHint", "Drag a document onto a line without a match."));
            renderDocTypes();
            loadDocs(true);
        }

        function closeDocs() {
            if (!$dp) return;
            if (dp.xhr) { dp.xhr.abort(); dp.xhr = null; }
            dp.open = false; dp.loading = false; dp.done = true;
            $dp.prop("hidden", true);
            $work.removeClass("with-docs");
            $dpList.empty();
        }

        /* no match and declined lines (both listed under No match) and lines a document was dragged onto */
        function canTakeDoc(r) { return !!r && (r.status === "N" || r.status === "D" || (r.status === "S" && r.matchCase === "DR")); }

        function renderDocTypes() {
            $dp.find(".va012-ar-dptypes button").each(function () { $(this).attr("aria-checked", $(this).data("type") === dp.type ? "true" : "false"); });
        }

        /* next page of documents; reset starts again (type, filters or search changed) */
        function loadDocs(reset) {
            if (!dp.open) return;
            if (reset) {
                if (dp.xhr) { dp.xhr.abort(); dp.xhr = null; }
                dp.page = 0; dp.done = false; dp.loading = false;
                $dpList.empty();
            }
            if (dp.loading || dp.done) return;
            dp.loading = true;
            var type = dp.type, page = dp.page + 1, bp = VIS.Utility.Util.getValueOfInt(bpControl.getValue());
            var $loading = $('<div class="va012-ar-muted va012-ar-dploading">' + esc(msg("VA012_ARLoadingDocs", "Loading documents...")) + '</div>').appendTo($dpList);
            dp.xhr = $.ajax({
                url: url + "LoadPayments",
                type: "GET",
                data: {
                    _accountID: opts.bankAccountId, _paymentPageNo: page, _PAGESIZE: PAGE_SIZE, _paymentMethodID: VIS.Utility.Util.getValueOfInt($pm.val()),
                    _transactionType: type, statementDate: st.statementDate || null, businessPartnerId: bp > 0 ? bp : null,
                    txtSearch: $.trim($dp.find(".va012-ar-dpsearch").val())
                },
                success: function (data) {
                    if (type !== dp.type) return;
                    var docs = parse(data) || [];
                    if (typeof docs === "string") docs = JSON.parse(docs);
                    dp.page = page;
                    if (docs.length < PAGE_SIZE) dp.done = true;
                    $dpList.append(docs.map(function (d) { return docCard(d, type); }).join(""));
                    if (page === 1 && !docs.length) $dpList.html('<div class="va012-ar-muted va012-ar-dploading">' + esc(msg("VA012_ARNoDocs", "No open documents for these filters.")) + '</div>');
                    makeDraggable();
                },
                error: function (x, status) { if (status !== "abort") dp.done = true; },
                complete: function (x, status) {
                    $loading.remove();
                    if (status === "abort") return;
                    dp.loading = false;
                    dp.xhr = null;
                    // the first pages may not fill the panel: keep loading until it scrolls
                    if (!dp.done && $dpList[0].scrollHeight <= $dpList[0].clientHeight + 40) loadDocs(false);
                }
            });
        }

        /* one document as the form shows it; a payment or cash line already on a statement line cannot be used (as in the form) */
        function docCard(d, type) {
            var taken = (type === "PY" || type === "CO") && VIS.Utility.Util.getValueOfInt(d.c_bankstatementline_id) > 0;
            var when = type === "IS" && d.DueDate ? d.DueDate : d.DateAcct;
            var converted = d.isconverted === "Y";
            return '<div class="va012-ar-doccard' + (taken ? " taken" : "") + '" data-kind="' + type + '" data-doc="' + VIS.Utility.Util.getValueOfInt(d.c_payment_id) + '">'
                + '<div class="va012-ar-dcrow"><b>' + esc(d.paymentno) + '</b><span class="va012-ar-amt">' + esc(d.currency) + ' ' + amt(d.paymentamount) + '</span></div>'
                + '<div class="va012-ar-dcrow"><span>' + esc(d.businesspartner) + '</span>'
                + (converted ? '<span class="va012-ar-muted">' + esc(d.basecurrency) + ' ' + amt(d.convertedamount) + '</span>' : '') + '</div>'
                + '<div class="va012-ar-muted">' + esc(when ? new Date(when).toLocaleDateString() : "") + (d.PaymentMethod ? ' · ' + esc(d.PaymentMethod) : "")
                + (d.paymenttype ? ' · ' + esc(d.paymenttype) : "") + '</div>'
                + (taken ? '<div class="va012-ar-muted">' + esc(msg("VA012_ARDocOnLine", "Already on a statement line")) + '</div>' : '')
                + '</div>';
        }

        /* the form's drag and drop (jQuery UI): documents from the panel onto the lines */
        function makeDraggable() {
            $dpList.find(".va012-ar-doccard").not(".taken").not(".ui-draggable").draggable({
                helper: function () { return $(this).clone().css({ width: $(this).outerWidth() }).addClass("va012-ar-dragging"); },
                appendTo: "body",
                zIndex: 100000,
                cursor: "move",
                cursorAt: { left: 5 },
                revert: "invalid"
            });
        }
        function makeDroppable($el, r) {
            if (!canTakeDoc(r)) return;
            $el.droppable({
                accept: ".va012-ar-doccard",
                hoverClass: "va012-ar-dropping",
                tolerance: "pointer",
                drop: function (e, ui) { assignDoc(r.id, ui.draggable.data("kind"), ui.draggable.data("doc")); }
            });
        }

        function assignDoc(id, kind, docId) {
            var r = st.rows[id];
            if (!canTakeDoc(r) || !kind || !docId) return;
            busy(true);
            $.ajax({
                url: url + "AssignAutoReconcileDocument",
                type: "POST",
                data: { suggestionId: id, kind: kind, docId: docId },
                complete: function () { busy(false); },
                success: function (data) {
                    var res = parse(data) || {};
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    if (!res.row) return;
                    delete st.choices[id];
                    disposeCharge(id);
                    st.rows[id] = res.row;
                    st.expanded[id] = true;
                    renderRow(res.row);
                    // the document is held by this line now
                    $dpList.find('.va012-ar-doccard[data-kind="' + kind + '"][data-doc="' + docId + '"]').remove();
                    showMessage("ok", '<strong>' + esc(msg("VA012_ARDocMatched", "Document matched to line")) + ' ' + res.row.lineNo + '.</strong> '
                        + esc(msg("VA012_ARCheckAcceptOrOff", "Check the details and select Accept to reconcile it, or × to take the document off.")));
                    refreshCounts();
                }
            });
        }

        /* the × on a dragged document: it leaves the line (back to No match when none is left) and is free for the panel again */
        function unassignDoc(id, docRowId) {
            var r = st.rows[id];
            if (!r || r.status !== "S" || r.matchCase !== "DR" || !docRowId) return;
            busy(true);
            $.ajax({
                url: url + "UnassignAutoReconcileDocument",
                type: "POST",
                data: { suggestionId: id, docRowId: docRowId },
                complete: function () { busy(false); },
                success: function (data) {
                    var res = parse(data) || {};
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    if (!res.row) return;
                    delete st.choices[id];
                    delete st.selected[id];
                    disposeCharge(id);
                    st.rows[id] = res.row;
                    renderRow(res.row);
                    renderSelection();
                    showMessage("ok", esc(msg("VA012_ARDocRemovedFrom", "Document taken off line")) + ' ' + res.row.lineNo + '.');
                    refreshCounts();
                    if (dp.open) loadDocs(true);
                }
            });
        }

        function updateParams() {
            $run.prop("disabled", VIS.Utility.Util.getValueOfInt($stmt.val()) <= 0);
            updateRunHint();
        }

        function updateRunHint() {
            if (!$hint) return;
            // no type chosen: every type is tried, payments first, and a line matched by one type is not tried by the next
            var rules = activeRules().filter(function (r) { return !docType || r.appliesTo.indexOf(docType) > -1; });
            var auto = !setup || !setup.header || !setup.header.active || setup.header.autoMatch;
            var text = rules.length
                ? rules.length + " " + msg("VA012_ARRulesApply", "rule(s) apply") + ": " + rules.map(function (r) { return r.name; }).join(", ") + ". " + msg("VA012_ARRestStandard", "Anything they do not cover uses standard matching.")
                : msg("VA012_ARStandardOnly", "No rules configured. Standard matching is used.");
            text += " " + (auto ? msg("VA012_ARAutoOn", "Lines that qualify are reconciled automatically.") : msg("VA012_ARAutoOff", "Auto Reconciliation is off: every match is suggested."));
            if (!docType) text = msg("VA012_ARAllTypesAlways", "Every document type is tried: payments first, then invoice schedules, prepay orders and contra.") + " " + text;
            $hint.text(text);
        }

        function bindRunEvents() {
            $dp.on("click", ".va012-ar-dptypes button", function () {
                var t = $(this).data("type");
                if (t === dp.type) return;
                dp.type = t;
                renderDocTypes();
                loadDocs(true);
            });
            $dp.on("click", ".va012-ar-dpclose", closeDocs);
            $dp.on("input", ".va012-ar-dpsearch", function () {
                clearTimeout(dp.searchTimer);
                dp.searchTimer = setTimeout(function () { loadDocs(true); }, 400);
            });
            $dpList.on("scroll", function () { if (this.scrollTop + this.clientHeight >= this.scrollHeight - 40) loadDocs(false); });
            $body.on("click", "button[data-undoc]", function (e) { e.stopPropagation(); unassignDoc($(this).data("undoc"), $(this).data("docrow")); });
            // the page's payment method and partner filter the documents too, as on the form
            $pm.on("change", function () { if (dp.open) loadDocs(true); });
            bpControl.fireValueChanged = function () { if (dp.open) loadDocs(true); };
            $stmt.on("change", function () { updateParams(); showStatement(); });
            $run.on("click", run);
            $results.on("scroll", function () {
                if (this.scrollTop + this.clientHeight >= this.scrollHeight - 60) loadPage();
            });
            $chips.on("click", "[data-filter]", function () {
                st.filter = $(this).data("filter");
                resetRows();
                renderBar();
                syncDocs();                                                        // the documents panel belongs to No match
                loadPage();
            });
            $bar.on("click", ".va012-ar-declinesel", function () { decline(Object.keys(st.selected)); });
            $bar.on("click", ".va012-ar-acceptsel", function () { accept(Object.keys(st.selected)); });
            $bar.on("click", ".va012-ar-paytoggle", function () { st.payOpen = !st.payOpen; renderBar(); });
            $bar.on("click", ".va012-ar-payapply", function () {
                applyPayment(payTargets(), VIS.Utility.Util.getValueOfInt($bar.find(".va012-ar-paypm").val()), VIS.Utility.Util.getValueOfInt($bar.find(".va012-ar-payct").val()));
            });
            $selAll.on("change", function () {
                var on = this.checked;
                st.order.forEach(function (id) { if (st.rows[id].status === "S") { if (on) st.selected[id] = true; else delete st.selected[id]; } });
                $body.find("input[data-sel]").prop("checked", on);
                renderSelection();
            });
            $body.on("change", "input[data-sel]", function () {
                var id = $(this).data("sel");
                if (this.checked) st.selected[id] = true; else delete st.selected[id];
                renderSelection();
            });
            $body.on("click", "button[data-decline]", function (e) { e.stopPropagation(); decline([$(this).data("decline")]); });
            $body.on("click", "button[data-accept]", function (e) { e.stopPropagation(); accept([$(this).data("accept")]); });
            // detail row choices: another candidate, the difference type, the tax rate (the charge lookup reports through fireValueChanged)
            $body.on("change", "input[data-choose]", function () {
                var id = $(this).data("choose");
                choice(id).docRowId = VIS.Utility.Util.getValueOfInt($(this).val());
                renderRow(st.rows[id]);
            });
            $body.on("change", "select[data-difftype]", function () { var id = $(this).data("difftype"); choice(id).diffType = $(this).val(); refreshAccept(st.rows[id]); });
            $body.on("change", "select[data-tax]", function () {
                var id = $(this).data("tax");
                choice(id).taxId = VIS.Utility.Util.getValueOfInt($(this).val());
                refreshAccept(st.rows[id]);
                updateSplit(st.rows[id]);
            });
            // payment details in the detail row: saved at once, like Set method / type for one line
            $body.on("change", "select[data-pm]", function () { applyPayment([$(this).data("pm")], VIS.Utility.Util.getValueOfInt($(this).val()), 0); });
            $body.on("change", "select[data-ct]", function () { applyPayment([$(this).data("ct")], 0, VIS.Utility.Util.getValueOfInt($(this).val())); });
            $body.on("change", "input[data-chq]", function () { applyPayment([$(this).data("chq")], 0, 0, $.trim($(this).val())); });
            $body.on("change", "input[data-chqdt]", function () { choice($(this).data("chqdt")).chequeDate = $(this).val(); });
            $body.on("click keydown", "tr.va012-ar-row", function (e) {
                if (e.type === "keydown" && e.key !== "Enter" && e.key !== " ") return;
                if ($(e.target).closest("input,button,a").length) return;
                e.preventDefault();
                var id = $(this).data("id");
                st.expanded[id] = !st.expanded[id];
                renderRow(st.rows[id]);
            });
        }

        function showMessage(kind, html) {
            if (!html) { $msg.attr("hidden", true).html(""); return; }
            $msg.removeAttr("hidden").attr("class", "va012-ar-msg " + kind).html(html);
        }

        function run() {
            var statementId = VIS.Utility.Util.getValueOfInt($stmt.val());
            if (statementId <= 0) return;
            if (xhr) xhr.abort();
            $run.prop("disabled", true).text(msg("VA012_ARRunning", "Running..."));
            showMessage("", null);
            busy(true);
            serverWork++;
            $.ajax({
                url: url + "RunAutoReconcile",
                type: "POST",
                data: {
                    bankAccountId: opts.bankAccountId, statementId: statementId, docType: docType || "AL",
                    payMethodId: VIS.Utility.Util.getValueOfInt($pm.val()), bpId: VIS.Utility.Util.getValueOfInt(bpControl.getValue())
                },
                success: function (data) {
                    var res = parse(data) || {};
                    // the page was closed while the run went on: only the form's lists are refreshed
                    if (!$root) { if (res.reconciled) formChanged(); return; }
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    // the list shows the whole statement again, with this run's results in it
                    st.statementId = statementId;
                    st.counts = res.counts;
                    st.dateWindow = res.dateWindow || 3;
                    st.defaults = res.defaults || {};
                    st.filter = "";
                    resetRows();
                    renderBar();
                    syncDocs();
                    if (!res.lineCount) showMessage("", esc(msg("VA012_ARNoLines", "No open lines on this statement for these filters.")));
                    else {
                        var parts = [];
                        parts.push('<strong>' + esc(msg("VA012_ARReconciledCount", "Reconciled automatically")) + ': ' + (res.reconciled || 0) + '</strong>');
                        if (res.notReconciled) parts.push(esc(msg("VA012_ARNotReconciledCount", "Could not be reconciled automatically, see the reason on the line")) + ': ' + res.notReconciled);
                        showMessage(res.reconciled ? "ok" : "", parts.join(" · "));
                        if (res.reconciled) formChanged();
                    }
                    loadPage();
                },
                error: function (x, status) { if ($root && status !== "abort") showMessage("bad", esc(msg("VA012_ErrorSaving", "Something went wrong."))); },
                complete: function () {
                    serverWork--;
                    if (!$root) return;
                    busy(false); $run.text(msg("VA012_ARReconcileBtn", "Reconcile")); updateParams();
                }
            });
        }

        function resetRows() {
            if (xhr) { xhr.abort(); xhr = null; }
            st.page = 0; st.done = false; st.loading = false;
            st.rows = {}; st.order = []; st.expanded = {}; st.selected = {}; st.choices = {};
            Object.keys(st.chargeCtrls).forEach(disposeCharge);
            $body.empty();
            $selAll.prop("checked", false);
            showEmpty(st.statementId ? "" : msg("VA012_ARChooseStatement", "Choose a statement to see its lines."));
            renderSelection();
        }

        function showEmpty(text) {
            $results.find(".va012-ar-empty").text(text || "").toggle(!!text);
        }

        /* next page of the current filter; called for a statement, after a run, on a filter change and when the list is scrolled to the bottom */
        function loadPage() {
            if (!st.statementId || st.loading || st.done) return;
            st.loading = true;
            var firstPage = st.page === 0;
            var $loading = $('<tr class="va012-ar-loading"><td colspan="7">' + esc(msg("VA012_ARLoadingMore", "Loading more lines...")) + '</td></tr>');
            if (firstPage) busy(true); else $body.append($loading);       // the first page blocks, later pages load under the list
            var statementId = st.statementId, filter = st.filter;
            xhr = $.ajax({
                url: url + "GetAutoReconcilePage",
                type: "GET",
                data: { statementId: statementId, filter: filter, pageNo: st.page + 1 },
                success: function (data) {
                    if (statementId !== st.statementId || filter !== st.filter) return;
                    var rows = parse(data) || [];
                    st.page++;
                    if (rows.length < PAGE_SIZE) st.done = true;
                    if (firstPage && !rows.length) showEmpty(msg("VA012_ARNoLinesHere", "No lines here."));
                    rows.forEach(function (r) {
                        if (st.rows[r.id]) return;
                        st.rows[r.id] = r;
                        st.order.push(r.id);
                        renderRow(r);
                    });
                    renderSelection();
                },
                complete: function () {
                    if (firstPage) busy(false);
                    $loading.remove();
                    st.loading = false;
                    xhr = null;
                    // the first pages may not fill the area: keep loading until it scrolls or the run is complete
                    if (!st.done && $results[0].scrollHeight <= $results[0].clientHeight + 60) loadPage();
                }
            });
        }

        function refreshCounts() {
            var statementId = st.statementId;
            VIS.dataContext.getJSONData(url + "GetAutoReconcileCounts", { statementId: statementId }, function (c) {
                if (statementId !== st.statementId) return;
                st.counts = c;
                renderBar();
            });
        }

        function renderBar() {
            // the chips sit in the run fields row, where the type choice was, and always show: with no statement (or its
            // counts still loading) they show 0 and are disabled; the bar keeps the bulk actions
            var ready = !!(st.statementId && st.counts), counts = st.counts || {};
            var chips = FILTERS.filter(function (f) { return f[0] !== "O" || counts.open > 0 || st.filter === "O"; }).map(function (f) {
                return '<button type="button" class="va012-ar-chip' + (st.filter === f[0] ? " on" : "") + '" data-filter="' + f[0] + '"' + (ready ? '' : ' disabled') + '>'
                    + esc(f[1]) + ' <b>' + (counts[f[2]] || 0) + '</b></button>';
            }).join("");
            $chips.html(chips);
            if (!ready) { $bar.attr("hidden", true); return; }
            $bar.removeAttr("hidden").html('<div class="va012-ar-bulk"><span class="va012-ar-selcount" aria-live="polite"></span>'
                + '<button type="button" class="va012-ar-btn primary va012-ar-acceptsel" disabled>' + esc(msg("VA012_ARAcceptSelected", "Accept selected")) + '</button>'
                + '<button type="button" class="va012-ar-btn va012-ar-declinesel" disabled>' + esc(msg("VA012_ARDeclineSelected", "Decline selected")) + '</button>'
                + '<button type="button" class="va012-ar-btn va012-ar-paytoggle" aria-expanded="' + st.payOpen + '" title="' + esc(fixedPaymentNote()) + '">'
                + esc(msg("VA012_ARSetMethodType", "Set method / type")) + ' ▾</button></div>'
                + (st.payOpen ? payPanel() : ''));
            renderSelection();
        }

        function renderSelection() {
            var n = Object.keys(st.selected).length;
            $bar.find(".va012-ar-selcount").text(n + " " + msg("VA012_ARSelected", "selected"));
            $bar.find(".va012-ar-declinesel, .va012-ar-acceptsel").prop("disabled", n === 0);
            $bar.find(".va012-ar-paytarget").text(payTargetText());
        }

        /* ------------------------------------------------------------------ Set method / type */
        /* open only where Accept creates a new payment: invoice schedules and prepay orders */
        /* the line's document type: the type of its document (a run over all types, or a document dragged onto the line,
           can differ from the run's), else its run's */
        function kindOf(r) {
            var d = effective(r).chosen[0] || (r.docs || [])[0];
            if (d && d.kind) return d.kind;
            return r.docType && r.docType !== "AL" ? r.docType : "";
        }
        /* new payment details are open for invoice schedules and prepay orders only; the list mixes lines of every type */
        function paymentOpen(r) {
            var k = kindOf(r);
            return k === "IS" || k === "PO";
        }
        function fixedPaymentNote() {
            return msg("VA012_ARSetOnlyNewPayments", "Only for invoice schedule and prepay order lines: a payment or contra line keeps its own method and conversion type.");
        }
        function nameOf(list, id) { var o = (list || []).filter(function (x) { return String(x.id) === String(id); })[0]; return o ? o.name : ""; }

        /* the selected suggested lines, or else every suggested line shown; only lines whose Accept creates a payment */
        function payTargets() {
            function open(id) { var r = st.rows[id]; return r && r.status === "S" && paymentOpen(r); }
            var selected = Object.keys(st.selected).map(function (id) { return VIS.Utility.Util.getValueOfInt(id); });
            return selected.length ? selected.filter(open) : st.order.filter(open);
        }
        function payTargetText() {
            var n = payTargets().length, selected = Object.keys(st.selected).length > 0;
            return (selected ? msg("VA012_ARSetOnSelected", "Set on the selected lines") : msg("VA012_ARSetOnShown", "Set on all suggested lines shown")) + " (" + n + "):";
        }

        function payPanel() {
            return '<div class="va012-ar-paypanel"><span class="va012-ar-paytarget">' + esc(payTargetText()) + '</span>'
                + '<select class="va012-ar-paypm" aria-label="' + esc(msg("VA012_PaymentMethod", "Payment method")) + '">' + options(payMethods, "", msg("VA012_ARPaymentMethodDots", "Payment method...")) + '</select>'
                + '<select class="va012-ar-payct" aria-label="' + esc(msg("VA012_ARConversionType", "Conversion type")) + '">' + options(setup ? setup.conversionTypes : [], "", msg("VA012_ARConversionTypeDots", "Conversion type...")) + '</select>'
                + '<button type="button" class="va012-ar-btn va012-ar-payapply">' + esc(msg("VA012_ARApply", "Apply")) + '</button>'
                + '<span class="va012-ar-muted">' + esc(msg("VA012_AROnlyOpenFields", "Only for the payment Accept creates; the conversion type only where the document is in another currency.")) + '</span></div>';
        }

        /* saves the payment method and/or conversion type (0 = unchanged) and a typed cheque number (undefined = unchanged)
           on suggested lines; the conversion type only where the document is foreign */
        function applyPayment(ids, payMethodId, conversionTypeId, chequeNo) {
            var hasCheque = typeof chequeNo === "string";
            if (!payMethodId && !conversionTypeId && !hasCheque) return;
            ids = ids.filter(function (id) { return st.rows[id] && paymentOpen(st.rows[id]); });
            if (!ids.length) { showMessage("", esc(fixedPaymentNote())); return; }
            var items = ids.map(function (id) {
                var r = st.rows[id], foreign = r && effective(r).chosen.some(function (d) { return d.foreign; });
                var item = { id: id, payMethodId: payMethodId, conversionTypeId: foreign ? conversionTypeId : 0 };
                if (hasCheque) item.chequeNo = chequeNo;
                return item;
            }).filter(function (i) { return st.rows[i.id] && st.rows[i.id].status === "S" && (i.payMethodId || i.conversionTypeId || hasCheque); });
            if (!items.length) { showMessage("", esc(msg("VA012_ARNothingToSet", "None of these lines takes a conversion type: their documents are in the bank's currency."))); return; }
            busy(true);
            $.ajax({
                url: url + "SetAutoReconcilePayment", type: "POST", data: { items: JSON.stringify(items) },
                complete: function () { busy(false); },
                success: function (data) {
                    var res = parse(data) || {};
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    items.forEach(function (i) {
                        var r = st.rows[i.id];
                        if (i.payMethodId) r.payMethodId = i.payMethodId;
                        if (i.conversionTypeId) r.conversionTypeId = i.conversionTypeId;
                        if (hasCheque) r.enteredCheque = i.chequeNo;
                        renderRow(r);
                    });
                    if (ids.length > 1) showMessage("ok", '<strong>' + esc(msg("VA012_ARPaymentSet", "Payment details set on")) + ' ' + (res.updated || 0) + ' '
                        + esc(msg("VA012_ARLines", "lines")) + '.</strong>');
                }
            });
        }

        /* ------------------------------------------------------------------ cheque number */
        function isChequeMethod(id) { var m = payMethods.filter(function (x) { return x.id === VIS.Utility.Util.getValueOfInt(id); })[0]; return !!m && m.base === "S"; }

        /* the cheque book's next number for a method (shown only; assigned when the payment is created) */
        function loadBook(methodId, then) {
            if (st.bookNext.hasOwnProperty(methodId)) return;
            st.bookNext[methodId] = null;                                            // loading
            $.ajax({
                url: url + "GetAutoReconcileChequeBook", type: "GET", data: { bankAccountId: opts.bankAccountId, payMethodId: methodId },
                success: function (data) { st.bookNext[methodId] = (parse(data) || {}).next || ""; if (then) then(); },
                error: function () { delete st.bookNext[methodId]; }
            });
        }

        /* where the new payment's cheque number comes from: stmt, entered, book, missing, loading; "" when no cheque method */
        function newChequeKind(r) {
            var d = effective(r).chosen[0];
            var methodId = r.payMethodId || (d ? d.payMethodId : 0);
            if (!isChequeMethod(methodId)) return "";
            if (r.chequeNo) return "stmt";
            if (r.enteredCheque) return "entered";
            var book = st.bookNext[methodId];
            if (book === undefined || book === null) return "loading";
            return book ? "book" : "missing";
        }

        function chequeField(r) {
            var d = effective(r).chosen[0], methodId = r.payMethodId || (d ? d.payMethodId : 0);
            var kind = newChequeKind(r);
            if (!kind) return "";
            if (kind === "loading") loadBook(methodId, function () { if (st.rows[r.id] && st.expanded[r.id]) renderRow(st.rows[r.id]); });
            var src = {
                stmt: msg("VA012_ARChqFromStatement", "from the statement"),
                entered: msg("VA012_ARChqEntered", "entered"),
                book: msg("VA012_ARChqFromBook", "cheque book, assigned on Accept"),
                missing: msg("VA012_ARChqEnter", "no cheque book: enter the number"),
                loading: "..."
            }[kind];
            var value = kind === "stmt" ? r.chequeNo : kind === "entered" ? r.enteredCheque : "";
            var placeholder = kind === "book" ? st.bookNext[methodId] : msg("VA012_AREnterNumber", "Enter number");
            return '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARChequeNo", "Cheque no.")) + (kind === "missing" ? ' <sup>*</sup>' : '')
                + ' <span class="va012-ar-muted">(' + esc(src) + ')</span></label>'
                + '<input type="text" class="va012-ar-chqno" maxlength="30" data-chq="' + r.id + '" value="' + esc(value || "") + '" placeholder="' + esc(placeholder || "") + '"'
                + (kind === "stmt" ? " disabled" : "") + '></div>'
                + chequeDateField(r, r.date);
        }

        /* the cheque date the line is reconciled with, as on the form: prefilled, can be changed; sent with Accept */
        function chequeDateField(r, fallback) {
            var value = (st.choices[r.id] || {}).chequeDate || fallback || "";
            return '<div class="va012-ar-fld"><label for="VA012_arChqDt_' + r.id + '">' + esc(msg("VA012_ARChequeDate", "Cheque date")) + ' <sup>*</sup></label>'
                + '<input type="date" class="va012-ar-chqdt" id="VA012_arChqDt_' + r.id + '" data-chqdt="' + r.id + '" value="' + esc(value) + '"></div>';
        }

        function chequeNote(r) {
            var kind = newChequeKind(r);
            if (kind === "stmt") return '<div class="va012-ar-muted">' + esc(msg("VA012_ARChqStmtNote", "The new payment takes the statement's cheque number, so the cheque book is not used.")) + '</div>';
            if (kind === "book") return '<div class="va012-ar-muted">' + esc(msg("VA012_ARChqBookNote", "Shown for information. The number is assigned on Accept, one line at a time in statement order. Type a number to use another.")) + '</div>';
            return "";
        }

        /* the payment details of a suggested line: open for a new payment, read-only for an existing payment */
        function payBox(r) {
            if (r.status !== "S" || kindOf(r) === "CO") return "";
            var d = effective(r).chosen[0];
            if (!d) return "";
            var methodId = r.payMethodId || d.payMethodId, typeId = r.conversionTypeId || d.conversionTypeId;
            var foreign = effective(r).chosen.some(function (x) { return x.foreign; });
            if (!paymentOpen(r)) {
                var chq = r.chequeNo || d.chequeNo
                    ? ' · ' + esc(msg("VA012_ARChequeNo", "Cheque no.")) + ': ' + esc(r.chequeNo || "-") + ' ' + esc(msg("VA012_ARStmt", "stmt")) + ' / ' + esc(d.chequeNo || "-") + ' ' + esc(msg("VA012_ARDoc", "doc"))
                    : '';
                // a cheque payment: the line needs its cheque date, the payment's by default
                var isCheque = !!d.chequeNo || isChequeMethod(d.payMethodId);
                return '<div class="va012-ar-box"><div><b>' + esc(msg("VA012_ARPaymentDetails", "Payment details")) + '</b> <span class="va012-ar-muted">'
                    + esc(msg("VA012_ARPaymentLocked", "The line keeps the payment's method and conversion type.")) + '</span></div>'
                    + '<div>' + esc(msg("VA012_PaymentMethod", "Payment method")) + ': ' + esc(nameOf(payMethods, d.payMethodId) || "-")
                    + (foreign ? ' · ' + esc(msg("VA012_ARConversionType", "Conversion type")) + ': ' + esc(nameOf(setup ? setup.conversionTypes : [], d.conversionTypeId) || "-") : '')
                    + chq + '</div>'
                    + (isCheque ? '<div class="va012-ar-sel">' + chequeDateField(r, d.chequeDate || r.date) + '</div>' : '') + '</div>';
            }
            return '<div class="va012-ar-box"><div><b>' + esc(msg("VA012_ARPaymentDetails", "Payment details")) + '</b> <span class="va012-ar-muted">'
                + esc(msg("VA012_ARNewPaymentWith", "The new payment is created with these.")) + '</span></div>'
                + '<div class="va012-ar-sel"><div class="va012-ar-fld"><label>' + esc(msg("VA012_PaymentMethod", "Payment method")) + '</label>'
                + '<select data-pm="' + r.id + '">' + options(payMethods, methodId || "", msg("VA012_ARChoose", "Choose...")) + '</select></div>'
                + chequeField(r)
                + (foreign
                    ? '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARConversionType", "Conversion type")) + ' (' + esc(d.currency) + ')</label>'
                        + '<select data-ct="' + r.id + '">' + options(setup ? setup.conversionTypes : [], typeId || "", msg("VA012_ARChoose", "Choose...")) + '</select></div>'
                    : '<div class="va012-ar-fld"><span class="va012-ar-muted">' + esc((d.currency || "") + " " + msg("VA012_ARSameCurrency", "same as the bank: no conversion.")) + '</span></div>')
                + '</div>' + chequeNote(r) + '</div>';
        }

        /* ------------------------------------------------------------------ Accept */
        function choice(id) { st.choices[id] = st.choices[id] || {}; return st.choices[id]; }

        /* the document(s) Accept will post: the candidate the user picked, else the suggested (reserved) ones */
        function effective(r) {
            var c = st.choices[r.id];
            if (r.status === "S" && c && c.docRowId) {
                var pick = r.docs.filter(function (d) { return d.docRowId === c.docRowId; });
                if (pick.length) return { chosen: pick, others: r.docs.filter(function (d) { return d.docRowId !== c.docRowId; }) };
            }
            return split(r);
        }

        function rowDiff(r) {
            var total = effective(r).chosen.reduce(function (s, d) { return s + Number(d.amount || 0); }, 0);
            return Math.round((Number(r.amount) - total) * 100) / 100;
        }

        /* the statement amount is larger than the document(s): the rest goes to a charge (also for invoice schedules, as the form) */
        function statementMore(r) {
            var total = effective(r).chosen.reduce(function (s, d) { return s + Number(d.amount || 0); }, 0);
            return Math.abs(Number(r.amount)) > Math.abs(total);
        }

        /* invoice schedules ask for a difference type only when the invoices are more than the statement amount */
        function needsDiffType(r) { return kindOf(r) === "IS" && !statementMore(r); }

        function chargeOf(id) {
            var c = st.choices[id] || {}, ctrl = st.chargeCtrls[id];
            return c.chargeId || (ctrl ? VIS.Utility.Util.getValueOfInt(ctrl.getValue()) : 0) || st.defaults.chargeId || 0;
        }

        /* what a suggested line still needs before Accept, or null when it is complete */
        function needs(r) {
            if (r.status !== "S") return null;
            if (!effective(r).chosen.length) return msg("VA012_ARChooseDocument", "Choose a document first.");
            if (paymentOpen(r) && !(r.payMethodId || effective(r).chosen[0].payMethodId)) return msg("VA012_ARNeedsPayMethod", "Needs payment method");
            if (paymentOpen(r) && newChequeKind(r) === "missing") return msg("VA012_ARNeedsChequeNo", "Needs cheque number");
            if (rowDiff(r) === 0) return null;
            var c = st.choices[r.id] || {};
            if (kindOf(r) === "CO") return msg("VA012_ARContraEqual", "Contra amounts must be equal");
            if (needsDiffType(r)) return (c.diffType || r.differenceType) ? null : msg("VA012_ARNeedsDiffType", "Needs difference type");
            return chargeOf(r.id) && (c.taxId || st.defaults.taxId) ? null : msg("VA012_ARNeedsChargeTax", "Needs charge and tax");
        }

        /* the row's Accept button follows the choices without redrawing the row (keeps the charge lookup) */
        function refreshAccept(r) {
            var need = needs(r);
            $body.find('button[data-accept="' + r.id + '"]').prop("disabled", !!need).attr("title", need || "");
        }

        function disposeCharge(id) {
            var ctrl = st.chargeCtrls[id];
            if (ctrl && ctrl.disposeComponent) ctrl.disposeComponent();
            delete st.chargeCtrls[id];
        }

        function chooser(r) {
            if (r.status !== "S" || r.docs.length < 2 || split(r).chosen.length > 1) return "";     // nothing to choose, or several invoices together
            var current = effective(r).chosen.length === 1 ? effective(r).chosen[0].docRowId : 0;
            return '<div class="va012-ar-box"><div><b>' + esc(msg("VA012_ARChooseTheDoc", "Document to reconcile")) + '</b> <span class="va012-ar-muted">'
                + esc(msg("VA012_ARChooseNote", "Pick another candidate if the suggestion is not the right one.")) + '</span></div>'
                + r.docs.map(function (d) {
                    return '<label class="va012-ar-opt va012-ar-pick"><input type="radio" name="va012-ar-ch-' + windowNo + '-' + r.id + '" data-choose="' + r.id + '" value="' + d.docRowId + '"'
                        + (d.docRowId === current ? " checked" : "") + '> <b>' + esc(d.docNo) + '</b> ' + esc(d.bpName) + ' · ' + esc(day(d.date)) + ' · ' + amt(d.amount) + '</label>';
                }).join("") + '</div>';
        }

        function diffBox(r) {
            if (r.status !== "S" || !effective(r).chosen.length) return "";
            var diff = rowDiff(r);
            if (diff === 0) return "";
            var c = st.choices[r.id] || {};
            var head = '<div><b>' + (statementMore(r)
                ? esc(msg("VA012_ARStatementMore", "The statement amount is more than the document by")) + ' ' + amt(Math.abs(diff)) + '.'
                : esc(msg("VA012_ARDocumentMore", "The document is more than the statement amount by")) + ' ' + amt(Math.abs(diff)) + '.') + '</b></div>';
            if (kindOf(r) === "CO")
                return '<div class="va012-ar-box warn">' + head + '<div class="va012-ar-muted">' + esc(msg("VA012_ARContraManual", "Contra amounts must be equal. Match this line in the form.")) + '</div></div>';
            if (needsDiffType(r))
                return '<div class="va012-ar-box warn">' + head + '<div class="va012-ar-sel"><div class="va012-ar-fld"><label>' + esc(msg("VA012_ARDifferenceType", "Difference type")) + ' <sup>*</sup></label>'
                    + '<select data-difftype="' + r.id + '">' + options(setup ? setup.differenceTypes : [], c.diffType || r.differenceType || "", msg("VA012_ARChoose", "Choose...")) + '</select></div></div></div>';
            // payment, prepay order, and an invoice schedule paid with more than the invoices: charge and tax rate
            return '<div class="va012-ar-box warn">' + head + '<div class="va012-ar-muted">' + esc(msg("VA012_ARChargeNote", "The difference is booked to the charge; the tax is taken out of it.")) + '</div>'
                + '<div class="va012-ar-sel"><div class="va012-ar-fld"><label>' + esc(msg("VA012_ARCharge", "Charge")) + ' <sup>*</sup></label><div class="va012-ar-chg" data-chg="' + r.id + '"></div></div>'
                + '<div class="va012-ar-fld"><label>' + esc(msg("VA012_ARTaxRate", "Tax rate")) + ' <sup>*</sup></label><select data-tax="' + r.id + '">'
                + options(setup ? setup.taxes : [], c.taxId || st.defaults.taxId || "", msg("VA012_ARChoose", "Choose...")) + '</select></div></div>'
                + '<div class="va012-ar-taxsplit" data-split="' + r.id + '"></div></div>';
        }

        /* the charge and tax split of the difference, from the form's own tax calculation (CalculateSurcharge, tax included) */
        function updateSplit(r) {
            if (!r) return;
            var $el = $body.find('[data-split="' + r.id + '"]');
            if (!$el.length) return;
            var c = st.choices[r.id] || {}, taxId = c.taxId || st.defaults.taxId, diff = rowDiff(r);
            if (!taxId) { $el.html('<div class="va012-ar-muted">' + esc(msg("VA012_ARChooseTaxForSplit", "Choose the tax rate to see the charge and tax amounts.")) + '</div>'); return; }
            var key = taxId + "|" + Math.abs(diff);
            if (st.taxCalc[key]) { $el.html(splitHtml(r, st.taxCalc[key], taxId)); return; }
            $el.html('<div class="va012-ar-muted">' + esc(msg("VA012_ARCalculatingTax", "Calculating the tax...")) + '</div>');
            $.ajax({
                url: url + "CalculateSurcharge", type: "GET",
                data: { _tax_ID: taxId, _chargeAmt: Math.abs(diff), _stdPrecision: VIS.Utility.Util.getValueOfInt(opts.precision) || 2 },
                success: function (data) {
                    var res = parse(data) || {};
                    st.taxCalc[key] = { tax: Math.abs(Number(res.TaxAmt || 0)), surcharge: Math.abs(Number(res.SurchargeAmt || 0)) };
                    updateSplit(st.rows[r.id]);
                },
                error: function () { $el.html('<div class="va012-ar-muted">' + esc(msg("VA012_ARTaxNotCalculated", "The tax could not be calculated.")) + '</div>'); }
            });
        }

        function splitHtml(r, calc, taxId) {
            var diff = rowDiff(r), sign = diff < 0 ? -1 : 1, abs = Math.abs(diff);
            var net = Math.round((abs - calc.tax - calc.surcharge) * 100) / 100;
            var total = effective(r).chosen.reduce(function (s, d) { return s + Number(d.amount || 0); }, 0);
            var ctrl = st.chargeCtrls[r.id], chargeName = ctrl ? $.trim(ctrl.getControl().val()) : "";
            var kind = kindOf(r);
            var docLabel = kind === "IS" ? msg("VA012_ARInvoices", "Invoices") : kind === "PO" ? msg("VA012_ARPrepayOrder", "Prepay order") : msg("VA012_ARPayment", "Payment");
            var rows = [
                [docLabel, total],
                [(chargeName || msg("VA012_ARCharge", "Charge")) + " " + msg("VA012_ARNetOfTax", "(net of tax)"), sign * net],
                [msg("VA012_ARTax", "Tax") + ", " + nameOf(setup ? setup.taxes : [], taxId), sign * calc.tax]
            ];
            if (calc.surcharge) rows.push([msg("VA012_ARSurcharge", "Surcharge"), sign * calc.surcharge]);
            return '<table class="va012-ar-brk"><tbody>'
                + rows.map(function (x) { return '<tr><td>' + esc(x[0]) + '</td><td class="va012-ar-v">' + amt(x[1]) + '</td></tr>'; }).join("")
                + '<tr class="va012-ar-total"><td>' + esc(msg("VA012_ARStatementAmount", "Statement amount")) + '</td><td class="va012-ar-v">' + amt(r.amount) + '</td></tr>'
                + '</tbody></table>';
        }

        /* the form's charge lookup inside an open detail row */
        function mountCharge(r, $detailRow) {
            var $holder = $detailRow.find('[data-chg="' + r.id + '"]');
            if (!$holder.length) return;
            var lookup = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), windowNo, 3787, VIS.DisplayType.Search);
            var ctrl = new VIS.Controls.VTextBoxButton("C_Charge_ID", false, false, true, VIS.DisplayType.Search, lookup);
            $holder.append(ctrl.getControl().addClass("va012-ar-bptext")).append(ctrl.getBtn(0).addClass("va012-ar-bpbtn"));
            var current = (st.choices[r.id] || {}).chargeId || st.defaults.chargeId;
            if (current) ctrl.setValue(current);
            ctrl.fireValueChanged = function () {
                choice(r.id).chargeId = VIS.Utility.Util.getValueOfInt(ctrl.getValue());
                refreshAccept(r);
                setTimeout(function () { updateSplit(st.rows[r.id]); }, 0);           // after the lookup shows the charge name
            };
            st.chargeCtrls[r.id] = ctrl;
        }

        function accept(ids) {
            ids = ids.map(function (id) { return VIS.Utility.Util.getValueOfInt(id); }).filter(function (id) { return st.rows[id] && st.rows[id].status === "S"; });
            if (!ids.length) return;
            var items = ids.map(function (id) {
                var c = st.choices[id] || {};
                return { id: id, docRowId: c.docRowId || 0, differenceType: c.diffType || "", chargeId: chargeOf(id), taxId: c.taxId || st.defaults.taxId || 0,
                    chequeDate: c.chequeDate || "" };
            });
            busy(true);
            serverWork++;
            $.ajax({
                url: url + "AcceptAutoReconcile",
                type: "POST",
                data: { items: JSON.stringify(items) },
                complete: function () { serverWork--; if ($root) busy(false); },
                success: function (data) {
                    var res = parse(data) || {};
                    // the page was closed while Accept went on: only the form's lists are refreshed
                    if (!$root) { if ((res.accepted || []).length) formChanged(); return; }
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    var done = res.accepted || [], skipped = res.skipped || [];
                    done.forEach(function (id) {
                        var r = st.rows[id];
                        if (!r) return;
                        r.status = "A"; r.reason = ""; delete st.selected[id]; st.expanded[id] = false;
                        renderRow(r);
                    });
                    var groups = {}, order = [];
                    skipped.forEach(function (s) {
                        var r = st.rows[s.id];
                        if (r) { r.reason = s.reason; if (s.status === "R") { r.status = "R"; delete st.selected[s.id]; } renderRow(r); }
                        if (!groups[s.reason]) { groups[s.reason] = []; order.push(s.reason); }
                        groups[s.reason].push(s.lineNo);
                    });
                    // one message for the whole action: reconciled count, then each reason with its lines
                    var html = '<strong>' + esc(msg("VA012_ARReconciledOf", "Reconciled")) + ' ' + done.length + ' ' + esc(msg("VA012_AROf", "of")) + ' ' + ids.length + ' '
                        + esc(ids.length === 1 ? msg("VA012_ARSelectedLine", "selected line") : msg("VA012_ARSelectedLines", "selected lines")) + '.</strong>';
                    if (skipped.length) {
                        html += '<div>' + skipped.length + ' ' + esc(msg("VA012_ARNotReconciled", "not reconciled")) + ':</div><ul class="va012-ar-reasons">'
                            + order.map(function (reason) {
                                return '<li>' + esc(reason) + ' (' + groups[reason].length + '): ' + groups[reason].map(function (n) { return esc(msg("VA012_ARLine", "Line")) + ' ' + n; }).join(", ") + '</li>';
                            }).join("") + '</ul>';
                    }
                    showMessage(skipped.length ? (done.length ? "" : "bad") : "ok", html);
                    renderSelection();
                    refreshCounts();
                    if (done.length) formChanged();
                }
            });
        }

        /* the matched document(s) (reserved) and the other candidates (not reserved); a declined row keeps its first document */
        function split(r) {
            var chosen = r.docs.filter(function (d) { return d.reserved; });
            if (!chosen.length && r.status === "D" && r.docs.length) chosen = [r.docs[0]];
            return { chosen: chosen, others: r.docs.filter(function (d) { return chosen.indexOf(d) < 0; }) };
        }

        function docsCell(r) {
            var s = effective(r);
            if (!s.chosen.length && !s.others.length) return '<span class="va012-ar-muted">-</span>';
            var dragged = r.status === "S" && r.matchCase === "DR";
            var html = s.chosen.map(function (d) {
                // a document dragged onto the line by mistake comes off with its ×
                var off = dragged && d.reserved && d.docRowId
                    ? ' <button type="button" class="va012-ar-undoc" data-undoc="' + r.id + '" data-docrow="' + d.docRowId + '" title="'
                    + esc(msg("VA012_ARTakeOff", "Take this document off the line")) + '" aria-label="' + esc(msg("VA012_ARTakeOff", "Take this document off the line") + " " + d.docNo) + '">×</button>'
                    : '';
                // the list mixes lines of every type and of every run: name the document's type
                return (d.kind ? '<div class="va012-ar-muted">' + esc(typeName(d.kind)) + '</div>' : '')
                    + '<div><b>' + esc(d.docNo) + '</b> <span class="va012-ar-muted">' + amt(d.amount) + '</span>' + off + '</div>';
            }).join("");
            if (s.others.length) {
                var shown = s.others.slice(0, 5);
                html += '<div class="va012-ar-muted va012-ar-others">' + esc(s.chosen.length ? msg("VA012_AROtherCandidates", "Other candidates") : msg("VA012_ARCandidates", "Candidates")) + '</div>'
                    + shown.map(function (d) {
                        return '<div class="va012-ar-muted">' + esc(d.docNo) + ' · ' + esc(day(d.date)) + ' · ' + amt(d.amount) + '</div>';
                    }).join("")
                    + (s.others.length > shown.length ? '<div class="va012-ar-muted">+' + (s.others.length - shown.length) + ' ' + esc(msg("VA012_ARMore", "more")) + '</div>' : '');
            }
            return html;
        }

        function chequeCell(r) {
            var doc = effective(r).chosen[0] || null;
            if (!r.chequeNo && !(doc && doc.chequeNo)) return '<span class="va012-ar-muted">-</span>';
            // the line's field is empty but its narration names the document's cheque number
            var fromNarration = !r.chequeNo && doc && doc.chequeNo && narrationHas(r.narration, doc.chequeNo);
            var mark = "";
            if ((r.chequeNo || fromNarration) && doc && doc.chequeNo) {
                mark = fromNarration || normRef(r.chequeNo) === normRef(doc.chequeNo)
                    ? ' <span class="va012-ar-pill ok">' + esc(msg("VA012_ARMatches", "Matches")) + '</span>'
                    : ' <span class="va012-ar-pill bad">' + esc(msg("VA012_ARDiffers", "Differs")) + '</span>';
            }
            return '<div class="va012-ar-mono">' + esc(r.chequeNo || (fromNarration ? doc.chequeNo : "-")) + ' <span class="va012-ar-muted">'
                + esc(fromNarration ? msg("VA012_ARNarration", "narration") : msg("VA012_ARStmt", "stmt")) + '</span></div>'
                + (doc && doc.chequeNo ? '<div class="va012-ar-mono">' + esc(doc.chequeNo) + ' <span class="va012-ar-muted">' + esc(msg("VA012_ARDoc", "doc")) + '</span></div>' : "")
                + mark;
        }

        /* the line against the matched document(s): [field, statement value, document value, [class, plain words]] */
        function compareRows(r) {
            var docs = effective(r).chosen, d = docs[0];
            if (!d) return [];
            var total = docs.reduce(function (s, x) { return s + Number(x.amount || 0); }, 0);
            function mark(a, b, same) {
                if (!a && !b) return null;                                                   // on neither side: no row
                if (!b) return ["na", msg("VA012_AROnlyStatement", "Only on the statement")];
                if (!a) return ["na", msg("VA012_AROnlyDocument", "Only on the document")];
                return same(a, b) ? ["ok", "✓ " + msg("VA012_ARSame", "Same")] : ["no", "✗ " + msg("VA012_ARDifferent", "Different")];
            }
            function ref(a, b) { return normRef(a) === normRef(b); }
            var rows = [];
            function add(label, a, b, m) { if (m) rows.push([label, a || "", b || "", m]); }
            // an empty field whose value the narration names counts as the same, as the matcher reads it
            var inNarration = ["ok", "✓ " + msg("VA012_ARSameInNarration", "Same, in the narration")];
            var tag = " (" + msg("VA012_ARNarration", "narration") + ")";
            var trxN = !r.trxNo && narrationHas(r.narration, d.trxNo);
            var chqN = !r.chequeNo && narrationHas(r.narration, d.chequeNo);
            var bpN = !r.bpName && narrationNames(r.narration, d.bpName);
            add(msg("VA012_ARTrxNo", "Trx No / UTR"), trxN ? d.trxNo + tag : r.trxNo, d.trxNo, trxN ? inNarration : mark(r.trxNo, d.trxNo, ref));
            add(msg("VA012_ARChequeNo", "Cheque no."), chqN ? d.chequeNo + tag : r.chequeNo, d.chequeNo, chqN ? inNarration : mark(r.chequeNo, d.chequeNo, ref));
            add(msg("VA012_BusinessPartner", "Business partner"), bpN ? d.bpName + tag : r.bpName, docs.length > 1 ? d.bpName + " (" + docs.length + ")" : d.bpName,
                bpN ? inNarration : mark(r.bpName, d.bpName, function (a, b) { return a === b; }));
            var diff = Math.round((Number(r.amount) - total) * 100) / 100;
            add(msg("VA012_ARAmount", "Amount"), amt(r.amount), amt(total),
                diff === 0 ? ["ok", "✓ " + msg("VA012_ARSame", "Same")] : ["near", msg("VA012_ARDiffersBy", "Differs by") + " " + amt(Math.abs(diff))]);
            var days = r.date && d.date ? Math.round(Math.abs(new Date(r.date) - new Date(d.date)) / 86400000) : null;
            add(msg("VA012_ARDate", "Date"), day(r.date), day(d.date),
                days === null ? null : days === 0 ? ["ok", "✓ " + msg("VA012_ARSameDate", "Same date")] : ["near", days + " " + msg("VA012_ARDaysApart", "days apart")]);
            return rows;
        }

        function docList(title, docs) {
            if (!docs.length) return "";
            return '<div class="va012-ar-docs"><div class="va012-ar-muted">' + esc(title) + '</div>' + docs.map(function (x) {
                return '<div><b>' + esc(x.docNo) + '</b> ' + esc(x.bpName) + ' · ' + esc(day(x.date)) + ' · ' + amt(x.amount) + '</div>';
            }).join("") + '</div>';
        }

        function detail(r) {
            // lines auto reconcile has no row for: reconciled elsewhere, or not covered by a run yet
            if (r.status === "M") return '<div class="va012-ar-detail"><div class="va012-ar-note">' + esc(msg("VA012_ARManualNote", "Reconciled in the form or by Match Statement, not by auto reconcile."))
                + '</div>' + docList(msg("VA012_ARReconciledWith", "Reconciled with"), r.docs) + '</div>';
            if (r.status === "O") return '<div class="va012-ar-detail"><div class="va012-ar-note">' + esc(msg("VA012_ARNotRunReconcile", "No run has covered this line yet. Select Reconcile."))
                + '</div></div>';
            var s = effective(r), html = '<div class="va012-ar-detail">', pick = chooser(r);
            if (r.reason) html += '<div class="va012-ar-note">' + esc(r.reason) + '</div>';
            // a declined line is matched by hand like a line without a match
            if (r.status === "D") html += '<div class="va012-ar-muted">' + esc(st.filter === "N"
                ? msg("VA012_ARDragDeclined", "Declined. Drag another document from the Documents panel onto this line to reconcile it.")
                : msg("VA012_ARChooseNoMatch", "Choose No match to drag a document onto this line.")) + '</div>';
            if (!s.chosen.length) {
                if (!s.others.length && !r.reason) html += '<div class="va012-ar-note">' + esc(msg("VA012_ARNoDocument", "No document was matched to this line.")) + '</div>';
                if (r.status === "N") html += '<div class="va012-ar-muted">' + esc(st.filter === "N"
                    ? msg("VA012_ARDragHere", "Drag a document from the Documents panel onto this line.")
                    : msg("VA012_ARChooseNoMatch", "Choose No match to drag a document onto this line.")) + '</div>';
                return html + (pick || docList(msg("VA012_ARCandidates", "Candidates"), s.others)) + '</div>';
            }
            html += '<table class="va012-ar-cmp"><thead><tr><th>' + esc(msg("VA012_ARField", "Field")) + '</th><th>'
                + esc(msg("VA012_ARStatementLine", "Statement line")) + '</th><th>' + esc(msg("VA012_ARDocument", "Document")) + '</th><th></th></tr></thead><tbody>'
                + compareRows(r).map(function (c) {
                    return '<tr><td>' + esc(c[0]) + '</td><td class="va012-ar-mono">' + esc(c[1]) + '</td><td class="va012-ar-mono">' + esc(c[2])
                        + '</td><td class="va012-ar-m-' + c[3][0] + '">' + esc(c[3][1]) + '</td></tr>';
                }).join("")
                + '</tbody></table>';
            if (s.chosen.length > 1) html += docList(msg("VA012_ARMatchedDocs", "Matched documents"), s.chosen);
            // a suggested line: choose among the candidates and settle the difference here; otherwise just list the others
            html += pick || docList(msg("VA012_AROtherCandidates", "Other candidates"), s.others);
            return html + diffBox(r) + payBox(r) + '</div>';
        }

        function renderRow(r) {
            var open = !!st.expanded[r.id];
            var cls = r.status === "C" || r.status === "A" || r.status === "M" ? "C" : r.status;
            var line = '<tr class="va012-ar-row s-' + cls + '" tabindex="0" data-id="' + r.id + '" aria-expanded="' + open + '">'
                + '<td class="va012-ar-chk">' + (r.status === "S" ? '<input type="checkbox" data-sel="' + r.id + '"' + (st.selected[r.id] ? " checked" : "")
                    + ' aria-label="' + esc(msg("VA012_ARSelectLine", "Select line") + " " + r.lineNo) + '">' : "") + '</td>'
                + '<td><span class="va012-ar-muted">' + esc(day(r.date)) + '</span> · <span class="va012-ar-amt">' + amt(r.amount) + '</span>'
                + '<div class="va012-ar-muted">' + esc(msg("VA012_ARLine", "Line")) + ' ' + r.lineNo + (r.payMethod ? ' · ' + esc(r.payMethod) : "") + '</div>'
                + (r.bpName ? '<div>' + esc(r.bpName) + '</div>' : '')
                + '<div class="va012-ar-narr">' + esc(r.narration) + '</div></td>'
                + '<td>' + docsCell(r) + '</td>'
                + '<td>' + chequeCell(r) + '</td>'
                + '<td><b>' + esc(matchedBy(r)) + '</b></td>'
                + '<td>' + statusPill(r) + (r.status !== "C" && r.status !== "A" && r.reason ? '<div class="va012-ar-muted va012-ar-reason">' + esc(r.reason) + '</div>' : "") + '</td>'
                + '<td class="va012-ar-acts-cell">' + (r.status === "S" ? acceptButton(r) + ' <button type="button" class="va012-ar-sbtn bad" data-decline="' + r.id + '">' + esc(msg("VA012_ARDecline", "Decline")) + '</button>' : "") + '</td>'
                + '</tr>';
            disposeCharge(r.id);
            var $existing = $body.find('tr.va012-ar-row[data-id="' + r.id + '"]');
            var $new = $(line);
            if ($existing.length) {
                $existing.next("tr.va012-ar-detailrow").remove();
                $existing.replaceWith($new);
            }
            else {
                var $loading = $body.find("tr.va012-ar-loading");
                if ($loading.length) $loading.before($new); else $body.append($new);
            }
            makeDroppable($new, r);
            if (open) {
                var $detailRow = $('<tr class="va012-ar-detailrow"><td colspan="7">' + detail(r) + '</td></tr>');
                $new.after($detailRow);
                makeDroppable($detailRow, r);
                mountCharge(r, $detailRow);
                updateSplit(r);
            }
        }

        function acceptButton(r) {
            var need = needs(r);
            return '<button type="button" class="va012-ar-sbtn ok" data-accept="' + r.id + '"' + (need ? ' disabled title="' + esc(need) + '"' : '') + '>'
                + esc(msg("VA012_ARAccept", "Accept")) + '</button>';
        }

        function decline(ids) {
            ids = ids.filter(function (id) { return st.rows[id] && st.rows[id].status === "S"; });
            if (!ids.length) return;
            busy(true);
            $.ajax({
                url: url + "DeclineAutoReconcile",
                type: "POST",
                data: { suggestionIds: ids.join(",") },
                complete: function () { busy(false); },
                success: function (data) {
                    var res = parse(data) || {};
                    if (res.error) { showMessage("bad", esc(res.error)); return; }
                    ids.forEach(function (id) { st.rows[id].status = "D"; delete st.selected[id]; renderRow(st.rows[id]); });
                    showMessage("ok", '<strong>' + esc(msg("VA012_ARDeclinedCount", "Declined")) + ' ' + (res.declined || 0) + ' / ' + ids.length + '</strong>');
                    refreshCounts();
                }
            });
        }

        /*
         * The tab's close (its × or close()) goes through the window's onClosed, which may refuse it by returning false. While
         * Reconcile or Accept is still working the user is asked first: the work goes on on the server either way, and its
         * results show when Auto Reconcile is opened again. Kept: nothing closes (and a waiting close(then) is dropped).
         */
        function guardClose() {
            var win = frame && frame.frame;
            if (!win) return;
            var closeTab = win.onClosed;
            win.onClosed = function () {
                if (serverWork > 0 && !closeConfirmed) {
                    VIS.ADialogUI.ask(msg("VA012_ARCloseWhileRunning", "Reconcile is still running. If you close this page, the run continues on the server "
                        + "and its results show when you open Auto Reconcile again. Close the page?"), msg("VA012_AutoReconcile", "Auto Reconcile"), function (ok) {
                        if (!ok) { closeThen = null; return; }
                        closeConfirmed = true;
                        if (frame && frame.frame) frame.frame.dispose();
                    });
                    return false;
                }
                return typeof closeTab === "function" ? closeTab.apply(this, arguments) : true;
            };
        }

        /* the page contract of VIS.CFrame: root element, size, refresh and dispose (called when the page's tab is closed) */
        this.getRoot = function () { return $root; };
        this.sizeChanged = function (height) {
            if ($root && height > 0) $root.css("height", height + "px");
        };
        this.refresh = function () { };
        this.dispose = function () {
            if (xhr) xhr.abort();
            if (dp.xhr) dp.xhr.abort();
            if (bpControl && bpControl.disposeComponent) bpControl.disposeComponent();
            if (chargeControl && chargeControl.disposeComponent) chargeControl.disposeComponent();
            if ($root) $root.remove();
            $root = null;
            frame = null;
            if (typeof opts.onClosed === "function") opts.onClosed();
            var then = closeThen;
            closeThen = null;
            if (typeof then === "function") then();
        };
        /* closes the page's tab, as its close icon does; then (optional) runs once the page is closed, which is not the case
           when a run or Accept is still in progress and the user keeps the page */
        this.close = function (then) {
            closeThen = then || null;
            if (frame && frame.frame) VIS.viewManager.closeFrame(frame.frame.getId());
            else { closeThen = null; if (typeof then === "function") then(); }
        };

        this.show = function () {
            build();
            $root.addClass("va012-ar-page");
            frame = new VIS.CFrame();
            frame.setName(msg("VA012_AutoReconcile", "Auto Reconcile"));
            frame.setTitle(msg("VA012_AutoReconcile", "Auto Reconcile") + (opts.accountName ? " · " + opts.accountName : ""));
            frame.setContent(this);
            frame.show();
            guardClose();
            // the frame passes the page its height (screen less header); on later resizes the view manager does it
            if (frame.frame && frame.frame.sizeChanged) frame.frame.sizeChanged();
            else this.sizeChanged(VIS.Env.getScreenHeight() - 85);
            showView("run");
            $tabs.on("mouseup", "button", function () { this.blur(); });
        };
    };

    // message with an English fallback until the VA012_AR* messages are seeded; the form uses it for its button label
    VA012.AutoReconcile.msg = msg;

    VA012.AutoReconcile.open = function (opts) {
        var d = new VA012.AutoReconcile(opts);
        d.show();
        return d;
    };
})(VA012, jQuery);
