/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    var currentMerchantAccount = '';
    $('.merchant-delete').on('click', function (e) {
        e.preventDefault();
        currentMerchantAccount = $(this).attr('data-id');
        RESTFUL.showConfirmation('Are you sure you want to delete the merchant account?', 'Delete Merchant', deleteMerchant);
    });

    function deleteMerchant() {
        /// <signature>
        /// <summary>Deletes the item linked to the anchor tag.</summary>
        /// <param name="item" type="jQuery">The anchor tag as jQuery object.</param>
        /// </signature>
        processing.showPleaseWait();
        var xhr = new XMLHttpRequest();
        xhr.open('delete', window.location.origin + '/MerchantAccount/Delete', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError();
        };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.messsage);
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result.message);
            }
            processing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ id: currentMerchantAccount })));
        xhr = null;
    }
}())