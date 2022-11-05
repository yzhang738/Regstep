/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

$(document).ready(function () {

    $('#SavedTrue').hide();
    $('#SavedFalse').hide();

    $('#form').submit(function () {
        var serializedForm = $(this).serialize();
        $.ajax({
            url: '../../EmailBuilder/EditEmailCampaign',
            type: "POST",
            data: serializedForm,
            dataType: "json",
            success: function (result) {
                var jsonResult = JSON.parse(result);
                if (jsonResult.success) {
                    $('#SavedTrue').show(200, "swing");
                    $('#SavedFalse').hide(200, "swing");
                } else {
                    $('#SavedTrue').show(200, "swing");
                    $('#SavedFalse').hide(200, "swing");
                }
                setTimeout(HideAllSavedStatus, 5000, null);
            },
            error: function (result) {
                $('#SavedTrue').hide(200, "swing");
                $('#SavedFalse').show(200, "swing");
            }
        });
        return false;
    });
});

function HideAllSavedStatus() {
    $('#SavedTrue').hide(200, "swing");
    $('#SavedFalse').hide(200, "swing");

}