$(document).ready(function () {

    var statementVariable = $('#variableSelect');

    $('#accordion').collapse();
    var currentGroup = "";
    var currentStatement = "";
    var currentCommand = "";
    var currentVariable = "";
    var currentValue = "";
    var currentForm = "";
    var loadedItems = {};
    var loadedForms = {};


    function hasOwnProperty(obj, prop) {
        var proto = obj.__proto__ || obj.constructor.prototype;
        return (prop in obj) &&
            (!(prop in proto) || proto[prop] !== obj[prop]);
    }

    function RemoveGroupLink(j_group) {
        j_group.children('input.logic-group-link').val('0');
        j_group.children('span.logic-group-link').html('');
    }

    function RemoveLogicLink(j_group) {
        j_group.children('.logic-statements').last().children('input.logic-statement-link').val('0');
        j_group.children('.logic-statements').last().children('span.logic-statement-link').html('');
    }

    function ResetOrdering() {
        $('.logic-group').each(function (i) {
            var group = $(this);
            group.children('div.logic-group-label').html('Logic Group ' + (i + 1));
            group.children('input.logic-group-uid').attr('name', 'LogicGroups[' + i + '].UId');
            group.children('input.logic-group-link').attr('name', 'LogicGroups[' + i + '].Link');
            group.children('.logic-statement').each(function (j) {
                var statement = $(this);
                statement.children('input.logic-statement-uid').attr('name', 'LogicGroups[' + i + '].LogicStatements[' + j + '].UId');
                statement.children('input.logic-statement-value').attr('name', 'LogicGroups[' + i + '].LogicStatements[' + j + '].Value');
                statement.children('input.logic-statement-variable').attr('name', 'LogicGroups[' + i + '].LogicStatements[' + j + '].Variable');
                statement.children('input.logic-statement-formkey').attr('name', 'LogicGroups[' + i + '].LogicStatements[' + j + '].FormKey');
                statement.children('input.logic-statement-test').attr('name', 'LogicGroups[' + i + '].LogicStatements[' + j + '].Test');
                statement.children('input.logic-statement-link').attr('name', 'LogicGroups[' + i + '].LogicStatements[' + j + '].Link');
            });
        });

        $('.logic-command').each(function (i) {
            $(this).find('input.logic-command-uid').attr('name', 'Commands[' + i + '].UId');
            $(this).find('input.logic-command-command').attr('name', 'Commands[' + i + '].Command');
            $(this).find('.logic-command-parameter').each(function (j) {
                $(this).find('.logic-command-paramter-key').attr('name', 'Commands[' + i + '].Paramters[' + j + '].Key');
                $(this).find('.logic-command-paramter-value').attr('name', 'Commands[' + i + '].Paramters[' + j + '].Value');
            });
        });
    }

    function SetVariables(j_select, formKey, variable) {
        if (variable === null || variable === undefined) {
            variable = "";
        }
        if (!hasOwnProperty(loadedForms, formKey)) {
            processing.showPleaseWait();
            $.ajax({
                url: "../../FormBuilder/FormVariables",
                data: { id: formKey },
                type: "get",
                dataType: "json",
                traditional: true,
                success: function (result) {
                    if (result.Success) {
                        j_select.html('');
                        loadedForms[result.UId] = result.Variables;
                        j_select.append('<option>Select a Variable</option>');
                        for (var i = 0; i < result.Variables.length; i++) {
                            j_select.append('<option value="' + result.Variables[i].UId + '" data-type="' + result.Variables[i].Type + '"' + (variable == result.Variables[i].UId ? ' selected="true"' : '') + '">' + result.Variables[i].Name + '</option>');
                        }
                        processing.hidePleaseWait();
                    } else {
                        j_select.html('<option value="">Error</option>');
                        processing.hidePleaseWait();
                        alert('There was an error processing the request.');
                    }
                },
                error: function (result) {
                    j_select.html('<option value="">Error</option>');
                    processing.hidePleaseWait();
                    alert('There was an error processing the request.');
                }
            });
        } else {
            j_select.append('<option>Select a Variable</option>')
            for (var i = 0; i < loadedForms[formKey].length; i++) {
                j_select.append('<option value="' + loadedForms[formKey][i].UId + '">' + loadedForms[formKey][i].Name + '</option>');
            }
        }

    }

    function SetValue(j_swap, j_select, a_id, d_action) {
        //Grab uid of variable selected
        var variable = j_select.val();
        var currentVariable = variable; //Legacy code

        //Check the input type.
        switch (j_select.children('option:selected').attr('data-type')) {
            case "checkboxgroup":
            case "radiogroup":
            case "dropdowngroup":
            case "audience":
            case "status":
            case "rsvp":
                //See if the items have already been fetched.
                if (!hasOwnProperty(loadedItems, variable)) {
                    processing.showPleaseWait();
                    //Fetch variable items
                    $.ajax({
                        url: "../../FormBuilder/GetComponentItems/" + currentVariable,
                        data: { form: currentForm },
                        type: "GET",
                        dataType: "json",
                        traditional: true,
                        success: function (result) {
                            input = $('<select class="form-control" id="' + a_id + '"></select>');
                            input.append('<option>Pick a Variable</option>');
                            for (var j = 0; j < result.Items.length; j++) {
                                var itemSelected = "";
                                if (result.Items[j].UId == currentValue)
                                    itemSelected = ' selected="true"';
                                input.append('<option value="' + result.Items[j].UId + '"' + itemSelected + '>' + result.Items[j].Name + '</option>');
                            }
                            $(j_swap).replaceWith(input);
                            input.on('input', function () {
                                d_action($(this));
                            });
                            processing.hidePleaseWait();
                        },
                        error: function (result) {
                            processing.hidePleaseWait();
                            alert('Error Loading Component Items');
                        }
                    });
                } else {
                    var input = $('<select id="' + a_id + '" class="form-control"><option>Pick a Variable</option><select>');
                    for (var i = 0; i < loadedItems[variable].length; i++) {
                        input.append('<option value="' + loadedItems[variable].UId + '">' + loadedItems[variable].Name + '</option>');
                    }
                    j_swap.replaceWith(input);
                    input.on('input', function () {
                        d_action($(this));
                    });
                }
                break;
            default:
                input = '<input type="text" id="' + a_id + '" value="' + currentValue + '"/>';
                j_swap.replaceWith(input);
                $('#' + a_id).on('input', function () {
                    d_action($(this));
                });
                break;
        }

    }

    //This deletes the current logic group if one is selected and reorders the indices for model binding.
    $('#deleteGroup').on('click', function () {
        // If no group selected, do nothing
        if (currentGroup == "")
            return;

        // Grab the group div id and see if it exists. Do nothing if it does not exist.
        var groupDiv = $('#' + currentGroup);
        if (groupDiv == null)
            return;

        //Remove the group
        groupDiv.remove();

        // Reset logic group ordering

        // Grab the last logic group and remove the link
        var lastGroup = $('.logic-groups').last();
        RemoveGroupLink(lastGroup);

        //Reset Group and Statement
        currentGroup = "";
        currentStatement = "";
    });

    //This deletes the current statement if one is selected and reorders the indices for model binding.
    $('#deleteStatement').on('click', function () {
        // Checks to see if there is a group selected. If not or cannot find it, do nothing
        if (currentGroup == "")
            return;
        var groupDiv = $('#' + currentGroup);
        if (groupDiv === null)
            return;

        // Checks to see if there is a statement selected. If not or cannot find it, do nothing.
        if (currentStatement == "")
            return;
        var statementDiv = $('#' + currentStatement);
        if (statementDiv === null)
            return;

        //Remove the statement
        statementDiv.remove();

        // Reorder
        ResetOrdering()

        // Remove last statment link
        RemoveLogicLink(groupDiv);

        // Clear current statement
        currentStatement = "";
    });

    //This deletes the current statement if one is selected and reorders the indices for model binding.
    $('#deleteCommand').on('click', function () {
        // Checks to see if there is a command selected. If not or cannot find it, do nothing
        if (currentCommand == "")
            return;
        var commandDiv = $('#' + currentCommand);
        if (commandDiv === null)
            return;

        //Remove the statement
        commandDiv.remove();

        // Reorder
        ResetOrdering()

        // Remove last statment link
        RemoveLogicLink(groupDiv);

        // Clear current statement
        currentStatement = "";
    });

    //This adds a new group to the logic and places it at the end of the site.
    $('#addGroup').on('click', function () {
        // Get a new guid.
        var uid = guid();

        // Get the next ordering number
        var i = $('.logic-group').length;

        // Generate the html.
        var div = '<div class="logic-group" id="' + uid + '"><div class="logic-group-label">Logic Group ' + (i + 1) + '</div><input type="hidden" class="logic-group-uid" value="' + emptyGuid + '" name="LogicGroups[' + i + '].UId" /><input type="hidden" class="logic-group-link" name="LogicGroups[' + i + '].Link" value="0" />';
        div += '<input type="hidden" class="logic-group-order" name="LogicGroups[' + i + '].Order" value="' + (i + 1) + '" />';
        div += '<div class="logic-statements"></div><span class="logic-group-link"></span></div>';

        //Write the html to the DOM
        $('.logic-groups').append(div);

        //Set current group and reset statment
        currentGroup = uid;
        currentStatement = "";

        // Bind group to click
        $('#' + uid).on('click', function (e) {
            e.stopPropagation();
            groupClick(this);
        })

    });

    //This adds a new statement to the currenty selected logic group if any.
    $('#addStatement').on('click', function () {

        //Get new Guid
        var uid = guid();

        // Check if there is a selected group. If not or cannot find it, do nothing.
        if (currentGroup == "")
            return;
        var groupDiv = $('#' + currentGroup);
        if (groupDiv === null)
            return;
        //Get the ordering index for the group.
        var name = groupDiv.children('.logic-group-uid').attr('name');
        var regex = /^LogicGroups\[(\d+)\].*$/;
        var match = regex.exec(name);
        var i = match[1];

        // Grab the next ordering index for statements in the group
        var j = groupDiv.find('.logic-statement').length;

        // Set the order for the class to j + 1;
        var order = j + 1;

        // Generate the html
        var div = '<div class="logic-statement" id="' + uid + '">';
        div += '<input type="hidden" class="logic-statement-uid" name="LogicGroups[' + i + '].LogicStatements[' + j + '].UId" value="' + uid + '" />';
        div += '<input type="hidden" class="logic-statement-value" name="LogicGroups[' + i + '].LogicStatements[' + j + '].Value" value="" />';
        div += '<input type="hidden" class="logic-statement-variable" name="LogicGroups[' + i + '].LogicStatements[' + j + '].Variable" value="" />';
        div += '<input type="hidden" class="logic-statement-formkey" name="LogicGroups[' + i + '].LogicStatements[' + j + '].FormKey" value="" />';
        div += '<input type="hidden" class="logic-statement-test" name="LogicGroups[' + i + '].LogicStatements[' + j + '].Test" value="0" />';
        div += '<input type="hidden" class="logic-statement-link" name="LogicGroups[' + i + '].LogicStatements[' + j + '].Link" value="0" />';
        div += '<input type="hidden" class="logic-statement-order" name="LogicGroups[' + i + '].LogicStatements[' + j + '].Order" value="' + order + '" />';
        div += '<span class="logic-statement-variable">EMPTY</span><span class="logic-statement-test">==</span><span class="logic-statement-value">EMPTY</span><span class="logic-statement-link"></span>';
        div += '</div>';

        // Add html to the DOM
        groupDiv.children('.logic-statements').append(div);

        // Bind click event
        $('#' + uid).on('click', function (e) {
            e.stopPropagation();
            statementClick(this);
        })

        //Set the current statement
        currentStatement = uid;
    });

    //This adds a then new comamnd to the logic
    $('#addThenCommand').on('click', function () {
        //Generate new guid
        var id = guid();

        // Grab the next ordering index;
        var i = $('.logic-command').length;

        // Generate the html
        var html = '<div class="logic-command" id="' + id + '">';
        html += '<input type="hidden" class="logic-command-uid" name="Commands[' + i + '].UId" value="' + emptyGuid + '"/>';
        html += '<input type="hidden" class="logic-command-command" name="Commands[' + i + '].Command" value=""/>';
        html += '<input type="hidden" class="logic-command-commandType" name="Commands[' + i + '].CommandType" value="0"/>';
        html += '<span class="logic-command-command">Select Command</span><span class="logic-command-parameters">';
        html += '</span>';
        html += '</div>';

        // Add html to DOM
        $('div.logic-commands-then').append(html);

        //Bind the click event
        $('#' + id).on('click', function (e) {
            e.stopPropagation();
            commandClick(this);
        })
    });

    //This adds a else new comamnd to the logic
    $('#addElseCommand').on('click', function () {
        //Generate new guid.
        var id = guid();

        // Grab the next ordering index
        var i = $('.logic-command').length;

        // Generate html
        var html = '<div class="logic-command" id="' + id + '">';
        html += '<input type="hidden" class="logic-command-uid" name="Commands[' + i + '].UId" value="' + emptyGuid + '"/>';
        html += '<input type="hidden" class="logic-command-command" name="Commands[' + i + '].Command" value=""/>';
        html += '<input type="hidden" class="logic-command-commandType" name="Commands[' + i + '].CommandType" value="1"/>';
        html += '<span class="logic-command-command">Select Command</span><span class="logic-command-parameters">';
        html += '</span>';
        html += '</div>';

        //Add html to DOM
        $('div.logic-commands-else').append(html);

        //Bind click event
        $('#' + id).on('click', function (e) {
            e.stopPropagation();
            commandClick(this);
        })
    });

    //Binds logic statements to a click.
    $('.logic-statement').on('click', function (e) {
        e.stopPropagation();
        //Run statement click function
        statementClick(this);
    });

    //Binds group to a click.
    $('.logic-group').on('click', function (e) {
        e.stopPropagation();
        //Run group click function
        groupClick(this);
    });

    //Binds command to a click.
    $('.logic-command').on('click', function (e) {
        e.stopPropagation();
        commandClick(this);
    });

    //Form change event.
    $('#formSelect').on('change', function () {
        formChanged();
    });

    //Binds to change in group link selection control.
    $('#groupLinkSelect').on('change', function (e) {
        //Grabs the div of the current group
        var group = $('#' + currentGroup);

        //Grabs the text of the selected item
        var name = $(this).children(':selected').text();

        //If link is None, set name to empty string.
        if (name == "None")
            name = "";

        //Update the group link control and html
        group.children('input.logic-group-link').val($(this).val());
        group.children('span.logic-group-link').html(name);
    });

    //Binds to change in statement link control.
    $('#statementLinkSelect').on('change', function (e) {
        //Grab the statement div
        var statement = $('#' + currentStatement);

        //sets the statement link control.
        statement.children('input.logic-statement-link').val($(this).val());

        //Create the html codes for link values
        var link = ['', '&#38;', '&#124;', '&#94;'];

        //Set the html value of the link
        statement.children('span.logic-statement-link').html(link[parseInt($(this).val())]);
    });

    //Binds to input of statement test.
    $('#testSelect').on('input', function () {
        //Grab the statement div
        var statement = $('#' + currentStatement);

        //Set the test for the control and the html
        statement.children('input.logic-statement-test').val($(this).val());
        statement.children('span.logic-statement-test').html($(this).children('option:selected').text());
    });

    //Binds to change of the select command.
    $('#commandSelect').on('change', function () {
        // Grabs the current command div
        var command = $('#' + currentCommand);

        //Grabs the command type and the command text
        var commandType = $(this).val();
        var commandText = $(this).children(':selected').text()

        // sets the command type on the control and html
        command.children('input.logic-command-command').val(commandType);
        command.children('span.logic-command-command').html(commandText);

        //Resets the params
        var params = {};

        //Find all the old params and sets them to param
        command.find('.logic-command-parameter').each(function () {
            var param = $(this);
            var key = param.find('.logic-command-parameter-key').val();
            var value = param.find('.logic-command-parameter-value').val();
            params[key] = value;
        });

        //Remove the paramater html values
        command.children('.logic-command-parameters').html('');

        //Initializes variable for the input and parameter div.
        var newParamDiv = '';
        var input = '';

        //Grabs the name of the logic command div and extracts the ordering index
        var name = command.children('.logic-command-uid').attr('name');
        var regex = /^Commands\[(\d+)\].*$/;
        var match = regex.exec(name);
        var ci = match[1];

        //Find which parameters to initiate.
        switch (commandType) {
            case "1":
                //Case 1: Set Variable

                //Grabs old values of the parameters if they exist.
                var form = params["Form"];
                var variable = params["Variable"];
                var value = params["Value"];

                //Generates the html
                newParamDiv = '<span class="logic-command-parameter" data-key="Form">';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[0].Key" class="logic-command-parameter-key" value="Form"/>';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[0].Value" data-key="Form" class="logic-command-parameter-value" value="' + form + '"/>';
                newParamDiv += '<span class="logic-command-parameter-view" data-key="Form">Form: ' + params["Form"] + '</span>';
                newParamDiv += '</span>';
                newParamDiv += '<span class="logic-command-parameter" data-key="Variable">';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[1].Key" class="logic-command-parameter-key" value="Variable"/>';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[1].Value" data-key="Variable" class="logic-command-parameter-value" value="' + variable + '"/>';
                newParamDiv += '<span class="logic-command-parameter-view" data-key="Variable">Variable: ' + params["Variable"] + '</span>';
                newParamDiv += '</span>';
                newParamDiv += '<span class="logic-command-parameter" data-key="Value">';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[2].Key" class="logic-command-parameter-key" value="Value"/>';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[2].Value" data-key="Value" class="logic-command-parameter-value" value="' + value + '"/>';
                newParamDiv += '<span class="logic-command-parameter-view" data-key="Value">Value: ' + params["Value"] + '</span>';
                newParamDiv += '</span>';

                //Generate the input html
                input += '<div class="input-group"><div class="input-group-addon">Form</div><select class="form-control" id="commandForm">';

                //Find the form and sets it to the top of the select.
                var topOption = "";
                var options = "";
                for (var i = 0; i < companyForms.length; i++) {
                    var selected = "";
                    if (form == companyForms[i].UId)
                        selected = ' selected="true"';
                    if (companyForms[i].UId == formId)
                        topOption = '<option value="' + companyForms[i].UId + '"' + selected + '>' + companyForms[i].Name + ' (current)</option>';
                    else
                        options += '<option value="' + companyForms[i].UId + '"' + selected + '>' + companyForms[i].Name + '</option>';
                }
                input += "<option>Pick a Form</option>" + topOption + options;
                input += '</select></div></div>';

                //Finish generating html input
                input += '<div class="input-group"><div class="input-group-addon">Variable</div><select class="form-control" id="commandVariable"><option>Select Form</option></select></div></div>';
                input += '<div class="input-group"><div class="input-group-addon">Value</div><input class="form-control" type="text" id="commandValue"></div></div>';

                //Append the controls
                $('#commandParameters').html(input);
                if (typeof (form) !== 'undefined' && form !== null) {
                    SetVariables($('#commandVariable'), form, variable);
                }
                $('#commandValue').val(value);

                //Append the parameter html
                command.children('.logic-command-parameters').html(newParamDiv);

                //Bind to command Form
                $('#commandForm').on('change', function () {
                    var formKey = $(this).val();
                    var currentForm = formKey;
                    var select = $('#commandVariable');
                    select.html('<option value="">Loading</option>');
                    SetVariables(select, formKey);
                    command.find('input[data-key="Form"]').val($(this).val());
                    command.find('span.logic-command-parameter[data-key="Form"]').find('.logic-command-parameter-view').html('Form: ' + $(this).children('option:selected').text());
                });

                //Bind to Variable
                $('#commandVariable').on('change', function () {
                    var select = $('#commandVariable');
                    var input = "";
                    var swap = $('#commandValue');
                    SetValue(swap, $(this), 'commandValue', function (input) {
                        var value = $(input).val();
                        var text = value;
                        if (input.is('select')) {
                            text = input.children('option:selected').text();
                        }
                        command.find('input[data-key="Value"]').val(value);
                        command.find('span.logic-command-parameter[data-key="Value"]').find('.logic-command-parameter-view').html('Value: ' + text);
                    });
                    command.find('input[data-key="Variable"]').val($(this).val());
                    command.find('span.logic-command-parameter[data-key="Variable"]').find('.logic-command-parameter-view').html('Variable: ' + $(this).children('option:selected').text());
                });
                $('#commandValue').on('input', function () {
                    var displayText = $(this).val();
                    command.find('input[data-key="Value"]').val(displayText)
                    command.find('span.logic-command-parameter[data-key="Value"]').find('.logic-command-parameter-view').html('Value: ' + displayText);
                });
                break;
            case "2":
                //Display Text
                var text = params["Text"];
                newParamDiv = '<span class="logic-command-parameter" data-key="Text">';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[0].Key" class="logic-command-parameter-key" value="Text"/>';
                newParamDiv += '<input type="hidden" name="Commands[' + ci + '].Parameters[0].Value" class="logic-command-parameter-value" data-key="Text" value="' + text + '"/>';
                newParamDiv += '<span class="logic-command-parameter-view" data-key="Text">Text: ' + text + '</span>';
                newParamDiv += '</span>';
                input += '<div class="input-group"><div class="input-group-addon">Text</div><input type="text" class="form-control" value="' + text + '" id="commandText"></div>';
                $('#commandParameters').html(input);
                command.children('.logic-command-parameters').html(newParamDiv);
                $('#commandText').on('input', function () {
                    var input = $(this);
                    var value = input.val();
                    command.find('input[data-key="Text"]').val($(this).val());
                    command.find('span[data-key="Text"]').find('.logic-command-parameter-view').html('Text: ' + value);
                });
                break;
            default:
                //Everything with no parameters
                $('#commandParameters').html('<input type="hidden" id="commandValue"/>');
                command.children('.logic-command-parameters').html('');
                break;
        }

    });

    //Binds to variable selection change.
    $('#variableSelect').on('change', function () {
        SetValue($('#value'), $(this), 'value', function (input) {
            var statement = $('#' + currentStatement);
            var value = statement.find('input.logic-statement-value');
            value.val(input.val());
            var p_value = input.find('option:selected').text();
            if (typeof (p_value) == 'undefined' || p_value === null || p_value === undefined || p_value == "")
                p_value = input.val();
            statement.find('span.logic-statement-value').html(p_value);
        });
        var statement = $('#' + currentStatement);
        statement.children('input.logic-statement-variable').val($(this).val());
        statement.children('span.logic-statement-variable').html($(this).children('option:selected').text());

    });

    function formChanged() {
        //Set the current form and current statement.
        currentForm = $('#formSelect').val();
        var statement = $('#' + currentStatement);

        //Set the current form for the current statement
        statement.children('input.logic-statement-formkey').val(currentForm);

        //Load form variables
        SetVariables($('#variableSelect'), currentForm);
    }

    function statementClick(object) {
        var statement = $(object);
        currentStatement = statement.attr('id');
        var link = statement.children('input.logic-statement-link').val();
        $('#statementLinkSelect').val(link).trigger('change');
        var form = statement.children('input.logic-statement-formkey').val();
        currentVariable = statement.children('input.logic-statement-variable').val();
        currentValue = statement.children('input.logic-statement-value').val();
        $('#formSelect').val(form);
        if (typeof (form) !== 'undefined' && form.trim() !== '') {
            $('#formSelect').trigger('change');
        }
        $('#logicActions').removeClass('in');
        $('#logicGroup').removeClass('in');
        $('#logicStatement').addClass('in');
        $('#logicCommand').removeClass('in');
    }

    function groupClick(object) {
        var group = $(object);
        currentGroup = group.attr('id');
        var link = group.children('input.logic-group-link').val();
        $('#groupLinkSelect').val(link);
        $('#logicActions').removeClass('in');
        $('#logicGroup').addClass('in');
        $('#logicStatemen').removeClass('in');
        $('#logicCommand').removeClass('in');
    }

    function commandClick(object) {
        currentGroup = "";
        currentStatement = "";
        var command = $(object);
        currentCommand = command.attr('id');
        var work = command.children('input.logic-command-command').val();
        $('#commandSelect').val(work).trigger('change');
        $('#logicActions').removeClass('in');
        $('#logicGroup').removeClass('in');
        $('#logicStatemen').removeClass('in');
        $('#logicCommand').addClass('in');
    }

    //Generates a UId
    var guid = (function () {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
                       .toString(16)
                       .substring(1);
        }
        return function () {
            return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
                   s4() + '-' + s4() + s4() + s4();
        };
    })();

    var emptyGuid = "00000000-0000-0000-0000-000000000000";
});