/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

$(window).ready(function () {
        $("section.GroupContents").addClass("GroupContentsHide");
});
$(window).ready(function () {
    $(".NavLabel").mouseenter(function () {
        $(this).css("background-color", "rgba(0, 0, 0, .2)");
    });
    $(".NavLabel").mouseleave(function () {
        $(this).css("background-color", "transparent");
    });
    $(".NavLabel").click(function () {
        $(this).parent().children(".GroupContents").toggleClass("GroupContentsHide").toggleClass("GroupContentsShow");
        $(this).parent().children(".glyphicon").toggleClass("glyphicon-chevron-right").toggleClass("glyphicon-chevron-down");
    });
});