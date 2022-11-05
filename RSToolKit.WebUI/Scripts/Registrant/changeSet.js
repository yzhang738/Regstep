/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    //#region reportInformation
    var registrantId = $('#registrant_id').val();
    //#endregion
    processing.showPleaseWait('Performing Initial Load', 'Please be patient', 100);
    loadInitPage();
    function loadInitPage() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Registrant/ChangeSet/' + registrantId, true);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                alert(result.message);
            }
            updateTable(result.table);
            result = null;
        }
        xhr.send();
        xhr = null;
    }
    function updateTable(table) {
        var html = '<table id="changeLog" class="table table-striped table-registration-data"><thead><tr>';
        for (var headerIndex = 0; headerIndex < table.Headers.length; headerIndex++) {
            html += '<th>' + table.Headers[headerIndex].Value + '</th>';
        }
        html += '</thead><tbody>';
        for (var rowIndex = 0; rowIndex < table.Rows.length; rowIndex++) {
            var t_row = table.Rows[rowIndex];
            html += '<tr id="' + t_row.Id + '" data-rowId="' + t_row.Id + '">';
            for (var valIndex = 0; valIndex < t_row.Values.length; valIndex++) {
                t_value = t_row.Values[valIndex];
                html += '<td' + (t_value.Editable && rowIndex != 1 ? ' class="data-editable cursor-pointer"' : '') + ' data-id="' + t_value.Id + '" data-headerId="' + t_value.HeaderId + '">';
                if (t_value.Type === 'date') {
                    var t_date = moment(t_value.Value);
                    if (t_date.isValid()) {
                        html += t_date.format("YYYY-MM-DD h:mm");
                    } else {
                        html += t_value.Value;
                    }
                } else {
                    html += t_value.Value;
                }
                html += '</td>';
            }
            html += '</tr>';
        }
        html += '</tbody></table>';
        $('#changeLogTableHolder').html(html);
        html = null;
        processing.hidePleaseWait();
    }
}());