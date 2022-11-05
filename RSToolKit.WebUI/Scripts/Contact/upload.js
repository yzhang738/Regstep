/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    $('#upload').on('submit', function (e) {
        e.preventDefault();
        var data = new FormData(this);
        toolkit.addJsonAntiForgeryToken(data);
        var xhr = new XMLHttpRequest();
        xhr.open('put', $(this).attr('action'), true);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError();
        };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (result.success) {
                    window.location.href = result.postBack;
                } else {
                    if (result.retry) {
                        return retry(result.postBack, result.token);
                    } else {
                        window.location.href = result.postBack;
                    }
                }
            } else {
                RESTFUL.showError(result.message);
            }
            result = null;
        };
        processing.showPleaseWait();
        xhr.send(data);
        xhr = null;
    });

    function retry(postBack) {
        var xhr = new XMLHttpRequest();
        xhr.open('get', postBack, true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError();
        };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (result.success) {
                    window.location.href = result.postBack;
                } else {
                    if (result.retry) {
                        processing.update(result.message, (result.progress * 100).toFixed(2), result.progress);
                        return retry(result.postBack, result.token);
                    } else {
                        RESTFUL.showError(result.message);
                    }
                }
            } else {
                RESTFUL.showError(result.message);
            }
        };
        xhr.send();
        xhr = null;
    }
}());