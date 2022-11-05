var filters = [];
var currentFilters = [];
var availableFilters = [];
var updateFilterIndex = -1;

$(document).ready(function () {
    $('#addFilter').on('click', function () {
        var col = $('#filterFields > option:selected').val();
        var equality = parseInt($('#filterEqualities > option:selected').val());
        var value = $('#filterValue').val();
        var link = parseInt($('#filterLink > option:selected').val());
        var group = $('#filterGrouped').is(':checked');
        currentFilters.push({ ActingOn: col, Test: equality, Value: value, Link: link, GroupNext: group, Priority: currentFilters.nextPriority() });
        SetUpFilters();
        ClearFilterFields();
    });

    $('#updateFilter').hide();
    $('#updateFilter').on('click', function () {
        $('#updateFilter').hide();
        $('#addFilter').show();
        var col = $('#filterFields > option:selected').val();
        var equality = parseInt($('#filterEqualities > option:selected').val());
        var value = $('#filterValue').val();
        var link = parseInt($('#filterLink > option:selected').val());
        var group = $('#filterGrouped').is(':checked');
        currentFilters[updateFilterIndex].ActingOn = col;
        currentFilters[updateFilterIndex].Test = equality;
        currentFilters[updateFilterIndex].Value = value;
        currentFilters[updateFilterIndex].Link = link;
        currentFilters[updateFilterIndex].GroupNext = group;
        SetUpFilters();
        ClearFilterFields();
    });

    $('#clearFilters').on('click', function () {
        while (currentFilters.length > 0) {
            currentFilters.pop();
        }
        SetUpFilters();
    });
});

function ClearFilterFields() {
    $('#filterEqualities').val('0');
    $('#filterValue').val('');
    $('#filterLink').val('0');
    $('#filterGrouped').attr('checked', false);
}

function SetUpFilters() {
    availableFilters = [];
    $('#filterFields').html('');
    for (var i = 0; i < filters.length; i++) {
        //if (currentFilters.find(filters[i]) == -1) {
            availableFilters.push(filters[i]);
            $('#filterFields').append('<option value="' + filters[i] + '">' + filters[i] + '</option>');
        //}
    }
    var groupNum = 0;
    var group = '<div class="row"><div class="filter-group col-sm-8 col-sm-offset-2"><span class="filter-group-heading">Group ';
    var groupHtml = '';
    var continueGroup = false;
    for (var i = 0; i < currentFilters.length; i++) {
        if (!continueGroup) {
            groupHtml += group + ++groupNum + '</span>';
        }

        groupHtml += '<div class="row"><div class="col-sm-10 col-sm-offset-1 filter" data-index="' + i + '"><div class="col-xs-1"><span class="glyphicon glyphicon-filter"></span></div><div class="col-xs-10">' + currentFilters[i].ActingOn + ' ';
        switch (currentFilters[i].Test) {
            case 0:
                groupHtml += " EQUALS ";
                break;
            case 1:
                groupHtml += " NOT EQUALS ";
                break;
            case 2:
                groupHtml += " GREATER THAN ";
                break;
            case 3:
                groupHtml += " GREATER THAN OR EQUAL TO ";
                break;
            case 4:
                groupHtml += " LESS THAN ";
                break;
            case 5:
                groupHtml += " LESS THAN OR EQUAL TO ";
                break;
            case 6:
                groupHtml += " IN ";
                break;
            case 7:
                groupHtml += " NOT IN ";
                break;
            case 8:
                groupHtml += " STARTS WITH ";
                break;
            case 9:
                groupHtml += " CONTAINS ";
                break;
            case 10:
                groupHtml += " ENDS WITH ";
                break;
        }
        groupHtml += currentFilters[i].Value;
        if (currentFilters[i].Link == 0 && i < currentFilters.length - 1) {
            currentFilters[i].Link = 1;
        }
        switch (currentFilters[i].Link) {
            case 0:
                break;
            case 1:
                groupHtml += " AND";
                break;
            case 2:
                groupHtml += " OR";
                break;
        }
        groupHtml += '</div></div></div>';
        if (!currentFilters[i].GroupNext) {
            groupHtml += '</div></div>';
            continueGroup = false;
        } else {
            continueGroup = true;
        }
    }
    if (currentFilters.length < 1 || currentFilters[currentFilters.length - 1].GroupNext) groupHtml += '</div></div>';
    if (currentFilters.length == 0) grouptHtml = '';
    $('#activeFilters').html(groupHtml);
    $('.filter').on('click', function () {
        EditFilter(parseInt($(this).attr('data-index')));
    });
}

function EditFilter(index) {
    $('#filterFields').val(currentFilters[index].ActingOn);
    $('#filterEqualities').val(currentFilters[index].Test);
    $('#filterValue').val(currentFilters[index].Value);
    $('#filterLink').val(currentFilters[index].Link);
    $('#filterGrouped').prop('checked', currentFilters[index].GroupNext);
    $('#updateFilter').show();
    $('#addFilter').hide();
    updateFilterIndex = index;
}

currentFilters.find = function (column) {
    for (var i = 0; i < this.length; i++) {
        if (this[i].ActingOn == column) {
            return i;
        }
    }
    return -1;
};

currentFilters.nextPriority = function () {
    if (this.length == 0) return 1;
    this.sort(compareFilter);
    this.checkPriorities();
    return (this[this.length - 1].Priority + 1);
};

currentFilters.checkPriorities = function () {
    var p = 0;
    for (var i = 0; i < this.length; i++) {
        if (p == this[i].Priority) {
            this[i].Priority = ++p;
        } else {
            p = this[i].Priority;
        }
    }
};

function compareFilter(a, b) {
    return a.Priority - b.Priority;
}