/// <reference path="../toolkit/b_toolkit.intellisense.js" />

/* 20150610: Andrew Jackson */

/* global popout */
/* global reg_id */

(function () {
    var reg_id = $('#registrant_registrantId').val();
    var reg_uid = $('#registrant_registrantUId').val();
    var form_id = $('#registrant_formId').val();
    var reg_live = $('#registrant_registrantLive').val() === 'True';

    //#region Adjustment & Transactions
    $('#adjustmentDate').datetimepicker({ autoclose: true }).on("dp.change", function (e) {
        $('#adjustmentDate').val(e.date.format('M/D/YYYY hh:mm:00 A Z'));
    });
    $('.adjust-warning').hide();
    $('#adjust').on('click', function (e) {
        e.preventDefault();
        $('.adjust-warning').hide();
        $('.adjustment-error-text').html('');
        var data = {};
        data.id = reg_id;
        data.amount = $('#adjustmentAmount').val();
        data.description = $('#adjustmentDescription').val();
        data.type = $('#type').val();
        data.transactionId = $('#transactionId').val();
        data.transactionDate = $('#adjustmentDate').val();
        if (/payment$/i.test(data.type)) {
            data.amount *= -1;
        }
        var error = false;
        if (isNaN(data.amount) || data.amount === '') {
            $('#ammountError').html('You must supply a valid number.');
            $('.ammount-warning').show();
            error = true;
        }
        if (typeof (data.description) === 'undefined' || data.description === '') {
            $('#descriptionError').html('You must supply a description.');
            $('.description-warning').show();
            error = true;
        }
        if (error) {
            return;
        }
        $('#adjustment').modal('hide');
        prettyProcessing.showPleaseWait('Creating Adjustment', 'The adjustment is being created.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/registrant/adjustment', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unknown error occured.', 'Server Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.message, 'Adjustment Failed');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result, 'RegStep Server Exception');
            } else {
                RESTFUL.showError('An unknown status code occured. ' + this.status, 'Unknown Status Code');
            }
            prettyProcessing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));
        data = null;
        xhr = null;
    });
    $('#registrant_adjustment').on('click', function (e) {
        e.preventDefault();
        $('.adjustment-payment').hide();
        $('.adjustment-payment-input').val('');
        $('.adjustment-type').val('Adjustment');
        $('#adjustment').modal('show');
    });
    $('#registrant_payment').on('click', function (e) {
        e.preventDefault();
        $('.adjustment-payment').show();
        $('.adjustment-adjustment').hide();
        $('#adjustment').modal('show');
    });
    $('.registrant-transaction').on('click', function (e) {
        e.preventDefault();
        var tranWindow = window.open(this.href, '_blank', 'toolbar=now,location=no,menubar=no,scrollbar=yes,resizable=yes,width=1000,height=600,top=100,left=100');
        tranWindow.onunload = function (e) {
            window.location.reload(true);
        };
    });
    $('.void-adjustment').on('click', function (e) {
        var xhr = new XMLHttpRequest();
        xhr.open('delete', window.location.origin + '/Registrant/Adjustment', true);
        xhr.onerror = function () { RESTFUL.showError(); };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                }
            }
            prettyProcessing.hidePleaseWait();
        };
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.send(JSON.stringify(toolkit.addJsonAntiForgeryToken({ id: $(this).attr('data-id') })));
        xhr = null;
        return false;
    });

    EmailSendInformation.bind('.email-information');
    //#endregion

    //#region modify registrant status
    $('#registrant_permadelete').on('click', function (e) {
        var conf = confirm('Are you sure you want to delete the registrant. This cannot be reversed.');
        if (!conf)
            return;
        e.preventDefault();
        prettyProcessing.showPleaseWait('Deleting Registrant', 'Please wait for registrant to be deleted.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('delete', window.location.origin + '/registrant', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unknown error occured.', 'Server Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.close();
                } else {
                    RESTFUL.showError(result.message, 'Registrant Failed to Delete');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result, 'RegStep Server Exception');
            } else {
                RESTFUL.showError('An unknown status code occured. ' + this.status, 'Unknown Status Code');
            }
            prettyProcessing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ id: reg_id })));
        xhr = null;
        link = null;
    });
    $('#registrant_delete').on('click', function (e) {
        var conf = confirm('Are you sure you want to delete the registrant.');
        if (!conf)
            return;
        e.preventDefault();
        prettyProcessing.showPleaseWait('Deleting Registrant', 'Please wait for registrant to be deleted.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('delete', window.location.origin + '/registrant/markregistrant', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unknown error occured.', 'Server Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.close();
                } else {
                    RESTFUL.showError(result.message, 'Registrant Failed to Delete');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result, 'RegStep Server Exception');
            } else {
                RESTFUL.showError('An unknown status code occured. ' + this.status, 'Unknown Status Code');
            }
            prettyProcessing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ id: reg_id })));
        xhr = null;
        link = null;
    });
    $('#registrant_deactivate').on('click', function (e) {
        e.preventDefault();
        prettyProcessing.showPleaseWait('Registrant Action', 'Please wait for registrant to be deactivated.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/registrant/deactivate', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unknown error occured.', 'Server Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.message, 'Registrant Action Failed');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result, 'RegStep Server Exception');
            } else {
                RESTFUL.showError('An unknown status code occured. ' + this.status, 'Unknown Status Code');
            }
            prettyProcessing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ id: reg_id })));
        xhr = null;
        link = null;
    });
    $('#registrant_activate').on('click', function (e) {
        e.preventDefault();
        prettyProcessing.showPleaseWait('Registrant Action', 'Please wait for registrant to be deactivated.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/registrant/activate', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unknown error occured.', 'Server Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.message, 'Registrant Action Failed');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result, 'RegStep Server Exception');
            } else {
                RESTFUL.showError('An unknown status code occured. ' + this.status, 'Unknown Status Code');
            }
            prettyProcessing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken({ id: reg_id })));
        xhr = null;
        link = null;
    });
    //#endregion

    //#region credit card
    $('#proccessCC').on('click', function () {
        var data = {};
        data.id = reg_id;
        data.Amount = $('#ccAmount').val();
        data.RegistrantKey = reg_uid;
        data.FormKey = form_id;
        data.CompanyKey = toolkit.companyId;
        data.CardNumber = $('#ccCardNumber').val();
        data.NameOnCard = $('#ccNameOnCard').val();
        data.ZipCode = $('#ccZipCode').val();
        data.CardCode = $('#ccCardCode').val();
        data.ExpMonth = $('#ccExpMonth').val();
        data.ExpYear = $('#ccExpYear').val();
        data.TransactionType = $('#ccTransactionType').val();
        data.Live = reg_live;
        data.Address1 = $('#ccAddress1').val();
        data.Address2 = $('#ccAddress2').val();
        data.City = $('#ccCity').val();
        data.State = $('#ccState').val();
        data.Country = $('#ccCountry').val();
        data.Phone = $('#ccPhone').val();
        data.CardType = $('#ccCardType').val();
        var error = false;
        if (isNaN(data.Amount) || data.Amount == '') {
            alert("The amount is invalid.");
            error = true;
        }
        if (error)
            return;
        $('#processCCModal').modal('hide');
        prettyProcessing.showPleaseWait('Processing Credit Card', 'Please wait while we process the transaction.', 100);
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/api/formgateway', true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('An unknown error occured.', 'Server Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var result = RESTFUL.parse(this);
                if (result.success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError(result.message, 'Registrant Failed to Delete');
                }
            } else if (this.status === 500 || this.status === 400) {
                RESTFUL.showError(result, 'RegStep Server Exception');
            } else {
                RESTFUL.showError('An unknown status code occured. ' + this.status, 'Unknown Status Code');
            }
            prettyProcessing.hidePleaseWait();
        };
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));
        xhr = null;
        data = null;
    });
    //#endregion
}());