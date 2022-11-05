/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
(function () {
    //#region reportInformation
    var reportInformation = {
        recordsPerPage: 0,
        totalPages: 0,
        saved: false,
        name: '',
        token: '',
        table: {},
        filterObject: {},
        sortingObject: {},
        headers: [],
        availableHeaders: [],
        update: function (table) {
            this.totalPages = table.TotalPages;
            this.recordsPerPage = table.RecordsPerPage;
            this.name = table.Name;
            this.table = table;
        }
    };
    //#endregion
    var height = $(window).innerHeight();
    height *= 0.72;
    $('.modal-fill .modal-body').css('max-height', height + 'px');
    $(window).on('resize', function () {
        var t_height = $(window).innerHeight();
        t_height *= 0.72;
        $('.modal-fill .modal-body').css('max-height', t_height + 'px');
    });
    prettyProcessing.showPleaseWait('Performing Initial Load', 'Please be patient', 100);
    loadInitPage();
    $('#saveEdit').on('click', function () {
        // Here we make the ajax call to save the data
        $('#editModal').find('.editingData').hide('fast');
        $('#editModal').find('#editingProgress').show('fast');
        // Get the data to send.
        var waitlisting = [];
        var w_index = 0;
        while ($('input[name^="Waitlistings[' + w_index + ']"]').length > 0) {
            var json = {};
            var items = $('input[name^="Waitlistings[' + w_index + ']"]');
            for (var i = 0; i < items.length; i++) {
                var item = $(items[i]);
                if (/Key$/i.test(item.attr('name'))) {
                    json.Key = item.val();
                } else if (item.attr('type') === 'checkbox') {
                    if (item.prop('checked')) {
                        json.Value = true;
                    } else {
                        json.Value = false;
                    }
                }
            }
            w_index++;
            waitlisting.push(json);
        }
        var editingId = $('#editingComp').val();
        var editingReg = $('#editingReg').val();
        var data = { componentId: editingId, registrantId: editingReg, value: $('#editModal').find('[id="' + editingId + '"]').val(), waitlistings: waitlisting };
        // Add the antiforgery token.
        toolkit.addJsonAntiForgeryToken(data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/RegistrantData', true);
        RESTFUL.jsonHeader(t_xhr);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        t_xhr.onload = function (event) {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    $('#' + result.registrantId).find('td[data-headerid="' + result.headerId + '"]').html(result.value);
                    $('#editModal').modal('hide');
                } else {
                    $('#editModal .modal-body').find('#editingProgress').hide('fast');
                    $('#editModal .modal-body').find('#editingData').show('fast');
                    var message = '';
                    for (var l = 0; l < result.errors.length; l++) {
                        if (l !== 0) {
                            message += '<br />';
                        }
                        message += result.errors[l];
                    }
                    $('#editModal').find('.form-messagebox').html(message).show('fast');
                }
            } else {
                $('#editModal').modal('hide');
                RESTFUL.showError('Unhandled Server Error');
            }
        };
        t_xhr.send(JSON.stringify(data));
        t_xhr = null;
        data = null;
    });
    function updateSortings() {
        prettyProcessing.showPleaseWait('Updating Sortings');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Cloud/PagedReportSortings', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            reportInformation.update(result.table);
            reportInformation.saved = true;
            result = null;
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, sortings: reportInformation.sortingObject.GetSortings() })));
        xhr = null;
    }
    function updateFields() {
        prettyProcessing.showPleaseWait('Updating Headers');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Cloud/PagedReportFields', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            reportInformation.update(result.table);
            reportInformation.saved = true;
            result = null;
        };
        var headers = [];
        $('#selectedHeaders').find('.header').each(function () {
            headers.push($(this).attr('data-id'));
        });
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, headers: headers })));
        xhr = null;
    }
    function updateFilters() {
        prettyProcessing.showPleaseWait('Updating Filters');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Cloud/PagedReportFilters', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            reportInformation.update(result.table);
            reportInformation.saved = true;
            result = null;
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, filters: reportInformation.filterObject.GenerateJsonFilters() })));
        xhr = null;
    }
    function updateRecordsPerPage() {
        prettyProcessing.showPleaseWait('Updating Records Per Page');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Cloud/PagedReportRecordsPerPage', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            reportInformation.update(result.table);
            reportInformation.saved = true;
            result = null;
        };
        var recordsPerPage = reportInformation.recordsPerPage;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, recordsPerPage: recordsPerPage })));
        xhr = null;
    }
    function loadPage(page) {
        prettyProcessing.showPleaseWait('Loading Page ' + page);
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Cloud/PagedReport/' + reportInformation.token + '/' + page, true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            reportInformation.recordsPerPage = result.table.RecordsPerPage;
            reportInformation.totalPages = result.table.TotalPages;
            reportInformation.name = result.table.Name;
            reportInformation.saved = true;
            reportInformation.table = result.table;
            result = null;
            $('#pageSize').val(reportInformation.recordsPerPage);
            $('.pagedReport_recordsReturned').val(reportInformation.recordsPerPage);
            updateInputs();
        };
        xhr.send();
        xhr = null;
    }
    function loadInitPage() {
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/Cloud/PagedReport', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            reportInformation.update(result.table);
            reportInformation.saved = true;
            reportInformation.token = result.table.Token;
            reportInformation.filterObject = new JsonFilters(result.table.Filters, result.headers, 'filterModal', false);
            reportInformation.sortingObject = new JsonSortings(result.table.Sortings, result.headers, 'sortingModal', false);
            reportInformation.filterObject.Generate();
            reportInformation.sortingObject.Generate();
            reportInformation.headers = result.headers;
            generateHeaders();
            result = null;
            reportInformation.sortingObject.Modal.find('.set-sortings').on('click', function () {
                reportInformation.sortingObject.Modal.modal('hide');
                updateSortings();
            });
            $('#filters_clearall').on('click', function (e) {
                e.preventDefault();
                reportInformation.filterObject.Clear();
                updateFilters();
            });
            reportInformation.filterObject.Modal.find('.set-filters').on('click', function () {
                $('#filterModal').modal('hide');
                updateFilters();
            });
            $('#pageSize').on('blur', function () {
                reportInformation.recordsPerPage = $(this).val();
                updateRecordsPerPage();
            });
            $('#pageSelect').on('change', function () {
                loadPage(parseInt($(this).val()));
            });
            $('#filters_edit').on('click', function (e) {
                e.preventDefault();
                reportInformation.filterObject.Modal.modal('show');
            });
            $('#sortings_edit').on('click', function (e) {
                e.preventDefault();
                reportInformation.sortingObject.Modal.modal('show');
            });
            $('#sortings_clearall').on('click', function (e) {
                e.preventDefault();
                reportInformation.sortingObject.Clear();
                updateSortings();
            });
            $('#formType').on('change', function (e) {
                handleQuickFilter(e, $(this));
            });
            if (!reportInformation.table.SavedReport) {
                $('#file_save,#file_delete,#options_favorite,#file_permissions').css('color', '#ccc');
            }
            $('#m_save_button').on('click', function () {
                $('#m_save').modal('hide');
                var t_id = $('#m_save_fileInputId').val();
                var t_name = $('#m_save_fileInput').val();
                $('#m_save_fileInputId').val('');
                $('#m_save_fileInput').val('');
                if (typeof (t_name) === 'undefined' || t_name === null || t_name === '') {
                    return;
                }
                prettyProcessing.showPleaseWait('Saving Registrant Data', 'Please Wait', 100);
                var t_xhr = new XMLHttpRequest();
                t_xhr.open('post', window.location.origin + '/Cloud/PagedReportSaveAs');
                RESTFUL.jsonHeader(t_xhr);
                t_xhr.onerror = function (event) {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.xhrError(event);
                };
                t_xhr.onload = function (event) {
                    var c_xhr = event.currentTarget;
                    if (c_xhr.status === 200) {
                        var result = RESTFUL.parse(c_xhr);
                        if (result.success) {
                            $('#file_save,#file_delete,#options_favorite,#file_permissions').css('color', '');
                            reportInformation.table.SavedReport = true;
                            reportInformation.table.Name = result.name;
                            reportInformation.table.SavedReportId = result.id;
                            $('.jTable_standardOnly').hide();
                            updateInputs();
                            prettyProcessing.hidePleaseWait();
                        } else {
                            prettyProcessing.hidePleaseWait();
                            RESTFUL.showError(result.message);
                        }
                    } else {
                        prettyProcessing.hidePleaseWait();
                        RESTFUL.showError();
                    }
                };
                t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, name: t_name, saveId: t_id })));
            });
            $('#file_saveas').on('click', function () {
                showFileModal();
            });
            $('#file_save').on('click', function (e) {
                e.preventDefault();
                if (!reportInformation.table.SavedReport) {
                    return;
                }
                prettyProcessing.showPleaseWait('Saving Report', 'Please Wait', 100);
                var t_xhr = new XMLHttpRequest();
                t_xhr.open('put', window.location.origin + '/Cloud/PagedReportSave');
                RESTFUL.jsonHeader(t_xhr);
                t_xhr.onerror = function (event) {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.xhrError(event);
                };
                t_xhr.onload = function (event) {
                    var c_xhr = event.currentTarget;
                    if (c_xhr.status === 200) {
                        var result = RESTFUL.parse(c_xhr);
                        if (result.success) {
                            $('#file_save,#file_delete,#options_favorite').css('color', '');
                            prettyProcessing.hidePleaseWait();
                        } else {
                            prettyProcessing.hidePleaseWait();
                            RESTFUL.showError();
                        }
                    } else {
                        prettyProcessing.hidePleaseWait();
                        RESTFUL.showError();
                    }
                };
                t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token })));
                t_xhr = null;
            });
            $('#file_load').on('click', function (e) {
                e.preventDefault();
                showFileModal(true);
            });
            $('#file_permissions').on('click', function (e) {
                e.preventDefault();
                if (!reportInformation.table.SavedReport) {
                    return;
                }
                window.location.replace(window.location.origin + '/Cloud/PagedReportPermissions/' + reportInformation.token);
            });
            $('#file_delete').on('click', function (e) {
                e.preventDefault();
                if (!reportInformation.table.SavedReport) {
                    return;
                }
                prettyProcessing.showPleaseWait('Deleting Report', 'Please Wait', 100);
                var t_xhr = new XMLHttpRequest();
                t_xhr.open('delete', window.location.origin + '/Cloud/PagedReportDelete');
                RESTFUL.jsonHeader(t_xhr);
                t_xhr.onerror = function (event) {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.xhrError(event);
                };
                t_xhr.onload = function (event) {
                    var c_xhr = event.currentTarget;
                    if (c_xhr.status === 200) {
                        var result = RESTFUL.parse(c_xhr);
                        if (result.success) {
                            //window.location.replace(window.location.origin + '/Cloud/PagedReport/' + reportInformation.table);
                        } else {
                            prettyProcessing.hidePleaseWait();
                            RESTFUL.showError();
                        }
                    } else {
                        prettyProcessing.hidePleaseWait();
                        RESTFUL.showError();
                    }
                };
                t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token })));
                t_xhr = null;
            });
            $('#options_favorite').on('click', function (e) {
                e.preventDefault();
                if (!reportInformation.table.SavedReport) {
                    return;
                }
                prettyProcessing.showPleaseWait('Favoriting Report', 'Please Wait', 100);
                var t_xhr = new XMLHttpRequest();
                t_xhr.open('put', window.location.origin + '/Cloud/PagedReportFavorite');
                RESTFUL.jsonHeader(t_xhr);
                RESTFUL.ajaxHeader(t_xhr);
                t_xhr.onerror = function (event) {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.xhrError(event);
                };
                t_xhr.onload = function (event) {
                    var c_xhr = event.currentTarget;
                    if (c_xhr.status === 200) {
                        var result = RESTFUL.parse(c_xhr);
                        if (result.success) {
                            if (result.favorite) {
                                $('#favoriteStar').removeClass('glyphicon-star-empty').addClass('glyphicon-star');
                                $('#favoriteText').text('Un-Favorite')
                            } else {
                                $('#favoriteStar').removeClass('glyphicon-star').addClass('glyphicon-star-empty');
                                $('#favoriteText').text('Favorite')
                            }
                            prettyProcessing.hidePleaseWait();
                        } else {
                            prettyProcessing.hidePleaseWait();
                            RESTFUL.showError();
                        }
                    } else {
                        prettyProcessing.hidePleaseWait();
                        RESTFUL.showError();
                    }
                };
                t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token })));
                t_xhr = null;
            });
            if (reportInformation.table.Favorite) {
                $('#favoriteStar').removeClass('glyphicon-star-empty').addClass('glyphicon-star');
                $('#favoriteText').text('Un-Favorite')
            } else {
                $('#favoriteStar').removeClass('glyphicon-star').addClass('glyphicon-star-empty');
                $('#favoriteText').text('Favorite')
            }
            $('#downloadReport').on('click', function () {
                //var newTable = new JTable();
                var dxhr = new XMLHttpRequest();
                dxhr.open('post', window.location.origin + '/Cloud/CreateFormReport');
                RESTFUL.jsonHeader(dxhr);
                RESTFUL.ajaxHeader(dxhr);
                dxhr.onerror = function () {
                    RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
                    prettyProcessing.hidePleaseWait();
                };
                dxhr.onload = function (event) {
                    if (this.status === 200) {
                        var result = RESTFUL.parse(this);
                        if (result.Success) {
                            prettyProcessing.hidePleaseWait();
                            window.location = window.location.origin + '/FormReport/Download/' + result.Id;
                        } else {
                            RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                            prettyProcessing.hidePleaseWait();
                        }
                    } else {
                        RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                        prettyProcessing.hidePleaseWait();
                    }
                };
                prettyProcessing.showPleaseWait('Creating Excel File', 'please wait');
                dxhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ token: reportInformation.token })));
                dxhr = null;
            });
        };
        xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ id: report_id })));
        xhr = null;
    }
    function showFileModal(load) {
        if (typeof (load) === 'undefined' || load === null) {
            load = false;
        }
        $('#m_save_files').html('Loading Files');
        prettyProcessing.showPleaseWait('Retrieving Saved Reports', 'Please Wait');
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('get', window.location.origin + '/Cloud/ReportDatas/' + reportInformation.token);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.success) {
                    prettyProcessing.hidePleaseWait();
                    var saveDiv = $('#m_save_files');
                    saveDiv.html('');
                    for (var i = 0; i < result.files.length; i++) {
                        var t_file = result.files[i];
                        saveDiv.append('<div class="report-file" data-id="' + t_file.id + '">' + t_file.name + '</div>');
                    }
                    if (!load) {
                        $('.report-file').on('click', function () {
                            $('#m_save_fileInputId').val($(this).attr('data-id'));
                            $('#m_save_fileInput').val($(this).html());
                        });
                        $('#m_save_fileInput').parent().show();
                    } else {
                        $('.report-file').on('click', function () {
                            var selectedReport = $(this).attr('data-id');
                            var newLocation = window.location.origin + '/Cloud/PagedReport/' + selectedReport;
                            window.location.replace(newLocation);
                            $('m_save').modal('hide');
                        });
                        $('#m_save_fileInput').parent().hide();
                    }
                    $('#m_save').modal('show');
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError(result.message);
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        t_xhr.send();
        t_xhr = null;
    }
    function updateTable(table) {
        reportInformation.table = table;
        updateInputs();
        var html = '<table id="pagedTable" class="table table-striped table-registration-data"><thead><tr>';
        for (var headerIndex = 0; headerIndex < table.Headers.length; headerIndex++) {
            html += '<th>' + table.Headers[headerIndex].Value + '</th>';
        }
        html += '</thead><tbody>';
        for (var rowIndex = 0; rowIndex < table.Rows.length; rowIndex++) {
            var t_row = table.Rows[rowIndex];
            html += '<tr id="' + t_row.Id + '" data-rowId="' + t_row.Id + '">';
            for (var valIndex = 0; valIndex < t_row.Values.length; valIndex++) {
                t_value = t_row.Values[valIndex];
                html += '<td' + (t_value.Editable ? ' class="data-editable cursor-pointer"' : '') + ' data-id="' + t_value.Id + '" data-headerId="' + t_value.HeaderId + '">';
                if (t_value.HeaderId.toLowerCase() === 'confirmation') {
                    html += '<a href="' + window.location.origin + '/Registrant/Get/' + t_row.Id + '?popout=true" class="registrant-confirmation">';
                }
                html += t_value.Value;
                if (t_value.HeaderId.toLowerCase() === 'confirmation') {
                    html += '</a>';
                }
                html += '</td>';
            }
            html += '</tr>';
        }
        html += '</tbody></table>';
        $('#pagedReportTableHolder').html(html);
        html = null;
        $('.registrant-confirmation').on('click', function (e) {
            e.preventDefault();
            var regWindow = window.open(this.href, '_blank', 'toolbar=now,location=no,menubar=no,scrollbar=yes,resizable=yes,width=1000,height=720,top=50,left=50');
            regWindow.onunload = function (e) {
                checkForUpdates();
            };
        });
        clickToEdit();
        prettyProcessing.hidePleaseWait();
    }
    function checkForUpdates() {
        return;
    }
    function updateInputs() {
        $('#recordsReturned').html(reportInformation.table.TotalRecords);
        var htmlOptions = '';
        for (var optInd = 0; optInd < reportInformation.table.TotalPages; optInd++) {
            htmlOptions += '<option value="' + (optInd + 1) + '"' + ((optInd + 1 === reportInformation.table.Page) ? ' selected="selected"' : '') + '>' + (optInd + 1) + '</option>';
        }
        $('#pageSelect').html(htmlOptions);
        $('#pageSize').val(reportInformation.table.RecordsPerPage);
        $('.pagedReport_recordsReturned').val(reportInformation.table.RecordsPerPage);
        $('.pagedReport_name').text(reportInformation.table.Name);
    }
    function clickToEdit() {
        $('.data-editable').on('click', function () {
            var td = $(this);
            $('#editModal .modal-body').find('#editingProgress').show();
            $('#editModal .modal-body').find('#editingData').hide();
            $('#editModal').modal('show');
            if ($(this).hasClass('file')) {
                setTimeout(function () { showEditable(td); }, 1000);
            } else {
                showEditable(td);
            }
        });
    }
    function showEditable(td) {
        /// <signature>
        /// <summary>Shows an editable item.</summary>
        /// <param name="td" type="jQuery">The td element that was clicked.</param>
        /// </signature>
        var width = $('#editModal .modal-dialog').width() - 2 - 30 - 30;
        var id = td.closest('tr').attr('data-rowId');
        var dataId = td.attr('data-headerId');
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Cloud/RegistrantData/?id=' + id + '&component=' + dataId + '&width=' + width, true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onload = function (event) {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success === true) {
                    $('#editModal').find('#editingData').html(result.html);
                    $('#editModal').find('.editingData').show('fast');
                    $('#editModal').find('#editingProgress').hide('fast');
                    RunBinding();
                }
            }
        };
        xhr.send();
        xhr = null;
    }
    function generateHeaders() {
        /// <signature>
        /// </signature>
        "use strict";
        var height = $(window).innerHeight();
        height *= 0.72;
        $('#headersModal').find('.modal-body').css('max-height', height + 'px');
        var usedHeadersDiv = $('#selectedHeaders');
        var unusedHeadersDiv = $('#availableHeaders');
        for (var uh_index = 0; uh_index < reportInformation.table.Headers.length; uh_index++) {
            var u_header = reportInformation.table.Headers[uh_index];
            usedHeadersDiv.append('<div class="header" data-id="' + u_header.Id + '">' + (u_header.Id !== 'Confirmation' && u_header.Id !== 'confirmation' ? '<input type="checkbox" /> &nbsp;&nbsp;' : '&nbsp;&nbsp;&nbsp;&nbsp;') + u_header.Value + '</div>');
        }
        for (var unh_index = 0; unh_index < reportInformation.headers.length; unh_index++) {
            var un_header = reportInformation.headers[unh_index];
            var used = false;
            for (var uht_index = 0; uht_index < reportInformation.table.Headers.length; uht_index++) {
                if (reportInformation.table.Headers[uht_index].Id.toLowerCase() === un_header.Id.toLowerCase()) {
                    used = true;
                    break;
                }
            }
            if (used) {
                continue;
            }
            unusedHeadersDiv.append('<div class="header" data-id="' + un_header.Id + '"><input type="checkbox" />&nbsp;&nbsp;' + un_header.Label + '</div>');
        }
        $('#headers_edit').on('click', function (e) {
            e.preventDefault();
            $('#headersModal').modal('show');
        });
        $('#headersModal').find('.add-header').on('click', function (e) {
            e.preventDefault();
            $('#availableHeaders').find('.header').each(function () {
                if ($(this).find('input').prop('checked')) {
                    $(this).appendTo($('#selectedHeaders'));
                }
            });
            $('#headersModal').find('.header>input[type="checkbox"]').prop('checked', false);
        });
        $('#headersModal').find('.remove-header').on('click', function (e) {
            e.preventDefault();
            $('#selectedHeaders').find('.header').each(function () {
                if ($(this).find('input').prop('checked')) {
                    $(this).appendTo($('#availableHeaders'));
                }
            });
            $('#headersModal').find('.header>input[type="checkbox"]').prop('checked', false);
        });
        $('#setHeaders').on('click', function (e) {
            $('#headersModal').modal('hide');
            updateFields();
        });
        $('#headers_clearall').on('click', function (e) {
            e.preventDefault();
            $('#selectedHeaders').find('.header[data-id!="Confirmation"]').appendTo('#availableHeaders');
            updateFields();
        });
        $('#headers_addall').on('click', function (e) {
            e.preventDefault();
            $('#availableHeaders').find('.header').appendTo('#selectedHeaders');
            updateFields();
        });
    }
    function handleQuickFilter(e, select) {
        /// <signature>
        /// <param name="e" type="Object">The event object.</param>
        /// <param name="select" type="jQuery">The select option.</param>
        /// </signature>
        e.preventDefault();

        var filter_submitted = new JsonFilter();
        filter_submitted.ActingOn = 'status';
        filter_submitted.Equality = '==';
        filter_submitted.Value = '1';

        var filter = select.val();
        reportInformation.filterObject.Clear();
        reportInformation.filterObject.filters = [];
        switch (filter) {
            case 'active':
                reportInformation.filterObject.filters.push(filter_submitted);
                break;
            case 'canceled':
                var canceledFilter1 = new JsonFilter();
                canceledFilter1.ActingOn = 'status';
                canceledFilter1.Equality = '==';
                canceledFilter1.Value = '2';
                var canceledFilter2 = new JsonFilter();
                canceledFilter2.ActingOn = 'status';
                canceledFilter2.Equality = '==';
                canceledFilter2.Value = '3';
                canceledFilter2.LinkedBy = 'or';
                var canceledFilter3 = new JsonFilter();
                canceledFilter3.ActingOn = 'status';
                canceledFilter3.Equality = '==';
                canceledFilter3.Value = '4';
                canceledFilter3.LinkedBy = 'or';
                reportInformation.filterObject.filters.push(canceledFilter1);
                reportInformation.filterObject.filters.push(canceledFilter2);
                reportInformation.filterObject.filters.push(canceledFilter3);
                break;
            case 'incompletes':
                var incompleteFilter = new JsonFilter();
                incompleteFilter.ActingOn = 'status';
                incompleteFilter.Equality = '==';
                incompleteFilter.Value = '0';
                reportInformation.filterObject.filters.push(incompleteFilter);
                break;
            case 'deleted':
                var deletedFilter = new JsonFilter();
                deletedFilter.ActingOn = 'status';
                deletedFilter.Equality = '==';
                deletedFilter.Value = '5';
                reportInformation.filterObject.filters.push(deletedFilter);
                break;
            case 'unbalanced':
                var unbalFilter = new JsonFilter();
                unbalFilter.ActingOn = 'balance';
                unbalFilter.Equality = '!=';
                unbalFilter.Value = '0';
                reportInformation.filterObject.filters.push(unbalFilter);
                break;
            case 'refund':
                var refFilter = new JsonFilter();
                refFilter.ActingOn = 'balance';
                refFilter.Equality = '<';
                refFilter.Value = '0';
                reportInformation.filterObject.filters.push(refFilter);
                break;
            case 'owed':
                var oweFilter = new JsonFilter();
                oweFilter.ActingOn = 'balance';
                oweFilter.Equality = '>';
                oweFilter.Value = '0';
                reportInformation.filterObject.filters.push(oweFilter);
                break;
            case 'rsvpAccept':
                var rsvpAFilter = new JsonFilter();
                rsvpAFilter.ActingOn = 'rsvp';
                rsvpAFilter.Equality = '==';
                rsvpAFilter.Value = 'true';
                reportInformation.filterObject.filters.push(rsvpAFilter);
                reportInformation.filterObject.filters.push(filter_submitted);
                break;
            case 'rsvpDecline':
                var rsvpDFilter = new JsonFilter();
                rsvpDFilter.ActingOn = 'rsvp';
                rsvpDFilter.Equality = '==';
                rsvpDFilter.Value = 'false';
                reportInformation.filterObject.filters.push(rsvpDFilter);
                reportInformation.filterObject.filters.push(filter_submitted);
                break;
        }
        reportInformation.filterObject.Generate();
        updateFilters();
    }
    function RunBinding() {
        $('.form-component').removeClass('col-sm-6').removeClass('col-md-4').removeClass('col-lg-3').addClass('col-sm-12');
        $('input[data-component-type="datetime"]').datetimepicker();

        /* Checkbox Group */
        $('input[type="hidden"][data-component-type="checkboxgroup"]').each(function () {
            var hidden_input = $(this);
            var value = hidden_input.val();
            if (typeof (value) === 'undefined' || value === '') {
                value = "[]";
            }
            var t_value = JSON.parse(value);
            hidden_input.data('value', t_value);
        });
        $('input[type="checkbox"]').on('change', function () {
            var input = $(this);
            if (/Waitlistings/i.test(input.attr('name'))) {
                return;
            }
            var hidden_input = $('#' + input.attr('data-parent'));
            var t_value = hidden_input.data('value');
            if (input.prop('checked')) {
                var accept = true;
                var t_index = t_value.indexOf(input.val());
                if (t_index === -1) {
                    t_value.push(input.val());
                }

                /* Time Exclusion */
                if (hidden_input.attr('data-component-timeexclusion') === 'True') {
                    var collision = false;
                    for (var i = 0; i < t_value.length; i++) {
                        var t_item = $('#' + t_value[i]);
                        var aStart = moment(t_item.attr('data-agenda-start'));
                        var aEnd = moment(t_item.attr('data-agenda-end'));
                        for (var j = 0; j < t_value.length; j++) {
                            if (t_value[i] === t_value[j]) {
                                continue;
                            }
                            var t_item2 = $('#' + t_value[j]);
                            var bEnd = moment(t_item2.attr('data-agenda-end'));
                            if ((bEnd.isAfter(aStart) || bEnd.isSame(aStart)) && (bEnd.isBefore(aEnd) || bEnd.isSame(aEnd))) {
                                collision = true;
                            }
                        }
                    }
                    if (collision) {
                        accept = false;
                    }
                }

                if (!accept) {
                    t_value.splice(t_index, 1);
                    input.prop('checked', false);
                    alert('Your selection has time conflictions.');
                }
            } else {
                var t_index2 = t_value.indexOf(input.val());
                if (t_index2 !== -1) {
                    t_value.splice(t_index2, 1);
                }
            }
            hidden_input.data('value', t_value);
            hidden_input.val(JSON.stringify(t_value));
        });
        /* End Checkbox Group */

        /* Radio Group */
        $('input[type="radio"]').on('change', function () {
            var input = $(this);
            var hidden_input = $('#' + input.attr('data-parent'));
            if (input.prop('checked')) {
                hidden_input.val(input.val());
            }
        });
        /* End Radio Group */


        $('.uploaded-image').each(function () {
            $.ajax({
                type: 'Get',
                url: '../../Cloud/RegistrantImageThumbnail/' + $(this).attr('data-form-registrant') + '?component=' + $(this).attr('data-form-component'),
                success: function (data) {
                    $(this).attr('src', data);
                },
            });
        });
    }
}());