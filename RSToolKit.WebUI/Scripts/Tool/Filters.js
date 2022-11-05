/* Filter v1.1.1.0         */
/* Created By: D.J. Enzyme */
/* Creation Date: 20141229 */
/* Modified Date: 20150120 */

/* global FindHeader */
/* global JTableFilter */
/* jshint unused: false*/

/// <reference path="jTable.js" />

var FILTER_colors = [
    "#FFFFB0",
    "#B0B0FF",
    "#68C473",
    "#C468B9",
    "#6669BA",
    "#BAB766"
];
var FILTER_modalIndex = -1;
var FILTER_VERSION = '1.1.1.0';

function Filter(table, modalId, minimal) {
    /// <signature>
    /// <summary>Creates a Filter object.</summary>
    /// <returns type="Filter" />
    /// <param name="table" type="JTable">The JTable the sorting will apply too.</param>
    /// <param name="modalId" type="String">The modal Id of the sorting. default: index sorting</param>
    /// <field name="Table" type="JTable">The JTable that is being used for the filtering.</field>
    /// <field name="Modal" type="jQuery">The modal that is being used to edit filtering.</field>
    /// </signature>
    if (typeof (modalId) === 'undefined') {
        modalId = null;
    }
    var min_layout = false;
    if (typeof (minimal) === 'boolean') {
        min_layout = minimal;
    }
    var filterIndex = -1;
    var colorIndex = -1;
    var t_holder = this;
    this.Table = table;
    this.Modal = null;
    if (!min_layout) {
        this.Modal = FILTER_GenerateModal(modalId);
    }
    this.GenerateActing = function (value) {
        /// <signature>
        /// <summary>Generating the acting on input.</summary>
        /// <param name="value" type="String">The value to prefil.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="filter-actingon form-control"><option>Make a Selection</option>';
        for (var i = 0; i < this.Table.Headers.length; i++) {
            html += '<option value="' + this.Table.Headers[i].Id + '"' + (this.Table.Headers[i].Id === value ? ' selected="selected"' : '') + '>' + this.Table.Headers[i].Label + '</option>';
        }
        html += '</select>';
        return html;
    };
    this.Generate = function () {
        /// <signature>
        /// <summary>Generates the sortings to place in the table</summary>
        /// </signature>
        if (this.Modal === null) {
            return;
        }
        this.Modal.find('.table-filter-body').html('');
        filterIndex = -1;
        var html = '';
        for (var i = 0; i < this.Table.Filters.length; i++) {
            filterIndex++;
            var t_filter = this.Table.Filters[i];
            var t_header = FindHeader(this.Table, t_filter.ActingOn);
            html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupNext-value" />';
            html += '<input type="checkbox" class="filter-groupNext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            if (i === 0) {
                html += '<input type="hidden" class="filter-link" value="none" />';
            } else {
                html += '<select class="filter-link form-control">';
                html += '<option value="and"' + (t_filter.Link === 'and' ? ' selected="selected"' : '') + '>And</option>';
                html += '<option value="or"' + (t_filter.Link === 'or' ? ' selected="selected"' : '') + '>Or</option>';
                html += '</select>';
            }
            html += '</td>';
            html += '<td>';
            html += this.GenerateActing(t_filter.ActingOn);
            html += '</td>';
            html += '</td>';
            html += '<td>';
            html += '<select class="filter-test form-control">';
            html += '</select>';
            html += '</td>';
            html += '<td>';
            html += this.GenerateValueInput(t_header, t_filter.Value);
            html += '</td>';
            html += '</tr>';
            this.Modal.find('.table-filter-body').append(html);
            var testHtml = "";
            if (t_header.Type === 'itemParent') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option>';
            } else if (t_header.Type === 'multipleSelection') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="in"' + (t_filter.Test === 'in' ? ' selected="selected"' : '') + '>In</option><option value="notin"' + (t_filter.Test === 'notin' ? ' selected="selected"' : '') + '>Not In</option>';
            } else if (t_header.Type === 'number' || t_header.Type.indexOf('rating') === 0) {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else if (t_header.Type === 'date') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="^="' + (t_filter.Test === '^=' ? ' selected="selected"' : '') + '>Starts With</option><option value="!^="' + (t_filter.Test === '!^=' ? ' selected="selected"' : '') + '>Does Not Start With</option><option value="$="' + (t_filter.Test === '$=' ? ' selected="selected"' : '') + '>Ends With</option><option value="!$="' + (t_filter.Test === '!$=' ? ' selected="selected"' : '') + '>Does Not End With</option><option value="=rgx="' + (t_filter.Test === '=rgx=' ? ' selected="selected"' : '') + '>Regex Match</option><option value="!=rgx="' + (t_filter.Test === '!=rgx=' ? ' selected="selected"' : '') + '>Regex Mismatch</option><option value="*="' + (t_filter.Test === '*=' ? ' selected="selected"' : '') + '>Contains</option><option value="!*="' + (t_filter.Test === '!*=' ? ' selected="selected"' : '') + '>Does Not Contain</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            }
            $('tr[data-index="' + filterIndex + '"] .filter-test').html(testHtml);
        }
        this.Modal.find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            t_holder.ActingOnChange(tr);
        });
        this.Modal.find('.filter-delete').on('click', function (e) {
            t_holder.DeleteStatement($(this).closest('tr'), e);
        });

    };
    this.DeleteStatement = function (tr, event) {
        /// <signature>
        /// <summary>Deletes a statement.</summary>
        /// <param name="tr" type="jQuery">The tr html tag to remove.</param>
        /// <param name="event" type="JavascriptEvent">The mouse event.</param>
        /// </signature>
        event.preventDefault();
        if (this.Modal === null) {
            return;
        }
        tr.remove();
        this.Modal.find('.filter').css('background-color', 'inherit');
        this.Modal.find('.filter').find('input[name$="GroupNext"]').val('false');
        colorIndex = -1;
        this.Modal.find('tr.filter').each(function (i) {
            filterIndex = i;
            $(this).find('input').each(function (j) {
                var name = $(this).attr('name');
                if (typeof (name) == 'undefined') {
                    return;
                }
                name = name.replace(/\[\d+\]/, '[' + filterIndex + ']');
                $(this).attr('name', name);
            });
            $(this).find('input[name$="Order"]').val(filterIndex + 1);
        });
        filterIndex--;
    };
    this.AddStatement = function () {
        /// <signature>
        /// <summary>Adds a new statement to the table.</summary>
        /// </signature>
        if (this.Modal === null) {
            return;
        }
        filterIndex++;
        var filter = new JTableFilter();
        var header = FindHeader(this.Table, filter.ActingOn);
        var html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupNext-value" />';
        html += '<input type="checkbox" class="filter-groupNext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        if (this.Modal.find('.filter').length === 0) {
            html += '<input type="hidden" class="filter-link" value="none" />';
        } else {
            html += '<select class="filter-link form-control">';
            html += '<option value="and">And</option>';
            html += '<option value="or">Or</option>';
            html += '</select>';
        }
        html += '</td>';
        html += '<td>';
        html += this.GenerateActing();
        html += '</td>';
        html += '</td>';
        html += '<td>';
        html += '<select class="filter-test form-control">';
        html += '</select>';
        html += '</td>';
        html += '<td>';
        html += this.GenerateValueInput(header);
        html += '</td>';
        html += '</tr>';
        this.Modal.find('.table-filter-body').append(html);
        this.Modal.find('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            t_holder.ActingOnChange(tr);
        });
        this.Modal.find('.filter[data-index="' + filterIndex + '"]').find('.filter-delete').on('click', function (e) {
            var tr = $(this).closest('tr');
            t_holder.DeleteStatement(tr, e);
        });
    };
    this.Group = function (e) {
        e.preventDefault();
        if (this.Modal === null) {
            return;
        }
        var t_checkedAmount = this.Modal.find('.filter-groupNext:checked').length;
        if (t_checkedAmount > 1) {
            colorIndex++;
        } else {
            return;
        }
        if (colorIndex >= FILTER_colors.length) {
            colorIndex = 0;
        }
        var found = false;
        var skipped = false;
        var lastItem = null;
        var count = 0;
        var changed = [];
        this.Modal.find('.filter-groupNext').each(function (i) {
            if (found) {
                if (!($(this).is(':checked'))) {
                    skipped = true;
                }
            }
            if ($(this).is(':checked') && !skipped) {
                var tr = $(this).closest('tr');
                count++;
                found = true;
                lastItem = tr.find('.filter-groupNext');
                lastItem.val('true');
                tr.css('background-color', FILTER_colors[colorIndex]);
                changed.push(tr);
            }
        });
        if (count == 1) {
            changed[0].css('background-color', 'inherit');
            changed[0].find('.filter-groupNext-value').val('false');
        }
        lastItem.val('false');
        this.Modal.find('.filter-groupNext').prop('checked', false);
    };
    this.Ungroup = function (e) {
        e.preventDefault();
        if (this.Modal === null) {
            return;
        }
        this.Modal.find('.filter').css('background-color', 'inherit');
        this.Modal.find('.filter').find('.filter-groupNext-value').val('false');
        colorIndex = -1;
    };
    this.ActingOnChange = function (tr) {
        /// <signature>
        /// <summary>Handles changes to the filter acting on field.</summary>
        /// <param name="tr" type="jQuery">The jquery object representing the html tr tag.</param>
        /// </signature>
        try {
            tr.find('.rating').rating('destroy');
        } catch (e) { }
        var oldInput = tr.find('.filter-value');
        var header = FindHeader(this.Table, tr.find('.filter-actingon').val());
        var html = this.GenerateValueInput(header);
        oldInput.replaceWith(html);
        var testHtml = '';
        if (header.Type === 'itemParent' || header.Type === 'boolean') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option>';
        } else if (header.Type === 'multipleSelection') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="in">In</option><option value="notin">Not In</option>';
        } else if (header.Type === 'number' || header.Type.indexOf('rating') === 0) {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else if (header.Type === 'date') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="^=">Starts With</option><option value="!^=">Does Not Start With</option><option value="$=">Ends With</option><option value="!$=">Does Not End With</option><option value="=rgx=">Regex Match</option><option value="!=rgx=">Regex Mismatch</option><option value="*=">Contains</option><option value="!*=">Does Not Contain</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        }

        tr.find('.filter-test').html(testHtml);
        try {
            tr.find('.rating').rating();
        } catch (e) {}
        try {
            tr.find('.datetimepicker').datetimepicker();
        } catch (e) { }
    };
    this.GenerateValueInput = function (header, value) {
        /// <signature>
        /// <summary>Generates the input for the filter value.</summary>
        /// <returns type="String" />
        /// <param name="header" type="JTableHeader">The JTableHeader to generate off of.</param>
        /// <param name="value" type="String">The value to prefil.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '';
        if (header === null) {
            return '<span class="filter-value">Make a Selection</span>';
        }
        if (header.Type === 'itemParent' || header.Type === 'multipleSelection' || header.Type === 'boolean') {
            html += '<select class="filter-value form-control">';
            for (var i = 0; i < header.PossibleValues.length; i++) {
                html += '<option value="' + header.PossibleValues[i].Id + '"' + (header.PossibleValues[i].Id === value ? ' selected="true"' : '') + '>' + header.PossibleValues[i].Label + '</option>';
            }
            html += '</select>';
        } else if (header.Type.indexOf('rating') === 0) {
            if (value === '') {
                value = '0';
            }
            var t_json = header.Type;
            t_json = t_json.split("=>")[1];
            var t_options = JSON.parse(t_json);
            html += '<input class="filter-value form-control rating" min="' + t_options.min + '" max="' + t_options.max + '" data-step="' + t_options.step + '" type="number" value="' + value + '" />';
        } else {
            html += '<input class="filter-value form-control' + (header.Type === 'date' ? ' datetimepicker' : '') + '" value="' + value + '" />';
        }
        return html;
    };
    if (this.Modal !== null) {
        this.Modal.find('.set-filters').on('click', function () {
            t_holder.Modal.modal('hide');
            t_holder.Table.Filters = [];
            t_holder.Modal.find('.filter').each(function (i) {
                var tr = $(this);
                var filter = new JTableFilter();
                filter.GroupNext = tr.find('.filter-groupNext-value').val().toLowerCase() === 'true';
                filter.ActingOn = tr.find('.filter-actingon').val();
                filter.Link = tr.find('.filter-link').val();
                filter.Order = i;
                filter.Test = tr.find('.filter-test').val();
                filter.Value = tr.find('.filter-value').val();
                t_holder.Table.Filters.push(filter);
            });
            t_holder.Table.Filtered = false;
            t_holder.Table.GetPage();
        });
        this.Modal.find('.add-filter').on('click', function (e) {
            e.preventDefault();
            t_holder.AddStatement();
        });
        this.Modal.find('.ungroup').on('click', function (e) {
            t_holder.Ungroup(e);
        });
        this.Modal.find('.group').on('click', function (e) {
            t_holder.Group(e);
        });
    }
}

