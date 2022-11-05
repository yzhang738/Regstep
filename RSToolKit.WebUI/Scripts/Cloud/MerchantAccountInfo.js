$(document).ready(function () {
    $('#Descriminator').on('change', function (event) {
        var t_select = $(this);
        if (t_select.val() === 'ipay') {
            $('#iPayParameters').show('fast');
        } else {
            $('#iPayParameters').hide('fast');
        }
    });
    $('#iPayEncryptionType').on('change', function (event) {
        var t_select = $(this);
        if (t_select.val() === '0') {
            $('.iPayKey').hide('fast');
        } else if (t_select.val() === '1') {
            $('.iPayKey').show('fast');
        } else if (t_select.val() === '2' || t_select.val() === '3' || t_select.val() === '4') {
            $('#iPayKey1').show('fast');
            $('#iPayKey2').hide('fast');
            $('#iPayKey3').hide('fast');
        }
    });
});