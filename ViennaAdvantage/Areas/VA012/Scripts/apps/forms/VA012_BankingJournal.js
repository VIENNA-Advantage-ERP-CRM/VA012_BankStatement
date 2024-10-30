﻿; VA012 = window.VA012 || {};

; (function (VA012, $) {

    //VA012.VA012_BankingJournal = VA012 || {};

    // Form class function fullnamespace

    // Document Uploader
    VA012.VA012_BankingJournal = function () {
        /* Global variable declaration */
        this.frame = null;
        this.windowNo = 0;
        this.widgetInfo = null;
        var $self = this;
        var $root = $("<div id='WidMainRoot_" + widgetID + "' class='VA012_root'></div>");
        var widgetID = null;
        var dropContainer = null;
        var uploadFile = null;
        var uploadImg = null;
        var cancelBtn = null;
        var uploadBtn = null;
        var nxtBtn = null;
        var dragDiv = null;
        var paramDiv = null;
        var paramFooter = null;
        var fileNameLabel = null;
        var openDMSBtn = null;
        var $bsyDiv = null;
        var _cmbBank = null;
        var _cmbBankAccount = null;
        var _cmbBankAccountClasses = null;
        var C_BANK_ID = 0;
        var C_BANKACCOUNT_ID = 0;
        var _statementDate = null;
        var _statementName = null;
        var isChecked = null;
        var $loadParaDiv = null;
        var _currencyId = null;
        var _selectedFiles = null;
        var _result = null;
        var Bank_Charge_ID = null;
        var folTreeArea = null;
        var folTreeUpload = null;
        var folTreeSelect = null;
        var folTreeCancel = null;
        var folderFader = null;
        var strFolderIds = "";
        var isDMS = false;
        VA012.Common =
        {
            IPadWidth: 1024,
            MimWidth: 1000,
            None_Access: 10,
            R_Access: 20,
            RW_Access: 30,
            RWD_Access: 40,
            Full_Access: 99,
            //Document Variables
            XLS: '.xls',
            XLSX: '.xlsx',
            CSV: '.csv',
            // ASC DESC
            UpdatedAsc: 'UA',
            UpdatedDesc: 'UD'
        };
        // init log class
        this.log = VIS.Logging.VLogger.getVLogger('BankJournalWidget');
        //Privilized function
        this.getRoot = function () {
            // $root = $("<div id='WidMainRoot_" + widgetID + "' class='VA012_root'></div>");
            $root.append(dropContainer);
            dragDiv = $root.find('.VA012-widgetContentArea');
            createBusyIndicator();
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
            if (widgetID == 0) {
                widgetID = $widgetID;
            }
            Design();
        };
        //Create design
        function Design() {
            dropContainer = $('<div class="VA012-bank-panel VA012-WidgetContainer">' +
                '<div class="VA012-panel-heading"><h6 class="VA012-headerPanelLbl">' + VIS.Msg.getMsg('VA012_BankingJournal') + '</h6></div>' +'<div class="VA012-fader-div VA012-folderFader d-none" id="VA012-folderFader_' + widgetID + '">' +
                '<div class="p-2 VA012-folderContainer VA012-folderContainer_' + widgetID + '">' +
                '<h6>' + VIS.Msg.getMsg('VA012_SelectFolder') + '</h6>' +
                '<div class="VA012-folTreeArea" id="VA012-folTreeArea_' + widgetID + '">' +
                '</div>' +
                '<div class="VA012-folTreeFooter">' +
                '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Back') + '" class="btn ui-button ui-corner-all ui-widget VA012-treeCancelBtn" id="VA012-treeCancelBtn_' + widgetID + '">' +
                '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Next') + '" disabled class="btn ui-button ui-corner-all ui-widget VA012-treeUploadBtn" id="VA012-treeUploadBtn_' + widgetID + '">' +                
                '</div > ' +
                '</div>' +
                '</div>' +
                '<div class="VA012-widgetContentArea">' +
                '<div class="VA012-shadow bg-white">' +
                '<div class="VA012-upload-col">' +
                '<input id="VA012-uploadFile_' + widgetID + '" class="VA012-input-file-field VA012-uploadFileWidget_' + widgetID
                + '" type="file" accept=".csv, .xls, .xlsx">' +
                '<label for="VA012-uploadFile_' + widgetID + '" class="VA012-files-label">' +
                '<i class="fa fa-cloud-upload VA012-uploadImg" id="VA012-uploadImg_' + widgetID + '" style="" aria-hidden="true"></i>' +
                '<div class="VA012-labelTxt">' + VIS.Msg.getMsg('VA012_DragFiles') + '<span class="VA012-browse-link">'
                + VIS.Msg.getMsg('VA012_Browse') + '</span>' +
                '<span class="VA012-FileNamelbl VA012-fileSize" id="VA012-FileNamelbl_' + widgetID + '"></span></div>' +
                '</label>' +
                '</div>' +
                '</div>' +
                '<div class="VA012-links">' +
                '<div class="VA012-DMS-folder">' +
                '<div class="VA012-folder-link text-center">' +
                '<i id="VA012-OpenDMSIcon_' + widgetID + '" class="fa fa-folder-open VA012-OpenDMS" aria-hidden="true"></i>' +
                '<a class="VA012-OpenDMS_' + widgetID + '" href="javascript:void(0)">' + VIS.Msg.getMsg('VA012_OpenDMS') + '</a>' +
                '</div>' +
                '<div class="VA012-folder-link text-center">' +
                '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Next') +
                '" id="VA012_NextBtn_' + widgetID + '" class="btn ui-button ui-corner-all ui-widget VA012-NextBtn">' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>');
            uploadFile = dropContainer.find('#VA012-uploadFile_' + widgetID);
            uploadImg = dropContainer.find('#VA012-uploadImg_' + widgetID);
            nxtBtn = dropContainer.find('#VA012_NextBtn_' + widgetID);
            selectedFileName = dropContainer.find('#VA012-selectedFileName_' + widgetID);
            fileNameLabel = dropContainer.find('#VA012-FileNamelbl_' + widgetID);
            openDMSBtn = dropContainer.find('#VA012-OpenDMSIcon_' + widgetID);
            folTreeArea = dropContainer.find('#VA012-folTreeArea_' + widgetID);
            folderFader = dropContainer.find('#VA012-folderFader_' + widgetID);
            folTreeUpload = dropContainer.find('#VA012-treeUploadBtn_' + widgetID);
            folTreeSelect = dropContainer.find('#VA012-treeSelectBtn_' + widgetID);
            folTreeCancel = dropContainer.find('#VA012-treeCancelBtn_' + widgetID);
            if (_selectedFiles == null) {
                nxtBtn.attr("disabled", true);
            }
            else {
                nxtBtn.attr("disabled", false);
                nxtBtn.css("opacity", 1);
            }
            var modulePrefix = VIS.dataContext.getJSONRecord("ModulePrefix/GetModulePrefix", "VA012_");
            if (modulePrefix == null) {
                openDMSBtn.css("display", "none");
            }
            else {
                openDMSBtn.css("display", "block");
            }
            Events();
        };
        function Events() {
            // Preventing page from redirecting
            dropContainer.on("dragover", function (e) {
                e.preventDefault();
                e.stopPropagation();
            });
            // Drop
            dropContainer.on('drop', function (e) {
                e.stopPropagation();
                e.preventDefault();
                isDMS = false;
                files = e.originalEvent.dataTransfer.files;
                var ctrl = $(dropContainer.find('.VA012-uploadFileWidget_' + widgetID)[0]);
                const originalFile = files[0]; // Assuming files[0] is the original Excel file
                // Create a FileReader to read the file content
                const reader = new FileReader();
                // Once the file is loaded, create a new File object with the same content
                reader.onload = function (event) {
                    // `event.target.result` contains the file content as an ArrayBuffer
                    const arrayBuffer = event.target.result;
                    // Create a Blob from the ArrayBuffer
                    const blob = new Blob([arrayBuffer], { type: originalFile.type });
                    // The file content (in binary form) is in event.target.result
                    const fileContent = event.target.result;
                    // Create a new File with the original content
                    const myFile = new File([blob], files[0].name,
                        { type: files[0].type, lastModified: files[0].lastModified });
                    const dataTransfer = new DataTransfer();
                    dataTransfer.items.add(myFile);
                    ctrl[0].files = dataTransfer.files;
                    _selectedFiles = files;
                    if (files.length > 0) {
                        UploadingFiles(files, ctrl[0]);
                    }
                };
                // Read the original file as an ArrayBuffer
                reader.readAsArrayBuffer(originalFile);
            });
            dropContainer.find('.VA012-uploadFileWidget_' + widgetID).on('click', function (e) {
                this.value = null;
            });
            dropContainer.find('.VA012-uploadFileWidget_' + widgetID).on('change', function (e) {
                e.stopPropagation();
                e.preventDefault();
                isDMS = false;
                var files = e.target.files;
                _selectedFiles = files;
                if (files.length > 0) {
                    UploadingFiles(files, this);
                }
            });
            nxtBtn.on('click', function (e) {
                if (paramDiv == null) {
                    //load parameter div
                    loadParam();
                }
                else {
                    //show parameter div
                    paramDiv.show();
                    paramFooter.show();
                }
                //  hide drag files div
                dragDiv.hide();
            });
            openDMSBtn.on('click', function (e) {
                // Loading effect
                // loadingFader.find('p').html('').append(VIS.Msg.getMsg('VA012_PleaseWait') + '<span>' + VIS.Msg.getMsg('VA012_LoadingFolders') + '</span>');
                $bsyDiv.show();
                // loadingFader.removeClass('d-none');
                // Show folder tree
                folderFader.removeClass('d-none');
                var folStruct = WidgetFolderTreeStruct();
                //loadingFader.addClass('d-none');
                $bsyDiv.hide();

            });
            // Cancel Button Folder
            folTreeCancel.on('click', function (e) {
                folderFader.addClass('d-none');
                strFolderIds = "";
                dragDiv.show();
                $bsyDiv.hide();
            });

        }
        /**
         * Uploading files to the DMS
         * @param {any} files
         */
        function UploadingFiles(files, obj) {
            var allowedExtensions = ".xlsx,.xls,.csv";
            var extNotAllowed = false;
            var invalidExtensions = [];
            var isExceded = null;
            var excededfileNames = null;
            // Looping over all files and add it to FormData object
            for (var i = 0; i < files.length; i++) {

                if (files[i].size > 209715200) {
                    isExceded = true;
                    excededfileNames += files[i].name + ',';
                }
                var filename = files[i].name.substr(0, files[i].name.lastIndexOf('.'));
                var fileExt = files[i].name.substr(files[i].name.lastIndexOf('.')).toLowerCase();
                if (allowedExtensions != null && allowedExtensions.length > 0) {
                    if (!Array.isArray(allowedExtensions)) {
                        allowedExtensions = allowedExtensions.split(',');
                    }
                    $.each(allowedExtensions, function (index, value) {
                        allowedExtensions[index] = value.toLowerCase().replace(/\s+/g, '');
                    });
                    if (allowedExtensions.indexOf(fileExt) == -1) {
                        extNotAllowed = true;
                        invalidExtensions.push(files[i].name);
                    }
                }
                // Check extention
                if (extNotAllowed && invalidExtensions.length > 0) {
                    var fileNames = '';
                    for (var item in invalidExtensions) {
                        fileNames += invalidExtensions[item].toString() + ', ';
                    }
                    var resString = '';
                    if (invalidExtensions.length == 1) {
                        resString += 'FileInvalidExtension';
                    }
                    else {
                        resString += 'FilesInvalidExtension';
                    }
                    return;
                }
                if (files.length > 0) {
                    fileNameLabel.text(filename + "" + fileExt);
                    fileNameLabel.val(filename + "" + fileExt);
                    var file = obj;
                    _result = $.parseJSON(VA012.UploadExcel(file, null, null));
                    if (paramDiv == null) {
                        //Display parameter div
                        loadParam();
                    }
                    else {
                        //If param div already in DOM then reset the controls values
                        // resetControls();
                        paramDiv.show();
                        paramFooter.show();
                    }
                    dragDiv.hide();
                }
            }
        }
        function loadParam() {
            $loadParaDiv = $('<div class="VA012_paramMainDiv" id="VA012_paramMainDiv_' + widgetID + '"><div class= "VA012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<select class="VA012-select" id="VA012_STAT_cmbBank_' + widgetID + '">'
                + '</select>'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_Bank") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class= "VA012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<select class="VA012-select" id="VA012_STAT_cmbBankAccount_' + widgetID + '">'
                + '</select>'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_BankAccount") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div > '
                + '<div class=VA012-form-data>' + '<div class="input-group vis-input-wrap VA012-paramdiv VA012-margin-B0">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<input class="VA012-select" type="date" max="9999-12-31" id=VA012_STAT_statementDate_' + widgetID + '>'
                + '<label class="VA012-labels" id="VA012_STAT_lblStatementDate_' + widgetID + '">' + VIS.Msg.getMsg("VA012_StatementDate") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="VA012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<select class="VA012-select" id="VA012_STAT_cmbBankAccountClassName_' + widgetID + '">'
                + '</select>'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_ClassName") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="VA012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<input class="VA012-select" type="text" id="VA012_STAT_txtStatementNo_' + widgetID + '" placeholder=" " data-placeholder="">'
                + '<label class="VA012-nameLbl">' + VIS.Msg.getMsg("VA012_StatementNumber") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="VA012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<input type="checkbox" value="checked" id="VA012_CheckBox_' + widgetID + '">'
                + '<label class="VA012-StatementDateAsAccountDate">' + VIS.Msg.getMsg("VA012_StatementDateAsAccountDate") + '</label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="VA012-paramFooterDiv" id="VA012-paramFooterDiv_' + widgetID + '">'
                + '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Back') + '" class="btn ui-button ui-corner-all VA012-CancelBtn">'
                + '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Upload') + '" class="btn ui-button ui-corner-all VA012-UploadBtn">'
                + '</div>'
                + '</div>');
            dropContainer.append($loadParaDiv);
            getControls();
            loadFunctions.loadBank();
            loadFunctions.loadBankAccountCharges();
            _cmbBank.on('click', function (e) {
                if (_cmbBank.val() != "null") {
                    loadFunctions.loadBankAccount();
                }
                else {
                    return "VA012_NoBankSelected";
                }
            });
            _cmbBankAccount.on('click', function (e) {
                if (_cmbBankAccount.val() != "null") {
                    loadFunctions.loadCurrency();
                    loadBankAccountClasses();

                }
                else {
                    return "VA012_NoBankAccountSelected";
                }
            });
            //Cancel button represnts back button
            cancelBtn.on('click', function (e) {
                paramDiv.hide();
                paramFooter.hide();
                nxtBtn.attr("disabled", false);
                nxtBtn.css("opacity", 1);
                strFolderIds = "";
                if (!isDMS) {
                    dragDiv.show();
                }
                else {
                    folderFader.removeClass('d-none');
                }
            });
            uploadBtn.on('click', function (e) {
                $bsyDiv.show();
                //Load Bank Statement From File Selected
                if (_cmbBank.val() == null || _cmbBank.val() == "" || _cmbBank.val() == "0") {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_SelectBankFirst", null, "", "");
                    return false;
                }
                if (_cmbBankAccount.val() == null || _cmbBankAccount.val() == "" || _cmbBankAccount.val() == "0") {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                    return false;
                }

                if (_cmbBankAccountClasses.val() == null || _cmbBankAccountClasses.val() == "" || _cmbBankAccountClasses.val() == "0") {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_PleaseSelectClassFirst", null, "", "");
                    return false;
                }
                if (_statementName.val() == null || _statementName.val() == "") {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_PleaseEnterStatementNo", null, "", "");
                    return false;
                }
                if (_statementDate.val() == null || _statementDate.val() == "") {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_PleaseEnterStatementDate", null, "", "");
                    return false;
                }
                //Bank Statement From File Path Scheduled
                if (_result != null) {
                    if (_result._filename == null || _result._filename == "" || _result._path == null || _result._path == "") {
                        $bsyDiv.hide();
                        VIS.ADialog.info("VA012_ErrorInGettingFile", null, "", "");
                        return;
                    }
                    else if (_result._error != null && _result._error != "") {
                        $bsyDiv.hide();
                        VIS.ADialog.info(_result._error, null, "", "");
                        return;
                    }
                    else {
                        var _path = _result._path;
                        var _filename = _result._filename;
                        var _bankaccount = _cmbBankAccount.val();
                        var _statementno = _statementName.val();
                        var _IsStatementDateAsAccountDate = isChecked.is(':checked');
                        var _statementClassName = _cmbBankAccountClasses.val();
                        var _statementCharges = Bank_Charge_ID;
                        $.ajax({
                            url: VIS.Application.contextUrl + "BankStatement/ImportStatement",
                            type: "GET",
                            datatype: "json",
                            contentType: "application/json; charset=utf-8",
                            async: true,
                            data: ({
                                _path: _path, _filename: _filename, _bankaccount: _bankaccount, _bankAccountCurrency: _currencyId, _statementno: _statementno,
                                _statementClassName: _statementClassName, _statementCharges: _statementCharges, statementDate: _statementDate.val(),
                                IsStatementDateAsAccountDate: _IsStatementDateAsAccountDate
                            }),
                            success: function (result) {
                                _statementID = result._statementID;
                                if (_statementID != null && _statementID != "") {
                                    $bsyDiv.hide();
                                    resetControls();
                                    paramDiv.hide();
                                    paramFooter.hide();
                                    dragDiv.show();
                                    dropContainer.find('.VA012-uploadFileWidget_' + widgetID).val(null);
                                    fileNameLabel.text('');
                                    nxtBtn.attr("disabled", true);
                                    isDMS = false;
                                    strFolderIds = "";
                                    VIS.ADialog.info("VA012_StatementUploadDone", null, "", "");
                                    return true;
                                }
                                else {
                                    if (result._error != null && result._error != "") {
                                        $bsyDiv.hide();
                                        VIS.ADialog.info(result._error, null, "", "");
                                        return false;
                                    }
                                }
                            },
                            error: function () {
                                $bsyDiv.hide();
                                return VIS.ADialog.info("error", null, "", "");
                                VIS.ADialog.info("VA012_ErrorWhileUploadExcel", null, "", "");
                                return false;
                            }
                        })
                    }
                }
                else {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_ErrorInGettingFile", null, "", "");
                    return;
                }
            });
        };
        //reset parameters and show and hide drag and parameter div
        function resetControls() {
            _cmbBank.prop('selectedIndex', 0);
            _cmbBankAccount.prop('selectedIndex', 0);
            _cmbBankAccountClasses.prop('selectedIndex', 0);
            _statementDate.val('');
            _statementName.val('');
            isChecked.prop("checked", false);
        };

        /**Get Bank,Bank Account detail like Currency,precision based on bank selected */
        var loadFunctions = {
            loadBank: function () {
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
                    error: function (error) {
                        console.log(error);
                    }
                });
                function callbackloadBank(_ds) {
                    _cmbBank.html("");
                    _cmbBank.append("<option value=0 ></option>");
                    if (_ds != null) {
                        for (var i = 0; i < _ds.length; i++) {
                            _cmbBank.append("<option value=" + VIS.Utility.Util.getValueOfInt(_ds[i].Value) + ">" + _ds[i].Name + "</option>");
                        }
                    }
                    _cmbBank.prop('selectedIndex', 0);
                    if (C_BANK_ID > 0) {
                        _cmbBank.val(C_BANK_ID).prop('selected', true);
                        C_BANK_ID = 0;
                    }
                    loadFunctions.loadBankAccount();
                }
            },
            loadBankAccount: function () {
                $.ajax({
                    url: VIS.Application.contextUrl + "BankStatement/GetBankAccount",
                    type: "GET",
                    datatype: "json",
                    contentType: "application/json; charset=utf-8",
                    data: ({ bankId: _cmbBank.val() }),
                    success: function (data) {
                        if (data != null && data != "") {
                            data = $.parseJSON(data);
                            callbackloadBankAccount(data);
                        }
                    },
                    error: function (error) {
                        console.log(error);
                    }
                });
                function callbackloadBankAccount(data) {
                    _cmbBankAccount.html("");
                    _cmbBankAccount.append("<option value=0 ></option>");
                    if (data != null) {
                        for (var i = 0; i < data.length; i++) {
                            _cmbBankAccount.append("<option orgid=" + VIS.Utility.Util.getValueOfInt(data[i].OrgId) + " stdprecision=" + VIS.Utility.Util.getValueOfInt(data[i].StdPrecision) + " currencyid=" + VIS.Utility.Util.getValueOfInt(data[i].CurrencyId) + " value=" + VIS.Utility.Util.getValueOfInt(data[i].BankAccountId) + " accounttype=" + VIS.Utility.Util.getValueOfString(data[i].AccountType) + ">" + VIS.Utility.encodeText(data[i].AccountNo) + "</option>");
                        }
                    }
                    _cmbBankAccount.prop('selectedIndex', 0);
                    if (C_BANKACCOUNT_ID > 0) {
                        _cmbBankAccount.val(C_BANKACCOUNT_ID).prop('selected', true);
                        C_BANKACCOUNT_ID = 0;
                    }
                }
            },
            loadCurrency: function () {
                //Set selected bankaccount currencyid and stdprecision
                var currencyid = VIS.Utility.Util.getValueOfInt($('option:selected', _cmbBankAccount).attr('currencyid'));
                var stdprecision = VIS.Utility.Util.getValueOfInt($('option:selected', _cmbBankAccount).attr('stdprecision'));

                if (currencyid > 0) {
                    _currencyId = currencyid;
                } else {
                    currencyid = null;
                }
                if (stdprecision > 0) {
                    _stdPrecision = stdprecision;
                } else {
                    _stdPrecision = null;
                }
            },
            loadBankAccountCharges: function () {
                VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetBankCharge", null, callbackloadBankAccountCharges);
                function callbackloadBankAccountCharges(_ds) {
                    if (_ds != null) {
                        Bank_Charge_ID = VIS.Utility.Util.getValueOfInt(_ds[0].chargeID);
                    }
                }
            }
        };
        function getControls() {
            _cmbBank = $loadParaDiv.find("#VA012_STAT_cmbBank_" + widgetID);
            _cmbBankAccount = $loadParaDiv.find("#VA012_STAT_cmbBankAccount_" + widgetID);
            _cmbBankAccountClasses = $loadParaDiv.find("#VA012_STAT_cmbBankAccountClassName_" + widgetID);
            paramDiv = dropContainer.find('#VA012_paramMainDiv_' + widgetID);
            cancelBtn = $loadParaDiv.find('.VA012-CancelBtn');
            uploadBtn = $loadParaDiv.find('.VA012-UploadBtn');
            paramFooter = dropContainer.find('#VA012-paramFooterDiv_' + widgetID);
            _statementDate = $loadParaDiv.find('#VA012_STAT_statementDate_' + widgetID);
            _statementName = $loadParaDiv.find('#VA012_STAT_txtStatementNo_' + widgetID);
            isChecked = $loadParaDiv.find('#VA012_CheckBox_' + widgetID);
        };
        function loadBankAccountClasses() {
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "BankStatement/GetBankAccountClasses",
                { _cmbBankAccount: _cmbBankAccount.val() }, callbackloadBankAccountClasses);
            function callbackloadBankAccountClasses(_ds) {
                _cmbBankAccountClasses.html("");
                _cmbBankAccountClasses.append("<option value=0 ></option>");
                if (_ds != null) {
                    for (var i = 0; i < _ds.length; i++) {
                        _cmbBankAccountClasses.append("<option value=" + _ds[i].Value + ">" + VIS.Utility.encodeText(_ds[i].Name) + "</option>");
                    }
                }
                _cmbBankAccountClasses.prop('selectedIndex', 0);
            }
        };
        // Creating folder tree ul
        function WidgetFolderTreeStruct() {
            var folderArrayRes = null;
            $.ajax({
                url: VIS.Application.contextUrl + "VA012_BankJournalWidget/GetFolders",
                type: "GET",
                async: false,
                dataType: "json",
                contentType: "application/json; charset=utf-8",
                data: ({
                    UserID: VIS.context.getAD_User_ID(),
                    roleID: VIS.context.getAD_Role_ID(),
                    clientID: VIS.context.getAD_Client_ID(),
                    orgID: VIS.context.getAD_Org_ID(),
                    orderBy: 'NA'
                }),
                success: function (data) {
                    if (data != null && data != "") {
                        folderArrayRes = JSON.parse(data);
                        if (folderArrayRes.length > 0) {
                            var $folderUl = $('<ul id="folderUL_' + widgetID + '" class="list-unstyled w-100"></ul>');
                            strFolderIds = "";
                            var arrayFolder = [];
                            var folderImage = '';
                            var folderName = "";
                            var folderPath = "";
                            var initialCharacter = "";
                            var folderIds = "";
                            var strFolderIds = "";
                            for (var i = 0; i < folderArrayRes.length; i++) {
                                arrayFolder = [];
                                folderPath = "";
                                folderName = folderArrayRes[i].FolderName;
                                if (folderArrayRes[i].FolderName.length > 25) {
                                    folderName = folderArrayRes[i].FolderName.substring(0, 25) + "...";
                                }
                                if (strFolderIds.contains(folderArrayRes[i].FolderID)) {
                                    continue;
                                }
                                var $folderLI = null;
                                if (folderArrayRes[i].ParentFolderID == 0) {

                                    if (folderArrayRes[i].FolderType == 1 || folderArrayRes[i].FolderType == 2 || folderArrayRes[i].FolderType == 5 || folderArrayRes[i].FolderType == 6 || folderArrayRes[i].FolderType == 7 || folderArrayRes[i].FolderType == 8) {
                                    }
                                    else {
                                        if (folderArrayRes[i].PublicAccess > 10) {
                                            folderImage = "<i class='vis vis-folder-globe'></i>";
                                        }
                                        else if (folderArrayRes[i].Sharecount > 2 || folderArrayRes[i].RoleSharecount > 0) {
                                            folderImage = "<i class='vis vis-folder-share'></i>";
                                        }
                                        else {
                                            folderImage = "<i class='fa fa-folder-o'></i>";
                                        }

                                        $folderLI = $(
                                            '<li id="' + folderArrayRes[i].FolderID + '" folderName="' + VIS.Utility.encodeText(folderArrayRes[i].FolderName) + '" parentID="' + folderArrayRes[i].ParentFolderID + '" useraccess="' + folderArrayRes[i].UserAccessOnFolder + '" roleaccess="' + folderArrayRes[i].RoleAccessOnFolder + '" createdBy="' + folderArrayRes[i].CreateUser + '" inputType="' + folderArrayRes[i].InputType + '" folType="' + folderArrayRes[i].FolderType + '" IsSubscribe="' + folderArrayRes[i].IsSubscribedFolder + '" InitialCharacter="' + initialCharacter.toUpperCase() + '">' +
                                            '<div class="VA012-leftTreeNode" title="' + folderArrayRes[i].FolderName + '">' +
                                            '<a id="a_' + folderArrayRes[i].FolderID + '" href="javascript:void(0);" val="' + VIS.Utility.encodeText(folderArrayRes[i].FolderPath) + '">' +
                                            folderImage +
                                            '<span>' + VIS.Utility.encodeText(folderName) + '</span>' +
                                            '</a>' +
                                            '</div>' +
                                            '</li>');
                                    }

                                    arrayFolder.push({ row: folderArrayRes[i], root: $folderLI });
                                    strFolderIds += folderArrayRes[i].FolderID + ",";
                                    folderPath = folderArrayRes[i].FolderName;
                                    WidgetRecursiveFolder(arrayFolder, folderArrayRes, 20, folderPath);
                                    $folderUl.append($folderLI);
                                }
                            }
                            $folderUl.on('click', 'li a', function (e) {
                                $bsyDiv.show();
                                e.stopPropagation();
                                var folRow = $(this).closest('li');
                                var currentTarget = $(e.currentTarget);
                                $.ajax({
                                    url: VIS.Application.contextUrl + "VA012_BankJournalWidget/CheckFolderAccess",
                                    type: "GET",
                                    async: false,
                                    dataType: "json",
                                    contentType: "application/json; charset=utf-8",
                                    data: ({
                                        folderID: folRow.attr('id')
                                    }),
                                    success: function (data) {
                                        if (data != null && data != "") {
                                            if (data == VA012.Common.Full_Access || data == VA012.Common.RWD_Access || data == VA012.Common.RW_Access) {
                                                $('#folderUL_' + widgetID).find(".VA012-selected").removeClass('VA012-selected');
                                                $('#folderUL_' + widgetID).find(".VA012-selectedTreeNode").css('background-color', 'inherit');
                                                folRow.find("div").eq(0).find('a span').addClass('VA012-selected');
                                                if (!folRow.find("div").eq(0).hasClass('VA012-selectedTreeNode')) {
                                                    folRow.find("div").eq(0).addClass('VA012-selectedTreeNode').css('background-color', 'rgba(var(--v-c-primary), .1)');
                                                }
                                                else if (folRow.find("div").eq(0).hasClass('VA012-selectedTreeNode')) {
                                                    folRow.find("div").eq(0).addClass('VA012-selectedTreeNode').css('background-color', 'rgba(var(--v-c-primary), .1)');
                                                }
                                                else if (folRow.children("ul").length > 0) {
                                                    folRow.find("div").eq(0).removeClass("VA012-selectedTreeNode");
                                                }
                                                //folTreeUpload.removeAttr('disabled');
                                                //folTreeSelect.removeAttr('disabled');
                                                var orderByColumn = [];
                                                orderByColumn.push('Updated');
                                                _scrollpage = 1;
                                                _searchDocName = '';
                                                $bsyDiv.hide();
                                                var result = LoadLinkedDocuments(orderByColumn, VA012.Common.UpdatedDesc, _scrollpage, false,
                                                    folRow.attr('id'),
                                                    folTreeArea.find('#' + folRow.attr('id'))
                                                );
                                                $bsyDiv.hide();
                                            }
                                            else {
                                                $bsyDiv.hide();
                                                VIS.ADialog.info('VA012_ReqWPermOnFol');
                                            }
                                        }
                                    },
                                    error: function (errorThrown) {
                                        $bsyDiv.hide();
                                        VIS.ADialog.error(errorThrown.statusText);
                                        return false;
                                    }
                                });

                            });
                            folTreeArea.html('').append($folderUl);
                            return $folderUl;
                        }
                        else {
                            VIS.ADialog.info('VA012_NoFolderFound');
                            return VIS.Msg.getMsg('VA012_NoFolderFound');
                        }
                    }
                },
                error: function (errorThrown) {
                    $bsyDiv.hide();
                    VIS.ADialog.error(errorThrown.statusText);
                    return false;
                }
            });

        };
        /**
         * Creating recursive child folders
         * @param {any} row
         * @param {any} data
         * @param {any} padding
         * @param {any} folderPath
         */
        function WidgetRecursiveFolder(row, data, padding, folderPath) {
            var $innerLI = null;
            var ArrayFolder = [];
            var image = "";
            var folderName = "";
            var initialCharacter = "";
            for (var j = 0; j < row.length; j++) {

                $innerLI = null;
                var $innerUl = $('<ul class="list-unstyled VA012-leftSideSubTreeUl {"></ul>');

                for (var i = 0; i < data.length; i++) {

                    if (data[i].FolderName.length > 25) {
                        folderName = data[i].FolderName.substring(0, 25) + "...";
                    }
                    else {
                        folderName = data[i].FolderName;
                    }

                    initialCharacter = (folderName.substring(0, 1)).trim();

                    if (data[i].ParentFolderID == row[j].row.FolderID) {

                        if (strFolderIds.contains(data[i].FolderID)) {
                            continue;
                        }
                        if (data[i].PublicAccess > 10) {
                            image = "<i class='vis vis-folder-globe'></i>";
                        }
                        else if (data[i].Sharecount > 2 || data[i].RoleSharecount > 0) {
                            image = "<i class='vis vis-folder-share'></i>";
                        }
                        else {
                            image = "<i class='fa fa-folder-o'></i>";
                        }

                        $innerLI = $(
                            '<li id="' + data[i].FolderID + '" folderName="' + VIS.Utility.encodeText(data[i].FolderName) + '" parentID="' + data[i].ParentFolderID + '" useraccess="' + data[i].UserAccessOnFolder + '" roleaccess="' + data[i].RoleAccessOnFolder + '" createdBy="' + data[i].CreateUser + '" inputType="' + data[i].InputType + '" folType="' + data[i].FolderType + '" IsSubscribe="' + data[i].IsSubscribedFolder + '" InitialCharacter="' + VIS.Utility.encodeText(initialCharacter.toUpperCase()) + '">' +
                            '<div class="VA012-leftTreeNode" title="' + VIS.Utility.encodeText(data[i].FolderPath) + '">' +
                            '<a id=a_"' + data[i].FolderID + '" href="javascript:void(0);">' +
                            image +
                            '<span>' + VIS.Utility.encodeText(data[i].FolderName) + '</span>' +
                            '</a>' +
                            '</div>' +
                            '</li>');

                        ArrayFolder.push({ row: data[i], root: $innerLI });
                        strFolderIds += data[i].FolderID + ",";
                        folderPath += "\\" + data[i].FolderName;
                        $innerUl.append($innerLI);

                    }
                }

                if ($innerLI != null) {
                    row[j].root.append($innerUl);
                    if (row[j].root.find('a span i').length <= 0) {
                        $(row[j].root.find('a span')[0]).append('<i class="vis vis-arrow-right"></i>');
                    }
                }
                else if (row[j].row.HasChild > 0) {
                    row[j].root.find('a span').append('<i class="vis vis-arrow-right"></i>');
                }
            }
            if (ArrayFolder.length > 0) {
                WidgetRecursiveFolder(ArrayFolder, data, padding + 10, folderPath);
            }
        }
        // Load documents for current record from database
        function LoadLinkedDocuments(orderByColumn, orderBy, pageNo, fromSort, folderID, currentFolder) {
            $bsyDiv.show();
            var files = folTreeArea.find("[data-parentfolderid= '" + folderID + "']");
            var folRow = null;
            if (files.length > 0) {
                files.remove();
            }
            var _pageSize = 50;
            var parameter = [];
            parameter.push(folderID);
            parameter.push(1);
            parameter.push("");
            parameter.push(0);
            parameter.push(0); //self.recordID
            parameter.push(false);
            parameter.push(false);
            parameter.push(false);
            parameter.push("0");
            parameter.push(_pageSize);
            parameter.push(0); //self.tableID
            parameter.push("UD");
            var url = VIS.Application.contextUrl + "VA012_BankJournalWidget/GetDocument";
            var documentData = VA012.ControllerRequestAjaxParameter1(url, parameter);
            if (documentData !== null) {
                documentData = documentData[0].LstDocument;
                if (documentData != null && documentData.length > 0) {
                    // Show list of documents
                    for (var i = 0; i < documentData.length; i++) {                        
                        var DocumentID = documentData[i].DocumentID;
                        var DocumentName = documentData[i].DocumentName;
                        var FileType = documentData[i].FileType;                        
                        var FolderID = documentData[i].FolderID;
                        var VersionNo = documentData[i].VersionNo;
                        var docExtClass = 'vis-doc-blank';
                        var docExtColor = 'rgba(var(--v-c-primary), 1)';
                        if (FileType == VA012.Common.XLS || FileType == VA012.Common.XLSX || FileType == VA012.Common.CSV) {
                            docExtClass = 'vis-doc-excel';
                            docExtColor = '#39b54a';
                            var $docLI = $(
                                '<li class="VA012-docTreeNode" data-documentid="' + DocumentID + '" data-versionNo="' + VersionNo +
                                '" data-parentfolderid="' + folderID + '" id="VA012-doclistingpanel_' + DocumentID + '">' +
                                '<div class="VA012-leftTreeNode">' +
                                '<a id="a_' + documentData[i].FolderID + '" href="javascript:void(0);" val="' + VIS.Utility.encodeText(documentData[i].DocumentName) + '">' +
                                //folderImage +
                                '<i class="vis ' + docExtClass + '" aria-hidden="true" style="color:' + docExtColor + '"></i>' +
                                '<span class="VA012-documentName" id="VA012_documentName_' + DocumentID + '">' + VIS.Utility.encodeText(documentData[i].DocumentName)
                                + '' + VIS.Utility.encodeText(documentData[i].FileType) + '</span>' +
                                '</a>' +
                                '</div>' +
                                '</li>');
                            $docLI.insertAfter(currentFolder);
                        }
                    };
                    $bsyDiv.hide();
                    folTreeArea.find(".VA012-docTreeNode").on('click', function (e) {
                        e.stopPropagation();
                        folTreeUpload.removeAttr('disabled');
                        folRow = $(this).closest('li');
                        $('#folderUL_' + widgetID).find(".VA012-selected").removeClass('VA012-selected');
                        $('#folderUL_' + widgetID).find(".VA012-selectedTreeNode").css('background-color', 'inherit');
                        folTreeArea.find("#VA012-doclistingpanel_" + folRow.data('documentid')).find(".VA012-selected").removeClass('VA012-selected');
                        folTreeArea.find("#VA012-doclistingpanel_" + folRow.data('documentid')).find(".VA012-selectedTreeNode").css('background-color', 'inherit');
                        folRow.find("div").eq(0).find('a i span').addClass('VA012-selected');
                        if (!folRow.find("div").eq(0).hasClass('VA012-selectedTreeNode')) {
                            folRow.find("div").eq(0).addClass('VA012-selectedTreeNode').css('background-color', 'rgba(var(--v-c-primary), .1)');
                        }
                        else if (folRow.find("div").eq(0).hasClass('VA012-selectedTreeNode')) {
                            folRow.find("div").eq(0).addClass('VA012-selectedTreeNode').css('background-color', 'rgba(var(--v-c-primary), .1)');
                        }
                        else if (folRow.children("ul").length > 0) {
                            folRow.find("div").eq(0).removeClass("VA012-selectedTreeNode");
                        }


                    });
                    folTreeUpload.on('click', function (e) {
                        e.stopPropagation();
                        $.ajax({
                            url: VIS.Application.contextUrl + "VA012_BankJournalWidget/GetMetaData",
                            type: "GET",
                            async: false,
                            dataType: "json",
                            contentType: "application/json; charset=utf-8",
                            data: ({
                                documentID: folRow.data('documentid')
                            }),
                            success: function (data) {
                                if (data != null && data != "") {
                                    _result = JSON.parse(data);
                                    folderFader.addClass('d-none');
                                    isDMS = true;
                                    if (paramDiv == null) {
                                        dragDiv.hide();
                                        //load parameter div
                                        loadParam();
                                    }
                                    else {
                                        dragDiv.hide();
                                        //show parameter div
                                        paramDiv.show();
                                        paramFooter.show();
                                    }
                                }
                            },
                            error: function (errorThrown) {
                                $bsyDiv.hide();
                                VIS.ADialog.error(errorThrown.statusText);
                                return false;
                            }
                        });

                    });
                };
            };
        };
        /**
       *  This function to call controller HTTPPOST action With parameter 
       *  @param url
       *  @param param
       *  @return dbResult
   */
        VA012.ControllerRequestAjaxParameter1 = function (url, param) {
            var dbResult = null;
            var parameter = param;
            $.ajax({
                type: "POST",
                async: false,
                url: url,
                dataType: "json",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(parameter),
                success: function (data) {
                    if (data != null) {
                        dbResult = JSON.parse(data);
                    }
                }, error: function (errorThrown) {
                    if (errorThrown.statusText != 'OK' && errorThrown.statusText != 'parsererror') {
                        alert(errorThrown.statusText);
                    }
                }
            });
            return dbResult;
        };

        /*this function is used to refresh design and data of widget*/
        this.refreshWidget = function () {
            $bsyDiv.hide();
            resetControls();
            paramDiv.hide();
            paramFooter.hide();
            dragDiv.show();
            dropContainer.find('.VA012-uploadFileWidget_' + widgetID).val(null);
            fileNameLabel.text('');
            fileNameLabel.val('');
            nxtBtn.attr("disabled", true);
            //this.initialize();
        };
        this.disposeComponents = function () {
            $self = null;
            $root = null;
            this.frame = null;
            this.windowNo = 0;
            this.widgetInfo = null;
            dropContainer = null;
            uploadFile = null;
            cancelBtn = null;
            uploadBtn = null;
            nxtBtn = null;
            dragDiv = null;
            paramDiv = null;
            paramFooter = null;
            selectedFileName = null;
            _cmbBank = null;
            _cmbBankAccount = null;
            _cmbBankAccountClasses = null;
            C_BANK_ID = 0
            C_BANKACCOUNT_ID = 0
            _statementDate = null;
            _statementName = null;
            isChecked = null;
            $loadParaDiv = null;
            _file = null;
            lstLatestFiles = [];
            oldFiles = [];
            currentchunk = 0;
            currentFile = 0;
            folder = Date.now().toString();
            chunkSize = 1 * 1024 * 1024;
            totalChunks = 0;
            currentFileChunkNo = 0;
            filesInfo = [];
            _selectedFiles = null;
            _currencyId = null;
            _result = null;
            Bank_Charge_ID = null;
            fileNameLabel.text('');
            folTreeArea = null;
            folTreeUpload = null;
            folTreeCancel = null;
            // loadingFader = null;
            folderFader = null;
            strFolderIds = "";

            _cmbBank.off('click');
            _cmbBankAccount.off('click');
            _cmbBankAccountClasses.off('click');
            dropContainer.off('click');
            cancelBtn.off('click');
            uploadBtn.off('click');
            nxtBtn.off('click');
            openDMSBtn.off('click');
            folTreeCancel.off('click');
            folTreeUpload.off('click');
        };
    };
    // Must Implement with same parameter
    VA012.VA012_BankingJournal.prototype.init = function (windowNo, frame) {
        this.frame = frame;
        this.windowNo = windowNo;
        // Widget info, we can save additional information in widget record
        this.widgetInfo = frame.widgetInfo;
        this.initialize();
        this.frame.getContentGrid().append(this.getRoot());
    };
    // To change size of the form
    VA012.VA012_BankingJournal.prototype.widgetSizeChange = function (size) {
        // Widget info, we can save additional information in widget record
        var x = size;
    };
    // Must implement dispose
    VA012.VA012_BankingJournal.prototype.dispose = function () {
        /*CleanUp Code */
        //Dispose this component
        this.disposeComponent();
        //Call frame dispose function
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
    };
    VA012.VA012_BankingJournal.prototype.refreshWidget = function () {
        this.refreshWidget();
    };
    // Fire window's event from widget
    VA012.VA012_BankingJournal.prototype.addChangeListener = function (listener) {
        this.listener = listener;
    };
    VA012.VA012_BankingJournal.prototype.widgetFirevalueChanged = function (value) {
        // Trigger custom event with the value
        if (this.listener)
            this.listener.widgetFirevalueChanged(value);
    };
})(VA012, jQuery);