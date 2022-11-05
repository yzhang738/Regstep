$(document).ready(function () {
    $('div.toggle-edit').each(function () {
        $(this).attr('data-toggled-edit', 'true');
        $(this).append('<span>' + $(this).children('input').val() + '</span>');
        $(this).children('input').hide().on('blur', function () {
            $(this).hide();
            $(this).parent().children('span').html($(this).val()).show();
            $(this).trigger('toggle-edit-change');
        }).on('change', function () {
            $(this).parent().children('span').html($(this).val());
        });
        $(this).children('span').on('click', function () {
            $(this).hide();
            $(this).parent().children('input').show().focus().select();
        });
    });
});

$.fn.toggleEdit