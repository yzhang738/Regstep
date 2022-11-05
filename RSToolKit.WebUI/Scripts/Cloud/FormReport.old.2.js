/* Form Report
 * Written By:   D.J. Enzyme
 * Date Created: 20141014
 * Version:      1.2.1.1
 */

/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Filters.js" />
/// <reference path="EmailSendInformation.js" />

/* global reports */
/* global headers */
/* global printUrl */

// restful.js Globals
/* global RESTFUL */

// Filter.js Globals
/* global generateActingInput */
/* global generateValueInput */
/* global tests */
/* global PopulateFilters */
/* global FillFilters */
/* global RunFilters */
/* global ActingOnChange */
/* global DeleteStatement */
/* global filterIndex */

var FORMREPORT_VERSION = null;

$(document).on('ready', function () {
    FORMREPORT_VERSION = '1.3.1.1';
    var oldData = { id: null, regKey: null, value: null, isSecure: false };

    if (typeof (RESTFUL) === 'undefined') {
        throw 'restful.js must be used.'
    }

    if (typeof (jQuery) === 'undefined') {
        throw 'jquery must be used.';
    }

    if (typeof (processing) === 'undefined') {
        throw 'processing.js must be used.';
    }

    if (typeof (EmailSendInformation) === 'undefined') {
        throw 'EmailSendInformation.js must be used.';
    }

    var headers = [];

    // Hide information

    $('#registrantHeader').hide();

    // Grab Data and Bind editables

    GetData();
    BindEditable();
    RunFilters(generateSelection, generateActing, p_FillFilters);

    function BingSort()
    {
        $('#registrantHeader .table-sort').on('click', function (e) {
            e.preventDefault();
            var link = $(this);
            var actingon = link.attr('data-actingon');
            if (report.Sortings.ActingOn == actingon) {
                report.Sortings.Descending = !report.Sortings.Descending;
                link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes');
                link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes-alt');
            } else {
                $('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt').removeClass('glyphicon-sort-by-attributes');
                report.Sortings.ActingOn = actingon;
                report.Sortings.Descending = false;
                link.children('.sort-icon').addClass('glyphicon-sort-by-attributes');
                link.children('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt');
            }
            GetData();
        });
    }

    $('.table-sort').on('click', function (e) {
        e.preventDefault();
        var link = $(this);
        var actingon = link.attr('data-actingon');
        if (report.Sortings.ActingOn == actingon) {
            report.Sortings.Descending = !report.Sortings.Descending;
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes-alt');
        } else {
            $('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt').removeClass('glyphicon-sort-by-attributes');
            report.Sortings.ActingOn = actingon;
            report.Sortings.Descending = false;
            link.children('.sort-icon').addClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt');
        }
        GetData();
    });

    // Events

    $('#saveEdit').on('click', function () {
        // Here we make the ajax call to save the data
        $('#editModal').find('#editingData').hide('fast');
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
        var data = { ComponentKey: oldData.id, RegistrantKey: oldData.regKey, Value: $('#' + oldData.id).val(), Waitlistings: waitlisting };
        // Add the antiforgery token.
        AddJsonAntiForgeryToken(data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', '../../Cloud/SaveRegistrantData', true);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        t_xhr.onload = function (event) {
            c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#' + oldData.regKey).find('td[data-id="' + oldData.id + '"]').html(result.NewValue);
                    $('#editModal').modal('hide');
                } else {
                    $('#editModal .modal-body').find('#editingProgress').hide('fast');
                    $('#editModal .modal-body').find('#editingData').show('fast');
                    var message = '';
                    for (var i = 0; i < result.Errors.length; i++) {
                        if (i != 0) {
                            message += '<br />';
                        }
                        message += result.Errors[i];
                    }
                    $('#editModal').find('.form-messagebox').html(message).show('fast');
                }
            } else {
                $('#editModal').modal('hide');
                RESTFUL.showError('Unhandled Server Error');
            }
        };
        t_xhr.send(JSON.stringify(data));
    });

    $('#pageSize').on('blur', function () {
        var input = $(this);
        if (isNaN(input.val())) {
            alert('You must provide a valid number.');
            input.val(report.PageSize);
        } else {
            report.PageSize = Math.round(input.val());
        }
        GetData();
    });

    $('#pageNumber').on('change', function () {
        report.Page = $(this).val();
        GetData();
    });

    $('#pageRight').on('click', function () {
        report.Page++;
        if ($(this).hasClass('disabled')) {
            return;
        }
        $('#pageNumber').val(report.Page);
        GetData();
    });

    $('#pageLeft').on('click', function () {
        report.Page--;
        if ($(this).hasClass('disabled')) {
            return;
        }
        $('#pageNumber').val(report.Page);
        GetData();
    });

    $('#formType').on('change', function () {
        report.ReportType = $(this).val();
        GetData();
    });

    $('#downloadReport').on('click', function (e) {
        e.preventDefault();
        report.Registrants = [];
        report.Fileds = [];
        window.location = '../../Cloud/DownloadFormReport?rawJson=' + JSON.stringify(report);
    });

    $('#setFilters').on('click', function (e) {
        e.preventDefault();
        $('#filterModal').modal('hide');
        report.Filters = PopulateFilters();
        GetData();
    });

    // FUNCTIONS

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
                if (t_index == -1) {
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
                            if (t_value[i] === t_value[j])
                                continue;
                            var t_item2 = $('#' + t_value[j]);
                            var bStart = moment(t_item2.attr('data-agenda-start'));
                            var bEnd = moment(t_item2.attr('data-agenda-end'));
                            if ((bEnd.isAfter(aStart) || bEnd.isSame(aStart)) && (bEnd.isBefore(aEnd) || bEnd.isSame(aEnd)))
                                collision = true;
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
                var t_index = t_value.indexOf(input.val());
                if (t_index != -1) {
                    t_value.splice(t_index, 1);
                }
            }
            hidden_input.data('value', t_value);
            hidden_input.val(JSON.stringify(t_value));
        });
        /* End Checkbox Group */

        /* Radio Group */
        $('input[data-component-type="radiogroup"][type="hidden"]').each(function () {
        });
        /* End Radio Group */

        $('input[type="radio"]').on('change', function () {
            var input = $(this);
            var hidden_input = $('#' + input.attr('data-parent'));
            if (input.prop('checked')) {
                hidden_input.val(input.val());
            }
        });

        $('.uploaded-image').each(function (i) {
            $.ajax({
                type: "Get",
                url: '../../Cloud/RegistrantImageThumbnail/' + $(this).attr('data-form-registrant') + '?component=' + $(this).attr('data-form-component'),
                success: function (data) {
                    $(this).attr('src', data);
                },
            });
        });
    }

    function GetData() {
        processing.showPleaseWait();
        report.Registrants = [];
        report.Fields = [];
        $('#registrantData').hide('fast');
        $.ajax({
            url: '../../Cloud/GetRegistrationReport',
            type: "get",
            data: { rawJson: JSON.stringify(report) },
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    $.extend(report, result);
                    $('#registrantData').html('');
                    var html = '';
                    var header = null;
                    if (result.ReportType == 'all' || result.ReportType == 'active' || result.ReportType == 'canceled' || result.ReportType == "incompletes" || result.ReportType == "deleted" || result.ReportType == "rsvpAccept" || result.ReportType == "rsvpDecline") {
                        headers = formHeaders;
                        $('#registrantHeaderAll').show();
                        $('#registrantHeader').hide();
                        for (var i = 0; i < report.Registrants.length; i++) {
                            var date = moment(report.Registrants[i].Date);
                            html += '<tr id="' + report.Registrants[i].Id + '"><td><a href="../../Cloud/Registrant/' + report.Registrants[i].Id + '">' + report.Registrants[i].Confirmation + '</a></td><td>' + date.format('YYYY-M-D h:mm:ss A') + '</td><td>' + report.Registrants[i].Status + '</td><td>' + report.Registrants[i].Email + '</td>';
                            if (rsvp) {
                                html += '<td>' + (report.Registrants[i].RSVP ? rsvpLabel[0] : rsvpLabel[1]) + '</td>';
                            }
                            if (audience) {
                                html += '<td>' + report.Registrants[i].AudienceName + '</td>';
                            }
                            for (var j = 0; j < fields.length; j++) {
                                var data = findDataObj(report.Registrants[i].Data, fields[j].UId);
                                if (data === null) {
                                    if (fields[j].SpecialDescriminator == 'file')
                                        html += '<td class="file" data-id="' + fields[j].UId + '"><i>No File</i></td>';
                                    else
                                        html += '<td class="editable-item cursor-pointer" data-id="' + fields[j].UId + '"><i>No Value</i></td>';
                                } else {
                                    if (fields[j].SpecialDescriminator == 'file')
                                        html += '<td class="file editable-item cursor-pointer" data-id="' + data.Id + '">' + data.FormattedValue + '</td>';
                                    else
                                        html += '<td class="editable-item cursor-pointer" data-issecure="' + data.IsSecure + '" data-id="' + data.Id + '">' + (data.FormattedValue === null ? "<i>No Selection</i>" : data.FormattedValue) + '</td>';
                                }
                            }
                            html += '</tr>';
                        }
                    } else if (result.ReportType == 'unbalanced') {
                        header = '<th class="table-sort">Registrant</th><th class="table-sort">Email</th><th class="table-sort">Amount</th><th>Note</th>';
                        for (var i = 0; i < report.Registrants.length; i++) {
                            var ammount = findData(report.Registrants[i].Data, 'Amount');
                            var note = findData(report.Registrants[i].Data, 'Note');
                            var refund = note == 'Refund Due';
                            html += '<tr><td><a href="../../Cloud/Registrant/' + report.Registrants[i].Id + '">' + report.Registrants[i].Confirmation + '</a></td><td>' + report.Registrants[i].Email + '</td>';
                            html += '<td>' + (refund ? '<span class="negative-balance">' : '') + ammount + (refund ? '</span>' : '') + '</td><td>' + note + '</td></tr>';
                        }
                    } else if (result.ReportType == 'owed' || result.ReportType == 'refund') {
                        header = '<th class="table-sort">Registrant</th><th class="table-sort">Email</th><th class="table-sort">Amount</th>';
                        for (var i = 0; i < report.Registrants.length; i++) {
                            html += '<tr><td><a href="../../Cloud/Registrant/' + report.Registrants[i].Id + '">' + report.Registrants[i].Confirmation + '</a></td><td>' + report.Registrants[i].Email + '</td>';
                            html += '<td>' + findData(report.Registrants[i].Data, 'Amount') + '</td></tr>';
                        }
                    } else if (result.ReportType == 'rsvp') {
                        header = '<th class="table-sort">Total</th><th class="table-sort">Accepted</th><th class="table-sort">Declined</th>';
                        html = '<tr><td>' + findData(report.Registrants[0].Data, 'Total') + '</td><td>' + findData(report.Registrants[0].Data, 'Accepted') + '</td><td>' + findData(report.Registrants[0].Data, 'Declined') + '</td></tr>';
                    } else {
                        headers = report.FilterHeaders;
                        report.FilterHeaders = [];
                        header = '';
                        for (var i = 0; i < result.Fields.length; i++) {
                            header += '<th class="table-sort cursor-pointer" data-actingon="' + result.Fields[i].Name + '"><span class="glyphicon sort-icon ' + ((report.Sortings.ActingOn == result.Fields[i].Name || report.Sortings.ActingOn == result.Fields[i].Id) ? (report.Sortings.Descending ? 'glyphicon-sort-by-attributes-alt' : 'glyphicon-sort-by-attributes') : '') + '"></span>' + result.Fields[i].Name + '</th>';
                        }
                        if (report.FilterHeaders.length > 0) {

                        }
                        for (var i = 0; i < result.Registrants.length; i++) {
                            html += '<tr>';
                            for (var j = 0; j < result.Fields.length; j++) {
                                var data = findDataObj(report.Registrants[i].Data, result.Fields[j].Name);
                                html += '<td>' + data.FormattedValue + '</td>';
                            }
                            html += '</tr>';
                        }
                    }
                    $('#registrantData').html(html);
                    if (header !== null) {
                        $('#registrantHeaderAll').hide();
                        $('#registrantHeader').html(header);
                        BingSort();
                        $('#registrantHeader').show();
                    }
                    $('#pageNumber').html('');
                    for (var i = 0; i < report.TotalPages; i++) {
                        $('#pageNumber').append('<option value="' + (i + 1) + '"' + ((i + 1) == result.Page ? ' selected="true"' : '') + '>' + (i + 1) + '</option>');
                    }
                    if (report.Page == report.TotalPages) {
                        $('#pageRight').addClass('disabled');
                    } else {
                        $('#pageRight').removeClass('disabled');
                    }
                    if (report.Page == 1) {
                        $('#pageLeft').addClass('disabled');
                    } else {
                        $('#pageLeft').removeClass('disabled');
                    }
                    BindEditable();
                    $('#totalRecords').html(report.TotalRecords);
                    $('#registrantData').show('fast');
                    var a_link = $('#printable');
                    var t_report = $.extend({}, report);
                    t_report.Registrants = [];
                    a_link.attr('href', printUrl + '?rawJson=' + JSON.stringify(t_report));
                } else {
                    $('#registrantData').hide('fast');
                    alert(result.Message);
                }
                processing.hidePleaseWait();
            },
            error: function (result) {
                alert('Server Error');
                processing.hidePleaseWait();
            }
        });
    }

    function BindEditable() {
        $('.editable-item').on('click', function () {
            $('#editModal .modal-body').find('#editingProgress').show();
            $('#editModal .modal-body').find('#editingData').hide();
            $('#editModal').modal('show');
            oldData.id = $(this).attr('data-id');
            oldData.regKey = $(this).parent('tr').attr('id');
            oldData.value = $(this).html();
            if ($(this).hasClass('file'))
                setTimeout(function () { ShowEditableItem(oldData.id, oldData.regKey); }, 1000);
            else
                ShowEditableItem(oldData.id, oldData.regKey);
        });
        $('a[href="#emailSend"]').on('click', function (e) {
            e.preventDefault();
            var t_id = $(this).attr('data-emailsend-id');
            EmailSendInformation.load(t_id);
        });
    }

    function ShowEditableItem(id, regKey) {
        var width = $('#editModal .modal-dialog').width() - 2 - 30 - 30;
        $.ajax({
            url: '../../Cloud/EditComponent/' + id + '?registrantKey=' + regKey + '&width=' + width,
            type: "get",
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    $('#editModal').find('#editingData').html(result.Html).show('fast');
                    $('#editModal').find('#editingProgress').hide('fast');
                    RunBinding();
                } else {
                    $('#editModal').modal('hide');
                    oldData.Id = null;
                    oldData.regKey = null;
                    alert(result.Message);
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                oldData.Id = null;
                oldData.regKey = null;
                alert('Server Error');
            }
        });
    }

    function findData(obj, id) {
        for (var i = 0; i < obj.length; i++) {
            if (obj[i].Id == id || obj[i].Variable == id)
                return obj[i].FormattedValue;
        }
        return "";
    }

    function findDataObj(obj, id) {
        for (var i = 0; i < obj.length; i++) {
            if (obj[i].Id == id || obj[i].Variable == id)
                return obj[i];
        }
        return null;
    }

    // Filter methods

    function p_FillFilters(filters) {
        $('#filters').html('');
        var filterIndex = -1;
        for (var i = 0; i < filters.length; i++) {
            filterIndex++;
            var html = '';
            html += '<tr class="filter" data-index="' + filterIndex + '"><td>';
            html += '<label><input type="checkbox" class="groupNext" value="true" /></label> <a href="#" class="delete-statement"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '<input type="hidden" name="Filters[' + filterIndex + '].GroupNext" value="' + filters[i].GroupNext + '" />';
            html += '<input type="hidden" name="Filters[' + filterIndex + '].UId" value="' + filters[i].UId + '" />';
            html += '<input type="hidden" name="Filters[' + filterIndex + '].Order" value="' + filters[i].Order + '" />';
            html += '</td>';
            html += '<td><select class="filter-link form-control" name="Filters[' + filterIndex + '].Link">';
            html += '<option value="0"' + (filters[i].Link === 0 ? ' selected="true"' : '') + '>None</option>';
            html += '<option value="1"' + (filters[i].Link === 1 ? ' selected="true"' : '') + '>And</option>';
            html += '<option value="2"' + (filters[i].Link === 2 ? ' selected="true"' : '') + '>Or</option>';
            html += '</select></td>';
            html += '<td>';
            html += generateActingInput(filters[i].ActingOn);
            html += '</td>';
            html += '<td><select class="filter-test form-control" name="Filters[' + filterIndex + '].Test">';
            for (var j = 0; j < tests.length; j++) {
                html += '<option value="' + tests[j].Index + '"' + (tests[j].Index == filters[i].Test ? ' selected="true"' : '') + '>' + tests[j].Name + '</option>';
            }
            html += '</select></td>';
            html += '<td>';
            html += generateValueInput(filters[i].ActingOn, filterIndex, filters[i].Value);
            html += '</td>';
            html += '</tr>';
            $('#filters').append(html);
            $('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
                var index = parseInt($(this).parents('tr').attr('data-index'));
                ActingOnChange($('tr[data-index="' + index + '"]'), $(this));
            });
            $('.filter[data-index="' + filterIndex + '"]').find('.delete-statement').on('click', function (e) {
                DeleteStatement($(this).parents('tr')[0], e);
            });
        }
        $('.datepicker').datetimepicker();
    }

    function generateActing(id) {
        if (typeof (id) == 'undefined' || id === null)
            id = '';
        var html = '<select class="filter-actingon form-control" name="Filters[' + filterIndex + '].ActingOn"><option value="default"><i>Component</i></option>';
        for (var i = 0; i < headers.length; i++) {
            html += '<option value="' + headers[i].id + '"' + (id == headers[i].id ? ' selected="true"' : '') + '>' + headers[i].name + '</option>';
        }
        html += '</select></td>';
        return html;
    }

    function generateSelection(id, index, value) {
        if (typeof (value) == 'undefined' || value === null)
            value = '';
        if (id === 'default') {
            id = '';
        }
        var header = findHeader(id);
        if (header === null) {
            return '<span class="filter-value">Select a component.</span>';
        }
        var t_html = header.html;
        t_html = t_html.replace(/_value_/i, value);
        t_html = t_html.replace(/_index_/i, index);
        return t_html;
    }

    function findHeader(id) {
        for (var i = 0; i < headers.length; i++) {
            if (headers[i].id == id) {
                return headers[i];
            }
        }
        return null;
    }

    // End filter methods

});

function ReportJson() {
        this.Id = "";
        this.Filters = [];
        this.Sortings = { ActingOn: "Confirmation", Descending: false };
        this.FilterHeaders = [];
        this.Registrants = [];
        this.Fields = [];
        this.Page = 1;
        this.PageSize = 25;
        this.Success = true;
        this.Message = "";
        this.ReportType = "all";
        this.TotalPages = 1;
        this.TotalRecords = 0;
    }
