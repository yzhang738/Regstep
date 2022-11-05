jQuery.fn.extend({
    disableSelection: function () {
        return this.each(function () {
            this.onselectstart = function () { return false; };
            this.unselectable = "on";
            jQuery(this).addClass('unselectable');
        });
    }
});