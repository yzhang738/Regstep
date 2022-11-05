/**********************/
/* Editing Email      */
/* Coded by DJ Enzyme */
/* Started 29SEP2014  */
/**********************/

CKEDITOR.disableAutoInline = true;
var debug = true;
var sortingIdLoc = 999999999;

var rgx_removeNonEdititng = /<!--editorignore-->[\s\S]*?<!--endeditorignore-->/gmi;

$(document).on('ready', function (e) {

    AddJsonAntiForgeryToken(email);

    $('#saveEmail').on('click', function (e) {
        e.preventDefault();
        SaveEmail();
    });
    
    /****************/
    /* Email Inputs */
    /****************/

    $('#Name').on('input', function (e) {
        email.Name = $(this).val();
    });
    $('#EmailListKey').on('change', function (e) {
        email.EmailListKey = $(this).val();
    });
    $('#Subject').on('input', function (e) {
        email.Subject = $(this).val();
    });
    $('#Description').on('input', function (e) {
        email.Description = $(this).val();
    });
    $('#From').on('input', function (e) {
        email.From = $(this).val();
    });
    $('#CC').on('input', function (e) {
        email.CC = $(this).val();
    });
    $('#BCC').on('input', function (e) {
        email.BCC = $(this).val();
    });
    $('#EmailType').on('change', function (e) {
        email.EmailType = $(this).val();
    });
    $('#SendTime').on('input', function (e) {
        email.SendTime = $(this).val();
    });
    $('#RepeatSending').on('change', function (e) {
        email.RepeatSending = $(this).is(':checked');
    });
    $('#SendTime').datetimepicker({
        autoclose: true,
        minuteStep: 15,
        format: 'M/D/YYYY hh:mm:00 A Z'
    }).on("dp.change", function (e) {
        email.SendTime = e.date.format('M/D/YYYY hh:mm:00 A Z');
    });

    $('#To').on('input', function (e) {
        email.To = $(this).val();
    });

    $('#b_testSend').on('click', function (e) {
        $('#m_testSend').modal('hide');
        TestSend();
    });

    $('#b_s_sendSettings').on('click', function (e) {
        var i_interval = $('#SendInterval');
        $('#m_sendSettings').modal('hide');
        var r_interval = i_interval.val();
        var m_interval = 0;
        var i_interval_suffix = i_interval.attr('data-interval');
        switch (i_interval_suffix) {
            case 'sec':
                m_interval = r_interval;
                break;
            case 'min':
                m_interval = r_interval * 60;
                break;
            case 'hr':
                m_interval = r_interval * 60 * 60;
                break;
            case 'day':
                m_interval = r_interval * 24 * 60 * 60;
                break;
            case 'mo':
                m_interval = r_interval * 30.5 * 24 * 60 * 60;
                break;
            case 'yr':
                m_interval = r_interval * 365 * 24 * 60 * 60;
                break;
        }
        email.IntervalSeconds = m_interval;
        email.MaxSends = $('#MaxSends').val();
    });

    $('#suffix_SendInterval').on('click', function (e) {
        var input = $('#SendInterval');
        var suffix = input.attr('data-interval');
        var n_suffix = '';
        switch (suffix) {
            case 'sec':
                n_suffix = 'min';
                break;
            case 'min':
                n_suffix = 'hr';
                break;
            case 'hr':
                n_suffix = 'day';
                break;
            case 'day':
                n_suffix = 'mo';
                break;
            case 'mo':
                n_suffix = 'yr';
                break;
            case 'yr':
                n_suffix = 'sec';
                break;
        }
        input.attr('data-interval', n_suffix);
        $(this).html(n_suffix);
    });

    $('#b_pullPlainText').on('click', function () {
        pullPlainText();
    });

    $('#b_plainText').on('click', function () {
        $(this).closest('.modal').modal('hide');
        email.PlainText = $('#plainText').val();
    });

    $('#b_c_plainText, #h_c_plainText').on('click', function () {
        $(this).closest('.modal').modal('hide');
        $('#plainText').val(email.PlainText);
    });

    /********************/
    /* End Email Inputs */
    /********************/

    /******************************/
    /* Email Area Variables Modal */
    /******************************/

    $('#emailVariables').on('click', function (e) {
        var t_modal = $('#variableModal');
        var t_body = t_modal.find('.modal-body');
        t_body.closest('.modal').data('email-areaid', -1);
        var t_html = '<div class="form-horizontal"><div class="row">';
        for (var i = 0; i < email.Variables.length; i++) {
            var t_variable = email.Variables[i];
            t_html += '<div class="col-sm-12"><label class="col-md-5 col-sm-12 control-label">' + t_variable.Name + '</label><div class="col-md-7 col-sm-12"><input class="form-control emailarea-variable" type="text" data-input-type="' + t_variable.Type + '" id="' + t_variable.Variable + '" value="' + (t_variable.Value === null ? "" : t_variable.Value) + '"></div></div>';
        }
        t_html += '</div></div>';
        t_body.html(t_html);
        t_body.runInputs();
        t_modal.modal('show');
    });

    $('#variableModal').on('hidden.bs.modal', function () {
        $(this).find('.modal-body').html('').data('id', '');
    });

    $('#modalSaveChanges').on('click', function (e) {
        var t_modal = $(this).closest('.modal');
        var t_id = t_modal.data('email-areaid');
        if (t_id == -1) {
            var inputs = t_modal.find('.emailarea-variable').each(function (i) {
                var t_input = $(this);
                for (var j = 0; j < email.Variables.length; j++) {
                    if (email.Variables[j].Variable == t_input.attr('id'))
                        email.Variables[j].Value = t_input.val();
                }
            });
        } else {
            var t_area = FindEmailArea(t_id, email.EmailAreas);
            var inputs = t_modal.find('.emailarea-variable').each(function (i) {
                var t_input = $(this);
                for (var j = 0; j < t_area.Variables.length; j++) {
                    if (t_area.Variables[j].Variable == t_input.attr('id'))
                        t_area.Variables[j].Value = t_input.val();
                }
            });
        }
        $('#variableModal').modal('hide');
        CompileEmail();
    });

    /**********************************/
    /* End Email Area Variables Modal */
    /**********************************/

    /************/
    /* New Area */
    /************/

    $('.newAreaLink').on('click', function (e) {
        $('#newArea').modal('hide');
        processing.showPleaseWait();
        e.preventDefault();
        e.stopPropagation();
        var j_item = $(this);
        var t_type = j_item.attr('data-email-type');
        var t_t_area = FindEmailTemplateArea(t_type, email.Template.EmailAreas);
        if (t_t_area === null) {
            processing.hidePleaseWait();
            alert("Invalid area type.");
            return;
        }
        var t_areas = FindEmailAreas(t_type, email.EmailAreas);
        var t_order = t_areas.length + 1;
        var n_area = {
            Name: "New Area",
            Type: t_type,
            Html: "New " + t_type + ".",
            Order: t_order,
            Variables: [],
            SortingId: --sortingIdLoc
        };
        for (var i = 0; i < t_t_area.Variables.length; i++) {
            n_area.Variables.push({
                Variable: t_t_area.Variables[i].Variable,
                Name: t_t_area.Variables[i].Name,
                Description: t_t_area.Variables[i].Description,
                Type: t_t_area.Variables[i].Type,
                Value: t_t_area.Variables[i].Value
            });
        }
        email.EmailAreas.push(n_area);
        CompileEmail();
    });

    /****************/
    /* End New Area */
    /****************/

    CompileEmail();
});

