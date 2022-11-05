/* Survey Report
 * Written By:   D.J. Enzyme
 * Date Created: 20141014
 * Version:      1.0.0.0
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

var SURVEYREPORT_VERSION = null;

$(document).on('ready', function () {
    SURVEYREPORT_VERSION = '1.0.0.0';
    var table = new JTable();
    //var rawTable = new JTable();

    if (typeof (RESTFUL) === 'undefined') {
        throw 'restful.js must be used.'
    }

    if (typeof (jQuery) === 'undefined') {
        throw 'jquery must be used.';
    }

    if (typeof (processing) === 'undefined') {
        throw 'processing.js must be used.';
    }

    // Initiation

    $.extend(table, rawTable);
    var sorting = new Sorting(table);
    var filter = new Filter(table);
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
        xhr.open('post', '../Create/SurveyReport');
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
        prettyProcessing.showPleaseWait('Creating Survey Report', 'Creating Report');
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

    // End Initiation

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