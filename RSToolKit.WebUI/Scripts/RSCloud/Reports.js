var searchInformation = { Page: 1, RecordsPerPage: 25, Company: CurrentCompany, Sortings: [{ ActingOn: "DateModified", Descending: true, Priority: 1 }], Filters: null, TotalRecords: -1, TotalPages: -1 };
var reports;

$(document).ready(function () {
    LoadReports();

    $('#changeFilters').on('click', function () {
        $('#filterModal').modal('show');
    });

    $('#filterNow').on('click', function () {
        searchInformation.Filters = currentFilters;
        searchInformation.Page = 1;
        LoadReports();
    });

    $('#newReport').on('click', function () {
        $('#informationModal .modal-title').html('Creating Report');
        $('#informationModal .modal-body').html('<div class="row"><div class="col-xs-8 col-xs-2">Creating report. You will be redirected when your report is ready.</div></div>');
        $('#informationModal').modal('show');
        $.ajax({
            url: '../RSCloud/NewReport',
            type: "post",
            data: JSON.stringify({ cid: CurrentCompany }),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (jsonResult) {
                if (jsonResult.Success) {
                    window.location = "../RSCloud/EditReport?cid=" + CurrentCompany + "&rid=" + jsonResult.UId;
                }
                else {
                    $('#informationModal .modal-title').html('Creating Report: Error');
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + jsonResult.Message + '</div></div>');
                }
            },
            error: function (result) {
                $('#informationModal .modal-title').html('Creating Report: Error');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">Internal Server Error</div></div>');
            }
        });

    });

});

function LoadReports() {
    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">Loading Data...</h3></div></div>');
    var information = '';
    var data = searchInformation;
    $.ajax({
        url: '../RSCloud/GetReports',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                reports = jsonResult.Nodes;
                searchInformation = jsonResult;
                filters = [];
                while (currentFilters.length > 0) {
                    currentFilters.pop();
                }
                if (searchInformation.Filters !== null) {
                    for (var i = 0; i < searchInformation.Filters.length; i++) {
                        currentFilters.push(searchInformation.Filters[i]);
                    }
                }
                if (reports.length < 0) return;
                $('#page').html('');
                for (var o = 1; o <= searchInformation.TotalPages; o++) {
                    $('#page').append('<option value="' + o + '">' + o + '</option>');
                }
                $('#page').val('' + searchInformation.Page);
                if (reports.length == 0) {
                    $('#mainHolder > section').html('<div class="row"><div class="col-md-12"><h3 class="text-center">No Reports Match The Criteria</h3></div></div>');
                    return;
                }
                //Lets build the table
                information += '<div class="table-responsive table-responsive-medium"><table class="table table-striped table-reports-list"><thead class="dataHeader"><tr>';
                information += '<th>Name</th><th>Date Created</th>'
                information += '</tr></thead><tbody>';
                for (var i = 0; i < reports.length; i++) {
                    var date = new Date(Date.parse(reports[i].DateCreated));
                    information += '<tr><td><a href="EditReport?rid=' + reports[i].UId + '&cid=' + CurrentCompany + '">' + reports[i].Name + '</a></td><td>' + date.toLocaleString() + '</td></tr>';
                }
                information += '</tbody></table></div>';
                $('#mainHolder > section').html(information);
                SetUpFilters();
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