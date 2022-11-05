/// <reference path="../jquery-2.1.1.intellisense.js" />

var report = new JsonTable();
var oldData = { id: null, regKey: null, value: null };
var checkboxgroups = {};
var radiogroups = {};

$(document).ready(function () {
    report.Id = id;

    GetData();

    $('#pageSize').on('blur', function () {
        var input = $(this);
        if (isNaN(input.val())) {
            alert('You must provide a valid number.');
            input.val(report.Paging.RecordsPerPage);
        } else {
            report.Paging.RecordsPerPage = Math.round(input.val());
        }
        GetData();
    });

    $('.table-sort').on('click', function (e) {
        e.preventDefault();
        var link = $(this);
        var actingon = link.attr('data-actingon');
        if (report.Sortings.ActingOn == actingon) {
            report.Sortings.Descending = !report.Sortings.Descending;
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes-alt');
        } else {
            $('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt').removeClass('glyphicon-sort-by-attributes');
            report.Sortings.ActingOn = actingon;
            report.Sortings.Descending = false;
            link.children('.sort-icon').addClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt');
        }
        GetData();
    });

    $('#pageNumber').on('change', function () {
        report.Paging.Page = $(this).val();
        GetData();
    });

    $('#pageRight').on('click', function () {
        report.Paging.Page++;
        if ($(this).hasClass('disabled')) {
            return;
        }
        $('#pageNumber').val(report.Page);
        GetData();
    });

    $('#pageLeft').on('click', function () {
        report.Paging.Page--;
        if ($(this).hasClass('disabled')) {
            return;
        }
        $('#pageNumber').val(report.Page);
        GetData();
    });



    $('#formType').on('change', function () {
        //report.ReportType = $(this).val();
        GetData();
    });

    $('#saveEdit').on('click', function () {
        // Here we make the ajax call to save the data
        $('#editModal').find('#editingData').hide('fast');
        $('#editModal').find('#editingProgress').show('fast');
        // Get the data to send.
        var waitlisting = [];
        var w_index = 0;
        while ($('input[name^="Waitlistings[' + w_index + ']"]').length > 0) {
            var json = {};
            var items = $('input[name^="Waitlistings[' + w_index + ']"]');
            for (var i = 0; i < items.length; i++) {
                var item = $(items[i]);
                if (/Key$/i.test(item.attr('name'))) {
                    json.Key = item.val();
                } else if (item.attr('type') === 'checkbox') {
                    if (item.prop('checked')) {
                        json.Value = true;
                    } else {
                        json.Value = false;
                    }
                }
            }
            w_index++;
            waitlisting.push(json);
        }
        var data = { ComponentKey: oldData.id, RegistrantKey: oldData.regKey, Value: $('#' + oldData.id).val(), Waitlistings: waitlisting };
        // Add the antiforgery token.
        AddJsonAntiForgeryToken(data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', '../../Cloud/SaveRegistrantData', true);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        t_xhr.onload = function (event) {
            c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#' + oldData.regKey).find('td[data-id="' + oldData.id + '"]').html(result.NewValue);
                    $('#editModal').modal('hide');
                } else {
                    $('#editModal .modal-body').find('#editingProgress').hide('fast');
                    $('#editModal .modal-body').find('#editingData').show('fast');
                    var message = '';
                    for (var i = 0; i < result.Errors.length; i++) {
                        if (i != 0) {
                            message += '<br />';
                        }
                        message += result.Errors[i];
                    }
                    $('#editModal').find('.form-messagebox').html(message).show('fast');
                }
            } else {
                $('#editModal').modal('hide');
                RESTFUL.showError('Unhandled Server Error');
            }
        };
        t_xhr.send(JSON.stringify(data));
    });

});