function TestSend() {
    var t_xhr = new XMLHttpRequest();
    var t_data = { id: email.UId, to: $('#testSend_To').val() };
    AddJsonAntiForgeryToken(t_data);
    t_xhr.open('post', '../../Email/TestSend', true);
    t_xhr.setRequestHeader('Content-Type', 'application/json');
    t_xhr.onerror = function (result) {
        processing.hidePleaseWait();
        alert('Unhandled server error.');
    };
    t_xhr.onload = function (result) {
        if (result.currentTarget.status == 200) {
            var c_xhr = result.currentTarget;
            var result = JSON.parse(c_xhr.responseText);
            if (result.Success) {
                processing.hidePleaseWait();
            } else {
                processing.hidePleaseWait();
                alert(result.Message);
            }
        } else {
            processing.hidePleaseWait();
            alert('Page not found.');
        }
    };
    t_xhr.send(JSON.stringify(t_data));
}

/**************/
/* Save Email */
/**************/

function SaveEmail() {
    var t_xhr = new XMLHttpRequest();
    t_xhr.open('put', '../../Email/Email', true);
    t_xhr.setRequestHeader('Content-Type', 'application/json');
    t_xhr.onerror = function (result) {
        processing.hidePleaseWait();
        alert('Unhandled server error.');
    };
    t_xhr.onload = function (result) {
        if (result.currentTarget.status == 200) {
            var c_xhr = result.currentTarget;
            var result = JSON.parse(c_xhr.responseText);
            if (result.Success) {
                processing.hidePleaseWait();
            } else {
                processing.hidePleaseWait();
                alert(result.Message);
            }
        } else {
            processing.hidePleaseWait();
            alert('Page not found.');
        }
    };
    processing.showPleaseWait();
    t_xhr.send(JSON.stringify(email));
}

