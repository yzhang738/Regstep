$(document).on('ready', function (e) {
    $('.nitrox').find('.nitrox-action').addClass('nitrox-blur');
    $('.nitrox').find('.nitrox-cover').addClass('nitrox-covering');
    $('.nitrox').on('click', function (e) {
        e.preventDefault();
        var nitrox = $(this);
        var covered = nitrox.find('.nitrox-cover').hasClass('nitrox-covering');
        if (covered)
            e.stopPropagation();
        var cover = nitrox.find('.nitrox-cover');
        cover.animate({ height: '0px' }, 2000, function (e) {
            nitrox.find('.nitrox-action').removeClass('nitrox-blur');
        });
    });
});