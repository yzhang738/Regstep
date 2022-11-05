/// <reference path="prettyProcessing.js" />
/// <reference path="globalVariables.js" />
/// <reference path="processing.js" />
/// <reference path="globalVariables.js" />

/* global EXT */
/* jshint unused: false*/

var RESTUFL_VERSION = 'unknown';
var RESTFUL = {
    xhrError: function (event, message) {
        /// <signature>
        /// <summary>Default error function for XMLHttpRequest.onError</summary>
        /// <param name="event" type="object">The event parameter of the onError delegate.</param>
        /// <param name="message" type="string">The message to display</param>
        /// </signature>
        if (typeof (message) === 'undefined') {
            message = 'Unhandled server error.';
        }
        prettyProcessing.hidePleaseWait();
        alert(message);
    },
    parse: function (xhr) {
        /// <signature>
        /// <summary>Gets the response text parsed out into a JavaScript object.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest object.</param>
        /// <returns type="Object">Returns an object parsed from the response text.</param>
        /// </signature>
        if (/^application\/json/i.test(xhr.getResponseHeader('content-type'))) {
            return JSON.parse(xhr.responseText);
        }
        if (/^application\/xml/i.test(xhr.getResponseHeader('content-type'))) {
            return XMLSerializer.parse(xhr.responseText);
        }
    },
    jsonHeader: function (xhr) {
        /// <signature>
        /// <summary>Sets a json header.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest object.</param>
        /// </signature>
        xhr.setRequestHeader('Content-Type', 'application/json');
    },
    ajaxHeader: function (xhr) {
        /// <signature>
        /// <summary>Sets an Ajax header.</summary>
        /// <param name="xhr" type="XMLHttpRequest">The XMLHttpRequest object.</param>
        /// </signature>
        xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
    },
    showError: function (message, title) {
        /// <signature>
        /// <summary>Displays a message in a modal.</summary>
        /// <param name="message" type="string">The message to display</param>
        /// <param name="title" type="string">The title to display</param>
        /// </signature>
        prettyProcessing.hidePleaseWait();
        message = message || 'An unhandled server error occured.';
        title = title || 'Server Error';
        var t_messageModal = $(restful_html_object.SHOWMESSAGE);
        $('body').append(t_messageModal);
        t_messageModal.find('.restful-showmessage-title').html(title);
        t_messageModal.find('.restful-showmessage-body').html(message);
        t_messageModal.on('hidden.bs.modal', function () {
            t_messageModal.remove();
        });
        t_messageModal.modal('show');
    },
    showConfirmation: function (message, title, yesFn, noFn, yesBtn, noBtn) {
        /// <signature>
        /// <summary>Displays a message in a modal.</summary>
        /// <param name="message" type="String">The message to display</param>
        /// <param name="title" type="String">The title to display</param>
        /// <param name="yesFn" type="Function">The function to run when yes is clicked.</param>
        /// <param name="noFn" type="Function">The function to run when no is clicked.</param>
        /// <param name="yesBtn" type="String">The label for the yes button.</param>
        /// <param name="noBtn" type="String">The label for the no button.</param>
        /// </signature>
        prettyProcessing.hidePleaseWait();
        message = message || 'Do you wish to continue?';
        title = title || 'Continue';
        yesBtn = yesBtn || 'Yes';
        noBtn = noBtn || 'No';
        var t_messageModal = $(restful_html_object.CONFIRMMESSAGE);
        $('body').append(t_messageModal);
        t_messageModal.find('.restful-showmessage-title').html(title);
        t_messageModal.find('.restful-showmessage-body').html(message);
        t_messageModal.find('.yes-btn').text(yesBtn).on('click', function (e) {
            e.preventDefault();
            t_messageModal.modal('hide');
            return true;
            yesFn();
        });
        t_messageModal.find('.no-btn').text(noBtn).on('click', function (e) {
            e.preventDefault();
            t_messageModal.modal('hide');
            return true;
            noFn();
        });;
        t_messageModal.on('hidden.bs.modal', function () {
            t_messageModal.remove();
        });
        t_messageModal.modal('show');
    },
    bind: function (selector) {
        $(selector).on('click', function (e) {
            RESTFUL.Click(e, $(this));
        });
    }
};

