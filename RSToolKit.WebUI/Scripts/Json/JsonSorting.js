/* JsonSorting v1.0.0.0        */
/* Created By: D.J. Enzyme     */
/* Creation Date: 20141229     */
/* Modified Date: 20150120     */

/* jshint unused: false*/


var jsonSorting = (function (_my) {

    var sortingIndex = -1;

    // Create modal
    var sortingModal_html = '<div class="modal fade" id="sortingModal"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Sortings</h3></div>';
    sortingModal_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Sortings</h4></div>';
    sortingModal_html += '<div class="row"><table class="table table-sorting">';
    sortingModal_html += '<thead><tr><th></th><th>Header</th><th>Sort Order</th></tr></thead>';
    sortingModal_html += '<tbody class="table-sorting-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-sorting"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    sortingModal_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default set-sortings">Apply Sortings</button>';
    sortingModal_html += '</div></div></div>';
    $('body').append(sortingModal_html);
    _my.modal = $('#sortingModal');
    _my.modal.find('.add-sorting').on('click', function (e) {
        e.preventDefault();
        sortingIndex++;
        var html = '<tr class="sorting" data-index="' + sortingIndex + '"><td>';
        html += '<a href="#" class="sorting-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        html += generateActing();
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
            deleteSorting($(this).closest('tr'), e);
        });
    });
    _my.modal.find('.set-sortings').on('click', function (e) {
        e.preventDefault();
        _my.modal.modal('hide');
        _my.getJsonSortings();
        _my.callback();
    });

    _my.fields = [];
    _my.sortings = [];
    _my.callback = function () {
        return;
    }

    _my.create = function (sortings, fields, callback) {
        _my.fields = fields;
        _my.sortings = sortings;
        _my.callback = callback;
    }

    _my.display = function () {
        _my.modal.find('.table-sorting-body').html('');
        sortingIndex = -1;
        var html = '';
        for (var i = 0; i < _my.sortings.length; i++) {
            var sorting = _my.sortings[i];
            sortingIndex++;
            html = '<tr class="sorting" data-index="' + sortingIndex + '"><td>';
            html += '<a href="#" class="sorting-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            html += generateActing(sorting.Id);
            html += '</td>';
            html += '<td>';
            html += '<select class="form-control sorting-ascending">';
            html += '<option value="true"' + (sorting.Ascending ? ' selected="selected"' : '') + '>Ascending</option>';
            html += '<option value="false"' + (!sorting.Ascending ? ' selected="selected"' : '') + '>Descending</option>';
            html += '</select>';
            html += '</td>';
            html += '</tr>';
            _my.modal.find('.table-sorting-body').append(html);
        }
        $('.sorting-delete').on('click', function (e) {
            deleteSorting($(this).parents('tr'), e);
        });
        _my.modal.modal('show');
    }

    _my.clear = function () {
        _my.modal.find('.sorting').remove();
    };

    _my.getJsonSortings = function () {
        var sortings = [];
        _my.modal.find('.sorting').each(function () {
            sortings.push({ Id: $(this).find('.sorting-actingon').val(), Ascending: $(this).find('.sorting-ascending').val() === 'true' });
        });
        _my.sortings = sortings;
        return _my.sortings;
    };

    function deleteSorting(tr, event) {
        event.preventDefault();
        $(tr).remove();
        _my.modal.find('.sorting').css('background-color', 'inherit');
        _my.modal.find('.sorting-groupnext').prop('checked', false);
        _my.modal.find('.sorting').each(function (i) {
            sortingIndex = i;
            $(this).attr('data-index', i);
        });
        sortingIndex--;
    }

    function generateActing(value) {
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="sorting-actingon form-control">';
        for (var i = 0; i < _my.fields.length; i++) {
            var field = _my.fields[i];
            html += '<option value="' + field.Id + '"' + (field.Id === value ? ' selected="selected"' : '') + '>' + field.Label + '</option>';
        }
        html += '</select>';
        return html;
    }

    return _my;
}(jsonSorting || {}));