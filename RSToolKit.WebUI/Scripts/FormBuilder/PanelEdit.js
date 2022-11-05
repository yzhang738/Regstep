/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

//http://jsfiddle.net/E2ZUe/23/

var changed = false;

$(function () {

    $(".LightBoxHidden").dialog({
        autoOpen: false,
        modal: true,
        width: 300
    });

    $("#AddComponent").click(function () {
        $("#NewComponent").dialog('open');
    });

    $("input").on("click", function () { changed = true; });

    var fixHelperModified = function (e, span) {
        var $originals = span.children();
        var $helper = span.clone();
        $helper.children().each(function (index) {
            $(this).width($originals.eq(index).width())
        });
        return $helper;
    };

    $(function () {
        var open = false;
        $("#Reorder").click(function () {
            if (!open) {
                $("#ComponentsOrder").show(500);
                $("#Reorder").text("Finish Reordering");
                open = true;
            } else {
                $("#ComponentsOrder").hide(500);
                $("#Reorder").text("Reorder Components");
                open = false;
            }
        });
        MakeSortable();
    });

});

function MakeSortable() {

    ///////////////////
    // Audience Link //
    ///////////////////
    $('.LightBoxLink').mouseenter(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    $('.LightBoxLink').mouseleave(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    $(".ComponentRow").sortable({
        connectWith: ".ComponentRow",
        stop: function (e, ui) {
            StopSorting();
        }
    }).disableSelection();
}

function StopSorting() {
    $('.ComponentRow').each(function (r) {
        var emptyRow = true;
        $('.Column', this).each(function (c) {
            emptyRow = false;
            return false;
        });
        if (emptyRow) {
            $(this).remove();
            return true;
        }
    });
    $('.ComponentRow').each(function (r) {
        $('.Column', this).each(function (c) {
            $("#" + $(this).attr("id") + "tag").attr("data-row", r + 1).attr("data-col", c + 1);
        })
    });
    $('#Components').append('<div class="ComponentRow"></div>');
    MakeSortable();
    SortComponents();
};

function SortComponents() {
    var jObjs = [];
    var rObjs = [];
    var ri = 0;
    $('.RowDes').each(function (i) {
        $(this).remove();
    });
    $(".InfoColumn").each(function (i) {
        if ($(this).hasClass("RowDes")) {
            $(this).remove();
        } else {
            var r = parseInt($(this).attr('data-row'), 10);
            var c = parseInt($(this).attr('data-col'), 10);
            $(this).find('input[name^=ComponentsRow]').attr('value', r);
            $(this).find('input[name^=ComponentsCol]').attr('value', c);
            jObjs[i] = this;
        }
    });
    jObjs.sort(function (a, b) {
        var rR = parseInt($(a).attr('data-row'), 10) - parseInt($(b).attr('data-row'), 10);
        var rC = parseInt($(a).attr('data-col'), 10) - parseInt($(b).attr('data-col'), 10);
        if (rR === 0) {
            return rC;
        } else {
            return rR;
        }
    });

    var curRow = -1;
    for (var i = 0; i < jObjs.length; i++) {
        var row = parseInt($(jObjs[i]).attr("data-row"), 10);
        if (row > curRow) {
            curRow = row;
            if ($("#Row" + curRow).length) {
                $("#Row" + curRow).appendTo(".InfoTable");
            } else {
                $(".InfoTable").append('<tr class="RowDes" id="Row' + curRow + '"><td colspan="10">Row ' + curRow + '</td></tr>');
            }
        }
        $(jObjs[i]).appendTo(".InfoTable");
    }
}




function SetAudience(index) {
    $("#AudienceFor").val(index);
    var UsedAudiences = [];
    var UnusedAudiences = Audiences.slice(0);
    var curCount = 0;
    var rgxTest = /!([^!]*)/g;
    var value = $("#ComponentsAudience_" + index + "_").val();
    var result = rgxTest.exec(value);
    while (result != null) {
        for (var i = 0; i < Audiences.length; i++) {
            if (Audiences[i].Id == RegExp.$1) {
                UsedAudiences[curCount++] = { Id: Audiences[i].Id, Name: Audiences[i].Name };
                UnusedAudiences[i] = "";
                break;
            }
        }
        result = rgxTest.exec(value);
    }
    for (var i = 0; i < UsedAudiences.length; i++) {
        $("#" + UsedAudiences[i].Id).appendTo('#selectedAudiences');
        $("#" + UsedAudiences[i].Id).find('span').text("Remove");
    }
    $("#AudienceLightBox").dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function (e, ui) {
            var ind = $("#AudienceFor").val();
            var value = "";
            $("#selectedAudiences div").each(function () {
                value += "!" + $(this).attr('id');
            });
            $("#ComponentsAudience_" + ind + "_").val(value);
            $(".AudienceLabel").each(function () {
                $(this).appendTo("#unselectedAudiences");
                $(this).find("span").text("Add");
            });
        }
    })
    $("#AudienceLightBox").dialog('open');
}

function moveAudience(id) {
    if ($.contains(document.getElementById("unselectedAudiences"), document.getElementById(id))) {
        $("#" + id).prependTo("#selectedAudiences");
        $("#" + id + " span").text("Remove");
    }
    else {
        $("#" + id).prependTo("#unselectedAudiences");
        $("#" + id + " span").text("Add");
    }
}
