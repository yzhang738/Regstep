/*! JTable v1.3.1.0         */ 
/*! Created By: D.J. Enzyme */ 
/*! Creation Date: 20141229 */ 
/*! Modified Date: 20150331 */
// This script requires versioning 1.0.0.0 or later;

/// <reference path="../../linq.js" />
/// <reference path="../../Sorting/advSorting.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />
/// <reference path="../Chartv2.min.js" />
/// <reference path="../moment.js" />
/// <reference path="../Bootstrap/Plugins/rating.js" />
/// <reference path="breadCrumb.js" />
/// <reference path="../../versioning.js" />

/* global Chart */
/* global prettyProcessing */
/* global RESTFUL */
/* global Sorting */
/* global Filter */
/* global BREADCRUMB_CURRENT */
/* global UpdateCrumb */
/* jshint unused:false */
/* global moment */

var JTables = {};
JTables.Version = new Version(1, 3, 1, 0);
JTables.tableIndex = -1;

Chart.defaults.global.responsive = true;

var graphColors = [
    { color: '#8B2323', highlight: '#D04545' },
    { color: '#00688B', highlight: '#00BEFE' },
    { color: '#006400', highlight: '#008000' },
    { color: '#FBEC5D', highlight: '#FCF18B' },
    { color: '#2E0854', highlight: '#6F13CB' }
];

if (!String.prototype.root) {
    String.prototype.root = function (root) {
        /// <signature>
        /// <summary>Returns the root of the strin up to the supplied root ending.</summar>
        /// <param name="root" type="String">Grabs everything to the left of the first occurence of this string.</param>
        /// <return type="string">
        /// </signature>
        "use strict";
        var rootIndex = this.indexOf(root);
        if (rootIndex === -1) {
            return this;
        } else {
            return this.substring(0, rootIndex);
        }
    };
}
if (!Math.roundAdv) {
    Math.roundAdv = function (number, decimals) {
        /// <signature>
        /// <summary>Returns a roounded number with up to the amount of decimals supplied.</summar>
        /// <param name="number" type="Number">The number to round.</param>
        /// <param name="number" type="Number" integer="true">The number to o decimal places to round to.</param>
        /// <return type="Number">
        /// </signature>
        "use strict";
        if (isNaN(number)) {
            return 0;
        }
        return +(Math.round(number + "e+" + decimals) + "e-" + decimals);
    };
}

