/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    var registrantKey = $('#RegistrantKey').val();
    $('#codeEnter').on('click', function () {
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/AdminRegister/PromotionCode', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (result.success) {
                    $('#codes').append('<tr><td>' + $('#code').val() + '</td><td>' + result.message + '</td><td class="fill"></td></tr>');
                } else {
                    RESTFUL.showError(result.message, 'Promotion Code Problem');
                }
            } else {
                RESTFUL.showError(result.message, 'Error');
            }
        };
        xhr.onerror = function () {
            RESTFUL.showError('Unhandled server error occured.', 'Unhandled Server Error');
        };
        xhr.send(toolkit.addJsonAntiForgeryToken({ id: registrantKey, code: $('#code').val() }));
    });
}());