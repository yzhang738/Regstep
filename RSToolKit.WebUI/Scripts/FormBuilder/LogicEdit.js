/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var changed = false;

$(document).ready(function () {

    $("input").on("click", function () { changed = true; });

    $('#DeleteLogicGroup').hide();
    $('#NewLogic').hide();
    $('#DeleteLogic').hide();
    $('#NewCommandThen').hide();
    $('#DeleteCommandThen').hide();
    $('#NewCommandElse').hide();
    $('#DeleteCommandElse').hide();

    $('#VariableLightBox').dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function () {
            var form = $('#FormSelect').val();
            var variable = $('#VariableSelect').val();
            var commonName = $('#VariableSelect option:selected').text();
            var group = $("#CurrentGroup").val();
            var statement = $("#CurrentStatement").val();
            $('#LGS' + group + '-' + statement).children('input[name$=commonvariable]').val(commonName);
            $('#LGS' + group + '-' + statement).children('input[name$=form]').val(form);
            $('#LGS' + group + '-' + statement).children('input[name$=Variable]').val(form + '=>' + variable);
            UpdateView();
        }
    });

    $('#ValueLightBox').dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function () {
            var form = $('#FormSelectValue').val();
            var variable = $('#VariableSelectValue').val();
            var commonName = $('#VariableSelectValue option:selected').text();
            var value = $('#ValueText').val();
            var group = $("#CurrentGroup").val();
            var statement = $("#CurrentStatement").val();
            if (value == '_NA_') {
                $('#LGS' + group + '-' + statement).children('input[name$=valueform]').val(form);
                $('#LGS' + group + '-' + statement).children('input[name$=Value]').val(form + '=>' + variable);
                $('#LGS' + group + '-' + statement).children('input[name$=commonvalue]').val(commonName);
            } else {
                $('#LGS' + group + '-' + statement).children('input[name$=valueform]').val('');
                $('#LGS' + group + '-' + statement).children('input[name$=Value]').val("pt=>" + value);
                $('#LGS' + group + '-' + statement).children('input[name$=commonvalue]').val(value);
            }
            UpdateView();
        }
    });

    $('#FormSelect').change(function () {
        updateVariables($(this).val());
    });

    $('#FormSelectValue').change(function () {
        updateVariablesVal($(this).val());
    });

    $('#SetVarFormSelect').change(function () {
        updateVariablesSetVar($(this).val());
    });


    // Tie in click events

    UpdateView();

    $('#DeleteCommandElse').click(function () {
        var command = $('#CurrentCommand').val();
        $('#E' + command).remove();
        Reorder();
        UpdateView();
    });

    $('#DeleteCommandThen').click(function () {
        var command = $('#CurrentCommand').val();
        $('#T' + command).remove();
        Reorder();
        UpdateView();
    });

    $('#NewCommandThen').click(function () {
        var thenNumb = 0;
        $('.ThenCommand').each(function (i) {
            thenNumb++;
        });
        var info = '<span class="ThenCommand" id="T' + thenNumb + '"><input type="hidden" name="Then[' + thenNumb + '].Command" value="0" />';
        $('#Then').append(info);
        UpdateView();
    });

    $('#NewCommandElse').click(function () {
        var thenNumb = 0;
        $('.ElseCommand').each(function (i) {
            thenNumb++;
        });
        var info = '<span class="ElseCommand" id="T' + thenNumb + '"><input type="hidden" name="Else[' + thenNumb + '].Command" value="0" />';
        $('#Else').append(info);
        UpdateView();
    });

    $('#ValueLB').click(function () {
        $('#ValueLightBox').dialog('open');
    });

    $('#DeleteLogicGroup').on('click', function () {
        var group = $("#CurrentGroup").val();
        $('#LGF' + group).remove();
        Reorder();
        UpdateView();
    });

    $('#DeleteLogic').on('click', function () {
        var group = $("#CurrentGroup").val();
        var statement = $("#CurrentStatement").val();
        $('#LGS' + group + '-' + statement).remove();
        Reorder();
        UpdateView();
    });

    $('#VariableLB').click(function () {
        $('#VariableLightBox').dialog('open');
    });

    $('#NewLogicGroup').on('click', function () {
        var logicGroupNumb = 0;
        $('.LogicFields').each(function (i) {
            if ($(this).children('input[name$=LogicLink]').val() == 0) {
                $(this).children('input[name$=LogicLink]').val('1');
            }
            logicGroupNumb++;
        });
        var info = '<div class="LogicFields" id="LGF' + logicGroupNumb + '"><input type="hidden" name="Model.LogicGroup[' + logicGroupNumb + '].LogicLink" value="0"></span>';
        $('#ModelInformation').append(info);
        UpdateView();
    });

    $('#NewLogic').on('click', function () {
        var group = currentGroup;
        var statement = 0;
        $('#LGF' + group).children(".LogicStatements").each(function (i) {
            if ($(this).children('input[name$=LogicLink]').val() == 0) {
                $(this).children('input[name$=LogicLink]').val(1);
            }
            statement++;
        });
        var info = '<div class="LogicStatements" id="LGS' + group + '-' + statement + '">';
        info += '<input type="hidden" name="commonvariable" value="NOT SET" />';
        info += '<input type="hidden" name="form" value="' + form + '" />';
        info += '<input type="hidden" name="commonvalue" value="NOT SET" />';
        info += '<input type="hidden" name="formvalue" value="' + form + '" />';
        info += '<input type="hidden" name="LogicGroup[' + group +'].LogicStatement[' + statement + '].Variable" value="' + form + '!NONE"/>';
        info += '<input type="hidden" name="LogicGroup[' + group +'].LogicStatement[' + statement + '].LogicTest" value="0" />';
        info += '<input type="hidden" name="LogicGroup[' + group + '].LogicStatement[' + statement + '].Value" value="NOT SET" />';
        info += '<input type="hidden" name="LogicGroup[' + group + '].LogicStatement[' + statement + '].LogicLink" value="0" />';
        $('#LGF' + group).append(info);
        UpdateView();
    });

    //End click event tie ins

});

