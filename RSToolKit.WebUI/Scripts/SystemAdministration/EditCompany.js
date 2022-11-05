$(document).on('ready', function () {
    $('#roleKey').on('change', function (e) {
        var val = $(this).val();
        var amount = '';
        for (var i = 0; i < roles.length; i++) {
            if (roles[i].Key == val) {
                amount = roles[i].Amount;
                break;
            }
        }
        $('#amount').val(amount);
    });
});