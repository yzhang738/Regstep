$(document).ready(function () {
    $('#refund').on('click', function () {
        processing.showPleaseWait();
        $('#ammountToRefundError').html('');
        var data = {};
        data.id = id;
        data.ammount = $('#ammountToRefund').val();
        if (isNaN(data.ammount))
        {
            $('#ammountToRefundError').html('You must supply a valid number.');
            return;
        }
        $('#partialRefund').modal('hide');
        data.__RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
        $.ajax({
            url: '../../Cloud/RefundAmmount',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                } else {
                    alert(result.Message);
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                alert("The refund failed on the server side.");
            }
        });
    });
});