/******************/
/* End Save Email */
/******************/

/*********************/
/* CKEditor Function */
/*********************/

function BindCKEditor() {
    
    $('.email-area-editable').each(function (i) {
        $(this).ckeditor({
            customConfig: 'email_config.js'
        });
        $(this).ckeditorGet().on('change', function (e) {
            var t_div = $(e.editor.element.$);
            var ea_id = t_div.attr('data-email-areaid');
            var t_area = FindEmailArea(ea_id, email.EmailAreas);
            t_area.Html = e.editor.getData();
        });
        $(this).ckeditorGet().on('removearea.ckeditor', function (e) {
            var t_div = $(e.editor.element.$);
            var t_type = t_div.attr('data-email-areatype');
            var ea_id = t_div.attr('data-email-areaid');
            for (var i = 0; i < email.EmailAreas.length; i++) {
                if (email.EmailAreas[i].SortingId == ea_id) {
                    email.EmailAreas.splice(i, 1);
                    break;
                }
            }
            var t_areas = FindEmailAreas(t_type, email.EmailAreas);
            var t_order = 0;
            for (var i = 0; i < t_areas.length; i++) {
                t_areas[i].Order = ++t_order;
            }
            var t_parent = t_div.closest('.email-area-holder');
            t_parent.remove();
            e.editor.destroy();
        });
        $(this).ckeditorGet().on('areaUp.ckeditor', function (e) {
            var t_div = $(e.editor.element.$);
            var ea_id = t_div.attr('data-email-areaid');
            var t_type = t_div.attr('data-email-areatype');
            var t_parent = t_div.closest('.email-area-holder');
            var t_typeHolder = $('#' + t_type);
            var prev = t_parent.prev();
            if (typeof (prev) == 'undefined' || prev === null)
                return;
            $(t_parent).insertBefore(prev);
            t_typeHolder.children('.email-area-holder').each(function (i) {
                FindEmailArea($(this).find('.email-area-editable').attr('data-email-areaid'), email.EmailAreas).Order = (i + 1);
            });
        });
        $(this).ckeditorGet().on('areaDown.ckeditor', function (e) {
            var t_div = $(e.editor.element.$);
            var ea_id = t_div.attr('data-email-areaid');
            var t_type = t_div.attr('data-email-areatype');
            var t_parent = t_div.closest('.email-area-holder');
            var t_typeHolder = $('#' + t_type);
            var next = t_parent.next();
            $(t_parent).insertAfter(next);
            t_typeHolder.children('.email-area-holder').each(function (i) {
                FindEmailArea($(this).find('.email-area-editable').attr('data-email-areaid'), email.EmailAreas).Order = (i + 1);
            });
        });
        $(this).ckeditorGet().on('emailareavariables.ckeditor', function (e) {
            var t_div = $(e.editor.element.$);
            var ea_id = t_div.attr('data-email-areaid');
            var t_type = t_div.attr('data-email-areatype');
            var t_parent = t_div.closest('.email-area-holder');
            var t_modal = $('#variableModal');
            var t_body = t_modal.find('.modal-body');
            t_body.closest('.modal').data('email-areaid', ea_id);
            var t_html = '<div class="form-horizontal"><div class="row">';
            var t_area = FindEmailArea(ea_id, email.EmailAreas);
            if (t_area === null)
                return;
            for (var i = 0; i < t_area.Variables.length; i++) {
                var t_variable = t_area.Variables[i];
                t_html += '<div class="col-sm-12"><label class="col-md-5 col-sm-12 control-label">' + t_variable.Name + '</label><div class="col-md-7 col-sm-12"><input class="form-control emailarea-variable" type="text" data-input-type="' + t_variable.Type + '" id="' + t_variable.Variable + '" value="' + (t_variable.Value === null ? "" : t_variable.Value) + '"></div></div>';
            }
            t_html += '</div></div>';
            t_body.html(t_html);
            t_body.runInputs();
            t_modal.modal('show');
        });
    });

}

