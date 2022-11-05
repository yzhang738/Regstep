/// <reference path="EmailSendInformation.js" />

$(document).ready(function () {
    $('#adjustmentDate').datetimepicker({
        autoclose: true
    }).on("dp.change", function (e) {
        $('#adjustmentDate').val(e.date.format('M/D/YYYY hh:mm:00 A Z'));
    });
    $('.adjustment-tooltip').each(function () {
        var creator = $(this).attr('data-creator');
        var description = $(this).attr('data-description');
        var transactionId = $(this).attr('data-transactionid');
        $(this).tooltip({
            html: true,
            title: 'Created By: ' + creator + '<br />Description: ' + description + (typeof (transactionId) == 'undefined' ? '' : '<br />Transaction Id: ' + transactionId)
        });
    });
    EmailSendInformation.bind('.email-information');
    $('.adjust-warning').hide();

    $('#adjust').on('click', function () {
        $('.adjust-warning').hide();
        $('.adjustment-error-text').html('');
        var data = {};
        data.id = id;
        data.ammount = $('#adjustmentAmmount').val();
        data.description = $('#adjustmentDescription').val();
        data.type = $('#type').val();
        data.transactionId = $('#transactionId').val();
        data.transactionDate = $('#adjustmentDate').val();
        if (/payment$/i.test(data.type))
            data.ammount *= -1;
        var error = false;
        if (isNaN(data.ammount) || data.ammount == '') {
            $('#ammountError').html('You must supply a valid number.');
            $('.ammount-warning').show();
            error = true;
        }
        if (typeof (data.description) == 'undefined' || data.description == '') {
            $('#descriptionError').html('You must supply a description.');
            $('.description-warning').show();
            error = true;
        }
        if (error)
            return;
        $('#adjustment').modal('hide');
        data.__RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
        processing.showPleaseWait();
        $.ajax({
            url: '../../Cloud/Adjustment',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                } else {
                    alert(result.Message);
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                alert("The adjustment failed on the server side.");
            }
        });
    });
    $('#proccessCC').on('click', function () {
        var data = {};
        data.id = id;
        data.Amount = $('#ccAmount').val();
        data.RegistrantKey = id;
        data.FormKey = form;
        data.CompanyKey = company;
        data.CardNumber = $('#ccCardNumber').val();
        data.NameOnCard = $('#ccNameOnCard').val();
        data.ZipCode = $('#ccZipCode').val();
        data.CardCode = $('#ccCardCode').val();
        data.ExpMonth = $('#ccExpMonth').val();
        data.ExpYear = $('#ccExpYear').val();
        data.TransactionType = $('#ccTransactionType').val();
        data.Live = live;
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
        processing.showPleaseWait();
        $.ajax({
            url: '../../api/FormGateway',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                } else {
                    alert(result.Message);
                    if (typeof (result.Location) == 'undefined' || result.Location === null || result.Location === undefined) {
                        processing.hidePleaseWait();
                    } else if (result.Location == 'refresh') {
                        window.location.reload();
                    } else {
                        window.location.replace(result.Location);
                    }
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                var json = JSON.parse(result.responseText);
                alert(json.Message);
            }
        });
    });
});