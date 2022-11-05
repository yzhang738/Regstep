/// <reference path="../jquery-2.0.3.min.map" />

//This is a global variable that keeps track of the index for
//the custom border picker plugin.
var borderPickerCount = 0;

$(document).ready(function () {
    //Initialize the modal that displays all the form errors.
    //This will keep retards or from f-ing up the style sheet.
    //Jason... I'm talking to you....
    $('#formErrorModal').dialog({
        autoOpen: false,
        modal: true,
        width: 600
    });

    //Lets set up all the text align pickers
    $('.TextAlign').each(function () {
        var val = $(this).val().toLowerCase();
        var name = $(this).attr('name');
        var replacement = '<select name="' + name + '">';
        replacement += '<option value="inherit"';
        if (val == 'inherit') replacement += ' selected="true" ';
        replacement += '>Inherit</option>';
        replacement += '<option value="left"';
        if (val == 'left') replacement += ' selected="true" ';
        replacement += '>Left</option>';
        replacement += '<option value="center"';
        if (val == 'center') replacement += ' selected="true" ';
        replacement += '>Center</option>';
        replacement += '<option value="right"';
        if (val == 'right') replacement += ' selected="true" ';
        replacement += '>Right</option>';
        replacement += '</select>';
        $(this).replaceWith(replacement);
    });

    //Lets set up all the font weight pickers
    $('.FontWeight').each(function () {
        var val = $(this).val().toLowerCase();
        var name = $(this).attr('name');
        var replacement = '<select name="' + name + '">';
        replacement += '<option value="inherit"';
        if (val == 'inherit') replacement += ' selected="true" ';
        replacement += '>Inherit</option>';
        replacement += '<option value="normal"';
        if (val == 'normal') replacement += ' selected="true" ';
        replacement += '>Normal</option>';
        replacement += '<option style="font-weight: bold;" value="bold"';
        if (val == 'bold') replacement += ' selected="true" ';
        replacement += '>Bold</option>';
        replacement += '</select>';
        $(this).replaceWith(replacement);
    });

    //Lets set up the border picker
    $(".BorderPicker").each(function () {
        var modalNumber = borderPickerCount++;
        $(this).data('modal', modalNumber);
        $(this).attr('data-modal', 'BP' + modalNumber);
        $(this).parent().append('<div class="BorderPicker-Modal" id="BP' + $(this).data('modal') + '"><section><table style="border: none; border-collapse: collapse; margin: auto;"><tr><td><label>Color:</label></td><td><input type="text" class="BorderColor-Swatch" /></td></tr><tr><td><label>Size:</label></td><td><input type="text" class="Width BorderColor-Size" /></td></tr></table></section></div>');
        $(this).parent().find('.BorderPicker-Modal').dialog({
            autoOpen: false,
            modal: true,
            width: 600,
            close: function (e, ui) {
                var val = 'solid #' + $(this).find('.BorderColor-Swatch').val() + ' ' + $(this).find('.BorderColor-Size').val();
                $('input[data-modal=' + $(this).attr('id') + ']').val(val);
            },
            buttons: [{ text: 'OK', click: function () { $(this).dialog('close') } }]
        });
        $('#BP' + modalNumber).find('.BorderColor-Swatch').ColorPicker({
            onSubmit: function (hsb, hex, rgb, el) {
                var par = $(el).parent('td');
                $(par).children('span').show();
                $(el).val(hex.toUpperCase());
                $(el).ColorPickerHide();
            },
            onBeforeShow: function () {
                $(this).ColorPickerSetColor($(this).val());
            }
        }).on('keyup', function () {
            $(this).ColorPickerSetColor($(this).val());
        });
        var val = $(this).val();
        var rgx = /([\d\w#]*) ?/g;
        var results = val.match(rgx);
        if (results != null) {
            for (var i = 0; i < results.length; i++) {
                //first we remove any trailing ; symbol.
                var item = results[i].trim().toLowerCase();
                item = item.replace(/;$/, '');
                if (item == 'solid' || item == 'none' || item == 'dotted' || item == 'dashed' || item == 'double' || item == 'groove' || item == 'ridge' || item == 'inset' || item == 'outset') {
                    //This item is the style property.
                    //In the simple editor we only use solid borders.
                    //So we ignore this
                }
                if (/#[0-9a-fA-F]{6}/.test(item)) {
                    //This item is the color.
                    //We will place this in the color swatch.
                    //First we remove the # symbol.
                    //Then we place it in the color swatch input.
                    item = item.replace(/^#/, '');
                    $('#BP' + modalNumber).find('.BorderColor-Swatch').val(item);
                }
                if (/[0-9]*(px|%|em)/.test(item)) {
                    //Now we are dealing with the border size.
                    //This goes directly in with no modification.
                    $('#BP' + modalNumber).find('.BorderColor-Size').val(item);
                }
            }
        }
        $(this).on('click', function () {
            var modalNumb = $(this).data('modal');
            $('#BP' + modalNumb).dialog('open');
        });
    });

    //Lets set up all the color pickers
    $('.Color').ColorPicker({
        onSubmit: function (hsb, hex, rgb, el) {
            var par = $(el).parent('td');
            $(par).children('span').show();
            $(el).val(hex.toUpperCase());
            $(el).ColorPickerHide();
        },
        onBeforeShow: function () {
            $(this).ColorPickerSetColor($(this).val());
        }
    }).on('keyup', function () {
        $(this).ColorPickerSetColor($(this).val());
    });

    //Lets set up all the inputs that will take a width
    $(".Width").on('blur', function () {
        var val = $(this).val();
        var testEnd = /(px|%)$/;
        var testNumber = /^[0-9]+(px|%)?$/;
        //Here lets find out if we actually got a number
        //with a correct suffix
        if (!testNumber.test(val)) {
            //It's not a number. Let the user know.
            $('#formErrorModal-Message').text('The input you provided was not a number or contained an incorrect suffix.  The only valid suffixes are "px" and "%".');
            $('#formErrorModal').dialog('open');
            $(this).val('500px');
            return;
        }
        //Here we see if it is suffixed with either px or %
        if (!testEnd.test(val)) {
            //The user forgot to place a suffix, instead of
            //be-littling them, we will do it for them
            $(this).val(val + 'px');
            return;
        }
    });

    //Lets set up all the inputs that take a font size
    $(".FontSize").on('blur', function () {
        var val = $(this).val();
        var testEnd = /(px|%|em|pt)$/;
        var testNumber = /^[0-9]+(px|%|pt|em)?$/;
        //Here lets find out if we actually got a number
        //with a correct suffix
        if (!testNumber.test(val)) {
            //It's not a number. Let the user know.
            $('#formErrorModal-Message').text('The input you provided was not a number or contained an incorrect suffix.  The only valid suffixes are "px", "pt", "em", and "%".');
            $('#formErrorModal').dialog('open');
            $(this).val('12pt');
            return;
        }
        //Here we see if it is suffixed with either px or %
        if (!testEnd.test(val)) {
            //The user forgot to place a suffix, instead of
            //be-littling them, we will do it for them
            $(this).val(val + 'pt');
            return;
        }
    });

    //Create the tabs that organize all this crap.
    $("#Tabs").tabs();

});