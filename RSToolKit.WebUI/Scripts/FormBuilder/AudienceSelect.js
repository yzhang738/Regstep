$(document).ready(function () {

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