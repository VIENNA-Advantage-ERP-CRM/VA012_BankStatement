/************************************************************
 * Module Name    : VA012
 * Purpose        : Bank charge Summary-Month wise widget
 * chronological  : Development
 * Created Date   : 7 Nov, 2024
 * Created by     : VIS103
 *
 * Redesigned against the Onfinity design system
 * (design.md > dashboard-widgets.md + foundations.md). The markup below
 * is the shared widget shell - header band, filter row, chart region -
 * and every size lives in VA012_BankChargeSummary.css as em against the
 * widget root anchor. The only px values built here are the Chart.js
 * font sizes, which are derived from the resolved root font-size so the
 * canvas rides the same lever as the rest of the widget.
 ***********************************************************/
; VA012 = window.VA012 || {};

; (function (VA012, $) {

    //VA012.VA012_BankChargeSummary = VA012 || {};

    // Form class function fullnamespace

    // Document Uploader
    VA012.VA012_BankChargeSummary = function () {
        /* Global variable declaration */
        this.frame = null;
        this.windowNo = 0;
        this.widgetInfo = null;
        var $self = this;
        var $root = null;
        var $bsyDiv = null;
        var widgetID = null;
        var shellDiv = null;
        var subtitleDiv = null;
        var totalPill = null;
        var chartArea = null;
        var emptyDiv = null;
        var chart = null;
        var _cmbBankAccountCtrl = null;
        var ctx = VIS.Env.getCtx();
        var _cmbChargeCtrl = null;
        var yrStartDate = null;
        var yrEndDate = null;
        var Year_ID = null;
        var C_Charge_ID = 0;

        /* Chart palette - foundations.md > Color Palette. Bars carry the
           primary blue; a negative period (a charge reversal) takes the
           danger tone so the sign reads without consulting the axis. */
        var CHART = {
            bar: '#0083DA',
            barHover: '#0069AE',
            barNegative: '#D14545',
            grid: '#E2EAF1',
            zeroLine: '#C5D2DD',
            tick: '#5F7283',
            text: '#102C3F',
            body: '#41576A',
            tooltipBg: '#FFFFFF',
            tooltipBorder: '#D9E2EB',
            font: "'Roboto', sans-serif"
        };

        // init log class
        this.log = VIS.Logging.VLogger.getVLogger('BankChargeSummaryMonthWidget');
        //Privilized function
        this.getRoot = function () {
            return $root;
        };

        /*Create Busy Indicator */
        function createBusyIndicator() {
            // No inline width - the stylesheet pins the overlay to all four
            // edges of the widget.
            $bsyDiv = $('<div id="busyDivId_' + widgetID + '" class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap">' +
                '<i class= "vis_widgetloader"></i></div></div>').hide();
            $root.append($bsyDiv);
        };

        this.setBusy = function (busy) {
            if (busy)
                $bsyDiv.show();
            else
                $bsyDiv.hide();
        };

        // Initialize the controls, Main function
        this.initialize = function () {
            widgetID = this.widgetInfo.AD_UserHomeWidgetID;
            //  widgetID = widgetID + '_' + VIS.Env.getWindowNo();
            if (widgetID == 0) {
                widgetID = $self.windowNo;
            }
            //Define root and busy indicator
            $root = $("<div id='VA012_rootBankCharge_" + widgetID + "' class='VA012-bcs'></div>");
            createBusyIndicator();
            $bsyDiv.show();
            //Get Finacial Year data
            $.ajax({
                url: VIS.Application.contextUrl + "VA012_BankChargeSummary/GetFinancialYearDetail",
                type: "GET",
                async: true,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    if (data != null && data != "") {
                        data = JSON.parse(data);
                        yrStartDate = data.StartDate;
                        yrEndDate = data.EndDate;
                        Year_ID = data.C_Year_ID;
                        Design();
                    }
                    $bsyDiv.hide();
                },
                error: function (errorThrown) {
                    $bsyDiv.hide();
                    VIS.ADialog.error(errorThrown.statusText);
                    return false;
                }
            });
        };

        function getDates(date) {
            var date = "TO_DATE('" + new Date(date).getDate() + "/" + (new Date(date).getMonth() + 1) + "/" + new Date(date).getFullYear() + "','dd/mm/yyyy')";
            return date;
        };

        /**
         * Financial year range for the header subtitle. Rendered through the
         * browser locale rather than a new message key, so it needs no
         * dictionary row to read correctly in every language.
         */
        function getYearRange() {
            if (yrStartDate == null || yrEndDate == null) {
                return "";
            }
            var from = new Date(yrStartDate);
            var to = new Date(yrEndDate);
            if (isNaN(from.getTime()) || isNaN(to.getTime())) {
                return "";
            }
            return from.toLocaleDateString() + ' – ' + to.toLocaleDateString();
        };

        //Create design
        function Design() {
            //Get year start and end date
            var startDate = getDates(yrStartDate);
            var endDate = getDates(yrEndDate);

            //Bank Account control validation
            var validation = "C_BankAccount.ISACTIVE='Y' AND C_BankAccount.C_BankAccount_ID IN" +
                "(SELECT bs.C_BankAccount_ID FROM C_BankStatement bs INNER JOIN C_BankStatementLine bsl ON (bs.C_BankStatement_ID = bsl.C_BankStatement_ID)" +
                " WHERE TRUNC(bsl.STATEMENTLINEDATE) BETWEEN " + startDate + " AND " + endDate + ") ";

            /* parameters are: context, windowno., coloumn id, display type, DB coloumn name, Reference key, Is parent, Validation Code*/
            var lookup = VIS.MLookupFactory.get(VIS.context, $self.windowNo, 0, VIS.DisplayType.TableDir, "C_BankAccount_ID", 0, false, validation);
            // Parameters are: columnName, mandatory, isReadOnly, isUpdateable, lookup,display length
            _cmbBankAccountCtrl = new VIS.Controls.VComboBox("C_BankAccount_ID", true, false, true, lookup, 50);
            //Get lookup data
            var data = lookup.getData(true, true, false, false);
            if (data != null && data != undefined && data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    _cmbBankAccountCtrl.getControl().append('<option value=' + data[i].Key + '>' + data[i].Name + '</option>');
                }
                //Set default value
                _cmbBankAccountCtrl.getControl().prop('selectedIndex', 0);
            }
            //Set value on context  for default selected bank account
            ctx.setContext($self.windowNo, "VA012_BankAccount_ID", VIS.Utility.Util.getValueOfInt(_cmbBankAccountCtrl.getValue()));
            //Charge control validation
            validation = "";
            validation = "C_Charge.ISACTIVE='Y' AND C_Charge.C_Charge_ID IN (SELECT bsl.C_Charge_ID FROM C_BankStatement bs INNER JOIN C_BankStatementLine bsl ON (bs.C_BankStatement_ID = bsl.C_BankStatement_ID)" +
                " WHERE bs.C_BankAccount_ID=@VA012_BankAccount_ID@ AND TRUNC(bsl.STATEMENTLINEDATE) BETWEEN " + startDate + " AND " + endDate + ")";
            var chargeLookup = VIS.MLookupFactory.get(VIS.context, $self.windowNo, 0, VIS.DisplayType.TableDir, "C_Charge_ID", 0, false, validation);
            _cmbChargeCtrl = new VIS.Controls.VComboBox("C_Charge_ID", false, false, true, chargeLookup, 50);

            var widgetTitle = VIS.Msg.getMsg('VA012_BankChargeSummary');
            var bankLabel = VIS.Msg.getMsg("VA012_BankAccount");
            var chargeLabel = VIS.Msg.getMsg("Charge");

            // Rebuild rather than patch - refreshWidget() comes back through
            // here with fresh lookups, so the previous shell (and its chart)
            // has to go first.
            destroyChart();
            $root.find('#VA012-bcs-shell_' + widgetID).remove();

            // Widget shell: header band, filter row, chart region
            // (dashboard-widgets.md > Widget Header).
            shellDiv = $('<div class="VA012-bcs-shell" id="VA012-bcs-shell_' + widgetID + '">'
                + '<div class="VA012-bcs-header">'
                + '<div class="VA012-bcs-iconwell"><i class="fa fa-bar-chart" aria-hidden="true"></i></div>'
                + '<div class="VA012-bcs-titlewrap">'
                + '<div class="VA012-bcs-title" title="' + widgetTitle + '">' + widgetTitle + '</div>'
                + '<div class="VA012-bcs-subtitle" id="VA012-bcs-subtitle_' + widgetID + '"></div>'
                + '</div>'
                + '<div class="VA012-bcs-pill" id="VA012-bcs-pill_' + widgetID + '" hidden></div>'
                + '</div>'

                //Start Parameters Div
                + '<div class="VA012-bcs-filters" id="VA012-paramtersDiv_' + widgetID + '">'
                + '<div class="VA012-bcs-field">'
                + '<label class="VA012-bcs-label">' + bankLabel + '<span class="VA012-bcs-req">*</span></label>'
                + '<div class="VA012-bcs-control" id="VA012_cmbBankAcct_' + widgetID + '">'
                + '<i class="fa fa-credit-card VA012-bcs-fieldicon" aria-hidden="true"></i>'
                + '</div>'
                + '</div>'
                + '<div class="VA012-bcs-field">'
                + '<label class="VA012-bcs-label">' + chargeLabel + '</label>'
                + '<div class="VA012-bcs-control" id="VA012_cmbCharge_' + widgetID + '">'
                + '<i class="fa fa-tag VA012-bcs-fieldicon" aria-hidden="true"></i>'
                + '</div>'
                + '</div>'
                + '</div>'
                //End Parameters div

                + '<div id="VA012-columnChart_' + widgetID + '" class="VA012-bcs-chart">'
                + '<div class="VA012-bcs-empty" id="VA012_norecordcont_' + widgetID + '" hidden>'
                + '<i class="fa fa-bar-chart" aria-hidden="true"></i>'
                + '<span></span>'
                + '</div>'
                + '</div>'
                + '</div>'
            );
            $root.append(shellDiv);

            subtitleDiv = $root.find('#VA012-bcs-subtitle_' + widgetID);
            totalPill = $root.find('#VA012-bcs-pill_' + widgetID);
            chartArea = $root.find('#VA012-columnChart_' + widgetID);
            emptyDiv = $root.find('#VA012_norecordcont_' + widgetID);

            subtitleDiv.text(getYearRange()).attr('title', getYearRange());

            //Append bank account and Charge controls in root. The labels are
            //rendered above, so only the controls themselves go in here.
            $root.find("#VA012_cmbBankAcct_" + widgetID)
                .append(_cmbBankAccountCtrl.getControl().addClass('VA012-bcs-input').attr('aria-label', bankLabel));
            $root.find("#VA012_cmbCharge_" + widgetID)
                .append(_cmbChargeCtrl.getControl().addClass('VA012-bcs-input').attr('aria-label', chargeLabel));

            events();
            syncCellVariant();
            if (_cmbBankAccountCtrl.getValue()) {
                $bsyDiv.show();
                _cmbBankAccountCtrl.setValue(_cmbBankAccountCtrl.getValue());
                GetCanvas();
            }
            else {
                showEmpty(VIS.Msg.getMsg("VA012_SelectBankAccountFirst"));
            }
        };

        /**
         * Adapt the chrome to the cell we were actually given. Short cells
         * drop the icon well and the subtitle; the Content Fit Budget spends
         * that height on the chart instead.
         */
        function syncCellVariant() {
            if ($root == null) {
                return;
            }
            var rootPx = parseFloat($root.css('font-size')) || 16;
            $root.toggleClass('VA012-bcs-compact', $root.height() < (18 * rootPx));
        };

        function events() {
            //Bank Accoount control change event
            _cmbBankAccountCtrl.fireValueChanged = function () {
                if (_cmbBankAccountCtrl.getValue()) {
                    $bsyDiv.show();
                    _cmbBankAccountCtrl.setValue(_cmbBankAccountCtrl.getValue());
                    //set charge value to null if user change bank account
                    if (_cmbChargeCtrl.getValue() != null) {
                        _cmbChargeCtrl.setValue(null);
                        C_Charge_ID = 0;
                    }
                    /*VIS_427 Set the value of bank account on context*/
                    ctx.setContext($self.windowNo, "VA012_BankAccount_ID", VIS.Utility.Util.getValueOfInt(_cmbBankAccountCtrl.getValue()));
                    GetCanvas();
                    // $bsyDiv.hide();
                }
                else {
                    destroyChart();
                    _cmbBankAccountCtrl.setValue(_cmbBankAccountCtrl.getValue());
                    _cmbChargeCtrl.setValue("");
                    showEmpty(VIS.Msg.getMsg("VA012_SelectBankAccountFirst"));
                    VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                    return false;
                }
            };
            //Charge control change event
            _cmbChargeCtrl.fireValueChanged = function () {
                if (_cmbChargeCtrl.getValue()) {
                    if (_cmbBankAccountCtrl.getValue()) {
                        C_Charge_ID = _cmbChargeCtrl.getValue();
                        _cmbChargeCtrl.setValue(_cmbChargeCtrl.getValue());
                        $bsyDiv.show();
                        GetCanvas();
                        // $bsyDiv.hide();
                    }
                    else {
                        _cmbChargeCtrl.setValue(_cmbChargeCtrl.getValue());
                        VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                        return false;
                    }
                }
                else {
                    C_Charge_ID = 0;
                    $bsyDiv.show();
                    GetCanvas();
                    // $bsyDiv.hide();
                }
            };
        };

        /* ========================================================
           Chart region
           ======================================================== */

        function destroyChart() {
            // Chart.js keeps the instance registered against the canvas, so
            // dropping the element alone leaks the chart and its resize
            // listener. Destroy first, then remove.
            if (chart != null) {
                chart.destroy();
                chart = null;
            }
            if (chartArea != null) {
                chartArea.find('canvas').remove();
            }
        };

        /* Empty state - shown instead of an empty axis frame. */
        function showEmpty(message) {
            if (emptyDiv == null) {
                return;
            }
            emptyDiv.find('span').text(message).attr('title', message);
            emptyDiv.prop('hidden', false);
            setTotal(null, null, 0);
        };

        function hideEmpty() {
            if (emptyDiv != null) {
                emptyDiv.prop('hidden', true);
            }
        };

        /* Header stat pill - the year total for the current selection
           (dashboard-widgets.md > Header Stat Pill). */
        function setTotal(total, isoCode, precision) {
            if (totalPill == null) {
                return;
            }
            if (total == null) {
                totalPill.text('').attr('title', '').prop('hidden', true);
                return;
            }
            var text = (isoCode ? isoCode + ' ' : '') + formatAmount(total, precision);
            totalPill.text(text).attr('title', text).prop('hidden', false);
        };

        function formatAmount(value, precision) {
            return value.toLocaleString(window.navigator.language,
                { minimumFractionDigits: precision, maximumFractionDigits: precision });
        };

        /* Axis labels only - keeps a 12-period axis readable in a 2-column
           cell without dropping to a rotated tick. Small ticks keep their
           decimals: rounding them to whole numbers printed the same label
           on consecutive gridlines ("1, 1, 0" on a 0-1 axis). */
        function formatCompact(value) {
            var abs = Math.abs(value);
            if (abs >= 1000000) {
                return (value / 1000000).toFixed(abs >= 10000000 ? 0 : 1) + 'M';
            }
            if (abs >= 1000) {
                return (value / 1000).toFixed(abs >= 10000 ? 0 : 1) + 'K';
            }
            return value.toLocaleString(window.navigator.language,
                { minimumFractionDigits: 0, maximumFractionDigits: abs < 10 ? 2 : 0 });
        };

        /**
         * The service LEFT JOINs every period of the year onto the statement
         * lines, so a bank account with no charges still comes back as a full
         * set of labels with nothing but zeros behind them. Plotting that
         * draws an empty axis frame, which reads as a broken widget rather
         * than as "no charges" - so it takes the empty state instead.
         */
        function hasChargeData(bankData) {
            var values = bankData.bankChargeData;
            if (values == null || values.length == 0) {
                return false;
            }
            for (var i = 0; i < values.length; i++) {
                if (Number(values[i])) {
                    return true;
                }
            }
            return false;
        };

        /* Chart.js takes px, so the canvas is put on the widget's own lever
           by reading the resolved root font-size: 0.6875em ticks and 0.75em
           tooltip text, the same tokens the rest of the widget uses. */
        function chartFonts() {
            var rootPx = ($root != null ? parseFloat($root.css('font-size')) : 16) || 16;
            return {
                tick: Math.round(rootPx * 0.6875),
                body: Math.round(rootPx * 0.75)
            };
        };

        // Get bank charge data and create chart
        function GetCanvas() {
            $.ajax({
                url: VIS.Application.contextUrl + "VA012_BankChargeSummary/GetBankChargeData",
                type: "GET",
                async: true,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: ({
                    C_BankAccount_ID: VIS.Utility.Util.getValueOfInt(_cmbBankAccountCtrl.getValue()), C_Charge_ID: C_Charge_ID, yrStartDate: yrStartDate, yrEndDate: yrEndDate, Year_ID: Year_ID
                }),
                success: function (bankData) {
                    if (bankData != null && bankData != "") {
                        bankData = JSON.parse(bankData);
                        // Remove existing chart if exists
                        destroyChart();

                        // The service returns empty arrays - or a full year of
                        // zeros - rather than an error when the selection has
                        // no charge lines, so the empty state keys off the
                        // data itself.
                        if (bankData == null || bankData.errorMessage != null
                            || bankData.labels == null || bankData.labels.length == 0
                            || !hasChargeData(bankData)) {
                            showEmpty(VIS.Msg.getMsg("VA012_RecordNotFound"));
                        }
                        else {
                            hideEmpty();
                            DrawChart(bankData);
                        }
                    }
                    $bsyDiv.hide();
                },
                error: function (errorThrown) {
                    $bsyDiv.hide();
                    VIS.ADialog.error(errorThrown.statusText);
                    return false;
                }
            });

        };

        function DrawChart(bankData) {
            var precision = VIS.Utility.Util.getValueOfInt(bankData.Precision);
            var labels = bankData.labels;
            var iso_code = bankData.currency || [];
            var values = bankData.bankChargeData || [];
            var fonts = chartFonts();

            // Year total for the header pill, plus the first currency the
            // service reported - every row is converted to the bank
            // account's currency, so one code covers the series.
            var total = 0;
            var currency = "";
            for (var i = 0; i < values.length; i++) {
                total += Number(values[i]) || 0;
                if (!currency && iso_code[i]) {
                    currency = iso_code[i];
                }
            }
            setTotal(total, currency, precision);

            // Prepare the data object for the chart
            var data = {
                labels: labels, // Dynamic labels
                datasets: [
                    {
                        data: values,
                        // A reversal reads as a reversal without consulting
                        // the axis - foundations.md > Semantic Colors.
                        backgroundColor: values.map(function (v) {
                            return v < 0 ? CHART.barNegative : CHART.bar;
                        }),
                        hoverBackgroundColor: values.map(function (v) {
                            return v < 0 ? CHART.barNegative : CHART.barHover;
                        }),
                        borderColor: 'rgba(0,0,0,0)', // Transparent border color
                        borderWidth: 0, // No border
                        borderRadius: 4,
                        maxBarThickness: 26,
                        categoryPercentage: 0.75,
                        barPercentage: 0.85,
                        order: 1
                    }
                ]
            };

            //this plugin will differntiate b/w -ve and +ve line
            var zeroLinePlugin = {
                id: 'zeroLine',
                beforeDatasetsDraw: function (chartInstance) {
                    var c = chartInstance.ctx;
                    var yScale = chartInstance.scales.y;
                    var xScale = chartInstance.scales.x;

                    // Find the pixel for 0 on the Y-axis
                    var zeroY = yScale.getPixelForValue(0);

                    // Draw the line
                    c.save();
                    c.beginPath();
                    c.moveTo(xScale.left, zeroY);
                    c.lineTo(xScale.right, zeroY);
                    c.lineWidth = 1;
                    c.strokeStyle = CHART.zeroLine;
                    c.stroke();
                    c.restore();
                }
            };

            // Define the chart configuration for BAR chart
            var config = {
                type: 'bar',
                data: data,
                options: {
                    responsive: true,
                    // The grid track owns the widget height, so the canvas
                    // fills the region it is given instead of holding a
                    // ratio (dashboard-widgets.md > Square Block Sizing).
                    maintainAspectRatio: false,
                    layout: {
                        padding: 0
                    },
                    scales: {
                        x: {
                            grid: {
                                // Vertical rules only - one per period, so the
                                // bars read as a month-by-month series. The
                                // horizontal rules stay off (they were the
                                // loudest thing on the tile when the data was
                                // sparse).
                                display: true,
                                color: CHART.grid,
                                drawBorder: false,
                                drawTicks: false
                            },
                            ticks: {
                                color: CHART.tick,
                                autoSkip: true,
                                maxRotation: 0,
                                font: { family: CHART.font, size: fonts.tick }
                            }
                        },
                        y: {
                            beginAtZero: true,
                            grid: {
                                // No gridlines on either axis - the bars and
                                // the zero baseline carry the read, and the
                                // horizontal rules were the loudest thing on
                                // the tile when the data was sparse.
                                display: false,
                                drawBorder: false,
                                drawTicks: false
                            },
                            ticks: {
                                color: CHART.tick,
                                padding: 6,
                                // Four rules at most - the cell is short and
                                // the gridlines are chrome, not content.
                                maxTicksLimit: 4,
                                font: { family: CHART.font, size: fonts.tick },
                                callback: function (value) {
                                    return formatCompact(value);
                                }
                            }
                        }
                    },
                    plugins: {
                        // Single series - the widget title already names it.
                        legend: {
                            display: false
                        },
                        tooltip: {
                            backgroundColor: CHART.tooltipBg,
                            titleColor: CHART.text,
                            bodyColor: CHART.body,
                            borderColor: CHART.tooltipBorder,
                            borderWidth: 1,
                            cornerRadius: 8,
                            padding: 8,
                            displayColors: false,
                            titleFont: { family: CHART.font, size: fonts.body, weight: '500' },
                            bodyFont: { family: CHART.font, size: fonts.body },
                            callbacks: {
                                label: function (tooltipItem) {
                                    var dataIndex = tooltipItem.dataIndex;
                                    var datasetIndex = tooltipItem.datasetIndex;
                                    var dataset = tooltipItem.chart.data.datasets[datasetIndex];
                                    var value = dataset.data[dataIndex];
                                    return (iso_code[dataIndex] ? iso_code[dataIndex] + ': ' : '')
                                        + formatAmount(value, precision);
                                }
                            }
                        }
                    }
                },
                plugins: [zeroLinePlugin]
            };

            // Create a new canvas element and append it to the chart region
            var canvas = $('<canvas class="VA012-columnChart-canvas"></canvas>');
            chartArea.append(canvas);

            // Initialize the chart with the new data
            chart = new Chart(canvas[0].getContext('2d'), config);
        };

        /* Re-measure the cell and re-derive the canvas font sizes. init()
           appends the root only after initialize() has run, so nothing is
           measurable until a tick later. */
        this.syncLayout = function () {
            if ($root == null) {
                return;
            }
            syncCellVariant();
            if (chart != null) {
                var fonts = chartFonts();
                chart.options.scales.x.ticks.font.size = fonts.tick;
                chart.options.scales.y.ticks.font.size = fonts.tick;
                chart.options.plugins.tooltip.titleFont.size = fonts.body;
                chart.options.plugins.tooltip.bodyFont.size = fonts.body;
                chart.update('none');
            }
        };

        /* Host hook - called by HomeMgr2 whenever the dashboard is resized or
           edit mode is toggled. The span drives the narrow-cell variant in
           the stylesheet; the cell's own height drives the compact header. */
        this.widgetSizeChange = function (size) {
            if ($root == null || size == null) {
                return;
            }
            $root.attr('data-bcs-rows', VIS.Utility.Util.getValueOfInt(size.rows) || 1);
            $root.attr('data-bcs-cols', VIS.Utility.Util.getValueOfInt(size.Cols) || 1);
            $self.syncLayout();
        };

        /*this function is used to refresh design and data of widget*/
        this.refreshWidget = function () {
            //VIS_427 Set charge id to 0 if user refresh the widget
            C_Charge_ID = 0;
            Design();
        };

        this.disposeComponents = function () {
            destroyChart();

            // Unbind while the references are still live.
            if ($root != null) {
                $root.find('*').off();
                $root.off();
            }

            $self = null;
            $root = null;
            this.frame = null;
            this.windowNo = 0;
            this.widgetInfo = null;
            $bsyDiv = null;
            shellDiv = null;
            subtitleDiv = null;
            totalPill = null;
            chartArea = null;
            emptyDiv = null;
            _cmbBankAccountCtrl = null;
            _cmbChargeCtrl = null;
            yrStartDate = null;
            yrEndDate = null;
            Year_ID = null;
            C_Charge_ID = 0;
        };
    };

    // Must Implement with same parameter
    VA012.VA012_BankChargeSummary.prototype.init = function (windowNo, frame) {
        this.frame = frame;
        this.windowNo = windowNo;
        // Widget info, we can save additional information in widget record
        this.widgetInfo = frame.widgetInfo;
        this.initialize();
        this.frame.getContentGrid().append(this.getRoot);
        // Measure once the cell exists, so the header treatment and the
        // canvas type scale are right on first paint.
        var self = this;
        setTimeout(function () {
            self.syncLayout();
        }, 0);
    };

    // Must implement dispose
    VA012.VA012_BankChargeSummary.prototype.dispose = function () {
        /*CleanUp Code */
        //Dispose this component
        this.disposeComponents();
        //Call frame dispose function
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
    };

    // Fire window's event from widget
    VA012.VA012_BankChargeSummary.prototype.addChangeListener = function (listener) {
        this.listener = listener;
    };

    VA012.VA012_BankChargeSummary.prototype.widgetFirevalueChanged = function (value) {
        // Trigger custom event with the value
        if (this.listener)
            this.listener.widgetFirevalueChanged(value);
    };

})(VA012, jQuery);
