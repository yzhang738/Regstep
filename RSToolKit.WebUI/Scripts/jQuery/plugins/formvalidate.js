/*
 * This plugin relies on jquery and bootstrap css.
 * Developed by Andrew B. Jackson (Spock) for use
 * under a free open liscene.  It can be used with
 * or without modification for personal or commercial
 * use.
 */

var formvalidate_emailRegex = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
var formvalidate_urlRegex = /((http|ftp|https):\/\/)?[\w-]+(\.[\w-]+)+([\w.,@?^=%&amp;:\/~+#-]*[\w@?^=%&amp;\/~+#-])?/;
var formvalidate_lettersAndNumberRegex = /^[a-zA-Z0-9]*$/;

$(document).ready(function () {
    /*
     * The first thing we do is check for forms that are asking to be validated
     * This is achieved by looking for a data-validate tag that is set to true
     */

    $('form[data-validate=true]').on('submit', function (ui, e) {
        var success = formvalidate_ValidateForm(this);
        if (success) {
            $(this).trigger("validatedsubmit");
        }
        if ($(this).is('[data-validate-ajax')) {
            return false;
        }
        return success;
    });

    /*
     * Next we will check all required fields and add a tooltip for them.
     * We will also add the required asterisk.
     */

    $('input[data-required]').each(function () {
        var position = $(this).attr('data-errorposition');
        if (position != 'top' || position != 'right' || position != 'bottom' || position != 'left') {
            position = 'right';
            if ($(this).attr('type') == 'checkbox') {
                position = 'left';
            }
        }
        var message = $(this).attr('data-errormessage');
        if (message === undefined || message == '') {
            message = 'This field is required.';
        }
        if ($(this).attr('type') == 'checkbox') {
            if ($(this).attr('data-noasterisk') != true) {
                $(this).after('<sup class="formvalidate-required formvalidate-padright formvalidate-padleft"><span class="glyphicon glyphicon-asterisk"></span></sup>');
            }
            $(this).popover({
                html: true,
                trigger: 'manual',
                content: '<span class="formvalidate-error">' + message + '</span>',
                container: 'body',
                placement: position
            });
        } else {
            //$(this).after('<sup class="formvalidate-required formvalidate-padright"><span class="glyphicon glyphicon-asterisk"></span></sup>');
            $(this).parents('.form-group').append('<sup class="formvalidate-required formvalidate-padleft"><span class="glyphicon glyphicon-asterisk"></span></sup>');
            $(this).popover({
                html: true,
                trigger: 'manual',
                content: '<span class="formvalidate-error">' + message + '</span>',
                container: 'body'
            });
        }
        $(this).on('focus', function () {
            $(this).parents('.form-group').removeClass('has-error');
        });
        $(this).on('keyup', function () {
            if ($(this).val() == '') {
                $(this).parents('.form-group').removeClass('has-error');
                $(this).parents('.form-group').addClass('has-warning');
            } else {
                $(this).parents('.form-group').removeClass('has-warning');
                $(this).parents('.form-group').removeClass('has-error');
            }
        });
        $(this).on('blur', function () {
            if ($(this).parents('.form-group').hasClass('has-warning') || $(this).val() == "") {
                $(this).parents('.form-group').removeClass('has-warning').addClass('has-error');
            }
        });
    });

    /*
     * Now we check for required fieldsets
     */

    $('fieldset[data-required][data-type="radio"]').each(function () {
        var position = $(this).attr('data-errorposition');
        if (position != 'top' || position != 'right' || position != 'bottom' || position != 'left') {
            position = 'right';
        }
        var message = $(this).attr('data-errormessage');
        if (message === undefined || message == '') {
            message = 'You must make a selection.';
        }
        $(this).find('[data-fieldsettitle]').popover({
            html: true,
            trigger: 'manual',
            content: '<span class="formvalidate-error">' + message + '</span>',
            container: 'body',
            placement: position
        });
        $(this).find('[data-fieldsettitle]').append('<sup class="formvalidate-required formvalidate-padleft"><span class="glyphicon glyphicon-asterisk"></span></sup>');
        var name = $(this).attr('data-for');
        $(this).find('input[type=radio]').attr('name', name);
    });

    /*
     * Now we start working with specific validators.
     * If the input is already required, this will override the required field.
     */

    $('input[data-validation]').each(function () {
        var validationType = $(this).attr('data-validation');
        var required = $(this).is('[data-required]');
        var value = $(this).val();
        var position = $(this).attr('data-errorposition');
        if (position != 'top' || position != 'right' || position != 'bottom' || position != 'left') {
            position = 'right';
        }
        var message = $(this).attr('data-errormessage');
        if (message === undefined || message == '') {
            message = 'The input is invalid.';
        }
        $(this).popover('destroy');
        $(this).popover({
            html: true,
            trigger: 'manual',
            content: '<span class="formvalidate-error">' + message + '</span>',
            container: 'body',
            placement: position
        });
        var testRgx = "none";
        switch (validationType) {
            case "email":
                testRgx = formvalidate_emailRegex;
                break;
            case "url":
                testRgx = formvalidate_urlRegex;
                break;
            case "letters and numbers":
                testRgx = formvalidate_lettersAndNumberRegex;
                break;
        }
        if (testRgx == 'none') {
            return;
        }
        $(this).off();
        $(this).on('focus', function () {
            $(this).parents('.form-group').removeClass('has-error');
        });
        $(this).on('keyup', function () {
            var value = $(this).val();
            if (required) {
                if (value == '') {
                    $(this).parents('.form-group').addClass('has-warning');
                } else if (!testRgx.test(value)) {
                    $(this).parents('.form-group').addClass('has-warning');
                } else {
                    $(this).parents('.form-group').removeClass('has-warning');
                }
            } else {
                if (value != '' && !testRgx.test(value)) {
                    $(this).parents('.form-group').addClass('has-warning');
                } else {
                    $(this).parents('.form-group').removeClass('has-warning');
                }
            }
        });
        $(this).on('blur', function () {
            var value = $(this).val();
            if (required) {
                if (value == '') {
                    $(this).parents('.form-group').removeClass('has-warning').addClass('has-error');
                } else if (!testRgx.test($(this).val())) {
                    $(this).parents('.form-group').removeClass('has-warning').addClass('has-error');
                } else {
                    $(this).parents('.form-group').removeClass('has-warning').removeClass('has-error');
                }
            } else {
                if (value != '' && !testRgx.test(value)) {
                    $(this).parents('.form-group').removeClass('has-warning').addClass('has-error');
                } else {
                    $(this).parents('.form-group').removeClass('has-warning').removeClass('has-error');
                }
            }
        });
    });

    /*
     * Now we check for confirmation validation
     */

    $('input[data-confirmation]').each(function () {
        var value = $(this).val();
        var position = $(this).attr('data-errorposition');
        if (position != 'top' || position != 'right' || position != 'bottom' || position != 'left') {
            position = 'right';
        }
        var message = $(this).attr('data-errormessage');
        if (message === undefined || message == '') {
            message = 'The fields must match.';
        }
        $(this).popover('destroy');
        $(this).popover({
            html: true,
            trigger: 'manual',
            content: '<span class="formvalidate-error">' + message + '</span>',
            container: 'body',
            placement: position
        });
        var confirmName = $(this).attr('data-confirmation');
        var originalField = $('[name="' + confirmName + '"]').first();
        $(this).off();
        $(this).on('focus', function () {
            if (!originalField.parents('.form-group').hasClass('has-error')) {
                originalField.parents('.form-group').removeClass('has-error').removeClass('has-warning');
            }
            $(this).parents('.form-group').removeClass('has-error').removeClass('has-warning');
        });
        $(this).on('keyup', function () {
            var value = $(this).val();
            var checkValue = originalField.val();
            if (value != checkValue) {
                if (!originalField.parents('.form-group').hasClass('has-error')) {
                    originalField.parents('.form-group').addClass('has-warning');
                }
                $(this).parents('.form-group').addClass('has-warning');
            } else {
                if (originalField.is('[data-required]') && checkValue == '') {
                    originalField.parents('.form-group').addClass('has-warning');
                    $(this).parents('.form-group').addClass('has-warning');
                } else {
                    originalField.parents('.form-group').removeClass('has-warning');
                    $(this).parents('.form-group').removeClass('has-warning');
                }
            }
        });
        $(this).on('blur', function () {
            var value = $(this).val();
            var checkValue = originalField.val();
            if (value != checkValue) {
                originalField.parents('.form-group').addClass('has-error').removeClass('has-warning');
                $(this).parents('.form-group').addClass('has-error').removeClass('has-warning');
            } else {
                if (originalField.is('[data-required]') && checkValue == '') {
                    originalField.parents('.form-group').addClass('has-error').removeClass('has-warning');
                    $(this).parents('.form-group').addClass('has-error').removeClass('has-warning');
                } else {
                    if (!originalField.parents('.form-group').hasClass('has-error')) {
                        originalField.parents('.form-group').removeClass('has-warning').removeClass('has-error');
                    }
                    $(this).parents('.form-group').removeClass('has-warning').removeClass('has-error');
                    originalField.trigger('blur');
                }
            }
        });

    });
});

function formvalidate_ValidateForm(form) {
    /*
     * First we remove any old errors.
     */

    $('.formvalidate-error', form).remove();
    $('.has-error', form).removeClass('has-error');

    /*
     * First we need to get all the input fields and test them
     */
    var formvalidate_error = false;

    /* 
     * First, required checkbox and inputs
     */

    $('input', form).each(function () {
        if ($(this).is('[data-required]')) {
            if ($(this).attr('type') == 'checkbox') {
                if (!$(this).is(':checked')) {
                    $(this).parents('.form-group').addClass('has-error');
                    $(this).popover('show');
                    $(this).one('click', function () {
                        $(this).popover('hide');
                    });
                    formvalidate_error = true;
                }
            } else {
                if ($(this).val() == '') {
                    $(this).parents('.form-group').addClass('has-error');
                    $(this).popover('show');
                    $(this).one('click', function () {
                        $(this).popover('hide');
                    });
                    formvalidate_error = true;
                }
            }
        }
    });

    /*
     * Now we check for required fieldsets
     */

    $('fieldset[data-required]', form).each(function () {
        var notChecked = true;
        $(this).find('input[type=radio]').each(function () {
            if ($(this).is(':checked')) {
                notChecked = false;
            }
        });
        if (notChecked) {
            $(this).find('[data-fieldsettitle]').popover('show');
            $(this).one('click', function () {
                $(this).find('[data-fieldsettitle]').popover('hide');
            })
            formvalidate_error = true;
        }
    });

    /*
     * Now we check for validation objects
     */

    $('[data-validation]').each(function () {
        var validationType = $(this).attr('data-validation');
        var required = $(this).is('[data-required]');
        if ((!required && $(this).val() == '') || (required && $(this).val() == '')) {
            return;
        }
        var testRgx = "none";
        switch (validationType) {
            case "email":
                testRgx = formvalidate_emailRegex;
                break;
        }
        if (testRgx == 'none') {
            return;
        }
        if (!testRgx.test($(this).val())) {
            $(this).parents('.form-group').addClass('has-error');
            $(this).popover('show');
            $(this).one('click', function () {
                $(this).popover('hide');
            });
            formvalidate_error = true;
        }
    });

    /*
     * Now lets check for confirm boxes
     */

    $('input[data-confirmation]').each(function () {
        var value = $(this).val();
        var confirmName = $(this).attr('data-confirmation');
        var originalField = $('[name="' + confirmName + '"]').first();
        var value = $(this).val();
        var checkValue = originalField.val();
        if (value != checkValue) {
            originalField.parents('.form-group').addClass('has-error').removeClass('has-warning');
            $(this).parents('.form-group').addClass('has-error').removeClass('has-warning');
            $(this).popover('show');
            $(this).one('click', function () {
                $(this).popover('hide');
            });
            formvalidate_error = true;
        } else {
            if (originalField.is('[data-required]') && checkValue == '') {
                originalField.parents('.form-group').addClass('has-error').removeClass('has-warning');
                $(this).parents('.form-group').addClass('has-error').removeClass('has-warning');
                $(this).popover('show');
                $(this).one('click', function () {
                    $(this).popover('hide');
                });
                formvalidate_error = true;

            }
        }
    });



    return !formvalidate_error;
}