function JTable(tableId, minimal) {
    /// <signature>
    /// <summary>Constructs a new JTable object.</summary>
    /// <returns type="JTable" />
    /// <field name="Table" type="String">The jquery selector for the table.</field>
    /// <field name="LastFullCheck" type="moment">The last time a check for updated data was conducted.</field>
    /// <field name="SortingObject" type="Sorting">The sorting class to use for the table.</field>
    /// <field name="FilterObject" type="Filter">The filter class to use for the table.</field>
    /// <field name="Options" type="Object">A list of options.</field>
    /// <field name="Average" type="Boolean">Represents if we are looking for avarages.</field>
    /// <field name="Graph" type="Boolean">Represents if we are looking for avarages represented by graphs.</field>
    /// <field name="Headers" type="Array" elementType="JTableHeader">Array of type JTableHeader.</field>
    /// <field name="Rows" type="Array" elementType="JTableRow">Array of type JTableRow.</field>
    /// <field name="Parent" type="String">The name of the parent owning the table.</field>
    /// <field name="Name" type="String">The name of the table.</field>
    /// <field name="Description" type="String">A description of the table</field>
    /// <field name="Id" type="String">The id of the table.</field>
    /// <field name="RecordsPerPage" type="Number">The number of records per page.</field>
    /// <field name="TotalRecords" type="Number">The total number of records.</field>
    /// <field name="Sortings" type="Array" elementType="JTableSorting">Array of type JTableSorting.</field>
    /// <field name="Filters" type="Array" elementType="JTableFilter">Array of type JTableFilter.</field>
    /// <field name="Page" type="Number">The page currently being viewed.</field>
    /// <field name="Filtered" type="Boolean">Represents the filtered state of the table.</field>
    /// <field name="Sorted" type="Boolean">Represents the sorting state of the table.</field>
    /// <field name="SavedId" type="String">The id of the saved file.</field>";
    /// <field name="Favorite" type="Boolean">Is the report a favorite.</field>";
    /// <field name="FilteredRows" type="Array" elementType="JTableRow">Array of type JTableRow that has been filtered from the original Rows.</field>
    /// </signature>
    "use strict";
    var tableIndex = ++JTables.tableIndex;
    var min_layout = false;
    if (typeof (minimal) === 'boolean') {
        min_layout = minimal;
    }
    this.Table = tableId;
    if (typeof (tableId) === 'undefined' || tableId == null) {
        this.Table = '#jTable' + tableIndex;
    }
    var thisTable = this;
    var currentHeader = '';
    //#region events
    if (!min_layout) {
        $('body').append(GenerateHeaderDiv('headerSelection' + tableIndex));
        $('#headerSelection' + tableIndex).find('.add-header').on('click', function (e) {
            e.preventDefault();
            var t_modal = $('#headerSelection' + tableIndex);
            t_modal.find('.header-available:checked').each(function () {
                var t_id = $(this).val();
                var found = false;
                for (var j = 0; j < thisTable.Headers.length; j++) {
                    var t_header = thisTable.Headers[j];
                    if (t_header.Id === t_id) {
                        found = true;
                        t_header.Removed = false;
                    }
                }
                if (!found) {
                    return;
                }
                var t_label = $(this).closest('label');
                t_label.find('input').removeClass('header-available').addClass('header-selected');
                t_label.parent().appendTo('.headers-container-selected');
                $('.headers-container-selected .header-move').show();
                $('.headers-container-selected .header-div').first().find('.header-up').hide();
                $('.headers-container-selected .header-div').last().find('.header-down').hide();
            });
            t_modal.find('.header-item').prop('checked', false);
        });
        $('#headerSelection' + tableIndex).find('.remove-header').on('click', function (e) {
            e.preventDefault();
            var t_modal = $('#headerSelection' + tableIndex);
            $('.header-selected:checked').each(function () {
                var t_id = $(this).val();
                var found = false;
                for (var j = 0; j < thisTable.Headers.length; j++) {
                    var t_header = thisTable.Headers[j];
                    if (t_header.Id === t_id) {
                        found = true;
                        t_header.Removed = true;
                    }
                }
                if (!found) {
                    return;
                }
                var t_label = $(this).closest('label');
                t_label.find('input').removeClass('header-selected').addClass('header-available');
                t_label.parent().appendTo('.headers-container-available');
                t_label.parent().find('.header-move').hide();
            });
            $('.header-item').prop('checked', false);
        });
        $('#headerSelection' + tableIndex).on('show.bs.modal', function () {
            var t_this = $(this);
            t_this.find('.headers-container-selected').html('');
            t_this.find('.headers-container-available').html('');
            var headers = thisTable.Headers.sort(function (a, b) { return a.Order - b.Order; });
            for (var i = 0; i < headers.length; i++) {
                var t_header = headers[i];
                if (t_header.Id === 'confirmation') {
                    t_this.find('.headers-container-selected').append('<label style="display: none;"><input type="checkbox" value="' + t_header.Id + '" class="header-item header-selected" />' + t_header.Label + '</label>');
                    continue;
                }
                if (t_header.Removed) {
                    t_this.find('.headers-container-available').append('<div class="header-div" data-id="' + t_header.Id + '"><label><input type="checkbox" value="' + t_header.Id + '" class="header-item header-available" />' + t_header.Label + '</label> <span class="header-move header-up cursor-pointer glyphicon glyphicon-arrow-up" data-id="' + t_header.Id + '"></span> <span class="header-move header-down cursor-pointer glyphicon glyphicon-arrow-down" data-id="' + t_header.Id + '"></span></div>');
                } else {
                    t_this.find('.headers-container-selected').append('<div class="header-div" data-id="' + t_header.Id + '"><label><input type="checkbox" value="' + t_header.Id + '" class="header-item header-selected" />' + t_header.Label + '</label> <span class="header-move header-up cursor-pointer glyphicon glyphicon-arrow-up" data-id="' + t_header.Id + '"></span> <span class="header-move header-down cursor-pointer glyphicon glyphicon-arrow-down" data-id="' + t_header.Id + '"></span></div>');
                }
            }
            t_this.find('.headers-container-available .header-move').hide();
            t_this.find('.headers-container-selected .header-div').first().find('.header-up').hide();
            t_this.find('.headers-container-selected .header-div').last().find('.header-down').hide();
            $('.header-move').on('click', function () {
                var item = $(this);
                var id = item.attr('data-id');
                var t_div = $(this).parent();
                var prev = t_div.prev();
                var next = t_div.next();
                if (item.hasClass('header-up')) {
                    if (prev.length === 0) {
                        return;
                    }
                    t_div.insertBefore(prev);
                } else {
                    if (next.length === 0) {
                        return;
                    }
                    t_div.insertAfter(next);
                }
                $('.headers-container-selected .header-move').show();
                $('.headers-container-selected .header-div').first().find('.header-up').hide();
                $('.headers-container-selected .header-div').last().find('.header-down').hide();
            });
        });
        $('#headerSelection' + tableIndex).find('.headers-set').on('click', function () {
            $(this).closest('.modal').modal('hide');
            var t_order = 0;
            $('.headers-container-selected .header-div').each(function () {
                var header = FindHeader(thisTable, $(this).attr('data-id'));
                if (header !== null) {
                    header.Order = ++t_order;
                }
            });
            $('.headers-container-available .header-div').each(function () {
                var header = FindHeader(thisTable, $(this).attr('data-id'));
                if (header !== null) {
                    header.Order = ++t_order;
                }
            });
            thisTable.GetPage();
        });
        $('.jTable_headers-edit[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            $('#headerSelection' + tableIndex).modal('show');
        });
        $('.jTable_pageLeft[data-jtable-target="' + this.Table + '"]').on('click', function () {
            if (thisTable.Page === 1) {
                return;
            }
            thisTable.Page--;
            $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').val(thisTable.Page);
            thisTable.GetPage();
        });
        $('.jTable_pageRight[data-jtable-target="' + this.Table + '"]').on('click', function () {
            if (thisTable.Page === thisTable.TotalPages()) {
                return;
            }
            thisTable.Page++;
            $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').val(thisTable.Page);
            thisTable.GetPage();
        });
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Page = parseInt($(this).val());
            thisTable.GetPage();
        });
        $('.jTable_recordsPerPage[data-jtable-target="' + this.Table + '"]').on('blur', function () {
            var val = $(this).val();
            if (isNaN(val)) {
                $(this).val(25);
                thisTable.RecordsPerPage = 25;
                thisTable.GetPage();
            } else {
                thisTable.RecordsPerPage = parseInt(val);
                thisTable.GetPage();
            }
        });
        $('.jTable_average[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Average = $(this).prop('checked');
            thisTable.GetPage();
        });
        $('.jTable_graph[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Graph = $(this).prop('checked');
            thisTable.GetPage();
        });
        $('.jTable_count[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Count = $(this).prop('checked');
            thisTable.GetPage();
        });
        $('.jTable_sortings-clearAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.Sortings = [];
            thisTable.SortingObject.Generate();
            thisTable.Sorted = false;
            thisTable.GetPage();
        });
        $('.jTable_sortings-edit[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.SortingObject.Modal.modal('show');
        });
        $('.jTable_filters-clearAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.Filters = [];
            thisTable.FilterObject.Generate();
            thisTable.Filtered = false;
            thisTable.GetPage();
        });
        $('.jTable_filters-edit[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.FilterObject.Modal.modal('show');
        });
        $('.jTable_favorite[data-jtable-target="' + this.Table + '"]').on('click', function () {
            if (thisTable.Favorite) {
                $(this).html('<span class="glyphicon glyphicon-star-empty"></span> Favorite');
                thisTable.Favorite = false;
            } else {
                $(this).html('<span class="glyphicon glyphicon-star"></span> Un-Favorite');
                thisTable.Favorite = true;
            }
        });
        $('.jTable_headers-clearAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            for (var i = 0; i < thisTable.Headers.length; i++) {
                var t_header = thisTable.Headers[i];
                if (t_header.Id === 'confirmation') {
                    continue;
                }
                t_header.Removed = true;
            }
            thisTable.GetPage();
        });
        $('.jTable_headers-addAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            for (var i = 0; i < thisTable.Headers.length; i++) {
                var t_header = thisTable.Headers[i];
                t_header.Removed = false;
            }
            thisTable.GetPage();
        });
    }
    //#endregion
    //#region fields
    this.LastFullCheck = moment();
    this.SortingObject = new Sorting(this, null, min_layout);
    this.FilterObject = new Filter(this, null, min_layout);
    this.Options = {};
    this.Average = false;
    this.Graph = false;
    this.Count = false;
    this.Headers = [];
    this.Rows = [];
    this.Parent = 'New Table';
    this.Name = 'New Table';
    this.Description = '';
    this.Id = '';
    this.RecordsPerPage = 250;
    this.TotalRecords = 0;
    this.Sortings = [];
    this.Filters = [];
    this.Page = 1;
    this.Filtered = false;
    this.Sorted = false;
    this.SavedId = null;
    this.Favorite = false;
    this.FilteredRows = [];
    //#endregion
    //#region functions
    this.FilteredRecords = function () {
        /// <signature>
        /// <summary>Gets the total amount of filtered records.</summary>
        /// <returns type="Number" integer="true" />
        /// </signature>
        return this.FilteredRows.length;
    };
    this.TotalPages = function () {
        /// <signature>
        /// <summary>Gets the total amount of pages available</summary>
        /// <returns type="Number" />
        /// </signature>
        var pages = Math.ceil(this.FilteredRows.length / this.RecordsPerPage);
        if (pages === 0) {
            return 1;
        }
        return pages;
    };
    this.OnGetComplete = function (p_table) {
        /// <signature>
        /// <summary>Runs when the table has been rendered to the screen.</summary>
        /// </signature>
    };
    this.GetPage = function (table, page) {
        /// <signature>
        /// <summary>Get the rows for a certain page.</summary>
        /// <param name="table" type="String">The selector for the table tag to fill.</param>
        /// <param name="page" type="Number">The page to get.</param>
        /// </signature>
        if (typeof (table) === 'undefined') {
            table = this.Table;
        }
        if (!/^\./.test(table) && !/^#/.test(table)) {
            table = '#' + table;
        }
        if (typeof (page) === 'undefined') {
            page = this.Page;
        }
        if (!this.Filtered) {
            this.Filter();
        }
        if (!this.Sorted) {
            this.Sort();
        }
        if (page > this.TotalPages() && page !== 1) {
            page = 1;
        }
        var chartData = [];
        $('.jTable_pageSelect').html(thisTable.FilteredRecords());
        $('.jTable_pageSelect').html('');
        for (var ind = 0; ind < this.TotalPages() ; ind++) {
            $('.jTable_pageSelect').append('<option value="' + (ind + 1) + '" ' + (this.Page === (ind + 1) ? 'selected="selected"' : '') + '>' + (ind + 1) + '</option>');
        }
        var start = (page - 1) * this.RecordsPerPage;
        var rows = [];
        this.Headers.sort(function (a, b) { return (a.Order - b.Order); });
        if (this.Count) {
            rows = this.GetCount(this, chartData);
        } else if (this.Average) {
            rows = this.GetAverage(this, chartData);
        } else if (this.Graph) {
            rows = this.GetGraph(this, chartData);
        } else {
            rows = this.GetNormal(this, start);
        }
        var t_html = this.GenerateTableHtml(this, rows);
        $(table).html(t_html);
        //Render Charts
        for (var i = 0; i < chartData.length; i++) {
            var template = "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=+(Math.round(((segments[i].value / total) * 100) + 'e+2')  + 'e-2')%>%<%}%></li><%}%></ul>";
            if (this.Count) {
                template = "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=segments[i].value%><%}%></li><%}%></ul>";
            }
            var ctx = {};
            var chart = {};
            switch (chartData[i].type) {
                case 'pie':
                    ctx = $('#' + chartData[i].id).get(0).getContext("2d");
                    chart = new Chart(ctx).Pie(chartData[i].data, {
                        legendTemplate: template
                    });
                    var legend = chart.generateLegend();
                    $('#' + chartData[i].id).parent().parent().children('.legend').html(legend);
                    break;
                case 'bar':
                    var step = 10;
                    var max = 100;
                    var chartStart = 100;
                    var options = {};
                    var data = chartData[i].data;
                    if (this.Count) {
                        for (var j = 0; j < data.datasets[0].data.length; j++) {
                            if (max < data.datasets[0].data[j]) {
                                max = data.datasets[0].data[j];
                            }
                            if (chartStart > data.datasets[0].data[j]) {
                                chartStart = data.datasets[0].data[j];
                            }
                        }
                        chartStart = (chartStart - 10) - (chartStart % 10);
                        if (chartStart < 0) {
                            chartStart = 0;
                        }
                    } else {
                        chartStart = 0;
                    }
                    options.scaleOverride = true;
                    options.scaleSteps = Math.ceil((max - chartStart) / step);
                    options.scaleStepWidth = step;
                    options.scaleStartValue = chartStart;
                    ctx = $('#' + chartData[i].id).get(0).getContext("2d");
                    chart = new Chart(ctx).Bar(chartData[i].data, options);
                    break;
            }
        }
        //Run Bindings
        //Allow sorting of the table.
        this.CreateLinks();
        //Add context menu to remove items.
        this.OnGetComplete(this);
    };
    this.CreateLinks = function () {
        /// <signature>
        /// <summary>Turns links into actual clickable links instead of dead refresh links.</summary>
        /// </signature>
        //Create href links
        $(this.Table).find('a').each(function () {
            var t_a = $(this);
            if (t_a.attr('href') === '#') {
                var action = t_a.attr('data-action');
                var controller = t_a.attr('data-controller');
                if (typeof (action) === 'undefined') {
                    action = null;
                }
                if (typeof (controller) === 'undefined') {
                    controller = null;
                }
                var rawOptions = t_a.attr('data-options');
                if (typeof (rawOptions) === 'undefined') {
                    rawOptions = '{}';
                }
                var options = JSON.parse(rawOptions);
                if (action !== null && controller !== null) {
                    var t_href = window.location.origin + '/' + controller + '/' + action + '?';
                    var keys = Object.keys(options);
                    for (var j = 0; j < keys.length; j++) {
                        t_href += keys[j] + '=' + options[keys[j]];
                        if (j !== keys.length - 1) {
                            t_href += '&';
                        }
                    }
                    t_a.attr('href', t_href);
                }
            }
        });

    };
    this.AddSorting = function (sorting) {
        /// <signature>
        /// <summary>Adds a sorting to the array Sortings and sets the appropriate order number.</summary>
        /// <param name="sorting" type="JTableSorting">The JTableSorting to add to the array Sortings</param>
        /// </signature>
        sorting.Order = this.Sortings.length + 1;
        this.Sortings.push(sorting);
        this.Sorted = false;
    };
    this.ClearSortings = function () {
        /// <signature>
        /// <summary>Clears the Sortings array</summary>
        /// </signature>
        this.Sortings = [];
        this.Sorted = false;
    };
    this.ClearFilters = function () {
        /// <signature>
        /// <summary>Clears the Filters array</summary>
        /// </signature>
        this.Filters = [];
        this.Filtered = false;
    };
    this.AddFilter = function (filter) {
        /// <signature>
        /// <summary>Adds a filter to the array Filters and sets the appropriate order number.</summary>
        /// <param name="filter" type="JTableFilter">The JTableFilter to add to the array Filters</param>
        /// </signature>
        filter.Order = this.Filters.length + 1;
        this.Filters.push(filter);
        this.Filtered = false;
    };
    this.Filter = function () {
        /// <signature>
        /// <summary>Filters the data.</summary>
        /// </signature>
        this.FilteredRows = [];
        this.Filters.sort(function (a, b) { return a.Order - b.Order; });
        
        //*
        for (var i = 0; i < this.Rows.length; i++) {
            var take = false;
            var grouping = true;
            var tests = [];
            var row = this.Rows[i];
            for (var j = 0; j < this.Filters.length; j++) {
                var filter = this.Filters[j];
                var groupTest = true;
                var first = true;
                grouping = filter.GroupNext;
                var test = false;
                do {
                    if (!filter.GroupNext) {
                        grouping = false;
                    }
                    var data = null;
                    for (var si = 0; si < row.Columns.length; si++) {
                        if (row.Columns[si].HeaderId === filter.ActingOn) {
                            data = row.Columns[si];
                            break;
                        }
                    }
                    if (data !== null) {
                        test = TestValue(data, filter.Value.toLowerCase(), filter.Test, FindHeader(this, data.HeaderId));
                    }
                    switch (filter.Link) {
                        case 'and':
                            groupTest = groupTest && test;
                            break;
                        case 'or':
                            if (first) {
                                groupTest = test;
                            } else {
                                groupTest = groupTest || test;
                            }
                            break;
                        default:
                            groupTest = test;
                            break;
                    }
                    first = false;
                    if (!grouping) {
                        break;
                    }
                    j++;
                    if (j < this.Filters.length) {
                        filter = this.Filters[j];
                    } else {
                        break;
                    }
                } while (grouping);
                tests.push({ test: groupTest, link: ((j < (this.Filters.length - 1)) ? this.Filters[j + 1].Link : 'none') });
            }
            take = tests.length > 0 ? tests[0].test : true;
            for (var ind = 1; ind < tests.length; ind++) {
                switch (tests[ind - 1].link) {
                    case 'and':
                        take = take && tests[ind].test;
                        break;
                    case 'or':
                        take = take || tests[ind].test;
                        break;
                    default:
                        take = tests[ind].test;
                        break;
                }
            }
            if (take) {
                this.FilteredRows.push(row);
            }
        }
        if (this.Filters.length === 0) {
            this.FilteredRows = this.Rows;
        }
        //*/
        this.Page = 1;
        this.Filtered = true;
        this.Sort();
        $('.jTable_totalRecords[data-jtable-target="' + this.Table + '"]').html(this.FilteredRows.length);
    };
    this.Sort = function () {
        /// <signature>
        /// <summary>Sorts the data.</summary>
        /// </signature>
        if (!this.Filtered) {
            this.Filter();
        }
        if (this.Sortings.length > 0) {
            this.Sortings.sort(function (a, b) { return a.Order - b.Order; });
            var sortings = this.Sortings;
            this.FilteredRows.sort(function (a, b) {
                var index = 0;
                var result = 0;
                var t_header = FindHeader(thisTable, sortings[index].ActingOn);
                if (t_header === null) {
                    return 1;
                }
                var a_data = FindColumn(a, sortings[index].ActingOn);
                var b_data = FindColumn(b, sortings[index].ActingOn);
                var aCompValue = null;
                var bCompValue = null;
                if (a_data !== null && a_data.Value !== null) {
                    aCompValue = a_data.Value;
                }
                if (b_data !== null && b_data.Value !== null) {
                    bCompValue = b_data.Value;
                }
                if (t_header.SortByPretty) {
                    if (a_data != null) {
                        aCompValue = a_data.PrettyValue;
                    } else {
                        aCompValue = null;
                    }
                    if (b_data != null) {
                        bCompValue = b_data.PrettyValue;
                    } else {
                        bCompValue = null;
                    }
                }
                if (aCompValue === null) {
                    aCompValue = '';
                }
                if (bCompValue === null) {
                    bCompValue = '';
                }
                if (t_header.Id === 'confirmation') {
                    result = advSorting.HexSort(aCompValue, bCompValue, sortings[index].Ascending);
                } else {
                    result = advSorting.NaturalSort(aCompValue, bCompValue, sortings[index].Ascending);
                }
                if (result === 0) {
                    index++;
                    while (result === 0) {
                        if (index >= sortings.length) {
                            break;
                        }
                        a_data = FindColumn(a, sortings[index].ActingOn);
                        b_data = FindColumn(b, sortings[index].ActingOn);
                        aCompValue = a_data.Value;
                        bCompValue = b_data.Value;
                        if (t_header.SortByPretty) {
                            aCompValue = a_data.PrettyValue;
                            bCompValue = b_data.PrettyValue;
                        }
                        if (aCompValue === null) {
                            aCompValue = '';
                        }
                        if (bCompValue === null) {
                            bCompValue = '';
                        }
                        if (t_header.Id === 'confirmation') {
                            result = advSorting.HexSort(aCompValue, bCompValue, sortings[index].Ascending);
                        } else {
                            result = advSorting.NaturalSort(aCompValue, bCompValue, sortings[index].Ascending);
                        }
                        index++;
                    }
                }
                return result;
            });
        }
        for (var i = 0; i < this.FilteredRows.length; i++) {
            this.FilteredRows[i].Order = (i + 1);
        }
        this.Sorted = true;
    };
    this.GetAverage = function (table, chartData) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="chartData" type="Array">The chartData to write to for a graph.</field>
        /// </signature>
        // We are looking for averages, so we need to add them up.  This works for numbers, single selection components, and multiple selection components.
        var rows = [];
        var row = new JTableRow();
        for (var i = 0; i < table.Headers.length; i++) {
            if (table.Headers[i].Id === 'email') {
                table.Headers[i].Hidden = true;
                continue;
            } else {
                table.Headers[i].Hidden = false;
            }
            var column = new JTableColumn();
            var total = 0;
            var count = 0;
            var takeColumn = true;
            var totals = {};
            var keys = [];
            for (var ind = 0; ind < table.Headers[i].PossibleValues.length; ind++) {
                totals[table.Headers[i].PossibleValues[ind].Label] = 0;
                keys.push({ key: table.Headers[i].PossibleValues[ind].Label, total: 0 });
            }
            for (var j = 0; j < table.FilteredRows.length; j++) {
                var t_col = FindColumn(table.FilteredRows[j], table.Headers[i].Id);
                if (t_col !== null) {
                    switch (table.Headers[i].Type.root('=>')) {
                        case 'itemParent':
                            if (typeof (totals[t_col.PrettyValue]) === 'undefined') {
                                totals[t_col.PrettyValue] = 0;
                                keys.push({ key: t_col.PrettyValue, total: 0 });
                            }
                            totals[t_col.PrettyValue]++;
                            count++;
                            break;
                        case 'multipleSelection':
                            var t_values = JSON.parse(t_col.Value);
                            var t_valueDone = [];
                            for (var k = 0; k < t_values.length; k++) {
                                var t_done = false;
                                for (var m = 0; m < t_valueDone.length; m++) {
                                    if (t_valueDone[m] === t_values[k]) {
                                        t_done = true;
                                        break;
                                    }
                                }
                                if (t_done) {
                                    continue;
                                } else {
                                    t_valueDone.push(t_values[k]);
                                }
                                var t_id = t_values[k];
                                var possibleValue = null;
                                for (var l = 0; l < table.Headers[i].PossibleValues.length; l++) {
                                    if (table.Headers[i].PossibleValues[l].Id === t_id) {
                                        possibleValue = table.Headers[i].PossibleValues[l];
                                    }
                                }
                                if (possibleValue !== null) {
                                    totals[possibleValue.Label]++;
                                }
                            }
                            count++;
                            break;
                        case 'boolean':
                            if (t_col.Value === '1') {
                                totals.Yes++;
                            } else {
                                totals.No++;
                            }
                            count++;
                            break;
                        case 'number':
                        case 'rating':
                            total += parseFloat(t_col.Value);
                            count++;
                            break;
                        default:
                            takeColumn = false;
                            break;
                    }
                }
            }
            if (takeColumn) {
                column.HeaderId = table.Headers[i].Id;
                column.Id = table.Headers[i].Id;
                column.Value = total / count;
                column.PrettyValue = column.Value;
                var t_pretty = '';
                var t_data = [];
                var t_barDataSet = [];
                var t_barLabels = [];
                var c_index = -1;
                var t_html = '<table><tbody>';
                switch (table.Headers[i].Type.root('=>')) {
                    case 'multipleSelection':
                    case 'itemParent':
                    case 'boolean':
                        for (var iPind = 0; iPind < keys.length; iPind++) {
                            keys[iPind].total = totals[keys[iPind].key];
                        }
                        keys.sort(function (a, b) { return b.total - a.total; });
                        for (var iPind2 = 0; iPind2 < keys.length; iPind2++) {
                            c_index++;
                            var avg = (totals[keys[iPind2].key] / count) * 100;
                            if (c_index > 4) {
                                c_index = 0;
                            }
                            t_data.push({
                                value: totals[keys[iPind2].key],
                                color: graphColors[c_index].color,
                                highlight: graphColors[c_index].highlight,
                                label: keys[iPind2].key,
                            });
                            t_barLabels.push(keys[iPind2].key);
                            t_barDataSet.push(avg);
                            t_html += '<tr><td style="text-align: right; padding-right: 3px; font-weight: bold;">' + keys[iPind2].key + ':</td><td>' + avg.toFixed(2) + '%</td></tr>';
                        }
                        t_html += '</tbody></table>';
                        column.Value = t_html;
                        t_pretty = t_html;
                        if (table.Graph) {
                            if (table.Headers[i].Type === 'multipleSelection') {
                                var t_barData = {
                                    labels: t_barLabels,
                                    datasets: [{
                                        label: 'Selections',
                                        fillColor: "rgba(220,220,220,0.5)",
                                        strokeColor: "rgba(220,220,220,0.8)",
                                        highlightFill: "rgba(220,220,220,0.75)",
                                        highlightStroke: "rgba(220,220,220,1)",
                                        data: t_barDataSet
                                    }]
                                };
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="300" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_barData, type: 'bar' });
                            } else {
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="500" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_data, type: 'pie' });
                            }
                        }
                        column.PrettyValue = t_pretty;
                        row.Columns.push(column);
                        break;
                    case 'number':
                    case 'rating':
                        column.Value = Math.roundAdv(total / count, 2);
                        column.PrettyValue = column.Value;
                        row.Columns.push(column);
                        break;
                    default:
                        table.Headers[i].Hidden = true;
                }
            } else {
                table.Headers[i].Hidden = true;
            }
        }
        rows.push(row);
        return rows;
    };
    this.GetGraph = function (table, chartData) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="chartData" type="Array">The chartData to write to for a graph.</field>
        /// </signature>
        return this.GetAverage(table, chartData);
    };
    this.GetCount = function (table, chartData) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="chartData" type="Array">The chartData to write to for a graph.</field>
        /// </signature>
        var rows = [];
        // We are looking for counts, so we need to add them up.  This works for single selection components, and multiple selection components.
        var row = new JTableRow();
        for (var i = 0; i < table.Headers.length; i++) {
            if (table.Headers[i].Id === 'email') {
                table.Headers[i].Hidden = true;
                continue;
            } else {
                table.Headers[i].Hidden = false;
            }
            var column = new JTableColumn();
            var total = 0;
            var count = 0;
            var takeColumn = true;
            var totals = {};
            var keys = [];
            for (var ind = 0; ind < table.Headers[i].PossibleValues.length; ind++) {
                totals[table.Headers[i].PossibleValues[ind].Label] = 0;
                keys.push({ key: table.Headers[i].PossibleValues[ind].Label, total: 0 });
            }
            if (table.Headers[i].Type.root('=>') === 'rating') {
                var t_json = table.Headers[i].Type;
                t_json = t_json.split("=>")[1];
                var t_options = JSON.parse(t_json);
                for (var x = t_options.min; x < t_options.max; x += t_options.step) {
                    totals[x.toString()] = 0;
                    keys.push({ key: x.toString(), total: 0 });
                }
            }
            for (var j = 0; j < table.FilteredRows.length; j++) {
                var t_col = FindColumn(table.FilteredRows[j], table.Headers[i].Id);
                if (t_col !== null) {
                    switch (table.Headers[i].Type.root('=>')) {
                        case 'itemParent':
                        case 'rating':
                            var iP_value = t_col.PrettyValue;
                            iP_value = iP_value.replace(/(<[^>]*>[^<]*<[^>]*>)/g, '');
                            iP_value = iP_value.trim();
                            if (typeof (totals[iP_value]) === 'undefined') {
                                totals[iP_value] = 0;
                                keys.push({ key: iP_value, total: 0 });
                            }
                            totals[iP_value]++;
                            count++;
                            break;
                        case 'multipleSelection':
                            var t_values = JSON.parse(t_col.Value);
                            var t_valueDone = [];
                            for (var k = 0; k < t_values.length; k++) {
                                var t_done = false;
                                for (var m = 0; m < t_valueDone.length; m++) {
                                    if (t_valueDone[m] === t_values[k]) {
                                        t_done = true;
                                        break;
                                    }
                                }
                                if (t_done) {
                                    continue;
                                } else {
                                    t_valueDone.push(t_values[k]);
                                }
                                var t_id = t_values[k];
                                var possibleValue = null;
                                for (var l = 0; l < table.Headers[i].PossibleValues.length; l++) {
                                    if (table.Headers[i].PossibleValues[l].Id === t_id) {
                                        possibleValue = table.Headers[i].PossibleValues[l];
                                    }
                                }
                                if (possibleValue !== null) {
                                    totals[possibleValue.Label]++;
                                }
                            }
                            count++;
                            break;
                        case 'boolean':
                            if (t_col.Value === '1') {
                                totals.Yes++;
                            } else {
                                totals.No++;
                            }
                            count++;
                            break;
                        case 'number':
                            total += parseFloat(t_col.Value);
                            count++;
                            break;
                        default:
                            takeColumn = false;
                            break;
                    }
                }
            }
            if (takeColumn) {
                column.HeaderId = table.Headers[i].Id;
                column.Id = table.Headers[i].Id;
                column.Value = total;
                column.PrettyValue = column.Value;
                var t_pretty = '';
                var t_data = [];
                var t_barDataSet = [];
                var t_barLabels = [];
                var c_index = -1;
                var t_html = '<table><tbody>';
                switch (table.Headers[i].Type.root('=>')) {
                    case 'multipleSelection':
                    case 'itemParent':
                    case 'rating':
                    case 'boolean':
                        for (var rind = 0; rind < keys.length; rind++) {
                            keys[rind].total = totals[keys[rind].key];
                        }
                        if (table.Headers[i].Type.root('=>') !== 'rating') {
                            keys.sort(function (a, b) { return b.total - a.total; });
                        }
                        for (var rind2 = 0; rind2 < keys.length; rind2++) {
                            c_index++;
                            if (c_index > 4) {
                                c_index = 0;
                            }
                            t_data.push({
                                value: totals[keys[rind2].key],
                                color: graphColors[c_index].color,
                                highlight: graphColors[c_index].highlight,
                                label: keys[rind2].key,
                            });
                            t_barLabels.push(keys[rind2].key);
                            t_barDataSet.push(totals[keys[rind2].key]);
                            t_html += '<tr><td style="text-align: right; padding-right: 3px; font-weight: bold;">' + keys[rind2].key + ':</td><td>' + totals[keys[rind2].key] + '</td></tr>';
                        }
                        t_html += '</tbody></table>';
                        column.Value = t_html;
                        t_pretty = t_html;
                        if (table.Graph) {
                            if (table.Headers[i].Type === 'multipleSelection') {
                                var t_barData = {
                                    labels: t_barLabels,
                                    datasets: [{
                                        label: 'Selections',
                                        fillColor: "rgba(220,220,220,0.5)",
                                        strokeColor: "rgba(220,220,220,0.8)",
                                        highlightFill: "rgba(220,220,220,0.75)",
                                        highlightStroke: "rgba(220,220,220,1)",
                                        data: t_barDataSet
                                    }]
                                };
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="300" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_barData, type: 'bar' });
                            } else {
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="500" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_data, type: 'pie' });
                            }
                        }
                        column.PrettyValue = t_pretty;
                        row.Columns.push(column);
                        break;
                    case 'number':
                        row.Columns.push(column);
                        break;
                    default:
                        table.Headers[i].Hidden = true;
                }
            } else {
                table.Headers[i].Hidden = true;
            }
        }
        rows.push(row);
        return rows;
    };
    this.GetNormal = function (table, start) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="start" type="Number" integer="true">The the record to start with.</field>
        /// </signature>
        var rows = [];
        var taken = 0;
        for (var i = start; i < table.FilteredRows.length; i++) {
            rows.push(table.FilteredRows[i]);
            taken++;
            if (taken >= table.RecordsPerPage) {
                break;
            }
        }
        return rows;
    };
    this.UpdateRow = function (table, row, skipAnimation) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="String" />
        /// <param name="table" type="JTable">The jTable we are working with.</param>
        /// <param name="row" type="JTableRow">The row to render.</param>
        /// <param name="skipAnimation" type="Boolean">Whether to skip the animation or not.</param>
        /// </signature>
        if ((table.Average || table.Graph || table.Count) && table.Headers[j].Hidden) {
            return;
        }
        var tr = $('#' + row.Id);
        if (tr.length === 0) {
            return;
        }
        if (typeof (skipAnimation) !== 'boolean') {
            skipAnimation = false;
        }
        if (this.Options.type === 'email' || this.Options.type === 'invitation') {
            skipAnimation = true;
        }
        var origBackgroundColor = tr.css('background-color');
        if (!skipAnimation) {
            tr.css('background-color', 'rgba(122,2,2,.5)');
        }
        for (var j = 0; j < table.Headers.length; j++) {
            var td = tr.find('td[data-headerid="' + table.Headers[j].Id + '"]');
            if (td.length === 0) {
                continue;
            }
            var data = FindColumn(row, table.Headers[j].Id);
            if (typeof (data) !== 'undefined' && data !== null) {
                td.html(data.PrettyValue);
            } else {
                td.html('');
            }
        }
        this.CreateLinks();
        this.OnUpdateComplete(tr);
        if (!skipAnimation) {
            setTimeout(function () { tr.animate({ backgroundColor: origBackgroundColor }, 1000, function () { tr.css('background-color', ''); }); }, 5000);
        }
    };
    this.OnUpdateComplete = function (tr) {
        /// <signature>
        /// <summary>Runs a function on an updated row.</summary>
        /// <param name="tr" type="jQuery">The tr that was updated.</param>
        /// </signature>
    };
    this.GenerateTableHtml = function (table, rows) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="String" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="rows" type="Array" elementType="JTableRow">The rows to render.</field>
        /// </signature>
        $('.jTable_parentName[data-jtable-target="' + this.Table + '"]').html(this.Parent);
        $('.jTable_name[data-jtable-target="' + this.Table + '"]').html(this.Name);
        var t_html = '<thead><tr>';
        for (var ind = 0; ind < table.Headers.length; ind++) {
            if ((table.Average || table.Graph || table.Count) && table.Headers[ind].Hidden) {
                continue;
            }
            if (table.Headers[ind].Removed) {
                continue;
            }
            var group = table.Headers[ind].Group;
            if (typeof (group) !== 'undefined' && group !== null && group !== '') {
                group = '<i>' + group + '</i><br />';
            } else {
                group = '';
            }
            t_html += '<th class="" data-header-id="' + table.Headers[ind].Id + '">' + group + table.Headers[ind].Label + '</th>';
        }
        t_html += '</tr></thead><tbody>';
        for (var i = 0; i < rows.length; i++) {
            t_html += '<tr id="' + rows[i].Id + '">';
            for (var j = 0; j < table.Headers.length; j++) {
                if ((table.Average || table.Graph || table.Count) && table.Headers[j].Hidden) {
                    continue;
                }
                if (table.Headers[j].Removed) {
                    continue;
                }
                var editable = table.Headers[j].Editable;
                if (table.Average || table.Graph || table.Count) {
                    editable = false;
                }
                var data = FindColumn(rows[i], table.Headers[j].Id);
                if (typeof (data) !== 'undefined' && data !== null) {
                    editable = editable || data.Editable;
                    var t_value = data.PrettyValue;
                    if (!this.Graph && !this.Count && !this.Average && t_value !== null && t_value.indexOf('<a') === -1 && t_value.length > 100) {
                        t_value = t_value.substr(0, 100) + "...";
                    }
                    t_html += '<td data-headerid="' + table.Headers[j].Id + '" id="' + data.Id + '" class="' + (editable ? 'editable-item cursor-pointer' : '') + '">' + t_value + '</td>';
                } else {
                    t_html += '<td data-headerid="' + table.Headers[j].Id + '" class="' + (editable ? 'editable-item cursor-pointer' : '') + '"></td>';
                }
            }
            t_html += '</tr>';
        }
        t_html += '</tbody>';
        return t_html;
    };
    this.GetPrintView = function (chartData) {
        /// <signature>
        /// <summary>Gets html that represents the table including all rows possible.</summary>
        /// <param name="chartData" type="Array">The list of chartData</param>
        /// <returns type="String" />
        /// </signature>
        if (!this.Filtered) {
            this.Filter();
        }
        if (!this.Sorted) {
            this.Sort();
        }
        var rows = [];
        if (this.Count) {
            rows = this.GetCount(this, chartData);
        } else if (this.Average) {
            rows = this.GetAverage(this, chartData);
        } else if (this.Graph) {
            rows = this.GetGraph(this, chartData);
        } else {
            for (var i = 0; i < this.FilteredRows.length; i++) {
                rows.push(this.FilteredRows[i]);
            }
        }
        var html = '<html><head><title>"' + this.Name + '"</title><script src="' + window.location.origin + '/Scripts/jquery-2.1.3.min.js"></script><script src="' + window.location.origin + '/Scripts/Chartv2.js"></script><link href="' + window.location.origin + '/Content/Bootstrap/bootstrap.min.css" rel="stylesheet" />';
        html += '<style> ';
        html += '.body { color: #3e3e3e; } ';
        html += '.report-header { position: fixed; left: 0; top: 0; width: 100%; height: 100px; overflow: hidden; padding: 10px 10px 0 10px; background: white; } ';
        html += 'img.img-report-header { display: block; width: 100%; max-width: 250px; max-height: 75px; } ';
        html += '.report-title { text-align: right; } ';
        html += '.report-content { padding-top: 100px; } ';
        html += 'table.table-full-page > thead > tr { border-top: 1px solid #ddd; } ';
        html += 'table.table-full-page > thead > tr > th { padding-right: 50px !important; white-space: nowrap; min-width: 150px; background: white; font-size: 12px; } ';
        html += 'table.table-full-page > tbody > tr { height: 75px; font-size: 12px; } ';
        html += 'table.table-full-page > tbody > tr:last-child { border-bottom: 1px solid #ddd; } ';
        html += 'table.table-full-page > tbody > tr:nth-child(even) > td { background: white; } ';
        html += 'table.table-full-page td { padding: 0 0 0 0 !important; } ';
        html += '.cell-height { height: 75px; padding: 5px; overflow-y: auto; } ';
        html += '@media screen and (max-width: 1199px) { .report-header { position: relative; height: auto; overflow: visible; } ';
        html += '.report-title { text-align: left; padding-top: 15px; } ';
        html += '.report-content { padding-top: 15px; } ';
        html += 'table.table-full-page > tbody > tr { height: auto; font-size: 12px; } ';
        html += '.cell-height { height: auto; max-height: 50px; padding: 5px; overflow-y: auto; } } ';
        html += '</style>';
        html += '</head><body>';
        html += '<div class="report-header"><div class="row"><div class="col-lg-3"><img class="img-report-header" src="https://toolkit.regstep.com/Images/Common/regstep.png"/></div>';
        html += '<div class="col-lg-9 report-title">' + ((typeof (this.Parent) !== 'undefined' && this.Parent !== null && this.Parent !== '') ? '<h3>' + this.Parent + '</h3>' : '') + '<h4>' + this.Name + '</h4>' + ((typeof (this.Description) !== 'undefined' && this.Description !== null && this.Description !== '') ? '<h5>' + this.Description + '</h5>' : '') + '</div></div></div><div class="report-content">';
        html += '<table class="table table-striped table-full-page">';
        html += this.GenerateTableHtml(this, rows);
        html += '</table><script type="text/javascript">\r\n';
        html += "//Render Charts\r\n" +
            "Chart.defaults.global.animation = false;\r\n" +
            "chartData = " + JSON.stringify(chartData) + "\r\n" +
            "for (var i = 0; i < chartData.length; i++) {\r\n" +
            "    var template = \"<ul class=\\\"<%=name.toLowerCase()%>-legend\\\"><% for (var i=0; i<segments.length; i++){%><li><span style=\\\"background-color:<%=segments[i].fillColor%>\\\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=+(Math.round(((segments[i].value / total) * 100) + 'e+2')  + 'e-2')%>%<%}%></li><%}%></ul>\";\r\n" +
            "    if (this.Count) {\r\n" +
            "        var template = \"<ul class=\\\"<%=name.toLowerCase()%>-legend\\\"><% for (var i=0; i<segments.length; i++){%><li><span style=\\\"background-color:<%=segments[i].fillColor%>\\\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=segments[i].value%><%}%></li><%}%></ul>\";\r\n" +
            "    }\r\n" +
            "    switch (chartData[i].type) {\r\n" +
            "        case 'pie':\r\n" +
            "            var ctx = $('#' + chartData[i].id).get(0).getContext('2d');\r\n" +
            "            var chart = new Chart(ctx).Pie(chartData[i].data, {\r\n" +
            "                legendTemplate: template\r\n" +
            "            });\r\n" +
            "            var legend = chart.generateLegend();\r\n" +
            "            $('#' + chartData[i].id).parent().parent().children('.legend').html(legend);\r\n" +
            "            break;\r\n" +
            "        case 'bar':\r\n" +
            "            var step = 10;\r\n" +
            "            var max = 100;\r\n" +
            "            var start = 100;\r\n" +
            "            var options = {};\r\n" +
            "            var data = chartData[i].data;\r\n" +
            "            if (this.Count) {\r\n" +
            "                for (var j = 0; j < data.datasets[0].data.length; j++) {\r\n" +
            "                    if (max < data.datasets[0].data[j]) {\r\n" +
            "                        max = data.datasets[0].data[j];\r\n" +
            "                    }\r\n" +
            "                    if (start > data.datasets[0].data[j]) {\r\n" +
            "                        start = data.datasets[0].data[j];\r\n" +
            "                    }\r\n" +
            "                }\r\n" +
            "                start = (start - 10) - (start % 10);\r\n" +
            "                if (start < 0) {\r\n" +
            "                    start = 0;\r\n" +
            "                }\r\n" +
            "            } else {\r\n" +
            "                start = 0;\r\n" +
            "            }\r\n" +
            "            options.scaleOverride = true;\r\n" +
            "            options.scaleSteps = Math.ceil((max - start) / step);\r\n" +
            "            options.scaleStepWidth = step;\r\n" +
            "            options.scaleStartValue = start;\r\n" +
            "            var ctx = $('#' + chartData[i].id).get(0).getContext('2d');\r\n" +
            "            var chart = new Chart(ctx).Bar(chartData[i].data, options);\r\n" +
            "            break;\r\n" +
            "    }\r\n" +
            "}\r\n" +
            "setInterval(function () { window.print(); }, 500);";
        html += '</script></body></html>';
        var wnd = window.open("about:blank", "", "_blank");
        wnd.document.write(html);
        return html;
    };
    this.Load = function (url, options) {
        /// <signature>
        /// <summary>Loads the table from an ajax call via XMLHttpRequest.</summary>
        /// <param name="url" type="String">The url that will be called to load the jTable data.</param>
        /// <param name="options" type="Object">The object that will be used for the get call.</param>
        /// </signature>
        if (options === null) {
            options = {};
        }
        var storageName = 'jTable: ';
        var params = '';
        if (typeof (options.id) !== 'undefined' && options.id !== null) {
            params += '?id=' + options.id;
            storageName += options.id;
        }
        if (typeof (options.type) !== 'undefined' && options.type !== null && options.type !== '') {
            params += '&type=' + options.type;
            storageName += '_' + options.type;
        }
        var t_xhr = new XMLHttpRequest();
        t_xhr.addEventListener('progress', this.UpdateProgress, false);
        url += params;
        t_xhr.open('get', url, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var data = RESTFUL.parse(c_xhr);
                var table = data.Table;
                if (data.Success) {
                    thisTable.Filters = table.Filters;
                    thisTable.Sortings = table.Sortings;
                    thisTable.Rows = table.Rows;
                    thisTable.Count = table.Count;
                    thisTable.Graph = table.Graph;
                    thisTable.Average = table.Average;
                    thisTable.Description = table.Description;
                    thisTable.Name = table.Name;
                    thisTable.Parent = table.Parent;
                    thisTable.Favorite = table.Favorite;
                    thisTable.Headers = table.Headers;
                    thisTable.Id = table.Id;
                    thisTable.Options = table.Options;
                    thisTable.Page = 1;
                    thisTable.RecordsPerPage = table.RecordsPerPage;
                    thisTable.TotalRecords = table.TotalRecords;
                    thisTable.SavedId = table.SavedId;
                    thisTable.LastFullCheck = moment();
                    thisTable.UpdateView(thisTable);
                    thisTable.AfterLoad(thisTable);
                    thisTable.Filter();
                    thisTable.GetPage();
                    if (thisTable.SavedId !== null) {
                        $('.jTable_standardOnly').hide();
                    } else {
                        $('.jTable_standardOnly').show();
                    }
                } else {
                    RESTFUL.showError("Failed to load data.", "Unhandled Exception");
                }
            } else {
                RESTFUL.showError("Failed to load data.", "Unhandled Exception");
            }
            prettyProcessing.hidePleaseWait();
        };
        t_xhr.onerror = function () {
            RESTFUL.showError();
            prettyProcessing.hidePleaseWait();
        };
        prettyProcessing.showPleaseWait("Requesting Data", "Requesting data from the server.");
        t_xhr.send();
    };
    this.UpdateProgress = function (event) {
        /// <signature>
        /// <summary>Runs the event for updating progress.</summary>
        /// <param name="event" type="Event">The url that will be called to load the jTable data.</param>
        /// </signature>
        if (event.lengthComputable) {
            var percentComplete = (event.loaded / event.total) * 100;
            prettyProcessing.update("Downloading Data", "Downloading the data from the server.", percentComplete);
        } else {
            prettyProcessing.update("Downloading Data", "Downloading the data from the server.", 100);
        }
    };
    this.AfterLoad = function (p_table) {
        /// <signature>
        /// <summary> Runs after the table has been loaded from the server.</summary>
        /// <param name="p_table" type="JTable">The JTable to manipulate.</param>
        /// </signature>
        if (typeof (BREADCRUMB_VERSION) !== 'undefined' && BREADCRUMB_CURRENT !== null) {
            BREADCRUMB_CURRENT.Label = p_table.Name + ' on ' + p_table.Parent;
            UpdateCrumb(BREADCRUMB_CURRENT);
        }
    };
    this.UpdateView = function () {
        /// <signature>
        /// <summary>Updates the view to show table statistics and properties.</summary>
        /// </signature>
        $('.jTable_parentName[data-jtable-target="' + this.Table + '"]').html(this.Parent);
        $('.jTable_name[data-jtable-target="' + this.Table + '"]').html(this.Name);
        $('.jTable_recordsPerPage[data-jtable-target="' + this.Table + '"]').val(this.RecordsPerPage);
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').html('');
        this.Filter();
        this.Sort();
        for (var ind = 0; ind < this.TotalPages(); ind++) {
            $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').append('<option value="' + (ind + 1) + '" ' + (this.Page === (ind + 1) ? 'selected="selected"' : '') + '>' + (ind + 1) + '</option>');
        }
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"] > option').prop('selected', false);
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"] > option[value="' + this.Page + '"]').prop('selected', true);
        if (this.SavedId === null || this.SavedId === '' || typeof (this.SavedId) === 'undefined') {
            $('.jTable_standarOnly[data-jtable-target="' + this.Table + '"]').show();
        } else {
            $('.jTable_standardOnly[data-jtable-target="' + this.Table + '"]').hide();
        }
        if (this.Favorite) {
            $('.jTable_favorite[data-jtable-target="' + this.Table + '"]').html('<span class="glyphicon glyphicon-star glyphicon-small"></span> Un-Favorite');
        } else {
            $('.jTable_favorite[data-jtable-target="' + this.Table + '"]').html('<span class="glyphicon glyphicon-star-empty glyphicon-small"></span> Favorite');
        }
        this.SortingObject.Generate();
        this.FilterObject.Generate();
    };
    this.GetAjaxData = function () {
        /// <signature>
        /// <summary>Gets data needed to submit on an ajax call. Original jTable is not modified.</summary>
        /// <returns type="Object" />
        /// </signature>
        var newTable = {};
        newTable.FilteredRows = [];
        newTable.Rows = [];
        newTable.Filters = this.Filters;
        newTable.Sortings = this.Sortings;
        newTable.Id = this.Id;
        newTable.SavedId = this.SavedId;
        newTable.Count = this.Count;
        newTable.Graph = this.Graph;
        newTable.Average = this.Average;
        newTable.Name = this.Name;
        newTable.Description = this.Description;
        newTable.Options = this.Options;
        newTable.Headers = this.Headers;
        newTable.Favorite = this.Favorite;
        newTable.Parent = this.Parent;
        return newTable;
    };
    //#endregion
}

