$(document).ready(function () {

    $('#headers').sortable({
        containerSelector: 'table',
        itemPath: '> tbody',
        itemSelector: 'tr',
        handle: 'span.icon-move',
        placeholder: '<tr class="placeholder"/>',
        onDrop: function (item, targetContainer, _super) {
            _super(item);
            $('#headers').find('input').each(function (i) {
                $(this).attr('name', 'Variables[' + i + ']');
            });
        },
    });

    $('.filter-actionon').on('change', function () {
        var index = parseInt($(this).parents('tr').attr('data-index'));
        ActingOnChange($('tr[data-index="' + index + '"]'), $(this));
    });

    $('#addHeader').on('click', function (e) {
        e.preventDefault();
        var header = {};
        header.UId = $('#header').val();
        header.Name = $('#header option:selected').text();
        if ($('.variable[data-id="' + header.UId + '"]').length > 0)
            return;
        variableIndex++;
        $('#headers').append('<tr><td class="variable" data-id="' + header.UId + '"><span class="glyphicon glyphicon-move icon-move"></span> ' + header.Name + '<input type="hidden" name="Variables[' + variableIndex + ']" value="' + header.UId + '" /></td><td><a href="#" class="delete-variable"><span class="glyphicon glyphicon-trash"></span></a></td></tr>');
        var tr = $('.variable[data-id="' + header.UId + '"]').parent();
        $('#sortOn').append('<option value="' + header.UId + '">' + header.Name + '</option>');
        tr.find('.delete-variable').on('click', function (e) {
            DeleteVariable($(this).parent().parent(), e);
        });
    }); 

    $('.delete-variable').on('click', function (e) {
        DeleteVariable($(this).parent().parent(), e);
    });

    $('#ungroup').on('click', function (e) {
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
            if ($(this).is(':checked') && !skipped)
            {
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
        html += '<td><select class="filter-actionon form-control" name="Filters[' + filterIndex + '].ActingOn">';
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
        html += '</select></td>';
        html += '<td><select class="filter-test form-control" name="Filters[' + filterIndex + '].Test">';
        for (var i = 0; i < tests.length; i++) {
            html += '<option value="' + tests[i].Index + '">' + tests[i].Name + '</option>';
        }
        html += '</select></td>';
        html += '<td>';
        html += GenerateValueSelection("default", filterIndex, "");
        html += '</td>';
        html += '</tr>';
        $('#filterTable').children('tbody').append(html);
        $('.filter[data-index="' + filterIndex + '"]').find('.filter-actionon').on('change', function () {
            var index = parseInt($(this).parents('tr').attr('data-index'));
            ActingOnChange($('tr[data-index="' + index + '"]'), $(this));
        });
        $('.filter[data-index="' + filterIndex + '"]').find('.delete-statement').on('click', function (e) {
            DeleteStatement($(this).parents('tr')[0], e);
        });
    });
    
});

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

function ActingOnChange(tr, input) {
    tr.find('.filter-value').replaceWith(GenerateValueSelection(input.val(), parseInt(tr.attr('data-index'))));
    $('.datepicker').datetimepicker(
        {
            autoclose: true
        });
}

function GenerateValueSelection(id, index, value)
{
    if (typeof (value) == 'undefined' || value === null)
        value = "";
    component = findComponent(id);
    type = id;
    if (component !== null)
        type = component.Type;
    var html = '';
    switch (type)
    {
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
            for (var i = 0; i < c_items.length; i ++) {
                html += '<option value="' + c_items[i].UId + '" ' + (value == c_items[i].UId ? 'selected="true"' : '') + '>' + c_items[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "radiogroup":
            var r_items = component.Items;
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < r_items.length; i ++) {
                html += '<option value="' + r_items[i].UId + '" ' + (value == r_items[i].UId ? 'selected="true"' : '') + '>' + r_items[i].Name + '</option>';
            }
            html += '</select>';
            break;
        case "dropdowngroup":
            var d_items = component.Items;
            html += '<select class="filter-value form-control" name="Filters[' + index + '].Value">';
            for (var i = 0; i < d_items.length; i ++) {
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

var emptyGuid = '00000000-0000-0000-0000-000000000000';

function findComponent(id) {
    for (var i = 0; i < components.length; i++) {
        if (components[i].UId == id)
            return components[i];
    }
    return null;
}