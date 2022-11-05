(function ($) {

    $.fn.jsonData = function (suffix) {
        /// <signature>
        /// <summary>Gets all the data that matches data-[suffix]-[name].</summary>
        /// <returns type="String" />
        /// <param name="suffix" type="String">The suffix to use afte rthe data and before the name.</param>
        /// </signature>
        var dom = this.get(0);
        var obj = {};
        var regEx = new RegExp('^data\-' + suffix + '\-(.+)$');
        $.each(dom.attributes, function (index, attr) {
            if (!regEx.test(attr.name)) {
                return;
            }
            var matches = regEx.exec(attr.name);
            obj[matches[1]] = attr.value;
        });
        return obj;
    };

}(jQuery));