//#region Classes
function JTableSorting() {
    /// <signature>
    /// <summary>Constructs a new JTableSorting object.</summary>
    /// <returns type="JTableSorting" />
    /// </signature>
    /// <field name="ActingOn" type="String">The header id that is being sorted.</field>
    /// <field name="Ascending" type="Boolean">A boolean value representing ascending sorting. default: true</field>
    /// <field name="Order" type="Number">The order of the sorting.</field>
    /// <signature>
    "use strict";
    this.ActingOn = '';
    this.Ascending = true;
    this.Order = 1;
}

function JTableFilter() {
    /// <signature>
    /// <summary>Constructs a new JTableFilter object.</summary>
    /// <returns type="JTableFilter" />
    /// </signature>
    /// <field name="ActingOn" type="String">The header id that is being sorted.</field>
    /// <field name="Test" type="String">A string representing the type of test. default: '=='</field>
    /// <field name="Value" type="String">A string representing the value to test against. default: 'true'</field>
    /// <field name="Order" type="Number">The order of the sorting.</field>
    /// <field name="Link" type="String">The link for multiple filters.</field>
    /// <field name="GroupNext" type="Boolean">Boolean representing if it is grouped with the next filter. default: false</field>
    /// <signature>
    "use strict";
    this.ActingOn = '';
    this.Test = '==';
    this.Value = '';
    this.Order = 1;
    this.Link = null;
    this.GroupNext = false;
    this.CaseSensitive = false;
}

