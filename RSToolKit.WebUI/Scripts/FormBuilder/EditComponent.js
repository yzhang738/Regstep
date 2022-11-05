$(document).ready(function () {
    $('.datetimepicker').datetimepicker();

    $('table.items-sortable').sortable({
        containerSelector: 'table',
        itemPath: '> tbody',
        itemSelector: 'tr',
        placeholder: '<tr class="placeholder"/>',
        handle: 'span.icon-move',
        onDrop: function (item, targetContainer, _super) {
            var clonedItem = $('<li/>').css({ height: 0 })
            item.before(clonedItem);
            clonedItem.animate({ 'height': item.height() })

            item.animate(clonedItem.position(), function () {
                clonedItem.detach();
                _super(item);
                $('table.items-sortable tr').each(function (i) {
                    var order = i + 1;
                    $(this).find('.item-order').val(order);
                });
            });
        }
    }).disableSelection();
});