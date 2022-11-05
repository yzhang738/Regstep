/*
 *  Email Send Information
 *   Date Created: 2014-12-11
 *  Date Modified: 2014-12-11
 *     Created By: D.J. Enzyme
 */

/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/restful.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />

// restful.js Globals
/* global RESTFUL */

var EmailSendInformation = {};
EmailSendInformation.load = function (id) {
    if (typeof (id) === 'undefined' || id === null) {
        return;
    }
    prettyProcessing.showPleaseWait();
    var t_xhr = new XMLHttpRequest();
    t_xhr.open('get', window.location.origin + '/Email/Events/' + id);
    RESTFUL.ajaxHeader(t_xhr);
    t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
    t_xhr.onload = function (event) {
        c_xhr = event.currentTarget;
        if (c_xhr.status === 200) {
            var result = RESTFUL.parse(c_xhr);
            if (result.Success) {
                var t_height = $(window).height() - 250;
                prettyProcessing.hidePleaseWait();
                var t_options = {};
                t_options.scheduled = false;
                t_options.id = result.Id;
                t_options.recipients = [];
                t_options.recipients.push(result.Recipient);
                var t_html = '';
                if (t_options.id !== null) {
                    t_html += '<div class="col-xs-12"><a class="resend-email btn btn-default" href="' + window.location.origin + '/api/email/send" data-xhr-oncomplete="showmessage" data-xhr-method="post" data-xhr-options=\'' + JSON.stringify(t_options) + '\'>Resend</a></div>';
                }
                t_html += '<div class="col-xs-12"><div class="table-responsive" style="height: ' + t_height + 'px;"><table class="table table-striped">';
                t_html += '<thead><tr><th>Event</th><th>Date</th><th>Note</th><th>Details</th></tr></thead><tbody>';
                for (var i = 0; i < result.Events.length; i++) {
                    var t_note = result.Events[i].Notes;
                    if (result.Events[i].Event === 'Clicked') {
                        var t_json = JSON.parse(t_note);
                        t_note = t_json.url;
                    }
                    var t_email = result.Events[i].Email;
                    switch (result.Events[i].Event) {
                        case 'Attempting to Send':
                        case 'Sending':
                        case 'Delivered':
                            t_email = '';
                            break;
                    }
                    t_html += '<tr><td style="white-space: nowrap;">' + result.Events[i].Event + '</td><td style="white-space: nowrap;">' + moment(result.Events[i].Date).format('YYYY-M-D HH:mm:ss') + '</td><td>' + t_note + '</td><td>' + t_email + '</td></tr>';
                }
                t_html += '</tbody></table></div></div>';
                $('#emailSendInformation').html(t_html);
                RESTFUL.bind('.resend-email');
                $('#modal_emailSendInformation').modal('show');
            } else {
                RESTFUL.showError(result.Message);
            }
        } else {
            RESTFUL.showError('Unhandled Server Error');
        }
    };
    t_xhr.send();
};
EmailSendInformation.bind = function (selector, container) {
    if (typeof (container) === 'undefined') {
        container = $('body');
    }
    container.find(selector).on('click', function (e) {
        e.preventDefault();
        var j_anchor = $(this);
        var t_id = j_anchor.attr('data-id');
        if (typeof (t_id) === 'undefined') {
            return;
        }
        EmailSendInformation.load(t_id);
    });
}
EmailSendInformation.bindEmailList = function (selector, container) {
    if (typeof (container) === 'undefined') {
        container = $('body');
    }
    container.find(selector).on('click', function (e) {
        e.preventDefault();
        var j_anchor = $(this);
        var t_id = j_anchor.attr('data-id');
        if (typeof (t_id) === 'undefined') {
            return;
        }
        EmailSendInformation.loadEmailList(t_id);
    });
}
EmailSendInformation.loadEmailList = function (id) {
    if (typeof (id) === 'undefined' || id === null) {
        return;
    }
    prettyProcessing.showPleaseWait();
    var t_xhr = new XMLHttpRequest();
    t_xhr.open('get', window.location.origin + '/Email/EmailList?id=' + id);
    RESTFUL.ajaxHeader(t_xhr);
    t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
    t_xhr.onload = function (event) {
        c_xhr = event.currentTarget;
        if (c_xhr.status === 200) {
            var result = RESTFUL.parse(c_xhr);
            if (result.Success) {
                var t_height = $(window).height() - 250;
                prettyProcessing.hidePleaseWait();
                var t_options = {};
                t_options.scheduled = false;
                t_options.id = result.Id;
                t_options.recipients = [];
                t_options.recipients.push(result.Recipient);
                var t_html = '';
                if (t_options.id !== null) {
                    t_html += '<div class="row"><div class="col-xs-12 add-margin-bottom-10"><a class="resend-email btn btn-default" href="' + window.location.origin + '/api/email/send" data-xhr-oncomplete="showmessage" data-xhr-method="post" data-xhr-options=\'' + JSON.stringify(t_options) + '\'>Resend</a></div></div>';
                }
                t_html += '<div class="row"><div class="col-xs-12"><div class="panel-group" id="emailList-accordion" role="tabelist" aria-multiselectable="true">';
                for (var i = 0; i < result.Sends.length; i++) {
                    t_html += '<div class="panel panel-default">';
                    t_html += '<div class="panel-heading" role="tab" id="emailSend_' + i + '">';
                    t_html += '<h4 class="panel-title"><a href="#" data-toggle="collapse" data-parent="#acc_emailSend_' + i + '" aria-expanded="true">Send #' + (i + 1) + '</a></h4>';
                    t_html += '</div>';
                    t_html += '<div id="acc_emailSend_' + i + '" class="panel-collapse collapse" role="tabpanel" aria-labelledby="emailSend_' + i + '">';
                    t_html += '<div class="panel-body">';
                    t_html += '<table class="table table-striped"><thead><tr><th>Event</th><th>Date</th><th>Note</th></tr></thead>';
                    t_html += '<tbody>';
                    for (var j = 0; j < result.Sends[i].EmailEvents.length; j++) {
                        var t_note = result.Sends[i].EmailEvents[j].Notes;
                        if (result.Sends[i].EmailEvents[j].Event === 'Clicked') {
                            var t_json = JSON.parse(t_note);
                            t_note = t_json.url;
                        }
                        var t_date = new moment(result.Sends[i].EmailEvents[j].Date);
                        t_html += '<tr>';
                        t_html += '<td style="white-space: nowrap;">' + result.Sends[i].EmailEvents[j].Event + '</td>';
                        t_html += '<td style="white-space: nowrap;">' + t_date.local().format('YYYY/MM/DD H:mm A') + '</td>';
                        t_html += '<td>' + t_note + '</td>';
                        t_html += '</tr>';
                    }
                    t_html += '</tbody></table>';
                    t_html += '</div></div></div>';
                }
                t_html += '</div></div></div>';
                $('#emailListInformation').html(t_html);
                RESTFUL.bind('.resend-email');
                $('#emailListInformation').find('.panel').on('click', function (e) {
                    var $_target = $(e.currentTarget);
                    var $_panelBody = $_target.find(".panel-collapse");
                    $_target.parents('.panel-group').eq(0).find('.panel-collapse.in').each(function (k) {
                        if ($_panelBody !== $(this)) {
                            $(this).collapse('hide');
                        }
                    });
                    if ($_panelBody) {
                        $_panelBody.collapse('toggle');
                    }
                });
                $('#modal_emailListInformation').modal('show');
            } else {
                RESTFUL.showError(result.Message);
            }
        } else {
            RESTFUL.showError('Unhandled Server Error');
        }
    };
    t_xhr.send();
};

