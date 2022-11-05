/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var currentI = 0;
var currentJ = 0;
var pt = false;
var afterArrow = new RegExp("([^=]*)=>(.*)");

$(document).ready(function () {
    updateLogicView();

    $('#newLogicGroup').on('click', function () {
        addLogicGroup();
    });

    $('#newThenStatement').on('click', function () {
        addThenCommand();
    })

    $('#newElseSatement').on('click', function () {
        addElseCommand();
    })

    $('#logicGroupEdit').dialog({
        modal: true,
        height: 500,
        width: 650,
        buttons: {
            Ok: function () {
                var val = "";
                var com = "";
                if (!pt) {
                    val = $('#variable').val() + '=>' + $('#valueGroup').val();
                    com = $('#valueGroup').children('option:selected').text();
                } else {
                    val = "pt=>" + $('#value').val();
                    com = $('#value').val();
                }
                logics[currentI].logics[currentJ].commonVariable = $('#variable').children('option:selected').attr('data-common');
                logics[currentI].logics[currentJ].variable = $('#variableForm').val() + "=>" + $('#variable').val();
                logics[currentI].logics[currentJ].form = $('#variableForm').val();
                logics[currentI].logics[currentJ].commonValue = com;
                logics[currentI].logics[currentJ].value = val;
                logics[currentI].logics[currentJ].test = parseInt($('#test').val());
                logics[currentI].logics[currentJ].link = parseInt($('#link').val());
                logics[currentI].link = parseInt($('#logicGroupLink').val());
                updateLogicView();
                $(this).dialog("close");
                $('#value').val('');
                $('#variable').val('');
                $('#valueGroup').val('');
                $('#variableForm').val('');
                $('.LogicEditArea').hide();
            }
        },
        autoOpen: false
    });

    $('#commandEdit').dialog({
        modal: true,
        height: 500,
        width: 650,
        buttons: {
            Ok: function () {
                var param = [];
                thens[currentI].command = parseInt($('#commandType').val());
                switch (thens[currentI].command) {
                    case 1:
                        param.push($('#setVariableForm').val() + '=>' + $('#setVariableVar').val());
                        param.push($('#setVariableTo').val());
                        break;
                    case 2:
                        param.push($('#displayText').val());
                        break;
                }
                thens[currentI].param = param;
                updateLogicView();
                $(this).dialog("close");
                $('#setVariableParams').addClass('Hidden');
                $('#displayTextParams').addClass('Hidden');
                $('#setVariableVar').attr('disabled', true);
                $('#setVariableVar').html('<option value="">Select Variable</option>')
                updateLogicView();
            }
        },
        autoOpen: false
    });

    $('#commandEditElse').dialog({
        modal: true,
        height: 500,
        width: 650,
        buttons: {
            Ok: function () {
                var param = [];
                elses[currentI].command = parseInt($('#commandTypeElse').val());
                switch (elses[currentI].command) {
                    case 1:
                        param.push($('#setVariableFormElse').val() + '=>' + $('#setVariableVarElse').val());
                        param.push($('#setVariableToElse').val());
                        break;
                    case 2:
                        param.push($('#displayTextElse').val());
                        break;
                }
                elses[currentI].param = param;
                updateLogicView();
                $(this).dialog("close");
                $('#setVariableParamsElse').addClass('Hidden');
                $('#displayTextParamsElse').addClass('Hidden');
                $('#setVariableVarElse').attr('disabled', true);
                $('#setVariableVarElse').html('<option value="">Select Variable</option>')
                updateLogicView();
            }
        },
        autoOpen: false
    });


    switch (type) {
        case "TextBlock":
            $('#commandType').children('option[value=0]').remove();
            $('#commandType').children('option[value=1]').remove();
            $('#commandType').children('option[value=3]').remove();
            $('#commandType').children('option[value=4]').remove();

            $('#commandTypeElse').children('option[value=0]').remove();
            $('#commandTypeElse').children('option[value=1]').remove();
            $('#commandTypeElse').children('option[value=3]').remove();
            $('#commandTypeElse').children('option[value=4]').remove();

            break;
        case "Page":
            $('#commandType').children('option[value=2]').remove();
            $('#commandType').children('option[value=3]').remove();
            $('#commandType').children('option[value=4]').remove();

            $('#commandTypeElse').children('option[value=2]').remove();
            $('#commandTypeElse').children('option[value=3]').remove();
            $('#commandTypeElse').children('option[value=4]').remove();
            break;
        default:
            $('#commandType').children('option[value=0]').remove();

            $('#commandTypeElse').children('option[value=0]').remove();
            break;
    }
});

