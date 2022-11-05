/* Report
 * Written By:   D.J. Enzyme
 * Date Created: 20141014
 * Version:      1.4.0.0
 */

/// <reference path="../Tool/breadCrumb.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Filters.js" />
/// <reference path="../Tool/jTable.js" />
/// <reference path="../jQuery/Plugins/sortable.js" />
/// <reference path="../browserGap.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />
/// <reference path="../Tool/Sortings.js" />
/// <reference path="../Tool/restful.js" />
/// <reference path="EmailSendInformation.js" />

/* global printUrl */
/* global components */
/* global rawTable */

// restful.js Globals
/* global RESTFUL */

// Filter.js Globals
/* global generateActingInput */
/* global generateValueInput */
/* global tests */
/* global PopulateFilters */
/* global FillFilters */
/* global RunFilters */
/* global ActingOnChange */
/* global DeleteStatement */
/* global filterIndex */

var FORMREPORT_VERSION = null;

$(document).on('ready', function () {

    FORMREPORT_VERSION = '1.4.0.0';
    var oldData = { id: null, regKey: null, value: null, isSecure: false };
    var table = new JTable();

    if (typeof (RESTFUL) === 'undefined') {
        throw 'restful.js must be used.';
    }

    if (typeof (jQuery) === 'undefined') {
        throw 'jquery must be used.';
    }

    if (typeof (prettyProcessing) === 'undefined') {
        throw 'prettyProcessing.js must be used.';
    }

    if (typeof (SORTING_VERSION) === 'undefined') {
        throw 'Sorting.js must be used.';
    }

    if (typeof (FILTER_VERSION) === 'undefined') {
        throw 'Filter.js must be used.';
    }

    if (typeof (BREADCRUMB_VERSION) === 'undefined') {
        throw 'breadCrumb.js must be used.';
    }

    table.OnGetComplete = function () {
        BindEditable();
        $('.balance').on('click', function () {
            prettyProcessing.showPleaseWait('Registrant Invoice', 'Getting Data');
            var xhr = new XMLHttpRequest();
            var tr = $(this).closest('tr');
            xhr.open('get', '../RegistrantInvoice/' + tr.attr('id'));
            xhr.onerror = function () {
                RESTFUL.showError('500 Internal Server Error', 'Registrant Invoice Error');
                prettyProcessing.hidePleaseWait();
            };
            xhr.onload = function (event) {
                var c_xhr = event.currentTarget;
                if (c_xhr.status === 200) {
                    var t_result = RESTFUL.parse(c_xhr);
                    if (t_result.Success) {
                        prettyProcessing.hidePleaseWait();
                        $('#invoiceModal').find('.modal-body').html(t_result.Html);
                        $('#invoiceModal').modal('show');
                    } else {
                        RESTFUL.showError(t_result.Message, 'Registrant Invoice Error');
                        prettyProcessing.hidePleaseWait();
                    }
                } else {
                    RESTFUL.showError('500 Internal Server Error', 'Registrant Invoice Error');
                    prettyProcessing.hidePleaseWait();
                }
            };
            RESTFUL.ajaxHeader(xhr);
            xhr.send();
        });
    };
    table.Load(window.location.origin + '/Cloud/RegistrantChangeSet', TABLE_options);
    table.AfterLoad = function (p_table) {
        BREADCRUMB_CURRENT.Label = p_table.Name + ' on ' + p_table.Parent;
        if (p_table.SavedId !== null && typeof (p_table.SavedId) !== 'undefined') {
            BREADCRUMB_CURRENT.Parameters['id'] = p_table.SavedId;
        }
        UpdateCrumb(BREADCRUMB_CURRENT);
    }

    $('#saveEdit').on('click', function () {
        // Here we make the ajax call to save the data
        $('#editModal').find('#editingData').hide('fast');
        $('#editModal').find('#editingProgress').show('fast');
        // Get the data to send.
        var waitlisting = [];
        var w_index = 0;
        while ($('input[name^="Waitlistings[' + w_index + ']"]').length > 0) {
            var json = {};
            var items = $('input[name^="Waitlistings[' + w_index + ']"]');
            for (var i = 0; i < items.length; i++) {
                var item = $(items[i]);
                if (/Key$/i.test(item.attr('name'))) {
                    json.Key = item.val();
                } else if (item.attr('type') === 'checkbox') {
                    if (item.prop('checked')) {
                        json.Value = true;
                    } else {
                        json.Value = false;
                    }
                }
            }
            w_index++;
            waitlisting.push(json);
        }
        var data = { ComponentKey: oldData.id, RegistrantKey: oldData.regKey, Value: $('#' + oldData.id).val(), Waitlistings: waitlisting };
        // Add the antiforgery token.
        AddJsonAntiForgeryToken(data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', '../../Cloud/SaveRegistrantData', true);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        t_xhr.onload = function (event) {
            c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#' + oldData.regKey).find('td[data-headerid="' + oldData.id + '"]').html(result.NewValue);
                    for (var i = 0; i < table.Rows.length; i++) {
                        if (table.Rows[i].Id === oldData.regKey) {
                            for (var j = 0; j < table.Rows[i].Columns.length; j++) {
                                if (table.Rows[i].Columns[j].HeaderId === oldData.id) {
                                    table.Rows[i].Columns[j].Value = result.NewValue;
                                    table.Rows[i].Columns[j].PrettyValue = result.NewValue;
                                }
                            }
                        }
                    }
                    $('#editModal').modal('hide');
                } else {
                    $('#editModal .modal-body').find('#editingProgress').hide('fast');
                    $('#editModal .modal-body').find('#editingData').show('fast');
                    var message = '';
                    for (var i = 0; i < result.Errors.length; i++) {
                        if (i != 0) {
                            message += '<br />';
                        }
                        message += result.Errors[i];
                    }
                    $('#editModal').find('.form-messagebox').html(message).show('fast');
                }
            } else {
                $('#editModal').modal('hide');
                RESTFUL.showError('Unhandled Server Error');
            }
        };
        t_xhr.send(JSON.stringify(data));
    });
    $('#downloadReport').on('click', function (e) {
        var data = {
            id: table.Id,
            average: $('#averages').prop('checked'),
            filters: table.Filters,
            sortings: table.Sortings,
            graph: $('#graphs').prop('checked')
        };
        var newTable = $.extend({}, table);
        //var newTable = new JTable();
        var xhr = new XMLHttpRequest();
        xhr.open('post', '../Create/Report');
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function (event) {
            RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
            prettyProccessing.hidePleaseWait();
        };
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    window.location = window.location.origin + '/Cloud/Download/Report/' + result.Id;
                } else {
                    RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                    prettyProccessing.hidePleaseWait();
                }
            } else {
                RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                prettyProccessing.hidePleaseWait();
            }
        };
        prettyProcessing.showPleaseWait('Creating Form Report', 'Creating Report');
        newTable.FilteredRows = [];
        newTable.Rows = [];
        newTable.FilterObject = null;
        newTable.SortingObject = null;
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(newTable)));
    });
    $('#printable').on('click', function (e) {
        e.preventDefault();
        table.GetPrintView();
    });

    // Events

    // FUNCTIONS

    function RunBinding() {
        $('.form-component').removeClass('col-sm-6').removeClass('col-md-4').removeClass('col-lg-3').addClass('col-sm-12');
        $('input[data-component-type="datetime"]').datetimepicker();
        
        /* Checkbox Group */
        $('input[type="hidden"][data-component-type="checkboxgroup"]').each(function () {
            var hidden_input = $(this);
            var value = hidden_input.val();
            if (typeof (value) === 'undefined' || value === '') {
                value = "[]";
            }
            var t_value = JSON.parse(value);
            hidden_input.data('value', t_value);
        });
        $('input[type="checkbox"]').on('change', function () {
            var input = $(this);
            if (/Waitlistings/i.test(input.attr('name'))) {
                return;
            }
            var hidden_input = $('#' + input.attr('data-parent'));
            var t_value = hidden_input.data('value');
            if (input.prop('checked')) {
                var accept = true;
                var t_index = t_value.indexOf(input.val());
                if (t_index == -1) {
                    t_value.push(input.val());
                }

                /* Time Exclusion */
                if (hidden_input.attr('data-component-timeexclusion') === 'True') {
                    var collision = false;
                    for (var i = 0; i < t_value.length; i++) {
                        var t_item = $('#' + t_value[i]);
                        var aStart = moment(t_item.attr('data-agenda-start'));
                        var aEnd = moment(t_item.attr('data-agenda-end'));
                        for (var j = 0; j < t_value.length; j++) {
                            if (t_value[i] === t_value[j])
                                continue;
                            var t_item2 = $('#' + t_value[j]);
                            var bStart = moment(t_item2.attr('data-agenda-start'));
                            var bEnd = moment(t_item2.attr('data-agenda-end'));
                            if ((bEnd.isAfter(aStart) || bEnd.isSame(aStart)) && (bEnd.isBefore(aEnd) || bEnd.isSame(aEnd)))
                                collision = true;
                        }
                    }
                    if (collision) {
                        accept = false;
                    }
                }

                if (!accept) {
                    t_value.splice(t_index, 1);
                    input.prop('checked', false);
                    alert('Your selection has time conflictions.');
                }
            } else {
                var t_index = t_value.indexOf(input.val());
                if (t_index != -1) {
                    t_value.splice(t_index, 1);
                }
            }
            hidden_input.data('value', t_value);
            hidden_input.val(JSON.stringify(t_value));
        });
        /* End Checkbox Group */

        /* Radio Group */
        $('input[data-component-type="radiogroup"][type="hidden"]').each(function () {
        });
        /* End Radio Group */

        $('input[type="radio"]').on('change', function () {
            var input = $(this);
            var hidden_input = $('#' + input.attr('data-parent'));
            if (input.prop('checked')) {
                hidden_input.val(input.val());
            }
        });

        $('.uploaded-image').each(function (i) {
            $.ajax({
                type: "Get",
                url: '../../Cloud/RegistrantImageThumbnail/' + $(this).attr('data-form-registrant') + '?component=' + $(this).attr('data-form-component'),
                success: function (data) {
                    $(this).attr('src', data);
                },
            });
        });
    }

    function BindEditable() {
        $('.editable-item').on('click', function () {
            $('#editModal .modal-body').find('#editingProgress').show();
            $('#editModal .modal-body').find('#editingData').hide();
            $('#editModal').modal('show');
            oldData.id = $(this).attr('data-headerid');
            oldData.regKey = $(this).parent('tr').attr('id');
            oldData.value = $(this).html();
            if ($(this).hasClass('file'))
                setTimeout(function () { ShowEditableItem(oldData.id, oldData.regKey); }, 1000);
            else
                ShowEditableItem(oldData.id, oldData.regKey);
        });
        $('a[href="#emailSend"]').on('click', function (e) {
            e.preventDefault();
            var t_id = $(this).attr('data-emailsend-id');
            EmailSendInformation.load(t_id);
        });
    }

    function ShowEditableItem(id, regKey) {
        var width = $('#editModal .modal-dialog').width() - 2 - 30 - 30;
        $.ajax({
            url: '../../Cloud/EditComponent/' + id + '?registrantKey=' + regKey + '&width=' + width,
            type: "get",
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    $('#editModal').find('#editingData').html(result.Html).show('fast');
                    $('#editModal').find('#editingProgress').hide('fast');
                    RunBinding();
                } else {
                    $('#editModal').modal('hide');
                    oldData.Id = null;
                    oldData.regKey = null;
                    alert(result.Message);
                }
            },
            error: function (result) {
                processing.hidePleaseWait();
                oldData.Id = null;
                oldData.regKey = null;
                alert('Server Error');
            }
        });
    }

});