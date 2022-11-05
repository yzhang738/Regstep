/// <reference path="../tinymce/jquery.tinymce.min.js" />
/// <reference path="../jquery-ui-vsdoc.js" />
/// <reference path="../../Views/FormBuilder/EditCustomText.cshtml" />
/// <reference path="Common.js" />
/// <reference path="../jquery.wysiwyg.js" />


//////////////////////
// Global Variables //
//////////////////////

var saved = true;
var currentName = "";
var editingValue = "";
var deleteClick = false;
var timer = null;

function SaveForm() {
    $.ajax({
        type: "POST",
        url: "../../EditCustomTextAjax",
        data: $("#form").serialize(),
        success: function (data) {
            if (data.saved) {
                console.log(data);
                IsSaved();
            } else {
                alert('There was an error in the asynchronous call to save your function.');
                NotSaved();
            }
        },
        error: function () {
            NotSaved();
        }
    });
}

function NotSaved() {
    if ($('#FormSave').text() == "Saved") {
        $('#FormSave').text('UnSaved');
        if (!$('#FormSave').hasClass('UnSavedForm')) {
            $('#FormSave').addClass('UnSavedForm');
        }
        if ($('#FormSave').hasClass('SavedForm')) {
            $('#FormSave').removeClass('SavedForm');
        }
    }
    saved = false;
}

function IsSaved() {
    if ($('#FormSave').text() == "UnSaved") {
        $('#FormSave').text('Saved');
        if (!$('#FormSave').hasClass('SavedForm')) {
            $('#FormSave').addClass('SavedForm');
        }
        if ($('#FormSave').hasClass('UnSavedForm')) {
            $('#FormSave').removeClass('UnSavedForm');
        }
    }
    saved = true;
}

$(window).ready(function () {

    setInterval(function () {
        if (!saved) {
            saved = true;
            SaveForm();
        }
    }, 5000);

    ///////////////////////////
    // Disable Enter to Save //
    ///////////////////////////
    $("#form").keypress(function (e) {
        if (e.which == 13) {
            return false;
        }
    });

    ///////////////////////////
    // Add Enter to TextArea //
    ///////////////////////////

    $('#CustomTextEditBox').keyup(function (e) {
        $("#CustomTextPreview").html($(this).val());
    });


    $('#CustomTextEditBox').keypress(function (e) {
        var value = $(this).val()
        if (e.keyCode == 13) {
            $(this).val($(this).val() + "\n");
        }
    });

    //////////////////
    // Click Events //
    //////////////////

    function TextKeyPress () {
        if ($('#Saved').text() == "Saved") {
            $('#Saved').text('UnSaved');
            if (!$('#Saved').hasClass('UnSaved')) {
                $('#Saved').addClass('UnSaved');
            }
            if ($('#Saved').hasClass('SavedBox')) {
                $('#Saved').removeClass('SavedBox');
            }
        }
        if (currentName == "") {
            return;
        }
        if (timer) {
            clearTimeout(timer);
        }
        timer = setTimeout(function () {
            Save();
        }, 2000);
        NotSaved();
    }

    $('.CustomTextLink').on('click', CustomTextClick);
    $('#AddCustomText').on('click', function () { AddCustomText(); });
    $('.DeleteCustomText').on('click', function () { Remove(this) });
    $('#CustomTextEditBox').keypress(function () {
        if ($('#Saved').text() == "Saved") {
            $('#Saved').text('UnSaved');
            if (!$('#Saved').hasClass('UnSaved')) {
                $('#Saved').addClass('UnSaved');
            }
            if ($('#Saved').hasClass('SavedBox')) {
                $('#Saved').removeClass('SavedBox');
            }
        }
                if (currentName == "") {
            return;
        }
        if (timer) {
            clearTimeout(timer);
        }
        timer = setTimeout(function () {
            Save();
        }, 2000);
        NotSaved();
    });
    $('#Save').on('click', function () {
        Save();
    });

    //////////////////////
    // End Click Events //
    //////////////////////

});

function Save() {
    if (currentName == "") {
        return;
    }
    $('#Saved').text('Saved');
    if ($('#Saved').hasClass('UnSaved')) {
        $('#Saved').removeClass('UnSaved');
    }
    if (!$('#Saved').hasClass('SavedBox')) {
        $('#Saved').addClass('SavedBox');
    }
    $('.KeyValuePair[data-name="' + currentName + '"]').children('input[name$=Value]').val($('#CustomTextEditBox').val());
}