var restful_html_object = {};
restful_html_object.SHOWMESSAGE = '<div class="modal fade restful-showmessage" data-backdrop="static" data-keyboard="false"><div class="modal-dialog"><div class="modal-header"><h3 class="restful-showmessage-title"></h3></div><div class="modal-body"><div class="row restful-showmessage-body"></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">OK</button></div></div></div>';
restful_html_object.CONFIRMMESSAGE = '<div class="modal fade restful-showmessage" data-backdrop="static" data-keyboard="false"><div class="modal-dialog"><div class="modal-header"><h3 class="restful-showmessage-title"></h3></div><div class="modal-body"><div class="row restful-showmessage-body"></div></div><div class="modal-footer"><button type="button" class="btn btn-default no-btn"></button><button type="button" class="btn btn-default yes-btn"></button></div></div></div>';

$(document).ready(function () {

    RESTFUL_VERSION = '1.2.1.1';

    if (typeof (jQuery) === 'undefined') {
        throw 'jQuery is required for restful.js';
    }
    
    if (typeof (prettyProcessing) === 'undefined') {
        throw 'prettyProcessing is requred for restful.js';
    }

    $('a[data-xhr-method]').on('click', function (e) {
        RESTFUL.Click(e, $(this));
    });

    $('form[data-xhr-method]').on('submit', function (e) {
        // First we need to prevent the form from submitting by default.
        e.preventDefault();
        var validate_result = $(this).triggerHandler('form.validate');
        if (typeof (validate_result) === 'undefined') {
            validate_result = true;
        } else {
            validate_result = validate_result.trim().toLowerCase() === 'true';
        }
        if (!validate_result) {
            return;
        }
        if (typeof (UpdateCKEDITOR) !== 'undefined') {
            UpdateCKEDITOR();
        }

        // We need to grab the form being submitted as a jquery object.
        var form = $(this);

        // Now we see if the form is contained inside a modal.
        var t_modal = form.closest('.modal');
        if (t_modal.length !== 0) {
            // The form is contained inside a modal.  We need to hide the modal now for the processing modal.
            t_modal.modal('hide');
        }

        // Show the processing modal.
            prettyProcessing.showPleaseWait();

        var t_method = form.attr('data-xhr-method');
        if (typeof (t_method) === 'undefined' || t_method === null || !t_method) {
            // If no method was supplied, we use the default method of the form tag.
            t_method = form.attr('method');
            if (typeof (t_method) === 'undefined' || t_method === null || !t_method) {
                // If the form method was not supplied we use a post.
                t_method = 'post';
            }
        }
        t_method = t_method.toLowerCase();

        // Now we grab information from the form tag.
        var t_uri = form.attr('action');
        var t_location = form.attr('data-xhr-oncomplete');
        var t_confirm = form.attr('data-xhr-confirm');
        var t_failmessage = form.attr('data-xhr-failmessage');
        if (typeof (t_failmessage) === 'undefined' || t_failmessage === null || !t_failmessage) {
            t_failmessage = "Unhandled Server Error";
        }

        // Now we build the form data.
        var t_data = new FormData(form[0]);

        // Now we add the antiforgery token.
        AddJsonAntiForgeryToken(t_data);
        var t_forceConfirm = false;
        // If deleting or a confirm message is present we ask for confirmation and cancel submission if not confirmed.
        if (typeof (t_confirm) !== 'undefined' || t_method === 'delete') {
            t_forceConfirm = true;
        }
        if (t_forceConfirm) {
            if (typeof (t_confirm) === 'undefined' || t_confirm === null || !t_confirm) {
                t_confirm = 'Are you sure you want to delete. This cannot be undone.';
            }
            if (!confirm(t_confirm)) {
                return;
            }
        }

        // We set the onComplete to Inherit initially.
        var onComplete = redirect.INHERIT;
        // Now we grab the oncomplete from the form tag.
        if (typeof (t_location) === 'undefined' || t_location === null || !t_location) {
            // If one doesn't exist, we go with inherit location.
            t_location = 'inherit';
        }
        // Now we set the onComplete from the form tag onComplete.
        switch (t_location.toLowerCase()) {
            case redirect.REFRESH:
                onComplete = redirect.REFRESH;
                break;
            case redirect.NOTHING:
                onComplete = redirect.NOTHING;
                break;
            case redirect.INHERIT:
                onComplete = redirect.INHERIT;
                break;
            default:
                onComplete = redirect.CUSTOM;
                break;
        }

        // We create the XML request object.
        var t_xhr = new XMLHttpRequest();
        // Open the connection.
        t_xhr.open(t_method, t_uri, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onload = function (event) {
            // We grab the current target.
            var c_xhr = event.currentTarget;
            //Check to see if it was succesfully recieved.
            if (event.currentTarget.status === 200) {
                // Parse the json response.
                var result = JSON.parse(c_xhr.responseText);
                // Check if server responed with success.
                if (result.Success) {
                    // Check if a location was sent back.
                    if (typeof (result.Location) === 'undefined' || result.Location === null || result.Location === '') {
                        // No location so we set it to blank.
                        result.Location = '';
                    }
                    // Action onComplete.
                    switch (onComplete) {
                        case redirect.INHERIT:
                            // Do what the server tells us to.
                            switch (result.Location.toLowerCase()) {
                                case redirect.REFRESH:
                                    window.location.reload();
                                    break;
                                case redirect.NOTHING:
                                    break;
                                default:
                                    // If a location is present we redirect;
                                    if (result.Location !== '') {
                                        window.location.href = result.Location;
                                    }
                            }
                            break;
                        case redirect.REFRESH:
                            window.location.reload();
                            break;
                        case redirect.NOTHING:
                            break;
                        case redirect.CUSTOM:
                            window.location.href = t_location;
                    }
                    // Hide the processing modal.
                    prettyProcessing.showPleaseWait();
                } else {
                    // Not successful. We hide the modal and show the fail message;
                    prettyProcessing.hidePleaseWait();
                    var t_fail = result.Message;
                    alert(t_fail);
                }
            } else {
                // Data not recieved so we hide the processing and show the fail message.
                prettyProcessing.hidePleaseWait();
                alert(t_failmessage);
            }
            // Now we check if we need to clear the formdata.
            var clear = form.attr('data-xhr-clearform');
            if (typeof (clear) !== 'undefined' && clear) {
                // We need to clear all inputs and selects.
                form.find('input').each(function () {
                    var input = $(this);
                    var type = input.attr('type');
                    if (typeof (type) === 'undefined' || type !== 'hidden') {
                        $(this).val('');
                    }
                });
                form.find('select').val('');
            }
        };

        t_xhr.onerror = function () {
            // There was an error and we hide the processing modal and show the fail message.
            prettyProcessing.hidePleaseWait();
            alert(t_failmessage);
        };

        // We send the data.
        t_xhr.send(t_data);
    });

    function UpdateCKEDITOR() {
        // Now we need to see if CKEDITOR is being used.
        /* jshint ignore:start */
        if (typeof (CKEDITOR) !== 'undefined') {
            for (var instance in CKEDITOR.instances) {
                CKEDITOR.instances[instance].updateElement();
            }
        }
        /* jshint ignore:end */

    }

});

var redirect = {};
redirect.INHERIT = 'inherit';
redirect.REFRESH = 'refresh';
redirect.NOTHING = 'nothing';
redirect.CUSTOM = 'custom';
redirect.SHOWMESSAGE = 'showmessage';

RESTFUL.Click = function (e, link) {
    // Prevent the anchor tag from doing it's default action.
    e.preventDefault();

    if (typeof (UpdateCKEDITOR) !== 'undefined') {
        UpdateCKEDITOR();
    }

    // Check if the anchor tag is part of a modal.  If it is we hide the modal.
    var t_modal = link.closest('.modal');
    var t_modalKeep_temp = link.attr('data-xhr-modalkeep');
    t_modalKeep = false;
    if (typeof (t_modalKeep_temp) !== 'undefined') {
        t_modalKeep = t_modalKeep_temp === 'true';
    }
    if (t_modal.length !== 0 && !t_modalKeep) {
        t_modal.modal('hide');
    }

    // We grab the desired method.
    var t_method = link.attr('data-xhr-method');

    // Set the default method if one is not supplied.
    if (typeof (t_method) === 'undefined') {
        t_method = 'get';
    }
    t_method = t_method.toLowerCase();

    // We grab the uri from the anchor.
    var t_uri = link.attr('href');

    // No we grab the options from the anchor tag as a json object.
    var t_data = {};
    if (typeof (link.attr('data-xhr-options')) !== 'undefined') {
        t_data = JSON.parse(link.attr('data-xhr-options'));
    }

    // Grab the oncomplete action.
    var t_location = link.attr('data-xhr-oncomplete');
    // Grab the confirm message.
    var t_confirm = link.attr('data-xhr-confirm');
    // Grab the fail message or sets a default on if there isn't one present.
    var t_failmessage = link.attr('data-xhr-failmessage');
    if (typeof (t_failmessage) === 'undefined' || t_failmessage === null || !t_failmessage) {
        t_failmessage = "Unhandled Server Error";
    }

    // Add the antiforgery token
    AddJsonAntiForgeryToken(t_data);

    // See if we need to show a confirmation message. If we do and the user cancels, we return and do nothing.
    var t_forceConfirm = false;
    if (typeof (t_confirm) !== 'undefined' || t_method === 'delete') {
        t_forceConfirm = true;
    }
    if (t_forceConfirm) {
        if (typeof (t_confirm) === 'undefined') {
            t_confirm = 'Are you sure you want to delete. This cannot be undone.';
        }
        if (!confirm(t_confirm)) {
            return;
        }
    }

    // Show the processing modal.
    prettyProcessing.showPleaseWait();
    var onComplete = redirect.INHERIT;
    if (typeof (t_location) === 'undefined') {
        t_location = 'inherit';
    }
    switch (t_location.toLowerCase()) {
        case redirect.REFRESH:
            onComplete = redirect.REFRESH;
            break;
        case redirect.NOTHING:
            onComplete = redirect.NOTHING;
            break;
        case redirect.INHERIT:
            onComplete = redirect.INHERIT;
            break;
        case redirect.SHOWMESSAGE:
            onComplete = redirect.SHOWMESSAGE;
            break;
        default:
            onComplete = redirect.CUSTOM;
            break;
    }

    // Create the request object.
    var t_xhr = new XMLHttpRequest();
    t_xhr.open(t_method, t_uri, true);
    // Set the content as json.
    RESTFUL.ajaxHeader(t_xhr);
    t_xhr.setRequestHeader('Content-Type', 'application/json');
    t_xhr.onload = function (event) {
        // Get the current request.
        var c_xhr = event.currentTarget;
        // Check if the request was successful.
        if (event.currentTarget.status === 200) {
            // Request successful.
            // Parse the response as a json.
            var result = RESTFUL.parse(event.currentTarget);
            if (result.Success) {
                // Action successful from server.
                // Check if a location was sent back. If not we set a default one.
                if (typeof (result.Location) === 'undefined' || result.Location === null || result.Location === '') {
                    result.Location = '';
                }
                if (onComplete === redirect.INHERIT) {
                    if (result.Location === redirect.SHOWMESSAGE) {
                        onComplete = redirect.SHOWMESSAGE;
                    } else if (result.Location === redirect.REFRESH) {
                        onComplete = redirect.REFRESH;
                    } else if (result.Location === redirect.NOTHING) {
                        onComplete = redirect.NOTHING;
                    }
                }
                // Perform onComplete action.
                switch (onComplete) {
                    case redirect.INHERIT:
                        if (result.Location !== '') {
                            window.location.href = result.Location;
                        }
                        break;
                    case redirect.SHOWMESSAGE:
                        RESTFUL.showError(result.SuccessMessage, result.SuccessMessageTitle);
                        break;
                    case redirect.REFRESH:
                        window.location.reload();
                        break;
                    case redirect.NOTHING:
                        break;
                    case redirect.CUSTOM:
                        window.location.href = t_location;
                }

                // Hide processing modal.
                prettyProcessing.hidePleaseWait();
            } else {
                // There was an error. We hide the processing modal and show the message.
                prettyProcessing.hidePleaseWait();
                var t_fail = result.Message;
                RESTFUL.showError(t_fail, 'Error Message');
            }
        } else if (event.currentTarget.status === 400) {
            RESTFUL.showError(event.currentTarget.responseBody, "Bad Request");
        } else {
            prettyProcessing.hidePleaseWait();
            RESTFUL.showError(t_failmessage, 'Error Message');
        }
    };
    t_xhr.onerror = function () {
        prettyProcessing.hidePleaseWait();
        RESTFUL.showError(t_failmessage, 'Error Message');
    };
    t_xhr.send(JSON.stringify(t_data));
};

function AddJsonAntiForgeryToken (data) {
    /// <signature>
    /// <summary>Adds the antiforgery token to an object.</summary>
    /// <param name="data" type="object">The object (or FormData) to append the antiforgery token to.</param>
    /// <returns type="object">Returns the object passed.</returns>
    /// </signature>
    /// <signature>
    /// <summary>Adds the antiforgery token to a FormData object.</summary>
    /// <param name="data" type="FormData">The FormData object to append the antiforgery token to.</param>
    /// <returns type="FormData">Returns the FormData object passed.</returns>
    /// </signature>
    var _jsonAntiForgeryToken = '';
    if (typeof (jsonAntiForgeryToken) === 'undefined') {
        _jsonAntiForgeryToken = toolkit.jsonAntiForgeryToken;
    }
    if (EXT.getName(data) === 'FormData') {
        data.append('__RequestVerificationToken', _jsonAntiForgeryToken);
    } else {
        data.__RequestVerificationToken = _jsonAntiForgeryToken;
    }
    return data;
}