/// <reference path="../Sorting/sortable.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    var holdUpdates = false;
    var holdRPGUpdate = false;
    //#region reportInformation
    var report_id = $('#report_id').val();
    var reportInformation = {
        recordsPerPage: 0,
        totalPages: 0,
        saved: false,
        name: '',
        token: null,
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
    canEdit = false;
    //#endregion
    var height = $(window).innerHeight();
    height *= 0.72;
    $('.modal-fill .modal-body').css('max-height', height + 'px');
    $(window).on('resize', function () {
        var t_height = $(window).innerHeight();
        t_height *= 0.72;
        $('.modal-fill .modal-body').css('max-height', t_height + 'px');
    });
    processing.showPleaseWait('Requesting Table Data', 'Waiting for Response', 100);
    getToken();
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
        t_xhr.open('put', window.location.origin + '/Registrant/Data', true);
        RESTFUL.jsonHeader(t_xhr);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); holdUpdates = false; };
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
            holdUpdates = false;
        };
        holdUpdates = true;
        t_xhr.send(JSON.stringify(data));
        t_xhr = null;
        data = null;
    });
    $('#pageSize').on('focus', function () { holdRPGUpdate = true; });
    $('#pageSize').on('blur', function () { holdRPGUpdate = false; });
    function updateSortings() {
        prettyProcessing.showPleaseWait('Updating Sortings');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormReport/Sortings', true);
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
            holdUpdates = false;
        };
        holdUpdates = true;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, sortings: jsonSorting.getJsonSortings() })));
        xhr = null;
    }
    function updateFields() {
        prettyProcessing.showPleaseWait('Updating Headers');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormReport/Fields', true);
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
            holdUpdates = false;
        };
        var headers = [];
        $('#selectedHeaders').find('.header').each(function () {
            headers.push($(this).attr('data-id'));
        });
        holdUpdates = true;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, headers: headers })));
        xhr = null;
    }
    function updateFilters() {
        prettyProcessing.showPleaseWait('Updating Filters');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormReport/Filters', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            reportInformation.update(result.table);
            updateTable(result.table);
            reportInformation.saved = true;
            result = null;
            holdUpdates = false;
        };
        holdUpdates = true;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, filters: jsonFiltering.generateJsonFilters() })));
        xhr = null;
    }
    function updateRecordsPerPage() {
        prettyProcessing.showPleaseWait('Updating Records Per Page');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormReport/RecordsPerPage', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            reportInformation.update(result.table);
            updateTable(result.table);
            reportInformation.saved = true;
            result = null;
            holdUpdates = false;
        };
        var recordsPerPage = reportInformation.recordsPerPage;
        holdUpdates = true;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: reportInformation.token, recordsPerPage: recordsPerPage })));
        xhr = null;
    }
    function loadPage(page) {
        prettyProcessing.showPleaseWait('Loading Page ' + page);
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/FormReport/Get/' + reportInformation.token + '/' + page, true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }

            reportInformation.recordsPerPage = result.data.table.RecordsPerPage;
            reportInformation.totalPages = result.data.table.TotalPages;
            reportInformation.name = result.data.table.Name;
            reportInformation.saved = true;
            reportInformation.table = result.data.table;
            updateTable(result.data.table);
            result = null;
            $('#pageSize').val(reportInformation.recordsPerPage);
            $('.pagedReport_recordsReturned').val(reportInformation.recordsPerPage);
            updateInputs();
        };
        xhr.send();
        xhr = null;
    }
    function getToken()
    {
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/FormReport/NewToken/' + report_id, true);
        xhr.ontimeout = function () { alert('timeout'); };
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
                return;
            }
            reportInformation.token = result.data.token;
            loadInitPage();
        };
        xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ id: report_id })));
        xhr = null;
    }
    function loadInitPage() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/FormReport/Get/' + reportInformation.token, true);
        xhr.ontimeout = function () { alert('timeout'); };
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
                return;
            }
            if (!result.data.progress.Complete) {
                processing.update(result.data.progress.Message, (result.data.progress.Status * 100).toFixed(2) + '%', result.data.progress.Status, result.data.progress.Details)
                setTimeout(loadInitPage, 200)
                return;
            }
            canEdit = result.data.canEdit;
            updateTable(result.data.table);
            reportInformation.update(result.data.table);
            reportInformation.saved = true;
            jsonFiltering.create(result.data.table.Filters, result.data.table.Headers, updateFilters);
            jsonSorting.create(result.data.table.Sortings, result.data.table.Headers, updateSortings);
            reportInformation.headers = result.data.table.Headers;
            generateHeaders();
            if (trail.current !== null) {
                trail.current.label = "Form Report: " + result.data.table.Name;
                if (result.data.table.IsReport) {
                    trail.current.parameters.id = resultdata.table.SavedReportId;
                }
                trail.updatePheromone();
            }
            result = null;
            $('#refresh').on('click', function (e) {
                e.preventDefault();
                var t_xhr = new XMLHttpRequest();
                processing.showPleaseWait("Refreshing the Report");
                t_xhr.open('post', window.location.origin + '/formreport/refresh/');
                t_xhr.onload = function () {
                    var result = RESTFUL.parse(this);
                    if (result.success = true) {
                        window.location.href = result.location;
                    } else {
                        RESTFUL.showError("Failed to refresh.", "FAILED");
                    }
                };
                var t_data = { id: $('#form_id').val() };
                RESTFUL.jsonHeader(t_xhr);
                RESTFUL.ajaxHeader(t_xhr);
                t_xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken(t_data)));
            });
            $('#filters_clearall').on('click', function (e) {
                e.preventDefault();
                jsonFiltering.clear();
                updateFilters();
            });
            $('#filters_edit').on('click', function (e) {
                e.preventDefault();
                jsonFiltering.display();
            });

            $('#sortings_edit').on('click', function (e) {
                e.preventDefault();
                jsonSorting.display();
            });
            $('#sortings_clearall').on('click', function (e) {
                e.preventDefault();
                jsonSorting.clear();
                updateSortings();
            });
            $('#pageSize').on('blur', function () {
                reportInformation.recordsPerPage = $(this).val();
                updateRecordsPerPage();
            });
            $('#pageSelect').on('change', function () {
                loadPage(parseInt($(this).val()));
            });
            $('#formType').on('change', function (e) {
                handleQuickFilter(e, $(this));
            });
            if (!reportInformation.table.SavedReport) {
                $('#file_save,#file_delete,#options_favorite,#file_permissions').css('color', '#ccc');
                $('.jTable_standardOnly').show();
            } else {
                $('.jTable_standardOnly').hide();
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
                t_xhr.open('post', window.location.origin + '/FormReport/SaveAs');
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
                            if (trail.current !== null) {
                                trail.current.label = "Form Report: " + result.name;
                                trail.current.parameters.id = result.id;
                                trail.updatePheromone();
                            }
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
                t_xhr.open('put', window.location.origin + '/FormReport/Save');
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
                            $('.jTable_standardOnly').hide();
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
                window.location.replace(window.location.origin + '/FormReport/Permissions/' + reportInformation.token);
            });
            $('#file_delete').on('click', function (e) {
                e.preventDefault();
                if (!reportInformation.table.SavedReport) {
                    return;
                }
                RESTFUL.showConfirmation("Are you sure you want to delete?", "Delete Confirmation", deleteReport);
            });
            $('#options_favorite').on('click', function (e) {
                e.preventDefault();
                if (!reportInformation.table.SavedReport) {
                    return;
                }
                prettyProcessing.showPleaseWait('Favoriting Report', 'Please Wait', 100);
                var t_xhr = new XMLHttpRequest();
                t_xhr.open('put', window.location.origin + '/FormReport/Favorite');
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
            $('#downloadReport').attr('href', window.location.origin + '/FormReport/Download/' + reportInformation.token);
            setTimeout(function () { Update(); }, 5000);
        };
        xhr.send();
        xhr = null;
    }
    function deleteReport(token)
    {
        prettyProcessing.showPleaseWait('Deleting Report', 'Please Wait', 100);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('delete', window.location.origin + '/FormReport/Delete');
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
                    window.location.replace(window.location.origin + '/FormReport/Get/' + result.id);
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
    }
    function showFileModal(load) {
        if (typeof (load) === 'undefined' || load === null) {
            load = false;
        }
        $('#m_save_files').html('Loading Files');
        prettyProcessing.showPleaseWait('Retrieving Saved Reports', 'Please Wait');
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('get', window.location.origin + '/FormReport/SavedReports/' + reportInformation.token);
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
                            var newLocation = window.location.origin + '/FormReport/Get/' + selectedReport;
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
            if (!table.Headers[headerIndex].Hidden) {
                html += '<th>' + table.Headers[headerIndex].Value + '</th>';
            }
        }
        html += '</thead><tbody>';
        for (var rowIndex = 0; rowIndex < table.Rows.length; rowIndex++) {
            var t_row = table.Rows[rowIndex];
            html += '<tr id="' + t_row.Id + '" data-rowId="' + t_row.Id + '">';
            for (var headerIndex = 0; headerIndex < table.Headers.length; headerIndex++) {
                if (table.Headers[headerIndex].Hidden) {
                    continue;
                }
                t_value = findValue(t_row, table.Headers[headerIndex].Id);
                html += '<td' + (t_value.Editable && canEdit ? ' class="data-editable cursor-pointer"' : '') + ' data-id="' + t_value.Id + '" data-headerId="' + t_value.HeaderId + '">';
                if (t_value.HeaderId.toLowerCase() === 'confirmation') {
                    html += '<a href="' + window.location.origin + '/Registrant/Get/' + t_row.Id + '?popout=true" class="registrant-confirmation">';
                }
                if (t_value.Type === 'date' || t_value.Type === 'datetime') {
                    var t_date = moment(t_value.Value);
                    if (t_date.isValid()) {
                        if (t_value.Type === 'date') {
                            html += t_date.format('MM/DD/YYYY');
                        } else {
                            html += t_date.format("MM/DD/YYYY h:mm A");
                        }
                    } else {
                        html += t_value.Value;
                    }
                } else {
                    html += t_value.Value;
                }
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
            var regWindow = window.open(this.href, '_blank', 'toolbar=now,location=no,menubar=no,width=1000,resizable,scrollbars,height=500,top=50,left=50');
            regWindow.onunload = function (e) {
                checkForUpdates();
            };
        });
        clickToEdit();
        prettyProcessing.hidePleaseWait();
    }
    function findValue(row, headerId) {
        for (var row_i = 0; row_i< row.Values.length; row_i++) {
            var value = row.Values[row_i];
            if (value.HeaderId === headerId) {
                return value;
            }
        }
    }
    function updateTableInformation(table) {
        if (reportInformation.table.Page !== table.Page) {
            return;
        }
        reportInformation.table = table;
        $('#recordsReturned').html(reportInformation.table.TotalRecords);
        for (var rowIndex = 0; rowIndex < table.Rows.length; rowIndex++) {
            var t_row = table.Rows[rowIndex];
            var jq_row = $('#' + t_row.Id);
            for (var valIndex = 0; valIndex < t_row.Values.length; valIndex++) {
                t_value = t_row.Values[valIndex];
                var jq_value = jq_row.find('[data-headerId="' + t_value.HeaderId + '"]');
                var html = '';
                if (t_value.HeaderId.toLowerCase() === 'confirmation') {
                    html += '<a href="' + window.location.origin + '/Registrant/Get/' + t_row.Id + '?popout=true" class="registrant-confirmation">';
                }
                if (t_value.Type === 'date' || t_value.Type === 'datetime') {
                    var t_date = moment(t_value.Value);
                    if (t_date.isValid()) {
                        if (t_value.Type === 'date') {
                            html += t_date.format('MM/DD/YYYY');
                        } else {
                            html += t_date.format("MM/DD/YYYY h:mm A");
                        }
                    } else {
                        html += t_value.Value;
                    }
                } else {
                    html += t_value.Value;
                }
                if (t_value.HeaderId.toLowerCase() === 'confirmation') {
                    html += '</a>';
                }
                jq_value.html(html);
            }
        }
        $('.registrant-confirmation').on('click', function (e) {
            e.preventDefault();
            var regWindow = window.open(this.href, '_blank', 'toolbar=now,location=no,menubar=no,width=1000,resizable,scrollbars,height=500,top=50,left=50');
            regWindow.onunload = function (e) {
                checkForUpdates();
            };
        });
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
        if (!canEdit) {
            return
        }
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
        xhr.open('get', window.location.origin + '/Registrant/Data/' + id + '/' + dataId + '/' + width, true);
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
        var usedHeadersDiv = $('#list-usedheaders');
        var unusedHeadersDiv = $('#list-availableheaders');
        for (var uh_index = 0; uh_index < reportInformation.table.Headers.length; uh_index++) {
            var u_header = reportInformation.table.Headers[uh_index];
            if (u_header.Hidden) {
                unusedHeadersDiv.append('<li class="header" data-id="' + u_header.Id + '"><input type="checkbox" />&nbsp;&nbsp;' + u_header.Label + '</li>');
            } else {
                usedHeadersDiv.append('<li class="header" data-id="' + u_header.Id + '">' + (u_header.Id !== 'Confirmation' && u_header.Id !== 'confirmation' ? '<input type="checkbox" /> &nbsp;&nbsp;' : '&nbsp;&nbsp;&nbsp;&nbsp;') + u_header.Value + '</li>');
            }
        }
        var sortable = Sortable.create(usedHeadersDiv[0]);
        $('#headers_edit').on('click', function (e) {
            e.preventDefault();
            $('#headersModal').modal('show');
        });
        $('#headersModal').find('.add-header').on('click', function (e) {
            e.preventDefault();
            $('#availableHeaders').find('.header').each(function () {
                if ($(this).find('input').prop('checked')) {
                    $(this).appendTo($('#list-usedheaders'));
                }
            });
            $('#headersModal').find('.header>input[type="checkbox"]').prop('checked', false);
        });
        $('#headersModal').find('.remove-header').on('click', function (e) {
            e.preventDefault();
            $('#selectedHeaders').find('.header').each(function () {
                if ($(this).find('input').prop('checked')) {
                    $(this).appendTo($('#list-availableheaders'));
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

        var filter_submitted = {};
        filter_submitted.ActingOn = 'Status';
        filter_submitted.Equality = '==';
        filter_submitted.Value = '1';

        var filter = select.val();
        jsonFiltering.clear();
        switch (filter) {
            case 'active':
                jsonFiltering.addFilter(filter_submitted);
                break;
            case 'canceled':
                var canceledFilter1 = {};
                canceledFilter1.ActingOn = 'Status';
                canceledFilter1.Equality = '==';
                canceledFilter1.Value = '2';
                var canceledFilter2 = {};
                canceledFilter2.ActingOn = 'Status';
                canceledFilter2.Equality = '==';
                canceledFilter2.Value = '3';
                canceledFilter2.LinkedBy = 'or';
                var canceledFilter3 = {};
                canceledFilter3.ActingOn = 'Status';
                canceledFilter3.Equality = '==';
                canceledFilter3.Value = '4';
                canceledFilter3.LinkedBy = 'or';
                jsonFiltering.addFilter(canceledFilter1);
                jsonFiltering.addFilter(canceledFilter2);
                jsonFiltering.addFilter(canceledFilter3);
                break;
            case 'incompletes':
                var incompleteFilter = {};
                incompleteFilter.ActingOn = 'Status';
                incompleteFilter.Equality = '==';
                incompleteFilter.Value = '0';
                jsonFiltering.addFilter(incompleteFilter);
                break;
            case 'deleted':
                var deletedFilter = {};
                deletedFilter.ActingOn = 'Status';
                deletedFilter.Equality = '==';
                deletedFilter.Value = '5';
                jsonFiltering.addFilter(deletedFilter);
                break;
            case 'unbalanced':
                var unbalFilter = {};
                unbalFilter.ActingOn = 'Balance';
                unbalFilter.Equality = '!=';
                unbalFilter.Value = '0';
                jsonFiltering.addFilter(unbalFilter);
                break;
            case 'refund':
                var refFilter = {};
                refFilter.ActingOn = 'Balance';
                refFilter.Equality = '<';
                refFilter.Value = '0';
                jsonFiltering.addFilter(refFilter);
                break;
            case 'owed':
                var oweFilter = {};
                oweFilter.ActingOn = 'Balance';
                oweFilter.Equality = '>';
                oweFilter.Value = '0';
                jsonFiltering.addFilter(oweFilter);
                break;
            case 'rsvpAccept':
                var rsvpAFilter = {};
                rsvpAFilter.ActingOn = 'RSVP';
                rsvpAFilter.Equality = '==';
                rsvpAFilter.Value = 'true';
                jsonFiltering.addFilter(rsvpAFilter);
                jsonFiltering.addFilter(filter_submitted);
                break;
            case 'rsvpDecline':
                var rsvpDFilter = {};
                rsvpDFilter.ActingOn = 'RSVP';
                rsvpDFilter.Equality = '==';
                rsvpDFilter.Value = 'false';
                jsonFiltering.addFilter(rsvpDFilter);
                jsonFiltering.addFilter(filter_submitted);
                break;
        }
        jsonFiltering.display(false);
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
            var t_value = JSON.parse(hidden_input.val());
            if (input.prop('checked')) {
                var accept = true;
                var t_index = t_value.indexOf(input.attr('id'));
                if (t_index === -1) {
                    t_value.push(input.attr('id'));
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
                var t_index2 = t_value.indexOf(input.attr('id'));
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
    function Update() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/FormReport/Get/' + reportInformation.token + '/' + reportInformation.table.Page, true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { };
        xhr.onload = function () {
            if (!holdUpdates) {
                var result = RESTFUL.parse(this);
                updateTableInformation(result.data.table);
                reportInformation.recordsPerPage = result.data.table.RecordsPerPage;
                reportInformation.totalPages = result.data.table.TotalPages;
                reportInformation.name = result.data.table.Name;
                reportInformation.saved = true;
                reportInformation.table = result.data.table;
                result = null;
                if (!holdRPGUpdate) {
                    $('#pageSize').val(reportInformation.recordsPerPage);
                    $('.pagedReport_recordsReturned').val(reportInformation.recordsPerPage);
                }
            }
            setTimeout(function () { Update(); }, 5000);
        };
        xhr.send();
        xhr = null;
    }
}());