var processing;
$(document).ready(function () {
    processing = processing || (function () {
        var pleaseWaitDiv = $('#processingModal');
        return {
            showPleaseWait: function () {
                pleaseWaitDiv.modal();
            },
            hidePleaseWait: function () {
                pleaseWaitDiv.modal('hide');
            },

        };
    })();
});
