var EXT = (function (_my) {
    _my.getName = function (obj) {
        /// <signature>
        /// <summary>Returns the name of the type of object.</summar>
        /// <return type="String">
        /// </signature>
        var funcNameRegex = /function (.{1,})\(/;
        var results = (funcNameRegex).exec((obj).constructor.toString());
        return (results && results.length > 1) ? results[1] : "";
    };
    return _my;
}(EXT || {}));
String.prototype.root = function (root) {
    /// <signature>
    /// <summary>Returns the root of the strin up to the supplied root ending.</summar>
    /// <param name="root" type="String">Grabs everything to the left of the first occurence of this string.</param>
    /// <return type="string">
    /// </signature>
    var rootIndex = this.indexOf(root);
    if (rootIndex === -1) {
        return this.trim();
    } else {
        return this.substring(0, rootIndex);
    }
};
Math.roundAdv = function (number, decimals) {
    /// <signature>
    /// <summary>Returns a roounded number with up to the amount of decimals supplied.</summar>
    /// <param name="number" type="Number">The number to round.</param>
    /// <param name="number" type="Number" integer="true">The number to o decimal places to round to.</param>
    /// <return type="Number">
    /// </signature>
    if (isNaN(number)) {
        return 0;
    }
    return +(Math.round(number + "e+" + decimals) + "e-" + decimals);
};
