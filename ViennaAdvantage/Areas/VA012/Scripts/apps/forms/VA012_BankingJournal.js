; VA012 = window.VA012 || {};

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
        var $root = null;
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
        var $paramFooterDiv = null;
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
        var C_BankStatement_ID = 0;

        /* Step model - see showStep(). Two steps: pick the file, then fill in
           the parameters. The parameter form always holds every control. */
        var STEP_FILE = 1;
        var STEP_PARAM = 2;
        var currentStep = STEP_FILE;
        var bodyDiv = null;
        var fileFooter = null;

        var Batchsuccesspay = null;
        var $loadStatementResult = "";
        var $successNoteofloadStatement = "";
        var ViewBankStatementForm = "";
        var bankStatementFormInfo = [];
        var BankStatementForm_ID = 0;

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
            createBusyIndicator();
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
            widgetID = widgetID + '_' + VIS.Env.getWindowNo();
            if (widgetID == 0) {
                widgetID = $self.windowNo;
            }
            $root = $("<div id='WidMainRoot_" + widgetID + "' class='VA012-bj'></div>");
            Design();

            //VIS_045: Get Bank Statement form ID
            GetBankStatementFormID("VA012_BankStatement");
        };

        /**
         * This funtion i sused to get the Form ID
         * @param {any} formName 
         * VIS_045
         */
        function GetBankStatementFormID(formName) {
            $.ajax({
                type: 'GET',
                data: {
                    formName: formName
                },
                contentType: "application/json; charset=utf-8",
                url: VIS.Application.contextUrl + "VA012_BankJournalWidget/GetBankStatementFormID",
                success: function (result) {
                    BankStatementForm_ID = JSON.parse(result);
                },
                error: function (err) {
                    console.log(err);
                }
            });
        };

        //Create design
        function Design() {
            // Accepted extensions, rendered as the drop-zone hint ("XLS · XLSX · CSV").
            // Built from the constants above so it needs no translated message.
            var acceptedTypes = (VA012.Common.XLS + ' ' + VA012.Common.XLSX + ' ' + VA012.Common.CSV)
                .replace(/\./g, '').toUpperCase().replace(/ /g, ' · ');
            var widgetTitle = VIS.Msg.getMsg('VA012_BankingJournal');

            // The widget root is the panel; the header/body/footer bands below are
            // the shared widget shell (dashboard-widgets.md > Widget Header).
            dropContainer = $root;
            $root.append('<div class="VA012-bj-header">' +
                // fa-book, not fa-bank: the Bank field below owns that glyph.
                '<div class="VA012-bj-iconwell"><i class="fa fa-book" aria-hidden="true"></i></div>' +
                '<div class="VA012-bj-titlewrap">' +
                '<div class="VA012-bj-title" title="' + widgetTitle + '">' + widgetTitle + '</div>' +
                '</div>' +
                '</div>' +

                '<div class="VA012-bj-body" id="VA012-bj-body_' + widgetID + '">' +
                '<div class="VA012-bj-step-pane" id="VA012-bj-paneFile_' + widgetID + '">' +
                '<input id="VA012-uploadFile_' + widgetID + '" class="VA012-bj-fileinput VA012-uploadFileWidget_' + widgetID
                + '" type="file" accept=".csv, .xls, .xlsx">' +
                '<label for="VA012-uploadFile_' + widgetID + '" class="VA012-bj-drop">' +
                '<i class="fa fa-cloud-upload VA012-bj-drop-icon" id="VA012-uploadImg_' + widgetID + '" aria-hidden="true"></i>' +
                '<div class="VA012-bj-drop-label">' + VIS.Msg.getMsg('VA012_DragFiles') + '<span class="VA012-bj-browse">'
                + VIS.Msg.getMsg('VA012_Browse') + '</span></div>' +
                '<div class="VA012-bj-drop-hint">' + acceptedTypes + '</div>' +
                '<div class="VA012-bj-filechip" id="VA012-FileNamelbl_' + widgetID + '"></div>' +
                '</label>' +
                '</div>' +
                '</div>' +

                '<div class="VA012-bj-footer" id="VA012-bj-fileFooter_' + widgetID + '">' +
                '<button type="button" class="VA012-bj-link" id="VA012-OpenDMSIcon_' + widgetID + '">' +
                '<i class="fa fa-folder-open" aria-hidden="true"></i>' +
                '<span>' + VIS.Msg.getMsg('VA012_OpenDMS') + '</span>' +
                '</button>' +
                '<div class="VA012-bj-actions">' +
                '<button type="button" class="VA012-bj-btn VA012-bj-btn--primary" id="VA012_NextBtn_' + widgetID + '">'
                + VIS.Msg.getMsg('VA012_Next') + '</button>' +
                '</div>' +
                '</div>' +

                // DMS folder picker - an overlay over the whole tile, not a
                // dialog. See the stylesheet for the scrolling exception.
                '<div class="VA012-bj-fader" id="VA012-folderFader_' + widgetID + '" hidden>' +
                '<div class="VA012-bj-fader-title">' + VIS.Msg.getMsg('VA012_SelectFolder') + '</div>' +
                '<div class="VA012-bj-tree" id="VA012-folTreeArea_' + widgetID + '"></div>' +
                '<div class="VA012-bj-footer VA012-bj-footer--split">' +
                '<div class="VA012-bj-actions">' +
                '<button type="button" class="VA012-bj-btn VA012-bj-btn--primary" id="VA012-treeCancelBtn_' + widgetID + '">'
                + VIS.Msg.getMsg('VA012_Back') + '</button>' +
                '<button type="button" class="VA012-bj-btn VA012-bj-btn--primary" id="VA012-treeUploadBtn_' + widgetID + '" disabled>'
                + VIS.Msg.getMsg('VA012_Next') + '</button>' +
                '</div>' +
                '</div>' +
                '</div>');

            bodyDiv = $root.find('#VA012-bj-body_' + widgetID);
            folderFader = $root.find('#VA012-folderFader_' + widgetID);
            folTreeArea = $root.find('#VA012-folTreeArea_' + widgetID);
            folTreeCancel = $root.find('#VA012-treeCancelBtn_' + widgetID);
            folTreeUpload = $root.find('#VA012-treeUploadBtn_' + widgetID);
            dragDiv = $root.find('#VA012-bj-paneFile_' + widgetID);
            fileFooter = $root.find('#VA012-bj-fileFooter_' + widgetID);
            uploadFile = $root.find('#VA012-uploadFile_' + widgetID);
            uploadImg = $root.find('#VA012-uploadImg_' + widgetID);
            nxtBtn = $root.find('#VA012_NextBtn_' + widgetID);
            fileNameLabel = $root.find('#VA012-FileNamelbl_' + widgetID);
            openDMSBtn = $root.find('#VA012-OpenDMSIcon_' + widgetID);

            nxtBtn.prop("disabled", _selectedFiles == null);

            var modulePrefix = VIS.dataContext.getJSONRecord("ModulePrefix/GetModulePrefix", "VADMS_");
            openDMSBtn.prop("hidden", modulePrefix == null);

            showStep(STEP_FILE);
            Events();
        };

        /**
         * Widget step model. Step 1 picks the file, step 2 holds the whole
         * parameter form.
         */
        function showStep(step) {
            currentStep = step;
            var onFile = (step === STEP_FILE);

            dragDiv.prop('hidden', !onFile);
            fileFooter.prop('hidden', !onFile);
            if (paramDiv != null) {
                paramDiv.prop('hidden', onFile);
                paramFooter.prop('hidden', onFile);
            }
        }

        /**
         * Adapt the chrome to the cell we were actually given.
         *
         * The parameter form is always a single step holding every control -
         * it is never paged. One field per line, and the form scrolls when the
         * cell cannot show all six at once.
         */
        function syncCellVariant() {
            var rootPx = parseFloat($root.css('font-size')) || 16;

            // Short cells drop the icon well; the Content Fit Budget spends
            // that height on form rows instead.
            $root.toggleClass('VA012-bj-compact', $root.height() < (18 * rootPx));
        }

        function Events() {
            // Preventing page from redirecting
            dropContainer.on("dragover", function (e) {
                e.preventDefault();
                e.stopPropagation();
                if (currentStep === STEP_FILE) {
                    $root.addClass('VA012-bj-dragover');
                }
            });
            dropContainer.on('dragleave', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $root.removeClass('VA012-bj-dragover');
            });
            // Drop
            dropContainer.on('drop', function (e) {
                e.stopPropagation();
                e.preventDefault();
                $root.removeClass('VA012-bj-dragover');
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
                goToParam();
            });
            openDMSBtn.on('click', function (e) {
                isDMS = true;
                openFolderPicker();
            });

            // Folder picker - Back
            folTreeCancel.on('click', function (e) {
                strFolderIds = "";
                isDMS = false;
                closeFolderPicker();
            });

            // Folder picker - Next. Pull the selected document's path/name,
            // then continue into the parameter form exactly as a dropped file
            // does.
            folTreeUpload.on('click', function (e) {
                e.stopPropagation();
                var documentID = folTreeArea.find(".VA012-selDoc").attr('data-documentid');
                fileNameLabel.text('').attr("title", '').removeClass('VA012-bj-has-file');
                nxtBtn.prop("disabled", true);
                $bsyDiv.show();
                $.ajax({
                    url: VIS.Application.contextUrl + "VA012_BankJournalWidget/GetMetaData",
                    type: "GET",
                    async: true,
                    dataType: "json",
                    contentType: "application/json; charset=utf-8",
                    data: ({
                        documentID: documentID
                    }),
                    success: function (data) {
                        if (data != null && data != "") {
                            //return filename and file path
                            _result = JSON.parse(data);
                            isDMS = true;
                            closeFolderPicker();
                            goToParam();
                        }
                        $bsyDiv.hide();
                    },
                    error: function (errorThrown) {
                        $bsyDiv.hide();
                        VIS.ADialog.error(errorThrown.statusText);
                        return false;
                    }
                });
            });
        }

        /**
         * Enter the parameter form, building it on first use.
         */
        function goToParam() {
            if (paramDiv == null) {
                loadParam();
            }
            syncCellVariant();
            showStep(STEP_PARAM);
        }

        /**
         * DMS folder picker - an overlay over the widget's own tile, refreshed
         * on every open. Its buttons are bound once, in Events().
         */
        function openFolderPicker() {
            folTreeUpload.prop('disabled', true);
            folderFader.prop('hidden', false);
            $bsyDiv.show();
            WidgetFolderTreeStruct();
            $bsyDiv.hide();
        }

        function closeFolderPicker() {
            if (folderFader != null) {
                folderFader.prop('hidden', true);
            }
            if (folTreeUpload != null) {
                folTreeUpload.prop('disabled', true);
            }
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
                    var displayName = filename + "" + fileExt;
                    fileNameLabel.text(displayName).attr("title", displayName).addClass('VA012-bj-has-file');
                    nxtBtn.prop("disabled", false);
                    var file = obj;
                    _result = $.parseJSON(VA012.UploadExcel(file, null, null));
                    goToParam();
                }
            }
        }

        function loadParam() {
            // Underline field: label above, then an icon + control row carrying
            // the rule (windows-and-panels.md > Form Field).
            var required = '<span class="VA012-bj-req">*</span>';

            function field(icon, labelKey, controlID, control, labelID) {
                return '<div class="VA012-bj-field">'
                    + '<label class="VA012-bj-label" for="' + controlID + '"'
                    + (labelID ? ' id="' + labelID + '"' : '') + '>'
                    + VIS.Msg.getMsg(labelKey) + required + '</label>'
                    + '<div class="VA012-bj-control">'
                    + '<i class="fa ' + icon + ' VA012-bj-fieldicon" aria-hidden="true"></i>'
                    + control
                    + '</div>'
                    + '</div>';
            }

            $loadParaDiv = $('<div class="VA012-bj-step-pane" id="VA012_paramMainDiv_' + widgetID + '" hidden>'
                + '<div class="VA012-bj-form">'

                + field('fa-bank', "VA012_Bank", 'VA012_STAT_cmbBank_' + widgetID,
                    '<select class="VA012-bj-input" id="VA012_STAT_cmbBank_' + widgetID + '"></select>')

                + field('fa-credit-card', "VA012_BankAccount", 'VA012_STAT_cmbBankAccount_' + widgetID,
                    '<select class="VA012-bj-input" id="VA012_STAT_cmbBankAccount_' + widgetID + '"></select>')

                + field('fa-calendar', "VA012_StatementDate", 'VA012_STAT_statementDate_' + widgetID,
                    '<input class="VA012-bj-input" type="date" max="9999-12-31" id="VA012_STAT_statementDate_' + widgetID + '">',
                    'VA012_STAT_lblStatementDate_' + widgetID)

                + field('fa-cogs', "VA012_ClassName", 'VA012_STAT_cmbBankAccountClassName_' + widgetID,
                    '<select class="VA012-bj-input" id="VA012_STAT_cmbBankAccountClassName_' + widgetID + '"></select>')

                + field('fa-file-text-o', "VA012_StatementNumber", 'VA012_STAT_txtStatementNo_' + widgetID,
                    '<input class="VA012-bj-input" type="text" id="VA012_STAT_txtStatementNo_' + widgetID + '">')

                + '<div class="VA012-bj-field">'
                + '<label class="VA012-bj-check" for="VA012_CheckBox_' + widgetID + '">'
                + '<input class="VA012-bj-checkbox" type="checkbox" value="checked" id="VA012_CheckBox_' + widgetID + '">'
                + '<span title="' + VIS.Msg.getMsg("VA012_StatementDateAsAccountDate") + '">'
                + VIS.Msg.getMsg("VA012_StatementDateAsAccountDate") + '</span>'
                + '</label>'
                + '</div>'

                + '</div>'
                + '</div>');
            bodyDiv.append($loadParaDiv);

            $paramFooterDiv = $('<div class="VA012-bj-footer VA012-bj-footer--split" id="VA012-paramFooterDiv_' + widgetID + '" hidden>'
                + '<div class="VA012-bj-actions">'
                + '<button type="button" class="VA012-bj-btn VA012-bj-btn--primary VA012-bj-back">'
                + VIS.Msg.getMsg('VA012_Back') + '</button>'
                + '<button type="button" class="VA012-bj-btn VA012-bj-btn--primary VA012-bj-upload">'
                + VIS.Msg.getMsg('VA012_Upload') + '</button>'
                + '</div>'
                + '</div>');
            $root.append($paramFooterDiv);

            getControls();
            loadFunctions.loadBank();
            loadFunctions.loadBankAccountCharges();
            /*VIS_427 Shown message on change of statement date*/
            _statementDate = paramDiv.find("#VA012_STAT_statementDate_" + widgetID);
            _statementDate.on('change', function (e) {

                if (Globalize.format(new Date(_statementDate.val()), "yyyy-MM-dd") > Globalize.format(new Date(), "yyyy-MM-dd")) {
                    // not required the VIS.Msg.getMsg() function
                    VIS.ADialog.info("VA012_StatementDateToday", null, "", "");
                    _statementDate.val("");
                    return false;
                }
            });
            // Clicking anywhere in the date control opens the picker. On WebKit
            // the invisible native picker button covers the whole control (see
            // the stylesheet), so the click never reaches here and the browser
            // opens the picker itself. This is the fallback for engines without
            // that pseudo-element.
            _statementDate.closest('.VA012-bj-control')
                .addClass('VA012-bj-control--date')
                .on('click', function (e) {
                    var input = _statementDate[0];
                    try {
                        if (input && typeof input.showPicker === 'function') {
                            input.showPicker();
                            return;
                        }
                    }
                    catch (err) {
                        // showPicker throws unless it is a user gesture on a
                        // supported browser; fall through to focus.
                    }
                    _statementDate.focus();
                });
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
                    C_BankStatement_ID = 0;
                    loadFunctions.loadCurrency();
                    loadBankAccountClasses();
                    CheckStatementExist();
                }
                else {
                    return "VA012_NoBankAccountSelected";
                }
            });
            //Cancel button represnts back button
            cancelBtn.on('click', function (e) {
                strFolderIds = "";
                showStep(STEP_FILE);
                if (!isDMS) {
                    nxtBtn.prop("disabled", _selectedFiles == null);
                }
                else {
                    // Came in from DMS - go back to the folder picker, not the
                    // drop zone.
                    openFolderPicker();
                }
                //isDMS = false;
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
                        if (C_BankStatement_ID > 0) {
                            VIS.ADialog.confirm("VA012_BankStatementExist", true, "", "Confirm", function (result) {
                                if (!result) {
                                    $bsyDiv.hide();
                                    return;
                                }
                                else {
                                    UploadStatment();
                                }
                            });
                        }
                        else {
                            UploadStatment();
                        }
                    }
                }
                else {
                    $bsyDiv.hide();
                    VIS.ADialog.info("VA012_ErrorInGettingFile", null, "", "");
                    return;
                }
            });
        };

        function UploadStatment() {
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

                        // Get Information to open Bank Statement form and set values
                        SetbankStatementFormInfo();

                        $bsyDiv.hide();
                        resetControls();
                        showStep(STEP_FILE);
                        dropContainer.find('.VA012-uploadFileWidget_' + widgetID).val(null);
                        fileNameLabel.text('').attr("title", '').removeClass('VA012-bj-has-file');
                        nxtBtn.prop("disabled", true);
                        isDMS = false;
                        strFolderIds = "";
                        //VIS.ADialog.info("VA012_StatementUploadDone", null, "", "");

                        // Open Success Dialog
                        Batchsuccesspay = new VIS.ChildDialog();
                        Batchsuccesspay.setContent(ResponseDialog());
                        $successNoteofloadStatement.text(VIS.Msg.getMsg("VA012_StatementUploadDone"));
                        $successNoteofloadStatement.css('visibility', 'visible');
                        Batchsuccesspay.setTitle(VIS.Msg.getMsg("VA012_LoadStatementRes"));
                        Batchsuccesspay.setWidth("24%");
                        Batchsuccesspay.show();
                        Batchsuccesspay.hidebuttons();

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
            });
        }

        /** VIS_045: This function is used to Open Dialog of Success */
        function ResponseDialog() {
            $loadStatementResult = $("<div class='VA012-bj-dialog'>"
                + "<label id='VA012_SuccessMsg_" + widgetID + "'></label>"
                + "</div>");

            $resltbtns = $("<div class='VA012-bj-dialog-footer'>" +
                "<button type='button' class='VA012-bj-btn VA012-bj-btn--primary' id='VA012_OpenBankStatementForm_" + widgetID + "'>" +
                VIS.Msg.getMsg('VA012_ViewStatement') + "</button>" +
                "</div>");
            $loadStatementResult.append($resltbtns);

            // Success Message 
            $successNoteofloadStatement = $loadStatementResult.find('#VA012_SuccessMsg_' + widgetID);

            // Open Bank Statement Form
            ViewBankStatementForm = $loadStatementResult.find('#VA012_OpenBankStatementForm_' + widgetID);
            ViewBankStatementForm.on('click', function (e) {
                $bsyDiv.show();

                // get bank Statement Form ID
                if (BankStatementForm_ID == 0) {
                    GetBankStatementFormID("VA012_BankStatement");
                }

                // Open Form with Additional Information
                VIS.viewManager.startForm(BankStatementForm_ID, bankStatementFormInfo);

                // Close Dialog
                Batchsuccesspay.close();

                // Hide Busy Indicator
                $bsyDiv.hide();
            });

            return $loadStatementResult;
        }

        /** VIS_045: This function is used to Set Information which will be used when Open Bank Statement Form */
        function SetbankStatementFormInfo() {
            bankStatementFormInfo = [];
            bankStatementFormInfo.push(_cmbBank.val());
            bankStatementFormInfo.push(_cmbBankAccount.val());
            bankStatementFormInfo.push(_statementDate.val());
        }

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
            paramDiv = $loadParaDiv;
            paramFooter = $paramFooterDiv;
            _cmbBank = $loadParaDiv.find("#VA012_STAT_cmbBank_" + widgetID);
            _cmbBankAccount = $loadParaDiv.find("#VA012_STAT_cmbBankAccount_" + widgetID);
            _cmbBankAccountClasses = $loadParaDiv.find("#VA012_STAT_cmbBankAccountClassName_" + widgetID);
            cancelBtn = $paramFooterDiv.find('.VA012-bj-back');
            uploadBtn = $paramFooterDiv.find('.VA012-bj-upload');
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

        /** VIS_045:  This function is used to get the Open bank Statement Details Exist or not */
        function CheckStatementExist() {
            VIS.dataContext.getJSONData(VIS.Application.contextUrl + "VA012_BankJournalWidget/CheckStatementExist",
                { C_BankAccount_ID: _cmbBankAccount.val() }, callbackCheckStatementExist);
            function callbackCheckStatementExist(result) {
                C_BankStatement_ID = JSON.parse(result);
            }
        };

        // Creating folder tree ul
        function WidgetFolderTreeStruct() {
            var folderArrayRes = null;
            $.ajax({
                url: VIS.Application.contextUrl + "VA012_BankJournalWidget/GetFolders",
                type: "GET",
                async: true,
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
                        if (folTreeArea == null) {
                            // picker was closed while the folders were loading
                            return;
                        }
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
                                        // Added folder in list
                                        $folderLI = $(
                                            '<li id="' + folderArrayRes[i].FolderID + '" folderName="' + VIS.Utility.encodeText(folderArrayRes[i].FolderName) + '" parentID="' + folderArrayRes[i].ParentFolderID + '" useraccess="' + folderArrayRes[i].UserAccessOnFolder + '" roleaccess="' + folderArrayRes[i].RoleAccessOnFolder + '" createdBy="' + folderArrayRes[i].CreateUser + '" inputType="' + folderArrayRes[i].InputType + '" folType="' + folderArrayRes[i].FolderType + '" IsSubscribe="' + folderArrayRes[i].IsSubscribedFolder + '" InitialCharacter="' + initialCharacter.toUpperCase() + '">' +
                                            '<div class="VA012-bj-treenode" title="' + folderArrayRes[i].FolderName + '">' +
                                            '<a id="a_' + folderArrayRes[i].FolderID + '" href="javascript:void(0);" val="' + VIS.Utility.encodeText(folderArrayRes[i].FolderPath) + '">' +
                                            folderImage +
                                            '<span>' + VIS.Utility.encodeText(folderName) + '</span>' +
                                            '</a>' +
                                            '</div>' +
                                            '</li>');
                                    }
                                    //Added folder in folder array
                                    arrayFolder.push({ row: folderArrayRes[i], root: $folderLI });
                                    strFolderIds += folderArrayRes[i].FolderID + ",";
                                    folderPath = folderArrayRes[i].FolderName;
                                    //Added child folder
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
                                    async: true,
                                    dataType: "json",
                                    contentType: "application/json; charset=utf-8",
                                    data: ({
                                        folderID: folRow.attr('id')
                                    }),
                                    success: function (data) {
                                        if (folTreeArea == null) {
                                            // picker closed while the check was in flight
                                            return;
                                        }
                                        if (data != null && data != "") {
                                            if (data == VA012.Common.Full_Access || data == VA012.Common.RWD_Access || data == VA012.Common.RW_Access) {
                                                // Selection is carried by classes only - the selected-row
                                                // gradient lives in the stylesheet.
                                                var $folderUL = $('#folderUL_' + widgetID);
                                                $folderUL.find('.VA012-bj-selected').removeClass('VA012-bj-selected');
                                                $folderUL.find('.VA012-bj-treenode-selected').removeClass('VA012-bj-treenode-selected');
                                                folRow.children('div').first()
                                                    .addClass('VA012-bj-treenode-selected')
                                                    .find('a span').addClass('VA012-bj-selected');
                                                var orderByColumn = [];
                                                orderByColumn.push('Updated');
                                                _scrollpage = 1;
                                                _searchDocName = '';
                                                $bsyDiv.hide();
                                                //Load documents for current record from database
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
                    else {
                        VIS.ADialog.info('VA012_NoFolderFound');
                        return VIS.Msg.getMsg('VA012_NoFolderFound');
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
                var $innerUl = $('<ul class="VA012-bj-subtree"></ul>');

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
                            '<div class="VA012-bj-treenode" title="' + VIS.Utility.encodeText(data[i].FolderPath) + '">' +
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
                        $(row[j].root.find('a span')[0]).append('<i class="vis vis-arrow-right VA012-bj-arrow"></i>');
                    }
                }
                else if (row[j].row.HasChild > 0) {
                    row[j].root.find('a span').append('<i class="vis vis-arrow-right VA012-bj-arrow"></i>');
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
            $.ajax({
                url: VIS.Application.contextUrl + "VA012_BankJournalWidget/GetDocument",
                type: "POST",
                async: true,
                dataType: "json",
                contentType: 'application/json; charset=utf-8',
                data: JSON.stringify(parameter),
                success: function (documentData) {
                    if (folTreeArea == null) {
                        // picker closed while the documents were loading
                        return;
                    }
                    if (documentData !== null) {
                        documentData = JSON.parse(documentData);
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
                                        '<li class="VA012-bj-docnode VA012-folderDocs" data-documentid="' + DocumentID + '" data-versionNo="' + VersionNo +
                                        '" data-parentfolderid="' + folderID + '" id="VA012-doclistingpanel_' + DocumentID + '">' +
                                        '<div class="VA012-bj-treenode">' +
                                        '<a id="a_' + documentData[i].FolderID + '" href="javascript:void(0);" val="' + VIS.Utility.encodeText(documentData[i].DocumentName) + '">' +
                                        //folderImage +
                                        '<i class="vis ' + docExtClass + '" aria-hidden="true" style="color:' + docExtColor + '"></i>' +
                                        '<span class="VA012-bj-docname" id="VA012_documentName_' + DocumentID + '">' + VIS.Utility.encodeText(documentData[i].DocumentName)
                                        + '' + VIS.Utility.encodeText(documentData[i].FileType) + '</span>' +
                                        '</a>' +
                                        '</div>' +
                                        '</li>');
                                    $docLI.insertAfter(currentFolder);
                                }
                            };
                            $bsyDiv.hide();
                            //Added and remove selected class from selected folder or file
                            folTreeArea.find(".VA012-bj-docnode").on('click', function (e) {
                                e.stopPropagation();
                                folTreeUpload.prop('disabled', false);
                                folRow = $(this).closest('li');
                                // Only one node in the whole tree is selected at a time.
                                var $folderUL = $('#folderUL_' + widgetID);
                                $folderUL.find('.VA012-bj-selected').removeClass('VA012-bj-selected');
                                $folderUL.find('.VA012-bj-treenode-selected').removeClass('VA012-bj-treenode-selected');
                                folRow.children('div').first()
                                    .addClass('VA012-bj-treenode-selected')
                                    .find('a span').addClass('VA012-bj-selected');
                                folTreeArea.find(".VA012-folderDocs").removeClass("VA012-selDoc");
                                $(folRow[0]).addClass("VA012-selDoc");

                            });
                        };
                    };
                },
                error: function (errorThrown) {
                    $bsyDiv.hide();
                    VIS.ADialog.error(errorThrown.statusText);
                    return false;
                }
            });
        };

        /**
         * Re-measure the cell. init() appends the root only after initialize()
         * has run, so nothing is measurable until a tick later.
         */
        this.syncLayout = function () {
            if ($root != null) {
                syncCellVariant();
            }
        };

        /**
         * Host hook - called by HomeMgr2 whenever the dashboard is resized or
         * edit mode is toggled. The span drives the narrow-cell variant in the
         * stylesheet; the cell's own height drives the compact header.
         */
        this.widgetSizeChange = function (size) {
            if ($root == null || size == null) {
                return;
            }
            $root.attr('data-bj-rows', VIS.Utility.Util.getValueOfInt(size.rows) || 1);
            $root.attr('data-bj-cols', VIS.Utility.Util.getValueOfInt(size.Cols) || 1);
            syncCellVariant();
        };

        /*this function is used to refresh design and data of widget*/
        this.refreshWidget = function () {
            $bsyDiv.hide();
            closeFolderPicker();
            if (paramDiv != null) {
                resetControls();
            }
            isDMS = false;
            showStep(STEP_FILE);
            dropContainer.find('.VA012-uploadFileWidget_' + widgetID).val(null);
            fileNameLabel.text('').attr("title", '').removeClass('VA012-bj-has-file');
            _selectedFiles = null;
            nxtBtn.prop("disabled", true);
            strFolderIds = "";
            //this.initialize();
        };

        this.disposeComponents = function () {
            closeFolderPicker();

            // Unbind while the references are still live - the previous order
            // nulled them first, so every .off() below threw.
            if ($root != null) {
                $root.find('*').off();
                $root.off();
            }

            $self = null;
            $root = null;
            this.frame = null;
            this.windowNo = 0;
            this.widgetInfo = null;
            dropContainer = null;
            uploadFile = null;
            uploadImg = null;
            cancelBtn = null;
            uploadBtn = null;
            nxtBtn = null;
            openDMSBtn = null;
            dragDiv = null;
            bodyDiv = null;
            fileFooter = null;
            fileNameLabel = null;
            paramDiv = null;
            paramFooter = null;
            _cmbBank = null;
            _cmbBankAccount = null;
            _cmbBankAccountClasses = null;
            C_BANK_ID = 0;
            C_BANKACCOUNT_ID = 0;
            _statementDate = null;
            _statementName = null;
            isChecked = null;
            $loadParaDiv = null;
            $paramFooterDiv = null;
            _selectedFiles = null;
            _currencyId = null;
            _result = null;
            Bank_Charge_ID = null;
            folderFader = null;
            folTreeArea = null;
            folTreeUpload = null;
            folTreeCancel = null;
            strFolderIds = "";
            isDMS = false;
            currentStep = STEP_FILE;
            C_BankStatement_ID = 0;
            Batchsuccesspay = null;
            $loadStatementResult = "";
            $successNoteofloadStatement = "";
            ViewBankStatementForm = "";
            bankStatementFormInfo = [];
            BankStatementForm_ID = 0;
        };
    };
    // Must Implement with same parameter
    VA012.VA012_BankingJournal.prototype.init = function (windowNo, frame) {
        this.frame = frame;
        this.windowNo = windowNo;
        // Widget info, we can save additional information in widget record
        this.widgetInfo = frame.widgetInfo;
        this.initialize();
        this.frame.getContentGrid().append(this.getRoot);
        // Measure once the cell exists, so the header treatment and the number
        // of parameter steps are right on first paint rather than on first use.
        var self = this;
        setTimeout(function () {
            self.syncLayout();
        }, 0);
    };

    // Must implement dispose
    VA012.VA012_BankingJournal.prototype.dispose = function () {
        /*CleanUp Code */
        //Dispose this component
        this.disposeComponents();
        //Call frame dispose function
        if (this.frame)
            this.frame.dispose();
        this.frame = null;
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