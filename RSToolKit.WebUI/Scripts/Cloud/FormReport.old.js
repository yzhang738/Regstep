/// <reference path="../jquery-2.1.1.intellisense.js" />

var oldData = { id: null, regKey: null, value: null };
var checkboxgroups = {};
var radiogroups = {};


$(document).ready(function () {

    GetData();

    $('#registrantHeader').hide();

    BindEditable();

    $('#saveEdit').on('click', function () {
        $('#editModal').modal('hide');
        processing.showPleaseWait();
        var data = { ComponentKey: oldData.id, RegistrantKey: oldData.regKey, Value: $('[name="value"]').val() };
        $.ajax({
            url: '../../Cloud/SaveRegistrantData',
            type: "put",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                processing.hidePleaseWait();
                if (result.Success)
                    $('#' + oldData.regKey).find('td[data-id="' + oldData.id + '"]').html(result.Value);
                else
                    alert(result.Message);
            },
            error: function (result) {
                processing.hidePleaseWait();
            }
        });
    });

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
        $('#printable').attr('href', basePrint + '?type=' + report.ReportType);
        GetData();
    });

    $('#downloadReport').on('click', function (e) {
        e.preventDefault();
        report.Registrants = [];
        report.Fileds = [];
        window.location = '../../Cloud/DownloadFormReport?rawJson=' + JSON.stringify(report);
    });
});

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

