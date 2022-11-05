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