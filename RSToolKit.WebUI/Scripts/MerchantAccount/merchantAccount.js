/// <reference path="../toolkit/b_toolkit.intellisense.js" />
/// <reference path="../toolkit/prettyProcessing.js" />
(function () {
    var form = $('#form');
    form.on('submit', function (e) {
        processing.showPleaseWait('Saving', 'Saving Merchant Account');
        e.preventDefault();
        var xhr = new XMLHttpRequest();
        xhr.open('put', window.location.origin + '/MerchantAccount/Put', true);
        RESTFUL.ajaxHeader(xhr);
        var data = new FormData(this);
        toolkit.addJsonAntiForgeryToken(data);
        xhr.onerror = function () {
            RESTFUL.showError();
        };
        xhr.onload = function () {
            var result = RESTFUL.parse(this);
            if (this.status === 200) {
                if (!result.success) {
                    RESTFUL.showError(result.message, 'Cannot Save');
                }
            } else if (this.status === 500) {
                RESTFUL.showError(result.message, 'Error');
            }
            processing.hidePleaseWait();
        }
        xhr.send(data);
        xhr = null;
        data = null;
    });

    form.find('#Descriminator').on('change', function (e) {
        var value = $('#Descriminator').val();
        if (value === 'ipay') {
            $('#parameters').html(ipayDiv);
            $('#usernameLabel').html('Company Number:');
            $('#passwordLabel').html('Pin:');
        } else if (value === 'payeezy') {
            $('#parameters').html(payeezyDiv);
            $('#usernameLabel').html('Merchant Reference:');
            $('#passwordLabel').html('Api Key:');
        } else {
            $('#parameters').html('');
            $('#usernameLabel').html('Username:');
            $('#passwordLabel').html('Password:');
        }
    });

    var ipayDiv = '<div class="row"><div class="col-lg-12"><label class="control-label"><input type="checkbox" class="checkbox-inline" value="True" name="Parameters[0].Value"/> Use Secure Socket</label><input type="hidden" name="Parameters[0].Value" value="False" /><input type="hidden" name="Parameters[0].Key" value="secure" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Encryption Type:</label><select class="form-control" name="Parameters[1].Value" id="iPayEncryptionType"><option value="0">None</option><option value="1">Triple DES</option><option value="2">Rijndael</option><option value="3">RC2</option><option value="4">DES</option></select><input type="hidden" name="Parameters[1].Key" value="encryption" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical iPayKey"><div class="col-lg-12"><label class="control-label">Encryption Key 1:</label><input type="text" class="form-control" name="Parameters[2].Value" /><input type="hidden" name="Parameters[2].Key" value="key1" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical iPayKey"><div class="col-lg-12"><label class="control-label">Encryption Key 2:</label><input type="text" class="form-control" name="Parameters[3].Value" /><input type="hidden" name="Parameters[3].Key" value="key2" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical iPayKey"><div class="col-lg-12"><label class="control-label">Encryption Key 3:</label><input type="text" class="form-control" name="Parameters[4].Value" /><input type="hidden" name="Parameters[4].Key" value="key3" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Currency Indicator:</label><input type="text" class="form-control" name="Parameters[5].Value" /><input type="hidden" name="Parameters[5].Key" value="currencyindicator" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Terminal Id:</label><input type="text" class="form-control" name="Parameters[6].Value" /><input type="hidden" name="Parameters[6].Key" value="terminalid" /></div></div>';
    ipayDiv += '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Amex Specific Company Number:</label><input type="text" class="form-control" name="Parameters[7].Value"><input type="hidden" name="Parameters[7].Key" value="amexcompanynumber" /></div></div>'

    var payeezyDiv = '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Currency Code:</label><input type="text" class="form-control" name="Parameters[0].Value" /><input type="hidden" name="Parameters[0].Key" value="currencycode" /></div></div>';
    payeezyDiv += '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Api Secret:</label><input type="text" class="form-control" name="Parameters[1].Value"><input type="hidden" name="Parameters[1].Key" value="apisecret" /></div></div>'
    payeezyDiv += '<div class="row add-padding-vertical"><div class="col-lg-12"><label class="control-label">Token:</label><input type="text" class="form-control" name="Parameters[2].Value"><input type="hidden" name="Parameters[2].Key" value="token" /></div></div>'

}());