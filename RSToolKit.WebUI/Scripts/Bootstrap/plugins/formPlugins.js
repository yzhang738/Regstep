(function ($) {
    $.fn.formCssMeasurement = function () {
        this.each(function (e) {
            var input = $(this);
            if (!input.is('input'))
                return;
            var div = '<div class="measurement-picker-container form-dialog"><div class="measurement-picker-type"><button type="button" class="btn measurement-picker measurement-picker-px">px</button><button type="button" class="btn measurement-picker measurement-picker-em">em</button><button type="button" class="btn measurement-picker measurement-picker-percent">%</button></div></div>';
            input.wrap('<div class="measurement-picker-wrapper"></div>');
            input.after(div);
            input.parent().children('.measurement-picker-container').hide();
            var pickers = input.parent().find('.measurement-picker');
            input.parent().find('.measurement-picker').on('click', function () {
                var number = $(this).parent().parent().parent().children('input').val();
                number = number.replace(/\D/g, '');
                if (number === '')
                    number = '0';
                var suffix = 'px';
                if ($(this).hasClass('measurement-picker-em'))
                    suffix = 'em';
                else if ($(this).hasClass('measurement-picker-percent'))
                    suffix = '%';
                if (number != '0')
                    number += suffix;
                $(this).parent().parent().parent().children('input').val(number);
                $(this).parent().parent().hide();
                var event = e || window.event;
                if (event.stopPropagation) {
                    event.stopPropagation();
                } else {
                    event.cancelBubble = true;
                }
            });
            input.on('click', function (e) {
                $(this).parent().children('.measurement-picker-container').show();
                var event = e || window.event;
                if (event.stopPropagation) {
                    event.stopPropagation();
                } else {
                    event.cancelBubble = true;
                }
            });
        });
        return this;
    };

    $.fn.formCssBorder = function () {
        this.each(function () {
            var input = $(this);
            var div = '<div class="border-picker-content"><div class="input-group border-picker-group-size border-picker"><input type="text" class="form-control border-picker-size input-sm border-picker-input" /><span class="input-group-addon">px</span></div><div class="input-group border-picker border-picker-group-type"><select class="form-control border-picker-type input-sm input-sm border-picker-input"><option value="solid">solid</option><option value="dashed">dashed</option></select></div><div class="input-group border-picker border-picker-group-color"><input type="text" class="form-control border-picker-color input-sm input-sm border-picker-input" /></div><div class="border-picker border-picker-group-none checkbox"><label class="control-label border-picker-none-label"><input type="checkbox" class="border-picker-none border-picker-input" /> None</label></div></div>';
            input.parent().append(div);
            input.attr('type', 'hidden');
            input.parent().find('.border-picker-color').colorpicker();
            var value = input.val();
            var valueItems = value.split(' ');
            if (valueItems.length < 3)
                valueItems = ['', '', ''];
            input.parent().find('.border-picker-size').val(valueItems[0].replace('px', ''));
            input.parent().find('.border-picker-type').val(valueItems[1]);
            input.parent().find('.border-picker-color').val(valueItems[2]);
            input.parent().find('.border-picker-input').on('change', function () {
                var input = $(this).parent().parent().parent().children('input');
                if ($(this).hasClass('border-picker-none'))
                    input = $(this).parent().parent().parent().parent().children('input');
                var inputParent = input.parent();
                var inputVal = input.val();
                var borderNone = inputParent.find('.border-picker-none');
                var noBorder = borderNone.is(':checked');
                var inputItems = inputVal.split(' ');
                if (inputItems.length < 3)
                    inputItems = ['', '', ''];
                if (!noBorder) {
                    inputItems[0] = inputParent.find('.border-picker-size').val() + "px ";
                    inputItems[1] = inputParent.find('.border-picker-type').val() + " ";
                    inputItems[2] = inputParent.find('.border-picker-color').val();
                } else {
                    inputParent.find('.border-picker-size').val('');
                    inputParent.find('.border-picker-type').val('');
                    inputParent.find('.border-picker-color').val('');
                }
                if ($(this).hasClass('border-picker-none') && $(this).is(':checked')) {
                    inputItems = ['none', '', ''];
                    inputParent.find('.border-picker-size').val('');
                    inputParent.find('.border-picker-type').val('');
                    inputParent.find('.border-picker-color').val('');
                }
                input.val(inputItems[0] + inputItems[1] + inputItems[2]);
            });
            input.parent().find('.border-picker-color').on('changeColor', function () {
                $(this).trigger('change');
            });
            //*/
        });
        return this;
    };

    $.fn.formCssBackgroundSize = function () {
        this.each(function () {
            var thisInput = $(this);
            var value = thisInput.val();
            var regex = /^\d+(\D\D?)$/;
            var numbRegex = /^(\d+)\D+(\d+)\D+$/;
            var numbOneRegex = /^(\d+)\D+$/;
            var type = 'px';
            var match = regex.exec(value);
            if (match !== null)
                type = match[1];
            var number = numbRegex.exec(value);
            var width = '';
            var height = '';
            if (number !== null) {
                width = number[1];
                height = number[2];
            }
            var numberOne = numbOneRegex.exec(value);
            if (numberOne !== null)
                width = number[1];
            var div = '';
            div += '<div class="backgroundsize-picker-content">';
            div += '<div class="input-group backgroundsize-picker-group-width backgroundsize-picker">';
            div +=          '<input type="text" class="form-control backgroundsize-picker-width input-sm backgroundsize-picker-input" data-toggle="tooltip" data-placement="top" title="Width" value="' + width + '"/>';
            div +=          '<span class="input-group-addon">px</span>';
            div +=      '</div>';
            div += '<div class="input-group backgroundsize-picker-group-height backgroundsize-picker">';
            div +=          '<input type="text" class="form-control backgroundsize-picker-height input-sm backgroundsize-picker-input" data-toggle="tooltip" data-placement="top" title="Height" value="' + height + '"/>';
            div +=          '<span class="input-group-addon">px</span>';
            div +=      '</div>';
            div +=      '<div class="input-group backgroundsize-picker backgroundsize-picker-group-type">';
            div +=          '<select class="form-control backgroundsize-picker-type input-sm input-sm backgroundsize-picker-input">';
            div +=              '<option value="px"' + selectedOption(type, 'px') + '>px</option>';
            div +=              '<option value="%"' + selectedOption(type, '%') + '>%</option>';
            div +=          '</select>';
            div +=      '</div>';
            div +=      '<div class="input-group backgroundsize-picker backgroundsize-picker-group-other">';
            div += '<select class="form-control backgroundsize-picker-other input-sm input-sm backgroundsize-picker-input">';
            div +=              '<option value=""></option>';
            div +=              '<option value="contain"' + selectedOption(value, 'contain') + '>Contain</option>';
            div +=              '<option value="cover"' + selectedOption(value, 'cover') + '>Cover</option>';
            div +=              '<option value="initial"' + selectedOption(value, 'initial') + '>Initial</option>';
            div +=              '<option value="inherit"' + selectedOption(value, 'inherit') + '>Inherit</option>';
            div +=          '</select>';
            div +=      '</div>';
            div += '</div>';
            thisInput.parent().append(div);
            thisInput.parent().find('.backgroundsize-picker-width').tooltip();
            thisInput.parent().find('.backgroundsize-picker-height').tooltip();
            thisInput.attr('type', 'hidden');
            thisInput.parent().find('.backgroundsize-picker-input').on('change', function () {
                var input = $(this).parent().parent().parent().children('input');
                var inputParent = input.parent();
                var inputVal = input.val();
                var inputType = inputParent.find('.backgroundsize-picker-type').val();
                inputVal = "";
                if (inputParent.find('.backgroundsize-picker-other').val() === "") {
                    var widthVal = inputParent.find('.backgroundsize-picker-width').val();
                    var heightVal = inputParent.find('.backgroundsize-picker-height').val();
                    if (heightVal === "")
                        inputVal = widthVal + inputType;
                    else
                        inputVal = widthVal + inputType + " " + heightVal + inputType;
                }
                else {
                    inputVal = inputParent.find('.backgroundsize-picker-other').val();
                    inputParent.find('.backgroundsize-picker-width').val('');
                    inputParent.find('.backgroundsize-picker-height').val('');
                }
                inputParent.find('.input-group-addon').html(inputParent.find('.backgroundsize-picker-type').val());
                input.val(inputVal);
            });
        });
        //*/
        return this;
    };
    
    $.fn.formCssFont = function () {
        this.each(function () {
            var input = $(this);
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control">';
            newInput += '<option value="Georgia, serif"' + selectedOption(font, "Georgia, serif") + '>Georgia</option>';
            newInput += '<option value=\'"Palatino Linotype", "Book Antiqua", Palatino, serif\'' + selectedOption(font, '"Palatino Linotype", "Book Antiqua", Palatino, serif') + '>Palatino</option>';
            newInput += '<option value=\'"Times New Roman", Times, serif\'' + selectedOption(font, '"Times New Roman", Times, serif') + '>Times New Roman</option>';
            newInput += '<option value=\'Arial, Helvetica, sans-serif\'' + selectedOption(font, 'Arial, Helvetica, sans-serif') + '>Arial</option>';
            newInput += '<option value=\'"Arial Black", Gadget, sans-serif\'' + selectedOption(font, '"Arial Black", Gadget, sans-serif') + '>Arial Black</option>';
            newInput += '<option value=\'"Comic Sans MS", cursive, sans-serif\'' + selectedOption(font, '"Comic Sans MS", cursive, sans-serif') + '>Comic Sans</option>';
            newInput += '<option value=\'Impact, Charcoal, sans-serif\'' + selectedOption(font, 'Impact, Charcoal, sans-serif') + '>Impact</option>';
            newInput += '<option value=\'"Lucida Sans Unicode", "Lucida Grande", sans-serif\'' + selectedOption(font, '"Lucida Sans Unicode", "Lucida Grande", sans-serif') + '>Lucida Sans</option>';
            newInput += '<option value=\'Tahoma, Geneva, sans-serif\'' + selectedOption(font, 'Tahoma, Geneva, sans-serif') + '>Tahoma</option>';
            newInput += '<option value=\'"Trebuchet MS", Helvetica, sans-serif\'' + selectedOption(font, '"Trebuchet MS", Helvetica, sans-serif') + '>Trebuchet</option>';
            newInput += '<option value=\'Verdana, Geneva, sans-serif\'' + selectedOption(font, 'Verdana, Geneva, sans-serif') + '>Verdana</option>';
            newInput += '<option value=\'"Courier New", Courier, monospace\'' + selectedOption(font, '"Courier New", Courier, monospace') + '>Courier New</option>';
            newInput += '<option value=\'"Lucida Console", Monaco, monospace\'' + selectedOption(font, '"Lucida Console", Monaco, monospace') + '>Lucida Console</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        return this;
    };

    $.fn.formCssFontSize = function () {
        this.each(function () {
            var input = $(this);
            var div = '<div class="fontsize-picker-container form-dialog"><div class="fontsize-picker-type"><button type="button" class="btn fontsize-picker fontsize-picker-pt">pt</button><button type="button" class="btn fontsize-picker fontsize-picker-px">px</button><button type="button" class="btn fontsize-picker fontsize-picker-em">em</button><button type="button" class="btn fontsize-picker fontsize-picker-percent">%</button></div></div>';
            input.wrap('<div class="fontsize-picker-wrapper"></div>');
            input.after(div);
            input.parent().children('.fontsize-picker-container').hide();
            var pickers = input.parent().find('.fontsize-picker');
            input.parent().find('.fontsize-picker').on('click', function () {
                var number = $(this).parent().parent().parent().children('input').val();
                number = number.replace(/\D/g, '');
                if (number === '')
                    number = '0';
                var suffix = 'pt';
                if ($(this).hasClass('fontsize-picker-px'))
                    suffix = 'px';
                if ($(this).hasClass('fontsize-picker-em'))
                    suffix = 'em';
                else if ($(this).hasClass('fontsize-picker-percent'))
                    suffix = '%';
                if (number != '0')
                    number += suffix;
                $(this).parent().parent().parent().children('input').val(number);
                $(this).parent().parent().hide();
                var event = e || window.event;
                if (event.stopPropagation) {
                    event.stopPropagation();
                } else {
                    event.cancelBubble = true;
                }
            });
            input.on('click', function (e) {
                $(this).parent().children('.fontsize-picker-container').show();
                var event = e || window.event;
                if (event.stopPropagation) {
                    event.stopPropagation();
                } else {
                    event.cancelBubble = true;
                }
            });

        });
        return this;
    };

    $.fn.formCssFontStyle = function () {
        this.each(function () {
            var input = $(this);
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control">';
            newInput += '<option value="normal"' + selectedOption(font, "normal") + '>Normal</option>';
            newInput += '<option value="italic"' + selectedOption(font, "italic") + '>Italic</option>';
            newInput += '<option value="oblique"' + selectedOption(font, "oblique") + '>Oblique</option>';
            newInput += '<option value="initial"' + selectedOption(font, "initial") + '>Initial</option>';
            newInput += '<option value="inherit"' + selectedOption(font, "inherit") + '>Inherit</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        return this;
    };

    $.fn.formCssFontWeight = function () {
        this.each(function () {
            var input = $(this);
            var name = input.attr('name');
            var font = input.val();
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control">';
            newInput += '<option value="normal"' + selectedOption(font, "normal") + '>Normal</option>';
            newInput += '<option value="bold"' + selectedOption(font, "bold") + '>Bold</option>';
            newInput += '<option value="bolder"' + selectedOption(font, "bolder") + '>Bolder</option>';
            newInput += '<option value="100"' + selectedOption(font, "100") + '>100</option>';
            newInput += '<option value="200"' + selectedOption(font, "200") + '>200</option>';
            newInput += '<option value="300"' + selectedOption(font, "300") + '>300</option>';
            newInput += '<option value="400"' + selectedOption(font, "400") + '>400</option>';
            newInput += '<option value="500"' + selectedOption(font, "500") + '>500</option>';
            newInput += '<option value="600"' + selectedOption(font, "600") + '>600</option>';
            newInput += '<option value="700"' + selectedOption(font, "700") + '>700</option>';
            newInput += '<option value="800"' + selectedOption(font, "800") + '>800</option>';
            newInput += '<option value="900"' + selectedOption(font, "900") + '>900</option>';
            newInput += '<option value="initial"' + selectedOption(font, "initial") + '>Initial</option>';
            newInput += '<option value="inherit"' + selectedOption(font, "inherit") + '>Inherit</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        var id = this.attr('id');
        return this;
    };

    $.fn.formCssBackgroundPosition = function () {
        this.each(function () {
            var input = $(this);
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control">';
            newInput += '<option value="left top"' + selectedOption(font, "left top") + '>Left Top</option>';
            newInput += '<option value="left center"' + selectedOption(font, "left center") + '>Left Center</option>';
            newInput += '<option value="left bottom"' + selectedOption(font, "left bottom") + '>Left Bottom</option>';
            newInput += '<option value="right top"' + selectedOption(font, "right top") + '>Right Top</option>';
            newInput += '<option value="right center"' + selectedOption(font, "right center") + '>Right Center</option>';
            newInput += '<option value="right bottom"' + selectedOption(font, "right bottom") + '>Right Bottom</option>';
            newInput += '<option value="center top"' + selectedOption(font, "center top") + '>Center Top</option>';
            newInput += '<option value="center center"' + selectedOption(font, "center center") + '>Center Center</option>';
            newInput += '<option value="center bottom"' + selectedOption(font, "center bottom") + '>Center Bottom</option>';
            newInput += '<option value="initial"' + selectedOption(font, "initial") + '>Initial</option>';
            newInput += '<option value="inherit"' + selectedOption(font, "inherit") + '>Inherit</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        return this;
    };

    $.fn.formCssBackgroundRepeat = function () {
        this.each(function () {
            var input = $(this);
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control">';
            newInput += '<option value="repeat"' + selectedOption(font, "repeat") + '>Repeat</option>';
            newInput += '<option value="repeat-x"' + selectedOption(font, "repeat-x") + '>Repeat X</option>';
            newInput += '<option value="repeat-y"' + selectedOption(font, "repeat-y") + '>Repeat Y</option>';
            newInput += '<option value="no-repeat"' + selectedOption(font, "no-repeat") + '>No Repeat</option>';
            newInput += '<option value="initial"' + selectedOption(font, "initial") + '>Initial</option>';
            newInput += '<option value="inherit"' + selectedOption(font, "inherit") + '>Inherit</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        return this;
    };

    $.fn.formCssTextAlign = function () {
        this.each(function () {
            var input = $(this);
            var classes = input.attr('class');
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control ' + classes + '">';
            newInput += '<option value="left"' + selectedOption(font, "left") + '>Left</option>';
            newInput += '<option value="center"' + selectedOption(font, "center") + '>Center</option>';
            newInput += '<option value="right"' + selectedOption(font, "right") + '>Right</option>';
            newInput += '<option value="justify"' + selectedOption(font, "justify") + '>Justify</option>';
            newInput += '<option value="initial"' + selectedOption(font, "initial") + '>Initial</option>';
            newInput += '<option value="inherit"' + selectedOption(font, "inherit") + '>Inherit</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        return this;
    };

    $('html').click(function () {
        hideDialogs();
    });

    function hideDialogs() {
        $('html').on('click', function () {
            $('.form-dialog').hide();
        });
    }

    function selectedOption(inputFont, font) {
        if (inputFont == font)
            return " selected=\"true\"";
        else
            return "";
    }

    var guid = function () {
        'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    };

}(jQuery));