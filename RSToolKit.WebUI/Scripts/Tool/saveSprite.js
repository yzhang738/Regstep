// This shows the cloud processing modal
function SaveSprite(modal) {
    var mod = $(modal);
    $(mod).modal('show');
    mod.find('.saved-sprite').removeClass('saved-sprite');
}

// This shows the cloud success modal
function SavedSprite(modal) {
    var mod = $(modal);
    mod.find('.save-sprite').addClass('saved-sprite');
    setTimeout(function () {
        mod.modal('hide');
        setTimeout(function () {
            mod.find('.saved-sprite').removeClass('saved-sprite');
        }, 5000);
    }, 800);
}

// This shows the cloud error modal
function SaveError(modal) {
    var mod = $(modal);
    mod.find('.save-sprite').addClass('error-sprite');
    setTimeout(function () {
        mod.modal('hide');
        setTimeout(function () {
            mod.find('.error-sprite').removeClass('error-sprite');
        }, 5000);
    }, 2000);
}
