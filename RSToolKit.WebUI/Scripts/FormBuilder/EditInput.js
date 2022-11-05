$(document).ready(function () {
    $('#Type').on('change', function () {
        var select = $(this).val();
        if (select == "5" || select == "6" || select == "7") {
            $('.filetype').hide();
            $('.datetime').show();
            if (select === '6') {
                $('.datetimepicker').datetimepicker({
                    pickTime: false,
                    pickDate: true
                });
            } else if (select === '7') {
                $('.datetimepicker').datetimepicker({
                    pickDate: false,
                    pickTime: true
                })
            }
        } else if (select == "8") {
            $('.filetype').show();
            $('.datetime').hide();
        } else {
            $('.filetype, .datetime').hide();
        }
        if (select !== '0') {
            $('#ValueType').closest('div').hide('fast');
            $('#Formatting').closest('div').hide('fast');
        } else {
            $('#ValueType').closest('div').show('fast');
            $('#Formatting').closest('div').show('fast');
        }
        if (select !== '0' && select !== '1' && select !== '2') {
            $('#Length').closest('div').hide('fast');
        } else {
            $('#Length').closest('div').show('fast');
        }
    });

    $('.datetimepicker').datetimepicker();
    $('#Type').trigger('change');
});