///#source 1 1 /Scripts/Sorting/advSorting.js
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
///#source 1 1 /Scripts/linq.js
/// <reference path="Sorting/advSorting.js" />
/// <ref/erence path="versioning.js" />
/// <reference path="Sorting/advSorting.js" />

//! Linq javascript

var linq = {};

linq.Version = new Version(1, 0, 0, 0)

linq.State = {
    Before: 0,
    Running: 1,
    After: 2
};

linq.Types = {
    Undefined: 'undefined',
    Boolean: 'boolean',
    String: 'string',
    Array: 'Array',
    Function: 'function',
};

linq.EnumerableType = {
    Ordered: 'ordered',
    Base: 'base'
}

linq.From = function (array) {
    /// <signature>
    /// <summary> Creates an enumerable object from an array.</summary>
    /// <returns type="Enumerable" />
    /// <param name="array" type="Array" elementType="Object">The array to make the enumeration from.</param>
    /// </signature>
    return new linq.Enumerable(array);
}

//#region Enumerable

linq.Enumerable = function (array) {
    /// <signature>
    /// <summary>An enumerable that holds information.</summary>
    /// <returns type="Enumerable" />
    /// <param name="array" type="Array">The array to hold in the enumeration.</param>
    /// <field name="state" type="String">The state of the enumeration.</field>
    /// <field name="type" type="String">The type of enumerable.</field>
    /// <field name="array" type="Array">The array that the enumerable is representing.</field>
    /// </signature>
    if (!(array instanceof Array)) {
        array = [];
    }
    this.state = linq.State.Before;
    this.type = linq.EnumerableType.Base;
    this.array = array;
};

linq.Enumerable.prototype.count = function () {
    /// <signature>
    /// <summary>Gets the amount of items in the enumerable.</summary>
    /// <returns type="Number" />
    /// </signature>
    return this.array.length;
}

linq.Enumerable.prototype.where = function (expr) {
    /// <signature>
    /// <summary>Filters an enumeration by the passed expression and returns a new enumerable.</summary>
    /// <returns type="Enumerable" />
    /// <param name="expr" type="Function">The function to use to filter the enumeration. The function must return true or false.</param>
    /// </signature>
    this.state = linq.State.Running;
    var enumerator = new linq.Enumerator(this);
    var array = [];
    while (enumerator.moveNext()) {
        if (expr(enumerator.current)) {
            array.push(enumerator.current);
        }
    }
    this.state = linq.State.After;
    return new linq.Enumerable(array);
};

linq.Enumerable.prototype.orderBy = function (expr, sortFunction) {
    /// <signature>
    /// <summary>Orders an enumeration in ascending order by the passed expression and returns a new enumerable.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="expr" type="Function">The function to use to filter the enumeration. The function must return a string or number or moment.</param>
    /// <param name="sortFunction" type="Function">The sorting function.</param>
    /// </signature>
    if (typeof (sortFunction) !== linq.Types.Function) {
        sortFunction = advSorting.NaturalSort;
    }
    this.state = linq.State.Running;
    var ordered = linq.startQuickSort(this, expr, sortFunction, true);
    this.state = linq.State.After;
    return ordered;
};

linq.Enumerable.prototype.orderByDescending = function (expr, sortFunction) {
    /// <signature>
    /// <summary>Orders an enumeration in descending order by the passed expression and returns a new enumerable.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="expr" type="Function">The function to use to filter the enumeration. The function must return a string or number or moment.</param>
    /// <param name="sortFunction" type="Function">The sorting function.</param>
    /// </signature>
    if (typeof (sortFunction) !== linq.Types.Function) {
        sortFunction = advSorting.NaturalSort;
    }
    this.state = linq.State.Running;
    var ordered = linq.startQuickSort(this, expr, sortFunction, false);
    this.state = linq.State.After;
    return ordered;
};

linq.Enumerable.prototype.getEnumerator = function () {
    /// <signature>
    /// <summary>Gets an enumerator that will iterate through the array.</summary>
    /// <returns type="Enumerator" />
    /// </signature>
    return new linq.Enumerator(this);
}

linq.Enumerable.prototype.toArray = function () {
    /// <signature>
    /// <summary>Gets the ordered array object.</summary>
    /// <returns type="Array" />
    /// </signature>
    var _array = [];
    var _enum = new linq.Enumerator(this);
    while (_enum.moveNext()) {
        _array.push(_enum.current);
    }
    return _array;
}
//#endregion

//#region OrderedEnumerable

linq.OrderedEnumerable = function (array) {
    /// <signature>
    /// <summary>An ordered enumerable that holds information.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="array" type="Array">The array to hold in the enumeration.</param>
    /// <field name="state" type="String">The state of the enumeration.</field>
    /// <field name="type" type="String">The type of enumerable.</field>
    /// <field name="array" type="Array">The array that the enumerable is representing.</field>
    /// </signature>
    /*
    if (!(array instanceof Array)) {
        array = [];
    }//*/
    this.state = linq.State.Before;
    this.type = linq.EnumerableType.Ordered;
    this.array = array;
}

linq.OrderedEnumerable.prototype.thenBy = function (expr, sortFunction) {
    /// <signature>
    /// <summary>Orders an enumeration in ascending order by the passed expression and returns a new enumerable.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="expr" type="Function">The function to use to filter the enumeration. The function must return a string or number or moment.</param>
    /// <param name="sortFunction" type="Function">The sorting function.</param>
    /// </signature>
    if (typeof (sortFunction) !== linq.Types.Function) {
        sortFunction = advSorting.NaturalSort;
    }
    this.state = linq.State.Running;
    var ordered = linq.startQuickSort(this, expr, sortFunction, true);
    this.state = linq.State.After;
    return ordered;
};

linq.OrderedEnumerable.prototype.thenByDescending = function (expr, sortFunction) {
    /// <signature>
    /// <summary>Orders an enumeration in descending order by the passed expression and returns a new enumerable.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="expr" type="Function">The function to use to filter the enumeration. The function must return a string or number or moment.</param>
    /// <param name="sortFunction" type="Function">The sorting function.</param>
    /// </signature>
    if (typeof (sortFunction) !== linq.Types.Function) {
        sortFunction = advSorting.NaturalSort;
    }
    this.state = linq.State.Running;
    var ordered = linq.startQuickSort(this, expr, sortFunction, false);
    this.state = linq.State.After;
    return ordered;
};

linq.OrderedEnumerable.prototype.getEnumerator = function () {
    /// <signature>
    /// <summary>Gets an enumerator that will iterate through the array.</summary>
    /// <returns type="Enumerator" />
    /// </signature>
    return new linq.Enumerator(this);
}

linq.OrderedEnumerable.prototype.toArray = function () {
    /// <signature>
    /// <summary>Gets the ordered array object.</summary>
    /// <returns type="Array" />
    /// </signature>
    var _array = [];
    var _enum = new linq.Enumerator(this);
    while (_enum.moveNext()) {
        _array.push(_enum.current);
    }
    return _array;
}

//#endregion

//#region Enumeration

linq.Enumerator = function (obj) {
    /// <signature>
    /// <summary>Enumerates through a Enumerable and gives values.</summary>
    /// <returns type="Enumerator" />
    /// <param name="obj" type="Enumerable">The enumerable that is to be iterated through.</param>
    /// </signature>
    /// <signature>
    /// <summary>Enumerates through a OrderedEnumerable and gives values.</summary>
    /// <returns type="Enumerator" />
    /// <param name="obj" type="OrderedEnumerable">The ordered enumerable that is to be iterated through.</param>
    /// </signature>
    /// <signature>
    /// <summary>Enumerates through an array and gives values.</summary>
    /// <returns type="Enumerator" />
    /// <param name="obj" type="Array">The array that is to be iterated through.</param>
    /// </signature>
    this.index = -1;
    this.array = [];
    if (typeof (obj.type) !== linq.Types.Undefined) {
        if (obj.type === linq.EnumerableType.Base) {
            this.array = obj.array;
        } else if (obj.type === linq.EnumerableType.Ordered) {
            this.array = [];
            for (var o_i = 0; o_i < obj.array.length; o_i++) {
                this.array.push(obj.array[o_i].object);
            }
        }
    } else if (obj instanceof Array) {
        this.array = obj
    }
    this.current = null;
    this.previous = null;
}

linq.Enumerator.prototype.moveNext = function () {
    /// <signature>
    /// <summary>Moves to the next item in the enumeration.</summary>
    /// <returns type="Boolean" />
    /// </signature>
    this.index++;
    this.previous = this.current;
    if (this.index < this.array.length) {
        this.current = this.array[this.index];
        return true;
    } else {
        this.current = null;
        return false;
    }
};

//#endregion

//#region quicksort

linq.startQuickSort = function (enumerable, expr, sortFunction, asc) {
    /// <signature>
    /// <summary>Sorts an Enumerable and returns and OrderedEnumerable.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="enumerable" type="Enumerable">The enumerable to sort.</param>
    /// <param name="expr" type="Function">The function that calls the item to sort by.</param>
    /// <param name="sortFunction" type="Function">The function that sorts the Enumeration. Must take two parameters and return a number.</param>
    /// <param name="asc" type="Boolean">Whether to sort in ascending or descending fashion.</param>
    /// </signature>
    /// <signature>
    /// <summary>Sorts an OrderedEnumerable and returns and OrderedEnumerable.</summary>
    /// <returns type="OrderedEnumerable" />
    /// <param name="enumerable" type="OrderedEnumerable">The ordered enumerable to sort.</param>
    /// <param name="expr" type="Function">The function that calls the item to sort by.</param>
    /// <param name="sortFunction" type="Function">The function that sorts the Enumeration. Must take two parameters and return a number.</param>
    /// <param name="asc" type="Boolean">Whether to sort in ascending or descending fashion.</param>
    /// </signature>
    if (typeof (sortFunction) !== 'function') {
        sortFunction = advSorting.NaturalSort;
    }
    var newArray = [];
    // First thing we do is check if the an enumerable was actually passed.
    if (typeof (enumerable) === linq.Types.Undefined) {
        // An enumerable was not passed so we throw an error.
        throw 'Must pass an enumerable object as the enumerable param.';
    }
    if (enumerable.type === linq.EnumerableType.Base) {
        //*
        // It is a plain enumerable, so we just do a quicksort with the desired expression and sortfunction.
        // First we create a dummy array that holds the old index and the value we are sorting by.
        var sortedArray = linq.quickSort(enumerable.array, expr, sortFunction, asc);
        // Now we reorder the array.
        for (var i = 0; i < sortedArray.length; i++) {
            newArray.push(enumerable.array[sortedArray[i].oldIndex]);
        }
        //*/
    } else if (enumerable.type === linq.EnumerableType.Ordered) {
        //*
        // We have an ordered enumerabel and we only need to sort groups that where previously equal.
        // We need to enumerate throught he ordered array and fine any items that where previously equal.
        var itemsEqual = [];
        if (enumerable.array.length < 2) {
            // If there are only two items they are already ordered. We just set the state to before and return it as sorted.
            enumerable.state = linq.State.Before;
            return enumerable;
        }
        var enumerator = new linq.Enumerator(enumerable.array);
        // We need to advance to second position for this sorting.
        var currentIndex = -1;
        while (enumerator.moveNext()) {
            if (!enumerator.current.equalsPrevious) {
                currentIndex++;
                itemsEqual.push([]);
            }
            itemsEqual[currentIndex].push(enumerator.current.object);
        }
        var itemsEqualSorted = [];
        for (var o_i = 0; o_i < itemsEqual.length; o_i++) {
            itemsEqualSorted.push(linq.quickSort(itemsEqual[o_i], expr, sortFunction, asc));
        }
        // Now we reorder the original array.
        for (var o_rA = 0; o_rA < itemsEqualSorted.length; o_rA++) {
            for (var o_nA = 0; o_nA < itemsEqualSorted[o_rA].length; o_nA++) {
                newArray.push(itemsEqual[o_rA][itemsEqualSorted[o_rA][o_nA].oldIndex]);
            }
        }
        //*/
    }
    // Now we create the array that is used in an ordered enumerable.
    var orderedArray = [];
    if (newArray.length !== 0) {
        orderedArray.push(new linq.sortedObject(newArray[0], false));
        for (var nA_i = 1; nA_i < newArray.length; nA_i++) {
            var t_previous = newArray[nA_i - 1];
            var t_current = newArray[nA_i];
            if (sortFunction(expr(t_current), expr(t_previous), asc) == 0) {
                orderedArray.push(new linq.sortedObject(newArray[nA_i], true));
            } else {
                orderedArray.push(new linq.sortedObject(newArray[nA_i], false));
            }
        }
    }
    return new linq.OrderedEnumerable(orderedArray);
};

linq.quickSort = function (oArray, expr, sortFunction, asc) {
    if (typeof (asc) !== 'boolean') {
        asc = true;
    }
    var array = [];
    for (var i = 0; i < oArray.length; i++) {
        array.push({ oldIndex: i, value: expr(oArray[i]) });
    }
    return array.sort(function (a, b) { return sortFunction(a.value, b.value, asc); });
};

linq.sortedObject = function (obj, equalsPrevious) {
    if (typeof (equalsPrevious) !== linq.Types.Boolean) {
        equalsPrevious = false;
    }
    this.object = obj;
    this.equalsPrevious = equalsPrevious;
};

//#endregion
///#source 1 1 /Scripts/Bootstrap/Plugins/rating.js
/*!
 * @copyright &copy; Kartik Visweswaran, Krajee.com, 2014
 * @version 3.3.0
 *
 * A simple yet powerful JQuery star rating plugin that allows rendering
 * fractional star ratings and supports Right to Left (RTL) input.
 * 
 * For more JQuery plugins visit http://plugins.krajee.com
 * For more Yii related demos visit http://demos.krajee.com
 */
(function ($) {
    var DEFAULT_MIN = 0;
    var DEFAULT_MAX = 5;
    var DEFAULT_STEP = 0.5;

    var isTouchCapable = 'ontouchstart' in window || (window.DocumentTouch && document instanceof window.DocumentTouch);

    var isEmpty = function (value, trim) {
        return typeof value === 'undefined' || value === null || value === undefined || value == []
            || value === '' || trim && $.trim(value) === '';
    };

    var validateAttr = function ($input, vattr, options) {
        var chk = isEmpty($input.data(vattr)) ? $input.attr(vattr) : $input.data(vattr);
        if (chk) {
            return chk;
        }
        return options[vattr];
    };

    var getDecimalPlaces = function (num) {
        var match = ('' + num).match(/(?:\.(\d+))?(?:[eE]([+-]?\d+))?$/);
        if (!match) {
            return 0;
        }
        return Math.max(0, (match[1] ? match[1].length : 0) - (match[2] ? +match[2] : 0));
    };

    var applyPrecision = function (val, precision) {
        return parseFloat(val.toFixed(precision));
    };

    // Rating public class definition
    var Rating = function (element, options) {
        this.$element = $(element);
        this.init(options);
    };

    Rating.prototype = {
        constructor: Rating,
        _parseAttr: function (vattr, options) {
            var self = this, $input = self.$element;
            if ($input.attr('type') === 'range' || $input.attr('type') === 'number') {
                var val = validateAttr($input, vattr, options);
                var chk = DEFAULT_STEP;
                if (vattr === 'min') {
                    chk = DEFAULT_MIN;
                }
                else if (vattr === 'max') {
                    chk = DEFAULT_MAX;
                }
                else if (vattr === 'step') {
                    chk = DEFAULT_STEP;
                }
                var final = isEmpty(val) ? chk : val;
                return parseFloat(final);
            }
            return parseFloat(options[vattr]);
        },
        /**
         * Listens to events
         */
        listen: function () {
            var self = this;
            self.initTouch();
            self.$rating.on("click", function (e) {
                if (self.inactive) {
                    return;
                }
                var w = e.pageX - self.$rating.offset().left;
                self.setStars(w);
                self.$element.trigger('change');
                self.$element.trigger('rating.change', [self.$element.val(), self.$caption.html()]);
                self.starClicked = true;
            });
            self.$rating.on("mousemove", function (e) {
                if (!self.hoverEnabled || self.inactive) {
                    return;
                }
                self.starClicked = false;
                var pos = e.pageX - self.$rating.offset().left, out = self.calculate(pos);
                self.toggleHover(out);
                self.$element.trigger('rating.hover', [out.val, out.caption, 'stars']);
            });
            self.$rating.on("mouseleave", function (e) {
                if (!self.hoverEnabled || self.inactive || self.starClicked) {
                    return;
                }
                var out = self.cache;
                self.toggleHover(out);
                self.$element.trigger('rating.hoverleave', ['stars']);
            });
            self.$clear.on("mousemove", function (e) {
                if (!self.hoverEnabled || self.inactive || !self.hoverOnClear) {
                    return;
                }
                self.clearClicked = false;
                var caption = '<span class="' + self.clearCaptionClass + '">' + self.clearCaption + '</span>',
                    val = self.clearValue, width = self.getWidthFromValue(val), out;
                out = { caption: caption, width: width, val: val };
                self.toggleHover(out);
                self.$element.trigger('rating.hover', [val, caption, 'clear']);
            });
            self.$clear.on("mouseleave", function (e) {
                if (!self.hoverEnabled || self.inactive || self.clearClicked || !self.hoverOnClear) {
                    return;
                }
                var out = self.cache;
                self.toggleHover(out);
                self.$element.trigger('rating.hoverleave', ['clear']);
            });
            self.$clear.on("click", function (e) {
                if (!self.inactive) {
                    self.clear();
                    self.clearClicked = true;
                }
            });
            $(self.$element[0].form).on("reset", function (e) {
                if (!self.inactive) {
                    self.reset();
                }
            });
        },
        destroy: function () {
            var self = this, $el = self.$element;
            if (!isEmpty(self.$container)) {
                self.$container.before($el).remove();
            }
            $.removeData($el.get(0));
            $el.off('rating').removeClass('hide');
        },
        create: function () {
            var options = arguments.length > 0 ? arguments[0] : {},
                $el = self.$element;
            self.destroy();
            $el.rating(options);
        },
        setTouch: function (e, update) {
            var self = this;
            if (!isTouchCapable || self.inactive) {
                return;
            }
            var ev = e.originalEvent,
                touches = ev.touches.length > 0 ? ev.touches : ev.changedTouches,
                pos = touches[0].pageX - self.$rating.offset().left;
            if (update === true) {
                self.setStars(pos);
                self.$element.trigger('change');
                self.$element.trigger('rating.change', [self.$element.val(), self.$caption.html()]);
                self.starClicked = true;
            } else {
                var out = self.calculate(pos), caption = out.val <= self.clearValue ? self.fetchCaption(self.clearValue) : out.caption,
                    w = self.getWidthFromValue(self.clearValue),
                    width = out.val <= self.clearValue ? (self.rtl ? (100 - w) + '%' : w + '%') : out.width;
                self.$caption.html(caption);
                self.$stars.css('width', width);
            }
        },
        initTouch: function () {
            var self = this;
            self.$rating.on("touchstart", function (e) {
                self.setTouch(e, false);
            });
            self.$rating.on("touchmove", function (e) {
                self.setTouch(e, false);
            });
            self.$rating.on("touchend", function (e) {
                self.setTouch(e, true);
            });
        },
        initSlider: function (options) {
            var self = this;
            if (isEmpty(self.$element.val())) {
                self.$element.val(0);
            }
            self.initialValue = self.$element.val();
            self.min = (typeof options.min !== 'undefined') ? options.min : self._parseAttr('min', options);
            self.max = (typeof options.max !== 'undefined') ? options.max : self._parseAttr('max', options);
            self.step = (typeof options.step !== 'undefined') ? options.step : self._parseAttr('step', options);
            if (isNaN(self.min) || isEmpty(self.min)) {
                self.min = DEFAULT_MIN;
            }
            if (isNaN(self.max) || isEmpty(self.max)) {
                self.max = DEFAULT_MAX;
            }
            if (isNaN(self.step) || isEmpty(self.step) || self.step == 0) {
                self.step = DEFAULT_STEP;
            }
            self.diff = self.max - self.min;
        },
        /**
         * Initializes the plugin
         */
        init: function (options) {
            var self = this;
            self.options = options;
            self.hoverEnabled = options.hoverEnabled;
            self.hoverChangeCaption = options.hoverChangeCaption;
            self.hoverChangeStars = options.hoverChangeStars;
            self.hoverOnClear = options.hoverOnClear;
            self.starClicked = false;
            self.clearClicked = false;
            self.initSlider(options);
            self.checkDisabled();
            $element = self.$element;
            self.containerClass = options.containerClass;
            self.glyphicon = options.glyphicon;
            var defaultStar = (self.glyphicon) ? '\ue006' : '\u2605';
            self.symbol = isEmpty(options.symbol) ? defaultStar : options.symbol;
            self.rtl = options.rtl || self.$element.attr('dir');
            if (self.rtl) {
                self.$element.attr('dir', 'rtl');
            }
            self.showClear = options.showClear;
            self.showCaption = options.showCaption;
            self.size = options.size;
            self.stars = options.stars;
            self.defaultCaption = options.defaultCaption;
            self.starCaptions = options.starCaptions;
            self.starCaptionClasses = options.starCaptionClasses;
            self.clearButton = options.clearButton;
            self.clearButtonTitle = options.clearButtonTitle;
            self.clearButtonBaseClass = !isEmpty(options.clearButtonBaseClass) ? options.clearButtonBaseClass : 'clear-rating';
            self.clearButtonActiveClass = !isEmpty(options.clearButtonActiveClass) ? options.clearButtonActiveClass : 'clear-rating-active';
            self.clearCaption = options.clearCaption;
            self.clearCaptionClass = options.clearCaptionClass;
            self.clearValue = isEmpty(options.clearValue) ? self.min : options.clearValue;
            self.$element.removeClass('form-control').addClass('form-control');
            self.$clearElement = isEmpty(options.clearElement) ? null : $(options.clearElement);
            self.$captionElement = isEmpty(options.captionElement) ? null : $(options.captionElement);
            if (typeof self.$rating == 'undefined' && typeof self.$container == 'undefined') {
                self.$rating = $(document.createElement("div")).html('<div class="rating-stars"></div>');
                self.$container = $(document.createElement("div"));
                self.$container.before(self.$rating);
                self.$container.append(self.$rating);
                self.$element.before(self.$container).appendTo(self.$rating);
            }
            self.$stars = self.$rating.find('.rating-stars');
            self.generateRating();
            self.$clear = !isEmpty(self.$clearElement) ? self.$clearElement : self.$container.find('.' + self.clearButtonBaseClass);
            self.$caption = !isEmpty(self.$captionElement) ? self.$captionElement : self.$container.find(".caption");
            self.setStars();
            self.$element.removeClass('hide').addClass('hide');
            self.listen();
            if (self.showClear) {
                self.$clear.attr({ "class": self.getClearClass() });
            }
            self.cache = {
                caption: self.$caption.html(),
                width: self.$stars.width(),
                val: self.$element.val()
            };
            self.$element.removeClass('rating-loading');
        },
        checkDisabled: function () {
            var self = this;
            self.disabled = validateAttr(self.$element, 'disabled', self.options);
            self.readonly = validateAttr(self.$element, 'readonly', self.options);
            self.inactive = (self.disabled || self.readonly);
        },
        getClearClass: function () {
            return this.clearButtonBaseClass + ' ' + ((this.inactive) ? '' : this.clearButtonActiveClass);
        },
        generateRating: function () {
            var self = this, clear = self.renderClear(), caption = self.renderCaption(),
                css = (self.rtl) ? 'rating-container-rtl' : 'rating-container',
                stars = self.getStars();
            css += (self.glyphicon) ? ((self.symbol == '\ue006') ? ' rating-gly-star' : ' rating-gly') : ' rating-uni';
            self.$rating.attr('class', css);
            self.$rating.attr('data-content', stars);
            self.$stars.attr('data-content', stars);
            var css = self.rtl ? 'star-rating-rtl' : 'star-rating';
            self.$container.attr('class', css + ' rating-' + self.size);

            if (self.inactive) {
                self.$container.removeClass('rating-active').addClass('rating-disabled');
            }
            else {
                self.$container.removeClass('rating-disabled').addClass('rating-active');
            }

            if (typeof self.$caption == 'undefined' && typeof self.$clear == 'undefined') {
                if (self.rtl) {
                    self.$container.prepend(caption).append(clear);
                }
                else {
                    self.$container.prepend(clear).append(caption);
                }
            }
            if (!isEmpty(self.containerClass)) {
                self.$container.removeClass(self.containerClass).addClass(self.containerClass);
            }
        },
        getStars: function () {
            var self = this, numStars = self.stars, stars = '';
            for (var i = 1; i <= numStars; i++) {
                stars += self.symbol;
            }
            return stars;
        },
        renderClear: function () {
            var self = this;
            if (!self.showClear) {
                return '';
            }
            var css = self.getClearClass();
            if (!isEmpty(self.$clearElement)) {
                self.$clearElement.removeClass(css).addClass(css).attr({ "title": self.clearButtonTitle });
                self.$clearElement.html(self.clearButton);
                return '';
            }
            return '<div class="' + css + '" title="' + self.clearButtonTitle + '">' + self.clearButton + '</div>';
        },
        renderCaption: function () {
            var self = this, val = self.$element.val();
            if (!self.showCaption) {
                return '';
            }
            var html = self.fetchCaption(val);
            if (!isEmpty(self.$captionElement)) {
                self.$captionElement.removeClass('caption').addClass('caption').attr({ "title": self.clearCaption });
                self.$captionElement.html(html);
                return '';
            }
            return '<div class="caption">' + html + '</div>';
        },
        fetchCaption: function (rating) {
            var self = this;
            var val = parseFloat(rating), css, cap;
            if (typeof (self.starCaptionClasses) == "function") {
                css = isEmpty(self.starCaptionClasses(val)) ? self.clearCaptionClass : self.starCaptionClasses(val);
            } else {
                css = isEmpty(self.starCaptionClasses[val]) ? self.clearCaptionClass : self.starCaptionClasses[val];
            }
            if (typeof (self.starCaptions) == "function") {
                var cap = isEmpty(self.starCaptions(val)) ? self.defaultCaption.replace(/\{rating\}/g, val) : self.starCaptions(val);
            } else {
                var cap = isEmpty(self.starCaptions[val]) ? self.defaultCaption.replace(/\{rating\}/g, val) : self.starCaptions[val];
            }
            var caption = (val == self.clearValue) ? self.clearCaption : cap;
            return '<span class="' + css + '">' + caption + '</span>';
        },
        getWidthFromValue: function (val) {
            var self = this, min = self.min, max = self.max, step = self.step;
            if (val <= min || min == max) {
                return 0;
            }
            if (val >= max) {
                return 100;
            }
            return (val - min) * 100 / (max - min);
        },
        getValueFromPosition: function (pos) {
            var self = this, precision = getDecimalPlaces(self.step),
                val, factor, maxWidth = self.$rating.width();
            factor = (self.diff * pos) / (maxWidth * self.step);
            factor = self.rtl ? Math.floor(factor) : Math.ceil(factor);
            val = applyPrecision(parseFloat(self.min + factor * self.step), precision);
            val = Math.max(Math.min(val, self.max), self.min);
            return self.rtl ? (self.max - val) : val;
        },
        toggleHover: function (out) {
            var self = this;
            if (self.hoverChangeCaption) {
                var caption = out.val <= self.clearValue ? self.fetchCaption(self.clearValue) : out.caption;
                self.$caption.html(caption);
            }
            if (self.hoverChangeStars) {
                var w = self.getWidthFromValue(self.clearValue),
                    width = out.val <= self.clearValue ? (self.rtl ? (100 - w) + '%' : w + '%') : out.width;
                self.$stars.css('width', width);
            }
        },
        calculate: function (pos) {
            var self = this, defaultVal = isEmpty(self.$element.val()) ? 0 : self.$element.val(),
                val = arguments.length ? self.getValueFromPosition(pos) : defaultVal,
                caption = self.fetchCaption(val), width = self.getWidthFromValue(val);
            if (self.rtl) {
                width = 100 - width;
            }
            width += '%';
            return { caption: caption, width: width, val: val };
        },
        setStars: function (pos) {
            var self = this, out = arguments.length ? self.calculate(pos) : self.calculate();
            self.$element.val(out.val);
            self.$stars.css('width', out.width);
            self.$caption.html(out.caption);
            self.cache = out
        },
        clear: function () {
            var self = this;
            var title = '<span class="' + self.clearCaptionClass + '">' + self.clearCaption + '</span>';
            self.$stars.removeClass('rated');
            if (!self.inactive) {
                self.$caption.html(title);
            }
            self.$element.val(self.clearValue);
            self.setStars();
            self.$element.trigger('rating.clear');
        },
        reset: function () {
            var self = this;
            self.$element.val(self.initialValue);
            self.setStars();
            self.$element.trigger('rating.reset');
        },
        update: function (val) {
            var self = this;
            if (!arguments.length) {
                return;
            }
            self.$element.val(val);
            self.setStars();
        },
        refresh: function (options) {
            var self = this;
            if (!arguments.length) {
                return;
            }
            self.$rating.off('rating');
            self.$clear.off();
            self.init($.extend(self.options, options));
            self.showClear ? self.$clear.show() : self.$clear.hide();
            self.showCaption ? self.$caption.show() : self.$caption.hide();
            self.$element.trigger('rating.refresh');
        }
    };

    //Star rating plugin definition
    $.fn.rating = function (option) {
        var args = Array.apply(null, arguments);
        args.shift();
        return this.each(function () {
            var $this = $(this),
                data = $this.data('rating'),
                options = typeof option === 'object' && option;

            if (!data) {
                $this.data('rating', (data = new Rating(this, $.extend({}, $.fn.rating.defaults, options, $(this).data()))));
            }

            if (typeof option === 'string') {
                data[option].apply(data, args);
            }
        });
    };

    $.fn.rating.defaults = {
        stars: 5,
        glyphicon: true,
        symbol: null,
        disabled: false,
        readonly: false,
        rtl: false,
        size: 'sm',
        showClear: false,
        showCaption: false,
        defaultCaption: '{rating} Stars',
        starCaptions: {
            0.5: 'Half Star',
            1: 'One Star',
            1.5: 'One & Half Star',
            2: 'Two Stars',
            2.5: 'Two & Half Stars',
            3: 'Three Stars',
            3.5: 'Three & Half Stars',
            4: 'Four Stars',
            4.5: 'Four & Half Stars',
            5: 'Five Stars'
        },
        starCaptionClasses: {
            0.5: 'label label-danger',
            1: 'label label-danger',
            1.5: 'label label-warning',
            2: 'label label-warning',
            2.5: 'label label-info',
            3: 'label label-info',
            3.5: 'label label-primary',
            4: 'label label-primary',
            4.5: 'label label-success',
            5: 'label label-success'
        },
        clearButton: '<i class="glyphicon glyphicon-minus-sign"></i>',
        clearButtonTitle: 'Clear',
        clearButtonBaseClass: 'clear-rating',
        clearButtonActiveClass: 'clear-rating-active',
        clearCaption: 'Not Rated',
        clearCaptionClass: 'label label-default',
        clearValue: null,
        captionElement: null,
        clearElement: null,
        containerClass: null,
        hoverEnabled: true,
        hoverChangeCaption: true,
        hoverChangeStars: true,
        hoverOnClear: true
    };


    /**
     * Convert automatically number inputs with class 'rating'
     * into the star rating control.
     */
    $('input.rating').addClass('rating-loading');

    $(document).ready(function () {
        var $input = $('input.rating'), count = Object.keys($input).length;
        if (count > 0) {
            $input.rating();
        }
    });
}(jQuery));
///#source 1 1 /Scripts/Chartv2.js
/*!
 * Chart.js
 * http://chartjs.org/
 * Version: 1.0.1-beta.4
 *
 * Copyright 2014 Nick Downie
 * Released under the MIT license
 * https://github.com/nnnick/Chart.js/blob/master/LICENSE.md
 */


