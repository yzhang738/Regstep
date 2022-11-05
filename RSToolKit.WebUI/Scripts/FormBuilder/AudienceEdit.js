
$(document).ready(function () {


    //Set on click for the audience edit span
    //it contains and id of 'audiences'
    $('#audiences').on('click', function () {
        SetAudiences();
    });



});

function SetAudiences() {
    var count = 0;
    var UsedAudiences = [];
    var UnusedAudiences = Audiences.slice(0);
    $('#setAudiences').children('input[type=hidden]').each(function (i) {
        for (var j = 0; j < Audiences.length; j++) {
            if (Audiences[j].Id == $(this).val()) {
                UsedAudiences[count++] = { Id: Audiences[j].Id, Name: Audiences[j].Name }
                UnusedAudiences[j] = "";
                break;
            }
        }
    });
    for (var i = 0; i < UsedAudiences.length; i++) {
        $("#" + UsedAudiences[i].Id).appendTo('#selectedAudiences');
        $("#" + UsedAudiences[i].Id).find('span.TextLink').text("Remove");
    }
    $('#AudienceLightBox').dialog({
        autoOpen: false,
        modal: true,
        width: 600,
        close: function (e, ui) {
            var ind = 0;
            var newList = "";
            $('#setAudiences').html('');
            $("#selectedAudiences div").each(function (i) {
                $('#setAudiences').append('<input type="hidden" name="SetAudiences[' + i + ']" value="' + $(this).attr('id') + '" />');
                newList += $(this).children('.audName').text() + '<br />';
            });
            if (newList.length > 6) {
                newList = newList.substr(0, newList.length - 6);
            } else {
                newList = "Edit";
            }
            $('#audiences').html(newList);
            $(".AudienceLabel").each(function () {
                $(this).appendTo("#unselectedAudiences");
                $(this).find("span.TextLink").text("Add");
            });
        }
    });
    $('#AudienceLightBox').dialog('open');
}

function moveAudience(id) {
    if ($.contains(document.getElementById("unselectedAudiences"), document.getElementById(id))) {
        $("#" + id).prependTo("#selectedAudiences");
        $("#" + id + " span.TextLink").text("Remove");
    }
    else {
        $("#" + id).prependTo("#unselectedAudiences");
        $("#" + id + " span.TextLink").text("Add");
    }
}