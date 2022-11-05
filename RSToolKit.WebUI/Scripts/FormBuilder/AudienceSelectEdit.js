$(document).ready(function () {


    $('input[name=BorderColor]').ColorPicker({
        onSubmit: function (hsb, hex, rgb, el) {
            var par = $(el).parent('td');
            $(par).children('span').show();
            $(el).val(hex);
            $(el).ColorPickerHide();
            var value = '';
            var size = $('input[name=BorderSize]').val();
            if (size == null || size == '') {
                size = '2px';
            }
            value = size + " solid " + "#" + hex;
            setStyle('border', value)
        },
        onBeforeShow: function () {
            $(this).ColorPickerSetColor($(this).val());
        }
    }).on('keyup', function () {
        $(this).ColorPickerSetColor($(this).val());
    });

    $('input[name=BackgroundColor]').ColorPicker({
        onSubmit: function (hsb, hex, rgb, el) {
            var par = $(el).parent('td');
            $(par).children('span').show();
            $(el).val(hex);
            $(el).ColorPickerHide();
            var value = '';
            value = "#" + hex;
            setStyle('background-color', value)
        },
        onBeforeShow: function () {
            $(this).ColorPickerSetColor($(this).val());
        }
    }).on('keyup', function () {
        $(this).ColorPickerSetColor($(this).val());
    });

    $('input[name=FontColor]').ColorPicker({
        onSubmit: function (hsb, hex, rgb, el) {
            var par = $(el).parent('td');
            $(par).children('span').show();
            $(el).val(hex);
            $(el).ColorPickerHide();
            var value = '';
            value = "#" + hex;
            setStyle('color', value)
        },
        onBeforeShow: function () {
            $(this).ColorPickerSetColor($(this).val());
        }
    }).on('keyup', function () {
        $(this).ColorPickerSetColor($(this).val());
    });




    /********************/
    /* Set Style Inputs */
    /********************/

    if ($('#InlineCss').val() == "") {
        $('#InlineCss').val("padding: 2px; border-radius: 4px;");
        $('.previewButton').attr('style', "padding: 2px; border-radius: 4px;");
    }

    var css = $('#InlineCss').val();
    var cssParse = /([^:]*): ?([^;]*); ?/g
    var result;
    while (result != cssParse.exec(css)) {
        var value = RegExp.$2.replace('#', '');
        if (RegExp.$1 == 'border') {
            var borderStuff = (/([^ ]*) ([^ ]*) (.*)/).exec(value);
            $('input[name=BorderColor]').val(borderStuff[3]);
            $('input[name=BorderSize]').val(borderStuff[1]);
        } else if (RegExp.$1 == 'font-style' || RegExp.$1 == 'font-weight') {
            $('input[data-tag="' + RegExp.$1 + '"]').attr('checked', 'true');
        } else {
            $('input[data-tag="' + RegExp.$1 + '"]').val(value);
        }
    }

    $('input.Style', '#Styling').on('blur', function () {
        var tag = $(this).attr('data-tag');
        var value = $(this).val();
        if (tag == null || tag == '') {
            return;
        }
        if (tag != 'border') {
            setStyle(tag, value);
        } else {
            var hex = $('input[name=BorderColor]').val();
            var value = '';
            var size = $('input[name=BorderSize]').val();
            if (size == null || size == '') {
                size = '2px';
            }
            value = size + " solid " + "#" + hex;
            setStyle('border', value)
        }
    });
    $('input.StyleCB', '#Styling').on('change', function () {
        var tag = $(this).attr('data-tag');
        var value = $(this).val();
        if (tag == null || tag == '') {
            return;
        }
        var inline = $('#InlineCss').val();
        var reg = "[^-]" + tag + ": ?[^;]*; ?";
        var regExp = new RegExp(reg);
        if (regExp.test(inline)) {
            var replace = tag + ": " + value + ";";
            if (!$(this).is(':checked')) {
                inline = inline.replace(regExp, "");
            }
            else {
                inline = inline.replace(regExp, replace);
            }
        }
        else {
            inline += tag + ": " + value + ";";
        }
        $('#InlineCss').val(inline);
        $('.previewButton').attr('style', inline);
    });


    $('#RSVPYes').on('blur', function () {
        var value = $(this).val();
        if (value == "") {
            value = "Accept";
            $(this).val(value);
        }
        $('#YesButton').text(value);
    });

    $('#RSVPNo').on('blur', function () {
        var value = $(this).val();
        if (value == "") {
            value = "Decline";
            $(this).val(value);
        }
        $('#NoButton').text(value);
    });

});


function setStyle(tag, val) {
    var inline = $('#InlineCss').val();
    var reg = tag + ": ?[^;]*; ?";
    var regExp = new RegExp(reg);
    if (tag == 'color') {
        reg = ";[^:]*:roloc(?!-)";
        regExp = new RegExp(reg);
        var inlineRev = inline.split('').reverse().join('');
        if (regExp.test(inlineRev)) {
            var replace = tag + ": " + val + ";";
            replace = replace.split('').reverse().join('');
            inlineRev = inlineRev.replace(regExp, replace);
            inline = inlineRev.split('').reverse().join('');
        } else {
            inline += tag + ": " + val + ";";
        }
    } else {
        var regSuffix = /((em)|(px)|%)$/;
        if (tag == 'border-size') {
            if (val != '' && !regSuffix.test(val)) {
                val = val + "px";
                $('input[data-tag=border-size]').val(val);
            }
            tag = 'border';
            val = val + ' solid #' + $('input[data-tag=border]').val();
            reg = tag + ": ?[^;]*; ?";
            regExp = new RegExp(reg);
        }
        if (tag == 'width') {
            if (val != '' && !regSuffix.test(val)) {
                val = val + "px";
            }
        }
        regSuffix = /((pt)|(em)|(px)|%)$/;
        if (tag == 'font-size') {
            if (val != '' && !regSuffix.test(val)) {
                val = val + "pt";
            }
        }
        if (regExp.test(inline)) {
            if (val == '') {
                inline = inline.replace(regExp, '');
            } else {
                var replace = tag + ": " + val + ";";
                inline = inline.replace(regExp, replace);
            }
        }
        else {
            inline += tag + ": " + val + ";";
        }
    }
    $('#InlineCss').val(inline);
    $('.previewButton').attr('style', inline);

}