(function () {

    "use strict";

    //Declare root variable - window in the browser, global on the server
    var root = this,
		previous = root.Chart;

    //Occupy the global variable of Chart, and create a simple base class
    var Chart = function (context) {
        var chart = this;
        this.canvas = context.canvas;

        this.ctx = context;

        //Variables global to the chart
        var width = this.width = context.canvas.width;
        var height = this.height = context.canvas.height;
        this.aspectRatio = this.width / this.height;
        //High pixel density displays - multiply the size of the canvas height/width by the device pixel ratio, then scale.
        helpers.retinaScale(this);

        return this;
    };
    //Globally expose the defaults to allow for user updating/changing
    Chart.defaults = {
        global: {
            // Boolean - Whether to animate the chart
            animation: true,

            // Number - Number of animation steps
            animationSteps: 60,

            // String - Animation easing effect
            animationEasing: "easeOutQuart",

            // Boolean - If we should show the scale at all
            showScale: true,

            // Boolean - If we want to override with a hard coded scale
            scaleOverride: false,

            // ** Required if scaleOverride is true **
            // Number - The number of steps in a hard coded scale
            scaleSteps: null,
            // Number - The value jump in the hard coded scale
            scaleStepWidth: null,
            // Number - The scale starting value
            scaleStartValue: null,

            // String - Colour of the scale line
            scaleLineColor: "rgba(0,0,0,.1)",

            // Number - Pixel width of the scale line
            scaleLineWidth: 1,

            // Boolean - Whether to show labels on the scale
            scaleShowLabels: true,

            // Interpolated JS string - can access value
            scaleLabel: "<%=value%>",

            // Boolean - Whether the scale should stick to integers, and not show any floats even if drawing space is there
            scaleIntegersOnly: true,

            // Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
            scaleBeginAtZero: false,

            // String - Scale label font declaration for the scale label
            scaleFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",

            // Number - Scale label font size in pixels
            scaleFontSize: 12,

            // String - Scale label font weight style
            scaleFontStyle: "normal",

            // String - Scale label font colour
            scaleFontColor: "#666",

            // Boolean - whether or not the chart should be responsive and resize when the browser does.
            responsive: false,

            // Boolean - whether to maintain the starting aspect ratio or not when responsive, if set to false, will take up entire container
            maintainAspectRatio: true,

            // Boolean - Determines whether to draw tooltips on the canvas or not - attaches events to touchmove & mousemove
            showTooltips: true,

            // Array - Array of string names to attach tooltip events
            tooltipEvents: ["mousemove", "touchstart", "touchmove", "mouseout"],

            // String - Tooltip background colour
            tooltipFillColor: "rgba(0,0,0,0.8)",

            // String - Tooltip label font declaration for the scale label
            tooltipFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",

            // Number - Tooltip label font size in pixels
            tooltipFontSize: 14,

            // String - Tooltip font weight style
            tooltipFontStyle: "normal",

            // String - Tooltip label font colour
            tooltipFontColor: "#fff",

            // String - Tooltip title font declaration for the scale label
            tooltipTitleFontFamily: "'Helvetica Neue', 'Helvetica', 'Arial', sans-serif",

            // Number - Tooltip title font size in pixels
            tooltipTitleFontSize: 14,

            // String - Tooltip title font weight style
            tooltipTitleFontStyle: "bold",

            // String - Tooltip title font colour
            tooltipTitleFontColor: "#fff",

            // Number - pixel width of padding around tooltip text
            tooltipYPadding: 6,

            // Number - pixel width of padding around tooltip text
            tooltipXPadding: 6,

            // Number - Size of the caret on the tooltip
            tooltipCaretSize: 8,

            // Number - Pixel radius of the tooltip border
            tooltipCornerRadius: 6,

            // Number - Pixel offset from point x to tooltip edge
            tooltipXOffset: 10,

            // String - Template string for single tooltips
            tooltipTemplate: "<%if (label){%><%=label%>: <%}%><%= value %>",

            // String - Template string for single tooltips
            multiTooltipTemplate: "<%= value %>",

            // String - Colour behind the legend colour block
            multiTooltipKeyBackground: '#fff',

            // Function - Will fire on animation progression.
            onAnimationProgress: function () { },

            // Function - Will fire on animation completion.
            onAnimationComplete: function () { }

        }
    };

    //Create a dictionary of chart types, to allow for extension of existing types
    Chart.types = {};

    //Global Chart helpers object for utility methods and classes
    var helpers = Chart.helpers = {};

    //-- Basic js utility methods
    var each = helpers.each = function (loopable, callback, self) {
        var additionalArgs = Array.prototype.slice.call(arguments, 3);
        // Check to see if null or undefined firstly.
        if (loopable) {
            if (loopable.length === +loopable.length) {
                var i;
                for (i = 0; i < loopable.length; i++) {
                    callback.apply(self, [loopable[i], i].concat(additionalArgs));
                }
            }
            else {
                for (var item in loopable) {
                    callback.apply(self, [loopable[item], item].concat(additionalArgs));
                }
            }
        }
    },
		clone = helpers.clone = function (obj) {
		    var objClone = {};
		    each(obj, function (value, key) {
		        if (obj.hasOwnProperty(key)) objClone[key] = value;
		    });
		    return objClone;
		},
		extend = helpers.extend = function (base) {
		    each(Array.prototype.slice.call(arguments, 1), function (extensionObject) {
		        each(extensionObject, function (value, key) {
		            if (extensionObject.hasOwnProperty(key)) base[key] = value;
		        });
		    });
		    return base;
		},
		merge = helpers.merge = function (base, master) {
		    //Merge properties in left object over to a shallow clone of object right.
		    var args = Array.prototype.slice.call(arguments, 0);
		    args.unshift({});
		    return extend.apply(null, args);
		},
		indexOf = helpers.indexOf = function (arrayToSearch, item) {
		    if (Array.prototype.indexOf) {
		        return arrayToSearch.indexOf(item);
		    }
		    else {
		        for (var i = 0; i < arrayToSearch.length; i++) {
		            if (arrayToSearch[i] === item) return i;
		        }
		        return -1;
		    }
		},
		where = helpers.where = function (collection, filterCallback) {
		    var filtered = [];

		    helpers.each(collection, function (item) {
		        if (filterCallback(item)) {
		            filtered.push(item);
		        }
		    });

		    return filtered;
		},
		findNextWhere = helpers.findNextWhere = function (arrayToSearch, filterCallback, startIndex) {
		    // Default to start of the array
		    if (!startIndex) {
		        startIndex = -1;
		    }
		    for (var i = startIndex + 1; i < arrayToSearch.length; i++) {
		        var currentItem = arrayToSearch[i];
		        if (filterCallback(currentItem)) {
		            return currentItem;
		        }
		    };
		},
		findPreviousWhere = helpers.findPreviousWhere = function (arrayToSearch, filterCallback, startIndex) {
		    // Default to end of the array
		    if (!startIndex) {
		        startIndex = arrayToSearch.length;
		    }
		    for (var i = startIndex - 1; i >= 0; i--) {
		        var currentItem = arrayToSearch[i];
		        if (filterCallback(currentItem)) {
		            return currentItem;
		        }
		    };
		},
		inherits = helpers.inherits = function (extensions) {
		    //Basic javascript inheritance based on the model created in Backbone.js
		    var parent = this;
		    var ChartElement = (extensions && extensions.hasOwnProperty("constructor")) ? extensions.constructor : function () { return parent.apply(this, arguments); };

		    var Surrogate = function () { this.constructor = ChartElement; };
		    Surrogate.prototype = parent.prototype;
		    ChartElement.prototype = new Surrogate();

		    ChartElement.extend = inherits;

		    if (extensions) extend(ChartElement.prototype, extensions);

		    ChartElement.__super__ = parent.prototype;

		    return ChartElement;
		},
		noop = helpers.noop = function () { },
		uid = helpers.uid = (function () {
		    var id = 0;
		    return function () {
		        return "chart-" + id++;
		    };
		})(),
		warn = helpers.warn = function (str) {
		    //Method for warning of errors
		    if (window.console && typeof window.console.warn == "function") console.warn(str);
		},
		amd = helpers.amd = (typeof define == 'function' && define.amd),
		//-- Math methods
		isNumber = helpers.isNumber = function (n) {
		    return !isNaN(parseFloat(n)) && isFinite(n);
		},
		max = helpers.max = function (array) {
		    return Math.max.apply(Math, array);
		},
		min = helpers.min = function (array) {
		    return Math.min.apply(Math, array);
		},
		cap = helpers.cap = function (valueToCap, maxValue, minValue) {
		    if (isNumber(maxValue)) {
		        if (valueToCap > maxValue) {
		            return maxValue;
		        }
		    }
		    else if (isNumber(minValue)) {
		        if (valueToCap < minValue) {
		            return minValue;
		        }
		    }
		    return valueToCap;
		},
		getDecimalPlaces = helpers.getDecimalPlaces = function (num) {
		    if (num % 1 !== 0 && isNumber(num)) {
		        return num.toString().split(".")[1].length;
		    }
		    else {
		        return 0;
		    }
		},
		toRadians = helpers.radians = function (degrees) {
		    return degrees * (Math.PI / 180);
		},
		// Gets the angle from vertical upright to the point about a centre.
		getAngleFromPoint = helpers.getAngleFromPoint = function (centrePoint, anglePoint) {
		    var distanceFromXCenter = anglePoint.x - centrePoint.x,
				distanceFromYCenter = anglePoint.y - centrePoint.y,
				radialDistanceFromCenter = Math.sqrt(distanceFromXCenter * distanceFromXCenter + distanceFromYCenter * distanceFromYCenter);


		    var angle = Math.PI * 2 + Math.atan2(distanceFromYCenter, distanceFromXCenter);

		    //If the segment is in the top left quadrant, we need to add another rotation to the angle
		    if (distanceFromXCenter < 0 && distanceFromYCenter < 0) {
		        angle += Math.PI * 2;
		    }

		    return {
		        angle: angle,
		        distance: radialDistanceFromCenter
		    };
		},
		aliasPixel = helpers.aliasPixel = function (pixelWidth) {
		    return (pixelWidth % 2 === 0) ? 0 : 0.5;
		},
		splineCurve = helpers.splineCurve = function (FirstPoint, MiddlePoint, AfterPoint, t) {
		    //Props to Rob Spencer at scaled innovation for his post on splining between points
		    //http://scaledinnovation.com/analytics/splines/aboutSplines.html
		    var d01 = Math.sqrt(Math.pow(MiddlePoint.x - FirstPoint.x, 2) + Math.pow(MiddlePoint.y - FirstPoint.y, 2)),
				d12 = Math.sqrt(Math.pow(AfterPoint.x - MiddlePoint.x, 2) + Math.pow(AfterPoint.y - MiddlePoint.y, 2)),
				fa = t * d01 / (d01 + d12),// scaling factor for triangle Ta
				fb = t * d12 / (d01 + d12);
		    return {
		        inner: {
		            x: MiddlePoint.x - fa * (AfterPoint.x - FirstPoint.x),
		            y: MiddlePoint.y - fa * (AfterPoint.y - FirstPoint.y)
		        },
		        outer: {
		            x: MiddlePoint.x + fb * (AfterPoint.x - FirstPoint.x),
		            y: MiddlePoint.y + fb * (AfterPoint.y - FirstPoint.y)
		        }
		    };
		},
		calculateOrderOfMagnitude = helpers.calculateOrderOfMagnitude = function (val) {
		    return Math.floor(Math.log(val) / Math.LN10);
		},
		calculateScaleRange = helpers.calculateScaleRange = function (valuesArray, drawingSize, textSize, startFromZero, integersOnly) {

		    //Set a minimum step of two - a point at the top of the graph, and a point at the base
		    var minSteps = 2,
				maxSteps = Math.floor(drawingSize / (textSize * 1.5)),
				skipFitting = (minSteps >= maxSteps);

		    var maxValue = max(valuesArray),
				minValue = min(valuesArray);

		    // We need some degree of seperation here to calculate the scales if all the values are the same
		    // Adding/minusing 0.5 will give us a range of 1.
		    if (maxValue === minValue) {
		        maxValue += 0.5;
		        // So we don't end up with a graph with a negative start value if we've said always start from zero
		        if (minValue >= 0.5 && !startFromZero) {
		            minValue -= 0.5;
		        }
		        else {
		            // Make up a whole number above the values
		            maxValue += 0.5;
		        }
		    }

		    var valueRange = Math.abs(maxValue - minValue),
				rangeOrderOfMagnitude = calculateOrderOfMagnitude(valueRange),
				graphMax = Math.ceil(maxValue / (1 * Math.pow(10, rangeOrderOfMagnitude))) * Math.pow(10, rangeOrderOfMagnitude),
				graphMin = (startFromZero) ? 0 : Math.floor(minValue / (1 * Math.pow(10, rangeOrderOfMagnitude))) * Math.pow(10, rangeOrderOfMagnitude),
				graphRange = graphMax - graphMin,
				stepValue = Math.pow(10, rangeOrderOfMagnitude),
				numberOfSteps = Math.round(graphRange / stepValue);

		    //If we have more space on the graph we'll use it to give more definition to the data
		    while ((numberOfSteps > maxSteps || (numberOfSteps * 2) < maxSteps) && !skipFitting) {
		        if (numberOfSteps > maxSteps) {
		            stepValue *= 2;
		            numberOfSteps = Math.round(graphRange / stepValue);
		            // Don't ever deal with a decimal number of steps - cancel fitting and just use the minimum number of steps.
		            if (numberOfSteps % 1 !== 0) {
		                skipFitting = true;
		            }
		        }
		            //We can fit in double the amount of scale points on the scale
		        else {
		            //If user has declared ints only, and the step value isn't a decimal
		            if (integersOnly && rangeOrderOfMagnitude >= 0) {
		                //If the user has said integers only, we need to check that making the scale more granular wouldn't make it a float
		                if (stepValue / 2 % 1 === 0) {
		                    stepValue /= 2;
		                    numberOfSteps = Math.round(graphRange / stepValue);
		                }
		                    //If it would make it a float break out of the loop
		                else {
		                    break;
		                }
		            }
		                //If the scale doesn't have to be an int, make the scale more granular anyway.
		            else {
		                stepValue /= 2;
		                numberOfSteps = Math.round(graphRange / stepValue);
		            }

		        }
		    }

		    if (skipFitting) {
		        numberOfSteps = minSteps;
		        stepValue = graphRange / numberOfSteps;
		    }

		    return {
		        steps: numberOfSteps,
		        stepValue: stepValue,
		        min: graphMin,
		        max: graphMin + (numberOfSteps * stepValue)
		    };

		},
		/* jshint ignore:start */
		// Blows up jshint errors based on the new Function constructor
		//Templating methods
		//Javascript micro templating by John Resig - source at http://ejohn.org/blog/javascript-micro-templating/
		template = helpers.template = function (templateString, valuesObject) {
		    // If templateString is function rather than string-template - call the function for valuesObject
		    if (templateString instanceof Function) {
		        return templateString(valuesObject);
		    }

		    var cache = {};
		    function tmpl(str, data) {
		        // Figure out if we're getting a template, or if we need to
		        // load the template - and be sure to cache the result.
		        var fn = !/\W/.test(str) ?
				cache[str] = cache[str] :

				// Generate a reusable function that will serve as a template
				// generator (and which will be cached).
				new Function("obj",
					"var p=[],print=function(){p.push.apply(p,arguments);};" +

					// Introduce the data as local variables using with(){}
					"with(obj){p.push('" +

					// Convert the template into pure JavaScript
					str
						.replace(/[\r\t\n]/g, " ")
						.split("<%").join("\t")
						.replace(/((^|%>)[^\t]*)'/g, "$1\r")
						.replace(/\t=(.*?)%>/g, "',$1,'")
						.split("\t").join("');")
						.split("%>").join("p.push('")
						.split("\r").join("\\'") +
					"');}return p.join('');"
				);

		        // Provide some basic currying to the user
		        return data ? fn(data) : fn;
		    }
		    return tmpl(templateString, valuesObject);
		},
		/* jshint ignore:end */
		generateLabels = helpers.generateLabels = function (templateString, numberOfSteps, graphMin, stepValue) {
		    var labelsArray = new Array(numberOfSteps);
		    if (labelTemplateString) {
		        each(labelsArray, function (val, index) {
		            labelsArray[index] = template(templateString, { value: (graphMin + (stepValue * (index + 1))) });
		        });
		    }
		    return labelsArray;
		},
		//--Animation methods
		//Easing functions adapted from Robert Penner's easing equations
		//http://www.robertpenner.com/easing/
		easingEffects = helpers.easingEffects = {
		    linear: function (t) {
		        return t;
		    },
		    easeInQuad: function (t) {
		        return t * t;
		    },
		    easeOutQuad: function (t) {
		        return -1 * t * (t - 2);
		    },
		    easeInOutQuad: function (t) {
		        if ((t /= 1 / 2) < 1) return 1 / 2 * t * t;
		        return -1 / 2 * ((--t) * (t - 2) - 1);
		    },
		    easeInCubic: function (t) {
		        return t * t * t;
		    },
		    easeOutCubic: function (t) {
		        return 1 * ((t = t / 1 - 1) * t * t + 1);
		    },
		    easeInOutCubic: function (t) {
		        if ((t /= 1 / 2) < 1) return 1 / 2 * t * t * t;
		        return 1 / 2 * ((t -= 2) * t * t + 2);
		    },
		    easeInQuart: function (t) {
		        return t * t * t * t;
		    },
		    easeOutQuart: function (t) {
		        return -1 * ((t = t / 1 - 1) * t * t * t - 1);
		    },
		    easeInOutQuart: function (t) {
		        if ((t /= 1 / 2) < 1) return 1 / 2 * t * t * t * t;
		        return -1 / 2 * ((t -= 2) * t * t * t - 2);
		    },
		    easeInQuint: function (t) {
		        return 1 * (t /= 1) * t * t * t * t;
		    },
		    easeOutQuint: function (t) {
		        return 1 * ((t = t / 1 - 1) * t * t * t * t + 1);
		    },
		    easeInOutQuint: function (t) {
		        if ((t /= 1 / 2) < 1) return 1 / 2 * t * t * t * t * t;
		        return 1 / 2 * ((t -= 2) * t * t * t * t + 2);
		    },
		    easeInSine: function (t) {
		        return -1 * Math.cos(t / 1 * (Math.PI / 2)) + 1;
		    },
		    easeOutSine: function (t) {
		        return 1 * Math.sin(t / 1 * (Math.PI / 2));
		    },
		    easeInOutSine: function (t) {
		        return -1 / 2 * (Math.cos(Math.PI * t / 1) - 1);
		    },
		    easeInExpo: function (t) {
		        return (t === 0) ? 1 : 1 * Math.pow(2, 10 * (t / 1 - 1));
		    },
		    easeOutExpo: function (t) {
		        return (t === 1) ? 1 : 1 * (-Math.pow(2, -10 * t / 1) + 1);
		    },
		    easeInOutExpo: function (t) {
		        if (t === 0) return 0;
		        if (t === 1) return 1;
		        if ((t /= 1 / 2) < 1) return 1 / 2 * Math.pow(2, 10 * (t - 1));
		        return 1 / 2 * (-Math.pow(2, -10 * --t) + 2);
		    },
		    easeInCirc: function (t) {
		        if (t >= 1) return t;
		        return -1 * (Math.sqrt(1 - (t /= 1) * t) - 1);
		    },
		    easeOutCirc: function (t) {
		        return 1 * Math.sqrt(1 - (t = t / 1 - 1) * t);
		    },
		    easeInOutCirc: function (t) {
		        if ((t /= 1 / 2) < 1) return -1 / 2 * (Math.sqrt(1 - t * t) - 1);
		        return 1 / 2 * (Math.sqrt(1 - (t -= 2) * t) + 1);
		    },
		    easeInElastic: function (t) {
		        var s = 1.70158;
		        var p = 0;
		        var a = 1;
		        if (t === 0) return 0;
		        if ((t /= 1) == 1) return 1;
		        if (!p) p = 1 * 0.3;
		        if (a < Math.abs(1)) {
		            a = 1;
		            s = p / 4;
		        } else s = p / (2 * Math.PI) * Math.asin(1 / a);
		        return -(a * Math.pow(2, 10 * (t -= 1)) * Math.sin((t * 1 - s) * (2 * Math.PI) / p));
		    },
		    easeOutElastic: function (t) {
		        var s = 1.70158;
		        var p = 0;
		        var a = 1;
		        if (t === 0) return 0;
		        if ((t /= 1) == 1) return 1;
		        if (!p) p = 1 * 0.3;
		        if (a < Math.abs(1)) {
		            a = 1;
		            s = p / 4;
		        } else s = p / (2 * Math.PI) * Math.asin(1 / a);
		        return a * Math.pow(2, -10 * t) * Math.sin((t * 1 - s) * (2 * Math.PI) / p) + 1;
		    },
		    easeInOutElastic: function (t) {
		        var s = 1.70158;
		        var p = 0;
		        var a = 1;
		        if (t === 0) return 0;
		        if ((t /= 1 / 2) == 2) return 1;
		        if (!p) p = 1 * (0.3 * 1.5);
		        if (a < Math.abs(1)) {
		            a = 1;
		            s = p / 4;
		        } else s = p / (2 * Math.PI) * Math.asin(1 / a);
		        if (t < 1) return -0.5 * (a * Math.pow(2, 10 * (t -= 1)) * Math.sin((t * 1 - s) * (2 * Math.PI) / p));
		        return a * Math.pow(2, -10 * (t -= 1)) * Math.sin((t * 1 - s) * (2 * Math.PI) / p) * 0.5 + 1;
		    },
		    easeInBack: function (t) {
		        var s = 1.70158;
		        return 1 * (t /= 1) * t * ((s + 1) * t - s);
		    },
		    easeOutBack: function (t) {
		        var s = 1.70158;
		        return 1 * ((t = t / 1 - 1) * t * ((s + 1) * t + s) + 1);
		    },
		    easeInOutBack: function (t) {
		        var s = 1.70158;
		        if ((t /= 1 / 2) < 1) return 1 / 2 * (t * t * (((s *= (1.525)) + 1) * t - s));
		        return 1 / 2 * ((t -= 2) * t * (((s *= (1.525)) + 1) * t + s) + 2);
		    },
		    easeInBounce: function (t) {
		        return 1 - easingEffects.easeOutBounce(1 - t);
		    },
		    easeOutBounce: function (t) {
		        if ((t /= 1) < (1 / 2.75)) {
		            return 1 * (7.5625 * t * t);
		        } else if (t < (2 / 2.75)) {
		            return 1 * (7.5625 * (t -= (1.5 / 2.75)) * t + 0.75);
		        } else if (t < (2.5 / 2.75)) {
		            return 1 * (7.5625 * (t -= (2.25 / 2.75)) * t + 0.9375);
		        } else {
		            return 1 * (7.5625 * (t -= (2.625 / 2.75)) * t + 0.984375);
		        }
		    },
		    easeInOutBounce: function (t) {
		        if (t < 1 / 2) return easingEffects.easeInBounce(t * 2) * 0.5;
		        return easingEffects.easeOutBounce(t * 2 - 1) * 0.5 + 1 * 0.5;
		    }
		},
		//Request animation polyfill - http://www.paulirish.com/2011/requestanimationframe-for-smart-animating/
		requestAnimFrame = helpers.requestAnimFrame = (function () {
		    return window.requestAnimationFrame ||
				window.webkitRequestAnimationFrame ||
				window.mozRequestAnimationFrame ||
				window.oRequestAnimationFrame ||
				window.msRequestAnimationFrame ||
				function (callback) {
				    return window.setTimeout(callback, 1000 / 60);
				};
		})(),
		cancelAnimFrame = helpers.cancelAnimFrame = (function () {
		    return window.cancelAnimationFrame ||
				window.webkitCancelAnimationFrame ||
				window.mozCancelAnimationFrame ||
				window.oCancelAnimationFrame ||
				window.msCancelAnimationFrame ||
				function (callback) {
				    return window.clearTimeout(callback, 1000 / 60);
				};
		})(),
		animationLoop = helpers.animationLoop = function (callback, totalSteps, easingString, onProgress, onComplete, chartInstance) {

		    var currentStep = 0,
				easingFunction = easingEffects[easingString] || easingEffects.linear;

		    var animationFrame = function () {
		        currentStep++;
		        var stepDecimal = currentStep / totalSteps;
		        var easeDecimal = easingFunction(stepDecimal);

		        callback.call(chartInstance, easeDecimal, stepDecimal, currentStep);
		        onProgress.call(chartInstance, easeDecimal, stepDecimal);
		        if (currentStep < totalSteps) {
		            chartInstance.animationFrame = requestAnimFrame(animationFrame);
		        } else {
		            onComplete.apply(chartInstance);
		        }
		    };
		    requestAnimFrame(animationFrame);
		},
		//-- DOM methods
		getRelativePosition = helpers.getRelativePosition = function (evt) {
		    var mouseX, mouseY;
		    var e = evt.originalEvent || evt,
				canvas = evt.currentTarget || evt.srcElement,
				boundingRect = canvas.getBoundingClientRect();

		    if (e.touches) {
		        mouseX = e.touches[0].clientX - boundingRect.left;
		        mouseY = e.touches[0].clientY - boundingRect.top;

		    }
		    else {
		        mouseX = e.clientX - boundingRect.left;
		        mouseY = e.clientY - boundingRect.top;
		    }

		    return {
		        x: mouseX,
		        y: mouseY
		    };

		},
		addEvent = helpers.addEvent = function (node, eventType, method) {
		    if (node.addEventListener) {
		        node.addEventListener(eventType, method);
		    } else if (node.attachEvent) {
		        node.attachEvent("on" + eventType, method);
		    } else {
		        node["on" + eventType] = method;
		    }
		},
		removeEvent = helpers.removeEvent = function (node, eventType, handler) {
		    if (node.removeEventListener) {
		        node.removeEventListener(eventType, handler, false);
		    } else if (node.detachEvent) {
		        node.detachEvent("on" + eventType, handler);
		    } else {
		        node["on" + eventType] = noop;
		    }
		},
		bindEvents = helpers.bindEvents = function (chartInstance, arrayOfEvents, handler) {
		    // Create the events object if it's not already present
		    if (!chartInstance.events) chartInstance.events = {};

		    each(arrayOfEvents, function (eventName) {
		        chartInstance.events[eventName] = function () {
		            handler.apply(chartInstance, arguments);
		        };
		        addEvent(chartInstance.chart.canvas, eventName, chartInstance.events[eventName]);
		    });
		},
		unbindEvents = helpers.unbindEvents = function (chartInstance, arrayOfEvents) {
		    each(arrayOfEvents, function (handler, eventName) {
		        removeEvent(chartInstance.chart.canvas, eventName, handler);
		    });
		},
		getMaximumWidth = helpers.getMaximumWidth = function (domNode) {
		    var container = domNode.parentNode;
		    // TODO = check cross browser stuff with this.
		    return container.clientWidth;
		},
		getMaximumHeight = helpers.getMaximumHeight = function (domNode) {
		    var container = domNode.parentNode;
		    // TODO = check cross browser stuff with this.
		    return container.clientHeight;
		},
		getMaximumSize = helpers.getMaximumSize = helpers.getMaximumWidth, // legacy support
		retinaScale = helpers.retinaScale = function (chart) {
		    var ctx = chart.ctx,
				width = chart.canvas.width,
				height = chart.canvas.height;

		    if (window.devicePixelRatio) {
		        ctx.canvas.style.width = width + "px";
		        ctx.canvas.style.height = height + "px";
		        ctx.canvas.height = height * window.devicePixelRatio;
		        ctx.canvas.width = width * window.devicePixelRatio;
		        ctx.scale(window.devicePixelRatio, window.devicePixelRatio);
		    }
		},
		//-- Canvas methods
		clear = helpers.clear = function (chart) {
		    chart.ctx.clearRect(0, 0, chart.width, chart.height);
		},
		fontString = helpers.fontString = function (pixelSize, fontStyle, fontFamily) {
		    return fontStyle + " " + pixelSize + "px " + fontFamily;
		},
		longestText = helpers.longestText = function (ctx, font, arrayOfStrings) {
		    ctx.font = font;
		    var longest = 0;
		    each(arrayOfStrings, function (string) {
		        var textWidth = ctx.measureText(string).width;
		        longest = (textWidth > longest) ? textWidth : longest;
		    });
		    return longest;
		},
		drawRoundedRectangle = helpers.drawRoundedRectangle = function (ctx, x, y, width, height, radius) {
		    ctx.beginPath();
		    ctx.moveTo(x + radius, y);
		    ctx.lineTo(x + width - radius, y);
		    ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
		    ctx.lineTo(x + width, y + height - radius);
		    ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
		    ctx.lineTo(x + radius, y + height);
		    ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
		    ctx.lineTo(x, y + radius);
		    ctx.quadraticCurveTo(x, y, x + radius, y);
		    ctx.closePath();
		};


    //Store a reference to each instance - allowing us to globally resize chart instances on window resize.
    //Destroy method on the chart will remove the instance of the chart from this reference.
    Chart.instances = {};

    Chart.Type = function (data, options, chart) {
        this.options = options;
        this.chart = chart;
        this.id = uid();
        //Add the chart instance to the global namespace
        Chart.instances[this.id] = this;

        // Initialize is always called when a chart type is created
        // By default it is a no op, but it should be extended
        if (options.responsive) {
            this.resize();
        }
        this.initialize.call(this, data);
    };

    //Core methods that'll be a part of every chart type
    extend(Chart.Type.prototype, {
        initialize: function () { return this; },
        clear: function () {
            clear(this.chart);
            return this;
        },
        stop: function () {
            // Stops any current animation loop occuring
            helpers.cancelAnimFrame.call(root, this.animationFrame);
            return this;
        },
        resize: function (callback) {
            this.stop();
            var canvas = this.chart.canvas,
				newWidth = getMaximumWidth(this.chart.canvas),
				newHeight = this.options.maintainAspectRatio ? newWidth / this.chart.aspectRatio : getMaximumHeight(this.chart.canvas);

            canvas.width = this.chart.width = newWidth;
            canvas.height = this.chart.height = newHeight;

            retinaScale(this.chart);

            if (typeof callback === "function") {
                callback.apply(this, Array.prototype.slice.call(arguments, 1));
            }
            return this;
        },
        reflow: noop,
        render: function (reflow) {
            if (reflow) {
                this.reflow();
            }
            if (this.options.animation && !reflow) {
                helpers.animationLoop(
					this.draw,
					this.options.animationSteps,
					this.options.animationEasing,
					this.options.onAnimationProgress,
					this.options.onAnimationComplete,
					this
				);
            }
            else {
                this.draw();
                this.options.onAnimationComplete.call(this);
            }
            return this;
        },
        generateLegend: function () {
            return template(this.options.legendTemplate, this);
        },
        destroy: function () {
            this.clear();
            unbindEvents(this, this.events);
            delete Chart.instances[this.id];
        },
        showTooltip: function (ChartElements, forceRedraw) {
            // Only redraw the chart if we've actually changed what we're hovering on.
            if (typeof this.activeElements === 'undefined') this.activeElements = [];

            var isChanged = (function (Elements) {
                var changed = false;

                if (Elements.length !== this.activeElements.length) {
                    changed = true;
                    return changed;
                }

                each(Elements, function (element, index) {
                    if (element !== this.activeElements[index]) {
                        changed = true;
                    }
                }, this);
                return changed;
            }).call(this, ChartElements);

            if (!isChanged && !forceRedraw) {
                return;
            }
            else {
                this.activeElements = ChartElements;
            }
            this.draw();
            if (ChartElements.length > 0) {
                // If we have multiple datasets, show a MultiTooltip for all of the data points at that index
                if (this.datasets && this.datasets.length > 1) {
                    var dataArray,
						dataIndex;

                    for (var i = this.datasets.length - 1; i >= 0; i--) {
                        dataArray = this.datasets[i].points || this.datasets[i].bars || this.datasets[i].segments;
                        dataIndex = indexOf(dataArray, ChartElements[0]);
                        if (dataIndex !== -1) {
                            break;
                        }
                    }
                    var tooltipLabels = [],
						tooltipColors = [],
						medianPosition = (function (index) {

						    // Get all the points at that particular index
						    var Elements = [],
								dataCollection,
								xPositions = [],
								yPositions = [],
								xMax,
								yMax,
								xMin,
								yMin;
						    helpers.each(this.datasets, function (dataset) {
						        dataCollection = dataset.points || dataset.bars || dataset.segments;
						        if (dataCollection[dataIndex] && dataCollection[dataIndex].hasValue()) {
						            Elements.push(dataCollection[dataIndex]);
						        }
						    });

						    helpers.each(Elements, function (element) {
						        xPositions.push(element.x);
						        yPositions.push(element.y);


						        //Include any colour information about the element
						        tooltipLabels.push(helpers.template(this.options.multiTooltipTemplate, element));
						        tooltipColors.push({
						            fill: element._saved.fillColor || element.fillColor,
						            stroke: element._saved.strokeColor || element.strokeColor
						        });

						    }, this);

						    yMin = min(yPositions);
						    yMax = max(yPositions);

						    xMin = min(xPositions);
						    xMax = max(xPositions);

						    return {
						        x: (xMin > this.chart.width / 2) ? xMin : xMax,
						        y: (yMin + yMax) / 2
						    };
						}).call(this, dataIndex);

                    new Chart.MultiTooltip({
                        x: medianPosition.x,
                        y: medianPosition.y,
                        xPadding: this.options.tooltipXPadding,
                        yPadding: this.options.tooltipYPadding,
                        xOffset: this.options.tooltipXOffset,
                        fillColor: this.options.tooltipFillColor,
                        textColor: this.options.tooltipFontColor,
                        fontFamily: this.options.tooltipFontFamily,
                        fontStyle: this.options.tooltipFontStyle,
                        fontSize: this.options.tooltipFontSize,
                        titleTextColor: this.options.tooltipTitleFontColor,
                        titleFontFamily: this.options.tooltipTitleFontFamily,
                        titleFontStyle: this.options.tooltipTitleFontStyle,
                        titleFontSize: this.options.tooltipTitleFontSize,
                        cornerRadius: this.options.tooltipCornerRadius,
                        labels: tooltipLabels,
                        legendColors: tooltipColors,
                        legendColorBackground: this.options.multiTooltipKeyBackground,
                        title: ChartElements[0].label,
                        chart: this.chart,
                        ctx: this.chart.ctx
                    }).draw();

                } else {
                    each(ChartElements, function (Element) {
                        var tooltipPosition = Element.tooltipPosition();
                        new Chart.Tooltip({
                            x: Math.round(tooltipPosition.x),
                            y: Math.round(tooltipPosition.y),
                            xPadding: this.options.tooltipXPadding,
                            yPadding: this.options.tooltipYPadding,
                            fillColor: this.options.tooltipFillColor,
                            textColor: this.options.tooltipFontColor,
                            fontFamily: this.options.tooltipFontFamily,
                            fontStyle: this.options.tooltipFontStyle,
                            fontSize: this.options.tooltipFontSize,
                            caretHeight: this.options.tooltipCaretSize,
                            cornerRadius: this.options.tooltipCornerRadius,
                            text: template(this.options.tooltipTemplate, Element),
                            chart: this.chart
                        }).draw();
                    }, this);
                }
            }
            return this;
        },
        toBase64Image: function () {
            return this.chart.canvas.toDataURL.apply(this.chart.canvas, arguments);
        }
    });

    Chart.Type.extend = function (extensions) {

        var parent = this;

        var ChartType = function () {
            return parent.apply(this, arguments);
        };

        //Copy the prototype object of the this class
        ChartType.prototype = clone(parent.prototype);
        //Now overwrite some of the properties in the base class with the new extensions
        extend(ChartType.prototype, extensions);

        ChartType.extend = Chart.Type.extend;

        if (extensions.name || parent.prototype.name) {

            var chartName = extensions.name || parent.prototype.name;
            //Assign any potential default values of the new chart type

            //If none are defined, we'll use a clone of the chart type this is being extended from.
            //I.e. if we extend a line chart, we'll use the defaults from the line chart if our new chart
            //doesn't define some defaults of their own.

            var baseDefaults = (Chart.defaults[parent.prototype.name]) ? clone(Chart.defaults[parent.prototype.name]) : {};

            Chart.defaults[chartName] = extend(baseDefaults, extensions.defaults);

            Chart.types[chartName] = ChartType;

            //Register this new chart type in the Chart prototype
            Chart.prototype[chartName] = function (data, options) {
                var config = merge(Chart.defaults.global, Chart.defaults[chartName], options || {});
                return new ChartType(data, config, this);
            };
        } else {
            warn("Name not provided for this chart, so it hasn't been registered");
        }
        return parent;
    };

    Chart.Element = function (configuration) {
        extend(this, configuration);
        this.initialize.apply(this, arguments);
        this.save();
    };
    extend(Chart.Element.prototype, {
        initialize: function () { },
        restore: function (props) {
            if (!props) {
                extend(this, this._saved);
            } else {
                each(props, function (key) {
                    this[key] = this._saved[key];
                }, this);
            }
            return this;
        },
        save: function () {
            this._saved = clone(this);
            delete this._saved._saved;
            return this;
        },
        update: function (newProps) {
            each(newProps, function (value, key) {
                this._saved[key] = this[key];
                this[key] = value;
            }, this);
            return this;
        },
        transition: function (props, ease) {
            each(props, function (value, key) {
                this[key] = ((value - this._saved[key]) * ease) + this._saved[key];
            }, this);
            return this;
        },
        tooltipPosition: function () {
            return {
                x: this.x,
                y: this.y
            };
        },
        hasValue: function () {
            return isNumber(this.value);
        }
    });

    Chart.Element.extend = inherits;


    Chart.Point = Chart.Element.extend({
        display: true,
        inRange: function (chartX, chartY) {
            var hitDetectionRange = this.hitDetectionRadius + this.radius;
            return ((Math.pow(chartX - this.x, 2) + Math.pow(chartY - this.y, 2)) < Math.pow(hitDetectionRange, 2));
        },
        draw: function () {
            if (this.display) {
                var ctx = this.ctx;
                ctx.beginPath();

                ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
                ctx.closePath();

                ctx.strokeStyle = this.strokeColor;
                ctx.lineWidth = this.strokeWidth;

                ctx.fillStyle = this.fillColor;

                ctx.fill();
                ctx.stroke();
            }


            //Quick debug for bezier curve splining
            //Highlights control points and the line between them.
            //Handy for dev - stripped in the min version.

            // ctx.save();
            // ctx.fillStyle = "black";
            // ctx.strokeStyle = "black"
            // ctx.beginPath();
            // ctx.arc(this.controlPoints.inner.x,this.controlPoints.inner.y, 2, 0, Math.PI*2);
            // ctx.fill();

            // ctx.beginPath();
            // ctx.arc(this.controlPoints.outer.x,this.controlPoints.outer.y, 2, 0, Math.PI*2);
            // ctx.fill();

            // ctx.moveTo(this.controlPoints.inner.x,this.controlPoints.inner.y);
            // ctx.lineTo(this.x, this.y);
            // ctx.lineTo(this.controlPoints.outer.x,this.controlPoints.outer.y);
            // ctx.stroke();

            // ctx.restore();



        }
    });

    Chart.Arc = Chart.Element.extend({
        inRange: function (chartX, chartY) {

            var pointRelativePosition = helpers.getAngleFromPoint(this, {
                x: chartX,
                y: chartY
            });

            //Check if within the range of the open/close angle
            var betweenAngles = (pointRelativePosition.angle >= this.startAngle && pointRelativePosition.angle <= this.endAngle),
				withinRadius = (pointRelativePosition.distance >= this.innerRadius && pointRelativePosition.distance <= this.outerRadius);

            return (betweenAngles && withinRadius);
            //Ensure within the outside of the arc centre, but inside arc outer
        },
        tooltipPosition: function () {
            var centreAngle = this.startAngle + ((this.endAngle - this.startAngle) / 2),
				rangeFromCentre = (this.outerRadius - this.innerRadius) / 2 + this.innerRadius;
            return {
                x: this.x + (Math.cos(centreAngle) * rangeFromCentre),
                y: this.y + (Math.sin(centreAngle) * rangeFromCentre)
            };
        },
        draw: function (animationPercent) {

            var easingDecimal = animationPercent || 1;

            var ctx = this.ctx;

            ctx.beginPath();

            ctx.arc(this.x, this.y, this.outerRadius, this.startAngle, this.endAngle);

            ctx.arc(this.x, this.y, this.innerRadius, this.endAngle, this.startAngle, true);

            ctx.closePath();
            ctx.strokeStyle = this.strokeColor;
            ctx.lineWidth = this.strokeWidth;

            ctx.fillStyle = this.fillColor;

            ctx.fill();
            ctx.lineJoin = 'bevel';

            if (this.showStroke) {
                ctx.stroke();
            }
        }
    });

    Chart.Rectangle = Chart.Element.extend({
        draw: function () {
            var ctx = this.ctx,
				halfWidth = this.width / 2,
				leftX = this.x - halfWidth,
				rightX = this.x + halfWidth,
				top = this.base - (this.base - this.y),
				halfStroke = this.strokeWidth / 2;

            // Canvas doesn't allow us to stroke inside the width so we can
            // adjust the sizes to fit if we're setting a stroke on the line
            if (this.showStroke) {
                leftX += halfStroke;
                rightX -= halfStroke;
                top += halfStroke;
            }

            ctx.beginPath();

            ctx.fillStyle = this.fillColor;
            ctx.strokeStyle = this.strokeColor;
            ctx.lineWidth = this.strokeWidth;

            // It'd be nice to keep this class totally generic to any rectangle
            // and simply specify which border to miss out.
            ctx.moveTo(leftX, this.base);
            ctx.lineTo(leftX, top);
            ctx.lineTo(rightX, top);
            ctx.lineTo(rightX, this.base);
            ctx.fill();
            if (this.showStroke) {
                ctx.stroke();
            }
        },
        height: function () {
            return this.base - this.y;
        },
        inRange: function (chartX, chartY) {
            return (chartX >= this.x - this.width / 2 && chartX <= this.x + this.width / 2) && (chartY >= this.y && chartY <= this.base);
        }
    });

    Chart.Tooltip = Chart.Element.extend({
        draw: function () {

            var ctx = this.chart.ctx;

            ctx.font = fontString(this.fontSize, this.fontStyle, this.fontFamily);

            this.xAlign = "center";
            this.yAlign = "above";

            //Distance between the actual element.y position and the start of the tooltip caret
            var caretPadding = 2;

            var tooltipWidth = ctx.measureText(this.text).width + 2 * this.xPadding,
				tooltipRectHeight = this.fontSize + 2 * this.yPadding,
				tooltipHeight = tooltipRectHeight + this.caretHeight + caretPadding;

            if (this.x + tooltipWidth / 2 > this.chart.width) {
                this.xAlign = "left";
            } else if (this.x - tooltipWidth / 2 < 0) {
                this.xAlign = "right";
            }

            if (this.y - tooltipHeight < 0) {
                this.yAlign = "below";
            }


            var tooltipX = this.x - tooltipWidth / 2,
				tooltipY = this.y - tooltipHeight;

            ctx.fillStyle = this.fillColor;

            switch (this.yAlign) {
                case "above":
                    //Draw a caret above the x/y
                    ctx.beginPath();
                    ctx.moveTo(this.x, this.y - caretPadding);
                    ctx.lineTo(this.x + this.caretHeight, this.y - (caretPadding + this.caretHeight));
                    ctx.lineTo(this.x - this.caretHeight, this.y - (caretPadding + this.caretHeight));
                    ctx.closePath();
                    ctx.fill();
                    break;
                case "below":
                    tooltipY = this.y + caretPadding + this.caretHeight;
                    //Draw a caret below the x/y
                    ctx.beginPath();
                    ctx.moveTo(this.x, this.y + caretPadding);
                    ctx.lineTo(this.x + this.caretHeight, this.y + caretPadding + this.caretHeight);
                    ctx.lineTo(this.x - this.caretHeight, this.y + caretPadding + this.caretHeight);
                    ctx.closePath();
                    ctx.fill();
                    break;
            }

            switch (this.xAlign) {
                case "left":
                    tooltipX = this.x - tooltipWidth + (this.cornerRadius + this.caretHeight);
                    break;
                case "right":
                    tooltipX = this.x - (this.cornerRadius + this.caretHeight);
                    break;
            }

            drawRoundedRectangle(ctx, tooltipX, tooltipY, tooltipWidth, tooltipRectHeight, this.cornerRadius);

            ctx.fill();

            ctx.fillStyle = this.textColor;
            ctx.textAlign = "center";
            ctx.textBaseline = "middle";
            ctx.fillText(this.text, tooltipX + tooltipWidth / 2, tooltipY + tooltipRectHeight / 2);
        }
    });

    Chart.MultiTooltip = Chart.Element.extend({
        initialize: function () {
            this.font = fontString(this.fontSize, this.fontStyle, this.fontFamily);

            this.titleFont = fontString(this.titleFontSize, this.titleFontStyle, this.titleFontFamily);

            this.height = (this.labels.length * this.fontSize) + ((this.labels.length - 1) * (this.fontSize / 2)) + (this.yPadding * 2) + this.titleFontSize * 1.5;

            this.ctx.font = this.titleFont;

            var titleWidth = this.ctx.measureText(this.title).width,
				//Label has a legend square as well so account for this.
				labelWidth = longestText(this.ctx, this.font, this.labels) + this.fontSize + 3,
				longestTextWidth = max([labelWidth, titleWidth]);

            this.width = longestTextWidth + (this.xPadding * 2);


            var halfHeight = this.height / 2;

            //Check to ensure the height will fit on the canvas
            //The three is to buffer form the very
            if (this.y - halfHeight < 0) {
                this.y = halfHeight;
            } else if (this.y + halfHeight > this.chart.height) {
                this.y = this.chart.height - halfHeight;
            }

            //Decide whether to align left or right based on position on canvas
            if (this.x > this.chart.width / 2) {
                this.x -= this.xOffset + this.width;
            } else {
                this.x += this.xOffset;
            }


        },
        getLineHeight: function (index) {
            var baseLineHeight = this.y - (this.height / 2) + this.yPadding,
				afterTitleIndex = index - 1;

            //If the index is zero, we're getting the title
            if (index === 0) {
                return baseLineHeight + this.titleFontSize / 2;
            } else {
                return baseLineHeight + ((this.fontSize * 1.5 * afterTitleIndex) + this.fontSize / 2) + this.titleFontSize * 1.5;
            }

        },
        draw: function () {
            drawRoundedRectangle(this.ctx, this.x, this.y - this.height / 2, this.width, this.height, this.cornerRadius);
            var ctx = this.ctx;
            ctx.fillStyle = this.fillColor;
            ctx.fill();
            ctx.closePath();

            ctx.textAlign = "left";
            ctx.textBaseline = "middle";
            ctx.fillStyle = this.titleTextColor;
            ctx.font = this.titleFont;

            ctx.fillText(this.title, this.x + this.xPadding, this.getLineHeight(0));

            ctx.font = this.font;
            helpers.each(this.labels, function (label, index) {
                ctx.fillStyle = this.textColor;
                ctx.fillText(label, this.x + this.xPadding + this.fontSize + 3, this.getLineHeight(index + 1));

                //A bit gnarly, but clearing this rectangle breaks when using explorercanvas (clears whole canvas)
                //ctx.clearRect(this.x + this.xPadding, this.getLineHeight(index + 1) - this.fontSize/2, this.fontSize, this.fontSize);
                //Instead we'll make a white filled block to put the legendColour palette over.

                ctx.fillStyle = this.legendColorBackground;
                ctx.fillRect(this.x + this.xPadding, this.getLineHeight(index + 1) - this.fontSize / 2, this.fontSize, this.fontSize);

                ctx.fillStyle = this.legendColors[index].fill;
                ctx.fillRect(this.x + this.xPadding, this.getLineHeight(index + 1) - this.fontSize / 2, this.fontSize, this.fontSize);


            }, this);
        }
    });

    Chart.Scale = Chart.Element.extend({
        initialize: function () {
            this.fit();
        },
        buildYLabels: function () {
            this.yLabels = [];

            var stepDecimalPlaces = getDecimalPlaces(this.stepValue);

            for (var i = 0; i <= this.steps; i++) {
                this.yLabels.push(template(this.templateString, { value: (this.min + (i * this.stepValue)).toFixed(stepDecimalPlaces) }));
            }
            this.yLabelWidth = (this.display && this.showLabels) ? longestText(this.ctx, this.font, this.yLabels) : 0;
        },
        addXLabel: function (label) {
            this.xLabels.push(label);
            this.valuesCount++;
            this.fit();
        },
        removeXLabel: function () {
            this.xLabels.shift();
            this.valuesCount--;
            this.fit();
        },
        // Fitting loop to rotate x Labels and figure out what fits there, and also calculate how many Y steps to use
        fit: function () {
            // First we need the width of the yLabels, assuming the xLabels aren't rotated

            // To do that we need the base line at the top and base of the chart, assuming there is no x label rotation
            this.startPoint = (this.display) ? this.fontSize : 0;
            this.endPoint = (this.display) ? this.height - (this.fontSize * 1.5) - 5 : this.height; // -5 to pad labels

            // Apply padding settings to the start and end point.
            this.startPoint += this.padding;
            this.endPoint -= this.padding;

            // Cache the starting height, so can determine if we need to recalculate the scale yAxis
            var cachedHeight = this.endPoint - this.startPoint,
				cachedYLabelWidth;

            // Build the current yLabels so we have an idea of what size they'll be to start
            /*
			 *	This sets what is returned from calculateScaleRange as static properties of this class:
			 *
				this.steps;
				this.stepValue;
				this.min;
				this.max;
			 *
			 */
            this.calculateYRange(cachedHeight);

            // With these properties set we can now build the array of yLabels
            // and also the width of the largest yLabel
            this.buildYLabels();

            this.calculateXLabelRotation();

            while ((cachedHeight > this.endPoint - this.startPoint)) {
                cachedHeight = this.endPoint - this.startPoint;
                cachedYLabelWidth = this.yLabelWidth;

                this.calculateYRange(cachedHeight);
                this.buildYLabels();

                // Only go through the xLabel loop again if the yLabel width has changed
                if (cachedYLabelWidth < this.yLabelWidth) {
                    this.calculateXLabelRotation();
                }
            }

        },
        calculateXLabelRotation: function () {
            //Get the width of each grid by calculating the difference
            //between x offsets between 0 and 1.

            this.ctx.font = this.font;

            var firstWidth = this.ctx.measureText(this.xLabels[0]).width,
				lastWidth = this.ctx.measureText(this.xLabels[this.xLabels.length - 1]).width,
				firstRotated,
				lastRotated;


            this.xScalePaddingRight = lastWidth / 2 + 3;
            this.xScalePaddingLeft = (firstWidth / 2 > this.yLabelWidth + 10) ? firstWidth / 2 : this.yLabelWidth + 10;

            this.xLabelRotation = 0;
            if (this.display) {
                var originalLabelWidth = longestText(this.ctx, this.font, this.xLabels),
					cosRotation,
					firstRotatedWidth;
                this.xLabelWidth = originalLabelWidth;
                //Allow 3 pixels x2 padding either side for label readability
                var xGridWidth = Math.floor(this.calculateX(1) - this.calculateX(0)) - 6;

                //Max label rotate should be 90 - also act as a loop counter
                while ((this.xLabelWidth > xGridWidth && this.xLabelRotation === 0) || (this.xLabelWidth > xGridWidth && this.xLabelRotation <= 90 && this.xLabelRotation > 0)) {
                    cosRotation = Math.cos(toRadians(this.xLabelRotation));

                    firstRotated = cosRotation * firstWidth;
                    lastRotated = cosRotation * lastWidth;

                    // We're right aligning the text now.
                    if (firstRotated + this.fontSize / 2 > this.yLabelWidth + 8) {
                        this.xScalePaddingLeft = firstRotated + this.fontSize / 2;
                    }
                    this.xScalePaddingRight = this.fontSize / 2;


                    this.xLabelRotation++;
                    this.xLabelWidth = cosRotation * originalLabelWidth;

                }
                if (this.xLabelRotation > 0) {
                    this.endPoint -= Math.sin(toRadians(this.xLabelRotation)) * originalLabelWidth + 3;
                }
            }
            else {
                this.xLabelWidth = 0;
                this.xScalePaddingRight = this.padding;
                this.xScalePaddingLeft = this.padding;
            }

        },
        // Needs to be overidden in each Chart type
        // Otherwise we need to pass all the data into the scale class
        calculateYRange: noop,
        drawingArea: function () {
            return this.startPoint - this.endPoint;
        },
        calculateY: function (value) {
            var scalingFactor = this.drawingArea() / (this.min - this.max);
            return this.endPoint - (scalingFactor * (value - this.min));
        },
        calculateX: function (index) {
            var isRotated = (this.xLabelRotation > 0),
				// innerWidth = (this.offsetGridLines) ? this.width - offsetLeft - this.padding : this.width - (offsetLeft + halfLabelWidth * 2) - this.padding,
				innerWidth = this.width - (this.xScalePaddingLeft + this.xScalePaddingRight),
				valueWidth = innerWidth / (this.valuesCount - ((this.offsetGridLines) ? 0 : 1)),
				valueOffset = (valueWidth * index) + this.xScalePaddingLeft;

            if (this.offsetGridLines) {
                valueOffset += (valueWidth / 2);
            }

            return Math.round(valueOffset);
        },
        update: function (newProps) {
            helpers.extend(this, newProps);
            this.fit();
        },
        draw: function () {
            var ctx = this.ctx,
				yLabelGap = (this.endPoint - this.startPoint) / this.steps,
				xStart = Math.round(this.xScalePaddingLeft);
            if (this.display) {
                ctx.fillStyle = this.textColor;
                ctx.font = this.font;
                each(this.yLabels, function (labelString, index) {
                    var yLabelCenter = this.endPoint - (yLabelGap * index),
						linePositionY = Math.round(yLabelCenter);

                    ctx.textAlign = "right";
                    ctx.textBaseline = "middle";
                    if (this.showLabels) {
                        ctx.fillText(labelString, xStart - 10, yLabelCenter);
                    }
                    ctx.beginPath();
                    if (index > 0) {
                        // This is a grid line in the centre, so drop that
                        ctx.lineWidth = this.gridLineWidth;
                        ctx.strokeStyle = this.gridLineColor;
                    } else {
                        // This is the first line on the scale
                        ctx.lineWidth = this.lineWidth;
                        ctx.strokeStyle = this.lineColor;
                    }

                    linePositionY += helpers.aliasPixel(ctx.lineWidth);

                    ctx.moveTo(xStart, linePositionY);
                    ctx.lineTo(this.width, linePositionY);
                    ctx.stroke();
                    ctx.closePath();

                    ctx.lineWidth = this.lineWidth;
                    ctx.strokeStyle = this.lineColor;
                    ctx.beginPath();
                    ctx.moveTo(xStart - 5, linePositionY);
                    ctx.lineTo(xStart, linePositionY);
                    ctx.stroke();
                    ctx.closePath();

                }, this);

                each(this.xLabels, function (label, index) {
                    var xPos = this.calculateX(index) + aliasPixel(this.lineWidth),
						// Check to see if line/bar here and decide where to place the line
						linePos = this.calculateX(index - (this.offsetGridLines ? 0.5 : 0)) + aliasPixel(this.lineWidth),
						isRotated = (this.xLabelRotation > 0);

                    ctx.beginPath();

                    if (index > 0) {
                        // This is a grid line in the centre, so drop that
                        ctx.lineWidth = this.gridLineWidth;
                        ctx.strokeStyle = this.gridLineColor;
                    } else {
                        // This is the first line on the scale
                        ctx.lineWidth = this.lineWidth;
                        ctx.strokeStyle = this.lineColor;
                    }
                    ctx.moveTo(linePos, this.endPoint);
                    ctx.lineTo(linePos, this.startPoint - 3);
                    ctx.stroke();
                    ctx.closePath();


                    ctx.lineWidth = this.lineWidth;
                    ctx.strokeStyle = this.lineColor;


                    // Small lines at the bottom of the base grid line
                    ctx.beginPath();
                    ctx.moveTo(linePos, this.endPoint);
                    ctx.lineTo(linePos, this.endPoint + 5);
                    ctx.stroke();
                    ctx.closePath();

                    ctx.save();
                    ctx.translate(xPos, (isRotated) ? this.endPoint + 12 : this.endPoint + 8);
                    ctx.rotate(toRadians(this.xLabelRotation) * -1);
                    ctx.font = this.font;
                    ctx.textAlign = (isRotated) ? "right" : "center";
                    ctx.textBaseline = (isRotated) ? "middle" : "top";
                    ctx.fillText(label, 0, 0);
                    ctx.restore();
                }, this);

            }
        }

    });

    Chart.RadialScale = Chart.Element.extend({
        initialize: function () {
            this.size = min([this.height, this.width]);
            this.drawingArea = (this.display) ? (this.size / 2) - (this.fontSize / 2 + this.backdropPaddingY) : (this.size / 2);
        },
        calculateCenterOffset: function (value) {
            // Take into account half font size + the yPadding of the top value
            var scalingFactor = this.drawingArea / (this.max - this.min);

            return (value - this.min) * scalingFactor;
        },
        update: function () {
            if (!this.lineArc) {
                this.setScaleSize();
            } else {
                this.drawingArea = (this.display) ? (this.size / 2) - (this.fontSize / 2 + this.backdropPaddingY) : (this.size / 2);
            }
            this.buildYLabels();
        },
        buildYLabels: function () {
            this.yLabels = [];

            var stepDecimalPlaces = getDecimalPlaces(this.stepValue);

            for (var i = 0; i <= this.steps; i++) {
                this.yLabels.push(template(this.templateString, { value: (this.min + (i * this.stepValue)).toFixed(stepDecimalPlaces) }));
            }
        },
        getCircumference: function () {
            return ((Math.PI * 2) / this.valuesCount);
        },
        setScaleSize: function () {
            /*
			 * Right, this is really confusing and there is a lot of maths going on here
			 * The gist of the problem is here: https://gist.github.com/nnnick/696cc9c55f4b0beb8fe9
			 *
			 * Reaction: https://dl.dropboxusercontent.com/u/34601363/toomuchscience.gif
			 *
			 * Solution:
			 *
			 * We assume the radius of the polygon is half the size of the canvas at first
			 * at each index we check if the text overlaps.
			 *
			 * Where it does, we store that angle and that index.
			 *
			 * After finding the largest index and angle we calculate how much we need to remove
			 * from the shape radius to move the point inwards by that x.
			 *
			 * We average the left and right distances to get the maximum shape radius that can fit in the box
			 * along with labels.
			 *
			 * Once we have that, we can find the centre point for the chart, by taking the x text protrusion
			 * on each side, removing that from the size, halving it and adding the left x protrusion width.
			 *
			 * This will mean we have a shape fitted to the canvas, as large as it can be with the labels
			 * and position it in the most space efficient manner
			 *
			 * https://dl.dropboxusercontent.com/u/34601363/yeahscience.gif
			 */


            // Get maximum radius of the polygon. Either half the height (minus the text width) or half the width.
            // Use this to calculate the offset + change. - Make sure L/R protrusion is at least 0 to stop issues with centre points
            var largestPossibleRadius = min([(this.height / 2 - this.pointLabelFontSize - 5), this.width / 2]),
				pointPosition,
				i,
				textWidth,
				halfTextWidth,
				furthestRight = this.width,
				furthestRightIndex,
				furthestRightAngle,
				furthestLeft = 0,
				furthestLeftIndex,
				furthestLeftAngle,
				xProtrusionLeft,
				xProtrusionRight,
				radiusReductionRight,
				radiusReductionLeft,
				maxWidthRadius;
            this.ctx.font = fontString(this.pointLabelFontSize, this.pointLabelFontStyle, this.pointLabelFontFamily);
            for (i = 0; i < this.valuesCount; i++) {
                // 5px to space the text slightly out - similar to what we do in the draw function.
                pointPosition = this.getPointPosition(i, largestPossibleRadius);
                textWidth = this.ctx.measureText(template(this.templateString, { value: this.labels[i] })).width + 5;
                if (i === 0 || i === this.valuesCount / 2) {
                    // If we're at index zero, or exactly the middle, we're at exactly the top/bottom
                    // of the radar chart, so text will be aligned centrally, so we'll half it and compare
                    // w/left and right text sizes
                    halfTextWidth = textWidth / 2;
                    if (pointPosition.x + halfTextWidth > furthestRight) {
                        furthestRight = pointPosition.x + halfTextWidth;
                        furthestRightIndex = i;
                    }
                    if (pointPosition.x - halfTextWidth < furthestLeft) {
                        furthestLeft = pointPosition.x - halfTextWidth;
                        furthestLeftIndex = i;
                    }
                }
                else if (i < this.valuesCount / 2) {
                    // Less than half the values means we'll left align the text
                    if (pointPosition.x + textWidth > furthestRight) {
                        furthestRight = pointPosition.x + textWidth;
                        furthestRightIndex = i;
                    }
                }
                else if (i > this.valuesCount / 2) {
                    // More than half the values means we'll right align the text
                    if (pointPosition.x - textWidth < furthestLeft) {
                        furthestLeft = pointPosition.x - textWidth;
                        furthestLeftIndex = i;
                    }
                }
            }

            xProtrusionLeft = furthestLeft;

            xProtrusionRight = Math.ceil(furthestRight - this.width);

            furthestRightAngle = this.getIndexAngle(furthestRightIndex);

            furthestLeftAngle = this.getIndexAngle(furthestLeftIndex);

            radiusReductionRight = xProtrusionRight / Math.sin(furthestRightAngle + Math.PI / 2);

            radiusReductionLeft = xProtrusionLeft / Math.sin(furthestLeftAngle + Math.PI / 2);

            // Ensure we actually need to reduce the size of the chart
            radiusReductionRight = (isNumber(radiusReductionRight)) ? radiusReductionRight : 0;
            radiusReductionLeft = (isNumber(radiusReductionLeft)) ? radiusReductionLeft : 0;

            this.drawingArea = largestPossibleRadius - (radiusReductionLeft + radiusReductionRight) / 2;

            //this.drawingArea = min([maxWidthRadius, (this.height - (2 * (this.pointLabelFontSize + 5)))/2])
            this.setCenterPoint(radiusReductionLeft, radiusReductionRight);

        },
        setCenterPoint: function (leftMovement, rightMovement) {

            var maxRight = this.width - rightMovement - this.drawingArea,
				maxLeft = leftMovement + this.drawingArea;

            this.xCenter = (maxLeft + maxRight) / 2;
            // Always vertically in the centre as the text height doesn't change
            this.yCenter = (this.height / 2);
        },

        getIndexAngle: function (index) {
            var angleMultiplier = (Math.PI * 2) / this.valuesCount;
            // Start from the top instead of right, so remove a quarter of the circle

            return index * angleMultiplier - (Math.PI / 2);
        },
        getPointPosition: function (index, distanceFromCenter) {
            var thisAngle = this.getIndexAngle(index);
            return {
                x: (Math.cos(thisAngle) * distanceFromCenter) + this.xCenter,
                y: (Math.sin(thisAngle) * distanceFromCenter) + this.yCenter
            };
        },
        draw: function () {
            if (this.display) {
                var ctx = this.ctx;
                each(this.yLabels, function (label, index) {
                    // Don't draw a centre value
                    if (index > 0) {
                        var yCenterOffset = index * (this.drawingArea / this.steps),
							yHeight = this.yCenter - yCenterOffset,
							pointPosition;

                        // Draw circular lines around the scale
                        if (this.lineWidth > 0) {
                            ctx.strokeStyle = this.lineColor;
                            ctx.lineWidth = this.lineWidth;

                            if (this.lineArc) {
                                ctx.beginPath();
                                ctx.arc(this.xCenter, this.yCenter, yCenterOffset, 0, Math.PI * 2);
                                ctx.closePath();
                                ctx.stroke();
                            } else {
                                ctx.beginPath();
                                for (var i = 0; i < this.valuesCount; i++) {
                                    pointPosition = this.getPointPosition(i, this.calculateCenterOffset(this.min + (index * this.stepValue)));
                                    if (i === 0) {
                                        ctx.moveTo(pointPosition.x, pointPosition.y);
                                    } else {
                                        ctx.lineTo(pointPosition.x, pointPosition.y);
                                    }
                                }
                                ctx.closePath();
                                ctx.stroke();
                            }
                        }
                        if (this.showLabels) {
                            ctx.font = fontString(this.fontSize, this.fontStyle, this.fontFamily);
                            if (this.showLabelBackdrop) {
                                var labelWidth = ctx.measureText(label).width;
                                ctx.fillStyle = this.backdropColor;
                                ctx.fillRect(
									this.xCenter - labelWidth / 2 - this.backdropPaddingX,
									yHeight - this.fontSize / 2 - this.backdropPaddingY,
									labelWidth + this.backdropPaddingX * 2,
									this.fontSize + this.backdropPaddingY * 2
								);
                            }
                            ctx.textAlign = 'center';
                            ctx.textBaseline = "middle";
                            ctx.fillStyle = this.fontColor;
                            ctx.fillText(label, this.xCenter, yHeight);
                        }
                    }
                }, this);

                if (!this.lineArc) {
                    ctx.lineWidth = this.angleLineWidth;
                    ctx.strokeStyle = this.angleLineColor;
                    for (var i = this.valuesCount - 1; i >= 0; i--) {
                        if (this.angleLineWidth > 0) {
                            var outerPosition = this.getPointPosition(i, this.calculateCenterOffset(this.max));
                            ctx.beginPath();
                            ctx.moveTo(this.xCenter, this.yCenter);
                            ctx.lineTo(outerPosition.x, outerPosition.y);
                            ctx.stroke();
                            ctx.closePath();
                        }
                        // Extra 3px out for some label spacing
                        var pointLabelPosition = this.getPointPosition(i, this.calculateCenterOffset(this.max) + 5);
                        ctx.font = fontString(this.pointLabelFontSize, this.pointLabelFontStyle, this.pointLabelFontFamily);
                        ctx.fillStyle = this.pointLabelFontColor;

                        var labelsCount = this.labels.length,
							halfLabelsCount = this.labels.length / 2,
							quarterLabelsCount = halfLabelsCount / 2,
							upperHalf = (i < quarterLabelsCount || i > labelsCount - quarterLabelsCount),
							exactQuarter = (i === quarterLabelsCount || i === labelsCount - quarterLabelsCount);
                        if (i === 0) {
                            ctx.textAlign = 'center';
                        } else if (i === halfLabelsCount) {
                            ctx.textAlign = 'center';
                        } else if (i < halfLabelsCount) {
                            ctx.textAlign = 'left';
                        } else {
                            ctx.textAlign = 'right';
                        }

                        // Set the correct text baseline based on outer positioning
                        if (exactQuarter) {
                            ctx.textBaseline = 'middle';
                        } else if (upperHalf) {
                            ctx.textBaseline = 'bottom';
                        } else {
                            ctx.textBaseline = 'top';
                        }

                        ctx.fillText(this.labels[i], pointLabelPosition.x, pointLabelPosition.y);
                    }
                }
            }
        }
    });

    // Attach global event to resize each chart instance when the browser resizes
    helpers.addEvent(window, "resize", (function () {
        // Basic debounce of resize function so it doesn't hurt performance when resizing browser.
        var timeout;
        return function () {
            clearTimeout(timeout);
            timeout = setTimeout(function () {
                each(Chart.instances, function (instance) {
                    // If the responsive flag is set in the chart instance config
                    // Cascade the resize event down to the chart.
                    if (instance.options.responsive) {
                        instance.resize(instance.render, true);
                    }
                });
            }, 50);
        };
    })());


    if (amd) {
        define(function () {
            return Chart;
        });
    } else if (typeof module === 'object' && module.exports) {
        module.exports = Chart;
    }

    root.Chart = Chart;

    Chart.noConflict = function () {
        root.Chart = previous;
        return Chart;
    };

}).call(this);

(function () {
    "use strict";

    var root = this,
		Chart = root.Chart,
		helpers = Chart.helpers;


    var defaultConfig = {
        //Boolean - Whether the scale should start at zero, or an order of magnitude down from the lowest value
        scaleBeginAtZero: true,

        //Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: true,

        //String - Colour of the grid lines
        scaleGridLineColor: "rgba(0,0,0,.05)",

        //Number - Width of the grid lines
        scaleGridLineWidth: 1,

        //Boolean - If there is a stroke on each bar
        barShowStroke: true,

        //Number - Pixel width of the bar stroke
        barStrokeWidth: 2,

        //Number - Spacing between each of the X value sets
        barValueSpacing: 5,

        //Number - Spacing between data sets within X values
        barDatasetSpacing: 1,

        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].fillColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>"

    };


    Chart.Type.extend({
        name: "Bar",
        defaults: defaultConfig,
        initialize: function (data) {

            //Expose options as a scope variable here so we can access it in the ScaleClass
            var options = this.options;

            this.ScaleClass = Chart.Scale.extend({
                offsetGridLines: true,
                calculateBarX: function (datasetCount, datasetIndex, barIndex) {
                    //Reusable method for calculating the xPosition of a given bar based on datasetIndex & width of the bar
                    var xWidth = this.calculateBaseWidth(),
						xAbsolute = this.calculateX(barIndex) - (xWidth / 2),
						barWidth = this.calculateBarWidth(datasetCount);

                    return xAbsolute + (barWidth * datasetIndex) + (datasetIndex * options.barDatasetSpacing) + barWidth / 2;
                },
                calculateBaseWidth: function () {
                    return (this.calculateX(1) - this.calculateX(0)) - (2 * options.barValueSpacing);
                },
                calculateBarWidth: function (datasetCount) {
                    //The padding between datasets is to the right of each bar, providing that there are more than 1 dataset
                    var baseWidth = this.calculateBaseWidth() - ((datasetCount - 1) * options.barDatasetSpacing);

                    return (baseWidth / datasetCount);
                }
            });

            this.datasets = [];

            //Set up tooltip events on the chart
            if (this.options.showTooltips) {
                helpers.bindEvents(this, this.options.tooltipEvents, function (evt) {
                    var activeBars = (evt.type !== 'mouseout') ? this.getBarsAtEvent(evt) : [];

                    this.eachBars(function (bar) {
                        bar.restore(['fillColor', 'strokeColor']);
                    });
                    helpers.each(activeBars, function (activeBar) {
                        activeBar.fillColor = activeBar.highlightFill;
                        activeBar.strokeColor = activeBar.highlightStroke;
                    });
                    this.showTooltip(activeBars);
                });
            }

            //Declare the extension of the default point, to cater for the options passed in to the constructor
            this.BarClass = Chart.Rectangle.extend({
                strokeWidth: this.options.barStrokeWidth,
                showStroke: this.options.barShowStroke,
                ctx: this.chart.ctx
            });

            //Iterate through each of the datasets, and build this into a property of the chart
            helpers.each(data.datasets, function (dataset, datasetIndex) {

                var datasetObject = {
                    label: dataset.label || null,
                    fillColor: dataset.fillColor,
                    strokeColor: dataset.strokeColor,
                    bars: []
                };

                this.datasets.push(datasetObject);

                helpers.each(dataset.data, function (dataPoint, index) {
                    //Add a new point for each piece of data, passing any required data to draw.
                    datasetObject.bars.push(new this.BarClass({
                        value: dataPoint,
                        label: data.labels[index],
                        datasetLabel: dataset.label,
                        strokeColor: dataset.strokeColor,
                        fillColor: dataset.fillColor,
                        highlightFill: dataset.highlightFill || dataset.fillColor,
                        highlightStroke: dataset.highlightStroke || dataset.strokeColor
                    }));
                }, this);

            }, this);

            this.buildScale(data.labels);

            this.BarClass.prototype.base = this.scale.endPoint;

            this.eachBars(function (bar, index, datasetIndex) {
                helpers.extend(bar, {
                    width: this.scale.calculateBarWidth(this.datasets.length),
                    x: this.scale.calculateBarX(this.datasets.length, datasetIndex, index),
                    y: this.scale.endPoint
                });
                bar.save();
            }, this);

            this.render();
        },
        update: function () {
            this.scale.update();
            // Reset any highlight colours before updating.
            helpers.each(this.activeElements, function (activeElement) {
                activeElement.restore(['fillColor', 'strokeColor']);
            });

            this.eachBars(function (bar) {
                bar.save();
            });
            this.render();
        },
        eachBars: function (callback) {
            helpers.each(this.datasets, function (dataset, datasetIndex) {
                helpers.each(dataset.bars, callback, this, datasetIndex);
            }, this);
        },
        getBarsAtEvent: function (e) {
            var barsArray = [],
				eventPosition = helpers.getRelativePosition(e),
				datasetIterator = function (dataset) {
				    barsArray.push(dataset.bars[barIndex]);
				},
				barIndex;

            for (var datasetIndex = 0; datasetIndex < this.datasets.length; datasetIndex++) {
                for (barIndex = 0; barIndex < this.datasets[datasetIndex].bars.length; barIndex++) {
                    if (this.datasets[datasetIndex].bars[barIndex].inRange(eventPosition.x, eventPosition.y)) {
                        helpers.each(this.datasets, datasetIterator);
                        return barsArray;
                    }
                }
            }

            return barsArray;
        },
        buildScale: function (labels) {
            var self = this;

            var dataTotal = function () {
                var values = [];
                self.eachBars(function (bar) {
                    values.push(bar.value);
                });
                return values;
            };

            var scaleOptions = {
                templateString: this.options.scaleLabel,
                height: this.chart.height,
                width: this.chart.width,
                ctx: this.chart.ctx,
                textColor: this.options.scaleFontColor,
                fontSize: this.options.scaleFontSize,
                fontStyle: this.options.scaleFontStyle,
                fontFamily: this.options.scaleFontFamily,
                valuesCount: labels.length,
                beginAtZero: this.options.scaleBeginAtZero,
                integersOnly: this.options.scaleIntegersOnly,
                calculateYRange: function (currentHeight) {
                    var updatedRanges = helpers.calculateScaleRange(
						dataTotal(),
						currentHeight,
						this.fontSize,
						this.beginAtZero,
						this.integersOnly
					);
                    helpers.extend(this, updatedRanges);
                },
                xLabels: labels,
                font: helpers.fontString(this.options.scaleFontSize, this.options.scaleFontStyle, this.options.scaleFontFamily),
                lineWidth: this.options.scaleLineWidth,
                lineColor: this.options.scaleLineColor,
                gridLineWidth: (this.options.scaleShowGridLines) ? this.options.scaleGridLineWidth : 0,
                gridLineColor: (this.options.scaleShowGridLines) ? this.options.scaleGridLineColor : "rgba(0,0,0,0)",
                padding: (this.options.showScale) ? 0 : (this.options.barShowStroke) ? this.options.barStrokeWidth : 0,
                showLabels: this.options.scaleShowLabels,
                display: this.options.showScale
            };

            if (this.options.scaleOverride) {
                helpers.extend(scaleOptions, {
                    calculateYRange: helpers.noop,
                    steps: this.options.scaleSteps,
                    stepValue: this.options.scaleStepWidth,
                    min: this.options.scaleStartValue,
                    max: this.options.scaleStartValue + (this.options.scaleSteps * this.options.scaleStepWidth)
                });
            }

            this.scale = new this.ScaleClass(scaleOptions);
        },
        addData: function (valuesArray, label) {
            //Map the values array for each of the datasets
            helpers.each(valuesArray, function (value, datasetIndex) {
                //Add a new point for each piece of data, passing any required data to draw.
                this.datasets[datasetIndex].bars.push(new this.BarClass({
                    value: value,
                    label: label,
                    x: this.scale.calculateBarX(this.datasets.length, datasetIndex, this.scale.valuesCount + 1),
                    y: this.scale.endPoint,
                    width: this.scale.calculateBarWidth(this.datasets.length),
                    base: this.scale.endPoint,
                    strokeColor: this.datasets[datasetIndex].strokeColor,
                    fillColor: this.datasets[datasetIndex].fillColor
                }));
            }, this);

            this.scale.addXLabel(label);
            //Then re-render the chart.
            this.update();
        },
        removeData: function () {
            this.scale.removeXLabel();
            //Then re-render the chart.
            helpers.each(this.datasets, function (dataset) {
                dataset.bars.shift();
            }, this);
            this.update();
        },
        reflow: function () {
            helpers.extend(this.BarClass.prototype, {
                y: this.scale.endPoint,
                base: this.scale.endPoint
            });
            var newScaleProps = helpers.extend({
                height: this.chart.height,
                width: this.chart.width
            });
            this.scale.update(newScaleProps);
        },
        draw: function (ease) {
            var easingDecimal = ease || 1;
            this.clear();

            var ctx = this.chart.ctx;

            this.scale.draw(easingDecimal);

            //Draw all the bars for each dataset
            helpers.each(this.datasets, function (dataset, datasetIndex) {
                helpers.each(dataset.bars, function (bar, index) {
                    if (bar.hasValue()) {
                        bar.base = this.scale.endPoint;
                        //Transition then draw
                        bar.transition({
                            x: this.scale.calculateBarX(this.datasets.length, datasetIndex, index),
                            y: this.scale.calculateY(bar.value),
                            width: this.scale.calculateBarWidth(this.datasets.length)
                        }, easingDecimal).draw();
                    }
                }, this);

            }, this);
        }
    });


}).call(this);
(function () {
    "use strict";

    var root = this,
		Chart = root.Chart,
		//Cache a local reference to Chart.helpers
		helpers = Chart.helpers;

    var defaultConfig = {
        //Boolean - Whether we should show a stroke on each segment
        segmentShowStroke: true,

        //String - The colour of each segment stroke
        segmentStrokeColor: "#fff",

        //Number - The width of each segment stroke
        segmentStrokeWidth: 2,

        //The percentage of the chart that we cut out of the middle.
        percentageInnerCutout: 50,

        //Number - Amount of animation steps
        animationSteps: 100,

        //String - Animation easing effect
        animationEasing: "easeOutBounce",

        //Boolean - Whether we animate the rotation of the Doughnut
        animateRotate: true,

        //Boolean - Whether we animate scaling the Doughnut from the centre
        animateScale: false,

        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>"

    };


    Chart.Type.extend({
        //Passing in a name registers this chart in the Chart namespace
        name: "Doughnut",
        //Providing a defaults will also register the deafults in the chart namespace
        defaults: defaultConfig,
        //Initialize is fired when the chart is initialized - Data is passed in as a parameter
        //Config is automatically merged by the core of Chart.js, and is available at this.options
        initialize: function (data) {

            //Declare segments as a static property to prevent inheriting across the Chart type prototype
            this.segments = [];
            this.outerRadius = (helpers.min([this.chart.width, this.chart.height]) - this.options.segmentStrokeWidth / 2) / 2;

            this.SegmentArc = Chart.Arc.extend({
                ctx: this.chart.ctx,
                x: this.chart.width / 2,
                y: this.chart.height / 2
            });

            //Set up tooltip events on the chart
            if (this.options.showTooltips) {
                helpers.bindEvents(this, this.options.tooltipEvents, function (evt) {
                    var activeSegments = (evt.type !== 'mouseout') ? this.getSegmentsAtEvent(evt) : [];

                    helpers.each(this.segments, function (segment) {
                        segment.restore(["fillColor"]);
                    });
                    helpers.each(activeSegments, function (activeSegment) {
                        activeSegment.fillColor = activeSegment.highlightColor;
                    });
                    this.showTooltip(activeSegments);
                });
            }
            this.calculateTotal(data);

            helpers.each(data, function (datapoint, index) {
                this.addData(datapoint, index, true);
            }, this);

            this.render();
        },
        getSegmentsAtEvent: function (e) {
            var segmentsArray = [];

            var location = helpers.getRelativePosition(e);

            helpers.each(this.segments, function (segment) {
                if (segment.inRange(location.x, location.y)) segmentsArray.push(segment);
            }, this);
            return segmentsArray;
        },
        addData: function (segment, atIndex, silent) {
            var index = atIndex || this.segments.length;
            this.segments.splice(index, 0, new this.SegmentArc({
                value: segment.value,
                outerRadius: (this.options.animateScale) ? 0 : this.outerRadius,
                innerRadius: (this.options.animateScale) ? 0 : (this.outerRadius / 100) * this.options.percentageInnerCutout,
                fillColor: segment.color,
                highlightColor: segment.highlight || segment.color,
                showStroke: this.options.segmentShowStroke,
                strokeWidth: this.options.segmentStrokeWidth,
                strokeColor: this.options.segmentStrokeColor,
                startAngle: Math.PI * 1.5,
                circumference: (this.options.animateRotate) ? 0 : this.calculateCircumference(segment.value),
                label: segment.label
            }));
            if (!silent) {
                this.reflow();
                this.update();
            }
        },
        calculateCircumference: function (value) {
            return (Math.PI * 2) * (value / this.total);
        },
        calculateTotal: function (data) {
            this.total = 0;
            helpers.each(data, function (segment) {
                this.total += segment.value;
            }, this);
        },
        update: function () {
            this.calculateTotal(this.segments);

            // Reset any highlight colours before updating.
            helpers.each(this.activeElements, function (activeElement) {
                activeElement.restore(['fillColor']);
            });

            helpers.each(this.segments, function (segment) {
                segment.save();
            });
            this.render();
        },

        removeData: function (atIndex) {
            var indexToDelete = (helpers.isNumber(atIndex)) ? atIndex : this.segments.length - 1;
            this.segments.splice(indexToDelete, 1);
            this.reflow();
            this.update();
        },

        reflow: function () {
            helpers.extend(this.SegmentArc.prototype, {
                x: this.chart.width / 2,
                y: this.chart.height / 2
            });
            this.outerRadius = (helpers.min([this.chart.width, this.chart.height]) - this.options.segmentStrokeWidth / 2) / 2;
            helpers.each(this.segments, function (segment) {
                segment.update({
                    outerRadius: this.outerRadius,
                    innerRadius: (this.outerRadius / 100) * this.options.percentageInnerCutout
                });
            }, this);
        },
        draw: function (easeDecimal) {
            var animDecimal = (easeDecimal) ? easeDecimal : 1;
            this.clear();
            helpers.each(this.segments, function (segment, index) {
                segment.transition({
                    circumference: this.calculateCircumference(segment.value),
                    outerRadius: this.outerRadius,
                    innerRadius: (this.outerRadius / 100) * this.options.percentageInnerCutout
                }, animDecimal);

                segment.endAngle = segment.startAngle + segment.circumference;

                segment.draw();
                if (index === 0) {
                    segment.startAngle = Math.PI * 1.5;
                }
                //Check to see if it's the last segment, if not get the next and update the start angle
                if (index < this.segments.length - 1) {
                    this.segments[index + 1].startAngle = segment.endAngle;
                }
            }, this);

        }
    });

    Chart.types.Doughnut.extend({
        name: "Pie",
        defaults: helpers.merge(defaultConfig, { percentageInnerCutout: 0 })
    });

}).call(this);
(function () {
    "use strict";

    var root = this,
		Chart = root.Chart,
		helpers = Chart.helpers;

    var defaultConfig = {

        ///Boolean - Whether grid lines are shown across the chart
        scaleShowGridLines: true,

        //String - Colour of the grid lines
        scaleGridLineColor: "rgba(0,0,0,.05)",

        //Number - Width of the grid lines
        scaleGridLineWidth: 1,

        //Boolean - Whether the line is curved between points
        bezierCurve: true,

        //Number - Tension of the bezier curve between points
        bezierCurveTension: 0.4,

        //Boolean - Whether to show a dot for each point
        pointDot: true,

        //Number - Radius of each point dot in pixels
        pointDotRadius: 4,

        //Number - Pixel width of point dot stroke
        pointDotStrokeWidth: 1,

        //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
        pointHitDetectionRadius: 20,

        //Boolean - Whether to show a stroke for datasets
        datasetStroke: true,

        //Number - Pixel width of dataset stroke
        datasetStrokeWidth: 2,

        //Boolean - Whether to fill the dataset with a colour
        datasetFill: true,

        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>"

    };


    Chart.Type.extend({
        name: "Line",
        defaults: defaultConfig,
        initialize: function (data) {
            //Declare the extension of the default point, to cater for the options passed in to the constructor
            this.PointClass = Chart.Point.extend({
                strokeWidth: this.options.pointDotStrokeWidth,
                radius: this.options.pointDotRadius,
                display: this.options.pointDot,
                hitDetectionRadius: this.options.pointHitDetectionRadius,
                ctx: this.chart.ctx,
                inRange: function (mouseX) {
                    return (Math.pow(mouseX - this.x, 2) < Math.pow(this.radius + this.hitDetectionRadius, 2));
                }
            });

            this.datasets = [];

            //Set up tooltip events on the chart
            if (this.options.showTooltips) {
                helpers.bindEvents(this, this.options.tooltipEvents, function (evt) {
                    var activePoints = (evt.type !== 'mouseout') ? this.getPointsAtEvent(evt) : [];
                    this.eachPoints(function (point) {
                        point.restore(['fillColor', 'strokeColor']);
                    });
                    helpers.each(activePoints, function (activePoint) {
                        activePoint.fillColor = activePoint.highlightFill;
                        activePoint.strokeColor = activePoint.highlightStroke;
                    });
                    this.showTooltip(activePoints);
                });
            }

            //Iterate through each of the datasets, and build this into a property of the chart
            helpers.each(data.datasets, function (dataset) {

                var datasetObject = {
                    label: dataset.label || null,
                    fillColor: dataset.fillColor,
                    strokeColor: dataset.strokeColor,
                    pointColor: dataset.pointColor,
                    pointStrokeColor: dataset.pointStrokeColor,
                    points: []
                };

                this.datasets.push(datasetObject);


                helpers.each(dataset.data, function (dataPoint, index) {
                    //Add a new point for each piece of data, passing any required data to draw.
                    datasetObject.points.push(new this.PointClass({
                        value: dataPoint,
                        label: data.labels[index],
                        datasetLabel: dataset.label,
                        strokeColor: dataset.pointStrokeColor,
                        fillColor: dataset.pointColor,
                        highlightFill: dataset.pointHighlightFill || dataset.pointColor,
                        highlightStroke: dataset.pointHighlightStroke || dataset.pointStrokeColor
                    }));
                }, this);

                this.buildScale(data.labels);


                this.eachPoints(function (point, index) {
                    helpers.extend(point, {
                        x: this.scale.calculateX(index),
                        y: this.scale.endPoint
                    });
                    point.save();
                }, this);

            }, this);


            this.render();
        },
        update: function () {
            this.scale.update();
            // Reset any highlight colours before updating.
            helpers.each(this.activeElements, function (activeElement) {
                activeElement.restore(['fillColor', 'strokeColor']);
            });
            this.eachPoints(function (point) {
                point.save();
            });
            this.render();
        },
        eachPoints: function (callback) {
            helpers.each(this.datasets, function (dataset) {
                helpers.each(dataset.points, callback, this);
            }, this);
        },
        getPointsAtEvent: function (e) {
            var pointsArray = [],
				eventPosition = helpers.getRelativePosition(e);
            helpers.each(this.datasets, function (dataset) {
                helpers.each(dataset.points, function (point) {
                    if (point.inRange(eventPosition.x, eventPosition.y)) pointsArray.push(point);
                });
            }, this);
            return pointsArray;
        },
        buildScale: function (labels) {
            var self = this;

            var dataTotal = function () {
                var values = [];
                self.eachPoints(function (point) {
                    values.push(point.value);
                });

                return values;
            };

            var scaleOptions = {
                templateString: this.options.scaleLabel,
                height: this.chart.height,
                width: this.chart.width,
                ctx: this.chart.ctx,
                textColor: this.options.scaleFontColor,
                fontSize: this.options.scaleFontSize,
                fontStyle: this.options.scaleFontStyle,
                fontFamily: this.options.scaleFontFamily,
                valuesCount: labels.length,
                beginAtZero: this.options.scaleBeginAtZero,
                integersOnly: this.options.scaleIntegersOnly,
                calculateYRange: function (currentHeight) {
                    var updatedRanges = helpers.calculateScaleRange(
						dataTotal(),
						currentHeight,
						this.fontSize,
						this.beginAtZero,
						this.integersOnly
					);
                    helpers.extend(this, updatedRanges);
                },
                xLabels: labels,
                font: helpers.fontString(this.options.scaleFontSize, this.options.scaleFontStyle, this.options.scaleFontFamily),
                lineWidth: this.options.scaleLineWidth,
                lineColor: this.options.scaleLineColor,
                gridLineWidth: (this.options.scaleShowGridLines) ? this.options.scaleGridLineWidth : 0,
                gridLineColor: (this.options.scaleShowGridLines) ? this.options.scaleGridLineColor : "rgba(0,0,0,0)",
                padding: (this.options.showScale) ? 0 : this.options.pointDotRadius + this.options.pointDotStrokeWidth,
                showLabels: this.options.scaleShowLabels,
                display: this.options.showScale
            };

            if (this.options.scaleOverride) {
                helpers.extend(scaleOptions, {
                    calculateYRange: helpers.noop,
                    steps: this.options.scaleSteps,
                    stepValue: this.options.scaleStepWidth,
                    min: this.options.scaleStartValue,
                    max: this.options.scaleStartValue + (this.options.scaleSteps * this.options.scaleStepWidth)
                });
            }


            this.scale = new Chart.Scale(scaleOptions);
        },
        addData: function (valuesArray, label) {
            //Map the values array for each of the datasets

            helpers.each(valuesArray, function (value, datasetIndex) {
                //Add a new point for each piece of data, passing any required data to draw.
                this.datasets[datasetIndex].points.push(new this.PointClass({
                    value: value,
                    label: label,
                    x: this.scale.calculateX(this.scale.valuesCount + 1),
                    y: this.scale.endPoint,
                    strokeColor: this.datasets[datasetIndex].pointStrokeColor,
                    fillColor: this.datasets[datasetIndex].pointColor
                }));
            }, this);

            this.scale.addXLabel(label);
            //Then re-render the chart.
            this.update();
        },
        removeData: function () {
            this.scale.removeXLabel();
            //Then re-render the chart.
            helpers.each(this.datasets, function (dataset) {
                dataset.points.shift();
            }, this);
            this.update();
        },
        reflow: function () {
            var newScaleProps = helpers.extend({
                height: this.chart.height,
                width: this.chart.width
            });
            this.scale.update(newScaleProps);
        },
        draw: function (ease) {
            var easingDecimal = ease || 1;
            this.clear();

            var ctx = this.chart.ctx;

            // Some helper methods for getting the next/prev points
            var hasValue = function (item) {
                return item.value !== null;
            },
			nextPoint = function (point, collection, index) {
			    return helpers.findNextWhere(collection, hasValue, index) || point;
			},
			previousPoint = function (point, collection, index) {
			    return helpers.findPreviousWhere(collection, hasValue, index) || point;
			};

            this.scale.draw(easingDecimal);


            helpers.each(this.datasets, function (dataset) {
                var pointsWithValues = helpers.where(dataset.points, hasValue);

                //Transition each point first so that the line and point drawing isn't out of sync
                //We can use this extra loop to calculate the control points of this dataset also in this loop

                helpers.each(dataset.points, function (point, index) {
                    if (point.hasValue()) {
                        point.transition({
                            y: this.scale.calculateY(point.value),
                            x: this.scale.calculateX(index)
                        }, easingDecimal);
                    }
                }, this);


                // Control points need to be calculated in a seperate loop, because we need to know the current x/y of the point
                // This would cause issues when there is no animation, because the y of the next point would be 0, so beziers would be skewed
                if (this.options.bezierCurve) {
                    helpers.each(pointsWithValues, function (point, index) {
                        var tension = (index > 0 && index < pointsWithValues.length - 1) ? this.options.bezierCurveTension : 0;
                        point.controlPoints = helpers.splineCurve(
							previousPoint(point, pointsWithValues, index),
							point,
							nextPoint(point, pointsWithValues, index),
							tension
						);

                        // Prevent the bezier going outside of the bounds of the graph

                        // Cap puter bezier handles to the upper/lower scale bounds
                        if (point.controlPoints.outer.y > this.scale.endPoint) {
                            point.controlPoints.outer.y = this.scale.endPoint;
                        }
                        else if (point.controlPoints.outer.y < this.scale.startPoint) {
                            point.controlPoints.outer.y = this.scale.startPoint;
                        }

                        // Cap inner bezier handles to the upper/lower scale bounds
                        if (point.controlPoints.inner.y > this.scale.endPoint) {
                            point.controlPoints.inner.y = this.scale.endPoint;
                        }
                        else if (point.controlPoints.inner.y < this.scale.startPoint) {
                            point.controlPoints.inner.y = this.scale.startPoint;
                        }
                    }, this);
                }


                //Draw the line between all the points
                ctx.lineWidth = this.options.datasetStrokeWidth;
                ctx.strokeStyle = dataset.strokeColor;
                ctx.beginPath();

                helpers.each(pointsWithValues, function (point, index) {
                    if (index === 0) {
                        ctx.moveTo(point.x, point.y);
                    }
                    else {
                        if (this.options.bezierCurve) {
                            var previous = previousPoint(point, pointsWithValues, index);

                            ctx.bezierCurveTo(
								previous.controlPoints.outer.x,
								previous.controlPoints.outer.y,
								point.controlPoints.inner.x,
								point.controlPoints.inner.y,
								point.x,
								point.y
							);
                        }
                        else {
                            ctx.lineTo(point.x, point.y);
                        }
                    }
                }, this);

                ctx.stroke();

                if (this.options.datasetFill && pointsWithValues.length > 0) {
                    //Round off the line by going to the base of the chart, back to the start, then fill.
                    ctx.lineTo(pointsWithValues[pointsWithValues.length - 1].x, this.scale.endPoint);
                    ctx.lineTo(pointsWithValues[0].x, this.scale.endPoint);
                    ctx.fillStyle = dataset.fillColor;
                    ctx.closePath();
                    ctx.fill();
                }

                //Now draw the points over the line
                //A little inefficient double looping, but better than the line
                //lagging behind the point positions
                helpers.each(pointsWithValues, function (point) {
                    point.draw();
                });
            }, this);
        }
    });


}).call(this);
(function () {
    "use strict";

    var root = this,
		Chart = root.Chart,
		//Cache a local reference to Chart.helpers
		helpers = Chart.helpers;

    var defaultConfig = {
        //Boolean - Show a backdrop to the scale label
        scaleShowLabelBackdrop: true,

        //String - The colour of the label backdrop
        scaleBackdropColor: "rgba(255,255,255,0.75)",

        // Boolean - Whether the scale should begin at zero
        scaleBeginAtZero: true,

        //Number - The backdrop padding above & below the label in pixels
        scaleBackdropPaddingY: 2,

        //Number - The backdrop padding to the side of the label in pixels
        scaleBackdropPaddingX: 2,

        //Boolean - Show line for each value in the scale
        scaleShowLine: true,

        //Boolean - Stroke a line around each segment in the chart
        segmentShowStroke: true,

        //String - The colour of the stroke on each segement.
        segmentStrokeColor: "#fff",

        //Number - The width of the stroke value in pixels
        segmentStrokeWidth: 2,

        //Number - Amount of animation steps
        animationSteps: 100,

        //String - Animation easing effect.
        animationEasing: "easeOutBounce",

        //Boolean - Whether to animate the rotation of the chart
        animateRotate: true,

        //Boolean - Whether to animate scaling the chart from the centre
        animateScale: false,

        //String - A legend template
        legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><%=segments[i].label%><%}%></li><%}%></ul>"
    };


    Chart.Type.extend({
        //Passing in a name registers this chart in the Chart namespace
        name: "PolarArea",
        //Providing a defaults will also register the deafults in the chart namespace
        defaults: defaultConfig,
        //Initialize is fired when the chart is initialized - Data is passed in as a parameter
        //Config is automatically merged by the core of Chart.js, and is available at this.options
        initialize: function (data) {
            this.segments = [];
            //Declare segment class as a chart instance specific class, so it can share props for this instance
            this.SegmentArc = Chart.Arc.extend({
                showStroke: this.options.segmentShowStroke,
                strokeWidth: this.options.segmentStrokeWidth,
                strokeColor: this.options.segmentStrokeColor,
                ctx: this.chart.ctx,
                innerRadius: 0,
                x: this.chart.width / 2,
                y: this.chart.height / 2
            });
            this.scale = new Chart.RadialScale({
                display: this.options.showScale,
                fontStyle: this.options.scaleFontStyle,
                fontSize: this.options.scaleFontSize,
                fontFamily: this.options.scaleFontFamily,
                fontColor: this.options.scaleFontColor,
                showLabels: this.options.scaleShowLabels,
                showLabelBackdrop: this.options.scaleShowLabelBackdrop,
                backdropColor: this.options.scaleBackdropColor,
                backdropPaddingY: this.options.scaleBackdropPaddingY,
                backdropPaddingX: this.options.scaleBackdropPaddingX,
                lineWidth: (this.options.scaleShowLine) ? this.options.scaleLineWidth : 0,
                lineColor: this.options.scaleLineColor,
                lineArc: true,
                width: this.chart.width,
                height: this.chart.height,
                xCenter: this.chart.width / 2,
                yCenter: this.chart.height / 2,
                ctx: this.chart.ctx,
                templateString: this.options.scaleLabel,
                valuesCount: data.length
            });

            this.updateScaleRange(data);

            this.scale.update();

            helpers.each(data, function (segment, index) {
                this.addData(segment, index, true);
            }, this);

            //Set up tooltip events on the chart
            if (this.options.showTooltips) {
                helpers.bindEvents(this, this.options.tooltipEvents, function (evt) {
                    var activeSegments = (evt.type !== 'mouseout') ? this.getSegmentsAtEvent(evt) : [];
                    helpers.each(this.segments, function (segment) {
                        segment.restore(["fillColor"]);
                    });
                    helpers.each(activeSegments, function (activeSegment) {
                        activeSegment.fillColor = activeSegment.highlightColor;
                    });
                    this.showTooltip(activeSegments);
                });
            }

            this.render();
        },
        getSegmentsAtEvent: function (e) {
            var segmentsArray = [];

            var location = helpers.getRelativePosition(e);

            helpers.each(this.segments, function (segment) {
                if (segment.inRange(location.x, location.y)) segmentsArray.push(segment);
            }, this);
            return segmentsArray;
        },
        addData: function (segment, atIndex, silent) {
            var index = atIndex || this.segments.length;

            this.segments.splice(index, 0, new this.SegmentArc({
                fillColor: segment.color,
                highlightColor: segment.highlight || segment.color,
                label: segment.label,
                value: segment.value,
                outerRadius: (this.options.animateScale) ? 0 : this.scale.calculateCenterOffset(segment.value),
                circumference: (this.options.animateRotate) ? 0 : this.scale.getCircumference(),
                startAngle: Math.PI * 1.5
            }));
            if (!silent) {
                this.reflow();
                this.update();
            }
        },
        removeData: function (atIndex) {
            var indexToDelete = (helpers.isNumber(atIndex)) ? atIndex : this.segments.length - 1;
            this.segments.splice(indexToDelete, 1);
            this.reflow();
            this.update();
        },
        calculateTotal: function (data) {
            this.total = 0;
            helpers.each(data, function (segment) {
                this.total += segment.value;
            }, this);
            this.scale.valuesCount = this.segments.length;
        },
        updateScaleRange: function (datapoints) {
            var valuesArray = [];
            helpers.each(datapoints, function (segment) {
                valuesArray.push(segment.value);
            });

            var scaleSizes = (this.options.scaleOverride) ?
				{
				    steps: this.options.scaleSteps,
				    stepValue: this.options.scaleStepWidth,
				    min: this.options.scaleStartValue,
				    max: this.options.scaleStartValue + (this.options.scaleSteps * this.options.scaleStepWidth)
				} :
				helpers.calculateScaleRange(
					valuesArray,
					helpers.min([this.chart.width, this.chart.height]) / 2,
					this.options.scaleFontSize,
					this.options.scaleBeginAtZero,
					this.options.scaleIntegersOnly
				);

            helpers.extend(
				this.scale,
				scaleSizes,
				{
				    size: helpers.min([this.chart.width, this.chart.height]),
				    xCenter: this.chart.width / 2,
				    yCenter: this.chart.height / 2
				}
			);

        },
        update: function () {
            this.calculateTotal(this.segments);

            helpers.each(this.segments, function (segment) {
                segment.save();
            });
            this.render();
        },
        reflow: function () {
            helpers.extend(this.SegmentArc.prototype, {
                x: this.chart.width / 2,
                y: this.chart.height / 2
            });
            this.updateScaleRange(this.segments);
            this.scale.update();

            helpers.extend(this.scale, {
                xCenter: this.chart.width / 2,
                yCenter: this.chart.height / 2
            });

            helpers.each(this.segments, function (segment) {
                segment.update({
                    outerRadius: this.scale.calculateCenterOffset(segment.value)
                });
            }, this);

        },
        draw: function (ease) {
            var easingDecimal = ease || 1;
            //Clear & draw the canvas
            this.clear();
            helpers.each(this.segments, function (segment, index) {
                segment.transition({
                    circumference: this.scale.getCircumference(),
                    outerRadius: this.scale.calculateCenterOffset(segment.value)
                }, easingDecimal);

                segment.endAngle = segment.startAngle + segment.circumference;

                // If we've removed the first segment we need to set the first one to
                // start at the top.
                if (index === 0) {
                    segment.startAngle = Math.PI * 1.5;
                }

                //Check to see if it's the last segment, if not get the next and update the start angle
                if (index < this.segments.length - 1) {
                    this.segments[index + 1].startAngle = segment.endAngle;
                }
                segment.draw();
            }, this);
            this.scale.draw();
        }
    });

}).call(this);
(function () {
    "use strict";

    var root = this,
		Chart = root.Chart,
		helpers = Chart.helpers;



    Chart.Type.extend({
        name: "Radar",
        defaults: {
            //Boolean - Whether to show lines for each scale point
            scaleShowLine: true,

            //Boolean - Whether we show the angle lines out of the radar
            angleShowLineOut: true,

            //Boolean - Whether to show labels on the scale
            scaleShowLabels: false,

            // Boolean - Whether the scale should begin at zero
            scaleBeginAtZero: true,

            //String - Colour of the angle line
            angleLineColor: "rgba(0,0,0,.1)",

            //Number - Pixel width of the angle line
            angleLineWidth: 1,

            //String - Point label font declaration
            pointLabelFontFamily: "'Arial'",

            //String - Point label font weight
            pointLabelFontStyle: "normal",

            //Number - Point label font size in pixels
            pointLabelFontSize: 10,

            //String - Point label font colour
            pointLabelFontColor: "#666",

            //Boolean - Whether to show a dot for each point
            pointDot: true,

            //Number - Radius of each point dot in pixels
            pointDotRadius: 3,

            //Number - Pixel width of point dot stroke
            pointDotStrokeWidth: 1,

            //Number - amount extra to add to the radius to cater for hit detection outside the drawn point
            pointHitDetectionRadius: 20,

            //Boolean - Whether to show a stroke for datasets
            datasetStroke: true,

            //Number - Pixel width of dataset stroke
            datasetStrokeWidth: 2,

            //Boolean - Whether to fill the dataset with a colour
            datasetFill: true,

            //String - A legend template
            legendTemplate: "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<datasets.length; i++){%><li><span style=\"background-color:<%=datasets[i].strokeColor%>\"></span><%if(datasets[i].label){%><%=datasets[i].label%><%}%></li><%}%></ul>"

        },

        initialize: function (data) {
            this.PointClass = Chart.Point.extend({
                strokeWidth: this.options.pointDotStrokeWidth,
                radius: this.options.pointDotRadius,
                display: this.options.pointDot,
                hitDetectionRadius: this.options.pointHitDetectionRadius,
                ctx: this.chart.ctx
            });

            this.datasets = [];

            this.buildScale(data);

            //Set up tooltip events on the chart
            if (this.options.showTooltips) {
                helpers.bindEvents(this, this.options.tooltipEvents, function (evt) {
                    var activePointsCollection = (evt.type !== 'mouseout') ? this.getPointsAtEvent(evt) : [];

                    this.eachPoints(function (point) {
                        point.restore(['fillColor', 'strokeColor']);
                    });
                    helpers.each(activePointsCollection, function (activePoint) {
                        activePoint.fillColor = activePoint.highlightFill;
                        activePoint.strokeColor = activePoint.highlightStroke;
                    });

                    this.showTooltip(activePointsCollection);
                });
            }

            //Iterate through each of the datasets, and build this into a property of the chart
            helpers.each(data.datasets, function (dataset) {

                var datasetObject = {
                    label: dataset.label || null,
                    fillColor: dataset.fillColor,
                    strokeColor: dataset.strokeColor,
                    pointColor: dataset.pointColor,
                    pointStrokeColor: dataset.pointStrokeColor,
                    points: []
                };

                this.datasets.push(datasetObject);

                helpers.each(dataset.data, function (dataPoint, index) {
                    //Add a new point for each piece of data, passing any required data to draw.
                    var pointPosition;
                    if (!this.scale.animation) {
                        pointPosition = this.scale.getPointPosition(index, this.scale.calculateCenterOffset(dataPoint));
                    }
                    datasetObject.points.push(new this.PointClass({
                        value: dataPoint,
                        label: data.labels[index],
                        datasetLabel: dataset.label,
                        x: (this.options.animation) ? this.scale.xCenter : pointPosition.x,
                        y: (this.options.animation) ? this.scale.yCenter : pointPosition.y,
                        strokeColor: dataset.pointStrokeColor,
                        fillColor: dataset.pointColor,
                        highlightFill: dataset.pointHighlightFill || dataset.pointColor,
                        highlightStroke: dataset.pointHighlightStroke || dataset.pointStrokeColor
                    }));
                }, this);

            }, this);

            this.render();
        },
        eachPoints: function (callback) {
            helpers.each(this.datasets, function (dataset) {
                helpers.each(dataset.points, callback, this);
            }, this);
        },

        getPointsAtEvent: function (evt) {
            var mousePosition = helpers.getRelativePosition(evt),
				fromCenter = helpers.getAngleFromPoint({
				    x: this.scale.xCenter,
				    y: this.scale.yCenter
				}, mousePosition);

            var anglePerIndex = (Math.PI * 2) / this.scale.valuesCount,
				pointIndex = Math.round((fromCenter.angle - Math.PI * 1.5) / anglePerIndex),
				activePointsCollection = [];

            // If we're at the top, make the pointIndex 0 to get the first of the array.
            if (pointIndex >= this.scale.valuesCount || pointIndex < 0) {
                pointIndex = 0;
            }

            if (fromCenter.distance <= this.scale.drawingArea) {
                helpers.each(this.datasets, function (dataset) {
                    activePointsCollection.push(dataset.points[pointIndex]);
                });
            }

            return activePointsCollection;
        },

        buildScale: function (data) {
            this.scale = new Chart.RadialScale({
                display: this.options.showScale,
                fontStyle: this.options.scaleFontStyle,
                fontSize: this.options.scaleFontSize,
                fontFamily: this.options.scaleFontFamily,
                fontColor: this.options.scaleFontColor,
                showLabels: this.options.scaleShowLabels,
                showLabelBackdrop: this.options.scaleShowLabelBackdrop,
                backdropColor: this.options.scaleBackdropColor,
                backdropPaddingY: this.options.scaleBackdropPaddingY,
                backdropPaddingX: this.options.scaleBackdropPaddingX,
                lineWidth: (this.options.scaleShowLine) ? this.options.scaleLineWidth : 0,
                lineColor: this.options.scaleLineColor,
                angleLineColor: this.options.angleLineColor,
                angleLineWidth: (this.options.angleShowLineOut) ? this.options.angleLineWidth : 0,
                // Point labels at the edge of each line
                pointLabelFontColor: this.options.pointLabelFontColor,
                pointLabelFontSize: this.options.pointLabelFontSize,
                pointLabelFontFamily: this.options.pointLabelFontFamily,
                pointLabelFontStyle: this.options.pointLabelFontStyle,
                height: this.chart.height,
                width: this.chart.width,
                xCenter: this.chart.width / 2,
                yCenter: this.chart.height / 2,
                ctx: this.chart.ctx,
                templateString: this.options.scaleLabel,
                labels: data.labels,
                valuesCount: data.datasets[0].data.length
            });

            this.scale.setScaleSize();
            this.updateScaleRange(data.datasets);
            this.scale.buildYLabels();
        },
        updateScaleRange: function (datasets) {
            var valuesArray = (function () {
                var totalDataArray = [];
                helpers.each(datasets, function (dataset) {
                    if (dataset.data) {
                        totalDataArray = totalDataArray.concat(dataset.data);
                    }
                    else {
                        helpers.each(dataset.points, function (point) {
                            totalDataArray.push(point.value);
                        });
                    }
                });
                return totalDataArray;
            })();


            var scaleSizes = (this.options.scaleOverride) ?
				{
				    steps: this.options.scaleSteps,
				    stepValue: this.options.scaleStepWidth,
				    min: this.options.scaleStartValue,
				    max: this.options.scaleStartValue + (this.options.scaleSteps * this.options.scaleStepWidth)
				} :
				helpers.calculateScaleRange(
					valuesArray,
					helpers.min([this.chart.width, this.chart.height]) / 2,
					this.options.scaleFontSize,
					this.options.scaleBeginAtZero,
					this.options.scaleIntegersOnly
				);

            helpers.extend(
				this.scale,
				scaleSizes
			);

        },
        addData: function (valuesArray, label) {
            //Map the values array for each of the datasets
            this.scale.valuesCount++;
            helpers.each(valuesArray, function (value, datasetIndex) {
                var pointPosition = this.scale.getPointPosition(this.scale.valuesCount, this.scale.calculateCenterOffset(value));
                this.datasets[datasetIndex].points.push(new this.PointClass({
                    value: value,
                    label: label,
                    x: pointPosition.x,
                    y: pointPosition.y,
                    strokeColor: this.datasets[datasetIndex].pointStrokeColor,
                    fillColor: this.datasets[datasetIndex].pointColor
                }));
            }, this);

            this.scale.labels.push(label);

            this.reflow();

            this.update();
        },
        removeData: function () {
            this.scale.valuesCount--;
            this.scale.labels.shift();
            helpers.each(this.datasets, function (dataset) {
                dataset.points.shift();
            }, this);
            this.reflow();
            this.update();
        },
        update: function () {
            this.eachPoints(function (point) {
                point.save();
            });
            this.reflow();
            this.render();
        },
        reflow: function () {
            helpers.extend(this.scale, {
                width: this.chart.width,
                height: this.chart.height,
                size: helpers.min([this.chart.width, this.chart.height]),
                xCenter: this.chart.width / 2,
                yCenter: this.chart.height / 2
            });
            this.updateScaleRange(this.datasets);
            this.scale.setScaleSize();
            this.scale.buildYLabels();
        },
        draw: function (ease) {
            var easeDecimal = ease || 1,
				ctx = this.chart.ctx;
            this.clear();
            this.scale.draw();

            helpers.each(this.datasets, function (dataset) {

                //Transition each point first so that the line and point drawing isn't out of sync
                helpers.each(dataset.points, function (point, index) {
                    if (point.hasValue()) {
                        point.transition(this.scale.getPointPosition(index, this.scale.calculateCenterOffset(point.value)), easeDecimal);
                    }
                }, this);



                //Draw the line between all the points
                ctx.lineWidth = this.options.datasetStrokeWidth;
                ctx.strokeStyle = dataset.strokeColor;
                ctx.beginPath();
                helpers.each(dataset.points, function (point, index) {
                    if (index === 0) {
                        ctx.moveTo(point.x, point.y);
                    }
                    else {
                        ctx.lineTo(point.x, point.y);
                    }
                }, this);
                ctx.closePath();
                ctx.stroke();

                ctx.fillStyle = dataset.fillColor;
                ctx.fill();

                //Now draw the points over the line
                //A little inefficient double looping, but better than the line
                //lagging behind the point positions
                helpers.each(dataset.points, function (point) {
                    if (point.hasValue()) {
                        point.draw();
                    }
                });

            }, this);

        }

    });





}).call(this);

