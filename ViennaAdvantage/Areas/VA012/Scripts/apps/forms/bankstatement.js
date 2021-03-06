﻿; VA012 = window.VA012 || {};
; (function (VA012, $) {
    function bankStatement() {

        //Variable Declaration
        this.windowNo;
        $self = this;
        var $root = $("<div/>");
        this.frame;
        var $BusyIndicator = null;
        var _cmbBank = null;
        var _cmbBankAccount = null;
        var _cmbBankAccountClasses = null;
        var _cmbBankAccountCharges = null;
        // declare varribale for Statement Date
        var _statementDate = null;
        var _btnLoadStatement = null;
        var _btnMatchStatement = null;
        var _cmbSearchPaymentMethod = null;
        var _cmbTransactionType = null;
        var _lstStatement = null;
        var _lstPayments = null;
        var _paymentLists = null;
        var _statementID = "";
        var _secReconciled = null;
        var _secUnreconciled = null;
        var _currencyCode = "";
        var _clientBaseCurrency = null;
        var _clientBaseCurrencyID = null;
        var _SEARCHREQUEST = false;
        var _statementLinesList = [];
        var _scheduleList = [];
        var _scheduleDataList = [];
        /* change by pratap*/
        var _scheduleAmount = [];
        /* change by pratap*/
        var _prepayList = [];
        var _prepayDataList = [];
        var _btnUnmatch = null;
        var _btnProcess = null;
        var _btnHide = null;
        var _tdLeft = null;
        var _table = null;
        var _currencyId = 0;
        var _stdPrecision = 2;
        var _openingFromDrop = false;
        var _openingFromEdit = false;
        var _paymentPageNo = 1;
        var _paymentPageSizeInc = 1;
        var _statementPageNo = 1;
        var _PAGESIZE = 50;
        var _paymentPAGESIZE = 50;
        var C_BANKACCOUNT_ID = 0
        var C_BANK_ID = 0;
        var VA012_BANKSTATEMENTCLASS_ID = 0;

        var _VA012_BankChargeDiv = null;

        //newRecord Form Variables
        var $_formNewRecord = null;
        var $_formBtnNewRecord = null;
        var _txtStatementNo = null;
        var _btnStatementNo = null;
        var _txtStatementPage = null;
        var _txtStatementLine = null;
        var _dtStatementDate = null;
        var _cmbPaymentMethod = null;
        // var _cmbCurrency = null;
        var _cmbContraType = null;
        var _cmbCashBook = null;
        var _cmbTransferType = null;
        var _txtCheckNo = null;
        var _cmbVoucherMatch = null;
        var _txtAmount = null;
        var _txtTrxAmt = null;
        var _txtDifference = null;
        var _cmbDifferenceType = null;
        var _txtDescription = null;
        var _txtVoucherNo = null;
        var _cmbCharge = null;
        var _txtCharge = null;
        var _cmbTaxRate = null;
        var _txtTaxAmount = null;
        var _ctrlCashLine = null;
        var _ctrlOrder = null;
        var _ctrlPayment = null;
        var _ctrlInvoice = null;
        var _ctrlBusinessPartner = null;
        var _chkUseNextTime = null;
        var _btnSave = null;
        var _btnPaymentSchedule = null;
        var _btnPrepay = null;
        var _txtPaymentSchedule = null;
        var _txtPrepayOrder = null;
        //var _btnCreatePayment = null;
        var _btnNewRecord = null;
        var _btnUndo = null;
        var _btnDelete = null;
        var _divVoucher = null;
        var _divMatch = null;

        var _divContraType = null;
        var _divCashBook = null;
        var _divTransferType = null;
        var _divCheckNo = null;
        var _divVoucherNo = null;
        var _divTrxAmt = null;
        var _divDifference = null;
        var _divDifferenceType = null;
        var _divCharge = null;
        var _btnCharge = null;
        var _divTaxRate = null;
        var _divTaxAmount = null;
        var _divCtrlPayment = null;
        var _divCtrlInvoice = null;
        var _divCtrlBusinessPartner = null;
        var _divPrepayOrder = null;
        var _divPaymentSchedule = null;
        var _txtSearch = null;
        var _btnSearch = null;
        var _btnMore = null;
        var _divMore = null;
        var _btnAmount = null;
        var _btnIn = null;
        var _btnOut = null;
        var _statementPageCount = 0;
        var _paymentPageCount = 0;
        var _lookupOrder = null;
        var $_ctrlOrder = null;
        var _orderSelectedVal = null;

        //CashLine Control Variables
        var _lookupCashLine = null;
        var $_ctrlCashLine = null;
        var _cashLineSelectedVal = null;
        //CashLine Control Variables

        //Payment Control Variables
        var _lookupPayment = null;
        var $_ctrlPayment = null;
        var _paymentSelectedVal = null;
        var _draggedPaymentID = null;
        //Payment Control Variables

        //Business Partner Control Variables
        var _lookupBusinessPartner = null;
        var $_ctrlBusinessPartner = null;
        var _bPartnerSelectedVal = null;
        //End Business Partner Control Variables

        //Invoice Control Variables
        var _lookupInvoice = null;
        var $_ctrlInvoice = null;
        var _invoiceSelectedVal = null;
        //Invoice Control Variables

        // End newRecord Form Variables
        //store payment list in array
        storepaymentdata = [];

        //End Variable Declaration

        //Syatem Date
        var now = new Date();
        var _today = now.getFullYear() + "-" + (("0" + (now.getMonth() + 1)).slice(-2)) + "-" + (("0" + now.getDate()).slice(-2));
        //
        var _maxStatement = "";

        var $divMatchStatementGridPopUp;
        var $GrdPayment;
        // var $CmbChargeType;
        var _chargeSrch;
        var $CmbTaxRate;
        //  var cartGrid = null;

        this.Initialize = function () {

            loadRoot(loadFunctions.loadFormDesign());
            _txtTrxAmt.addVetoableChangeListener(this);
            _txtTaxAmount.addVetoableChangeListener(this);
            _txtAmount.addVetoableChangeListener(this);
            _txtDifference.addVetoableChangeListener(this);

            //$.ajax({
            //    url: VIS.Application.contextUrl + "VA012/BankStatement/Index",
            //    type: 'Get',
            //    async: false,
            //    data: {
            //        windowno: $self.windowNo
            //    },
            //    success: function (data) {
            //        if (data != null) {
            //            loadRoot(data);
            //        }
            //    },
            //    error: function (data) {
            //        VIS.ADialog.info(data, null, "", "");
            //    },
            //});


        };
        function loadRoot(data) {
            if (data != null && data != "") {
                $root = $(data);

                loadFunctions.getControls();
                loadFunctions.setBankAndAccount();

                //loadFunctions.loadBank();
                //loadFunctions.loadBankAccount();


                //loadFunctions.loadSearchPaymentMethod();

                newRecordForm.newRecord();
                //loadFunctions.getMaxStatement();
                loadFunctions.getBaseCurrency();
                loadFunctions.loadPayments("0", "0", "PY", _statementDate.val());

                InitializeEvents();

                loadFunctions.dragPayments();
                loadFunctions.dropPayments();


            }
        };
        function InitializeEvents() {

            _btnHide.on(VIS.Events.onTouchStartOrClick, function (e) {
                e.stopPropagation();
                var w = _tdLeft.width();
                if (w > 50) {
                    $(".va012-left-content").hide();
                }

                _tdLeft.animate({
                    "width": w > 50 ? 40 : 200
                }, 300, 'swing', function () {

                    if (w < 50) {
                        $(".va012-left-content").show();
                    }
                });
            });

            _cmbBank.on('change', function () {

                _cmbBankAccount.val('0');
                loadFunctions.loadBankAccount();
            });
            _cmbBankAccount.on('change', function () {

                //called loadPayment to update the data based on BankAccount
                newRecordForm.loadPayment();
                newRecordForm.loadCurrency();
                newRecordForm.loadCashLine();
                _lstPayments.html("");
                newRecordForm.scheduleRefresh();
                newRecordForm.prepayRefresh();
                newRecordForm.refreshForm();
                _paymentPageNo = 1;
                storepaymentdata = [];
                loadFunctions.loadPayments(_cmbBankAccount.val() == null ? 0 : _cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                _statementLinesList = [];
                _lstStatement.html("");
                _statementPageNo = 1;
                childDialogs.loadStatement(_statementID);

            });
            _cmbSearchPaymentMethod.on('change', function () {

                _lstPayments.html("");
                // newRecordForm.scheduleRefresh();
                // newRecordForm.prepayRefresh();
                _paymentPageNo = 1;
                storepaymentdata = [];
                loadFunctions.loadPayments(_cmbBankAccount.val() == null ? 0 : _cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());

            });
            _cmbTransactionType.on('change', function () {

                if (_cmbTransactionType.val() == 'CO') {
                    _cmbSearchPaymentMethod.prop('selectedIndex', 0);
                    _cmbSearchPaymentMethod.prop('disabled', true);
                }
                else {
                    _cmbSearchPaymentMethod.prop('selectedIndex', 0);
                    _cmbSearchPaymentMethod.prop('disabled', false);
                }

                _lstPayments.html("");
                newRecordForm.scheduleRefresh();
                newRecordForm.prepayRefresh();
                newRecordForm.refreshForm();
                _paymentPageNo = 1;
                //Used to handle the Scrolling for Transactions
                _paymentPageCount = 0;
                _paymentPAGESIZE = 50;
                _paymentPageSizeInc = 1;
                storepaymentdata = [];
                loadFunctions.loadPayments(_cmbBankAccount.val() == null ? 0 : _cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());

            });
            _btnLoadStatement.on(VIS.Events.onTouchStartOrClick, function () {
                if (_statementDate.val() == "") {
                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_PleaseEnterStatementDate"), null, "", "");
                }
                else
                    childDialogs.statementDialog();
            });
            _btnMatchStatement.on(VIS.Events.onTouchStartOrClick, function () {

                //childDialogs.matchStatementGridDialog();
                childDialogs.matchStatementDialog();
            });
            _btnPaymentSchedule.on(VIS.Events.onTouchStartOrClick, function () {

                childDialogs.paymentScheduleDialog();
            });
            _btnPrepay.on(VIS.Events.onTouchStartOrClick, function () {

                childDialogs.prepayOrderDialog();
            });
            _btnSearch.on(VIS.Events.onTouchStartOrClick, function () {

                if (_txtSearch.val() != null && _txtSearch.val() != "") {
                    _SEARCHREQUEST = true;
                    _statementLinesList = [];
                    _lstStatement.html("");
                    _statementPageNo = 1;
                    childDialogs.loadStatement(_statementID);

                    //_SEARCHREQUEST = false;
                }
                else {
                    _statementLinesList = [];
                    _lstStatement.html("");
                    _statementPageNo = 1;
                    childDialogs.loadStatement(_statementID);
                }
            });
            _txtSearch.keypress(function (e) {
                if (e.which == 13) {

                    if (_txtSearch.val() != null && _txtSearch.val() != "") {
                        _SEARCHREQUEST = true;
                        _statementLinesList = [];
                        _lstStatement.html("");
                        _statementPageNo = 1;
                        childDialogs.loadStatement(_statementID);

                        //_SEARCHREQUEST = false;
                    }
                    else {
                        _statementLinesList = [];
                        _lstStatement.html("");
                        _statementPageNo = 1;
                        childDialogs.loadStatement(_statementID);

                    }
                }
            });
            _btnUnmatch.on(VIS.Events.onTouchStartOrClick, function () {

                if (_statementLinesList.length > 0) {
                    $.ajax({
                        type: 'POST',
                        url: VIS.Application.contextUrl + "VA012/BankStatement/UnmatchStatement",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ _statementLinesList: _statementLinesList.toString() }),
                        success: function (data) {

                            if (data != null && data != "") {
                                data = $.parseJSON(data);
                                if (data[0]._status == "Success") {
                                    if (data[0]._statementOk != null) {
                                        VIS.ADialog.info(data[0]._statementOk + " " + VIS.Msg.getMsg("VA012_StatementsUnmatched"), null, "", "");
                                    }
                                }
                                if (data[0]._error != null) {
                                    VIS.ADialog.info(data[0]._error, null, "", "");
                                }
                                if (data[0]._statementNo != null) {
                                    VIS.ADialog.info(data[0]._statementNo + " " + VIS.Msg.getMsg("VA012_CompletedRecord"), null, "", "");
                                }
                                if (data[0]._statementNoNotUpdate != null) {
                                    VIS.ADialog.info(data[0]._statementNoNotUpdate + " " + VIS.Msg.getMsg("VA012_ErrorSaving"), null, "", "");
                                }

                                newRecordForm.scheduleRefresh();
                                newRecordForm.prepayRefresh();
                                newRecordForm.refreshForm();
                                _statementLinesList = [];
                                storepaymentdata = [];
                                _lstStatement.html("");
                                _statementPageNo = 1;
                                childDialogs.loadStatement(_statementID);
                                _lstPayments.html("");
                                newRecordForm.scheduleRefresh();
                                newRecordForm.prepayRefresh();
                                _paymentPageNo = 1;
                                loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                            }

                        },
                        error: function (data) { VIS.ADialog.info(data, null, "", ""); }
                    });
                }
                else {
                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_NoRecordSelected"), null, "", "");
                }

            });

            ///Process
            _btnProcess.on(VIS.Events.onTouchStartOrClick, function () {

                if (_cmbBankAccount.val() == null || _cmbBankAccount.val() == "" || _cmbBankAccount.val() == "0") {
                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_SelectBankAccountFirst"), null, "", "");
                    return;
                }
                //if (_statementLinesList.length > 0) {
                else {
                    $.ajax({
                        type: 'POST',
                        url: VIS.Application.contextUrl + "VA012/BankStatement/ProcessStatement",
                        contentType: "application/json; charset=utf-8",
                        data: JSON.stringify({ _statementLinesList: _statementLinesList.toString(), _accountID: _cmbBankAccount.val() }),
                        success: function (data) {
                            if (data != null && data != "") {
                                data = $.parseJSON(data);
                                //if (data[0]._status == "Success") {
                                //    if (data[0]._statementProcessed != null) {
                                //        VIS.ADialog.info(data[0]._statementProcessed + " " + VIS.Msg.getMsg("VA012_StatementsProcessed"), null, "", "");
                                //    }
                                //}
                                if (data[0]._statementProcessed != null) {
                                    VIS.ADialog.info(data[0]._statementProcessed + " " + VIS.Msg.getMsg("VA012_StatementsProcessed"), null, "", "");
                                }
                                if (data[0]._error != null) {
                                    VIS.ADialog.info(data[0]._error, null, "", "");
                                }
                                if (data[0]._statementNotProcessed != null) {
                                    VIS.ADialog.info(data[0]._statementNotProcessed + " " + VIS.Msg.getMsg("VA012_StatementsNotProcessed"), null, "", "");
                                }
                                if (data[0]._statementUnmatchedLines != null) {
                                    VIS.ADialog.info(data[0]._statementUnmatchedLines + " " + VIS.Msg.getMsg("VA012_ExistsUnmatched"), null, "", "");
                                }
                                newRecordForm.scheduleRefresh();
                                newRecordForm.prepayRefresh();
                                newRecordForm.refreshForm();
                                storepaymentdata = [];
                                _statementLinesList = [];
                                _lstStatement.html("");
                                _statementPageNo = 1;
                                childDialogs.loadStatement(_statementID);

                                _lstPayments.html("");
                                newRecordForm.scheduleRefresh();
                                newRecordForm.prepayRefresh();
                                _paymentPageNo = 1;
                                loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                            }

                        },
                        error: function (data) { VIS.ADialog.info(data, null, "", ""); }
                    });
                }
                //else {
                //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_NoRecordSelected"), null, "", "");
                //}
            });
            ///
            ///Delete
            _btnDelete.on(VIS.Events.onTouchStartOrClick, function () {
                if (_statementLinesList.length > 0 || parseInt($_formNewRecord.attr("data-uid")) > 0) {
                    if (VIS.ADialog.ask(VIS.Msg.getMsg("VA012_WantToDelete"))) {
                        $.ajax({
                            type: 'POST',
                            url: VIS.Application.contextUrl + "VA012/BankStatement/DeleteStatement",
                            contentType: "application/json; charset=utf-8",
                            data: JSON.stringify({ _statementLinesList: _statementLinesList.toString(), _statementLineID: $_formNewRecord.attr("data-uid") }),
                            success: function (data) {
                                if (data != null && data != "") {
                                    data = $.parseJSON(data);
                                    if (data[0]._status == "Success") {
                                        if (data[0]._statementProcessed != null) {
                                            VIS.ADialog.info(data[0]._statementProcessed + " " + VIS.Msg.getMsg("VA012_StatementsDeleted"), null, "", "");
                                        }
                                    }
                                    if (data[0]._statementNotProcessed != null) {
                                        VIS.ADialog.info(data[0]._statementNotProcessed + " " + VIS.Msg.getMsg("VA012_StatementsNotDeleted"), null, "", "");
                                    }
                                    if (data[0]._error != null) {
                                        VIS.ADialog.info(data[0]._error, null, "", "");
                                    }

                                    newRecordForm.scheduleRefresh();
                                    newRecordForm.prepayRefresh();
                                    newRecordForm.refreshForm();
                                    _statementLinesList = [];
                                    storepaymentdata = [];
                                    _lstStatement.html("");
                                    _statementPageNo = 1;
                                    childDialogs.loadStatement(_statementID);
                                    _lstPayments.html("");
                                    newRecordForm.scheduleRefresh();
                                    newRecordForm.prepayRefresh();
                                    _paymentPageNo = 1;
                                    loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                                }

                            },
                            error: function (data) { VIS.ADialog.info(data, null, "", ""); }
                        });

                    }
                }
                else {
                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_NoRecordSelected"), null, "", "");
                }
            });

            _lstPayments.on("scroll", loadFunctions.paymentScroll);
            _lstStatement.on("scroll", loadFunctions.statementScroll);


            //Call Edit Button Click
            _lstStatement.on(VIS.Events.onTouchStartOrClick, childDialogs.statementListEdit);
            //call info button
            _lstStatement.on(VIS.Events.onTouchStartOrClick, childDialogs.openStatement);
            _lstStatement.on(VIS.Events.onTouchStartOrClick, childDialogs.selectedStatementLinesList);
            _lstPayments.on(VIS.Events.onTouchStartOrClick, childDialogs.selectedScheduleList);
            _lstPayments.on(VIS.Events.onTouchStartOrClick, function (e) {

                // when we click on div, mark checkbox as True
                if (e.target.type != "checkbox") {
                    if ($(e.target).closest(".row").find(':checkbox').is(':checked')) {
                        $(e.target).closest(".row").find(":checkbox").prop('checked', false);
                    }
                    else {
                        $(e.target).closest(".row").find(":checkbox").prop('checked', true);
                    }
                }

                _txtAmount.getControl().trigger('blur');
            });
            _statementDate.addClass("va012-mandatory");
            //Change event of Statement Date Filter
            _statementDate.on('change', function (e) {
                //on change event of Statement Date to set background color logic
                if (_statementDate.val() == "") {
                    _statementDate.addClass("va012-mandatory");
                }
                else {
                    if (new Date(_statementDate.val()) > new Date()) {
                        VIS.ADialog.info(VIS.Msg.getMsg("VA012_StatementDateToday"), null, "", "");
                        _statementDate.val("");
                        return false;
                    }
                    _statementDate.removeClass("va012-mandatory");
                }
                storepaymentdata = [];
                loadFunctions.loadPayments(_cmbBankAccount.val() == null ? 0 : _cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
            });
            //_cmbBankAccountClasses.on('change', function (obj) {
            //    var str = obj.value;
            //    var clsName = str.substr(0, str.indexOf("_"));
            //    if (clsName.toLowerCase() == "va012.models.va012_trxno.importstatement")
            //        document.getElementById('VA012_BankChargeDiv').style.display = "block";
            //});            
        };

        //Load All Functions
        var loadFunctions = {

            loadFormDesign: function () {

                _formDesign = $('<div class="va012-assign-content">');
                divContainer = $('  <div id="VA012_mainContainer_' + $self.windowNo + '" class="va012-main-container">');
                divTable = $('<table id="VA012_table_' + $self.windowNo + '" style="width: 100%;">');// splitted table tags into $variables
                tableTr = $('<tr>');
                tableTd = $('<td id="VA012_tdLeft_' + $self.windowNo + '" style="width: 200px;position: relative;">'
                    + '   <div class="va012-left-part">'
                    + '              <div class="va012-left-title">'
                    + '                  <h4>'
                    //+ '                      <img id="VA012_btnHide_' + $self.windowNo + '" src="Areas/VA012/Images/lines.png" alt="lines" style = "cursor: pointer;" ></h4>'
                    + '    <i id="VA012_btnHide_' + $self.windowNo + '" class="fa fa-bars" alt="lines" style="cursor: pointer;background: rgba(var(--v-c-primary),1);"></i> </h4>'
                    + '              </div>'
                    + '              <div class="va012-left-content">'
                    + '                  <div class="va012-left-data">'
                    + '                      <label  id="VA012_lblBank_' + $self.windowNo + '" >' + VIS.Msg.getMsg("VA012_Bank") + '<sup style="color: red;">*</sup></label>'
                    + '                      <select id="VA012_cmbBank_' + $self.windowNo + '" ></select>'
                    + '                  </div>'
                    + '                  <!-- end of left-data -->'
                    + '  '
                    + '                  <div class="va012-left-data">'
                    + '                      <label  id="VA012_lblBankAccount_' + $self.windowNo + '" >' + VIS.Msg.getMsg("VA012_BankAccount") + '<sup style="color: red;">*</sup></label>'
                    + '                      <select id="VA012_cmbBankAccount_' + $self.windowNo + '"   ></select>'
                    + '                  </div>'
                    + '                  <!-- end of left-data -->'
                    /*Added new parameter Statement Date*/
                    + '                  <div class="va012-left-data">'
                    + '                      <label  id="VA012_lblStatementDate_' + $self.windowNo + '" >' + VIS.Msg.getMsg("VA012_StatementDate") + '<sup style="color: red;">*</sup></label>'
                    + '                      <input type="date" max="9999-12-31" id="VA012_statementDate_' + $self.windowNo + '"   >'
                    + '                  </div>'
                    + '                  <!-- end of left-data -->'
                    /*End*/
                    + '  '
                    + '                  <div class="va012-left-data">'
                    + '                      <a id="VA012_btnLoadStatement_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA012_LoadStatement") + '</a>'
                    + '                      <a id="VA012_btnMatchStatement_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA012_MatchStatement") + '</a>'
                    + '                  </div>'
                    + '                  <!-- end of left-data -->'
                    + '              </div>'
                    + '              <!-- end of left-content -->'
                    + '          </div>'
                    + '          <!-- end of left-part -->'
                    + ' </td>');
                tableTd1 = $('<td style="position: relative;">');
                contentDiv = $('<div id="VA012_contentArea_' + $self.windowNo + '" class="va012-content-area" style="position: absolute;" >');
                divMidWrap = $('<div id="VA012_middleWrap_' + $self.windowNo + '" class="va012-middle-wrap">');
                divtopWrap = $('<div class="va012-mid-top-wrap" id="VA012_formBtnNewRecord_' + $self.windowNo + '">'
                    + '                      <div class="va012-icons-wrap">'
                    //+ '                          <span>'
                    //+ '                              <img class="va012-delete" alt="delete" id="VA012_btnDelete_' + $self.windowNo + '"></span>'
                    + '                          <span>'
                    //+ '                              <img class="va012-undo" alt="undo" title="Undo" id="VA012_btnUndo_' + $self.windowNo + '"></span>'
                    + '                              <i class="va012-undo vis vis-ignore" alt="undo" title="Undo" id="VA012_btnUndo_' + $self.windowNo + '"></i></span>'
                    + '                          <span>'
                    //+ '                              <img class="va012-hide-show-newform" src="Areas/VA012/Images/add.png" activestatus="0" alt="add" title = "Expand" id="VA012_btnNewRecord_' + $self.windowNo + '"></span>'
                    + '                              <i class="va012-hide-show-newform vis vis-plus" activestatus="0" alt="add" title = "Expand" id="VA012_btnNewRecord_' + $self.windowNo + '"></i></span>'
                    + '                      </div>'
                    + '                      <!-- end of icons-wrap -->'
                    + '                  </div>');
                //+ '                  <!-- end of mid-top-wrap -->'
                //+ '  '
                divformWrap = $('<div class="va012-form-wrap va012-newform" id="VA012_formNewRecord_' + $self.windowNo + '" data-uid="0" style="height:55%;overflow-y:auto;width:101%;">');
                divRow1 = $('<div class="row va012-fl-padd" style="width:102%">'
                    + '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_StatementNumber") + ' <sup style="color: red;">*</sup></label>'
                    + '                                  <input  tabindex="1" id="VA012_txtStatementNo_' + $self.windowNo + '" type="text" class="va012-input-size">'
                    + '                                  <i  id="VA012_btnStatementNo_' + $self.windowNo + '" class="fa fa-plus va012-add-icon"></i>'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_StatementPage") + '</label>'
                    + '                                  <input tabindex="2" value="1" id="VA012_txtStatementPage_' + $self.windowNo + '" type="text">'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_StatementLine") + '</label>'
                    + '                                  <input tabindex="3" value="10" id="VA012_txtStatementLine_' + $self.windowNo + '" type="text">'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->'
                    + '  ');
                divRow2 = $('<div class="row va012-fl-padd" style="width:102%">');
                row2Col1 = $('<div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_StatementDate") + '<sup style="color: red;">*</sup></label>'
                    + '                                  <input tabindex="4" id="VA012_dtStatementDate_' + $self.windowNo + '" type="date" max="9999-12-31">'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->');
                row2Col2 = $('<div class="col-md-4 col-sm-4 va012-padd-0">');
                row2Col2divAmt = $('<div class="va012-form-group va012-form-data" >');
                row2Col2Lble = $('<label>' + VIS.Msg.getMsg("VA012_Amount") + '<sup style="color: red;">*</sup></label>');
                row2Col2btnIn = $('<a tabindex="5" id="VA012_btnIn_' + $self.windowNo + '" v_active="1" class="va012-inout-icon va012-active">In</a>');
                row2Col2btnOut = $('<a tabindex="6" id="VA012_btnOut_' + $self.windowNo + '" v_active="0" class="va012-inout-icon va012-inactive">Out</a>');
                _txtAmount = new VIS.Controls.VAmountTextBox("VA012_txtAmount_" + $self.windowNo + "", false, false, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                _txtAmount.setValue(0);
                _txtAmount.getControl().addClass('va012-input-size-amt va012-right-align va012-txtamount');
                //$('<input tabindex="7" autofocus  value="0.00" id="VA012_txtAmount_' + $self.windowNo + '" type="number" class="va012-input-size-amt va012-right-align va012-txtamount">'
                row2Col2btnIcon = $('<a id="VA012_btnAmount_' + $self.windowNo + '" class="va012-info-icon"></a>');
                //+ '                              </div>'
                //+ '                              <!-- end of form-group -->'
                //+ '                          </div>'
                //+ '                          <!-- end of col -->');
                //+ '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                //+ '                              <div class="va012-form-group va012-form-data">'
                //+ '                                  <label>' + VIS.Msg.getMsg("VA012_Currency") + '</label>'
                //+ '                                  <select id="VA012_cmbCurrency_' + $self.windowNo + '">'
                ////+ '                                  <label>' + VIS.Msg.getMsg("VA012_PaymentMethod") + '</label>'
                ////+ '                                  <select id="VA012_cmbPaymentMethod_' + $self.windowNo + '">'
                //+ '                                  </select>'
                //+ '                              </div>'
                //+ '                              <!-- end of form-group -->'
                //+ '                          </div>'
                //+ '                          <!-- end of col -->'
                row2Col3 = $('<div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_VoucherMatch") + '<sup style="color: red;">*</sup></label>'
                    + '                                  <select tabindex="8" id="VA012_cmbVoucherMatch_' + $self.windowNo + '">'
                    + '                                  </select>'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->'
                    + '  ');
                divRow3 = $('                      <div class="row va012-fl-padd" style="width:102%">'
                    //+ '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    //+ '                              <div class="va012-form-group va012-form-data" >'
                    //+ '                                  <label>' + VIS.Msg.getMsg("VA012_Amount") + '</label>'
                    //+ '                                  <input value="0" id="VA012_txtAmount_' + $self.windowNo + '" type="number" class="va012-input-size">'
                    //  + '                                <a id="VA012_btnAmount_' + $self.windowNo + '" class="va012-info-icon"></a>'
                    //+ '                              </div>'
                    //+ '                              <!-- end of form-group -->'
                    //+ '                          </div>'
                    //+ '                          <!-- end of col -->'

                    //+ '                          <div class="col-md-2 col-sm-2 va012-padd-0">'
                    //+ '                              <div class="va012-form-group va012-form-data">'
                    //+ '                                  <label>' + VIS.Msg.getMsg("VA012_Currency") + '</label>'
                    //+ '                                  <select id="VA012_cmbCurrency_' + $self.windowNo + '">'
                    //+ '                                  </select>'
                    //+ '                              </div>'
                    //+ '                              <!-- end of form-group -->'
                    //+ '                          </div>'
                    //+ '                          <!-- end of col -->'
                    + '                          <div class="col-md-8 col-sm-8 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_Description") + '</label>'
                    + '                                  <input tabindex="9" id="VA012_txtDescription_' + $self.windowNo + '" type="text">'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div id="VA012_divVoucherNo_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_VoucherNo") + '</label>'
                    + '                                  <input tabindex="9" id="VA012_txtVoucherNo_' + $self.windowNo + '" type="text">'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->'
                    + '  ');

                //TrxAmount
                divRow4 = $('<div class="row va012-fl-padd" style="width:102%">');
                //  + '                          <div style="padding-left: 7px;padding-right: 7px;">'
                divRow4Col1 = $('<div class="col-md-4 col-sm-4 va012-padd-0">');
                divRow4Col1TrxAmt = $('<div id="VA012_divTrxAmt_' + $self.windowNo + '" class="va012-form-group va012-form-data">');
                divRow4Col1Lbl = $('<label>' + VIS.Msg.getMsg("VA012_TrxAmt") + '</label>');
                _txtTrxAmt = new VIS.Controls.VAmountTextBox("VA012_txtTrxAmt_" + $self.windowNo + "", false, true, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                _txtTrxAmt.getControl().addClass('va012-right-align');
                _txtTrxAmt.setValue((0).toFixed(_stdPrecision));
                divRow4Col1TrxAmt.append(divRow4Col1Lbl).append(_txtTrxAmt.getControl());
                divRow4Col1.append(divRow4Col1TrxAmt);
                //+ '                                   <input disabled tabindex="9" id="VA012_txtTrxAmt_' + $self.windowNo + '" type="number" class="va012-right-align">'
                // + '                                 </div>'
                //+ '                                  <!-- end of form-group -->'
                //+ '                              </div>'
                // + '                              <!-- end of col -->');

                divRow4Col2 = $('<div class="col-md-4 col-sm-4 va012-padd-0">');
                divRow4Col2Diff = $('<div id="VA012_divDifference_' + $self.windowNo + '" class="va012-form-group va012-form-data">');
                divRow4Col2DiffLbl = $('<label>' + VIS.Msg.getMsg("VA012_Difference") + '</label>');
                _txtDifference = new VIS.Controls.VAmountTextBox("VA012_txtDifference_" + $self.windowNo + "", false, true, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                _txtDifference.getControl().addClass('va012-right-align');
                _txtDifference.setValue(0);
                divRow4Col2Diff.append(divRow4Col2DiffLbl).append(_txtDifference.getControl());
                divRow4Col2.append(divRow4Col2Diff);
                //$('                                   <input disabled tabindex="9" vchangable="Y" id="VA012_txtDifference_' + $self.windowNo + '" type="number" class="va012-right-align">'
                //+ '                                 </div>'
                //+ '                                  <!-- end of form-group -->'
                //+ '                              </div>'
                //+ '                              <!-- end of col -->');
                divRow4Col3 = $('<div class="col-md-4 col-sm-4 va012-padd-0">');
                divRow4Col3DiffType = $('<div id="VA012_divDifferenceType_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                  <label>' + VIS.Msg.getMsg("VA012_DifferenceType") + '</label>'
                    + '                                  <select tabindex="9" id="VA012_cmbDifferenceType_' + $self.windowNo + '">'
                    + '                                  </select>'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->');
                // + '                          </div>'
                //+ '                          <!-- end of col -->'
                //+ '                          </div>'
                // + '                      </div>'
                // + '                      <!-- end of row -->'
                //+ '  ');
                //end Trxamount
                divRow4Col3.append(divRow4Col3DiffType);
                divRow4.append(divRow4Col1).append(divRow4Col2).append(divRow4Col3);

                //Add Contra 
                divRow5 = $('<div class="row va012-fl-padd" style="width:102%">'
                    // + '                          <div id="VA012_divContra_' + $self.windowNo + '" style="padding-left: 7px;padding-right: 7px;">'
                    + '                              <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divContraType_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_ContraType") + '</label>'
                    + '                                      <select tabindex="10" id="VA012_cmbContraType_' + $self.windowNo + '">'
                    + '                                      </select>'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->'
                    + '                              <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divCashBook_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_CashBook") + '</label>'
                    + '                                      <select tabindex="10" id="VA012_cmbCashBook_' + $self.windowNo + '">'
                    + '                                      </select>'
                    + '                                  </div>'

                    + '                                  <div id="VA012_divCtrlCashLine_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_CashJournalLine") + '</label>'
                    + '                                      <div id="VA012_ctrlCashLine_' + $self.windowNo + '" ></div>'
                    + '                                  </div>'

                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->'
                    //+ '                              <div class="col-md-3 col-sm-3 va012-padd-0">'
                    //+ '                                  <div id="VA012_divTransferType_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    //+ '                                      <label>' + VIS.Msg.getMsg("VA012_TransferType") + '</label>'
                    //+ '                                      <select tabindex="11" id="VA012_cmbTransferType_' + $self.windowNo + '">'
                    //+ '                                      </select>'
                    //+ '                                  </div>'
                    //+ '                                  <!-- end of form-group -->'
                    //+ '                              </div>'
                    //+ '                              <!-- end of col -->'
                    + '                              <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divCheckNo_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_CheckNo") + '</label>'
                    + '                                      <input  disabled tabindex="12" id="VA012_txtCheckNo_' + $self.windowNo + '" type="text">'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->'
                    + '                          </div>'
                    // + '                      </div>'
                    + '                      <!-- end of row -->'
                    + '  ');
                // End Contra



                divRow6 = $('<div class="row va012-fl-padd" style="width:102%">');
                //  + '                          <div id="VA012_divVoucher_' + $self.windowNo + '" style=" padding-left: 7px;padding-right: 7px;">'
                divRow6Col1 = $('<div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divCharge_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_Charge") + '</label>'
                    + '                                      <div style=" position: relative; float: left; width: 100%; ">'
                    + '                                      <input tabindex="10" chargeid="" type="text" id="VA012_txtCharge_' + $self.windowNo + '" style=" width: 100%; ">'
                    + '                                      <img id="VA012_btnCharge_' + $self.windowNo + '" class="VA012-img-combo" alt="">'
                    + '                                     </div>'
                    //+ '                                      <select id="VA012_cmbCharge_' + $self.windowNo + '">'
                    //+ '                                      </select>'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->');
                divRow6Col2 = $('<div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divTaxRate_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_TaxRate") + '</label>'
                    + '                                      <select tabindex="11" id="VA012_cmbTaxRate_' + $self.windowNo + '">'
                    + '                                      </select>'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->');
                divRow6Col3 = $('<div class="col-md-4 col-sm-4 va012-padd-0">');
                divRow6Col3divTax = $('<div id="VA012_divTaxAmount_' + $self.windowNo + '" class="va012-form-group va012-form-data">');
                divRow6Col3divTaxLbl = $('<label>' + VIS.Msg.getMsg("VA012_TaxAmount") + '</label>');
                _txtTaxAmount = new VIS.Controls.VAmountTextBox("VA012_txtTaxAmount_" + $self.windowNo + "", false, false, true, 50, 100, VIS.DisplayType.Amount, VIS.Msg.getMsg("Amount"));
                _txtTaxAmount.setValue(0);
                _txtTaxAmount.getControl().addClass('va012-right-align');
                divRow6Col3divTax.append(divRow6Col3divTaxLbl).append(_txtTaxAmount.getControl());
                divRow6Col3.append(divRow6Col3divTax);
                divRow6.append(divRow6Col1).append(divRow6Col2).append(divRow6Col3);
                //+ '<input tabindex="12" class="va012-right-align"  value="0.00" id="VA012_txtTaxAmount_' + $self.windowNo + '" type="number">'
                //+ '                                  </div>'
                //+ '                                  <!-- end of form-group -->'
                //+ '                              </div>'
                //+ '                              <!-- end of col -->'
                //+ '                          </div>'
                //// + '                      </div>'
                //+ '                      <!-- end of row -->'
                //+ '  ');
                divRow7 = $('<div class="row va012-fl-padd" style="width:102%">'
                    // + '                          <div id="VA012_divMatch_' + $self.windowNo + '" style="padding-left: 7px;padding-right: 7px;">'
                    + '                              <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divCtrlPayment_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_Payment") + '</label>'
                    + '                                      <div id="VA012_ctrlPayment_' + $self.windowNo + '" ></div>'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->'
                    + '                              <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divCtrlInvoice_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_Invoice") + '</label>'
                    + '                                      <div id="VA012_ctrlInvoice_' + $self.windowNo + '" ></div>'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->'
                    + '                              <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                                  <div id="VA012_divCtrlBusinessPartner_' + $self.windowNo + '" class="va012-form-group va012-form-data">'
                    + '                                      <label>' + VIS.Msg.getMsg("VA012_BusinessPartner") + '</label>'
                    + '                                      <div id="VA012_ctrlBusinessPartner_' + $self.windowNo + '" ></div>'
                    + '                                  </div>'
                    + '                                  <!-- end of form-group -->'
                    + '                              </div>'
                    + '                              <!-- end of col -->'
                    + '                          </div>'
                    //+ '                      </div>'
                    + '                      <!-- end of row -->'
                    + '  ');
                divRow8 = $('<div class="row va012-fl-padd" style="width:102%">'
                    + '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div id="VA012_divPaymentSchedule_' + $self.windowNo + '" class="va012-form-data" >'
                    + '                              <label>' + VIS.Msg.getMsg("VA012_PaymentSchedules") + '</label>'
                    + '                              <input disabled id="VA012_txtPaymentSchedule_' + $self.windowNo + '" type="text" class="va012-input-size">'
                    + '                              <a tabindex="13"  id="VA012_btnPaymentSchedule_' + $self.windowNo + '" class="va012-edit-icon"></a>'
                    + '                              </div>'
                    + '                              <!-- end of form-data -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                          <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div id="VA012_divPrepayOrder_' + $self.windowNo + '" class="va012-form-data" >'
                    + '                              <label>' + VIS.Msg.getMsg("VA012_PrepayOrders") + '</label>'
                    + '                              <div id="VA012_ctrlOrder_' + $self.windowNo + '" ></div>'
                    //+ '                              <input disabled id="VA012_txtPrepayOrder_' + $self.windowNo + '" type="text" class="va012-input-size">'
                    //+ '                              <a tabindex="1" id="VA012_btnPrepay_' + $self.windowNo + '" class="va012-edit-icon"></a>'
                    + '                              </div>'
                    + '                              <!-- end of form-data -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    //+ '                          <div class="col-md-4 col-sm-4 va012-padd-0" style=" padding-top: 25px; padding-bottom: 10px; ">'
                    //+ '                              <div class="va012-form-group va012-form-check">'
                    //+ '                                  <input tabindex="1" id="VA012_chkUseNextTime_' + $self.windowNo + '" type="checkbox">'
                    //+ '                                  <label style="position: absolute;">' + VIS.Msg.getMsg("VA012_UseNextTime") + '</label>'
                    //+ '                              </div>'
                    //+ '                              <!-- end of form-group -->'
                    //+ '                          </div>'
                    //+ '                          <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->'



                    + '  ');
                divRow9 = $('<div class="row va012-fl-padd" style="width:102%">'
                    + '                       <div class="col-md-4 col-sm-4 va012-padd-0">'
                    + '                              <div class="va012-form-group va012-form-check">'
                    + '                                  <input tabindex="13" id="VA012_chkUseNextTime_' + $self.windowNo + '" type="checkbox">'
                    + '                                  <label style="position: absolute;">' + VIS.Msg.getMsg("VA012_UseNextTime") + '</label>'
                    + '                              </div>'
                    + '                              <!-- end of form-group -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                          <div class="col-md-2 col-sm-2 va012-padd-0" >'
                    + '                              <div class="va012-form-data" id="VA012_divMore_' + $self.windowNo + '">'
                    + '                                  <a tabindex="14" visiblestatus="0" class="va012-more" id="VA012_btnMore_' + $self.windowNo + '" href="#">' + VIS.Msg.getMsg("VA012_More") + '</a>'
                    + '                              </div>'
                    + '                              <!-- end of form-data -->'
                    + '                          </div>'
                    + '                          <!-- end of col -->'

                    + '                          <div class="col-md-6 col-sm-6 va012-padd-0" >'
                    + '                          <div style="float:right;">'
                    //               + '                              <a id="VA012_btnCreatePayment_' + $self.windowNo + '" class="va012-frm-btn va012-btn-blue"  style=" float: left; margin-right: 10px; display: none;">' + VIS.Msg.getMsg("VA012_CreatePayment") + '</a>'
                    + '                              <a tabindex="15" title="Save Record" id="VA012_btnSave_' + $self.windowNo + '" class="va012-frm-btn va012-btn-blue" style=" float: left;">' + VIS.Msg.getMsg("VA012_SaveandNew") + '</a>'
                    + '                          </div>'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->');
                //+ '                  </div>');
                //+ '                  <!-- end of form-wrap -->'
                payHeaderWrap = $('<div id="VA012_paymentHeaderWrap_' + $self.windowNo + '" class="va012-payment-header-wrap">'
                    + '                          <div class="pull-left">'
                    + '                              <a class="va012-frm-btn va012-btn-gray" style="cursor: default;">' + VIS.Msg.getMsg("VA012_UpcomingTransactions") + '</a>'
                    + '                          </div>'
                    + '                      <div style=" float: right; width: 60%; "> '
                    + '                          <div class="va012-width-30" style=" float: left; width: 49%; ">'
                    + '                              <select id="VA012_cmbTransactionType_' + $self.windowNo + '">'
                    + '                             <option value="PY">' + VIS.Msg.getMsg("VA012_Payments") + '</option>'
                    + '                             <option value="PO">' + VIS.Msg.getMsg("VA012_PrepayOrders") + '</option>'
                    + '                              <option value="IS">' + VIS.Msg.getMsg("VA012_InvoiceSchedule") + '</option>'
                    + '                              <option value="CO">' + VIS.Msg.getMsg("VA012_Contra") + '</option>'
                    + '                              </select>'
                    + '                          </div>'
                    + '                          <div class="va012-width-30" style=" float: right; width: 49%; ">'
                    + '                              <select id="VA012_cmbSearchPaymentMethod_' + $self.windowNo + '">'
                    + '                              </select>'
                    + '                          </div>'
                    + '                          </div>'
                    + '                      </div>'
                    + '                      <!-- end of payment-header-wrap -->'
                    + '                  <div id="VA012_paymentList_' + $self.windowNo + '" class="va012-payment-list">'
                    + '                      '
                    + '  '
                    + '                   <div class="va012-payment-content" id="VA012_lstPayments_' + $self.windowNo + '">'
                    + '                   </div>'
                    + '                  </div>'
                    + '                  <!-- end of payment-list -->'
                    + '  '
                    + '              </div>');
                // + '              <!-- end of middle-wrap -->'
                rightWrap = $('<div id="VA012_rightWrap_' + $self.windowNo + '" class="va012-right-wrap">'
                    + '                  <div id="VA012_rightTop_' + $self.windowNo + '" class="va012-right-top">'
                    + '                      <div class="row">'
                    + '                          <div class="col-md-3 col-sm-3" style=" padding-right: 5px; ">'
                    + '                              <div class="va012-pay-text" id="VA012_secReconciled_' + $self.windowNo + '" >'
                    + '                                <p>' + VIS.Msg.getMsg("VA012_Reconciled") + '</p>'
                    + '                                <p style="margin-top: 4px;">' + VIS.Msg.getMsg("VA012_Unreconciled") + '</p>'
                    + '                              </div>'
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                          <div class="col-md-9 col-sm-9" style=" padding-left: 5px; ">'
                    + '                              <div class="va012-pay-text" id="VA012_secUnreconciled_' + $self.windowNo + '">'
                    + '                                <span style="padding-bottom: 2px;" class="va012-amount va012-font-green"><span class="va012-base-curr"></span> 0</span>'
                    + '                                <span style="padding-bottom: 2px;" class="va012-amount va012-font-red"><span class="va012-base-curr"></span> 0</span>'
                    + '                              </div>'
                    + '                          </div>'

                    + '                          <!-- end of col -->'

                    + '                      </div>'
                    + '                      <!-- end of row -->'

                    + '                      <div class="row">'
                    + '                          <div class="col-md-12 col-sm-12" style=" padding-left: 5px; ">'
                    + '                      <div style=" /* float: right; */ padding-left: 10px;"> '
                    + '                              <a id="VA012_btnUnmatch_' + $self.windowNo + '" class="va012-frm-btn va012-btn-blue" title="Show Unmatched Record" style=" float: left; ">' + VIS.Msg.getMsg("VA012_Unmatch") + '</a>'
                    + '                              <a id="VA012_btnProcess_' + $self.windowNo + '" class="va012-frm-btn va012-btn-blue" title="Clear Matched Record" style=" float: left; margin-left: 5px; ">' + VIS.Msg.getMsg("VA012_Process") + '</a>'
                    + '                             <a id="VA012_btnDelete_' + $self.windowNo + '" class="va012-frm-btn va012-btn-blue" title="Delete Selected Record" style=" float: left; margin-left: 5px; ">' + VIS.Msg.getMsg("VA012_Delete") + '</a>'
                    + '                       </div> '
                    + '                          </div>'
                    + '                          <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->'

                    + '                     <div class="row">'
                    + '                          <div class="col-md-12 col-sm-12" style=" padding-right: 5px; ">'
                    + '                           <div class="va012-right-search">'
                    + '                            <div class="va012-search-wrap">'
                    + '                               <input id="VA012_txtSearch_' + $self.windowNo + '" value="" placeholder="' + VIS.Msg.getMsg("VA012_Search") + '..." type="text">'
                    + '                               <a id="VA012_btnSearch_' + $self.windowNo + '" class="va012-search-icon"><span class="glyphicon glyphicon-search"></span></a>'
                    + '                          </div>'
                    + '                      <!-- end of search-wrap -->'
                    + '                         </div>'
                    + '                  <!-- end of right-search -->'
                    + '                        </div>'
                    + '                       <!-- end of col -->'
                    + '                      </div>'
                    + '                      <!-- end of row -->'
                    + '                     </div>'
                    + '                  <!-- end of right-top -->'
                    + '  '
                    + '                  <div class="va012-right-content" id="VA012_lstStatement_' + $self.windowNo + '">'
                    + '                  </div>'
                    + '                  <!-- end of right-content -->'
                    + '              </div>');
                //+ '              <!-- end of right-wrap -->'
                //+ '          </div>'
                //+ '          <!-- end of content-area -->'
                //+ '  </td>');
                //</tr>
                //</table>');
                row2Col2divAmt.append(row2Col2Lble).append(row2Col2btnIn).append(row2Col2btnOut).append(_txtAmount.getControl()).append(row2Col2btnIcon);
                row2Col2.append(row2Col2divAmt);
                divRow2.append(row2Col1).append(row2Col2).append(row2Col3);
                divformWrap.append(divRow1).append(divRow2).append(divRow3).append(divRow4).append(divRow5).append(divRow6).append(divRow7).append(divRow8).append(divRow9);
                divMidWrap.append(divtopWrap).append(divformWrap).append(payHeaderWrap);
                contentDiv.append(divMidWrap).append(rightWrap);
                tableTd1.append(contentDiv);
                tableTr.append(tableTd).append(tableTd1);                   //+ '      </div>'
                //+ '      <!-- end of main-container -->'
                //+ '  '
                //+ '  </div>'
                //+ '  <!-- end of assign-content -->'
                //+ '  '
                //;
                $divMatchStatementGridPopUp = $('<div  id="VA012_gridMatchStatePopUp_' + $self.windowNo + '"" style="display:block;width:auto;height:auto">'
                    + '<div  style="    width: 50%;float: left;" ><label>' + VIS.Msg.getMsg("VA012_ChargeType") + '</label><div  id="VA012_ChargeSrch_' + $self.windowNo + '"></div></div>'
                    + '<div ><label>' + VIS.Msg.getMsg("VA012_Taxrate") + '</label><select  id="VA012_cmbTaxRate_' + $self.windowNo + '"></select></div>'
                    + '<div style="width:auto;height:270px" id="VA012_gridMatchState_' + $self.windowNo + '""></div></div>');

                $GrdPayment = $divMatchStatementGridPopUp.find("#VA012_gridMatchState_" + $self.windowNo);
                _chargeSrch = $divMatchStatementGridPopUp.find("#VA012_ChargeSrch_" + $self.windowNo);
                $CmbTaxRate = $divMatchStatementGridPopUp.find("#VA012_cmbTaxRate_" + $self.windowNo);
                //_formDesign = _formDesign + $divMatchStatementGridPopUp;
                //Added Charge Search Lookup
                _ChargeLookUp = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), $self.windowNo, 3787, VIS.DisplayType.Search);
                $ChargeControl = new VIS.Controls.VTextBoxButton("C_Charge_ID", true, false, true, VIS.DisplayType.Search, _ChargeLookUp);
                _chargeSrch.append($ChargeControl.getControl().css('width', '93%')).append($ChargeControl.getBtn(0).css('width', '30px').css('height', '30px').css('padding', '0px').css('border-color', '#BBBBBB'));
                divTable.append(tableTr);
                _formDesign.append(divContainer).append(divTable);
                //_txtTrxAmt.addVetoableChangeListener(this);
                return _formDesign;
            },
            setBankAndAccount: function () {

                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/SetBankAndAccount",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {

                        if (data != null && data != "") {
                            data = $.parseJSON(data);
                            if (data != null) {
                                C_BANK_ID = data.Table[0].C_BANK_ID;
                                C_BANKACCOUNT_ID = data.Table[0].C_BANKACCOUNT_ID;

                                //_cmbBank.val(data.Table[0].C_BANK_ID).prop('selected', true);
                                //_cmbBankAccount.val(data.Table[0].C_BANKACCOUNT_ID).prop('selected', true);
                            }
                        }
                        loadFunctions.loadBank();
                    },
                });
            },
            addEffect: function (btn, _to, functionName) {

                var options = { to: _to, className: "wsp-ui-effects-transfer" };
                btn.effect("transfer", options, 600, functionName);
            },
            setInvoiceAndBPartner: function (_paymentID, _cmbTransactionType) {
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/SetInvoiceAndBPartner",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    data: ({ _paymentID: _paymentID, _cmbTransactionType: _cmbTransactionType }),
                    success: function (data) {
                        if (data != null && data != "") {
                            data = $.parseJSON($.parseJSON(data));
                            if (_cmbTransactionType == "PY") {

                                if (data._invoiceID > 0) {
                                    $_ctrlInvoice.setValue(data._invoiceID, false, true);
                                }
                                else {
                                    $_ctrlInvoice.setValue();
                                }
                                if (data._bPartnerID > 0) {
                                    $_ctrlBusinessPartner.setValue(data._bPartnerID, false, true);
                                }
                                else {
                                    $_ctrlBusinessPartner.setValue();
                                }
                                var count = VIS.DB.executeScalar("SELECT COUNT(*) FROM AD_ModuleInfo WHERE Prefix='VA034_' AND IsActive='Y'");
                                if (count == 1) {
                                    if (data.VA034_DepositSlipNo != "") {
                                        _txtVoucherNo.val(data.VA034_DepositSlipNo);
                                    }
                                    else {
                                        // _txtVoucherNo.val("");
                                    }
                                }
                            }
                            else if (_cmbTransactionType == "PO") {
                                if (!$_ctrlBusinessPartner.value) {//will evaluate to true if value is not:  null, undefined, NaN, empty string (""), 0,false
                                    $_ctrlBusinessPartner.setValue(data._bPartnerID, false, true);
                                }
                            }
                            else if (_cmbTransactionType == "IS") {
                                if (!$_ctrlBusinessPartner.value) {
                                    $_ctrlBusinessPartner.setValue(data._bPartnerID, false, true);
                                }

                            }
                        }
                    },
                });
            },
            setBPartner: function (_invoiceID) {

                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/SetBPartner",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    data: ({ _invoiceID: _invoiceID }),
                    success: function (data) {
                        if (data != null && data != "") {
                            data = $.parseJSON(data);
                            if (data > 0) {
                                $_ctrlBusinessPartner.setValue(data, false, true);
                            }
                            else {
                                $_ctrlBusinessPartner.setValue();
                            }
                        }
                    },
                });
            },
            getMaxStatement: function (_origin) {
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/MaxStatement",
                    type: "GET",
                    datatype: "json",
                    data: ({ _bankAccount: _cmbBankAccount.val() == null ? 0 : _cmbBankAccount.val(), _origin: _origin }),
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (data != null && data != "") {
                            data = $.parseJSON($.parseJSON(data));
                            _txtStatementNo.val(data.statementNo);
                            _txtStatementPage.val(data.pageno);
                            _txtStatementLine.val(data.lineno);

                        }
                    },
                });

            },
            getOverUnderPayment: function (_paymentID) {

                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/getOverUnderPayment",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    data: ({ _paymentID: _paymentID }),
                    success: function (data) {
                        if (data != null && data != "") {
                            data = $.parseJSON($.parseJSON(data));
                            if (data._difference != 0) {
                                _txtDifference.setValue(VIS.Utility.Util.getValueOfDecimal(Math.abs(data._difference)));
                                _divDifferenceType.find("*").prop("disabled", false);
                                _cmbDifferenceType.val(data._differenceType).prop('selected', true);
                                _txtDifference.getControl().attr("vchangable", "N");
                                //if (_cmbVoucherMatch.val() == "M") {
                                //    _txtTrxAmt.val((parseFloat(_txtAmount.val()) + parseFloat(_txtDifference.val())).toFixed(_stdPrecision));
                                //}
                                if (_cmbVoucherMatch.val() == "M") {
                                    _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(data._payamt.toFixed(_stdPrecision)));
                                }
                            }
                        }
                    },
                });
            },
            getControls: function () {


                _cmbBank = $root.find("#VA012_cmbBank_" + $self.windowNo);
                _cmbBankAccount = $root.find("#VA012_cmbBankAccount_" + $self.windowNo);
                _cmbBankAccountClasses = $root.find("#VA012_STAT_cmbBankAccountClassName_" + $self.windowNo);
                //Get Statement Date Control from Root
                _statementDate = $root.find("#VA012_statementDate_" + $self.windowNo);


                //_cmbBankAccountCharges = $root.find("#VA012_STAT_cmbBankAccountCharges_" + $self.windowNo);                
                _VA012_BankChargeDiv = $root.find("#VA012_BankChargeDiv" + $self.windowNo);

                _cmbSearchPaymentMethod = $root.find("#VA012_cmbSearchPaymentMethod_" + $self.windowNo);
                _cmbTransactionType = $root.find("#VA012_cmbTransactionType_" + $self.windowNo);
                _btnLoadStatement = $root.find("#VA012_btnLoadStatement_" + $self.windowNo);
                _btnMatchStatement = $root.find("#VA012_btnMatchStatement_" + $self.windowNo);
                _lstStatement = $root.find("#VA012_lstStatement_" + $self.windowNo);
                _lstPayments = $root.find("#VA012_lstPayments_" + $self.windowNo);
                //to handling busyIdicator for paymentList
                _paymentLists = $root.find("#VA012_paymentList_" + $self.windowNo);
                _secReconciled = $root.find("#VA012_secReconciled_" + $self.windowNo);
                _secUnreconciled = $root.find("#VA012_secUnreconciled_" + $self.windowNo);
                // _divVoucher = $root.find("#VA012_divVoucher_" + $self.windowNo);
                // _divMatch = $root.find("#VA012_divMatch_" + $self.windowNo);
                _txtSearch = $root.find("#VA012_txtSearch_" + $self.windowNo);
                _btnSearch = $root.find("#VA012_btnSearch_" + $self.windowNo);
                _btnUnmatch = $root.find("#VA012_btnUnmatch_" + $self.windowNo);
                _btnProcess = $root.find("#VA012_btnProcess_" + $self.windowNo);
                _btnHide = $root.find("#VA012_btnHide_" + $self.windowNo);
                _tdLeft = $root.find("#VA012_tdLeft_" + $self.windowNo);
                _table = $root.find("#VA012_table_" + $self.windowNo);
                _btnMore = $root.find("#VA012_btnMore_" + $self.windowNo);
                _divMore = $root.find("#VA012_divMore_" + $self.windowNo);

                //get all control div

                _divContraType = $root.find("#VA012_divContraType_" + $self.windowNo);
                _divCashBook = $root.find("#VA012_divCashBook_" + $self.windowNo);
                _divCtrlCashLine = $root.find("#VA012_divCtrlCashLine_" + $self.windowNo);
                _divTransferType = $root.find("#VA012_divTransferType_" + $self.windowNo);
                _divCheckNo = $root.find("#VA012_divCheckNo_" + $self.windowNo);
                _divVoucherNo = $root.find("#VA012_divVoucherNo_" + $self.windowNo);
                _divTrxAmt = $root.find("#VA012_divTrxAmt_" + $self.windowNo);
                _divDifference = $root.find("#VA012_divDifference_" + $self.windowNo);
                _divDifferenceType = $root.find("#VA012_divDifferenceType_" + $self.windowNo);
                _divCharge = $root.find("#VA012_divCharge_" + $self.windowNo);
                _btnCharge = $root.find("#VA012_btnCharge_" + $self.windowNo);
                _divTaxRate = $root.find("#VA012_divTaxRate_" + $self.windowNo);
                _divTaxAmount = $root.find("#VA012_divTaxAmount_" + $self.windowNo);
                _divCtrlPayment = $root.find("#VA012_divCtrlPayment_" + $self.windowNo);
                _divCtrlInvoice = $root.find("#VA012_divCtrlInvoice_" + $self.windowNo);
                _divCtrlBusinessPartner = $root.find("#VA012_divCtrlBusinessPartner_" + $self.windowNo);

                _divPrepayOrder = $root.find("#VA012_divPrepayOrder_" + $self.windowNo);
                _divPaymentSchedule = $root.find("#VA012_divPaymentSchedule_" + $self.windowNo);
                ///




            },
            getBaseCurrency: function () {

                //var _sql = " SELECT BCURR.ISO_CODE AS BASECURRENCY "
                //+ " FROM AD_CLIENTINFO CINFO "
                //+ " INNER JOIN C_ACCTSCHEMA AC "
                //+ " ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID "
                //+ " LEFT JOIN C_CURRENCY BCURR "
                //+ " ON AC.C_CURRENCY_ID      =BCURR.C_CURRENCY_ID "
                //+ " WHERE CINFO.AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID();
                //var baseCurr = VIS.DB.executeScalar(_sql.toString());
                //if (baseCurr != "" || baseCurr != null) {
                //    _clientBaseCurrency = baseCurr;
                //}

                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/GetBaseCurrency",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (data != null && data != "") {

                            data = $.parseJSON($.parseJSON(data));
                            _clientBaseCurrency = data._code;
                            _clientBaseCurrencyID = data._id;
                            //_cmbCurrency.val(_clientBaseCurrencyID).prop('selected', true);
                            $(".va012-base-curr").text(_clientBaseCurrency);
                        }
                    },
                });


            },

            loadBank: function () {

                //var _sql = "SELECT NAME,C_BANK_ID FROM C_BANK WHERE ISACTIVE='Y'  AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID() + " AND AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID();
                //var _sql = "SELECT NAME,C_BANK_ID FROM C_Bank WHERE ISACTIVE='Y'";

                //_sql = VIS.MRole.getDefault().addAccessSQL(_sql, "C_Bank", true, false);
                //var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadBank);
                //get Bank's from Controller and append to bank list dropdown
                //fetch IsOwnBank is true those bank only will get
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/GetBank",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    success: function (data) {
                        if (data != null && data != "") {
                            _ds = $.parseJSON(data);
                            callbackloadBank(_ds);
                        }
                    },
                });
                function callbackloadBank(_ds) {
                    _cmbBank.html("");
                    _cmbBank.append("<option value=0 ></option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.length; i++) {
                            //_cmbBank.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_bank_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                            _cmbBank.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds[i].Value) + ">" + _ds[i].Name + "</option>");
                        }
                    }
                    //_ds.dispose();
                    //_ds = null;
                    //_sql = null;
                    _cmbBank.prop('selectedIndex', 0);
                    if (C_BANK_ID > 0) {
                        _cmbBank.val(C_BANK_ID).prop('selected', true);
                        C_BANK_ID = 0;
                        // _cmbBank.trigger('change');
                    }
                    loadFunctions.loadBankAccount();


                }
            },

            loadBankAccount: function () {

                var _sql = "SELECT ACCOUNTNO,C_BANKACCOUNT_ID FROM C_BANKACCOUNT WHERE ISACTIVE='Y' AND C_BANK_ID=" + _cmbBank.val();
                var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadBankAccount);
                function callbackloadBankAccount(_ds) {
                    _cmbBankAccount.html("");
                    _cmbBankAccount.append("<option value=0 ></option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                            _cmbBankAccount.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_bankaccount_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.accountno) + "</option>");
                        }
                    }
                    _ds.dispose();
                    _ds = null;
                    _sql = null;
                    _cmbBankAccount.prop('selectedIndex', 0);
                    if (C_BANKACCOUNT_ID > 0) {
                        _cmbBankAccount.val(C_BANKACCOUNT_ID).prop('selected', true);
                        C_BANKACCOUNT_ID = 0;
                    }
                    loadFunctions.getMaxStatement("LO");
                    loadFunctions.loadSearchPaymentMethod();
                }
            },



            LoadPaymentsPages: function (_accountID, _paymentMethodID, _transactionType) {
                if (_txtSearch.val() != null && _txtSearch.val() != "") {
                    _SEARCHREQUEST = true;
                }
                else {
                    _SEARCHREQUEST = false;
                }
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/LoadPaymentsPages",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _accountID: _accountID, _paymentPageNo: _paymentPageNo, _PAGESIZE: _paymentPAGESIZE, _paymentMethodID: _paymentMethodID, _transactionType: _transactionType }),
                    success: function (data) {
                        if (data != null && data != "") {

                            _paymentPageCount = JSON.parse(data);
                        }
                    },
                })
            },
            /**
             * To load the payments 
             * @param {any} _accountID: Bank Account ID
             * @param {any} _paymentMethodID: Payment Method ID
             * @param {any} _transactionType: Transaction Type
             * @param {any} _statementDate: Statement Date
             */
            loadPayments: function (_accountID, _paymentMethodID, _transactionType, _statementDate) {

                // $BusyIndicator[0].style.visibility = "visible";
                //_scheduleList = [];
                //_scheduleDataList = [];
                //_prepayList = [];
                //_prepayDataList = [];
                //_txtPaymentSchedule.val("");
                //_txtPrepayOrder.val("");
                //handled Scrolling for Transaction tab
                busyIndicator($(_paymentLists), true, "inherit");


                window.setTimeout(function () {
                    $.ajax({
                        url: VIS.Application.contextUrl + "BankStatement/LoadPayments",
                        type: "GET",
                        datatype: "json",
                        contentType: "application/json; charset=utf-8",
                        data: ({ _accountID: _accountID, _paymentPageNo: _paymentPageNo, _PAGESIZE: _PAGESIZE, _paymentMethodID: _paymentMethodID, _transactionType: _transactionType, statementDate: (_statementDate == null || _statementDate == "") ? new (Date) : _statementDate }),
                        success: function (data) {
                            if (data != null && data != "") {

                                callbackloadPayments(data);
                                busyIndicator($(_paymentLists), false, "inherit");
                            }
                        },
                        error: function () {
                            busyIndicator($(_paymentLists), false, "inherit");
                        }
                    });
                }, 2);
                function callbackloadPayments(data) {
                    data = $.parseJSON(data);
                    if (data.length > 0) {
                        storepaymentdata = storepaymentdata.concat(data);
                    }
                    data = storepaymentdata;
                    var _PaymentsHTML = "";
                    _lstPayments.html(""); //To Clear the Payment grid
                    if (data != null && data.length > 0) {
                        for (var i = 0; i < data.length; i++) {
                            var dateAcc = new Date(data[i].DateAcct).toLocaleDateString();
                            var status = "va012-red-color";

                            if (data[i].c_bankstatementline_id == null || data[i].c_bankstatementline_id == "0" || data[i].c_bankstatementline_id == 0) {

                                status = "va012-red-color";

                            }
                            else {
                                status = "va012-green-color";

                            }


                            _PaymentsHTML = "";
                            _PaymentsHTML = '<div class="drag-no-drag"><div draggable="true" class="va012-mid-data-wrap ';
                            if (_transactionType == "PY" || _transactionType == "CO") {
                                _PaymentsHTML += status;
                            }
                            _PaymentsHTML += ' " paymentdata="' + VIS.Utility.encodeText(new Date(data[i].DueDate).toLocaleDateString()) + "_" + VIS.Utility.encodeText(data[i].DueAmt) + '" data-uid="' + data[i].c_payment_id;

                            /* change by pratap*/
                            _PaymentsHTML += ' " paymentamount = " ' + VIS.Utility.Util.getValueOfDecimal(data[i].convertedamount, "N") + '">'
                                /* end change by pratap*/

                                + '<div class="va012-payment-wrap" >'
                                + '<div class="row">'
                                + ' <div class="col-md-3 col-sm-3">'
                                + '    <div class="va012-form-check">'
                                + '        <input type="checkbox" data-uid="' + data[i].c_payment_id + '"  >'
                                + '    <div title="' + VIS.Msg.getMsg('VA012_PaymentAmount') + '" class="va012-inside-form-check" style=" float: left; width: 85%; ">'
                                + '      <label style=" width: 100%; ">' + data[i].currency + ' ' + parseFloat(data[i].paymentamount).toLocaleString(navigator.language, { minimumFractionDigits: _stdPrecision, maximumFractionDigits: _stdPrecision }) + '</label>';
                            if (data[i].isconverted == "Y") {
                                _PaymentsHTML += '      <span>' + data[i].basecurrency + ' ' + parseFloat(data[i].convertedamount).toLocaleString(navigator.language, { minimumFractionDigits: _stdPrecision, maximumFractionDigits: _stdPrecision }) + '</span>';
                            }
                            _PaymentsHTML += '   </div></div>'
                                + '    <!-- end of form-group -->'
                                + '  </div>'
                                + '  <!-- end of col -->'
                                + '  <div class="col-md-4 col-sm-4">'
                                + '     <div class="va012-form-check">'
                                + '         <div class="va012-pay-text">'
                                + '           <p title="Business Partner">' + VIS.Utility.encodeText(data[i].businesspartner) + '</p>'
                                + '         <span title="' + VIS.Msg.getMsg("VA012_InvReference") + '">' + VIS.Utility.encodeText(data[i].bpgroup) + '</span>'
                                + '       <span title="Document No.">' + VIS.Utility.encodeText(data[i].paymentno) + '</span>'
                                + '  </div>'
                                + ' </div>'
                                + '<!-- end of form-group -->'
                                + '</div>'
                                + '<!-- end of col -->'


                                /*change by pratap*/
                                + ' <div class="col-md-2 col-sm-2">'
                                + '     <div class="va012-form-check">'
                                + '         <div class="va012-pay-text">'
                                + '<p title="Payment Type">' + VIS.Utility.encodeText(data[i].paymenttype) + '</p>'
                                + '</div>'
                                + '</div>'
                                + '<!-- end of form-group -->'
                                + '</div>'
                                + '<!-- end of col -->'
                            /*End change by pratap*/


                            //By SUkhwinder

                            var countVA034 = VIS.Utility.Util.getValueOfInt(VIS.DB.executeScalar("SELECT Count(AD_ModuleInfo_ID) FROM AD_ModuleInfo WHERE PREFIX='VA034_' AND IsActive = 'Y'"));

                            if (_transactionType == "PY" && countVA034 > 0) {
                                _PaymentsHTML += '  <div class="col-md-2 col-sm-2">'
                                    + '     <div class="va012-form-check">'
                                    + '         <div class="va012-pay-text">'
                                    + '           <p title="Deposit Slip No">' + VIS.Utility.encodeText(data[i].depositslipno) + '</p>'
                                    + '           <p title="Authentication Code">' + VIS.Utility.encodeText(data[i].authcode) + '</p>'

                                    + '           <p title="Account Date">' + dateAcc + '</p>'
                                    + '           <p title="Payment Method">' + data[i].PaymentMethod + '</p>'

                                    + '  </div>'
                                    + ' </div>'
                                    + '<!-- end of form-group -->'
                                    + '</div>'
                                    + '<!-- end of col -->';
                            }
                            else {
                                _PaymentsHTML += '  <div class="col-md-2 col-sm-2">'
                                    + '     <div class="va012-form-check">'
                                    + '         <div class="va012-pay-text">'
                                    + '           <p title="Authentication Code">' + VIS.Utility.encodeText(data[i].authcode) + '</p>'

                                    + '           <p title="Account Date">' + dateAcc + '</p>'
                                    + '           <p title="Payment Method">' + data[i].PaymentMethod + '</p>'

                                    + '  </div>'
                                    + ' </div>'
                                    + '<!-- end of form-group -->'
                                    + '</div>'
                                    + '<!-- end of col -->';
                            }
                            //

                            //////if (_transactionType == "PY" && countVA034 > 0) {
                            //////    _PaymentsHTML += '<div class="col-md-1 col-sm-1">'
                            //////}
                            //////else {
                            //////    _PaymentsHTML += '<div class="col-md-3 col-sm-3">'
                            //////}


                            //////          + '  <div class="va012-form-data">';

                            //////if (data[i].imageurl != null && data[i].imageurl != "") {
                            //////    _PaymentsHTML += '    <img src="' + data[i].imageurl + '" alt="">';
                            //////}
                            //////    //else if (data[i].binarydata != null) {

                            //////    //    _PaymentsHTML += '    <img src="data:image/png;base64,' + data[i].binarydata + '" alt="">';
                            //////    //}
                            //////else {
                            //////    _PaymentsHTML += '<img src="Areas/VA012/Images/defaultBP.png" alt="">';
                            //////}


                            ////// _PaymentsHTML += '</div>'
                            //////             + '<!-- end of form-group -->'
                            //////         + '</div>'
                            //////         + '<!-- end of col -->'
                            //////     + '</div>'
                            //////     + '<!-- end of row -->'
                            ////// + '</div>'
                            //////+ ' <!-- end of payment-wrap -->'
                            //////  + '</div>'
                            //////+ ' <!-- end of mid-data-wrap -->'    



                            _PaymentsHTML += '</div>'
                                + '<!-- end of row -->'
                                + '</div>'
                                + ' <!-- end of payment-wrap -->'
                                + '</div>'
                                + ' <!-- end of mid-data-wrap -->'


                            //_PaymentsHTML += '<div class="va012-mid-data-wrap-img-no-drag ">'

                            //if (_transactionType == "PY" && countVA034 > 0) {
                            //    _PaymentsHTML += '<div class="col-md-1 col-sm-1">'
                            //}
                            //else {
                            //    _PaymentsHTML += '<div class="col-md-3 col-sm-3">'
                            //}
                            //+ '  <div class="va012-form-data">';
                            ////if (data[i].imageurl != null && data[i].imageurl != "") {
                            ////    _PaymentsHTML += '    <img src="' + data[i].imageurl + '" alt=""x>';
                            ////}
                            ////else {
                            ////    //_PaymentsHTML += '<img src="Areas/VA012/Images/defaultBP.png" alt=""x>';t
                            ////    _PaymentsHTML += '<i class="vis-chatimgwrap fa fa-user"></i>';
                            ////} VA009_Name,DateAcct
                            //_PaymentsHTML += '</div>'
                            //    + '</div>'
                            //    + '</div>'
                            //    + '<!-- end of no-drag -->'
                            //    + '</div>'
                            //    + '<!-- end of drag-no-drag -->';

                            _lstPayments.append(_PaymentsHTML);
                        }
                    }
                    _sql = null;
                    loadFunctions.setPaymentListHeight();
                    loadFunctions.dragPayments();

                }

            },
            setPaymentListHeight: function () {
                var id;

                clearTimeout(id);
                id = setTimeout(function () {
                    $("#VA012-content-area" + $self.windowNo).height($("#VA012_mainContainer_" + $self.windowNo).height() - 20);
                    if (_btnNewRecord.attr("activestatus") == "1") {

                        $("#VA012_paymentList_" + $self.windowNo).height($("#VA012_middleWrap_" + $self.windowNo).height() - $("#VA012_formBtnNewRecord_" + $self.windowNo).height() - $("#VA012_formNewRecord_" + $self.windowNo).height() - $("#VA012_paymentHeaderWrap_" + $self.windowNo).height() - 42);
                    }
                    else {

                        $("#VA012_paymentList_" + $self.windowNo).height($("#VA012_middleWrap_" + $self.windowNo).height() - $("#VA012_formBtnNewRecord_" + $self.windowNo).height() - $("#VA012_paymentHeaderWrap_" + $self.windowNo).height() - 27);
                    }

                }, 2);
            },
            loadSearchPaymentMethod: function () {

                var _sql = "SELECT VA009_NAME,VA009_PAYMENTMETHOD_ID FROM VA009_PAYMENTMETHOD WHERE ISACTIVE='Y' AND VA009_PAYMENTBASETYPE!='B' AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID();

                if (VIS.Env.getCtx().getAD_Org_ID() != 0) {
                    _sql += " AND AD_ORG_ID IN( " + VIS.Env.getCtx().getAD_Org_ID() + ",0)";
                }

                var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadSearchPaymentMethod);
                function callbackloadSearchPaymentMethod(_ds) {
                    _cmbSearchPaymentMethod.html("");
                    _cmbSearchPaymentMethod.append("<option value=0 >" + VIS.Msg.getMsg("VA012_SelectPaymentMethod") + "</option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                            _cmbSearchPaymentMethod.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.va009_paymentmethod_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.va009_name) + "</option>");
                        }
                    }
                    _ds.dispose();
                    _ds = null;
                    _sql = null;
                    _cmbSearchPaymentMethod.prop('selectedIndex', 0);
                    _cmbBankAccount.trigger('change');
                }
            },
            paymentScroll: function () {
                //handled scrolling issue
                _paymentPageCount = 0;
                if ($(this).scrollTop() + $(this).innerHeight() + 2 >= this.scrollHeight) {
                    //if ($(this).scrollTop() > 0 && $(this).scrollTop() + $(this).innerHeight() + 2 >= this.scrollHeight) {
                    //if (_paymentPageCount != 1) {
                    //_paymentPAGESIZE = _PAGESIZE * _paymentPageSizeInc;

                    loadFunctions.LoadPaymentsPages(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val());
                    if (_paymentPageNo < _paymentPageCount) {
                        _paymentPageNo++;
                        //_paymentPageSizeInc++;
                        loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                    }
                    //}

                }
            },
            statementScroll: function () {

                _statementPageCount = 0;

                if ($(this).scrollTop() > 0 && $(this).scrollTop() + $(this).innerHeight() + 1 >= this.scrollHeight) {
                    childDialogs.LoadStatementsPages();
                    if (_statementPageNo < _statementPageCount) {
                        _statementPageNo++;
                        childDialogs.loadStatement(_statementID);
                    }
                }
            },
            dragPayments: function () {
                _lstPayments.find(".va012-mid-data-wrap").draggable({

                    start: function (event, ui) {
                        ui.helper.addClass('va012-dragging');
                    },
                    drag: function (event, ui) {
                    },
                    stop: function (event, ui) {
                        ui.helper.removeClass('va012-dragging');
                    },
                    cursor: "move",
                    cursorAt: { left: 5 },
                    revert: "invalid",
                    helper: "clone",
                });
            },
            dropPayments: function () {
                _lstStatement.find(".va012-right-data-wrap").droppable({
                    hoverClass: "va012-dropping",
                    drop: function (event, ui) {

                        if (($(ui.draggable)).data('uid') > 0) {

                            ////// open record for edit
                            if ($(this).data("uid") > 0) {



                                if (_cmbTransactionType.val() == "PY") {
                                    var _dragPaymentID = ($(ui.draggable)).data('uid');

                                    if (($(ui.draggable)).hasClass("va012-green-color")) {
                                        VIS.ADialog.info(VIS.Msg.getMsg("VA012_PaymentAlreadyMatchedOthrStatement"), null, "", "");
                                        return;
                                    }

                                    var _dragStatementID = $(this).data("uid");
                                    if (loadFunctions.checkPaymentCondition(($(ui.draggable)).data('uid'), $(this).data("uid"), _txtAmount.getValue())) {
                                        childDialogs.statementOpenEdit($(this).data("uid"), _dragPaymentID);


                                        window.setTimeout(function () {
                                            _openingFromDrop = true;
                                            $_ctrlPayment.setValue(_dragPaymentID, false, true);
                                            //if (_txtVoucherNo.val() == "") {
                                            //    var Voucher = VIS.Utility.Util.getValueOfString(VIS.DB.executeScalar("select trxno from C_Payment where C_Payment_ID=" + _dragPaymentID));
                                            //    _txtVoucherNo.val(Voucher);
                                            //}
                                            loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "PY");
                                            // loadFunctions.getOverUnderPayment(($(ui.draggable)).data('uid'));
                                            _openingFromDrop = false;
                                        }, 200);
                                    }

                                }
                                else if (_cmbTransactionType.val() == "IS") {
                                    var _dragScheduleID = ($(ui.draggable)).data('uid');

                                    if (parseInt($_formNewRecord.attr("data-uid")) != $(this).data("uid")) {
                                        newRecordForm.scheduleRefresh();
                                    }
                                    if (loadFunctions.checkScheduleCondition(($(ui.draggable)).data('uid'), $(this).data("uid"), _scheduleList.toString(), _txtAmount.getValue())) {
                                        if (!isInList(parseInt(($(ui.draggable)).data('uid')), _scheduleList)) {
                                            _scheduleList.push(parseInt(($(ui.draggable)).data('uid')));
                                            _scheduleDataList.push($(ui.draggable).attr('paymentdata'));


                                            /*change by pratap*/
                                            //if (_txtAmount.val() == "0.00") {
                                            //    _scheduleAmount.push("0");
                                            //}
                                            _scheduleAmount.push($(ui.draggable).attr('paymentamount'));

                                            if (Number(_scheduleAmount[0]) == "0") {
                                                var amount = 0;
                                                for (var i = 0; i < _scheduleAmount.length; i++) {
                                                    amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                                                }
                                                if (amount < 0) {
                                                    _btnOut.removeClass("va012-inactive");
                                                    _btnOut.addClass("va012-active");
                                                    _btnOut.attr("v_active", "1");
                                                    _btnIn.removeClass("va012-active");
                                                    _btnIn.addClass("va012-inactive");
                                                    _btnIn.attr("v_active", "0");
                                                }
                                                else {
                                                    _btnIn.removeClass("va012-inactive");
                                                    _btnIn.addClass("va012-active");
                                                    _btnIn.attr("v_active", "1");
                                                    _btnOut.removeClass("va012-active");
                                                    _btnOut.addClass("va012-inactive");
                                                    _btnOut.attr("v_active", "0");
                                                }
                                                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                                                //_txtTrxAmt.val((amount).toFixed(_stdPrecision));
                                                //_txtTrxAmt.trigger('change');
                                            }
                                            /*change by pratap*/


                                            //loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "IS");
                                        }
                                        else {
                                            VIS.ADialog.info(VIS.Msg.getMsg("VA012_AlreadySelected"), null, "", "");
                                        }
                                        _openingFromDrop = true;
                                        //to get Invoice schedule amount
                                        childDialogs.statementOpenEdit($(this).data("uid"), _dragScheduleID);
                                        loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "IS");
                                        _txtPaymentSchedule.val(_scheduleDataList.toString());

                                        window.setTimeout(function () {
                                            var amount = 0;
                                            for (var i = 0; i < _scheduleAmount.length; i++) {
                                                amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                                            }
                                            _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                                            _txtTrxAmt.getControl().trigger('blur');
                                        }, 200);
                                    }

                                }
                                else if (_cmbTransactionType.val() == "PO") {
                                    if (parseInt($_formNewRecord.attr("data-uid")) != $(this).data("uid")) {
                                        newRecordForm.prepayRefresh();
                                    }
                                    if (loadFunctions.checkPrepayCondition(($(ui.draggable)).data('uid'), $(this).data("uid"), _prepayList.toString(), _txtAmount.getValue())) {

                                        childDialogs.statementOpenEdit($(this).data("uid"));
                                        //loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "PO");
                                        window.setTimeout(function () {
                                            _openingFromDrop = true;
                                            $_ctrlOrder.setValue(($(ui.draggable)).data('uid'), false, true);
                                            loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "PO");
                                            _openingFromDrop = false;
                                        }, 200);
                                        //childDialogs.statementOpenEdit($(this).data("uid"));
                                    }

                                }
                                if (_cmbTransactionType.val() == "CO") {
                                    var _dragPaymentID = ($(ui.draggable)).data('uid');
                                    if (($(ui.draggable)).hasClass("va012-green-color")) {
                                        VIS.ADialog.info(VIS.Msg.getMsg("VA012_CashLineAlreadyMatchedOthrStmt"), null, "", "");
                                        return;
                                    }
                                    var _dragStatementID = $(this).data("uid");
                                    if (loadFunctions.checkContraCondition(($(ui.draggable)).data('uid'), $(this).data("uid"), _txtAmount.getValue())) {
                                        childDialogs.statementOpenEdit($(this).data("uid"));

                                        window.setTimeout(function () {
                                            _openingFromDrop = true;
                                            $_ctrlCashLine.setValue(_dragPaymentID, false, true);
                                            _openingFromDrop = false;
                                        }, 200);
                                    }

                                }

                            }

                            //////

                        }

                    }
                });
                //$_formNewRecord.find(".va012-drop-schedule").droppable({
                $_formNewRecord.droppable({
                    hoverClass: "va012-dropping",
                    drop: function (event, ui) {

                        if (($(ui.draggable)).data('uid') > 0) {


                            if (_cmbTransactionType.val() == "CO") {
                                if (_cmbVoucherMatch.val() == "C") {
                                    if (_cmbContraType.val() != "CB") {
                                        VIS.ADialog.info(VIS.Msg.getMsg("VA012_ContraSelectCashToBank"), null, "", "");
                                        return;
                                    }
                                }
                                else {
                                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_SelectVoucherContra"), null, "", "");
                                    return;
                                }

                            }
                            else {
                                if (_cmbVoucherMatch.val() != "M") {
                                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_SelectVoucherPayment"), null, "", "");
                                    return;
                                }
                            }


                            if (_cmbTransactionType.val() == "PY") {
                                if (($(ui.draggable)).hasClass("va012-green-color")) {
                                    VIS.ADialog.info(VIS.Msg.getMsg("VA012_PaymentAlreadyMatchedOthrStatement"), null, "", "");
                                    return;
                                }
                                if (($(ui.draggable)).data('uid') > 0) {
                                    if (loadFunctions.checkPaymentCondition(($(ui.draggable)).data('uid'), $(this).attr("data-uid"), _txtAmount.getValue())) {
                                        $_ctrlPayment.setValue(($(ui.draggable)).data('uid'), false, true);
                                        //_lstStatement.html("");
                                        //_statementPageNo = 1;
                                        //childDialogs.loadStatement(_statementID);
                                    }
                                }
                            }

                            if (_cmbTransactionType.val() == "IS") {

                                if (loadFunctions.checkScheduleCondition(($(ui.draggable)).data('uid'), $(this).attr("data-uid"), _scheduleList.toString(), _txtAmount.getValue())) {
                                    //alert("done");
                                    var amount = 0;
                                    if (!isInList(parseInt(($(ui.draggable)).data('uid')), _scheduleList)) {
                                        //_scheduleList.push(parseInt(($(ui.draggable)).data('uid')));
                                        var _ds = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetConvtAmount", { recordID: parseInt(($(ui.draggable)).data('uid')), bnkAct_Id: _cmbBankAccount.val(), transcType: _cmbTransactionType.val(), stmtDate: _dtStatementDate.val() });
                                        if (_ds.length == 0 || _ds[0].DueAmount == 0) {
                                            VIS.ADialog.info("VA012_ConversionRateNotFound", null, "", "");
                                            return;
                                        }
                                        _scheduleList.push(parseInt(($(ui.draggable)).data('uid')));
                                        _scheduleDataList.push($(ui.draggable).attr('paymentdata'));
                                        /*change by pratap*/
                                        //not required
                                        //if (_txtAmount.getValue() == 0) {
                                        //    _scheduleAmount.push("0");
                                        //}
                                        //_scheduleAmount.push($(ui.draggable).attr('paymentamount'));
                                        _scheduleAmount.push(_ds[0].DueAmount);
                                        var amount = 0;
                                        if (Number(_scheduleAmount.length) > 0) {

                                            for (var i = 0; i < _scheduleAmount.length; i++) {
                                                amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                                            }
                                        }
                                        if (amount < 0) {
                                            _btnOut.removeClass("va012-inactive");
                                            _btnOut.addClass("va012-active");
                                            _btnOut.attr("v_active", "1");
                                            _btnIn.removeClass("va012-active");
                                            _btnIn.addClass("va012-inactive");
                                            _btnIn.attr("v_active", "0");
                                        }
                                        else {
                                            _btnIn.removeClass("va012-inactive");
                                            _btnIn.addClass("va012-active");
                                            _btnIn.attr("v_active", "1");
                                            _btnOut.removeClass("va012-active");
                                            _btnOut.addClass("va012-inactive");
                                            _btnOut.attr("v_active", "0");
                                        }
                                        _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                                        //_txtTrxAmt.val((amount).toFixed(_stdPrecision));
                                        //_txtTrxAmt.trigger('change');
                                        /*change by pratap*/
                                        loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "IS");
                                    }
                                    else {
                                        VIS.ADialog.info("VA012_AlreadySelected", null, "", "");
                                        return;
                                    }
                                    _txtPaymentSchedule.val(_scheduleDataList.toString());
                                    //repeated code in above line not reqauired again
                                    //var amount = 0;
                                    //for (var i = 0; i < _scheduleAmount.length; i++) {
                                    //    amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                                    //}
                                    _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                                    _txtTrxAmt.getControl().trigger('blur');
                                }
                                else {
                                    //alert("Notdone");
                                }
                            }
                            if (_cmbTransactionType.val() == "PO") {

                                if (loadFunctions.checkPrepayCondition(($(ui.draggable)).data('uid'), $(this).attr("data-uid"), _prepayList.toString(), _txtAmount.getValue())) {
                                    //alert("done");
                                    //newRecordForm.prepayRefresh();
                                    //if (!isInList(parseInt(($(ui.draggable)).data('uid')), _prepayList)) {
                                    //    _prepayList.push(parseInt(($(ui.draggable)).data('uid')));
                                    //    _prepayDataList.push($(ui.draggable).attr('paymentdata'));

                                    //}
                                    //else {
                                    //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_AlreadySelected"), null, "", "");
                                    //}
                                    //_txtPrepayOrder.val(_prepayDataList.toString());
                                    $_ctrlOrder.setValue(($(ui.draggable)).data('uid'), false, true);
                                    loadFunctions.setInvoiceAndBPartner(($(ui.draggable)).data('uid'), "PO");
                                }
                                else {
                                    //alert("Notdone");

                                }


                            }

                            if (_cmbTransactionType.val() == "CO") {

                                if (($(ui.draggable)).data('uid') > 0) {

                                    if (($(ui.draggable)).hasClass("va012-green-color")) {
                                        VIS.ADialog.info("VA012_CashLineAlreadyMatchedOthrStmt", null, "", "");
                                        return;
                                    }
                                    if (loadFunctions.checkContraCondition(($(ui.draggable)).data('uid'), $(this).attr("data-uid"), _txtAmount.getValue())) {
                                        $_ctrlCashLine.setValue(($(ui.draggable)).data('uid'), false, true);
                                    }
                                }
                            }

                        }
                        _openingFromDrop = false;
                    }
                });
            },

            checkFormPaymentCondition: function (_paymentID, _amount) {

                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckFormPaymentCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _paymentID: _paymentID, _amount: _amount }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result._status == "Success") {
                            _status = true;
                            if (result._amount != null && result._amount != 0) {

                                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(result._amount.toFixed(_stdPrecision)));
                                if (_txtAmount.getValue() < 0) {
                                    _btnOut.removeClass("va012-inactive");
                                    _btnOut.addClass("va012-active");
                                    _btnOut.attr("v_active", "1");
                                    _btnIn.removeClass("va012-active");
                                    _btnIn.addClass("va012-inactive");
                                    _btnIn.attr("v_active", "0");
                                }
                                else {
                                    _btnIn.removeClass("va012-inactive");
                                    _btnIn.addClass("va012-active");
                                    _btnIn.attr("v_active", "1");
                                    _btnOut.removeClass("va012-active");
                                    _btnOut.addClass("va012-inactive");
                                    _btnOut.attr("v_active", "0");
                                }
                                // _txtTrxAmt.val((result._amount).toFixed(_stdPrecision));
                                //_txtTrxAmt.trigger('change');
                            }
                        }
                        else {
                            VIS.ADialog.info(result._status, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                return _status;
            },
            checkFormPrepayCondition: function (_orderID, _amount) {

                var _formBPartnerID = 0;
                if ($_ctrlBusinessPartner.value) {//will evaluate to true if value is not:  null, undefined, NaN, empty string (""), 0,false
                    _formBPartnerID = $_ctrlBusinessPartner.value;
                }
                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckFormPrepayCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _orderID: _orderID, _amount: _amount, _currencyId: _currencyId, _formBPartnerID: _formBPartnerID }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result._status == "Success") {
                            _status = true;
                            if (result._amount > 0) {

                                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(result._amount.toFixed(_stdPrecision)));
                                if (_txtAmount.getValue() < 0) {
                                    _btnOut.removeClass("va012-inactive");
                                    _btnOut.addClass("va012-active");
                                    _btnOut.attr("v_active", "1");
                                    _btnIn.removeClass("va012-active");
                                    _btnIn.addClass("va012-inactive");
                                    _btnIn.attr("v_active", "0");
                                }
                                else {
                                    _btnIn.removeClass("va012-inactive");
                                    _btnIn.addClass("va012-active");
                                    _btnIn.attr("v_active", "1");
                                    _btnOut.removeClass("va012-active");
                                    _btnOut.addClass("va012-inactive");
                                    _btnOut.attr("v_active", "0");
                                }
                                // _txtTrxAmt.val((result._amount).toFixed(_stdPrecision));
                                // _txtTrxAmt.trigger('change');
                            }
                        }
                        else {
                            VIS.ADialog.info(result._status, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                return _status;
            },
            checkInvoiceCondition: function (_invoiceID, _amount) {

                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckInvoiceCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _invoiceID: _invoiceID, _amount: _amount }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result == "Success") {
                            _status = true;

                        }
                        else {
                            VIS.ADialog.info(result, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                return _status;
            },
            //strictly advised that condider amount only in case of new record otherwise get from destination
            checkPaymentCondition: function (_dragSourceID, _dragDestinationID, _amount) {

                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckPaymentCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _dragSourceID: _dragSourceID, _dragDestinationID: _dragDestinationID, _amount: _amount, statmtDate: _dtStatementDate.val(), accountID: _cmbBankAccount.val() }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result._status == "Success") {
                            _status = true;
                            if (result._amount != null && result._amount != 0) {

                                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(result._amount.toFixed(_stdPrecision)));
                                if (_txtAmount.getValue() < 0) {
                                    _btnOut.removeClass("va012-inactive");
                                    _btnOut.addClass("va012-active");
                                    _btnOut.attr("v_active", "1");
                                    _btnIn.removeClass("va012-active");
                                    _btnIn.addClass("va012-inactive");
                                    _btnIn.attr("v_active", "0");
                                }
                                else {
                                    _btnIn.removeClass("va012-inactive");
                                    _btnIn.addClass("va012-active");
                                    _btnIn.attr("v_active", "1");
                                    _btnOut.removeClass("va012-active");
                                    _btnOut.addClass("va012-inactive");
                                    _btnOut.attr("v_active", "0");
                                }
                                _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(result._trxamount.toFixed(_stdPrecision)));
                                //_txtTrxAmt.trigger('change');
                            }
                        }
                        else {
                            VIS.ADialog.info(result._status, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                // alert(_dragSourceID + "," + _dragDestinationID + "-----" + _listToCheck + "-----" + _amount);
                return _status;
            },
            //strictly advised that condider amount only in case of new record otherwise get from destination
            checkScheduleCondition: function (_dragSourceID, _dragDestinationID, _listToCheck, _amount) {
                if (_currencyId == null || _currencyId == 0) {
                    VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                    return;
                }
                var _formBPartnerID = 0;
                if ($_ctrlBusinessPartner.value) {//will evaluate to true if value is not:  null, undefined, NaN, empty string (""), 0,false
                    _formBPartnerID = $_ctrlBusinessPartner.value;
                }

                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckScheduleCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _dragSourceID: _dragSourceID, _dragDestinationID: _dragDestinationID, _listToCheck: _listToCheck, _amount: _amount, _currencyId: _currencyId, _formBPartnerID: _formBPartnerID }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result == "Success") {
                            _status = true;
                        }
                        else {
                            VIS.ADialog.info(result, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                // alert(_dragSourceID + "," + _dragDestinationID + "-----" + _listToCheck + "-----" + _amount);
                return _status;
            },
            checkScheduleTotal: function (_listToCheck, _amount, _destinationID) {

                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckScheduleTotal",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _listToCheck: _listToCheck, _amount: _amount, _currencyId: _currencyId, _destinationID: _destinationID }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result == "Success") {
                            _status = true;
                        }
                        else {
                            VIS.ADialog.info(result, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                return _status;
            },
            //strictly advised that condider amount only in case of new record otherwise get from destination
            checkPrepayCondition: function (_dragSourceID, _dragDestinationID, _listToCheck, _amount) {

                var _formBPartnerID = 0;
                if ($_ctrlBusinessPartner.value) {//will evaluate to true if value is not:  null, undefined, NaN, empty string (""), 0,false
                    _formBPartnerID = $_ctrlBusinessPartner.value;
                }
                var _status = false;

                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckPrepayCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _dragSourceID: _dragSourceID, _dragDestinationID: _dragDestinationID, _listToCheck: _listToCheck, _amount: _amount, _currencyId: _currencyId, _formBPartnerID: _formBPartnerID, stateDate: _dtStatementDate.val() }),

                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result._status == "Success") {
                            _status = true;
                            if (result._amount > 0) {
                                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(result._amount.toFixed(_stdPrecision)));
                                if (_txtAmount.getValue() < 0) {
                                    _btnOut.removeClass("va012-inactive");
                                    _btnOut.addClass("va012-active");
                                    _btnOut.attr("v_active", "1");
                                    _btnIn.removeClass("va012-active");
                                    _btnIn.addClass("va012-inactive");
                                    _btnIn.attr("v_active", "0");
                                }
                                else {
                                    _btnIn.removeClass("va012-inactive");
                                    _btnIn.addClass("va012-active");
                                    _btnIn.attr("v_active", "1");
                                    _btnOut.removeClass("va012-active");
                                    _btnOut.addClass("va012-inactive");
                                    _btnOut.attr("v_active", "0");
                                }
                                //_txtTrxAmt.val((result._amount).toFixed(_stdPrecision));
                                //_txtTrxAmt.trigger('change');
                            }
                        }
                        else {
                            VIS.ADialog.info(result._status, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                // alert(_dragSourceID + "," + _dragDestinationID + "-----" + _listToCheck + "-----" + _amount);
                return _status;
            },

            //strictly advised that condider amount only in case of new record otherwise get from destination
            checkContraCondition: function (_dragSourceID, _dragDestinationID, _amount) {

                var _formBPartnerID = 0;
                if ($_ctrlBusinessPartner.value) {//will evaluate to true if value is not:  null, undefined, NaN, empty string (""), 0,false
                    _formBPartnerID = $_ctrlBusinessPartner.value;
                }
                var _status = false;

                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/CheckContraCondition",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _dragSourceID: _dragSourceID, _dragDestinationID: _dragDestinationID, _amount: _amount, _currencyId: _currencyId, _formBPartnerID: _formBPartnerID, stateDate: _dtStatementDate.val() }),

                    success: function (result) {
                        result = $.parseJSON(result);
                        if (result._status == "Success") {
                            _status = true;

                            _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(result._amount.toFixed(_stdPrecision)));
                            if (_txtAmount.getValue() < 0) {
                                _btnOut.removeClass("va012-inactive");
                                _btnOut.addClass("va012-active");
                                _btnOut.attr("v_active", "1");
                                _btnIn.removeClass("va012-active");
                                _btnIn.addClass("va012-inactive");
                                _btnIn.attr("v_active", "0");
                            }
                            else {
                                _btnIn.removeClass("va012-inactive");
                                _btnIn.addClass("va012-active");
                                _btnIn.attr("v_active", "1");
                                _btnOut.removeClass("va012-active");
                                _btnOut.addClass("va012-inactive");
                                _btnOut.attr("v_active", "0");
                            }

                        }
                        else {
                            VIS.ADialog.info(result._status, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                return _status;
            },

            matchByDrag: function (_dragPaymentID, _dragStatementID) {
                var _status = false;
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/MatchByDrag",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _dragPaymentID: _dragPaymentID, _dragStatementID: _dragStatementID }),
                    success: function (result) {

                        result = $.parseJSON(result);
                        if (result._status == "Success") {
                            _status = true;
                            if (result._amount != null && result._amount != 0) {
                                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(result._amount.toFixed(_stdPrecision)));
                                if (_txtAmount.getValue() < 0) {
                                    _btnOut.removeClass("va012-inactive");
                                    _btnOut.addClass("va012-active");
                                    _btnOut.attr("v_active", "1");
                                    _btnIn.removeClass("va012-active");
                                    _btnIn.addClass("va012-inactive");
                                    _btnIn.attr("v_active", "0");
                                }
                                else {
                                    _btnIn.removeClass("va012-inactive");
                                    _btnIn.addClass("va012-active");
                                    _btnIn.attr("v_active", "1");
                                    _btnOut.removeClass("va012-active");
                                    _btnOut.addClass("va012-inactive");
                                    _btnOut.attr("v_active", "0");
                                }
                                // _txtTrxAmt.val((result._amount).toFixed(_stdPrecision));
                                // _txtTrxAmt.trigger('change');

                                _txtCharge.trigger("focus");

                            }
                            //_lstStatement.html("");
                            //_statementPageNo = 1;
                            //childDialogs.loadStatement(_statementID);
                        }
                        else {
                            VIS.ADialog.info(result._status, null, "", "");
                            _status = false;
                        }
                    },
                    error: function () {
                        VIS.ADialog.info("error", null, "", "");
                        _status = false;
                    }
                });
                return _status;
            }


        };
        this.vetoablechange = function (evt) {
            if (evt.propertyName == "VA012_txtDifference_" + $self.windowNo + "") {
                _txtDifference.setValue(VIS.Utility.Util.getValueOfDecimal(evt.newValue.toFixed(_stdPrecision)));
            }
            else if (evt.propertyName == "VA012_txtTaxAmount_" + $self.windowNo + "") {
                _txtTaxAmount.setValue(VIS.Utility.Util.getValueOfDecimal(evt.newValue.toFixed(_stdPrecision)));
            }
            else if (evt.propertyName == "VA012_txtTrxAmt_" + $self.windowNo + "") {
                _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(evt.newValue.toFixed(_stdPrecision)));
            }
            else if (evt.propertyName == "VA012_txtAmount_" + $self.windowNo + "") {
                _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(evt.newValue.toFixed(_stdPrecision)));
            }
        };
        /*
         * to get list of Matching Base List data
         * */
        function getMatchingBaseData(_cmbMatchingBase) {
            $.ajax({
                type: 'POST',
                url: VIS.Application.contextUrl + "VA012/BankStatement/getMatchRecData",
                contentType: "application/json; charset=utf-8",
                success: function (data) {
                    _cmbMatchingBase.html("");
                    _cmbMatchingBase.append("<option value=0 >-</option>");
                    data = JSON.parse(data);
                    if (data != null && data.length > 0) {
                        for (var i = 0; i < data.length; i++) {
                            _cmbMatchingBase.append("<option value=" + data[i].Value + ">" + data[i].Name + "</option>");
                        }
                    }
                },
                error: function (data) { VIS.ADialog.info(data, null, "", ""); }
            });

        };

        function GetMatchStatGrid($_divMatchingBase, data) {
            //$divCart.append($divCartMain);

            if (w2ui['VA012_gridPayment_' + $self.windowNo] != null) {
                w2ui['VA012_gridPayment_' + $self.windowNo].destroy();
            }

            var cartGrid = null;
            cartGrid = $_divMatchingBase.w2grid({
                name: 'VA012_gridPayment_' + $self.windowNo,
                recordHeight: 25,
                show: {
                    toolbar: false,  // indicates if toolbar is v isible
                    //columnHeaders: true,   // indicates if columns is visible
                    //lineNumbers: true,  // indicates if line numbers column is visible
                    selectColumn: false,  // indicates if select column is visible
                    toolbarReload: false,   // indicates if toolbar reload button is visible
                    toolbarColumns: false,   // indicates if toolbar columns button is visible
                    toolbarSearch: false,   // indicates if toolbar search controls are visible
                    toolbarAdd: false,   // indicates if toolbar add new button is visible
                    toolbarDelete: false,   // indicates if toolbar delete button is visible
                    toolbarSave: false,   // indicates if toolbar save button is visible
                    //selectionBorder: false,	 // display border arround selection (for selectType = 'cell')
                    //recordTitles: false	 // indicates if to define titles for records
                },
                records: [],
                columns: [
                    { field: "_trxDate", caption: "Tran. Date", sortable: false, size: '80px', display: false },
                    { field: "_trxNo", caption: 'Auth Code/ Trx no.', sortable: false, size: '30%', hidden: false },
                    { field: "_salesAmt", caption: 'Trx Amount', sortable: false, size: '15%', hidden: false, style: 'text-align: right' },
                    { field: "_netAmt", caption: 'Statement Amount', sortable: false, size: '15%', hidden: false, style: 'text-align: right' },
                    { field: "_difference", caption: 'Difference', sortable: false, size: '15%', hidden: false, style: 'text-align: right' },
                    { field: "_chargeType", caption: 'Charge Type', sortable: false, size: '80px', display: true },
                    { field: "_taxRate", caption: 'Tax Rate', sortable: false, size: '80px', display: true },
                    { field: "_taxAmt", caption: 'Tax Amt', sortable: false, size: '15%', hidden: false, style: 'text-align: right' },
                    //{ field: "Pay_ID", caption: 'Payment ID', sortable: false, size: '80px', display: true },
                    //{ field: "Statement_ID", caption: 'Statement ID', sortable: false, size: '80px', display: true }       
                ],

                onChange: function (event) {
                },

                onClick: function (event) {

                },
                onDelete: function (event) {

                },
                onSubmit: function (event) {

                },

            });

            cartGrid.add(data);

        };

        //End Load All Functions

        //Load Child Dialogs
        var childDialogs = {

            //Load Statement Dialog
            statementDialog: function () {
                var STAT_cmbBank = null;
                var STAT_cmbBankAccount = null;
                var STAT_cmbBankAccountClassName = null;
                //variable declaration  
                var STAT_statementDate = null;

                var STAT_cmbBankAccountCharges = null;
                var Bank_Charge_ID = null;


                var STAT_txtStatementNo = null;
                var STAT_ctrlLoadFile = null;
                var _result = null;
                $statement = $("<div class='va012-popform-content'>");
                var _statement = "";
                _statement = "<div>"
                    + "<div class='va012-left-btn'>"
                    + "<a id='VA012_STAT_tabFile_" + $self.windowNo + "' class='btn va012-blueBtn' activestatus='1'>" + VIS.Msg.getMsg("VA012_File") + "</a>"
                    // + "<a id='VA012_STAT_tabSchedule_" + $self.windowNo + "' class='btn va012-grayBtn' activestatus='0' >" + VIS.Msg.getMsg("VA012_Schedule") + "</a>"
                    + "</div></div>"

                    + "<div style='border: 2px solid rgba(var(--v-c-primary), 1);padding: 10px; float: left;'>"
                    + "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_Bank") + "</label>"
                    + "<select id='VA012_STAT_cmbBank_" + $self.windowNo + "' class='vis-ev-col-readonly' disabled>" //Made readonly
                    + "</select></div> "

                    + "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_BankAccount") + "</label>"
                    + "<select id='VA012_STAT_cmbBankAccount_" + $self.windowNo + "' class='vis-ev-col-readonly' disabled>" //Made readonly
                    + "</select></div>"
                    /*Added New Parameter for information purpose the statement date which was selected on form*/
                    + '<div class=va012-popform-data>'
                    + '<label id=VA012_STAT_lblStatementDate_' + $self.windowNo + '>' + VIS.Msg.getMsg("VA012_StatementDate") + '</label>'
                    + '<input type=date max="9999-12-31" class=vis-ev-col-readonly disabled id=VA012_STAT_statementDate_' + $self.windowNo + '>'
                    + '</div>'
                    + "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_ClassName") + '<sup style="color: red;">*</sup></label>'
                    + "<select id='VA012_STAT_cmbBankAccountClassName_" + $self.windowNo + "'>"
                    + "</select></div>"


                    //+ "<div class='va012-popform-data' id='VA012_BankChargeDiv' style='visibility: hidden;'>"
                    //+ "<label>" + VIS.Msg.getMsg("VA012_BankCharges") + "</label>"
                    //+ "<div><select id='VA012_STAT_cmbBankAccountCharges_" + $self.windowNo + "'>"
                    //+ "</select></div>"


                    + "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_StatementNumber") + '<sup style="color: red;">*</sup></label>'
                    + "<input type='text' id='VA012_STAT_txtStatementNo_" + $self.windowNo + "' /> </div>"

                    //+ "<div class='va012-popform-data'>"
                    //+ "<label>" + VIS.Msg.getMsg("VA012_Format") + "</label>"
                    //+ "<select>"
                    //+ "<option>xlsx</option>"
                    //+ "<option>csv</option>"
                    //+ "</select></div>"

                    + "<div class='va012-popform-data va012-pop-file'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_SelectFile") + '<sup style="color: red;">*</sup></label>'
                    + "<input type='text' id='VA012_ctrlLoadFileTxt_" + $self.windowNo + "' disabled>"
                    + "<div class='va012-file-upload'>"
                    + "<label for='file-input' class='va012-file-label'>" + VIS.Msg.getMsg("VA012_Browse") + "</label>"
                    + "<input id='VA012_ctrlLoadFile_" + $self.windowNo + "' type='file' accept='.csv,text/plain,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet,application/vnd.ms-excel'/>"
                    + "</div></div>"
                    + "</div>";

                $statement.append(_statement);

                STAT_getControls();
                STAT_loadBank();
                STAT_loadBankAccount();
                loadBankAccountClasses();
                //set statement date parameter whcih was already selected on header
                STAT_statementDate.val(Globalize.format(new Date(_statementDate.val()), "yyyy-MM-dd"));


                loadBankAccountCharges();
                var statementDialog = new VIS.ChildDialog();
                statementDialog.setContent($statement);
                statementDialog.setTitle(VIS.Msg.getMsg("VA012_LoadStatement"));
                statementDialog.setWidth("500px");
                statementDialog.setEnableResize(false);
                statementDialog.setModal(true);
                statementDialog.show();

                function STAT_getControls() {
                    STAT_cmbBank = $statement.find("#VA012_STAT_cmbBank_" + $self.windowNo);
                    STAT_cmbBankAccount = $statement.find("#VA012_STAT_cmbBankAccount_" + $self.windowNo);
                    STAT_cmbBankAccountClassName = $statement.find("#VA012_STAT_cmbBankAccountClassName_" + $self.windowNo);
                    //get control from dialog design
                    STAT_statementDate = $statement.find("#VA012_STAT_statementDate_" + $self.windowNo);


                    //// STAT_cmbBankAccountCharges = $statement.find("#VA012_STAT_cmbBankAccountCharges_" + $self.windowNo);

                    STAT_txtStatementNo = $statement.find("#VA012_STAT_txtStatementNo_" + $self.windowNo);
                    STAT_ctrlLoadFile = $statement.find("#VA012_ctrlLoadFile_" + $self.windowNo);
                    STAT_ctrlLoadFileTxt = $statement.find("#VA012_ctrlLoadFileTxt_" + $self.windowNo);
                    _tabFile = $statement.find("#VA012_STAT_tabFile_" + $self.windowNo);
                    _tabSchedule = $statement.find("#VA012_STAT_tabSchedule_" + $self.windowNo);
                    _cmbBankAccountClasses = $statement.find("#VA012_STAT_cmbBankAccountClassName_" + $self.windowNo);


                    ////_cmbBankAccountClasses.on('change', function () {
                    ////    var str = this.value;
                    ////    if (str != "0") {
                    ////        var clsName = str.substr(0, str.indexOf("ImportStatement_") + 15);
                    ////        if (clsName.toLowerCase() == "va012.models.va012_trxno.importstatement")
                    ////            document.getElementById('VA012_BankChargeDiv').style.display = "block";
                    ////        else
                    ////            document.getElementById('VA012_BankChargeDiv').style.display = "none";
                    ////    }
                    ////    else {
                    ////        document.getElementById('VA012_BankChargeDiv').style.display = "none";
                    ////    }
                    ////});

                };

                function STAT_loadBank() {
                    STAT_cmbBank.html("");
                    STAT_cmbBank.append("<option value=" + _cmbBank.val() + ">" + _cmbBank.children()[_cmbBank[0].selectedIndex].text + "</option>");
                    STAT_cmbBank.prop('selectedIndex', 0);
                };

                function STAT_loadBankAccount() {
                    STAT_cmbBankAccount.html("");
                    STAT_cmbBankAccount.append("<option value=" + _cmbBankAccount.val() + ">" + _cmbBankAccount.children()[_cmbBankAccount[0].selectedIndex].text + "</option>");
                    STAT_cmbBankAccount.prop('selectedIndex', 0);
                };

                //function STAT_loadBankAccountClasses() {
                //    STAT_cmbBankAccountClassName.html("");
                //    STAT_cmbBankAccountClassName.append("<option value=" + _cmbBankAccountClasses.val() + ">" + _cmbBankAccountClasses.children()[_cmbBankAccountClasses[0].selectedIndex].text + "</option>");
                //    STAT_cmbBankAccountClassName.prop('selectedIndex', 0);
                //}

                function loadBankAccountClasses() {

                    // var _sql = "SELECT VA012_BANKSTATEMENTCLASSNAME as NAME, VA012_BANKSTATEMENTCLASS_ID FROM VA012_BANKSTATEMENTCLASS WHERE C_BANKACCOUNT_ID = " + _cmbBankAccount.val();
                    var _sql = " SELECT BSC.VA012_BANKSTATEMENTCLASSNAME AS NAME, CONCAT(CONCAT(SC.NAME,'_'),VA012_BANKSTATEMENTCLASS_ID) AS VA012_BANKSTATEMENTCLASS_ID FROM VA012_BANKSTATEMENTCLASS BSC INNER JOIN VA012_STATEMENTCLASS SC ON BSC.VA012_STATEMENTCLASS_ID=SC.VA012_STATEMENTCLASS_ID WHERE C_BANKACCOUNT_ID = " + _cmbBankAccount.val();
                    var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadBankAccountClasses);
                    function callbackloadBankAccountClasses(_ds) {
                        STAT_cmbBankAccountClassName.html("");
                        STAT_cmbBankAccountClassName.append("<option value=0 ></option>");
                        if (_ds != null) {
                            for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                                var str = _ds.tables[0].rows[i].cells.va012_bankstatementclass_id;
                                //var id = str.substr(str.indexOf("ImportStatement_") + 16);
                                //STAT_cmbBankAccountClassName.append("<option value=" + VIS.Utility.Util.getValueOfInt(str) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                                STAT_cmbBankAccountClassName.append("<option value=" + str + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                            }
                        }
                        _ds.dispose();
                        _ds = null;
                        _sql = null;
                        STAT_cmbBankAccountClassName.prop('selectedIndex', 0);
                    }
                };

                function loadBankAccountCharges() {
                    var _sql = "SELECT C_CHARGE_ID, NAME FROM C_CHARGE WHERE ISACTIVE = 'Y' AND AD_CLIENT_ID = " + VIS.Env.getCtx().getAD_Client_ID() + " AND ROWNUM = 1 ORDER BY NAME  ";
                    var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadBankAccountCharges);
                    function callbackloadBankAccountCharges(_ds) {
                        ////STAT_cmbBankAccountCharges.html("");
                        ////STAT_cmbBankAccountCharges.append("<option value=0 ></option>");
                        if (_ds != null) {
                            for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                                // STAT_cmbBankAccountCharges.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_charge_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                                Bank_Charge_ID = VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_charge_id);

                            }
                        }
                        _ds.dispose();
                        _ds = null;
                        _sql = null;
                        //STAT_cmbBankAccountCharges.prop('selectedIndex', 0);
                    }
                };

                //STAT_txtStatementNo.on("keypress", function (event) {
                //    if (event.which != 8 && event.which != 0 && (event.which < 48 || event.which > 57)) {
                //        return false;
                //    }
                //});
                STAT_ctrlLoadFile.on('change', function (evt) {

                    var file = this;

                    if (STAT_cmbBankAccount.val() != null && STAT_cmbBankAccount.val() > 0) {
                        _result = $.parseJSON(VA012.UploadExcel(file, null, null));
                        STAT_ctrlLoadFileTxt.val(_result._orgfilename);

                    }
                    else {
                        STAT_ctrlLoadFile = null;
                        VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");

                    }


                });
                statementDialog.onOkClick = function () {
                    //Clear Search Text on load of new statement                   
                    _txtSearch.val("");

                    //Load Bank Statement From File Selected
                    if (_tabFile.attr("activestatus") == "1") {
                        if (STAT_cmbBankAccount.val() == null || STAT_cmbBankAccount.val() == "" || STAT_cmbBankAccount.val() == "0") {
                            VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                            return false;
                        }

                        if (STAT_cmbBankAccountClassName.val() == null || STAT_cmbBankAccountClassName.val() == "" || STAT_cmbBankAccountClassName.val() == "0") {
                            VIS.ADialog.info("VA012_PleaseSelectClassFirst", null, "", "");
                            return false;
                        }

                        //COMMENTED DUE TO NEW CHANGE GIVEN BY RAJESH/ASHISH/ATUL UPDATED BY SUKHWINDER
                        ////if (document.getElementById('VA012_BankChargeDiv').style.display != "none") {
                        ////    if (STAT_cmbBankAccountCharges.val() == null || STAT_cmbBankAccountCharges.val() == "" || STAT_cmbBankAccountCharges.val() == "0") {
                        ////        VIS.ADialog.info(VIS.Msg.getMsg("VA012_PleaseSelectChargesFirst"), null, "", "");
                        ////        return false;
                        ////    }
                        ////}

                        if (STAT_txtStatementNo.val() == null || STAT_txtStatementNo.val() == "") {
                            VIS.ADialog.info("VA012_PleaseEnterStatementNo", null, "", "");
                            return false;
                        }

                        if (STAT_ctrlLoadFile.val() == "" == null || STAT_ctrlLoadFile.val() == "") {
                            VIS.ADialog.info("VA012_SelectFileFirst", null, "", "");
                            return false;
                        }


                        if (_result != null) {
                            if (_result._filename == null || _result._filename == "" || _result._path == null || _result._path == "") {

                                VIS.ADialog.info("VA012_ErrorInGettingFile", null, "", "");
                                return;
                            }
                            else if (_result._error != null && _result._error != "") {
                                VIS.ADialog.info(_result._error, null, "", "");
                                return;
                            }
                            else {
                                var _path = _result._path;
                                var _filename = _result._filename;
                                var _bankaccount = STAT_cmbBankAccount.val();
                                var _statementno = STAT_txtStatementNo.val();
                                var $_statement = $statement;
                                var _statementClassName = STAT_cmbBankAccountClassName.val();

                                var _statementCharges = Bank_Charge_ID;// STAT_cmbBankAccountCharges.val();



                                //busyIndicator($_statement, true, "absolute");
                                busyIndicator($root, true, "absolute");
                                window.setTimeout(function () {
                                    $.ajax({
                                        url: VIS.Application.contextUrl + "BankStatement/ImportStatement",
                                        type: "GET",
                                        datatype: "json",
                                        contentType: "application/json; charset=utf-8",
                                        async: false,
                                        data: ({ _path: _path, _filename: _filename, _bankaccount: _bankaccount, _bankAccountCurrency: _currencyId, _statementno: _statementno, _statementClassName: _statementClassName, _statementCharges: _statementCharges, statementDate: STAT_statementDate.val() }),
                                        success: function (result) {


                                            _statementID = result._statementID;
                                            if (_statementID != null && _statementID != "") {
                                                _statementLinesList = [];
                                                _lstStatement.html("");
                                                _statementPageNo = 1;
                                                //busyIndicator($_statement, false, "absolute");
                                                busyIndicator($root, false, "absolute");
                                                childDialogs.loadStatement(_statementID);
                                                newRecordForm.refreshForm();


                                            }
                                            else {
                                                if (result._error != null && result._error != "") {
                                                    //busyIndicator($_statement, false, "absolute");
                                                    busyIndicator($root, false, "absolute");
                                                    VIS.ADialog.info(result._error, null, "", "");

                                                }
                                            }
                                        },
                                        error: function () {

                                            //busyIndicator($_statement, false, "absolute");
                                            busyIndicator($root, false, "absolute");
                                            VIS.ADialog.info("error", null, "", "");

                                        }
                                    })
                                }, 2);
                            }
                        }
                    }
                    //End Load Bank Statement From File Selected

                    //End Bank Statement From File Path Scheduled
                    if (_tabSchedule.attr("activestatus") == "1") {
                        alert("Schedule");
                        return false;
                    }
                    //Bank Statement From File Path Scheduled
                };
                statementDialog.onCancelClick = function () {
                };
                statementDialog.onClose = function () {

                    statementDispose();

                };

                _tabFile.on(VIS.Events.onTouchStartOrClick, function () {
                    _tabFile.removeClass("va012-grayBtn");
                    _tabFile.addClass("va012-blueBtn");
                    _tabFile.attr("activestatus", "1");
                    _tabSchedule.removeClass("va012-blueBtn");
                    _tabSchedule.addClass("va012-grayBtn");
                    _tabSchedule.attr("activestatus", "0");
                });
                _tabSchedule.on(VIS.Events.onTouchStartOrClick, function () {
                    _tabSchedule.removeClass("va012-grayBtn");
                    _tabSchedule.addClass("va012-blueBtn");
                    _tabSchedule.attr("activestatus", "1");
                    _tabFile.removeClass("va012-blueBtn");
                    _tabFile.addClass("va012-grayBtn");
                    _tabFile.attr("activestatus", "0");
                });
                function statementDispose() {
                    _statement = null;
                    $statement = null;
                    STAT_cmbBank = null;
                    STAT_cmbBankAccount = null;
                    STAT_txtStatementNo = null;
                    STAT_ctrlLoadFile = null;
                    STAT_ctrlLoadFileTxt = null;
                    _tabSchedule = null;
                    _tabFile = null;
                    _result = null;
                }

                //_cmbBankAccountClasses.on('change', function (obj) {
                //    var str = obj.value;
                //    var clsName = str.substr(0, indexOf("ImportStatement_") + 16);                    
                //    if (clsName.toLowerCase() == "va012.models.va012_trxno.importstatement")
                //        document.getElementById('VA012_BankChargeDiv').style.display = "block";
                //});
            },

            LoadStatementsPages: function () {
                if (_txtSearch.val() != null && _txtSearch.val() != "") {
                    _SEARCHREQUEST = true;
                }
                else {
                    _SEARCHREQUEST = false;
                }
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/LoadStatementsPages",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    async: false,
                    data: ({ _cmbBankAccount: _cmbBankAccount.val(), _txtSearch: _txtSearch.val(), _statementPageNo: _statementPageNo, _PAGESIZE: _PAGESIZE, _SEARCHREQUEST: _SEARCHREQUEST }),
                    success: function (data) {
                        if (data != null && data != "") {
                            _statementPageCount = JSON.parse(data);
                        }
                    },
                })
            },
            loadStatement: function (_statementID) {


                // _statementLinesList = [];
                //var _amtReconciled = 0;
                //var _amtUnreconciled = 0;
                if (_txtSearch.val() != null && _txtSearch.val() != "") {
                    _SEARCHREQUEST = true;
                }
                else {
                    _SEARCHREQUEST = false;
                }

                busyIndicator($(_lstStatement), true, "inherit");
                //_secUnreconciled.html("");
                //_secReconciled.html("");

                //var _sqlCon = "SELECT NVL(ROUND(SUM( "
                //    + " CASE "
                //    + " WHEN (BSL.C_PAYMENT_ID IS NOT NULL OR BSL.C_CHARGE_ID IS NOT NULL OR BSL.C_CASHLINE_ID IS NOT NULL) "
                //    + " THEN ( "
                //    + " CASE "
                //    + " WHEN ( BSL.C_CURRENCY_ID! =BCURR.C_CURRENCY_ID) "
                //    + " THEN BSL.StmtAmt*( "
                //    + "  CASE "
                //    + " WHEN CCR.MULTIPLYRATE IS NOT NULL "
                //    + " THEN CCR.MULTIPLYRATE "
                //    + "  ELSE CCR1.DIVIDERATE "
                //    + "  END) "
                //    + " ELSE BSL.StmtAmt "
                //    + " END) "
                //    + " END),NVL(CURR.StdPrecision,2)),0) AS RECONCILED, "
                //    + " NVL(ROUND(SUM( "
                //    + " CASE "
                //    + " WHEN (BSL.C_PAYMENT_ID IS NULL AND BSL.C_CHARGE_ID IS NULL AND  BSL.C_CASHLINE_ID IS NULL) "
                //    + " THEN ( "
                //    + " CASE "
                //    + " WHEN ( BSL.C_CURRENCY_ID! = BCURR.C_CURRENCY_ID) "
                //    + " THEN BSL.StmtAmt*( "
                //    + " CASE  "
                //    + "  WHEN CCR.MULTIPLYRATE IS NOT NULL "
                //    + " THEN CCR.MULTIPLYRATE "
                //    + "  ELSE CCR1.DIVIDERATE "
                //    + "  END) "
                //    + " ELSE BSL.StmtAmt "
                //    + " END) "
                //    + " END),NVL(CURR.StdPrecision,2)),0) AS UNRECONCILED,BCURR.ISO_CODE AS BASECURRENCY "
                //    + "  FROM C_BANKSTATEMENT BS"
                //    + "   INNER JOIN C_BANKSTATEMENTLINE BSL"
                //    + "  ON BS.C_BANKSTATEMENT_ID=BSL.C_BANKSTATEMENT_ID"
                //    + "  LEFT JOIN C_BPARTNER BP"
                //    + "  ON BSL.C_BPARTNER_ID     =BP.C_BPARTNER_ID"





                //    ///
                //    + "  LEFT JOIN C_CURRENCY CURR "
                //    + "  ON BSL.C_CURRENCY_ID=CURR.C_CURRENCY_ID "

                //    + " INNER JOIN AD_CLIENTINFO CINFO  "
                //    + " ON CINFO.AD_CLIENT_ID =BSL.AD_CLIENT_ID  "
                //    + " INNER JOIN C_ACCTSCHEMA AC  "
                //    + " ON AC.C_ACCTSCHEMA_ID =CINFO.C_ACCTSCHEMA1_ID  "
                //    + " LEFT JOIN C_CURRENCY BCURR  "
                //    + " ON " + _currencyId + " =BCURR.C_CURRENCY_ID  "
                //    + " LEFT JOIN C_CONVERSION_RATE CCR  "
                //    + " ON (CCR.C_CURRENCY_ID   =BSL.C_CURRENCY_ID  "
                //    + " AND CCR.ISACTIVE        ='Y'  "
                //    + " AND CCR.C_CURRENCY_TO_ID=" + _currencyId
                //    + " AND CCR.AD_CLIENT_ID    =BSL.AD_CLIENT_ID "
                //    + " AND CCR.AD_ORG_ID      IN (BSL.AD_ORG_ID,0) "
                //    + " AND SYSDATE BETWEEN CCR.VALIDFROM AND CCR.VALIDTO)  "


                //    + " LEFT JOIN C_CONVERSION_RATE CCR1 "
                //    + " ON (CCR1.C_CURRENCY_ID   =" + _currencyId
                //    + " AND CCR1.C_CURRENCY_TO_ID=BSL.C_CURRENCY_ID "
                //    + " AND CCR1.ISACTIVE        ='Y' "
                //    + " AND CCR1.AD_CLIENT_ID    =BSL.AD_CLIENT_ID "
                //    + " AND CCR1.AD_ORG_ID      IN (BSL.AD_ORG_ID,0) "
                //    + " AND SYSDATE BETWEEN CCR1.VALIDFROM AND CCR1.VALIDTO) "

                //    ////


                //    + " WHERE BS.ISACTIVE='Y' AND BS.C_BANKACCOUNT_ID= " + _cmbBankAccount.val() + " AND BS.DOCSTATUS !='VO' AND BS.AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID();

                //if (VIS.Env.getCtx().getAD_Org_ID() != 0) {
                //    _sqlCon += " AND BS.AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID();
                //}

                //if (_SEARCHREQUEST) {

                //    _sqlCon += " AND (UPPER(BP.NAME) LIKE UPPER('%" + _txtSearch.val() + "%')"
                //        + " OR UPPER(BSL.DESCRIPTION) LIKE UPPER('%" + _txtSearch.val() + "%')"
                //        + " OR UPPER(BS.NAME) LIKE UPPER('%" + _txtSearch.val() + "%')"
                //        + " OR UPPER(BSL.StmtAmt) LIKE UPPER('%" + _txtSearch.val() + "%')"
                //        + " OR UPPER(BSL.TRXNO) LIKE UPPER('%" + _txtSearch.val() + "%')"
                //        + " OR UPPER(BSL.TrxAmt) LIKE UPPER('%" + _txtSearch.val() + "%'))";
                //}
                //_sqlCon += " GROUP BY BCURR.ISO_CODE ,NVL(CURR.StdPrecision,2)";

                window.setTimeout(function () {
                    //var _dsCon = VIS.DB.executeDataSet(_sqlCon.toString(), null, callbackloadConUncon);
                    $.ajax({
                        url: VIS.Application.contextUrl + "BankStatement/LoadConciledOrUnConciledStatements",
                        type: "GET",
                        datatype: "json",
                        contentType: "application/json; charset=utf-8",
                        async: false,
                        data: ({ _cmbBankAccount: _cmbBankAccount.val(), _txtSearch: _txtSearch.val(), _currencyID: _currencyId != null ? _currencyId : 0, _searchRequest: _SEARCHREQUEST }),
                        success: function (data) {
                            var _dsCon = $.parseJSON(data);
                            //if (_dsCon != null && _dsCon != "") {
                            callbackloadConUncon(_dsCon);
                            busyIndicator($(_lstStatement), false, "inherit");
                            //}
                        },
                        error: function () {
                            busyIndicator($(_lstStatement), false, "inherit");
                        }
                    })
                }, 2);

                function callbackloadConUncon(_dsCon) {

                    if (_dsCon != null && _dsCon.length > 0) {
                        _secUnreconciled.html("");
                        _secReconciled.html("");
                        _secReconciled.append("<p>" + VIS.Msg.getMsg("VA012_Reconciled") + "</p><p style='margin-top: 4px;'>" + VIS.Msg.getMsg("VA012_Unreconciled") + "</p>")
                        _secUnreconciled.append("<span style='padding-bottom: 2px;' class='va012-amount va012-font-green'><span class='va012-base-curr'>" + _dsCon[0].basecurrency + "</span> " + parseFloat(_dsCon[0].reconciled).toLocaleString(navigator.language, { minimumFractionDigits: _stdPrecision, maximumFractionDigits: _stdPrecision }) + "</span><span style='padding-bottom: 2px;' class='va012-amount va012-font-red'> <span class='va012-base-curr'>" + _dsCon[0].basecurrency + "</span> " + parseFloat(_dsCon[0].unreconciled).toLocaleString(navigator.language, { minimumFractionDigits: _stdPrecision, maximumFractionDigits: _stdPrecision }) + "</span>");
                        //_dsCon.dispose();
                        //_dsCon = null;
                        //_sqlCon = null;

                    }
                    else {
                        _secUnreconciled.html("");
                        _secReconciled.html("");
                        _secReconciled.append("<p>" + VIS.Msg.getMsg("VA012_Reconciled") + "</p><p style='margin-top: 4px;'>" + VIS.Msg.getMsg("VA012_Unreconciled") + "</p>")
                        _secUnreconciled.append("<span style='padding-bottom: 2px;' class='va012-amount va012-font-green'><span class='va012-base-curr'>" + _clientBaseCurrency + "</span> 0</span><span style='padding-bottom: 2px;' class='va012-amount va012-font-red'><span class='va012-base-curr'>" + _clientBaseCurrency + "</span> 0</span>");
                    }

                    childDialogs.setStatementListHeight();

                }






                // _lstStatement.html("");
                var status = "va012-red-color";




                ////////////////////
                window.setTimeout(function () {
                    $.ajax({
                        url: VIS.Application.contextUrl + "BankStatement/LoadStatements",
                        type: "GET",
                        datatype: "json",
                        contentType: "application/json; charset=utf-8",
                        async: false,
                        data: ({ _cmbBankAccount: _cmbBankAccount.val(), _txtSearch: _txtSearch.val(), _statementPageNo: _statementPageNo, _PAGESIZE: _PAGESIZE, _SEARCHREQUEST: _SEARCHREQUEST }),
                        success: function (data) {
                            if (data != null && data != "") {

                                callbackloadStatement(data);
                                busyIndicator($(_lstStatement), false, "inherit");
                            }
                        },
                        error: function () {
                            busyIndicator($(_lstStatement), false, "inherit");
                        }
                    })
                }, 2);


                //VIS.DB.executeDataSetPaging(_sql.toString(), _statementPageNo, _PAGESIZE, null, callbackloadStatement);
                function callbackloadStatement(data) {
                    data = $.parseJSON(data);
                    var _StatementsHTML = "";


                    if (data != null && data.length > 0) {
                        for (var i = 0; i < data.length; i++) {
                            _StatementsHTML = "";
                            if (data[i].docstatus == "CO" || data[i].docstatus == "CL" || data[i].docstatus == "RE") {
                                status = "va012-gray-color";
                            }
                            else {
                                if (data[i].c_payment_id == null || data[i].c_payment_id == "0" || data[i].c_payment_id == 0) {

                                    //Set Green Color if charge amount = Statement amount suggested by Ashish

                                    //if ((data[i].c_charge_id != null || data[i].c_charge_id == "") && (data[i].trxno != null || data[i].trxno == "")) {
                                    //    status = "va012-red-color";
                                    //}
                                    if ((data[i].c_cashline_id != null && data[i].c_cashline_id != "0" && data[i].c_cashline_id != 0) /*&& data[i].usenexttime == true*/) {
                                        status = "va012-green-color";
                                    }
                                    else if ((data[i].c_charge_id != null && data[i].c_charge_id != "0" && data[i].c_charge_id != 0) /*&& data[i].usenexttime == true*/) {

                                        if (data[i].STMTAMT == data[i].trxamount)
                                            status = "va012-green-color";
                                        else
                                            status = "va012-red-color";
                                    }
                                    else if ((data[i].c_charge_id == null || data[i].c_charge_id == "") && (data[i].trxno == null || data[i].trxno == "")) {
                                        status = "va012-red-color";
                                    }


                                    else {
                                        status = "va012-red-color";
                                    }
                                    //_amtUnreconciled += data[i].trxamount;
                                }
                                else {
                                    status = "va012-green-color";
                                    //_amtReconciled += data[i].trxamount;
                                }
                            }

                            _StatementsHTML = '<div  data-uid="' + data[i].c_bankstatementline_id + '" class="va012-right-data-wrap ' + status + '">'
                                + '<div class="va012-statement-wrap">'
                                + '<div class="va012-fl-padd">'
                                + '<div class="col-md-4 col-sm-4 va012-padd-0">'
                                + '<div class="va012-form-check">'
                                + ' <input type="checkbox" data-uid="' + data[i].c_bankstatementline_id + '" >'
                                // + ' <div class="va012-form-text"> <span style="background: #999;color: white; padding: 3px;margin-left: 2px;">' + data[i].page + '/' + data[i].line + '</span>'
                                + ' <div class="va012-form-text"> <span style="background: rgba(var(--v-c-on-secondary), .4);color: rgba(var(--v-c-on-primary), 1); padding: 3px;margin-left: 2px;">' + data[i].statementno + '/' + data[i].page + '/' + data[i].line + '</span>'
                                + '<label>' + data[i].currency + ' ' + parseFloat(data[i].trxamount).toLocaleString(navigator.language, { minimumFractionDigits: _stdPrecision, maximumFractionDigits: _stdPrecision }) + '</label>';

                            //if (data[i].isconverted == "Y") {
                            //    _StatementsHTML += '<span>' + data[i].basecurrency + ' ' + Globalize.format(data[i].convertedamount, "N") + '</span>';
                            //}

                            if (data[i].trxno == "" || data[i].trxno == null) {
                                _StatementsHTML += '</div>'
                                    + '</div>'
                                    + '<!-- end of form-group -->'
                                    + ' </div>'
                                    + '<!-- end of col -->'
                                    + '<div class="col-md-4 col-sm-4 va012-padd-0 va012-width-sm">'
                                    + '<div class="va012-form-check">'
                                    + '<div class="va012-pay-text">'
                                    + ' <p>' + VIS.Utility.encodeText(data[i].description) + '</p>'
                                    //+ ' <span>' + VIS.Utility.encodeText(data[i].bpgroup) + '</span>'
                                    + '<span>' + VIS.Utility.encodeText(data[i].invoiceno) + '</span>'
                                    + ' </div>'
                                    + '</div>'
                                    + ' <!-- end of form-group -->'
                                    + '</div>'
                                    + ' <!-- end of col -->'


                                    + ' <div class="col-md-2 col-sm-2 va012-padd-0">'
                                    + '<div class="va012-form-check">'
                                    + '<div class="va012-pay-text">'
                                    + ' <p> </p>'
                                    + ' </div>'
                                    + '</div>'
                                    + ' <!-- end of form-group -->'
                                    + ' </div>'
                                    + ' <!-- end of col -->'



                                    + ' <div class="col-md-1 col-sm-1 va012-padd-0">'
                                    + '<div class="va012-form-check">'
                                    + '<div class="va012-pay-text">'
                                    + ' <p><span data-uid="' + data[i].c_bankstatementline_id + '" class="glyphicon glyphicon-edit" title=' + VIS.Msg.getMsg("EditRecord") + '></span> <span data-uid="' + data[i].c_bankstatement_id + '" class="glyphicon glyphicon-zoom-in" title=' + VIS.Msg.getMsg("ZoomToRecord") + '></span> </p>'
                                    + ' </div>'
                                    + '</div>'
                                    + ' <!-- end of form-group -->'
                                    + ' </div>'
                                    + ' <!-- end of col -->'


                                    + ' <div class="col-md-1 col-sm-1 va012-padd-0 va012-width-xs">'
                                    + '<div class="va012-form-data">';
                            }
                            else {
                                _StatementsHTML += '</div>'
                                    + '</div>'
                                    + '<!-- end of form-group -->'
                                    + ' </div>'
                                    + '<!-- end of col -->'
                                    + '<div class="col-md-4 col-sm-4 va012-padd-0 va012-width-sm">'
                                    + '<div class="va012-form-check">'
                                    + '<div class="va012-pay-text">'
                                    + ' <p>' + VIS.Utility.encodeText(data[i].description) + '</p>'
                                    + ' <span>' + VIS.Utility.encodeText(data[i].bpgroup) + '</span>'
                                    + '<span>' + VIS.Utility.encodeText(data[i].invoiceno) + '</span>'
                                    + ' </div>'
                                    + '</div>'
                                    + ' <!-- end of form-group -->'
                                    + '</div>'
                                    + ' <!-- end of col -->'


                                    + ' <div class="col-md-2 col-sm-2 va012-padd-0">'
                                    + '<div class="va012-form-check">'
                                    + '<div class="va012-pay-text">'
                                    + ' <p>' + VIS.Utility.encodeText(data[i].trxno) + '</p>'
                                    + ' </div>'
                                    + '</div>'
                                    + ' <!-- end of form-group -->'
                                    + ' </div>'
                                    + ' <!-- end of col -->'



                                    + ' <div class="col-md-1 col-sm-1 va012-padd-0">'
                                    + '<div class="va012-form-check">'
                                    + '<div class="va012-pay-text">'
                                    + ' <p><span data-uid="' + data[i].c_bankstatementline_id + '" class="glyphicon glyphicon-edit"></span> <span data-uid="' + data[i].c_bankstatement_id + '" class="glyphicon glyphicon-zoom-in"></span> </p>'
                                    + ' </div>'
                                    + '</div>'
                                    + ' <!-- end of form-group -->'
                                    + ' </div>'
                                    + ' <!-- end of col -->'


                                    + ' <div class="col-md-1 col-sm-1 va012-padd-0 va012-width-xs">'
                                    + '<div class="va012-form-data">';
                            }

                            //if (data[i].imageurl != null && data[i].imageurl != "") {
                            //    _StatementsHTML += '    <img src="' + data[i].imageurl + '" alt="">';
                            //}
                            ////else if (data[i].binarydata != null) {

                            ////    _StatementsHTML += '    <img src="data:image/png;base64,' + data[i].binarydata + '" alt="">';
                            ////}
                            //else {
                            //    //_StatementsHTML += ' <img src="Areas/VA012/Images/defaultBP.png" alt="">';
                            //    _StatementsHTML += '<i class="vis-chatimgwrap fa fa-user"></i>';
                            //}

                            _StatementsHTML += ' </div>'
                                + ' <!-- end of form-group -->'
                                + ' </div>'
                                + ' <!-- end of col -->'
                                + ' </div>'
                                + ' <!-- end of row -->'
                                + '</div>'
                                + '<!-- end of payment-wrap -->'
                                + '</div>'
                                + '<!-- end of right-data-wrap -->';
                            _lstStatement.append(_StatementsHTML);

                        }
                    }
                    _sql = null;

                    childDialogs.setStatementListHeight();
                    loadFunctions.dropPayments();

                }

            },
            setStatementListHeight: function () {
                //$(".va012-content-area").height($("#VA012_mainContainer_" + $self.windowNo).height() - 20);
                //$(".va012-right-content").height($(".va012-right-wrap").height() - $(".va012-right-top").height() - 18)
                $("#VA012_contentArea_" + $self.windowNo).height($("#VA012_mainContainer_" + $self.windowNo).height() - 20);
                $("#VA012_lstStatement_" + $self.windowNo).height($("#VA012_rightWrap_" + $self.windowNo).height() - $("#VA012_rightTop_" + $self.windowNo).height() - 18)
            },



            statementOpenEdit: function (_bankStatementLineID, _dragPaymentID) {
                _btnNewRecord.attr("activestatus", "0");
                _btnNewRecord.attr("src", "Areas/VA012/Images/hide.png");
                _btnNewRecord.attr("title", "Collapse");
                _btnNewRecord.removeClass("vis vis-plus");
                _btnNewRecord.addClass("fa fa-minus");
                $_formNewRecord.show();
                loadFunctions.setPaymentListHeight()
                childDialogs.statementListRecordEdit(_bankStatementLineID, _dragPaymentID);
                _bankStatementLineID = 0;
                return true;
            },

            statementListEdit: function (e) {
                var target = $(e.target);

                var _bankStatementLineID = 0;
                var _dragPaymentID = 0;//to avoid undefined issue
                if (target.hasClass('glyphicon-edit')) {
                    _bankStatementLineID = target.data("uid");
                    _btnNewRecord.attr("activestatus", "0");
                    _btnNewRecord.attr("src", "Areas/VA012/Images/hide.png");
                    _btnNewRecord.attr("title", "Collapse");
                    $_formNewRecord.show();
                    _btnNewRecord.removeClass("vis vis-plus");
                    _btnNewRecord.addClass("fa fa-minus");
                    loadFunctions.setPaymentListHeight()
                    newRecordForm.scheduleRefresh();
                    newRecordForm.prepayRefresh();

                    _openingFromEdit = true;
                    childDialogs.statementListRecordEdit(_bankStatementLineID, _dragPaymentID);
                    _bankStatementLineID = 0;
                    loadFunctions.addEffect(target, $_formNewRecord);
                }
            },
            openStatement: function (e) {
                var target = $(e.target);

                var _cbankStatementID = 0;
                if (target.hasClass('glyphicon glyphicon-zoom-in')) {
                    _cbankStatementID = target.data("uid");

                    ////

                    var sql = "select ad_window_id from ad_window where name = 'Bank Statement'";
                    var ad_window_Id = 0;
                    try {
                        var dr = VIS.DB.executeDataReader(sql);
                        if (dr.read()) {
                            ad_window_Id = dr.getInt(0);
                        }
                        dr.dispose();
                        if (ad_window_Id > 0) {
                            var zoomQuery = new VIS.Query();
                            zoomQuery.addRestriction("C_BankStatement_ID", VIS.Query.prototype.EQUAL, _cbankStatementID);
                            zoomQuery.setRecordCount(1);
                            VIS.viewManager.startWindow(ad_window_Id, zoomQuery);
                        }
                    }
                    catch (e) {
                        console.log(e);
                    }

                    /////

                    _cbankStatementID = 0;
                }
            },
            selectedStatementLinesList: function (e) {
                var target = $(e.target);
                if (e.target.type == "checkbox") {
                    if (target.is(":checked")) {
                        _bankStatementLineID = parseInt(target.data("uid"));
                        _statementLinesList.push(_bankStatementLineID);
                        _bankStatementLineID = 0;
                    }
                    if (!target.is(":checked")) {
                        _bankStatementLineID = parseInt(target.data("uid"));
                        _statementLinesList.splice(_statementLinesList.indexOf(_bankStatementLineID), 1)
                        _bankStatementLineID = 0;
                    }
                }
            },
            statementListRecordEdit: function (_bankStatementLineID, _dragPaymentID) {

                newRecordForm.refreshForm();
                childDialogs.getStatementLineForEdit(_bankStatementLineID, _dragPaymentID, childDialogs.afterRecordGet);

            },
            selectedScheduleList: function (e) {

                var target = $(e.target);
                if (e.target.type == "checkbox") {
                    if (target.is(":checked")) {
                        // _scheduleList.push(parseInt(target.data("uid")));
                    }
                    if (!target.is(":checked")) {
                        // _scheduleList.splice(_scheduleList.indexOf(parseInt(target.data("uid"))), 1)
                    }
                }
            },

            getStatementLineForEdit: function (_bankStatementLineID, _dragPaymentID, callback) {
                $.ajax({
                    type: 'POST',
                    url: VIS.Application.contextUrl + "VA012/BankStatement/GetStatementLine",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ _bankStatementLineID: _bankStatementLineID, trxType: _cmbTransactionType.val(), payment_ID: _dragPaymentID != null ? _dragPaymentID : 0, _statementDt: _dtStatementDate.val() }),
                    success: function (data) { callback(data); },
                    error: function (data) { VIS.ADialog.info(data, null, "", ""); }
                });
            },
            afterRecordGet: function (data) {
                if (data != null && data != "") {
                    //debugger;
                    var _result = $.parseJSON(data);
                    $_formNewRecord.attr("data-uid", _result._bankStatementLineID);
                    //if (_result._bankStatementLineID > 0 && _result._ctrlPayment <= 0) {
                    //    _btnCreatePayment.show();
                    //}
                    //else {
                    //    _btnCreatePayment.hide();
                    //}
                    _txtStatementNo.val(_result._txtStatementNo);
                    _txtStatementPage.val(_result._txtStatementPage);
                    _txtStatementLine.val(_result._txtStatementLine);
                    _dtStatementDate.val(Globalize.format(new Date(_result._dtStatementDate), "yyyy-MM-dd"));
                    _cmbPaymentMethod.val(_result._cmbPaymentMethod).prop('selected', true);
                    if (_result._cmbVoucherMatch != null && _result._cmbVoucherMatch != "") {
                        _cmbVoucherMatch.val(_result._cmbVoucherMatch).prop('selected', true);
                    }
                    else {
                        _cmbVoucherMatch.prop('selectedIndex', 0);
                    }

                    // when cmb on transaction type Contra then bind "Voucher/Match" type as Contra
                    if (_cmbTransactionType.val() == "CO") {
                        _cmbVoucherMatch.val("C").prop('selected', true);
                    }

                    _cmbVoucherMatch.trigger('change');

                    if (_result._cmbContraType != null && _result._cmbContraType != "") {
                        _cmbContraType.val(_result._cmbContraType).prop('selected', true);
                    }
                    else {
                        _cmbContraType.prop('selectedIndex', 0);
                    }

                    // when voucher match is contra and voucher type not eselected then bind as " Cash To Bank"
                    if (_cmbVoucherMatch.val() != null && _cmbVoucherMatch.val() != ""
                        && _cmbContraType.val() != "" && _cmbContraType.val() != "") {
                        _cmbContraType.val("CB").prop('selected', true);
                    }
                    _cmbContraType.trigger('change');

                    _cmbCashBook.val(_result._cmbCashBook).prop('selected', true);
                    _txtCheckNo.val(_result._txtCheckNo);
                    _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_result._txtAmount.toFixed(_stdPrecision)));
                    _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(_result._txtTrxAmt.toFixed(_stdPrecision)));
                    if (_cmbVoucherMatch.val() == "M" && _txtTrxAmt.getValue() != 0) {
                        _txtDifference.setValue(VIS.Utility.Util.getValueOfDecimal(Math.abs(_result._txtDifference.toFixed(_stdPrecision))));
                        if (_result._txtDifference != 0) {
                            _txtDifference.getControl().attr("vchangable", "N");
                            _divDifferenceType.find("*").prop("disabled", false);
                        }
                        else {
                            _txtDifference.getControl().attr("vchangable", "Y");
                        }

                    }
                    if (_result._cmbDifferenceType != null && _result._cmbDifferenceType != "") {
                        _divDifferenceType.find("*").prop("disabled", false);
                        _cmbDifferenceType.val(_result._cmbDifferenceType).prop('selected', true);
                    }
                    else {
                        _cmbDifferenceType.prop('selectedIndex', 0);
                    }
                    _cmbDifferenceType.trigger('change');
                    //_txtTrxAmt.trigger('change');
                    if (_txtAmount.getValue() < 0) {
                        _btnOut.removeClass("va012-inactive");
                        _btnOut.addClass("va012-active");
                        _btnOut.attr("v_active", "1");
                        _btnIn.removeClass("va012-active");
                        _btnIn.addClass("va012-inactive");
                        _btnIn.attr("v_active", "0");
                    }
                    else {
                        _btnIn.removeClass("va012-inactive");
                        _btnIn.addClass("va012-active");
                        _btnIn.attr("v_active", "1");
                        _btnOut.removeClass("va012-active");
                        _btnOut.addClass("va012-inactive");
                        _btnOut.attr("v_active", "0");
                    }


                    // _cmbCurrency.val(_result._cmbCurrency).prop('selected', true);
                    _txtVoucherNo.val(_result._txtVoucherNo);
                    _txtDescription.val(_result._txtDescription);
                    _cmbCharge.val(_result._cmbCharge).prop('selected', true);
                    _txtCharge.val(_result._txtCharge);
                    _txtCharge.attr('chargeid', _result._cmbCharge);
                    _cmbTaxRate.val(_result._cmbTaxRate).prop('selected', true);
                    _txtTaxAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_result._txtTaxAmount.toFixed(_stdPrecision)));
                    _chkUseNextTime.prop('checked', _result._chkUseNextTime);

                    if (_result._ctrlCashLine > 0) {
                        $_ctrlCashLine.setValue(_result._ctrlCashLine, false, true);
                        _openingFromEdit = false;
                    }
                    else {
                        if (!_openingFromDrop) {
                            $_ctrlCashLine.setValue();
                            if (_result._ctrlPayment <= 0) {
                                _openingFromEdit = false;
                            }
                        }
                    }

                    if ($_ctrlPayment) {
                        if ($_ctrlPayment.value > 0) {

                            if (_result._ctrlPayment == 0) {
                                _result._ctrlPayment = $_ctrlPayment.value;
                            }
                            //if (_txtVoucherNo.val() == "") {
                            //    var Voucher = VIS.Utility.Util.getValueOfString(VIS.DB.executeScalar("select trxno from C_Payment where C_Payment_ID=" + $_ctrlPayment.value));
                            //    _txtVoucherNo.val(Voucher);
                            //}
                        }
                    }

                    if (_result._ctrlPayment > 0) {
                        $_ctrlPayment.setValue(_result._ctrlPayment, false, true);
                        if (_txtVoucherNo.val() == "") {
                            var Voucher = VIS.Utility.Util.getValueOfString(VIS.DB.executeScalar("select trxno from C_Payment where C_Payment_ID=" + _result._ctrlPayment));
                            _txtVoucherNo.val(Voucher);
                        }
                        _openingFromEdit = false;
                    }
                    else {
                        if (!_openingFromDrop) {
                            $_ctrlPayment.setValue();
                            _openingFromEdit = false;
                        }
                    }


                    if (_result._ctrlOrder > 0) {
                        $_ctrlOrder.setValue(_result._ctrlOrder, false, true);
                    }
                    else {
                        if (!_openingFromDrop) {
                            $_ctrlOrder.setValue();
                        }
                    }
                    if (_result._ctrlBusinessPartner > 0) {
                        $_ctrlBusinessPartner.setValue(_result._ctrlBusinessPartner, false, true);
                    }
                    else {
                        if (!_openingFromDrop) {
                            $_ctrlBusinessPartner.setValue();
                        }
                    }
                    if (_result._ctrlInvoice > 0) {
                        $_ctrlInvoice.setValue(_result._ctrlInvoice, false, true);
                    }
                    else {
                        if (!_openingFromDrop) {
                            $_ctrlInvoice.setValue();
                        }
                    }

                    _txtCharge.trigger("focus");
                    //_openingFromDrop = false;
                }
            },
            //End Load Statement Dialog

            //Match Statement Dialog
            matchStatementDialog: function () {
                var _cmbMatchingBase = null;
                var $_divMatchingBase = null;
                var _matchingBaseItemList = [];
                var _cmbMatchingCriteria = null;
                var _cmbStatementNo = null;
                //var _cmbChargeType = null;
                //Charge Type Search 
                var _chargeSrch = null;
                var $ChargeControl = null;
                var _cmbTaxRate = null;
                var _btnAccept = null;

                $match = $("<div class='va012-popform-content'>");
                var _match = "";
                _match =
                    "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_StatementNo") + '<sup style="color: red;">*</sup></label>'
                    + "<select id='VA012_cmbStatementNo_" + $self.windowNo + "'>"
                    + "</select></div> "

                    + "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_MatchingBase") + '<sup style="color: red;">*</sup></label>'
                    + "<select id='VA012_cmbMatchingBase_" + $self.windowNo + "'>"
                    //Commeted because now we laod this from database
                    //+ "<option value='0'></option>"

                    // + "<option value='BP'>" + VIS.Msg.getMsg("VA012_BusinessPartner") + "</option>"
                    // + "<option value='PA'>" + VIS.Msg.getMsg("VA012_PaymentAmount") + "</option>"
                    // + "<option value='CN'>" + VIS.Msg.getMsg("VA012_CheckNumber") + "</option>"
                    // + "<option value='CH'>" + VIS.Msg.getMsg("VA012_Charge") + "</option>"
                    // + "<option value='IN'>" + VIS.Msg.getMsg("VA012_Invoice") + "</option>"
                    // + "<option value='OR'>" + VIS.Msg.getMsg("VA012_Order") + "</option>"
                    // + "<option value='AC'>" + VIS.Msg.getMsg("VA012_AuthCode") + "</option>"

                    + "</select></div> "
                    + "<div id='VA012_divMatchingBase_" + $self.windowNo + "' style='width:100%;border: 2px solid rgba(var(--v-c-primary), 1);padding: 10px; float: left; margin-bottom: 10px; min-height: 150px; max-height: 200px; overflow:auto'>"
                    + "</div>"
                    + "<div class='va012-popform-data'>"
                    + "<label>" + VIS.Msg.getMsg("VA012_MatchingCriteria") + '<sup style="color: red;">*</sup></label>'
                    + "<select id='VA012_cmbMatchingCriteria_" + $self.windowNo + "'>"
                    + "<option value='0'></option>"
                    + "<option value='AT'>" + VIS.Msg.getMsg("VA012_MatchAnyTwo") + "</option>"
                    + "<option value='AR'>" + VIS.Msg.getMsg("VA012_MatchAnyThree") + "</option>"
                    + "<option value='AL'>" + VIS.Msg.getMsg("VA012_MatchAll") + "</option>"

                    + "</select></div> "

                    //+ "<div class='va012-popform-data'>"
                    //+ "<label title = '" + VIS.Msg.getMsg("VA012_SetChargeType") + "' > " + VIS.Msg.getMsg("VA012_Charge") + '<sup style="color: red;">*</sup></label>'
                    //+ "<select id='VA012_cmbChargeType_" + $self.windowNo + "'>"
                    //+ "</select></div> "

                    + "<div class='va012-popform-data'>"
                    + "<label title = '" + VIS.Msg.getMsg("VA012_SetChargeType") + "' > " + VIS.Msg.getMsg("VA012_Charge") + '<sup style="color: red;">*</sup></label>'
                    + "<div id='VA012_ChargeSrch_" + $self.windowNo + "'>"
                    + "</div></div> "

                    + "<div class='va012-popform-data'>"
                    + "<label title =' " + VIS.Msg.getMsg("VA012_SetTaxRate") + "' >" + VIS.Msg.getMsg("VA012_TaxRate") + '<sup style="color: red;">*</sup></label>'
                    + "<select id='VA012_cmbTaxRate_" + $self.windowNo + "'>"
                    + "</select></div> "
                    //added Accept button manually
                    + "<div class='va012-frm-btn va012-btn-blue' style='float: right;'>"
                    + "<label id ='VA012_accept_" + $self.windowNo + "'>" + VIS.Msg.getMsg("VA012_Accept") + "</label></div> "

                    + "</div>";
                $match.append(_match);
                _getMatchControls();
                //    getlookupdata();
                // to load Maching Base combo
                getMatchingBaseData(_cmbMatchingBase);
                loadBankStatementNo();

                // loadBankAccountCharges();

                loadTaxRate();

                //function loadBankAccountCharges() {
                //    var _sql = "SELECT C_CHARGE_ID, NAME FROM C_CHARGE WHERE DTD001_ChargeType = 'BNK' AND ISACTIVE = 'Y' AND AD_CLIENT_ID = " + VIS.Env.getCtx().getAD_Client_ID() + " ORDER BY NAME  ";
                //    var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadBankAccountCharges);
                //    function callbackloadBankAccountCharges(_ds) {
                //        _cmbChargeType.html("");
                //        _cmbChargeType.append("<option value=0 >-</option>");
                //        if (_ds != null) {
                //            for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                //                _cmbChargeType.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_charge_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                //            }
                //        }
                //        _ds.dispose();
                //        _ds = null;
                //        _sql = null;
                //        _cmbChargeType.prop('selectedIndex', 0);
                //    }
                //};

                function loadBankStatementNo() {

                    // Change here for only picking statementes which are not completed, closed or Voided
                    // Lokesh Chauhan
                    var _sql = "Select Cb.C_Bankstatement_Id, CB.NAME,Cb.Docstatus, COUNT(VA012_ISMATCHINGCONFIRMED) FROM C_BANKSTATEMENT CB INNER JOIN C_BANKSTATEMENTLINE CBL ON cbl.C_BANKSTATEMENT_ID = Cb.C_BANKSTATEMENT_ID Where Cb.Isactive = 'Y' And Cb.Ad_Client_Id = " + VIS.Env.getCtx().getAD_Client_ID() + " And Cb.C_Bankaccount_Id = " + _cmbBankAccount.val() + " And Cb.Docstatus NOT IN  ('CO','CL','VO') GROUP BY CB.C_BANKSTATEMENT_ID,Cb.Docstatus, CB.NAME"

                    var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadBankStatementNo);
                    function callbackloadBankStatementNo(_ds) {
                        _cmbStatementNo.html("");
                        _cmbStatementNo.append("<option value=0 >-</option>");
                        if (_ds != null) {
                            for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                                _cmbStatementNo.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_bankstatement_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                            }
                        }
                        _ds.dispose();
                        _ds = null;
                        _sql = null;
                    }
                };

                function loadTaxRate() {
                    //Select Taxes which is not Surcharge and having no Parent Tax
                    var _sql = "SELECT Name,C_Tax_ID FROM C_Tax WHERE IsActive='Y'AND IsSurcharge='N' AND NVL(Parent_Tax_ID, 0)=0";
                    _sql = VIS.MRole.getDefault().addAccessSQL(_sql, "C_Tax", true, false);
                    var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadloadTaxRate);
                    function callbackloadloadTaxRate(_ds) {
                        _cmbTaxRate.html("");
                        _cmbTaxRate.append("<option value=0 >-</option>");
                        if (_ds != null) {
                            for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                                _cmbTaxRate.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_tax_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                            }
                        }
                        _ds.dispose();
                        _ds = null;
                        _sql = null;
                    }
                };
                //  var $POP_lookCharge = null;
                var matchDialog = new VIS.ChildDialog();
                matchDialog.setContent($match);
                matchDialog.setTitle(VIS.Msg.getMsg("VA012_MatchStatement"));
                matchDialog.setWidth(700);
                matchDialog.setHeight(650);//Increased height to visuble the Accept button
                matchDialog.setEnableResize(false);
                matchDialog.setModal(true);
                matchDialog.show();
                matchDialog.hideButtons();//hide buttons (Ok and Cacel) which generated by VIS.ChildDialog()
                function _getMatchControls() {
                    _cmbMatchingBase = $match.find("#VA012_cmbMatchingBase_" + $self.windowNo);
                    $_divMatchingBase = $match.find("#VA012_divMatchingBase_" + $self.windowNo);
                    _cmbMatchingCriteria = $match.find("#VA012_cmbMatchingCriteria_" + $self.windowNo);
                    _cmbStatementNo = $match.find("#VA012_cmbStatementNo_" + $self.windowNo);
                    //_cmbChargeType = $match.find("#VA012_cmbChargeType_" + $self.windowNo);
                    _chargeSrch = $match.find("#VA012_ChargeSrch_" + $self.windowNo);
                    _cmbTaxRate = $match.find("#VA012_cmbTaxRate_" + $self.windowNo);
                    //Accept button Control - get Id
                    _btnAccept = $match.find("#VA012_accept_" + $self.windowNo);

                    //Added Charge Search Lookup
                    _ChargeLookUp = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), $self.windowNo, 3787, VIS.DisplayType.Search);
                    $ChargeControl = new VIS.Controls.VTextBoxButton("C_Charge_ID", true, false, true, VIS.DisplayType.Search, _ChargeLookUp);
                    _chargeSrch.append($ChargeControl.getControl().css('width', '93%')).append($ChargeControl.getBtn(0).css('width', '30px').css('height', '30px').css('padding', '0px').css('border-color', '#BBBBBB'));
                    //VIS.Utility.Util.getValueOfInt($ChargeControl.value)
                }

                //function getlookupdata() {
                //    _ChargeLookUp = VIS.MLookupFactory.getMLookUp(VIS.Env.getCtx(), $self.windowNo, 3787, VIS.DisplayType.Search);
                //    $ChargeControl = new VIS.Controls.VTextBoxButton("C_Charge_ID", true, false, true, VIS.DisplayType.Search, _ChargeLookUp);
                //    $POP_lookCharge.append($ChargeControl.getControl().css('width', '93%')).append($ChargeControl.getBtn(0).css('width', '30px').css('height', '30px').css('padding', '0px').css('border-color', '#BBBBBB'));
                //};

                //matchDialog.onOkClick = function () {
                //button Click event created manually
                _btnAccept.click(function (e) {

                    if (_matchingBaseItemList.length > 0) {

                        if (_cmbStatementNo.val() == null || _cmbStatementNo.val() == "" || _cmbStatementNo.val() == "0") {
                            VIS.ADialog.info("VA012_NoLinesFoundToMatch", null, "", "");
                            return false;
                        }

                        if (_cmbBankAccount.val() == null || _cmbBankAccount.val() == "" || _cmbBankAccount.val() == "0") {
                            VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                            return false;
                        }

                        if (_cmbMatchingCriteria.val() == null || _cmbMatchingCriteria.val() == "" || _cmbMatchingCriteria.val() == "0") {
                            VIS.ADialog.info("VA012_SelectMatchingCriteria", null, "", "");
                            return false;
                        }
                        else {
                            if (_cmbMatchingCriteria.val() == "AT" && _matchingBaseItemList.length < 2) {
                                VIS.ADialog.info("VA012_SelectMin2MatchingBase", null, "", "");
                                return false;
                            }
                            if (_cmbMatchingCriteria.val() == "AR" && _matchingBaseItemList.length < 3) {
                                VIS.ADialog.info("VA012_SelectMin3MatchingBase", null, "", "");
                                return false;
                            }
                        }

                        //if (_cmbChargeType.val() == null || _cmbChargeType.val() == "" || _cmbChargeType.val() == "0") {
                        //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_NoChargeSelected"), null, "", "");
                        //    return false;
                        //}

                        //added same check which was working before i.e charge is mandatory
                        if ($ChargeControl.value == null || (VIS.Utility.Util.getValueOfInt($ChargeControl.value) == 0)) {
                            VIS.ADialog.info("VA012_NoChargeSelected", null, "", "");
                            return false;
                        }

                        if (_cmbTaxRate.val() == null || _cmbTaxRate.val() == "" || _cmbTaxRate.val() == "0") {
                            VIS.ADialog.info("VA012_NoTaxRateSelected", null, "", "");
                            return false;
                        }

                        //MATCH STATEMENT
                        busyIndicator($match, true, "absolute");
                        window.setTimeout(function () {
                            //_cmbChargeType.val()
                            //VIS.Utility.Util.getValueOfInt($ChargeControl.value)
                            $.ajax({
                                url: VIS.Application.contextUrl + "BankStatement/MatchStatementGridData",
                                type: "GET",
                                dataType: "json",
                                contentType: "application/json; charset=utf-8",
                                async: false,
                                data: ({ _matchingBaseItemList: _matchingBaseItemList.toString(), _cmbMatchingCriteria: _cmbMatchingCriteria.val(), _BankAccount: _cmbBankAccount.val(), _StatementNo: _cmbStatementNo.val(), _BankCharges: VIS.Utility.Util.getValueOfInt($ChargeControl.value), _TaxRate: _cmbTaxRate.val() }),
                                success: function (data) {

                                    if (data != null && data != "") {
                                        data = $.parseJSON(data);
                                        _cmbMatchingBase.prop('selectedIndex', 0);
                                        _cmbStatementNo.prop('selectedIndex', 0);
                                        // _cmbChargeType.prop('selectedIndex', 0);
                                        $ChargeControl.value = null;
                                        _cmbTaxRate.prop('selectedIndex', 0);

                                        _matchingBaseItemList = [];
                                        var _addmatchingbaseitem = "";
                                        $_divMatchingBase.html("");
                                        if (data.length > 0) {
                                            GetMatchStatGrid($_divMatchingBase, data);
                                            // $_divMatchingBase.add(data);

                                            ////////for (var i = 0; i < data.length; i++) {

                                            ////////    if (data[i]._error != null) {
                                            ////////        _addmatchingbaseitem = "<div> "
                                            ////////        + " <p><b>" + vis.msg.getmsg("va012_error") + ": </b>" + data[i]._error + "</p>"
                                            ////////               + " </div>";
                                            ////////        $_divMatchingBase.append(_addmatchingbaseitem);
                                            ////////    }
                                            ////////    else if (data[i]._StatementNo != null || data[i]._statementline != null) {
                                            ////////        _addmatchingbaseitem = "<div>"
                                            ////////       + " <p> <b>" + vis.msg.getmsg("va012_statementnumber") + ": </b>" + data[i]._statementno + "<br> <b>" + vis.msg.getmsg("va012_statementline") + ": </b>" + data[i]._statementline + " <br><b>" + vis.msg.getmsg(data[i]._paymentorcash == "p" ? "va012_matchedtopayment" : "va012_matchedtocashline") + ": </b>" + data[i]._paymentno

                                            ////////        if (data[i]._warning != null) {
                                            ////////            _addmatchingbaseitem += " <br>" + data[i]._warning;
                                            ////////        }
                                            ////////        _addmatchingbaseitem += " </p> </div>";
                                            ////////        $_divMatchingBase.append(_addmatchingbaseitem);
                                            ////////    }
                                            ////////}


                                            _statementlineslist = [];
                                            _lstStatement.html("");
                                            _statementPageNo = 1;
                                            childDialogs.loadStatement(_statementID);

                                            _lstPayments.html("");
                                            newRecordForm.scheduleRefresh();
                                            newRecordForm.prepayRefresh();
                                            _paymentPageNo = 1;
                                            loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                                        }
                                        else {
                                            _addmatchingbaseitem = "<div>"
                                                + " <p> <b>" + VIS.Msg.getMsg("va012_norecordmatched") + "</b> </div>";
                                            $_divMatchingBase.append(_addmatchingbaseitem);
                                        }
                                        busyIndicator($match, false, "absolute");
                                    }
                                    else {
                                        busyIndicator($match, false, "absolute");
                                        VIS.ADialog.info(data.toString(), null, "", "");
                                    }
                                },
                                error: function () {
                                    busyIndicator($match, false, "absolute");
                                    VIS.ADialog.info("error", null, "", "");
                                }
                            })
                        }, 2);
                        //MATCH STATEMENT -END




                        //MATCH STATEMENT GRID
                        ////////busyindicator($match, true, "absolute");
                        ////////window.settimeout(function () {
                        ////////    $.ajax({                                
                        ////////        //url: vis.application.contexturl + "bankstatement/matchstatementgriddata",
                        ////////        type: "get",
                        ////////        datatype: "json",
                        ////////        contenttype: "application/json; charset=utf-8",
                        ////////        async: false,
                        ////////        data: ({ _matchingbaseitemlist: _matchingbaseitemlist.tostring(), _cmbmatchingcriteria: _cmbmatchingcriteria.val(), _bankaccount: _cmbbankaccount.val(), _statementno: _cmbstatementno.val() }),
                        ////////        success: function (data) {

                        ////////            if (data != null && data != "") {
                        ////////                data = $.parsejson(data);
                        ////////                cartGrid.add(data);                                       
                        ////////                busyindicator($match, false, "absolute");
                        ////////            }
                        ////////            else {
                        ////////                busyindicator($match, false, "absolute");
                        ////////                vis.adialog.info(vis.msg.getmsg(data.tostring()), null, "", "");
                        ////////            }
                        ////////        },
                        ////////        error: function () {
                        ////////            busyindicator($match, false, "absolute");
                        ////////            vis.adialog.info(vis.msg.getmsg("error"), null, "", "");
                        ////////        }
                        ////////    })
                        ////////}, 2);
                        //MATCH STATEMENT GRID -END



                    }
                    else {
                        VIS.ADialog.info("VA012_NoMatchingBaseSelected", null, "", "");
                        return false;
                    }
                    return false;
                });
                matchDialog.onCancelClick = function () {
                    if (w2ui['VA012_gridPayment_' + $self.windowNo] != null) {
                        w2ui['VA012_gridPayment_' + $self.windowNo].destroy();
                    }
                };
                matchDialog.onClose = function () {
                    matchDispose();
                };
                _cmbMatchingBase.on('change', function () {

                    if (_matchingBaseItemList.length <= 0) {
                        $_divMatchingBase.html("");
                    }

                    if (!isInList(_cmbMatchingBase.val(), _matchingBaseItemList) && (_cmbMatchingBase.val() != "0" && _cmbMatchingBase.val() != "")) {
                        _matchingBaseItemList.push(_cmbMatchingBase.val());
                        var _addMatchingBaseItem = "<div class='va012-matchingbaselist'> "
                            + " <a class='va012-remove-icon'>"
                            + " <span data-uid='" + _cmbMatchingBase.val() + "'class='glyphicon glyphicon-remove'></span></a>"
                            + " <span>" + _cmbMatchingBase.children()[_cmbMatchingBase[0].selectedIndex].text; +"</span> </div>";
                        $_divMatchingBase.append(_addMatchingBaseItem);

                    }

                });
                $_divMatchingBase.on(VIS.Events.onTouchStartOrClick, removeMatchingBaseItem);

                function removeMatchingBaseItem(e) {
                    var target = $(e.target);
                    if (target.hasClass('glyphicon-remove')) {

                        _matchingBaseItemList.splice(_matchingBaseItemList.indexOf(target.data("uid")), 1);
                        target.parent().parent().remove();
                    }
                }
                function matchDispose() {
                    _cmbMatchingBase = null;
                    $_divMatchingBase = null;
                    _matchingBaseItemList = [];
                    _cmbMatchingCriteria = null;
                    if (w2ui['VA012_gridPayment_' + $self.windowNo] != null) {
                        w2ui['VA012_gridPayment_' + $self.windowNo].destroy();
                    }
                }
            },
            //End Match Statement Dialog



            //Match Statement Grid
            matchStatementGridDialog: function () {

                var matchDialog = new VIS.ChildDialog();
                matchDialog.setContent($divMatchStatementGridPopUp);
                matchDialog.setTitle(VIS.Msg.getMsg("VA012_MatchStatementGrid"));
                matchDialog.setWidth("780px");
                matchDialog.setHeight(450);
                matchDialog.setEnableResize(true);
                matchDialog.setModal(true);

                matchDialog.show();
                GetMatchStatGrid();


            },
            //END Match Statement Grid



            //Payment Schedule  Dialog
            paymentScheduleDialog: function () {
                //handled the issue when click on Edit Schedule button it's clearing the seleted records
                if (_scheduleList.length == 0 && _scheduleDataList.length == 0) {
                    var _lookupPSInvoice = null;
                    var $_ctrlPSInvoice = null;
                    var _psInvoiceSelectedVal = null;
                    var _ctrlPSInvoice = null;

                    var _lookupPSBP = null;
                    var $_ctrlPSBP = null;
                    var _psBpSelectedVal = null;
                    var _ctrlPSBP = null;
                    var _cmbPaymentSchedule = null;
                }
                $paymentSchedule = $("<div class='va012-popform-content'>");
                var _paymentSchedule = "";

                _paymentSchedule = "<div>"
                    + " <label>" + VIS.Msg.getMsg("VA012_BusinessPartner") + "</label>"
                    + " <div id='VA012_ctrlPSBP_" + $self.windowNo + "' ></div>"
                    + " </div>";

                _paymentSchedule += "<div>"
                    + " <label>" + VIS.Msg.getMsg("VA012_Invoice") + "</label>"
                    + " <div id='VA012_ctrlPSInvoice_" + $self.windowNo + "' ></div>"
                    + " </div>";


                _paymentSchedule += "<div>"
                    + " <label>" + VIS.Msg.getMsg("VA012_PaymentSchedules") + "</label>"
                    + " <select id='VA012_cmbPaymentSchedule_" + $self.windowNo + "'  style=' width: 100%; ' ></select>"
                    + " </div>"

                _paymentSchedule += "<div id='VA012_divPaymentSchedules_" + $self.windowNo + "' style='width:100%;border: 2px solid rgba(var(--v-c-primary), 1);padding: 10px; float: left; margin-bottom: 10px;margin-top: 10px; min-height: 150px; max-height: 200px; overflow:auto'>"
                    + "</div> </div>";
                $paymentSchedule.append(_paymentSchedule);
                _getScheduleControls();
                var paymentScheduleDialog = new VIS.ChildDialog();
                paymentScheduleDialog.setContent($paymentSchedule);
                paymentScheduleDialog.setTitle(VIS.Msg.getMsg("VA012_PaymentSchedules"));
                paymentScheduleDialog.setWidth("500px");
                loadPaymentScheduleItems();
                paymentScheduleDialog.setEnableResize(false);
                paymentScheduleDialog.setModal(true);
                paymentScheduleDialog.show();
                loadPSBP();
                loadPSInvoice();


                if ($_ctrlBusinessPartner.value) {

                    $_ctrlPSBP.setValue($_ctrlBusinessPartner.value, false, true);
                    _ctrlPSBP.find("*").prop("disabled", true);
                }
                if ($_ctrlInvoice.value) {
                    $_ctrlPSInvoice.setValue($_ctrlInvoice.value, false, true);
                    _ctrlPSInvoice.find("*").prop("disabled", true);

                }

                //pratap update this value _cmbPaymentSchedule.val()

                _cmbPaymentSchedule.on('change', function () {
                    if (_cmbPaymentSchedule.val() > 0) {
                        if (loadFunctions.checkScheduleCondition(_cmbPaymentSchedule.val(), parseInt($_formNewRecord.attr("data-uid")), _scheduleList.toString(), _txtAmount.getValue())) {
                            //alert("done");
                            // fixed Issue while checking condition with interger like 123 compare with "123"
                            if (!isInList(parseInt(_cmbPaymentSchedule.val()), _scheduleList)) {
                                _scheduleList.push(parseInt(_cmbPaymentSchedule.val()));
                                _scheduleDataList.push(this.options[this.selectedIndex].getAttribute('paymentdata'));
                                //get the amount and push into scheduleamount list
                                _scheduleAmount.push(this.options[this.selectedIndex].getAttribute('paymentamount'));
                                loadPaymentScheduleItems();
                                loadFunctions.setInvoiceAndBPartner(_cmbPaymentSchedule.val(), "IS");
                            }
                            else {
                                VIS.ADialog.info("VA012_AlreadySelected", null, "", "");
                            }
                            _txtPaymentSchedule.val(_scheduleDataList.toString());
                        }
                    }

                });

                function loadPaymentScheduleItems() {
                    var _addItem = "";
                    var paySumAmt = 0;
                    $_divPaymentSchedules.html("");
                    for (var i = 0; i < _scheduleList.length; i++) {
                        _addItem = "<div class='va012-matchingbaselist'> "
                            + " <a class='va012-remove-icon'>"
                            + " <span data-uid='" + _scheduleList[i] + "'  data-udata='" + _scheduleDataList[i] + "' class='glyphicon glyphicon-remove'></span></a>"
                            + " <span>" + _scheduleDataList[i] + "</span> </div>";
                        $_divPaymentSchedules.append(_addItem);
                        //amount = _scheduleDataList[i].split("_");
                        if (_scheduleAmount.length > 0) {
                            paySumAmt += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                        }
                    }
                    //assign the total AMount to the fields
                    _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(paySumAmt.toFixed(_stdPrecision)));
                    _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(paySumAmt.toFixed(_stdPrecision)));
                    if (_txtAmount.getValue() < 0) {
                        _btnIn.attr("v_active", "0");
                    }
                    _txtAmount.getControl().trigger('blur');
                }
                $_divPaymentSchedules.on(VIS.Events.onTouchStartOrClick, removeItem);

                function removeItem(e) {
                    var target = $(e.target);
                    if (target.hasClass('glyphicon-remove')) {

                        /*removeAmount*/
                        if (_scheduleAmount != null && _scheduleAmount.length > 0) {
                            if (_scheduleAmount.length > _scheduleList.length) {
                                _scheduleAmount.splice(_scheduleList.indexOf(target.data("uid")) + 1, 1);
                            }

                            else if (_scheduleAmount.length == _scheduleList.length) {
                                _scheduleAmount.splice(_scheduleList.indexOf(target.data("uid")), 1);
                            }
                            ///
                            var amount = 0;
                            if (Number(_scheduleAmount.length) > 0) {

                                for (var i = 0; i < _scheduleAmount.length; i++) {
                                    amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                                }
                            }
                            if (amount < 0) {
                                _btnOut.removeClass("va012-inactive");
                                _btnOut.addClass("va012-active");
                                _btnOut.attr("v_active", "1");
                                _btnIn.removeClass("va012-active");
                                _btnIn.addClass("va012-inactive");
                                _btnIn.attr("v_active", "0");
                            }
                            else {
                                _btnIn.removeClass("va012-inactive");
                                _btnIn.addClass("va012-active");
                                _btnIn.attr("v_active", "1");
                                _btnOut.removeClass("va012-active");
                                _btnOut.addClass("va012-inactive");
                                _btnOut.attr("v_active", "0");
                            }
                            _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                            //_txtTrxAmt.val((amount).toFixed(_stdPrecision));
                            //_txtTrxAmt.trigger('change');
                            //not required 
                            //if (_scheduleAmount.length == 1) {
                            //    if (Number(_scheduleAmount[0]) == "0") {
                            //        _scheduleAmount.splice(0, 1);
                            //    }
                            //}
                            //}
                            //
                        }
                        /*removeAmount*/

                        _scheduleList.splice(_scheduleList.indexOf(target.data("uid")), 1);
                        _scheduleDataList.splice(_scheduleDataList.indexOf(target.data("udata")), 1);

                        target.parent().parent().remove();
                        _txtPaymentSchedule.val(_scheduleDataList.toString());
                        var amount = 0;
                        for (var i = 0; i < _scheduleAmount.length; i++) {
                            amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                        }
                        _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                        _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(amount.toFixed(_stdPrecision)));
                        _txtAmount.getControl().trigger('blur');
                        if (amount == 0) {
                            $_ctrlBusinessPartner.setValue();
                        }
                    }
                }
                function _getScheduleControls() {
                    $_divPaymentSchedules = $paymentSchedule.find("#VA012_divPaymentSchedules_" + $self.windowNo);
                    _ctrlPSInvoice = $paymentSchedule.find("#VA012_ctrlPSInvoice_" + $self.windowNo);
                    _ctrlPSBP = $paymentSchedule.find("#VA012_ctrlPSBP_" + $self.windowNo);
                    _cmbPaymentSchedule = $paymentSchedule.find("#VA012_cmbPaymentSchedule_" + $self.windowNo);
                }

                function loadPSInvoice() {
                    _lookupPSInvoice = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 3484, VIS.DisplayType.Search, "C_Invoice_ID", 0, false, "DocStatus IN ('CO','CL') AND C_BPartner_ID=" + _psBpSelectedVal);
                    $_ctrlPSInvoice = new VIS.Controls.VTextBoxButton("C_Invoice_ID", false, false, true, VIS.DisplayType.Search, _lookupPSInvoice);
                    $_ctrlPSInvoice.getControl().addClass("va012-input-size-2");
                    _ctrlPSInvoice.append($_ctrlPSInvoice.getControl());
                    _ctrlPSInvoice.append($_ctrlPSInvoice.getBtn(0));
                    _ctrlPSInvoice.append($_ctrlPSInvoice.getBtn(1));
                    $_ctrlPSInvoice.fireValueChanged = function () {
                        _psInvoiceSelectedVal = 0;
                        _psInvoiceSelectedVal = $_ctrlPSInvoice.value;
                        _cmbPaymentSchedule.html("");
                        loadPaymentSchedule();
                    };
                };

                function loadPSBP() {
                    _lookupPSBP = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 2893, VIS.DisplayType.Search, "C_BPartner_ID", 0, false, null);
                    $_ctrlPSBP = new VIS.Controls.VTextBoxButton("C_BPartner_ID", false, false, true, VIS.DisplayType.Search, _lookupBusinessPartner);
                    $_ctrlPSBP.getControl().addClass("va012-input-size-2");
                    _ctrlPSBP.append($_ctrlPSBP.getControl());
                    _ctrlPSBP.append($_ctrlPSBP.getBtn(0));
                    _ctrlPSBP.append($_ctrlPSBP.getBtn(1));
                    $_ctrlPSBP.fireValueChanged = function () {
                        _psBpSelectedVal = 0;
                        _psBpSelectedVal = $_ctrlPSBP.value;
                        _lookupPSInvoice = null;
                        $_ctrlPSInvoice = null;
                        _ctrlPSInvoice.html("");
                        _psInvoiceSelectedVal = null;
                        loadPSInvoice();
                    };
                };
                function loadPaymentSchedule() {

                    //var _sql = "SELECT BP.NAME as businesspartner,PAY.C_INVOICEPAYSCHEDULE_ID, "
                    //    + " PAY.DOCUMENTNO AS NAME "
                    //    + " FROM C_INVOICEPAYSCHEDULE PAY "
                    //    + " INNER JOIN C_INVOICE INV "
                    //    + " ON PAY.C_INVOICE_ID=INV.C_INVOICE_ID "
                    //    + " INNER JOIN C_BPARTNER BP "
                    //    + " ON BP.C_BPARTNER_ID=INV.C_BPARTNER_ID "
                    //    + " INNER JOIN VA009_PAYMENTMETHOD PM "
                    //    + " ON (PM.VA009_PAYMENTMETHOD_ID =PAY.VA009_PAYMENTMETHOD_ID ) "
                    //    + " INNER JOIN C_DOCTYPE DT "
                    //    + " ON DT.C_DOCTYPE_ID            =INV.C_DOCTYPE_ID "
                    //    + " WHERE PAY.VA009_ISPAID        ='N' "
                    //    + " AND PAY.ISACTIVE              ='Y' "
                    //    + " AND INV.DOCSTATUS            IN ('CO','CL') "
                    //    + " AND PM.VA009_PAYMENTBASETYPE! ='B'"
                    //    + " AND PAY.C_INVOICE_ID=" + _psInvoiceSelectedVal;
                    //used ajax call to get InvoicePaySchedules for selected Invoice
                    $.ajax({
                        url: VIS.Application.contextUrl + "BankStatement/GetInvPaySchedule",
                        dataType: "json",
                        data: { seltdInvoice: _psInvoiceSelectedVal, accountID: _cmbBankAccount.val(), statemtDate: _dtStatementDate.val() },
                        success: function (data) {
                            var result = JSON.parse(data);
                            if (result != null || result != "") {
                                callbackPaymentSchedule(result);
                            }
                        }
                    });

                    //VIS.DB.executeDataSet(_sql.toString(), null, callbackPaymentSchedule);
                    function callbackPaymentSchedule(_ds) {
                        _cmbPaymentSchedule.html("");
                        _cmbPaymentSchedule.append("<option value=0 ></option>");
                        if (_ds != null) {
                            for (var i = 0; i < _ds.length; i++) {
                                if (_ds[i].DueAmount == 0) {
                                    VIS.ADialog.info("VA012_ConversionRateNotFound", null, "", "");
                                    return;
                                }
                                //_cmbPaymentSchedule.append("<option paymentdata='" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "/" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.businesspartner) + "' value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_invoicepayschedule_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                                _cmbPaymentSchedule.append("<option paymentamount=" + _ds[i].DueAmount + " paymentdata='" + VIS.Utility.encodeText(new Date(_ds[i].DueDate).toLocaleDateString()) + "_" + VIS.Utility.encodeText(_ds[i].DueAmt) + "' value=" + VIS.Utility.Util.getValueOfInt(_ds[i].c_invoicepayschedule_id) + ">" + VIS.Utility.encodeText(new Date(_ds[i].DueDate).toLocaleDateString() + "_" + _ds[i].DueAmt) + "</option>");
                            }
                        }
                        //_ds.dispose();
                        //_ds = null;
                        //_sql = null;
                        _cmbPaymentSchedule.prop('selectedIndex', 0);

                    };
                };


                paymentScheduleDialog.onOkClick = function () {
                    //if (!loadFunctions.checkScheduleTotal(_scheduleList.toString(), _txtAmount.val(), parseInt($_formNewRecord.attr("data-uid")))) {
                    //    return false;
                    //}
                };
                paymentScheduleDialog.onCancelClick = function () {
                    newRecordForm.scheduleRefresh();
                    newRecordForm.refreshForm();
                    disposeSchedule();
                };
                //paymentScheduleDialog.onClose = function () {
                //    newRecordForm.scheduleRefresh();
                //    disposeSchedule()
                //};

                function disposeSchedule() {
                    _lookupPSInvoice = null;
                    $_ctrlPSInvoice = null;
                    _psInvoiceSelectedVal = null;
                    _ctrlPSInvoice = null;

                    _lookupPSBP = null;
                    $_ctrlPSBP = null;
                    _psBpSelectedVal = null;
                    _ctrlPSBP = null;
                    _cmbPaymentSchedule = null;
                };


            },
            //End Payment Schedule  Dialog
            //Prepay Order  Dialog
            prepayOrderDialog: function () {
                $prepayOrder = $("<div class='va012-popform-content'>");
                var _prepayOrder = "";
                _prepayOrder = "<div id='VA012_divPrepayOrders_" + $self.windowNo + "' style='width:100%;border: 2px solid rgba(var(--v-c-primary), 1);padding: 10px; float: left; margin-bottom: 10px; min-height: 150px; max-height: 200px; overflow:auto'>"
                    + "</div> </div>";
                $prepayOrder.append(_prepayOrder);
                _getPrepayOrderControls();
                var prepayOrderDialog = new VIS.ChildDialog();
                prepayOrderDialog.setContent($prepayOrder);
                prepayOrderDialog.setTitle(VIS.Msg.getMsg("VA012_PrepayOrders"));
                prepayOrderDialog.setWidth("500px");
                loadPrepayOrderItems()
                prepayOrderDialog.setEnableResize(false);
                prepayOrderDialog.setModal(true);
                prepayOrderDialog.show();


                function loadPrepayOrderItems() {
                    var _addItem = "";
                    $_divPrepayOrders.html("");
                    for (var i = 0; i < _prepayList.length; i++) {
                        _addItem = "<div class='va012-matchingbaselist'> "
                            + " <a class='va012-remove-icon'>"
                            + " <span data-uid='" + _prepayList[i] + "'  data-udata='" + _prepayDataList[i] + "' class='glyphicon glyphicon-remove'></span></a>"
                            + " <span>" + _prepayDataList[i] + "</span> </div>";
                        $_divPrepayOrders.append(_addItem);
                    }
                }
                $_divPrepayOrders.on(VIS.Events.onTouchStartOrClick, removeItem);

                function removeItem(e) {
                    var target = $(e.target);
                    if (target.hasClass('glyphicon-remove')) {

                        _prepayList.splice(_prepayList.indexOf(target.data("uid")), 1);
                        _prepayDataList.splice(_prepayDataList.indexOf(target.data("udata")), 1)
                        target.parent().parent().remove();
                        _txtPrepayOrder.val(_prepayDataList.toString());
                    }
                }
                function _getPrepayOrderControls() {
                    $_divPrepayOrders = $prepayOrder.find("#VA012_divPrepayOrders_" + $self.windowNo);
                }


            },
            //End Prepay Order Dialog           

        }
        //End Load Child Dialogs
        //New Record Form
        var newRecordForm = {
            newRecord: function () {

                $_formNewRecord = $root.find("#VA012_formNewRecord_" + $self.windowNo);
                $_formBtnNewRecord = $root.find("#VA012_formBtnNewRecord_" + $self.windowNo);
                newRecordForm.getNewRecordDesign();
                newRecordForm.getNewRecordControls();
                newRecordForm.loadCurrency();
                newRecordForm.loadPaymentMethod();
                newRecordForm.loadVoucherMatch();
                newRecordForm.loadContraType();
                newRecordForm.loadCashBook();
                newRecordForm.loadDifferenceType();
                newRecordForm.loadTransferType();
                newRecordForm.loadCharge();
                newRecordForm.loadTaxRate();
                newRecordForm.loadPayment();
                newRecordForm.loadOrder();
                newRecordForm.loadBusinessPartner();
                newRecordForm.loadInvoice();
                newRecordForm.loadCashLine();
                //to check mandatory fields and their logic to set background color
                if (_txtAmount.getValue() <= 0)
                    _txtAmount.getControl().addClass("va012-mandatory");
                if (_txtStatementNo.val() != "")
                    _txtStatementNo.removeClass("va012-mandatory");
                if (_dtStatementDate.val() != "")
                    _dtStatementDate.removeClass("va012-mandatory");

                _txtCharge.on('blur', function () {
                    if (_txtCharge.val() == "" || _txtCharge.val() == null) {
                        _txtCharge.attr('chargeid', 0);
                    }
                });

                _btnCharge.on(VIS.Events.onTouchStartOrClick, function (e) {
                    VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetCharge", { "searchText": "" }, chargeDropDown);
                });
                function chargeDropDown(result) {
                    datasource = [];
                    for (var i = 0; i < result.length; i++) {
                        datasource.push({ 'label': result[i].name, 'value': result[i].name, 'ids': result[i].chargeID });
                    }
                    _txtCharge.autocomplete('option', 'source', datasource)
                    _txtCharge.autocomplete("search", "");
                    _txtCharge.trigger("focus");
                };
                _txtCharge.autocomplete({
                    minLength: 0,
                    scroll: true,
                    source: function (request, response) {
                        if (request.term.trim().length == 0) {
                            return;
                        }

                        fillAutoCompleteonTextBox(request.term, response);
                    },
                    select: function (ev, ui) {
                        $(this).val(ui.item.ids);
                        $(this).attr("chargeid", ui.item.ids);
                        if (parseInt($(this).val())) {
                            _txtCharge.removeClass("va012-mandatory");
                        }
                    }
                });





                function fillAutoCompleteonTextBox(text, response) {

                    _txtCharge.attr("chargeid", 0);
                    $.ajax({
                        url: VIS.Application.contextUrl + "BankStatement/GetCharge",
                        dataType: "json",
                        data: { searchText: text },
                        success: function (data) {
                            var result = JSON.parse(data);
                            datasource = [];
                            response($.map(result, function (item) {
                                return {
                                    label: item.name,
                                    value: item.name,
                                    ids: item.chargeID
                                }
                            }));
                            $(self.div).autocomplete("search", "");
                            $(self.div).trigger("focus");
                        }
                    });
                };






                _btnNewRecord.on(VIS.Events.onTouchStartOrClick, function () {
                    if (_btnNewRecord.attr("activestatus") == "0") {
                        $_formNewRecord.show();
                        _btnNewRecord.attr("activestatus", "1");
                        //_btnNewRecord.attr("src", "Areas/VA012/Images/hide.png");
                        _btnNewRecord.removeClass("vis vis-plus");
                        _btnNewRecord.addClass("fa fa-minus");
                        _btnNewRecord.attr("title", "Collapse");
                    }
                    else {
                        $_formNewRecord.hide()
                        _btnNewRecord.attr("activestatus", "0");
                        //_btnNewRecord.attr("src", "Areas/VA012/Images/add.png");
                        _btnNewRecord.addClass("vis vis-plus");
                        _btnNewRecord.removeClass("fa fa-minus");
                        _btnNewRecord.attr("title", "Expand");
                    }
                    //to check mandatory fields and their logic to set background color
                    if (_txtAmount.getValue() <= 0)
                        _txtAmount.getControl().addClass("va012-mandatory");
                    if (_txtStatementNo.val() != "")
                        _txtStatementNo.removeClass("va012-mandatory");
                    if (_dtStatementDate.val() != "")
                        _dtStatementDate.removeClass("va012-mandatory");
                    newRecordForm.scheduleRefresh();
                    newRecordForm.prepayRefresh();
                    newRecordForm.refreshForm();
                    loadFunctions.setPaymentListHeight();
                });
                _btnUndo.on(VIS.Events.onTouchStartOrClick, function () {

                    //if (parseInt($_formNewRecord.attr("data-uid")) > 0) {
                    //    newRecordForm.scheduleRefresh();
                    //    newRecordForm.prepayRefresh();
                    //    _openingFromEdit = true;
                    //    childDialogs.statementListRecordEdit($_formNewRecord.attr("data-uid"))
                    //}
                    //else {
                    newRecordForm.scheduleRefresh();
                    newRecordForm.prepayRefresh();
                    newRecordForm.refreshForm();
                    //}

                });
                $_formNewRecord.hide();
                _cmbVoucherMatch.on('change', function () {

                    if (_cmbVoucherMatch.val() == "M") {

                        _divCtrlCashLine.find("*").prop("disabled", true);
                        _divCtrlCashLine.hide();

                        _divContraType.find("*").prop("disabled", true);
                        _divCashBook.find("*").prop("disabled", true);
                        _divTransferType.find("*").prop("disabled", true);
                        _divCheckNo.find("*").prop("disabled", true);
                        _divContraType.hide();
                        _divCashBook.hide();
                        _divTransferType.hide();
                        _divCheckNo.hide();


                        ////By Sukhwinder on 25-08-2016
                        var countVA034 = VIS.Utility.Util.getValueOfInt(VIS.DB.executeScalar("SELECT Count(AD_ModuleInfo_ID) FROM AD_ModuleInfo WHERE PREFIX='VA034_' AND IsActive = 'Y'"));
                        if (countVA034 > 0) {
                            _divVoucherNo.show();
                            _divVoucherNo.find("*").prop("disabled", false);
                        }
                        else {
                            _divVoucherNo.find("*").prop("disabled", true);
                            _divVoucherNo.hide();
                        }
                        //


                        _divCharge.find("*").prop("disabled", true);
                        _divTaxRate.find("*").prop("disabled", true);
                        _divTaxAmount.find("*").prop("disabled", true);

                        _divCharge.hide();
                        _divTaxRate.hide();
                        _divTaxAmount.hide();

                        divRow4Col1TrxAmt.show();
                        _divDifference.show();
                        _divDifferenceType.show();
                        _divCtrlPayment.show();
                        _divCtrlInvoice.show();
                        _divCtrlBusinessPartner.show();
                        _divPrepayOrder.show();
                        _divPaymentSchedule.show();


                        _divCtrlPayment.find("*").prop("disabled", false);
                        _divCtrlInvoice.find("*").prop("disabled", false);
                        _divCtrlBusinessPartner.find("*").prop("disabled", false);
                        _divPrepayOrder.find("*").prop("disabled", false);
                        _divPaymentSchedule.find("*").prop("disabled", false);
                        _txtPaymentSchedule.prop("disabled", true);
                        _btnPaymentSchedule.css('pointer-events', 'auto');

                        _cmbTaxRate.removeClass("va012-mandatory");
                        _txtCharge.removeClass("va012-mandatory");
                        //// _divMatch.find("*").prop("disabled", false);

                        /////
                        //_ctrlPayment.find("*").prop("disabled", false);
                        //_ctrlInvoice.find("*").prop("disabled", false);
                        //_ctrlOrder.find("*").prop("disabled", false);
                        //_btnPaymentSchedule.prop("disabled", false)
                        ////

                        //// _divMatch.removeClass("va012-disabled");
                        //_divMatch.show();
                        //_divPrepayOrder.show();
                        //_divPaymentSchedule.show();

                        //_divVoucher.find("*").prop("disabled", true);
                        //// _divVoucher.addClass("va012-disabled");
                        //_divVoucher.hide();
                    }
                    else if (_cmbVoucherMatch.val() == "V") {

                        _divCtrlCashLine.find("*").prop("disabled", true);
                        _divCtrlCashLine.hide();

                        _divContraType.find("*").prop("disabled", true);
                        _divCashBook.find("*").prop("disabled", true);
                        _divTransferType.find("*").prop("disabled", true);
                        _divCheckNo.find("*").prop("disabled", true);
                        _divContraType.hide();
                        _divCashBook.hide();
                        _divTransferType.hide();
                        _divCheckNo.hide();

                        _divVoucherNo.show();
                        _divCharge.show();
                        _divTaxRate.show();
                        _divTaxAmount.show();
                        _divVoucherNo.find("*").prop("disabled", false);
                        _divCharge.find("*").prop("disabled", false);
                        _divTaxRate.find("*").prop("disabled", false);
                        _divTaxAmount.find("*").prop("disabled", false);

                        _txtCharge.addClass("va012-mandatory");
                        _cmbTaxRate.addClass("va012-mandatory");
                        _divCtrlPayment.find("*").prop("disabled", true);
                        _divCtrlInvoice.find("*").prop("disabled", true);
                        _divCtrlBusinessPartner.find("*").prop("disabled", false);
                        _divPrepayOrder.find("*").prop("disabled", true);
                        _divPaymentSchedule.find("*").prop("disabled", true);
                        _btnPaymentSchedule.css('pointer-events', 'none');
                        divRow4Col1TrxAmt.hide();
                        _divDifference.hide();
                        _divDifferenceType.hide();
                        _divCtrlPayment.hide();
                        _divCtrlInvoice.hide();
                        _divCtrlBusinessPartner.hide();
                        _divPrepayOrder.hide();
                        _divPaymentSchedule.hide();





                        //_divVoucher.find("*").prop("disabled", false);
                        ////_divVoucher.removeClass("va012-disabled");
                        //_divVoucher.show();

                        ////  _divMatch.find("*").prop("disabled", true);
                        //_ctrlPayment.find("*").prop("disabled", true);
                        //_ctrlInvoice.find("*").prop("disabled", true);
                        //_ctrlOrder.find("*").prop("disabled", true);
                        //_btnPaymentSchedule.prop("disabled", true)
                        //// _divMatch.addClass("va012-disabled");
                        //_divMatch.hide();
                        //_divPrepayOrder.hide();
                        //_divPaymentSchedule.hide();
                    }
                    else if (_cmbVoucherMatch.val() == "C") {
                        _divContraType.show();
                        //Commented For Contra-29-1-16
                        //_divCashBook.show();
                        _divTransferType.show();
                        //_divCheckNo.show();
                        _divContraType.find("*").prop("disabled", false);
                        //Commented For Contra-29-1-16
                        //_divCashBook.find("*").prop("disabled", false);
                        _divTransferType.find("*").prop("disabled", false);
                        //_divCheckNo.find("*").prop("disabled", false);


                        _divVoucherNo.find("*").prop("disabled", true);
                        _divCharge.find("*").prop("disabled", true);
                        _divTaxRate.find("*").prop("disabled", true);
                        _divTaxAmount.find("*").prop("disabled", true);
                        _divVoucherNo.hide();
                        _divCharge.hide();
                        _divTaxRate.hide();
                        _divTaxAmount.hide();



                        _divCtrlPayment.find("*").prop("disabled", true);
                        _divCtrlInvoice.find("*").prop("disabled", true);
                        _divCtrlBusinessPartner.find("*").prop("disabled", true);
                        _divPrepayOrder.find("*").prop("disabled", true);
                        _divPaymentSchedule.find("*").prop("disabled", true);
                        _btnPaymentSchedule.css('pointer-events', 'none');
                        divRow4Col1TrxAmt.hide();
                        _divDifference.hide();
                        _divDifferenceType.hide();
                        _divCtrlPayment.hide();
                        _divCtrlInvoice.hide();
                        _divCtrlBusinessPartner.hide();
                        _divPrepayOrder.hide();
                        _divPaymentSchedule.hide();

                        _cmbTaxRate.removeClass("va012-mandatory");
                        _txtCharge.removeClass("va012-mandatory");

                    }
                    //remove the mandatory class
                    //_cmbTaxRate.removeClass("va012-mandatory");
                    //_txtCharge.removeClass("va012-mandatory");
                    _cmbContraType.prop('selectedIndex', 0);
                    _txtCharge.attr('chargeid', 0);
                    _txtCharge.val("");
                    _cmbTaxRate.prop('selectedIndex', 0);

                    //_divMore.show();
                    _btnMore.text(VIS.Msg.getMsg("VA012_More"));
                    loadFunctions.setPaymentListHeight();
                });
                _cmbDifferenceType.on('change', function () {

                    if (_cmbDifferenceType.val() == "CH") {

                        _divVoucherNo.show();
                        _divCharge.show();
                        _divTaxRate.show();
                        _divTaxAmount.show();
                        _divVoucherNo.find("*").prop("disabled", false);
                        _divCharge.find("*").prop("disabled", false);
                        _divTaxRate.find("*").prop("disabled", false);
                        _divTaxAmount.find("*").prop("disabled", false);
                        //mandatory fields
                        _txtCharge.addClass("va012-mandatory");
                        _cmbTaxRate.addClass("va012-mandatory");
                    }
                    else if (_cmbVoucherMatch.val() == "M") {
                        //_divVoucherNo.find("*").prop("disabled", true);
                        _divCharge.find("*").prop("disabled", true);
                        _divTaxRate.find("*").prop("disabled", true);
                        _divTaxAmount.find("*").prop("disabled", true);
                        //_divVoucherNo.hide();
                        _divCharge.hide();
                        _divTaxRate.hide();
                        _divTaxAmount.hide();
                        //clear the selected Values
                        _cmbCharge.prop('selectedIndex', 0);
                        _txtCharge.attr('chargeid', 0);
                        _txtCharge.val("");
                        _cmbTaxRate.prop('selectedIndex', 0);
                        _txtTaxAmount.setValue(0);
                        //remvoing the madatory class
                        _txtCharge.removeClass("va012-mandatory");
                        _cmbTaxRate.removeClass("va012-mandatory");
                    }

                    loadFunctions.setPaymentListHeight();
                });

                _cmbContraType.on('change', function () {

                    if (_cmbContraType.val() == "CB") {
                        _divCashBook.find("*").prop("disabled", false);
                        _divCheckNo.find("*").prop("disabled", false);
                        //_divCashBook.show();
                        // _divCheckNo.show();
                    }
                    else {
                        _cmbCashBook.prop('selectedIndex', 0);
                        _txtCheckNo.val("");
                        _divCashBook.find("*").prop("disabled", true);
                        _divCheckNo.find("*").prop("disabled", true);
                        //_divCashBook.hide();
                        // _divCheckNo.hide();
                    }
                    //Commented For Contra-29-1-16
                    //if (_cmbContraType.val() == "BB" || _cmbContraType.val() == "CB") {
                    if (_cmbContraType.val() == "BB") {

                        _divCtrlCashLine.find("*").prop("disabled", true);
                        _divCtrlCashLine.hide();


                        _divVoucherNo.show();
                        _divCharge.show();
                        _divTaxRate.show();
                        _divTaxAmount.show();
                        _divVoucherNo.find("*").prop("disabled", false);
                        _divCharge.find("*").prop("disabled", false);
                        _divTaxRate.find("*").prop("disabled", false);
                        _divTaxAmount.find("*").prop("disabled", false);
                        //should be mandatory
                        _txtCharge.addClass("va012-mandatory");
                        _cmbTaxRate.addClass("va012-mandatory");
                    }
                    else if (_cmbVoucherMatch.val() == "C") {
                        _divCtrlCashLine.show();
                        _divCtrlCashLine.find("*").prop("disabled", false);

                        _divVoucherNo.find("*").prop("disabled", true);
                        _divCharge.find("*").prop("disabled", true);
                        _divTaxRate.find("*").prop("disabled", true);
                        _divTaxAmount.find("*").prop("disabled", true);
                        _divVoucherNo.hide();
                        _divCharge.hide();
                        _divTaxRate.hide();
                        _divTaxAmount.hide();
                    }

                    loadFunctions.setPaymentListHeight();
                });
                //_cmbTransferType.on('change', function () {
                //    if (_cmbTransferType.val() == "CK") {
                //        _txtCheckNo.prop("disabled", false);
                //    }
                //    else {
                //        _txtCheckNo.prop("disabled", true);
                //    }

                //});

                _cmbTaxRate.on('change', function () {
                    _txtTaxAmount.setValue(0);
                    if (_cmbTaxRate.val() > 0) {
                        var _rate = VIS.DB.executeScalar("SELECT RATE FROM C_TAX WHERE C_TAX_ID=" + _cmbTaxRate.val());
                        //_txtTaxAmount.setValue(_txtAmount.getValue() - (_txtAmount.getValue() / ((_rate / 100) + 1)));
                        _txtTaxAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_txtAmount.getValue() - (_txtAmount.getValue() / ((_rate / 100) + 1))).toFixed(_stdPrecision));//handle precision
                        //_txtTaxAmount.val(((_txtAmount.val() * _rate) / 100).toFixed(_stdPrecision));
                        _cmbTaxRate.removeClass("va012-mandatory");
                    }
                    _txtTrxAmt.getControl().trigger("blur");
                });

                _btnMore.on(VIS.Events.onTouchStartOrClick, function () {

                    if (_btnMore.attr("visiblestatus") == "0") {
                        _btnMore.attr("visiblestatus", "1");
                        _btnMore.text(VIS.Msg.getMsg("VA012_Hide"));

                        //_divMatch.show();
                        //_divPrepayOrder.show();
                        //_divPaymentSchedule.show();
                        //_divVoucher.show();

                        _divContraType.show();

                        // _divCashBook.show();
                        _divTransferType.show();
                        // _divCheckNo.show();

                        _divCtrlCashLine.show();
                        _divVoucherNo.show();
                        _divCharge.show();
                        _divTaxRate.show();
                        _divTaxAmount.show();

                        divRow4Col1TrxAmt.show();
                        _divDifference.show();
                        _divDifferenceType.show();
                        _divCtrlPayment.show();
                        _divCtrlInvoice.show();
                        _divCtrlBusinessPartner.show();
                        _divPrepayOrder.show();
                        _divPaymentSchedule.show();
                    }
                    else {
                        _btnMore.attr("visiblestatus", "0");
                        _btnMore.text(VIS.Msg.getMsg("VA012_More"));
                        _cmbVoucherMatch.trigger('change');

                    }


                    // _divMore.hide();

                    loadFunctions.setPaymentListHeight();
                });
                //_btnMore.on('focus', function (event) {
                //    //$('foo').is(':focus');
                //    if (event.which == 32) {
                //        _btnMore.trigger('click');
                //    }



                //});

                $_formNewRecord.on("keyup", function (event) {

                    if (event.which == 32) {

                        if (_btnIn.is(':focus')) {
                            _btnIn.trigger('click');

                        }
                        if (_btnOut.is(':focus')) {
                            _btnOut.trigger('click');
                        }
                        if (_btnMore.is(':focus')) {
                            _btnMore.trigger('click');
                        }
                    }
                    if (event.which == 13) {
                        if (_btnSave.is(':focus')) {
                            _btnSave.trigger('click');
                        }
                    }

                });

                //_btnPaymentSchedule.on('focus', function () {
                //    _btnPaymentSchedule.blur();
                //    _btnPaymentSchedule.trigger('click');

                //});
                //_btnPrepay.on('focus', function () {
                //    _btnPrepay.blur();
                //    _btnPrepay.trigger('click');

                //});

                _btnSave.on(VIS.Events.onTouchStartOrClick, function () {

                    var _formData = newRecordForm.getFormData();
                    if (_formData[0]["_cmbBankAccount"] == null || _formData[0]["_cmbBankAccount"] == "" || _formData[0]["_cmbBankAccount"] == "0") {
                        VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                        return;
                    }

                    if (_formData[0]["_txtStatementNo"] == null || _formData[0]["_txtStatementNo"] == "" || _formData[0]["_txtStatementNo"] == "0") {
                        VIS.ADialog.info("VA012_PleaseEnterStatementNo", null, "", "");
                        return;
                    }

                    if (_formData[0]["_dtStatementDate"] == null || _formData[0]["_dtStatementDate"] == "" || _formData[0]["_dtStatementDate"] == "0") {
                        VIS.ADialog.info("VA012_PleaseEnterStatementDate", null, "", "");
                        return;
                    }
                    if (_formData[0]["_txtAmount"] == null || _formData[0]["_txtAmount"] == "" || _formData[0]["_txtAmount"] == "0" || _formData[0]["_txtAmount"] == "0.00") {
                        VIS.ADialog.info("VA012_PleaseEnterAmount", null, "", "");
                        return;
                    }

                    if (!(_formData[0]["_ctrlInvoice"] == null || _formData[0]["_ctrlInvoice"] == "" || _formData[0]["_ctrlInvoice"] == "0") &&
                        !(_formData[0]["_ctrlBusinessPartner"] == null || _formData[0]["_ctrlBusinessPartner"] == "" || _formData[0]["_ctrlBusinessPartner"] == "0") &&
                        (_formData[0]["_ctrlPayment"] == null || _formData[0]["_ctrlPayment"] == "" || _formData[0]["_ctrlPayment"] == "0") &&
                        (_formData[0]["_scheduleList"] == null || _formData[0]["_scheduleList"] == "")) {
                        VIS.ADialog.info("VA012_PleaseSelectPaySchedule", null, "", "");
                        return;
                    }

                    if (!(_formData[0]["_ctrlBusinessPartner"] == null || _formData[0]["_ctrlBusinessPartner"] == "" || _formData[0]["_ctrlBusinessPartner"] == "0") &&
                        (_formData[0]["_ctrlInvoice"] == null || _formData[0]["_ctrlInvoice"] == "" || _formData[0]["_ctrlInvoice"] == "0") &&
                        (_formData[0]["_ctrlPayment"] == null || _formData[0]["_ctrlPayment"] == "" || _formData[0]["_ctrlPayment"] == "0") &&
                        (_formData[0]["_scheduleList"] == null || _formData[0]["_scheduleList"] == "") &&
                        (_formData[0]["_ctrlOrder"] == null || _formData[0]["_ctrlOrder"] == "" || _formData[0]["_ctrlOrder"] == "0") &&
                        (_formData[0]["_cmbCharge"] == null || _formData[0]["_cmbCharge"] == "" || _formData[0]["_cmbCharge"] == "0")
                    ) {
                        VIS.ADialog.info("VA012_PleaseSelectAnyOne", null, "", "");
                        return;
                    }

                    if (_formData[0]["_cmbVoucherMatch"] == "C") {


                        if (_formData[0]["_cmbContraType"] == "BB") {
                            if (_formData[0]["_cmbCharge"] == null || _formData[0]["_cmbCharge"] == "" || _formData[0]["_cmbCharge"] == "0") {
                                VIS.ADialog.info("VA012_PleaseSelectCharge", null, "", "");
                                _txtCharge.addClass("va012-mandatory");
                                return;
                            }
                            else if (_formData[0]["_cmbTaxRate"] == null || _formData[0]["_cmbTaxRate"] == "" || _formData[0]["_cmbTaxRate"] == "0") {
                                VIS.ADialog.info("VA012_PleaseSelectTaxRate", null, "", "");
                                _cmbTaxRate.addClass("va012-mandatory");
                                return;
                            }
                        }
                        else if (_formData[0]["_cmbContraType"] == "CB") {

                            if (_formData[0]["_ctrlCashLine"] == null || _formData[0]["_ctrlCashLine"] == "" || _formData[0]["_ctrlCashLine"] == "0") {
                                VIS.ADialog.info("VA012_PleaseSelectCashJournalLine", null, "", "");
                                return;
                            }
                            //Commented For Contra-29-1-16
                            //if (_formData[0]["_cmbCashBook"] == null || _formData[0]["_cmbCashBook"] == "" || _formData[0]["_cmbCashBook"] == "0") {
                            //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_PleaseSelectCashBook"), null, "", "");
                            //    return;
                            //}
                            //if (_formData[0]["_cmbCharge"] == null || _formData[0]["_cmbCharge"] == "" || _formData[0]["_cmbCharge"] == "0") {
                            //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_PleaseSelectCharge"), null, "", "");
                            //    return;
                            // }
                            //End Commented For Contra-29-1-16


                            //if (_formData[0]["_cmbTransferType"] == null || _formData[0]["_cmbTransferType"] == "" || _formData[0]["_cmbTransferType"] == "0") {
                            //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_PleaseSelectTransferType"), null, "", "");
                            //    return;
                            //}
                            //else if (_formData[0]["_cmbTransferType"] == "CK") {
                            //    if (_formData[0]["_txtCheckNo"] == null || _formData[0]["_txtCheckNo"] == "" || _formData[0]["_txtCheckNo"] == "0") {
                            //        VIS.ADialog.info(VIS.Msg.getMsg("VA012_PleaseEnterCheckNo"), null, "", "");
                            //        return;
                            //    }
                            //}
                        }
                        else {
                            VIS.ADialog.info("VA012_PleaseSelectContraType", null, "", "");
                            return;
                        }
                    }
                    if (_formData[0]["_cmbVoucherMatch"] == "V") {
                        if (_formData[0]["_cmbCharge"] == null || _formData[0]["_cmbCharge"] == "" || _formData[0]["_cmbCharge"] == "0") {
                            VIS.ADialog.info("VA012_PleaseSelectCharge", null, "", "");
                            _txtCharge.addClass("va012-mandatory");
                            return;
                        }
                        else if (_formData[0]["_cmbTaxRate"] == null || _formData[0]["_cmbTaxRate"] == "" || _formData[0]["_cmbTaxRate"] == "0") {
                            VIS.ADialog.info("VA012_PleaseSelectTaxRate", null, "", "");
                            _cmbTaxRate.addClass("va012-mandatory");
                            return;
                        }
                    }

                    //if (_formData[0]["_ctrlBusinessPartner"] == null || _formData[0]["_ctrlBusinessPartner"] == "" || _formData[0]["_ctrlBusinessPartner"] == "0") {
                    //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_SelectBusinessPartner"), null, "", "");
                    //    return;
                    //}
                    //if (_formData[0]["_scheduleList"] != null && _formData[0]["_scheduleList"] != "") {

                    //    if (!loadFunctions.checkScheduleTotal(_formData[0]["_scheduleList"], _txtAmount.val(), parseInt($_formNewRecord.attr("data-uid")))) {
                    //        return;
                    //    }
                    //}

                    ///* change by pratap */
                    //if (_formData[0]["_scheduleList"] != null && _formData[0]["_scheduleList"] != "") {
                    //    if (parseInt($_formNewRecord.attr("data-uid")) > 0) {
                    //        if (!loadFunctions.checkScheduleTotal(_formData[0]["_scheduleList"], _txtAmount.val(), parseInt($_formNewRecord.attr("data-uid")))) {
                    //            return;
                    //        }
                    //    }
                    //    else {
                    //        var amount = 0;
                    //        for (var i = 0; i < _scheduleAmount.length ; i++) {
                    //            amount += VIS.Utility.Util.getValueOfDecimal(_scheduleAmount[i]);
                    //        }
                    //        if (amount.toFixed(_stdPrecision) != _txtAmount.val() /*&& scheduleAmount[0] != "0"*/) {
                    //            VIS.ADialog.info(VIS.Msg.getMsg("VA012_ScheduleAmtNotMatched"), null, "", "");
                    //            return;
                    //        }
                    //    }
                    //}
                    ///*change by pratap*/




                    if (parseFloat(_formData[0]["_txtDifference"]) != 0 && _formData[0]["_cmbVoucherMatch"] == "M") {
                        if (_formData[0]["_cmbDifferenceType"] == null || _formData[0]["_cmbDifferenceType"] == "" || _formData[0]["_cmbDifferenceType"] == "0") {
                            VIS.ADialog.info("VA012_PleaseSelectDifferenceType", null, "", "");
                            return;
                        }
                        //not required this Message
                        //else if (((_txtAmount.getValue() > 0 && parseFloat(_formData[0]["_txtDifference"]) < 0) || 
                        //          (_txtAmount.getValue() < 0 && parseFloat(_formData[0]["_txtDifference"]) > 0))
                        //        && _formData[0]["_cmbDifferenceType"] != "OU") {
                        //    VIS.ADialog.info("VA012_PleaseSelectDifferenceTypeOU", null, "", "");
                        //    return;
                        //}
                    }



                    busyIndicator($root, true, "absolute");
                    newRecordForm.insertNewRecord(_formData, newRecordForm.afterInsertion);


                    //// Inserting New Record
                    //if (_formData[0]._bankStatementLineID <= 0) {
                    //    var _sql = " SELECT TBSL.C_BANKSTATEMENTLINE_ID, "
                    //    + " TBSL.VA012_STATEMENTNO, "
                    //    + " TBSL.VA012_PAGE,  "
                    //    + " TBSL.LINE "
                    //    + " FROM VA012_TEMPSTATEMENT tbsl "
                    //    + " WHERE TBSL.AMOUNT=" + _formData[0]._txtAmount
                    //    + " AND TBSL.C_CHARGE_ID=" + _formData[0]._cmbCharge
                    //    + " AND TBSL.C_TAX_ID=" + _formData[0]._cmbTaxRate
                    //    //+ " AND TBSL.C_CURRENCY_ID=" + _formData[0]._cmbCurrency
                    //    + " AND TBSL.C_CURRENCY_ID=" + _currencyId
                    //    + " AND TBSL.STATEMENTDATE BETWEEN TBSL.STATEMENTDATE-30 AND TBSL.STATEMENTDATE";
                    //    VIS.DB.executeDataSet(_sql, null, checkExisting);
                    //    function checkExisting(_dsIn) {
                    //          
                    //        _chkSave = 1;
                    //        if (_dsIn != null) {
                    //            if (_dsIn.tables[0].rows.length > 0) {

                    //                if (VIS.ADialog.ask(VIS.Msg.getMsg("VA012_SimilarStatementExist") + ":" + _dsIn.tables[0].rows[0].cells.va012_statementno + "/" + _dsIn.tables[0].rows[0].cells.va012_page + "/" + _dsIn.tables[0].rows[0].cells.line)) {
                    //                    _chkSave = 0;
                    //                    newRecordForm.insertNewRecord(_formData, newRecordForm.afterInsertion);
                    //                }
                    //                else {
                    //                    _chkSave = 0;
                    //                    return;
                    //                }
                    //                //return VIS.Msg.getMsg("VA012_SimilarStatementExist") + ":" + _dsIn.tables[0].rows[0].cells.va012_statementno + "/" + _dsIn.tables[0].rows[0].cells.va012_page + "/" + _dsIn.tables[0].rows[0].cells.line;
                    //            }
                    //            else {
                    //                if (_chkSave == 1) {
                    //                    newRecordForm.insertNewRecord(_formData, newRecordForm.afterInsertion);
                    //                }
                    //            }
                    //        }
                    //    }

                    //}
                    //    //Edit Existing  Record
                    //else {
                    //    newRecordForm.insertNewRecord(_formData, newRecordForm.afterInsertion);
                    //}

                });
                //_btnCreatePayment.on(VIS.Events.onTouchStartOrClick, function () {
                //      
                //    var _bankStatementLineID = parseInt($_formNewRecord.attr("data-uid"));
                //    if (_bankStatementLineID > 0) {
                //        $.ajax({
                //            url: VIS.Application.contextUrl + "BankStatement/CreatePayment",
                //            type: "GET",
                //            datatype: "json",
                //            contentType: "application/json; charset=utf-8",
                //            async: false,
                //            data: ({ _bankStatementLineID: _bankStatementLineID }),
                //            success: function (data) {
                //                if (data != null) {
                //                      
                //                    alert(data);
                //                    _lstPayments.html("");
                //                    _paymentPageNo = 1;
                //                    loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val());
                //                    _lstStatement.html("");
                //                    _statementPageNo = 1;
                //                    childDialogs.loadStatement(_statementID);
                //                    newRecordForm.refreshForm();

                //                }
                //            },
                //            error: function () {
                //                alert(data);
                //            }
                //        });
                //    }

                //});


                //_txtStatementNo.on("keypress keyup blur", function (event) {
                //    $(this).val($(this).val().replace(/[^\d.].+/, ""));
                //    if ((event.which < 48 || event.which > 57)) {
                //        event.preventDefault();
                //    }

                //    //this.value = this.value.replace(/[^0-9.]/g, '');
                //    //this.value = this.value.replace(/(\..*)\./g, '$1');
                //});
                //_txtAmount.on("keypress keyup blur", function (event) {
                //    $(this).val($(this).val().replace(/[^\d]+/, ""));
                //    if ((event.which < 48 || event.which > 57)) {
                //        event.preventDefault();
                //    }
                //});
                //_txtStatementLine.on("keypress keyup blur", function (event) {
                //    $(this).val($(this).val().replace(/[^\d].+/, ""));
                //    if ((event.which < 48 || event.which > 57)) {
                //        event.preventDefault();
                //    }
                //});

                //_txtStatementPage.on("keypress keyup blur", function (event) {
                //    $(this).val($(this).val().replace(/[^\d].+/, ""));
                //    if ((event.which < 48 || event.which > 57)) {
                //        event.preventDefault();
                //    }
                //});

                //_txtTaxAmount.on("keypress keyup blur", function (event) {
                //    $(this).val($(this).val().replace(/[^\d]+/, ""));
                //    if ((event.which < 48 || event.which > 57)) {
                //        event.preventDefault();
                //    }
                //});



                _txtStatementNo.on("keypress", function (event) {
                    //if (event.which != 8 && event.which != 0 && (event.which < 48 || event.which > 57)) {
                    //    return false;
                    //}
                    _txtStatementPage.val("1");
                    _txtStatementLine.val("10");
                });
                //on change event of Statement Name to set background color logic
                _txtStatementNo.on("change", function () {
                    if (_txtStatementNo.val() == "") {
                        _txtStatementNo.addClass("va012-mandatory");
                    }
                    else
                        _txtStatementNo.removeClass("va012-mandatory");

                });
                _txtStatementLine.on("keypress", function (event) {
                    if (event.which != 8 && event.which != 0 && (event.which < 48 || event.which > 57)) {
                        return false;
                    }
                });


                _txtStatementPage.on("keypress", function (event) {
                    if (event.which != 8 && event.which != 0 && (event.which < 48 || event.which > 57)) {
                        return false;
                    }
                    _txtStatementLine.val("10");
                });
                _btnStatementNo.on(VIS.Events.onTouchStartOrClick, function () {
                    newRecordForm.scheduleRefresh();
                    newRecordForm.prepayRefresh();
                    newRecordForm.refreshForm();

                    if (_txtStatementNo.val() == "" || _txtStatementNo.val() == null) {
                        _txtStatementNo.val("0");
                    }
                    loadFunctions.getMaxStatement("BT");
                    $_formNewRecord.attr("data-uid", 0);
                    // _txtStatementNo.val(parseInt(_txtStatementNo.val()) + 1);
                    _txtStatementPage.val("1");
                    _txtStatementLine.val("10");
                });
                //_btnStatementNo.on('focus', function () { _btnStatementNo.trigger('click') });
                _txtTaxAmount.getControl().on("change", function () {
                    if (_txtTaxAmount.getValue() == 0 || _txtTaxAmount.getValue() == null) {
                        _txtTaxAmount.setValue(0);
                    }
                    else {
                        _txtTaxAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_txtTaxAmount.getValue().toFixed(_stdPrecision)));
                    }
                });

                _txtTrxAmt.getControl().on("blur", function () {
                    if (_txtTrxAmt.getValue() == 0) {
                        _txtTrxAmt.setValue(0);
                        _txtTrxAmt.getControl().addClass('va012-mandatory');
                    }
                    else {
                        _txtTrxAmt.setValue(VIS.Utility.Util.getValueOfDecimal(_txtTrxAmt.getValue().toFixed(_stdPrecision)));
                        _txtTrxAmt.getControl().removeClass('va012-mandatory');
                    }
                    //if (parseInt($_formNewRecord.attr("data-uid")) <= 0)
                    if (_txtDifference.getControl().attr("vchangable") == "Y") {
                        _txtDifference.setValue(0);
                        _divDifferenceType.find("*").prop("disabled", true);
                    }
                    if (_cmbVoucherMatch.val() == "M") {

                        //if (parseInt($_formNewRecord.attr("data-uid")) <= 0)
                        if (_txtDifference.getControl().attr("vchangable") == "Y") {
                            _txtDifference.setValue(VIS.Utility.Util.getValueOfDecimal((_txtTrxAmt.getValue() - _txtAmount.getValue()).toFixed(_stdPrecision)));
                            if (_txtDifference.getValue() != 0) {
                                _txtDifference.getControl().removeClass('va012-mandatory');//color change
                                _divDifferenceType.find("*").prop("disabled", false);
                            }
                        }

                        if (_cmbTaxRate.val() > 0 && _txtDifference.getValue() != 0) {
                            _txtDifference.getControl().removeClass('va012-mandatory');//color change
                            var _rate = VIS.DB.executeScalar("SELECT RATE FROM C_TAX WHERE C_TAX_ID=" + _cmbTaxRate.val());
                            _txtTaxAmount.setValue(VIS.Utility.Util.getValueOfDecimal((_txtDifference.getValue() - (_txtDifference.getValue() / ((_rate / 100) + 1))).toFixed(_stdPrecision)));//handle precision
                        }
                    }

                });


                _txtAmount.getControl().on("blur", function () {
                    if (_txtAmount.getValue() == "" || _txtAmount.getValue() == 0 || _txtAmount.getValue() == null) {
                        _txtAmount.setValue(0);
                        _txtAmount.getControl().addClass("va012-mandatory");
                    }
                    //repeated code not required here
                    //if (_txtAmount.getValue() > 0)
                    //    _txtAmount.getControl().removeClass("va012-mandatory");
                    //else
                    //    _txtAmount.getControl().addClass("va012-mandatory");

                    //_txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_txtAmount.getValue().toFixed(_stdPrecision)));

                    //if (_btnOut.attr("v_active") == "1") {
                    //    if (_txtAmount.val() > 0) {
                    //        _txtAmount.val((-(_txtAmount.val())).toFixed(_stdPrecision));
                    //    }
                    //}
                    //else {
                    //    if (_txtAmount.val() < 0) {
                    //        _txtAmount.val((-(_txtAmount.val())).toFixed(_stdPrecision));
                    //    }
                    //}




                    if (_btnOut.attr("v_active") == "1" && _txtAmount.getValue() > 0) {
                        _txtAmount.setValue(-VIS.Utility.Util.getValueOfDecimal(_txtAmount.getValue().toFixed(_stdPrecision)));
                    }

                    if (_btnIn.attr("v_active") == "1" && _txtAmount.getValue() < 0) {

                        _txtAmount.setValue(-1 * VIS.Utility.Util.getValueOfDecimal(_txtAmount.getValue().toFixed(_stdPrecision)));
                        //_btnOut.removeClass("va012-inactive");
                        //_btnOut.addClass("va012-active");
                        //_btnOut.attr("v_active", "1");
                        //_btnIn.removeClass("va012-active");
                        //_btnIn.addClass("va012-inactive");
                        //_btnIn.attr("v_active", "0");
                    }


                    if (_txtAmount.getValue() < 0) {
                        _btnOut.removeClass("va012-inactive");
                        _btnOut.addClass("va012-active");
                        _btnOut.attr("v_active", "1");
                        _btnIn.removeClass("va012-active");
                        _btnIn.addClass("va012-inactive");
                        _btnIn.attr("v_active", "0");
                        _txtAmount.getControl().addClass("va012-mandatory");
                    }
                    else if (_txtAmount.getValue() > 0) {
                        _btnIn.removeClass("va012-inactive");
                        _btnIn.addClass("va012-active");
                        _btnIn.attr("v_active", "1");
                        _btnOut.removeClass("va012-active");
                        _btnOut.addClass("va012-inactive");
                        _btnOut.attr("v_active", "0");
                        _txtAmount.getControl().removeClass("va012-mandatory");
                    }

                    if (_cmbTaxRate.val() > 0) {
                        var _rate = VIS.DB.executeScalar("SELECT RATE FROM C_TAX WHERE C_TAX_ID=" + _cmbTaxRate.val());
                        _txtTaxAmount.setValue(VIS.Utility.Util.getValueOfDecimal((_txtAmount.getValue() - (_txtAmount.getValue() / ((_rate / 100) + 1))).toFixed(_stdPrecision)));
                    }
                    _txtTrxAmt.getControl().trigger('blur');
                    //if ($_ctrlInvoice.value) {
                    //    loadFunctions.checkInvoiceCondition($_ctrlInvoice.value, _txtAmount.val());
                    //}
                    //if ($_ctrlPayment.value) {
                    //    loadFunctions.checkPaymentCondition($_ctrlPayment.value, 0, _txtAmount.val());
                    //    //pratap
                    //    //loadFunctions.checkFormPaymentCondition($_ctrlPayment.value, _txtAmount.val());
                    //}
                });
                _btnAmount.hover(function (e) {

                    $tooltip = $('<div  class="va012-div-tooltip"><h4>' + VIS.Msg.getMsg("VA012_Information") + '</h4><p>' + VIS.Msg.getMsg("VA012_InformationText") + '</p></div>');
                    $root.append($tooltip);
                    $('.va012-div-tooltip').show();
                }, function () {

                    // $('.va012-div-tooltip').hide();
                    $root.find(".va012-div-tooltip").remove();
                });
                //on change event of Statement Date to set background color logic
                _dtStatementDate.on("change", function () {
                    if (_dtStatementDate.val() == "") {
                        _dtStatementDate.addClass("va012-mandatory");
                    }
                    else {
                        _dtStatementDate.removeClass("va012-mandatory");
                        var amt = 0;
                        if (_cmbVoucherMatch.val() == "M") {
                            if (_txtPaymentSchedule.val() != 0 && ($_ctrlPayment.getValue() == null || $_ctrlPayment.getValue() == 0)) {
                                if (_scheduleList.length != 0) {
                                    var transtype = "IS";
                                    var _ds = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetConvtAmount", { recordID: _scheduleList.join(), bnkAct_Id: _cmbBankAccount.val(), transcType: transtype, stmtDate: _dtStatementDate.val() });
                                    for (var i = 0; i < _ds.length; i++) {
                                        if (_ds.length == 0 || _ds[i].DueAmount == 0) {
                                            _txtAmount.setValue();
                                            _txtTrxAmt.setValue();
                                            _txtDifference.setValue();
                                            VIS.ADialog.info("VA012_ConversionRateNotFound", null, "", "");
                                            return;
                                        }
                                        else {
                                            amt += _ds[i].DueAmount;
                                        }
                                    }
                                    if (amt != 0) {
                                        _txtAmount.setValue(amt);
                                        _txtTrxAmt.setValue(amt);
                                    }
                                    else {
                                        _txtAmount.setValue();
                                        _txtTrxAmt.setValue();
                                        _txtDifference.setValue();
                                        VIS.ADialog.info("VA012_ConversionRateNotFound", null, "", "");
                                        return;
                                    }
                                }
                            }
                            else if ((_txtPaymentSchedule.val() == null || _txtPaymentSchedule.val() == 0) && ($_ctrlPayment.getValue() != null && $_ctrlPayment.getValue() != 0)) {
                                var transtype = "PY";
                                var _ds = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetConvtAmount", { recordID: $_ctrlPayment.getValue(), bnkAct_Id: _cmbBankAccount.val(), transcType: transtype, stmtDate: _dtStatementDate.val() });
                                for (var i = 0; i < _ds.length; i++) {
                                    if (_ds.length == 0 || _ds[i].DueAmount == 0) {
                                        _txtAmount.setValue();
                                        _txtTrxAmt.setValue();
                                        VIS.ADialog.info("VA012_ConversionRateNotFound", null, "", "");
                                        return;
                                    }
                                    else {
                                        amt += _ds[i].DueAmount;
                                    }
                                }
                                if (amt != 0) {
                                    _txtAmount.setValue(amt);
                                    _txtTrxAmt.setValue(amt);
                                    _txtDifference.setValue();
                                }
                                else {
                                    _txtAmount.setValue();
                                    _txtTrxAmt.setValue();
                                    _txtDifference.setValue();
                                    VIS.ADialog.info("VA012_ConversionRateNotFound", null, "", "");
                                    return;
                                }
                            }
                        }
                    }
                });
                _btnAmount.mousemove(function (e) {

                    $(".va012-div-tooltip").css('top', e.pageY - 50).css('left', e.pageX);
                });


                _btnIn.on(VIS.Events.onTouchStartOrClick, function () {
                    if (_txtAmount.getValue() < 0) {
                        _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_txtAmount.getValue().toFixed(_stdPrecision)) * -1);
                    }

                    _btnIn.removeClass("va012-inactive");
                    _btnIn.addClass("va012-active");
                    _btnIn.attr("v_active", "1");
                    _btnOut.removeClass("va012-active");
                    _btnOut.addClass("va012-inactive");
                    _btnOut.attr("v_active", "0");
                    _txtAmount.getControl().blur();
                    //if ($_ctrlInvoice.value) {
                    //    loadFunctions.checkInvoiceCondition($_ctrlInvoice.value, _txtAmount.val());
                    //}
                    //if ($_ctrlPayment.value) {
                    //    loadFunctions.checkFormPaymentCondition($_ctrlPayment.value, _txtAmount.val());
                    //}
                });


                _btnOut.on(VIS.Events.onTouchStartOrClick, function () {
                    if (_txtAmount.getValue() > 0) {
                        _txtAmount.setValue(VIS.Utility.Util.getValueOfDecimal(_txtAmount.getValue().toFixed(_stdPrecision)) * -1);
                    }
                    _btnOut.removeClass("va012-inactive");
                    _btnOut.addClass("va012-active");
                    _btnOut.attr("v_active", "1");
                    _btnIn.removeClass("va012-active");
                    _btnIn.addClass("va012-inactive");
                    _btnIn.attr("v_active", "0");
                    _txtAmount.getControl().blur();
                    //if ($_ctrlInvoice.value) {
                    //    loadFunctions.checkInvoiceCondition($_ctrlInvoice.value, _txtAmount.val());
                    //}
                    //if ($_ctrlPayment.value) {
                    //    loadFunctions.checkFormPaymentCondition($_ctrlPayment.value, _txtAmount.val());
                    //}
                });


                //_btnIn.on('focus', function (event) {

                //    if (event.which == 32) {
                //        _btnIn.trigger('click');
                //    }

                //});
                //_btnOut.on('focus', function (event) {
                //    if (event.which == 32) {
                //        _btnOut.trigger('click');
                //    }

                //});

                //_ctrlPayment.find("input[type=text]").on("keyup", function(event){
                //    _ctrlInvoice.find("input[type=text]").focus();
                //});

                //_ctrlInvoice.find("input[type=text]").on("keyup", function (event) {
                //    _ctrlBusinessPartner.find("input[type=text]").focus();
                //});
            },
            getNewRecordDesign: function () { },
            getNewRecordControls: function () {
                _txtStatementNo = $_formNewRecord.find("#VA012_txtStatementNo_" + $self.windowNo);
                _btnStatementNo = $_formNewRecord.find("#VA012_btnStatementNo_" + $self.windowNo);
                _txtStatementPage = $_formNewRecord.find("#VA012_txtStatementPage_" + $self.windowNo);
                _txtStatementLine = $_formNewRecord.find("#VA012_txtStatementLine_" + $self.windowNo);
                _dtStatementDate = $_formNewRecord.find("#VA012_dtStatementDate_" + $self.windowNo);
                _cmbPaymentMethod = $_formNewRecord.find("#VA012_cmbPaymentMethod_" + $self.windowNo);
                // _cmbCurrency = $_formNewRecord.find("#VA012_cmbCurrency_" + $self.windowNo);
                _cmbContraType = $_formNewRecord.find("#VA012_cmbContraType_" + $self.windowNo);
                _cmbCashBook = $_formNewRecord.find("#VA012_cmbCashBook_" + $self.windowNo);
                _cmbTransferType = $_formNewRecord.find("#VA012_cmbTransferType_" + $self.windowNo);
                _txtCheckNo = $_formNewRecord.find("#VA012_txtCheckNo_" + $self.windowNo);
                _cmbVoucherMatch = $_formNewRecord.find("#VA012_cmbVoucherMatch_" + $self.windowNo);
                //_txtAmount = $_formNewRecord.find("#VA012_txtAmount_" + $self.windowNo);
                _txtAmount.getControl().addClass('va012-mandatory');
                //_txtTrxAmt = $_formNewRecord.find("#VA012_txtTrxAmt_" + $self.windowNo);
                _txtTrxAmt.getControl().addClass('va012-mandatory');
                //_txtDifference = $_formNewRecord.find("#VA012_txtDifference_" + $self.windowNo);
                _txtDifference.getControl().addClass('va012-mandatory');
                _cmbDifferenceType = $_formNewRecord.find("#VA012_cmbDifferenceType_" + $self.windowNo);
                _txtVoucherNo = $_formNewRecord.find("#VA012_txtVoucherNo_" + $self.windowNo);
                _txtDescription = $_formNewRecord.find("#VA012_txtDescription_" + $self.windowNo);
                _cmbCharge = $_formNewRecord.find("#VA012_cmbCharge_" + $self.windowNo);
                _txtCharge = $_formNewRecord.find("#VA012_txtCharge_" + $self.windowNo);
                _cmbTaxRate = $_formNewRecord.find("#VA012_cmbTaxRate_" + $self.windowNo);
                //_txtTaxAmount = $_formNewRecord.find("#VA012_txtTaxAmount_" + $self.windowNo);
                _ctrlCashLine = $_formNewRecord.find("#VA012_ctrlCashLine_" + $self.windowNo);
                _ctrlPayment = $_formNewRecord.find("#VA012_ctrlPayment_" + $self.windowNo);
                _ctrlOrder = $_formNewRecord.find("#VA012_ctrlOrder_" + $self.windowNo);
                _ctrlInvoice = $_formNewRecord.find("#VA012_ctrlInvoice_" + $self.windowNo);
                _ctrlBusinessPartner = $_formNewRecord.find("#VA012_ctrlBusinessPartner_" + $self.windowNo);
                _chkUseNextTime = $_formNewRecord.find("#VA012_chkUseNextTime_" + $self.windowNo);
                _btnSave = $_formNewRecord.find("#VA012_btnSave_" + $self.windowNo);
                _btnPaymentSchedule = $_formNewRecord.find("#VA012_btnPaymentSchedule_" + $self.windowNo);
                _btnPrepay = $_formNewRecord.find("#VA012_btnPrepay_" + $self.windowNo);
                _txtPaymentSchedule = $_formNewRecord.find("#VA012_txtPaymentSchedule_" + $self.windowNo);
                _txtPrepayOrder = $_formNewRecord.find("#VA012_txtPrepayOrder_" + $self.windowNo);
                // _btnCreatePayment = $_formNewRecord.find("#VA012_btnCreatePayment_" + $self.windowNo);
                _btnNewRecord = $_formBtnNewRecord.find("#VA012_btnNewRecord_" + $self.windowNo);
                _btnUndo = $_formBtnNewRecord.find("#VA012_btnUndo_" + $self.windowNo);
                //_btnDelete = $_formBtnNewRecord.find("#VA012_btnDelete_" + $self.windowNo);
                _btnDelete = $root.find("#VA012_btnDelete_" + $self.windowNo);
                _btnAmount = $_formNewRecord.find("#VA012_btnAmount_" + $self.windowNo);
                _btnIn = $_formNewRecord.find("#VA012_btnIn_" + $self.windowNo);
                _btnOut = $_formNewRecord.find("#VA012_btnOut_" + $self.windowNo);
            },


            loadPaymentMethod: function () {

                var _sql = "SELECT VA009_NAME,VA009_PAYMENTMETHOD_ID FROM VA009_PAYMENTMETHOD WHERE ISACTIVE='Y'  AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID() + " AND AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID();
                var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadPaymentMethod);
                function callbackloadPaymentMethod(_ds) {
                    _cmbPaymentMethod.html("");
                    _cmbPaymentMethod.append("<option value=0 ></option>");

                    if (_ds != null) {
                        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                            _cmbPaymentMethod.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.va009_paymentmethod_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.va009_name) + "</option>");
                        }
                    }
                    _ds.dispose();
                    _ds = null;
                    _sql = null;
                    _cmbPaymentMethod.prop('selectedIndex', 0);
                }
            },
            loadCurrency: function () {
                //var _sql = "SELECT ISO_CODE,C_CURRENCY_ID FROM C_CURRENCY WHERE ISACTIVE='Y'";
                //var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadCurrency);
                //function callbackloadCurrency(_ds) {
                //    _cmbCurrency.html("");
                //    if (_ds != null) {
                //        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                //            _cmbCurrency.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_currency_id) + ">" + _ds.tables[0].rows[i].cells.iso_code + "</option>");
                //        }
                //    }
                //    _ds.dispose();
                //    _ds = null;
                //    _sql = null;

                _currencyId = VIS.DB.executeScalar("SELECT C_CURRENCY_ID FROM C_BANKACCOUNT WHERE C_BANKACCOUNT_ID=" + _cmbBankAccount.val());
                _stdPrecision = VIS.DB.executeScalar("SELECT STDPRECISION FROM C_CURRENCY WHERE C_CURRENCY_ID=" + _currencyId);


                //newRecordForm.loadCashBook();

                //var _ds= VIS.DB.executeDataSet("SELECT BA.C_CURRENCY_ID,   CR.STDPRECISION FROM C_BANKACCOUNT BA INNER JOIN C_CURRENCY CR ON CR.C_CURRENCY_ID=BA.C_CURRENCY_ID WHERE BA.C_BANKACCOUNT_ID=" + _cmbBankAccount.val(), null, callbackloadCurrency);
                //function callbackloadCurrency(_ds) {

                //    if (_ds != null) {
                //        _currencyId = VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[0].cells.c_currency_id);
                //        _stdPrecision = VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[0].cells.stdprecision);
                //        _ds.dispose();
                //        _ds = null;
                //        _sql = null;
                //    }
                //}

                //    if (_currencyId != null && _currencyId > 0) {
                //        _cmbCurrency.val(_currencyId).prop('selected', true);
                //        _currencyCode = _cmbCurrency.children()[_cmbCurrency[0].selectedIndex].text;
                //    }
                //    else {
                //        loadFunctions.getBaseCurrency();
                //    }
                //}
            },
            loadCashBook: function () {

                // var _sql = "SELECT NAME,C_CashBook_ID FROM C_CashBook WHERE ISACTIVE='Y'  AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID() + " AND AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID() + " AND C_CURRENCY_ID=" + _currencyId;
                var _sql = "SELECT NAME,C_CashBook_ID FROM C_CashBook WHERE ISACTIVE='Y'  AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID() + " AND AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID();
                var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadCashBook);
                function callbackloadCashBook(_ds) {
                    _cmbCashBook.html("");
                    _cmbCashBook.append("<option value=0 ></option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                            _cmbCashBook.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_cashbook_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                        }
                    }
                    _ds.dispose();
                    _ds = null;
                    _sql = null;
                    _cmbCashBook.prop('selectedIndex', 0);
                }
            },

            loadContraType: function () {
                _cmbContraType.html("");
                _cmbContraType.append("<option value=0 ></option>");
                _cmbContraType.append("<option value='BB' >" + VIS.Msg.getMsg("VA012_BankToBank") + "</option>");
                _cmbContraType.append("<option value='CB' >" + VIS.Msg.getMsg("VA012_CashToBank") + "</option>");
                _cmbContraType.prop('selectedIndex', 0);
            },

            loadTransferType: function () {
                _cmbTransferType.html("");
                _cmbTransferType.append("<option value=0 ></option>");
                _cmbTransferType.append("<option value='CH' >" + VIS.Msg.getMsg("VA012_Cash") + "</option>");
                _cmbTransferType.append("<option value='CK' >" + VIS.Msg.getMsg("VA012_Check") + "</option>");
                _cmbTransferType.prop('selectedIndex', 0);
            },

            loadVoucherMatch: function () {
                _cmbVoucherMatch.html("");
                _cmbVoucherMatch.append("<option value='M' >" + VIS.Msg.getMsg("VA012_Payment") + "</option>");
                _cmbVoucherMatch.append("<option value='V' >" + VIS.Msg.getMsg("VA012_Voucher") + "</option>");
                _cmbVoucherMatch.append("<option value='C' >" + VIS.Msg.getMsg("VA012_Contra") + "</option>");
                _cmbVoucherMatch.prop('selectedIndex', 0);
            },
            loadDifferenceType: function () {
                _cmbDifferenceType.html("");
                _cmbDifferenceType.append("<option value=0 ></option>");
                _cmbDifferenceType.append("<option value='OU' >" + VIS.Msg.getMsg("VA012_OverUnderPayment") + "</option>");
                _cmbDifferenceType.append("<option value='DA' >" + VIS.Msg.getMsg("VA012_DiscountAmount") + "</option>");
                _cmbDifferenceType.append("<option value='WO' >" + VIS.Msg.getMsg("VA012_WriteoffAmount") + "</option>");
                _cmbDifferenceType.append("<option value='CH' >" + VIS.Msg.getMsg("VA012_Charge") + "</option>");
                _cmbDifferenceType.prop('selectedIndex', 0);
            },

            loadCharge: function () {
                //Not in Use
                var _sql = "SELECT NAME,C_CHARGE_ID FROM C_CHARGE WHERE ISACTIVE='Y' AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID() + " AND AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID();
                var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadCharge);
                function callbackloadCharge(_ds) {
                    _cmbCharge.html("");
                    _cmbCharge.append("<option value=0 ></option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                            _cmbCharge.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_charge_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                        }
                    }
                    _ds.dispose();
                    _ds = null;
                    _sql = null;
                    _cmbCharge.prop('selectedIndex', 0);
                }
            },
            loadTaxRate: function () {

                //var _sql = "SELECT NAME,C_TAX_ID FROM C_TAX WHERE ISACTIVE='Y'  AND AD_CLIENT_ID=" + VIS.Env.getCtx().getAD_Client_ID() + " AND AD_ORG_ID=" + VIS.Env.getCtx().getAD_Org_ID();
                //var _sql = "SELECT Name,C_Tax_ID FROM C_Tax WHERE IsActive='Y' AND EXPORT_ID IS NOT NULL";
                //Select Taxes which is not Surcharge and having no Parent Tax
                var _sql = "SELECT Name,C_Tax_ID FROM C_Tax WHERE IsActive='Y'AND IsSurcharge='N' AND NVL(Parent_Tax_ID, 0)=0";
                //debugger;
                _sql = VIS.MRole.getDefault().addAccessSQL(_sql, "C_Tax", true, false);
                var _ds = VIS.DB.executeDataSet(_sql.toString(), null, callbackloadTaxRate);
                function callbackloadTaxRate(_ds) {
                    _cmbTaxRate.html("");
                    _cmbTaxRate.append("<option value=0 ></option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.tables[0].rows.length; i++) {
                            _cmbTaxRate.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds.tables[0].rows[i].cells.c_tax_id) + ">" + VIS.Utility.encodeText(_ds.tables[0].rows[i].cells.name) + "</option>");
                        }

                    }
                    _ds.dispose();
                    _ds = null;
                    _sql = null;
                    _cmbTaxRate.prop('selectedIndex', 0);
                }
            },
            loadPayment: function () {
                //remove the child elements before updating the lookup for Payment
                if (_ctrlPayment != undefined && _ctrlPayment != null) {
                    _ctrlPayment.empty();
                }
                //if back account is null then it's take only DocStustus
                var status = _cmbBankAccount.val() != null ? "DocStatus IN ('CO','CL') AND C_BankAccount_ID = " + _cmbBankAccount.val() : "DocStatus IN ('CO','CL')";
                _lookupPayment = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 5043, VIS.DisplayType.Search, "C_Payment_ID", 0, false, status);
                $_ctrlPayment = new VIS.Controls.VTextBoxButton("C_Payment_ID", false, false, true, VIS.DisplayType.Search, _lookupPayment);
                $_ctrlPayment.getControl().addClass("va012-input-size-2");
                $_ctrlPayment.getControl().attr("tabindex", "10");
                _ctrlPayment.append($_ctrlPayment.getControl());
                _ctrlPayment.append($_ctrlPayment.getBtn(0));
                _ctrlPayment.append($_ctrlPayment.getBtn(1));
                $_ctrlPayment.fireValueChanged = function (e) {


                    _paymentSelectedVal = 0;
                    _paymentSelectedVal = $_ctrlPayment.value;
                    //if ($_ctrlPayment.value) {
                    //    loadFunctions.setInvoiceAndBPartner(_paymentSelectedVal, "PY");
                    //}
                    if (!$_ctrlPayment.value) {
                        newRecordForm.refreshForm();
                        // loadFunctions.getOverUnderPayment(_paymentSelectedVal);
                    }
                    //pratap
                    if (!_openingFromDrop && !_openingFromEdit) {
                        if ($_ctrlPayment.value) {
                            // if (!loadFunctions.checkFormPaymentCondition(_paymentSelectedVal, _txtAmount.val())) {
                            if (!loadFunctions.checkPaymentCondition(_paymentSelectedVal, 0, _txtAmount.getValue())) {
                                $_ctrlPayment.setValue();
                            }
                            else {
                                loadFunctions.setInvoiceAndBPartner(_paymentSelectedVal, "PY");
                                //loadFunctions.getOverUnderPayment(_paymentSelectedVal);
                            }
                        }
                    }
                    _openingFromEdit = false;

                };
                _openingFromEdit = false;
            },
            loadOrder: function () {
                var _orderWhere = "C_ORDER_ID IN (SELECT ORD.C_ORDER_ID "
                    + " FROM C_ORDER ORD "
                    + " LEFT JOIN C_DOCTYPE DT "
                    + " ON ORD.C_DOCTYPETARGET_ID=DT.C_DOCTYPE_ID "
                    + " INNER JOIN VA009_PAYMENTMETHOD PM "
                    + " ON (PM.VA009_PAYMENTMETHOD_ID =ORD.VA009_PAYMENTMETHOD_ID ) "
                    + " WHERE DT.DOCSUBTYPESO         ='PR' "
                    + "  AND ORD.DOCSTATUS             ='WP' "
                    + "  AND ORD.ISACTIVE              ='Y' "
                    + "  AND PM.VA009_PAYMENTBASETYPE! ='B')";
                _lookupOrder = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 5043, VIS.DisplayType.Search, "C_Order_ID", 0, false, _orderWhere);
                $_ctrlOrder = new VIS.Controls.VTextBoxButton("C_Order_ID", false, false, true, VIS.DisplayType.Search, _lookupOrder);
                $_ctrlOrder.getControl().addClass("va012-input-size-2");
                $_ctrlOrder.getControl().attr("tabindex", "13");
                _ctrlOrder.append($_ctrlOrder.getControl());
                _ctrlOrder.append($_ctrlOrder.getBtn(0));
                _ctrlOrder.append($_ctrlOrder.getBtn(1));

                $_ctrlOrder.fireValueChanged = function () {
                    _orderSelectedVal = 0;
                    _orderSelectedVal = $_ctrlOrder.value;
                    //if ($_ctrlOrder.value) {
                    //    loadFunctions.setInvoiceAndBPartner(_orderSelectedVal, "PO");
                    //}
                    if ($_ctrlOrder.value) {

                        if (!_openingFromDrop) {
                            // if (!loadFunctions.checkFormPrepayCondition($_ctrlOrder.value, _txtAmount.val())) {
                            if (!loadFunctions.checkPrepayCondition($_ctrlOrder.value, 0, null, _txtAmount.getValue())) {
                                $_ctrlOrder.setValue();
                            }
                            else {
                                loadFunctions.setInvoiceAndBPartner(_orderSelectedVal, "PO");
                            }
                        }
                    }
                };
            },

            loadCashLine: function () {
                //debugger;
                var _cashLineWhere = " C_CASHLINE_ID IN (SELECT CSL.C_CASHLINE_ID "
                    + " FROM C_CASH CS "
                    + " INNER JOIN C_CASHLINE CSL "
                    + " ON CS.C_CASH_ID=CSL.C_CASH_ID "
                    + " INNER JOIN c_charge chrg "
                    + " ON chrg.c_charge_id        =csl.c_charge_id "
                    + " WHERE CS.ISACTIVE          ='Y' "
                    + " AND CSL.CashType           ='C' "
                    + " AND chrg.dtd001_chargetype ='CON' "
                    + " AND CS.DOCSTATUS          IN ('CO','CL') "
                    + " AND CSL.VA012_ISRECONCILED ='N' "
                    + " AND CSL.C_BankAccount_ID ="
                    + ((_cmbBankAccount.val() == null) ? 0 : _cmbBankAccount.val())
                    + " )";
                //debugger;
                _ctrlCashLine.html("");
                _lookupCashLine = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 1008317, VIS.DisplayType.Search, "C_CashLine_ID", 0, false, _cashLineWhere);
                $_ctrlCashLine = new VIS.Controls.VTextBoxButton("C_CashLine_ID", false, false, true, VIS.DisplayType.Search, _lookupCashLine);
                $_ctrlCashLine.getControl().addClass("va012-input-size-2");
                $_ctrlCashLine.getControl().attr("tabindex", "10");
                _ctrlCashLine.append($_ctrlCashLine.getControl());
                _ctrlCashLine.append($_ctrlCashLine.getBtn(0));
                _ctrlCashLine.append($_ctrlCashLine.getBtn(1));

                $_ctrlCashLine.fireValueChanged = function () {
                    _cashLineSelectedVal = 0;
                    _cashLineSelectedVal = $_ctrlCashLine.value;

                    if ($_ctrlCashLine.value) {
                        if (!_openingFromDrop && !_openingFromEdit) {
                            if (!loadFunctions.checkContraCondition($_ctrlCashLine.value, 0, _txtAmount.getValue())) {
                                $_ctrlCashLine.setValue();
                            }
                        }
                        _openingFromEdit = false;
                    }
                    _openingFromEdit = false;
                };
            },


            loadBusinessPartner: function () {
                _lookupBusinessPartner = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 2893, VIS.DisplayType.Search, "C_BPartner_ID", 0, false, null);
                $_ctrlBusinessPartner = new VIS.Controls.VTextBoxButton("C_BPartner_ID", false, false, true, VIS.DisplayType.Search, _lookupBusinessPartner);
                $_ctrlBusinessPartner.getControl().addClass("va012-input-size-2");
                $_ctrlBusinessPartner.getControl().attr("tabindex", "12");
                _ctrlBusinessPartner.append($_ctrlBusinessPartner.getControl());
                _ctrlBusinessPartner.append($_ctrlBusinessPartner.getBtn(0));
                _ctrlBusinessPartner.append($_ctrlBusinessPartner.getBtn(1));
                $_ctrlBusinessPartner.fireValueChanged = function () {
                    _bPartnerSelectedVal = 0;
                    _bPartnerSelectedVal = $_ctrlBusinessPartner.value;
                };
            },
            loadInvoice: function () {
                _lookupInvoice = VIS.MLookupFactory.get(VIS.Env.getCtx(), $self.windowNo, 3484, VIS.DisplayType.Search, "C_Invoice_ID", 0, false, "DocStatus IN ('CO','CL')");
                $_ctrlInvoice = new VIS.Controls.VTextBoxButton("C_Invoice_ID", false, false, true, VIS.DisplayType.Search, _lookupInvoice);
                $_ctrlInvoice.getControl().addClass("va012-input-size-2");
                $_ctrlInvoice.getControl().attr("tabindex", "11");
                _ctrlInvoice.append($_ctrlInvoice.getControl());
                _ctrlInvoice.append($_ctrlInvoice.getBtn(0));
                _ctrlInvoice.append($_ctrlInvoice.getBtn(1));
                $_ctrlInvoice.fireValueChanged = function () {
                    _invoiceSelectedVal = 0;
                    _invoiceSelectedVal = $_ctrlInvoice.value;
                    if ($_ctrlInvoice.value) {
                        if (!loadFunctions.checkInvoiceCondition(_invoiceSelectedVal, _txtAmount.getValue())) {
                            $_ctrlInvoice.setValue();
                        }
                        else {
                            loadFunctions.setBPartner(_invoiceSelectedVal);
                        }
                    }
                };
            },
            getFormData: function () {
                var formData = {};
                var _formData = [];

                formData["_bankStatementLineID"] = $_formNewRecord.attr("data-uid");
                formData["_txtStatementNo"] = _txtStatementNo.val();
                formData["_txtStatementPage"] = _txtStatementPage.val()
                formData["_txtStatementLine"] = _txtStatementLine.val();
                formData["_dtStatementDate"] = _dtStatementDate.val();
                formData["_cmbPaymentMethod"] = _cmbPaymentMethod.val();
                //formData["_cmbCurrency"] = _cmbCurrency.val();
                formData["_cmbCurrency"] = _currencyId;
                formData["_cmbVoucherMatch"] = _cmbVoucherMatch.val();

                formData["_txtDescription"] = VIS.Utility.encodeText(_txtDescription.val());
                formData["_txtVoucherNo"] = VIS.Utility.encodeText(_txtVoucherNo.val());
                //formData["_cmbCharge"] = _cmbCharge.val();
                formData["_cmbCharge"] = _txtCharge.attr('chargeid');
                formData["_cmbTaxRate"] = _cmbTaxRate.val();
                formData["_txtTaxAmount"] = _txtTaxAmount.getValue();
                formData["_ctrlPayment"] = _paymentSelectedVal;
                formData["_ctrlOrder"] = _orderSelectedVal;
                formData["_ctrlCashLine"] = _cashLineSelectedVal;
                formData["_ctrlInvoice"] = _invoiceSelectedVal;
                formData["_ctrlBusinessPartner"] = _bPartnerSelectedVal;
                formData["_chkUseNextTime"] = _chkUseNextTime.is(":checked");
                formData["_cmbBank"] = _cmbBank.val();
                formData["_cmbBankAccount"] = _cmbBankAccount.val();
                formData["_cmbBankAccountClasses"] = _cmbBankAccountClasses.val();
                formData["_cmbTransactionType"] = _cmbTransactionType.val();
                formData["_scheduleList"] = _scheduleList.toString();

                formData["_cmbContraType"] = _cmbContraType.val();
                formData["_cmbCashBook"] = _cmbCashBook.val();

                formData["_cmbTransferType"] = _cmbTransferType.val();
                formData["_txtCheckNo"] = _txtCheckNo.val();

                formData["_cmbDifferenceType"] = _cmbDifferenceType.val();

                if (_cmbDifferenceType.val() == "CH") {
                    formData["_txtAmount"] = _txtTrxAmt.getValue();
                    formData["_txtTrxAmt"] = _txtAmount.getValue();
                }
                else {
                    formData["_txtAmount"] = _txtAmount.getValue();
                    formData["_txtTrxAmt"] = _txtTrxAmt.getValue();
                }

                formData["_txtDifference"] = _txtDifference.getValue();

                _formData.push(formData);
                return _formData;
            },
            insertNewRecord: function (_formData, callback) {
                $.ajax({
                    type: 'POST',
                    url: VIS.Application.contextUrl + "VA012/BankStatement/InsertData",
                    contentType: "application/json; charset=utf-8",
                    data: JSON.stringify({ _formData: _formData }),
                    success: function (data) { callback(data); },
                    error: function (data) { VIS.ADialog.info(data, null, "", ""); }
                });
            },
            afterInsertion: function (data) {
                if (data != null && data != "") {

                    var _result = $.parseJSON(data);
                    if (_result == "Success") {
                        _txtStatementLine.val(parseInt(_txtStatementLine.val()) + 10);
                        busyIndicator($root, false, "absolute");
                        VIS.ADialog.info("VA012_RecordSaved", null, "", "");
                        //if (parseInt($_formNewRecord.attr("data-uid")) > 0) {
                        //    loadFunctions.addEffect($_formNewRecord, _lstStatement.find('div[data-uid="' + parseInt($_formNewRecord.attr("data-uid")) + '"]'));
                        //}
                        //else {
                        //    VIS.ADialog.info(VIS.Msg.getMsg("VA012_RecordSaved"), null, "", "");
                        //}
                        //if (_statementID != null && _statementID != "") {
                        _statementLinesList = [];
                        _lstStatement.html("");
                        _statementPageNo = 1;
                        childDialogs.loadStatement(_statementID);
                        _lstPayments.html("");

                        newRecordForm.scheduleRefresh();
                        newRecordForm.prepayRefresh();
                        _paymentPageNo = 1;
                        storepaymentdata = [];
                        loadFunctions.loadPayments(_cmbBankAccount.val(), _cmbSearchPaymentMethod.val(), _cmbTransactionType.val(), _statementDate.val());
                        //}

                        newRecordForm.scheduleRefresh();
                        newRecordForm.prepayRefresh();
                        newRecordForm.refreshForm();
                    }
                    else {
                        busyIndicator($root, false, "absolute");
                        VIS.ADialog.info(_result, null, "", "");
                    }
                }
            },
            refreshForm: function () {
                $_formNewRecord.attr("data-uid", 0);
                // _btnCreatePayment.hide();
                loadFunctions.getMaxStatement("LO");
                //_txtStatementPage.val("1");
                //_txtStatementLine.val("");
                //get BankStatement Date and set as Statement Date for new Record
                var stateDate = VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetStatementDate", { bankAcct: _cmbBankAccount.val() });
                if (stateDate != "" && stateDate != null) {
                    _dtStatementDate.val(Globalize.format(new Date(stateDate), "yyyy-MM-dd"));
                }
                else {
                    _dtStatementDate.val(_today);
                }
                _cmbPaymentMethod.prop('selectedIndex', 0);
                _cmbVoucherMatch.prop('selectedIndex', 0);
                _cmbVoucherMatch.trigger('change');
                _txtAmount.setValue(0);
                _txtTrxAmt.setValue(0);
                _txtDifference.setValue(0);
                _txtDifference.getControl().attr("vchangable", "Y");
                _cmbDifferenceType.prop('selectedIndex', 0);
                _txtDescription.val("");
                _txtVoucherNo.val("");
                _cmbCharge.prop('selectedIndex', 0);
                _txtCharge.attr('chargeid', 0);
                _txtCharge.val("");
                _cmbTaxRate.prop('selectedIndex', 0);
                _txtTaxAmount.setValue(0);
                _chkUseNextTime.attr('checked', false);
                $_ctrlPayment.setValue();
                $_ctrlOrder.setValue();
                $_ctrlCashLine.setValue();
                $_ctrlBusinessPartner.setValue();
                $_ctrlInvoice.setValue();
                _bPartnerSelectedVal = 0;
                _paymentSelectedVal = 0;
                _orderSelectedVal = 0;
                _invoiceSelectedVal = 0;
                //_btnIn.attr("v_active", "1");
                _btnIn.trigger('click');
                //_btnOut.attr("v_active", "0");
                //loadFunctions.getBaseCurrency();
                newRecordForm.loadCurrency();
                //fixed issue, amount is getting blur while Open the form or press on Plus(+) Button
                //_txtAmount.getControl().trigger("focus");
                //_txtAmount.getControl().select();

                _cmbContraType.prop('selectedIndex', 0);
                _cmbCashBook.prop('selectedIndex', 0);
                _cmbTransferType.prop('selectedIndex', 0);
                _txtCheckNo.val("");
            },
            scheduleRefresh: function () {
                _scheduleList = [];
                _scheduleDataList = [];
                /*change by pratap*/
                _scheduleAmount = [];
                /*change by pratap*/
                _txtPaymentSchedule.val("");
            },
            prepayRefresh: function () {
                _prepayList = [];
                _prepayDataList = [];
                _txtPrepayOrder.val("");
            },
            newRecordDispose: function () {
                $_formNewRecord = null;
                $_formBtnNewRecord = null;
                _txtStatementNo = null;
                _btnStatementNo = null;
                _txtStatementPage = null;
                _txtStatementLine = null;
                _dtStatementDate = null;
                _cmbPaymentMethod = null;
                // _cmbCurrency = null;
                _cmbContraType = null;
                _cmbCashBook = null;

                _cmbTransferType = null;
                _txtCheckNo = null;
                _currencyId = null;
                _cmbVoucherMatch = null;
                _txtAmount = null;
                //_txtTrxAmt = null;
                _txtDifference = null;
                _cmbDifferenceType = null;
                _btnAmount = null;
                _btnIn = null;
                _btnOut = null;
                _txtDescription = null;
                _txtVoucherNo = null;
                _cmbCharge = null;
                _txtCharge = null;
                _cmbTaxRate = null;
                _txtTaxAmount = null;
                _ctrlCashLine = null;
                _ctrlOrder = null;
                _ctrlPayment = null;
                _ctrlInvoice = null;
                _ctrlBusinessPartner = null;
                _chkUseNextTime = null;
                _btnSave = null;
                _btnPaymentSchedule = null;
                _btnPrepay = null;
                _txtPaymentSchedule = null;
                _txtPrepayOrder = null;
                // _btnCreatePayment = null;
                _btnNewRecord = null;
                _btnUndo = null;
                _btnDelete = null;
                _btnMore = null;
                _divMore = null;
                _lookupPayment = null;
                $_ctrlOrder = null;
                _orderSelectedVal = null;
                $_ctrlPayment = null;
                _draggedPaymentID = null;
                _paymentSelectedVal = null;
                $_ctrlCashLine = null;
                _lookupCashLine = null;
                _cashLineSelectedVal = null;
                _lookupBusinessPartner = null;
                $_ctrlBusinessPartner = null;
                _bPartnerSelectedVal = null;
                _lookupInvoice = null;
                $_ctrlInvoice = null;
                _invoiceSelectedVal = null;
                _today = null;
                now = null;
            },
        };
        //End New Record Form

        function isInList(value, array) {
            return array.indexOf(value) > -1;
        }

        $(window).resize(function () {

            loadFunctions.setPaymentListHeight();
            childDialogs.setStatementListHeight();
            //_table.height($(".va012-main-container").height());
            _table.height($("#VA012_mainContainer_" + $self.windowNo).height());
        });

        this.getRoot = function () {
            return $root;
        };
        this.setSize = function () {
            //_table.height($(".va012-main-container").height());
            _table.height($("#VA012_mainContainer_" + $self.windowNo).height());

        };
        this.disposeComponents = function () {
            $self = null;
            $root = null;
            _cmbBank = null;
            _cmbBankAccount = null;
            _cmbBankAccountClasses = null;
            _statementDate = null;
            _VA012_BankChargeDiv = null;
            _cmbSearchPaymentMethod = null;
            _cmbTransactionType = null;
            _btnLoadStatement = null;
            _btnMatchStatement = null;
            _lstStatement = null;
            _lstPayments = null;
            _statementID = "";
            _secReconciled = null;
            _secUnreconciled = null;
            _divVoucher = null;
            _divMatch = null;

            _divContraType = null;
            _divCashBook = null;
            _divCtrlCashLine = null;
            _divTransferType = null;
            _divCheckNo = null;

            _divVoucherNo = null;
            _divTrxAmt = null;
            divRow4Col1TrxAmt = null;
            _divDifference = null;
            _divDifferenceType = null;
            _divCharge = null;
            _btnCharge = null;
            _divTaxRate = null;
            _divTaxAmount = null;
            _divCtrlPayment = null;
            _divCtrlInvoice = null;
            _divCtrlBusinessPartner = null;

            _divPrepayOrder = null;
            _divPaymentSchedule = null;
            _txtSearch = null;
            _btnSearch = null;
            _btnUnmatch = null;
            _btnProcess = null;
            _btnHide = null;
            _tdLeft = null;
            _table = null;
            _clientBaseCurrency = null;
            _clientBaseCurrencyID = null;
            _statementLinesList = [];
            _scheduleList = [];
            _scheduleDataList = [];
            _prepayList = [];
            _prepayDataList = [];
            newRecordForm.newRecordDispose();
        };
        function busyIndicator(_obj, _isShow, _position) {
            $BusyIndicator = $("<div class='vis-apanel-busy va012-busy-bank-statement'>");
            $BusyIndicator.css({
                "position": _position, "width": "95%", "height": "95%", 'text-align': 'center', "visibility": "hidden"
            });
            if (_isShow) {
                $BusyIndicator.css({
                    "visibility": "visible"
                });
            }
            else {
                _obj.find(".va012-busy-bank-statement").remove();
                return;
            }
            _obj.append($BusyIndicator);
        };


    };

    //bankStatement.prototype.sizeChanged = function (height) {
    //    this.setSize(height);
    //};
    bankStatement.prototype.dispose = function () {
        this.disposeComponents();
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
    };
    bankStatement.prototype.init = function (windowNo, frame) {

        this.windowNo = windowNo;
        this.frame = frame;
        this.Initialize();
        this.frame.getContentGrid().append(this.getRoot());
        this.setSize();
    };
    VA012.AForms = VA012.AForms || {};
    VA012.AForms.bankStatement = bankStatement;
})(VA012, jQuery);