/// <reference path="jquery-1.10.2.js" />
/// <reference path="jquery-ui-1.10.3.RegStep.js" />
/// <reference path="jquery-1.9.1.js" />
/// <reference path="jquery.validate.js" />
/// <reference path="jquery.validate.unobtrusive.js" />
/// <reference path="knockout-2.2.0.debug.js" />
/// <reference path="modernizr-2.6.2.js" />

var validateErrors = false;
var audTd;

$(function () {

    $('.Color').ColorPicker({
        onSubmit: function (hsb, hex, rgb, el) {
            var par = $(el).parent('td');
            $(par).children('span').show();
            $(el).val(hex);
            $(el).ColorPickerHide();
        },
        onBeforeShow: function () {
            $(this).ColorPickerSetColor($(this).val());
        }
    }).on('keyup', function () {
        $(this).ColorPickerSetColor($(this).val());
    });

    ///////////////////
    // Audience Link //
    ///////////////////
    $('.LightBoxLink').mouseenter(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    $('.LightBoxLink').mouseleave(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    /////////////////
    // Footer Link //
    /////////////////
    $('.FooterLink').mouseenter(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    $('.FooterLink').mouseleave(function () {
        $(this).toggleClass('LightBoxLinkUnderline');
    });

    //////////////
    //  Styles  //
    //////////////
    $('#StyleTabs').tabs();

    $(".LightBoxHidden").dialog({
        autoOpen: false,
        modal: true,
        width: 600
    });

    //Input Css
    if ($("input[name=CssBackgroundColor]").val() == "") {
        $("#MainBGCClear").show();
    } else {
        $("#MainBGCClear").hide();
    }

    $("#ClearMainBGC").on('click', function () {
        $("input[name=CssBackgroundColor]").val('');
        $("#MainBGCClear").show();
    });

    if ($("input[name=CssColor]").val() == "") {
        $("#MainCClear").show();
    } else {
        $("#MainCClear").hide();
    }

    $("#ClearMainC").on('click', function () {
        $("input[name=CssColor]").val('');
        $("#MainCClear").show();
    });

    if ($("input[name=CssBorderColor]").val() == "") {
        $("#MainBorderCClear").show();
    } else {
        $("#MainBorderCClear").hide();
    }

    $("#ClearMainBorderC").on('click', function () {
        $("input[name=CssBorderColor]").val('');
        $("#MainBorderCClear").show();
    });

    //Light Box
    $("#StyleOpener").on('click', function () {
        $("#StyleLightBox").dialog('open');
    });

    

    $("#StyleLightBox").dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function (e, ui) {
            var mainCss = "";
            var labelCss = "";
            var altCss = "";
            var first = true;
            $('.MainCss').each(function (i) {
                var value = $(this).val();
                var tag = $(this).attr('data-tag');
                if (tag == 'font-weight') {
                    if ($(this).is(':checked')) {
                        value = "bold";
                    }
                }
                if (tag == 'font-style') {
                    if ($(this).is(':checked')) {
                        value = "italic";
                    }
                }
                if (tag == 'font-family') {
                    value = $(this).find(':selected').attr('value');
                }
                if (value == "") return;
                if (tag == 'border') {
                    value = '2px solid ' + value;
                    $("#MainBorderCClear").hide();
                }
                if (tag == "color") {
                    $("#MainCClear").hide();
                }
                if (tag == "background-color") {
                    $("#MainBGCClear").hide();
                }
                if (tag == 'font-size') {
                    var testEnd = /(pt|px|em)$/;
                    var testNumber = /^[0-9]+(pt|px|em)?$/;
                    if (!testNumber.test(value)) {
                        value = "";
                        $(this).val(value);
                        return;
                    }
                    if (!testEnd.test(value)) {
                        value += "pt";
                        $(this).val(value);
                    }
                }
                if (!first) mainCss += " ";
                mainCss += tag + ": " + value + ";";
                first = false;
            });
            $("#InlineCss").attr('value', mainCss);
        }
    });

    $('#PriceBox').dialog({
        autoOpen: false,
        modal: true,
        width: 800,
        close: function (e, ui) {
            var ind = $('input[name=CurrentPriceList]').val();
            $('#PriceList_' + ind + '_').html('');
            $('.LBPriceItem').each(function (i) {
                var id = $(this).find('.TempId').val();
                var date = $(this).find('input[data-type=PriceDate]').val();
                var price = $(this).find('input[data-type=Price]').val();
                var list = $('<div class="PriceItem_' + ind + '_"></div>').appendTo('#PriceList_' + ind + '_');
                $('<input type="hidden" name="Prices[' + ind + '].Prices[' + i + '].Id" value="' + id + '" data-type="PriceId" />').appendTo(list);
                $('<input type="hidden" name="Prices[' + ind + '].Prices[' + i + '].Date" value="' + date + '" data-type="PriceDate" />').appendTo(list);
                $('<input type="hidden" name="Prices[' + ind + '].Prices[' + i + '].Price" value="' + price + '" data-type="Price" />').appendTo(list);
            });
        }
    });
});

function EditPrice(ind) {
    $('#PriceItems').html('');
    $('input[name=CurrentPriceList]').val(ind);
    $('.PriceItem_' + ind + '_').each(function (i) {
        var id = $('input[name$=Id]', this).val();
        var date = $('input[name$=Date]', this).val();
        var price = $('input[name$=Price]', this).val();
        console.log(id + "\n" + date + "\n" + price + "\n\n");
        $('<tr class="LBPriceItem" id="LBPriceItem_' + i + '"><td><input type="hidden" class="TempId" value="' + id + '"><input class="PriceDate" type="text" data-type="PriceDate" name="Date_' + i + '_" value="' + date + '" /></td><td><input type="text" class="ItemPrice" data-type="Price" name="Price_' + i + '_" value="' + price + '" /></td><td><span class="LightBoxLink" onclick="DeletePriceItem(this)">Delete</span></td></tr>').appendTo("#PriceItems");
    });
    $('.PriceDate').datepicker('destroy');
    $('#PriceBox').dialog('open');
    $('.PriceDate').datepicker({
        changeMonth: true,
        changeYear: true
    });
}

function AddPrice() {
    $('<tr class="LBPriceItem"><td><input type="hidden" class="TempId" value="00000000000000000000000000000000"><input class="PriceDate" type="text" data-type="PriceDate" value="6/6/1944 4:30:00 AM" /></td><td><input type="text" class="ItemPrice" data-type="Price" value="0.00" /></td><td><span class="LightBoxLink" onclick="DeletePrice(this)">Delete</span></td></tr>').appendTo("#PriceItems");
    $('.PriceDate').datepicker();
}

function DeletePrice(ind) {
    $('#PriceListTr_' + ind + '_').remove();
    $('.PriceList').each(function (i) {
        $('.PriceLabel', this).text('Price List ' + (i + 1));
        $(this).attr('id', 'PriceListTr_' + i + '_');
        $(this).find('.PriceAud').attr('data-ind', i);
        $(this).find('input[data-type=PriceListId]').attr('name', 'Prices[' + i + '].Id');
        $(this).find('input[data-type=AudId]').each(function (i2) {
            $(this).attr('name', 'Prices[' + i + '].Aud[' + i2 + '].Id');
        });
        $(this).find('input[data-type=AudName]').each(function (i2) {
            $(this).attr('name', 'Prices[' + i + '].Aud[' + i2 + '].Name');
        });
        $(this).find('input[data-type=PriceId]').each(function (i2) {
            $(this).attr('name', 'Prices[' + i + '].Prices[' + i2 + '].Id');
        });
        $(this).find('input[data-type=PriceDate]').each(function (i2) {
            $(this).attr('name', 'Prices[' + i + '].Prices[' + i2 + '].Date');
        });
        $(this).find('input[data-type=Price]').each(function (i2) {
            $(this).attr('name', 'Prices[' + i + '].Prices[' + i2 + '].Price');
        });
        $(this).find('span[onclick^=Edit]').attr('onclick', 'EditPrice(' + i + ')');
        $(this).find('span[onclick^=Delete]').attr('onclick', 'DeletePrice(' + i + ')');
        $(this).find('div[id^=PriceList_]').attr('id', 'PriceList_' + i + '_');
        $(this).find('div[class^=PriceItem_]').attr('class', 'PriceItem_' + i + '_');
    });
}

function AddPriceList() {
    var num = -1;
    $('.PriceList').each(function (i) {
        num = i;
    });
    num++;
    var tr = $('<tr id="PriceListTr_' + num + '_" class="PriceList"></tr>').appendTo('#Pricing');
    $('<td class="PriceLabel">Price List ' + (num + 1) + '</td>').appendTo(tr);
    $('<td class="PriceAud" data-ind="' + num + '"><span class="AudienceDisplay">ALL<br /></span><span class="LightBoxLink" onclick="SetAudience(this)">Edit</span></td>').appendTo(tr);
    var td = $('<td></td>').appendTo(tr);
    $('<span class="LightBoxLink" onclick="EditPrice(' + num + ')">Edit Prices</span>').appendTo(td);
    var div = $('<div id="PriceList_' + num + '_"></div>').appendTo(td);
    var itemDiv = $('<div class="PriceItem_' + num + '_"></div>').appendTo(div);
    $('<input type="hidden" name="Prices[' + num + '].Price[0].Id" data-type="PriceId" value="00000000000000000000000000000000" />').appendTo(itemDiv);
    $('<input type="hidden" name="Prices[' + num + '].Price[0].Date" data-type="PriceDate" value="6/6/1944 4:30:00 AM" />').appendTo(itemDiv);
    $('<input type="hidden" name="Prices[' + num + '].Price[0].Price" data-type="Price" value="0.00" />').appendTo(itemDiv);
    $('<td><span class="LightBoxLink" onclick="DeletePrice(' + num + ')">Delete</span><input type="hidden" name="Prices[' + num + '].Id" value="00000000000000000000000000000000" data-type="PriceListId" /></td>').appendTo(tr);
}

function DeletePriceItem(p) {
    $(p).parent().parent().remove();
}

function SetAudience(s) {
    //$("#AudienceFor").val(s);
    var UsedAudiences = [];
    var UnusedAudiences = Audiences.slice(0);
    var curCount = 0;
    var rgxTest = /!([^!]*)/g;
    audTd = $(s).parent();
    var value = "";
    $(audTd).find("input[name$=Id]").each(function (i) {
        value += "!" + $(this).val();
    });
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
            //var ind = $("#AudienceFor").val();
            var value = "";
            var index = $(audTd).attr('data-ind');
            var display = '';
            $(audTd).find('input').each(function (i) {
                $(this).remove();
            });
            $("#selectedAudiences div").each(function (i) {
                var id = $(this).attr('id');
                var name = $(this).text().replace(' Remove', '');
                $('<input type="hidden" name="Prices[' + index + '].Aud[' + i + '].Id" value="' + id + '" data-type="AudId" />').prependTo(audTd);
                $('<input type="hidden" name="Prices[' + index + '].Aud[' + i + '].Name" value="' + name + '" data-type="AudName" />').prependTo(audTd);
                display += name + "<br />";
            });
            if (display == '') {
                display = 'All<br />';
            }
            $(audTd).find('.AudienceDisplay').html(display);
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