function newStatement() {
    logics[currentI].logics.push({ commonVariable: 'NOTSET', commonValue: 'NOTSET', variable: '', value: '', form: '', link: 0, test: 0 });
    insertStatement(currentI, logics[currentI].logics.length - 1);
}

function addLogicGroup() {
    if (logics.length > 0 && logics[logics.length - 1].link == 0) logics[logics.length - 1].link = 1;
    logics.push({ link: 0, logics: [{ commonVariable: 'NOTSET', commonValue: 'NOTSET', variable: '', value: '', form: '', link: 0, test: 0 }] });
    updateLogicView();
}

function insertStatement(i, j) {
    currentI = i;
    currentJ = j;
    $('.LogicEditArea').slideDown(500);
    var form = logics[i].logics[j].form;
    $('#variableForm').val(form);
    if (form != '') {
        $('#variable').removeAttr('disabled');
        formSelected(logics[i].logics[j].variable);
    } else {
        $('#variable').attr('disabled', true);
        $('#variable').val('');
    }
    $('#test').val(logics[i].logics[j].test)
    $('#value').val(logics[i].logics[j].value)
    $('#link').val(logics[i].logics[j].link)
}

function formSelected(set) {
    if (!set) {
    } else {
        var matches = afterArrow.exec(set);
        var pre = matches[1];
        var suf = matches[2];
    }
    var id = $('#variableForm').val();
    $('#variable').html('');
    $('#variable').html('<option>Loading Values</option>')
    $.ajax({
        url: "GetFormVariables?cids=" + company + "&fids=" + id,
        success: function (data) {
            $('#variable').html('<option data-common="NOTSET" value="" selected>Select Variable</option>')
            for (var i = 0; i < data.id.length; i++) {
                $("#variable").append('<option data-common="' + data.variable[i] + '" value="' + data.id[i] + '">' + data.variable[i] + '</option>');
            }
            $('#variable').removeAttr('disabled');
            if (!set) {
            } else {
                $('#variable').val(suf);
                variableSelect('logicVar', logics[currentI].logics[currentJ].value);
            }
        },
        type: 'GET'
    })
}

function itemSelect(place) {
    var value = "";
    var select = "";
    switch (place) {
        case "logicVar":
            value = "#value";
            select = "#valueGroup";
            break;
    }
    $(value).val($(select).val());
}

function fillSelect(type, compId, select, test, val, gr, data, input, set) {
    var pre = "";
    var suf = "";
    if (!set) { } else {
        var matches = afterArrow.exec(set);
        pre = matches[1];
        suf = matches[2];
    }
    if (type != "") {
        pt = false;
        switch (type) {
            case "checkbox":
                $('#test').children('[value=0]').html('Contains');
                $('#test').children('[value=5]').html('Does Not Contain');
                break;
        }

        $('#test').children('[value=1]').attr("disabled", true);
        $('#test').children('[value=2]').attr("disabled", true);
        $('#test').children('[value=3]').attr("disabled", true);
        $('#test').children('[value=4]').attr("disabled", true);
        var options = '<option value="">Select Variable</option>';
        for (var i = 0; i < data.id.length; i++) {
            options += '<option value="' + data.id[i] + '">' + data.commons[i] + '</option>';
        }
        $(select).html(options);
        if (set) {
            $(select).val(suf);
        }
        $(val).slideUp(500);
        $(gr).slideDown(500);
    } else {
        pt = true;
        $('#test').children('[value=1]').removeAttr("disabled");
        $('#test').children('[value=2]').removeAttr("disabled");
        $('#test').children('[value=3]').removeAttr("disabled");
        $('#test').children('[value=4]').removeAttr("disabled");
        $('#test').children('[value=0]').html('==');
        $('#test').children('[value=5]').html('!=');
        if (set) {
            $(input).val(suf);
        }
        $(gr).slideUp(500);
        $(val).slideDown(500);
    }
}

function variableSelectFirstAjax(type, compId, select, test, val, gr, input, set) {
    var itemData;
    $.ajax({
        url: "GetComponentItems",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        traditional: true,
        data: JSON.stringify({ cid: company, compId: compId }),
        success: function (data) {
            itemData = data;
            fillSelect(type, compId, select, test, val, gr, data, input, set);
        },
        type: 'POST'
    });
}