function FILTER_GenerateModal(modalId) {
    /// <signature>
    /// <summary>Creates a sorting modal.</summary>
    /// <return type="jQuery" />
    /// <param name="modalId" type="String">The id to use for the modal. default: a incremental number.</modalId>
    /// </signature>
    if (modalId === null) {
        modalId = 'm_filter' + (++FILTER_modalIndex);
    }
    if (modalId.legth > 0 && modalId[0] === '#') {
        modalId = modalId.substr(1, modalId.length);
    }
    var object = $(modalId);
    if (object.length > 0) {
        return object;
    }
    var filterModal_html = '<div class="modal fade" id="' + modalId + '"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Filters</h3></div>';
    filterModal_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Filters</h4></div>';
    filterModal_html += '<div class="row color-grey-2 add-padding-vertical-5"><div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="group"><span class="glyphicon glyphicon-link"></span> Group Selected</a></div></div>';
    filterModal_html += '<div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="ungroup"><span class="glyphicon glyphicon-link"></span> Ungroup All</a></div></div></div>';
    filterModal_html += '<div class="row"><table class="table table-filter">';
    filterModal_html += '<thead><tr><th></th><th>Link</th><th>Variable</th><th>Test</th><th>Value</th></tr></thead>';
    filterModal_html += '<tbody class="table-filter-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-filter"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    filterModal_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default set-filters">Apply Filters</button>';
    filterModal_html += '</div></div></div>';
    $('body').append(filterModal_html);
    object = $('#' + modalId);
    return object;
}