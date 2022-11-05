$( document ).ready(function () {
    $('.panel-table').sortable({
        containerSelector: 'table',
        itemPath: '> tbody',
        itemSelector: 'tr',
        placeholder: '<tr class="placeholder"/>',
        handle: 'span.icon-move',
        onDrop: function (item, targetContainer, _super) {
            var clonedItem = $('<tr/>').css({ height: 0 })
            item.before(clonedItem)
            clonedItem.animate({ 'height': item.height() })

            item.animate(clonedItem.position(), function () {
                clonedItem.detach()
                _super(item)
                $('.panel-table tr').each(function (i) {
                    var order = i + 1;
                    var input = $(this).find('input.panel-order');
                    $(this).find('.panel-number').html("Panel " + order + ":");
                    input.val(order);
                });
            });
        }
    }).disableSelection();

    $('#addTags').on('click', function () {
        $('.tags-notSelected > .tags > .tag.tag-visible > .tag-input:checked').each(function () {
            $(this).parent().removeClass('tag-visible').addClass('tag-hidden');
            $('.tags-selected > .tags > .tag[data-id="' + $(this).attr('data-id') + '"]').addClass('tag-visible').removeClass('tag-hidden');
            $(this).attr('checked', false);
        });
        CompileSelectedaudiences();
    });
    $('#removeTags').on('click', function () {
        $('.tags-selected > .tags > .tag.tag-visible > .tag-input:checked').each(function () {
            $(this).parent().removeClass('tag-visible').addClass('tag-hidden');
            $('.tags-notSelected > .tags > .tag[data-id="' + $(this).attr('data-id') + '"]').addClass('tag-visible').removeClass('tag-hidden');
            $(this).attr('checked', false);
        });
        CompileSelectedaudiences();
    });

});

function CompileSelectedaudiences() {
    var audienceUIds = [];
    $('.tags-selected > .tags > .tag.tag-visible').each(function () {
        audienceUIds.push($(this).attr('data-id'));
    });
    $('input[name=audienceUIds]').val(JSON.stringify(audienceUIds));
}