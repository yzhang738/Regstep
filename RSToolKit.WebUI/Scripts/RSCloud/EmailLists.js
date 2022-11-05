var searchInformation = { Page: 1, RecordsPerPage: 25, Company: CurrentCompany, Sortings: [{ ActingOn: "Id", Descending: true, Priority: 1 }], Filters: null, TotalRecords: -1, TotalPages: -1, UId: null };
var customers;

$(document).ready(function () {

    LoadEmailLists();

    $('#recordsPerPage').on('blur', function () {
        searchInformation.RecordsPerPage = $(this).val();
        searchInformation.Page = 1;
        LoadEmails();
    });

    $('#page').on('change', function () {
        searchInformation.Page = $(this).val();
        LoadEmails();
    });

    $('#pageLeft').on('click', function (e) {
        if (searchInformation.Page > 1) {
            searchInformation.Page--;
            $('#page').val(searchInformation.Page);
            LoadEmails();
        }
    });

    $('#pageRight').on('click', function (e) {
        if (searchInformation.Page < searchInformation.TotalPages) {
            searchInformation.Page++;
            $('#page').val(searchInformation.Page);
            LoadEmails();
        }
    });

    $('#activeEmailList').on('change', function () {
        searchInformation.UId = $(this).val();
        LoadEmails();
    });

    $('#deleteList').on('click', function () {
        if (searchInformation.UId === null || searchInformation.UId == '00000000-0000-0000-0000-000000000000') {
            return;
        }
        $.ajax({
            url: '../RSCloud/DeleteEmailList',
            type: "post",
            data: JSON.stringify({ cid: CurrentCompany, elid: searchInformation.UId }),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    searchInformation.UId = '00000000-0000-0000-0000-000000000000';
                    $('#mainHolder > section').html('<div class="row"><div class="col-sm-12"><h3 class="text-center">No List Selected</h3></div></div>');
                    LoadEmailLists();
                } else {
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + jsonResult.Message + '</div></div>');
                    $('#informationModal').modal('show');
                }
            },
            error: function (result) {
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
                $('#informationModal').modal('show');

            }
        });

    });

    // FILTERS

    $('#changeFilters').on('click', function () {
        $('#filterModal').modal('show');
    });

    $('#filterNow').on('click', function () {
        searchInformation.Filters = currentFilters;
        searchInformation.Page = 1;
        LoadEmails();
    });

    $('#editVal').keyup(function (e) {
        if (e.which == 13) {
            $('#editDataSave').trigger('click');
        }
        if (e.which == 27) {
            $('#editData').modal('hide');
        }
    });

    $('#editData').on('shown.bs.modal', function () {
        $('#editVal').focus();
    });

    $('#editDataSave').on('click', function () {
        var tr = $('tr[data-index=' + currentIndex + ']');
        var td = $(tr).children('[data-horizontal=' + currentHorizontal + ']');
        var editVal = $('#editVal').val();
        customers[currentIndex].Data[currentHorizontal].Value = editVal;
        $(td).html(editVal);
        var data = { Company: CurrentCompany, Data: customers[currentIndex] };
        $.ajax({
            url: '../RSCloud/SaveCustomer',
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                }
                else {
                    $('#saveError #saveMessage').html(jsonResult.Message);
                    $('#saveError').modal();
                }
            },
            error: function (result) {
                $('#saveError').modal("show");
                $('#saveMessage').html('');
            }
        });
    });
});

function LoadEmailLists() {
    $.ajax({
        url: '../RSCloud/GetEmailLists',
        type: "post",
        data: JSON.stringify({ cid: CurrentCompany }),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                $('#activeEmailList').html('');
                for (var i = 0; i < jsonResult.Data.length; i++) {
                    $('#activeEmailList').append('<option value="' + jsonResult.Data[i].UId + '">' + jsonResult.Data[i].Name + '</option>');
                }
            }
        }
    });

}

