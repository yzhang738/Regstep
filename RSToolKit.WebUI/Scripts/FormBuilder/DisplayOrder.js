$(document).ready(function () {
    $('.order-list').sortable({
        onDrop: function (item, targetContainer, _super) {
            var clonedItem = $('<li/>').css({ height: 0 })
            item.before(clonedItem);
            clonedItem.animate({ 'height': item.height() })

            item.animate(clonedItem.position(), function () {
                clonedItem.detach();
                _super(item);
                $('ol.order-list').each(function (i) {
                    if ($(this).children('li').length < 1) {
                        i--;
                        $(this).parents('.row-container').remove();
                    }
                    var row = i + 1;
                    $(this).children('li').each(function (j) {
                        var order = j + 1;
                        $(this).children('.component-order').val(order);
                    });
                })
            });
        }
    }).disableSelection();

    $('.order-list .glyphicon-trash').parent().on('click', function () {
        removeItem(this);
    });


    $('#addLabel').on('click', function () {
        var list = $('.order-list');
        if (list.has('li[data-item="Label"]').length)
            return;
        var order = list.children('li').length + 1;
        var index = list.children('li').length;
        list.append('<li data-item="Label">Label <a href="#"><span class="glyphicon glyphicon-trash"></span></a><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Item" class="component-item" value="Label" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Class" class="component-class" value="form-component-class" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Order" class="component-order" value="' + order + '" /></li>');
        list.find('li[data-item="Label"] .glyphicon-trash').parent().on('click', function () {
            removeItem(this);
        });
    });
    $('#addDescription').on('click', function () {
        var list = $('.order-list');
        if (list.has('li[data-item="Description"]').length)
            return;
        var order = list.children('li').length + 1;
        var index = list.children('li').length;
        list.append('<li data-item="Description">Description <a href="#"><span class="glyphicon glyphicon-trash"></span></a><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Item" class="component-item" value="Description" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Class" class="component-class" value="form-component-description" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Order" class="component-order" value="' + order + '" /></li>');
        list.find('li[data-item="Description"] .glyphicon-trash').parent().on('click', function () {
            removeItem(this);
        });
    });
    $('#addDate').on('click', function () {
        var list = $('.order-list');
        if (list.has('li[data-item="Date"]').length)
            return;
        var order = list.children('li').length + 1;
        var index = list.children('li').length;
        list.append('<li data-item="Date">Date <a href="#"><span class="glyphicon glyphicon-trash"></span></a><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Item" class="component-item" value="Date" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Class" class="component-class" value="form-component-date" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Order" class="component-order" value="' + order + '" /></li>');
        list.find('li[data-item="Date"] .glyphicon-trash').parent().on('click', function () {
            removeItem(this);
        });
    });
    $('#addPrice').on('click', function () {
        var list = $('.order-list');
        if (list.has('li[data-item="Price"]').length)
            return;
        var order = list.children('li').length + 1;
        var index = list.children('li').length;
        list.append('<li data-item="Price">Price <a href="#"><span class="glyphicon glyphicon-trash"></span></a><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Item" class="component-item" value="Price" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Class" class="component-class" value="form-component-description" /><input type="hidden" name="DisplayComponentOrder.Items[' + index + '].Order" class="component-order" value="' + order + '" /></li>');
        list.find('li[data-item="Price"] .glyphicon-trash').parent().on('click', function () {
            removeItem(this);
        });
    });
});

function removeItem(t)
{
    var item = $(t).parent();
    item.remove();
    $('.order-list').children('li').each(function (i) {
        var order = i + 1;
        $(this).children('.component-order').val(order);
        $(this).children('.component-order').attr('name', 'DisplayComponentOrde.Itemsr[' + i + '].Order');
        $(this).children('.component-item').attr('name', 'DisplayComponentOrder.Items[' + i + '].Item');
        $(this).children('.component-class').attr('name', 'DisplayComponentOrder.Items[' + i + '].Class');
    });
}