///#source 1 1 /Scripts/Tool/Filters.js
/* Filter v1.1.1.0         */
/* Created By: D.J. Enzyme */
/* Creation Date: 20141229 */
/* Modified Date: 20150120 */

/* global FindHeader */
/* global JTableFilter */
/* jshint unused: false*/

/// <reference path="jTable.js" />

var FILTER_colors = [
    "#FFFFB0",
    "#B0B0FF",
    "#68C473",
    "#C468B9",
    "#6669BA",
    "#BAB766"
];
var FILTER_modalIndex = -1;
var FILTER_VERSION = '1.1.1.0';

function Filter(table, modalId, minimal) {
    /// <signature>
    /// <summary>Creates a Filter object.</summary>
    /// <returns type="Filter" />
    /// <param name="table" type="JTable">The JTable the sorting will apply too.</param>
    /// <param name="modalId" type="String">The modal Id of the sorting. default: index sorting</param>
    /// <field name="Table" type="JTable">The JTable that is being used for the filtering.</field>
    /// <field name="Modal" type="jQuery">The modal that is being used to edit filtering.</field>
    /// </signature>
    if (typeof (modalId) === 'undefined') {
        modalId = null;
    }
    var min_layout = false;
    if (typeof (minimal) === 'boolean') {
        min_layout = minimal;
    }
    var filterIndex = -1;
    var colorIndex = -1;
    var t_holder = this;
    this.Table = table;
    this.Modal = null;
    if (!min_layout) {
        this.Modal = FILTER_GenerateModal(modalId);
    }
    this.GenerateActing = function (value) {
        /// <signature>
        /// <summary>Generating the acting on input.</summary>
        /// <param name="value" type="String">The value to prefil.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="filter-actingon form-control"><option>Make a Selection</option>';
        for (var i = 0; i < this.Table.Headers.length; i++) {
            html += '<option value="' + this.Table.Headers[i].Id + '"' + (this.Table.Headers[i].Id === value ? ' selected="selected"' : '') + '>' + this.Table.Headers[i].Label + '</option>';
        }
        html += '</select>';
        return html;
    };
    this.Generate = function () {
        /// <signature>
        /// <summary>Generates the sortings to place in the table</summary>
        /// </signature>
        if (this.Modal === null) {
            return;
        }
        this.Modal.find('.table-filter-body').html('');
        filterIndex = -1;
        var html = '';
        for (var i = 0; i < this.Table.Filters.length; i++) {
            filterIndex++;
            var t_filter = this.Table.Filters[i];
            var t_header = FindHeader(this.Table, t_filter.ActingOn);
            html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupNext-value" />';
            html += '<input type="checkbox" class="filter-groupNext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            if (i === 0) {
                html += '<input type="hidden" class="filter-link" value="none" />';
            } else {
                html += '<select class="filter-link form-control">';
                html += '<option value="and"' + (t_filter.Link === 'and' ? ' selected="selected"' : '') + '>And</option>';
                html += '<option value="or"' + (t_filter.Link === 'or' ? ' selected="selected"' : '') + '>Or</option>';
                html += '</select>';
            }
            html += '</td>';
            html += '<td>';
            html += this.GenerateActing(t_filter.ActingOn);
            html += '</td>';
            html += '</td>';
            html += '<td>';
            html += '<select class="filter-test form-control">';
            html += '</select>';
            html += '</td>';
            html += '<td>';
            html += this.GenerateValueInput(t_header, t_filter.Value);
            html += '</td>';
            html += '</tr>';
            this.Modal.find('.table-filter-body').append(html);
            var testHtml = "";
            if (t_header.Type === 'itemParent') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option>';
            } else if (t_header.Type === 'multipleSelection') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="in"' + (t_filter.Test === 'in' ? ' selected="selected"' : '') + '>In</option><option value="notin"' + (t_filter.Test === 'notin' ? ' selected="selected"' : '') + '>Not In</option>';
            } else if (t_header.Type === 'number' || t_header.Type.indexOf('rating') === 0) {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else if (t_header.Type === 'date') {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            } else {
                testHtml = '<option value="=="' + (t_filter.Test === '==' ? ' selected="selected"' : '') + '>Equals</option><option value="!="' + (t_filter.Test === '!=' ? ' selected="selected"' : '') + '>Not Equals</option><option value="^="' + (t_filter.Test === '^=' ? ' selected="selected"' : '') + '>Starts With</option><option value="!^="' + (t_filter.Test === '!^=' ? ' selected="selected"' : '') + '>Does Not Start With</option><option value="$="' + (t_filter.Test === '$=' ? ' selected="selected"' : '') + '>Ends With</option><option value="!$="' + (t_filter.Test === '!$=' ? ' selected="selected"' : '') + '>Does Not End With</option><option value="=rgx="' + (t_filter.Test === '=rgx=' ? ' selected="selected"' : '') + '>Regex Match</option><option value="!=rgx="' + (t_filter.Test === '!=rgx=' ? ' selected="selected"' : '') + '>Regex Mismatch</option><option value="*="' + (t_filter.Test === '*=' ? ' selected="selected"' : '') + '>Contains</option><option value="!*="' + (t_filter.Test === '!*=' ? ' selected="selected"' : '') + '>Does Not Contain</option><option value=">"' + (t_filter.Test === '>' ? ' selected="selected"' : '') + '>Greater Than</option><option value=">="' + (t_filter.Test === '>=' ? ' selected="selected"' : '') + '>Greater Than or Equal</option><option value="<"' + (t_filter.Test === '<' ? ' selected="selected"' : '') + '>Less Than</option><option value="<="' + (t_filter.Test === '<=' ? ' selected="selected"' : '') + '>Less Than or Equal</option>';
            }
            $('tr[data-index="' + filterIndex + '"] .filter-test').html(testHtml);
        }
        this.Modal.find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            t_holder.ActingOnChange(tr);
        });
        this.Modal.find('.filter-delete').on('click', function (e) {
            t_holder.DeleteStatement($(this).closest('tr'), e);
        });

    };
    this.DeleteStatement = function (tr, event) {
        /// <signature>
        /// <summary>Deletes a statement.</summary>
        /// <param name="tr" type="jQuery">The tr html tag to remove.</param>
        /// <param name="event" type="JavascriptEvent">The mouse event.</param>
        /// </signature>
        event.preventDefault();
        if (this.Modal === null) {
            return;
        }
        tr.remove();
        this.Modal.find('.filter').css('background-color', 'inherit');
        this.Modal.find('.filter').find('input[name$="GroupNext"]').val('false');
        colorIndex = -1;
        this.Modal.find('tr.filter').each(function (i) {
            filterIndex = i;
            $(this).find('input').each(function (j) {
                var name = $(this).attr('name');
                if (typeof (name) == 'undefined') {
                    return;
                }
                name = name.replace(/\[\d+\]/, '[' + filterIndex + ']');
                $(this).attr('name', name);
            });
            $(this).find('input[name$="Order"]').val(filterIndex + 1);
        });
        filterIndex--;
    };
    this.AddStatement = function () {
        /// <signature>
        /// <summary>Adds a new statement to the table.</summary>
        /// </signature>
        if (this.Modal === null) {
            return;
        }
        filterIndex++;
        var filter = new JTableFilter();
        var header = FindHeader(this.Table, filter.ActingOn);
        var html = '<tr class="filter" data-index="' + filterIndex + '"><td><input type="hidden" class="filter-groupNext-value" />';
        html += '<input type="checkbox" class="filter-groupNext" /> <a href="#" class="filter-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        if (this.Modal.find('.filter').length === 0) {
            html += '<input type="hidden" class="filter-link" value="none" />';
        } else {
            html += '<select class="filter-link form-control">';
            html += '<option value="and">And</option>';
            html += '<option value="or">Or</option>';
            html += '</select>';
        }
        html += '</td>';
        html += '<td>';
        html += this.GenerateActing();
        html += '</td>';
        html += '</td>';
        html += '<td>';
        html += '<select class="filter-test form-control">';
        html += '</select>';
        html += '</td>';
        html += '<td>';
        html += this.GenerateValueInput(header);
        html += '</td>';
        html += '</tr>';
        this.Modal.find('.table-filter-body').append(html);
        this.Modal.find('.filter[data-index="' + filterIndex + '"]').find('.filter-actingon').on('change', function () {
            var tr = $(this).closest('tr');
            var index = parseInt(tr.attr('data-index'));
            t_holder.ActingOnChange(tr);
        });
        this.Modal.find('.filter[data-index="' + filterIndex + '"]').find('.filter-delete').on('click', function (e) {
            var tr = $(this).closest('tr');
            t_holder.DeleteStatement(tr, e);
        });
    };
    this.Group = function (e) {
        e.preventDefault();
        if (this.Modal === null) {
            return;
        }
        var t_checkedAmount = this.Modal.find('.filter-groupNext:checked').length;
        if (t_checkedAmount > 1) {
            colorIndex++;
        } else {
            return;
        }
        if (colorIndex >= FILTER_colors.length) {
            colorIndex = 0;
        }
        var found = false;
        var skipped = false;
        var lastItem = null;
        var count = 0;
        var changed = [];
        this.Modal.find('.filter-groupNext').each(function (i) {
            if (found) {
                if (!($(this).is(':checked'))) {
                    skipped = true;
                }
            }
            if ($(this).is(':checked') && !skipped) {
                var tr = $(this).closest('tr');
                count++;
                found = true;
                lastItem = tr.find('.filter-groupNext');
                lastItem.val('true');
                tr.css('background-color', FILTER_colors[colorIndex]);
                changed.push(tr);
            }
        });
        if (count == 1) {
            changed[0].css('background-color', 'inherit');
            changed[0].find('.filter-groupNext-value').val('false');
        }
        lastItem.val('false');
        this.Modal.find('.filter-groupNext').prop('checked', false);
    };
    this.Ungroup = function (e) {
        e.preventDefault();
        if (this.Modal === null) {
            return;
        }
        this.Modal.find('.filter').css('background-color', 'inherit');
        this.Modal.find('.filter').find('.filter-groupNext-value').val('false');
        colorIndex = -1;
    };
    this.ActingOnChange = function (tr) {
        /// <signature>
        /// <summary>Handles changes to the filter acting on field.</summary>
        /// <param name="tr" type="jQuery">The jquery object representing the html tr tag.</param>
        /// </signature>
        try {
            tr.find('.rating').rating('destroy');
        } catch (e) { }
        var oldInput = tr.find('.filter-value');
        var header = FindHeader(this.Table, tr.find('.filter-actingon').val());
        var html = this.GenerateValueInput(header);
        oldInput.replaceWith(html);
        var testHtml = '';
        if (header.Type === 'itemParent' || header.Type === 'boolean') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option>';
        } else if (header.Type === 'multipleSelection') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="in">In</option><option value="notin">Not In</option>';
        } else if (header.Type === 'number' || header.Type.indexOf('rating') === 0) {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else if (header.Type === 'date') {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        } else {
            testHtml = '<option value="==">Equals</option><option value="!=">Not Equals</option><option value="^=">Starts With</option><option value="!^=">Does Not Start With</option><option value="$=">Ends With</option><option value="!$=">Does Not End With</option><option value="=rgx=">Regex Match</option><option value="!=rgx=">Regex Mismatch</option><option value="*=">Contains</option><option value="!*=">Does Not Contain</option><option value=">">Greater Than</option><option value=">=">Greater Than or Equal</option><option value="<">Less Than</option><option value="<=">Less Than or Equal</option>';
        }

        tr.find('.filter-test').html(testHtml);
        try {
            tr.find('.rating').rating();
        } catch (e) {}
        try {
            tr.find('.datetimepicker').datetimepicker();
        } catch (e) { }
    };
    this.GenerateValueInput = function (header, value) {
        /// <signature>
        /// <summary>Generates the input for the filter value.</summary>
        /// <returns type="String" />
        /// <param name="header" type="JTableHeader">The JTableHeader to generate off of.</param>
        /// <param name="value" type="String">The value to prefil.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '';
        if (header === null) {
            return '<span class="filter-value">Make a Selection</span>';
        }
        if (header.Type === 'itemParent' || header.Type === 'multipleSelection' || header.Type === 'boolean') {
            html += '<select class="filter-value form-control">';
            for (var i = 0; i < header.PossibleValues.length; i++) {
                html += '<option value="' + header.PossibleValues[i].Id + '"' + (header.PossibleValues[i].Id === value ? ' selected="true"' : '') + '>' + header.PossibleValues[i].Label + '</option>';
            }
            html += '</select>';
        } else if (header.Type.indexOf('rating') === 0) {
            if (value === '') {
                value = '0';
            }
            var t_json = header.Type;
            t_json = t_json.split("=>")[1];
            var t_options = JSON.parse(t_json);
            html += '<input class="filter-value form-control rating" min="' + t_options.min + '" max="' + t_options.max + '" data-step="' + t_options.step + '" type="number" value="' + value + '" />';
        } else {
            html += '<input class="filter-value form-control' + (header.Type === 'date' ? ' datetimepicker' : '') + '" value="' + value + '" />';
        }
        return html;
    };
    if (this.Modal !== null) {
        this.Modal.find('.set-filters').on('click', function () {
            t_holder.Modal.modal('hide');
            t_holder.Table.Filters = [];
            t_holder.Modal.find('.filter').each(function (i) {
                var tr = $(this);
                var filter = new JTableFilter();
                filter.GroupNext = tr.find('.filter-groupNext-value').val().toLowerCase() === 'true';
                filter.ActingOn = tr.find('.filter-actingon').val();
                filter.Link = tr.find('.filter-link').val();
                filter.Order = i;
                filter.Test = tr.find('.filter-test').val();
                filter.Value = tr.find('.filter-value').val();
                t_holder.Table.Filters.push(filter);
            });
            t_holder.Table.Filtered = false;
            t_holder.Table.GetPage();
        });
        this.Modal.find('.add-filter').on('click', function (e) {
            e.preventDefault();
            t_holder.AddStatement();
        });
        this.Modal.find('.ungroup').on('click', function (e) {
            t_holder.Ungroup(e);
        });
        this.Modal.find('.group').on('click', function (e) {
            t_holder.Group(e);
        });
    }
}

