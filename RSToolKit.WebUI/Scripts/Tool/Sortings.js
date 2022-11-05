/* Sorting v1.1.1.0        */
/* Created By: D.J. Enzyme */
/* Creation Date: 20141229 */
/* Modified Date: 20150120 */

/// <reference path="jTable.js" />

/* global JTableSorting */
/* jshint unused: false*/

var SORTING_VERSION = '1.1.1.0';

var SORTING_modalIndex = -1;

function Sorting(table, modalId, minimal) {
    /// <signature>
    /// <summary>Creates a Sorting object.</summary>
    /// <returns type="Sorting" />
    /// <param name="table" type="JTable">The JTable the sorting will apply too.</param>
    /// <param name="modalId" type="String">The modal Id of the sorting. default: index sorting</param>
    /// <field name="Table" type="jQuery">The Table that is being used for the sortings.</field>
    /// <field name="Modal" type="jQuery">The modal that is being used to edit sortings.</field>
    /// </signature>
    if (typeof (modalId) === 'undefined') {
        modalId = null;
    }
    var min_layout = false;
    if (typeof (minimal) === 'boolean') {
        min_layout = minimal;
    }
    var sortingIndex = -1;
    var colorIndex = -1;
    var t_holder = this;
    this.Table = table;
    this.Modal = null;
    if (!min_layout) {
        this.Modal = SORTING_GenerateModal(modalId);
    }
    this.GenerateActing = function (value) {
        /// <signature>
        /// <summary>Generating the acting on input.</summary>
        /// <param name="value" type="String">The value to prefill for acting on.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="sorting-actingon form-control">';
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
        this.Modal.find('.table-sorting-body').html('');
        sortingIndex = -1;
        var html = '';
        for (var i = 0; i < this.Table.Sortings.length; i++) {
            var t_sorting = this.Table.Sortings[i];
            sortingIndex++;
            html = '<tr class="sorting" data-index="' + sortingIndex + '"><td>';
            html += '<a href="#" class="sorting-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            html += this.GenerateActing(t_sorting.ActingOn);
            html += '</td>';
            html += '<td>';
            html += '<select class="form-control sorting-ascending">';
            html += '<option value="true"' + (t_sorting.Ascending ? ' selected="selected"' : '') + '>Ascending</option>';
            html += '<option value="false"' + (!t_sorting.Ascending ? ' selected="selected"' : '') + '>Descending</option>';
            html += '</select>';
            html += '</td>';
            html += '</tr>';
            this.Modal.find('.table-sorting-body').append(html);
        }
        $('.sorting-delete').on('click', function (e) {
            t_holder.DeleteSorting($(this).parents('tr'), e);
        });

    };
    this.DeleteSorting = function (tr, event) {
        /// <signature>
        /// <summary>Deletes a sorting.</summary>
        /// <param name="tr" type="jQuery">The tr html tag to remove.</param>
        /// <param name="evemt" type="JavascriptEvent">The mouse event.</param>
        /// </signature>
        event.preventDefault();
        if (this.Modal === null) {
            return;
        }
        $(tr).remove();
        this.Modal.find('.sorting').css('background-color', 'inherit');
        this.Modal.find('.sorting-groupnext').prop('checked', false);
        colorIndex = -1;
        this.Modal.find('.sorting').each(function (i) {
            sortingIndex = i;
            $(this).attr('data-index', i);
        });
        sortingIndex--;
    };
    this.AddSorting = function () {
        /// <signature>
        /// <summary>Adds a new sorting to the table.</summary>
        /// </signature>
        sortingIndex++;
        var html = '<tr class="sorting" data-index="' + sortingIndex + '"><td>';
        html += '<a href="#" class="sorting-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        html += this.GenerateActing();
        html += '</td>';
        html += '<td>';
        html += '<select class="form-control sorting-ascending">';
        html += '<option value="true">Ascending</option>';
        html += '<option value="false">Descending</option>';
        html += '</select>';
        html += '</td>';
        html += '</tr>';
        $('.table-sorting-body').append(html);
        $('.sorting[data-index="' + sortingIndex + '"]').find('.sorting-delete').on('click', function (e) {
            t_holder.DeleteSorting($(this).closest('tr'), e);
        });
    };
    if (this.Modal !== null) {
        this.Modal.find('.set-sortings').on('click', function () {
            t_holder.Modal.modal('hide');
            t_holder.Table.Sortings = [];
            t_holder.Modal.find('.sorting').each(function (i) {
                var sorting = new JTableSorting();
                sorting.ActingOn = $(this).find('.sorting-actingon').val();
                sorting.Order = i;
                sorting.Ascending = $(this).find('.sorting-ascending').val().toLowerCase() === 'true';
                t_holder.Table.Sortings.push(sorting);
                t_holder.Table.Sorted = false;
            });
            t_holder.Table.GetPage();
        });
        this.Modal.find('.add-sorting').on('click', function (e) {
            e.preventDefault();
            t_holder.AddSorting();
        });
    }
}

function SORTING_GenerateModal(modalId) {
    /// <signature>
    /// <summary>Creates a sorting modal.</summary>
    /// <return type="jQuery" />
    /// <param name="modalId" type="String">The id to use for the modal. default: a incremental number.</modalId>
    /// </signature>
    if (modalId === null) {
        modalId = 'm_sorting' + (++SORTING_modalIndex);
    }
    if (modalId.legth > 0 && modalId[0] === '#') {
        modalId = modalId.substr(1, modalId.length);
    }
    var object = $(modalId);
    if (object.length > 0) {
        return object;
    }
    var sortingModal_html = '<div class="modal fade" id="' + modalId + '"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Sortings</h3></div>';
    sortingModal_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Sortings</h4></div>';
    sortingModal_html += '<div class="row"><table class="table table-sorting">';
    sortingModal_html += '<thead><tr><th></th><th>Header</th><th>Sort Order</th></tr></thead>';
    sortingModal_html += '<tbody class="table-sorting-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-sorting"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    sortingModal_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default set-sortings">Apply Sortings</button>';
    sortingModal_html += '</div></div></div>';
    $('body').append(sortingModal_html);
    object = $('#' + modalId);
    return object;
}