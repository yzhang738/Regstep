var toolkit = (function (_my) {
    _my.jsonAntiForgeryToken = document.getElementById('toolkitGlobal_jsonAntiForgeryToken').value;
    _my.addJsonAntiForgeryToken = function (data) {
        /// <signature>
        /// <summary>Adds the antiforgery token to an object.</summary>
        /// <param name="data" type="object">The object (or FormData) to append the antiforgery token to.</param>
        /// <returns type="object">Returns the object passed.</returns>
        /// </signature>
        /// <signature>
        /// <summary>Adds the antiforgery token to a FormData object.</summary>
        /// <param name="data" type="FormData">The FormData object to append the antiforgery token to.</param>
        /// <returns type="FormData">Returns the FormData object passed.</returns>
        /// </signature>
        var _jsonAntiForgeryToken = _my.jsonAntiForgeryToken;
        if (EXT.getName(data) === 'FormData') {
            data.append('__RequestVerificationToken', _jsonAntiForgeryToken);
        } else {
            data.__RequestVerificationToken = _jsonAntiForgeryToken;
        }
        return data;
    };
    _my.companyId = document.getElementById('toolkitGlobal_companyId').value;
    return _my;
}(toolkit || {}));