/*jshint unused:false*/
(function ($) {
 
    var inputs = {};

    var fileref = document.createElement("link");
    fileref.setAttribute("rel", "stylesheet");
    fileref.setAttribute("type", "text/css");
    fileref.setAttribute("href", "../../Content/Bootstrap/formPlugin.min.css");
    document.getElementsByTagName("head")[0].appendChild(fileref);

    $.fn.formCssMeasurementAny = function () {
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            var div = '<div class="measurement-picker-container form-dialog"><div class="measurement-picker-type"><button type="button" class="btn measurement-picker measurement-picker-px">px</button><button type="button" class="btn measurement-picker measurement-picker-em">em</button><button type="button" class="btn measurement-picker measurement-picker-percent">%</button></div></div>';
            input.wrap('<div class="measurement-picker-wrapper"></div>');
            input.after(div);
            var value = input.val();
            var suffix = value.replace(/\d/g, '');
            input.data('suffix', suffix);
            input.parent().children('.measurement-picker-container').hide();
            var pickers = input.parent().find('.measurement-picker');
            input.parent().find('.measurement-picker').on('click', function () {
                var input = $(this).parent().parent().parent().children('input');
                var suffix = 'px';
                var number = input.val();
                number = number.replace(/\D/g, '');
                if (number === '')
                    number = '0';
                if ($(this).hasClass('measurement-picker-em'))
                    suffix = 'em';
                else if ($(this).hasClass('measurement-picker-percent'))
                    suffix = '%';
                if (number !== '0')
                    number += suffix;
                input.data('suffix', suffix);
                $(this).parent().parent().parent().children('input').val(number);
                $(this).parent().parent().hide();
            });
            input.on('blur', function () {
                var number = $(this).val();
                if (number.match(/^\d+$/)) {
                    var suffix = $(this).data('suffix');
                    $(this).val(number + suffix);
                }
            });
            input.on('click', function (e) {
                var input = $(this);
                var parent = input.parent();
                $('.form-dialog').each(function () {
                    if ($(this).parent()[0] === parent[0])
                        return;
                    $(this).hide();
                });
                $(this).parent().children('.measurement-picker-container').show();
                var event = e || window.event;
                if (event.stopPropagation) {
                    event.stopPropagation();
                } else {
                    event.cancelBubble = true;
                }
            });
            input.numberOnly();

        });
        return this;
    };

    $.fn.formCssMeasurementPx = function () {
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            input.attr('type', 'hidden');
            var id = guid();
            input.after('<div class="input-group"><input type="text" id="' + id + '" class="form-control" placeholder="13"><span class="input-group-addon">px</span></div>');
            $('#' + id).numberOnly();
            $('#' + id).on('input', function () {
                input.val($(this).val() + 'px');
            });
        });
        return this;
    };

    $.fn.formCssMeasurementPc = function () {
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            input.attr('type', 'hidden');
            var id = guid();
            input.after('<div class="input-group"><input type="text" id="' + id + '" class="form-control" placeholder="13"><span class="input-group-addon">%</span></div>');
            $('#' + id).numberOnly();
            $('#' + id).on('input', function () {
                input.val($(this).val() + '%');
            });
        });
        return this;
    };

    $.fn.formCssMeasurementEm = function () {
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            input.attr('type', 'hidden');
            var id = guid();
            input.after('<div class="input-group"><input type="text" id="' + id + '" class="form-control" placeholder="13"><span class="input-group-addon">em</span></div>');
            $('#' + id).numberOnly();
            $('#' + id).on('input', function () {
                input.val($(this).val() + 'em');
            });
        });
        return this;
    };

    $.fn.formCssMeasurement = function () {
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            input.numberOnly().on('blur', function () {
                var t_val = $.trim($(this).val());
                if (t_val === '' || t_val === '0') {
                    return;
                }
                t_val += 'px';
                input.val(t_val);
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
            if (valueItems.length < 3) {
                valueItems = ['', '', ''];
                input.parent().find('.border-picker-none').prop('checked', true);
            }
            input.parent().find('.border-picker-size').val(valueItems[0].replace('px', ''));
            input.parent().find('.border-picker-type').val(valueItems[1]);
            input.parent().find('.border-picker-color').val(valueItems[2]);
            input.parent().find('input.border-picker-input, select.border-picker-input').on('change', function () {
                var input = $(this).parent().parent().parent().children('input');
                if ($(this).hasClass('border-picker-none'))
                    input = $(this).parent().parent().parent().parent().children('input');
                else
                    $(this).closest('.border-picker-content').find('.border-picker-none').prop('checked', false);
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
                if (!/^#/.test(inputItems[2])) {
                    inputItems[2] = '#' + inputItems[2];
                }
                input.val(inputItems[0] + inputItems[1] + inputItems[2]);
            });
            input.parent().find('.border-picker-color').on('changeColor', function () {
                $(this).trigger('change');
            });
            input.parent().find('.border-picker-size').on('keydown', function (e) {
                // Allow: backspace, delete, tab, escape, enter and .
                if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                    // Allow: Ctrl+A
                    (e.keyCode === 65 && e.ctrlKey === true) ||
                    // Allow: home, end, left, right
                    (e.keyCode >= 35 && e.keyCode <= 39)) {
                    // let it happen, don't do anything
                    return;
                }
                // Ensure that it is a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
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
            div += '<input type="text" class="form-control backgroundsize-picker-width input-sm backgroundsize-picker-input" data-toggle="tooltip" data-placement="top" title="Width" value="' + width + '"/>';
            div += '<span class="input-group-addon">px</span>';
            div += '</div>';
            div += '<div class="input-group backgroundsize-picker-group-height backgroundsize-picker">';
            div += '<input type="text" class="form-control backgroundsize-picker-height input-sm backgroundsize-picker-input" data-toggle="tooltip" data-placement="top" title="Height" value="' + height + '"/>';
            div += '<span class="input-group-addon">px</span>';
            div += '</div>';
            div += '<div class="input-group backgroundsize-picker backgroundsize-picker-group-type">';
            div += '<select class="form-control backgroundsize-picker-type input-sm input-sm backgroundsize-picker-input">';
            div += '<option value="px"' + selectedOption(type, 'px') + '>px</option>';
            div += '<option value="%"' + selectedOption(type, '%') + '>%</option>';
            div += '</select>';
            div += '</div>';
            div += '<div class="input-group backgroundsize-picker backgroundsize-picker-group-other">';
            div += '<select class="form-control backgroundsize-picker-other input-sm input-sm backgroundsize-picker-input">';
            div += '<option value=""></option>';
            div += '<option value="contain"' + selectedOption(value, 'contain') + '>Contain</option>';
            div += '<option value="cover"' + selectedOption(value, 'cover') + '>Cover</option>';
            div += '<option value="initial"' + selectedOption(value, 'initial') + '>Initial</option>';
            div += '<option value="inherit"' + selectedOption(value, 'inherit') + '>Inherit</option>';
            div += '</select>';
            div += '</div>';
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
            newInput += '<option value="null">No Value</option>';
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
            input.parent().find('.fontsize-picker').on('click', function (e) {
                var number = $(this).parent().parent().parent().children('input').val();
                number = number.replace(/\D/g, '');
                if (number !== '') {
                    var suffix = 'pt';
                    if ($(this).hasClass('fontsize-picker-px'))
                        suffix = 'px';
                    if ($(this).hasClass('fontsize-picker-em'))
                        suffix = 'em';
                    else if ($(this).hasClass('fontsize-picker-percent'))
                        suffix = '%';
                    if (number !== '0')
                        number += suffix;
                }
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
            newInput += '<option value="null">No Value</option>'
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
            newInput += '<option value="null">No Value</option>';
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
            newInput += '<option value="null">No Value</option>';
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
            newInput += '<option value="null">No Value</option>';
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
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var classes = input.attr('class');
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control ' + classes + '">';
            newInput += '<option value="null">No Value</option>';
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

    $.fn.formCssTextValign = function () {
        this.each(function () {
            var input = $(this);
            var id = input.attr('id');
            var name = input.attr('name');
            var font = input.val();
            var classes = input.attr('class');
            classes = classes.replace(/ ?form-control ?/, '');
            $.trim(classes);
            var newInput = '<select name="' + name + '" id="' + id + '" class="form-control ' + classes + '">';
            newInput += '<option value="null">No Value</option>';
            newInput += '<option value="top"' + selectedOption(font, "top") + '>Top</option>';
            newInput += '<option value="middle"' + selectedOption(font, "middle") + '>Middle</option>';
            newInput += '<option value="bottom"' + selectedOption(font, "bottom") + '>Bottom</option>';
            newInput += '<option value="initial"' + selectedOption(font, "initial") + '>Initial</option>';
            newInput += '<option value="inherit"' + selectedOption(font, "inherit") + '>Inherit</option>';
            newInput += '</select>';
            input.replaceWith(newInput);
        });
        return this;
    };

    $.fn.formCssPadding = function () {
        this.each(function (i) {
            var input = $(this);
            if (!input.is('input'))
                return;
            input.formHide();
            var id = guid();
            inputs[id] = input;
            var value = input.val();
            value = value.replace(/[a-zA-Z]/g, "");
            var v_padding = value.split(' ');
            if (v_padding.length !== 4)
                v_padding = ['0', '0', '0', '0'];
            for (var i_2 = 0; i_2 < 4; i_2++) {
                if (!/^\d+$/.test(v_padding[i_2]))
                    v_padding[i_2] = '0';
            }
            input.val(paddingString(v_padding));
            input.after('<div id="' + id + '" class="padding-div form-inline"><div class="row">' +
                '<div class="form-group col-sm-3"><div class="input-group"><span class="input-group-addon"><span class="glyphicon glyphicon-arrow-up"></span></span><input class="form-control" data-form-padding="top" value="' + v_padding[0] + '"><span class="input-group-addon">px</span></div></div>' +
                '<div class="form-group col-sm-3"><div class="input-group"><span class="input-group-addon"><span class="glyphicon glyphicon-arrow-right"></span></span><input class="form-control" data-form-padding="right" value="' + v_padding[1] + '"><span class="input-group-addon">px</span></div></div>' +
                '<div class="form-group col-sm-3"><div class="input-group"><span class="input-group-addon"><span class="glyphicon glyphicon-arrow-down"></span></span><input class="form-control" data-form-padding="bottom" value="' + v_padding[2] + '"><span class="input-group-addon">px</span></div></div>' +
                '<div class="form-group col-sm-3"><div class="input-group"><span class="input-group-addon"><span class="glyphicon glyphicon-arrow-left"></span></span><input class="form-control" data-form-padding="left" value="' + v_padding[3] + '"><span class="input-group-addon">px</span></div></div>' +
                '</div></div>');
            $('#' + id).find('input[data-form-padding]').numberOnly().on('input', function () {
                var t_padding = input.val();
                t_padding = t_padding.replace(/[a-zA-Z]/g, "");
                t_padding = t_padding.split(' ');
                if (t_padding.length !== 4)
                    t_padding = ['0', '0', '0', '0'];
                for (var i = 0; i < 4; i++) {
                    if (!/^\d+$/.test(t_padding[i]))
                        t_padding[i] = '0';
                }
                var val = $(this).val();
                switch ($(this).attr('data-form-padding')) {
                    case 'top':
                        t_padding[0] = val;
                        break;
                    case 'right':
                        t_padding[1] = val;
                        break;
                    case 'bottom':
                        t_padding[2] = val;
                        break;
                    case 'left':
                        t_padding[3] = val;
                        break;
                }
                input.val(paddingString(t_padding));
            });
            $('#' + id).find('input[data-form-padding]').on('blur', function () {
                var t_padding = $.trim($(this).val());
                if (t_padding === '' || t_padding === '0') {
                    return;
                }
                t_padding += 'px';
                input.val(paddingString(t_padding));
            });
        });
        return this;
    };

    $.fn.formColorPicker = function () {
        this.each(function () {
            var fp_input = $(this);
            if (!fp_input.is('input'))
                return;
            if (fp_input.val().toLowerCase() === 'transparent' || fp_input.val().toLowerCase() === 'none')
                fp_input.val('');
            fp_input.colorpicker();
        });
        return this;
    };

    $.fn.runInputs = function () {
        return this.find('input[data-input-type]').each(function () {
            var t_inputTypes_input = $(this);
            var t_inputTypes_type = t_inputTypes_input.attr('data-input-type');
            switch (t_inputTypes_type.toLowerCase()) {
                case 'color':
                    t_inputTypes_input.colorpicker();
                    break;
                case 'padding':
                    t_inputTypes_input.formCssPadding();
                    break;
                case 'border':
                    t_inputTypes_input.formCssBorder();
                    break;
                case 'align':
                    t_inputTypes_input.formCssTextAlign();
                    break;
                case 'valign':
                case 'verticalalign':
                    t_inputTypes_input.formCssTextValign();
                    break;
                case 'measurement':
                    t_inputTypes_input.formCssMeasurement();
                    break;
                case 'measurement_px':
                    t_inputTypes_input.formCssMeasurementPx();
                    break;
                case 'measurement_em':
                    t_inputTypes_input.formCssMeasurementEm();
                    break;
                case 'measurement_%':
                    t_inputTypes_input.formCssMeasurementPc();
                    break;
            }
        });
    };

    $('html').click(function () {
        hideDialogs();
    });

    $.fn.numberOnly = function () {
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            input.on('keydown', function (e) {
                // Allow: backspace, delete, tab, escape, enter and .
                if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 190]) !== -1 ||
                    // Allow: Ctrl+A
                    (e.keyCode === 65 && e.ctrlKey === true) ||
                    // Allow: home, end, left, right
                    (e.keyCode >= 35 && e.keyCode <= 39)) {
                    // let it happen, don't do anything
                    return;
                }
                // Ensure that it is not a number and stop the keypress
                if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
                    e.preventDefault();
                }
            });
        });
        return this;
    };

    $.fn.formHide = function (hide) {
        hide = hide || true;
        this.each(function () {
            var input = $(this);
            if (!input.is('input'))
                return;
            if (hide) {
                input.data('form-origtype', input.attr('type'));
                input.attr('type', 'hidden');
            }
            else {
                input.attr('type', input.data('form-origtype'));
            }
        });

    };

    function hideDialogs() {
        $('html').on('click', function () {
            $('.form-dialog').hide();
        });
    }

    function selectedOption(inputFont, font) {
        if (inputFont === font)
            return " selected=\"true\"";
        else
            return "";
    }

    function paddingString(padding) {
        padding = padding || ['0', '0', '0', '0'];
        if (padding.length !== 4)
            padding = ['0', '0', '0', '0'];
        var string = '';
        for (var i = 0; i < padding.length; i++) {
            string += padding[i];
            if (padding[i] !== '0')
                string += 'px';
            string += ' ';
        }
        string = $.trim(string);
        return string;
    }

    function guid() {
        /// <signature>
        /// <summary>Creates a psuedo Global Unique Identifier.</summary>
        /// <returns type="String" />
        /// </signature>
        /*jshint ignore:start*/
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
        /*jshint ignore:end*/
    }

    // Form Validation Plugin

    var formvalidate_emailRegex = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    var formvalidate_urlRegex = /((http|ftp|https):\/\/)?[\w-]+(\.[\w-]+)+([\w.,@?^=%&amp;:\/~+#-]*[\w@?^=%&amp;\/~+#-])?/;
    var formvalidate_lettersAndNumberRegex = /^[a-zA-Z0-9]*$/;
    var formvalidate_phoneRegex = /^(?:(?:\+?1\s*(?:[.-]\s*)?)?(?:\(\s*([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9])\s*\)|([2-9]1[02-9]|[2-9][02-8]1|[2-9][02-8][02-9]))\s*(?:[.-]\s*)?)?([2-9]1[02-9]|[2-9][02-9]1|[2-9][02-9]{2})\s*(?:[.-]\s*)?([0-9]{4})(?:\s*(?:#|x\.?|ext\.?|extension)\s*(\d+))?$/;
    var formvalidate_zipRegex = /^\d{5}(?:[-\s]\d{4})?$/;

    $.fn.runInputs = function () {
        this.find('form').each(function () {
            $(this).formValidate();
        });
        return this;
    };

    $.fn.formValidate = function (options) {
        /// <signature>
        /// <summary>Sets a form up for validation.</summary>
        /// <param name="options" type="PlainObject">A map of additional options to pass to the method.</param>
        /// <returns type="jQuery" />
        /// </signature>
        /* Get settings */
        var defaults = new ItemSettings(options);
        //$(defaults.messageClass).hide();
        this.each(function () {
            /* We run through each form. */
            var submitDelegates = [];
            var j_form = $(this);
            if (!j_form.is('form')) {
                return this;
            }
            var form_items = new FormInfo(j_form, submitDelegates);
            var t_defaults = new ItemSettings(defaults);
            t_defaults.containerClass = j_form.getTag('data-form-containerclass', '.form-group');
            if (!/^\..+/.test(t_defaults.containerClass)) {
                t_defaults.containerClass = '.' + t_defaults.containerClass;
            }
            t_defaults.warning = j_form.getTag('data-form-warning', '.form-warning');
            t_defaults.inputClass = j_form.getTag('data-form-inputclass', t_defaults.inputClass);
            var formSettings = $.extend({}, t_defaults);
            formSettings.onSubmit = j_form.getTag('data-form-onsubmit', formSettings.onSubmit, 'bool');
            formSettings.messageClass = j_form.getTag('data-form-messageclass', formSettings.messageClass);
            formSettings.showProcessing = j_form.getTag('data-form-showprocessing', formSettings.showProcessing);
            if (formSettings.onSubmit) {
                j_form.on('submit', function () {
                    var error = form_items.submit();
                    if (error) {
                        $(t_defaults.warning).html('There are one or more errors with your data entries.  Please see below for details.').show('fast');
                    } else {
                        var t_procModal = $('#formPlugin_procModal');
                        if (t_procModal.length == 0) {
                            $('body').append('<div class="modal fade" style="z-index: 9999 !important" id="formPlugin_procModal" data-backdrop="static" data-keyboard="false"><div class="modal-dialog modal-content"><div class="modal-header"><h3 id="title_prettyProccessing">Processing Data...</h3></div><div class="modal-body"><div class="progress"><div id="bar_prettyProccessing" class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%"><span id="status_prettyProcessing" class="text-bold">Processing Data</span></div></div></div></div></div>');
                            t_procModal = $('#formPlugin_procModal');
                        }
                        t_procModal.modal('show');
                    }
                    return !error;
                });
            } else {
                j_form.on('bs.form.submit', function () {
                    var error = form_items.setErrors();
                    if (error)
                        $(t_defaults.warning).html('There are one or more errors with your data entries.  Please see below for details.');
                    return !error;
                });
            }
            t_defaults.requiredMessage = j_form.getTag('data-form-requiredmessage', t_defaults.requiredMessage);
            //$(t_defaults.messageClass).hide();
            j_form.find('input, selection').not('[type="hidden"]').not('[type="file"]').each(function () {
                /* We run through each input and selection. */
                var settings = new ItemSettings(t_defaults);
                var j_item = $(this);
                if (formSettings.inputClass !== null) {
                    if (!j_item.is(formSettings.inputClass)) {
                        return;
                    }
                }
                var t_id = j_item.getTag('id', guid());
                j_item.attr('id', t_id);
                /* Grab the container. */
                var container = j_item.closest(settings.containerClass);
                if (typeof (container) === 'undefined' || container === null) {
                    /* contianer was not present.  We will create it. */
                    container = $('<div class="form-component"></div>');
                    j_item.wrap(container);
                }
                var item_status = new ItemInfo(j_item, container,settings);
                /* Add the item status to the form list. */
                form_items.set(t_id, item_status);
                j_item.data('item-errors', item_status);
                var textInput = 1;
                /*jshint ignore:start*/
                textInput &= !j_item.is('select');
                textInput &= !j_item.is('[type="checkbox"]');
                textInput &= !j_item.is('[type="radio"]');
                textInput = textInput !== 0;
                /*jshint ignore:end*/
                j_item.attr('data-form-id', t_id);
                /* Check for required data tag. */
                settings.required = j_item.getTag('data-form-required', settings.required, 'bool');
                /* check for validation data tag. */
                var t_validtation = j_item.getTag('data-form-validation', settings.validation);
                if (typeof (t_validtation) === 'undefined' || t_validtation === null) {
                    t_validtation = 'text';
                }
                settings.setValidation(t_validtation.toLowerCase());
                /* Check for max data tag. */
                settings.max = j_item.getTag('data-form-max', settings.max);
                if (isNaN(settings.max)) {
                    settings.max = -1;
                } else {
                    settings.max = parseInt(settings.max);
                }
                /* Check for showCharacters tag. */
                settings.showCharacters = j_item.getTag('data-form-showcharacters', settings.showCharacters, 'bool');
                /* Check for onchange tag. */
                settings.onInput = j_item.getTag('data-form-oninput', settings.onInput, 'bool');
                /* Check for required message tag. */
                settings.requiredMessage = j_item.getTag('data-form-requiredmessage', settings.requiredMessage);
                /* Check for max message tag. */
                settings.maxMessage = j_item.getTag('data-form-maxmessage', settings.maxMessage);
                settings.maxMessage = settings.maxMessage.replace('[max]', settings.max);
                /* Check for default container tag. */
                settings.containerClass = j_form.getTag('data-form-containerclass', settings.containerClass);
                if (!/^\..+/.test(settings.containerClass)) {
                    settings.containerClass = '.' + settings.containerClass;
                }
                /* Check for max range tag. */
                settings.rangeMax = j_item.getTag('data-form-rangemax', settings.rangeMax, 'number?');
                /* Check for min range tag. */
                settings.rangeMin = j_item.getTag('data-form-rangemin', settings.rangeMin, 'number?');
                /* Check for min and max inclusive. */
                settings.rangeMinInclusive = j_item.getTag('data-form-rangemininclusive', settings.rangeMinInclusive, 'bool');
                settings.rangeMaxInclusive = j_item.getTag('data-form-rangemaxinclusive', settings.rangeMaxInclusive, 'bool');
                /* Check for validation message. */
                settings.validationMessage = j_item.getTag('data-form-validationmessage', settings.validationMessage);
                /* Check for range message. */
                settings.rangeMessage = j_item.getTag('data-form-rangemessage', settings.rangeMessage);

                /* Now we check if required. */
                if (settings.required) {
                    submitDelegates.push(function () {
                        TestRequired(item_status, settings);
                    });
                }
                /* Now we check if it has max characters. */
                if (textInput && settings.max > 0) {
                    if (settings.onInput) {
                        /* We need to bind to the input event. */
                        j_item.on('input', function () {
                            if (j_item.val().length > settings.max) {
                                var t_message = settings.maxMessage.replace('[now]', j_item.val().length);
                                item_status.set('max', INPUT_STATUS.ERROR, t_message);
                            } else {
                                item_status.set('max', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                        if (settings.showCharacters) {
                            var showCharacters = $('<div class="form-charactercount"><span class="form-characters">0</span> of ' + settings.max + '</div>');
                            j_item.after(showCharacters);
                            j_item.on('input', function () {
                                showCharacters.find('.form-characters').html(j_item.val().length);
                            });
                        }
                    }

                    /* We submit the submit delegate. */
                    submitDelegates.push(function () {
                        TestCharacters(item_status, settings);
                    });
                }

                /* Now we run tests if validation type is number */
                if (textInput && settings.validation === 'number') {
                    if (settings.validationMessage === null) {
                        settings.validationMessage = 'You must supply a number.';
                    }
                    if (settings.onInput) {
                        j_item.on('input', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('number', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (isNaN(j_item.val())) {
                                item_status.set('number', INPUT_STATUS.ERROR, settings.validationMessage);
                            } else {
                                item_status.set('number', INPUT_STATUS.VALID);
                                var t_value = parseInt(j_item.val());
                                var t_rmessage = settings.rangeMessage.replace('[min]', settings.rangeMin).replace('[max]', settings.rangeMax);
                                if (settings.rangeMin !== null && ((settings.rangeMinInclusive && t_value < settings.rangeMin) || (!settings.rangeMinInclusive && t_value <= settings.rangeMin))) {
                                    item_status.set('number', INPUT_STATUS.ERROR, t_rmessage);
                                } else if (settings.rangeMax !== null && ((settings.rangeMaxInclusive && t_value > settings.rangeMax) || (!settings.rangeMaxInclusive && t_value >= settings.rangeMax))) {
                                    item_status.set('number', INPUT_STATUS.ERROR, t_rmessage);
                                } else {
                                    item_status.set('number', INPUT_STATUS.VALID);
                                }
                            }
                            item_status.setError();
                        });
                    }
                    submitDelegates.push(function () {
                        TestNumberRange(item_status, settings);
                    });
                }

                /* Now we run validation if input type is decimal. */
                if (settings.validation === 'money' || settings.validation === 'decimal') {
                    if (settings.validationMessage === null)
                        settings.validationMessage = "You must supply a number.";
                    if (settings.onInput) {
                        j_item.on('input', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('number', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (isNaN(j_item.val())) {
                                item_status.set('number', INPUT_STATUS.ERROR, settings.validationMessage);
                            } else {
                                item_status.set('number', INPUT_STATUS.VALID);
                                var t_value = parseFloat(j_item.val());
                                if (!/^\d+\.\d+$/.test(j_item.val())) {
                                    item_status.set('number', INPUT_STATUS.WARNING);
                                }
                            }
                            item_status.setError();
                        });
                    }
                    if (settings.validation === 'money') {
                        j_item.on('blur', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('number', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (isNaN(j_item.val())) {
                                return;
                            }
                            if (!/^\d+\.\d+$/.test(j_item.val())) {
                                if (/^\d+$/.test(j_item.val())) {
                                    j_item.val(j_item.val() + '.00');
                                } else if (/^\d+\.$/.test(j_item.val())) {
                                    j_item.val(j_item.val() + '00');
                                } else if (/^\.\d+/.test(j_item.val())) {
                                    j_item.val('0' + j_item.val());
                                }
                            }
                            j_item.val(j_item.val().replace(/^(\d+\.\d{2})\d*/, "$1"));
                            item_status.set('number', INPUT_STATUS.VALID);
                            item_status.setError();
                        });
                    }
                    submitDelegates.push(function () {
                        TestNumber(item_status, settings);
                    });
                }

                /* Now we run validation for email. */
                if (settings.validation === 'email') {
                    if (settings.validationMessage === null)
                        settings.validationMessage = "You must supply a valid email.";
                    if (settings.onInput) {
                        j_item.on('input', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('validation', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (!formvalidate_emailRegex.test(j_item.val())) {
                                item_status.set('validation', INPUT_STATUS.WARNING);
                            } else {
                                item_status.set('validation', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                        j_item.on('blur', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('validation', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (!formvalidate_emailRegex.test(j_item.val())) {
                                item_status.set('validation', INPUT_STATUS.ERROR, settings.validationMessage);
                            } else {
                                item_status.set('validation', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                    }
                    submitDelegates.push(function () {
                        TestEmail(item_status, settings);
                    });
                }

                /* Now we run valiation for phone. */
                if (settings.validation === 'phone' || settings.validation === 'usphone') {
                    if (settings.validationMessage === null)
                        settings.validationMessage = "You must supply a valid U.S. phone.";
                    if (settings.onInput) {
                        j_item.on('input', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('validation', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (!formvalidate_phoneRegex.test(j_item.val())) {
                                item_status.set('validation', INPUT_STATUS.WARNING);
                            } else {
                                item_status.set('validation', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                        j_item.on('blur', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('validation', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (!formvalidate_phoneRegex.test(j_item.val())) {
                                item_status.set('validation', INPUT_STATUS.ERROR, settings.validationMessage);
                            } else {
                                item_status.set('validation', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                    }
                    submitDelegates.push(function () {
                        TestPhone(item_status, settings);
                    });
                }

                /* Now we run valiation for zip. */
                if (settings.validation === 'zipcode' || settings.validation === 'zip') {
                    if (settings.validationMessage === null)
                        settings.validationMessage = "You must supply a valid U.S. zip code.";
                    if (settings.onInput) {
                        j_item.on('input', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('validation', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (!formvalidate_zipRegex.test(j_item.val())) {
                                item_status.set('validation', INPUT_STATUS.WARNING);
                            } else {
                                item_status.set('validation', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                        j_item.on('blur', function () {
                            if (emptyNotRequired(j_item, settings)) {
                                item_status.set('validation', INPUT_STATUS.VALID);
                                item_status.setError();
                                return;
                            }
                            if (!formvalidate_zipRegex.test(j_item.val())) {
                                item_status.set('validation', INPUT_STATUS.ERROR, settings.validationMessage);
                            } else {
                                item_status.set('validation', INPUT_STATUS.VALID);
                            }
                            item_status.setError();
                        });
                    }
                    submitDelegates.push(function () {
                        TestZip(item_status, settings);
                    });
                }

            });
        });
        return this;
    };

    //#region Functions

    function TestEmail(input, settings) {
        /// <signature>
        /// <summary>Test to see if an email is valid.</summary>
        /// <param name="input" type="ItemInfo">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (emptyNotRequired(input.item, settings)) {
            input.set('validation', INPUT_STATUS.VALID);
            return true;
        }
        if (!formvalidate_emailRegex.test(input.item.val())) {
            input.set('validation', INPUT_STATUS.ERROR, settings.validationMessage);
            return false;
        }
        input.set('validation', INPUT_STATUS.VALID);
        return true;
    }

    function TestPhone(input, settings) {
        /// <signature>
        /// <summary>Test to see if an phone is valid.</summary>
        /// <param name="input" type="ErrorList">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (emptyNotRequired(input.item, settings)) {
            input.set('validation', INPUT_STATUS.VALID);
            return true;
        }
        if (!formvalidate_phoneRegex.test(input.item.val())) {
            input.set('validation', INPUT_STATUS.ERROR, settings.validationMessage);
            return false;
        }
        input.set('validation', INPUT_STATUS.VALID);
        return true;

    }

    function TestZip(input, settings) {
        /// <signature>
        /// <summary>Test to see if a zip code is valid.</summary>
        /// <param name="input" type="ErrorList">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (emptyNotRequired(input.item, settings)) {
            input.set('validation', INPUT_STATUS.VALID);
            return true;
        }
        if (!formvalidate_zipRegex.test(input.item.val())) {
            input.set('validation', INPUT_STATUS.ERROR, settings.validationMessage);
            return false;
        }
        input.set('validation', INPUT_STATUS.VALID);
        return true;

    }

    function TestNumber(input, settings) {
        /// <signature>
        /// <summary>Test to see if a valid number.</summary>
        /// <param name="input" type="ErrorList">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (emptyNotRequired(input.item, settings)) {
            input.set('validation', INPUT_STATUS.VALID);
            return true;
        }
        if (isNaN(input.item.val())) {
            input.set('number', INPUT_STATUS.ERROR, settings.validationMessage);
            return false;
        }
        input.set('validation', INPUT_STATUS.VALID);
        return true;
    }

    function TestNumberRange(input, settings) {
        /// <signature>
        /// <summary>Test to see if a valid number.</summary>
        /// <param name="input" type="ErrorList">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (emptyNotRequired(input.item, settings)) {
            input.set('validation', INPUT_STATUS.VALID);
            return true;
        }
        if (settings.validationMessage === null) {
            settings.validationMessage = 'You must supply a number.';
        }
        if (isNaN(input.item.val())) {
            input.set('number', INPUT_STATUS.ERROR, settings.validationMessage);
        } else {
            input.set('number', INPUT_STATUS.VALID);
            var t_value = parseInt(input.item.val());
            var t_rmessage = settings.rangeMessage.replace('[min]', settings.rangeMin).replace('[max]', settings.rangeMax);
            if (settings.rangeMin !== null && ((settings.rangeMinInclusive && t_value < settings.rangeMin) || (!settings.rangeMinInclusive && t_value <= settings.rangeMin))) {
                input.set('number', INPUT_STATUS.ERROR, t_rmessage);
                return false;
            } else if (settings.rangeMax !== null && ((settings.rangeMaxInclusive && t_value > settings.rangeMax) || (!settings.rangeMaxInclusive && t_value >= settings.rangeMax))) {
                input.set('number', INPUT_STATUS.ERROR, t_rmessage);
                return false;
            } else {
                input.set('number', INPUT_STATUS.VALID);
                return true;
            }
        }
    }

    function TestCharacters(input, settings) {
        /// <signature>
        /// <summary>Test to see if a valid number.</summary>
        /// <param name="input" type="ErrorList">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (emptyNotRequired(input.item, settings)) {
            input.set('validation', INPUT_STATUS.VALID);
            return true;
        }
        if (input.item.val().length > settings.max) {
            var t_message = settings.maxMessage.replace('[now]', input.item.val().length);
            input.set('max', INPUT_STATUS.ERROR, t_message);
            return false;
        }
        input.set('max', INPUT_STATUS.VALID);
        return false;
    }

    function TestRequired(input, settings) {
        /// <signature>
        /// <summary>Test to see if the field is filled out.</summary>
        /// <param name="input" type="ErrorList">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if ($.trim(input.item.val()) === '') {
            input.set('required', INPUT_STATUS.ERROR, settings.requiredMessage);
            return false;
        }
        input.set('required', INPUT_STATUS.VALID);
        return true;
    }

    function emptyNotRequired(item, settings) {
        /// <signature>
        /// <summary>Check to see if the item is required and no value.</summary>
        /// <param name="item" type="jQuery">A list of errors.</param>
        /// <param name="settings" type="Plain Object">A list of item options</param>
        /// <returns type="Boolean" />
        /// </signature>
        if ($.trim(settings.required && item.val()) === '') {
            return false;
        }
        if ($.trim(item.val()) === '') {
            return true;
        }
        return false;
    }

    //#endregion

    //#region Objects
    function FormInfo (form, delegates) {
        /// <signature>
        /// <summary>Creates a FormInfo object with a form.</summary>
        /// <param name="form" type="jQuery">The form object.</param>
        /// <returns type="FormInfo" />
        /// </signature>
        /// <field name="form" type="jQuery">The form being manipulated.</field>
        /// <field name="items" type="Array" elementType="ItemInfo">The list of items in the form.</field>
        /// <field name="delegates" type="Array" elementType="Function">The list of delegates to run on submission.</field>
        if (typeof (form) === 'undefined' || form === null) {
            throw 'The form must be supplied for FormItems.';
        }
        if (typeof (delegates) === 'undefined' || delegates === null) {
            delegates = [];
        }
        this.delegates = delegates;
        this.form = form;
        this.items = [];
        this.find = function (id) {
            /// <signature>
            /// <summary>Find the list of errors for a specified input by id.</summary>
            /// <param name="id" type="String">The id of the input.</param>
            /// <returns type="FormItem" />
            /// </signature>
            for (var item_index = 0; item_index < this.items.length; item_index++) {
                if (this.items[item_index].id === id) {
                    return this.items[item_index];
                }
            }
            return null;
        };
        this.set = function (id, info) {
            /// <signature>
            /// <summary>Sets error list of a sepcific id.</summary>
            /// <param name="id" type="String">The id of the input.</param>
            /// <param name="list" type="ItemInfo">The list of errors.</param>
            /// </signature>
            var _current = this.find(id);
            if (_current === null) {
                this.items.push(info);
            } else {
                _current = info;
            }
        };
        this.hasErrors = function () {
            /// <signature>
            /// <summary>Checks if there are errors in the form.</summary>
            /// <returns type="Boolean" />
            /// </signature>
            for (var i = 0; i < this.items.length; i++) {
                if (this.items[i].errors.getStatus().error > 0) {
                    return true;
                }
            }
            return false;
        };
        this.setErrors = function () {
            /// <signature>
            /// <summary>Sets the form errors if any. Returns true if there where errors, false otherwise.</summary>
            /// <returns type="Boolean" />
            /// </signature>
            var _error = false;
            for (var i = 0; i < this.items.length; i++) {
                var t_error = this.items[i].setError();
                _error = _error || t_error;
            }
            return _error;
        };
        this.submit = function () {
            /// <signature>
            /// <sumary>Runs all delegates for submission.</sumary>
            /// <returns type="Boolean" />
            /// </signature>
            for (var i = 0; i < this.delegates.length; i++) {
                this.delegates[i]();
            }
            return this.setErrors();
        };
    };

    function InputStatus (type, error, message) {
        /// <signature>
        /// <summary>Contsructs a new InputError object.</summary>
        /// <param name="type" type="String">The error type.</param>
        /// <param name="error" type="Number">The error status.</param>
        /// <param name="message" type="String">The message to display for the error.</param>
        /// <returns type="InputStatus" />
        /// </signature>
        /// <field name="type" type="String">The type of error.</field>
        /// <field name="message" type="String">The message to display on an error.</field>
        /// <field name="error" type="Number">The error status.</field>
        this.type = type;
        this.error = error;
        this.message = message;
        if (typeof (type) === 'undefined' || type === null) {
            this.type = 'none';
        }
        if (typeof (error) === 'undefined' || error === null || error > 2) {
            this.error = INPUT_STATUS.VALID;
        }
        if (typeof (message) === 'undefined' || message === null) {
            this.message = '';
        }
    }

    function ItemInfo(item, container, settings) {
        /// <signature>
        /// <summary>Contsructs a new ItemInfo object.</summary>
        /// <param name="item" type="jQuery">The input to manipulate.</param>
        /// <param name="container" type="jQuery">The container to manipulate.</param>
        /// <returns type="ItemInfo" />
        /// </signature>
        /// <field name="item" type="jQuery">The item being manipulated.</field>
        /// <field name="container" type="jQuery">The container of the item.</field>
        /// <field name="settings" type="ItemSettings">The settings for the item.</field>
        /// <field name="id" type="String">The id of the item dom tag.</field>
        /// <field name="errors" type="Array" elementType="InputStatus">Item Statuses.</field>
        /// <field name="messageBox" type="jQuery">Message div to place the error message.</field>
        var _errors = [];
        if (typeof (item) === 'undefined' || item === null) {
            throw 'Item must be supplied for ItemInfo object constructor.';
        }
        if (typeof (container) === 'undefined' || item === null) {
            throw 'Container must be supplied for ItemInfo in constructor.';
        }
        this.errors = _errors;
        this.item = item;
        this.id = item.attr('id');
        this.settings = settings;
        this.container = container;
        this.messageBox = container.find(this.settings.messageClass);
        this.find = function (type) {
            /// <signature>
            /// <summary>Finds the specified error type.</summary>
            /// <param name="type" type="String">The type of error to find.</param>
            /// <returns type="InputStatus" />
            /// </signature>
            for (var error_index = 0; error_index < _errors.length; error_index++) {
                if (_errors[error_index].type === type) {
                    return _errors[error_index];
                }
            }
            return null;
        };
        this.set = function (type, error, message) {
            /// <signature>
            /// <summary>Sets current status of the item.</summary>
            /// <param name="type" type="String">The type of error to add.</param>
            /// <param name="error" type="Number">The error status.</param>
            /// <param name="message" type="String">The message to display.</param>
            /// </signature>
            var _current = this.find(type);
            if (typeof (message) === 'undefined') {
                message = "There was an error.";
            }
            if (_current === null) {
                _errors.push(new InputStatus(type, error, message));
            } else {
                _current.error = error;
                _current.message = message;
            }
        };
        this.remove = function (type) {
            /// <signature>
            /// <summary>Removes the passed error type.</summary>
            /// <param name="type" type="String">The type of error to remove.</param>
            /// </signature>
            var _index = -1;
            for (var error_index = 0; error_index < this.errors.length; error_index++) {
                if (this.errors[error_index].type === type) {
                    _index = error_index;
                    break;
                }
            }
            if (_index !== -1) {
                this.errors.splice(_index, 1);
            }
        };
        this.getStatus = function getStatus () {
            /// <signature>
            /// <summary>Gets current status of the item.</summary>
            /// <returns type="InputStatus" />
            /// </signature>
            var t_status = new InputStatus('none', INPUT_STATUS.VALID, '');
            for (var error_index = 0; error_index < _errors.length; error_index++) {
                if (_errors[error_index].error > t_status.error) {
                    t_status = _errors[error_index];
                }
            }
            return t_status;
        };
        this.setError = function () {
            /// <signature>
            /// <summary>Sets the error status for a specific container. Returns true if there where errors, false otherwise.</summary>
            /// <returns type="Boolean" />
            /// </signature>
            var c_status = this.getStatus();
            if (c_status.error === INPUT_STATUS.WARNING) {
                this.container.addClass('has-warning');
                this.container.removeClass('has-error');
                if (this.messageBox !== null) {
                    this.messageBox.hide();
                }
                return false;
            }
            if (c_status.error === INPUT_STATUS.ERROR) {
                this.container.removeClass('has-warning');
                this.container.addClass('has-error');
                if (this.messageBox !== null) {
                    this.messageBox.show()
                    this.messageBox.find(this.settings.messageClass + '-message').html(c_status.message);
                }
                return true;
            }
            this.container.removeClass('has-warning');
            this.container.removeClass('has-error');
            if (this.messageBox !== null) {
                this.messageBox.hide();
            }
            return false;
        };
        if (this.messageBox.length < 1) {
            this.messageBox = null;
        }
    }

    function ItemSettings(options)
    {
        /// <signature>
        /// <summary>Contsructs a new ItemSettings object.</summary>
        /// <param name="options" type="PlainObject">The list of settings.</param>
        /// <returns type="ItemSettings" />
        /// </signature>
        /// <signature>
        /// <summary>Contsructs a new ItemSettings object.</summary>
        /// <param name="options" type="ItemSettings">The list of settings.</param>
        /// <returns type="ItemSettings" />
        /// </signature>
        /// <field name="required" type="Boolean">The input is required.</field>
        /// <field name="validation" type="String">The type of validation.</field>
        /// <field name="max" type="Number">The number of allowed characters. -1 for no limit.</field>
        /// <field name="showCharacters" type="Boolean">Wether the character count should be shown.</field>
        /// <field name="onInput" type="Boolean">The input should be tested when changed.</field>
        /// <field name="onSubmit" type="Boolean">The form should run validation on submit. If false, the validation is run on event "bs.form.submit".</field>
        /// <field name="containerClass" type="String">The class the container of input items, ".form-group" by default.</field>
        /// <field name="requiredMessage" type="String">The message displayed when required.</field>
        /// <field name="maxMessage" type="String">The message to display when there are too many characters.</field>
        /// <field name="rangeMin" type="Number">The minimum range for numbers, null if no range.</field>
        /// <field name="rangeMax" type="Number">The maximum range for numbers, null if no range.</field>
        /// <field name="rangeMinInclusive" type="Boolean">The range min should be included.</field>
        /// <field name="rangeMaxInclusive" type="Boolean">The range max should be included.</field>
        /// <field name="validationMessage" type="String">The message to diplay when validation fails, null if no message. default: null</field>
        /// <field name="rangeMessage" type="String">The message to display when the number is out of range.</field>
        /// <field name="inputClass" type="String">A class to narrow down the inputs to use.</field>
        /// <field name="messageClass" type="String">The class for the message box. default: ".form-message"</field>
        /// <field name="warning" type="String">The class for the form warning box. default: ".form-warning"</field>
        this.required = false;
        this.validation = 'text';
        this.max = -1;
        this.showCharacters = false;
        this.onInput = true;
        this.onSubmit = true;
        this.containerClass = '.form-group';
        this.requiredMessage = 'This field is required.';
        this.maxMessage = 'Too many characters. [max] characters max.';
        this.rangeMin = null;
        this.rangeMax = null;
        this.rangeMinInclusive = true;
        this.rangeMaxInclusive = true;
        this.validationMessage = null;
        this.rangeMessage = 'The item is outside the bounds.  It must be between [min] and [max].';
        this.inputClass = null;
        this.messageClass = '.form-messagebox';
        this.warning = '.form-warning';
        this.showProcessing = true;
        $.extend(this, options);
        this.maxMessage = this.maxMessage.replace(/\[max\]/, this.max);
        this.rangeMessage = this.rangeMessage.replace(/\[min\]/, this.rangeMin).replace(/\[max\]/, this.rangeMax);
        this.setValidation = function (value) {
            this.validation = value;
            switch (this.validation) {
                case 'email':
                    this.validationMessage = 'You must supply a valid email.';
                    break;
                case 'phone':
                case 'usphone':
                    this.validationMessage = 'You must supply a valid phone.';
                    break;
                case 'zip':
                case 'zipcode':
                    this.validationMessage = 'You must supply a valid zip code.';
                    break;
                case 'number':
                    this.validationMessage = 'You must supply a valid number.';
                    break;
            }
        };
        this.setValidation(this.validationMessage);
    }
    //#endregion

    /*jshint ignore:start*/
    function showModal(message, title) {
        /// <signature>
        /// <summary>Displays a message in a modal.</summary>
        /// <param name="message" type="string">The message to display</param>
        /// <param name="title" type="string">The title to display</param>
        /// </signature>
        message = message || 'Form Error';
        title = title || 'Form Error';
        var t_messageModal = $(restful_html_object.SHOWMESSAGE);
        $('body').append(t_messageModal);
        t_messageModal.find('.form-modal-title').html(title);
        t_messageModal.find('.form-modal-body').html(message);
        t_messageModal.on('hidden.bs.modal', function () {
            t_messageModal.remove();
        });
        t_messageModal.modal('show');

    }
    /*jshint ignore:end*/

    var INPUT_STATUS = {
        VALID: 0,
        WARNING: 1,
        ERROR: 2
    };
    var MESSAGE_MODAL = '<div class="modal fade form-modal-showmessage" data-backdrop="static" data-keyboard="false"><div class="modal-dialog"><div class="modal-header"><h3 class="form-modal-title"></h3></div><div class="modal-body"><div class="row form-modal-body"></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">OK</button></div></div></div>';

    /* Get tag */
    $.fn.getTag = function (tag, defaultValue, type) {
        /// <signature>
        /// <summary>Gets the tag value if it exists. If not it returns the default passed value.</summary>
        /// <param name="tag" type="String">The tag to search for.</param>
        /// <param name="defaultValue" type="String">The default value to return if the tag is not present.</param>
        /// <param name="type" type="String">The type of variable you want returned (string by default).</param>
        /// <returns type="String" />
        /// </signature>
        if (typeof (type) === 'undefined' || type === null) {
            type = 'string';
        } else {
            type = type.toLowerCase();
        }
        if (this.length < 1) {
            return null;
        }
        var t_value = $(this[0]).attr(tag);
        if (typeof (t_value) === 'undefined' || t_value === null) {
            return defaultValue;
        } else {
            switch (type) {
                case 'bool':
                    t_value = t_value.toLowerCase() === 'true';
                    break;
                case 'number':
                    if (isNaN(t_value)) {
                        t_value = 0;
                    } else {
                        t_value = parseInt(t_value);
                    }
                    break;
                case 'number?':
                    if (isNaN(t_value)) {
                        t_value = null;
                    } else {
                        t_value = parseInt(t_value);
                    }
                    break;
            }
        }
        return t_value;
    };

}(jQuery));