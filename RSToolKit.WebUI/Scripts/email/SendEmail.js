/// <reference path="../datetimepicker.js" />

$(document).ready(function () {
    $('#SendTime').datetimepicker({
        changeMonth: true,
        changeYear: true,
        yearRange: '-50:+50'
    });
});

function SendNow() {
    $('#SendTime').val("06/06/1944 00:00:00");
    document.forms["form"].submit();
}