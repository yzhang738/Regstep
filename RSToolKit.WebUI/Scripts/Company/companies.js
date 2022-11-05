/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    $('#newCompany').on('click', function () {
        $('#newCompanyInputs').show('slow');
    });
    $('#addCompany').on('click', function (e) {
        e.preventDefault();
        var c_name = $('#companyName').val().trim();
        if (c_name === '') {
            $('#newCompanyDiv').addClass('has-error');
            return;
        }
        /*XHR REGION*/
        // Add the antiforgery token
        var t_data = { name: c_name };
        toolkit.addJsonAntiForgeryToken(t_data);

        // Show the processing modal.
        processing.showPleaseWait();
        // Create the request object.
        var t_xhr = new XMLHttpRequest();
        t_xhr.open("post", window.location.origin + '/Company/New', true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        // Set the content as json.
        t_xhr.onload = function (event) {
            // Check if the request was successful.
            if (this.status == 200) {
                // Request successful.
                // Parse the response as a json.
                var result = JSON.parse(this.responseText);
                if (result.success) {
                    // Action successful from server.
                    window.location.href = result.location.getURI();
                    // Hide processing modal.
                } else {
                    // There was an error. We hide the processing modal and show the message.
                    processing.hidePleaseWait();
                    var t_fail = result.Message;
                    RESTFUL.showError(t_fail, 'Error Message');
                }
            } else {
                RESTFUL.showError(result.message);
            }
        };
        t_xhr.onerror = function () {
            processing.hidePleaseWait();
            RESTFUL.showError();
        };
        t_xhr.send(JSON.stringify(t_data));
        t_xhr = null;
    });
}());