var logicTest = ['==', '>', '>=', '<', '<=', '!='];
var logicLink = ['', '&', '|', '^'];
var currentGroup = -1;
var currentStatement = -1;
var statementClick = false;
var commandClick = false;

function UpdateView() {
    var logic = '<span class="LogicStatementTop">if</span>';
    $('.LogicFields').each(function (i) {
        logic += '<span class="LogicGroup" id="lg' + i + '" data-group="' + i + '" data-statement="-1">(';
        $('.LogicStatements', this).each(function (j) {
            var variable = $('input[name=commonvariable]', this).val();
            var value = $('input[name$=commonvalue]', this).val();
            var test = logicTest[$('input[name$=LogicTest]', this).val()];
            var slink = logicLink[$('input[name$=LogicLink]', this).val()];
            logic += '<span class="LogicStatement" id="ls' + i + '-' + j + '" data-group="' + i + '" data-statement="' + j + '">' + variable + ' ' + test + ' ' + value + ' ' + slink + ' </span>';
        });
        var linkI = $(this).children('input[name$=LogicLink]').val();
        var link = logicLink[linkI];
        logic += ')&nbsp;&nbsp;' + link + '</span>';
    });
    logic += '<br /><div class="LogicThen"><span class="LogicThenLink">Then</span><br />';
    $('.ThenCommand').each(function (i) {
        var command = $('input[name$=Command]', this).val();
        logic += '<div id="ts' + i + '" class="ThenStatement" data-then="' + i + '">';
        logic += $('#CommandSelect').children('option[value=' + command + ']').text();
        logic += '(';
        var foundParam = false;
        $('input[name*=Params]', this).each(function (j) {
            foundParam = true;
            var param = $(this).val();
            logic += param + ',';
        });
        if (foundParam) {
            logic = logic.substring(0, logic.length - 1);
        }
        logic += ');</div>';
    });
    logic += '</div>';
    logic += '<br /><div class="LogicElse"><span class="LogicElseLink">Else</span><br />';
    $('.ElseCommand').each(function (i) {
        var command = $('input[name$=Command]', this).val();
        logic += '<div id="es' + i + '" class="ElseStatement" data-else="' + i + '">';
        logic += $('#CommandSelect').children('option[value=' + command + ']').text();
        logic += '(';
        var foundParam = false;
        $('.ElseParam').each(function (j) {
            foundParam = true;
            var param = $('input[name*=Params]', this).val();
            logic += param + ',';
        });
        if (foundParam) {
            logic = logic.substring(0, logic.length - 1);
        }
        logic += ');</div>';
    });
    logic += '</div>';
    $('#LogicView').html(logic);
    $('.LogicGroup').unbind();
    $('.LogicStatement').unbind();
    $('.LogicThen').unbind();
    $('.LogicElse').unbind();

    //Update click events
    $(".LogicGroup").on('click', function (e) {
        $('#LinkType').unbind();
        if (statementClick) {
            statementClick = false;
            return;
        }
        $('#DeleteLogicGroup').show();
        $('#NewLogic').show();
        $('#DeleteLogic').hide();
        $('#NewCommandThen').hide();
        $('#DeleteCommandThen').hide();
        $('#NewCommandElse').hide();
        $('#DeleteCommandElse').hide();
        $('#DeleteLogic').hide();
        $('#ThenCommand').hide();
        var group = $(this).attr('data-group');
        currentGroup = group;
        currentStatement = -1;
        var statement = -1;
        $("#CurrentGroup").val(group);
        $("#CurrentStatement").val(statement);
        $("#LogicStatement").hide();
        $("#LogicGroup").show();
        var link = $('#LGF' + group).children('input[name$=LogicLink]').val();
        $('#LinkType').val(link);
        $('#LinkType').change(function () {
            $('#LGF' + group).children('input[name$=LogicLink]').val($(this).val());
            UpdateView();
        });
    });

    $('.ThenStatement').on('click', function (e) {
        $('#DeleteLogicGroup').hide();
        $('#NewLogic').hide();
        $('#DeleteLogic').hide();
        $('#NewCommandThen').show();
        $('#DeleteCommandThen').show();
        $('#NewCommandElse').hide();
        $('#DeleteCommandElse').hide();
        $("#LogicStatement").hide();
        $("#LogicGroup").hide();
        $("#ThenCommand").show();
        $("#CommandSelect").unbind();
        $("#CurrentCommand").val($(this).attr('data-then'));
        $('#ElseOrThen').val('Then');
        var command = $('#T' + $(this).attr('data-then')).children('input[name$=Command]').val();
        commandSet(command);
        $("#CommandSelect").change(function (e) {
            commandSet($(this).val());
            UpdateView();
        });
        commandClick = true;
    });

    $('.ElseStatement').on('click', function (e) {
        $('#DeleteLogicGroup').hide();
        $('#NewLogic').hide();
        $('#DeleteLogic').hide();
        $('#NewCommandThen').hide();
        $('#DeleteCommandThen').hide();
        $('#NewCommandElse').show();
        $('#DeleteCommandElse').show();
        $("#LogicStatement").hide();
        $("#LogicGroup").hide();
        $("#ThenCommand").show();
        $("#CommandSelect").unbind();
        $("#CurrentCommand").val($(this).attr('data-else'));
        $('#ElseOrThen').val('Else');
        var command = $('#E' + $(this).attr('data-else')).children('input[name$=Command]').val();
        commandSet(command);
        $("#CommandSelect").change(function (e) {
            commandSet($(this).val());
            UpdateView();
        });
        commandClick = true;
    });

    $('.LogicThen').on('click', function (e) {
        if (commandClick) {
            commandClick = false;
            return;
        }
        $('#DeleteLogicGroup').hide();
        $('#NewLogic').hide();
        $('#DeleteLogic').hide();
        $('#NewCommandThen').show();
        $('#DeleteCommandThen').hide();
        $('#NewCommandElse').hide();
        $('#DeleteCommandElse').hide();
        $("#LogicStatement").hide();
        $("#LogicGroup").hide();
        $("#ThenCommand").hide();
        $('#ElseOrThen').val('Then');
    });

    $('.LogicElse').on('click', function (e) {
        if (commandClick) {
            commandClick = false;
            return;
        }
        $('#DeleteLogicGroup').hide();
        $('#NewLogic').hide();
        $('#DeleteLogic').hide();
        $('#NewCommandThen').hide();
        $('#DeleteCommandThen').hide();
        $('#NewCommandElse').show();
        $('#DeleteCommandElse').hide();
        $("#LogicStatement").hide();
        $("#LogicGroup").hide();
        $("#ThenCommand").hide();
        $('#ElseOrThen').val('Else');
    });

    $('.LogicStatement').on('click', function (e) {
        $('#DeleteLogicGroup').show();
        $('#NewLogic').show();
        $('#DeleteLogic').show();
        $('#NewCommandThen').hide();
        $('#DeleteCommandThen').hide();
        $('#NewCommandElse').hide();
        $('#DeleteCommandElse').hide();
        $('#TestType').unbind();
        $('#StatementLink').unbind();
        var group = $(this).attr('data-group');
        currentGroup = group;
        var statement = $(this).attr('data-statement');
        currentStatement = -1;
        $("#CurrentGroup").val(group);
        $("#CurrentStatement").val(statement);
        $("#LogicStatement").show();
        $("#LogicGroup").hide();
        $("#ThenCommand").hide();
        var test = $('#LGS' + group + '-' + statement).children('input[name$=LogicTest]').val();
        var link = $('#LGS' + group + '-' + statement).children('input[name$=LogicLink]').val();
        var variableFull = $('#LGS' + group + '-' + statement).children('input[name$=Variable]').val();
        var valueFull = $('#LGS' + group + '-' + statement).children('input[name$=Value]').val();
        var rgxTest = /^(.*)=>(.*)$/g;
        result = rgxTest.exec(variableFull);
        var variable = RegExp.$2;
        var curForm = RegExp.$1;
        var rgxTest2 = /^(.*)=>(.*)$/g;
        result = rgxTest2.exec(valueFull);
        var value = RegExp.$2;
        var valueForm = RegExp.$1;
        $('#TestType').val(test);
        $('#StatementLink').val(link);
        $('#ValueText').val('_NA_');
        if (valueForm == 'pt') {
            $('#ValueText').val(value);
        }
        $('#TestType').change(function () {
            $('#LGS' + group + '-' + statement).children('input[name$=LogicTest]').val($(this).val());
            UpdateView();
        });
        $('#StatementLink').change(function () {
            $('#LGS' + group + '-' + statement).children('input[name$=LogicLink]').val($(this).val());
            UpdateView();
        });
        $('#FormSelect').val(curForm);
        $("#VariableSelect").html("");
        $.ajax({
            url: "GetFormVariables?cids=" + company + "&fids=" + curForm,
            success: function (data) {
                for (var i = 0; i < data.id.length; i++) {
                    $("#VariableSelect").append('<option value="' + data.id[i] + '">' + data.variable[i] + '</option>');
                }
                $('#VariableSelect').val(variable);
            },
            type: 'GET'
        });
        $('#FormSelectValue').val(valueForm);
        $("#VariableSelectValue").html("");
        $.ajax({
            url: "GetFormVariables?cids=" + company + "&fids=" + valueForm,
            success: function (data) {
                for (var i = 0; i < data.id.length; i++) {
                    $("#VariableSelectValue").append('<option value="' + data.id[i] + '">' + data.variable[i] + '</option>');
                }
                $('#VariableSelectValue').val(value);
            },
            type: 'GET'
        });
        statementClick = true;
    });
}

