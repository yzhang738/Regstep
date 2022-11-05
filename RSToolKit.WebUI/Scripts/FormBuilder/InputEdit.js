/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="../ColorPicker/colorpicker.js" />
/// <reference path="modernizr-2.6.2.js" />

var validateErrors = false;

$(document).ready(function () {

    ///////////////////////////
    //  Regular Expressions  //
    ///////////////////////////
    if ($('#RegularExpressionSelect').find(':selected').text() != 'Custom') {
        $('.CustomRegex').hide();
    }

    $('#RegularExpressionSelect').change(function () {
        if ($('#RegularExpressionSelect').find(':selected').text() == 'Custom') {
            $('.CustomRegex').show();
        } else {
            var rgx = $('#RegularExpressionSelect').find(':selected').attr('data-rgx');
            var pattern = $('#RegularExpressionSelect').find(':selected').attr('data-pattern');
            var message = $('#RegularExpressionSelect').find(':selected').attr('data-error');
            $('input[name=Regex]').val(rgx);
            $('input[name=RegexHuman]').val(pattern);
            $('input[name=RegexErrorMessage]').val(message);
            $('.CustomRegex').hide();
        }

    });


    //////////////
    //  Styles  //
    //////////////

    $(".LightBoxHidden").dialog({
        autoOpen: false,
        modal: true,
        width: 600
    });

    //Light Box
    $("#StyleOpener").on('click', function () {
        $("#StyleLightBox").dialog('open');
    });




    ///////////
    // Order //
    ///////////

    $('.DisplayItem').each(function (i) {
        var type = $(this).attr('data-item');
        $('#' + type).hide();
    });

    $("#DisplayOrderOpener").on('click', function () {
        $("#DisplayOrderLightBox").dialog('open');
    });

    //Add Fields
    $('.AddDisplayItem').on('click', function () {
        var type = $(this).attr('id');
        if (type == 'lbl') {
            $('#OrderHolder div.SortDisplayOrder:nth-last-child(2)').append('<div class="DisplayItem" data-item="lbl">Label<br /><span class="RemoveDisplay" onclick="removeComponent(this)" data-item="lbl">Remove</span></div>');
        }
        if (type == 'dsc') {
            $('#OrderHolder div.SortDisplayOrder:nth-last-child(2)').append('<div class="DisplayItem" data-item="dsc">Alternate Text<br /><span class="RemoveDisplay" onclick="removeComponent(this)" data-item="dsc">Remove</span></div>');
        }
        $('#' + type).hide();
        StopSorting();
    });

    //Remove Fields
    $('.RemoveDisplay').on('click', function () {
        removeComponent(this);
    });

    StopSorting();
    MakeSortable();

    
    //////////////
    // Variable //
    //////////////
    $("#Variable").on('blur', function () {
        validateVariable();
    });

    $("#Variable").on('click', function () {
        this.select();
    });

    ////////////
    // Length //
    ////////////
    $("#Length").on('blur', function () {
        validateLength();
    });


    ///////////
    // Label //
    ///////////
    $('input[name=Label]').on('blur', function () {
        $('#PreviewLabel').text($(this).val());
    });

    $('input[name=AltText]').on('blur', function () {
        $('#PreviewAlt').text($(this).val());
    });
});

function removeComponent(item) {
    $('#' + $(item).parent().attr('data-item')).show();
    $(item).parent().remove();
    StopSorting();
}

function validateVariable() {
    var selectedVariable = $('#Variable').val();
    selectedVariable = selectedVariable.trim();
    var regexTest = /^[a-zA-Z0-9]+$/
    if (!regexTest.test(selectedVariable)) {
        $("#VariableError").text("* You can only have letters and numbers.");
        validateErrors = true;
        return;
    }
    for (var i = 0; i < variables.length; i++) {
        if (variables[i] == selectedVariable) {
            $("#VariableError").text("* You cannot use a variable already in use.");
            validateErrors = true;
            return;
        }
    }
    $("#VariableError").text("");
    validateErrors = (validateErrors || false);
}

function validateLength() {
    var length = $('#Length').val();
    if (isNaN(length)) {
        $('#LengthError').text("* You must provide a valid number.");
        validateErrors = true;
        return;
    }
    $("#LengthError").text("");
    validateErrors = (validateErrors || false);
}

var fixHelperModified = function (e, span) {
    var $originals = span.children();
    var $helper = span.clone();
    $helper.children().each(function (index) {
        $(this).width($originals.eq(index).width())
    });
    return $helper;
};

function MakeSortable() {
    $(".SortDisplayOrder").sortable({
        connectWith: ".SortDisplayOrder",
        stop: function (e, ui) {
            StopSorting();
        }
    }).disableSelection();
}

function StopSorting() {
    $('#PreviewBox').children().each(function (i) {
        $(this).appendTo('#BlankPreview');
    });
    $('#BlankPreview br').each(function (i) {
        $(this).remove();
    });
    var firstRow = true;
    $('.SortDisplayOrder').each(function (r) {
        var emptyRow = true;
        $('.DisplayItem', this).each(function (c) {
            emptyRow = false;
            return false;
        });
        if (emptyRow) {
            $(this).remove();
            return true;
        }
    });
    var value = "";
    $('.SortDisplayOrder').each(function (r) {
        if (!firstRow) {
            value += '!nl';
        }
        firstRow = false;
        $('.DisplayItem', this).each(function (c) {
            value += '!' + $(this).attr('data-item');
        });
    });
    var rgxTest = /!([^!]*)/g;
    var result = rgxTest.exec(value);
    while (result != null) {
        switch (RegExp.$1) {
            case "lbl":
                $('#BlankPreview').find('[data-item=lbl]').appendTo('#PreviewBox');
                break;
            case 'itm':
                $('#BlankPreview').find('[data-item=itm]').appendTo('#PreviewBox');
                break;
            case 'dsc':
                $('#BlankPreview').find('[data-item=dsc]').appendTo('#PreviewBox');
                break;
            case 'nl':
                var lineBreak = "<br />";
                $('#PreviewBox').append(lineBreak);
                break;
            default:
                break;
        }
        result = rgxTest.exec(value);
    }
    $('input:hidden[name=DisplayOrder]').attr('value', value);
    $('#OrderHolder').append('<div class="SortDisplayOrder"></div>');
    MakeSortable();
};