function JTableHeader() {
    /// <signature>
    /// <summary>Creates a new table header object.</summary>
    /// <returns type="JTableHeader" />
    /// <field name="Label" type="String">The label of the header for the table.</field>
    /// <field name="Id" type="String">The unique id of the header for searching, sorting, and filtering.</field>
    /// <field name="Order" type="Number">The order of the header for the table.</field>
    /// <field name="Type" type="String">The type of the header.</field>
    /// <field name="PossibleValues" type="Array" elementType="JTableHeaderPossibleValue">The possible values for mulitpleSelections and itemParent.</field>
    /// <field name="Removed" type="Boolean">If the header is removed or not.</field>
    /// <field name="SortByPretty" type="Boolean">If the header is to be sorted by its pretty value or raw value.</field>
    /// </signature>
    "use strict";
    this.Label = '';
    this.Id = '';
    this.Order = 1;
    this.Hidden = false;
    this.Group = '';
    this.Type = 'text';
    this.PossibleValues = [];
    this.Editable = false;
    this.Removed = false;
    this.SortByPretty = false;
}

function JTableHeaderPossibleValue() {
    /// <signature>
    /// <summary>Creates a new JTableHeaderPossibleValue.</summary>
    /// <field name="Id" type="String">The id of the value.</field>
    /// <field name="Label" type="String">The label of the value.</field>
    /// </signature>
    "use strict";
    this.Id = '';
    this.Label = '';
}