function Reorder() {
    $('.LogicFields').each(function (i) {
        $('.LogicStatements', this).each(function (j) {
            $(this).attr('id', 'LGS' + i + '-' + j);
            $('input[name$=Value]', this).attr('name', 'LogicGroup[' + i + '].LogicStatement[' + j + '].Value');
            $('input[name$=LogicTest]', this).attr('name', 'LogicGroup[' + i + '].LogicStatement[' + j + '].LogicTest');
            $('input[name$=Variable]', this).attr('name', 'LogicGroup[' + i + '].LogicStatement[' + j + '].Variable');
            $('input[name$=LogicLink]', this).attr('name', 'LogicGroup[' + i + '].LogicStatement[' + j + '].LogicLink');
            $('input[name$=LogicTest]', this).attr('name', 'LogicGroup[' + i + '].LogicStatement[' + j + '].LogicTest');
        });
        $(this).attr('id', 'LGF' + i);
        $('input[name$=LogicLink]', this).attr('name', 'LogicGroup[' + i + '].LogicLink');
    });
    $('.ThenCommand').each(function (i) {
        $('input[name$=Command]', this).attr('name', 'Then[' + i + '].Command');
        $('.ThenParam').each(function (j) {
            $('input[name$=Params]', this).attr('name', 'Then[' + i + '].Params[' + j + ']');
        });
    });
    $('.ElseCommand').each(function (i) {
        $('input[name$=Command]', this).attr('name', 'Else[' + i + '].Command');
        $('.ElseParam').each(function (j) {
            $('input[name$=Params]', this).attr('name', 'Else[' + i + '].Params[' + j + ']');
        });
    });
}