function FindEmailArea(id, emailAreas) {
    for (var i = 0; i < emailAreas.length; i++) {
        if (emailAreas[i].SortingId == id)
            return emailAreas[i];
    }
    return null;
}

/*************************/
/* End CKEditor Function */
/*************************/

/*****************/
/* Compile Email */
/*****************/

function CompileEmail() {
    processing.showPleaseWait();
    var c_html = '';
    var c_htmlArea = FindEmailTemplateArea('html', email.Template.EmailAreas);
    if (typeof(c_htmlArea) == 'undefined' || c_htmlArea === null) {
        if (debug)
            alert('Email Parsing Error');
        return;
    }
    c_html = c_htmlArea.Html;
    c_html = c_html.replace(rgx_removeNonEdititng, '');
    for (var i = 0; i < email.Variables.length; i++) {
        var var_value = email.Variables[i].Value;
        switch (email.Variables[i].Type) {
            case 'border':
                if (var_value === null || var_value == '')
                    var_value = 'none';
                break;
            default:
                if (var_value === null)
                    var_value = '';
        }
        c_html = c_html.replace(new RegExp('@render_var_' + email.Variables[i].Variable, 'gi'), var_value);
    }
    for (var i = 0; i < emailAreaTypes.length; i++) {
        if (emailAreaTypes[i].toLowerCase() == 'html')
            continue;
        var t_area = FindEmailTemplateArea(emailAreaTypes[i], email.Template.EmailAreas);
        var e_areas = FindEmailAreas(emailAreaTypes[i], email.EmailAreas);
        var c_a_html = '';
        for (var j = 0; j < e_areas.length; j++) {
            var t_html = t_area.Html;
            for (var k = 0; k < e_areas[j].Variables.length; k++) {
                var var_value = e_areas[j].Variables[k].Value;
                switch (e_areas[j].Variables[k].Type) {
                    case 'border':
                        if (var_value === null || var_value == '')
                            var_value = 'none';
                        break;
                    default:
                        if (var_value === null)
                            var_value = '';
                }
                t_html = t_html.replace(new RegExp('@render_var_' + e_areas[j].Variables[k].Variable, 'gi'), var_value);
            }
            t_html = t_html.replace(/@render_body/gi, '<div class="email-area-editable" contenteditable="true" data-email-areatype="' + emailAreaTypes[i] + '" data-email-areaid="' + e_areas[j].SortingId + '">' + e_areas[j].Html + '</div>');
            c_a_html += t_html;
        }
        c_html = c_html.replace(new RegExp('@render_' + emailAreaTypes[i], 'gi'), c_a_html);
    }
    $('#email').html(c_html);
    $('div[data-email-automargin]').css('margin', 'auto');
    BindCKEditor();

    processing.hidePleaseWait();
}

