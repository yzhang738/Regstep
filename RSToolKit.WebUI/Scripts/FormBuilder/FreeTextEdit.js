/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var validateErrors = false;

$(function () {

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
    $('#StyleTabs').tabs();

    $(".LightBoxHidden").dialog({
        autoOpen: false,
        modal: true,
        width: 600
    });

    //Input Css
    if ($("input[name=CssBackgroundColor]").val() == "") {
        $("#MainBGCClear").show();
    } else {
        $("#MainBGCClear").hide();
    }

    $("#ClearMainBGC").on('click', function () {
        $("input[name=CssBackgroundColor]").val('');
        $("#MainBGCClear").show();
    });

    if ($("input[name=CssColor]").val() == "") {
        $("#MainCClear").show();
    } else {
        $("#MainCClear").hide();
    }

    $("#ClearMainC").on('click', function () {
        $("input[name=CssColor]").val('');
        $("#MainCClear").show();
    });

    if ($("input[name=CssBorderColor]").val() == "") {
        $("#MainBorderCClear").show();
    } else {
        $("#MainBorderCClear").hide();
    }

    $("#ClearMainBorderC").on('click', function () {
        $("input[name=CssBorderColor]").val('');
        $("#MainBorderCClear").show();
    });

    //Light Box
    $("#StyleOpener").on('click', function () {
        $("#StyleLightBox").dialog('open');
    });

    $('#AudienceLightBox').hide();

    $("#StyleLightBox").dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function (e, ui) {
            var mainCss = "";
            var labelCss = "";
            var altCss = "";
            var first = true;
            $('.MainCss').each(function (i) {
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
                if (value == "") return;
                if (tag == 'border') {
                    value = '2px solid ' + value;
                    $("#MainBorderCClear").hide();
                }
                if (tag == "color") {
                    $("#MainCClear").hide();
                }
                if (tag == "background-color") {
                    $("#MainBGCClear").hide();
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
                if (tag == 'width') {
                    var testEnd = /(px|%)$/;
                    var testNumber = /^[0-9]+(px|%)?$/;
                    if (!testNumber.test(value)) {
                        value = "";
                        $(this).val(value);
                        return;
                    }
                    if (!testEnd.test(value)) {
                        value += "px";
                        $(this).val(value);
                    }
                }
                if (!first) mainCss += " ";
                mainCss += tag + ": " + value + ";";
                first = false;
            });
            $("#InlineCss").attr('value', mainCss);
            $("#PreviewInput").attr('style', 'clear: both; ' + mainCss);
        }
    });

    $("textarea[name=Value]").on('blur', function () {
        $('#ValuePreview').html($('textarea[name=Value]').val());
    });

    $('#Value').ckeditor({
        extraPlugins: 'imagepicker',
        toolbar: [
            ['Source', 'imagepicker'],
            ['Font', 'TextColor', 'BGColor'],
            ['Bold', 'Italic', 'Underline', 'Subscript', 'Superscript', 'JustifyLeft', 'JustifyCenter', 'JustifyRight']
        ],
        uiColor: 'grey',
        width: 800,
        height: 300,
        enterMode: CKEDITOR.ENTER_BR
    });
});