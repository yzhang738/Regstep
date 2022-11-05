(function ($) {
    $.fn.getTag = function (tag, defaultValue, type) {
        /// <signature>
        /// <summary>Gets the tag value if it exists. If not it returns the default passed value.</summary>
        /// <param name="tag" type="String">The tag to search for.</param>
        /// <param name="defaultValue" type="String">The default value to return if the tag is not present.</param>
        /// <param name="type" type="String">The type of variable you want returned (string by default).</param>
        /// <returns type="String" />
        /// </signature>
        if (typeof (type) === 'undefined' || type === null) {
            type = 'string';
        } else {
            type = type.toLowerCase();
        }
        if (this.length < 1) {
            return null;
        }
        var t_value = $(this[0]).attr(tag);
        if (typeof (t_value) === 'undefined' || t_value === null) {
            return defaultValue;
        } else {
            switch (type) {
                case 'bool':
                    t_value = t_value.toLowerCase() === 'true';
                    break;
                case 'number':
                    if (isNaN(t_value)) {
                        t_value = 0;
                    } else {
                        t_value = parseInt(t_value);
                    }
                    break;
                case 'number?':
                    if (isNaN(t_value)) {
                        t_value = null;
                    } else {
                        t_value = parseInt(t_value);
                    }
                    break;
            }
        }
        return t_value;
    };
    $.fn.hasTag = function (tag) {
        /// <signature>
        /// <summary>Checks if the tag exists.</summary>
        /// <param name="tag" type="String">The tag to search for.</param>
        /// <returns type="Boolean" />
        /// </signature>
        if (this.length != 1)
            return false;
        var tag = $(this[0]).attr(tag);
        return (typeof (tag) !== 'undefined' && tag !== null && tag !== false);
    }
}(jQuery));