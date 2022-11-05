/// <reference path="../Bootstrap/Plugins/formPlugins.js" />

$(document).ready(function () {
    $('[data-type="color"]').formColorPicker();
    $('[data-type="measurement"]').formCssMeasurement();
    $('[data-type="single-border"]').formCssBorder();
    $('[data-type="font"]').formCssFont();
    $('[data-type="font-size"]').formCssFontSize();
    $('[data-type="font-style"]').formCssFontStyle();
    $('[data-type="font-weight"]').formCssFontWeight();
    $('[data-type="background-position"]').formCssBackgroundPosition();
    $('[data-type="background-repeat"]').formCssBackgroundRepeat();
    $('[data-type="text-align"]').formCssTextAlign();
    $('[data-type="background-size"]').formCssBackgroundSize();
});