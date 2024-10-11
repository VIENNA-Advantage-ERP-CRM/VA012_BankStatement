; VA012 = window.VA012 || {};
; (function (VA012, $) {

    VA012.VA012_BankConnectWidget = function () {
        this.frame;
        this.windowNo;
        this.widgetInfo;
        var $bsyDiv;
        var $self = this;
        var $root = $('<div class="h-100 w-100">'); // Root container
        var createBank = null;
        var connectBank = null;

        var $cbDialog = null;
        var selectionParaDiv = null;
        var countryDialog = null;
        var connectViaDialog = null;
        var $CountryLookUpName = null;
        var $CountryDropdown = null;

        var $ConnectViaLookUpName = null;
        var $ConnectViaDropdown = null;

        /** Load Design */
        this.initalize = function () {
            widgetID = this.widgetInfo.AD_UserHomeWidgetID;

            var design = '<div class="vis-dynamicWidget-main" style="background-color: #e1f0fa; padding-top: 25px;">' +
                '<div class="vis-add-row">' +
                '<button id="VA012-bnkcnt-Create_' + widgetID + '" class="vis-widget-field VA012_bankCnt-NewBtn" index="0">' + VIS.Msg.getMsg("VA012_CreateBank") + '</button>' +
                '</div> ' +
                '<div class="vis-add-row">' +
                '<button id="VA012-bnkcnt-Connect_' + widgetID + '" class="vis-widget-field VA012_bankCnt-NewBtn" index="1">' + VIS.Msg.getMsg("VA012_ConnectBank") + '</button>' +
                '</div>' +
                '</div> ';

            // Create busy indicator
            createBusyIndicator();

            $root.append(design);
        };

        /** Find Element and Event handling  */
        this.intialLoad = function () {
            // Show busy indicator
            $bsyDiv.css('visibility', 'visible');

            FindElements();

            EventHandling();

            $bsyDiv.css('visibility', 'hidden');
        };

        /** Find Element */
        function FindElements() {
            createBank = $root.find("#VA012-bnkcnt-Create_" + widgetID);
            connectBank = $root.find("#VA012-bnkcnt-Connect_" + widgetID);
        };

        /** Event Handling */
        function EventHandling() {
            /* Open Bank Record */
            createBank.on("click", function (e) {
                var windowParam = {
                    "TabLayout": "Y",  // 'N'[Grid],'Y'[Single],'C'[Card]}	 	
                    "IsTabInNewMode": true,
                    "TabIndex": "0",
                }
                $self.widgetFirevalueChanged(windowParam);
            });

            /* Connect Via*/
            connectBank.on("click", function (e) {
                $bsyDiv.css('visibility', 'visible');
                window.setTimeout(function () {
                    ConnectBankDialog();
                }, 200);
            });
        };

        /** Connect Bank Dialog Design */
        function ConnectBankDialog() {
            $cbDialog = $("<div class='va012-popform-content'>");
            var _cbContent = '<div class="VA012_bankCnt-container">' +
                '<div class="VA012-bankCnt-selection" id="VA012-bnkcnt-Selection_' + widgetID + '">' +
                '<div class="input-group vis-input-wrap" id="VA012-bnkcnt-Country_' + widgetID + '"></div>' +
                '<div class="input-group vis-input-wrap" id="VA012-bnkcnt-ConnectVia_' + widgetID + '"></div></div>' +
                '<div class="VA012_bankCnt-header">' +
                /*Automatic Bank Feeds Supported Banks*/
                '<h2>' + VIS.Msg.getMsg("VA012_BankFeed") + '</h2>' +
                '<button>' + VIS.Msg.getMsg("VA012_ConnectNow") + '</button>' +
                '</div>' +
                /*Connect your bank accounts and fetch the bank feeds using one of our third-party bank feeds service providers.*/
                '<p>' + VIS.Msg.getMsg("VA012_BankConnectDesc") + '</p>' +
                '<div class="VA012_bankCnt-banks-grid">' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-iciciSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/icici.jpg" alt="ICICI Bank">' +
                '<span>' + VIS.Msg.getMsg("VA012_ICICIBank") + '</span>' +
                '</div>' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-hdfcSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/hdfc.jpg" alt="HDFC Bank">' +
                '<span>' + VIS.Msg.getMsg("VA012_HDFCBank") + '</span>' +
                '</div>' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-sbiSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/sbi.jpg" alt="State Bank of India">' +
                '<span>' + VIS.Msg.getMsg("VA012_SBIBank") + '</span>' +
                '</div>' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-kotakSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/kotak.png" alt="Kotak Mahindra Bank">' +
                '<span>' + VIS.Msg.getMsg("VA012_KotakBank") + '</span>' +
                '</div>' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-axisSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/axis.jpg" alt="Axis Bank">' +
                '<span>' + VIS.Msg.getMsg("VA012_AxisBank") + '</span>' +
                '</div>' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-hsbcSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/hsbc.jpg" alt="HDFC Bank Credit">' +
                '<span>' + VIS.Msg.getMsg("VA012_HSBC-CreditBank") + '</span>' +
                '</div>' +
                '<div class="VA012_bankCnt-bank-card" id="VA012-bnkcnt-otherSelected_' + widgetID + '">' +
                '<img src="Areas/VA012/Images/OtherBank.png" alt="Other Bank">' +
                '<span>' + VIS.Msg.getMsg("VA012_OtherBank") + '</span>' +
                '</div>' +
                '</div>' +
                '</div>';
            $cbDialog.append(_cbContent);

            DialogFindElements();

            DialogLoadControl();

            DialogEventHandling();

            var statementDialog = new VIS.ChildDialog();
            statementDialog.setContent($cbDialog);
            statementDialog.setTitle(VIS.Msg.getMsg("VA012_ConnectBank"));
            statementDialog.setWidth("780px");
            statementDialog.setEnableResize(false);
            statementDialog.setModal(true);
            statementDialog.show();
            statementDialog.hidebuttons();

            $bsyDiv.css('visibility', 'hidden');
        };

        /** Find Dialog Element */
        function DialogFindElements() {
            selectionParaDiv = $cbDialog.find("#VA012-bnkcnt-Selection_" + widgetID);
            countryDialog = $cbDialog.find("#VA012-bnkcnt-Country_" + widgetID);
            connectViaDialog = $cbDialog.find("#VA012-bnkcnt-ConnectVia_" + widgetID);
        };

        /**Load Dialog Control */
        function DialogLoadControl() {

            // Country Control
            var $CountryLbl = new VIS.Controls.VLabel(VIS.Msg.getMsg("VA012_Country_ID"), "C_Country_ID", false, true);
            var _CountryDivInputWrap = $('<div class="input-group vis-input-wrap">');
            var _CountryCtrlWrap = $('<div class="vis-control-wrap">');
            var _CountryInputGroupBtn = $('<div class="input-group-append">');
            //context, WindowNo,_columnid, Ad_reference_id, columnName, Ad_reference_value_ID, IsParent, validation Code
            $CountryLookUpName = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 866, VIS.DisplayType.Table, "C_Country_ID", 0, false, "");
            //ColumnName, Mandatory, isReadOnly, isUpdateable, lookup, displayLength, displayType, zoomWindow_ID
            $CountryDropdown = new VIS.Controls.VComboBox("C_Country_ID", true, false, true, $CountryLookUpName);
            $CountryDropdown.setMandatory(true);
            _CountryCtrlWrap.append($CountryDropdown.getControl()).append($CountryLbl.getControl());
            _CountryInputGroupBtn.append($CountryDropdown.getBtn(0));
            _CountryDivInputWrap.append(_CountryCtrlWrap);
            countryDialog.append(_CountryCtrlWrap).append(_CountryInputGroupBtn);

            // Connect Via Control
            var data = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "Widget/GetReference");
            var $ConnectViaLbl = new VIS.Controls.VLabel(VIS.Msg.getMsg("VA012_ConnectVia"), "VA012_ConnectVia", false, true);
            var _ConnectViaDivInputWrap = $('<div class="input-group vis-input-wrap">');
            var _ConnectViaCtrlWrap = $('<div class="vis-control-wrap">');
            var _ConnectViaInputGroupBtn = $('<div class="input-group-append">');
            //context, WindowNo,_columnid, Ad_reference_id, columnName, Ad_reference_value_ID, IsParent, validation Code
            $ConnectViaLookUpName = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 0, VIS.DisplayType.List, "VA012_ConnectVia",
                VIS.Utility.Util.getValueOfInt(data["BankConnectVia_Reference_ID"]), false, "");
            //ColumnName, Mandatory, isReadOnly, isUpdateable, lookup, displayLength, displayType, zoomWindow_ID
            $ConnectViaDropdown = new VIS.Controls.VComboBox("VA012_ConnectVia", true, false, true, $ConnectViaLookUpName, 50);
            $ConnectViaDropdown.setValue("01");
            _ConnectViaCtrlWrap.append($ConnectViaDropdown.getControl()).append($ConnectViaLbl.getControl());
            _ConnectViaInputGroupBtn.append($ConnectViaDropdown.getBtn(0));
            _ConnectViaDivInputWrap.append(_ConnectViaCtrlWrap);
            connectViaDialog.append(_ConnectViaCtrlWrap).append(_ConnectViaInputGroupBtn);
        };

        /** Dialog Event Handling */
        function DialogEventHandling() {
            $cbDialog.on('click', '#VA012-bnkcnt-iciciSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

            $cbDialog.on('click', '#VA012-bnkcnt-hdfcSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

            $cbDialog.on('click', '#VA012-bnkcnt-sbiSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

            $cbDialog.on('click', '#VA012-bnkcnt-kotakSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

            $cbDialog.on('click', '#VA012-bnkcnt-axisSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

            $cbDialog.on('click', '#VA012-bnkcnt-hsbcSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

            $cbDialog.on('click', '#VA012-bnkcnt-otherSelected_' + widgetID, function () {
                /* Remove Classes */
                $cbDialog.find(".VA012_bankCnt-banks-grid div").removeClass("VA012-bankCnt-bnkselected");
                /* Add Class */
                $(this).addClass("VA012-bankCnt-bnkselected");
            });

        };

        /* This function is used to create the busy indicator */
        function createBusyIndicator() {
            $bsyDiv = $('<div class="vis-busyindicatorouterwrap"><div class="vis-busyindicatorinnerwrap"><i class="vis_widgetloader"></i></div></div>');
            $root.append($bsyDiv);
        };

        /**Root */
        this.getRoot = function () {
            return $root;
        };

        /* This function is used to refresh the widget data */
        this.refreshWidget = function () {
            $self.intialLoad();
        };
    };

    VA012.VA012_BankConnectWidget.prototype.init = function (windowNo, frame) {
        this.frame = frame;
        this.widgetInfo = frame.widgetInfo;
        this.windowNo = windowNo;
        this.initalize();
        this.frame.getContentGrid().append(this.getRoot());
        var ssef = this;
        window.setTimeout(function () {
            ssef.intialLoad();
        }, 50);
    };


    VA012.VA012_BankConnectWidget.prototype.widgetFirevalueChanged = function (value) {
        if (this.listener)
            this.listener.widgetFirevalueChanged(value);
    };

    VA012.VA012_BankConnectWidget.prototype.addChangeListener = function (listener) {
        this.listener = listener;
    };

    VA012.VA012_BankConnectWidget.prototype.widgetSizeChange = function (widget) {
        this.widgetInfo = widget;
    };


    VA012.VA012_BankConnectWidget.prototype.refreshWidget = function () {
        this.refreshWidget();
    };

    VA012.VA012_BankConnectWidget.prototype.dispose = function () {
        this.frame = null;
        this.windowNo = null;
        $bsyDiv = null;
        $self = null;
        $root = null;
        createBank = null;
        connectBank = null;
    };

})(VA012, jQuery);
