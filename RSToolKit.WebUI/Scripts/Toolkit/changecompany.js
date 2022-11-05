$(document).ready(function () {
    $('#changeCompany').on('click', function () {
        $('#companySelect').val(CurrentCompany);
        $('#changeCompanyDialog').modal('show');
    });
    $('#selectCompanyButton').on('click', function () {
        var newCompanyId = $('#companySelect > option:selected').val();
        changeCompany(newCompanyId);
    });
});

function changeCompany(newCompanyId) {
    var data = { cid: newCompanyId };
    $.ajax({
        url: '../RSCloud/ChangeCurrentCompany',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                window.location = location.protocol + '//' + location.host + location.pathname;
            }
        }
    });
}