function GetData() {
    processing.showPleaseWait();
    report.Registrants = [];
    $.ajax({
        url: '../../Cloud/SingleFormReportData/' + report.Id + '/' + report.Paging.Page + '/' + report.Paging.RecordsPerPage,
        type: "get",
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result.Success) {
                $.extend(report, result.Data);
                $('#registrantData').html('');
                var table = {};
                table.header = {};
                table.data = {};
                table.pageNumber = {};
                table.header.html = '';
                table.header.tr = $('#headers tr');
                table.data.tbody = $('#registrantData');
                table.data.html = '';
                table.pageNumber.input = $('#pageNumer');
                for (var i = 0; i < report.Headers.length; i++) {
                    table.header.html += '<th>' + report.Headers[i].Label + '</th>';
                }
                table.header.tr.html(table.header.html);
                for (var i = 0; i < report.Rows.length; i++) {
                    table.data.html += '<tr id="' + report.Rows[i].Id + '"">';
                    for (var j = 0; j < report.Headers.length; j++) {
                        var data = findData(report.Rows[i].Columns, report.Headers[j].Id);
                        var editable = ' ';
                        var header = report.Headers[j].Id;
                        if (header != 'status' && header != 'audience' && header != 'confirmation' && header != 'dateregistered' && header != 'rsvp' && header != 'type' && header != 'email' && header != "")
                            editable += 'class="editable-item cursor-pointer"';
                        var id = '';
                        if (data !== null)
                            id = data.Id;
                        table.data.html += '<td' + editable + ' data-id="' + id +'">';
                        if (data !== null) {
                            if (data.PRettyValue !== null) {
                                table.data.html +=  data.PrettyValue;
                            } else {
                                table.data.html += '<i>No Value</i>';
                            }
                        }
                        table.data.html += '</td>';
                    }
                    table.data.html += '</tr>';
                }
                table.data.tbody.html(table.data.html);
                $('#pageNumber').html('');
                for (var i = 0; i < report.Paging.TotalPages; i++) {
                    $('#pageNumber').append('<option value="' + (i + 1) + '"' + ((i + 1) == report.Paging.Page ? ' selected="true"' : '') + '>' + (i + 1) + '</option>');
                }
                if (report.Paging.Page == report.Paging.TotalPages) {
                    $('#pageRight').addClass('disabled');
                } else {
                    $('#pageRight').removeClass('disabled');
                }
                if (report.Paging.Page == 1) {
                    $('#pageLeft').addClass('disabled');
                } else {
                    $('#pageLeft').removeClass('disabled');
                }
                BindEditable();
                $('#totalRecords').html(report.Paging.TotalRecords);
            } else {
                alert(result.Message);
            }
            processing.hidePleaseWait();
        },
        error: function (result) {
            alert('Server Error');
            processing.hidePleaseWait();
        }
    });
}

function JsonTable() {
    this.Headers = [];
    this.Rows = [];
    this.Name = '';
    this.Id = '';
    this.Paging = {
        Page: 1,
        RecordsPerPage: 25
    };
}

function ReportJson() {
    this.UId = "";
    this.Page = 1;
    this.PageSize = 25;
    this.Success = true;
    this.TotalPages = 1;
    this.Headers = new DataObject();
    this.Data = [];
}

function DataObject() {
    this.Data = [];
    this.Id = "";
}

function DataSet() {
    this.Header = "";
    this.Value = "";
    this.Id = "";
}

function findData(obj, header) {
    for (var i = 0; i < obj.length; i++) {
        if (obj[i].HeaderId == header)
            return obj[i];
    }
    return null;
}