function variableSelect(place, set)
{
    var compId = "";
    var type = "";
    var select = "";
    var test = "";
    var val = "";
    var gr = "";
    var input = "";
    switch (place) {
        case 'logicVar':
            compId = $('#variable').val();
            test = "#test";
            select = "#valueGroup";
            val = "#logicValue";
            gr = "#logicValueRC"
            input = "#value";
            break;
    }
    $(gr).slideUp(500);
    $(val).slideUp(500);
    $(select).html('');
    $(select).val('');
    $(input).val('');
    $.ajax({
        url: "GetVariableType",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        traditional: true,
        data: JSON.stringify({ cid: company, compId: compId }),
        success: function (data) {
            if (data.success) {
                switch (data.type) {
                    case "RadioGroup":
                        type = "radio";
                        break;
                    case "CheckboxGroup":
                        type = "checkbox";
                        break;
                }
                variableSelectFirstAjax(type, compId, select, test, val, gr, input, set);
            }
        },
        type: 'POST'
    });

}

function formSelectedSetVar(set) {
    var id = $('#setVariableForm').val();
    $('#setVariableVar').html('');
    $('#setVariableVar').html('<option>Loading Values</option>')
    $.ajax({
        url: "GetFormVariables?cids=" + company + "&fids=" + id,
        success: function (data) {
            $('#setVariableVar').html('<option data-common="NOTSET" value="" selected>Select Variable</option>')
            for (var i = 0; i < data.id.length; i++) {
                $("#setVariableVar").append('<option data-common="' + data.variable[i] + '" value="' + data.id[i] + '">' + data.variable[i] + '</option>');
            }
            $('#setVariableVar').removeAttr('disabled');
            if (!set) {
            } else {
                $('#setVariableVar').val(set);
            }
        },
        type: 'GET'
    })
}

function formSelectedSetVarElse(set) {
    if (!set) {
        set = null;
    }
    var id = $('#setVariableFormElse').val();
    $('#setVariableVarElse').html('');
    $('#setVariableVarElse').html('<option>Loading Values</option>')
    $.ajax({
        url: "GetFormVariables?cids=" + company + "&fids=" + id,
        success: function (data) {
            $('#setVariableVarElse').html('<option data-common="NOTSET" value="" selected>Select Variable</option>')
            for (var i = 0; i < data.id.length; i++) {
                $("#setVariableVarElse").append('<option data-common="' + data.variable[i] + '" value="' + data.id[i] + '">' + data.variable[i] + '</option>');
            }
            $('#setVariableVarElse').removeAttr('disabled');
            if (!set) {
            } else {
                $('#setVariableVarElse').val(set);
            }
        },
        type: 'GET'
    })
}

function insertLogic(i) {
    var logic = '';
    currentI = i;
    $('#logicGroupLink').val(logics[i].link);
    for (var j = 0; j < logics[i].logics.length; j++) {
        logic += '<span class="MediumText LogicGroup LogicItem" onclick="insertStatement(' + i + ',' + j + ')" data-id="' + j + '">';
        logic += logics[i].logics[j].commonVariable;
        switch (logics[i].logics[j].test) {
            case 0:
                logic += ' <span class="LogicTest">==</span> ';
                break;
            case 1:
                logic += ' <span class="LogicTest">&gt;</span> ';
                break;
            case 2:
                logic += ' <span class="LogicTest">&gt;=</span> ';
                break;
            case 3:
                logic += ' <span class="LogicTest">&lt;</span> ';
                break;
            case 4:
                logic += ' <span class="LogicTest">&lt;=</span> ';
                break;
            case 5:
                logic += ' <span class="LogicTest">!=</span> ';
                break;
        }
        logic += logics[i].logics[j].commonValue;
        switch (logics[i].logics[j].link) {
            case 0:
                break;
            case 1:
                logic += ' <span class="LogicLink">&</span> ';
                break;
            case 2:
                logic += ' <span class="LogicLink">|</span> ';
                break;
            case 3:
                logic += ' <span class="LogicLink">^</span> ';
                break;
        }
        logic += "</span>";
        logic += "<br />";
    }
    logic += "<br />";
    switch (logics[i].link) {
        case 0:
            break;
        case 1:
            logic += ' <span class="LogicLink">&</span> ';
            break;
        case 2:
            logic += ' <span class="LogicLink">|</span> ';
            break;
        case 3:
            logic += ' <span class="LogicLink">^</span> ';
            break;
    }
    $('#logicGroup').html(logic);
    $('#logicGroupEdit').dialog('open');
}

function addThenCommand()
{
    thens.push({command: 0, param: []});
    updateLogicView();
}

