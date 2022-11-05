/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />
/// <reference path="../Tool/filter.js" />
/// <reference path="../Tool/saveSprite.js" />


var searchInformation = { Page: 1, RecordsPerPage: 25, Company: CurrentCompany, Sortings: [{ ActingOn: "Id", Descending: true, Priority: 1 }], Filters: null, TotalRecords: -1, TotalPages: -1 };
var customers;
var currentIndex;
var currentHorizontal;
var currentHeader;
var currentEmailList;

$(document).ready(function () {

    $('#dateTimePicker').datetimepicker({
        language: 'en'
    }).hide();

    $('#emailListError').hide();
    $('#timeZoneInfoDiv').hide();

    LoadEmailLists();

    $('#recordsPerPage').on('blur', function () {
        searchInformation.RecordsPerPage = $(this).val();
        searchInformation.Page = 1;
        LoadCustomers();
    });

    $('#page').on('change', function () {
        searchInformation.Page = $(this).val();
        LoadCustomers();
    });

    $('#pageLeft').on('click', function (e) {
        if (searchInformation.Page > 1) {
            searchInformation.Page--;
            $('#page').val(searchInformation.Page);
            LoadCustomers();
        }
    });

    $('#pageRight').on('click', function (e) {
        if (searchInformation.Page < searchInformation.TotalPages) {
            searchInformation.Page++;
            $('#page').val(searchInformation.Page);
            LoadCustomers();
        }
    });

    $('#newColumnDescriminatorTypeDiv').hide();

    $('#newColumnDescriminator').on('change', function () {
        if ($(this).val() == 'decimal' || $(this).val() == 'datetimeoffset')
            $('#newColumnDescriminatorTypeDiv').slideDown();
        else
            $('#newColumnDescriminatorTypeDiv').slideUp();
        if ($(this).val() == 'datetimeoffset')
            $('#timeZoneInfoDiv').slideDown();
        else
            $('#timeZoneInfoDiv').slideUp();

    });

    $('#addColumn').on('click', function () {
        $('#newColumn').val('');
        $('#columnModal').modal('show');
    });

    $('#uploadContactList').on('click', function () {
        $('#uploadListProgress').hide();
        $('#uploadContactListModal').modal('show');
    });

    $('#newColumnSave').on('click', function () {
        var column = $('#newColumn').val();
        var descriminator = $('#newColumnDescriminator').val();
        if (descriminator == '')
            descriminator = 'string';
        var descriminatorType = $('#newColumnDescriminatorType').val();
        var offset = $('#timeZoneInfo').val();
        if (descriminator == 'decimal')
            descriminator = 'decimal!' + descriminatorType;
        if (descriminator = 'datetimeoffset')
            descriminator = 'datetimeoffset!' + descriminatorType + ':' + offset;
        column = column.replace(/ /g, "_");
        column = column.replace(/[^a-zA-Z0-9_]/g, "");
        var data = { Company: CurrentCompany, Column: column, Descriminator: descriminator };
        $('#newColumnDescriminator').val('string');
        $('#newColumnDescriminatorTypeDiv').hide();
        $.ajax({
            url: '../Cloud/AddColumnToCustomerTable',
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    LoadCustomers();
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

    $('#uploadList').on('submit', function () {
        if ('undefined' === typeof window.FormData) {
            return true;
        } else {
            var fd = new FormData;
            if ('undefined' === typeof fd.append) {
                return true;
            } else {
                fd.append('files[]', $('#file').get(0).files[0]);
                fd.append('Company', CurrentCompany);
                $.ajax({
                    url: '../Cloud/UploadExcelSheetForContactList',
                    type: "post",
                    data: fd,
                    cache: false,
                    contentType: false,
                    processData: false,
                    success: function (result) {
                        $('#uploadContactListModal').modal('hide');
                        if (result.Success) {
                            $('#informationModal .modal-title').html('Upload Successful');
                            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">File Successfully Uploaded.</div></div>');
                            LoadCustomers();
                        } else {
                            $('#informationModal .modal-title').html('Upload Error');
                            $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + result.Message + '</div><div class="row"><div class="col-sm-10 col-sm-offset-1"><p class="text-center"><a href="RectifyContactListUploadError?cid=' + CurrentCompany + '&ulid=' + result.UId + '">Rectify Errors</a></p></div>');
                        }
                        $('#informationModal').modal('show');
                    }
                });
                return false;
            }
        }
    });

    $('#emailListSave').on('click', function () {
        var data = searchInformation;
        data.Name = $('#emailListName').val();
        $('#emailListError').html('<h4 class="text-center"><img src="../../Images/Programming/loading.gif" height="50px" /></h3>');
        $('#emailListError').show();
        $.ajax({
            url: '../Cloud/NewEmailListByFilter',
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    $('#emailListError').html('<h4 class="text-center">Email List Created</h3>');
                    $('#emailListError').show();
                    setTimeout(function () { ClearEmailListError() }, 1500);
                } else {
                    $('#emailListError').html('<h4>' + result.Message + '</h4>');
                    $('#emailListError').show();
                }
                $('#emailListName').val('');
            },
            error: function (result) {
                $('#emailListError').html('sets');
                $('#emailListError').show();
            }
        });
    });

    $('#createEmailList').on('click', function () {
        $('#emailListError').hide();
        $('#emailListModal').modal("show");
    });
    
    $('#activeEmailList').on('change', function () {
        currentEmailList = $(this).val();
        SetUpEmailListActions();
    });

    LoadCustomers();

    //*/
    $('#editDataSave').on('click', function () {
        SaveSprite("#spriteSave");
        var thisIndex = currentIndex;
        var thisHorizontal = currentHorizontal;
        var tr = $('tr[data-index=' + thisIndex + ']');
        var td = $(tr).children('[data-horizontal=' + thisHorizontal + ']');
        var editVal = $('#editVal').val();
        customers[thisIndex].Data[thisHorizontal].Value = editVal;
        $(td).html(editVal);
        var data = { Company: CurrentCompany, Data: customers[currentIndex] };
        $.ajax({
            url: '../Cloud/SaveCustomer',
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    for (var i = 0; i < jsonResult.Data.Data.length; i++) {
                        if (jsonResult.Data.Data[i].Key == customers[thisIndex].Data[thisHorizontal].Key) {
                            customers[thisIndex].Data[thisHorizontal].FormattedValue = jsonResult.Data.Data[i].FormattedValue;
                            $(td).html(customers[thisIndex].Data[thisHorizontal].FormattedValue);
                        }
                    }
                }
                else {
                    SaveError("#spriteSave");
                }
            },
            error: function (result) {
                SaveError("#spriteSave");
            }
        });
    });

    $(window).on('click', function () {
        $('.context-menu').hide();
    });

    $('#removeColumn').on('click', function (e) {
        column = currentHeader;
        var data = { Company: CurrentCompany, Column: column };
        $.ajax({
            url: '../Cloud/RemoveColumnFromCustomerTable',
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    LoadCustomers();
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
        e.stopPropagation();
        $('#contextMenuHeader').hide();
    });

    $('#editData').on('shown.bs.modal', function () {
        $('#editVal').focus();
    });

    $('#editVal').keyup(function (e) {
        if (e.which == 13) {
            $('#editDataSave').trigger('click');
        }
        if (e.which == 27) {
            $('#editData').modal('hide');
        }
    });

    $('#changeFilters').on('click', function () {
        $('#filterModal').modal('show');
    });

    $('#filterNow').on('click', function () {
        searchInformation.Filters = currentFilters;
        searchInformation.Page = 1;
        LoadCustomers();
    });

    $('#dateTimePicker').on('changeDate', function (e) {
        var date = $('#dateTimePicker').data('datetimepicker').getLocalDate();
        $('#editVal').val(date.toLocaleString());
    });
});

function LoadEmailLists() {
    $.ajax({
        url: '../Cloud/GetEmailLists',
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

function ClearEmailListError() {
    $('#emailListError').html('');
    $('#emailListModal').modal('hide');
}

function LoadCustomers() {
    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">Loading Data...</h3></div></div>');
    var information = '';
    var data = searchInformation;
    $.ajax({
        url: '../Cloud/Contacts',
        type: "get",
        data: data,
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
                for (var i = 0; i < searchInformation.Filters.length; i++) {
                    currentFilters.push(searchInformation.Filters[i]);
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
                information += '<table class="table table-striped table-registration-data"><thead class="dataHeader"><tr>';
                information += '<th class="no-wrap el-action-td ela-header">ELA</th>'
                for (var i = 0; i < jsonResult.Data[0].Data.length; i++) {
                    filters.push(jsonResult.Data[0].Data[i].Key);
                    var head = jsonResult.Data[0].Data[i].Key;
                    head = head.replace(/_/g, " ");
                    var param = "";
                    switch (jsonResult.Data[0].Data[i].Key)
                    {
                        case "Email":
                            break;
                        default:
                            param = " deletable";
                    }
                    information += '<th data-id="' + jsonResult.Data[0].Data[i].Key + '" class="no-wrap' + param + '">' + head + '</th>'
                }
                information += '</thead><tbody>';
                for (var i = 0; i < jsonResult.Data.length; i++) {
                    information += '<tr data-id="' + jsonResult.Data[i].Id + '" data-uid="' + jsonResult.Data[i].UId + '" data-index="' + i + '">';
                    information += '<td class="el-action-td el-action-click pointer text-center editable el-cell" data-el-id="' + jsonResult.Data[i].UId + '"></td>';
                    for (var j = 0; j < jsonResult.Data[i].Data.length; j++) {
                        information += '<td data-horizontal="' + j + '"  onclick="EditData(this, event)" class=\"editable no-wrap\">' + jsonResult.Data[i].Data[j].FormattedValue + '</td>';
                    }
                    information += '</tr>';
                }
                information += '</tbody></table>';
                $('#mainHolder > section').html(information);
                SetUpFilters();
                SetUpEmailListActions();
                ClickHeaders();
            }
            else {
                $('#loadingError #loadingMessage').html(jsonResult.Message);
                $('#loadingError').modal('show');
            }
        },
        error: function (result) {
            $('#loadingError').modal("show");
        }
    });
}

function SetUpEmailListActions() {
    if (currentEmailList != null && currentEmailList != '00000000-0000-0000-0000-000000000000') {
        for (var i = 0; i < customers.length; i++) {
            $.ajax({
                url: '../Cloud/GetInEmailList',
                type: "post",
                data: JSON.stringify({ cid: CurrentCompany, cuid: customers[i].UId, elid: currentEmailList }),
                contentType: "application/json",
                dataType: "json",
                traditional: "true",
                success: function (jsonResult) {
                    if (!jsonResult.In) {
                        $('.el-action-td[data-el-id="' + jsonResult.Id + '"]').html('<span data-id="' + jsonResult.Id + '" class="el-action el-action-add glyphicon glyphicon-plus-sign"></span>');
                    } else {
                        $('.el-action-td[data-el-id="' + jsonResult.Id + '"]').html('<span data-id="' + jsonResult.Id + '" class="el-action el-action-remove glyphicon glyphicon-minus-sign"></span>');
                    }
                }
            });
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
    var add = $(span).hasClass('el-action-add');
    if (add) {
        $.ajax({
            url: '../Cloud/AddCustomerToEmailList',
            type: "post",
            data: JSON.stringify({ cid: CurrentCompany, cuid: id, elid: currentEmailList }),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    $(span).removeClass('el-action-add');
                    $(span).addClass('el-action-remove');
                    $(span).removeClass('glyphicon-plus-sign');
                    $(span).addClass('glyphicon-minus-sign');
                }
            }
        });
    } else {
        $.ajax({
            url: '../Cloud/RemoveCustomerFromEmailList',
            type: "post",
            data: JSON.stringify({ cid: CurrentCompany, cuid: id, elid: currentEmailList }),
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
            }
        });
    }
}

function EditData(item, event) {
    var int = parseInt($(item).attr('data-horizontal'));
    currentIndex = $(item).parent().attr('data-index');
    currentHorizontal = int;
    var descriminatorFull = customers[currentIndex].Data[int].Descriminator;
    var descriminatorRgx = /([^!]*)/g;
    var match = descriminatorRgx.exec(descriminatorFull)
    var descriminator = match[1];
    switch(descriminator)
    {
        case "datetimeoffset":
            $("#dateTimePicker").show();
            $("#editString").hide();
            $("#dateTimePicker").data('datetimepicker').setDate(Date.parse(customers[currentIndex].Data[int].Value));
            break;
        default:
            $("#dateTimePicker").hide();
            $("#editString").show();
            break;
    }
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
        LoadCustomers();
    });
    $('.dataHeader th.deletable').on('contextmenu', function (event) {
        currentHeader = $(this).attr('data-id');
        var topSpot = event.pageY;
        var leftSpot = event.pageX;
        $('#contextMenuHeader').css({ top: topSpot, left: leftSpot });
        $('#contextMenuHeader').show();
        event.preventDefault();
        return false;
    });
    if (searchInformation.Sortings[0].Descending) {
        span = ' <span class="sort glyphicon glyphicon-arrow-down"></span>';
    } else {
        span = ' <span class="sort glyphicon glyphicon-arrow-up"></span>';
    }
    $('.dataHeader th[data-id="' + searchInformation.Sortings[0].ActingOn + '"]').append(span);
}