function FILTER_GenerateModal(modalId) {
    /// <signature>
    /// <summary>Creates a sorting modal.</summary>
    /// <return type="jQuery" />
    /// <param name="modalId" type="String">The id to use for the modal. default: a incremental number.</modalId>
    /// </signature>
    if (modalId === null) {
        modalId = 'm_filter' + (++FILTER_modalIndex);
    }
    if (modalId.legth > 0 && modalId[0] === '#') {
        modalId = modalId.substr(1, modalId.length);
    }
    var object = $(modalId);
    if (object.length > 0) {
        return object;
    }
    var filterModal_html = '<div class="modal fade" id="' + modalId + '"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Filters</h3></div>';
    filterModal_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Filters</h4></div>';
    filterModal_html += '<div class="row color-grey-2 add-padding-vertical-5"><div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="group"><span class="glyphicon glyphicon-link"></span> Group Selected</a></div></div>';
    filterModal_html += '<div class="col-sm-6"><div class="add-padding-vertical-5"><a href="#" class="ungroup"><span class="glyphicon glyphicon-link"></span> Ungroup All</a></div></div></div>';
    filterModal_html += '<div class="row"><table class="table table-filter">';
    filterModal_html += '<thead><tr><th></th><th>Link</th><th>Variable</th><th>Test</th><th>Value</th></tr></thead>';
    filterModal_html += '<tbody class="table-filter-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-filter"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    filterModal_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default set-filters">Apply Filters</button>';
    filterModal_html += '</div></div></div>';
    $('body').append(filterModal_html);
    object = $('#' + modalId);
    return object;
}
///#source 1 1 /Scripts/Tool/Sortings.js
/* Sorting v1.1.1.0        */
/* Created By: D.J. Enzyme */
/* Creation Date: 20141229 */
/* Modified Date: 20150120 */

