/* POFilter v1.1.1.0         */
/* Created By: D.J. Enzyme   */
/* Creation Date: 20141229   */
/* Modified Date: 20150120   */

/* global FindHeader */
/* global JTableFilter */
/* jshint unused: false*/

/// <reference path="jTable.js" />

var POFILTER_colors = [
    "#FFFFB0",
    "#B0B0FF",
    "#68C473",
    "#C468B9",
    "#6669BA",
    "#BAB766"
];
var POFILTER_modalIndex = -1;
var POFILTER_logicIndex = 0;
var POFILTER_VERSION = '1.1.1.0';

function POFilter(id) {
    /// <signature>
    /// <summary>Creates a POFilter object.</summary>
    /// <returns type="POFilter" />
    /// <param name="id" type="String">The id of the owner.</field>
    /// <field name="Filters" type="Array" elementtype="JTableFilter">The list of filters.</field>
    /// <field name="Commands" type="Array">The list of commands.</field>
    /// <field name="Headers" type="Array">The list of headers being used.</field>
    /// <field name="Order" type="Number" integer="true">The order of the logic.</field>
    /// <field name="Name" type="String">The name of the logic.</field>
    /// <field name="Id" type="String">The Id of the logic.</field>
    /// <field name="OnLoad" type="Boolean">The boolean of whether it is run on load or on unload.</field>
    /// </signature>
    var p_process = prettyProcessing;
    var filterIndex = -1;
    var colorIndex = -1;
    var t_logic = this;
    this.Owner = id;
    this.Filters = [];
    this.Commands = [];
    this.Headers = [];
    this.Order = 0;
    this.Name = '';
    this.OnLoad = true;
    this.Id = '';
    this.FindPoHeader = function (headerId) {
        /// <signature>
        /// <summary>Finds a specific header.</summary>
        /// <returns type="JTableHeader" />
        /// <param name="headerId" type="String">The header id to search for.</param>
        /// </signature>
        for (var i = 0; i < this.Headers.length; i++) {
            if (this.Headers[i].Id === headerId) {
                return this.Headers[i];
            }
        }
        return null;
    };
    this.GenerateActing = function (value) {
        /// <signature>
        /// <summary>Generating the acting on input.</summary>
        /// <param name="value" type="String">The value to prefil.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="form-control filter-actingon" data-param-name="Variable"><option value="">Make a Selection</option>';
        var fheads = this.GetHeadersByType('form');
        var cheads = this.GetHeadersByType('contact');
        if (fheads.length > 0) {
            html += '<optgroup label="Form Components">';
            for (var l = 0; l < fheads.length; l++) {
                html += '<option' + (value == fheads[l].Id ? ' selected="true"' : '') + ' value="' + fheads[l].Id + '">' + fheads[l].Label + '</option>';
            }
            html += '</optgroup>';
        }
        if (cheads.length > 0) {
            html += '<optgroup label="Contact Headers">';
            for (var l = 0; l < cheads.length; l++) {
                html += '<option' + (value == cheads[l].Id ? ' selected="true"' : '') + ' value="' + cheads[l].Id + '">' + cheads[l].Label + '</option>';
            }
            html += '</optgroup>';
        }
        html += '</select>';
        return html;
    };
    this.Generate = function () {
        /// <signature>
        /// <summary>Generates the sortings to place in the table</summary>
        /// </signature>

        var filter_html = '<div class="add-padding-top"><label class="control-label">Name:</label> <input type="text" class="form-component logic-name" value="' + this.Name + '" /></div>';
        filter_html += '<div class="add-padding-top text-color-1"><h4>Current Logic</h4></div>';
        filter_html += '<div class="row color-grey-2 add-padding-vertical-5"><div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="group"><span class="glyphicon glyphicon-link"></span> Group Selected</a></div></div>';
        filter_html += '<div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="ungroup"><span class="glyphicon glyphicon-link"></span> Ungroup All</a></div></div></div>';
        filter_html += '<div class="row"><table class="table table-filter">';
        filter_html += '<thead><tr><th></th><th>Link</th><th>Variable</th><th>Test</th><th>Value</th><th>Case Sensitive<th></tr></thead>';
        filter_html += '<tbody class="table-filter-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-filter"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
        filter_html += '</table></div>';
        filter_html += '<div class="add-padding-vertical-10 text-color-1"><h4>Current Commands</h4></div>';
        filter_html += '<div class="add-padding-vertical-5 color-grey-2" style="padding-left:30px;">Then</div>';
        filter_html += '<div class="row" style="padding-left:60px"><table class="table table-then">';
        filter_html += '<thead><tr><th></th><th>Command</th><th>Parameters</th></tr></thead>';
        filter_html += '<tbody class="table-then-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-then"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Then Command</a></td></tr></tbody>';
        filter_html += '</table></div>';
        filter_html += '<div class="add-padding-vertical-5 color-grey-2" style="padding-left:30px;">Else</div>';
        filter_html += '<div class="row" style="padding-left:60px;"><table class="table table-else">';
        filter_html += '<thead><tr><th></th><th>Command</th><th>Parameters</th></tr></thead>';
        filter_html += '<tbody class="table-else-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-else"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Else Command</a></td></tr></tbody>';
        filter_html += '</table></div>';

        $('#filters').html(filter_html);
        $('.logic-name').on('input', function (e) {
            t_logic.Name = $(this).val();
        });
        filterIndex = -1;
        var html = '';
        var grouping = false;
        for (var i = 0; i < this.Filters.length; i++) {
            filterIndex++;
            var t_filter = this.Filters[i];
            if (t_filter.GroupNext && !grouping) {
                colorIndex++;
                grouping = true;
                if (colorIndex >= POFILTER_colors.length) {
                    colorIndex = 0;
                }
            }
            var t_header = this.FindPoHeader(t_filter.ActingOn);
            html = '<tr class="filter"' + (grouping ? ' style="background-color:' + POFILTER_colors[colorIndex] + ';"' : '') + ' data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupnext-value" value="' + (t_filter.GroupNext ? 'true' : 'false') + '" />';
            html += '<input type="checkbox" class="filter-groupnext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            if (i != 0) {
                html += '<select class="filter-link form-control">';
                html += '<option value="and"' + (t_filter.Link === 'and' ? ' selected="selected"' : '') + '>And</option>';
                html += '<option value="or"' + (t_filter.Link === 'or' ? ' selected="selected"' : '') + '>Or</option>';
                html += '</select>';
            } else {
                html += '<input type="hidden" value="none" class="filter-link" />';
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
            html += '<td><input type="checkbox" class="filter-casesensitive"' + (t_filter.CaseSensitive ? ' checked="true"' : '') + ' /></td>';
            html += '</tr>';
            $('#filters').find('.table-filter-body').append(html);
            var testHtml = "";
            if (t_header.Type === 'itemParent') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option>';
            } else if (t_header.Type === 'multipleSelection') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="in"' + (t_filter.Test === 'in' ? ' selected="selected"' : '') + '>In</option><option value="notin"' + (t_filter.Test === 'notin' ? ' selected="selected"' : '') + '>Not In</option>';
            } else if (t_header.Type === 'number' || t_header.Type.indexOf('rating') === 0) {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else if (t_header.Type === 'date' || t_header.Type === 'datetime') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else if (t_header.Type === 'contact') {
                testHtml = '<option value="in"' + (t_filter.Test === 'in' ? ' selected="selected"' : '') + '>Has</option><option value="notin"' + (t_filter.Test === 'notin' ? ' selected="selected"' : '') + '>Has Not</option>';
            } else {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="^="' + (t_filter.Test === '^=' ? ' selected="selected"' : '') + '>Starts With</option><option value="!^="' + (t_filter.Test === '!^=' ? ' selected="selected"' : '') + '>Does Not Start With</option><option value="$="' + (t_filter.Test === '$=' ? ' selected="selected"' : '') + '>Ends With</option><option value="!$="' + (t_filter.Test === '!$=' ? ' selected="selected"' : '') + '>Does Not End With</option><option value="=rgx="' + (t_filter.Test === '=rgx=' ? ' selected="selected"' : '') + '>Regex Match</option><option value="!=rgx="' + (t_filter.Test === '!=rgx=' ? ' selected="selected"' : '') + '>Regex Mismatch</option><option value="*="' + (t_filter.Test === '*=' ? ' selected="selected"' : '') + '>Contains</option><option value="!*="' + (t_filter.Test === '!*=' ? ' selected="selected"' : '') + '>Does Not Contain</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            }
            if (!t_filter.GroupNext) {
                grouping = false;
            }
            $('tr[data-index="' + filterIndex + '"] .filter-test').html(testHtml);
        }
        $('#filters').find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            t_logic.ActingOnChange(tr);
        });
        $('#filters').find('.filter-delete').on('click', function (e) {
            t_logic.DeleteStatement($(this).closest('tr'), e);
        });
        var tCom = "";
        var eCom = "";
        for (var j = 0; j < this.Commands.length; j++) {
            var cmd = this.Commands[j];
            var html = '<tr class="command"><td><span class="glyphicon glyphicon-trash command-delete cursor-pointer"></span></td><td><select class="form-control command-command"><option value="-1">Select Command</option>';
            for (var ci = 0; ci < POFILTER_WORK.length; ci++) {
                html += '<option' + (cmd.Command === ci ? ' selected="true"' : '') + ' value="' + ci + '">' + POFILTER_WORK[ci] + '</option>';
            }
            html += '</select></td><td class="command-parameters form-horizontal">';
            if (cmd.Command === 1) {
                html += '<div class="form-group"><label class="control-label col-sm-12 col-md-4">Variable: </label>';
                html += this.GenerateHeaders(cmd.Parameters["Variable"]);
                html += '</div>';
                html += '<div class="param-value form-group"><label class="control-label col-sm-12 col-md-4">Value: </label><div class="col-sm-12 col-md-8">';
                html += this.GenerateValueInput(this.FindPoHeader(cmd.Parameters['Variable']), cmd.Parameters['Value'], 'Value');
                html += '</div></div>';
            } else if (cmd.Command === 2) {
                html += '<div class="form-group"><label class="control-label col-sm-12 col-md-4">Text: </label><div class="col-sm-12 col-md-8"><input class="form-control command-parameter" data-param-name="Text" value="' + cmd.Parameters['Text'] + '" /></div></div>';
            } else if (cmd.Command === 7) {
                html += '<div class="form-group"><label class="control-label col-sm-12 col-md-4">Error Message: </label><div class="col-sm-12 col-md-8"><input class="form-control command-parameter" data-param-name="Text" value="' + cmd.Parameters['Text'] + '" /></div></div>';
            }
            html += '</td></tr>';
            if (cmd.CommandType === 0) {
                tCom += html;
            } else {
                eCom += html;
            }
        }
        $('.table-else-body').html(eCom);
        $('.table-then-body').html(tCom);
        $('.command-command').on('change', function () {
            t_logic.GenerateParameters($(this).val(), $(this).closest('tr'));
        });
        $('.command-parameter-var').on('change', function (i) {
            $(this).closest('td').find('.param-value').html('<label class="control-label col-xs-12 col-md-4">Value: </label><div class="col-sm-12 col-md-8">' + t_logic.GenerateValueInput(t_logic.FindPoHeader($(this).val()), '', 'Value') + '</div>');
        });
        $('.command-delete').on('click', function () {
            $(this).closest('tr').remove();
        });
        $('.add-then').on('click', function () {
            t_logic.AddThen();
        });
        $('.add-else').on('click', function () {
            t_logic.AddElse();
        });
        try {
            $('.datetimepicker').datetimepicker();
        } catch (e) { }
    };
    this.DeleteStatement = function (tr, event) {
        /// <signature>
        /// <summary>Deletes a statement.</summary>
        /// <param name="tr" type="jQuery">The tr html tag to remove.</param>
        /// <param name="event" type="JavascriptEvent">The mouse event.</param>
        /// </signature>
        event.preventDefault();
        tr.remove();
        $('#filters').find('.filter').css('background-color', 'inherit');
        $('#filters').find('.filter').find('input[name$="GroupNext"]').val('false');
        colorIndex = -1;
        $('#filters').find('tr.filter').each(function (i) {
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
    this.AddThen = function () {
        var order = this.Headers.length + 1;
        var id = 'work_' + POFILTER_logicIndex++;
        var html = '<tr class="command" id="' + id + '" ><td><span class="glyphicon glyphicon-trash command-delete cursor-pointer"></span></td><td><select class="form-control command-command"><option value="-1">Select Command</option>';
        for (var ci = 0; ci < POFILTER_WORK.length; ci++) {
            html += '<option value="' + ci + '">' + POFILTER_WORK[ci] + '</option>';
        }
        html += '</select></td><td class="command-parameters form-horizontal">';
        html += '</td></tr>';
        $('.table-then-body').append(html);
        $('#' + id).find('.command-command').on('change', function () {
            t_logic.GenerateParameters($(this).val(), $(this).closest('tr'));
        });
        $('#' + id).find('.command-delete').on('click', function () {
            $(this).closest('tr').remove();
        });
    };
    this.AddElse = function () {
        var order = this.Headers.length + 1;
        var id = 'work_' + POFILTER_logicIndex++;
        var html = '<tr class="command" id="' + id + '" ><td><span class="glyphicon glyphicon-trash command-delete cursor-pointer"></span></td><td><select class="form-control command-command"><option value="-1">Select Command</option>';
        for (var ci = 0; ci < POFILTER_WORK.length; ci++) {
            html += '<option value="' + ci + '">' + POFILTER_WORK[ci] + '</option>';
        }
        html += '</select></td><td class="command-parameters form-horizontal">';
        html += '</td></tr>';
        $('.table-else-body').append(html);
        $('#' + id).find('.command-command').on('change', function () {
            t_logic.GenerateParameters($(this).val(), $(this).closest('tr'));
        });
        $('#' + id).find('.command-delete').on('click', function () {
            $(this).closest('tr').remove();
        });
    };
    this.GenerateHeaders = function (val) {
        if (typeof (val) === 'undefined') {
            val = '';
        }
        var html = '<div class="col-sm-12 col-md-8"><select class="form-control command-parameter command-parameter-var" data-param-name="Variable"><option value="">Make a Selection</option>';
        var fheads = this.GetHeadersByType('form');
        var cheads = this.GetHeadersByType('contact');
        if (fheads.length > 0) {
            html += '<optgroup label="Form Components">';
            for (var l = 0; l < fheads.length; l++) {
                if (!fheads[l].Editable) {
                    continue;
                }
                html += '<option' + (val == fheads[l].Id ? ' selected="true"' : '') + ' value="' + fheads[l].Id + '">' + fheads[l].Label + '</option>';
            }
            html += '</optgroup>';
        }
        if (cheads.length > 0) {
            html += '<optgroup label="Contact Headers">';
            for (var l = 0; l < cheads.length; l++) {
                if (!cheads[l].Editable) {
                    continue;
                }
                html += '<option' + (val == cheads[l].Id ? ' selected="true"' : '') + ' value="' + cheads[l].Id + '">' + cheads[l].Label + '</option>';
            }
            html += '</optgroup>';
        }
        html += '</select></div>';
        return html;
    };
    this.GenerateParameters = function (workVal, tr) {
        var work = parseInt(workVal);
        if (work === -1) {
            tr.find('.command-parameters').html('');
        }
        if (work === 1) {
            var html = '<div class="form-group"><label class="control-label col-xs-12 col-s-12 col-md-4">Variable: </label>';
            html += this.GenerateHeaders('');
            html += '</div>';
            html += '<div class="param-value form-group"><label class="control-label col-xs-12 col-s-12 col-md-4">Value: </label>';
            html += '</div>';
            tr.find('.command-parameters').html(html);
            tr.find('.command-parameter-var').on('change', function (i) {
                $(this).closest('td').find('.param-value').html('<label class="control-label col-xs-12 col-md-4">Value: </label><div class="col-sm-12 col-md-8">' + t_logic.GenerateValueInput(t_logic.FindPoHeader($(this).val()), '', 'Value') + '</div>');
            });

        } else if (work === 2) {
            tr.find('.command-parameters').html('<div class="form-group"><label class="control-label col-xs-12 col-s-12 col-md-4">Text: </label><div class=" col-xs-12 col-s-12 col-md-8"><input class="form-control command-parameter" data-param-name="Text" value="" /></div></div>');
        } else if (work === 7) {
            tr.find('.command-parameters').html('<div class="form-group"><label class="control-label col-xs-12 col-s-12 col-md-4">Error Message: </label><div class=" col-xs-12 col-s-12 col-md-8"><input class="form-control command-parameter" data-param-name="Text" value="" /></div></div>');
        } else {
            tr.find('.command-parameters').html('');
        }
    };
    this.AddStatement = function () {
        /// <signature>
        /// <summary>Adds a new statement to the table.</summary>
        /// </signature>
        filterIndex++;
        var first = false;
        if ($('.filter').length == 0) {
            first = true;
        }
        var t_filter = new JTableFilter();
        var header = this.FindPoHeader(t_filter.ActingOn);
        var html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupnext-value" />';
        html += '<input type="checkbox" class="filter-groupnext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        if (!first) {
            html += '<select class="filter-link form-control">';
            html += '<option value="and">And</option>';
            html += '<option value="or">Or</option>';
            html += '</select>';
        } else {
            html += '<input type="hidden" value="none" class="filter-link" />';
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
        html += '<td><input type="checkbox" class="filter-casesensitive"' + (t_filter.CaseSensitive ? ' checked="true"' : '') + ' /></td>';
        html += '</tr>';
        $('#filters').find('.table-filter-body').append(html);
        $('#filters').find('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            t_logic.ActingOnChange(tr);
        });
        $('#filters').find('.filter[data-index="' + filterIndex + '"]').find('.filter-delete').on('click', function (e) {
            var tr = $(this).closest('tr');
            t_logic.DeleteStatement(tr, e);
        });
    };
    this.Group = function (e) {
        e.preventDefault();
        var t_checkedAmount = $('.table-filter').find('.filter-groupnext:checked').length;
        if (t_checkedAmount > 1) {
            colorIndex++;
        } else {
            return;
        }
        if (colorIndex >= POFILTER_colors.length) {
            colorIndex = 0;
        }
        var found = false;
        var skipped = false;
        var lastItem = null;
        var count = 0;
        var changed = [];
        $('#filters').find('.filter-groupnext').each(function (i) {
            if (found) {
                if (!($(this).is(':checked'))) {
                    skipped = true;
                }
            }
            if ($(this).is(':checked') && !skipped) {
                var tr = $(this).closest('tr');
                count++;
                found = true;
                lastItem = tr.find('.filter-groupnext-value');
                lastItem.val('true');
                tr.css('background-color', POFILTER_colors[colorIndex]);
                changed.push(tr);
            }
        });
        if (count == 1) {
            changed[0].css('background-color', 'inherit');
            changed[0].find('.filter-groupnext-value').val('false');
        }
        lastItem.val('false');
        $('#filters').find('.filter-groupnext').prop('checked', false);
    };
    this.Ungroup = function (e) {
        e.preventDefault();
        $('#filters').find('.filter').css('background-color', 'inherit');
        $('#filters').find('.filter').find('.filter-groupnext-value').val('false');
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
        var header = this.FindPoHeader(tr.find('.filter-actingon').val());
        var html = this.GenerateValueInput(header);
        oldInput.replaceWith(html);
        var testHtml = '';
        if (header.Type === 'itemParent' || header.Type === 'boolean') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option>';
        } else if (header.Type === 'multipleSelection') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="in">In</option><option value="notin">Not In</option>';
        } else if (header.Type === 'number' || header.Type.indexOf('rating') === 0) {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else if (header.Type === 'date' || header.Type === 'datetime') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else if (header.Type === 'contact') {
            testHtml = '<option value="in" >Has</option><option value="notin">Has Not</option>';
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
    this.GenerateValueInput = function (header, value, param) {
        /// <signature>
        /// <summary>Generates the input for the filter value.</summary>
        /// <returns type="String" />
        /// <param name="header" type="JTableHeader">The JTableHeader to generate off of.</param>
        /// <param name="value" type="String">The value to prefil.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var paramClass = true;
        if (typeof (param) === 'undefined') {
            paramClass = false;
        }
        var html = '';
        if (header === null) {
            return '<span class="filter-value">Make a Selection</span>';
        }
        if (header.Type === 'itemParent' || header.Type === 'boolean') {
            html += '<select class="filter-value form-control' + (paramClass ? ' command-parameter command-parameter-val" data-param-name="' + param + '"' : '"') + '>';
            for (var i = 0; i < header.PossibleValues.length; i++) {
                html += '<option value="' + header.PossibleValues[i].Id + '"' + (header.PossibleValues[i].Id === value ? ' selected="true"' : '') + '>' + header.PossibleValues[i].Label + '</option>';
            }
            html += '</select>';
        } else if (header.Type === 'multipleSelection') {
            if (paramClass) {
                if (value === '') {
                    value = '[]';
                }
                var t_array = JSON.parse(value);
                for (var i = 0; i < header.PossibleValues.length; i++) {
                    var selected = false;
                    if ($.inArray(header.PossibleValues[i].Id, t_array) !== -1) {
                        selected = true;
                    }
                    if (i != 0) {
                        html += '<br />';
                    }
                    html += '<label><input type="checkbox" value="' + header.PossibleValues[i].Id + '" class="command-parameter command-parameter-val command-parameter-multiple" data-param-name="Value"' + (selected ? ' checked=\"true\"' : '') + ' /> ' + header.PossibleValues[i].Label + '</label>';
                }
            } else {
                html += '<select class="filter-value form-control" >';
                html += '<option value="">No Selection</option>';
                for (var i = 0; i < header.PossibleValues.length; i++) {
                    html += '<option value="' + header.PossibleValues[i].Id + '"' + (header.PossibleValues[i].Id === value ? ' selected="true"' : '') + '>' + header.PossibleValues[i].Label + '</option>';
                }
                html += '</select>';
            }
        } else if (header.Type.indexOf('rating') === 0) {
            if (value === '') {
                value = '0';
            }
            var t_json = header.Type;
            t_json = t_json.split("=>")[1];
            var t_options = JSON.parse(t_json);
            html += '<input class="filter-value form-control rating' + (paramClass ? ' command-parameter command-parameter-val" data-param-name="' + param + '"' : '"') + '" min="' + t_options.min + '" max="' + t_options.max + '" data-step="' + t_options.step + '" type="number" value="' + value + '" />';
        } else {
            var itemClass = 'filter-value form-control';
            if (header.Type === 'date' || header.Type === 'datetime') {
                itemClass += ' datetimepicker';
            }
            if (paramClass) {
                itemClass += ' command-parameter command-parameter-val';
            }
            html += '<input class="' + itemClass + '"' + (paramClass ? ' data-param-name="' + param + '"' : '') + '  value="' + (value === null ? '' : value) + '" />';
        }
        return html;
    };
    this.UpdateFilters = function () {
        this.Filters = [];
        $('.filter').each(function (i) {
            var tr = $(this);
            var filter = new JTableFilter();
            filter.ActingOn = tr.find('.filter-actingon').val();
            filter.GroupNext = tr.find('.filter-groupnext-value').val().toLowerCase() == 'true';
            filter.CaseSensitive = tr.find('.filter-casesensitive').prop('checked');
            filter.Link = tr.find('.filter-link').val();
            filter.Test = tr.find('.filter-test').val();
            filter.Order = (i + 1);
            filter.Value = tr.find('.filter-value').val();
            this.Filters.push(filter);
        });
    };
    this.Bind = function () {
        $('#filters').find('.add-filter').on('click', function (e) {
            e.preventDefault();
            t_logic.AddStatement();
        });
        $('#filters').find('.ungroup').on('click', function (e) {
            t_logic.Ungroup(e);
        });
        $('#filters').find('.group').on('click', function (e) {
            t_logic.Group(e);
        });

    };
    this.Load = function (id, lid) {
        var url = window.location.origin + '/FormBuilder/JLogic'
        if (typeof (id) === 'undefined' || typeof (lid) === 'undefined') {
            throw "You must define id and lid for the POFilter.Load function.";
        }
        var t_data = { id: id, lid: lid };
        AddJsonAntiForgeryToken(t_data);
        var t_param = '?id=' + id + '&lid=' + lid;
        var t_xhr = new XMLHttpRequest();
        t_xhr.addEventListener('progress', this.UpdateProgress, false);
        url += t_param;
        t_xhr.open('get', url, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var data = RESTFUL.parse(c_xhr);
                var logic = data.Logic;
                if (data.Success) {
                    t_logic.Filters = logic.Filters;
                    t_logic.Commands = logic.Commands;
                    t_logic.Headers = logic.Headers;
                    t_logic.Order = logic.Order;
                    t_logic.Name = logic.Name;
                    t_logic.Id = logic.Id;
                    t_logic.OnLoad = logic.OnLoad;
                    t_logic.Generate();
                    t_logic.Bind();
                } else {
                    RESTFUL.showError("Failed to load data.", "Unhandled Exception");
                }
            } else {
                RESTFUL.showError("Failed to load data.", "Unhandled Exception");
            }
            p_process.hidePleaseWait();
        }
        t_xhr.onerror = function (event) {
            RESTFUL.showError();
            p_process.hidePleaseWait();
        };
        p_process.showPleaseWait("Requesting Data", "Requesting data from the server.");
        t_xhr.send();
    }
    this.Save = function () {
        var logic = $.extend({}, this);
        logic.Filters = [];
        logic.Headers = [];
        logic.Commands = [];
        logic.Name = $('.logic-name').val();
        // First we grab all the filters
        $('.filter').each(function (i) {
            var filter = new JTableFilter();
            var tr = $(this);
            filter.Order = i + 1;
            filter.CaseSensitive = tr.find('.filter-casesensitive').prop('checked');
            filter.ActingOn = tr.find('.filter-actingon').val();
            filter.GroupNext = tr.find('.filter-groupnext-value').val() === 'true';
            filter.Link = tr.find('.filter-link').val();
            filter.Test = tr.find('.filter-test').val();
            filter.Value = tr.find('.filter-value').val();
            logic.Filters.push(filter);
        });
        // No we grab all the commands
        var thenOrder = 0;
        var elseOrder = 0;
        $('.command').each(function (i) {
            var command = {};
            var tr = $(this);
            command.Parameters = {};
            if (tr.closest('table').hasClass('table-then')) {
                command.CommandType = 0;
                command.Order = ++thenOrder;
            } else {
                command.CommandType = 1;
                command.Order = ++elseOrder;
            }
            command.Command = parseInt(tr.find('.command-command').val());
            // We need to reed in the parameters
            var values = [];
            var multSel = false;
            tr.find('.command-parameter').each(function (j) {
                if ($(this).hasClass('command-parameter-multiple')) {
                    multSel = true;
                    if (!$(this).prop('checked')) {
                        return;
                    }
                }
                if ($(this).hasClass('command-parameter-val')) {
                    values.push($(this).val());
                } else {
                    var key = $(this).attr('data-param-name');
                    var val = $(this).val();
                    command.Parameters[key] = val;
                }
            });
            if (command.Command === 1) {
                if (multSel) {
                    command.Parameters['Value'] = JSON.stringify(values);
                } else {
                    if (values.length == 0) {
                        command.Parameters['Value'] = '';
                    } else {
                        command.Parameters['Value'] = values[0];
                    }
                }
            }
            logic.Commands.push(command);
        });
        var url = window.location.origin + '/FormBuilder/JLogic'
        var t_data = { id: logic.Owner, model: logic };
        AddJsonAntiForgeryToken(t_data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.addEventListener('progress', this.UpdateProgress, false);
        t_xhr.open('put', url, true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var data = RESTFUL.parse(c_xhr);
                var logic = data.Logic;
                if (data.Success) {
                } else {
                    RESTFUL.showError("Failed to load data.", "Unhandled Exception");
                }
            } else {
                RESTFUL.showError("Failed to load data.", "Unhandled Exception");
            }
            prettyProcessing.hidePleaseWait();
        }
        t_xhr.onerror = function (event) {
            RESTFUL.showError();
            prettyProcessing.hidePleaseWait();
        };
        prettyProcessing.showPleaseWait("Requesting Data", "Requesting data from the server.");
        t_xhr.send(JSON.stringify(t_data));
    }
    this.GetHeadersByType = function (group) {
        var heads = [];
        for (var i = 0; i < t_logic.Headers.length; i++) {
            if (t_logic.Headers[i].Group === group) {
                heads.push(t_logic.Headers[i]);
            }
        }
        return heads;
    };
}

var POFILTER_WORK = ['Page Skip', 'Set Var', 'Display Text', 'Hide', 'Show', 'Form Halt', 'Send Email', 'Page Halt', 'Don\'t Send Email'];