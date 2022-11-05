var generateValueInput;
var generateActingInput;
var generateFilters
var colors = [
    "#FFFFB0",
    "#B0B0FF",
    "#68C473",
    "#C468B9",
    "#6669BA",
    "#BAB766"
];
var tests = [{ "Index": 0, "Name": "equals" }, { "Index": 1, "Name": "greater than" }, { "Index": 2, "Name": "greater than or equal" }, { "Index": 3, "Name": "less than" }, { "Index": 4, "Name": "less than or equal" }, { "Index": 5, "Name": "not equals" }, { "Index": 6, "Name": "starts with" }, { "Index": 7, "Name": "does not start with" }, { "Index": 8, "Name": "ends with" }, { "Index": 9, "Name": "does not end with" }, { "Index": 10, "Name": "contains" }, { "Index": 11, "Name": "does not contain" }, { "Index": 12, "Name": "=rgx=" }, { "Index": 13, "Name": "!=rgx=" }, { "Index": 14, "Name": "in" }, { "Index": 15, "Name": "not in" }];
var colorIndex = -1;
var filterIndex = -1;
var OLDFILTER_VERSION = '1.1.0.0';

function RunFilters(generateFunction, generateActing, filterGeneration) {

    generateValueInput = generateFunction || GenerateValueSelection;
    generateActingInput = generateActing || GenerateActingSelection;
    generateFilters = filterGeneration || FilterGen;

    $('.delete-variable').on('click', function (e) {
        DeleteVariable($(this).parent(), e);
    });

    $('#unGroup').on('click', function (e) {
        e.preventDefault();
        $('.filter').css('background-color', 'inherit');
        $('.filter').find('input[name$="GroupNext"]').val('false');
        colorIndex = -1;
    });

    $('#groupTogether').on('click', function (e) {
        e.preventDefault();
        if ($('input.groupNext:checked').length > 1)
            colorIndex++;
        else
            return;
        if (colorIndex >= colors.length)
            colorIndex = 0;
        var found = false;
        var skipped = false;
        var lastItem = null;
        var count = 0;
        var changed = [];
        $('input.groupNext').each(function (i) {
            if (found)
                if (!($(this).is(':checked')))
                    skipped = true;
            if ($(this).is(':checked') && !skipped) {
                var tr = $(this).parent().parent().parent();
                count++;
                found = true;
                lastItem = tr.find('input[name$="GroupNext"]');
                lastItem.val('true');
                tr.css('background-color', colors[colorIndex]);
                changed.push(tr);
            }
        });
        if (count == 1) {
            changed[0].css('background-color', 'inherit');
            changed[o].find('input.groupNext').val('false');
        }
        lastItem.val('false');
        $('input.groupNext').prop('checked', false);
    })

    $('.delete-statement').on('click', function (e) {
        DeleteStatement($(this).parents('tr')[0], e);
    });

    $('#addStatement').on('click', function () {
        filterIndex++;
        var filter = new Filter();
        var order = filterIndex + 1;
        var html = '<tr class="filter" data-index="' + filterIndex + '"><td>';
        html += '<label><input type="checkbox" class="groupNext" value="true" /></label> <a href="#" class="delete-statement"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '<input type="hidden" name="Filters[' + filterIndex + '].GroupNext" value="false" />';
        html += '<input type="hidden" name="Filters[' + filterIndex + '].UId" value="' + emptyGuid + '" />';
        html += '<input type="hidden" name="Filters[' + filterIndex + '].Order" value="' + order + '" />';
        html += '</td>';
        html += '<td><select class="filter-link form-control" name="Filters[' + filterIndex + '].Link">';
        html += '<option value="0">None</option>';
        html += '<option value="1">And</option>';
        html += '<option value="2">Or</option>';
        html += '</select></td>';
        html += '<td>';
        html += generateActingInput();
        html += '</td>';
        html += '<td><select class="filter-test form-control" name="Filters[' + filterIndex + '].Test">';
        for (var i = 0; i < tests.length; i++) {
            html += '<option value="' + tests[i].Index + '">' + tests[i].Name + '</option>';
        }
        html += '</select></td>';
        html += '<td>';
        html += generateValueInput("default", filterIndex, "");
        html += '</td>';
        html += '</tr>';
        $('#filterTable').children('tbody').append(html);
        $('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
            var index = parseInt($(this).parents('tr').attr('data-index'));
            ActingOnChange($('tr[data-index="' + index + '"]'), $(this));
        });
        $('.filter[data-index="' + filterIndex + '"]').find('.delete-statement').on('click', function (e) {
            DeleteStatement($(this).parents('tr')[0], e);
        });
    });

}

