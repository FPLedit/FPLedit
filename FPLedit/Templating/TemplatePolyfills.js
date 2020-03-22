// polyfill some generic linq methods
Array.prototype.last = function(predicate) {
    if (predicate == null) {
        return this.slice(-1)[0];
    }
    return this.matches(predicate).last();
};

Array.prototype.first = function(predicate) {
    if (predicate == null) {
        return this[0];
    }
    return this.matches(predicate).first();
};

Array.prototype.takeWhile = function(predicate) {
    if (predicate == null) {
        return this;
    }
    for (var i = 0; i < this.length; i++) {
        if (!predicate(this[i]))
            return this.slice(0, i); // from 0 to i-1
    }
    return this;
};

Array.prototype.skipWhile = function(predicate) {
    if (predicate == null) {
        return this;
    }
    for (var i = 0; i < this.length; i++) {
        if (!predicate(this[i]))
            return this.slice(i); // from i to end
    }
    return this;
};

Array.prototype.skip = function(count) {
    return this.slice(count);
};

Array.prototype.take = function(count) {
    return this.slice(0, count);
};

// Dictionary helper
function dict_last(dict, predicate) {
    var enumerator = dict.GetEnumerator();
    var result = {};
    while (enumerator.MoveNext()) {
        var key = enumerator.Current.Key;
        var value = enumerator.Current.Value;
        if (predicate(key, value))
            result = { key: key, value: value };
    }
    return result;
}

// String helpers
var s_isNullOrEmpty = function(s) {
    return s === '' || s === undefined || s == null;
};