/// <reference path="jTable.js" />

/* global JTableSorting */
/* jshint unused: false*/

var SORTING_VERSION = '1.1.1.0';

var SORTING_modalIndex = -1;

function Sorting(table, modalId, minimal) {
    /// <signature>
    /// <summary>Creates a Sorting object.</summary>
    /// <returns type="Sorting" />
    /// <param name="table" type="JTable">The JTable the sorting will apply too.</param>
    /// <param name="modalId" type="String">The modal Id of the sorting. default: index sorting</param>
    /// <field name="Table" type="jQuery">The Table that is being used for the sortings.</field>
    /// <field name="Modal" type="jQuery">The modal that is being used to edit sortings.</field>
    /// </signature>
    if (typeof (modalId) === 'undefined') {
        modalId = null;
    }
    var min_layout = false;
    if (typeof (minimal) === 'boolean') {
        min_layout = minimal;
    }
    var sortingIndex = -1;
    var colorIndex = -1;
    var t_holder = this;
    this.Table = table;
    this.Modal = null;
    if (!min_layout) {
        this.Modal = SORTING_GenerateModal(modalId);
    }
    this.GenerateActing = function (value) {
        /// <signature>
        /// <summary>Generating the acting on input.</summary>
        /// <param name="value" type="String">The value to prefill for acting on.</param>
        /// </signature>
        if (typeof (value) === 'undefined') {
            value = '';
        }
        var html = '<select class="sorting-actingon form-control">';
        for (var i = 0; i < this.Table.Headers.length; i++) {
            html += '<option value="' + this.Table.Headers[i].Id + '"' + (this.Table.Headers[i].Id === value ? ' selected="selected"' : '') + '>' + this.Table.Headers[i].Label + '</option>';
        }
        html += '</select>';
        return html;
    };
    this.Generate = function () {
        /// <signature>
        /// <summary>Generates the sortings to place in the table</summary>
        /// </signature>
        if (this.Modal === null) {
            return;
        }
        this.Modal.find('.table-sorting-body').html('');
        sortingIndex = -1;
        var html = '';
        for (var i = 0; i < this.Table.Sortings.length; i++) {
            var t_sorting = this.Table.Sortings[i];
            sortingIndex++;
            html = '<tr class="sorting" data-index="' + sortingIndex + '"><td>';
            html += '<a href="#" class="sorting-delete"><span class="glyphicon glyphicon-trash"></span></a>';
            html += '</td>';
            html += '<td>';
            html += this.GenerateActing(t_sorting.ActingOn);
            html += '</td>';
            html += '<td>';
            html += '<select class="form-control sorting-ascending">';
            html += '<option value="true"' + (t_sorting.Ascending ? ' selected="selected"' : '') + '>Ascending</option>';
            html += '<option value="false"' + (!t_sorting.Ascending ? ' selected="selected"' : '') + '>Descending</option>';
            html += '</select>';
            html += '</td>';
            html += '</tr>';
            this.Modal.find('.table-sorting-body').append(html);
        }
        $('.sorting-delete').on('click', function (e) {
            t_holder.DeleteSorting($(this).parents('tr'), e);
        });

    };
    this.DeleteSorting = function (tr, event) {
        /// <signature>
        /// <summary>Deletes a sorting.</summary>
        /// <param name="tr" type="jQuery">The tr html tag to remove.</param>
        /// <param name="evemt" type="JavascriptEvent">The mouse event.</param>
        /// </signature>
        event.preventDefault();
        if (this.Modal === null) {
            return;
        }
        $(tr).remove();
        this.Modal.find('.sorting').css('background-color', 'inherit');
        this.Modal.find('.sorting-groupnext').prop('checked', false);
        colorIndex = -1;
        this.Modal.find('.sorting').each(function (i) {
            sortingIndex = i;
            $(this).attr('data-index', i);
        });
        sortingIndex--;
    };
    this.AddSorting = function () {
        /// <signature>
        /// <summary>Adds a new sorting to the table.</summary>
        /// </signature>
        sortingIndex++;
        var html = '<tr class="sorting" data-index="' + sortingIndex + '"><td>';
        html += '<a href="#" class="sorting-delete"><span class="glyphicon glyphicon-trash"></span></a>';
        html += '</td>';
        html += '<td>';
        html += this.GenerateActing();
        html += '</td>';
        html += '<td>';
        html += '<select class="form-control sorting-ascending">';
        html += '<option value="true">Ascending</option>';
        html += '<option value="false">Descending</option>';
        html += '</select>';
        html += '</td>';
        html += '</tr>';
        $('.table-sorting-body').append(html);
        $('.sorting[data-index="' + sortingIndex + '"]').find('.sorting-delete').on('click', function (e) {
            t_holder.DeleteSorting($(this).closest('tr'), e);
        });
    };
    if (this.Modal !== null) {
        this.Modal.find('.set-sortings').on('click', function () {
            t_holder.Modal.modal('hide');
            t_holder.Table.Sortings = [];
            t_holder.Modal.find('.sorting').each(function (i) {
                var sorting = new JTableSorting();
                sorting.ActingOn = $(this).find('.sorting-actingon').val();
                sorting.Order = i;
                sorting.Ascending = $(this).find('.sorting-ascending').val().toLowerCase() === 'true';
                t_holder.Table.Sortings.push(sorting);
                t_holder.Table.Sorted = false;
            });
            t_holder.Table.GetPage();
        });
        this.Modal.find('.add-sorting').on('click', function (e) {
            e.preventDefault();
            t_holder.AddSorting();
        });
    }
}

function SORTING_GenerateModal(modalId) {
    /// <signature>
    /// <summary>Creates a sorting modal.</summary>
    /// <return type="jQuery" />
    /// <param name="modalId" type="String">The id to use for the modal. default: a incremental number.</modalId>
    /// </signature>
    if (modalId === null) {
        modalId = 'm_sorting' + (++SORTING_modalIndex);
    }
    if (modalId.legth > 0 && modalId[0] === '#') {
        modalId = modalId.substr(1, modalId.length);
    }
    var object = $(modalId);
    if (object.length > 0) {
        return object;
    }
    var sortingModal_html = '<div class="modal fade" id="' + modalId + '"><div class="modal-dialog modal-lg"><div class="modal-header"><h3 class="modal-title">Sortings</h3></div>';
    sortingModal_html += '<div class="modal-body"><div class="add-padding-top text-color-1"><h4>Current Sortings</h4></div>';
    sortingModal_html += '<div class="row"><table class="table table-sorting">';
    sortingModal_html += '<thead><tr><th></th><th>Header</th><th>Sort Order</th></tr></thead>';
    sortingModal_html += '<tbody class="table-sorting-body"></tbody><tfoot><tr><td colspan="5"><a href="#" class="add-sorting"><span class="glyphicon glyphicon-small glyphicon-plus"></span> New Statement</a></td></tr></tbody>';
    sortingModal_html += '</table></div></div><div class="modal-footer"><button type="button" class="btn btn-default" data-dismiss="modal">Cancel</button><button type="button" class="btn btn-default set-sortings">Apply Sortings</button>';
    sortingModal_html += '</div></div></div>';
    $('body').append(sortingModal_html);
    object = $('#' + modalId);
    return object;
}
///#source 1 1 /Scripts/Cloud/Reports/Report.js
/*! Report 2.0.1.1 */

/// <reference path="../Tool/breadCrumb.js" />
/// <reference path="../Moment/moment.js" />
/// <reference path="../Tool/Filters.js" />
/// <reference path="../Tool/jTable.js" />
/// <reference path="../jQuery/Plugins/sortable.js" />
/// <reference path="../browserGap.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />
/// <reference path="../Tool/Sortings.js" />
/// <reference path="../Tool/restful.js" />
/// <reference path="EmailSendInformation.js" />
/// <reference path="../jQuery/Plugins/jsonData.js" />
/// <reference path="../Tool/cycle.js" />
/// <reference path="EmailSendInformation.js" />

/* global TABLE_options*/
/* global prettyProcessing */
/* global EmailSendInformation */
/* global JTableFilter */
/* global JTable */
/* global RESTFUL */
/* global BREADCRUMB */
/* global FindColumn */
/* global ReplaceRow */
/* global ReplaceHeader */
/* global UpdateCrumb */
/* global AddJsonAntiForgeryToken */

var FORMREPORT = {};