function FindEmailTemplateArea(key, emailAreas) {
    for (var i = 0; i < emailAreas.length; i++) {
        if (emailAreas[i].Type.toLowerCase() == key.toLowerCase())
            return emailAreas[i];
    }
}

function FindEmailAreas(key, emailAreas) {
    var areas = [];
    for (var i = 0; i < emailAreas.length; i++) {
        if (emailAreas[i].Type.toLowerCase() == key.toLowerCase())
            areas.push(emailAreas[i]);
    }
    return areas.sort(emailComparer);
}

function emailComparer(a, b) {
    if (a.Order < b.Order)
        return -1;
    if (a.Order > b.Order)
        return 1;
    return 0;
}

function pullPlainText() {
    // Set up the email areas.
    var t_plain = "";
    // This will hold the html email.
    var c_html = '';
    var c_htmlArea = FindEmailTemplateArea('html', email.Template.EmailAreas);
    if (typeof (c_htmlArea) == 'undefined' || c_htmlArea === null) {
        if (debug)
            alert('Email Parsing Error');
        return;
    }
    c_html = c_htmlArea.Html;
    c_html = c_html.replace(rgx_removeNonEdititng, '');
    for (var i = 0; i < email.Variables.length; i++) {
        var var_value = email.Variables[i].Value;
        switch (email.Variables[i].Type) {
            case 'border':
                if (var_value === null || var_value == '')
                    var_value = 'none';
                break;
            default:
                if (var_value === null)
                    var_value = '';
        }
        var_value = var_value.trim();
        c_html = c_html.replace(new RegExp('@render_var_' + email.Variables[i].Variable, 'gi'), var_value);
    }
    for (var i = 0; i < emailAreaTypes.length; i++) {
        if (emailAreaTypes[i].toLowerCase() == 'html')
            continue;
        var t_area = FindEmailTemplateArea(emailAreaTypes[i], email.Template.EmailAreas);
        var e_areas = FindEmailAreas(emailAreaTypes[i], email.EmailAreas);
        var c_a_html = '';
        for (var j = 0; j < e_areas.length; j++) {
            var t_html = t_area.Html.trim();
            for (var k = 0; k < e_areas[j].Variables.length; k++) {
                var var_value = e_areas[j].Variables[k].Value;
                switch (e_areas[j].Variables[k].Type) {
                    case 'border':
                        if (var_value === null || var_value == '')
                            var_value = 'none';
                        break;
                    default:
                        if (var_value === null)
                            var_value = '';
                }
                var_value = var_value.trim();
                t_html = t_html.replace(new RegExp('@render_var_' + e_areas[j].Variables[k].Variable, 'gi'), var_value);
            }
            t_html = t_html.replace(/@render_body/gi, '<div class="email-area-editable" contenteditable="true" data-email-areatype="' + emailAreaTypes[i] + '" data-email-areaid="' + e_areas[j].SortingId + '">' + e_areas[j].Html.trim() + '</div>');
            c_a_html += t_html;
        }
        c_html = c_html.replace(new RegExp('@render_' + emailAreaTypes[i], 'gi'), c_a_html.trim());
    }
    // We will put the email into a jQuery object for easy access.
    var t_email = $(c_html);
    // Now we need to grab only the text;
    t_plain = $(c_html).text().trim().replace(/\s{2,}/g, '\r\n');;
    email.PlainText = t_plain;
    $('#plainText').val(t_plain);
}

/*********************/
/* End Compile Email */
/*********************/