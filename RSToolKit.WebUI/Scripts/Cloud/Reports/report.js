/*! Report 2.0.1.1 */

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
/// <reference path="../jQuery/Plugins/jsonData.js" />
/// <reference path="../Tool/cycle.js" />
/// <reference path="EmailSendInformation.js" />

/* global TABLE_options*/
/* global prettyProcessing */
/* global EmailSendInformation */
/* global JTableFilter */
/* global JTable */
/* global RESTFUL */
/* global BREADCRUMB */
/* global FindColumn */
/* global ReplaceRow */
/* global ReplaceHeader */
/* global UpdateCrumb */
/* global AddJsonAntiForgeryToken */

var FORMREPORT = {};

$(document).on('ready', function () {
    "use strict";

    var table = new JTable('#jTable');

    FORMREPORT.VERSION = '2.0.1.1';

    //#region Modal Events (Window Resize)
    var height = $(window).innerHeight();
    height *= 0.72;
    $('.modal-fill .modal-body').css('max-height', height + 'px');
    $(window).on('resize', function () {
        var t_height = $(window).innerHeight();
        t_height *= 0.72;
        $('.modal-fill .modal-body').css('max-height', t_height + 'px');
    });
    //#endregion

    //#region Registrant Information
    var registrant = {};
    var changeSetTable = null;
    var currentTransaction = 0;
    var oldData = { id: null, regKey: null, value: null, isSecure: false };
    //#endregion

    //#region static filters
    /* quick filter constants */
    var filter_status_submitted = new JTableFilter();
    filter_status_submitted.ActingOn = 'status';
    filter_status_submitted.Value = '1';
    filter_status_submitted.Test = '==';
    var filter_status_notIncomplete = new JTableFilter();
    filter_status_notIncomplete.ActingOn = 'status';
    filter_status_notIncomplete.Value = '0';
    filter_status_notIncomplete.Test = '!=';
    var filter_status_notCanceled1 = new JTableFilter();
    filter_status_notCanceled1.ActingOn = 'status';
    filter_status_notCanceled1.Value = '2';
    filter_status_notCanceled1.Test = '!=';
    var filter_status_notCanceled2 = new JTableFilter();
    filter_status_notCanceled2.ActingOn = 'status';
    filter_status_notCanceled2.Value = '3';
    filter_status_notCanceled2.Test = '!=';
    var filter_status_notCanceled3 = new JTableFilter();
    filter_status_notCanceled3.ActingOn = 'status';
    filter_status_notCanceled3.Value = '4';
    filter_status_notCanceled3.Test = '!=';
    var filter_status_notDeleted = new JTableFilter();
    filter_status_notDeleted.ActingOn = 'status';
    filter_status_notDeleted.Value = '5';
    filter_status_notDeleted.Test = '!=';
    /* end quick filter contsants */
    //#endregion

    //#region bindings
    /* bindings */
    $('#checkForUpdates').on('click', function () {
        CheckForUpdates(table);
    });
    $('#getFull').on('click', function () {
        table.Load(window.location.origin + '/Cloud/Report', TABLE_options);
    });
    $('#downloadReport').on('click', function () {
        //var newTable = new JTable();
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/Cloud/Create/Report');
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
            prettyProcessing.hidePleaseWait();
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
                    prettyProcessing.hidePleaseWait();
                }
            } else {
                RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                prettyProcessing.hidePleaseWait();
            }
        };
        prettyProcessing.showPleaseWait('Creating Form Report', 'Creating Report');
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(table.GetAjaxData())));
    });
    $('#printable').on('click', function (e) {
        e.preventDefault();
        table.GetPrintView();
    });
    $('#m_save_button').on('click', function () {
        $('#m_save').modal('hide');
        var t_id = $('#m_save_fileInputId').val();
        var t_name = $('#m_save_fileInput').val();
        $('#m_save_fileInputId').val('');
        $('#m_save_fileInput').val('');
        if (typeof (t_name) === 'undefined' || t_name === null || t_name === '') {
            return;
        }
        prettyProcessing.showPleaseWait('Saving Report', 'Please Wait', 100);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/Report');
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#file_save').removeAttr('style');
                    $('#file_delete').removeAttr('style');
                    table.SavedId = result.Id;
                    table.Name = result.Name;
                    $('.jTable').html(table.Name);
                    $('#file_delete').attr('data-xhr-options', '{"id":"' + table.SavedId + '"}');
                    BREADCRUMB.CURRENT.Label = 'Report: ' + table.Name + ' on ' + table.Parent;
                    BREADCRUMB.CURRENT.Parameters.id = table.SavedId;
                    UpdateCrumb(BREADCRUMB.CURRENT);
                    $('.jTable_standardOnly').hide();
                    table.GetPage();
                    prettyProcessing.hidePleaseWait();
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError();
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        var data = table.GetAjaxData();
        data.SavedId = t_id;
        data.Name = t_name;
        t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));
    });
    $('#file_saveas').on('click', function () {
        showFileModal();
    });
    $('#file_save').on('click', function (e) {
        e.preventDefault();
        if (typeof (table.SavedId) === 'undefined' || table.SavedId === null) {
            return;
        }
        prettyProcessing.showPleaseWait('Saving Report', 'Please Wait', 100);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/Report');
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#file_save').removeAttr('style');
                    $('#file_delete').removeAttr('style');
                    table.SavedId = result.Id;
                    table.Name = result.Name;
                    prettyProcessing.hidePleaseWait();
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError();
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        var data = table.GetAjaxData();
        t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));

    });
    $('#file_load').on('click', function () {
        showFileModal(true);
    });
    $('#file_permissions').on('click', function (e) {
        e.preventDefault();
        if (typeof (table.SavedId) === 'undefined' || table.SavedId === null) {
            return;
        }
        window.location = window.location.origin + '/Security/Permissions/' + table.SavedId;
    });
    $('#formType').on('change', function () {
        table.Filters = [];
        switch ($(this).val()) {
            case 'all':
                table.Name = 'All Registrations';
                break;
            case 'active':
                table.Name = 'Active Registrations';
                table.Filters.push(filter_status_submitted);
                break;
            case 'canceled':
                table.Name = 'Cancelled Registrations';
                var canc_filter1 = new JTableFilter();
                canc_filter1.GroupNext = true;
                canc_filter1.ActingOn = 'status';
                canc_filter1.Order = 1;
                canc_filter1.Test = '==';
                canc_filter1.Link = 'or';
                canc_filter1.Value = '2';
                var canc_filter2 = new JTableFilter();
                canc_filter2.GroupNext = true;
                canc_filter2.ActingOn = 'status';
                canc_filter2.Order = 3;
                canc_filter2.Test = '==';
                canc_filter2.Link = 'or';
                canc_filter2.Value = '3';
                var canc_filter3 = new JTableFilter();
                canc_filter3.GroupNext = false;
                canc_filter3.ActingOn = 'status';
                canc_filter3.Order = 4;
                canc_filter3.Test = '==';
                canc_filter3.Link = 'or';
                canc_filter3.Value = '4';
                table.Filters.push(canc_filter1);
                table.Filters.push(canc_filter2);
                table.Filters.push(canc_filter3);
                break;
            case 'incompletes':
                table.Name = 'Incomplete Registrations';
                var inc_filter = new JTableFilter();
                inc_filter.ActingOn = 'status';
                inc_filter.Value = '0';
                inc_filter.Test = '==';
                inc_filter.Order = 1;
                table.Filters.push(inc_filter);
                break;
            case 'deleted':
                table.Name = 'Deleted Registrations';
                var del_filter = new JTableFilter();
                del_filter.ActingOn = 'status';
                del_filter.Value = '5';
                del_filter.Test = '==';
                del_filter.Order = 1;
                table.Filters.push(del_filter);
                break;
            case 'unbalanced':
                table.Name = 'Unbalanced Accounts';
                var unb_filter = new JTableFilter();
                unb_filter.ActingOn = 'balance';
                unb_filter.Value = '0';
                unb_filter.Test = '!=';
                unb_filter.Order = 1;
                unb_filter.Link = 'none';
                table.Filters.push(unb_filter);
                filter_status_notIncomplete.Order = 2;
                filter_status_notIncomplete.Link = 'and';
                filter_status_notDeleted.Order = 3;
                filter_status_notDeleted.Link = 'and';
                table.Filters.push(filter_status_notIncomplete);
                table.Filters.push(filter_status_notDeleted);
                break;
            case 'refund':
                table.Name = 'Refunds Due';
                var ref_filter = new JTableFilter();
                ref_filter.ActingOn = 'balance';
                ref_filter.Value = '0';
                ref_filter.Test = '<';
                ref_filter.Order = 1;
                table.Filters.push(ref_filter);
                filter_status_notIncomplete.Order = 2;
                filter_status_notIncomplete.Link = 'and';
                filter_status_notDeleted.Order = 3;
                filter_status_notDeleted.Link = 'and';
                table.Filters.push(filter_status_notIncomplete);
                table.Filters.push(filter_status_notDeleted);
                break;
            case 'owed':
                table.Name = 'Balances Due';
                var owe_filter = new JTableFilter();
                owe_filter.ActingOn = 'balance';
                owe_filter.Value = '0';
                owe_filter.Test = '>';
                owe_filter.Order = 1;
                table.Filters.push(owe_filter);
                filter_status_notIncomplete.Order = 2;
                filter_status_notIncomplete.Link = 'and';
                filter_status_notDeleted.Order = 3;
                filter_status_notDeleted.Link = 'and';
                table.Filters.push(filter_status_notIncomplete);
                table.Filters.push(filter_status_notDeleted);
                break;
            case 'rsvpAccept':
                table.Name = 'RSVP Accept';
                var rsvpA_filter = new JTableFilter();
                rsvpA_filter.ActingOn = 'rsvp';
                rsvpA_filter.Value = '1';
                rsvpA_filter.Test = '==';
                rsvpA_filter.Order = 1;
                table.Filters.push(rsvpA_filter);
                filter_status_submitted.Link = 'and';
                filter_status_submitted.Order = 2;
                table.Filters.push(filter_status_submitted);
                break;
            case 'rsvpDecline':
                table.Name = 'RSVP Decline';
                var rsvpD_filter = new JTableFilter();
                rsvpD_filter.ActingOn = 'rsvp';
                rsvpD_filter.Value = '0';
                rsvpD_filter.Test = '==';
                rsvpD_filter.Order = 1;
                table.Filters.push(rsvpD_filter);
                filter_status_submitted.Link = 'and';
                filter_status_submitted.Order = 2;
                table.Filters.push(filter_status_submitted);
                break;
            case 'rsvpCount':
                break;
        }
        table.Filtered = false;
        table.FilterObject.Generate();
        table.GetPage();
    });
    /* end bindings */
    //#endregion

    //#region CSS styling
    $('#file_save').css('color', '#eeeeee');
    $('#file_delete').css('color', '#eeeeee');
    $('#file_permissions').css('color', '#eeeeee');
    //#endregion

    //#region Table definitions
    table.OnGetComplete = function (p_table, container) {
        if (typeof (container) === 'undefined') {
            container = $(p_table.Table);
        }
        BindEditable(container);
        container.find('.balance').on('click', function (e) {
            e.preventDefault();
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
        EmailSendInformation.bind('.email-information', container);
        EmailSendInformation.bindEmailList('.email-sendlist', container);
        container.find('.reg-ajax').on('click', function (e) {
            e.preventDefault();
            registrant = {};
            registrant.id = $(this).attr('data-id');
            loadRegistrant();
        });
        prettyProcessing.hidePleaseWait();
    };
    table.OnUpdateComplete = function (tr) {
        tr.find('.reg-ajax').on('click', function (e) {
            e.preventDefault();
            registrant = {};
            registrant.id = $(this).attr('data-id');
            loadRegistrant();
        });
    };
    table.AfterLoad = function (p_table) {
        if (BREADCRUMB.CURRENT !== null) {
            UpdateBreadCrumb(p_table);
        } else {
            setTimeout(function () { UpdateBreadCrumb(p_table); }, 5000);
        }
        if (table.SavedId !== null && typeof (table.SavedId) !== 'undefined') {
            $('#file_save').css('color', '');
            $('#file_delete').css('color', '');
            $('#file_permissions').css('color', '');
            $('#file_delete').attr('data-xhr-options', '{"id":"' + table.SavedId + '"}');
        }
        setTimeout(function () { CheckForUpdates(table); }, 10000);
    };
    table.Load(window.location.origin + '/Cloud/Report', TABLE_options);
    //#endregion

    //#region Table Functions
    /* functions */
    function showFileModal(load) {
        if (typeof (load) === 'undefined' || load === null) {
            load = false;
        }
        $('#m_save_files').html('Loading Files');
        prettyProcessing.showPleaseWait('Retrieving Report', 'Please Wait');
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('get', window.location.origin + '/Cloud/Reports/' + table.Id);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    var saveDiv = $('#m_save_files');
                    saveDiv.html('');
                    for (var i = 0; i < result.Files.length; i++) {
                        var t_file = result.Files[i];
                        saveDiv.append('<div class="report-file" data-id="' + t_file.Id + '">' + t_file.Name + '</div>');
                    }
                    if (!load) {
                        $('.report-file').on('click', function () {
                            $('#m_save_fileInputId').val($(this).attr('data-id'));
                            $('#m_save_fileInput').val($(this).html());
                        });
                        $('#m_save_fileInput').parent().show();
                    } else {
                        $('.report-file').on('click', function () {
                            LoadTable($(this).attr('data-id'));
                            $('m_save').modal('hide');
                        });
                        $('#m_save_fileInput').parent().hide();
                    }
                    $('#m_save').modal('show');
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError();
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        t_xhr.send();
    }
    function LoadTable(id) {
        $('#m_save').modal('hide');
        table.Load(window.location.origin + '/Cloud/Report', { id: id });
    }
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
                if (t_index === -1) {
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
                            if (t_value[i] === t_value[j]) {
                                continue;
                            }
                            var t_item2 = $('#' + t_value[j]);
                            var bEnd = moment(t_item2.attr('data-agenda-end'));
                            if ((bEnd.isAfter(aStart) || bEnd.isSame(aStart)) && (bEnd.isBefore(aEnd) || bEnd.isSame(aEnd))) {
                                collision = true;
                            }
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
                var t_index2 = t_value.indexOf(input.val());
                if (t_index2 !== -1) {
                    t_value.splice(t_index2, 1);
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

        $('.uploaded-image').each(function () {
            $.ajax({
                type: 'Get',
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
            if ($(this).hasClass('file')) {
                setTimeout(function () { ShowEditableItem(oldData.id, oldData.regKey); }, 1000);
            } else {
                ShowEditableItem(oldData.id, oldData.regKey);
            }
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
            url: '../../Cloud/EditComponent?key=' + id + '&registrantKey=' + regKey + '&width=' + width,
            type: "get",
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    $('#editModal').find('#editingData').html(result.Html);
                    $('#editModal').find('.editingData').show('fast');
                    $('#editModal').find('#editingProgress').hide('fast');
                    RunBinding();
                } else {
                    $('#editModal').modal('hide');
                    oldData.Id = null;
                    oldData.regKey = null;
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                oldData.Id = null;
                oldData.regKey = null;
                alert('Server Error');
            }
        });
    }
    function CheckForUpdates(p_table) {
        /// <signature>
        /// <summary>Checks for updates.</summary>
        /// <param name="p_table" type="JTable">The jTable to check for updates.</field>
        /// </signature>
        // We need to put together a list of ids and modifcation tokens for each row;
        var modificationCheck = {
            id: p_table.Id,
            savedId: p_table.SavedId,
            options: p_table.Options,
            items: [],
            lastFullCheck: p_table.LastFullCheck.format()
        };
        for (var i = 0; i < p_table.Rows.length; i++) {
            var modificationToken = FindColumn(p_table.Rows[i], 'modificationToken');
            if (modificationToken !== null) {
                modificationCheck.items.push({ id: p_table.Rows[i].Id, token: modificationToken.Value });
            }
        }
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/Cloud/ReportUpdate', true);
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.action === 'expired') {
                    p_table.DeleteStore(p_table);
                    p_table.Load(window.location.origin + '/Cloud/Report', TABLE_options);
                } else {
                    var rowsAdded = false;
                    if (/rows/.test(result.action)) {
                        for (var i = 0; i < result.rows.length; i++) {
                            var row = result.rows[i];
                            rowsAdded = rowsAdded || !ReplaceRow(p_table, row);
                            p_table.UpdateRow(p_table, row);
                            p_table.OnGetComplete(p_table, $('#' + row.Id));
                        }
                    }
                    if (/headers/.test(result.action)) {
                        for (var j = 0; j < result.headers.length; j++) {
                            var header = result.headers[j];
                            ReplaceHeader(p_table, header);
                        }
                        p_table.GetPage();
                    }
                    //p_table.Store(p_table);
                    if (rowsAdded && p_table.FilteredRecords() < p_table.RecordsPerPage) {
                        p_table.Filter();
                        p_table.GetPage();
                    }
                }
            }
            setTimeout(function () { CheckForUpdates(p_table); }, 30000);
        };
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(modificationCheck)));
    }
    function UpdateBreadCrumb(p_table) {
        if (BREADCRUMB.CURRENT === null) {
            setTimeout(function () { UpdateBreadCrumb(p_table); }, 5000);
            return;
        }
        BREADCRUMB.CURRENT.Label = p_table.Name + ' on ' + p_table.Parent;
        if (p_table.SavedId !== null && typeof (p_table.SavedId) !== 'undefined') {
            BREADCRUMB.CURRENT.Parameters.id = p_table.SavedId;
        } else {
            BREADCRUMB.CURRENT.Parameters.id = p_table.Id;
        }
        UpdateCrumb(BREADCRUMB.CURRENT);
    }
    /* end function */
    //#endregion

    //#region Registrant Bindings
    /* Registrant Bindings */
    $('#saveEdit').on('click', function () {
        // Here we make the ajax call to save the data
        $('#editModal').find('.editingData').hide('fast');
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
        var data = { ComponentKey: oldData.id, RegistrantKey: oldData.regKey, Value: $('#editModal').find('[id="' + oldData.id + '"]').val(), Waitlistings: waitlisting };
        // Add the antiforgery token.
        AddJsonAntiForgeryToken(data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/SaveRegistrantData', true);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#' + oldData.regKey).find('td[data-headerid="' + oldData.id + '"]').html(result.NewValue);
                    for (var j = 0; j < table.Rows.length; j++) {
                        if (table.Rows[j].Id === oldData.regKey) {
                            var t_rowFound = false;
                            for (var k = 0; k < table.Rows[j].Columns.length; k++) {
                                if (table.Rows[j].Columns[k].HeaderId === oldData.id) {
                                    t_rowFound = true;
                                    table.Rows[j].Columns[k].Value = result.Value;
                                    table.Rows[j].Columns[k].PrettyValue = result.PrettyValue;
                                }
                            }
                            if (!t_rowFound) {
                                var column = new JTableColumn();
                                column.HeaderId = result.HeaderId;
                                column.PrettyValue = result.PrettyValue;
                                column.Value = result.Value;
                                column.Id = result.Id;
                                column.Editable = true;
                                table.Rows[j].Columns.push(column);
                            }
                            table.UpdateRow(table, table.Rows[j], true);
                        }
                    }
                    $('#editModal').modal('hide');
                } else {
                    $('#editModal .modal-body').find('#editingProgress').hide('fast');
                    $('#editModal .modal-body').find('#editingData').show('fast');
                    var message = '';
                    for (var l = 0; l < result.Errors.length; l++) {
                        if (l !== 0) {
                            message += '<br />';
                        }
                        message += result.Errors[l];
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
    $('#adjustmentDate').datetimepicker();
    $('#adjust').on('click', function () {
        $('.adjust-warning').hide();
        $('.adjustment-error-text').html('');
        var data = {};
        data.id = registrant.id;
        data.ammount = $('#adjustmentAmmount').val();
        data.description = $('#adjustmentDescription').val();
        data.type = $('#type').val();
        data.transactionId = $('#transactionId').val();
        data.transactionDate = $('#adjustmentDate').val();
        if (/payment$/i.test(data.type)) {
            data.ammount *= -1;
        }
        var error = false;
        if (isNaN(data.ammount) || data.ammount === '') {
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
        data.__RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
        prettyProcessing.showPleaseWait();
        $.ajax({
            url: '../../Cloud/Adjustment',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    loadRegistrant();
                } else {
                    alert(result.Message);
                }
                prettyProcessing.hidePleaseWait();
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert("The adjustment failed on the server side.");
            }
        });
    });
    $('#proccessCC').on('click', function () {
        var data = {};
        data.id = registrant.id;
        data.Amount = $('#ccAmount').val();
        data.RegistrantKey = registrant.id;
        data.FormKey = registrant.formKey;
        data.CompanyKey = registrant.companyKey;
        data.CardNumber = $('#ccCardNumber').val();
        data.NameOnCard = $('#ccNameOnCard').val();
        data.ZipCode = $('#ccZipCode').val();
        data.CardCode = $('#ccCardCode').val();
        data.ExpMonth = $('#ccExpMonth').val();
        data.ExpYear = $('#ccExpYear').val();
        data.TransactionType = $('#ccTransactionType').val();
        data.Live = registrant.live;
        data.Address1 = $('#ccAddress1').val();
        data.Address2 = $('#ccAddress2').val();
        data.City = $('#ccCity').val();
        data.State = $('#ccState').val();
        data.Country = $('#ccCountry').val();
        data.Phone = $('#ccPhone').val();
        data.CardType = $('#ccCardType').val();
        var error = false;
        if (isNaN(data.Amount) || data.Amount === '') {
            alert("The amount is invalid.");
            error = true;
        }
        if (error) {
            return;
        }
        $('#processCCModal').modal('hide');
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('post', window.location.origin + '/api/formgateway', true);
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            prettyProcessing.hidePleaseWait();
            var result = JSON.parse(c_xhr.responseText);
            if (c_xhr.status === 200) {
                if (result.Success) {
                    loadRegistrant();
                } else {
                    alert(result.Message);
                }
            } else if (c_xhr.status === 400) {
                alert(result.Message);
            }
        };
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            var json = JSON.parse(result.responseText);
            alert(json.Message);
        };
        prettyProcessing.showPleaseWait();
        t_xhr.send(JSON.stringify(data));
    });
    $('#registrant-activate, #registrant-deactivate, #registrant-delete').on('click', function (e) {
        prettyProcessing.showPleaseWait();
        e.preventDefault();
        var link = $(this);
        var t_data = {};
        if (typeof (link.attr('data-xhr-options')) !== 'undefined') {
            t_data = JSON.parse(link.attr('data-xhr-options'));
        }
        var t_uri = link.attr('href');
        var t_xhr = new XMLHttpRequest();
        var t_method = 'put';
        if (link.is('#registrant-delete')) {
            t_method = 'delete';
        }
        t_xhr.open(t_method, t_uri, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.setRequestHeader('Content-Type', 'application/json');
        t_xhr.onload = function (event) {
            if (event.currentTarget.status === 200) {
                var result = RESTFUL.parse(event.currentTarget);
                if (result.Success) {
                    loadRegistrant();
                } else {
                    var t_fail = result.Message;
                    RESTFUL.showError(t_fail, 'Error Message');
                }
            } else if (event.currentTarget.status === 400) {
                RESTFUL.showError(event.currentTarget.responseBody, "Bad Request");
            } else {
                RESTFUL.showError('Server Error', 'Error Message');
            }
        };
        t_xhr.onerror = function () {
            prettyProcessing.hidePleaseWait();
            RESTFUL.showError('Server Error', 'Error Message');
        };
        t_xhr.send(JSON.stringify(t_data));
    });
    $('#refund').on('click', function () {
        prettyProcessing.showPleaseWait();
        $('#ammountToRefundError').html('');
        var data = {};
        data.id = $('#transaction-id').val();
        data.ammount = $('#ammountToRefund').val();
        if (isNaN(data.ammount)) {
            $('#ammountToRefundError').html('You must supply a valid number.');
            return;
        }
        $('#partialRefund').modal('hide');
        AddJsonAntiForgeryToken(data);
        $.ajax({
            url: '../../Cloud/RefundAmmount',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    loadTransaction();
                } else {
                    prettyProcessing.hidePleaseWait();
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert('The refund failed on the server side.');
            }
        });
    });
    $('.transaction-refundBalance').on('click', function (e) {
        e.preventDefault();
        prettyProcessing.showPleaseWait();
        var data = {};
        data.id = $('#transaction-id').val();
        AddJsonAntiForgeryToken(data);
        $.ajax({
            url: window.location.origin + '/Cloud/RefundBalance',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    loadTransaction();
                } else {
                    prettyProcessing.hidePleaseWait();
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert("The refund failed on the server side.");
            }
        });
    });
    $('#m_registrant-changeset').on('hide.bs.modal', function (e) {
        changeSetTable = null;
    });
    $('#registrant-adjustment').on('click', function (e) {
        e.preventDefault();
        $('.adjustment-payment').hide();
        $('.adjustment-payment-input').val('');
        $('.adjustment-type').val('Adjustment');
        $('#adjustment').modal('show');
    });
    $('#registrant-payment').on('click', function (e) {
        e.preventDefault();
        $('.adjustment-payment').show();
        $('.adjustment-adjustment').hide();
        $('#adjustment').modal('show');
    });
    /* end Registrant Bindings */
    //#endregion

    //#region Registrant Functions
    /* registrant functions */
    function loadRegistrant() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Cloud/Registrant/' + registrant.id, true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                registrant = RESTFUL.parse(c_xhr);
                $('.registrant-cancelled').hide();
                $('.registrant-submitted').hide();
                $('.registrant-deleted').hide();
                $('.registrant-incomplete').hide();
                $('.registrant-notincomplete').show();
                $('.registrant-notcancelled').show();
                $('.registrant-notdeleted').show();
                $('.registrant-notsubmitted').show();
                $('.registrant-name').html(registrant.fullName);
                $('.registrant-confirmation').children('td').eq(1).html(registrant.confirmation);
                $('.registrant-editedBy').children('td').eq(1).html(registrant.editedBy);
                var t_seatings = '';
                for (var t_seating_i = 0; t_seating_i < registrant.seatings.length; t_seating_i++) {
                    var t_seat = registrant.seatings[t_seating_i];
                    t_seatings += '<div class="row">';
                    if (t_seat.seated) {
                        t_seatings += '<div class="col-xs-4">' + t_seat.component + '</div><div class="col-xs-4">Seated</div><div class="col-xs-4">' + t_seat.dateSeated + '</div>';
                    } else {
                        t_seatings += '<div class="col-xs-4">' + t_seat.component + '</div><div class="col-xs-4">Waitlisted</div><div class="col-xs-4">' + t_seat.date + '</div>';
                    }
                    t_seatings += '<div class="row">';
                }
                $('.registrant-seatings').children('td').eq(0).html(t_seatings);
                if (registrant.rsvp == null) {
                    $('.registrant-rsvp').hide();
                } else {
                    $('.registrant-rsvp').show();
                    $('.registrant-rsvp').children('td').eq(1).html(registrant.rsvp);
                }
                if (registrant.status === 0) {
                    $('.registrant-notincomplete').hide();
                    $('.registrant-incomplete').show();
                } else if (registrant.status === 1) {
                    $('.registrant-notsubmitted').hide();
                    $('.registrant-submitted').show();
                } else if (registrant.status > 1 && registrant.status < 5) {
                    $('.registrant-notcancelled').hide();
                    $('.registrant-cancelled').show();
                } else if (registrant.status === 5) {
                    $('.registrant-notdeleted').hide();
                    $('.registrant-deleted').show();
                }
                if (registrant.phone !== null) {
                    $('.registrant-phone').children('td').eq(1).html(registrant.phone);
                    $('.registrant-phone').show();
                } else {
                    $('.registrant-phone').hide();
                }
                if (registrant.email !== null) {
                    $('.registrant-email').children('td').eq(1).html(registrant.email);
                    $('.registrant-email').show();
                } else {
                    $('.registrant-email').hide();
                }
                if (registrant.audience !== null) {
                    $('.registrant-audience').children('td').eq(1).html(registrant.audience);
                    $('.registrant-audience').show();
                } else {
                    $('.registrant-audience').hide();
                }
                $('.registrant-balance').children('td').eq(1).html(registrant.balance);
                $('.registrant-balanceNow').html(registrant.balance);
                if (registrant.dateCreated !== null) {
                    $('.registrant-datecreated').children('td').eq(1).html(registrant.dateCreated);
                    $('.registrant-datecreated').show();
                } else {
                    $('.registrant-datecreated').hide();
                }
                if (registrant.dateModified !== null) {
                    $('.registrant-datemodified').children('td').eq(1).html(registrant.dateModified);
                    $('.registrant-datemodified').show();
                } else {
                    $('.registrant-datemodified').hide();
                }
                if (registrant.statusLabel !== null) {
                    $('.registrant-status').children('td').eq(1).html(registrant.statusLabel);
                    $('.registrant-status').show();
                } else {
                    $('.registrant-status').hide();
                }
                $('.registrant-transactions').html('');
                var f_fullHtml = '';
                for (var f_id = 0; f_id < registrant.finances.length; f_id++) {
                    var finance = registrant.finances[f_id];
                    var t_date = new moment(registrant.finances[f_id].date);
                    var f_html = '<tr><td>' + t_date.format() + '</td>';
                    f_html += '<td>' + finance.label + (finance.voidable && !finance.voided ? ' <a class="text-rsred finance-void" href="' + window.location.origin + '/cloud/voidadjustment?aid=' + finance.id + '" data-id="' + finance.id + '">[void]</a>' : '') + '</td>';
                    f_html += '<td style="text-align: right;">' + (finance.failed || finance.voided ? '<s>' : '') + finance.amount + (finance.failed || finance.voided ? '</s>' : '') + '</td></tr>';
                    f_fullHtml += f_html;
                }
                $('.registrant-transactions').html(f_fullHtml);
                $('.finance-void').on('click', function (event) {
                    event.preventDefault();
                    var fin_xhr = new XMLHttpRequest();
                    fin_xhr.open('post', $(this).attr('href'), true);
                    fin_xhr.onload = function (event) {
                        var result = RESTFUL.parse(event.currentTarget);
                        if (result.Success) {
                            loadRegistrant();
                        } else {
                            alert('Failed to void adjustment.');
                        }
                    };
                    fin_xhr.send(JSON.stringify({ aid: $(this).attr('data-id') }));
                });
                f_fullHtml = '';
                for (var e_id = 0; e_id < registrant.emailActivities.length; e_id++) {
                    var t_date2 = new moment(registrant.emailActivities[e_id].date);
                    var f_html2 = '<tr><td>' + t_date2.format() + '</td>';
                    f_html2 += '<td><a href="#" class="registrant-emailSendInformation" data-id="' + registrant.emailActivities[e_id].id + '">' + registrant.emailActivities[e_id].name + '</a></td>';
                    f_html2 += '<td>' + registrant.emailActivities[e_id].deepestEvent + '</td>';
                    f_html2 += '<tr>';
                    f_fullHtml += f_html2;
                }
                $('.registrant-emailactivity').html(f_fullHtml);
                $('.registrant-confirmationLink').html(window.location.origin + '/Register/ShowConfirmation/' + registrant.id);
                $('.registrant-continueLink').html(window.location.origin + '/Register/ContinueRegistration/' + registrant.id);
                $('.registrant-link-continueLink').attr('href', window.location.origin + '/AdminRegister/Start?formKey=' + registrant.formKey + '&email=' + registrant.email);
                $('.registrant-link-confirmationLink').attr('href', window.location.origin + '/AdminRegister/Confirmation?registrantKey=' + registrant.id + '&changeStatus=false');
                //#region Bindings for Registrant Modal
                $('.registrant-transaction').on('click', function (e) {
                    e.preventDefault();
                    loadTransaction($(this).attr('id'));
                });
                EmailSendInformation.bind('.registrant-emailSendInformation');
                $('.registrant-addOptions').each(function () {
                    var json = $(this).attr('data-xhr-options-template');
                    if (typeof (json) === 'undefined') {
                        return;
                    }
                    json = json.replace(/_UId_/i, registrant.id);
                    $(this).attr('data-xhr-options', json);
                });
                $('.registrant-changeSet').on('click', function (e) {
                    e.preventDefault();
                    $('#registrant-changeSetTable').html('');
                    changeSetTable = new JTable('#registrant-changeSetTable', true);
                    changeSetTable.OnGetComplete = function (p_table) {
                        $(changeSetTable.Table).find('tbody').find('tr').not(':first').children('td').each(function () {
                            var td = $(this);
                            var header = td.attr('data-headerid');
                            if (header === 'email' || header === 'audience' || header === 'confirmation' || header === 'lastmodified' || header === 'modifiedby') {
                                return;
                            }
                            td.addClass('oldData-copy');
                            td.append('<div><a class="copyData-old" href="' + window.location.origin + '/Cloud/RegistrantCopyOldData" data-xhr-modalKeep="true" data-xhr-oncomplete="nothing" data-xhr-method="put" data-xhr-options=\'{"id":"' + td.attr('id') + '"}\'>USE FOR CURRENT</a></div>');
                        });
                        $('.copyData-old').on('click', function (e) {
                            prettyProcessing.showPleaseWait();
                            e.preventDefault();
                            var link = $(this);
                            var t_data = {};
                            if (typeof (link.attr('data-xhr-options')) !== 'undefined') {
                                t_data = JSON.parse(link.attr('data-xhr-options'));
                            }
                            var t_uri = link.attr('href');
                            var t_xhr = new XMLHttpRequest();
                            t_xhr.open('put', t_uri, true);
                            RESTFUL.ajaxHeader(t_xhr);
                            t_xhr.setRequestHeader('Content-Type', 'application/json');
                            t_xhr.onload = function (event) {
                                if (event.currentTarget.status === 200) {
                                    var result = RESTFUL.parse(event.currentTarget);
                                    if (result.Success) {
                                        var t_td = $(t_table.Table).find('[id="' + result.Id + '"]');
                                        t_td.html(result.Value);
                                    } else {
                                        var t_fail = result.Message;
                                        RESTFUL.showError(t_fail, 'Error Message');
                                    }
                                } else if (event.currentTarget.status === 400) {
                                    RESTFUL.showError(event.currentTarget.responseBody, "Bad Request");
                                } else {
                                    RESTFUL.showError('Server Error', 'Error Message');
                                }
                                prettyProcessing.hidePleaseWait();
                            };
                            t_xhr.onerror = function () {
                                prettyProcessing.hidePleaseWait();
                                RESTFUL.showError('Server Error', 'Error Message');
                            };
                            t_xhr.send(JSON.stringify(t_data));
                        });
                    };
                    changeSetTable.Load(window.location.origin + '/Cloud/RegistrantChangeSet', { id: registrant.id });
                    $('#m_registrant-changeset').modal('show');
                });
                //#endregion
                // Show modal now
                $('#m_registrant').modal('show');
            } else {
                RESTFUL.showError('Server Error');
            }
            prettyProcessing.hidePleaseWait();
        };
        prettyProcessing.showPleaseWait();
        xhr.send();
    }
    function loadTransaction(tr_id) {
        if (typeof(tr_id) !== 'undefined' && tr_id !== null) {
            currentTransaction = tr_id;
        }
        prettyProcessing.showPleaseWait();
        var tr_xhr = new XMLHttpRequest();
        $('#transaction-id').val(tr_id);
        tr_xhr.open('get', window.location.origin + '/Cloud/Transaction/' + currentTransaction, true);
        RESTFUL.ajaxHeader(tr_xhr);
        RESTFUL.jsonHeader(tr_xhr);
        tr_xhr.onload = function (event) {
            var c_tr_xhr = event.currentTarget;
            if (c_tr_xhr.status === 200) {
                var result = RESTFUL.parse(c_tr_xhr);
                if (result.Success) {
                    var trans = result.Data;
                    var modal = $('#m_transaction');
                    modal.find('.transaction-addOptions').each(function () {
                        var el = $(this);
                        var t_json = el.attr('data-xhr-options');
                        if (typeof (t_json) === 'undefined') {
                            return;
                        }
                        el.attr('data-xhr-options', t_json.replace(/_UId_/, trans.id));
                    });
                    modal.find('.transaction-refundBalance').attr('data-id', trans.id);

                    var t_tran_date = new moment(trans.date);
                    modal.find('.transaction-date').html(t_tran_date.format());
                    modal.find('.transaction-amount').html(trans.amount);
                    modal.find('.transaction-type').html(trans.type);
                    modal.find('.transaction-lastfour').html(trans.lastFour);
                    modal.find('.transaction-status').html(trans.status);
                    modal.find('.transaction-totalamount').html(trans.prettyBalance);
                    var t_htmlDetails = '';
                    for (var i_det = 0; i_det < trans.details.length; i_det++) {
                        var t_det_date = new moment(trans.details[i_det].date);
                        t_htmlDetails += '<tr><td>' + t_det_date.format() + '</td>';
                        t_htmlDetails += '<td>' + trans.details[i_det].type + '</td>';
                        t_htmlDetails += '<td>' + trans.details[i_det].id + '</td>';
                        t_htmlDetails += '<td>' + trans.details[i_det].status + '</td>';
                        t_htmlDetails += '<td>' + (trans.details[i_det].failed ? '<s>' : '') + trans.details[i_det].amount + (trans.details[i_det].failed ? '</s>' : '') + '</td></tr>';
                        for (var i_cred = 0; i_cred < trans.details[i_det].credits.length; i_cred++) {
                            var t_cred_date = new moment(trans.details[i_det].credits[i_cred].date);
                            t_htmlDetails += '<tr><td>&nbsp;&nbsp;' + t_cred_date.format() + '</td>';
                            t_htmlDetails += '<td>' + trans.details[i_det].credits[i_cred].type + '</td>';
                            t_htmlDetails += '<td>' + trans.details[i_det].credits[i_cred].id + '</td>';
                            t_htmlDetails += '<td>' + trans.details[i_det].credits[i_cred].status + '</td>';
                            t_htmlDetails += '<td>' + (trans.details[i_det].credits[i_cred].failed ? '<s>' : '') + trans.details[i_det].credits[i_cred].amount + (trans.details[i_det].credits[i_cred].failed ? '</s>' : '') + '</td></tr>';
                        }
                    }
                    modal.find('.transaction-details').html(t_htmlDetails);
                    prettyProcessing.hidePleaseWait();
                    if (trans.balance > 0) {
                        $('.transaction.hasBalance').hide();
                    } else {
                        $('.transaction.hasBalance').show();
                    }
                    $('#m_transaction').modal('show');
                } else {
                    $('#transaction-id').val('');
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError(result.Message);
                }
            } else {
                $('#transaction-id').val('');
                prettyProcessing.hidePleaseWait();
                alert("Server error");
            }
        };
        tr_xhr.send();

    }
    /* end registrant functions */
    //#endregion
});