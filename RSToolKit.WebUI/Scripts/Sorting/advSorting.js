/// <reference path="../versioning.js" />

//! Advance Sorting
//! v 1.1.0.0

var advSorting = {
    Version: new Version(1, 1, 0, 0),
    Description: 'Allows advance sorting behaviors',
    SortFunction: function (toSort) {
        /// <signature>
        /// <summary>Tries to determine if a natural sort or a hexidecimal sort should be used. Returns the sorting function to use.</summary>
        /// <returns type="Function" />
        /// <param name="toSort" type="Array" elementType="String">An array of strings to sort.</param>
        /// <param name="asc" type="Boolean">True if sorting in ascending order.</param>
        /// </signature>
        if (!Array.isArray(toSort) || toSort.length < 2) {
            return advSorting.NaturalSort;
        }
        var sampleSize = Math.floor(toSort.length / 5);
        if (sampleSize < 2) {
            sampleSize = 2;
        }
        if (sampleSize > 50) {
            sampleSize = 50;
        }
        var hexSort = null;
        for (var t_sI = 0; t_sI < sampleSize; t_sI++) {
            var t_sampleIndex = Math.floor(Math.random() * toSort.length);
            var sample = toSort[t_sampleIndex];
            if (typeof (sample) !== 'string') {
                continue;
            }
            if (hexSort === null) {
                hexSort = /^[a-fA-F0-9]*$/.test(sample);
            } else {
                hexSort = hexSort && /^[a-fA-F0-9]*$/.test(sample);
            }
        }
        if (hexSort) {
            return advSorting.HexSort;
        }
        return advSorting.NaturalSort;
    },
    SmartSort: function (toSort, asc) {
        /// <signature>
        /// <summary>Tries to determine if a natural sort or a hexidecimal sort should be used.</summary>
        /// <returns type="Array" elementType="String" />
        /// <param name="toSort" type="Array" elementType="String">An array of strings to sort.</param>
        /// <param name="asc" type="Boolean">True if sorting in ascending order.</param>
        /// </signature>
        if (!Array.isArray(toSort) || toSort.length < 2) {
            return [];
        }
        if (typeof (asc) !== 'boolean') {
            asc = true;
        }
        var sampleSize = Math.floor(toSort.length / 5);
        if (sampleSize < 2) {
            sampleSize = 2;
        }
        if (sampleSize > 50) {
            sampleSize = 50;
        }
        var hexSort = null;
        for (var t_sI = 0; t_sI < sampleSize; t_sI++) {
            var t_sampleIndex = Math.floor(Math.random() * toSort.length);
            var sample = toSort[t_sampleIndex];
            if (typeof (sample) !== 'string') {
                continue;
            }
            if (hexSort === null) {
                hexSort = /^[a-fA-F0-9]*$/.test(sample);
            } else {
                hexSort = hexSort && /^[a-fA-F0-9]*$/.test(sample);
            }
        }
        if (hexSort) {
            toSort.sort(function (a, b) { return advSorting.HexSort(a, b, asc); });
        } else {
            toSort.sort(function (a, b) { return advSorting.NaturalSort(a, b, asc); });
        }
        return toSort;
    },
    HexSort: function (a, b, asc) {
        /// <signature>
        /// <returns type="Number" />
        /// <summary>Compares various types of objects against another and sorts them as hexidecimals.</summary>
        /// <param name="a" type="Object">The first object to compare.</param>
        /// <param name="b" type="Object">The second object to compare.</param>
        /// <param name="asc" type="Boolean">True if the order is ascending.</param>
        /// </signature>
        if (typeof (asc) !== 'boolean') {
            asc = true;
        }
        if (typeof (a) !== typeof (b)) {
            return -1;
        }
        if ((typeof (a) === 'undefined' || a === null) && (typeof (b) !== 'undefined' || b !== null)) {
            // If a is null and b is not, we put a after b.
            return 1;
        }
        if ((typeof (b) === 'undefined' || b === null) && (typeof (a) !== 'undefined' || a !== null)) {
            // If b is null and a is not, then we put a before b.
            return -1;
        }
        if ((typeof (a) === 'undefined' || a === null) && (typeof (b) === 'undefined' || b === null)) {
            // If a and b are null, we say they are equal.
            return 0;
        }
        if (typeof (a) === 'string') {
            // If a and b are not the same length then we just return the difference;
            var aNumb = parseInt(a, 16);
            var bNumb = parseInt(b, 16);
            if (isNaN(aNumb) && !isNaN(bNumb)) {
                return 1;
            }
            else if (isNaN(bNumb) && !isNaN(aNumb)) {
                return -1;
            }
            else if (isNaN(aNumb) && isNaN(bNumb)) {
                return 0;
            }
            return aNumb - bNumb;
        }
        return a - b;
    },
    NaturalSort: function (a, b, asc) {
        /// <signature>
        /// <returns type="Number" />
        /// <summary>Compares various types of objects against another.</summary>
        /// <param name="a" type="Object">The first object to compare.</param>
        /// <param name="b" type="Object">The second object to compare.</param>
        /// <param name="asc" type="Boolean">True if the order is ascending.</param>
        /// </signature>
        if (typeof (asc) !== 'boolean') {
            asc = true;
        }
        if ((typeof (a) === 'undefined' || a === null || a === '') && (typeof (b) !== 'undefined' || b !== null || b === '')) {
            // If a is null and b is not, we put a after b.
            return 1;
        }
        if ((typeof (b) === 'undefined' || b === null || b === '') && (typeof (a) !== 'undefined' || a !== null || a === '')) {
            // If b is null and a is not, then we put a before b.
            return -1;
        }
        if ((typeof (a) === 'undefined' || a === null || a === '') && (typeof (b) === 'undefined' || b === null || b === '')) {
            // If a and b are null, we say they are equal.
            return 1;
        }
        if (typeof (a) !== typeof (b)) {
            return -1;
        }
        if (typeof (a) === 'number' && typeof (b) === 'number') {
            return asc ? a - b : b - a;
        }
        if (typeof (a) === 'string') {
            // Here we need to apply a natural sorting algorithm.
            if (!isNaN(a) && !isNaN(b)) {
                var n_a = parseFloat(a);
                var n_b = parseFloat(b);
                return (asc ? n_a - n_b : n_b - n_a);
            }
            a = a.toLowerCase();
            b = b.toLowerCase();
            var aMarker = 0;
            var bMarker = 0;
            var aLength = a.length;
            var bLength = b.length;
            while (aMarker < aLength && bMarker < b.length) {
                var aChunk = this.getChunk(a, aLength, aMarker);
                aMarker += aChunk.length;
                var bChunk = this.getChunk(b, bLength, bMarker);
                bMarker += bChunk.length;
                var result = 0;
                if (this.isDigit(aChunk) && this.isDigit(bChunk)) {
                    result = aChunk.length - bChunk.length;
                    if (result === 0) {
                        for (var t_i = 0; t_i < aChunk.length; t_i++) {
                            result = aChunk.charCodeAt(t_i) - bChunk.charCodeAt(t_i);
                            if (result !== 0) {
                                return (asc ? result : result * -1);
                            }
                        }
                    }
                } else {
                    result = aChunk.localeCompare(bChunk);
                }
                if (result !== 0) {
                    return (asc ? result : result * -1);
                }
            }
            return (asc ? aLength - bLength : bLength - aLength);
        }
        return (asc ? a.length - b.length : a.length - b.length);
    },
    getChunk: function (str, lngth, mrkr) {
        /// <signature>
        /// <returns type="String" />
        /// <summary>Gets the first consecutive set of characters or numbers.</summary>
        /// <param name="str" type="String">The string to get a chunk from.</param>
        /// <param name="lngth" type="Number">The length of the string.</param>
        /// <param name="mrkr" type="Number">The marker area.</param>
        /// </signature>
        var chunk = '';
        var chr = str.charAt(mrkr);
        chunk += chr;
        mrkr++;
        if (this.isDigit(chr)) {
            while (mrkr < lngth) {
                chr = str.charAt(mrkr);
                if (!this.isDigit(chr)) {
                    break;
                }
                chunk += chr;
                mrkr++;
            }
        } else {
            while (mrkr < lngth) {
                chr = str.charAt(mrkr);
                if (this.isDigit(chr)) {
                    break;
                }
                chunk += chr;
                mrkr++;
            }
        }
        return chunk;
    },
    isDigit: function (ch) {
        /// <signature>
        /// <returns type="Boolean" />
        /// <summary>Gets the character value of a character.</summary>
        /// <param name="ch" type="String">A single character in a string.</param>
        /// </signature>

        if (typeof (ch) === "number") {
            return true;
        }
        if (typeof (ch) !== "string") {
            return false;
        }
        if (ch.length < 1) {
            return false;
        }
        return ch.charCodeAt(0) >= 48 && ch.charCodeAt(0) <= 57;
    }
};