$(document).ready(function () {

    if (typeof (RESTFUL) === 'undefined') {
        throw 'restful.js must be used.'
    }

    if (typeof (jQuery) === 'undefined') {
        throw 'jquery must be used.';
    }

    if (typeof (processing) === 'undefined') {
        throw 'processing.js must be used.';
    }

    if (typeof (moment) === 'undefiend') {
        throw 'moment.js must be used.';
    }

    var modalHtml = '<div class="modal fade" id="modal_emailSendInformation"><div class="modal-dialog modal-lg"><div class="modal-header"><button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button><h3 class="modal-title">Email Send Information</h3></div>';
    modalHtml += '<div class="modal-body"><div class="row"><div class="col-sm-12" id="emailSendInformation"></div></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button></div></div></div>';

    $('body').append(modalHtml);

    modalHtml = '<div class="modal fade" id="modal_emailListInformation"><div class="modal-dialog modal-lg"><div class="modal-header"><button type="button" class="close" data-dismiss="modal"><span aria-hidden="true">&times;</span><span class="sr-only">Close</span></button><h3 class="modal-title">Email Sends</h3></div>';
    modalHtml += '<div class="modal-body"><div class="row"><div class="col-sm-12" id="emailListInformation"></div></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Close</button></div></div></div>';

    $('body').append(modalHtml);
});