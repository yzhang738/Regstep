/// <reference path="../jquery-2.1.1.intellisense.js" />

var report = new ReportJson();
var oldData = { id: null, regKey: null, value: null };
var checkboxgroups = {};
var radiogroups = {};

$(document).ready(function () {
    report.UId = id;

    GetData();

    $('#pageSize').on('blur', function () {
        var input = $(this);
        if (isNaN(input.val())) {
            alert('You must provide a valid number.');
            input.val(report.PageSize);
        } else {
            report.PageSize = Math.round(input.val());
        }
        GetData();
    });

    $('#pageNumber').on('change', function () {
        report.Page = $(this).val();
        GetData();
    });

    $('#formType').on('change', function () {
        report.ReportType = $(this).val();
        GetData();
    });

    $('#saveEdit').on('click', function () {
        processing.showPleaseWait();
        var data = { registrantKey: oldData.regKey, value: $('[name="value"]').val() };
        $.ajax({
            url: '../../Cloud/EditComponent/' + oldData.id,
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success)
                    $('#' + oldData.regKey).find('td[data-id="' + oldData.id + '"]').html(result.Value);
                else
                    alert(result.Message);
                processing.hidePleaseWait();
            },
            error: function (result) {
                $('#editModal').modal('hide');
            }
        });
    });

});

function GetData() {
    processing.showPleaseWait();
    report.Registrants = [];
    $.ajax({
        url: '../../Cloud/SingleFormReportData/' + report.UId + '/' + report.Page + '/' + report.PageSize,
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
                for (var i = 0; i < report.Headers.Data.length; i++) {
                    table.header.html += '<th>' + report.Headers.Data[i].Header + '</th>';
                }
                table.header.tr.html(table.header.html);
                for (var i = 0; i < report.Data.length; i++) {
                    table.data.html += '<tr id="' + report.Data[i].Id + '"">';
                    for (var j = 0; j < report.Headers.Data.length; j++) {
                        var data = findData(report.Data[i].Data, report.Headers.Data[j].Id);
                        var editable = ' ';
                        var header = report.Headers.Data[j].Id;
                        if (header != 'status' && header != 'audience' && header != 'confirmation' && header != 'dateregistered' && header != 'rsvp' && header != 'type' && header != 'email')
                            editable += 'class="editable-item cursor-pointer"';
                        var id = '';
                        if (data !== null)
                            id = data.Id;
                        table.data.html += '<td' + editable + ' data-id="' + id +'">';
                        if (data !== null)
                            table.data.html +=  data.Value;
                        table.data.html += '</td>';
                    }
                    table.data.html += '</tr>';
                }
                table.data.tbody.html(table.data.html);
                if (table.pageNumber.input.find('option').length == report.TotalPages) {
                    table.pageNumber.input.html('');
                    for (var i = 0; i < report.TotalPages; i++) {
                        table.pageNumber.input.append('<option value="' + (i + 1) + '" ' + (report.Page == (i + 1) ? 'selected="true"' : '') + '>' + (i + 1) + '</option>');
                    }
                }
                BindEditable();
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
        if (obj[i].Id == header)
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
        oldData.id = $(this).attr('data-id');
        oldData.regKey = $(this).parent('tr').attr('id');
        oldData.value = $(this).html();
        ShowEditableItem(oldData.id, oldData.regKey);
    });
}

function ShowEditableItem(id, regKey) {
    $.ajax({
        url: '../../Cloud/EditComponent/' + id + '?registrantKey=' + regKey,
        type: "get",
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result.Success) {
                $('#editModal').find('.modal-body').html('<div class="row">' + result.Html + '</div>');
                RunBinding();
                $('#editModal').modal('show');
            } else {
                oldData.Id = null;
                oldData.regKey = null;
                alert(result.Message);
            }
        },
        error: function (result) {
            oldData.Id = null;
            oldData.regKey = null;
            alert('Server Error');
        }
    });
}