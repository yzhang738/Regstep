$(document).on('ready', function () {
    $('form[data-type="xhr"][data-http-method="put"]').on('submit', function (e) {
        $('#uploadStatus').html('Preparing Upload');
        $('#uploadProgressBar').css('width', '0');
        $('#uploadProgress').modal('show');
        e.preventDefault();
        var data = new FormData(document.getElementById('rectifyForm'));
        var xhr = new XMLHttpRequest
        xhr.open("put", $('#rectifyForm').attr('action'), true);
        xhr.onload = function (event) {
            upload_errorCount = 0;
            upload_processing = false;
            upload_updating = false;
            upload_intervalId = '';
            upload_criticalError = false;
            upload_finished = false;
            var result = JSON.parse(xhr.responseText);
            if (result.Success) {
                $('#uploadStatus').html('Processing Complete');
                setTimeout(function () {
                    $('#uploadProgressBar').css('width', '0');
                    $('#uploadStatus').html('Processing Data');
                    upload_intervalId = setInterval(function () {
                        CheckStatus(result.PostBack);
                    }, 500);
                }, 1000)
            } else {
                $('#uploadProgress').modal('hide');
                alert(result.Message);
            }
        };
        xhr.onerror = function () {
            $('#uploadProgress').modal('hide');
            alert('Error rectifying.');
        };
        xhr.send(data);
    });
});

var upload_errorCount = 0;
var upload_processing = false;
var upload_updating = false;
var upload_intervalId = '';
var upload_criticalError = false;
var upload_finished = false;

function CheckStatus(url) {
    if (upload_criticalError)
        return;
    xhrS = new XMLHttpRequest;
    xhrS.open("get", url, true);
    xhrS.onload = function (event) {
        if (upload_criticalError || upload_finished) {
            $('#uploadProgress').modal('hide');
            return;
        }
        var result = JSON.parse(event.currentTarget.responseText);
        if (result.CriticalFailure) {
            upload_criticalError = true;
            $('#uploadProgress').modal('hide');
            alert(result.Message);
        } else if (result.NeedsRectified) {
            upload_criticalError = true;
            window.location.href = result.RectifyLocation;
        } else if (result.NeedsSheetSelection) {
            upload_criticalError = true;
            window.location.href = result.SheetLocation;
        } else if (!result.ProcessingComplete) {
            var percent = Math.ceil(result.ProcessingPercent) + '%';
            if (!upload_processing) {
                $('#uploadStatus').html('Processing Data');
                upload_processing = true;
            }
            $('#uploadProgressBar').css('width', percent);
        } else if (!result.UpdateComplete) {
            var percent = Math.ceil(result.ProcessingPercent) + '%';
            if (!upload_updating) {
                $('#uploadStatus').html('Updating Contacts');
                upload_updating = true;
            }
            $('#uploadProgressBar').css('width', percent);
        } else {
            upload_finished = true;
            clearInterval(upload_intervalId);
            $('#uploadProgressBar').css('width', '100%');
            $('#uploadStatus').html('Completed');
            setTimeout(function () {
                window.location.href = "../../Cloud/ContactList";
            }, 3000);
        }
    }
    xhrS.onerror = function () {
        if (upload_criticalError || upload_finished) {
            $('#uploadProgress').modal('hide');
            return;
        }
        upload_criticalError = true;
        clearInterval(upload_intervalId);
        $('#uploadProgress').modal('hide');
        alert('Error Processing Data.');
    };
    xhrS.send();
}