function ActingOnChange(tr, input) {
    try {
        tr.find('.rating').rating('destroy');
    } catch (e) { }
    var span = tr.find('.filter-value');
    var html = generateValueInput(input.val(), parseInt(tr.attr('data-index')));
    $(span).replaceWith(html);
    try {
        tr.find('.rating').rating();
        tr.find('.datepicker').datetimepicker();
    } catch (e) { }
}

function FillFilters(filters) {
    generateFilters(filters);
}

function FilterGen(filters) {
    $('#filters').html('');
    var filterIndex = -1;
    var html = '';
    for (var i = 0; i < filters.length; i++) {
        filterIndex++;
        html += '<tr class="filter" data-index="' + filterIndex + '"><td>';
        html += '<label><input type="checkbox" class="groupNext" value="true" /></label> <a href="#" class="delete-statement"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '<input type="hidden" name="Filters[' + filterIndex + '].GroupNext" value="' + filters[i].GroupNext + '" />';
        html += '<input type="hidden" name="Filters[' + filterIndex + '].UId" value="' + filters[i].UId + '" />';
        html += '<input type="hidden" name="Filters[' + filterIndex + '].Order" value="' + filters[i].Order + '" />';
        html += '</td>';
        html += '<td><select class="filter-link form-control" name="Filters[' + filterIndex + '].Link">';
        html += '<option value="0"' + (filters[i].Link == 0 ? ' selected="true"' : '') + '>None</option>';
        html += '<option value="1"' + (filters[i].Link == 1 ? ' selected="true"' : '') + '>And</option>';
        html += '<option value="2"' + (filters[i].Link == 2 ? ' selected="true"' : '') + '>Or</option>';
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
        html += generateValueInput("default", filterIndex, filters[i].Value);
        html += '</td>';
        html += '</tr>';
        $('#filterTable').children('tbody').append(html);
        $('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
            var index = parseInt($(this).parents('tr').attr('data-index'));
            ActingOnChange($('tr[data-index="' + index + '"]'), $(this));
        });
        $('.filter[data-index="' + filterIndex + '"]').find('.delete-statement').on('click', function (e) {
            DeleteStatement($(this).parents('tr')[0], e);
        });
    }
}

function GenerateActingSelection() {
    var html = '<select class="filter-actingon form-control" name="Filters[' + filterIndex + '].ActingOn">';
    html += '<option value="email">Email</option>';
    html += '<option value="confirmation">Confirmation</option>';
    html += '<option value="status">Status</option>';
    html += '<option value="dateregistered">Date Registered</option>';
    html += '<option value="type">Type</option>';
    html += '<option value="audience">Audience</option>';
    html += '<option value="rsvp">RSVP</option>';
    for (var i = 0; i < components.length; i++) {
        html += '<option value="' + components[i].UId + '">' + components[i].Name + '</option>';
    }
    return html;
}

function PopulateFilters() {
    var filters = [];
    $('.filter').each(function (i) {
        var tr = $(this);
        var filter = {};
        filter.GroupNext = tr.find('input[name$="GroupNext"]').val();
        filter.UId = tr.find('input[name$="UId"]').val();
        filter.Order = tr.find('input[name$="Order"]').val();
        filter.Link = tr.find('.filter-link').val();
        filter.ActingOn = tr.find('.filter-actingon').val();
        filter.Test = tr.find('.filter-test').val();
        filter.Value = tr.find('.filter-value').val();
        filter.SortingId = -1;
        filters.push(filter);
    });
    return filters;
}

function DeleteStatement(tr, event) {
    event.preventDefault();
    $(tr).remove();
    $('.filter').css('background-color', 'inherit');
    $('.filter').find('input[name$="GroupNext"]').val('false');
    colorIndex = -1;
    $('tr.filter').each(function (i) {
        filterIndex = i;
        $(this).find('input').each(function (j) {
            var name = $(this).attr('name');
            if (typeof (name) == 'undefined')
                return;
            name = name.replace(/\[\d+\]/, '[' + filterIndex + ']');
            $(this).attr('name', name);
        });
        $(this).find('input[name$="Order"]').val(filterIndex + 1);
    });
    filterIndex--;
}

function DeleteVariable(div, event) {
    event.preventDefault();
    div.remove();
    variableIndex = -1;
    $('#sortOn option[value="' + div.attr('data-id') + '"]').remove()
    $('.variable').each(function (i) {
        variableIndex++;
        $(this).find('input').attr('name', 'Variables[' + variableIndex + ']');
    });
}