function JTableRow() {
    /// <signature>
    /// <summary>Creates a new JTableRow</summary>
    /// <returns type="JTableRow" />
    /// <field name="Id" type="String">The unique identifier for searching.</field>
    /// <field name="Columns" type="Array" elementType="JTableColumn">An array of the columns that correspond to headers.</field>
    /// <field name="Order" type="Number">The order of the elements to sort.</field>
    /// <field name="PreviousSort" type="Number">The qualifier result of the last sorting method.</field>
    /// </signature>
    "use strict";
    this.Id = '';
    this.Columns = [];
    this.Order = 1;
    this.PreviousSort = 0;
}

function JTableColumn() {
    /// <signature>
    /// <summary>Creates a new JTableColumn</summary>
    /// <returns type="JTableColumn" />
    /// <field name="HeaderId" type="String">The unique identifier pertaining to the Header it corresponds to.</field>
    /// <field name="PrettyValue" type="String">The pretty value of the column.</field>
    /// <field name="Id" type="String">The unique identifier of the column.</field>
    /// <field name="Value" type="String">The raw value of the column.</field>
    /// <field name="Type" type="String">The type of value</field>
    /// <field name="Editable" type="Boolean">Whether the field can be edited.</field>
    /// </signature>
    "use strict";
    this.HeaderId = '';
    this.PrettyValue = '';
    this.Id = '';
    this.Value = '';
    this.Editable = true;
}

