/// <reference path="../ckeditor/ckeditor.js" />
/// <reference path="../jquery-ui-1.10.3.min.js" />
/// <reference path="../ckeditor/adapters/jquery.js" />
var currentItem;




$(window).load(function () {
    $('#HFBox').ckeditor({
        extraPlugins: 'imagepicker',
        toolbar: [
            ['Source', 'imagepicker'],
            ['Font', 'TextColor', 'BGColor'],
            ['Bold', 'Italic', 'Underline', 'Subscript', 'Superscript', 'JustifyLeft', 'JustifyCenter', 'JustifyRight']
        ],
        uiColor: 'grey',
        enterMode: CKEDITOR.ENTER_BR
    });
});

function CKPickImage(href) {
    var insert = '<img src="' + href + '" />';
    CKEDITOR.instances['HFBox'].insertHtml(insert);
    $('#imagePick').dialog('close');
}