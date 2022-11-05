/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    //#region reportInformation
    var reportId = $('#report_id').val();
    var token = null;
    //#endregion
    processing.showPleaseWait('Performing Initial Load', 'Please be patient', 100);
    loadInitPage();
    function loadInitPage() {
        var xhr = new XMLHttpRequest();
        var useId = reportId;
        if (token !== null)
            useId = token;
        xhr.open('get', window.location.origin + '/InventoryReport/View/' + useId, true);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (!result.success) {
                    processing.update(result.message, (result.progress * 100).toFixed(2) + '%', result.progress);
                    token = result.token;
                    setTimeout(function () { loadProgress(); }, 100);
                    return;
                } else {
                    updateTable(result.table);
                    result = null;
                }
            } else {
                RESTFUL.showError(result.message, 'Unhandled Server Error');
            }
        }
        xhr.send();
        xhr = null;
    }

    function loadProgress() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/InventoryReport/View/' + token, true);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (!result.success) {
                    processing.update(result.data.Message, (result.data.Progress * 100).toFixed(2) + '%', result.data.Progress, result.data.Details);
                    setTimeout(function () { loadProgress(); }, 100);
                    return;
                } else {
                    updateTable(result.data);
                    result = null;
                }
            } else {
                RESTFUL.showError(result.message, 'Unhandled Server Error');
            }
        }
        xhr.send();
        xhr = null;
    }


    function updateTable(table) {
        var html = '<table id="reportTable" class="table table-striped table-registration-data"><thead><tr>';
        for (var headerIndex = 0; headerIndex < table.Headers.length; headerIndex++) {
            html += '<th>' + table.Headers[headerIndex].Value + '</th>';
        }
        html += '</thead><tbody>';
        for (var rowIndex = 0; rowIndex < table.Rows.length; rowIndex++) {
            var t_row = table.Rows[rowIndex];
            html += '<tr id="' + t_row.Id + '" data-rowId="' + t_row.Id + '">';
            for (var valIndex = 0; valIndex < t_row.Values.length; valIndex++) {
                t_value = t_row.Values[valIndex];
                html += '<td>';
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
        $('#reportTable').html(html);
        html = null;
        processing.hidePleaseWait();
    }
}());