function updateVariables(curForm) {
    $("#VariableSelect").html("");
    $.ajax({
        url: "GetFormVariables?cids=" + company + "&fids=" + curForm,
        success: function (data) {
            for (var i = 0; i < data.id.length; i++) {
                $("#VariableSelect").append('<option value="' + data.id[i] + '">' + data.variable[i] + '</option>');
            }
        },
        type: 'GET'
    });
}

function updateVariablesVal(curForm) {
    $("#VariableSelectValue").html("");
    $.ajax({
        url: "GetFormVariables?cids=" + company + "&fids=" + curForm,
        success: function (data) {
            for (var i = 0; i < data.id.length; i++) {
                $("#VariableSelectValue").append('<option value="' + data.id[i] + '">' + data.variable[i] + '</option>');
            }
        },
        type: 'GET'
    });
}

function updateVariablesSetVar(curForm, select) {
    $("#SetVarSelect").html("");
    $("#SetVarFormSelect").val(curForm);
    $.ajax({
        url: "GetFormVariables?cids=" + company + "&fids=" + curForm,
        success: function (data) {
            for (var i = 0; i < data.id.length; i++) {
                $("#SetVarSelect").append('<option value="' + data.id[i] + '">' + data.variable[i] + '</option>');
            }
            $("#SetVarSelect").val(select);
        },
        type: 'GET'
    });
}