$(document).on('ready', function () {
    "use strict";

    var table = new JTable('#jTable');

    FORMREPORT.VERSION = '2.0.1.1';

    //#region Modal Events (Window Resize)
    var height = $(window).innerHeight();
    height *= 0.72;
    $('.modal-fill .modal-body').css('max-height', height + 'px');
    $(window).on('resize', function () {
        var t_height = $(window).innerHeight();
        t_height *= 0.72;
        $('.modal-fill .modal-body').css('max-height', t_height + 'px');
    });
    //#endregion

    //#region Registrant Information
    var registrant = {};
    var changeSetTable = null;
    var currentTransaction = 0;
    var oldData = { id: null, regKey: null, value: null, isSecure: false };
    //#endregion

    //#region static filters
    /* quick filter constants */
    var filter_status_submitted = new JTableFilter();
    filter_status_submitted.ActingOn = 'status';
    filter_status_submitted.Value = '1';
    filter_status_submitted.Test = '==';
    var filter_status_notIncomplete = new JTableFilter();
    filter_status_notIncomplete.ActingOn = 'status';
    filter_status_notIncomplete.Value = '0';
    filter_status_notIncomplete.Test = '!=';
    var filter_status_notCanceled1 = new JTableFilter();
    filter_status_notCanceled1.ActingOn = 'status';
    filter_status_notCanceled1.Value = '2';
    filter_status_notCanceled1.Test = '!=';
    var filter_status_notCanceled2 = new JTableFilter();
    filter_status_notCanceled2.ActingOn = 'status';
    filter_status_notCanceled2.Value = '3';
    filter_status_notCanceled2.Test = '!=';
    var filter_status_notCanceled3 = new JTableFilter();
    filter_status_notCanceled3.ActingOn = 'status';
    filter_status_notCanceled3.Value = '4';
    filter_status_notCanceled3.Test = '!=';
    var filter_status_notDeleted = new JTableFilter();
    filter_status_notDeleted.ActingOn = 'status';
    filter_status_notDeleted.Value = '5';
    filter_status_notDeleted.Test = '!=';
    /* end quick filter contsants */
    //#endregion

    //#region bindings
    /* bindings */
    $('#checkForUpdates').on('click', function () {
        CheckForUpdates(table);
    });
    $('#getFull').on('click', function () {
        table.Load(window.location.origin + '/Cloud/Report', TABLE_options);
    });
    $('#downloadReport').on('click', function () {
        //var newTable = new JTable();
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/Cloud/Create/Report');
        RESTFUL.jsonHeader(xhr);
        RESTFUL.ajaxHeader(xhr);
        xhr.onerror = function () {
            RESTFUL.showError('There was an error creating the report. 500 Internal server error.', 'Report Creation Error');
            prettyProcessing.hidePleaseWait();
        };
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    window.location = window.location.origin + '/Cloud/Download/Report/' + result.Id;
                } else {
                    RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                    prettyProcessing.hidePleaseWait();
                }
            } else {
                RESTFUL.showError('There was an error creating the report.', 'Report Creation Error');
                prettyProcessing.hidePleaseWait();
            }
        };
        prettyProcessing.showPleaseWait('Creating Form Report', 'Creating Report');
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(table.GetAjaxData())));
    });
    $('#printable').on('click', function (e) {
        e.preventDefault();
        table.GetPrintView();
    });
    $('#m_save_button').on('click', function () {
        $('#m_save').modal('hide');
        var t_id = $('#m_save_fileInputId').val();
        var t_name = $('#m_save_fileInput').val();
        $('#m_save_fileInputId').val('');
        $('#m_save_fileInput').val('');
        if (typeof (t_name) === 'undefined' || t_name === null || t_name === '') {
            return;
        }
        prettyProcessing.showPleaseWait('Saving Report', 'Please Wait', 100);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/Report');
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#file_save').removeAttr('style');
                    $('#file_delete').removeAttr('style');
                    table.SavedId = result.Id;
                    table.Name = result.Name;
                    $('.jTable').html(table.Name);
                    $('#file_delete').attr('data-xhr-options', '{"id":"' + table.SavedId + '"}');
                    BREADCRUMB.CURRENT.Label = 'Report: ' + table.Name + ' on ' + table.Parent;
                    BREADCRUMB.CURRENT.Parameters.id = table.SavedId;
                    UpdateCrumb(BREADCRUMB.CURRENT);
                    $('.jTable_standardOnly').hide();
                    table.GetPage();
                    prettyProcessing.hidePleaseWait();
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError();
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        var data = table.GetAjaxData();
        data.SavedId = t_id;
        data.Name = t_name;
        t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));
    });
    $('#file_saveas').on('click', function () {
        showFileModal();
    });
    $('#file_save').on('click', function (e) {
        e.preventDefault();
        if (typeof (table.SavedId) === 'undefined' || table.SavedId === null) {
            return;
        }
        prettyProcessing.showPleaseWait('Saving Report', 'Please Wait', 100);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/Report');
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#file_save').removeAttr('style');
                    $('#file_delete').removeAttr('style');
                    table.SavedId = result.Id;
                    table.Name = result.Name;
                    prettyProcessing.hidePleaseWait();
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError();
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        var data = table.GetAjaxData();
        t_xhr.send(JSON.stringify(AddJsonAntiForgeryToken(data)));

    });
    $('#file_load').on('click', function () {
        showFileModal(true);
    });
    $('#file_permissions').on('click', function (e) {
        e.preventDefault();
        if (typeof (table.SavedId) === 'undefined' || table.SavedId === null) {
            return;
        }
        window.location = window.location.origin + '/Security/Permissions/' + table.SavedId;
    });
    $('#formType').on('change', function () {
        table.Filters = [];
        switch ($(this).val()) {
            case 'all':
                table.Name = 'All Registrations';
                break;
            case 'active':
                table.Name = 'Active Registrations';
                table.Filters.push(filter_status_submitted);
                break;
            case 'canceled':
                table.Name = 'Cancelled Registrations';
                var canc_filter1 = new JTableFilter();
                canc_filter1.GroupNext = true;
                canc_filter1.ActingOn = 'status';
                canc_filter1.Order = 1;
                canc_filter1.Test = '==';
                canc_filter1.Link = 'or';
                canc_filter1.Value = '2';
                var canc_filter2 = new JTableFilter();
                canc_filter2.GroupNext = true;
                canc_filter2.ActingOn = 'status';
                canc_filter2.Order = 3;
                canc_filter2.Test = '==';
                canc_filter2.Link = 'or';
                canc_filter2.Value = '3';
                var canc_filter3 = new JTableFilter();
                canc_filter3.GroupNext = false;
                canc_filter3.ActingOn = 'status';
                canc_filter3.Order = 4;
                canc_filter3.Test = '==';
                canc_filter3.Link = 'or';
                canc_filter3.Value = '4';
                table.Filters.push(canc_filter1);
                table.Filters.push(canc_filter2);
                table.Filters.push(canc_filter3);
                break;
            case 'incompletes':
                table.Name = 'Incomplete Registrations';
                var inc_filter = new JTableFilter();
                inc_filter.ActingOn = 'status';
                inc_filter.Value = '0';
                inc_filter.Test = '==';
                inc_filter.Order = 1;
                table.Filters.push(inc_filter);
                break;
            case 'deleted':
                table.Name = 'Deleted Registrations';
                var del_filter = new JTableFilter();
                del_filter.ActingOn = 'status';
                del_filter.Value = '5';
                del_filter.Test = '==';
                del_filter.Order = 1;
                table.Filters.push(del_filter);
                break;
            case 'unbalanced':
                table.Name = 'Unbalanced Accounts';
                var unb_filter = new JTableFilter();
                unb_filter.ActingOn = 'balance';
                unb_filter.Value = '0';
                unb_filter.Test = '!=';
                unb_filter.Order = 1;
                unb_filter.Link = 'none';
                table.Filters.push(unb_filter);
                filter_status_notIncomplete.Order = 2;
                filter_status_notIncomplete.Link = 'and';
                filter_status_notDeleted.Order = 3;
                filter_status_notDeleted.Link = 'and';
                table.Filters.push(filter_status_notIncomplete);
                table.Filters.push(filter_status_notDeleted);
                break;
            case 'refund':
                table.Name = 'Refunds Due';
                var ref_filter = new JTableFilter();
                ref_filter.ActingOn = 'balance';
                ref_filter.Value = '0';
                ref_filter.Test = '<';
                ref_filter.Order = 1;
                table.Filters.push(ref_filter);
                filter_status_notIncomplete.Order = 2;
                filter_status_notIncomplete.Link = 'and';
                filter_status_notDeleted.Order = 3;
                filter_status_notDeleted.Link = 'and';
                table.Filters.push(filter_status_notIncomplete);
                table.Filters.push(filter_status_notDeleted);
                break;
            case 'owed':
                table.Name = 'Balances Due';
                var owe_filter = new JTableFilter();
                owe_filter.ActingOn = 'balance';
                owe_filter.Value = '0';
                owe_filter.Test = '>';
                owe_filter.Order = 1;
                table.Filters.push(owe_filter);
                filter_status_notIncomplete.Order = 2;
                filter_status_notIncomplete.Link = 'and';
                filter_status_notDeleted.Order = 3;
                filter_status_notDeleted.Link = 'and';
                table.Filters.push(filter_status_notIncomplete);
                table.Filters.push(filter_status_notDeleted);
                break;
            case 'rsvpAccept':
                table.Name = 'RSVP Accept';
                var rsvpA_filter = new JTableFilter();
                rsvpA_filter.ActingOn = 'rsvp';
                rsvpA_filter.Value = '1';
                rsvpA_filter.Test = '==';
                rsvpA_filter.Order = 1;
                table.Filters.push(rsvpA_filter);
                filter_status_submitted.Link = 'and';
                filter_status_submitted.Order = 2;
                table.Filters.push(filter_status_submitted);
                break;
            case 'rsvpDecline':
                table.Name = 'RSVP Decline';
                var rsvpD_filter = new JTableFilter();
                rsvpD_filter.ActingOn = 'rsvp';
                rsvpD_filter.Value = '0';
                rsvpD_filter.Test = '==';
                rsvpD_filter.Order = 1;
                table.Filters.push(rsvpD_filter);
                filter_status_submitted.Link = 'and';
                filter_status_submitted.Order = 2;
                table.Filters.push(filter_status_submitted);
                break;
            case 'rsvpCount':
                break;
        }
        table.Filtered = false;
        table.FilterObject.Generate();
        table.GetPage();
    });
    /* end bindings */
    //#endregion

    //#region CSS styling
    $('#file_save').css('color', '#eeeeee');
    $('#file_delete').css('color', '#eeeeee');
    $('#file_permissions').css('color', '#eeeeee');
    //#endregion

    //#region Table definitions
    table.OnGetComplete = function (p_table, container) {
        if (typeof (container) === 'undefined') {
            container = $(p_table.Table);
        }
        BindEditable(container);
        container.find('.balance').on('click', function (e) {
            e.preventDefault();
            prettyProcessing.showPleaseWait('Registrant Invoice', 'Getting Data');
            var xhr = new XMLHttpRequest();
            var tr = $(this).closest('tr');
            xhr.open('get', '../RegistrantInvoice/' + tr.attr('id'));
            xhr.onerror = function () {
                RESTFUL.showError('500 Internal Server Error', 'Registrant Invoice Error');
                prettyProcessing.hidePleaseWait();
            };
            xhr.onload = function (event) {
                var c_xhr = event.currentTarget;
                if (c_xhr.status === 200) {
                    var t_result = RESTFUL.parse(c_xhr);
                    if (t_result.Success) {
                        prettyProcessing.hidePleaseWait();
                        $('#invoiceModal').find('.modal-body').html(t_result.Html);
                        $('#invoiceModal').modal('show');
                    } else {
                        RESTFUL.showError(t_result.Message, 'Registrant Invoice Error');
                        prettyProcessing.hidePleaseWait();
                    }
                } else {
                    RESTFUL.showError('500 Internal Server Error', 'Registrant Invoice Error');
                    prettyProcessing.hidePleaseWait();
                }
            };
            RESTFUL.ajaxHeader(xhr);
            xhr.send();
        });
        EmailSendInformation.bind('.email-information', container);
        EmailSendInformation.bindEmailList('.email-sendlist', container);
        container.find('.reg-ajax').on('click', function (e) {
            e.preventDefault();
            registrant = {};
            registrant.id = $(this).attr('data-id');
            loadRegistrant();
        });
        prettyProcessing.hidePleaseWait();
    };
    table.OnUpdateComplete = function (tr) {
        tr.find('.reg-ajax').on('click', function (e) {
            e.preventDefault();
            registrant = {};
            registrant.id = $(this).attr('data-id');
            loadRegistrant();
        });
    };
    table.AfterLoad = function (p_table) {
        if (BREADCRUMB.CURRENT !== null) {
            UpdateBreadCrumb(p_table);
        } else {
            setTimeout(function () { UpdateBreadCrumb(p_table); }, 5000);
        }
        if (table.SavedId !== null && typeof (table.SavedId) !== 'undefined') {
            $('#file_save').css('color', '');
            $('#file_delete').css('color', '');
            $('#file_permissions').css('color', '');
            $('#file_delete').attr('data-xhr-options', '{"id":"' + table.SavedId + '"}');
        }
        setTimeout(function () { CheckForUpdates(table); }, 10000);
    };
    table.Load(window.location.origin + '/Cloud/Report', TABLE_options);
    //#endregion

    //#region Table Functions
    /* functions */
    function showFileModal(load) {
        if (typeof (load) === 'undefined' || load === null) {
            load = false;
        }
        $('#m_save_files').html('Loading Files');
        prettyProcessing.showPleaseWait('Retrieving Report', 'Please Wait');
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('get', window.location.origin + '/Cloud/Reports/' + table.Id);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            RESTFUL.xhrError(event);
        };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    prettyProcessing.hidePleaseWait();
                    var saveDiv = $('#m_save_files');
                    saveDiv.html('');
                    for (var i = 0; i < result.Files.length; i++) {
                        var t_file = result.Files[i];
                        saveDiv.append('<div class="report-file" data-id="' + t_file.Id + '">' + t_file.Name + '</div>');
                    }
                    if (!load) {
                        $('.report-file').on('click', function () {
                            $('#m_save_fileInputId').val($(this).attr('data-id'));
                            $('#m_save_fileInput').val($(this).html());
                        });
                        $('#m_save_fileInput').parent().show();
                    } else {
                        $('.report-file').on('click', function () {
                            LoadTable($(this).attr('data-id'));
                            $('m_save').modal('hide');
                        });
                        $('#m_save_fileInput').parent().hide();
                    }
                    $('#m_save').modal('show');
                } else {
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError();
                }
            } else {
                prettyProcessing.hidePleaseWait();
                RESTFUL.showError();
            }
        };
        t_xhr.send();
    }
    function LoadTable(id) {
        $('#m_save').modal('hide');
        table.Load(window.location.origin + '/Cloud/Report', { id: id });
    }
    function RunBinding() {
        $('.form-component').removeClass('col-sm-6').removeClass('col-md-4').removeClass('col-lg-3').addClass('col-sm-12');
        $('input[data-component-type="datetime"]').datetimepicker();

        /* Checkbox Group */
        $('input[type="hidden"][data-component-type="checkboxgroup"]').each(function () {
            var hidden_input = $(this);
            var value = hidden_input.val();
            if (typeof (value) === 'undefined' || value === '') {
                value = "[]";
            }
            var t_value = JSON.parse(value);
            hidden_input.data('value', t_value);
        });
        $('input[type="checkbox"]').on('change', function () {
            var input = $(this);
            if (/Waitlistings/i.test(input.attr('name'))) {
                return;
            }
            var hidden_input = $('#' + input.attr('data-parent'));
            var t_value = hidden_input.data('value');
            if (input.prop('checked')) {
                var accept = true;
                var t_index = t_value.indexOf(input.val());
                if (t_index === -1) {
                    t_value.push(input.val());
                }

                /* Time Exclusion */
                if (hidden_input.attr('data-component-timeexclusion') === 'True') {
                    var collision = false;
                    for (var i = 0; i < t_value.length; i++) {
                        var t_item = $('#' + t_value[i]);
                        var aStart = moment(t_item.attr('data-agenda-start'));
                        var aEnd = moment(t_item.attr('data-agenda-end'));
                        for (var j = 0; j < t_value.length; j++) {
                            if (t_value[i] === t_value[j]) {
                                continue;
                            }
                            var t_item2 = $('#' + t_value[j]);
                            var bEnd = moment(t_item2.attr('data-agenda-end'));
                            if ((bEnd.isAfter(aStart) || bEnd.isSame(aStart)) && (bEnd.isBefore(aEnd) || bEnd.isSame(aEnd))) {
                                collision = true;
                            }
                        }
                    }
                    if (collision) {
                        accept = false;
                    }
                }

                if (!accept) {
                    t_value.splice(t_index, 1);
                    input.prop('checked', false);
                    alert('Your selection has time conflictions.');
                }
            } else {
                var t_index2 = t_value.indexOf(input.val());
                if (t_index2 !== -1) {
                    t_value.splice(t_index2, 1);
                }
            }
            hidden_input.data('value', t_value);
            hidden_input.val(JSON.stringify(t_value));
        });
        /* End Checkbox Group */

        /* Radio Group */
        $('input[data-component-type="radiogroup"][type="hidden"]').each(function () {
        });
        /* End Radio Group */

        $('input[type="radio"]').on('change', function () {
            var input = $(this);
            var hidden_input = $('#' + input.attr('data-parent'));
            if (input.prop('checked')) {
                hidden_input.val(input.val());
            }
        });

        $('.uploaded-image').each(function () {
            $.ajax({
                type: 'Get',
                url: '../../Cloud/RegistrantImageThumbnail/' + $(this).attr('data-form-registrant') + '?component=' + $(this).attr('data-form-component'),
                success: function (data) {
                    $(this).attr('src', data);
                },
            });
        });
    }
    function BindEditable() {
        $('.editable-item').on('click', function () {
            $('#editModal .modal-body').find('#editingProgress').show();
            $('#editModal .modal-body').find('#editingData').hide();
            $('#editModal').modal('show');
            oldData.id = $(this).attr('data-headerid');
            oldData.regKey = $(this).parent('tr').attr('id');
            oldData.value = $(this).html();
            if ($(this).hasClass('file')) {
                setTimeout(function () { ShowEditableItem(oldData.id, oldData.regKey); }, 1000);
            } else {
                ShowEditableItem(oldData.id, oldData.regKey);
            }
        });
        $('a[href="#emailSend"]').on('click', function (e) {
            e.preventDefault();
            var t_id = $(this).attr('data-emailsend-id');
            EmailSendInformation.load(t_id);
        });
    }
    function ShowEditableItem(id, regKey) {
        var width = $('#editModal .modal-dialog').width() - 2 - 30 - 30;
        $.ajax({
            url: '../../Cloud/EditComponent?key=' + id + '&registrantKey=' + regKey + '&width=' + width,
            type: "get",
            contentType: "application/json",
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    $('#editModal').find('#editingData').html(result.Html);
                    $('#editModal').find('.editingData').show('fast');
                    $('#editModal').find('#editingProgress').hide('fast');
                    RunBinding();
                } else {
                    $('#editModal').modal('hide');
                    oldData.Id = null;
                    oldData.regKey = null;
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                oldData.Id = null;
                oldData.regKey = null;
                alert('Server Error');
            }
        });
    }
    function CheckForUpdates(p_table) {
        /// <signature>
        /// <summary>Checks for updates.</summary>
        /// <param name="p_table" type="JTable">The jTable to check for updates.</field>
        /// </signature>
        // We need to put together a list of ids and modifcation tokens for each row;
        var modificationCheck = {
            id: p_table.Id,
            savedId: p_table.SavedId,
            options: p_table.Options,
            items: [],
            lastFullCheck: p_table.LastFullCheck.format()
        };
        for (var i = 0; i < p_table.Rows.length; i++) {
            var modificationToken = FindColumn(p_table.Rows[i], 'modificationToken');
            if (modificationToken !== null) {
                modificationCheck.items.push({ id: p_table.Rows[i].Id, token: modificationToken.Value });
            }
        }
        var xhr = new XMLHttpRequest();
        xhr.open('post', window.location.origin + '/Cloud/ReportUpdate', true);
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.action === 'expired') {
                    p_table.DeleteStore(p_table);
                    p_table.Load(window.location.origin + '/Cloud/Report', TABLE_options);
                } else {
                    var rowsAdded = false;
                    if (/rows/.test(result.action)) {
                        for (var i = 0; i < result.rows.length; i++) {
                            var row = result.rows[i];
                            rowsAdded = rowsAdded || !ReplaceRow(p_table, row);
                            p_table.UpdateRow(p_table, row);
                            p_table.OnGetComplete(p_table, $('#' + row.Id));
                        }
                    }
                    if (/headers/.test(result.action)) {
                        for (var j = 0; j < result.headers.length; j++) {
                            var header = result.headers[j];
                            ReplaceHeader(p_table, header);
                        }
                        p_table.GetPage();
                    }
                    //p_table.Store(p_table);
                    if (rowsAdded && p_table.FilteredRecords() < p_table.RecordsPerPage) {
                        p_table.Filter();
                        p_table.GetPage();
                    }
                }
            }
            setTimeout(function () { CheckForUpdates(p_table); }, 30000);
        };
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.send(JSON.stringify(AddJsonAntiForgeryToken(modificationCheck)));
    }
    function UpdateBreadCrumb(p_table) {
        if (BREADCRUMB.CURRENT === null) {
            setTimeout(function () { UpdateBreadCrumb(p_table); }, 5000);
            return;
        }
        BREADCRUMB.CURRENT.Label = p_table.Name + ' on ' + p_table.Parent;
        if (p_table.SavedId !== null && typeof (p_table.SavedId) !== 'undefined') {
            BREADCRUMB.CURRENT.Parameters.id = p_table.SavedId;
        } else {
            BREADCRUMB.CURRENT.Parameters.id = p_table.Id;
        }
        UpdateCrumb(BREADCRUMB.CURRENT);
    }
    /* end function */
    //#endregion

    //#region Registrant Bindings
    /* Registrant Bindings */
    $('#saveEdit').on('click', function () {
        // Here we make the ajax call to save the data
        $('#editModal').find('.editingData').hide('fast');
        $('#editModal').find('#editingProgress').show('fast');
        // Get the data to send.
        var waitlisting = [];
        var w_index = 0;
        while ($('input[name^="Waitlistings[' + w_index + ']"]').length > 0) {
            var json = {};
            var items = $('input[name^="Waitlistings[' + w_index + ']"]');
            for (var i = 0; i < items.length; i++) {
                var item = $(items[i]);
                if (/Key$/i.test(item.attr('name'))) {
                    json.Key = item.val();
                } else if (item.attr('type') === 'checkbox') {
                    if (item.prop('checked')) {
                        json.Value = true;
                    } else {
                        json.Value = false;
                    }
                }
            }
            w_index++;
            waitlisting.push(json);
        }
        var data = { ComponentKey: oldData.id, RegistrantKey: oldData.regKey, Value: $('#editModal').find('[id="' + oldData.id + '"]').val(), Waitlistings: waitlisting };
        // Add the antiforgery token.
        AddJsonAntiForgeryToken(data);
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('put', window.location.origin + '/Cloud/SaveRegistrantData', true);
        RESTFUL.jsonHeader(t_xhr);
        t_xhr.onerror = function (event) { RESTFUL.xhrError(event); };
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var result = RESTFUL.parse(c_xhr);
                if (result.Success) {
                    $('#' + oldData.regKey).find('td[data-headerid="' + oldData.id + '"]').html(result.NewValue);
                    for (var j = 0; j < table.Rows.length; j++) {
                        if (table.Rows[j].Id === oldData.regKey) {
                            var t_rowFound = false;
                            for (var k = 0; k < table.Rows[j].Columns.length; k++) {
                                if (table.Rows[j].Columns[k].HeaderId === oldData.id) {
                                    t_rowFound = true;
                                    table.Rows[j].Columns[k].Value = result.Value;
                                    table.Rows[j].Columns[k].PrettyValue = result.PrettyValue;
                                }
                            }
                            if (!t_rowFound) {
                                var column = new JTableColumn();
                                column.HeaderId = result.HeaderId;
                                column.PrettyValue = result.PrettyValue;
                                column.Value = result.Value;
                                column.Id = result.Id;
                                column.Editable = true;
                                table.Rows[j].Columns.push(column);
                            }
                            table.UpdateRow(table, table.Rows[j], true);
                        }
                    }
                    $('#editModal').modal('hide');
                } else {
                    $('#editModal .modal-body').find('#editingProgress').hide('fast');
                    $('#editModal .modal-body').find('#editingData').show('fast');
                    var message = '';
                    for (var l = 0; l < result.Errors.length; l++) {
                        if (l !== 0) {
                            message += '<br />';
                        }
                        message += result.Errors[l];
                    }
                    $('#editModal').find('.form-messagebox').html(message).show('fast');
                }
            } else {
                $('#editModal').modal('hide');
                RESTFUL.showError('Unhandled Server Error');
            }
        };
        t_xhr.send(JSON.stringify(data));
    });
    $('#adjustmentDate').datetimepicker();
    $('#adjust').on('click', function () {
        $('.adjust-warning').hide();
        $('.adjustment-error-text').html('');
        var data = {};
        data.id = registrant.id;
        data.ammount = $('#adjustmentAmmount').val();
        data.description = $('#adjustmentDescription').val();
        data.type = $('#type').val();
        data.transactionId = $('#transactionId').val();
        data.transactionDate = $('#adjustmentDate').val();
        if (/payment$/i.test(data.type)) {
            data.ammount *= -1;
        }
        var error = false;
        if (isNaN(data.ammount) || data.ammount === '') {
            $('#ammountError').html('You must supply a valid number.');
            $('.ammount-warning').show();
            error = true;
        }
        if (typeof (data.description) === 'undefined' || data.description === '') {
            $('#descriptionError').html('You must supply a description.');
            $('.description-warning').show();
            error = true;
        }
        if (error) {
            return;
        }
        $('#adjustment').modal('hide');
        data.__RequestVerificationToken = $('input[name=__RequestVerificationToken]').val();
        prettyProcessing.showPleaseWait();
        $.ajax({
            url: '../../Cloud/Adjustment',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    loadRegistrant();
                } else {
                    alert(result.Message);
                }
                prettyProcessing.hidePleaseWait();
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert("The adjustment failed on the server side.");
            }
        });
    });
    $('#proccessCC').on('click', function () {
        var data = {};
        data.id = registrant.id;
        data.Amount = $('#ccAmount').val();
        data.RegistrantKey = registrant.id;
        data.FormKey = registrant.formKey;
        data.CompanyKey = registrant.companyKey;
        data.CardNumber = $('#ccCardNumber').val();
        data.NameOnCard = $('#ccNameOnCard').val();
        data.ZipCode = $('#ccZipCode').val();
        data.CardCode = $('#ccCardCode').val();
        data.ExpMonth = $('#ccExpMonth').val();
        data.ExpYear = $('#ccExpYear').val();
        data.TransactionType = $('#ccTransactionType').val();
        data.Live = registrant.live;
        data.Address1 = $('#ccAddress1').val();
        data.Address2 = $('#ccAddress2').val();
        data.City = $('#ccCity').val();
        data.State = $('#ccState').val();
        data.Country = $('#ccCountry').val();
        data.Phone = $('#ccPhone').val();
        data.CardType = $('#ccCardType').val();
        var error = false;
        if (isNaN(data.Amount) || data.Amount === '') {
            alert("The amount is invalid.");
            error = true;
        }
        if (error) {
            return;
        }
        $('#processCCModal').modal('hide');
        var t_xhr = new XMLHttpRequest();
        t_xhr.open('post', window.location.origin + '/api/formgateway', true);
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            prettyProcessing.hidePleaseWait();
            var result = JSON.parse(c_xhr.responseText);
            if (c_xhr.status === 200) {
                if (result.Success) {
                    loadRegistrant();
                } else {
                    alert(result.Message);
                }
            } else if (c_xhr.status === 400) {
                alert(result.Message);
            }
        };
        t_xhr.onerror = function (event) {
            prettyProcessing.hidePleaseWait();
            var json = JSON.parse(result.responseText);
            alert(json.Message);
        };
        prettyProcessing.showPleaseWait();
        t_xhr.send(JSON.stringify(data));
    });
    $('#registrant-activate, #registrant-deactivate, #registrant-delete').on('click', function (e) {
        prettyProcessing.showPleaseWait();
        e.preventDefault();
        var link = $(this);
        var t_data = {};
        if (typeof (link.attr('data-xhr-options')) !== 'undefined') {
            t_data = JSON.parse(link.attr('data-xhr-options'));
        }
        var t_uri = link.attr('href');
        var t_xhr = new XMLHttpRequest();
        var t_method = 'put';
        if (link.is('#registrant-delete')) {
            t_method = 'delete';
        }
        t_xhr.open(t_method, t_uri, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.setRequestHeader('Content-Type', 'application/json');
        t_xhr.onload = function (event) {
            if (event.currentTarget.status === 200) {
                var result = RESTFUL.parse(event.currentTarget);
                if (result.Success) {
                    loadRegistrant();
                } else {
                    var t_fail = result.Message;
                    RESTFUL.showError(t_fail, 'Error Message');
                }
            } else if (event.currentTarget.status === 400) {
                RESTFUL.showError(event.currentTarget.responseBody, "Bad Request");
            } else {
                RESTFUL.showError('Server Error', 'Error Message');
            }
        };
        t_xhr.onerror = function () {
            prettyProcessing.hidePleaseWait();
            RESTFUL.showError('Server Error', 'Error Message');
        };
        t_xhr.send(JSON.stringify(t_data));
    });
    $('#refund').on('click', function () {
        prettyProcessing.showPleaseWait();
        $('#ammountToRefundError').html('');
        var data = {};
        data.id = $('#transaction-id').val();
        data.ammount = $('#ammountToRefund').val();
        if (isNaN(data.ammount)) {
            $('#ammountToRefundError').html('You must supply a valid number.');
            return;
        }
        $('#partialRefund').modal('hide');
        AddJsonAntiForgeryToken(data);
        $.ajax({
            url: '../../Cloud/RefundAmmount',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    loadTransaction();
                } else {
                    prettyProcessing.hidePleaseWait();
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert('The refund failed on the server side.');
            }
        });
    });
    $('.transaction-refundBalance').on('click', function (e) {
        e.preventDefault();
        prettyProcessing.showPleaseWait();
        var data = {};
        data.id = $('#transaction-id').val();
        AddJsonAntiForgeryToken(data);
        $.ajax({
            url: window.location.origin + '/Cloud/RefundBalance',
            type: "post",
            data: data,
            dataType: "json",
            success: function (result) {
                if (result.Success) {
                    loadTransaction();
                } else {
                    prettyProcessing.hidePleaseWait();
                    alert(result.Message);
                }
            },
            error: function () {
                prettyProcessing.hidePleaseWait();
                alert("The refund failed on the server side.");
            }
        });
    });
    $('#m_registrant-changeset').on('hide.bs.modal', function (e) {
        changeSetTable = null;
    });
    $('#registrant-adjustment').on('click', function (e) {
        e.preventDefault();
        $('.adjustment-payment').hide();
        $('.adjustment-payment-input').val('');
        $('.adjustment-type').val('Adjustment');
        $('#adjustment').modal('show');
    });
    $('#registrant-payment').on('click', function (e) {
        e.preventDefault();
        $('.adjustment-payment').show();
        $('.adjustment-adjustment').hide();
        $('#adjustment').modal('show');
    });
    /* end Registrant Bindings */
    //#endregion

    //#region Registrant Functions
    /* registrant functions */
    function loadRegistrant() {
        var xhr = new XMLHttpRequest();
        xhr.open('get', window.location.origin + '/Cloud/Registrant/' + registrant.id, true);
        RESTFUL.ajaxHeader(xhr);
        RESTFUL.jsonHeader(xhr);
        xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                registrant = RESTFUL.parse(c_xhr);
                $('.registrant-cancelled').hide();
                $('.registrant-submitted').hide();
                $('.registrant-deleted').hide();
                $('.registrant-incomplete').hide();
                $('.registrant-notincomplete').show();
                $('.registrant-notcancelled').show();
                $('.registrant-notdeleted').show();
                $('.registrant-notsubmitted').show();
                $('.registrant-name').html(registrant.fullName);
                $('.registrant-confirmation').children('td').eq(1).html(registrant.confirmation);
                $('.registrant-editedBy').children('td').eq(1).html(registrant.editedBy);
                var t_seatings = '';
                for (var t_seating_i = 0; t_seating_i < registrant.seatings.length; t_seating_i++) {
                    var t_seat = registrant.seatings[t_seating_i];
                    t_seatings += '<div class="row">';
                    if (t_seat.seated) {
                        t_seatings += '<div class="col-xs-4">' + t_seat.component + '</div><div class="col-xs-4">Seated</div><div class="col-xs-4">' + t_seat.dateSeated + '</div>';
                    } else {
                        t_seatings += '<div class="col-xs-4">' + t_seat.component + '</div><div class="col-xs-4">Waitlisted</div><div class="col-xs-4">' + t_seat.date + '</div>';
                    }
                    t_seatings += '<div class="row">';
                }
                $('.registrant-seatings').children('td').eq(0).html(t_seatings);
                if (registrant.rsvp == null) {
                    $('.registrant-rsvp').hide();
                } else {
                    $('.registrant-rsvp').show();
                    $('.registrant-rsvp').children('td').eq(1).html(registrant.rsvp);
                }
                if (registrant.status === 0) {
                    $('.registrant-notincomplete').hide();
                    $('.registrant-incomplete').show();
                } else if (registrant.status === 1) {
                    $('.registrant-notsubmitted').hide();
                    $('.registrant-submitted').show();
                } else if (registrant.status > 1 && registrant.status < 5) {
                    $('.registrant-notcancelled').hide();
                    $('.registrant-cancelled').show();
                } else if (registrant.status === 5) {
                    $('.registrant-notdeleted').hide();
                    $('.registrant-deleted').show();
                }
                if (registrant.phone !== null) {
                    $('.registrant-phone').children('td').eq(1).html(registrant.phone);
                    $('.registrant-phone').show();
                } else {
                    $('.registrant-phone').hide();
                }
                if (registrant.email !== null) {
                    $('.registrant-email').children('td').eq(1).html(registrant.email);
                    $('.registrant-email').show();
                } else {
                    $('.registrant-email').hide();
                }
                if (registrant.audience !== null) {
                    $('.registrant-audience').children('td').eq(1).html(registrant.audience);
                    $('.registrant-audience').show();
                } else {
                    $('.registrant-audience').hide();
                }
                $('.registrant-balance').children('td').eq(1).html(registrant.balance);
                $('.registrant-balanceNow').html(registrant.balance);
                if (registrant.dateCreated !== null) {
                    $('.registrant-datecreated').children('td').eq(1).html(registrant.dateCreated);
                    $('.registrant-datecreated').show();
                } else {
                    $('.registrant-datecreated').hide();
                }
                if (registrant.dateModified !== null) {
                    $('.registrant-datemodified').children('td').eq(1).html(registrant.dateModified);
                    $('.registrant-datemodified').show();
                } else {
                    $('.registrant-datemodified').hide();
                }
                if (registrant.statusLabel !== null) {
                    $('.registrant-status').children('td').eq(1).html(registrant.statusLabel);
                    $('.registrant-status').show();
                } else {
                    $('.registrant-status').hide();
                }
                $('.registrant-transactions').html('');
                var f_fullHtml = '';
                for (var f_id = 0; f_id < registrant.finances.length; f_id++) {
                    var finance = registrant.finances[f_id];
                    var t_date = new moment(registrant.finances[f_id].date);
                    var f_html = '<tr><td>' + t_date.format() + '</td>';
                    f_html += '<td>' + finance.label + (finance.voidable && !finance.voided ? ' <a class="text-rsred finance-void" href="' + window.location.origin + '/cloud/voidadjustment?aid=' + finance.id + '" data-id="' + finance.id + '">[void]</a>' : '') + '</td>';
                    f_html += '<td style="text-align: right;">' + (finance.failed || finance.voided ? '<s>' : '') + finance.amount + (finance.failed || finance.voided ? '</s>' : '') + '</td></tr>';
                    f_fullHtml += f_html;
                }
                $('.registrant-transactions').html(f_fullHtml);
                $('.finance-void').on('click', function (event) {
                    event.preventDefault();
                    var fin_xhr = new XMLHttpRequest();
                    fin_xhr.open('post', $(this).attr('href'), true);
                    fin_xhr.onload = function (event) {
                        var result = RESTFUL.parse(event.currentTarget);
                        if (result.Success) {
                            loadRegistrant();
                        } else {
                            alert('Failed to void adjustment.');
                        }
                    };
                    fin_xhr.send(JSON.stringify({ aid: $(this).attr('data-id') }));
                });
                f_fullHtml = '';
                for (var e_id = 0; e_id < registrant.emailActivities.length; e_id++) {
                    var t_date2 = new moment(registrant.emailActivities[e_id].date);
                    var f_html2 = '<tr><td>' + t_date2.format() + '</td>';
                    f_html2 += '<td><a href="#" class="registrant-emailSendInformation" data-id="' + registrant.emailActivities[e_id].id + '">' + registrant.emailActivities[e_id].name + '</a></td>';
                    f_html2 += '<td>' + registrant.emailActivities[e_id].deepestEvent + '</td>';
                    f_html2 += '<tr>';
                    f_fullHtml += f_html2;
                }
                $('.registrant-emailactivity').html(f_fullHtml);
                $('.registrant-confirmationLink').html(window.location.origin + '/Register/ShowConfirmation/' + registrant.id);
                $('.registrant-continueLink').html(window.location.origin + '/Register/ContinueRegistration/' + registrant.id);
                $('.registrant-link-continueLink').attr('href', window.location.origin + '/AdminRegister/Start?formKey=' + registrant.formKey + '&email=' + registrant.email);
                $('.registrant-link-confirmationLink').attr('href', window.location.origin + '/AdminRegister/Confirmation?registrantKey=' + registrant.id + '&changeStatus=false');
                //#region Bindings for Registrant Modal
                $('.registrant-transaction').on('click', function (e) {
                    e.preventDefault();
                    loadTransaction($(this).attr('id'));
                });
                EmailSendInformation.bind('.registrant-emailSendInformation');
                $('.registrant-addOptions').each(function () {
                    var json = $(this).attr('data-xhr-options-template');
                    if (typeof (json) === 'undefined') {
                        return;
                    }
                    json = json.replace(/_UId_/i, registrant.id);
                    $(this).attr('data-xhr-options', json);
                });
                $('.registrant-changeSet').on('click', function (e) {
                    e.preventDefault();
                    $('#registrant-changeSetTable').html('');
                    changeSetTable = new JTable('#registrant-changeSetTable', true);
                    changeSetTable.OnGetComplete = function (p_table) {
                        $(changeSetTable.Table).find('tbody').find('tr').not(':first').children('td').each(function () {
                            var td = $(this);
                            var header = td.attr('data-headerid');
                            if (header === 'email' || header === 'audience' || header === 'confirmation' || header === 'lastmodified' || header === 'modifiedby') {
                                return;
                            }
                            td.addClass('oldData-copy');
                            td.append('<div><a class="copyData-old" href="' + window.location.origin + '/Cloud/RegistrantCopyOldData" data-xhr-modalKeep="true" data-xhr-oncomplete="nothing" data-xhr-method="put" data-xhr-options=\'{"id":"' + td.attr('id') + '"}\'>USE FOR CURRENT</a></div>');
                        });
                        $('.copyData-old').on('click', function (e) {
                            prettyProcessing.showPleaseWait();
                            e.preventDefault();
                            var link = $(this);
                            var t_data = {};
                            if (typeof (link.attr('data-xhr-options')) !== 'undefined') {
                                t_data = JSON.parse(link.attr('data-xhr-options'));
                            }
                            var t_uri = link.attr('href');
                            var t_xhr = new XMLHttpRequest();
                            t_xhr.open('put', t_uri, true);
                            RESTFUL.ajaxHeader(t_xhr);
                            t_xhr.setRequestHeader('Content-Type', 'application/json');
                            t_xhr.onload = function (event) {
                                if (event.currentTarget.status === 200) {
                                    var result = RESTFUL.parse(event.currentTarget);
                                    if (result.Success) {
                                        var t_td = $(t_table.Table).find('[id="' + result.Id + '"]');
                                        t_td.html(result.Value);
                                    } else {
                                        var t_fail = result.Message;
                                        RESTFUL.showError(t_fail, 'Error Message');
                                    }
                                } else if (event.currentTarget.status === 400) {
                                    RESTFUL.showError(event.currentTarget.responseBody, "Bad Request");
                                } else {
                                    RESTFUL.showError('Server Error', 'Error Message');
                                }
                                prettyProcessing.hidePleaseWait();
                            };
                            t_xhr.onerror = function () {
                                prettyProcessing.hidePleaseWait();
                                RESTFUL.showError('Server Error', 'Error Message');
                            };
                            t_xhr.send(JSON.stringify(t_data));
                        });
                    };
                    changeSetTable.Load(window.location.origin + '/Cloud/RegistrantChangeSet', { id: registrant.id });
                    $('#m_registrant-changeset').modal('show');
                });
                //#endregion
                // Show modal now
                $('#m_registrant').modal('show');
            } else {
                RESTFUL.showError('Server Error');
            }
            prettyProcessing.hidePleaseWait();
        };
        prettyProcessing.showPleaseWait();
        xhr.send();
    }
    function loadTransaction(tr_id) {
        if (typeof(tr_id) !== 'undefined' && tr_id !== null) {
            currentTransaction = tr_id;
        }
        prettyProcessing.showPleaseWait();
        var tr_xhr = new XMLHttpRequest();
        $('#transaction-id').val(tr_id);
        tr_xhr.open('get', window.location.origin + '/Cloud/Transaction/' + currentTransaction, true);
        RESTFUL.ajaxHeader(tr_xhr);
        RESTFUL.jsonHeader(tr_xhr);
        tr_xhr.onload = function (event) {
            var c_tr_xhr = event.currentTarget;
            if (c_tr_xhr.status === 200) {
                var result = RESTFUL.parse(c_tr_xhr);
                if (result.Success) {
                    var trans = result.Data;
                    var modal = $('#m_transaction');
                    modal.find('.transaction-addOptions').each(function () {
                        var el = $(this);
                        var t_json = el.attr('data-xhr-options');
                        if (typeof (t_json) === 'undefined') {
                            return;
                        }
                        el.attr('data-xhr-options', t_json.replace(/_UId_/, trans.id));
                    });
                    modal.find('.transaction-refundBalance').attr('data-id', trans.id);

                    var t_tran_date = new moment(trans.date);
                    modal.find('.transaction-date').html(t_tran_date.format());
                    modal.find('.transaction-amount').html(trans.amount);
                    modal.find('.transaction-type').html(trans.type);
                    modal.find('.transaction-lastfour').html(trans.lastFour);
                    modal.find('.transaction-status').html(trans.status);
                    modal.find('.transaction-totalamount').html(trans.prettyBalance);
                    var t_htmlDetails = '';
                    for (var i_det = 0; i_det < trans.details.length; i_det++) {
                        var t_det_date = new moment(trans.details[i_det].date);
                        t_htmlDetails += '<tr><td>' + t_det_date.format() + '</td>';
                        t_htmlDetails += '<td>' + trans.details[i_det].type + '</td>';
                        t_htmlDetails += '<td>' + trans.details[i_det].id + '</td>';
                        t_htmlDetails += '<td>' + trans.details[i_det].status + '</td>';
                        t_htmlDetails += '<td>' + (trans.details[i_det].failed ? '<s>' : '') + trans.details[i_det].amount + (trans.details[i_det].failed ? '</s>' : '') + '</td></tr>';
                        for (var i_cred = 0; i_cred < trans.details[i_det].credits.length; i_cred++) {
                            var t_cred_date = new moment(trans.details[i_det].credits[i_cred].date);
                            t_htmlDetails += '<tr><td>&nbsp;&nbsp;' + t_cred_date.format() + '</td>';
                            t_htmlDetails += '<td>' + trans.details[i_det].credits[i_cred].type + '</td>';
                            t_htmlDetails += '<td>' + trans.details[i_det].credits[i_cred].id + '</td>';
                            t_htmlDetails += '<td>' + trans.details[i_det].credits[i_cred].status + '</td>';
                            t_htmlDetails += '<td>' + (trans.details[i_det].credits[i_cred].failed ? '<s>' : '') + trans.details[i_det].credits[i_cred].amount + (trans.details[i_det].credits[i_cred].failed ? '</s>' : '') + '</td></tr>';
                        }
                    }
                    modal.find('.transaction-details').html(t_htmlDetails);
                    prettyProcessing.hidePleaseWait();
                    if (trans.balance > 0) {
                        $('.transaction.hasBalance').hide();
                    } else {
                        $('.transaction.hasBalance').show();
                    }
                    $('#m_transaction').modal('show');
                } else {
                    $('#transaction-id').val('');
                    prettyProcessing.hidePleaseWait();
                    RESTFUL.showError(result.Message);
                }
            } else {
                $('#transaction-id').val('');
                prettyProcessing.hidePleaseWait();
                alert("Server error");
            }
        };
        tr_xhr.send();

    }
    /* end registrant functions */
    //#endregion
});
///#source 1 1 /Scripts/Cloud/Reports/jTable.js
/*! JTable v1.3.1.0         */ 
/*! Created By: D.J. Enzyme */ 
/*! Creation Date: 20141229 */ 
/*! Modified Date: 20150331 */
// This script requires versioning 1.0.0.0 or later;

