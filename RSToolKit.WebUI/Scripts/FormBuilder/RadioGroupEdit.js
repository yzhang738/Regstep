/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var validateErrors = false;

$(function () {

    $('#AgendaStart').datetimepicker({
        changeMonth: true,
        changeYear: true,
        yearRange: '-50:+50'
    });
    $('#AgendaEnd').datetimepicker({
        changeMonth: true,
        changeYear: true,
        yearRange: '-50:+50'
    });

    $('.NumberBiggerThan1').spinner({
        min: 1
    });
    $('.NumberBiggerThan1').on('blur', function () {
        var num = parseInt($(this).val(), 10);
        if (isNaN(num) || num < 1) {
            $(this).val("1");
        }
    });

    //////////////
    //  Styles  //
    //////////////
    $('#StyleTabs').tabs();

    $(".LightBoxHidden").dialog({
        autoOpen: false,
        modal: true,
        width: 600
    });

    //Agenda Css
    if ($("input[name=AgendaBackgroundColor]").val() == "") {
        $("#AgendaBGCClear").show();
    } else {
        $("#AgendaBGCClear").hide();
    }

    $("#AgendaClearBGC").on('click', function () {
        $("input[name=AgendaBackgroundColor]").val('');
        $("#AgendaBGCClear").show();
    });

    if ($("input[name=AgendaColor]").val() == "") {
        $("#AgendaCClear").show();
    } else {
        $("#AgendaCClear").hide();
    }

    $("#AgendaClearC").on('click', function () {
        $("input[name=AgendaColor]").val('');
        $("#AgendaCClear").show();
    });

    //Label Css
    if ($("input[name=LabelBackgroundColor]").val() == "") {
        $("#LabelBGCClear").show();
    } else {
        $("#LabelBGCClear").hide();
    }

    $("#LabelClearBGC").on('click', function () {
        $("input[name=LabelBackgroundColor]").val('');
        $("#LabelBGCClear").show();
    });

    if ($("input[name=LabelColor]").val() == "") {
        $("#LabelCClear").show();
    } else {
        $("#LabelCClear").hide();
    }

    $("#LabelClearC").on('click', function () {
        $("input[name=LabelColor]").val('');
        $("#LabelCClear").show();
    });

    //Description Css
    if ($("input[name=AltBackgroundColor]").val() == "") {
        $("#AltBGCClear").show();
    } else {
        $("#AltBGCClear").hide();
    }

    $("#AltClearBGC").on('click', function () {
        $("input[name=AltBackgroundColor]").val('');
        $("#AltBGCClear").show();
    });

    if ($("input[name=AltColor]").val() == "") {
        $("#AltCClear").show();
    } else {
        $("#AltCClear").hide();
    }

    $("#AltClearC").on('click', function () {
        $("input[name=AltColor]").val('');
        $("#AltCClear").show();
    });

    //Light Box
    $("#StyleOpener").on('click', function () {
        $("#StyleLightBox").dialog('open');
    });

    $("#StyleLightBox").dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function (e, ui) {
            var mainCss = "";
            var labelCss = "";
            var altCss = "";
            var agendaCss = "";
            var first = true;

            first = true;
            $('.LabelCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    }
                }
                if (tag == 'font-family') {
                    value = $(this).find(':selected').attr('value');
                }
                if (tag == 'font-size') {
                    var testEnd = /(pt|px|em)$/;
                    var testNumber = /^[0-9]+(pt|px|em)?$/;
                    if (!testNumber.test(value)) {
                        value = "";
                        $(this).val(value);
                        return;
                    }
                    if (!testEnd.test(value)) {
                        value += "pt";
                        $(this).val(value);
                    }
                }
                if (value == "") return;
                if (tag == "color") {
                    $("#LabelCClear").hide();
                }
                if (tag == "background-color") {
                    $("#LabelBGCClear").hide();
                }
                if (!first) labelCss += " ";
                labelCss += tag + ": " + value + ";";
                first = false;
            });
            $("#LabelInlineCss").attr('value', labelCss);
            $("#PreviewLabel").attr('style', 'clear: both; ' + labelCss);

            first = true;
            $('.AltCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    }
                }
                if (tag == 'font-family') {
                    value = $(this).find(':selected').attr('value');
                }
                if (tag == 'font-size') {
                    var testEnd = /(pt|px|em)$/;
                    var testNumber = /^[0-9]+(pt|px|em)?$/;
                    if (!testNumber.test(value)) {
                        value = "";
                        $(this).val(value);
                        return;
                    }
                    if (!testEnd.test(value)) {
                        value += "pt";
                        $(this).val(value);
                    }
                }
                if (value == "") return;
                if (tag == "color") {
                    $("#AltCClear").hide();
                }
                if (tag == "background-color") {
                    $("#AltBGCClear").hide();
                }
                if (!first) altCss += " ";
                altCss += tag + ": " + value + ";";
                first = false;
            });
            $("#AltInlineCss").attr('value', altCss);
            $("#PreviewAlt").attr('style', 'clear: both; ' + altCss);

            first = true;
            $('.AgendaCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    }
                }
                if (tag == 'font-family') {
                    value = $(this).find(':selected').attr('value');
                }
                if (tag == 'font-size') {
                    var testEnd = /(pt|px|em)$/;
                    var testNumber = /^[0-9]+(pt|px|em)?$/;
                    if (!testNumber.test(value)) {
                        value = "";
                        $(this).val(value);
                        return;
                    }
                    if (!testEnd.test(value)) {
                        value += "pt";
                        $(this).val(value);
                    }
                }
                if (value == "") return;
                if (tag == "color") {
                    $("#AgendaCClear").hide();
                }
                if (tag == "background-color") {
                    $("#AgendaBGCClear").hide();
                }
                if (!first) altCss += " ";
                agendaCss += tag + ": " + value + ";";
                first = false;
            });
            $("#AgendaInlineCss").attr('value', agendaCss);
            $("#PreviewAgenda").attr('style', 'clear: both; ' + agendaCss);
        }
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
        if (type == 'prc') {
            $('#OrderHolder div.SortDisplayOrder:nth-last-child(2)').append('<div class="DisplayItem" data-item="prc">Pricing<br /><span class="RemoveDisplay" onclick="removeComponent(this)" data-item="prc">Remove</span></div>');
        }
        if (type == 'tmst') {
            $('#OrderHolder div.SortDisplayOrder:nth-last-child(2)').append('<div class="DisplayItem" data-item="tmst">Time<br /><span class="RemoveDisplay" onclick="removeComponent(this)" data-item="tmst">Remove</span></div>');
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
            case 'prc':
                $('#BlankPreview').find('[data-item=prc]').appendTo('#PreviewBox');
                break;
            case 'tmst':
                $('#BlankPreview').find('[data-item=tmst]').appendTo('#PreviewBox');
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

    $("#ItemList tbody").sortable({
        helper: fixHelperModified,
        stop: function (e, ui) {
            $(".PageNumber").each(function (i) {
                $(this).text('RadioGroup Item: ' + (i + 1));
            });
            $('input[name^=ItemOrder]').each(function (i) {
                $(this).val((i + 1));
            });
        }
    })
};