function GenerateValueSelection(id, index, value) {
    if (typeof (value) == 'undefined' || value === null)
        value = "";
    component = findComponent(id);
    type = id;
    if (component !== null)
        type = component.Type;
    var html = '';
    switch (type) {
        case "audience":
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < audiences.length; i++) {
                html += '<option value="' + audiences[i].UId + '"' + (value == audiences[i].UId ? 'selected="true"' : '') + '>' + audiences[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "type":
            html += '<select class="form-control filter-value" class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < types.length; i++) {
                html += '<option value="' + types[i].Index + '"' + (value == (types[i].Index) ? 'selected="true"' : '') + '>' + types[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "status":
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < statusi.length; i++) {
                html += '<option value="' + statusi[i].Index + '"' + (value == (statusi[i].Index) ? 'selected="true"' : '') + '>' + statusi[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "rsvp":
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            html += '<option value="true" ' + (value == 'true' ? 'selected="true"' : '') + '>Accepted</option>';
            html += '<option value="false" ' + (value == 'false' ? 'selected="true"' : '') + '>Declined</option>';
            html += '</select>';
            break;
        case "dateregistered":
            html += '<input class="filter-value datepicker form-control" name="Filters[' + index + '].Value" value="" />';
            break;
        case "checkboxgroup":
            var c_items = component.Items;
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < c_items.length; i++) {
                html += '<option value="' + c_items[i].UId + '" ' + (value == c_items[i].UId ? 'selected="true"' : '') + '>' + c_items[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "radiogroup":
            var r_items = component.Items;
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < r_items.length; i++) {
                html += '<option value="' + r_items[i].UId + '" ' + (value == r_items[i].UId ? 'selected="true"' : '') + '>' + r_items[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "dropdowngroup":
            var d_items = component.Items;
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < d_items.length; i++) {
                html += '<option value="' + d_items[i].UId + '" ' + (value == d_items[i].UId ? 'selected="true"' : '') + '>' + d_items[i].Name + '</option>';
            }
            html += '</select>';
            break;
        default:
            html += '<input type="text" class="filter-value form-control" name="Filters[' + index + '].Value" value="' + value + '" />';
    }
    return html;
}

var Filter = function () {
    this.Link = 0;
    this.GroupNext = false;
    this.Order = -1;
    this.Test = 0;
    this.ActingOn = '';
    this.Value = '';
    this.SortingId = -1;
    this.UId = '';
};

var Link = function () {
    this.None = 0;
    this.And = 1;
    this.Or = 2;
};

var LinkString = [
    'None',
    'And',
    'Or'
]

var Test = function () {
    this.Equal = 0,
    this.GreaterThan = 1,
    this.GreaterThanOrEqual = 2,
    this.LessThan = 3,
    this.LessThanOrEqual = 4,
    this.NotEqual = 5,
    this.StartsWith = 6,
    this.NotStartsWith = 7,
    this.EndsWith = 8,
    this.NotEndsWith = 9,
    this.Contains = 10,
    this.DoesNotContain = 11,
    this.RegexMatch = 12,
    this.RegexNotMatch = 13,
    this.In = 14,
    this.NotIn = 15
}

var TestString = [
    'Equal',
    'GreaterThan',
    'GreaterThanOrEqual',
    'LessThan',
    'LessThanOrEqual',
    'NotEqual',
    'StartsWith',
    'NotStartsWith',
    'EndsWith',
    'NotEndsWith',
    'Contains',
    'DoesNotContain',
    'RegexMatch',
    'RegexNotMatch',
    'In',
    'NotIn'
];

var Filter = function () {
    this.SortingId = -1;
    this.UId = '';
    this.ReportKey = '';
    this.SingleFormReportKey = '';
    this.Link = 0;
    this.Test = 0;
    this.ActingOn = '';
    this.Value = '';
    this.Order = 1;
    this.GroupNext = false;
}

var emptyGuid = '00000000-0000-0000-0000-000000000000';

function findComponent(id) {
    for (var i = 0; i < components.length; i++) {
        if (components[i].UId == id)
            return components[i];
    }
    return null;
}

function generateFilterModal() {
    var t_html = '<div class="modal fade" id="filterModal"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Filters</h3></div>';
    t_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Filters</h4></div>';
    t_html += '<div class="row color-grey-2 add-padding-vertical-5"><div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" id="groupTogether"><span class="glyphicon glyphicon-link"></span> Group Selected</a></div></div>';
    t_html += '<div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" id="unGroup"><span class="glyphicon glyphicon-link"></span> Ungroup All</a></div></div></div>';
    t_html += '<div class="row"><table class="table table-filter filter-table" id="filterTable">';
    t_html += '<thead><tr><th></th><th>Link</th><th>Variable</th><th>Test</th><th>Value</th></tr></thead>';
    t_html += '<tbody id="filters"></tbody><tfoot><tr><td colspan="5"><a href="#" id="addStatement"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    t_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default" id="setFilters">Apply Filters</button>';
    t_html += '</div></div></div>';
    $('body').append(t_html);
}