/// <reference path="../../linq.js" />
/// <reference path="../../Sorting/advSorting.js" />
/// <reference path="../Bootstrap/Plugins/prettyProcessing.js" />
/// <reference path="../Chartv2.min.js" />
/// <reference path="../moment.js" />
/// <reference path="../Bootstrap/Plugins/rating.js" />
/// <reference path="breadCrumb.js" />
/// <reference path="../../versioning.js" />

/* global Chart */
/* global prettyProcessing */
/* global RESTFUL */
/* global Sorting */
/* global Filter */
/* global BREADCRUMB_CURRENT */
/* global UpdateCrumb */
/* jshint unused:false */
/* global moment */

var JTables = {};
JTables.Version = new Version(1, 3, 1, 0);
JTables.tableIndex = -1;

Chart.defaults.global.responsive = true;

var graphColors = [
    { color: '#8B2323', highlight: '#D04545' },
    { color: '#00688B', highlight: '#00BEFE' },
    { color: '#006400', highlight: '#008000' },
    { color: '#FBEC5D', highlight: '#FCF18B' },
    { color: '#2E0854', highlight: '#6F13CB' }
];

if (!String.prototype.root) {
    String.prototype.root = function (root) {
        /// <signature>
        /// <summary>Returns the root of the strin up to the supplied root ending.</summar>
        /// <param name="root" type="String">Grabs everything to the left of the first occurence of this string.</param>
        /// <return type="string">
        /// </signature>
        "use strict";
        var rootIndex = this.indexOf(root);
        if (rootIndex === -1) {
            return this;
        } else {
            return this.substring(0, rootIndex);
        }
    };
}
if (!Math.roundAdv) {
    Math.roundAdv = function (number, decimals) {
        /// <signature>
        /// <summary>Returns a roounded number with up to the amount of decimals supplied.</summar>
        /// <param name="number" type="Number">The number to round.</param>
        /// <param name="number" type="Number" integer="true">The number to o decimal places to round to.</param>
        /// <return type="Number">
        /// </signature>
        "use strict";
        if (isNaN(number)) {
            return 0;
        }
        return +(Math.round(number + "e+" + decimals) + "e-" + decimals);
    };
}

function JTable(tableId, minimal) {
    /// <signature>
    /// <summary>Constructs a new JTable object.</summary>
    /// <returns type="JTable" />
    /// <field name="Table" type="String">The jquery selector for the table.</field>
    /// <field name="LastFullCheck" type="moment">The last time a check for updated data was conducted.</field>
    /// <field name="SortingObject" type="Sorting">The sorting class to use for the table.</field>
    /// <field name="FilterObject" type="Filter">The filter class to use for the table.</field>
    /// <field name="Options" type="Object">A list of options.</field>
    /// <field name="Average" type="Boolean">Represents if we are looking for avarages.</field>
    /// <field name="Graph" type="Boolean">Represents if we are looking for avarages represented by graphs.</field>
    /// <field name="Headers" type="Array" elementType="JTableHeader">Array of type JTableHeader.</field>
    /// <field name="Rows" type="Array" elementType="JTableRow">Array of type JTableRow.</field>
    /// <field name="Parent" type="String">The name of the parent owning the table.</field>
    /// <field name="Name" type="String">The name of the table.</field>
    /// <field name="Description" type="String">A description of the table</field>
    /// <field name="Id" type="String">The id of the table.</field>
    /// <field name="RecordsPerPage" type="Number">The number of records per page.</field>
    /// <field name="TotalRecords" type="Number">The total number of records.</field>
    /// <field name="Sortings" type="Array" elementType="JTableSorting">Array of type JTableSorting.</field>
    /// <field name="Filters" type="Array" elementType="JTableFilter">Array of type JTableFilter.</field>
    /// <field name="Page" type="Number">The page currently being viewed.</field>
    /// <field name="Filtered" type="Boolean">Represents the filtered state of the table.</field>
    /// <field name="Sorted" type="Boolean">Represents the sorting state of the table.</field>
    /// <field name="SavedId" type="String">The id of the saved file.</field>";
    /// <field name="Favorite" type="Boolean">Is the report a favorite.</field>";
    /// <field name="FilteredRows" type="Array" elementType="JTableRow">Array of type JTableRow that has been filtered from the original Rows.</field>
    /// </signature>
    "use strict";
    var tableIndex = ++JTables.tableIndex;
    var min_layout = false;
    if (typeof (minimal) === 'boolean') {
        min_layout = minimal;
    }
    this.Table = tableId;
    if (typeof (tableId) === 'undefined' || tableId == null) {
        this.Table = '#jTable' + tableIndex;
    }
    var thisTable = this;
    var currentHeader = '';
    //#region events
    if (!min_layout) {
        $('body').append(GenerateHeaderDiv('headerSelection' + tableIndex));
        $('#headerSelection' + tableIndex).find('.add-header').on('click', function (e) {
            e.preventDefault();
            var t_modal = $('#headerSelection' + tableIndex);
            t_modal.find('.header-available:checked').each(function () {
                var t_id = $(this).val();
                var found = false;
                for (var j = 0; j < thisTable.Headers.length; j++) {
                    var t_header = thisTable.Headers[j];
                    if (t_header.Id === t_id) {
                        found = true;
                        t_header.Removed = false;
                    }
                }
                if (!found) {
                    return;
                }
                var t_label = $(this).closest('label');
                t_label.find('input').removeClass('header-available').addClass('header-selected');
                t_label.parent().appendTo('.headers-container-selected');
                $('.headers-container-selected .header-move').show();
                $('.headers-container-selected .header-div').first().find('.header-up').hide();
                $('.headers-container-selected .header-div').last().find('.header-down').hide();
            });
            t_modal.find('.header-item').prop('checked', false);
        });
        $('#headerSelection' + tableIndex).find('.remove-header').on('click', function (e) {
            e.preventDefault();
            var t_modal = $('#headerSelection' + tableIndex);
            $('.header-selected:checked').each(function () {
                var t_id = $(this).val();
                var found = false;
                for (var j = 0; j < thisTable.Headers.length; j++) {
                    var t_header = thisTable.Headers[j];
                    if (t_header.Id === t_id) {
                        found = true;
                        t_header.Removed = true;
                    }
                }
                if (!found) {
                    return;
                }
                var t_label = $(this).closest('label');
                t_label.find('input').removeClass('header-selected').addClass('header-available');
                t_label.parent().appendTo('.headers-container-available');
                t_label.parent().find('.header-move').hide();
            });
            $('.header-item').prop('checked', false);
        });
        $('#headerSelection' + tableIndex).on('show.bs.modal', function () {
            var t_this = $(this);
            t_this.find('.headers-container-selected').html('');
            t_this.find('.headers-container-available').html('');
            var headers = thisTable.Headers.sort(function (a, b) { return a.Order - b.Order; });
            for (var i = 0; i < headers.length; i++) {
                var t_header = headers[i];
                if (t_header.Id === 'confirmation') {
                    t_this.find('.headers-container-selected').append('<label style="display: none;"><input type="checkbox" value="' + t_header.Id + '" class="header-item header-selected" />' + t_header.Label + '</label>');
                    continue;
                }
                if (t_header.Removed) {
                    t_this.find('.headers-container-available').append('<div class="header-div" data-id="' + t_header.Id + '"><label><input type="checkbox" value="' + t_header.Id + '" class="header-item header-available" />' + t_header.Label + '</label> <span class="header-move header-up cursor-pointer glyphicon glyphicon-arrow-up" data-id="' + t_header.Id + '"></span> <span class="header-move header-down cursor-pointer glyphicon glyphicon-arrow-down" data-id="' + t_header.Id + '"></span></div>');
                } else {
                    t_this.find('.headers-container-selected').append('<div class="header-div" data-id="' + t_header.Id + '"><label><input type="checkbox" value="' + t_header.Id + '" class="header-item header-selected" />' + t_header.Label + '</label> <span class="header-move header-up cursor-pointer glyphicon glyphicon-arrow-up" data-id="' + t_header.Id + '"></span> <span class="header-move header-down cursor-pointer glyphicon glyphicon-arrow-down" data-id="' + t_header.Id + '"></span></div>');
                }
            }
            t_this.find('.headers-container-available .header-move').hide();
            t_this.find('.headers-container-selected .header-div').first().find('.header-up').hide();
            t_this.find('.headers-container-selected .header-div').last().find('.header-down').hide();
            $('.header-move').on('click', function () {
                var item = $(this);
                var id = item.attr('data-id');
                var t_div = $(this).parent();
                var prev = t_div.prev();
                var next = t_div.next();
                if (item.hasClass('header-up')) {
                    if (prev.length === 0) {
                        return;
                    }
                    t_div.insertBefore(prev);
                } else {
                    if (next.length === 0) {
                        return;
                    }
                    t_div.insertAfter(next);
                }
                $('.headers-container-selected .header-move').show();
                $('.headers-container-selected .header-div').first().find('.header-up').hide();
                $('.headers-container-selected .header-div').last().find('.header-down').hide();
            });
        });
        $('#headerSelection' + tableIndex).find('.headers-set').on('click', function () {
            $(this).closest('.modal').modal('hide');
            var t_order = 0;
            $('.headers-container-selected .header-div').each(function () {
                var header = FindHeader(thisTable, $(this).attr('data-id'));
                if (header !== null) {
                    header.Order = ++t_order;
                }
            });
            $('.headers-container-available .header-div').each(function () {
                var header = FindHeader(thisTable, $(this).attr('data-id'));
                if (header !== null) {
                    header.Order = ++t_order;
                }
            });
            thisTable.GetPage();
        });
        $('.jTable_headers-edit[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            $('#headerSelection' + tableIndex).modal('show');
        });
        $('.jTable_pageLeft[data-jtable-target="' + this.Table + '"]').on('click', function () {
            if (thisTable.Page === 1) {
                return;
            }
            thisTable.Page--;
            $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').val(thisTable.Page);
            thisTable.GetPage();
        });
        $('.jTable_pageRight[data-jtable-target="' + this.Table + '"]').on('click', function () {
            if (thisTable.Page === thisTable.TotalPages()) {
                return;
            }
            thisTable.Page++;
            $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').val(thisTable.Page);
            thisTable.GetPage();
        });
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Page = parseInt($(this).val());
            thisTable.GetPage();
        });
        $('.jTable_recordsPerPage[data-jtable-target="' + this.Table + '"]').on('blur', function () {
            var val = $(this).val();
            if (isNaN(val)) {
                $(this).val(25);
                thisTable.RecordsPerPage = 25;
                thisTable.GetPage();
            } else {
                thisTable.RecordsPerPage = parseInt(val);
                thisTable.GetPage();
            }
        });
        $('.jTable_average[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Average = $(this).prop('checked');
            thisTable.GetPage();
        });
        $('.jTable_graph[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Graph = $(this).prop('checked');
            thisTable.GetPage();
        });
        $('.jTable_count[data-jtable-target="' + this.Table + '"]').on('change', function () {
            thisTable.Count = $(this).prop('checked');
            thisTable.GetPage();
        });
        $('.jTable_sortings-clearAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.Sortings = [];
            thisTable.SortingObject.Generate();
            thisTable.Sorted = false;
            thisTable.GetPage();
        });
        $('.jTable_sortings-edit[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.SortingObject.Modal.modal('show');
        });
        $('.jTable_filters-clearAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.Filters = [];
            thisTable.FilterObject.Generate();
            thisTable.Filtered = false;
            thisTable.GetPage();
        });
        $('.jTable_filters-edit[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            thisTable.FilterObject.Modal.modal('show');
        });
        $('.jTable_favorite[data-jtable-target="' + this.Table + '"]').on('click', function () {
            if (thisTable.Favorite) {
                $(this).html('<span class="glyphicon glyphicon-star-empty"></span> Favorite');
                thisTable.Favorite = false;
            } else {
                $(this).html('<span class="glyphicon glyphicon-star"></span> Un-Favorite');
                thisTable.Favorite = true;
            }
        });
        $('.jTable_headers-clearAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            for (var i = 0; i < thisTable.Headers.length; i++) {
                var t_header = thisTable.Headers[i];
                if (t_header.Id === 'confirmation') {
                    continue;
                }
                t_header.Removed = true;
            }
            thisTable.GetPage();
        });
        $('.jTable_headers-addAll[data-jtable-target="' + this.Table + '"]').on('click', function (e) {
            e.preventDefault();
            for (var i = 0; i < thisTable.Headers.length; i++) {
                var t_header = thisTable.Headers[i];
                t_header.Removed = false;
            }
            thisTable.GetPage();
        });
    }
    //#endregion
    //#region fields
    this.LastFullCheck = moment();
    this.SortingObject = new Sorting(this, null, min_layout);
    this.FilterObject = new Filter(this, null, min_layout);
    this.Options = {};
    this.Average = false;
    this.Graph = false;
    this.Count = false;
    this.Headers = [];
    this.Rows = [];
    this.Parent = 'New Table';
    this.Name = 'New Table';
    this.Description = '';
    this.Id = '';
    this.RecordsPerPage = 250;
    this.TotalRecords = 0;
    this.Sortings = [];
    this.Filters = [];
    this.Page = 1;
    this.Filtered = false;
    this.Sorted = false;
    this.SavedId = null;
    this.Favorite = false;
    this.FilteredRows = [];
    //#endregion
    //#region functions
    this.FilteredRecords = function () {
        /// <signature>
        /// <summary>Gets the total amount of filtered records.</summary>
        /// <returns type="Number" integer="true" />
        /// </signature>
        return this.FilteredRows.length;
    };
    this.TotalPages = function () {
        /// <signature>
        /// <summary>Gets the total amount of pages available</summary>
        /// <returns type="Number" />
        /// </signature>
        var pages = Math.ceil(this.FilteredRows.length / this.RecordsPerPage);
        if (pages === 0) {
            return 1;
        }
        return pages;
    };
    this.OnGetComplete = function (p_table) {
        /// <signature>
        /// <summary>Runs when the table has been rendered to the screen.</summary>
        /// </signature>
    };
    this.GetPage = function (table, page) {
        /// <signature>
        /// <summary>Get the rows for a certain page.</summary>
        /// <param name="table" type="String">The selector for the table tag to fill.</param>
        /// <param name="page" type="Number">The page to get.</param>
        /// </signature>
        if (typeof (table) === 'undefined') {
            table = this.Table;
        }
        if (!/^\./.test(table) && !/^#/.test(table)) {
            table = '#' + table;
        }
        if (typeof (page) === 'undefined') {
            page = this.Page;
        }
        if (!this.Filtered) {
            this.Filter();
        }
        if (!this.Sorted) {
            this.Sort();
        }
        if (page > this.TotalPages() && page !== 1) {
            page = 1;
        }
        var chartData = [];
        $('.jTable_pageSelect').html(thisTable.FilteredRecords());
        $('.jTable_pageSelect').html('');
        for (var ind = 0; ind < this.TotalPages() ; ind++) {
            $('.jTable_pageSelect').append('<option value="' + (ind + 1) + '" ' + (this.Page === (ind + 1) ? 'selected="selected"' : '') + '>' + (ind + 1) + '</option>');
        }
        var start = (page - 1) * this.RecordsPerPage;
        var rows = [];
        this.Headers.sort(function (a, b) { return (a.Order - b.Order); });
        if (this.Count) {
            rows = this.GetCount(this, chartData);
        } else if (this.Average) {
            rows = this.GetAverage(this, chartData);
        } else if (this.Graph) {
            rows = this.GetGraph(this, chartData);
        } else {
            rows = this.GetNormal(this, start);
        }
        var t_html = this.GenerateTableHtml(this, rows);
        $(table).html(t_html);
        //Render Charts
        for (var i = 0; i < chartData.length; i++) {
            var template = "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=+(Math.round(((segments[i].value / total) * 100) + 'e+2')  + 'e-2')%>%<%}%></li><%}%></ul>";
            if (this.Count) {
                template = "<ul class=\"<%=name.toLowerCase()%>-legend\"><% for (var i=0; i<segments.length; i++){%><li><span style=\"background-color:<%=segments[i].fillColor%>\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=segments[i].value%><%}%></li><%}%></ul>";
            }
            var ctx = {};
            var chart = {};
            switch (chartData[i].type) {
                case 'pie':
                    ctx = $('#' + chartData[i].id).get(0).getContext("2d");
                    chart = new Chart(ctx).Pie(chartData[i].data, {
                        legendTemplate: template
                    });
                    var legend = chart.generateLegend();
                    $('#' + chartData[i].id).parent().parent().children('.legend').html(legend);
                    break;
                case 'bar':
                    var step = 10;
                    var max = 100;
                    var chartStart = 100;
                    var options = {};
                    var data = chartData[i].data;
                    if (this.Count) {
                        for (var j = 0; j < data.datasets[0].data.length; j++) {
                            if (max < data.datasets[0].data[j]) {
                                max = data.datasets[0].data[j];
                            }
                            if (chartStart > data.datasets[0].data[j]) {
                                chartStart = data.datasets[0].data[j];
                            }
                        }
                        chartStart = (chartStart - 10) - (chartStart % 10);
                        if (chartStart < 0) {
                            chartStart = 0;
                        }
                    } else {
                        chartStart = 0;
                    }
                    options.scaleOverride = true;
                    options.scaleSteps = Math.ceil((max - chartStart) / step);
                    options.scaleStepWidth = step;
                    options.scaleStartValue = chartStart;
                    ctx = $('#' + chartData[i].id).get(0).getContext("2d");
                    chart = new Chart(ctx).Bar(chartData[i].data, options);
                    break;
            }
        }
        //Run Bindings
        //Allow sorting of the table.
        this.CreateLinks();
        //Add context menu to remove items.
        this.OnGetComplete(this);
    };
    this.CreateLinks = function () {
        /// <signature>
        /// <summary>Turns links into actual clickable links instead of dead refresh links.</summary>
        /// </signature>
        //Create href links
        $(this.Table).find('a').each(function () {
            var t_a = $(this);
            if (t_a.attr('href') === '#') {
                var action = t_a.attr('data-action');
                var controller = t_a.attr('data-controller');
                if (typeof (action) === 'undefined') {
                    action = null;
                }
                if (typeof (controller) === 'undefined') {
                    controller = null;
                }
                var rawOptions = t_a.attr('data-options');
                if (typeof (rawOptions) === 'undefined') {
                    rawOptions = '{}';
                }
                var options = JSON.parse(rawOptions);
                if (action !== null && controller !== null) {
                    var t_href = window.location.origin + '/' + controller + '/' + action + '?';
                    var keys = Object.keys(options);
                    for (var j = 0; j < keys.length; j++) {
                        t_href += keys[j] + '=' + options[keys[j]];
                        if (j !== keys.length - 1) {
                            t_href += '&';
                        }
                    }
                    t_a.attr('href', t_href);
                }
            }
        });

    };
    this.AddSorting = function (sorting) {
        /// <signature>
        /// <summary>Adds a sorting to the array Sortings and sets the appropriate order number.</summary>
        /// <param name="sorting" type="JTableSorting">The JTableSorting to add to the array Sortings</param>
        /// </signature>
        sorting.Order = this.Sortings.length + 1;
        this.Sortings.push(sorting);
        this.Sorted = false;
    };
    this.ClearSortings = function () {
        /// <signature>
        /// <summary>Clears the Sortings array</summary>
        /// </signature>
        this.Sortings = [];
        this.Sorted = false;
    };
    this.ClearFilters = function () {
        /// <signature>
        /// <summary>Clears the Filters array</summary>
        /// </signature>
        this.Filters = [];
        this.Filtered = false;
    };
    this.AddFilter = function (filter) {
        /// <signature>
        /// <summary>Adds a filter to the array Filters and sets the appropriate order number.</summary>
        /// <param name="filter" type="JTableFilter">The JTableFilter to add to the array Filters</param>
        /// </signature>
        filter.Order = this.Filters.length + 1;
        this.Filters.push(filter);
        this.Filtered = false;
    };
    this.Filter = function () {
        /// <signature>
        /// <summary>Filters the data.</summary>
        /// </signature>
        this.FilteredRows = [];
        this.Filters.sort(function (a, b) { return a.Order - b.Order; });
        
        //*
        for (var i = 0; i < this.Rows.length; i++) {
            var take = false;
            var grouping = true;
            var tests = [];
            var row = this.Rows[i];
            for (var j = 0; j < this.Filters.length; j++) {
                var filter = this.Filters[j];
                var groupTest = true;
                var first = true;
                grouping = filter.GroupNext;
                var test = false;
                do {
                    if (!filter.GroupNext) {
                        grouping = false;
                    }
                    var data = null;
                    for (var si = 0; si < row.Columns.length; si++) {
                        if (row.Columns[si].HeaderId === filter.ActingOn) {
                            data = row.Columns[si];
                            break;
                        }
                    }
                    if (data !== null) {
                        test = TestValue(data, filter.Value.toLowerCase(), filter.Test, FindHeader(this, data.HeaderId));
                    }
                    switch (filter.Link) {
                        case 'and':
                            groupTest = groupTest && test;
                            break;
                        case 'or':
                            if (first) {
                                groupTest = test;
                            } else {
                                groupTest = groupTest || test;
                            }
                            break;
                        default:
                            groupTest = test;
                            break;
                    }
                    first = false;
                    if (!grouping) {
                        break;
                    }
                    j++;
                    if (j < this.Filters.length) {
                        filter = this.Filters[j];
                    } else {
                        break;
                    }
                } while (grouping);
                tests.push({ test: groupTest, link: ((j < (this.Filters.length - 1)) ? this.Filters[j + 1].Link : 'none') });
            }
            take = tests.length > 0 ? tests[0].test : true;
            for (var ind = 1; ind < tests.length; ind++) {
                switch (tests[ind - 1].link) {
                    case 'and':
                        take = take && tests[ind].test;
                        break;
                    case 'or':
                        take = take || tests[ind].test;
                        break;
                    default:
                        take = tests[ind].test;
                        break;
                }
            }
            if (take) {
                this.FilteredRows.push(row);
            }
        }
        if (this.Filters.length === 0) {
            this.FilteredRows = this.Rows;
        }
        //*/
        this.Page = 1;
        this.Filtered = true;
        this.Sort();
        $('.jTable_totalRecords[data-jtable-target="' + this.Table + '"]').html(this.FilteredRows.length);
    };
    this.Sort = function () {
        /// <signature>
        /// <summary>Sorts the data.</summary>
        /// </signature>
        if (!this.Filtered) {
            this.Filter();
        }
        if (this.Sortings.length > 0) {
            this.Sortings.sort(function (a, b) { return a.Order - b.Order; });
            var sortings = this.Sortings;
            this.FilteredRows.sort(function (a, b) {
                var index = 0;
                var result = 0;
                var t_header = FindHeader(thisTable, sortings[index].ActingOn);
                if (t_header === null) {
                    return 1;
                }
                var a_data = FindColumn(a, sortings[index].ActingOn);
                var b_data = FindColumn(b, sortings[index].ActingOn);
                var aCompValue = null;
                var bCompValue = null;
                if (a_data !== null && a_data.Value !== null) {
                    aCompValue = a_data.Value;
                }
                if (b_data !== null && b_data.Value !== null) {
                    bCompValue = b_data.Value;
                }
                if (t_header.SortByPretty) {
                    if (a_data != null) {
                        aCompValue = a_data.PrettyValue;
                    } else {
                        aCompValue = null;
                    }
                    if (b_data != null) {
                        bCompValue = b_data.PrettyValue;
                    } else {
                        bCompValue = null;
                    }
                }
                if (aCompValue === null) {
                    aCompValue = '';
                }
                if (bCompValue === null) {
                    bCompValue = '';
                }
                if (t_header.Id === 'confirmation') {
                    result = advSorting.HexSort(aCompValue, bCompValue, sortings[index].Ascending);
                } else {
                    result = advSorting.NaturalSort(aCompValue, bCompValue, sortings[index].Ascending);
                }
                if (result === 0) {
                    index++;
                    while (result === 0) {
                        if (index >= sortings.length) {
                            break;
                        }
                        a_data = FindColumn(a, sortings[index].ActingOn);
                        b_data = FindColumn(b, sortings[index].ActingOn);
                        aCompValue = a_data.Value;
                        bCompValue = b_data.Value;
                        if (t_header.SortByPretty) {
                            aCompValue = a_data.PrettyValue;
                            bCompValue = b_data.PrettyValue;
                        }
                        if (aCompValue === null) {
                            aCompValue = '';
                        }
                        if (bCompValue === null) {
                            bCompValue = '';
                        }
                        if (t_header.Id === 'confirmation') {
                            result = advSorting.HexSort(aCompValue, bCompValue, sortings[index].Ascending);
                        } else {
                            result = advSorting.NaturalSort(aCompValue, bCompValue, sortings[index].Ascending);
                        }
                        index++;
                    }
                }
                return result;
            });
        }
        for (var i = 0; i < this.FilteredRows.length; i++) {
            this.FilteredRows[i].Order = (i + 1);
        }
        this.Sorted = true;
    };
    this.GetAverage = function (table, chartData) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="chartData" type="Array">The chartData to write to for a graph.</field>
        /// </signature>
        // We are looking for averages, so we need to add them up.  This works for numbers, single selection components, and multiple selection components.
        var rows = [];
        var row = new JTableRow();
        for (var i = 0; i < table.Headers.length; i++) {
            if (table.Headers[i].Id === 'email') {
                table.Headers[i].Hidden = true;
                continue;
            } else {
                table.Headers[i].Hidden = false;
            }
            var column = new JTableColumn();
            var total = 0;
            var count = 0;
            var takeColumn = true;
            var totals = {};
            var keys = [];
            for (var ind = 0; ind < table.Headers[i].PossibleValues.length; ind++) {
                totals[table.Headers[i].PossibleValues[ind].Label] = 0;
                keys.push({ key: table.Headers[i].PossibleValues[ind].Label, total: 0 });
            }
            for (var j = 0; j < table.FilteredRows.length; j++) {
                var t_col = FindColumn(table.FilteredRows[j], table.Headers[i].Id);
                if (t_col !== null) {
                    switch (table.Headers[i].Type.root('=>')) {
                        case 'itemParent':
                            if (typeof (totals[t_col.PrettyValue]) === 'undefined') {
                                totals[t_col.PrettyValue] = 0;
                                keys.push({ key: t_col.PrettyValue, total: 0 });
                            }
                            totals[t_col.PrettyValue]++;
                            count++;
                            break;
                        case 'multipleSelection':
                            var t_values = JSON.parse(t_col.Value);
                            var t_valueDone = [];
                            for (var k = 0; k < t_values.length; k++) {
                                var t_done = false;
                                for (var m = 0; m < t_valueDone.length; m++) {
                                    if (t_valueDone[m] === t_values[k]) {
                                        t_done = true;
                                        break;
                                    }
                                }
                                if (t_done) {
                                    continue;
                                } else {
                                    t_valueDone.push(t_values[k]);
                                }
                                var t_id = t_values[k];
                                var possibleValue = null;
                                for (var l = 0; l < table.Headers[i].PossibleValues.length; l++) {
                                    if (table.Headers[i].PossibleValues[l].Id === t_id) {
                                        possibleValue = table.Headers[i].PossibleValues[l];
                                    }
                                }
                                if (possibleValue !== null) {
                                    totals[possibleValue.Label]++;
                                }
                            }
                            count++;
                            break;
                        case 'boolean':
                            if (t_col.Value === '1') {
                                totals.Yes++;
                            } else {
                                totals.No++;
                            }
                            count++;
                            break;
                        case 'number':
                        case 'rating':
                            total += parseFloat(t_col.Value);
                            count++;
                            break;
                        default:
                            takeColumn = false;
                            break;
                    }
                }
            }
            if (takeColumn) {
                column.HeaderId = table.Headers[i].Id;
                column.Id = table.Headers[i].Id;
                column.Value = total / count;
                column.PrettyValue = column.Value;
                var t_pretty = '';
                var t_data = [];
                var t_barDataSet = [];
                var t_barLabels = [];
                var c_index = -1;
                var t_html = '<table><tbody>';
                switch (table.Headers[i].Type.root('=>')) {
                    case 'multipleSelection':
                    case 'itemParent':
                    case 'boolean':
                        for (var iPind = 0; iPind < keys.length; iPind++) {
                            keys[iPind].total = totals[keys[iPind].key];
                        }
                        keys.sort(function (a, b) { return b.total - a.total; });
                        for (var iPind2 = 0; iPind2 < keys.length; iPind2++) {
                            c_index++;
                            var avg = (totals[keys[iPind2].key] / count) * 100;
                            if (c_index > 4) {
                                c_index = 0;
                            }
                            t_data.push({
                                value: totals[keys[iPind2].key],
                                color: graphColors[c_index].color,
                                highlight: graphColors[c_index].highlight,
                                label: keys[iPind2].key,
                            });
                            t_barLabels.push(keys[iPind2].key);
                            t_barDataSet.push(avg);
                            t_html += '<tr><td style="text-align: right; padding-right: 3px; font-weight: bold;">' + keys[iPind2].key + ':</td><td>' + avg.toFixed(2) + '%</td></tr>';
                        }
                        t_html += '</tbody></table>';
                        column.Value = t_html;
                        t_pretty = t_html;
                        if (table.Graph) {
                            if (table.Headers[i].Type === 'multipleSelection') {
                                var t_barData = {
                                    labels: t_barLabels,
                                    datasets: [{
                                        label: 'Selections',
                                        fillColor: "rgba(220,220,220,0.5)",
                                        strokeColor: "rgba(220,220,220,0.8)",
                                        highlightFill: "rgba(220,220,220,0.75)",
                                        highlightStroke: "rgba(220,220,220,1)",
                                        data: t_barDataSet
                                    }]
                                };
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="300" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_barData, type: 'bar' });
                            } else {
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="500" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_data, type: 'pie' });
                            }
                        }
                        column.PrettyValue = t_pretty;
                        row.Columns.push(column);
                        break;
                    case 'number':
                    case 'rating':
                        column.Value = Math.roundAdv(total / count, 2);
                        column.PrettyValue = column.Value;
                        row.Columns.push(column);
                        break;
                    default:
                        table.Headers[i].Hidden = true;
                }
            } else {
                table.Headers[i].Hidden = true;
            }
        }
        rows.push(row);
        return rows;
    };
    this.GetGraph = function (table, chartData) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="chartData" type="Array">The chartData to write to for a graph.</field>
        /// </signature>
        return this.GetAverage(table, chartData);
    };
    this.GetCount = function (table, chartData) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="chartData" type="Array">The chartData to write to for a graph.</field>
        /// </signature>
        var rows = [];
        // We are looking for counts, so we need to add them up.  This works for single selection components, and multiple selection components.
        var row = new JTableRow();
        for (var i = 0; i < table.Headers.length; i++) {
            if (table.Headers[i].Id === 'email') {
                table.Headers[i].Hidden = true;
                continue;
            } else {
                table.Headers[i].Hidden = false;
            }
            var column = new JTableColumn();
            var total = 0;
            var count = 0;
            var takeColumn = true;
            var totals = {};
            var keys = [];
            for (var ind = 0; ind < table.Headers[i].PossibleValues.length; ind++) {
                totals[table.Headers[i].PossibleValues[ind].Label] = 0;
                keys.push({ key: table.Headers[i].PossibleValues[ind].Label, total: 0 });
            }
            if (table.Headers[i].Type.root('=>') === 'rating') {
                var t_json = table.Headers[i].Type;
                t_json = t_json.split("=>")[1];
                var t_options = JSON.parse(t_json);
                for (var x = t_options.min; x < t_options.max; x += t_options.step) {
                    totals[x.toString()] = 0;
                    keys.push({ key: x.toString(), total: 0 });
                }
            }
            for (var j = 0; j < table.FilteredRows.length; j++) {
                var t_col = FindColumn(table.FilteredRows[j], table.Headers[i].Id);
                if (t_col !== null) {
                    switch (table.Headers[i].Type.root('=>')) {
                        case 'itemParent':
                        case 'rating':
                            var iP_value = t_col.PrettyValue;
                            iP_value = iP_value.replace(/(<[^>]*>[^<]*<[^>]*>)/g, '');
                            iP_value = iP_value.trim();
                            if (typeof (totals[iP_value]) === 'undefined') {
                                totals[iP_value] = 0;
                                keys.push({ key: iP_value, total: 0 });
                            }
                            totals[iP_value]++;
                            count++;
                            break;
                        case 'multipleSelection':
                            var t_values = JSON.parse(t_col.Value);
                            var t_valueDone = [];
                            for (var k = 0; k < t_values.length; k++) {
                                var t_done = false;
                                for (var m = 0; m < t_valueDone.length; m++) {
                                    if (t_valueDone[m] === t_values[k]) {
                                        t_done = true;
                                        break;
                                    }
                                }
                                if (t_done) {
                                    continue;
                                } else {
                                    t_valueDone.push(t_values[k]);
                                }
                                var t_id = t_values[k];
                                var possibleValue = null;
                                for (var l = 0; l < table.Headers[i].PossibleValues.length; l++) {
                                    if (table.Headers[i].PossibleValues[l].Id === t_id) {
                                        possibleValue = table.Headers[i].PossibleValues[l];
                                    }
                                }
                                if (possibleValue !== null) {
                                    totals[possibleValue.Label]++;
                                }
                            }
                            count++;
                            break;
                        case 'boolean':
                            if (t_col.Value === '1') {
                                totals.Yes++;
                            } else {
                                totals.No++;
                            }
                            count++;
                            break;
                        case 'number':
                            total += parseFloat(t_col.Value);
                            count++;
                            break;
                        default:
                            takeColumn = false;
                            break;
                    }
                }
            }
            if (takeColumn) {
                column.HeaderId = table.Headers[i].Id;
                column.Id = table.Headers[i].Id;
                column.Value = total;
                column.PrettyValue = column.Value;
                var t_pretty = '';
                var t_data = [];
                var t_barDataSet = [];
                var t_barLabels = [];
                var c_index = -1;
                var t_html = '<table><tbody>';
                switch (table.Headers[i].Type.root('=>')) {
                    case 'multipleSelection':
                    case 'itemParent':
                    case 'rating':
                    case 'boolean':
                        for (var rind = 0; rind < keys.length; rind++) {
                            keys[rind].total = totals[keys[rind].key];
                        }
                        if (table.Headers[i].Type.root('=>') !== 'rating') {
                            keys.sort(function (a, b) { return b.total - a.total; });
                        }
                        for (var rind2 = 0; rind2 < keys.length; rind2++) {
                            c_index++;
                            if (c_index > 4) {
                                c_index = 0;
                            }
                            t_data.push({
                                value: totals[keys[rind2].key],
                                color: graphColors[c_index].color,
                                highlight: graphColors[c_index].highlight,
                                label: keys[rind2].key,
                            });
                            t_barLabels.push(keys[rind2].key);
                            t_barDataSet.push(totals[keys[rind2].key]);
                            t_html += '<tr><td style="text-align: right; padding-right: 3px; font-weight: bold;">' + keys[rind2].key + ':</td><td>' + totals[keys[rind2].key] + '</td></tr>';
                        }
                        t_html += '</tbody></table>';
                        column.Value = t_html;
                        t_pretty = t_html;
                        if (table.Graph) {
                            if (table.Headers[i].Type === 'multipleSelection') {
                                var t_barData = {
                                    labels: t_barLabels,
                                    datasets: [{
                                        label: 'Selections',
                                        fillColor: "rgba(220,220,220,0.5)",
                                        strokeColor: "rgba(220,220,220,0.8)",
                                        highlightFill: "rgba(220,220,220,0.75)",
                                        highlightStroke: "rgba(220,220,220,1)",
                                        data: t_barDataSet
                                    }]
                                };
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="300" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_barData, type: 'bar' });
                            } else {
                                t_pretty = '<div class="labeled-chart-container"><div class="canvas-holder"><canvas id="chart_' + column.Id + '" width="500" height="300"></canvas></div><div class="legend"></div></div>';
                                chartData.push({ id: 'chart_' + column.Id, data: t_data, type: 'pie' });
                            }
                        }
                        column.PrettyValue = t_pretty;
                        row.Columns.push(column);
                        break;
                    case 'number':
                        row.Columns.push(column);
                        break;
                    default:
                        table.Headers[i].Hidden = true;
                }
            } else {
                table.Headers[i].Hidden = true;
            }
        }
        rows.push(row);
        return rows;
    };
    this.GetNormal = function (table, start) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="Array" elementType="JTableRow" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="start" type="Number" integer="true">The the record to start with.</field>
        /// </signature>
        var rows = [];
        var taken = 0;
        for (var i = start; i < table.FilteredRows.length; i++) {
            rows.push(table.FilteredRows[i]);
            taken++;
            if (taken >= table.RecordsPerPage) {
                break;
            }
        }
        return rows;
    };
    this.UpdateRow = function (table, row, skipAnimation) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="String" />
        /// <param name="table" type="JTable">The jTable we are working with.</param>
        /// <param name="row" type="JTableRow">The row to render.</param>
        /// <param name="skipAnimation" type="Boolean">Whether to skip the animation or not.</param>
        /// </signature>
        if ((table.Average || table.Graph || table.Count) && table.Headers[j].Hidden) {
            return;
        }
        var tr = $('#' + row.Id);
        if (tr.length === 0) {
            return;
        }
        if (typeof (skipAnimation) !== 'boolean') {
            skipAnimation = false;
        }
        if (this.Options.type === 'email' || this.Options.type === 'invitation') {
            skipAnimation = true;
        }
        var origBackgroundColor = tr.css('background-color');
        if (!skipAnimation) {
            tr.css('background-color', 'rgba(122,2,2,.5)');
        }
        for (var j = 0; j < table.Headers.length; j++) {
            var td = tr.find('td[data-headerid="' + table.Headers[j].Id + '"]');
            if (td.length === 0) {
                continue;
            }
            var data = FindColumn(row, table.Headers[j].Id);
            if (typeof (data) !== 'undefined' && data !== null) {
                td.html(data.PrettyValue);
            } else {
                td.html('');
            }
        }
        this.CreateLinks();
        this.OnUpdateComplete(tr);
        if (!skipAnimation) {
            setTimeout(function () { tr.animate({ backgroundColor: origBackgroundColor }, 1000, function () { tr.css('background-color', ''); }); }, 5000);
        }
    };
    this.OnUpdateComplete = function (tr) {
        /// <signature>
        /// <summary>Runs a function on an updated row.</summary>
        /// <param name="tr" type="jQuery">The tr that was updated.</param>
        /// </signature>
    };
    this.GenerateTableHtml = function (table, rows) {
        /// <signature>
        /// <summary>Gets the rows for a table of averages.</summary>
        /// <returns type="String" />
        /// <field name="table" type="JTable">The jTable we are working with.</field>
        /// <field name="rows" type="Array" elementType="JTableRow">The rows to render.</field>
        /// </signature>
        $('.jTable_parentName[data-jtable-target="' + this.Table + '"]').html(this.Parent);
        $('.jTable_name[data-jtable-target="' + this.Table + '"]').html(this.Name);
        var t_html = '<thead><tr>';
        for (var ind = 0; ind < table.Headers.length; ind++) {
            if ((table.Average || table.Graph || table.Count) && table.Headers[ind].Hidden) {
                continue;
            }
            if (table.Headers[ind].Removed) {
                continue;
            }
            var group = table.Headers[ind].Group;
            if (typeof (group) !== 'undefined' && group !== null && group !== '') {
                group = '<i>' + group + '</i><br />';
            } else {
                group = '';
            }
            t_html += '<th class="" data-header-id="' + table.Headers[ind].Id + '">' + group + table.Headers[ind].Label + '</th>';
        }
        t_html += '</tr></thead><tbody>';
        for (var i = 0; i < rows.length; i++) {
            t_html += '<tr id="' + rows[i].Id + '">';
            for (var j = 0; j < table.Headers.length; j++) {
                if ((table.Average || table.Graph || table.Count) && table.Headers[j].Hidden) {
                    continue;
                }
                if (table.Headers[j].Removed) {
                    continue;
                }
                var editable = table.Headers[j].Editable;
                if (table.Average || table.Graph || table.Count) {
                    editable = false;
                }
                var data = FindColumn(rows[i], table.Headers[j].Id);
                if (typeof (data) !== 'undefined' && data !== null) {
                    editable = editable || data.Editable;
                    var t_value = data.PrettyValue;
                    if (!this.Graph && !this.Count && !this.Average && t_value !== null && t_value.indexOf('<a') === -1 && t_value.length > 100) {
                        t_value = t_value.substr(0, 100) + "...";
                    }
                    t_html += '<td data-headerid="' + table.Headers[j].Id + '" id="' + data.Id + '" class="' + (editable ? 'editable-item cursor-pointer' : '') + '">' + t_value + '</td>';
                } else {
                    t_html += '<td data-headerid="' + table.Headers[j].Id + '" class="' + (editable ? 'editable-item cursor-pointer' : '') + '"></td>';
                }
            }
            t_html += '</tr>';
        }
        t_html += '</tbody>';
        return t_html;
    };
    this.GetPrintView = function (chartData) {
        /// <signature>
        /// <summary>Gets html that represents the table including all rows possible.</summary>
        /// <param name="chartData" type="Array">The list of chartData</param>
        /// <returns type="String" />
        /// </signature>
        if (!this.Filtered) {
            this.Filter();
        }
        if (!this.Sorted) {
            this.Sort();
        }
        var rows = [];
        if (this.Count) {
            rows = this.GetCount(this, chartData);
        } else if (this.Average) {
            rows = this.GetAverage(this, chartData);
        } else if (this.Graph) {
            rows = this.GetGraph(this, chartData);
        } else {
            for (var i = 0; i < this.FilteredRows.length; i++) {
                rows.push(this.FilteredRows[i]);
            }
        }
        var html = '<html><head><title>"' + this.Name + '"</title><script src="' + window.location.origin + '/Scripts/jquery-2.1.3.min.js"></script><script src="' + window.location.origin + '/Scripts/Chartv2.js"></script><link href="' + window.location.origin + '/Content/Bootstrap/bootstrap.min.css" rel="stylesheet" />';
        html += '<style> ';
        html += '.body { color: #3e3e3e; } ';
        html += '.report-header { position: fixed; left: 0; top: 0; width: 100%; height: 100px; overflow: hidden; padding: 10px 10px 0 10px; background: white; } ';
        html += 'img.img-report-header { display: block; width: 100%; max-width: 250px; max-height: 75px; } ';
        html += '.report-title { text-align: right; } ';
        html += '.report-content { padding-top: 100px; } ';
        html += 'table.table-full-page > thead > tr { border-top: 1px solid #ddd; } ';
        html += 'table.table-full-page > thead > tr > th { padding-right: 50px !important; white-space: nowrap; min-width: 150px; background: white; font-size: 12px; } ';
        html += 'table.table-full-page > tbody > tr { height: 75px; font-size: 12px; } ';
        html += 'table.table-full-page > tbody > tr:last-child { border-bottom: 1px solid #ddd; } ';
        html += 'table.table-full-page > tbody > tr:nth-child(even) > td { background: white; } ';
        html += 'table.table-full-page td { padding: 0 0 0 0 !important; } ';
        html += '.cell-height { height: 75px; padding: 5px; overflow-y: auto; } ';
        html += '@media screen and (max-width: 1199px) { .report-header { position: relative; height: auto; overflow: visible; } ';
        html += '.report-title { text-align: left; padding-top: 15px; } ';
        html += '.report-content { padding-top: 15px; } ';
        html += 'table.table-full-page > tbody > tr { height: auto; font-size: 12px; } ';
        html += '.cell-height { height: auto; max-height: 50px; padding: 5px; overflow-y: auto; } } ';
        html += '</style>';
        html += '</head><body>';
        html += '<div class="report-header"><div class="row"><div class="col-lg-3"><img class="img-report-header" src="https://toolkit.regstep.com/Images/Common/regstep.png"/></div>';
        html += '<div class="col-lg-9 report-title">' + ((typeof (this.Parent) !== 'undefined' && this.Parent !== null && this.Parent !== '') ? '<h3>' + this.Parent + '</h3>' : '') + '<h4>' + this.Name + '</h4>' + ((typeof (this.Description) !== 'undefined' && this.Description !== null && this.Description !== '') ? '<h5>' + this.Description + '</h5>' : '') + '</div></div></div><div class="report-content">';
        html += '<table class="table table-striped table-full-page">';
        html += this.GenerateTableHtml(this, rows);
        html += '</table><script type="text/javascript">\r\n';
        html += "//Render Charts\r\n" +
            "Chart.defaults.global.animation = false;\r\n" +
            "chartData = " + JSON.stringify(chartData) + "\r\n" +
            "for (var i = 0; i < chartData.length; i++) {\r\n" +
            "    var template = \"<ul class=\\\"<%=name.toLowerCase()%>-legend\\\"><% for (var i=0; i<segments.length; i++){%><li><span style=\\\"background-color:<%=segments[i].fillColor%>\\\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=+(Math.round(((segments[i].value / total) * 100) + 'e+2')  + 'e-2')%>%<%}%></li><%}%></ul>\";\r\n" +
            "    if (this.Count) {\r\n" +
            "        var template = \"<ul class=\\\"<%=name.toLowerCase()%>-legend\\\"><% for (var i=0; i<segments.length; i++){%><li><span style=\\\"background-color:<%=segments[i].fillColor%>\\\"></span><%if(segments[i].label){%><b><%=segments[i].label%></b>: <%=segments[i].value%><%}%></li><%}%></ul>\";\r\n" +
            "    }\r\n" +
            "    switch (chartData[i].type) {\r\n" +
            "        case 'pie':\r\n" +
            "            var ctx = $('#' + chartData[i].id).get(0).getContext('2d');\r\n" +
            "            var chart = new Chart(ctx).Pie(chartData[i].data, {\r\n" +
            "                legendTemplate: template\r\n" +
            "            });\r\n" +
            "            var legend = chart.generateLegend();\r\n" +
            "            $('#' + chartData[i].id).parent().parent().children('.legend').html(legend);\r\n" +
            "            break;\r\n" +
            "        case 'bar':\r\n" +
            "            var step = 10;\r\n" +
            "            var max = 100;\r\n" +
            "            var start = 100;\r\n" +
            "            var options = {};\r\n" +
            "            var data = chartData[i].data;\r\n" +
            "            if (this.Count) {\r\n" +
            "                for (var j = 0; j < data.datasets[0].data.length; j++) {\r\n" +
            "                    if (max < data.datasets[0].data[j]) {\r\n" +
            "                        max = data.datasets[0].data[j];\r\n" +
            "                    }\r\n" +
            "                    if (start > data.datasets[0].data[j]) {\r\n" +
            "                        start = data.datasets[0].data[j];\r\n" +
            "                    }\r\n" +
            "                }\r\n" +
            "                start = (start - 10) - (start % 10);\r\n" +
            "                if (start < 0) {\r\n" +
            "                    start = 0;\r\n" +
            "                }\r\n" +
            "            } else {\r\n" +
            "                start = 0;\r\n" +
            "            }\r\n" +
            "            options.scaleOverride = true;\r\n" +
            "            options.scaleSteps = Math.ceil((max - start) / step);\r\n" +
            "            options.scaleStepWidth = step;\r\n" +
            "            options.scaleStartValue = start;\r\n" +
            "            var ctx = $('#' + chartData[i].id).get(0).getContext('2d');\r\n" +
            "            var chart = new Chart(ctx).Bar(chartData[i].data, options);\r\n" +
            "            break;\r\n" +
            "    }\r\n" +
            "}\r\n" +
            "setInterval(function () { window.print(); }, 500);";
        html += '</script></body></html>';
        var wnd = window.open("about:blank", "", "_blank");
        wnd.document.write(html);
        return html;
    };
    this.Load = function (url, options) {
        /// <signature>
        /// <summary>Loads the table from an ajax call via XMLHttpRequest.</summary>
        /// <param name="url" type="String">The url that will be called to load the jTable data.</param>
        /// <param name="options" type="Object">The object that will be used for the get call.</param>
        /// </signature>
        if (options === null) {
            options = {};
        }
        var storageName = 'jTable: ';
        var params = '';
        if (typeof (options.id) !== 'undefined' && options.id !== null) {
            params += '?id=' + options.id;
            storageName += options.id;
        }
        if (typeof (options.type) !== 'undefined' && options.type !== null && options.type !== '') {
            params += '&type=' + options.type;
            storageName += '_' + options.type;
        }
        var t_xhr = new XMLHttpRequest();
        t_xhr.addEventListener('progress', this.UpdateProgress, false);
        url += params;
        t_xhr.open('get', url, true);
        RESTFUL.ajaxHeader(t_xhr);
        t_xhr.onload = function (event) {
            var c_xhr = event.currentTarget;
            if (c_xhr.status === 200) {
                var data = RESTFUL.parse(c_xhr);
                var table = data.Table;
                if (data.Success) {
                    thisTable.Filters = table.Filters;
                    thisTable.Sortings = table.Sortings;
                    thisTable.Rows = table.Rows;
                    thisTable.Count = table.Count;
                    thisTable.Graph = table.Graph;
                    thisTable.Average = table.Average;
                    thisTable.Description = table.Description;
                    thisTable.Name = table.Name;
                    thisTable.Parent = table.Parent;
                    thisTable.Favorite = table.Favorite;
                    thisTable.Headers = table.Headers;
                    thisTable.Id = table.Id;
                    thisTable.Options = table.Options;
                    thisTable.Page = 1;
                    thisTable.RecordsPerPage = table.RecordsPerPage;
                    thisTable.TotalRecords = table.TotalRecords;
                    thisTable.SavedId = table.SavedId;
                    thisTable.LastFullCheck = moment();
                    thisTable.UpdateView(thisTable);
                    thisTable.AfterLoad(thisTable);
                    thisTable.Filter();
                    thisTable.GetPage();
                    if (thisTable.SavedId !== null) {
                        $('.jTable_standardOnly').hide();
                    } else {
                        $('.jTable_standardOnly').show();
                    }
                } else {
                    RESTFUL.showError("Failed to load data.", "Unhandled Exception");
                }
            } else {
                RESTFUL.showError("Failed to load data.", "Unhandled Exception");
            }
            prettyProcessing.hidePleaseWait();
        };
        t_xhr.onerror = function () {
            RESTFUL.showError();
            prettyProcessing.hidePleaseWait();
        };
        prettyProcessing.showPleaseWait("Requesting Data", "Requesting data from the server.");
        t_xhr.send();
    };
    this.UpdateProgress = function (event) {
        /// <signature>
        /// <summary>Runs the event for updating progress.</summary>
        /// <param name="event" type="Event">The url that will be called to load the jTable data.</param>
        /// </signature>
        if (event.lengthComputable) {
            var percentComplete = (event.loaded / event.total) * 100;
            prettyProcessing.update("Downloading Data", "Downloading the data from the server.", percentComplete);
        } else {
            prettyProcessing.update("Downloading Data", "Downloading the data from the server.", 100);
        }
    };
    this.AfterLoad = function (p_table) {
        /// <signature>
        /// <summary> Runs after the table has been loaded from the server.</summary>
        /// <param name="p_table" type="JTable">The JTable to manipulate.</param>
        /// </signature>
        if (typeof (BREADCRUMB_VERSION) !== 'undefined' && BREADCRUMB_CURRENT !== null) {
            BREADCRUMB_CURRENT.Label = p_table.Name + ' on ' + p_table.Parent;
            UpdateCrumb(BREADCRUMB_CURRENT);
        }
    };
    this.UpdateView = function () {
        /// <signature>
        /// <summary>Updates the view to show table statistics and properties.</summary>
        /// </signature>
        $('.jTable_parentName[data-jtable-target="' + this.Table + '"]').html(this.Parent);
        $('.jTable_name[data-jtable-target="' + this.Table + '"]').html(this.Name);
        $('.jTable_recordsPerPage[data-jtable-target="' + this.Table + '"]').val(this.RecordsPerPage);
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').html('');
        this.Filter();
        this.Sort();
        for (var ind = 0; ind < this.TotalPages(); ind++) {
            $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"]').append('<option value="' + (ind + 1) + '" ' + (this.Page === (ind + 1) ? 'selected="selected"' : '') + '>' + (ind + 1) + '</option>');
        }
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"] > option').prop('selected', false);
        $('.jTable_pageSelect[data-jtable-target="' + this.Table + '"] > option[value="' + this.Page + '"]').prop('selected', true);
        if (this.SavedId === null || this.SavedId === '' || typeof (this.SavedId) === 'undefined') {
            $('.jTable_standarOnly[data-jtable-target="' + this.Table + '"]').show();
        } else {
            $('.jTable_standardOnly[data-jtable-target="' + this.Table + '"]').hide();
        }
        if (this.Favorite) {
            $('.jTable_favorite[data-jtable-target="' + this.Table + '"]').html('<span class="glyphicon glyphicon-star glyphicon-small"></span> Un-Favorite');
        } else {
            $('.jTable_favorite[data-jtable-target="' + this.Table + '"]').html('<span class="glyphicon glyphicon-star-empty glyphicon-small"></span> Favorite');
        }
        this.SortingObject.Generate();
        this.FilterObject.Generate();
    };
    this.GetAjaxData = function () {
        /// <signature>
        /// <summary>Gets data needed to submit on an ajax call. Original jTable is not modified.</summary>
        /// <returns type="Object" />
        /// </signature>
        var newTable = {};
        newTable.FilteredRows = [];
        newTable.Rows = [];
        newTable.Filters = this.Filters;
        newTable.Sortings = this.Sortings;
        newTable.Id = this.Id;
        newTable.SavedId = this.SavedId;
        newTable.Count = this.Count;
        newTable.Graph = this.Graph;
        newTable.Average = this.Average;
        newTable.Name = this.Name;
        newTable.Description = this.Description;
        newTable.Options = this.Options;
        newTable.Headers = this.Headers;
        newTable.Favorite = this.Favorite;
        newTable.Parent = this.Parent;
        return newTable;
    };
    //#endregion
}

