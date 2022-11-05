/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    var rForm = $('#registerForm');
    rForm.on('submit', function (e) {
        // First we need to prevent the form from submitting by default.
        e.preventDefault();


        // We need to trigger validation if needed.
        var validate_result = $(this).triggerHandler('form.validate');
        if (typeof (validate_result) === 'undefined') {
            validate_result = true;
        } else {
            validate_result = validate_result.trim().toLowerCase() === 'true';
        }


        if (!validate_result) {
            // Validation failed. We return and don't submit;
            return;
        }

        // We need to grab the form being submitted as a jquery object.
        var form = $(this);

        processing.showPleaseWait("Submitting Page", "The page is being submitted.");


        // Now we grab information from the form tag.
        var t_uri = form.attr('action');

        // Now we build the form data.
        var t_data = new FormData(form[0]);

        // Now we add the antiforgery token.
        toolkit.addJsonAntiForgeryToken(t_data);

        // We create the XML request object.
        var t_xhr = new XMLHttpRequest();
        // Open the connection.
        t_xhr.open('post', t_uri, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onload = function (event) {
            //Check to see if it was succesfully recieved.
            if (this.status === 200) {
                // Parse the json response.
                var result = RESTFUL.parse(this);
                // Check if server responed with success.
                if (result.success) {
                    window.location.href = result.location;
                } else {
                    // Not successful. We hide the modal and show the fail message;
                    processing.hidePleaseWait();
                    for (var e_i = 0; result.errors.length; e_i++) {
                        $('#' + result.errors[e_i].id + '-error').html(result.errors[e_i].message);
                    }
                }
            } else {
                // Data not recieved so we hide the processing and show the fail message.
                processing.hidePleaseWait();
                RESTFUL.showError('An unexpected error occured.', 'Unexpected Error');
            }
        };

        t_xhr.onerror = function () {
            // There was an error and we hide the processing modal and show the fail message.
            processing.hidePleaseWait();
            RESTFUL.showError("Unhandled server error occured.", "Unhandled Server Error");
        };

        // We send the data.
        t_xhr.send(t_data);
        t_xhr = null;
        t_data = null;
    });
}());