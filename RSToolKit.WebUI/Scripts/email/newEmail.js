/// <reference path="../Bootstrap/Plugins/processing.js" />
$(document).on('ready', function () {
    $('#createEmail').on('click', function (e) {
        e.preventDefault();
        $('#newEmailModal').modal('hide');
        processing.showPleaseWait();
        var xhr = new XMLHttpRequest();
        xhr.open("post", "../../Email/Email", true);
        xhr.onerror = function () {
            processing.hidePleaseWait();
            alert("Unhandled server error.");
        };
        xhr.onload = function (event) {
            if (event.currentTarget.status == 200) {
                var result = JSON.parse(event.currentTarget.responseText);
                if (result.Success) {
                    window.location.href = result.Location;
                } else {
                    proc.hidePleaseWait();
                    alert(result.Message);
                }
            } else {
                processing.hidePleaseWait();
                alert("The page you tried to reach was not found.");
            }
        };
        xhr.setRequestHeader('Content-Type', 'application/json');
        xhr.send(JSON.stringify({ id: ehid, templateId: $('#emailTemplate').val() }));
    });
});