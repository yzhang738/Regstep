$(document).ready(function () {
    var data = { cid: CurrentCompany };
    $.ajax({
        url: '../RSCloud/ActiveForms',
        type: "post",
        data: JSON.stringify(data),
        contentType: "application/json",
        dataType: "json",
        traditional: "true",
        success: function (jsonResult) {
            if (jsonResult.Success) {
                for (var i = 0; i < jsonResult.Forms.length; i++) {
                    var html = '<tr><td><a href="RSCloud/RegData?id=' + jsonResult.Forms[i].Id + '&cid=' + CurrentCompany + '">' + jsonResult.Forms[i].Name + '</a></td>';
                    html += '<td>' + jsonResult.Forms[i].Description + '</td>';
                    var date = new Date(Date.parse(jsonResult.Forms[i].DateString));
                    html += '<td>' + date.toLocaleString() + '</td>';
                    var formStatus = "";
                    switch (jsonResult.Forms[i].Status) {
                        case 0:
                            formStatus = "Developement";
                            break;
                        case 1:
                            formStatus = "Active";
                            break;
                    }
                    html += '<td>' + formStatus + '</td>';
                    html += '</tr>';
                    $('#activeFormsTableBody').append(html);
                }
            }
        },
        error: function (result) {
            $('#loadingError #loadingMessage').html(result.Message);
            $('#loadingError').modal("show");
        }
    });
});
