; VA012 = window.VA012 || {};

; (function (VA012, $) {

    //VA012.VA012_BankingJournal = VA012 || {};

    // Form class function fullnamespace

    // Document Uploader
    VA012.VA012_BankingJournal = function () { //folderID, windowID, tableID, recordID, windowName, tabName, windowNo
        /* Global variable declaration */
        this.frame = null;
        this.windowNo = 0;
        this.isInWindow = null;
        this.widgetInfo = null;
        this.windowName = '';
        this.windowID = -99999;
        this.recordID = 0;
        this.tableID = 0;
        var $self = this;
        var $root = null;
        var dropContainer = null;
        var uploadFile = null;
        var folTreeCancel = null;
        var folTreeUpload = null;
        var folTreeSelect = null;
        var nxtBtn = null;
        var dragDiv = null;
        var paramDiv = null;
        var paramFooter = null;
        var folderFader = null;
        var infoBoxFader = null;
        var infoBoxIcon = null;
        var infoMessage = null;
        var infoBoxCross = null;
        var folTreeArea = null;
        var loadingFader = null;
        var selectedFileName = null;
        var OpenScanAppWG = null;
        var OpenDMSWG = null;
        var ShowUploadedDocs = null;
        var refreshDoc = null;
        var wddoclisting = null;
        var allowedFileLength = 10;
        var wdSearchDocTxt = null;
        var wdSearchDocBtn = null;
        var folderIds = "";
        var strFolderIds = "";
        var _scrollpage = 1;
        var _pageSize = 8;
        var _selectedPageNo = 0;
        var _recordCount = 0;
        var _isFromSearch = false;
        var _searchDocName = '';
        var _docsort = null;
        var _drivedocsortarrowmain = null;
        var sortorder = 'UD';
        var Dialog = null;
        var bankAccount_ID = null;
        var VA012wdbusydiv = $('<div class="vis-apanel-busy vis-height-full">').css({ 'width': 'calc(100% - 40px)' }).hide();
        var _cmbBank = null;
        var _cmbBankAccount = null;
        var _cmbBankAccountClasses = null;
        var C_BANK_ID = 0
        var C_BANKACCOUNT_ID = 0
        // declare varribale for Statement Date
        var _statementDate = null;
        var _statementName = null;
        var isChecked = null;
        var $loadParaDiv = null;
        var _file = null;
        var lstLatestFiles = [];
        var oldFiles = [];
        var currentchunk = 0;
        var currentFile = 0;
        var folder = Date.now().toString();
        var chunkSize = 1 * 1024 * 1024;
        var totalChunks = 0;
        var currentFileChunkNo = 0;
        var filesInfo = [];
        var _currencyId = null;
        var _selectedFiles = null;
        var norecord = $('<div class="VA012-emptyFolderWrap VA012-wd-emptydocWrap_' + $self.windowNo + '">' +
            '<div class="VA012-driveEmptyFolder">' +
            '<img src="' + VIS.Application.contextUrl + 'Areas/VA012/Images/files-icon.png" alt="icon">' +
            '<h5></h5>' +
            '<small></small>' +
            '</div>' +
            '</div>');

        // init log class
        this.log = VIS.Logging.VLogger.getVLogger('BankJournalWidget');

        //Privilized function
        this.getRoot = function () {

            $root = $("<div id='WidMainRoot_" + $self.windowNo + "' class='VA012_root'></div>");
            $root.append(dropContainer);
            //$root.append(VA012wdbusydiv);

            return $root;
        };

        this.setBusy = function (busy) {

            if (busy)
                VA012wdbusydiv.show();
            else
                VA012wdbusydiv.hide();
        };

        // Initialize the controls, Main function
        this.initialize = function () {
            //widgetID = this.widgetInfo.AD_UserHomeWidgetID;
            //if (widgetID == 0) {
            //    widgetID = $$self.windowNo;
            //}
            Design();
        };

        function Design() {
            /*docDataDesign = $(
                '<div class="VA012-uploadeddoc">' + //style="width: 600px;"
                '<div class="VA012-doc-header">' +
                '<h1>' + VIS.Msg.getMsg('VA012_UploadedDocuments') + ' (<span class="VA012-wddoclistingtotalrec_' + $self.windowNo + '"></span>) <i class="fa fa-refresh VA012-wdrefreshdoc_' + $self.windowNo + '" aria-hidden="true" title="' + VIS.Msg.getMsg('VA012_Refresh') + '"></i></h1>' +

                '<div class="vis-ad-w-p-tb-search VA012-wd-searchWrap">' +
                '<div class="d-flex align-items-center input-group">' +
                '<input type="text" class="form-control vis-ad-w-p-tb-s-input VA012-wdSearchDocTxt_' + $self.windowNo + '" maxlength="50" placeholder="' + VIS.Msg.getMsg('VA012_Search') + '">' +
                '<i class="vis vis-search VA012-wdSearchDocBtn_' + $self.windowNo + '"></i>' +
                '</div>' +
                '</div>' +

                '<div class="VA012-wd-lastupdated">' +
                '<select class="mr-1 VA012_wd-Sorting_' + $self.windowNo + '">' +
                '<option value="N">' + VIS.Msg.getMsg('VA012_Name') + '</option>' +
                '<option value="U" selected="selected">' + VIS.Msg.getMsg('VA012_Updated') + '</option>' +
                '<option value="C">' + VIS.Msg.getMsg('VA012_Created') + '</option>' +
                '</select>' +
                '<i title="' + VIS.Msg.getMsg('VA012_SortOrder') + '" class="fa fa-sort-amount-desc VA012-wd-drivedocsortarrowmain_' + $self.windowNo + '" aria-hidden="true"></i>' +
                '</div>' +

                '</div>' +
                '<div class="VA012-docList VA012-wddoclisting_' + $self.windowNo + '"></div>' +
                '</div>'
            );
            docDataDesign.append(VA012wdbusydiv);*/
            var showdocNavigator = 'd-none';
            if ($self.isInWindow) {
                showdocNavigator = '';
            }
            dropContainer = $('<div class="VA012-bank-panel VA012-WidgetContainer" data-folderid="0">' +
                '<div class="VA012-fader-div VA012-loadingFader_' + $self.windowNo + ' d-none">' +
                '<div class="VA012-loader-container">' +
                '<div style="height:35px;"><div class="vis_widgetloader m-auto"></div></div>' +
                '<p></p>' +
                '</div>' +
                '</div>' +
                '<div class="VA012-fader-div VA012-folderFader VA012-folderFader_' + $self.windowNo + ' d-none">' +
                '<div class="p-2 VA012-folderContainer VA012-folderContainer_' + $self.windowNo + '">' +
                '<label id="VA012-selectedFileName_' + $self.windowNo + '"></label>' +
                '<div class="VA012-folTreeArea VA012-folTreeArea_' + $self.windowNo + '">' +
                '</div>'
                + '<div class="vadms-folTreeFooter">' +
                + '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Select')
                + '" disabled class="btn ui-button ui-corner-all ui-widget VA012-treeSelectBtn_' + self.windowNo + ' d-none">' +
                '</div>' +
                '</div>' +
                '</div>' +

                '<div class="VA012-fader-div VA012-infoFader_' + $self.windowNo + ' p-5 d-none">' +
                '<div class="VA012-success-msg">' +
                '<span class="vis vis-cross VA012-infoBoxCrossBtn_' + $self.windowNo + '"></span>' +
                '<div class="text-center">' +
                '<i class="VA012-infoBoxIcon_' + $self.windowNo + ' fa fa-check-circle" aria-hidden="true"></i>' +
                '<p class="VA012-InfoMessage_' + $self.windowNo + '"></p>' +
                '</div>' +
                '</div>' +
                '</div>' +

                '<div class="VA012-widgetContentArea" style="height: calc(100% - 0px);">' +
                //'<div class="VA086-panel-heading"><h6>' + VIS.Msg.getMsg('VA012_DMS') + '</h6></div>' +
                '<div class="VA012-shadow bg-white">' +
                '<div class="VA012-upload-col">' +
                '<input id="VA012-uploadFile_' + $self.windowNo + '" class="VA012-input-file-field VA012-uploadFileWidget_' + $self.windowNo
                + '" type="file">' +
                '<label for="VA012-uploadFile_' + $self.windowNo + '" class="VA012-files-label">' +
                '<i class="vis vis-uploaddocument" aria-hidden="true"></i>' +
                '<div class="VA012-labelTxt">' + VIS.Msg.getMsg('VA012_DragFiles') + '<span class="VA012-browse-link">'
                + VIS.Msg.getMsg('VA012_Browse') + '</span> <span class="VA012-fileSize">'
                + VIS.Msg.getMsg('VA012_FileSizeLessThan200MB') + '</span></div>' +
                '</label>' +
                '</div>' +
                '</div>' +

                '<div class="VA012-links mt-3">' +

                '<div class="VA012-scan-folder">' +

                '<div class="VA012-folder-link text-center">' +
                '<i class="fa fa-folder-open VA012-wgOpenDMS_' + $self.windowNo + '" aria-hidden="true"></i>' +
                '<a class="VA012-wgOpenDMS_' + $self.windowNo + '" href="javascript:void(0)">' + VIS.Msg.getMsg('VA012_OpenDMS') + '</a>' +
                '</div>' +
                '<div class="VA012-folder-link text-center">' +
                /*'<a href="#"><i id="VA012_NextArrow_' + $self.windowNo + '" class="fa fa-arrow-circle-right" aria-hidden="true"></i></a>' +
                '<a class="VA012-wgNext_' + $self.windowNo + '" href="javascript:void(0)">' + VIS.Msg.getMsg('VA012_Next') + '</a>' +*/
                '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Next') +
                '" id="VA012_NextArrow_' + $self.windowNo + '" class="btn ui-button ui-corner-all ui-widget VA012-treeNextBtn_' + $self.windowNo + '">' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>');
            uploadFile = dropContainer.find('#VA012-uploadFile_' + $self.windowNo);
            folTreeSelect = dropContainer.find('.VA012-treeSelectBtn_' + $self.windowNo);
            nxtBtn = dropContainer.find('.VA012-treeNextBtn_' + $self.windowNo);
            folderFader = dropContainer.find('.VA012-folderFader_' + $self.windowNo);
            infoBoxFader = dropContainer.find('.VA012-infoFader_' + $self.windowNo);
            infoBoxIcon = dropContainer.find('.VA012-infoBoxIcon_' + $self.windowNo);
            infoMessage = dropContainer.find('.VA012-InfoMessage_' + $self.windowNo);
            infoBoxCross = dropContainer.find('.VA012-infoBoxCrossBtn_' + $self.windowNo);
            folTreeArea = dropContainer.find('.VA012-folTreeArea_' + $self.windowNo);
            loadingFader = dropContainer.find('.VA012-loadingFader_' + $self.windowNo);
            selectedFileName = dropContainer.find('#VA012-selectedFileName_' + $self.windowNo);
            /// OpenScanAppWG = dropContainer.find('.VA012-wgOpenScanApp_' + $self.windowNo);
            // OpenDMSWG = dropContainer.find('.VA012-wgOpenDMS_' + $self.windowNo);
            ShowUploadedDocs = dropContainer.find('.VA012-wgShowUploadedDocs_' + $self.windowNo);
            //wddoclisting = docDataDesign.find('.VA012-wddoclisting_' + $self.windowNo);
            dragDiv = dropContainer.find('.VA012-widgetContentArea');
            nxtBtn.css("display", "none");
            //if (uploadFile.val() == '') {
            //    nxtBtn.hide();
            //}
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
                files = e.originalEvent.dataTransfer.files;
                _selectedFiles = files;
                if (files.length > 0) {
                    UploadingFiles(files);
                }
            });
            // Browse
            dropContainer.find('.VA012-uploadFileWidget_' + $self.windowNo).off('click');
            dropContainer.find('.VA012-uploadFileWidget_' + $self.windowNo).on('click', function (e) {
                this.value = null;
            });
            dropContainer.find('.VA012-uploadFileWidget_' + $self.windowNo).off('change');
            dropContainer.find('.VA012-uploadFileWidget_' + $self.windowNo).on('change', function (e) {
                e.stopPropagation();
                e.preventDefault();
                var files = e.target.files;
                _selectedFiles = files;
                if (files.length > 0) {
                    UploadingFiles(files);
                    $(this).attr('value', '');
                }
            });

            


            nxtBtn.on('click', function (e) {
                //folderFader.addClass('d-none');
                //dropContainer.find('.VA012-uploadFileWidget_' + $self.windowNo).val(null);
                if (paramDiv == null) {
                    loadParam();
                }
                else {
                    _cmbBank.prop('selectedIndex', 0);
                    _cmbBankAccount.prop('selectedIndex', 0);
                    _cmbBankAccountClasses.prop('selectedIndex', 0);
                    _statementDate.val('');
                    _statementName.val('');
                    isChecked.prop("checked", false);
                    paramDiv.show();
                    paramFooter.show();
                }
                dragDiv.hide();
                loadingFader.hide();
                folderFader.hide();
                folTreeArea.hide();
            });
            // Cancel Button InfoBox
            infoBoxCross.off('click');
            infoBoxCross.on('click', function () {
                // Clear InfoBox
                infoBoxIcon.removeClass('fa fa-check-circle').removeClass('fa fa-warning');
                infoMessage.html('');
                infoBoxFader.addClass('d-none');
            });
        }

        function appendFile(sender) {
            var invalidFiles = [];
            for (var i = 0; i < sender.length; i++) {
                _file = sender[i];
                if (_file == undefined) {
                    return;
                }
                if (_file.size <= 0) {
                    lstLatestFiles = [];
                    oldFiles = [];
                    currentchunk = 0;
                    currentFile = 0;
                    folder = null;
                    chunkSize = null;
                    totalChunks = 0;
                    currentFileChunkNo = 0;
                    filesInfo = [];
                    _file = null;
                    VIS.ADialog.info('VADMS_FileSizeShouldGreaterThan0KB');
                    //showProgress(false);
                    return false;
                }

                for (var itm in lstLatestFiles) {
                    if (_file.name == lstLatestFiles[itm].name) {
                        window.setTimeout(function () {
                            //showProgress(false);
                            VIS.ADialog.info('fileAlreadyAttached');
                        }, 20);
                        return;
                    }
                }
                if (oldFiles != null) {
                    for (var itm in oldFiles) {
                        if (_file.name == oldFiles[itm].Name && !oldFiles[itm].IsDeleted) {
                            window.setTimeout(function () {
                                //showProgress(false);
                                VIS.ADialog.info('fileAlreadyAttached');
                            }, 20);
                            return;
                        }
                    }
                }
                var fileInfo = {};
                fileInfo.Name = _file.name;
                fileInfo.Size = _file.size;
                lstLatestFiles.push(_file);
            }
            if (invalidFiles.length > 0) {
                var resString = '';
                for (var item in invalidFiles) {
                    resString += invalidFiles[item].toString() + '\n';
                }
                if (invalidFiles.length == 1) {
                    resString += 'File has';
                }
                else {
                    resString += 'Files have';
                }
                resString += ' invalid name. Please change the file name and try again.';
                showProgress(false);
                VIS.ADialog.info("", true, resString);
            }
        };

        function UploadDataFiles() {
            var fileInfo = null;
            var tcSingleFile = 0;
            var currentChunk = 0;
            for (var itm in lstLatestFiles) {
                tcSingleFile = parseInt(lstLatestFiles[itm].size / chunkSize);
                if (_file.size % chunkSize > 0) {
                    tcSingleFile++;
                }
                totalChunks += tcSingleFile;

                fileInfo = {};
                fileInfo.Name = lstLatestFiles[itm].name;
                fileInfo.Size = lstLatestFiles[itm].size;
                filesInfo.push(fileInfo);
            }
            TransferFile();
        };

        function TransferFile(e) {
            if (lstLatestFiles == null || lstLatestFiles.length == 0) {
                showProgress(false);
                return;
            }
            if (currentFile >= lstLatestFiles.length) {
                return;
            }

            //Transfer files in chuncks
            var xhr = new XMLHttpRequest();
            var fd = new FormData();

            var fileName = lstLatestFiles[currentFile].name.substring(0, lstLatestFiles[currentFile].name.lastIndexOf(".")) + folder + lstLatestFiles[currentFile].name.substring(lstLatestFiles[currentFile].name.lastIndexOf("."))
            if (fileName.indexOf('&')) {     //--Avoid '&' in Excel File Name
                fileName = fileName.replace("&", "");
            }
            if (fileName.indexOf('?')) {     //--Avoid '?' in Excel File Name
                fileName = fileName.replace("?", "");
            }
            if (fileName.indexOf('/')) {     //--Avoid '/' in Excel File Name
                fileName = fileName.replace("/", "");
            }
            if (fileName.indexOf('\\')) {    //--Avoid '\\' in Excel File Name
                fileName = fileName.replace("\\", "");
            }
            savedFileName = fileName;
            fd.append("file", lstLatestFiles[currentFile].slice(currentFileChunkNo * chunkSize, currentFileChunkNo * chunkSize + Number(chunkSize)));
            xhr.open("POST", VIS.Application.contextUrl + "VA012/VA012_Widget/SaveFileinTemp?fileName=" + fileName + "&folderKey=" + folder, false);
            xhr.send(fd);
            currentchunk++;
            currentFileChunkNo++;
            var totalFileChunk = parseInt(lstLatestFiles[currentFile].size / chunkSize);
            if (lstLatestFiles[currentFile].size % chunkSize > 0) {
                totalFileChunk++;
            }

            if (currentFileChunkNo == totalFileChunk) {
                currentFile++;
                currentFileChunkNo = 0;
            }
            if (currentchunk <= totalChunks) {
                //setProgressValue(parseInt((currentchunk / totalChunks) * 80));
            }
            window.setTimeout(function () {
                TransferFile();
            }, 2);
        };
        /**
         * Uploading files to the DMS
         * @param {any} files
         */
        function UploadingFiles(files) {
            // Check files count            
            if (files.length > allowedFileLength) {

                infoBoxIcon.addClass('fa fa-info-circle').css('color', '#007bff');
                infoMessage.append(VIS.Msg.getMsg('VA012_FileCountExceed'));
                infoBoxFader.removeClass('d-none');
                return;
            }
            var fd = new FormData();
            var allowedExtensions = ".xlsx,.xls,.csv";
            var isExceded = false;
            var excededfileNames = '';
            var extNotAllowed = false;
            var invalidExtensions = [];
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

                // Check file size
                if (isExceded) {

                    infoBoxIcon.addClass('fa fa-info-circle').css('color', '#007bff');
                    infoMessage.append(excededfileNames.substring(0, excededfileNames.length - 1));
                    infoMessage.append('<span>' + VIS.Msg.getMsg('VA012_FileSizeShouldLessThan200MB') + '</span>');
                    infoBoxFader.removeClass('d-none');

                    return;
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

                    infoBoxIcon.addClass('fa fa-warning').css('color', '#E9D502');
                    infoMessage.append(VIS.Msg.getMsg(resString) + " " + fileNames.substring(0, fileNames.length - 2));
                    infoBoxFader.removeClass('d-none');

                    return;
                }
                if (files.length > 0) {

                    appendFile(files);
                    UploadDataFiles();
                    if (paramDiv == null) {
                        loadParam();
                    }
                    else {
                        _cmbBank.prop('selectedIndex', 0);
                        _cmbBankAccount.prop('selectedIndex', 0);
                        _cmbBankAccountClasses.prop('selectedIndex', 0);
                        _statementDate.val('');
                        _statementName.val('');
                        isChecked.prop("checked", false);
                        paramDiv.show();
                        paramFooter.show();
                    }
                    dragDiv.hide();
                    loadingFader.hide();
                    folderFader.hide();
                    folTreeArea.hide();
                }
            }
        }
        function loadParam() {
            $loadParaDiv = $('<div class="VA012_paramMainDiv" id="VA012_paramMainDiv_' + $self.windowNo + '"><div class= "va012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<select id="VA012_STAT_cmbBank_' + $self.windowNo + '">'
                + '</select>'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_Bank") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class= "va012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<select id="VA012_STAT_cmbBankAccount_' + $self.windowNo + '">'
                + '</select>'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_BankAccount") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div > '
                + '<div class=va012-form-data>' + '<div class="input-group vis-input-wrap VA012-paramdiv VA012-margin-B0">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<input type="date" max="9999-12-31" id=VA012_STAT_statementDate_' + $self.windowNo + '>'
                + '<label class="VA012-labels" id="VA012_STAT_lblStatementDate_' + $self.windowNo + '">' + VIS.Msg.getMsg("VA012_StatementDate") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="va012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<select id="VA012_STAT_cmbBankAccountClassName_' + $self.windowNo + '">'
                + '</select>'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_ClassName") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="va012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<input type="text" id="VA012_STAT_txtStatementNo_' + $self.windowNo + '" placeholder=" " data-placeholder="">'
                + '<label class="VA012-labels">' + VIS.Msg.getMsg("VA012_StatementNumber") + '<sup style="color: red;">*</sup></label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="va012-form-data">'
                + '<div class="input-group vis-input-wrap VA012-paramdiv">'
                + '<div class="vis-control-wrap VA012-controls">'
                + '<input type="checkbox" value="checked" id="VA012_CheckBox_' + $self.windowNo + '">'
                + '<label class="VA012-labels VA012-StatementDateAsAccountDate">' + VIS.Msg.getMsg("VA012_StatementDateAsAccountDate") + '</label>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '</div>'
                + '<div class="VA012-folTreeFooter" id="VA012-folTreeFooter_' + $self.windowNo + '">'
                + '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Cancel') + '" class="btn ui-button ui-corner-all ui-widget VA012-treeCancelBtn_' + $self.windowNo + '">'
                + '<input type="submit" value="' + VIS.Msg.getMsg('VA012_Upload') + '" class="btn ui-button ui-corner-all ui-widget VA012-treeUploadBtn_' + $self.windowNo + '">'

                + '</div>'
                + '</div>');
            dropContainer.append($loadParaDiv);

            getControls();
            loadFunctions.loadBank();
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
                    loadFunctions.loadCurrency90;
                    loadBankAccountClasses();
                }
                else {
                    return "VA012_NoBankAccountSelected";
                }
            });
            /// folTreeCancel.off('click');
            folTreeCancel.on('click', function (e) {
                //folderFader.addClass('d-none');
                _cmbBank.prop('selectedIndex', 0);
                _cmbBankAccount.prop('selectedIndex', 0);
                _cmbBankAccountClasses.prop('selectedIndex', 0);
                _statementDate.val('');
                _statementName.val('');
                isChecked.prop("checked", false);
                paramDiv.hide();
                paramFooter.hide();
                dragDiv.show();
                dropContainer.find('.VA012-uploadFileWidget_' + $self.windowNo).val(null);
            });

            folTreeUpload.on('click', function (e) {
                //Load Bank Statement From File Selected
                if (_cmbBank.val() == null || _cmbBank.val() == "" || _cmbBank.val() == "0") {
                    VIS.ADialog.info("VA012_SelectBankFirst", null, "", "");
                    return false;
                }
                if (_cmbBankAccount.val() == null || _cmbBankAccount.val() == "" || _cmbBankAccount.val() == "0") {
                    VIS.ADialog.info("VA012_SelectBankAccountFirst", null, "", "");
                    return false;
                }

                if (_cmbBankAccountClasses.val() == null || _cmbBankAccountClasses.val() == "" || _cmbBankAccountClasses.val() == "0") {
                    VIS.ADialog.info("VA012_PleaseSelectClassFirst", null, "", "");
                    return false;
                }
                if (_statementName.val() == null || _statementName.val() == "") {
                    VIS.ADialog.info("VA012_PleaseEnterStatementNo", null, "", "");
                    return false;
                }
                //Bank Statement From File Path Scheduled

                if (_selectedFiles.length > 0) {
                    var filename = _selectedFiles;
                    if (filename == null || filename == "" || filename._path == null || filename._path == "") {

                        VIS.ADialog.info("VA012_ErrorInGettingFile", null, "", "");
                        return;
                    }
                    else if (filename._error != null && filename._error != "") {
                        VIS.ADialog.info(filename._error, null, "", "");
                        return;
                    }
                    else {
                        var _path = filename._path;
                        var _filename = filename._filename;
                        var _bankaccount = _cmbBankAccount.val();
                        var _statementno = _statementName.val();
                        var $_statement = $statement;
                        var _IsStatementDateAsAccountDate = isChecked;
                        var _statementClassName = _cmbBankAccountClasses.val();
                        // var _statementCharges = Bank_Charge_ID;// STAT_cmbBankAccountCharges.val();
                        VA012wdbusydiv.show();
                        window.setTimeout(function () {
                            $.ajax({
                                url: VIS.Application.contextUrl + "BankStatement/ImportStatement",
                                type: "GET",
                                datatype: "json",
                                contentType: "application/json; charset=utf-8",
                                async: false,
                                data: ({
                                    _path: _path, _filename: _filename, _bankaccount: _bankaccount, _bankAccountCurrency: _currencyId, _statementno: _statementno,
                                    _statementClassName: _statementClassName, _statementCharges: _statementCharges, statementDate: STAT_statementDate.val(),
                                    IsStatementDateAsAccountDate: _IsStatementDateAsAccountDate
                                }),
                                success: function (result) {


                                    _statementID = result._statementID;
                                    if (_statementID != null && _statementID != "") {
                                        //_statementLinesList = [];
                                        ///_lstStatement.html("");
                                        //_statementPageNo = 1;
                                        //busyIndicator($_statement, false, "absolute");
                                        //VA012wdbusydiv.hide();
                                        ///childDialogs.loadStatement(_statementID);
                                        //newRecordForm.refreshForm();
                                        return "statement uploaded succesfuly";


                                    }
                                    else {
                                        if (result._error != null && result._error != "") {
                                            //busyIndicator($_statement, false, "absolute");
                                            VA012wdbusydiv.hide();
                                            VIS.ADialog.info(result._error, null, "", "");

                                        }
                                    }
                                },
                                error: function () {

                                    //busyIndicator($_statement, false, "absolute");
                                    VA012wdbusydiv.hide();
                                    VIS.ADialog.info("error", null, "", "");

                                }
                            })
                        }, 2);
                    }
                }
            });
            // Loading effect
            loadingFader.find('p').html('').append(VIS.Msg.getMsg('VA012_PleaseWait') + '<span>' + VIS.Msg.getMsg('VA012_LoadingFiles') + '</span>');
            loadingFader.removeClass('d-none');
            window.setTimeout(function () {
                // Show folder tree
                folderFader.removeClass('d-none');
                //Neha  folTreeArea.html('').append(WidgetFolderTreeStruct());
                folTreeUpload.removeClass('d-none');
                folTreeSelect.addClass('d-none');
                loadingFader.addClass('d-none');
            }, 500);

            // Tree upload button
            /*folTreeUpload.off('click');
            folTreeUpload.on('click', function (e) {
                var selectedFolder = folTreeArea.find('.VA012-selected').closest('li');
                dropContainer.attr('data-folderid', selectedFolder.attr('id'));
                folderFader.addClass('d-none');
                // Loading effect
                loadingFader.find('p').html('').append(VIS.Msg.getMsg('VA012_PleaseWait') + '<span>'
                    + VIS.Msg.getMsg('VA012_UploadingFiles') + '</span>');
                loadingFader.removeClass('d-none');
                fd.append('folderID', selectedFolder.attr('id'));
                fd.append('windowID', $self.windowID);
            });*/
            // Show uploaded docs
            ShowUploadedDocs.off('click');
            ShowUploadedDocs.on('click', function () {

                loadingFader.find('p').html('');
                loadingFader.removeClass('d-none');

                var orderByColumn = [];
                orderByColumn.push('Updated');

                _scrollpage = 1;
                _searchDocName = '';

                // LoadDocuments(orderByColumn, VADMS.Common.UpdatedDesc, _scrollpage, false);
            });
        };


        /**Get Bank Account detail like Currency,precision based on bank selected */
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
                        // _cmbBank.trigger('change');
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
                            //VIS_427 DevOpsID:4207 Added value of bank Account Type in attribute
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
                                C_BANKACCOUNT_ID = data.Table[0].C_BANKACCOUNT_ID;

                                //_cmbBank.val(data.Table[0].C_BANK_ID).prop('selected', true);
                                //_cmbBankAccount.val(data.Table[0].C_BANKACCOUNT_ID).prop('selected', true);
                            }
                        }
                        loadFunctions.loadBankAccount();
                    },
                });
            },
            loadCurrency: function () {
                //VA230:Set selected bankaccount currencyid and stdprecision
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
            }
        };

        function getControls() {
            _cmbBank = $loadParaDiv.find("#VA012_STAT_cmbBank_" + $self.windowNo);
            _cmbBankAccount = $loadParaDiv.find("#VA012_STAT_cmbBankAccount_" + $self.windowNo);
            _cmbBankAccountClasses = $loadParaDiv.find("#VA012_STAT_cmbBankAccountClassName_" + $self.windowNo);
            paramDiv = dropContainer.find('#VA012_paramMainDiv_' + $self.windowNo);
            folTreeCancel = $loadParaDiv.find('.VA012-treeCancelBtn_' + $self.windowNo);
            folTreeUpload = $loadParaDiv.find('.VA012-treeUploadBtn_' + $self.windowNo);
            //folTreeSelect = dropContainer.find('.VA012-treeSelectBtn_' + self.windowNo);
            paramFooter = dropContainer.find('#VA012-folTreeFooter_' + $self.windowNo);
            _statementDate = $loadParaDiv.find('#VA012_STAT_statementDate_' + $self.windowNo);
            _statementName = $loadParaDiv.find('#VA012_STAT_txtStatementNo_' + $self.windowNo);
            isChecked = $loadParaDiv.find('#VA012_CheckBox_' + $self.windowNo);
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
        /*this function is used to refresh design and data of widget*/
        this.refreshWidget = function () {
            this.initialize();
        };
        this.disposeComponents = function () {
            $self = null;
            $root = null;
            this.frame = null;
            this.windowNo = 0;
            this.isInWindow = null;
            this.widgetInfo = null;
            this.recordID = 0;
            this.tableID = 0;
            dropContainer = null;
            uploadFile = null;
            folTreeCancel = null;
            folTreeUpload = null;
            folTreeSelect = null;
            nxtBtn = null;
            dragDiv = null;
            paramDiv = null;
            paramFooter = null;
            folderFader = null;
            infoBoxFader = null;
            infoBoxIcon = null;
            infoMessage = null;
            infoBoxCross = null;
            folTreeArea = null;
            loadingFader = null;
            selectedFileName = null;
            OpenScanAppWG = null;
            OpenDMSWG = null;
            ShowUploadedDocs = null;
            refreshDoc = null;
            wddoclisting = null;
            allowedFileLength = 10;
            wdSearchDocTxt = null;
            wdSearchDocBtn = null;
            folderIds = "";
            strFolderIds = "";
            _scrollpage = 1;
            _pageSize = 8;
            _selectedPageNo = 0;
            _recordCount = 0;
            _isFromSearch = false;
            _searchDocName = '';
            _docsort = null;
            _drivedocsortarrowmain = null;
            sortorder = 'UD';
            Dialog = null;
            bankAccount_ID = null;
            docDataDesign = null;
            ajaxReqLoadDoc = null;
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
        };
    };

    // Must Implement with same parameter
    VA012.VA012_BankingJournal.prototype.init = function (windowNo, frame) {

        //Assign to this Variable
        this.frame = frame;
        this.windowNo = windowNo; // VIS.Env.getWindowNo();
        this.windowID = VIS.context.getWindowContext(windowNo, 'AD_Window_ID');
        this.isInWindow = true;
        if (windowNo == null || windowNo == undefined || windowNo <= 0) {

            this.isInWindow = false;
            this.windowID = 0;
        }
        // Widget info, we can save additional information in widget record
        this.widgetInfo = frame.widgetInfo;
        this.windowName = VIS.context.getWindowContext(windowNo, 'WindowName'); // 'User Management'; //
        this.recordID = 0;
        this.tableID = 0;
        ////frame.hideHeader(true);
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
    }

    VA012.VA012_BankingJournal.prototype.widgetFirevalueChanged = function (value) {

        //this.getRoot().trigger('widgetFirevalueChanged', value); // Trigger custom event with the value

        if (this.listener)
            this.listener.widgetFirevalueChanged(value);
    };


})(VA012, jQuery);