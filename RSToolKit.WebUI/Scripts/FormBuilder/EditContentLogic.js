$(document).ready(function () {
    $('ol.logic-list').sortable({
        onDrop: function (item, targetContainer, _super) {
            var clonedItem = $('<li/>').css({ height: 0 })
            item.before(clonedItem)
            clonedItem.animate({ 'height': item.height() })

            item.animate(clonedItem.position(), function () {
                clonedItem.detach()
                _super(item)
                $('.logic-list li').each(function (i) {
                    var order = i + 1;
                    var input = $('#Logics_' + i + '__Order');
                    input.val(order);
                    $(this).find('.list-order').html(order + '.');
                });
            });

        }
    }).disableSelection();
});