function AddCustomText() {
    var input = '<div><input type="text" class="EditCustomTextName" id="CustomTextEdit" value="New Custom Text" /><span id="NameError" class="NameError"></span></div>';
    $('#CustomTexts').append(input);
    $('.CustomTextLink').off();
    $('.CustomTextLink').on('click', CustomTextClick(this));
    editingValue = '';
    BindCustomTextClick();
}

function ReBindCustomTextLink() {
    $('.CustomTextLink').off();
    $('.CustomTextLink').on('click', CustomTextClick);
    $('.DeleteCustomText').off();
    $('.DeleteCustomText').on('click', Remove);
}

function UnbindAll() {
    $('.CustomTextLink').off();
    $('.DeleteCustomText').off();
    $('#AddCustomText').off();
}

function CustomTextClick(e) {
    var item = e.target;
    if (deleteClick) {
        deleteClick = false;
        return;
    }
    $('#CustomTextEditBox').removeAttr('disabled');
    var name = $(item).attr('data-name');
    if (name == currentName) {
        UnbindAll();
        var name = $(item).attr('data-name');
        var input = '<input type="text" class="EditCustomTextName" id="CustomTextEdit" value="' + name + '" /><span id="NameError" class="NameError"></span>';
        $(item).text('');
        $(item).append(input);
        $(item).removeAttr('data-name');
        $(item).removeAttr('class');
        editingValue = name;
        BindCustomTextClick();
    } else {
        currentName = name;
        $('#CustomTextEditHeader').html('Edit <i>"' + name + '</i>" Custom Text');
        $('#CustomTextEditBox').val($('.KeyValuePair[data-name="' + name + '"]').children('input[name$=Value]').val());
        $("#CustomTextPreview").html($('#CustomTextEditBox').val());
        $('#Saved').text('Saved');
        if ($('#Saved').hasClass('UnSaved')) {
            $('#Saved').removeClass('UnSaved');
        }
        if (!$('#Saved').hasClass('SavedBox')) {
            $('#Saved').addClass('SavedBox');
        }
    }
}

function BindCustomTextClick() {
    $('#CustomTextEdit').off();
    UnbindAll();
    $('#CustomTextEdit').on("finished", function () {
        var value = $(this).val();
        var collision = false;
        $('.CustomTextLink').each(function () {
            if ($(this).attr('data-name') == value) {
                collision = true;
            }
        });
        if (collision) {
            $('#NameError').text('* That custom text name already exists.');
            return;
        }
        var parent = $(this).parent();
        $(this).remove();
        $(parent).html(value + '<span class="DeleteCustomText">X</span>');
        $(parent).attr('data-name', value);
        $(parent).attr('class', 'CustomTextLink');
        $(parent).children('#NameError').remove();
        if (editingValue != "") {
            var parent = $('.KeyValuePair[data-name="' + editingValue + '"]');
            $(parent).children('input[name$=Key]').val(value);
            $(parent).attr('data-name', value);
        } else {
            var info = '<span class="KeyValuePair" data-name="' + value + '"><input type="hidden" name="CustomText[9999].Key" value="' + value + '" /><input type="hidden" name="CustomText[9999].Value" value="" /></span>';
            $('#CustomTextElements').append(info);
            Reorder();
        }
        currentName = value;
        $('#CustomTextEditHeader').html('Edit "<i>' + value + '</i>" Custom Text');
        $('#CustomTextEditBox').val($('.KeyValuePair[data-name="' + value + '"]').children('input[name$=Value]').val());
        $("#CustomTextPreview").html($('#CustomTextEditBox').val());
        $('#Saved').text('Saved');
        if ($('#Saved').hasClass('UnSaved')) {
            $('#Saved').removeClass('UnSaved');
        }
        if (!$('#Saved').hasClass('SavedBox')) {
            $('#Saved').addClass('SavedBox');
        }
        $('#AddCustomText').on('click', function () { AddCustomText(); });
        NotSaved();
        ReBindCustomTextLink();

    });
    $('#CustomTextEdit').on("blur", function () {
        $(this).trigger("finished");
    })
    $('#CustomTextEdit').keyup(function (e) {
        if (e.keyCode == 13) {
            $(this).trigger("finished");
        }
    });
}

function Remove(item) {
    deleteClick = true;
    $('#CustomTextEditBox').attr('disabled', 'disabled');
    var name = $(item).parent().attr('data-name');
    $(item).parent().remove();
    $('.KeyValuePair[data-name="' + name + '"]').remove()
    NotSaved();
    Reorder();
}

function Reorder() {
    $('#CustomTextElements').children('.KeyValuePair').each(function (i) {
        $('input[name$=Key]', this).attr('name', 'CustomText[' + i + '].Key');
        $('input[name$=Value]', this).attr('name', 'CustomText[' + i + '].Value');
    });
    NotSaved();
}