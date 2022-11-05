/// <reference path="b_toolkit.intellisense.js" />
/* Filter v1.1.1.0         */
/* Created By: D.J. Enzyme */
/* Creation Date: 20141229 */
/* Modified Date: 20150120 */

/* global FindHeader */
/* global JTableFilter */
/* jshint unused: false*/

/// <reference path="jTable.js" />

var jsonFiltering = (function (_my) {

    // #region values

    var groupColors = [
        '#FFFFB0',
        '#B0B0FF',
        '#68C473',
        '#C468B9',
        '#6669BA',
        '#BAB766'
    ];

    var filterIndex = -1;

    var colorIndex = -1;

    // #endregion

    // #region modal

    var filterModal_html = '<div class="modal fade" id="filter_modal"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Filters</h3></div>';
    filterModal_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Filters</h4></div>';
    filterModal_html += '<div class="row color-grey-2 add-padding-vertical-5"><div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="group"><span class="glyphicon glyphicon-link"></span> Group Selected</a></div></div>';
    filterModal_html += '<div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="ungroup"><span class="glyphicon glyphicon-link"></span> Ungroup All</a></div></div></div>';
    filterModal_html += '<div class="row"><table class="table table-filter">';
    filterModal_html += '<thead><tr><th></th><th>Link</th><th>Variable</th><th>Test</th><th>Value</th></tr></thead>';
    filterModal_html += '<tbody class="table-filter-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-filter"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    filterModal_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default set-filters">Apply Filters</button>';
    filterModal_html += '</div></div></div>';
    $('body').append(filterModal_html);
    _my.modal = $('#filter_modal');

    // Set up events on modal
    _my.modal.find('.add-filter').on('click', function (e) {
        e.preventDefault();
        filterIndex++;
        var filter = {};
        var html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupNext-value" />';
        html += '<input type="checkbox" class="filter-groupNext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        if (_my.modal.find('.filter').length === 0) {
            html += '<input type="hidden" class="filter-link" value="none" />';
        } else {
            html += '<select class="filter-link form-control">';
            html += '<option value="and">And</option>';
            html += '<option value="or">Or</option>';
            html += '</select>';
        }
        html += '</td>';
        html += '<td>';
        html += generateActing();
        html += '</td>';
        html += '</td>';
        html += '<td>';
        html += '<select class="filter-test form-control">';
        html += '</select>';
        html += '</td>';
        html += '<td>';
        html += generateValueInput(null);
        html += '</td>';
        html += '</tr>';
        _my.modal.find('.table-filter-body').append(html);
        _my.modal.find('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            actingOnChange(tr);
        });
        _my.modal.find('.filter[data-index="' + filterIndex + '"]').find('.filter-delete').on('click', function (e) {
            var tr = $(this).closest('tr');
            deleteStatement(tr, e);
        });
    });
    _my.modal.find('.set-filters').on('click', function () {
        _my.filters = [];
        _my.modal.find('.filter').each(function (i) {
            var tr = $(this);
            var filter = {};
            filter.GroupNext = tr.find('.filter-groupNext-value').val().toLowerCase() === 'true';
            filter.ActingOn = tr.find('.filter-actingon').val();
            filter.LinkedBy = tr.find('.filter-link').val();
            filter.Order = i;
            filter.Equality = tr.find('.filter-test').val();
            filter.Value = tr.find('.filter-value').val();
            _my.filters.push(filter);
        });
        _my.modal.modal('hide');
        _my.callback();
    });
    _my.modal.find('.ungroup').on('click', function (e) {
        ungroup(e);
    });
    _my.modal.find('.group').on('click', function (e) {
        group(e);
    });
    
    // #endregion

    _my.created = false;
    _my.filters = [];
    _my.fields = [];
    _my.callback = function ()
    {
        return;
    }

    _my.create = function (filters, fields, callback) {
        _my.filters = filters;
        _my.fields = fields;
        _my.created = true;
        _my.callback = callback;
    };

    _my.display = function (show) {
        if (show === null || typeof (show) === 'undefined') {
            show = true;
        }
        // Clear the modal.
        _my.modal.find('.table-filter-body').html('');

        filterIndex = -1;
        // Start generating the html.
        var html = '';
        var filterError = [];
        for (var i = 0; i < _my.filters.length; i++) {
            filterIndex++;
            var t_filter = _my.filters[i];
            var t_header = findField(t_filter.ActingOn);
            if (t_header == null) {
                filterError.push(t_filter.ActingOn);
                continue;
            }
            html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupNext-value" />';
            html += '<input type="checkbox" class="filter-groupNext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            if (i === 0) {
                html += '<input type="hidden" class="filter-link" value="none" />';
            } else {
                html += '<select class="filter-link form-control">';
                html += '<option value="and"' + (t_filter.LinkedBy === 'and' ? ' selected="selected"' : '') + '>And</option>';
                html += '<option value="or"' + (t_filter.LinkedBy === 'or' ? ' selected="selected"' : '') + '>Or</option>';
                html += '</select>';
            }
            html += '</td>';
            html += '<td>';
            html += generateActing(t_filter.ActingOn);
            html += '</td>';
            html += '</td>';
            html += '<td>';
            html += '<select class="filter-test form-control">';
            html += '</select>';
            html += '</td>';
            html += '<td>';
            html += generateValueInput(t_header, t_filter.Value);
            html += '</td>';
            html += '</tr>';
            _my.modal.find('.table-filter-body').append(html);
            var testHtml = "";
            if (t_header.Type === 'itemParent' || t_header.Type === 'single selection') {
                testHtml = '<option value="=="' + (t_filter.Equality === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Equality === '!=' ? ' selected="selected"' : '') + '>Not Equals</option>';
            } else if (t_header.Type === 'multiple selection' || t_header.Type === 'multipleSelection') {
                testHtml = '<option value="=="' + (t_filter.Equality === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Equality === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="in"' + (t_filter.Equality === 'in' ? ' selected="selected"' : '') + '>Includes</option><option value="notin"' + (t_filter.Equality === 'notin' ? ' selected="selected"' : '') + '>Does Not Include</option>';
            } else if (t_header.Type === 'number' || t_header.Type.indexOf('rating') === 0) {
                testHtml = '<option value="=="' + (t_filter.Equality === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Equality === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Equality === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Equality === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Equality === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Equality === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else if (t_header.Type === 'date') {
                testHtml = '<option value="=="' + (t_filter.Equality === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Equality === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Equality === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Equality === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Equality === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Equality === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else {
                testHtml = '<option value="=="' + (t_filter.Equality === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Equality === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="^="' + (t_filter.Equality === '^=' ? ' selected="selected"' : '') + '>Starts With</option><option value="!^="' + (t_filter.Equality === '!^=' ? ' selected="selected"' : '') + '>Does Not Start With</option><option value="$="' + (t_filter.Equality === '$=' ? ' selected="selected"' : '') + '>Ends With</option><option value="!$="' + (t_filter.Equality === '!$=' ? ' selected="selected"' : '') + '>Does Not End With</option><option value="=rgx="' + (t_filter.Equality === '=rgx=' ? ' selected="selected"' : '') + '>Regex Match</option><option value="!=rgx="' + (t_filter.Equality === '!=rgx=' ? ' selected="selected"' : '') + '>Regex Mismatch</option><option value="*="' + (t_filter.Equality === '*=' ? ' selected="selected"' : '') + '>Contains</option><option value="!*="' + (t_filter.Equality === '!*=' ? ' selected="selected"' : '') + '>Does Not Contain</option><option value=">"' + (t_filter.Equality === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Equality === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Equality === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Equality === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            }
            $('tr[data-index="' + filterIndex + '"] .filter-test').html(testHtml);
        }
        if (filterError.length > 0) {
            var items = filterError[0];
            for (var e_i = 1; e_i < filterError.length; e_i++) {
                items += '<br />' + filterError[e_i];
            }
            RESTFUL.showError("The report filters are corrupted. This could happen if the item the filter is attached to has been removed.<br />Please rebuild them.<br />" + items, "Filter Error");
        }
        _my.modal.find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            actingOnChange(tr);
        });
        _my.modal.find('.filter-delete').on('click', function (e) {
            deleteStatement($(this).closest('tr'), e);
        });
        if (show) {
            _my.modal.modal('show');
        }
    };

    _my.generateJsonFilters = function () {
        this.filters = [];
        _my.modal.find('.filter').each(function (i) {
            var tr = $(this);
            var filter = {
                GroupNext: tr.find('.filter-groupNext-value').val().toLowerCase() === 'true',
                ActingOn: tr.find('.filter-actingon').val(),
                LinkedBy: tr.find('.filter-link').val(),
                Order: i,
                Equality: tr.find('.filter-test').val(),
                Value: tr.find('.filter-value').val()
            }
            _my.filters.push(filter);
        });
        return _my.filters;
    };

    _my.clear = function () {
        _my.modal.find('.filter').remove();
    }

    _my.addFilter = function (filter) {
        _my.filters.push(filter);
    }

    // #region functions

    function findField(id) {
        "use strict";
        for (var i = 0; i < _my.fields.length; i++) {
            if (_my.fields[i].Id === id) {
                return _my.fields[i];
            }
        }
        return null;
    }

    function generateActing(value) {
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="filter-actingon form-control"><option>Make a Selection</option>';
        for (var i = 0; i < _my.fields.length; i++) {
            var field = _my.fields[i];
            html += '<option value="' + field.Id + '"' + (field.Id === value ? ' selected="selected"' : '') + '>' + field.Label + '</option>';
        }
        html += '</select>';
        return html;
    };

    function generateValueInput(header, value) {
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '';
        if (header === null) {
            return '<span class="filter-value">Make a Selection</span>';
        }
        if (header.Type === 'single selection' || header.Type === 'multiple selection' || header.Type === 'boolean') {
            html += '<select class="filter-value form-control">';
            for (var i = 0; i < header.PossibleValues.length; i++) {
                html += '<option value="' + header.PossibleValues[i].Id + '"' + (header.PossibleValues[i].Id === value ? ' selected="true"' : '') + '>' + header.PossibleValues[i].Value + '</option>';
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
            html += '<input class="filter-value form-control' + (header.Type === 'date' || header.Type === 'datetime' ? ' datetimepicker' : '') + '" value="' + value + '" />';
        }
        return html;
    };

    function actingOnChange(tr) {
        try {
            tr.find('.rating').rating('destroy');
        } catch (e) { }
        var oldInput = tr.find('.filter-value');
        var field = findField(tr.find('.filter-actingon').val());
        var html = generateValueInput(field);
        oldInput.replaceWith(html);
        var testHtml = '';
        if (field.Type === 'single selection' || field.Type === 'boolean') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option>';
        } else if (field.Type === 'multiple selection') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="in">In</option><option value="notin">Not In</option>';
        } else if (field.Type === 'number' || field.Type.indexOf('rating') === 0) {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else if (field.Type === 'date') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="^=">Starts With</option><option value="!^=">Does Not Start With</option><option value="$=">Ends With</option><option value="!$=">Does Not End With</option><option value="=rgx=">Regex Match</option><option value="!=rgx=">Regex Mismatch</option><option value="*=">Contains</option><option value="!*=">Does Not Contain</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        }

        tr.find('.filter-test').html(testHtml);
        try {
            tr.find('.rating').rating();
        } catch (e) { }
        try {
            tr.find('.datetimepicker').datetimepicker();
        } catch (e) { }
    };

    function deleteStatement(tr, event) {
        event.preventDefault();
        tr.remove();
        _my.modal.find('.filter').css('background-color', 'inherit');
        _my.modal.find('.filter').find('input[name$="GroupNext"]').val('false');
        colorIndex = -1;
        _my.modal.find('tr.filter').each(function (i) {
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
    
    function group(e) {
        e.preventDefault();
        var t_checkedAmount = _my.modal.find('.filter-groupNext:checked').length;
        if (t_checkedAmount > 1) {
            colorIndex++;
        } else {
            return;
        }
        if (colorIndex >= groupColors.length) {
            colorIndex = 0;
        }
        var found = false;
        var skipped = false;
        var lastItem = null;
        var count = 0;
        var changed = [];
        _my.modal.find('.filter-groupNext').each(function (i) {
            if (found) {
                if (!($(this).is(':checked'))) {
                    skipped = true;
                }
            }
            if ($(this).is(':checked') && !skipped) {
                var tr = $(this).closest('tr');
                count++;
                found = true;
                lastItem = tr.find('.filter-groupNext-value');
                lastItem.val('true');
                tr.css('background-color', groupColors[colorIndex]);
                changed.push(tr);
            }
        });
        if (count == 1) {
            changed[0].css('background-color', 'inherit');
            changed[0].find('.filter-groupNext-value').val('false');
        }
        lastItem.val('false');
        _my.modal.find('.filter-groupNext').prop('checked', false);
    };

    function ungroup(e) {
        e.preventDefault();
        _my.modal.find('.filter').css('background-color', 'inherit');
        _my.modal.find('.filter').find('.filter-groupNext-value').val('false');
        colorIndex = -1;
    };

    // #endregion

    return _my;
}(jsonFiltering || {}));