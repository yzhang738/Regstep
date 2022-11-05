$(document).ready(function () {
    var navItem = $("#Navigation");
    var pos = navItem.position();
    $(window).scroll(function () {
        var windowPos = $(window).scrollTop();
        if (windowPos >= pos.top) {
            navItem.addClass("Stick");
        } else {
            navItem.removeClass("Stick");
        }
    });
});