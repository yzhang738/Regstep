/// <reference path="../Sorting/sortable.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Layout/breadCrumb.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/JsonFilter.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    //#region reportInformation
    var report_id = $('#report_id').val();
    //#endregion
    processing.showPleaseWait('Requesting Table Data', 'Waiting for Response', 100);
    load();

    function load() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/GlobalReport/View/' + report_id, true);
        xhr.ontimeout = function () { alert('timeout'); };
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () { alert('an error occured') };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (!result.success) {
                // The table is not done yet, we will keep trying.
                if (result.data.progress.HardFail) {
                    // There was a hard failure.
                    window.location = result.location;
                    return;
                }
                processing.update(result.data.progress.Message, (result.data.progress.Status * 100).toFixed(2) + '%', result.data.progress.Status, result.data.progress.Details)
                setTimeout(load, 200)
                return;
            }
            var tables = result.data;
            var html = '<div><ul class="nav nav-tabs" rolw="tablist">';
            for (var t_index = 0; t_index < tables.length; t_index++) {
                html += '<li role="presentation"' + (t_index == 0 ? ' class="active"' : '') + '><a href="#gp_' + t_index + '" aria-controls="home" role="tab" data-toggle="tab">' + tables[t_index].Name + '</a></li>';
            }
            html += '</ul><div class="tab-content">';
            for (var t_index = 0; t_index < tables.length; t_index++) {
                var table = tables[t_index];
                html += '<div role="tabpanel" class="tab-pane' + (t_index == 0 ? ' active' : '') + '" id="gp_' + t_index + '">';
                html += '<table id="table_' + t_index + '" class="table table-striped table-registration-data"><thead><tr>';
                if (table.Vertical) {
                    html += '<th>Label</th><th>Count</th></tr>';
                    html += '</thead><tbody>';
                    for (var v_index = 0; v_index < table.Rows[0].Values.length; v_index++) {
                        var value = table.Rows[0].Values[v_index];
                        html += '<tr><td>' + value.HeaderValue + '</td><td>' + value.Value + '</td></tr>';
                    }
                } else {
                    for (var headerIndex = 0; headerIndex < table.Headers.length; headerIndex++) {
                        html += '<th>' + table.Headers[headerIndex].Value + '</th>';
                    }
                    html += '</tr></thead><tbody>';
                    for (var rowIndex = 0; rowIndex < table.Rows.length; rowIndex++) {
                        var t_row = table.Rows[rowIndex];
                        html += '<tr id="' + t_row.Id + '" data-rowId="' + t_row.Id + '">';
                        for (var headerIndex = 0; headerIndex < table.Headers.length; headerIndex++) {
                            t_value = findValue(t_row, table.Headers[headerIndex].Value);
                            if (t_value === null || typeof (t_value) === 'undefined') {
                                html += '<td></td>';
                            }
                            else {
                                html += '<td>' + t_value.Value + '</td>';
                            }
                        }
                        html += '</tr>';
                    }
                }
                html += '</tbody></table></div>';
            }
            html += '</div>';
            $('#tablesHolder').html(html);
            html = null;
            prettyProcessing.hidePleaseWait();
        }
        xhr.send();
        xhr = null;
    }
    function findValue(row, headerId) {
        for (var row_i = 0; row_i < row.Values.length; row_i++) {
            var value = row.Values[row_i];
            if (value.HeaderValue === headerId) {
                return value;
            }
        }
    }

}());