function insertThen(i) {
    currentI = i;
    var param = '';
    $('.Params').hide();
    $('#commandType').val(thens[i].command);
    switch (thens[i].command) {
        case 0:
            param = 'Page Skip';
            break;
        case 1:
            param = 'Set Variable';
            var result = new RegExp(/([^(?:=>)]*)=>(.*)/);
            var matches = result.exec(thens[i].param[0]);
            $('#setVariableForm').val(matches[1]);
            $('#setVariabltTo').val(thens[i].param[1]);
            $('#setVariableParams').show();
            formSelectedSetVar(matches[2]);
            break;
        case 2:
            param = 'Display Text';
            $('#displayText').val(thens[i].param[0]);
            $('#displayTextParams').show();
            break;
        case 3:
            param = 'Hide';
            break;
        case 4:
            param = 'Show';
            break;
    }
    $('#commandText').html(param);
    $('#commandEdit').dialog('open');
}

function addElseCommand() {
    elses.push({ command: 0, param: [] });
    updateLogicView();
}

function insertElse(i) {
    currentI = i;
    var param = '';
    $('.Params').hide();
    $('#commandTypeElse').val(elses[i].command);
    switch (elses[i].command) {
        case 0:
            param = 'Page Skip';
            break;
        case 1:
            param = 'Set Variable';
            var result = new RegExp(/([^(?:=>)]*)=>(.*)/);
            var matches = result.exec(elses[i].param[0]);
            $('#setVariableFormElse').val(matches[1]);
            $('#setVariabltToElse').val(elses[i].param[1]);
            $('#setVariableParamsElse').show();
            formSelectedSetVarElse(matches[2]);
            break;
        case 2:
            param = 'Display Text';
            $('#displayTextElse').val(elses[i].param[0]);
            $('#displayTextParamsElse').show();
            break;
        case 3:
            param = 'Hide';
            break;
        case 4:
            param = 'Show';
            break;
    }
    $('#commandTextElse').html(param);
    $('#commandEditElse').dialog('open');
}

function commandSelect() {
    var value = parseInt($('#commandType').val());
    switch (value)
    {
        case 0:
        case 3:
        case 4:
        case -1:
            $('#setVariableParams').slideUp();
            $('#displayTextParams').slideUp();
            break;
        case 1:
            $('#setVariableParams').slideDown();
            $('#displayTextParams').slideUp();
            break;
        case 2:
            $('#setVariableParams').slideUp();
            $('#displayTextParams').slideDown();
            break;
    }
}

function commandSelectElse() {
    var value = parseInt($('#commandTypeElse').val());
    switch (value) {
        case 0:
        case 3:
        case 4:
        case -1:
            $('#setVariableParamsElse').slideUp();
            $('#displayTextParamsElse').slideUp();
            break;
        case 1:
            $('#setVariableParamsElse').slideDown();
            $('#displayTextParamsElse').slideUp();
            break;
        case 2:
            $('#setVariableParamsElse').slideUp();
            $('#displayTextParamsElse').slideDown();
            break;
    }
}

function deleteThenCommand(i, e) {
    thens.splice(i, 1);
    updateLogicView();
    e.stopPropagation();
}

function deleteElseCommand(i, e) {
    elses.splice(i, 1);
    updateLogicView();
    e.stopPropagation();
}

function deleteLogicGroup(i, e) {
    logics.splice(i, 1);
    updateLogicView();
    e.stopPropagation();
}

