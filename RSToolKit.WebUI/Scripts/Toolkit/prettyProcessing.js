var processing, prettyProcessing;
prettyProcessing = processing = (function (_my, $) {
    _my.div = null;
    _my.inUse = false;
    _my.showPleaseWait = function (title, message, percent) {
        /// <signature>
        /// <summary>Shows the proccessing modal with the desired settings.</summary>
        /// <param name="title" type="String">The title of the modal.</param>
        /// <param name="message" type="String">The message to show.</param>
        /// <param name="percent" type="Number">The percent completed.</param>
        /// </signature>
        _my.inUse = true;
        if (_my.div === null) {
            _my.div = _generateDiv();
        }
        if (typeof (title) === 'undefined' || title === null) {
            title = 'Processing';
        }
        if (typeof (message) === 'undefined' || message === null) {
            message = '';
        }
        if (isNaN(percent)) {
            percent = 100;
        }
        if (percent < 1) {
            percent *= 100;
        }
        _my.div.find('.details').hide();
        _my.div.find('.progress-bar').css('width', percent + '%');
        _my.div.find('.modal-header>h3').html(title);
        _my.div.find('.pp-message').html(message);
        _my.div.modal('show');
    };
    _my.update = function (title, message, percent, details) {
        /// <signature>
        /// <summary>Updates the progress bar percent.</summary>
        /// <param name="title" type="String">The title of the modal.</param>
        /// <param name="message" type="String">The message to show.</param>
        /// <param name="percent" type="Number">The percent completed.</param>
        /// </signature>
        if (typeof (title) === 'undefined' || title === null) {
            title = 'Processing';
        }
        if (typeof (message) === 'undefined' || message === null) {
            message = '';
        }
        if (typeof (details) === 'undefined' || details === null) {
            details = null;
        }

        if (isNaN(percent)) {
            percent = 100;
        }
        if (percent < 1) {
            percent *= 100;
        }
        _my.div.find('.progress-bar').css('width', percent + '%');
        _my.div.find('.modal-header>h3').html(title);
        _my.div.find('.pp-message').html(message);
        if (details === null) {
            _my.div.find('.details').hide();
        } else {
            _my.div.find('.details').html(details);
            _my.div.find('.details').show();
        }
    };
    _my.hidePleaseWait = function () {
        /// <signature>
        /// <summary>Hides the dialog.</summary>
        /// </signature>
        if (!_my.inUse) {
            return;
        }
        _my.inUse = false;
        _my.div.modal('hide');
        $('body').attr('style', '');
    };
    function _generateDiv() {
        /// <signature>
        /// <returns type="jQuery" />
        /// </signature>
        var t_div = $.parseHTML('<div class="modal fade" id="m_prettyProccessing" data-backdrop="static">' +
                                    '<div class="modal-dialog">' +
                                        '<div class="modal-header">' +
                                            '<h3>Processing...</h3>' +
                                        '</div>' +
                                        '<div class="modal-body">' +
                                            '<div class="progress">' +
                                                '<div class="progress-bar progress-bar-striped active" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 100%"><span class="text-bold pp-message">Processing</span></div>' +
                                            '</div>' +
                                            '<div class="details">' +
                                            '</div>' +
                                        '</div>' +
                                    '</div>' +
                                '</div>');
        t_div = $(t_div);
        $('body').append(t_div);
        return t_div;
    }
    return _my;
}(prettyProcessing || {}, jQuery));