function commandSet(val) {
    $('#CommandParams').children('div').each(function () {
        $(this).appendTo('#HiddenForms');
    });
    var int = val;
    $('#CommandSelect').val(val);
    var type = $('#ElseOrThen').val();
    var prefix = "T";
    if (type == 'Else') {
        prefix = 'E';
    }
    var currentCommand = $("#CurrentCommand").val();
    var parent = $('#' + prefix + currentCommand);
    $('input[name$=Command]', parent).val($('#CommandSelect').val());
    switch (val) {
        case '0':
            $('input[name*=Params]', parent).each(function (i) {
                $(this).remove();
            });
            break;
        case '1':
            var fullVariable = $('input[name$="Params[0]"]', parent).val();
            var rgxTest = /^(.*)=>(.*)$/g;
            var result = rgxTest.exec(fullVariable);
            updateVariablesSetVar(RegExp.$1, RegExp.$2);
            $('#SetVarText').val($('input[name$="Params[1]"]', parent).val());
            $('#SetVarForm').appendTo('#CommandParams');
            $('#SetVarText').on('blur', function () {
                $('input[name*=Params]', parent).each(function (i) {
                    $(this).remove();
                });
                var int = currentCommand;
                $(parent).append('<input type="hidden" name="' + type + '[' + int + '].Params[0]" value="' + $('#SetVarFormSelect').val() + '=>' + $('#SetVarSelect').val() + '" />');
                $(parent).append('<input type="hidden" name="' + type + '[' + int + '].Params[1]" value="' + $(this).val() + '" />');
                UpdateView();
            });
            break;
        case '2':
            var customText = $('input[name$="Params[0]"]', parent).val();
            $('#SetCustomText').val($('input[name$="Params[0]"]', parent).val());
            $('#DisplayText').appendTo('#CommandParams');
            $('#DisplayText').on('blur', function () {
                $('input[name*=Params]', parent).each(function (i) {
                    $(this).remove();
                });
                var int = currentCommand;
                $(parent).append('<input type="hidden" name="' + type + '[' + int + '].Params[0]" value="' + $('#SetCustomText').val() + '" />');
                UpdateView();
            });
            break;
    }
}