$(document).ready(function () {
    $('.matched-column').on('change', function () {
        var id = $(this).attr('data-header');
        var selectedHeader = $(this).val();
        for (var i = 0; i < data.Headers.length; i++) {
            if (data.Headers[i].Header == id) {
                data.Headers[i].RecomendedMatch = selectedHeader;
                data.Headers[i].Matched = true;
                break;
            }
        }
    });

    $('#rectify').on('click', function () {
        $.ajax({
            url: '../RSCloud/RectifyContactListUpload',
            type: "post",
            data: JSON.stringify(data),
            contentType: "application/json",
            dataType: "json",
            traditional: "true",
            success: function (result) {
                if (result.Success) {
                    $('#informationModal .modal-title').html('Rectification Successful');
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">File Successfully Uploaded.</div></div><div class="row"><div class="col-sm-10 col-sm-offset-1"><a href="../RSCloud/CustomerTable?cid=' + CurrentCompany + '">Return To Contact List</a></div></div>');
                    setTimeout(function () { ClearEmailListError() }, 1500);
                } else {
                    $('#informationModal .modal-title').html('Rectification Error');
                    $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">' + result.Message + '</div><div class="row"><div class="col-sm-10 col-sm-offset-1"><p class="text-center"><a href="RectifyContactListUploadError?cid=' + CurrentCompany + '&ulid=' + result.UId + '">Rectify Errors</a></p></div>');
                }
                $('#informationModal').modal('show');
            },
            error: function (result) {
                $('#informationModal .modal-title').html('Internal Server Error');
                $('#informationModal .modal-body').html('<div class="row"><div class="col-sm-10 col-sm-offset-1">internal Server Error</div><div class="row"><div class="col-sm-10 col-sm-offset-1"><p class="text-center"><a href="RectifyContactListUploadError?cid=' + CurrentCompany + '&ulid=' + result.UId + '">Rectify Errors</a></p></div>');
                $('#informationModal').modal('show');
            }
        });
    });
});