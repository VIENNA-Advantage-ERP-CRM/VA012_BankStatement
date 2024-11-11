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
        var _cmbBankAccountCtrl = null;
        var _cmbChargeCtrl = null;
        var yrStartDate = null;
        var yrEndDate = null;
        var Year_ID = null;
        var C_Charge_ID = 0;

        // init log class
        this.log = VIS.Logging.VLogger.getVLogger('BankChargeSummaryMonthWidget');
        //Privilized function
        this.getRoot = function () {
            return $root;
        };
        /*Create Busy Indicator */
        function createBusyIndicator() {
            $bsyDiv = $('<div id="busyDivId_' + widgetID + '" class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap">' +
                '<i class= "vis_widgetloader"></i></div></div>').css({ 'width': 'calc(100% - 40px)' }).hide();
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
            $root = $("<div id='VA012_rootBankCharge_" + widgetID + "' class='VA012_rootBankCharge'></div>");
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
                        yrStartDate = data.Table[0].STARTDATE;
                        yrEndDate = data.Table[data.Table.length - 1].ENDDATE;
                        Year_ID = data.Table[0].C_YEAR_ID;
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
        //Create design
        function Design() {
            //Get year start and end date
            var startDate = getDates(yrStartDate);
            var endDate = getDates(yrEndDate);

            //Bank Account control validation
            var validation = "C_BankAccount.ISACTIVE='Y' AND C_BankAccount.C_BankAccount_ID IN (SELECT C_BankAccount_ID FROM C_BANKSTATEMENTLINE"
                + " WHERE TRUNC(STATEMENTLINEDATE) BETWEEN " + startDate + " AND " + endDate + ") ";

            /* parameters are: context, windowno., coloumn id, display type, DB coloumn name, Reference key, Is parent, Validation Code*/
            var lookup = VIS.MLookupFactory.get(VIS.context, 0, 0, VIS.DisplayType.TableDir, "C_BankAccount_ID", 0, false, validation);
            // Parameters are: columnName, mandatory, isReadOnly, isUpdateable, lookup,display length
            _cmbBankAccountCtrl = new VIS.Controls.VComboBox("C_BankAccount_ID", false, false, true, lookup, 50);

            //Charge control validation
            validation = "";
            validation = "C_Charge.ISACTIVE='Y' AND C_Charge.C_Charge_ID IN ( SELECT C_Charge_ID FROM C_BANKSTATEMENTLINE WHERE TRUNC(STATEMENTLINEDATE) BETWEEN " + startDate
                + " AND " + endDate + ")"
            var chargeLookup = VIS.MLookupFactory.get(VIS.context, 0, 0, VIS.DisplayType.TableDir, "C_Charge_ID", 0, false, validation);
            _cmbChargeCtrl = new VIS.Controls.VComboBox("C_Charge_ID", false, false, true, chargeLookup, 50);

            dropContainer = $('<div class="VA012-bankcharge-panel VA012-BankChargeContainer" id="VA012-BankChargeContainer_' + widgetID + '">'
                + '<div class="VA012-bankcharge-heading">'
                + '<h6 class= "VA012-bankchargePanelLbl">' + VIS.Msg.getMsg('VA012_BankChargeSummary') + '</h6></div>'
                //Start Parameters Div
                + '<div class="VA012-paramtersDiv" id="VA012-paramtersDiv_' + widgetID + '">'
                + '<div class="input-group vis-input-wrap VA012-input-wrap">'
                + '<div class="vis-control-wrap">'
                + '<div class="VA012-cmbBankAcct" id="VA012_cmbBankAcct_' + widgetID + '">'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="input-group vis-input-wrap VA012-input-wrap">'
                + '<div class="vis-control-wrap">'
                + '<div class="VA012-cmbCharge" id="VA012_cmbCharge_' + widgetID + '">'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
                //End Parameters div
                + '<div id="VA012-columnChart_' + widgetID + '" class="VA012-columnChart">'
                + '</div>'
                + '</div>'
                + '</div>'
            );
            $root.append(dropContainer);
            //Append bank account and Charge controls in root
            $root.find(".VA012-cmbBankAcct").append(_cmbBankAccountCtrl.getControl()).append('<label class="VA012-ctrlLbl">' + VIS.Msg.getMsg("VA012_BankAccount")
                + '<sup style="color: red;">*</sup></label>');
            $root.find(".VA012-cmbCharge").append(_cmbChargeCtrl.getControl()).append('<label class="VA012-ctrlLbl">' + VIS.Msg.getMsg("Charge") + '</label>');
            $root.find('select').addClass("VA012-selectCtrls");
            events();
        };

        function events() {
            _cmbBankAccountCtrl.fireValueChanged = function () {
                if (_cmbBankAccountCtrl.getValue()) {
                    $bsyDiv.show();
                    _cmbBankAccountCtrl.setValue(_cmbBankAccountCtrl.getValue());
                    GetCanvas();
                    $bsyDiv.hide();
                }
                else {
                    $root.find('canvas').remove();
                    _cmbChargeCtrl.setValue("");
                    VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                    return false;
                }
            };
            _cmbChargeCtrl.fireValueChanged = function () {
                if (_cmbChargeCtrl.getValue()) {
                    if (_cmbBankAccountCtrl.getValue()) {
                        C_Charge_ID = _cmbChargeCtrl.getValue();
                        _cmbChargeCtrl.setValue(_cmbChargeCtrl.getValue());
                        $bsyDiv.show();
                        GetCanvas();
                        $bsyDiv.hide();
                    }
                    else {
                        VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                        return false;
                    }
                }
                else {
                    C_Charge_ID = 0;
                    $bsyDiv.show();
                    GetCanvas();
                    $bsyDiv.hide();
                }
            };
        };
        function GetCanvas() {
            $.ajax({
                url: VIS.Application.contextUrl + "VA012_BankChargeSummary/GetBankChargeData",
                type: "GET",
                async: true,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: ({
                    C_BankAccount_ID: _cmbBankAccountCtrl.getValue(), C_Charge_ID: C_Charge_ID, yrStartDate: yrStartDate, yrEndDate: yrEndDate, Year_ID: Year_ID
                }),
                success: function (bankData) {
                    if (bankData != null && bankData != "") {
                        bankData = JSON.parse(bankData);
                        // Remove existing canvas if exists
                        $root.find('canvas').remove();

                        if (bankData != null && bankData.errorMessage != null) {
                            $root.find('#VA012-columnChart_' + widgetID).append('<div class="VA012-notfounddiv" id="VA012_norecordcont_' + widgetID + '">'
                                + VIS.Msg.getMsg("VA012_RecordNotFound") + '</div>')
                        }
                        else {
                            // Define static labels and colors
                            const labels = bankData.labels;
                            // Prepare the data object for the chart
                            const data = {
                                labels: labels, // Dynamic labels
                                datasets: [
                                    {
                                       // label: VIS.Msg.getMsg("VA012_ChargeAmt"),
                                        data: bankData.bankChargeData,
                                        borderColor: 'rgba(0,0,0,0)', // Transparent border color
                                        borderWidth: 0, // No border
                                       // backgroundColor: 'rgb(204, 0, 0, 0.6)',
                                        backgroundColor:'rgba(249, 179, 29, 1)',
                                        order: 1
                                    }
                                ],
                            };

                            // this is used to set the padding for legend
                            const plugin = {
                                beforeInit: function (chart) {
                                    const originalFit = chart.legend.fit;
                                    chart.legend.fit = function fit() {
                                        originalFit.bind(chart.legend)();
                                        this.height += -10;
                                    }
                                }
                            };

                            // Define the chart configuration for BAR / Line chart
                            const config = {
                                type: 'bar',
                                data: data,
                                options: {
                                    responsive: true,
                                    maintainAspectRatio: true,
                                    layout: {
                                        padding: 0
                                    },
                                    scales: {
                                        x: {
                                            grid: {
                                                display: true // Hide the grid lines on the x-axis   
                                            }
                                        },
                                        y: {
                                            grid: {
                                                display: false, // Hide the grid lines on the y-axis
                                            }
                                        },
                                        y1: {
                                            display: false,
                                            beginAtZero: false,
                                        },
                                    },
                                    plugins: {
                                        title: {
                                            display: true,
                                            text: " ",
                                            align: 'start',
                                            padding: {
                                                top: 0,
                                                bottom: 0
                                            }
                                        },
                                        legend: {
                                            display: false,
                                            position: 'bottom', // Positioning the legend on the right
                                            padding: {
                                                top: 0,
                                                bottom: 0
                                            },
                                        },
                                        tooltip: {
                                            callbacks: {
                                                label: function (tooltipItem) {
                                                    const dataIndex = tooltipItem.dataIndex;
                                                    const datasetIndex = tooltipItem.datasetIndex;
                                                    const dataset = tooltipItem.chart.data.datasets[datasetIndex];
                                                    const labels = tooltipItem.chart.data.labels;
                                                    const dsLabel = dataset.label;
                                                    const value = dataset.data[dataIndex];
                                                    return dsLabel + " - " + labels[dataIndex] + ': ' + value;
                                                }
                                            }
                                        },
                                        datalabels: {
                                            display: false,
                                            color: '#000',
                                            anchor: 'end',
                                            align: 'end',
                                            formatter: function (value) {
                                                return value; // Return value for external use only
                                            },
                                            font: {
                                                weight: 'bold'
                                            }
                                        }
                                    }
                                },
                                plugins: [plugin]
                            };

                            // Create a new canvas element and append it to the root
                            const canvas = $('<canvas class="Va012-columnChart-canvas"></canvas>');
                            var polarChart = $root.find('#VA012-columnChart_' + widgetID);
                            polarChart.append(canvas);

                            // Initialize the chart with the new data
                            const ctx = canvas[0].getContext('2d');
                            new Chart(ctx, config);
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
        /*this function is used to refresh design and data of widget*/
        this.refreshWidget = function () {
            _cmbBankAccountCtrl.setValue("");
            _cmbChargeCtrl.setValue("");
            C_Charge_ID = 0;
            $root.find('canvas').remove();
        };
        this.disposeComponents = function () {
            $self = null;
            $root = null;
            this.frame = null;
            this.windowNo = 0;
            this.widgetInfo = null;
            $bsyDiv = null;
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
    };
    // To change size of the form
    VA012.VA012_BankChargeSummary.prototype.widgetSizeChange = function (size) {
        // Widget info, we can save additional information in widget record
        var x = size;
    };
    // Must implement dispose
    VA012.VA012_BankChargeSummary.prototype.dispose = function () {
        /*CleanUp Code */
        //Dispose this component
        this.disposeComponent();
        //Call frame dispose function
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
    };
    VA012.VA012_BankChargeSummary.prototype.refreshWidget = function () {
        this.refreshWidget();
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