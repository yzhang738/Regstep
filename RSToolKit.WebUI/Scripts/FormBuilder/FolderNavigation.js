$(document).ready(function () {
    $('.form-list').attr('unselectable', 'on').css('user-select', 'none').on('selectstart', false);
    $('.folder').on('click', function () {
        $(this).children('.folder-label').children('.glyphicon').toggleClass('glyphicon-folder-open').toggleClass('glyphicon-folder-close');
        if ($(this).parent().children('.folder-contents').is('.collapse')) {
            $(this).parent().children('.folder-contents').toggleClass('collapse');
        } else {
            $(this).parent().children('.folder-contents').addClass('collapse');
        }
    });
});