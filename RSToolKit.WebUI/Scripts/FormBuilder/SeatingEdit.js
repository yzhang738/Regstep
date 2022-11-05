/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />
/// <reference path="../jquery-2.0.3.min.map" />

var validateErrors = false;
var audTd;

$(document).ready(function () {

    $('#MaxSeats').spinner();

    $('#StyleTabs').tabs();

    $('#StyleOpener').on('click', function () { $('#StyleLightBox').dialog('open'); });

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

    $('#WaitLabelColorClear').on('click', function () {
        $('.WaitLabelCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#WaitLabelBackgroundColorClear').on('click', function () {
        $('.WaitLabelCss[data-tag="background-color"]').val('');
        $(this).hide();
    });

    $('#FullColorClear').on('click', function () {
        $('.FullLabelCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#FullBackgroundColorClear').on('click', function () {
        $('.FullLabelCss[data-tag="background-color"]').val('');
        $(this).hide();
    });

    $('#WaitItemColorClear').on('click', function () {
        $('.WaitItemCss[data-tag=color]').ColorPickerSetColor('FFFFFF').val('');
        $(this).hide();
    });
    $('#WaitItemBackgroundColorClear').on('click', function () {
        $('.WaitItemCss[data-tag="background-color"]').val('');
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
            $('.FullLabelCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    } else {
                        return;
                        value = "normal";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    } else {
                        return;
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
            $("#FullLabelInlineCss").attr('value', mainCss);

            first = true;
            mainCss = "";
            prevMainCss = "";
            $('.WaitLabelCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    } else {
                        return;
                        value = "normal";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    } else {
                        return;
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
                if (!first) mainCss += " ";
                mainCss += tag + ": " + value + ";";
                prevMainCss += tag + ": " + value + " !Important;";
                first = false;
            });
            $("#WaitListLabelInlineCss").attr('value', mainCss);

            first = true;
            mainCss = "";
            prevMainCss = "";
            $('.WaitItemCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    } else {
                        return;
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    } else {
                        return;
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
            $("#WaitListGroupLabelInlineCss").attr('value', mainCss);
        }
    });




});