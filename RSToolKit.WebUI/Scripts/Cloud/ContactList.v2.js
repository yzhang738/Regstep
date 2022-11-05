/*      Contact List
*
*    Created By: D.J. Enzyme
*  Date Started: 20141013
* Date Complete:
*       Version: 2.0.1.1
*/

/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Filters.js" />

/* global reports */
/* global headers */

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

var CONTACTLIST_VERSION = null;

$(document).on('ready', function () {
    // Initialization

    CONTACTLIST_VERSION = '2.0.1.1';

    var request = new ContactListRequest();
    request.CompanyKey = CompanyKey;

    var editing = {
        contact: new Contact(),
        data: new ContactData(),
        header: new ContactHeader(),
        contactIndex: -1,
        dataIndex: -1,
        td: null,
        tr: null,
    };

    $('#yesOverwriteReport').hide();
    $('#moneyDiv').hide();
    $('#timezoneDiv').hide();

    // Events

    $('#editModal').on('shown.bs.modal', function () {
        $('.editing-value').focus();
        if ($('.editing-value').val() === '') {
            $('.editing-value').select();
        }
    });

    // Table sorting.
    $('.table-sort').on('click', function (e) {
        e.preventDefault();
        var link = $(this);
        var actingon = link.attr('data-actingon');
        if (request.Sorting.ActingOn == actingon) {
            request.Sorting.Descending = !request.Sorting.Descending;
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').toggleClass('glyphicon-sort-by-attributes-alt');
        } else {
            $('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt').removeClass('glyphicon-sort-by-attributes');
            request.Sorting.ActingOn = actingon;
            request.Sorting.Descending = false;
            link.children('.sort-icon').addClass('glyphicon-sort-by-attributes');
            link.children('.sort-icon').removeClass('glyphicon-sort-by-attributes-alt');
        }
        LoadContacts();
    });

    $('#saveEdit').on('click', function () {
        var data = {
            ContactKey: editing.data.UId,
            HeaderKey: editing.data.HeaderKey,
            Value: $('.editing-value').val()
        };
        // Validate the input data.
        var t_error = null;
        switch (editing.header.Descriminator.toLowerCase()) {
            case 'number':
                if (isNaN(data.Value)) {
                    t_error = 'You must enter a valid number.';
                }
                break;
            case 'email':
                if (!(/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(data.Value))) {
                    t_error = 'You must enter a valid email.';
                }
                break;
        }
        if (t_error !== null) {
            $('#editModalError').html(t_error);
            return;
        }
        $('#editModal').modal('hide');
        saveContactData(data, $('#editModal'), $('#editModalError'));
    });
    
    $('#addContact').on('click', function () {
        $('#newContact').modal('hide');
    });

    $('#selectAll').on('change', function () {
        var checked = $(this).is(':checked');
        $('.contact-selected').prop('checked', checked);
    });

    $('#downloadContactReport').on('click', function () {
        request.Filters = PopulateFilters();
        request.Contacts = [];
        window.location = "../../Cloud/DownloadContactReport?rawJson=" + JSON.stringify(request);
    });

    $('#deleteSelected').on('click', function (e) {
        e.preventDefault();
        $('.contact-selected:checked').each(function () {
            var deleteXHR = new XMLHttpRequest();
            var tr = $(this).parent().parent();
            tr.children('td').first().html('<div class="progress" style="background-color: #777"><div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%"></div></div>');
            var id = tr.attr('id');
            deleteXHR.open('delete', '../../Cloud/Contact', true);
            deleteXHR.setRequestHeader('Content-Type', 'application/json');
            deleteXHR.onerror = function () {
                processing.hidePleaseWait();
                alert('An unhandled error occurred.');
            };
            deleteXHR.onload = function (event) {
                var result = JSON.parse(event.currentTarget.responseText);
                if (result.Success) {
                    $('#' + result.UId).remove();
                } else {
                    $('#' + result.UId).children('td').first().html('<input type="checkbox" class="contact-selected" />');
                }
            };
            deleteXHR.send(JSON.stringify(AddJsonAntiForgeryToken({ id: id })));
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

    $('#addHeader').on('click', function () {
        processing.showPleaseWait();
        $('#newHeader').modal('hide');
        var data = {};
        data.Name = $('#headerName').val();
        data.Descriminator = $('#headerDescriminator').val();
        data.CompanyKey = CompanyKey;
        data.DescriminatorOptions = [];
        data.UId = $('#headerUId').val();
        switch (data.Descriminator) {
            case "datetimeoffset":
                data.DescriminatorOptions.push({ Key: 'timezone', Value: $('#headerTimezone').val() });
                break;
            case "decimal":
                data.DescriminatorOptions.push({ Key: 'culture', Value: $('#headerMoney').val() });
                break;
        }

        $.ajax({
            url: '../../Cloud/ContactHeader',
            type: "post",
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    window.location.reload();
                } else {
                    alert(result.Message);
                }
                processing.hidePleaseWait();
            },
            error: function () {
                alert('Server Error');
                processing.hidePleaseWait();
            }
        });
    });

    $('#overwriteReport').on('change', function () {
        var item = $(this);
        if (item.is(':checked')) {
            $('#noOverwriteReport').hide();
            $('#yesOverwriteReport').show();
        } else {
            $('#yesOverwriteReport').hide();
            $('#noOverwriteReport').show();
        }
    });

    $('#setFilters').on('click', function (e) {
        e.preventDefault();
        $('#filterModal').modal('hide');
        LoadContacts();
    });

    $('#reports').on('change', function () {
        var index = $(this).find('option:selected').attr('data-index');
        FillFilters(reports[index].Filters);
        $('#noOverwriteReport').hide();
        $('#yesOverwriteReport').show();
        $('#overwriteReport').prop('checked', true);
        $('#reportSelect').val(reports[index].UId);
    });

    $('#pageSize').on('blur', function () {
        var records = $(this).val();
        if (isNaN(records)) {
            $(this).val('25');
            return;
        }
        request.RecordsPerPage = records;
        LoadContacts();
    });

    $('#pageNumber').on('change', function () {
        request.Page = $(this).val();
        LoadContacts();
    });

    $('#pageLeft').on('click', function () {
        if (request.Page == 1)
            return;
        request.Page--;
        $('#pageNumber').val(request.Page);
        LoadContacts();
    });

    $('#pageRight').on('click', function () {
        if (request.Page == request.TotalPages)
            return;
        request.Page++;
        $('#pageNumber').val(request.Page);
        LoadContacts();
    });

    $('#saveReport').on('click', function () {
        processing.showPleaseWait();
        $('#newContactReport').modal('hide');
        var data = {};
        data.Name = $('#reportName').val();
        data.Filters = PopulateFilters();
        data.Overwrite = $('#overwriteReport').is(':checked');
        var method = 'post';
        if (data.Overwrite) {
            data.Name = $('#reportSelect').val();
            method = 'put';
        }
        $.ajax({
            url: '../../Cloud/ContactReport',
            type: method,
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    if (result.NewItem) {
                        $('#reports').append('<option value="' + result.UId + '">' + result.Name + '</option>');
                    }
                } else {
                    alert(result.Message);
                }
                processing.hidePleaseWait();
            },
            error: function () {
                alert('Server Error');
                processing.hidePleaseWait();
            }
        });
    });

    $('#actionList').on('click', function () {
        processing.showPleaseWait();
        $('#savedList').modal('hide');
        var data = {};
        data.id = $('#listSelected').val();
        var method = 'post';
        var action = 'SavedListContacts';
        if ($('#savedListAction').val() == 'remove' || $('#savedListAction').val() == 'removeByFilter') {
            method = 'delete';
        }
        if ($('#savedListAction').val() == 'addByFilter' || $('#savedListAction').val() == 'removeByFilter') {
            action = 'SavedListContactsByFilters';
            data.filters = PopulateFilters();
        } else {
            data.contacts = [];
            $('.contact-selected:checked').each(function () {
                var tr = $(this).parent().parent();
                data.contacts.push(tr.attr('id'));
            });
        }
        $.ajax({
            url: '../../Cloud/' + action,
            type: method,
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (!result.Success) {
                    alert(result.Message);
                }
                processing.hidePleaseWait();
            },
            error: function () {
                alert('Server Error');
                processing.hidePleaseWait();
            }
        });
    });

    $('#createSavedList').on('click', function () {
        processing.showPleaseWait();
        $('#newSavedList').modal('hide');
        var data = {};
        data.name = $('#savedListName').val();
        data.filters = PopulateFilters();
        $.ajax({
            url: '../../Cloud/SavedList',
            type: 'post',
            data: AddJsonAntiForgeryToken(data),
            dataType: "json",
            success: function (result) {
                if (!result.Success) {
                    alert(result.Message);
                }
                processing.hidePleaseWait();
            },
            error: function () {
                alert('Server Error');
                processing.hidePleaseWait();
            }
        });
    });

    $('#refreshList').on('click', function (e) {
        e.preventDefault();
        LoadContacts();
    });

    LoadContacts();

    RunFilters(generateSelection, generateActing, p_FillFilters);

    // Functions

    function p_FillFilters(filters) {
        $('#filters').html('');
        var filterIndex = -1;
        for (var i = 0; i < filters.length; i++) {
            filterIndex++;
            var html = '';
            html += '<tr class="filter" data-index="' + filterIndex + '"><td>';
            html += '<label><input type="checkbox" class="groupNext" value="true" /></label> <a href="#" class="delete-statement"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '<input type="hidden" name="Filters[' + filterIndex + '].GroupNext" value="' + filters[i].GroupNext + '" />';
            html += '<input type="hidden" name="Filters[' + filterIndex + '].UId" value="' + filters[i].UId + '" />';
            html += '<input type="hidden" name="Filters[' + filterIndex + '].Order" value="' + filters[i].Order + '" />';
            html += '</td>';
            html += '<td><select class="filter-link form-control" name="Filters[' + filterIndex + '].Link">';
            html += '<option value="0"' + (filters[i].Link === 0 ? ' selected="true"' : '') + '>None</option>';
            html += '<option value="1"' + (filters[i].Link === 1 ? ' selected="true"' : '') + '>And</option>';
            html += '<option value="2"' + (filters[i].Link === 2 ? ' selected="true"' : '') + '>Or</option>';
            html += '</select></td>';
            html += '<td>';
            html += generateActingInput(filters[i].ActingOn);
            html += '</td>';
            html += '<td><select class="filter-test form-control" name="Filters[' + filterIndex + '].Test">';
            for (var j = 0; j < tests.length; j++) {
                html += '<option value="' + tests[j].Index + '"' + (tests[j].Index == filters[i].Test ? ' selected="true"' : '') + '>' + tests[j].Name + '</option>';
            }
            html += '</select></td>';
            html += '<td>';
            html += generateValueInput(filters[i].ActingOn, filterIndex, filters[i].Value);
            html += '</td>';
            html += '</tr>';
            $('#filters').append(html);
            $('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
                var index = parseInt($(this).parents('tr').attr('data-index'));
                ActingOnChange($('tr[data-index="' + index + '"]'), $(this));
            });
            $('.filter[data-index="' + filterIndex + '"]').find('.delete-statement').on('click', function (e) {
                DeleteStatement($(this).parents('tr')[0], e);
            });
        }
        $('.datepicker').datetimepicker();
    }

    function generateActing(id) {
        if (typeof (id) == 'undefined' || id === null)
            id = '';
        var html = '<select class="filter-actingon form-control" name="Filters[' + filterIndex + '].ActingOn">';
        html += '<option value="email"' + (id == 'email' ? ' selected="true"' : '') + '>Email</option>';
        for (var i = 0; i < headers.length; i++) {
            html += '<option value="' + headers[i].UId + '"' + (id == headers[i].UId ? ' selected="true"' : '') + '>' + headers[i].Name + '</option>';
        }
        html += '</select></td>';
        return html;

    }

    function generateSelection(id, index, value) {
        if (typeof (value) == 'undefined' || value === null)
            value = "";
        var header = findHeader(id);
        var type = null;
        if (typeof (header) == 'undefined' || header !== null)
            type = header.Descriminator;
        else
            type = id;
        if (typeof (type) == 'undefined' || type === null)
            type = "default";
        var html = '';
        switch (type.toLowerCase()) {
            case 'date':
                html += '<input type="text" class="filter-value form-control datepicker" name="Filters[' + index + '].Value" value="' + value + '" data-date-pickTime="false" />';
                break;
            case 'datetime':
                html += '<input type="text" class="filter-value form-control datepicker" name="Filters[' + index + '].Value" value="' + value + '" data-date-pickTime="false" />';
                break;
            case 'time':
                html += '<input type="text" class="filter-value form-control datepicker" name="Filters[' + index + '].Value" value="' + value + '" data-date-pickDate="false" />';
                break;
            default:
                html += '<input type="text" class="filter-value form-control" name="Filters[' + index + '].Value" value="' + value + '" />';
                break;
        }
        return html;
    }

    function LoadContacts() {
        processing.showPleaseWait();
        // Grab filters
        request.Filters = PopulateFilters();
        request.Contacts = [];
        $.ajax({
            url: '../../Cloud/Contacts',
            type: "get",
            data: { rawJson: JSON.stringify(request) },
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    var table = {};
                    table.object = $('#contactData');
                    table.newHtml = '';
                    table.oldHtml = table.object.html();
                    $.extend(true, request, result.Data);
                    for (var i_1 = 0; i_1 < request.Contacts.length; i_1++) {
                        var contact = new Contact();
                        $.extend(contact, request.Contacts[i_1]);
                        table.newHtml += '<tr data-index="' + i_1 + '" id="' + contact.UId + '">';
                        table.newHtml += '<td><input type="checkbox" class="contact-selected" /></td>';
                        table.newHtml += '<td><a href="../../Cloud/Contact/' + contact.UId + '">' + contact.Email + '</a></td>';
                        for (var j_1 = 0; j_1 < headers.length; j_1++) {
                            var data = new ContactData();
                            var t_data = findData(contact.Data, headers[j_1].UId);
                            if (t_data === null || typeof (t_data) == 'undefined') {
                                data.HeaderKey = headers[j_1].UId;
                                data.UId = contact.UId;
                                request.Contacts[i_1].Data.push(data);
                                t_data = { index: request.Contacts[i_1].Data.length - 1 };
                            } else {
                                $.extend(data, t_data.data);
                            }
                            if (data.Value === null || /^\s*$/.test(data.Value))
                                data.Value = "";
                            table.newHtml += '<td class="item-editable" data-header-index="' + j_1 + '" data-index="' + t_data.index + '">' + data.PrettyValue + '</td>';
                        }
                        table.newHtml += '</tr>';
                    }
                    var pageOptions = $('#pageNumber option');
                    if (pageOptions.length < request.TotalPages) {
                        for (var i_2 = pageOptions.length; i_2 < request.TotalPages; i_2++) {
                            $('#pageNumber').append('<option value="' + (i_2 + 1) + '"' + ((i_2 + 1) == result.Page ? ' selected="true"' : '') + '>' + (i_2 + 1) + '</option>');
                        }
                    } else {
                        var removeAt = pageOptions.length - 1;
                        for (var i_3 = 0; i_3 < pageOptions.length - request.TotalPages; i_3++) {
                            pageOptions[removeAt].remove();
                            removeAt--;
                        }
                    }
                    table.object.html(table.newHtml);
                    BindEditables();
                } else {
                    alert(result.Message);
                }
                processing.hidePleaseWait();
            },
            error: function () {
                alert('Server Error');
                processing.hidePleaseWait();
            }
        });

    }

    function BindEditables() {
        $('.item-editable').on('click', function () {
            editing.td = $(this);
            editing.tr = $(this).parent('tr');
            editing.contactIndex = editing.tr.attr('data-index');
            editing.dataIndex = editing.td.attr('data-index');
            if (editing.dataIndex == '-10') {
                editing.header.Descriminator == "email";
            } else {
                editing.header = headers[editing.td.attr('data-header-index')];
            }
            if (editing.contactIndex == -1)
                return;
            editing.contact = request.Contacts[editing.contactIndex];
            if (editing.dataIndex == -1)
                return;
            editing.data = request.Contacts[editing.contactIndex].Data[editing.dataIndex];
            ShowEditableItem();
        });
    }

    function RunBinding() {
        $('.editing-value[data-validate="datetime"]').datetimepicker();
        $('.editing-value[data-validate="date"]').datetimepicker({
            pickTime: false
        });
        $('.editing-value[data-validate="time"]').datetimepicker({
            pickDate: false
        });
    }

    function saveContactData(data, modal, error) {
        // Check to see if all the information is passed successfully.
        if (typeof (data.ContactKey) === 'undefined') {
            throw 'No contact key passed.';
        }
        if (typeof (data.HeaderKey) === 'undefined') {
            throw 'No header key passed.';
        }
        if (typeof (data.Value) === 'undefined') {
            throw 'No value passed.';
        }
        processing.showPleaseWait();
        //We set up the xml request.
        var sd_xhr = new XMLHttpRequest();
        sd_xhr.open('put', '../../Cloud/ContactData', true);
        RESTFUL.jsonHeader(sd_xhr);
        sd_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        sd_xhr.onload = function (event) {
            var t_xhr = event.currentTarget;
            if (t_xhr.status === 200) {
                var t_response = RESTFUL.parse(t_xhr);
                if (t_response.Success) {
                    request.Contacts[editing.contactIndex].Data[editing.dataIndex].Value = data.Value;
                    editing.td.html(data.Value);
                    processing.hidePleaseWait();
                } else {
                    processing.hidePleaseWait();
                    modal.modal('show');
                    error.html(t_response.Message);
                }
            } else {
                RESTFUL.xhrError(event);
            }
        };
        AddJsonAntiForgeryToken(data);
        sd_xhr.send(JSON.stringify(data));        
    }

    function ShowEditableItem() {
        var html = '';
        html += '<div class="row">';
        var value = null;
        switch (editing.header.Descriminator) {
            case 'datetime':
                value = editing.data.Value;
                if (value == '')
                    value = moment().format('MM/DD/YYYY h:mm A');
                html += '<input type="text" class="editing-value form-control" data-validate="datetime" value="' + value + '">';
                break;
            case 'date':
                value = editing.data.Value;
                if (value == '')
                    value = moment().format('MM/DD/YYYY');
                html += '<input type="text" class="editing-value form-control" data-validate="date" value="' + value + '">';
                break;
            case 'time':
                value = editing.data.Value;
                if (value == '')
                    value = moment().format('h:mm A');
                html += '<input type="text" class="editing-value form-control" data-validate="time" value="' + value + '">';
                break;
            case 'number':
                html += '<input type="number" class="editing-value form-control" data-validate="number" value="' + editing.data.Value + '">';
                break;
            case 'email':
                html += '<input type="text" class="editing-value form-control" data-validate="email" value="' + editing.data.Value + '">';
                break;
            case 'text':
                html += '<input type="text" class="editing-value form-control" data-validate="text" value="' + editing.data.Value + '">';
                break;
            default:
                html += '<input type="text" class="editing-value form-control" data-validate="text" value="' + editing.data.Value + '">';
                break;
        }
        html += '</div>';
        $('#editModal').find('.modal-body').html(html);
        $('#editModalError').html('');
        RunBinding();
        $('#editModal').modal('show');
    }

    function findData(obj, header) {
        for (var i = 0; i < obj.length; i++) {
            if (obj[i].HeaderKey == header)
                return { index: i, data: obj[i] };
        }
        return null;
    }

    function findHeader(id) {
        for (var i = 0; i < headers.length; i++) {
            if (headers[i].UId == id) {
                return headers[i];
            }
        }
        return null;
    }

    // Custom Objects

    function ContactListRequest() {
        this.Page = 1;
        this.RecordsPerPage = 25;
        this.TotalRecords = -1;
        this.TotalPages = -1;
        this.CompanyKey = '';
        this.Sorting = new Sorting();
        this.Filters = [];
        this.Contacts = [];
    }

    function Sorting() {
        this.ActingOn = 'Email';
        this.Descending = false;
    }

    function Contact() {
        this.SortingId = -1;
        this.UId = '00000000-0000-0000-0000-000000000000';
        this.CompanyKey = '';
        this.Name = '';
        this.Description = '';
        this.Permission = '';
        this.DateCreated = '';
        this.DateModified = '';
        this.Owner = '';
        this.Group = '';
        this.ModificationToken = '';
        this.ModifiedBy = '';
        this.Data = [];
        this.Email = '';
    }

    function ContactData() {
        this.UId = '00000000-0000-0000-0000-000000000000';
        this.SortingId = -1;
        this.Value = '';
        this.PrettyValue = '';
        this.HeaderKey = '';
        this.IsSecure = '';
    }

    function ContactHeader() {
        this.UId = '00000000-0000-0000-0000-000000000000';
        this.SortingId = -1;
        this.Name = '';
        this.CompanyKey = '';
        this.SavedListKey = '';
        this.Descriminator = '';
        this.DescriminatorOptions = {};
    }

});