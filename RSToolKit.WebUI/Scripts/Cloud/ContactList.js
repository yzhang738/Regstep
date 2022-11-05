/* Contact List v3.0.0.1      */
/*  Date Created: 20140921    */
/* Date Modified: 20150305    */
/*       Creator: D.J. Enzyme */

/// <reference path="../Tool/breadCrumb.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/jTable.js" />
/// <reference path="../jQuery/Plugins/sortable.js" />
/// <reference path="../browserGap.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />
/// <reference path="../Tool/restful.js" />
/// <reference path="../Bootstrap/Plugins/datetimepicker/bootstrap-datetimepicker.js" />

// restful.js Globals
/* global RESTFUL */

var CONTACTLIST = {};
CONTACTLIST.VERSION = '3.0.0.1';
CONTACTLIST.EditingData = function () {
    /// <signature>
    /// <summary>Holds information on the data being edited.</summary>
    /// <returns type="EditingData" />
    /// <field name="header" type="JTableHeader">The id of the contact header being edited.</field>
    /// <field name="row" type="JTableRow">The row being edited.</field>
    /// <field name="column" type="JTableColumn">The column being edited.</field>
    /// </signature>
    this.row = null;
    this.column = null;
    this.header = null;
};
CONTACTLIST.EditingHeader = function () {
    /// <signature>
    /// <summary>Holds information on the header being edited.</summary>
    /// <returns type="EditingHeader" />
    /// <field name="header" type="JTableHeader">The id of the contact header being edited.</field>
    /// <field name="name" type="String">The name of the header.</field>
    /// <field name="descriminator" type="Number" integer="True">The descriminator.</field>
    /// <field name="descriminatorOptions" type="Object">The options for the descriminator.</field>
    /// </signature>
    this.header = '';
    this.name = '';
    this.descriminator = 6;
    this.descriminatorOptions = {};
};

