/// <reference path="../toolkit/b_toolkit.intellisense.js" />
(function () {
    $('.form-process').each(function (i) {
        var table = $(this);
        var id = table.attr('id');
        getFormData(id);
    });
    var xhr = new XMLHttpRequest();
    xhr.open('get', window.location.origin + '/Cloud/Notifications', true);
    RESTFUL.ajaxHeader(xhr);
    xhr.onerror = function () { RESTFUL.showError('Notifications are unavailable.', 'Notifications Unavailable') };
    xhr.onload = function () {
        var result = RESTFUL.parse(this);
        if (this.status === 200) {
            if (!result.success) {
                RESTFUL.showError(result.message);
                return;
            }
            for (var n_i = 0; n_i < result.notifications.length; n_i++) {
                var notification = result.notifications[n_i];
                $('#notifications').append('<tr><td class="cloud-dashboard-icon-cell"><span class="glyphicon glyphicon-arrow-right text-color-1"></span></td><td><a href="' + notification.Url + '">' + notification.Message + '</a></td></tr>');
            }
            if (result.notifications.length == 0) {
                $('#notifications').html('<tr><td colspan="2">No Notifications</td></tr>');
            }
        } else {
            RESTFUL.showError(result.message);
        }
    };
    xhr.send();
    xhr = null;
    function getFormData(id) {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Cloud/FormStatistics/' + id, true);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError();
        };
        xhr.onload = function () {
            if (this.status === 200) {
                var data = RESTFUL.parse(this);
                if (data.success) {
                    fillFormData(data);
                    return;
                }
            }
            fillFormData({id: id});
        }
        xhr.send();
        xhr = null;
    }
    function fillFormData(data) {
        var error = '<span class="glyphicon glyphicon-remove"></span>'
        if (!('registrations' in data)) {
            data.registrations = error;
            data.activeRegistrations = error;
            data.cancelledRegistrations = error;
            data.sales = error;
            data.outstanding = error;
            data.adjustments = error;
            data.invitationOpens = error;
            data.invitationSends = error;
            data.invitationBounces = error;
            data.collected = error;
        }
        var table = $('#' + data.id);
        table.find('.registrations>td:nth-child(2)').html(data.registrations);
        table.find('.registrationsActive>td:nth-child(2)').html(data.activeRegistrations);
        table.find('.registrationsCancelled>td:nth-child(2)').html(data.cancelledRegistrations);
        table.find('.sales>td:nth-child(2)').html(data.sales);
        table.find('.collected>td:nth-child(2)').html(data.collected);
        table.find('.outstanding>td:nth-child(2)').html(data.outstanding);
        table.find('.adjustments>td:nth-child(2)').html(data.adjustments);
        table.find('.invitationOpens>td:nth-child(2)').html(data.invitationOpens);
        table.find('.invitationSends>td:nth-child(2)').html(data.invitationSends);
        table.find('.invitationBounces>td:nth-child(2)').html(data.invitationBounces);
    }
}());