function LoadEmails() {
    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">Loading Data...</h3></div></div>');
    var information = '';
    var data = searchInformation;
    $.ajax({
        url: '../RSCloud/GetEmailListEmails',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                customers = jsonResult.Data;
                searchInformation = jsonResult.Information;
                filters = [];
                while (currentFilters.length > 0) {
                    currentFilters.pop();
                }
                if (searchInformation.Filters !== null) {
                    for (var i = 0; i < searchInformation.Filters.length; i++) {
                        currentFilters.push(searchInformation.Filters[i]);
                    }
                }
                if (jsonResult.Data.length < 0) return;
                $('#page').html('');
                for (var o = 1; o <= searchInformation.TotalPages; o++) {
                    $('#page').append('<option value="' + o + '">' + o + '</option>');
                }
                $('#page').val('' + searchInformation.Page);
                if (jsonResult.Data.length == 0) {
                    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">No Customers Matches Criteria</h3></div></div>');
                    return;
                }
                //Lets build the table
                information += '<div class="table-responsive"><table class="table table-striped table-registration-data"><thead class="dataHeader"><tr>';
                information += '<th class="no-wrap el-action-td">ELA</th>'
                for (var i = 0; i < jsonResult.Data[0].Data.length; i++) {
                    filters.push(jsonResult.Data[0].Data[i].Key);
                    var head = jsonResult.Data[0].Data[i].Key;
                    head = head.replace(/_/g, " ");
                    var param = "";
                    switch (jsonResult.Data[0].Data[i].Key) {
                        case "Email":
                            break;
                        case "FirstName":
                            head = "First Name";
                            break;
                        case "LastName":
                            head = "Last Name";
                            break;
                        case "PhoneNumber":
                            head = "Phone Number";
                            break;
                        case "Address":
                            break;
                        case "Address2":
                            head = "Address Line 2";
                            break;
                        case "City":
                            break;
                        case "State":
                            break;
                        case "ZipCode":
                            head = "Zip Code";
                            break;
                        default:
                            param = " deletable";
                    }
                    information += '<th data-id="' + jsonResult.Data[0].Data[i].Key + '" class="no-wrap' + param + '">' + head + '</th>'
                }
                information += '</thead><tbody>';
                for (var i = 0; i < jsonResult.Data.length; i++) {
                    information += '<tr data-id="' + jsonResult.Data[i].Id + '" data-uid="' + jsonResult.Data[i].UId + '" data-index="' + i + '">';
                    information += '<td class="el-action-td el-action-click pointer text-center editable" data-el-id="' + jsonResult.Data[i].UId + '"></td>';
                    for (var j = 0; j < jsonResult.Data[i].Data.length; j++) {
                        information += '<td data-horizontal="' + j + '"  onclick="EditData(this, event)" class=\"editable no-wrap\">' + jsonResult.Data[i].Data[j].Value + '</td>';
                    }
                    information += '</tr>';
                }
                information += '</tbody></table></div>';
                $('#mainHolder > section').html(information);
                SetUpFilters();
                SetUpEmailListActions();
                ClickHeaders();
            }
            else {
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + jsonResult.Message + '</div></div>');
                $('#informationModal').modal('show');
            }
        },
        error: function (result) {
            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            $('#informationModal').modal('show');
        }
    });

}

function SetUpEmailListActions() {
    if (searchInformation.UId != null && searchInformation.UId != '00000000-0000-0000-0000-000000000000') {
        for (var i = 0; i < customers.length; i++) {
            $('.el-action-td[data-el-id="' + customers[i].UId + '"]').html('<span data-id="' + customers[i].UId + '" class="el-action el-action-add glyphicon glyphicon-minus-sign"></span>');
        }
        $('.el-action-click').on('click', function () {
            ELAction(this);
        });
        $('.el-action-td').show();
    } else {
        $('.el-action-td').hide();
    }
}

function ELAction(td) {
    var span = $(td).children('span');
    var id = $(span).attr('data-id');
    $.ajax({
        url: '../RSCloud/RemoveCustomerFromEmailList',
        type: "post",
        data: JSON.stringify({ cid: CurrentCompany, cuid: id, elid: searchInformation.UId }),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                $(span).addClass('el-action-add');
                $(span).removeClass('el-action-remove');
                $(span).removeClass('glyphicon-minus-sign');
                $(span).addClass('glyphicon-plus-sign');
            }
            LoadEmails();
        }
    });
}

function EditData(item, event) {
    var int = parseInt($(item).attr('data-horizontal'));
    currentIndex = $(item).parent().attr('data-index');
    currentHorizontal = int;
    $('#editHeader').val(customers[currentIndex].Data[int].Key);
    $('#editVal').val(customers[currentIndex].Data[int].Value);
    $('#editData').modal('show');
}

function ClickHeaders() {
    $('.dataHeader th').on('click', function () {
        var columnId = $(this).attr('data-id');
        var descending = true;
        if (searchInformation.Sortings[0].ActingOn == columnId) {
            descending = !searchInformation.Sortings[0].Descending;
        }
        searchInformation.Sortings[0].ActingOn = columnId;
        searchInformation.Sortings[0].Descending = descending;
        searchInformation.Sortings = [{ ActingOn: columnId, Descending: descending, Priority: 1 }];
        $(this).parent('.dataHeader').children('.sort').remove();
        LoadEmails();
    });
    if (searchInformation.Sortings[0].Descending) {
        span = ' <span class="sort glyphicon glyphicon-arrow-down"></span>';
    } else {
        span = ' <span class="sort glyphicon glyphicon-arrow-up"></span>';
    }
    $('.dataHeader th[data-id="' + searchInformation.Sortings[0].ActingOn + '"]').append(span);
}