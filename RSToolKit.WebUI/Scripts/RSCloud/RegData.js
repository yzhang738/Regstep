/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var recordsPerPage = 25;
var currentPage = 1;
var registrationData;
var formData;
var currentIndex;
var currentHorizontal;
var totalPages = 0;
var filter = { column: "Confirmation", value: ""}
var sort = { SortColumn: "Confirmation", Ascending: true }
var live = true;

$(document).ready(function () {


    $('#liveRecords').bootstrapSwitch('size', 'small');
    $('#liveRecords').bootstrapSwitch('onColor', 'success');
    $('#liveRecords').bootstrapSwitch('offColor', 'danger');
    $('#liveRecords').on('switchChange', function (e, data) {
        if (data.value) {
            live = true;
            LoadRegistrations(formId, companyId);
        } else {
            live = false;
            LoadRegistrations(formId, companyId);
        }
    });

    $(".FolderContents").hide();
    $(".FolderClick").on("click", function () {
        if ($(this).parent(".TopFolder").children(".FolderContents").is(":visible")) {
            $(".glyphicon", this).removeClass("glyphicon-chevron-down").addClass("glyphicon-chevron-right");
            $(this).parent(".TopFolder").children(".FolderContents").slideUp(500);
        } else {
            $(".glyphicon", this).removeClass("glyphicon-chevron-right").addClass("glyphicon-chevron-down");
            $(this).parent(".TopFolder").children(".FolderContents").slideDown(500);
        }
    });
    $(".ItemClick").on("click", function () {
        var id = $(this).attr("data-id");
        formId = id;
        var company = $(this).attr("data-company");
        companId = company;
        sortColumn = "Confirmation";
        ascending = true;
        LoadRegistrations(id, company);
    });

    $('#recordsPerPage').on('blur', function () {
        recordsPerPage = $(this).val();
        currentPage = 1;
        LoadRegistrations(formId, companyId);
    });

    $('#page').on('change', function () {
        currentPage = $(this).val();
        LoadRegistrations(formId, companyId);
    });
    
    $('#pageLeft').on('click', function (e) {
        if (currentPage > 1) {
            currentPage--;
            $('#page').val(currentPage);
            LoadRegistrations(formId, companyId);
        }
    });

    $('#pageRight').on('click', function (e) {
        if (currentPage < totalPages) {
            currentPage++;
            $('#page').val(currentPage);
            LoadRegistrations(formId, companyId);
        }
    });

    $('#filter').on('click', function () {
        filter.column = $('#filterColumn > option:selected').val();
        filter.value = $('#filterText').val();
        LoadRegistrations(formId, companyId);
    })

    //*/
    $('#loadingError').modal();
    $('#editData').modal();
    $('#editDataSave').on('click', function () {
        var tr = $('tr[data-index=' + currentIndex + ']');
        var td = $(tr).children('[data-horizontal=' + currentHorizontal + ']');
        var editVal = $('#editVal').val();
        var viewVal = $('#viewVal').val();
        if (viewVal === null || viewVal === undefined || viewVal == "") viewVal = editVal;
        registrationData[currentIndex].Data[currentHorizontal].Value = editVal;
        $(td).html(viewVal);
        $('#editDataBody').html('');
        var data = { form: formId, company: companyId, data: registrationData[currentIndex] };
        $.ajax({
            url: '../RSCloud/SaveRegistrant',
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

function LoadRegistrations(id, company) {
    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">Loading Data...</h3></div></div>');
    formId = id;
    var information = '';
    var data = { Id: id, Company: company, RecordsPerPage: recordsPerPage, Page: currentPage, SortType: sort, FilterType: filter, Live: live };
    $.ajax({
        url: '../RSCloud/RegistrationData',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                registrationData = jsonResult.Data;
                formData = jsonResult.Form;
                totalPages = jsonResult.Information.TotalPages;
                if (jsonResult.Data.length < 0) return;
                $('#page').html('');
                for (var o = 1; o <= jsonResult.Information.TotalPages; o++) {
                    $('#page').append('<option value="' + o + '">' + o + '</option>');
                }
                $('#page').val('' + currentPage);
                if (jsonResult.Data.length == 0)
                {
                    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">No Data Matches Criteria</h3></div></div>');
                    return;
                }
                var header = '<div class="row"><div class="col-md-12">' + jsonResult.Form.Name + '</div></div>';
                $('#headerText').html(header);
                //Lets build the table
                var filterOptions = '<option value="Confirmation">Confirmation</option><option value="RSVP">RSVP</option><option value="Audience">Audience</option><option value="Status">Status</option><option value="Date">Date</option><option value="Type">Type</option><option value="Email">Email</option>';
                information += '<div class="table-responsive"><table class="table table-striped table-registration-data"><thead class="dataHeader"><tr>';
                information += '<th data-id="Confirmation" class="no-wrap">Confirmation</th><th data-id="RSVP" class="no-wrap">RSVP</th><th data-id="Audience" class="no-wrap">Audience</th><th data-id="Status" class="no-wrap">Status</th><th data-id="ModifiedDate" class="no-wrap">Date</th><th data-id="Type" class="no-wrap">Type</th><th data-id="Email" class="no-wrap">Email</th>';
                for (var i = 5; i < jsonResult.Data[0].Data.length; i++) {
                    filterOptions += '<option value="' + jsonResult.Data[0].Data[i].Key + '">' + jsonResult.Data[0].Data[i].Variable + '</option>';
                    information += '<th data-id="' + jsonResult.Data[0].Data[i].Key + '" class="no-wrap">' + jsonResult.Data[0].Data[i].Variable + '</th>'
                }
                $('#filterColumn').html(filterOptions);
                $('#filterColumn > option[value="' + filter.column + '"]').attr('selected', 'true');
                information += '</thead><tbody>';
                for (var i = 0; i < jsonResult.Data.length; i++) {
                    information += '<tr data-id="' + jsonResult.Data[i].Id + '" data-index="' + i + '"><td class="conf">' + jsonResult.Data[i].Confirmation + '</td>';
                    var rsvp = 'Yes';
                    if (jsonResult.Data[i].Data[0].Value == "1") {
                        rsvp = 'No';
                    }
                    information += '<td>' + rsvp + '</td>';
                    information += '<td class="audience">RETRIEVING</td>';
                    GetAudience(jsonResult.Form.Company, jsonResult.Data[i].Data[1].Value, jsonResult.Data[i].Id)
                    information += '<td>' + GetStatus(jsonResult.Data[i].Data[2].Value) + "</td>";
                    information += '<td class="no-wrap">' + jsonResult.Data[i].RegistrationDate + '</td>';
                    var testOrLive = 'Live';
                    if (jsonResult.Data[i].Data[3].Value == "0") testOrLive = 'Test';
                    information += '<td>' + testOrLive + '</td>';
                    information += '<td data-horizontal="4" onclick="EditData(this, event)" class=\"editable\">' + jsonResult.Data[i].Data[4].Value + '</td>';
                    for (var j = 5; j < jsonResult.Data[i].Data.length; j++) {
                        information += '<td data-horizontal="' + j + '"  onclick="EditData(this, event)" class=\"editable\">' + jsonResult.Data[i].Data[j].ViewValue + '</td>';
                    }
                    information += '</tr>';
                }
                information += '</tbody></table></div>';
                $('#mainHolder > section').html(information);
                ClickHeaders();
            }
            else {
                $('#loadingError #loadingMessage').html(jsonResult.Message);
                $('#loadingError').modal();
            }
        },
        error: function (result) {
            $('#loadingError').modal("show");
        }
    });
}

function EditData(item, event) {
    var int = parseInt($(item).attr('data-horizontal'));
    currentIndex = $(item).parent().attr('data-index');
    currentHorizontal = int;
    var data = { id: registrationData[0].Data[int].Key, form: formId, company: companyId, conf: $(item).parent().children('.conf').html() };
    $.ajax({
        url: '../RSCloud/GetComponent',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                $('#editDataBody').html(jsonResult.Html);
                $('#editJavaScript').html(jsonResult.JavaScript);
                eval(jsonResult.JavaScript);
                $('#editData').modal('show');
            }
            else {
                $('#loadingError #loadingMessage').html(jsonResult.Message);
                $('#loadingError').modal();
            }
        },
        error: function (result) {
            $('#loadingError #loadingMessage').html(result.Message);
            $('#loadingError').modal("show");
        }
    });
}

function ClickHeaders() {
    $('.dataHeader th').on('click', function () {
        var columnId = $(this).attr('data-id');
        var ascending = true;
        if (sort.SortColumn == columnId) {
            ascending = !sort.Ascending;
        }
        sort.SortColumn = columnId;
        sort.Ascending = ascending;
        $(this).parent('.dataHeader').children('.sort').remove();
        LoadRegistrations(formId, companyId);
    });
    if (sort.Ascending) {
        span = '<span class="sort glyphicon glyphicon-arrow-down"></span>';
    } else {
        span = '<span class="sort glyphicon glyphicon-arrow-up"></span>';
    }
    $('.dataHeader th[data-id="' + sort.SortColumn + '"]').append(span);
}

function GetStatus (status) {
    switch (status) {
        case "0":
            return "Unregistered";
        case "1":
            return "Incomplete";
        case "2":
            return "Completed";
    }
}

function GetAudience(company, id, reg) {
    var data = { company: company, id: id };
    $.ajax({
        url: '../RSCloud/AudienceName',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                if (jsonResult.Name == "") {
                    $('tr[data-id="' + reg + '"] > .audience').html("N/A");
                } else {
                    $('tr[data-id="' + reg + '"] > .audience').html(jsonResult.Name);
                }
            }
            else {
                $('tr[data-id="' + reg + '"] > .audience').html("Error");
            }
        },
        error: function (result) {
            $('tr[data-id="' + reg + '"] > .audience').html("Error");
        }
    });
}