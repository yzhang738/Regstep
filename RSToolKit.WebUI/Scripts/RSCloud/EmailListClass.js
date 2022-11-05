function CustomerEmailList() {
    this.Emails = [];
    this.Add = function (customer) {
        var index = this.Emails.binarySortLocation(customer);
        if (index == -1) {
            return;
        } else {
            this.splice(index, 0, customer);
        }
    };

    this.Remove = function (customer) {
        var index = this.Emails.binarySearch(customer);
        if (index == -1) return;
        this.Emails.splice(index, 1);
    };

    this.Emails.binarySearch = function (searchElement) {
        var minIndex = 0;
        var maxIndex = this.length - 1;
        var currentIndex;
        var currentElement;

        while (minIndex <= maxIndex) {
            currentIndex = (minIndex + maxIndex) / 2 | 0;
            currentElement = this[currentIndex];

            if (currentElement.UId.localCompare(searchElement) < 0) {
                minIndex = currentIndex + 1;
            }
            else if (currentElement.UId.localCompare(searchElement) > 0) {
                maxIndex = currentIndex - 1;
            }
            else {
                return currentIndex;
            }
        }
        return -1;
    };

    this.Emails.binarySortLocation = function (searchElement) {
        return InverseBinarySearch(this, searchElement, 0, this.length - 1);
    };

    function InverseBinarySearch(values, target, min, max) {
        var mid = Math.floor((0 + max) / 2);

        if (values.length == 0)
            return 0;

        if (min > max) {
            return min;
        } else if (max == min) {
            if (mid < values.length && target.UId == values[mid].UId)
                return -1;
            if (mid >= values.length || target.UId.localCompare(values[mid].UId))
                return max;
            return max + 1;
        }

        if (target > values[mid])
            return this.Emails.InverseBinarySearch(values, target, mid + 1, max);
        else if (target < values[mid])
            return InverseBinarySearch(values, target, min, mid - 1);
        return -1;
    };
}

function CustomerEmail(uid) {
    this.UId = uid;
    this.Email = "";
}