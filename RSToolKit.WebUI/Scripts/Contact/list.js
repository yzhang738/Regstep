/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    //#region reportInformation
    var report_id = $('#report_id').val();
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
    processing.showPleaseWait('Requesting Table Data', 'Waiting for Response', 100);
    loadInitPage();
    function updateSortings() {
        prettyProcessing.showPleaseWait('Updating Sortings');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Contact/Set', true);
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
        xhr.open('put', window.location.origin + '/Contact/Set', true);
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
        xhr.open('put', window.location.origin + '/Contact/Set', true);
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
        xhr.open('put', window.location.origin + '/Contact/Set', true);
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
        xhr.open('get', window.location.origin + '/Contact/List/' + reportInformation.token + '/' + page, true);
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
        xhr.open('get', window.location.origin + '/Contact/List', true);
        xhr.ontimeout = function () { alert('timeout'); };
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                if (result.retry) {
                    report_id = result.token;
                    reportInformation.token = result.token;
                    processing.update('Processing Table', (result.progress * 100).toFixed(2) + '%', result.progress);
                    setTimeout(function () { loadInitPage(); }, 200);
                    return;
                } else {
                    alert(result.message);
                }
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
            $('#downloadReport').on('click', function () {
                //var newTable = new JTable();
                var dxhr = new XMLHttpRequest();
                dxhr.open('post', window.location.origin + '/Contact/CreateXcel');
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
            //setTimeout(function () { Update(); }, 5000);
        };
        xhr.send();
        xhr = null;
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
                if (t_value.Type === 'date') {
                    var t_date = moment(t_value.Value);
                    if (t_date.isValid()) {
                        html += t_date.format("YYYY-MM-DD h:mm");
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
        $('#list').html(html);
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
        xhr.open('get', window.location.origin + '/Contact/Data/' + id + '/' + dataId);
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
    function Update() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Contact/List/' + reportInformation.token + '/' + reportInformation.table.Page, true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
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
            setTimeout(function () { Update(); }, 5000);
        };
        xhr.send();
        xhr = null;
    }
}());