function TestValue(data, testValue, test, header) {
    /// <signature>
    /// <summary>Tests a value against another according to a test</summary>
    /// <returns type="Boolean" />
    /// <param name="data" type="JTableColumn">The value of the data.</field>
    /// <param name="testValue" type="String">The value to test against.</field>
    /// <param name="test" type="String">The type of test to perform.</field>
    /// <param name="header" type="JTableHeader">The header.</field>
    /// </signature>
    "use strict";
    var type = header.Type;
    var t_value = "";
    var found = false;
    if (data.Value === null) {
        data.Value = "";
    }
    if (testValue === null) {
        testValue = "";
    }
    if (type === 'string' || type === 'text') {
        switch (test) {
            case '==':
                return data.Value.toLowerCase() === testValue.toLowerCase();
            case '!=':
                return data.Value.toLowerCase() !== testValue.toLowerCase();
            case '>':
                return data.Value.length > parseInt(testValue);
            case '>=':
                return data.Value.length >= parseInt(testValue);
            case '<':
                return data.Value.lenth < parseInt(testValue);
            case '<=':
                return data.Value.length <= parseInt(testValue);
            case '*=':
                return data.Value.toLowerCase().indexOf(testValue) !== -1;
            case '!*=':
                return data.Value.toLowerCase().indexOf(testValue) === -1;
            case '^=':
                return data.Value.toLowerCase().indexOf(testValue) === 0;
            case '!^=':
                return data.Value.toLowerCase().indexOf(testValue) !== 0;
            case '$=':
                return data.Value.toLowerCase().indexOf(testValue, data.Value.length - testValue.length) !== -1;
            case '!$=':
                return data.Value.toLowerCase().indexOf(testValue, data.Value.length - testValue.length) === -1;
            case '=rgx=':
                var t_rgx = new RegExp(testValue, 'i');
                return t_rgx.test(data.Value);
            case '!=rgx=':
                var t2_rgx = new RegExp(testValue, 'i');
                return !t2_rgx.test(data.Value);
        }
    } else if (type === 'number' || type.indexOf('rating') === 0) {
        t_value = parseFloat(data.Value);
        var t_testValue = parseFloat(testValue);
        switch (test) {
            case '==':
                return t_value === t_testValue;
            case '!=':
                return t_value !== t_testValue;
            case '>':
                return t_value > t_testValue;
            case '>=':
                return t_value >= t_testValue;
            case '<':
                return t_value < t_testValue;
            case '<=':
                return t_value <= t_testValue;
        }
    } else if (type === 'multipleSelection') {
        t_value = JSON.parse(data.Value);
        found = false;
        switch (test) {
            case '==':
                return (t_value.length === 1) && (t_value[0] === testValue);
            case '!=':
                for (var si = 0; si < t_value.length; si++) {
                    if (t_value[si] === testValue) {
                        found = true;
                        break;
                    }
                }
                return !found;
            case 'in':
                for (var si2 = 0; si2 < t_value.length; si2++) {
                    if (t_value[si2] === testValue) {
                        found = true;
                        break;
                    }
                }
                return found;
            case 'notin':
                for (var si3 = 0; si3 < t_value.length; si3++) {
                    if (t_value[si3] === testValue) {
                        found = true;
                        break;
                    }
                }
                return !found;
        }
    } else if (type === 'itemParent') {
        found = false;
        switch (test) {
            case '==':
                return data.Value === testValue;
            case '!=':
                return data.Value !== testValue;
        }
    } else if (type === 'date') {
        t_value = moment(data.Value);
        var t_value2 = moment(testValue);
        switch (test) {
            case '==':
                return t_value.isSame(t_value2);
            case '!=':
                return !t_value.isSame(t_value2);
            case '>':
                return t_value.isAfter(t_value2);
            case '>=':
                return t_value.isSame(t_value2) || t_value.isAfter(t_value2);
            case '<':
                return t_value.isBefore(t_value2);
            case '<=':
                return t_value.isSame(t_value2) || t_value.isBefore(t_value2);
        }
    } else if (type === 'boolean') {
        switch (test) {
            case '==':
                return data.Value === testValue;
            case '!=':
                return data.Value !== testValue;
        }
    }
    return false;
}