function updateLogicView() {
    var logic = '<div><span class="Command">if</span> ';
    for (var i = 0; i < logics.length; i++) {
        if (i < logics.length - 1 && logics[i].link == 0) {
            logics[i].link = 1;
        }
        if (i == logics.length - 1 && logics[i].link != 0) {
            logics[i].link = 0;
        }
        logic += '<span class="LogicItem LogicGroup" onclick="insertLogic(' + i + ')" data-id="' + i + '"><span onclick="deleteLogicGroup(' + i + ')" class="DeleteItem"><span class="glyphicon glyphicon-remove-circle"></span></span>(';
        for (var j = 0; j < logics[i].logics.length; j++) {
            if (logics[i].logics[j].link == 0 && j < logics[i].logics.length - 1) {
                logics[i].logics[j].link = 1;
            }
            if (j == logics[i].logics.length - 1 && logics[i].logics[j].link != 0) {
                logics[i].logics[j].link = 0;
            }
            logic += logics[i].logics[j].commonVariable;
            switch (logics[i].logics[j].test) {
                case 0:
                    logic += ' <span class="LogicTest">==</span> ';
                    break;
                case 1:
                    logic += ' <span class="LogicTest">&gt;</span> ';
                    break;
                case 2:
                    logic += ' <span class="LogicTest">&gt;=</span> ';
                    break;
                case 3:
                    logic += ' <span class="LogicTest">&lt;</span> ';
                    break;
                case 4:
                    logic += ' <span class="LogicTest">&lt;=</span> ';
                    break;
                case 5:
                    logic += ' <span class="LogicTest">!=</span> ';
                    break;
            }
            logic += "'" + logics[i].logics[j].commonValue + "'";
            switch (logics[i].logics[j].link) {
                case 0:
                    break;
                case 1:
                    logic += ' <span class="LogicLink">&</span> ';
                    break;
                case 2:
                    logic += ' <span class="LogicLink">|</span> ';
                    break;
                case 3:
                    logic += ' <span class="LogicLink">^</span> ';
                    break;
            }
        }
        logic += ")";
        switch (logics[i].link) {
            case 0:
                break;
            case 1:
                logic += ' <span class="LogicLink">&</span> ';
                break;
            case 2:
                logic += ' <span class="LogicLink">|</span> ';
                break;
            case 3:
                logic += ' <span class="LogicLink">^</span> ';
                break;
        }
        logic += "</span></div>";
    }
    logic += '<div><span class="Command">THEN</span><div>';
    for (var i = 0; i < thens.length; i++) {
        logic += '<div class="Command">';
        logic += '<span class="LogicItem LogicGroup" onclick="insertThen(' + i + ')"><span onclick="deleteThenCommand(' + i + ', event)" class="DeleteItem"><span class="glyphicon glyphicon-remove-circle"></span></span>';
        switch (thens[i].command) {
            case 0:
                logic += 'PageSkip';
                break;
            case 1:
                logic += 'SetVar';
                break;
            case 2:
                logic += 'DisplayText';
                break;
            case 3:
                logic += 'Hide';
                break;
            case 4:
                logic += 'Show';
                break;
        }
        logic += ' (';
        for (var j = 0; j < thens[i].param.length; j++) {
            logic += thens[i].param[j];
            if (j < thens[i].param.length - 1) {
                logic += ", "
            }
        }
        logic += ")";
        logic += '</span></div>'
    }
    logic += '<div><span class="Command">ELSE</span><div>';
    for (var i = 0; i < elses.length; i++) {
        logic += '<div class="Command">';
        logic += '<span class="LogicItem LogicGroup" onclick="insertElse(' + i + ')"><span onclick="deleteElseCommand(' + i + ', event)" class="DeleteItem"><span class="glyphicon glyphicon-remove-circle"></span></span>';
        switch (elses[i].command) {
            case 0:
                logic += 'PageSkip';
                break;
            case 1:
                logic += 'SetVar';
                break;
            case 2:
                logic += 'DisplayText';
                break;
            case 3:
                logic += 'Hide';
                break;
            case 4:
                logic += 'Show';
                break;
        }
        logic += ' (';
        for (var j = 0; j < elses[i].param.length; j++) {
            logic += elses[i].param[j];
            if (j < elses[i].param.length - 1) {
                logic += ", "
            }
        }
        logic += ")";
        logic += '</span></div>'
    }


    $('#logicView').html(logic);
    $('.DeleteItem').unbind('mouseenter');
    $('.DeleteItem').unbind('mouseleave');
    $('.LogicItem').on('mouseenter', function () {
        $(this).children('.DeleteItem').show();
    });
    $('.LogicItem').on('mouseleave', function () {
        $(this).children('.DeleteItem').hide();
    });
}

function saveLogic() {
    var info = { id: lid, company: company };
    var incoming = $('#RunFirst').is(':checked');
    $('#save').val('Saving...');
    $.ajax({
        url: "SaveLogic",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        traditional: true,
        data: JSON.stringify({ info: info, logics: logics, thens: thens, elses: elses, incoming: incoming }),
        success: function (data) {
            if (data.success) {
                $('#save').removeClass('btn-default').addClass('btn-success');
                $('#save').val("Saved");
                setTimeout(function () { $('#save').removeClass('btn-success').addClass('btn-defalut').val('Save'); }, 5000);
            } else {
                $('#save').removeClass('btn-default').addClass('btn-warning');
                $('#save').val('Error')
                setTimeout(function () { $('#save').removeClass('btn-warning').addClass('btn-defalut').val('Save'); }, 5000);
            }
        },
        error: function (data) {
            $('#save').removeClass('btn-default').addClass('btn-warning');
            $('#save').val('Error')
            setTimeout(function () { $('#save').removeClass('btn-warning').addClass('btn-defalut').val('Save'); }, 5000);
        },
        type: 'POST'
    });

}