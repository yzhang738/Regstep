/// <reference path="../Tool/Layout/restful.js" />
/// <reference path="../Tool/Layout/prettyProcessing.js" />
/// <reference path="../Tool/Layout/browserGap.js" />
$(document).ready(function () {
    var id = $('#transactionId').val();
    $('#refund').on('click', function () {
        $('#ammountToRefundError').html('');
        var data = {};
        data.id = id;
        data.amount = $('#amountToRefund').val();
        if (isNaN(data.amount)) {
            $('#amountToRefundError').html('You must supply a valid number.');
            return;
        }
        $('#partialRefund').modal('hide');
        prettyProcessing.showPleaseWait('Refunding Transaction', 'Please wait while transaction is refunded.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Transaction/RefundAmount', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unhandled server error occured.', 'Server Error');
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.message, 'Transaction Error');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(RESTFUL.parse(this), 'Server Error');
            } else {
                RESTFUL.showError();
            }
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));
        xhr = null;
        data = null;
    });
    $('#refundBalance').on('click', function (e) {
        e.preventDefault();
        RESTFUL.showConfirmation('Are you sure you wish to refund the entire balance? This cannot be undone.', 'Continue', refundBalance);
    });
    function refundBalance() {
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/Transaction/RefundBalance', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unhandled server error occured.', 'Server Error');
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.message, 'Transaction Error');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(RESTFUL.parse(this), 'Server Error');
            } else {
                RESTFUL.showError();
            }
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ id: id })));
        xhr = null;
    }
});