/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="../ColorPicker/colorpicker.js" />
/// <reference path="modernizr-2.6.2.js" />

$(document).ready(function () {

    $('.color').ColorPicker({
        onSubmit: function (hsb, hex, rgb, el) {
            var par = $(el).parent('td');
            $(par).children('span').show();
            $(el).val(hex);
            $(el).ColorPickerHide();
        },
        onBeforeShow: function () {
            $(this).ColorPickerSetColor($(this).val());
        }
    }).on('keyup', function () {
        $(this).ColorPickerSetColor($(this).val());
    });


    $('#StyleTabs').tabs();

    $('#LabelColorClear').on('click', function () {
        $('.LabelCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#LabelBackgroundColorClear').on('click', function () {
        $('.LabelCss[data-tag="background-color"]').val('');
        $(this).hide();
    });

    $('#InputColorClear').on('click', function () {
        $('.InputCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#InputBackgroundColorClear').on('click', function () {
        $('.InputCss[data-tag="background-color"]').val('');
        $(this).hide();
    });

    $('#DescriptionColorClear').on('click', function () {
        $('.DescriptionCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#DescriptionBackgroundColorClear').on('click', function () {
        $('.DescriptionCss[data-tag="background-color"]').val('');
        $(this).hide();
    });

    $('#AgendaColorClear').on('click', function () {
        $('.AgendaCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#AgendaBackgroundColorClear').on('click', function () {
        $('.AgendaCss[data-tag="background-color"]').val('');
        $(this).hide();
    });




    $("#StyleLightBox").dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function (e, ui) {
            var mainCss = "";
            var prevMainCss = "";
            var first = true;
            $('.LabelCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    } else {
                        value = "normal";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    } else {
                        value = "normal";
                    }
                }
                if (tag == 'font-family') {
                    value = $(this).find(':selected').attr('value');
                }
                if (value == "") return;
                if (tag == 'border') {
                    value = '2px solid ' + value;
                }
                if (tag == "color") {
                    value = "#" + value;
                }
                if (tag == "background-color") {
                    value = "#" + value;
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
                if (tag == 'width' | tag == 'height') {
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
                prevMainCss += tag + ": " + value + " !Important;";
                first = false;
            });
            $("#LabelInlineCss").attr('value', mainCss);
            $("#PreviewLabel").attr('style', prevMainCss);

            mainCss = "";
            prevMainCss = "";
            $('.InputCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    } else {
                        value = "normal";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    } else {
                        value = "normal";
                    }
                }
                if (tag == 'font-family') {
                    value = $(this).find(':selected').attr('value');
                }
                if (value == "") return;
                if (tag == 'border') {
                    value = '2px solid ' + value;
                }
                if (tag == "color") {
                    value = "#" + value;
                }
                if (tag == "background-color") {
                    value = "#" + value;
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
                if (tag == "height") {
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
                prevMainCss += tag + ": " + value + " !Important;";
                first = false;
            });
            $("#InlineCss").attr('value', mainCss);
            $("#PreviewInput").attr('style', prevMainCss);

            mainCss = "";
            prevMainCss = "";
            $('.DescriptionCss').each(function (i) {
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
                }
                if (tag == "color") {
                    value = "#" + value;
                }
                if (tag == "background-color") {
                    value = "#" + value;
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
                if (tag == 'width' | tag == 'height') {
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
                prevMainCss += tag + ": " + value + " !Important;";
                first = false;
            });
            $("#AltInlineCss").attr('value', mainCss);
            $("#PreviewAlt").attr('style', prevMainCss);

            mainCss = "";
            prevMainCss = "";
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
                if (value == "") return;
                if (tag == 'border') {
                    value = '2px solid ' + value;
                }
                if (tag == "color") {
                    value = "#" + value;
                }
                if (tag == "background-color") {
                    value = "#" + value;
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
                if (tag == 'width' | tag == 'height') {
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
                prevMainCss += tag + ": " + value + " !Important;";
                first = false;
            });
            $("#AgendaInlineCss").attr('value', mainCss);
            $("#PreviewAgenda").attr('style', prevMainCss);
        }
    });


});