function ReplaceRow(table, row) {
    /// <signature>
    /// <summary>Finds a specific row from a JTable and replaces it with the supplied one.</summary>
    /// <returns type="JTableRow" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="row" type="JTableRow">The row id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Rows.length; i++) {
        if (table.Rows[i].Id === row.Id) {
            table.Rows[i] = row;
            return true;
        }
    }
    table.Rows.push(row);
    return false;
}

function ReplaceHeader(table, header) {
    /// <signature>
    /// <summary>Finds a specific header from a JTable and replaces it with the supplied one.</summary>
    /// <returns type="JTableRow" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="row" type="JTableHeader">The row id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Headers.length; i++) {
        if (table.Headers[i].Id === header.Id) {
            table.Headers[i] = header;
            return;
        }
    }
    table.Headers.push(header);
}

function FindColumn(row, headerId) {
    /// <signature>
    /// <summary>Finds a specific column form a JTableRow</summary>
    /// <returns type="JTableColumn" />
    /// <param name="row" type="JTableRow">The JTableRow to search through.</param>
    /// <param name="headerId" type="String">The header id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < row.Columns.length; i++) {
        if (row.Columns[i].HeaderId === headerId) {
            return row.Columns[i];
        }
    }
    return null;
}

function FindRow(table, rowId) {
    /// <signature>
    /// <summary>Finds a specific row form a JTable.</summary>
    /// <returns type="JTableRow" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="rowId" type="String">The row id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Rows.length; i++) {
        if (table.Rows[i].Id === rowId) {
            return table.Rows[i];
        }
    }
    return null;
}

