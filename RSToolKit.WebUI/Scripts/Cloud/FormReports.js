/// <reference path="../jquery-2.1.1.intellisense.js" />

$(document).ready(function () {

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
        report.FormType = $(this).val();
        GetData();
    });

    function GetData() {
        processing.showPleaseWait();
        report.Registrants = [];
        $.ajax({
            url: '../../Cloud/GetFormReports',
            type: "get",
            data: report,
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    $.extend(report, result);
                    var html = '';
                    for (var i = 0; i < report.Forms.length; i++) {
                        var date = moment(report.Forms[i].DateCreated);
                        html += '<tr><td><a href="' + window.location.origin + '/Cloud/PagedReport/' + report.Forms[i].Id + '">' + report.Forms[i].Name + '</a></td>';
                        html += '<td>' + report.Forms[i].Registrations + '</td>';
                        html += '<td>' + date.format('MM-DD-YYYY HH:mm A') + '</td></tr>';
                    }
                    $('#forms').html(html);
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
        this.Company = "";
        this.Filters = [];
        this.Sortings = [];
        this.Forms = [];
        this.Page = 1;
        this.PageSize = 15;
        this.Success = true;
        this.FormType = "all";
        this.TotalPages = 1;
    }
});