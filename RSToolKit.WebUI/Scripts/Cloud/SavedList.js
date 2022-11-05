/// <reference path="../Tool/restful.js" />
var request = new SaveListRequest();
var editing = {
    contact: new Contact(),
    data: new ContactData(),
    header: new ContactHeader(),
    contactIndex: -1,
    dataIndex: -1,
    td: null,
    tr: null,
};

$(document).on('ready', function () {

    $('#yesOverwriteReport').hide();

    $('#timezoneDiv').hide();
    $('#moneyDiv').hide();

    request.CompanyKey = CompanyKey;

    $('#addContact').on('click', function () {
        $('#newContact').modal('hide');
    });

    $('.table-sort').on('click', function (e) {
        e.preventDefault();
        var link = $(this);
        var actingon = link.attr('data-actingon');
        if (request.Sorting.ActingOn == actingon) {
            request.Sorting.Descending = !request.Sorting.Descending;
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes-alt');
        } else {
            $('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt').removeClass('glyphicon-sort-by-attributes');
            request.Sorting.ActingOn = actingon;
            request.Sorting.Descending = false;
            link.children('.sort-icon').addClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt');
        }
        LoadContacts();
    });

    $('#headerDescriminator').on('change', function () {
        var value = $(this).val();
        switch (value) {
            case "text":
            case "int":
            case "double":
            case "float":
            case "datetime":
            case "time":
            case "date":
                $('#timezoneDiv').hide();
                $('#moneyDiv').hide();
                break;
            case "datetimeoffset":
                $('#moneyDiv').hide();
                $('#timezoneDiv').show();
                break;
            case "decimal":
                $('#timezoneDiv').hide();
                $('#moneyDiv').show();
                break;
        }
    });

    $('#addHeader').on('click', function () {
        processing.showPleaseWait();
        $('#newHeader').modal('hide');
        var data = {};
        data.Name = $('#headerName').val();
        data.Descriminator = $('#headerDescriminator').val();
        data.CompanyKey = CompanyKey;
        data.DescriminatorOptions = [];
        data.SavedListKey = uid;
        switch (data.Descriminator) {
            case "datetimeoffset":
                data.DescriminatorOptions.push({ Key: 'timezone', Value: $('#headerTimezone').val() });
                break;
            case "decimal":
                data.DescriminatorOptions.push({ Key: 'culture', Value: $('#headerMoney').val() });
                break;
        }

        $.ajax({
            url: '../../Cloud/ContactHeader',
            type: "post",
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    window.location.reload();
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
    });

    $('#pageSize').on('blur', function () {
        var records = $(this).val();
        if (isNaN(records)) {
            $(this).val('25');
            return;
        }
        request.RecordsPerPage = records;
        LoadContacts();
    });
    $('#pageNumber').on('change', function () {
        request.Page = $(this).val();
        LoadContacts();
    });
    $('#pageLeft').on('click', function () {
        if (request.Page == 1)
            return;
        request.Page--;
        $('#pageNumber').val(request.Page);
        LoadContacts();
    });
    $('#pageRight').on('click', function () {
        if (request.Page == request.TotalPages)
            return;
        request.Page++;
        $('#pageNumber').val(request.Page);
        LoadContacts();
    });

    $('#removeContact').on('click', function () {
        processing.showPleaseWait();
        $('#savedList').modal('hide');
        var data = {};
        data.contacts = [];
        $('.contact-selected:checked').each(function (i) {
            var tr = $(this).parent().parent();
            data.contacts.push(tr.attr('id'));
        });
        data.id = uid;
        $.ajax({
            url: '../../Cloud/SavedListContacts',
            type: 'delete',
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    for (var i = 0; i < result.Removed.length; i++) {
                        $('#' + result.Removed[i]).remove();
                    }
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
    });

    LoadContacts();

    $('#saveEdit').on('click', function () {
        SaveData();
    });

    $('#refreshList').on('click', function (e) {
        e.preventDefault();
        LoadContacts();
    });

});

function LoadContacts() {
    processing.showPleaseWait();
    // Grab filters
    request.Filters = PopulateFilters();
    request.Contacts = [];
    request.UId = uid;
    $.ajax({
        url: '../../Cloud/SavedListContacts',
        type: "get",
        data: { rawJson: JSON.stringify(request) },
        contentType: "application/json",
        dataType: "json",
        success: function (result) {
            if (result.Success) {
                var table = {};
                table.object = $('#contactData');
                table.newHtml = '';
                table.oldHtml = table.object.html();
                $.extend(true, request, result.Data);
                for (var i = 0; i < request.Contacts.length; i++) {
                    var contact = new Contact();
                    $.extend(contact, request.Contacts[i]);
                    table.newHtml += '<tr data-index="' + i + '" id="' + contact.UId + '">';
                    table.newHtml += '<td><input type="checkbox" class="contact-selected" /></td>';
                    table.newHtml += '<td><a href="../../Cloud/Contact/' + contact.UId + '">' + contact.Email + '</a></td>';
                    for (var j = 0; j < headers.length; j++) {
                        var data = new ContactData();
                        var t_data = findData(contact.Data, headers[j].UId);
                        if (t_data === null || typeof (t_data) == 'undefined') {
                            data.HeaderKey = headers[j].UId;
                            data.UId = contact.UId;
                            request.Contacts[i].Data.push(data);
                            t_data = { index: request.Contacts[i].Data.length - 1 }
                        } else {
                            $.extend(data, t_data.data);
                        }
                        if (data.Value === null || /^\s*$/.test(data.Value))
                            data.Value = "";
                        table.newHtml += '<td class="item-editable" data-header-index="' + j + '" data-index="' + t_data.index + '">' + data.PrettyValue + '</td>';
                    }
                    table.newHtml += '</tr>';
                }
                var pageOptions = $('#pageNumber option');
                if (pageOptions.length < request.TotalPages) {
                    for (var i = pageOptions.length; i < request.TotalPages; i++) {
                        $('#pageNumber').append('<option value="' + (i + 1) + '"' + ((i + 1) == result.Page ? ' selected="true"' : '') + '>' + (i + 1) + '</option>');
                    }
                } else {
                    var removeAt = pageOptions.length - 1;
                    for (var i = 0; i < pageOptions.length - request.TotalPages; i++) {
                        pageOptions[removeAt].remove();
                        removeAt--;
                    }
                }
                table.object.html(table.newHtml);
                BindEditables();
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

function BindEditables() {
    $('.item-editable').on('click', function () {
        editing.td = $(this);
        editing.tr = $(this).parent('tr');
        editing.contactIndex = editing.tr.attr('data-index');
        editing.dataIndex = editing.td.attr('data-index');
        if (editing.dataIndex == '-10') {
            editing.header.Descriminator == "email";
        } else {
            editing.header = headers[editing.td.attr('data-header-index')];
        }
        if (editing.contactIndex == -1)
            return
        editing.contact = request.Contacts[editing.contactIndex];
        if (editing.dataIndex == -1)
            return;
        editing.data = request.Contacts[editing.contactIndex].Data[editing.dataIndex];
        ShowEditableItem();
    });
}

function RunBinding() {
    $('.editing-value[data-validate="datetime"]').datetimepicker();
    $('.editing-value[data-validate="date"]').datetimepicker({
        pickTime: false
    });
    $('.editing-value[data-validate="time"]').datetimepicker({
        pickDate: false
    });
}

function SaveData() {
    // First we need to validate the data being sent
    var editor = $('.editing-value');
    var validator = editor.attr('data-validate');
    switch (validator) {
        case 'number':
            if (isNaN(editing.val())) {
                alert("You must enter a valid number.");
                return;
            }
            break;
        case 'email':
            if (!(/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(editor.val()))) {
                alert("You must enter a valid email.");
                return;
            }
            break;
    }
    processing.showPleaseWait();
    var data = {};
    data.UId = editing.data.UId;
    data.HeaderKey = editing.data.HeaderKey;
    data.Value = editor.val();
    $.ajax({
        url: '../../Cloud/ContactData',
        type: "put",
        data: AddJsonAntiForgeryToken(data),
        dataType: "json",
        success: function (result) {
            if (result.Success) {
                request.Contacts[editing.contactIndex].Data[editing.dataIndex].Value = editor.val();
                editing.td.html(editor.val());
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

function ShowEditableItem(id, regKey) {
    var html = '';
    html += '<div class="row">';
    switch (editing.header.Descriminator) {
        case 'datetime':
            var value = editing.data.Value;
            if (value == '')
                value = moment().format('MM/DD/YYYY h:mm A');
            html += '<input type="text" class="editing-value form-control" data-validate="datetime" value="' + value + '">';
            break;
        case 'date':
            var value = editing.data.Value;
            if (value == '')
                value = moment().format('MM/DD/YYYY');
            html += '<input type="text" class="editing-value form-control" data-validate="date" value="' + value + '">';
            break;
        case 'time':
            var value = editing.data.Value;
            if (value == '')
                value = moment().format('h:mm A');
            html += '<input type="text" class="editing-value form-control" data-validate="time" value="' + value + '">';
            break;
        case 'number':
            html += '<input type="number" class="editing-value form-control" data-validate="number" value="' + editing.data.Value + '">';
            break;
        case 'email':
            html += '<input type="text" class="editing-value form-control" data-validate="email" value="' + editing.data.Value + '">';
            break;
        case 'text':
        default:
            html += '<input type="text" class="editing-value form-control" data-validate="text" value="' + editing.data.Value + '">';
            break;
    }
    html += '</div>';
    $('#editModal').find('.modal-body').html(html);
    RunBinding();
    $('#editModal').modal('show');
}

function findData(obj, header) {
    for (var i = 0; i < obj.length; i++) {
        if (obj[i].HeaderKey == header)
            return { index: i, data: obj[i] };
    }
    return null;
}

function findContact(obj, id) {
    for (var i = 0; i < obj.length; i++) {
        if (obj[i].UId == id) {
            return obj[i];
        }
    }
    return null;
}

function findHeader(id) {
    for (var i = 0; i < headers.length; i++) {
        if (headers[i].UId == id) {
            return headers[i];
        }
    }
    return null;
}

/******************/
/* Custom Objects */
/******************/

function SaveListRequest() {
    this.UId = '00000000-0000-0000-0000-000000000000';
    this.Page = 1;
    this.RecordsPerPage = 25;
    this.TotalRecords = -1;
    this.TotalPages = -1;
    this.CompanyKey = '';
    this.Sorting = new Sorting();
    this.Filters = [];
    this.Contacts = [];
}

function Sorting() {
    this.ActingOn = 'Email';
    this.Descending = false;
}

function Contact() {
    this.SortingId = -1;
    this.UId = '00000000-0000-0000-0000-000000000000';
    this.CompanyKey = '';
    this.Name = '';
    this.Description = '';
    this.Permission = '';
    this.DateCreated = '';
    this.DateModified = '';
    this.Owner = '';
    this.Group = '';
    this.ModificationToken = '';
    this.ModifiedBy = '';
    this.Data = [];
    this.Email = '';
}

function ContactData() {
    this.UId = '00000000-0000-0000-0000-000000000000';
    this.SortingId = -1;
    this.Value = '';
    this.PrettyValue = '';
    this.HeaderKey = '';
}

function ContactHeader() {
    this.UId = '00000000-0000-0000-0000-000000000000';
    this.SortingId = -1;
    this.Name = '';
    this.CompanyKey = '';
    this.SavedListKey = '';
    this.Descriminator = '';
    this.DescriminatorOptions = {};
}

/**********************/
/* End Custom Objects */
/**********************/