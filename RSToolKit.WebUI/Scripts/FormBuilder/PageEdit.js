/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var changed = false;

$(function () {

    ///////////////////
    // Audience Link //
    ///////////////////
    $('.LightBoxLink').mouseenter(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    $('.LightBoxLink').mouseleave(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    $(".LightBoxHidden").dialog({
        autoOpen: false,
        modal: true,
        width: 600
    })

    $("input").on("click", function () { changed = true; });

    var fixHelperModified = function (e, tr) {
        var $originals = tr.children();
        var $helper = tr.clone();
        $helper.children().each(function (index) {
            $(this).width($originals.eq(index).width())
        });
        return $helper;
    };

    $("#FormPages tbody").sortable({
        helper: fixHelperModified,
        stop: function (e, ui) {
            $('input[name^=PanelNumber]').each(function (i) {
                $(this).val((i + 1));
            });
            $('span.PageNumber').each(function (i) {
                $(this).text("Panel " + (i + 1) + ":");
            });
        }
    })
});