$(document).ready(function () {

    var currentComponent = '';

    $('#btn_m_moveComponent').on('click', function (e) {
        e.preventDefault();
        $(this).closest('modal').modal('hide');
        processing.showPleaseWait();
        var xhr = new XMLHttpRequest();
        xhr.open('put', '../../FormBuilder/MoveComponent');
        xhr.onerror = function (event) { RESTFUL.showError(); };
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (event.currentTarget.status == 200) {
                var result = JSON.parse(c_xhr.responseText);
                if (result.Success) {
                    window.location.reload();
                } else {
                    RESTFUL.showError();
                }
            } else {
                RESTFUL.showError();
            }
        };
        RESTFUL.jsonHeader(xhr);
        var data = {
            id: currentComponent,
            panelKey: $('#panel_m_moveComponent').val()
        };
        $(this).closest('.modal').modal('hide');
        xhr.send(JSON.stringify(data));
    });

    $('.moveComponent').on('click', function (e) {
        e.preventDefault();
        currentComponent = $(this).attr('data-id');
        $('#m_moveComponent').modal('show');
    });

    $('ol.component-sortable').sortable({
        handle: 'span.icon-move',
        group: 'component-sortable',
        onDrop: function (item, targetContainer, _super) {
            var clonedItem = $('<li/>').css({ height: 0 })
            item.before(clonedItem);
            clonedItem.animate({ 'height': item.height() })

            item.animate(clonedItem.position(), function () {
                clonedItem.detach();
                _super(item);
                $('ol.component-sortable').each(function (i) {
                    if ($(this).children('li').length < 1) {
                        i--;
                        $(this).parents('.row-container').remove();
                    }
                    var row = i + 1;
                    $(this).children('li').each(function (j) {
                        var order = j + 1;
                        $(this).find('.component-row').val(row);
                        $(this).find('.component-order').val(order);
                    });
                })
            });
        }
    }).disableSelection();

    $('#addRow').on('click', function () {
        var row = $('ol.component-sortable').length + 1;
        var rowTitle = "Row " + row;
        $('#components').append('<div class="row row-container"><div class="col-sm-12"><span class="row-number">' + rowTitle + '</span></div><div class="col-sm-12"><ol class="component-sortable component-row-' + row + '"></ol></div></div>');
        $('ol.component-sortable').sortable('destroy');
        $('ol.component-sortable').sortable({
            handle: 'span.icon-move',
            group: 'component-sortable',
            onDrop: function (item, targetContainer, _super) {
                var clonedItem = $('<li/>').css({ height: 0 })
                item.before(clonedItem);
                clonedItem.animate({ 'height': item.height() })

                item.animate(clonedItem.position(), function () {
                    clonedItem.detach();
                    _super(item);
                    $('ol.component-sortable').each(function (i) {
                        if ($(this).children('li').length < 1) {
                            i--;
                            $(this).parents('.row-container').remove();
                        }
                        var row = i + 1;
                        $(this).children('li').each(function (j) {
                            var order = j + 1;
                            $(this).find('.component-row').val(row);
                            $(this).find('.component-order').val(order);
                        });
                    })
                });
            }
        }).disableSelection();
    });

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