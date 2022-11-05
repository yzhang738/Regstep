$(document).ready(function () {
    $('.audience-table').sortable({
        containerSelector: 'table',
        itemPath: '> tbody',
        itemSelector: 'tr',
        placeholder: '<tr class="placeholder"/>',
        onDrop: function (item, targetContainer, _super) {
            var clonedItem = $('<tr/>').css({ height: 0 })
            item.before(clonedItem)
            clonedItem.animate({ 'height': item.height() })

            item.animate(clonedItem.position(), function () {
                clonedItem.detach()
                _super(item)
                $('.audience-table tr').each(function (i) {
                    var order = i + 1;
                    var input = $(this).find('input.audience-order');
                    input.val(order);
                });
            });
        }
    });
});