function GetData() {
    processing.showPleaseWait();
    report.Registrants = [];
    report.Fields = [];
    $.ajax({
        url: '../../Cloud/GetRegistrationReport',
        type: "post",
        data: JSON.stringify(report),
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result.Success) {
                $.extend(report, result);
                $('#registrantData').html('');
                var html = '';
                var header = null;
                if (result.ReportType == 'all' || result.ReportType == 'active' || result.ReportType == 'canceled' || result.ReportType == "incompletes" || result.ReportType == "deleted" || result.ReportType == "rsvpAccept" || result.ReportType == "rsvpDecline") {
                    $('#registrantHeaderAll').show();
                    $('#registrantHeader').hide();
                    for (var i = 0; i < report.Registrants.length; i++) {
                        var date = moment(report.Registrants[i].Date);
                        html += '<tr id="' + report.Registrants[i].Id + '"><td><a href="../../Cloud/Registrant/' + report.Registrants[i].Id + '">' + report.Registrants[i].Confirmation + '</a></td><td>' + date.format('M/D/YYYY h:mm:ss A Z') + '</td><td>' + report.Registrants[i].Status + '</td><td>' + report.Registrants[i].Email + '</td>';
                        if (rsvp) {
                            html += '<td>' + (report.Registrants[i].RSVP ? rsvpLabel[0] : rsvpLabel[1]) + '</td>';
                        }
                        if (audience) {
                            html += '<td>' + report.Registrants[i].AudienceName + '</td>';
                        }
                        for (var j = 0; j < fields.length; j++) {
                            var data = findDataObj(report.Registrants[i].Data, fields[j].UId);
                            if (data === null) {
                                if (fields[j].SpecialDescriminator == 'file')
                                    html += '<td class="file" data-id="' + fields[j].UId + '">No File</td>';
                                else
                                    html += '<td class="editable-item cursor-pointer" data-id="' + fields[j].UId + '"></td>';
                            } else {
                                if (fields[j].SpecialDescriminator == 'file')
                                    html += '<td class="file editable-item cursor-pointer" data-id="' + data.Id + '">' + data.FormattedValue + '</td>';
                                else
                                    html += '<td class="editable-item cursor-pointer" data-id="' + data.Id + '">' + data.FormattedValue + '</td>';
                            }
                        }
                        html += '</tr>';
                    }
                } else if (result.ReportType == 'unbalanced') {
                    header = '<th>Registrant</th><th>Email</th><th>Amount</th><th>Note</th>';
                    for (var i = 0; i < report.Registrants.length; i++) {
                        var ammount = findData(report.Registrants[i].Data, 'Amount');
                        var note = findData(report.Registrants[i].Data, 'Note');
                        var refund = note == 'Refund Due';
                        html += '<tr><td><a href="../../Cloud/Registrant/' + report.Registrants[i].Id + '">' + report.Registrants[i].Confirmation + '</a></td><td>' + report.Registrants[i].Email + '</td>';
                        html += '<td>' + (refund ? '<span class="negative-balance">' : '') + ammount + (refund ? '</span>' : '') + '</td><td>' + note + '</td></tr>';
                    }
                } else if (result.ReportType == 'owed' || result.ReportType == 'refund') {
                    header = '<th>Registrant</th><th>Email</th><th>Amount</th>';
                    for (var i = 0; i < report.Registrants.length; i++) {
                        html += '<tr><td><a href="../../Cloud/Registrant/' + report.Registrants[i].Id + '">' + report.Registrants[i].Confirmation + '</a></td><td>' + report.Registrants[i].Email + '</td>';
                        html += '<td>' + findData(report.Registrants[i].Data, 'Amount') + '</td></tr>';
                    }
                } else if (result.ReportType == 'emailReport') {
                    header = '<th>Recipient</th><th>Event</th>';
                    for (var i = 0; i < report.Registrants.length; i++) {
                        html += '<tr><td>' + findDate(report.Registrants[i].Data, 'Recipient') + '</td><td>' + findData(report.Registrants[i].Data, 'Event') + '</td></tr>';
                    }
                } else if (result.ReportType == 'rsvp') {
                    header = '<th>Total</th><th>Accepted</th><th>Declined</th>';
                    html = '<tr><td>' + findData(report.Registrants[0].Data, 'Total') + '</td><td>' + findData(report.Registrants[0].Data, 'Accepted') + '</td><td>' + findData(report.Registrants[0].Data, 'Declined') + '</td></tr>';
                } else {
                    header = '';
                    for (var i = 0; i < result.Fields.length; i++) {
                        header += '<th>' + result.Fields[i].Name + '</th>';
                    }
                    html += '<tr>';
                    for (var i = 0; i < result.Fields.length; i++) {
                        var data = findDataObj(report.Registrants[0].Data, result.Fields[i].Name);
                        html += '<td>' + data.FormattedValue + '</td>';
                    }
                    html += '</tr>';
                }
            } else {
                alert(result.Message);
            }
            $('#registrantData').html(html);
            if (header !== null) {
                $('#registrantHeaderAll').hide();
                $('#registrantHeader').html(header);
                $('#registrantHeader').show();
            }
            $('#pageNumber').html('');
            for (var i = 0; i < report.TotalPages; i++) {
                $('#pageNumber').append('<option value="' + (i + 1) + '"' + ((i + 1) == result.Page ? ' selected="true"' : '') + '>' + (i + 1) + '</option>');
            }
            processing.hidePleaseWait();
            BindEditable();
        },
        error: function (result) {
            alert('Server Error');
            processing.hidePleaseWait();
        }
    });
}

function BindEditable() {
    $('.editable-item').on('click', function () {
        $('#editModal .modal-body').html('<div class="progress"><div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%"><span class="sr-only">Processing</span></div></div>');
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
                $('#editModal').find('.modal-body').html('<div class="row">' + result.Html + '</div>');
                RunBinding();
            } else {
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

function ReportJson() {
    this.Id = "";
    this.Filters = [];
    this.Sortings = [];
    this.Registrants = [];
    this.Fields = [];
    this.Page = 1;
    this.PageSize = 25;
    this.Success = true;
    this.Message = "";
    this.ReportType = "all";
    this.TotalPages = 1;
}

function findData(obj, id) {
    for (var i = 0; i < obj.length; i++) {
        if (obj[i].Id == id || obj[i].Variable == id)
            return obj[i].FormattedValue;
    }
    return "";
}

function findDataObj(obj, id) {
    for (var i = 0; i < obj.length; i++) {
        if (obj[i].Id == id || obj[i].Variable == id)
            return obj[i];
    }
    return null;
}