function RunBinding() {
    $('input[data-component-type="datetime"]').datetimepicker({
        autoclose: true
    });

    $('input[type="hidden"][data-component-type="checkboxgroup"]').each(function () {
        checkboxgroups[$(this).attr('id')] = { parent: $(this), timeExclusion: $(this).parent().attr('data-component-timeexclusion') == "True", Items: [] };
        $(this).parent().find('input[type="checkbox"]').each(function () {
            var parentId = $(this).attr('data-parent');
            checkboxgroups[parentId].Items.push($(this));
            if (checkboxgroups[parentId].timeExclusion) {
                $(this).on('change', function () {
                    var parentId = $(this).attr('data-parent');
                    var json = [];
                    var history = [];
                    var checked = [];
                    var collision = false;
                    for (var i = 0; i < checkboxgroups[parentId].Items.length; i++) {
                        if (checkboxgroups[parentId].Items[i].prop('checked')) {
                            if (checkboxgroups[parentId].Items[i].val() != $(this).val()) {
                                history.push($(this));
                            }
                            json.push(checkboxgroups[parentId].Items[i].val());
                            checked.push(checkboxgroups[parentId].Items[i]);
                        }
                    }
                    for (var i = 0; i < checked.length; i++) {
                        var aStart = moment(checked[i].attr('data-agenda-start'));
                        var aEnd = moment(checked[i].attr('data-agenda-end'));
                        for (var j = 0; j < checked.length; j++) {
                            if (checked[i] == checked[j])
                                continue;
                            var bStart = moment(checked[j].attr('data-agenda-start'));
                            var bEnd = moment(checked[j].attr('data-agenda-end'));
                            if (bEnd.isAfter(aStart) && bEnd.isBefore(aEnd))
                                collision = true;
                        }
                    }
                    if (collision) {
                        // we need to roll back to what we had before
                        for (var i = 0; i < checkboxgroups[parentId].Items.length; i++) {
                            checkboxgroups[parentId].Items[i].prop('checked', false);
                        }
                        for (var i = 0; i < history.length; i++) {
                            history[i].prop('checked', true);
                        }
                        alert("Your selection has time conflictions");
                    } else {
                        checkboxgroups[parentId].parent.val(JSON.stringify(json));
                    }
                });
            }
            else {
                $(this).on('change', function () {
                    var parentId = $(this).attr('data-parent');
                    var checked = [];
                    for (var i = 0; i < checkboxgroups[parentId].Items.length; i++) {
                        if (checkboxgroups[parentId].Items[i].prop('checked'))
                            checked.push(checkboxgroups[parentId].Items[i].val());
                    }
                    checkboxgroups[parentId].parent.val(JSON.stringify(checked));
                })
            }
        });
    });

    $('input[type="hidden"][data-component-type="radiogroup"]').each(function () {
        radiogroups[$(this).attr('id')] = { parent: $(this), Items: [] };
        $(this).parent().find('input[type="radio"]').each(function () {
            var parentId = $(this).attr('data-parent');
            radiogroups[parentId].Items.push($(this));
            $(this).on('change', function () {
                var parentId = $(this).attr('data-parent');
                radiogroups[parentId].parent.val($(this).val());
            })
        });
    });

    $('.uploaded-image').each(function (i) {
        $.ajax({
            type: "Get",
            url: '../../Cloud/RegistrantImageThumbnail/' + $(this).attr('data-form-registrant') + '?component=' + $(this).attr('data-form-component'),
            success: function (data) {
                $(this).attr('src', data);
            },
        });
    });
}

function BindEditable() {
    $('.editable-item').on('click', function () {
        $('#editModal .modal-body').find('#editingProgress').show();
        $('#editModal .modal-body').find('#editingData').hide();
        $('#editModal').modal('show');
        oldData.id = $(this).attr('data-id');
        oldData.regKey = $(this).parent('tr').attr('id');
        oldData.value = $(this).html();
        if ($(this).hasClass('file'))
            setTimeout(function () { ShowEditableItem(oldData.id, oldData.regKey); }, 1000);
        else
        ShowEditableItem(oldData.id, oldData.regKey);
    });
}

function ShowEditableItem(id, regKey) {
        var width = $('#editModal .modal-dialog').width() - 2 - 30 - 30;
    $.ajax({
            url: '../../Cloud/EditComponent/' + id + '?registrantKey=' + regKey + '&width=' + width,
        type: "get",
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result.Success) {
                $('#editModal').find('#editingData').html(result.Html).show('fast');
                $('#editModal').find('#editingProgress').hide('fast');
                RunBinding();
            } else {
                $('#editModal').modal('hide');
                oldData.Id = null;
                oldData.regKey = null;
                alert(result.Message);
            }
        },
        error: function (result) {
                processing.hidePleaseWait();
            oldData.Id = null;
            oldData.regKey = null;
            alert('Server Error');
        }
    });
}