function FindHeader(table, headerId) {
    /// <signature>
    /// <summary>Finds a specific header form a JTable</summary>
    /// <returns type="JTableHeader" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="headerId" type="String">The header id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Headers.length; i++) {
        if (table.Headers[i].Id === headerId) {
            return table.Headers[i];
        }
    }
    return null;
}

function GenerateHeaderDiv(id, tableId) {
    /// <signature>
    /// <returns type="String" />
    /// <summary>Generates the html needed to make a modal for manipulating headers.</summary>
    /// <param name="id" type="Stirng">The id of the modal</param>
    /// </signature>
    "use strict";
    var height = $(window).innerHeight();
    height *= 0.72;
    var t_html = '<div class="modal fade" id="' + id + '" data-jtable-target="' + tableId + '"><div class="modal-dialog modal-fill"><div class="modal-header"><h3 class="modal-title">Report Fields</h3></div>';
    t_html += '<div class="modal-body" style="max-height: ' + height + 'px;"><div class="row"><div class="col-md-5 header-selection-window"><div class="headers-container headers-available"><div class="headers-title">Available Fields</div><div class="headers-container-available"></div></div></div>';
    t_html += '<div class="col-md-2 header-selection-buttons"><div class="headers-commands"><a href="#" class="add-header">Add <span class="glyphicon glyphicon-chevron-right glyphicon-small"></span></a><br /><br /><a href="#" class="remove-header"><span class="glyphicon glyphicon-chevron-left glyphicon-small"></span></span> Remove</a></div></div>';
    t_html += '<div class="col-md-5 header-selection-window"><div class="headers-container headers-selected"><div class="headers-title">Included Fields</div><div class="headers-container-selected"></div></div></div>';
    t_html += '</div></div>';
    t_html += '<div class="modal-footer"><button type="button" class="btn btn-default headers-set">Set</button></div></div></div>';
    return t_html;
}
//#endregion

//#region Array Values
var JTestString = [
    '==',
    '>',
    '>=',
    '<',
    '<=',
    '!=',
    '^=',
    '!^=',
    '$=',
    '!$=',
    '*=',
    '!*=',
    '=rgx=',
    '!=rgx=',
    'in',
    'notIn'
];

var JLinkString = [
    'none',
    'and',
    'or'
];
//#endregion