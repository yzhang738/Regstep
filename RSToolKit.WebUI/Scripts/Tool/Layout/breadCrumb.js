/* global RESTFUL */
/* global AddJsonAntiForgeryToken */
/* jshint unused:false */

/// <reference path="restful.js" />

var BREADCRUMB = {
    VERSION : '1.0.0.0',
    CURRENT: null
};

$(document).ready(function () {
    "user strict";
    var crumbId = $('#breadcrumb_currentcrumb').val();
    if (typeof (crumbId) !== 'undefined' && crumbId !== null) {
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('get', window.location.origin + '/Account/Crumb/' + crumbId, true);
        t_xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        t_xhr.onerror = function () { };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = null;
                if (/^application\/json/i.test(t_xhr.getResponseHeader('content-type'))) {
                    result = JSON.parse(t_xhr.responseText);
                } else if (/^application\/xml/i.test(t_xhr.getResponseHeader('content-type'))) {
                    result = XMLSerializer.parse(t_xhr.responseText);
                }
                if (result !== null && result.Success) {
                    BREADCRUMB.CURRENT = result.Crumb;
                    $('#breadcrumb_currentcrumb').trigger('updated.breadcrumb');
                }
            }
        };
        t_xhr.send();
    }
});

function Crumb() {
    /// <signature>
    /// <summary>Holds breadcrumb information for the system</summary>
    /// <returns type="Crumb" />
    /// <field name="Id" type="String">The id of the breadcrumb.</field>
    /// <field name="ActionDate" type="Date">The date of the crumb.</field>
    /// <field name="Action" type="String">The action that was invoked.</field>
    /// <field name="Controller" type="String">The controller that was invoked.</field>
    /// <field name="Parameters" type="Object">The query parameter string used.</field>
    /// <field name="Label" type="String">The label to use for the link.</field>
    "use strict";
    this.Id = '';
    this.ActionDate = new Date();
    this.Action = 'Action';
    this.Label = 'Action';
    this.Controller = 'Controller';
    this.Parameters = {};
}

function UpdateCurrentCrumb(label, controller, action, parameters) {
    if (BREADCRUMB.CURRENT == null) {
        setTimeout(function () {
            UpdateCurrentCrumb(label, controller, action, parameters)
        }, 10000);
    } else {
        if (typeof (label) !== 'undefined' && label !== null) {
            BREADCRUMB.CURRENT.Label = label;
        }
        if (typeof (controller) !== 'undefined' && controller !== null) {
            BREADCRUMB.CURRENT.Controller = controller;
        }
        if (typeof (action) !== 'undefined' && action !== null) {
            BREADCRUMB.CURRENT.Action = action;
        }
        if (typeof (parameters) !== 'undefined' && parameters !== null) {
            BREADCRUMB.CURRENT.parameters = parameters;
        }
        //UpdateCrumb(BREADCRUMB.CURRENT);
    }
}

function UpdateCrumb(crumb) {
    $('#crumb_' + crumb.Id).html(crumb.Label);
    var t_xhr = new XMLHttpRequest();
    t_xhr.open('put', window.location.origin + '/Account/Crumb');
    t_xhr.onerror = function () {
    };
    t_xhr.onload = function (event) {
        try {
            if (event.currentTarget.status === 200) {
                var result = RESTFUL.parse(event.currentTarget);
                if (result.Success) {
                    $('#crumb_' + crumb.Id).attr('href', result.href);
                }
            }
        } catch (exc) { }
    };
    RESTFUL.ajaxHeader(t_xhr);
    RESTFUL.jsonHeader(t_xhr);
    try {
        t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken(crumb)));
    } catch (exc) { }
}