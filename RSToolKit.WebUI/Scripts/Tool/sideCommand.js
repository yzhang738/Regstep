$(document).ready(function () {
    sideCommandShown = false;
    $('footer').each(function () {
        var text = "QUICK COMMANDS"
        text = $(this).html();
        var textArray = text.split('');
        text = "";
        for (var i = 0; i < textArray.length; i++) {
            text += textArray[i] + '<br />';
        }
        $(this).html(text);
    });
    $('footer').on('click', function () {
        sideCommandShown = !sideCommandShown;
        var pos = sideCommandShown ? 0 : -240;
        $(this).parents('[data-commands-side-left]').stop().animate({ left: pos + 'px' });
    });
    $('[data-commands-side-left]').on('mouseenter', function () {
        $(this).stop().animate({ left: '-230px' });
    });
    $('[data-commands-side-left]').on('mouseleave', function () {
        $(this).stop().animate({ left: '-240px' });
        sideCommandShown = false;
    });
});