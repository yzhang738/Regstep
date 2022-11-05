var report;
var modifiedCode = '00000000-0000-0000-0000-000000000000';
var tableDrag = false;
var tableDragging = null;
var startX = -1;
var startY = -1;
var cursorOffsetX = -1;
var cursorOffsetY = -1;
var availableTables = [
    {
        Name: "Name",
        UId: '00000000-0000-0000-0000-000000000000'
    }
];
var sqlHeaderHtml = '<div id="sqlHeaderInfo" class="form-group col-sm-12"><div class="row"><label class="control-label col-sm-4 col-sm-offset-1">Form Header</label><div class="col-sm-6"><select class="form-control" id="formVariable"></select></div></div><div class="row"><label class="col-sm-4 control-label col-sm-offset-1">Report Header</label> <div class="col-sm-6"><select class="form-control" id="reportHeader"></select></div></div><div class="row"><div class="col-sm-4 col-sm-offset-5"><button id="btnDataSave" class="btn btn-default">Save</button></div></div>';

var header = null;

var headerId = -1;

var tableContext = {
    UId: "",
    Id: -1
}

var headerContext = {
    UId: "",
    Id: -1
};

$(document).ready(function () {
    // Show cloud animation for loading report.
    SaveSprite('#cloudSave');

    // Load the report.
    $.ajax({
        url: '../RSCloud/GetReport',
        type: 'post',
        data: { cid: CurrentCompany, rid: ReportId },
        dataType: 'json',
        traditional: 'true',
        success: function (json) {
            if (json.Success) {
                SavedSprite('#cloudSave');
                report = json.Report;
                $('#name').val(report.Name).trigger('change');
                var select = '<div class="form-group col-sm-12"><label class="control-label col-sm-8">Type</label><div class="col-sm-4"><select class="form-control" id="reportType"><option value="0">Data</option><option value="1">Inventory</option></select></div></div>';
                $('#reportTypeDiv').html(select);
                $('#reportType').val(json.Report.ReportType).on('change', function () {
                    SaveReportType();
                });
                var holder = $('#mainHolder > section');
                // Checking for headers
                var headerTable = '<div class="table-responsive"><table class="table table-striped table-report-headers"><thead id="reportHeaders"><tr><th></th>';
                for (var i = 0; i < report.Headers.length; i++) {
                    headerTable += '<th class="report-header report-table-header-editable report-header" data-id="' + report.Headers[i].UId + '">' + report.Headers[i].Name + '</th>';
                }
                headerTable += '</tr></thead><tbody class="report-table-body"></tbody></table></div>';
                headerTable += '<div class="report-work-area"><div class="report-work-align-left text-white">Align Tables Left</div>';
                headerTable += '<div class="report-work-preview text-white">Preview Table</div>';
                for (var i = 0; i < report.Tables.length; i++) {
                    headerTable += '<div class="report-table" data-table-uid="' + report.Tables[i].UId + '"><div class="row"><div class="table-title col-sm-12">' + report.Tables[i].Name + '</div></div></div>'
                }
                headerTable += '</div>';
                holder.html(headerTable);
                for (var i = 0; i < report.Tables.length; i++) {
                    var html = '<tr data-table-sql-uid="' + report.Tables[i].TableUId + '" data-table-uid="' + report.Tables[i].UId + '"><td class="report-form">' + report.Tables[i].Name + '<input type="hidden" id="filtersData" /><span class="psuedo-link table-filter text-rsred"><span class="glyphicon glyphicon-filter"></span></span></td>';
                    for (var j = 0; j < report.Headers.length; j++) {
                        var found = false;
                        for (var k = 0; k < report.Tables[i].Headers.length; k++) {
                            if (report.Tables[i].Headers[k].ReportHeaderUId == report.Headers[j].UId) {
                                html += '<td class="report-table-header-editable table-header" data-report-header-uid="' + report.Headers[j].UId + '" data-table-header-uid="' + report.Tables[i].Headers[k].UId + '">' + report.Tables[i].Headers[k].Name + '</td>';
                                found = true;
                                break;
                            }
                        }
                        if (!found) {
                            html += '<td data-report-header-uid="' + report.Headers[j].UId + '" data-table-header-uid="" class="report-table-header-editable table-header table-header-empty"></td>';
                        }
                    }
                    html += '</tr>';
                    $('.report-table-body').append(html);
                }
                $('.report-work-align-left').on('click', function () {
                    OrderTablesLeft();
                });
                $('.report-work-preview').on('click', function () {
                    PreviewTable();
                });
                OrderTablesLeft();
                GetAvailableTables();
                StartDrag();
                ClickHeaders();
            } else {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });

    // This sets a timeout to be used for checking if the report was modified by another user
    setTimeout(function () {
        GetModificationToken();
    }, 20000);

    // Link for adding a new header
    $('#newHeader').on('click', function () {
        var pos = report.Headers.length + 1;
        header = {
            Id: -1,
            UId: '00000000-0000-0000-0000-000000000000',
            Company: CurrentCompany,
            Name: "NewHeader",
            DateCreated: null,
            DateModified: null,
            Owner: '00000000-0000-0000-0000-000000000000',
            Group: '00000000-0000-0000-0000-000000000000',
            Permission: '755',
            Type: "ReportHeader",
            ReportUId: ReportId,
            Styling: null,
            Position: pos
        };
        $('#newHeaderModal').modal('show');
    });

    //Link for saving new header.
    $('#saveNewHeader').on('click', function () {
        SaveNewHeader();
    });

    //This sets up the save event for the name of the report
    $('#name').on('toggle-edit-change', function () {
        var name = $(this).val();
        SaveReportName(name);
    });

    // This binds the new table link to the modal
    $('#newTable').on('click', function () {
        $('#newTableModal').modal('show');
    });

    // THis binds the save new table button in the modal
    $('#saveNewTable').on('click', function () {
        SaveNewTable();
    });

    $(document).on('click contextmenu', function () {
        $('.context-menu').hide();
    })

    $('#removeHeader').on('click', function () {
        SaveSprite('#cloudSave');
        $.ajax({
            url: '../RSCloud/RemoveReportHeader',
            type: 'post',
            data: { cid: CurrentCompany, rid: ReportId, rhid: headerContext.UId },
            dataType: 'json',
            traditional: 'true',
            success: function (json) {
                SavedSprite('#cloudSave');
                if (json.Success) {
                    for (var i = 0; i < report.Headers.length; i++) {
                        if (report.Headers[i].UId == json.Header.UId) {
                            report.Headers.splice(i, 1);
                            break;
                        }
                    }
                    for (var i = 0; i < report.Tables.length; i++) {
                        for (var j = 0; j < report.Tables[i].Headers.length; j++) {
                            if (report.Tables[i].Headers[j].ReportHeaderUId == json.Header.UId) {
                                report.Tables[i].Headers.splice(j, 1);
                                break;
                            }
                        }
                    }
                    $('[data-id="' + json.Header.UId + '"], [data-report-header-uid="' + json.Header.UId + '"').remove();
                    GetAvailableTables();
                    StartDrag();
                    ClickHeaders();
                } else {
                    SaveError('#cloudSave');
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                    $('#informationModal').modal('show');
                }
            },
            error: function (result) {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
                $('#informationModal').modal('show');
            }
        });
    });

    $('#removeForm').on('click', function () {
        SaveSprite('#cloudSave');
        $.ajax({
            url: '../RSCloud/RemoveReportTable',
            type: 'post',
            data: { cid: CurrentCompany, rid: ReportId, rtid: tableContext.UId },
            dataType: 'json',
            traditional: 'true',
            success: function (json) {
                if (json.Success) {
                    SavedSprite('#cloudSave');
                    for (var i = 0; i < report.Tables.length; i++) {
                        if (report.Tables[i].UId == json.TableUId) {
                            report.Tables.splice(i, 1);
                            break;
                        }
                    }
                    // Remove the tr of the table
                    var tr = $('tr[data-table-uid="' + json.TableUId + '"]');
                    tr
                        .children('td, th')
                        .animate({ padding: 0 })
                        .wrapInner('<div />')
                        .children()
                        .slideUp(function () { tr.remove(); });
                    $('.report-table[data-table-uid="' + json.TableUId + '"]').remove();
                    GetAvailableTables();
                    StartDrag();
                    ClickHeaders();
                } else {
                    SaveError('#cloudSave');
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                    $('#informationModal').modal('show');
                }
            },
            error: function (result) {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
                $('#informationModal').modal('show');
            }
        });
    });

    $('#removeTableHeader').on('click', function () {
        var td = $('td[data-table-header-uid="' + headerContext.UId + '"]');
        if (td.hasClass('table-header-empty')) {
            return;
        }
        SaveSprite();
        $.ajax({
            url: '../RSCloud/RemoveTableHeader',
            type: 'post',
            data: JSON.stringify({ company: CurrentCompany, report: ReportId, table: tableContext.UId, header: headerContext.UId }),
            dataType: 'json',
            traditional: 'true',
            contentType: 'application/json; charset=utf-8',
            success: function (json) {
                SavedSprite();
                if (json.Success) {
                    // Remove the table from the array
                    for (var i = 0; i < report.Tables.length; i++) {
                        if (report.Tables[i].UId == json.TableUId) {
                            for (var j = 0; j < report.Tables[i].Headers.length; j++) {
                                if (report.Tables[i].Headers[j].UId == json.HeaderUId) {
                                    // We found the header removed, now we remove it from the array
                                    report.Tables[i].Headers.splice(j, 1);
                                    break;
                                }
                            }
                        }
                    }
                    SavedSprite('#cloudSave');
                    // Remove the td data of the table
                    td.attr('data-table-header-uid', '');
                    td.wrapInner('<div></div>');
                    td.children('div').slideUp(function () {
                        td.html('').addClass('table-header-empty');
                        td.find('div').remove();
                    });
                } else {
                    SaveError('#cloudSave');
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                    $('#informationModal').modal('show');
                }
            },
            error: function (result) {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
                $('#informationModal').modal('show');
            }
        });
    });

    $('#previewReport').on('click', function () {
        window.open('ViewReport?cid=' + CurrentCompany + '&rid=' + ReportId);
    });

});

// This saves a new header into the report.
function SaveNewHeader() {
    SavedSprite('#cloudSave');
    header.Name = $('#newHeaderLabel').val();
    $('#name').val('');
    $.ajax({
        url: '../RSCloud/AddHeader',
        type: 'post',
        data: header,
        dataType: 'json',
        traditional: 'true',
        success: function (json) {
            if (json.Success) {
                SavedSprite('#cloudSave');
                report.ModificationToken = json.ModificationToken;
                $('#reportHeaders > tr').append('<th class="report-header report-table-header-editable report-header" data-id="' + header.UId + '">' + header.Name + '</th>');
                $('.report-table-body > tr').each(function (ind) {
                    $(this).append('<td class="report-table-header-editable table-header-empty" data-report-header-uid="' + json.Header.UId + '"></td>');
                });
                report.Headers.push(json.Header);
                ClickHeaders();
            } else {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });

}

// Saves the name of the report
function SaveReportName(name) {
    if (report.Name == name) return;
    SaveSprite('#cloudSave');
    $.ajax({
        url: '../RSCloud/UpdateReportName',
        type: 'post',
        data: { Company: CurrentCompany, UId: ReportId, Name: name },
        dataType: 'json',
        traditional: 'true',
        success: function (json) {
            if (json.Success) {
                SavedSprite('#cloudSave');
                report.ModificationToken = json.ModificationToken;
                report.Name = $('#name').val();
                $('#name').trigger('change');
            } else {
                SaveError('#cloudSave');
                $(this).val(report.Name);
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });

}

// Saves a new table into the report
function SaveNewTable() {
    SaveSprite('#cloudSave');
    var tableId = $('#tables').val();
    $('#name').val('');
    $.ajax({
        url: '../RSCloud/AddTable',
        type: 'post',
        data: { tableUId: tableId, cid: CurrentCompany, rid: ReportId },
        dataType: 'json',
        traditional: 'true',
        success: function (json) {
            if (json.Success) {
                SavedSprite('#cloudSave');
                report.ModificationToken = json.ModificationToken;
                report.Tables.push(json.Table);
                $('#mainHolder .report-work-area').append('<div class="report-table" data-table-uid="' + json.Table.UId + '">' + json.Table.Name + '</div>');
                var tableHtml = '<tr data-table-sql-uid="' + json.Table.TableUId + '" data-table-uid="' + json.Table.UId + '"><td class="report-form">' + json.Table.Name + '<input type="hidden" id="filtersData" /><span class="psuedo-link table-filter text-rsred"><span class="glyphicon glyphicon-filter"></span></span></td>';
                for (var i = 0; i < report.Headers.length; i++) {
                    tableHtml += '<td data-report-header-uid="' + report.Headers[i].UId + '" data-table-header-uid="" class="report-table-header-editable table-header table-header-empty"></td>';
                }
                tableHtml += '</tr>';
                $('.report-table-body').append(tableHtml);
                OrderTablesLeft();
                ClickHeaders();
                GetAvailableTables();
            } else {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });

}

// This shows the cloud processing modal
function SaveSprite(modal) {
    var mod = $(modal);
    $(mod).modal('show');
    mod.find('.saved-sprite').removeClass('saved-sprite');
}

// This shows the cloud success modal
function SavedSprite(modal) {
    var mod = $(modal);
    mod.find('.save-sprite').addClass('saved-sprite');
    setTimeout(function () {
        mod.modal('hide');
        setTimeout(function () {
            mod.find('.saved-sprite').removeClass('saved-sprite');
        }, 5000);
    }, 800);
}

// This shows the cloud error modal
function SaveError(modal) {
    var mod = $(modal);
    mod.find('.save-sprite').addClass('error-sprite');
    setTimeout(function () {
        mod.modal('hide');
        setTimeout(function () {
            mod.find('.error-sprite').removeClass('error-sprite');
        }, 5000);
    }, 2000);
}

// This enables dragging of the windows.
function StartDrag() {
    $('.report-table, .report-work-area').off();
    $('.report-table').on('mousedown', function (event) {
        tableDrag = true;
        startX = event.pageX - $('.report-work-area').offset().left
        startY = event.pageY - $('.report-work-area').offset().top
        cursorOffsetX = event.pageX - $(this).offset().left;
        cursorOffsetY = event.pageY - $(this).offset().top;
        tableDragging = this;
    });
    $('.report-work-area').on('mousemove', function (event) {
        if (tableDrag) {
            var fX = event.pageX - $('.report-work-area').offset().left;
            var fY = event.pageY - $('.report-work-area').offset().top;
            var x = event.pageX - $('.report-work-area').offset().left - cursorOffsetX;
            var y = event.pageY - $('.report-work-area').offset().top - cursorOffsetY;
            var distance = Math.abs(Math.sqrt(Math.pow(startX - fX, 2) + Math.pow(startY - fY, 2)));
            if (distance > 20) {
                $(tableDragging).css('top', y).css('left', x);
            }
        }
    });
    $('.report-work-area').on('mouseup', function () {
        tableDrag = false;
        tableDragging = null;
    });
}

// This grabs the current modification token.
function GetModificationToken() {
    $.ajax({
        url: '../RSCloud/GetModificationToken',
        type: 'post',
        data: { cid: CurrentCompany, id: ReportId },
        dataType: 'json',
        traditional: 'true',
        success: function (json) {
            if (json.ModificationToken != report.ModificationToken) {
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">The report was edited by someone else.</div></div>');
                $('#informationModal').modal('show');
            }
        }
    });
    setTimeout(function () {
        GetModificationToken();
    }, 20000);
}

// This grabs the available tables for insertion into the report
function GetAvailableTables() {
    $.ajax({
        url: '../RSCloud/GetReportTablesAvailable',
        type: 'post',
        data: { cid: CurrentCompany },
        dataType: 'json',
        traditional: 'true',
        success: function (json) {
            if (json.Success) {
                availableTables = json.Tables;
                SetTables();
            } else {
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });
}

// This sets the add table modal available tables.
function SetTables() {
    $('#tables').html('');
    if (report.ReportType == 0) {
        for (var i = 0; i < availableTables.length; i++) {
            for (var j = 0; j < report.Tables.length; j++) {
                if (availableTables[i].UId == report.Tables[j].TableUId) {
                    availableTables.splice(i, 1);
                    i--;
                    break;
                }
            }
        }
    }
    for (var i = 0; i < availableTables.length; i++) {
        $('#tables').append('<option value="' + availableTables[i].UId + '">' + availableTables[i].Name + '</option>');
    }
}

// This orders all tables to the left
function OrderTablesLeft() {
    $('#sqlHeaderInfo').remove();
    $('.report-table').each(function (i) {
        var x = (20 * i) + 20;
        $(this).animate({
            top: x + 'px',
            left: '20px',
            width: 'auto'
        });
        $(this).css('z-index', '');
    });
}

// This sets up the clickable headers and positions in the report information table
function ClickHeaders() {
    $('.table-header').off();
    $('.report-header').off();
    $('.report-form').off();
    $('.table-filter').off();

    $('.table-filter').on('click', function () {
        var uid = $(this).parents('tr').attr('data-table-uid');
        var sqlUId = $(this).parents('tr').attr('data-table-sql-uid');
        filterTable = sqlUId;
        var tableIndex = -1;

        for (var i = 0; report.Tables.length; i++) {
            if (report.Tables[i].UId == uid) {
                tableIndex = i;
                break;
            }
        }

        $.ajax({
            url: '../FormBuilder/GetFormVariables',
            type: 'post',
            data: { cids: CurrentCompany, fids: sqlUId },
            dataType: 'json',
            traditional: 'true',
            success: function (json) {
                filters = [];
                json.id.push('Confirmation');
                json.variable.push('Confirmation');
                json.id.push('Audience');
                json.variable.push('Audience');
                json.id.push('Status');
                json.variable.push('Status');
                json.id.push('Type');
                json.variable.push('Type');
                for (var i = 0; i < json.id.length; i++) {
                    if (json.variable[i].toLowerCase() == 'email') {
                        json.id[i] = "Email";
                    }
                    filters.push({ UId: json.id[i], Name: json.variable[i] });
                }
                while (currentFilters.length > 0) {
                    currentFilters.pop();
                }
                for (var i = 0; i < report.Tables[tableIndex].Filters.length; i++) {
                    currentFilters.push(report.Tables[tableIndex].Filters[i]);
                }
                SetUpFilters(report.Tables[tableIndex].TableUId);
                $('#filterModal').modal('show');
                $('#filterNow').off().on('click', function () {
                    report.Tables[tableIndex].Filters = [];
                    for (var i = 0; currentFilters.length > i; i++) {
                        report.Tables[tableIndex].Filters.push(currentFilters[i]);
                    }
                    SaveTableFilter(uid);
                });
            },
            error: function (result) {
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
                $('#informationModal').modal('show');
            }
        });
    });

    $('.report-form').on('contextmenu', function (event) {
        $('.context-menu').hide();
        tableContext.UId = $(this).parent().attr('data-table-uid');
        var topSpot = event.pageY;
        var leftSpot = event.pageX;
        $('#contextMenuForm').css({ top: topSpot, left: leftSpot });
        $('#contextMenuForm').show();
        event.preventDefault();
        return false;
    });
    
    $('.report-header').on('contextmenu', function (event) {
        $('.context-menu').hide();
        headerContext.UId = $(this).attr('data-id');
        var topSpot = event.pageY;
        var leftSpot = event.pageX;
        $('#contextMenuHeader').css({ top: topSpot, left: leftSpot });
        $('#contextMenuHeader').show();
        event.preventDefault();
        return false;
    });

    $('.table-header').on('contextmenu', function (event) {
        $('.context-menu').hide();
        headerContext.UId = $(this).attr('data-table-header-uid');
        tableContext.UId = $(this).parent().attr('data-table-uid');
        var topSpot = event.pageY;
        var leftSpot = event.pageX;
        $('#contextMenuTableHeader').css({ top: topSpot, left: leftSpot });
        $('#contextMenuTableHeader').show();
        event.preventDefault();
        return false;
    });

    $('.table-header').on('click', function () {
        $('#sqlHeaderInfo').remove();
        var cell = $(this);
        var tableUId = cell.parent().attr('data-table-uid');
        var tableSqlUId = cell.parent().attr('data-table-sql-uid');
        var headerUId = cell.attr('data-report-header-uid');
        var tableDiv = $('.report-table[data-table-uid="' + tableUId + '"]');
        var tableIndex = -1;
        var headerIndex = -1;
        var newHeader = cell.hasClass('table-header-empty');
        var headerNoSpaceLower = "";
        if (tableDiv == null) return;
        var sqlHeader = {
            Id: -1,
            SqlTableUId: tableUId,
            ReportHeaderUId: headerUId,
            Filters: [],
            SqlHeaderUId: tableSqlUId,
            UId: "00000000-0000-0000-0000-000000000000",
            Company: CurrentCompany,
            Name: "New Header",
            DateCreated: null,
            DateModified: null,
            Owner: "00000000-0000-0000-0000-000000000000",
            Group: "00000000-0000-0000-0000-000000000000",
            Permission: "755",
            Type: "SqlHeader",
            ModificationToken: "00000000-0000-0000-0000-000000000000",
            ModifiedBy: "00000000-0000-0000-0000-000000000000"
        }
        // Grab the table index and the header index
        for (var i = 0; i < report.Tables.length; i++) {
            if (report.Tables[i].UId == tableUId) {
                // We found the table and are going to save the index location
                tableIndex = i;
                for (var j = 0; j < report.Tables[i].Headers.length; j++) {
                    if (report.Tables[i].Headers[j].ReportHeaderUId == headerUId) {
                        // Lets save the 
                        sqlHeader = report.Tables[i].Headers[j];
                        // We found the header and are going to store the indices
                        headerIndex = j;
                        break;
                    }
                }
                break;
            }
        }
        for (var i = 0; i < report.Headers.length; i++) {
            if (report.Headers[i].UId == sqlHeader.ReportHeaderUId) {
                headerNoSpaceLower = report.Headers[i].Name.replace(' ', '').toLowerCase();
            }
        }
        OrderTablesLeft();
        tableDiv.stop();
        tableDiv.animate({
            width: "800px",
            left: "200px",
            top: "30px",
        }, 500);
        tableDiv.css('z-index', '30');
        tableDiv.append(sqlHeaderHtml);
        $.ajax({
            url: '../FormBuilder/GetFormVariables',
            type: 'post',
            data: { cids: CurrentCompany, fids: tableSqlUId },
            dataType: 'json',
            traditional: 'true',
            success: function (json) {
                filters = [];
                json.id.push('Confirmation');
                json.variable.push('Confirmation');
                json.id.push('Audience');
                json.variable.push('Audience');
                json.id.push('Status');
                json.variable.push('Status');
                json.id.push('Type');
                json.variable.push('Type');
                var selectedVal = "";
                for (var i = 0; i < json.id.length; i++) {
                    if (newHeader) {
                        // If this is a new header, we will see if any of the values match the header value closely and autoselect it.
                        if (headerNoSpaceLower == json.variable[i].toLowerCase()) {
                            selectedVal = json.id[i];
                        }
                    }
                    $('#formVariable').append('<option value="' + json.id[i] + '" data-name="' + json.variable[i] + '">' + json.variable[i] + '</option>');
                    if (json.variable[i].toLowerCase() == 'email') {
                        json.id[i] = "Email";
                    }
                    filters.push({ UId: json.id[i], Name: json.variable[i] });
                }
                if (newHeader) {
                    $('#formVariable').val(selectedVal);
                } else {
                    $('#formVariable').val(sqlHeader.SqlHeaderUId);
                }
            },
            error: function (result) {
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
                $('#informationModal').modal('show');
            }
        });
        // We need to populate the report headers select box with headers that are not used and the currently selected header
        for (var i = 0; i < report.Headers.length; i++) {
            var used = false;
            for (var j = 0; j < report.Tables[tableIndex].Headers.length; j++) {
                if (report.Tables[tableIndex].Headers[j].ReportHeaderUId == report.Headers[i].UId) {
                    // The report header is currently in use
                    used = true;
                    break;
                }
            }
            if (!used || report.Tables[tableIndex].Headers[j].ReportHeaderUId == headerUId) {
                // The report header was not in use or the header is the currently selected one
                $('#reportHeader').append('<option value="' + report.Headers[i].UId + '" data-name="' + report.Headers[i].Name + '">' + report.Headers[i].Name + '</option>')
            }
        }
        $('#reportHeader').val(headerUId);
        $('#btnDataSave').on('click', function () {
            SaveHeader(sqlHeader, tableIndex, headerIndex, newHeader);
        });
    });
}

// This saves the header
function SaveHeader(sqlHeader, tableIndex, headerIndex, newHeader) {
    SaveSprite('#cloudSave');
    var action = 'UpdateSqlHeader';
    if (newHeader) {
        action = 'AddSqlHeader';
    }
    sqlHeader.SqlHeaderUId = $('#formVariable').val();
    sqlHeader.ReportHeaderUId = $('#reportHeader').val();
    sqlHeader.Name = $('#formVariable option[value="' + sqlHeader.SqlHeaderUId + '"]').text();
    $.ajax({
        url: '../RSCloud/' + action,
        type: 'post',
        data: JSON.stringify(sqlHeader),
        dataType: 'json',
        traditional: 'true',
        contentType: 'application/json; charset=utf-8',
        success: function (json) {
            if (json.Success) {
                report.ModificationToken = json.ModificationToken;
                modifiedCode = json.ModificationToken;
                SavedSprite('#cloudSave');
                if (tableIndex == -1) return;
                if (headerIndex == -1) {
                    report.Tables[tableIndex].Headers.push(json.SqlHeader);
                } else {
                    report.Tables[tableIndex].Headers[headerIndex] = json.SqlHeader;
                }
                $('tr[data-table-uid="' + json.SqlHeader.SqlTableUId + '"]').find('td[data-report-header-uid="' + json.SqlHeader.ReportHeaderUId + '"]').html(json.SqlHeader.Name);
                if (newHeader) {
                    $('tr[data-table-uid="' + json.SqlHeader.SqlTableUId + '"]').find('td[data-report-header-uid="' + json.SqlHeader.ReportHeaderUId + '"]').removeClass('table-header-empty').attr('data-table-header-uid', json.SqlHeader.UId);
                }
            } else {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });
}

// This saves the filter of the specified table.
function SaveTableFilter(uid) {
    var indexToSave = -1;
    // Find the index of the table to update
    for (var i = 0; i < report.Tables.length; i++) {
        if (report.Tables[i].UId == uid) {
            indexToSave = i;
            break;
        }
    }
    if (indexToSave == -1) {
        return;
    }
    SaveSprite('#cloudSave');
    $.ajax({
        url: '../RSCloud/UpdateSqlTableFilters',
        type: 'post',
        data: JSON.stringify({ Company: CurrentCompany, SqlTable: uid, Filters: report.Tables[indexToSave].Filters}),
        dataType: 'json',
        traditional: 'true',
        contentType: 'application/json; charset=utf-8',
        success: function (json) {
            if (json.Success) {
                SavedSprite('#cloudSave');
            } else {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });
}

// Gets a table preview
function PreviewTable() {
    SaveSprite('#cloudSave');
    $.ajax({
        url: '../RSCloud/GetReportData',
        type: 'post',
        data: JSON.stringify({ company: CurrentCompany, report: ReportId }),
        dataType: 'json',
        traditional: 'true',
        contentType: 'application/json; charset=utf-8',
        success: function (json) {
            if (json.Success) {
                $('#reportPreview').remove();
                SavedSprite('#cloudSave');
                OrderTablesLeft();
                var html = '<div class="table-responsive table-report-view" id="reportPreview" style="z-index: 61;"><span class="remove-preview psuedo-link text-rsred" onclick="$(this).parent().remove();">&times;</span><table class="table table-striped table-report-view"><thead id="reportHeaders"><tr>';
                for (var i = 0; i < json.Data.Headers.length; i++) {
                    html += '<th>' + json.Data.Headers[i] + '</th>';
                }
                html += '</thead><tbody>';
                for (var i = 0; i < json.Data.Rows.length; i++) {
                    html += '<tr>';
                    for (var j = 0; j < json.Data.Rows[i].length; j++) {
                        html += '<td>' + json.Data.Rows[i][j] + '</td>';
                    }
                    html += '</tr>';
                }
                html += '</tbody></table></div>';
                $('.report-work-area').append(html);
            } else {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });
}

// Saves the report type;
function SaveReportType() {
    SaveSprite('#cloudSave');
    var reportType = parseInt($('#reportType').val());
    $.ajax({
        url: '../RSCloud/UpdateReportType',
        type: 'post',
        data: JSON.stringify({ company: CurrentCompany, report: ReportId, reportType: reportType }),
        dataType: 'json',
        traditional: 'true',
        contentType: 'application/json; charset=utf-8',
        success: function (json) {
            if (!json.Success) {
                SaveError('#cloudSave');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + json.Message + '</div></div>');
                $('#informationModal').modal('show');
            } else {
                SavedSprite('#cloudSave');
                report.ModificationToken = json.ModificationToken;
                modifiedCode = json.ModificationToken;
                report.ReportType = reportType;
                GetAvailableTables();
            }
        },
        error: function (result) {
            SaveError('#cloudSave');
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });
}