$(document).on('ready', function () {
    if (typeof (RESTFUL) === 'undefined') {
        throw 'restful.js must be used.';
    }

    if (typeof (jQuery) === 'undefined') {
        throw 'jquery must be used.';
    }

    if (typeof (prettyProcessing) === 'undefined') {
        throw 'prettyProcessing.js must be used.';
    }
    var currentData = new CONTACTLIST.EditingData();
    // Create the table.
    var table = new JTable('#jTable');

    // Set up the edit data inputs.
    $('#i-text_editData').on('input', function (e) {
        $('#i_editData').val($(this).val());
    });
    $('#i-number_editData').on('input', function (e) {
        $('#i_editData').val($(this).val());
    });
    $('#i-money_editData').on('input', function (e) {
        $('#i_editData').val($(this).val());
    });
    $('#i-money_editData').on('input', function (e) {
        $('#i_editData').val($(this).val());
    });
    $('#i-date_editData').datetimepicker({
        format: 'm/d/yyyy',
        linkField: "i_editData",
        linkFormat: "m/d/yyyy",
        type: 'date'
    });
    $('#i-datetime_editData').datetimepicker({
        format: 'm/d/yyyy H:ii P',
        showMeridian: true,
        linkField: "i_editData",
        linkFormat: "m/d/yyyy H:ii P"
    })
    $('#i-time_editData').datetimepicker({
        format: 'H:ii P',
        showMeridian: true,
        pickDate: false,
        linkField: "i_editData",
        linkFormat: "H:ii P",
    });

    $('#a_headers_delete').on('click', function () {
        $('#m_headerDelete').modal('show');
    });
    $('#b_headerDelete').on('click', function (e) {
        $('#m_headerDelete').modal('hide');
        var headerId = $('#header_headerDelete').val();
        if (typeof (headerId) === 'undefined' || headerId === '') {
            return;
        }
        var xhr = new XMLHttpRequest();
        xhr.open('delete', '../../Cloud2/ContactHeader', true);
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function (event) {
            RESTFUL.showError('There was an error deleting the header. 500 Internal server error.', 'Report Creation Error');
            prettyProccessing.hidePleaseWait();
        };
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    var deletedId = result.Id;
                    for (var i = 0; i < table.Headers.length; i++) {
                        if (table.Headers[i].Id == deletedId) {
                            table.Headers.splice(i, 1);
                            break;
                        }
                    }
                    table.GetPage();
                } else {
                    RESTFUL.showError(result.Message, 'Delete Header Error');
                    prettyProcessing.hidePleaseWait();
                }
            } else {
                RESTFUL.showError('There was an error deleting the header.', 'Header Deletion Error');
                prettyProcessing.hidePleaseWait();
            }
        };
        prettyProcessing.showPleaseWait('Deleting Header', 'Deleting Header');
        xhr.send(JSON.stringify({ id: headerId }));
        table.GetPage();
    });

    // Set up adding and removing contacts
    $('#a_action_add').on('click', function (e) {
        e.preventDefault();
        var modal = $('#m_action');
        modal.find('#b_action_add').show();
        modal.find('#b_action_remove').hide();
        modal.modal('show');
    });
    $('#a_action_remove').on('click', function (e) {
        e.preventDefault();
        var modal = $('#m_action');
        modal.find('#b_action_add').hide();
        modal.find('#b_action_remove').show();
        modal.modal('show');
    });
    $('#b_action_add').on('click', function (e) {
        var modal = $('#m_action');
        modal.modal('hide');
        prettyProcessing.showPleaseWait("Adding Contacts", "Adding");
        var t_data = { id: $('#id_action').val() };
        t_data.ids = [];
        $('.jTable_selected:checked').each(function (i) {
            t_data.ids.push($(this).val());
        });
        AddJsonAntiForgeryToken(t_data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('post', window.location.origin + '/Cloud2/SavedListContacts', true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function () { prettyProcessing.hidePleaseWait(); RESTFUL.showError(); }
        t_xhr.onload = function (event) {
            prettyProcessing.hidePleaseWait();
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (!result.Success) {
                    RESTFUL.showError(result.Message, "Adding Contact Error");
                }
            } else {
                RESTFUL.showError();
            }
        };
        t_xhr.send(JSON.stringify(t_data));
    });
    $('#b_action_remove').on('click', function (e) {
        var modal = $('#m_action');
        modal.modal('hide');
        prettyProcessing.showPleaseWait("Removing Contacts", "Adding");
        var t_data = { id: $('#id_action').val() };
        t_data.ids = [];
        $('.jTable_selected:checked').each(function (i) {
            t_data.ids.push($(this).val());
        });
        AddJsonAntiForgeryToken(t_data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('delete', window.location.origin + '/Cloud2/SavedListContacts', true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function () { prettyProcessing.hidePleaseWait(); RESTFUL.showError(); }
        t_xhr.onload = function (event) {
            prettyProcessing.hidePleaseWait();
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (!result.Success) {
                    RESTFUL.showError(result.Message, "Removing Contact Error");
                } else {
                    if (table.SavedId == t_data.id) {
                        table.Load(window.location.origin + '/Cloud2/Contacts', { id: table.SavedId, type: 'email list' });
                    }
                }
            } else {
                RESTFUL.showError();
            }
        };
        t_xhr.send(JSON.stringify(t_data));
    });
    var gbl_type = "SavedList";
    $('#file_createStaticList').on('click', function (e) {
        e.preventDefault();
        gbl_type = "SavedList";
        $('#m_fileSave').modal('show');
    });
    $('#file_createContactList').on('click', function (e) {
        e.preventDefault();
        gbl_type = "ContactList";
        $('#m_fileSave').modal('show');
    });
    $('#b_fileSave').on('click', function (e) {
        prettyProcessing.showPleaseWait('Creating List', 'Creating');
        $(this).closest('.modal').modal('hide');
        var t_data = { name: $('#name_fileSave').val(), type: gbl_type };
        t_data.table = $.extend({}, table);
        t_data.table.Rows = [];
        t_data.table.FilteredRows = [];
        t_data.table.FilterObject = null;
        t_data.table.SortingObject = null;
        t_data.contacts = [];
        if (gbl_type === 'SavedList') {
            for (var ci = 0; ci < table.FilteredRows.length; ci++) {
                t_data.contacts.push(table.FilteredRows[ci].Id);
            }
        }
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('post', window.location.origin + '/Cloud2/EmailList', true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function () { prettyProcessing.hidePleaseWait(); RESTFUL.showError(); };
        t_xhr.onload = function (event) {
            prettyProcessing.hidePleaseWait();
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    table.Load(window.location.origin + '/Cloud2/Contacts', { id: result.Id, type: 'email list' });
                } else {
                    RESTFUL.showError();
                }
            } else {
                RESTFUL.showError();
            }
        };
        AddJsonAntiForgeryToken(t_data);
        t_xhr.send(JSON.stringify(t_data));
    });


    // Contact Header
    $('#addHeader').on('click', function () {
        $(this).closest('.modal').modal('hide');
        prettyProcessing.showPleaseWait();
        $('#newHeader').modal('hide');
        var data = {};
        data.Name = $('#headerName').val();
        data.Descriminator = $('#headerDescriminator').val();
        data.CompanyKey = CompanyKey;
        data.DescriminatorOptions = [];
        data.UId = $('#headerUId').val();
        data.SavedListKey = $('#headerSavedList').val();
        switch (data.Descriminator) {
            case "datetimeoffset":
                data.DescriminatorOptions.push({ Key: 'timezone', Value: $('#headerTimezone').val() });
                break;
            case "decimal":
                data.DescriminatorOptions.push({ Key: 'culture', Value: $('#headerMoney').val() });
                break;
        }

        $.ajax({
            url: '../../Cloud2/ContactHeader',
            type: "post",
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                prettyProcessing.hidePleaseWait();
                if (result.Success) {
                    window.location.reload();
                } else {
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert('Server Error');
            }
        });
    });
    $('#headerDescriminator').on('change', function () {
        var value = $(this).val();
        if (value === '2' || value === '3' || value === '9') {
            $('#timezoneDiv').show();
        } else {
            $('#timezoneDiv').hide();
        }
        if (value === '5') {
            $('#moneyDiv').show();
        } else {
            $('#moneyDiv').hide();
        }
    });
    $('#a_headers_add').on('click', function (e) {
        e.preventDefault();
        $('#m_headerModal').modal('show');
    });


    //Contact
    $('#addContact').on('click', function () {
        $('#newContact').modal('hide');
    });
    $('#a_contact_add').on('click', function (e) {
        e.preventDefault();
        $('#newContact').modal('show');
    });
    $('#a_contact_delete').on('click', function (e) {
        e.preventDefault();
        prettyProcessing.showPleaseWait();
        var t_data = {};
        t_data.ids = [];
        $('.jTable_selected:checked').each(function (i) {
            t_data.ids.push($(this).val());
        });
        AddJsonAntiForgeryToken(t_data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('delete', window.location.origin + '/Cloud2/Contacts', true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function () { prettyProcessing.hidePleaseWait(); RESTFUL.showError(); }
        t_xhr.onload = function (event) {
            prettyProcessing.hidePleaseWait();
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (!result.Success) {
                    RESTFUL.showError(result.Message, "Removing Contact Error");
                } else {
                    window.location.reload();
                }
            } else {
                RESTFUL.showError();
            }
        };
        t_xhr.send(JSON.stringify(t_data));
    });


    // Loading Data
    $('#file_load').on('click', function (e) {
        showFileModal(true);
    });
    function showFileModal(load) {
        if (typeof (load) === 'undefined' || load === null) {
            load = false;
        }
        $('#m_save_files').html('Loading Files');
        prettyProcessing.showPleaseWait("Retrieving Report", "Please Wait");
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('get', window.location.origin + '/Cloud2/EmailLists');
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            c_xhr = event.currentTarget;
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
                        $('.report-file').on('click', function (e) {
                            $('#m_save_fileInputId').val($(this).attr('data-id'));
                            $('#m_save_fileInput').val($(this).html());
                        });
                        $('#m_save_fileInput').parent().show();
                    } else {
                        $('.report-file').on('click', function (e) {
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
        table.Load(window.location.origin + '/Cloud2/Contacts', { id: id });
        $('#id_action option').prop('selected', false);
        $('#id_action option[value="' + id + '"]').prop('selected', true);
        $('#headerSavedList option').prop('selected', false);
        $('#headerSavedList option[value="' + id + '"]').prop('selected', true);
    }

    // Set the event for saving data.
    $('#b_editData').on('click', function (e) {
        var t_data = $.extend({}, currentData);
        t_data.column.Value = $('#i_editData').val();
        $(this).closest('.modal').modal('hide');
        prettyProcessing.showPleaseWait("Saving Contact Data", "Saving");
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud2/ContactData', true);
        RESTFUL.ajaxHeader(t_xhr);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.showError(); };
        t_xhr.onload = function (event) {
            prettyProcessing.hidePleaseWait();
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    t_data.column.Value = result.Value;
                    t_data.column.PrettyValue = result.PrettyValue;
                    t_data.column.Id = result.Id;
                    var td = $('#' + t_data.row.Id + "_" + t_data.header.Id);
                    if (td.length == 0) {
                        var tr = $('#' + t_data.row.Id);
                        td = tr.find('td[data-headerid="' + t_data.header.Id + '"]');
                        td.attr('id', t_data.row.Id + '_' + t_data.header.Id);
                    }
                    td.html(t_data.column.PrettyValue);
                } else {
                    RESTFUL.showError();
                }
            } else {
                RESTFUL.showError();
            }
        };
        AddJsonAntiForgeryToken(t_data);
        t_xhr.send(JSON.stringify(t_data));
    })

    // We need to set up a custom onGetComplete
    table.OnGetComplete = function () {
        $('.jTable_contact-link').each(function (i) {
            $(this).attr('href', window.location.origin + '/Cloud2/Contact/' + $(this).attr('data-id'));
        });
        $('#jTable_selectAll').on('change', function (i) {
            var t_input = $(this);
            $('.jTable_selected').prop('checked', t_input.prop('checked'));
        });
        $('.editable-item').on('click', function () {
            var t_td = $(this);
            var t_tr = $(this).parent('tr');
            var t_id = t_tr.attr('id');
            var t_headerid = t_td.attr('data-headerid');
            currentData.row = FindRow(table, t_id);
            currentData.column = FindColumn(currentData.row, t_headerid);
            if (currentData.column === null) {
                currentData.column = new JTableColumn();
                currentData.column.Id = null;
                currentData.column.HeaderId = t_headerid;
                currentData.column.Editable = true;
                currentData.row.Columns.push(currentData.column);
            }
            var header = FindHeader(table, t_headerid);
            currentData.header = header;
            if (header === null)
                return;
            $('.header-input').hide();
            if (header.Type === 'date') {
                $('.header-date').show();
            } else if (header.Type === 'time') {
                $('.header-time').show();
            } else if (header.Type === 'datetime') {
                $('.header-datetime').show();
            } else if (header.Type === 'number') {
                $('.header-number').show();
            } else if (header.Type === 'money') {
                $('.header-money').show();
            } else if (header.Type === 'itemParent') {
                var t_iP = $('.header-itemparent');
                t_iP.html('');
                var t_iP_html = '';
                for (var index_iP = 0; index_iP < header.PossibleValues.length; index_iP++) {
                    t_iP_html += '<label class="control-label col-sm-12"><input value="' + header.PossibleValues[index_iP].Id + '" type="radio" name="item-single-selection" class=".item-single-selection" /> ' + header.PossibleValues[index_iP].Label + '</label>';
                }
                t_iP.html(t_mS_html);
                $('.item-single-selection').on('change', function (e) {
                    $('#i_editData').val($('.item-single-selection:checked').val());
                });
                t_iP.show();
            } else if (header.Type === 'multipleSelection') {
                var t_mS = $('.header-itemparent');
                t_mS.html('');
                var t_mS_html = '';
                for (var index_mS = 0; index_mS < header.PossibleValues.length; index_mS++) {
                    t_mS_html += '<label class="control-label col-sm-12"><input value="' + header.PossibleValues[index_mS].Id + '" type="checkbox" class=".item-multiple-selection" /> ' + header.PossibleValues[index_mS].Label + '</label>';
                }
                $('.item-multiple-selection').on('change', function (e) {
                    var selected = [];
                    $('.item-multiple-selection:checked').each(function (i) {
                        selected.push($(this).val());
                    });
                    $('#i_editData').val(JSON.stringify(selected));
                });
                t_mS.html(t_mS_html);
                t_mS.show();
            } else {
                $('.header-text').show();
                $('#i-text_editData').val(currentData.column.Value);
            }
            $('#i_editData').val(currentData.column.Value);
            if (currentData.row === null) {
                return;
            }
            var t_modal = $('#m_editData');
            $('#m_editData').modal('show');
        });
        var t_html = '<option value="">Select a Header</option>';
        for (var i = 0; i < table.Headers.length; i++) {
            t_html += '<option value="' + table.Headers[i].Id + '">' + table.Headers[i].Label + '</option>';
        }
        $('#header_headerDelete').html(t_html);
    };

    // Now we need to create a custom GenerateHtmlFunction
    table.GenerateTableHtml = function (table, rows) {
        var t_html = '<thead><tr><td><input type="checkbox" id="jTable_selectAll" /></td>';
        var t_headGroup = '';
        for (var ind = 0; ind < table.Headers.length; ind++) {
            if ((table.Average || table.Graph || table.Count) && table.Headers[ind].Hidden) {
                continue;
            }
            if (table.Headers[ind].Removed) {
                continue;
            }
            var sortedHere = false;
            var ascending = true;
            var group = table.Headers[ind].Group;
            if (typeof (group) !== 'undefined' && group !== null && group !== '') {
                group = '<i>' + group + '</i><br />';
            } else {
                group = '';
            }
            t_html += '<th class="cursor-pointer sortable-header" data-header-id="' + table.Headers[ind].Id + '">' + group + table.Headers[ind].Label + '</th>';
        }
        t_html += '</tr></thead><tbody>';
        for (var i = 0; i < rows.length; i++) {
            t_html += '<tr id="' + rows[i].Id + '"><td><input type="checkbox" class="jTable_selected" value="' + rows[i].Id + '" />';
            for (var j = 0; j < table.Headers.length; j++) {
                if ((table.Average || table.Graph || table.Count) && table.Headers[j].Hidden) {
                    continue;
                }
                if (table.Headers[j].Removed) {
                    continue;
                }
                var editable = table.Headers[j].Editable;
                if (table.Average || table.Graph || table.Count) {
                    editable = false;
                }
                var data = FindColumn(rows[i], table.Headers[j].Id);
                if (typeof (data) !== 'undefined' && data !== null) {
                    var t_value = data.PrettyValue;
                    if (!this.Graph && !this.Count && !this.Average && t_value !== null && t_value.indexOf('<a') !== 0 && t_value.length > 100) {
                        t_value = t_value.substr(0, 100) + "...";
                    }
                    t_html += '<td data-headerid="' + table.Headers[j].Id + '" id="' + data.Id + '" class="' + (editable ? 'editable-item cursor-pointer' : '') + '">' + t_value + '</td>';
                } else {
                    t_html += '<td data-headerid="' + table.Headers[j].Id + '" class="' + (editable ? 'editable-item cursor-pointer' : '') + '"></td>';
                }
            }
            t_html += '</tr>';
        }
        t_html += '</tbody>';
        return t_html;
    }

    // Load the table data into the jTable;
    table.Load(window.location.origin + '/Cloud2/Contacts', TABLE_options);

    // Set up the download link and printable link.
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
        xhr.open('post', '../../Cloud2/Create/Report');
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
                    window.location = window.location.origin + '/Cloud2/Download/Report/' + result.Id;
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
});