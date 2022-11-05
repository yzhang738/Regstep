/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    // #region reportInformation
    var report_id = $('#key').val();
    var page = {};
    var token = '';
    // #endregion

    // #region click events

    $('.email-report').on('click', function (e) {
        e.preventDefault();
        loadInitPage('email');
    });
    $('.invitation-report').on('click', function (e) {
        e.preventDefault();
        loadInitPage('invitation');
    });

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


    //#endregion

    //#region Loading Pages

    function loadInitPage(reportType) {
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/FormEmailReport/NewToken', true);
        xhr.ontimeout = function () { alert('timeout'); };
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                return;
            }
            token = result.data.token;
            setTimeout(function () { loadPage(1); }, 200);
            result = null;
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
                page.RecordsPerPage = $(this).val();
                updateRecordsPerPage();
            });
            $('#pageSelect').on('change', function () {
                loadPage(parseInt($(this).val()));
            });
            $('#formType').on('change', function (e) {
                handleQuickFilter(e, $(this));
            });
            $('#downloadReport').on('click', function () {
                //var newTable = new JTable();
                var dxhr = new XMLHttpRequest();
                dxhr.open('post', window.location.origin + '/FormEmailReport/CreateXcel');
                RESTFUL.jsonHeader(dxhr);
                RESTFUL.ajaxHeader(dxhr);
                dxhr.onerror = function () {
                    RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
                    processing.hidePleaseWait();
                };
                dxhr.onload = function (event) {
                    if (this.status === 200) {
                        var result = RESTFUL.parse(this);
                        if (result.success) {
                            processing.hidePleaseWait();
                            window.location = result.location;
                        } else {
                            RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                            processing.hidePleaseWait();
                        }
                    } else {
                        RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                        processing.hidePleaseWait();
                    }
                };
                processing.showPleaseWait('Creating Excel File', 'please wait');
                dxhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ token: token })));
                dxhr = null;
            });
        };
        xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ id: report_id, reportType: reportType })));
        xhr = null;
    }

    function loadPage(pageNumber) {
        if (!processing.inUse) {
            processing.showPleaseWait('Loading Page ' + pageNumber);
        }
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/FormEmailReport/get/' + token + '/' + pageNumber, true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                processing.update(result.data.Message, (result.data.Progress * 100).toFixed(2) + '%', result.data.Progress, result.data.Details);
                setTimeout(function () { loadPage(1); }, 50);
                return;
            }
            page = result.data;
            jsonFiltering.create(result.data.Filters, result.data.Fields, updateFilters);
            jsonSorting.create(result.data.Sortings, result.data.Fields, updateSortings);
            $('#pageSize').val(page.RecordsPerPage);
            $('.pagedReport_recordsReturned').val(page.RecordsPerPage);
            updateTable();
            generateHeaders();
            result = null;
        };
        xhr.send();
        xhr = null;
    }

    //#endregion

    function updateTable() {
        updateInputs();
        var html = '<table id="pagedTable" class="table table-striped table-registration-data"><thead><tr>';
        for (var headerIndex = 0; headerIndex < page.Headers.length; headerIndex++) {
            html += '<th>' + page.Headers[headerIndex].Value + '</th>';
        }
        html += '</thead><tbody>';
        for (var rowIndex = 0; rowIndex < page.Rows.length; rowIndex++) {
            var t_row = page.Rows[rowIndex];
            html += '<tr id="' + t_row.Id + '" data-rowId="' + t_row.Id + '">';
            for (var hIndex = 0; hIndex < page.Headers.length; hIndex++) {
                var header = page.Headers[hIndex];
                for (var valIndex = 0; valIndex < t_row.Values.length; valIndex++) {
                    t_value = t_row.Values[valIndex];
                    if (t_value.HeaderId !== header.Id) {
                        continue;
                    }
                    var editable = header.Editable || t_value.Editable;
                    if (!t_value.Editable) {
                        editable = false;
                    }
                    html += '<td' + (editable ? ' class="data-editable cursor-pointer"' : '') + ' data-id="' + t_value.Id + '" data-headerId="' + t_value.HeaderId + '">';
                    if (t_value.HeaderId.toLowerCase() === 'confirmation') {
                        html += '<a href="' + window.location.origin + '/Registrant/Get/' + t_row.Id + '?popout=true" class="registrant-confirmation">';
                    } else if (t_value.Link !== null) {
                        html += '<a href="' + window.location.origin + t_value.Link.substring(1) + '" location="_blank">'
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
                    if (t_value.SubScript !== null) {
                        html += '<sub>' + t_value.SubScript + '</sub>';
                    }
                    if (t_value.HeaderId.toLowerCase() === 'confirmation' || t_value.Link !== null) {
                        html += '</a>';
                    }
                    html += '</td>';
                    break;
                }
            }
            html += '</tr>';
        }
        html += '</tbody></table>';
        $('#report').html(html);
        $('#buttons').hide();
        html = null;
        $('.registrant-confirmation').on('click', function (e) {
            e.preventDefault();
            var regWindow = window.open(this.href, '_blank', 'toolbar=now,location=no,menubar=no,width=1000,resizable,scrollbars,height=500,top=50,left=50');
        });
        clickToEdit();
        prettyProcessing.hidePleaseWait();
    }

    function updateInputs() {
        $('.recordsReturned').html(page.TotalRecords);
        var htmlOptions = '';
        for (var optInd = 0; optInd < page.TotalPages; optInd++) {
            htmlOptions += '<option value="' + (optInd + 1) + '"' + ((optInd + 1 === page.Page) ? ' selected="selected"' : '') + '>' + (optInd + 1) + '</option>';
        }
        $('#pageSelect').html(htmlOptions);
        $('#pageSize').val(page.RecordsPerPage);
        $('.pagedReport_recordsReturned').val(page.RecordsPerPage);
        $('.pagedReport_name').text(page.Name);
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
                    runBinding();
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
        for (var h_index = 0; h_index < page.Fields.length; h_index++) {
            var header = page.Fields[h_index];
            if (header.Using) {
                usedHeadersDiv.append('<div class="header" data-id="' + header.Id + '">' + (header.Id !== 'Confirmation' && header.Id !== 'confirmation' ? '<input type="checkbox" /> &nbsp;&nbsp;' : '&nbsp;&nbsp;&nbsp;&nbsp;') + header.Label + '</div>');
            } else {
                unusedHeadersDiv.append('<div class="header" data-id="' + header.Id + '"><input type="checkbox" /> &nbsp;&nbsp;' + header.Label + '</div>');
            }
        }

        $('#fields_edit').on('click', function (e) {
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

        $('#fields_clearall').on('click', function (e) {
            e.preventDefault();
            $('#selectedHeaders').find('.header[data-id!="Confirmation"]').appendTo('#availableHeaders');
            updateFields();
        });

        $('#fields_addall').on('click', function (e) {
            e.preventDefault();
            $('#availableHeaders').find('.header').appendTo('#selectedHeaders');
            updateFields();
        });
    }

    function updateFields() {
        processing.showPleaseWait('Updating Headers');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormEmailReport/Fields', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            page = result.data;
            updateTable();
            result = null;
        };
        var headers = [];
        $('#selectedHeaders').find('.header').each(function () {
            headers.push($(this).attr('data-id'));
        });
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: token, headers: headers })));
        xhr = null;
    }

    function updateFilters() {
        processing.showPleaseWait('Updating Filters');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormEmailReport/Filters', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            page = result.data;
            updateTable();
            result = null;
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: token, filters: jsonFiltering.generateJsonFilters() })));
        xhr = null;
    }

    function updateRecordsPerPage() {
        processing.showPleaseWait('Updating Records Per Page');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormEmailReport/RecordsPerPage', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            page = result.data;
            updateTable();
            result = null;
        };
        var recordsPerPage = page.RecordsPerPage;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: token, recordsPerPage: recordsPerPage })));
        xhr = null;
    }

    function runBinding() {
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

    function updateSortings() {
        processing.showPleaseWait('Updating Sortings');
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/FormEmailReport/Sortings', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            page = result.data;
            updateTable();
            result = null;
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ token: token, sortings: jsonSorting.getJsonSortings() })));
        xhr = null;
    }
}());