//#region Classes
function JTableSorting() {
    /// <signature>
    /// <summary>Constructs a new JTableSorting object.</summary>
    /// <returns type="JTableSorting" />
    /// </signature>
    /// <field name="ActingOn" type="String">The header id that is being sorted.</field>
    /// <field name="Ascending" type="Boolean">A boolean value representing ascending sorting. default: true</field>
    /// <field name="Order" type="Number">The order of the sorting.</field>
    /// <signature>
    "use strict";
    this.ActingOn = '';
    this.Ascending = true;
    this.Order = 1;
}

function JTableFilter() {
    /// <signature>
    /// <summary>Constructs a new JTableFilter object.</summary>
    /// <returns type="JTableFilter" />
    /// </signature>
    /// <field name="ActingOn" type="String">The header id that is being sorted.</field>
    /// <field name="Test" type="String">A string representing the type of test. default: '=='</field>
    /// <field name="Value" type="String">A string representing the value to test against. default: 'true'</field>
    /// <field name="Order" type="Number">The order of the sorting.</field>
    /// <field name="Link" type="String">The link for multiple filters.</field>
    /// <field name="GroupNext" type="Boolean">Boolean representing if it is grouped with the next filter. default: false</field>
    /// <signature>
    "use strict";
    this.ActingOn = '';
    this.Test = '==';
    this.Value = '';
    this.Order = 1;
    this.Link = null;
    this.GroupNext = false;
    this.CaseSensitive = false;
}

function JTableHeader() {
    /// <signature>
    /// <summary>Creates a new table header object.</summary>
    /// <returns type="JTableHeader" />
    /// <field name="Label" type="String">The label of the header for the table.</field>
    /// <field name="Id" type="String">The unique id of the header for searching, sorting, and filtering.</field>
    /// <field name="Order" type="Number">The order of the header for the table.</field>
    /// <field name="Type" type="String">The type of the header.</field>
    /// <field name="PossibleValues" type="Array" elementType="JTableHeaderPossibleValue">The possible values for mulitpleSelections and itemParent.</field>
    /// <field name="Removed" type="Boolean">If the header is removed or not.</field>
    /// <field name="SortByPretty" type="Boolean">If the header is to be sorted by its pretty value or raw value.</field>
    /// </signature>
    "use strict";
    this.Label = '';
    this.Id = '';
    this.Order = 1;
    this.Hidden = false;
    this.Group = '';
    this.Type = 'text';
    this.PossibleValues = [];
    this.Editable = false;
    this.Removed = false;
    this.SortByPretty = false;
}

function JTableHeaderPossibleValue() {
    /// <signature>
    /// <summary>Creates a new JTableHeaderPossibleValue.</summary>
    /// <field name="Id" type="String">The id of the value.</field>
    /// <field name="Label" type="String">The label of the value.</field>
    /// </signature>
    "use strict";
    this.Id = '';
    this.Label = '';
}

function JTableRow() {
    /// <signature>
    /// <summary>Creates a new JTableRow</summary>
    /// <returns type="JTableRow" />
    /// <field name="Id" type="String">The unique identifier for searching.</field>
    /// <field name="Columns" type="Array" elementType="JTableColumn">An array of the columns that correspond to headers.</field>
    /// <field name="Order" type="Number">The order of the elements to sort.</field>
    /// <field name="PreviousSort" type="Number">The qualifier result of the last sorting method.</field>
    /// </signature>
    "use strict";
    this.Id = '';
    this.Columns = [];
    this.Order = 1;
    this.PreviousSort = 0;
}

function JTableColumn() {
    /// <signature>
    /// <summary>Creates a new JTableColumn</summary>
    /// <returns type="JTableColumn" />
    /// <field name="HeaderId" type="String">The unique identifier pertaining to the Header it corresponds to.</field>
    /// <field name="PrettyValue" type="String">The pretty value of the column.</field>
    /// <field name="Id" type="String">The unique identifier of the column.</field>
    /// <field name="Value" type="String">The raw value of the column.</field>
    /// <field name="Type" type="String">The type of value</field>
    /// <field name="Editable" type="Boolean">Whether the field can be edited.</field>
    /// </signature>
    "use strict";
    this.HeaderId = '';
    this.PrettyValue = '';
    this.Id = '';
    this.Value = '';
    this.Editable = true;
}

function TestValue(data, testValue, test, header) {
    /// <signature>
    /// <summary>Tests a value against another according to a test</summary>
    /// <returns type="Boolean" />
    /// <param name="data" type="JTableColumn">The value of the data.</field>
    /// <param name="testValue" type="String">The value to test against.</field>
    /// <param name="test" type="String">The type of test to perform.</field>
    /// <param name="header" type="JTableHeader">The header.</field>
    /// </signature>
    "use strict";
    var type = header.Type;
    var t_value = "";
    var found = false;
    if (data.Value === null) {
        data.Value = "";
    }
    if (testValue === null) {
        testValue = "";
    }
    if (type === 'string' || type === 'text') {
        switch (test) {
            case '==':
                return data.Value.toLowerCase() === testValue.toLowerCase();
            case '!=':
                return data.Value.toLowerCase() !== testValue.toLowerCase();
            case '>':
                return data.Value.length > parseInt(testValue);
            case '>=':
                return data.Value.length >= parseInt(testValue);
            case '<':
                return data.Value.lenth < parseInt(testValue);
            case '<=':
                return data.Value.length <= parseInt(testValue);
            case '*=':
                return data.Value.toLowerCase().indexOf(testValue) !== -1;
            case '!*=':
                return data.Value.toLowerCase().indexOf(testValue) === -1;
            case '^=':
                return data.Value.toLowerCase().indexOf(testValue) === 0;
            case '!^=':
                return data.Value.toLowerCase().indexOf(testValue) !== 0;
            case '$=':
                return data.Value.toLowerCase().indexOf(testValue, data.Value.length - testValue.length) !== -1;
            case '!$=':
                return data.Value.toLowerCase().indexOf(testValue, data.Value.length - testValue.length) === -1;
            case '=rgx=':
                var t_rgx = new RegExp(testValue, 'i');
                return t_rgx.test(data.Value);
            case '!=rgx=':
                var t2_rgx = new RegExp(testValue, 'i');
                return !t2_rgx.test(data.Value);
        }
    } else if (type === 'number' || type.indexOf('rating') === 0) {
        t_value = parseFloat(data.Value);
        var t_testValue = parseFloat(testValue);
        switch (test) {
            case '==':
                return t_value === t_testValue;
            case '!=':
                return t_value !== t_testValue;
            case '>':
                return t_value > t_testValue;
            case '>=':
                return t_value >= t_testValue;
            case '<':
                return t_value < t_testValue;
            case '<=':
                return t_value <= t_testValue;
        }
    } else if (type === 'multipleSelection') {
        t_value = JSON.parse(data.Value);
        found = false;
        switch (test) {
            case '==':
                return (t_value.length === 1) && (t_value[0] === testValue);
            case '!=':
                for (var si = 0; si < t_value.length; si++) {
                    if (t_value[si] === testValue) {
                        found = true;
                        break;
                    }
                }
                return !found;
            case 'in':
                for (var si2 = 0; si2 < t_value.length; si2++) {
                    if (t_value[si2] === testValue) {
                        found = true;
                        break;
                    }
                }
                return found;
            case 'notin':
                for (var si3 = 0; si3 < t_value.length; si3++) {
                    if (t_value[si3] === testValue) {
                        found = true;
                        break;
                    }
                }
                return !found;
        }
    } else if (type === 'itemParent') {
        found = false;
        switch (test) {
            case '==':
                return data.Value === testValue;
            case '!=':
                return data.Value !== testValue;
        }
    } else if (type === 'date') {
        t_value = moment(data.Value);
        var t_value2 = moment(testValue);
        switch (test) {
            case '==':
                return t_value.isSame(t_value2);
            case '!=':
                return !t_value.isSame(t_value2);
            case '>':
                return t_value.isAfter(t_value2);
            case '>=':
                return t_value.isSame(t_value2) || t_value.isAfter(t_value2);
            case '<':
                return t_value.isBefore(t_value2);
            case '<=':
                return t_value.isSame(t_value2) || t_value.isBefore(t_value2);
        }
    } else if (type === 'boolean') {
        switch (test) {
            case '==':
                return data.Value === testValue;
            case '!=':
                return data.Value !== testValue;
        }
    }
    return false;
}

function ReplaceRow(table, row) {
    /// <signature>
    /// <summary>Finds a specific row from a JTable and replaces it with the supplied one.</summary>
    /// <returns type="JTableRow" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="row" type="JTableRow">The row id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Rows.length; i++) {
        if (table.Rows[i].Id === row.Id) {
            table.Rows[i] = row;
            return true;
        }
    }
    table.Rows.push(row);
    return false;
}

function ReplaceHeader(table, header) {
    /// <signature>
    /// <summary>Finds a specific header from a JTable and replaces it with the supplied one.</summary>
    /// <returns type="JTableRow" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="row" type="JTableHeader">The row id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Headers.length; i++) {
        if (table.Headers[i].Id === header.Id) {
            table.Headers[i] = header;
            return;
        }
    }
    table.Headers.push(header);
}

function FindColumn(row, headerId) {
    /// <signature>
    /// <summary>Finds a specific column form a JTableRow</summary>
    /// <returns type="JTableColumn" />
    /// <param name="row" type="JTableRow">The JTableRow to search through.</param>
    /// <param name="headerId" type="String">The header id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < row.Columns.length; i++) {
        if (row.Columns[i].HeaderId === headerId) {
            return row.Columns[i];
        }
    }
    return null;
}

function FindRow(table, rowId) {
    /// <signature>
    /// <summary>Finds a specific row form a JTable.</summary>
    /// <returns type="JTableRow" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="rowId" type="String">The row id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Rows.length; i++) {
        if (table.Rows[i].Id === rowId) {
            return table.Rows[i];
        }
    }
    return null;
}

function FindHeader(table, headerId) {
    /// <signature>
    /// <summary>Finds a specific header form a JTable</summary>
    /// <returns type="JTableHeader" />
    /// <param name="table" type="JTable">The JTable to search through.</param>
    /// <param name="headerId" type="String">The header id to search for.</param>
    /// </signature>
    "use strict";
    for (var i = 0; i < table.Headers.length; i++) {
        if (table.Headers[i].Id === headerId) {
            return table.Headers[i];
        }
    }
    return null;
}

function GenerateHeaderDiv(id, tableId) {
    /// <signature>
    /// <returns type="String" />
    /// <summary>Generates the html needed to make a modal for manipulating headers.</summary>
    /// <param name="id" type="Stirng">The id of the modal</param>
    /// </signature>
    "use strict";
    var height = $(window).innerHeight();
    height *= 0.72;
    var t_html = '<div class="modal fade" id="' + id + '" data-jtable-target="' + tableId + '"><div class="modal-dialog modal-fill"><div class="modal-header"><h3 class="modal-title">Report Fields</h3></div>';
    t_html += '<div class="modal-body" style="max-height: ' + height + 'px;"><div class="row"><div class="col-md-5 header-selection-window"><div class="headers-container headers-available"><div class="headers-title">Available Fields</div><div class="headers-container-available"></div></div></div>';
    t_html += '<div class="col-md-2 header-selection-buttons"><div class="headers-commands"><a href="#" class="add-header">Add <span class="glyphicon glyphicon-chevron-right glyphicon-small"></span></a><br /><br /><a href="#" class="remove-header"><span class="glyphicon glyphicon-chevron-left glyphicon-small"></span></span> Remove</a></div></div>';
    t_html += '<div class="col-md-5 header-selection-window"><div class="headers-container headers-selected"><div class="headers-title">Included Fields</div><div class="headers-container-selected"></div></div></div>';
    t_html += '</div></div>';
    t_html += '<div class="modal-footer"><button type="button" class="btn btn-default headers-set">Set</button></div></div></div>';
    return t_html;
}
//#endregion

//#region Array Values
var JTestString = [
    '==',
    '>',
    '>=',
    '<',
    '<=',
    '!=',
    '^=',
    '!^=',
    '$=',
    '!$=',
    '*=',
    '!*=',
    '=rgx=',
    '!=rgx=',
    'in',
    'notIn'
];

var JLinkString = [
    'none',
    'and',
    'or'
];
//#endregion
