/* Form Report
 * Written By:   D.J. Enzyme
 * Date Created: 20141014
 * Version:      1.4.0.0
 */

/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Filters.js" />
/// <reference path="../Tool/jTable.js" />
/// <reference path="../jQuery/Plugins/sortable.js" />
/// <reference path="../browserGap.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />

/* global printUrl */
/* global components */
/* global rawTable */

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
    FORMREPORT_VERSION = '1.4.0.0';
    var oldData = { id: null, regKey: null, value: null, isSecure: false };
    var table = new JTable();

    if (typeof (RESTFUL) === 'undefined') {
        throw 'restful.js must be used.'
    }

    if (typeof (jQuery) === 'undefined') {
        throw 'jquery must be used.';
    }

    if (typeof (prettyProcessing) === 'undefined') {
        throw 'prettyProcessing.js must be used.';
    }
    
    $.extend(table, rawTable);
    var sorting = new Sorting(table);
    var filter = new Filter(table);
    table.OnGetComplete = function () {
        BindEditable();
    }
    table.GetPage();
    $('#setFilters').on('click', function (e) {
        e.preventDefault();
        $('#filterModal').modal('hide');
        var filters = PopulateFilters();
        table.Filters = [];
        for (var i = 0; i < filters.length; i++) {
            var filter = new JTableFilter();
            filter.ActingOn = filters[i].ActingOn;
            filter.GroupNext = filters[i].GroupNext;
            filter.Link = JLinkString[parseInt(filters[i].Link)];
            filter.Order = filters[i].Order;
            filter.Test = JTestString[parseInt(filters[i].Test)];
            filter.Value = filters[i].Value;
            table.Filters.push(filter);
        }
        table.Filtered = false;
        table.GetPage();
    });
    $('#pageSize').on('blur', function (e) {
        var val = $(this).val();
        if (isNaN(val)) {
            $(this).val(25);
            table.RecordsPerPage = 25;
            table.GetPage();
        } else {
            table.RecordsPerPage = parseInt(val);
            table.GetPage();
        }
    });
    $('#pageLeft').on('click', function (e) {
        if (table.Page === 1) {
            return;
        }
        table.Page--;
        $('#jTable_pageNumber').val(table.Page);
        table.GetPage();
    });
    $('#pageRight').on('click', function (e) {
        if (table.Page === table.TotalPages()) {
            return;
        }
        table.Page++;
        $('#jTable_pageNumber').val(table.Page);
        table.GetPage();
    });
    $('#jTable_pageNumber').on('change', function (e) {
        table.Page = parseInt($(this).val());
        table.GetPage();
    });
    $('#averages').on('change', function (e) {
        table.Average = $(this).prop('checked');
        table.GetPage();
    });
    $('#graphs').on('change', function (e) {
        table.Graph = $(this).prop('checked');
        table.GetPage();
    });
    $('#count').on('change', function (e) {
        table.Count = $(this).prop('checked');
        table.GetPage();
    });
    $('#downloadReport').on('click', function (e) {
        var data = {
            id: table.Id,
            average: $('#averages').prop('checked'),
            filters: table.Filters,
            sortings: table.Sortings,
            graph: $('#graphs').prop('checked')
        };
        var newTable = $.extend({}, table);
        //var newTable = new JTable();
        var xhr = new XMLHttpRequest();
        xhr.open('post', '../Create/IReport');
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function (event) {
            RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
            prettyProccessing.hidePleaseWait();
        };
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    window.open(window.location.origin + '/Cloud/Download/Report/' + result.Id);
                } else {
                    RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                    prettyProccessing.hidePleaseWait();
                }
            } else {
                RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                prettyProccessing.hidePleaseWait();
            }
        };
        prettyProcessing.showPleaseWait('Creating Report', 'Creating Report');
        newTable.FilteredRows = [];
        newTable.Rows = [];
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(newTable)));
    });
    $('#link_sortings').on('click', function (e) {
        e.preventDefault();
        sorting.Modal.modal('show');
    });
    $('#link_filters').on('click', function (e) {
        e.preventDefault();
        filter.Modal.modal('show');
    });
    $('#printable').on('click', function (e) {
        e.preventDefault();
        table.GetPrintView()
    });

    // Events

    $('#reportType').on('change', function () {
        table.Filters = [];
        switch ($(this).val()) {
            case 'all':
                break;
            case 'active':
                var active_filter = new JTableFilter();
                active_filter.ActingOn = 'status';
                active_filter.Value = '1';
                active_filter.Test = '==';
                active_filter.Order = 1;
                table.Filters.push(active_filter);
                break;
            case 'canceled':
                var canc_filter1 = new JTableFilter();
                canc_filter1.GroupNext = true;
                canc_filter1.ActingOn = 'status';
                canc_filter1.Order = 1
                canc_filter1.Test = '==';
                canc_filter1.Link = 'or';
                canc_filter1.Value = '2';
                var canc_filter2 = new JTableFilter();
                canc_filter2.GroupNext = true;
                canc_filter2.ActingOn = 'status';
                canc_filter2.Order = 3
                canc_filter2.Test = '==';
                canc_filter2.Link = 'or';
                canc_filter2.Value = '3';
                var canc_filter3 = new JTableFilter();
                canc_filter3.GroupNext = false;
                canc_filter3.ActingOn = 'status';
                canc_filter3.Order = 4
                canc_filter3.Test = '==';
                canc_filter3.Link = 'or';
                canc_filter3.Value = '4';
                table.Filters.push(canc_filter1);
                table.Filters.push(canc_filter2);
                table.Filters.push(canc_filter3);
                break;
            case 'incompletes':
                var inc_filter = new JTableFilter();
                inc_filter.ActingOn = 'status';
                inc_filter.Value = '0';
                inc_filter.Test = '==';
                inc_filter.Order = 1;
                table.Filters.push(inc_filter);
                break;
            case 'deleted':
                var del_filter = new JTableFilter();
                del_filter.ActingOn = 'status';
                del_filter.Value = '5';
                del_filter.Test = '==';
                del_filter.Order = 1;
                table.Filters.push(del_filter);
                break;
            case 'unbalanced':
                var unb_filter = new JTableFilter();
                unb_filter.ActingOn = 'balance';
                unb_filter.Value = '0';
                unb_filter.Test = '!=';
                unb_filter.Order = 1;
                table.Filters.push(unb_filter);
                break;
            case 'refund':
                var ref_filter = new JTableFilter();
                ref_filter.ActingOn = 'balance';
                ref_filter.Value = '0';
                ref_filter.Test = '<';
                ref_filter.Order = 1;
                table.Filters.push(ref_filter);
                break;
            case 'owed':
                var owe_filter = new JTableFilter();
                owe_filter.ActingOn = 'balance';
                owe_filter.Value = '0';
                owe_filter.Test = '>';
                owe_filter.Order = 1;
                table.Filters.push(owe_filter);
                break;
            case 'rsvpAccept':
                var rsvpA_filter = new JTableFilter();
                rsvpA_filter.ActingOn = 'rsvp';
                rsvpA_filter.Value = 'True';
                rsvpA_filter.Test = '==';
                rsvpA_filter.Order = 1;
                table.Filters.push(rsvpA_filter);
                break;
            case 'rsvpDecline':
                var rsvpD_filter = new JTableFilter();
                rsvpD_filter.ActingOn = 'rsvp';
                rsvpD_filter.Value = 'False';
                rsvpD_filter.Test = '==';
                rsvpD_filter.Order = 1;
                table.Filters.push(rsvpD_filter);
                break;
            case 'rsvpCount':
                break;
        }
        table.Filtered = false;
        table.GetPage();
    });

    $('#downloadReport').on('click', function (e) {
        e.preventDefault();
        report.Registrants = [];
        report.Fileds = [];
        window.location = '../../Cloud/DownloadFormReport?rawJson=' + JSON.stringify(report);
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

    function BindEditable() {
        $('.editable-item').on('click', function () {
            $('#editModal .modal-body').find('#editingProgress').show();
            $('#editModal .modal-body').find('#editingData').hide();
            $('#editModal').modal('show');
            oldData.id = $(this).attr('data-headerid');
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
        for (var i = 0; i < components.length; i++) {
            html += '<option value="' + components[i].id + '"' + (id == components[i].id ? ' selected="true"' : '') + '>' + components[i].name + '</option>';
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
        for (var i = 0; i < components.length; i++) {
            if (components[i].id == id) {
                return components[i];
            }
        }
        return null;
    }

    // End filter methods

});