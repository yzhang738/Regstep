$(document).ready(function () {
    var registrantKey = $('#RegistrantKey').val();
    $('#codeEnter').on('click', function () {
        $.ajax({
            url: "../../Register/AddPromotionCode/",
            data: { codeEntered: $('#code').val(), registrantKey: registrantKey },
            type: "POST",
            dataType: "json",
            traditional: true,
            success: function (result) {
                if (result.Success) {
                    $('#codes').append('<tr><td>' + $('#code').val() + '</td><td>' + result.Message + '</td><td class="fill"></td></tr>');
                } else {
                    alert(result.Message);
                }
                $('#code').val('');
            },
            error: function (result) {
                $('#code').val('');
                alert('FAILED');
            }
        });
    });
});