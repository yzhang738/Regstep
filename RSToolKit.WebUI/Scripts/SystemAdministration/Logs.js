/* Report
 * Written By:   D.J. Enzyme
 * Date Created: 20141014
 * Version:      1.4.0.0
 */

/// <reference path="../Tool/breadCrumb.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Filters.js" />
/// <reference path="../Tool/jTable.js" />
/// <reference path="../jQuery/Plugins/sortable.js" />
/// <reference path="../browserGap.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />
/// <reference path="../Tool/Sortings.js" />
/// <reference path="../Tool/restful.js" />

/* global printUrl */
/* global components */
/* global rawTable */

// restful.js Globals
/* global RESTFUL */

// Filter.js Globals
/* global generateActingInput */
/* global generateValueInput */
/* global tests */
/* global PopulateFilters */
/* global FillFilters */
/* global RunFilters */
/* global ActingOnChange */
/* global DeleteStatement */
/* global filterIndex */

var FORMREPORT_VERSION = null;

$(document).on('ready', function () {
    FORMREPORT_VERSION = '1.4.0.0';
    var table = new JTable();

    table.AfterLoad = function (p_table) {
        if (BREADCRUMB.CURRENT !== null) {
            UpdateBreadCrumb(p_table);
        } else {
            setTimeout(function () { UpdateBreadCrumb(p_table); }, 5000);
        }
        CheckForUpdates(table);
        //setInterval(function () { CheckForUpdates(table); }, 10000);
    }
    table.Load(window.location.origin + '/SystemAdministration/Logs', TABLE_options);
    $('#downloadReport').on('click', function (e) {
        var data = {
            id: table.Id,
            average: $('#averages').prop('checked'),
            filters: table.Filters,
            sortings: table.Sortings,
            graph: $('#graphs').prop('checked')
        };
        var newTable = $.extend({}, table);
        //var newTable = new JTable();
        var xhr = new XMLHttpRequest();
        xhr.open('post', '../Create/Report');
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function (event) {
            RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
            prettyProccessing.hidePleaseWait();
        };
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    window.location = window.location.origin + '/Cloud/Download/Report/' + result.Id;
                } else {
                    RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                    prettyProccessing.hidePleaseWait();
                }
            } else {
                RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                prettyProccessing.hidePleaseWait();
            }
        };
        prettyProcessing.showPleaseWait('Creating Form Report', 'Creating Report');
        newTable.FilteredRows = [];
        newTable.Rows = [];
        newTable.FilterObject = null;
        newTable.SortingObject = null;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(newTable)));
    });
    $('#printable').on('click', function (e) {
        e.preventDefault();
        table.GetPrintView();
    });
});

function CheckForUpdates(p_table) {
    /// <signature>
    /// <summary>Checks for updates.</summary>
    /// <param name="p_table" type="JTable">The jTable to check for updates.</field>
    /// </signature>
    // We need to put together a list of ids and modifcation tokens for each row;
    var modificationCheck = {
        id: p_table.Id,
        savedId: p_table.SavedId,
        options: p_table.Options,
        items: [],
        lastFullCheck: p_table.LastFullCheck.format()
    };
    for (var i = 0; i < p_table.Rows.length; i++) {
        var modificationToken = FindColumn(p_table.Rows[i], "modificationToken");
        if (modificationToken !== null) {
            modificationCheck.items.push({ id: p_table.Rows[i].Id, token: modificationToken.Value });
        }
    }
    var xhr = new XMLHttpRequest();
    xhr.open('post', window.location.origin + '/SystemAdministration/LogUpdates', true);
    xhr.onload = function (event) {
        var c_xhr = event.currentTarget;
        if (c_xhr.status === 200) {
            var result = RESTFUL.parse(c_xhr);
            if (result.action === 'expired') {
                p_table.DeleteStore(p_table);
                p_table.Load(window.location.origin + '/SystemAdministration/Logs', TABLE_options);
            } else {
                if (/rows/.test(result.action)) {
                    for (var i = 0; i < result.rows.length; i++) {
                        var row = result.rows[i];
                        ReplaceRow(p_table, row);
                        p_table.UpdateRow(p_table, row);
                    }
                }
                if (/headers/.test(result.action)) {
                    for (var j = 0; j < result.headers.length; j++) {
                        var header = result.headers[j];
                        ReplaceHeader(p_table, header);
                    }
                    p_table.GetPage();
                }
                p_table.Filter();
                if (p_table.FilteredRecords() < p_table.RecordsPerPage) {
                    p_table.GetPage();
                }
                for (var i = 0; i < result.rows.length; i++) {
                    p_table.UpdateRow(p_table, row);
                }
            }
        }
        setTimeout(function () { CheckForUpdates(p_table); }, 30000);
    };
    RESTFUL.ajaxHeader(xhr);
    RESTFUL.jsonHeader(xhr);
    xhr.send(JSON.stringify(AddJsonAntiForgeryToken(modificationCheck)));
}

function UpdateBreadCrumb(p_table) {
    if (BREADCRUMB.CURRENT === null) {
        setTimeout(function () { UpdateBreadCrumb(p_table); }, 5000);
        return;
    }
    BREADCRUMB.CURRENT.Label = p_table.Name + ' on ' + p_table.Parent;
    UpdateCrumb(BREADCRUMB.CURRENT);
}
