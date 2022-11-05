$(document).ready(function () {
    var fixHelperModified = function (e, tr) {
        var $originals = tr.children();
        var $helper = tr.clone();
        $helper.children().each(function (index) {
            $(this).width($originals.eq(index).width())
        });
        return $helper;
    };

    $("#ItemList tbody").sortable({
        helper: fixHelperModified,
        stop: function (e, ui) {
            $('.Order').each(function (i) {
                $('input', this).val((i + 1));
            });
        }
    })
});