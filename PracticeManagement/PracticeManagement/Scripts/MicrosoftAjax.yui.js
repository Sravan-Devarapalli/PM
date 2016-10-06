﻿//----------------------------------------------------------
// Copyright (C) Microsoft Corporation. All rights reserved.
//----------------------------------------------------------
// MicrosoftAjax.js
Function.__typeName = "Function";
Function.__class = true;
Function.createCallback = function(b, a) {
    return function() {
        var e = arguments.length;
        if (e > 0) {
            var d = [];
            for (var c = 0; c < e; c++) d[c] = arguments[c];
            d[e] = a;
            return b.apply(this, d)
        }
        return b.call(this, a)
    }
};
Function.createDelegate = function(a, b) {
    return function() {
        return b.apply(a, arguments)
    }
};
Function.emptyFunction = Function.emptyMethod = function() { };
Function._validateParams = function(e, c) {
    var a;
    a = Function._validateParameterCount(e, c);
    if (a) {
        a.popStackFrame();
        return a
    }
    for (var b = 0; b < e.length; b++) {
        var d = c[Math.min(b, c.length - 1)],
            f = d.name;
        if (d.parameterArray) f += "[" + (b - c.length + 1) + "]";
        a = Function._validateParameter(e[b], d, f);
        if (a) {
            a.popStackFrame();
            return a
        }
    }
    return null
};
Function._validateParameterCount = function(e, a) {
    var c = a.length,
        d = 0;
    for (var b = 0; b < a.length; b++) if (a[b].parameterArray) c = Number.MAX_VALUE;
    else if (!a[b].optional) d++;
    if (e.length < d || e.length > c) {
        var f = Error.parameterCount();
        f.popStackFrame();
        return f
    }
    return null
};
Function._validateParameter = function(c, a, h) {
    var b, g = a.type,
        l = !!a.integer,
        k = !!a.domElement,
        m = !!a.mayBeNull;
    b = Function._validateParameterType(c, g, l, k, m, h);
    if (b) {
        b.popStackFrame();
        return b
    }
    var e = a.elementType,
        f = !!a.elementMayBeNull;
    if (g === Array && typeof c !== "undefined" && c !== null && (e || !f)) {
        var j = !!a.elementInteger,
            i = !!a.elementDomElement;
        for (var d = 0; d < c.length; d++) {
            var n = c[d];
            b = Function._validateParameterType(n, e, j, i, f, h + "[" + d + "]");
            if (b) {
                b.popStackFrame();
                return b
            }
        }
    }
    return null
};
Function._validateParameterType = function(b, c, k, j, h, d) {
    var a;
    if (typeof b === "undefined") if (h) return null;
    else {
        a = Error.argumentUndefined(d);
        a.popStackFrame();
        return a
    }
    if (b === null) if (h) return null;
    else {
        a = Error.argumentNull(d);
        a.popStackFrame();
        return a
    }
    if (c && c.__enum) {
        if (typeof b !== "number") {
            a = Error.argumentType(d, Object.getType(b), c);
            a.popStackFrame();
            return a
        }
        if (b % 1 === 0) {
            var e = c.prototype;
            if (!c.__flags || b === 0) {
                for (var g in e) if (e[g] === b) return null
            } else {
                var i = b;
                for (var g in e) {
                    var f = e[g];
                    if (f === 0) continue;
                    if ((f & b) === f) i -= f;
                    if (i === 0) return null
                }
            }
        }
        a = Error.argumentOutOfRange(d, b, String.format(Sys.Res.enumInvalidValue, b, c.getName()));
        a.popStackFrame();
        return a
    }
    if (j && b !== window && b !== document && !(window.HTMLElement && b instanceof HTMLElement) && typeof b.nodeName !== "string") {
        a = Error.argument(d, Sys.Res.argumentDomElement);
        a.popStackFrame();
        return a
    }
    if (c && !c.isInstanceOfType(b)) {
        a = Error.argumentType(d, Object.getType(b), c);
        a.popStackFrame();
        return a
    }
    if (c === Number && k) if (b % 1 !== 0) {
        a = Error.argumentOutOfRange(d, b, Sys.Res.argumentInteger);
        a.popStackFrame();
        return a
    }
    return null
};
Error.__typeName = "Error";
Error.__class = true;
Error.create = function(d, b) {
    var a = new Error(d);
    a.message = d;
    if (b) for (var c in b) a[c] = b[c];
    a.popStackFrame();
    return a
};
Error.argument = function(a, c) {
    var b = "Sys.ArgumentException: " + (c ? c : Sys.Res.argument);
    if (a) b += "\n" + String.format(Sys.Res.paramName, a);
    var d = Error.create(b, {
        name: "Sys.ArgumentException",
        paramName: a
    });
    d.popStackFrame();
    return d
};
Error.argumentNull = function(a, c) {
    var b = "Sys.ArgumentNullException: " + (c ? c : Sys.Res.argumentNull);
    if (a) b += "\n" + String.format(Sys.Res.paramName, a);
    var d = Error.create(b, {
        name: "Sys.ArgumentNullException",
        paramName: a
    });
    d.popStackFrame();
    return d
};
Error.argumentOutOfRange = function(c, a, d) {
    var b = "Sys.ArgumentOutOfRangeException: " + (d ? d : Sys.Res.argumentOutOfRange);
    if (c) b += "\n" + String.format(Sys.Res.paramName, c);
    if (typeof a !== "undefined" && a !== null) b += "\n" + String.format(Sys.Res.actualValue, a);
    var e = Error.create(b, {
        name: "Sys.ArgumentOutOfRangeException",
        paramName: c,
        actualValue: a
    });
    e.popStackFrame();
    return e
};
Error.argumentType = function(d, c, b, e) {
    var a = "Sys.ArgumentTypeException: ";
    if (e) a += e;
    else if (c && b) a += String.format(Sys.Res.argumentTypeWithTypes, c.getName(), b.getName());
    else a += Sys.Res.argumentType;
    if (d) a += "\n" + String.format(Sys.Res.paramName, d);
    var f = Error.create(a, {
        name: "Sys.ArgumentTypeException",
        paramName: d,
        actualType: c,
        expectedType: b
    });
    f.popStackFrame();
    return f
};
Error.argumentUndefined = function(a, c) {
    var b = "Sys.ArgumentUndefinedException: " + (c ? c : Sys.Res.argumentUndefined);
    if (a) b += "\n" + String.format(Sys.Res.paramName, a);
    var d = Error.create(b, {
        name: "Sys.ArgumentUndefinedException",
        paramName: a
    });
    d.popStackFrame();
    return d
};
Error.format = function(a) {
    var c = "Sys.FormatException: " + (a ? a : Sys.Res.format),
        b = Error.create(c, {
            name: "Sys.FormatException"
        });
    b.popStackFrame();
    return b
};
Error.invalidOperation = function(a) {
    var c = "Sys.InvalidOperationException: " + (a ? a : Sys.Res.invalidOperation),
        b = Error.create(c, {
            name: "Sys.InvalidOperationException"
        });
    b.popStackFrame();
    return b
};
Error.notImplemented = function(a) {
    var c = "Sys.NotImplementedException: " + (a ? a : Sys.Res.notImplemented),
        b = Error.create(c, {
            name: "Sys.NotImplementedException"
        });
    b.popStackFrame();
    return b
};
Error.parameterCount = function(a) {
    var c = "Sys.ParameterCountException: " + (a ? a : Sys.Res.parameterCount),
        b = Error.create(c, {
            name: "Sys.ParameterCountException"
        });
    b.popStackFrame();
    return b
};
Error.prototype.popStackFrame = function() {
    if (typeof this.stack === "undefined" || this.stack === null || typeof this.fileName === "undefined" || this.fileName === null || typeof this.lineNumber === "undefined" || this.lineNumber === null) return;
    var a = this.stack.split("\n"),
        c = a[0],
        e = this.fileName + ":" + this.lineNumber;
    while (typeof c !== "undefined" && c !== null && c.indexOf(e) === -1) {
        a.shift();
        c = a[0]
    }
    var d = a[1];
    if (typeof d === "undefined" || d === null) return;
    var b = d.match(/@(.*):(\d+)$/);
    if (typeof b === "undefined" || b === null) return;
    this.fileName = b[1];
    this.lineNumber = parseInt(b[2]);
    a.shift();
    this.stack = a.join("\n")
};
if (!window) this.window = this;
window.Type = Function;
window.__rootNamespaces = [];
window.__registeredTypes = {};
Type.prototype.callBaseMethod = function(a, d, b) {
    var c = this.getBaseMethod(a, d);
    if (!b) return c.apply(a);
    else return c.apply(a, b)
};
Type.prototype.getBaseMethod = function(d, c) {
    var b = this.getBaseType();
    if (b) {
        var a = b.prototype[c];
        return a instanceof Function ? a : null
    }
    return null
};
Type.prototype.getBaseType = function() {
    return typeof this.__baseType === "undefined" ? null : this.__baseType
};
Type.prototype.getInterfaces = function() {
    var a = [],
        b = this;
    while (b) {
        var c = b.__interfaces;
        if (c) for (var d = 0, f = c.length; d < f; d++) {
            var e = c[d];
            if (!Array.contains(a, e)) a[a.length] = e
        }
        b = b.__baseType
    }
    return a
};
Type.prototype.getName = function() {
    return typeof this.__typeName === "undefined" ? "" : this.__typeName
};
Type.prototype.implementsInterface = function(d) {
    this.resolveInheritance();
    var c = d.getName(),
        a = this.__interfaceCache;
    if (a) {
        var e = a[c];
        if (typeof e !== "undefined") return e
    } else a = this.__interfaceCache = {};
    var b = this;
    while (b) {
        var f = b.__interfaces;
        if (f) if (Array.indexOf(f, d) !== -1) return a[c] = true;
        b = b.__baseType
    }
    return a[c] = false
};
Type.prototype.inheritsFrom = function(b) {
    this.resolveInheritance();
    var a = this.__baseType;
    while (a) {
        if (a === b) return true;
        a = a.__baseType
    }
    return false
};
Type.prototype.initializeBase = function(a, b) {
    this.resolveInheritance();
    if (this.__baseType) if (!b) this.__baseType.apply(a);
    else this.__baseType.apply(a, b);
    return a
};
Type.prototype.isImplementedBy = function(a) {
    if (typeof a === "undefined" || a === null) return false;
    var b = Object.getType(a);
    return !!(b.implementsInterface && b.implementsInterface(this))
};
Type.prototype.isInstanceOfType = function(b) {
    if (typeof b === "undefined" || b === null) return false;
    if (b instanceof this) return true;
    var a = Object.getType(b);
    return !!(a === this) || a.inheritsFrom && a.inheritsFrom(this) || a.implementsInterface && a.implementsInterface(this)
};
Type.prototype.registerClass = function(c, b, d) {
    this.prototype.constructor = this;
    this.__typeName = c;
    this.__class = true;
    if (b) {
        this.__baseType = b;
        this.__basePrototypePending = true
    }
    if (!window.__classes) window.__classes = {};
    window.__classes[c.toUpperCase()] = this;
    if (d) {
        this.__interfaces = [];
        for (var a = 2; a < arguments.length; a++) {
            var e = arguments[a];
            this.__interfaces.push(e)
        }
    }
    return this
};
Type.prototype.registerInterface = function(a) {
    this.prototype.constructor = this;
    this.__typeName = a;
    this.__interface = true;
    return this
};
Type.prototype.resolveInheritance = function() {
    if (this.__basePrototypePending) {
        var b = this.__baseType;
        b.resolveInheritance();
        for (var a in b.prototype) {
            var c = b.prototype[a];
            if (!this.prototype[a]) this.prototype[a] = c
        }
        delete this.__basePrototypePending
    }
};
Type.getRootNamespaces = function() {
    return Array.clone(window.__rootNamespaces)
};
Type.isClass = function(a) {
    if (typeof a === "undefined" || a === null) return false;
    return !!a.__class
};
Type.isInterface = function(a) {
    if (typeof a === "undefined" || a === null) return false;
    return !!a.__interface
};
Type.isNamespace = function(a) {
    if (typeof a === "undefined" || a === null) return false;
    return !!a.__namespace
};
Type.parse = function(typeName, ns) {
    var fn;
    if (ns) {
        if (!window.__classes) return null;
        fn = window.__classes[ns.getName().toUpperCase() + "." + typeName.toUpperCase()];
        return fn || null
    }
    if (!typeName) return null;
    if (!Type.__htClasses) Type.__htClasses = {};
    fn = Type.__htClasses[typeName];
    if (!fn) {
        fn = eval(typeName);
        Type.__htClasses[typeName] = fn
    }
    return fn
};
Type.registerNamespace = function(f) {
    var d = window,
        c = f.split(".");
    for (var b = 0; b < c.length; b++) {
        var e = c[b],
            a = d[e];
        if (!a) {
            a = d[e] = {};
            if (b === 0) window.__rootNamespaces[window.__rootNamespaces.length] = a;
            a.__namespace = true;
            a.__typeName = c.slice(0, b + 1).join(".");
            a.getName = function() {
                return this.__typeName
            }
        }
        d = a
    }
};
Object.__typeName = "Object";
Object.__class = true;
Object.getType = function(b) {
    var a = b.constructor;
    if (!a || typeof a !== "function" || !a.__typeName || a.__typeName === "Object") return Object;
    return a
};
Object.getTypeName = function(a) {
    return Object.getType(a).getName()
};
Boolean.__typeName = "Boolean";
Boolean.__class = true;
Boolean.parse = function(b) {
    var a = b.trim().toLowerCase();
    if (a === "false") return false;
    if (a === "true") return true
};
Date.__typeName = "Date";
Date.__class = true;
Date._appendPreOrPostMatch = function(e, b) {
    var d = 0,
        a = false;
    for (var c = 0, g = e.length; c < g; c++) {
        var f = e.charAt(c);
        switch (f) {
            case "'":
                if (a) b.append("'");
                else d++;
                a = false;
                break;
            case "\\":
                if (a) b.append("\\");
                a = !a;
                break;
            default:
                b.append(f);
                a = false;
                break
        }
    }
    return d
};
Date._expandFormat = function(a, b) {
    if (!b) b = "F";
    if (b.length === 1) switch (b) {
        case "d":
            return a.ShortDatePattern;
        case "D":
            return a.LongDatePattern;
        case "t":
            return a.ShortTimePattern;
        case "T":
            return a.LongTimePattern;
        case "F":
            return a.FullDateTimePattern;
        case "M":
        case "m":
            return a.MonthDayPattern;
        case "s":
            return a.SortableDateTimePattern;
        case "Y":
        case "y":
            return a.YearMonthPattern;
        default:
            throw Error.format(Sys.Res.formatInvalidString)
    }
    return b
};
Date._expandYear = function(c, a) {
    if (a < 100) {
        var b = (new Date).getFullYear();
        a += b - b % 100;
        if (a > c.Calendar.TwoDigitYearMax) return a - 100
    }
    return a
};
Date._getParseRegExp = function(b, e) {
    if (!b._parseRegExp) b._parseRegExp = {};
    else if (b._parseRegExp[e]) return b._parseRegExp[e];
    var c = Date._expandFormat(b, e);
    c = c.replace(/([\^\$\.\*\+\?\|\[\]\(\)\{\}])/g, "\\\\$1");
    var a = new Sys.StringBuilder("^"),
        j = [],
        f = 0,
        i = 0,
        h = Date._getTokenRegExp(),
        d;
    while ((d = h.exec(c)) !== null) {
        var l = c.slice(f, d.index);
        f = h.lastIndex;
        i += Date._appendPreOrPostMatch(l, a);
        if (i % 2 === 1) {
            a.append(d[0]);
            continue
        }
        switch (d[0]) {
            case "dddd":
            case "ddd":
            case "MMMM":
            case "MMM":
                a.append("(\\D+)");
                break;
            case "tt":
            case "t":
                a.append("(\\D*)");
                break;
            case "yyyy":
                a.append("(\\d{4})");
                break;
            case "fff":
                a.append("(\\d{3})");
                break;
            case "ff":
                a.append("(\\d{2})");
                break;
            case "f":
                a.append("(\\d)");
                break;
            case "dd":
            case "d":
            case "MM":
            case "M":
            case "yy":
            case "y":
            case "HH":
            case "H":
            case "hh":
            case "h":
            case "mm":
            case "m":
            case "ss":
            case "s":
                a.append("(\\d\\d?)");
                break;
            case "zzz":
                a.append("([+-]?\\d\\d?:\\d{2})");
                break;
            case "zz":
            case "z":
                a.append("([+-]?\\d\\d?)");
                break
        }
        Array.add(j, d[0])
    }
    Date._appendPreOrPostMatch(c.slice(f), a);
    a.append("$");
    var k = a.toString().replace(/\s+/g, "\\s+"),
        g = {
            "regExp": k,
            "groups": j
        };
    b._parseRegExp[e] = g;
    return g
};
Date._getTokenRegExp = function() {
    return /dddd|ddd|dd|d|MMMM|MMM|MM|M|yyyy|yy|y|hh|h|HH|H|mm|m|ss|s|tt|t|fff|ff|f|zzz|zz|z/g
};
Date.parseLocale = function(a) {
    return Date._parse(a, Sys.CultureInfo.CurrentCulture, arguments)
};
Date.parseInvariant = function(a) {
    return Date._parse(a, Sys.CultureInfo.InvariantCulture, arguments)
};
Date._parse = function(g, c, h) {
    var e = false;
    for (var a = 1, i = h.length; a < i; a++) {
        var f = h[a];
        if (f) {
            e = true;
            var b = Date._parseExact(g, f, c);
            if (b) return b
        }
    }
    if (!e) {
        var d = c._getDateTimeFormats();
        for (var a = 0, i = d.length; a < i; a++) {
            var b = Date._parseExact(g, d[a], c);
            if (b) return b
        }
    }
    return null
};
Date._parseExact = function(s, y, j) {
    s = s.trim();
    var m = j.dateTimeFormat,
        v = Date._getParseRegExp(m, y),
        x = (new RegExp(v.regExp)).exec(s);
    if (x !== null) {
        var w = v.groups,
            f = null,
            c = null,
            h = null,
            g = null,
            d = 0,
            n = 0,
            o = 0,
            e = 0,
            k = null,
            r = false;
        for (var p = 0, z = w.length; p < z; p++) {
            var a = x[p + 1];
            if (a) switch (w[p]) {
                case "dd":
                case "d":
                    h = Date._parseInt(a);
                    if (h < 1 || h > 31) return null;
                    break;
                case "MMMM":
                    c = j._getMonthIndex(a);
                    if (c < 0 || c > 11) return null;
                    break;
                case "MMM":
                    c = j._getAbbrMonthIndex(a);
                    if (c < 0 || c > 11) return null;
                    break;
                case "M":
                case "MM":
                    var c = Date._parseInt(a) - 1;
                    if (c < 0 || c > 11) return null;
                    break;
                case "y":
                case "yy":
                    f = Date._expandYear(m, Date._parseInt(a));
                    if (f < 0 || f > 9999) return null;
                    break;
                case "yyyy":
                    f = Date._parseInt(a);
                    if (f < 0 || f > 9999) return null;
                    break;
                case "h":
                case "hh":
                    d = Date._parseInt(a);
                    if (d === 12) d = 0;
                    if (d < 0 || d > 11) return null;
                    break;
                case "H":
                case "HH":
                    d = Date._parseInt(a);
                    if (d < 0 || d > 23) return null;
                    break;
                case "m":
                case "mm":
                    n = Date._parseInt(a);
                    if (n < 0 || n > 59) return null;
                    break;
                case "s":
                case "ss":
                    o = Date._parseInt(a);
                    if (o < 0 || o > 59) return null;
                    break;
                case "tt":
                case "t":
                    var u = a.toUpperCase();
                    r = u === m.PMDesignator.toUpperCase();
                    if (!r && u !== m.AMDesignator.toUpperCase()) return null;
                    break;
                case "f":
                    e = Date._parseInt(a) * 100;
                    if (e < 0 || e > 999) return null;
                    break;
                case "ff":
                    e = Date._parseInt(a) * 10;
                    if (e < 0 || e > 999) return null;
                    break;
                case "fff":
                    e = Date._parseInt(a);
                    if (e < 0 || e > 999) return null;
                    break;
                case "dddd":
                    g = j._getDayIndex(a);
                    if (g < 0 || g > 6) return null;
                    break;
                case "ddd":
                    g = j._getAbbrDayIndex(a);
                    if (g < 0 || g > 6) return null;
                    break;
                case "zzz":
                    var q = a.split(/:/);
                    if (q.length !== 2) return null;
                    var i = Date._parseInt(q[0]);
                    if (i < -12 || i > 13) return null;
                    var l = Date._parseInt(q[1]);
                    if (l < 0 || l > 59) return null;
                    k = i * 60 + (a.startsWith("-") ? -l : l);
                    break;
                case "z":
                case "zz":
                    var i = Date._parseInt(a);
                    if (i < -12 || i > 13) return null;
                    k = i * 60;
                    break
            }
        }
        var b = new Date;
        if (f === null) f = b.getFullYear();
        if (c === null) c = b.getMonth();
        if (h === null) h = b.getDate();
        b.setFullYear(f, c, h);
        if (b.getDate() !== h) return null;
        if (g !== null && b.getDay() !== g) return null;
        if (r && d < 12) d += 12;
        b.setHours(d, n, o, e);
        if (k !== null) {
            var t = b.getMinutes() - (k + b.getTimezoneOffset());
            b.setHours(b.getHours() + parseInt(t / 60), t % 60)
        }
        return b
    }
};
Date._parseInt = function(a) {
    return parseInt(a.replace(/^[\s0]+(\d+)$/, "$1"))
};
Date.prototype.format = function(a) {
    return this._toFormattedString(a, Sys.CultureInfo.InvariantCulture)
};
Date.prototype.localeFormat = function(a) {
    return this._toFormattedString(a, Sys.CultureInfo.CurrentCulture)
};
Date.prototype._toFormattedString = function(e, h) {
    if (!e || e.length === 0 || e === "i") if (h && h.name.length > 0) return this.toLocaleString();
    else return this.toString();
    var d = h.dateTimeFormat;
    e = Date._expandFormat(d, e);
    var a = new Sys.StringBuilder,
        b;

    function c(a) {
        if (a < 10) return "0" + a;
        return a.toString()
    }

    function g(a) {
        if (a < 10) return "00" + a;
        if (a < 100) return "0" + a;
        return a.toString()
    }
    var j = 0,
        i = Date._getTokenRegExp();
    for (; true; ) {
        var l = i.lastIndex,
            f = i.exec(e),
            k = e.slice(l, f ? f.index : e.length);
        j += Date._appendPreOrPostMatch(k, a);
        if (!f) break;
        if (j % 2 === 1) {
            a.append(f[0]);
            continue
        }
        switch (f[0]) {
            case "dddd":
                a.append(d.DayNames[this.getDay()]);
                break;
            case "ddd":
                a.append(d.AbbreviatedDayNames[this.getDay()]);
                break;
            case "dd":
                a.append(c(this.getDate()));
                break;
            case "d":
                a.append(this.getDate());
                break;
            case "MMMM":
                a.append(d.MonthNames[this.getMonth()]);
                break;
            case "MMM":
                a.append(d.AbbreviatedMonthNames[this.getMonth()]);
                break;
            case "MM":
                a.append(c(this.getMonth() + 1));
                break;
            case "M":
                a.append(this.getMonth() + 1);
                break;
            case "yyyy":
                a.append(this.getFullYear());
                break;
            case "yy":
                a.append(c(this.getFullYear() % 100));
                break;
            case "y":
                a.append(this.getFullYear() % 100);
                break;
            case "hh":
                b = this.getHours() % 12;
                if (b === 0) b = 12;
                a.append(c(b));
                break;
            case "h":
                b = this.getHours() % 12;
                if (b === 0) b = 12;
                a.append(b);
                break;
            case "HH":
                a.append(c(this.getHours()));
                break;
            case "H":
                a.append(this.getHours());
                break;
            case "mm":
                a.append(c(this.getMinutes()));
                break;
            case "m":
                a.append(this.getMinutes());
                break;
            case "ss":
                a.append(c(this.getSeconds()));
                break;
            case "s":
                a.append(this.getSeconds());
                break;
            case "tt":
                a.append(this.getHours() < 12 ? d.AMDesignator : d.PMDesignator);
                break;
            case "t":
                a.append((this.getHours() < 12 ? d.AMDesignator : d.PMDesignator).charAt(0));
                break;
            case "f":
                a.append(g(this.getMilliseconds()).charAt(0));
                break;
            case "ff":
                a.append(g(this.getMilliseconds()).substr(0, 2));
                break;
            case "fff":
                a.append(g(this.getMilliseconds()));
                break;
            case "z":
                b = this.getTimezoneOffset() / 60;
                a.append((b >= 0 ? "+" : "-") + Math.floor(Math.abs(b)));
                break;
            case "zz":
                b = this.getTimezoneOffset() / 60;
                a.append((b >= 0 ? "+" : "-") + c(Math.floor(Math.abs(b))));
                break;
            case "zzz":
                b = this.getTimezoneOffset() / 60;
                a.append((b >= 0 ? "+" : "-") + c(Math.floor(Math.abs(b))) + d.TimeSeparator + c(Math.abs(this.getTimezoneOffset() % 60)));
                break
        }
    }
    return a.toString()
};
Number.__typeName = "Number";
Number.__class = true;
Number.parseLocale = function(a) {
    return Number._parse(a, Sys.CultureInfo.CurrentCulture)
};
Number.parseInvariant = function(a) {
    return Number._parse(a, Sys.CultureInfo.InvariantCulture)
};
Number._parse = function(g, f) {
    var a = g.trim();
    if (a.match(/infinity/i) !== null) return parseFloat(a);
    if (a.match(/^0x[a-f0-9]+$/i) !== null) return parseInt(a);
    var d = f.numberFormat,
        b = d.NumberDecimalSeparator,
        c = d.NumberGroupSeparator,
        e = new RegExp("^[+-]?[\\d\\" + c + "]*\\" + b + "?\\d*([eE][+-]?\\d+)?$");
    if (!a.match(e)) return Number.NaN;
    a = a.split(c).join("");
    a = a.replace(b, ".");
    return parseFloat(a)
};
Number.prototype.format = function(a) {
    return this._toFormattedString(a, Sys.CultureInfo.InvariantCulture)
};
Number.prototype.localeFormat = function(a) {
    return this._toFormattedString(a, Sys.CultureInfo.CurrentCulture)
};
Number.prototype._toFormattedString = function(d, j) {
    if (!d || d.length === 0 || d === "i") if (j && j.name.length > 0) return this.toLocaleString();
    else return this.toString();
    var q = ["n %", "n%", "%n"],
        p = ["-n %", "-n%", "-%n"],
        r = ["(n)", "-n", "- n", "n-", "n -"],
        o = ["$n", "n$", "$ n", "n $"],
        n = ["($n)", "-$n", "$-n", "$n-", "(n$)", "-n$", "n-$", "n$-", "-n $", "-$ n", "n $-", "$ n-", "$ -n", "n- $", "($ n)", "(n $)"];

    function i(p, k, j, l, o) {
        var e = j[0],
            g = 1,
            c = p.toString(),
            a = "",
            m = "",
            i = c.split(".");
        if (i.length > 1) {
            c = i[0];
            a = i[1];
            var h = a.split(/e/i);
            if (h.length > 1) {
                a = h[0];
                m = "e" + h[1]
            }
        }
        if (k > 0) {
            var f = a.length - k;
            if (f > 0) a = a.slice(0, k);
            else if (f < 0) for (var n = 0; n < Math.abs(f); n++) a += "0";
            a = o + a
        } else a = "";
        a += m;
        var b = c.length - 1,
            d = "";
        while (b >= 0) {
            if (e === 0 || e > b) if (d.length > 0) return c.slice(0, b + 1) + l + d + a;
            else return c.slice(0, b + 1) + a;
            if (d.length > 0) d = c.slice(b - e + 1, b + 1) + l + d;
            else d = c.slice(b - e + 1, b + 1);
            b -= e;
            if (g < j.length) {
                e = j[g];
                g++
            }
        }
        return c.slice(0, b + 1) + l + d + a
    }
    var a = j.numberFormat,
        e = Math.abs(this);
    if (!d) d = "D";
    var b = -1;
    if (d.length > 1) b = parseInt(d.slice(1));
    var c;
    switch (d.charAt(0)) {
        case "d":
        case "D":
            c = "n";
            if (b !== -1) {
                var g = "" + e,
                k = b - g.length;
                if (k > 0) for (var m = 0; m < k; m++) g = "0" + g;
                e = g
            }
            if (this < 0) e = -e;
            break;
        case "c":
        case "C":
            if (this < 0) c = n[a.CurrencyNegativePattern];
            else c = o[a.CurrencyPositivePattern];
            if (b === -1) b = a.CurrencyDecimalDigits;
            e = i(Math.abs(this), b, a.CurrencyGroupSizes, a.CurrencyGroupSeparator, a.CurrencyDecimalSeparator);
            break;
        case "n":
        case "N":
            if (this < 0) c = r[a.NumberNegativePattern];
            else c = "n";
            if (b === -1) b = a.NumberDecimalDigits;
            e = i(Math.abs(this), b, a.NumberGroupSizes, a.NumberGroupSeparator, a.NumberDecimalSeparator);
            break;
        case "p":
        case "P":
            if (this < 0) c = p[a.PercentNegativePattern];
            else c = q[a.PercentPositivePattern];
            if (b === -1) b = a.PercentDecimalDigits;
            e = i(Math.abs(this), b, a.PercentGroupSizes, a.PercentGroupSeparator, a.PercentDecimalSeparator);
            break;
        default:
            throw Error.format(Sys.Res.formatBadFormatSpecifier)
    }
    var l = /n|\$|-|%/g,
        f = "";
    for (; true; ) {
        var s = l.lastIndex,
            h = l.exec(c);
        f += c.slice(s, h ? h.index : c.length);
        if (!h) break;
        switch (h[0]) {
            case "n":
                f += e;
                break;
            case "$":
                f += a.CurrencySymbol;
                break;
            case "-":
                f += a.NegativeSign;
                break;
            case "%":
                f += a.PercentSymbol;
                break
        }
    }
    return f
};
RegExp.__typeName = "RegExp";
RegExp.__class = true;
Array.__typeName = "Array";
Array.__class = true;
Array.add = Array.enqueue = function(a, b) {
    a[a.length] = b
};
Array.addRange = function(a, b) {
    a.push.apply(a, b)
};
Array.clear = function(a) {
    a.length = 0
};
Array.clone = function(a) {
    if (a.length === 1) return [a[0]];
    else return Array.apply(null, a)
};
Array.contains = function(a, b) {
    return Array.indexOf(a, b) >= 0
};
Array.dequeue = function(a) {
    return a.shift()
};
Array.forEach = function(b, e, d) {
    for (var a = 0, f = b.length; a < f; a++) {
        var c = b[a];
        if (typeof c !== "undefined") e.call(d, c, a, b)
    }
};
Array.indexOf = function(d, e, a) {
    if (typeof e === "undefined") return -1;
    var c = d.length;
    if (c !== 0) {
        a = a - 0;
        if (isNaN(a)) a = 0;
        else {
            if (isFinite(a)) a = a - a % 1;
            if (a < 0) a = Math.max(0, c + a)
        }
        for (var b = a; b < c; b++) if (typeof d[b] !== "undefined" && d[b] === e) return b
    }
    return -1
};
Array.insert = function(a, b, c) {
    a.splice(b, 0, c)
};
Array.parse = function(value) {
    if (!value) return [];
    return eval(value)
};
Array.remove = function(b, c) {
    var a = Array.indexOf(b, c);
    if (a >= 0) b.splice(a, 1);
    return a >= 0
};
Array.removeAt = function(a, b) {
    a.splice(b, 1)
};
String.__typeName = "String";
String.__class = true;
String.prototype.endsWith = function(a) {
    return this.substr(this.length - a.length) === a
};
String.prototype.startsWith = function(a) {
    return this.substr(0, a.length) === a
};
String.prototype.trim = function() {
    return this.replace(/^\s+|\s+$/g, "")
};
String.prototype.trimEnd = function() {
    return this.replace(/\s+$/, "")
};
String.prototype.trimStart = function() {
    return this.replace(/^\s+/, "")
};
String.format = function() {
    return String._toFormattedString(false, arguments)
};
String.localeFormat = function() {
    return String._toFormattedString(true, arguments)
};
String._toFormattedString = function(l, j) {
    var c = "",
        e = j[0];
    for (var a = 0; true; ) {
        var f = e.indexOf("{", a),
            d = e.indexOf("}", a);
        if (f < 0 && d < 0) {
            c += e.slice(a);
            break
        }
        if (d > 0 && (d < f || f < 0)) {
            c += e.slice(a, d + 1);
            a = d + 2;
            continue
        }
        c += e.slice(a, f);
        a = f + 1;
        if (e.charAt(a) === "{") {
            c += "{";
            a++;
            continue
        }
        if (d < 0) break;
        var h = e.substring(a, d),
            g = h.indexOf(":"),
            k = parseInt(g < 0 ? h : h.substring(0, g)) + 1,
            i = g < 0 ? "" : h.substring(g + 1),
        b = j[k];
        if (typeof b === "undefined" || b === null) b = "";
        if (b.toFormattedString) c += b.toFormattedString(i);
        else if (l && b.localeFormat) c += b.localeFormat(i);
        else if (b.format) c += b.format(i);
        else c += b.toString();
        a = d + 1
    }
    return c
};
Type.registerNamespace("Sys");
Sys.IDisposable = function() { };
Sys.IDisposable.prototype = {};
Sys.IDisposable.registerInterface("Sys.IDisposable");
Sys.StringBuilder = function(a) {
    this._parts = typeof a !== "undefined" && a !== null && a !== "" ? [a.toString()] : [];
    this._value = {};
    this._len = 0
};
Sys.StringBuilder.prototype = {
    append: function(a) {
        this._parts[this._parts.length] = a
    },
    appendLine: function(a) {
        this._parts[this._parts.length] = typeof a === "undefined" || a === null || a === "" ? "\r\n" : a + "\r\n"
    },
    clear: function() {
        this._parts = [];
        this._value = {};
        this._len = 0
    },
    isEmpty: function() {
        if (this._parts.length === 0) return true;
        return this.toString() === ""
    },
    toString: function(a) {
        a = a || "";
        var b = this._parts;
        if (this._len !== b.length) {
            this._value = {};
            this._len = b.length
        }
        var d = this._value;
        if (typeof d[a] === "undefined") {
            if (a !== "") for (var c = 0; c < b.length; ) if (typeof b[c] === "undefined" || b[c] === "" || b[c] === null) b.splice(c, 1);
            else c++;
            d[a] = this._parts.join(a)
        }
        return d[a]
    }
};
Sys.StringBuilder.registerClass("Sys.StringBuilder");
if (!window.XMLHttpRequest) window.XMLHttpRequest = function() {
    var b = ["Msxml2.XMLHTTP", "Microsoft.XMLHTTP"];
    for (var a = 0; a < b.length; a++) try {
        var c = new ActiveXObject(b[a]);
        return c
    } catch (d) { }
    return null
};
Sys.Browser = {};
Sys.Browser.InternetExplorer = {};
Sys.Browser.Firefox = {};
Sys.Browser.Safari = {};
Sys.Browser.Opera = {};
Sys.Browser.Chrome = {};
Sys.Browser.agent = null;
Sys.Browser.hasDebuggerStatement = false;
Sys.Browser.name = navigator.appName;
Sys.Browser.version = parseFloat(navigator.appVersion);
if (navigator.userAgent.indexOf(" MSIE ") > -1) {
    Sys.Browser.agent = Sys.Browser.InternetExplorer;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/MSIE (\d+\.\d+)/)[1]);
    Sys.Browser.hasDebuggerStatement = true
} else if (navigator.userAgent.indexOf(" Firefox/") > -1) {
    Sys.Browser.agent = Sys.Browser.Firefox;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/Firefox\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = "Firefox";
    Sys.Browser.hasDebuggerStatement = true
} else if (navigator.userAgent.indexOf(' Chrome/') > -1) {
    Sys.Browser.agent = Sys.Browser.Chrome;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/ Chrome\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = 'Chrome';
    Sys.Browser.hasDebuggerStatement = true;
}
else if (navigator.userAgent.indexOf(" Safari/") > -1) {
    Sys.Browser.agent = Sys.Browser.Safari;
    Sys.Browser.version = parseFloat(navigator.userAgent.match(/Safari\/(\d+\.\d+)/)[1]);
    Sys.Browser.name = "Safari"
} else if (navigator.userAgent.indexOf("Opera/") > -1) Sys.Browser.agent = Sys.Browser.Opera;
Type.registerNamespace("Sys.UI");
Sys._Debug = function() { };
Sys._Debug.prototype = {
    _appendConsole: function(a) {
        if (typeof Debug !== "undefined" && Debug.writeln) Debug.writeln(a);
        if (window.console && window.console.log) window.console.log(a);
        if (window.opera) window.opera.postError(a);
        if (window.debugService) window.debugService.trace(a)
    },
    _appendTrace: function(b) {
        var a = document.getElementById("TraceConsole");
        if (a && a.tagName.toUpperCase() === "TEXTAREA") a.value += b + "\n"
    },
    assert: function(c, a, b) {
        if (!c) {
            a = b && this.assert.caller ? String.format(Sys.Res.assertFailedCaller, a, this.assert.caller) : String.format(Sys.Res.assertFailed, a);
            if (confirm(String.format(Sys.Res.breakIntoDebugger, a))) this.fail(a)
        }
    },
    clearTrace: function() {
        var a = document.getElementById("TraceConsole");
        if (a && a.tagName.toUpperCase() === "TEXTAREA") a.value = ""
    },
    fail: function(message) {
        this._appendConsole(message);
        if (Sys.Browser.hasDebuggerStatement) eval("debugger")
    },
    trace: function(a) {
        this._appendConsole(a);
        this._appendTrace(a)
    },
    traceDump: function(a, b) {
        var c = this._traceDump(a, b, true)
    },
    _traceDump: function(a, c, f, b, d) {
        c = c ? c : "traceDump";
        b = b ? b : "";
        if (a === null) {
            this.trace(b + c + ": null");
            return
        }
        switch (typeof a) {
            case "undefined":
                this.trace(b + c + ": Undefined");
                break;
            case "number":
            case "string":
            case "boolean":
                this.trace(b + c + ": " + a);
                break;
            default:
                if (Date.isInstanceOfType(a) || RegExp.isInstanceOfType(a)) {
                    this.trace(b + c + ": " + a.toString());
                    break
                }
                if (!d) d = [];
                else if (Array.contains(d, a)) {
                    this.trace(b + c + ": ...");
                    return
                }
                Array.add(d, a);
                if (a == window || a === document || window.HTMLElement && a instanceof HTMLElement || typeof a.nodeName === "string") {
                    var k = a.tagName ? a.tagName : "DomElement";
                    if (a.id) k += " - " + a.id;
                    this.trace(b + c + " {" + k + "}")
                } else {
                    var i = Object.getTypeName(a);
                    this.trace(b + c + (typeof i === "string" ? " {" + i + "}" : ""));
                    if (b === "" || f) {
                        b += "    ";
                        var e, j, l, g, h;
                        if (Array.isInstanceOfType(a)) {
                            j = a.length;
                            for (e = 0; e < j; e++) this._traceDump(a[e], "[" + e + "]", f, b, d)
                        } else for (g in a) {
                            h = a[g];
                            if (!Function.isInstanceOfType(h)) this._traceDump(h, g, f, b, d)
                        }
                    }
                }
                Array.remove(d, a)
        }
    }
};
Sys._Debug.registerClass("Sys._Debug");
Sys.Debug = new Sys._Debug;
Sys.Debug.isDebug = false;

function Sys$Enum$parse(c, e) {
    var a, b, i;
    if (e) {
        a = this.__lowerCaseValues;
        if (!a) {
            this.__lowerCaseValues = a = {};
            var g = this.prototype;
            for (var f in g) a[f.toLowerCase()] = g[f]
        }
    } else a = this.prototype;
    if (!this.__flags) {
        i = e ? c.toLowerCase() : c;
        b = a[i.trim()];
        if (typeof b !== "number") throw Error.argument("value", String.format(Sys.Res.enumInvalidValue, c, this.__typeName));
        return b
    } else {
        var h = (e ? c.toLowerCase() : c).split(","),
            j = 0;
        for (var d = h.length - 1; d >= 0; d--) {
            var k = h[d].trim();
            b = a[k];
            if (typeof b !== "number") throw Error.argument("value", String.format(Sys.Res.enumInvalidValue, c.split(",")[d].trim(), this.__typeName));
            j |= b
        }
        return j
    }
}

function Sys$Enum$toString(c) {
    if (typeof c === "undefined" || c === null) return this.__string;
    var d = this.prototype,
        a;
    if (!this.__flags || c === 0) {
        for (a in d) if (d[a] === c) return a
    } else {
        var b = this.__sortedValues;
        if (!b) {
            b = [];
            for (a in d) b[b.length] = {
                key: a,
                value: d[a]
            };
            b.sort(function(a, b) {
                return a.value - b.value
            });
            this.__sortedValues = b
        }
        var e = [],
            g = c;
        for (a = b.length - 1; a >= 0; a--) {
            var h = b[a],
                f = h.value;
            if (f === 0) continue;
            if ((f & c) === f) {
                e[e.length] = h.key;
                g -= f;
                if (g === 0) break
            }
        }
        if (e.length && g === 0) return e.reverse().join(", ")
    }
    return ""
}
Type.prototype.registerEnum = function(c, b) {
    for (var a in this.prototype) this[a] = this.prototype[a];
    this.__typeName = c;
    this.parse = Sys$Enum$parse;
    this.__string = this.toString();
    this.toString = Sys$Enum$toString;
    this.__flags = b;
    this.__enum = true
};
Type.isEnum = function(a) {
    if (typeof a === "undefined" || a === null) return false;
    return !!a.__enum
};
Type.isFlags = function(a) {
    if (typeof a === "undefined" || a === null) return false;
    return !!a.__flags
};
Sys.EventHandlerList = function() {
    this._list = {}
};
Sys.EventHandlerList.prototype = {
    addHandler: function(b, a) {
        Array.add(this._getEvent(b, true), a)
    },
    removeHandler: function(c, b) {
        var a = this._getEvent(c);
        if (!a) return;
        Array.remove(a, b)
    },
    getHandler: function(b) {
        var a = this._getEvent(b);
        if (!a || a.length === 0) return null;
        a = Array.clone(a);
        if (!a._handler) a._handler = function(c, d) {
            for (var b = 0, e = a.length; b < e; b++) a[b](c, d)
        };
        return a._handler
    },
    _getEvent: function(a, b) {
        if (!this._list[a]) {
            if (!b) return null;
            this._list[a] = []
        }
        return this._list[a]
    }
};
Sys.EventHandlerList.registerClass("Sys.EventHandlerList");
Sys.EventArgs = function() { };
Sys.EventArgs.registerClass("Sys.EventArgs");
Sys.EventArgs.Empty = new Sys.EventArgs;
Sys.CancelEventArgs = function() {
    Sys.CancelEventArgs.initializeBase(this);
    this._cancel = false
};
Sys.CancelEventArgs.prototype = {
    get_cancel: function() {
        return this._cancel
    },
    set_cancel: function(a) {
        this._cancel = a
    }
};
Sys.CancelEventArgs.registerClass("Sys.CancelEventArgs", Sys.EventArgs);
Sys.INotifyPropertyChange = function() { };
Sys.INotifyPropertyChange.prototype = {};
Sys.INotifyPropertyChange.registerInterface("Sys.INotifyPropertyChange");
Sys.PropertyChangedEventArgs = function(a) {
    Sys.PropertyChangedEventArgs.initializeBase(this);
    this._propertyName = a
};
Sys.PropertyChangedEventArgs.prototype = {
    get_propertyName: function() {
        return this._propertyName
    }
};
Sys.PropertyChangedEventArgs.registerClass("Sys.PropertyChangedEventArgs", Sys.EventArgs);
Sys.INotifyDisposing = function() { };
Sys.INotifyDisposing.prototype = {};
Sys.INotifyDisposing.registerInterface("Sys.INotifyDisposing");
Sys.Component = function() {
    if (Sys.Application) Sys.Application.registerDisposableObject(this)
};
Sys.Component.prototype = {
    _id: null,
    _initialized: false,
    _updating: false,
    get_events: function() {
        if (!this._events) this._events = new Sys.EventHandlerList;
        return this._events
    },
    get_id: function() {
        return this._id
    },
    set_id: function(a) {
        this._id = a
    },
    get_isInitialized: function() {
        return this._initialized
    },
    get_isUpdating: function() {
        return this._updating
    },
    add_disposing: function(a) {
        this.get_events().addHandler("disposing", a)
    },
    remove_disposing: function(a) {
        this.get_events().removeHandler("disposing", a)
    },
    add_propertyChanged: function(a) {
        this.get_events().addHandler("propertyChanged", a)
    },
    remove_propertyChanged: function(a) {
        this.get_events().removeHandler("propertyChanged", a)
    },
    beginUpdate: function() {
        this._updating = true
    },
    dispose: function() {
        if (this._events) {
            var a = this._events.getHandler("disposing");
            if (a) a(this, Sys.EventArgs.Empty)
        }
        delete this._events;
        Sys.Application.unregisterDisposableObject(this);
        Sys.Application.removeComponent(this)
    },
    endUpdate: function() {
        this._updating = false;
        if (!this._initialized) this.initialize();
        this.updated()
    },
    initialize: function() {
        this._initialized = true
    },
    raisePropertyChanged: function(b) {
        if (!this._events) return;
        var a = this._events.getHandler("propertyChanged");
        if (a) a(this, new Sys.PropertyChangedEventArgs(b))
    },
    updated: function() { }
};
Sys.Component.registerClass("Sys.Component", null, Sys.IDisposable, Sys.INotifyPropertyChange, Sys.INotifyDisposing);

function Sys$Component$_setProperties(a, i) {
    var d, j = Object.getType(a),
        e = j === Object || j === Sys.UI.DomElement,
        h = Sys.Component.isInstanceOfType(a) && !a.get_isUpdating();
    if (h) a.beginUpdate();
    for (var c in i) {
        var b = i[c],
            f = e ? null : a["get_" + c];
        if (e || typeof f !== "function") {
            var k = a[c];
            if (!b || typeof b !== "object" || e && !k) a[c] = b;
            else Sys$Component$_setProperties(k, b)
        } else {
            var l = a["set_" + c];
            if (typeof l === "function") l.apply(a, [b]);
            else if (b instanceof Array) {
                d = f.apply(a);
                for (var g = 0, m = d.length, n = b.length; g < n; g++, m++) d[m] = b[g]
            } else if (typeof b === "object" && Object.getType(b) === Object) {
                d = f.apply(a);
                Sys$Component$_setProperties(d, b)
            }
        }
    }
    if (h) a.endUpdate()
}

function Sys$Component$_setReferences(c, b) {
    for (var a in b) {
        var e = c["set_" + a],
            d = $find(b[a]);
        e.apply(c, [d])
    }
}
var $create = Sys.Component.create = function(h, f, d, c, g) {
    var a = g ? new h(g) : new h,
    b = Sys.Application,
    i = b.get_isCreatingComponents();
    a.beginUpdate();
    if (f) Sys$Component$_setProperties(a, f);
    if (d) for (var e in d) a["add_" + e](d[e]);
    b._createdComponents[b._createdComponents.length] = a;
    if (a.get_id()) b.addComponent(a);
    if (i) if (c) b._addComponentToSecondPass(a, c);
    else a.endUpdate();
    else {
        if (c) Sys$Component$_setReferences(a, c);
        a.endUpdate()
    }
    return a
};
Sys.UI.MouseButton = function() {
    throw Error.notImplemented()
};
Sys.UI.MouseButton.prototype = {
    leftButton: 0,
    middleButton: 1,
    rightButton: 2
};
Sys.UI.MouseButton.registerEnum("Sys.UI.MouseButton");
Sys.UI.Key = function() {
    throw Error.notImplemented()
};
Sys.UI.Key.prototype = {
    backspace: 8,
    tab: 9,
    enter: 13,
    esc: 27,
    space: 32,
    pageUp: 33,
    pageDown: 34,
    end: 35,
    home: 36,
    left: 37,
    up: 38,
    right: 39,
    down: 40,
    del: 127
};
Sys.UI.Key.registerEnum("Sys.UI.Key");
Sys.UI.DomEvent = function(c) {
    var a = c;
    this.rawEvent = a;
    this.altKey = a.altKey;
    if (typeof a.button !== "undefined") this.button = typeof a.which !== "undefined" ? a.button : a.button === 4 ? Sys.UI.MouseButton.middleButton : a.button === 2 ? Sys.UI.MouseButton.rightButton : Sys.UI.MouseButton.leftButton;
    if (a.type === "keypress") this.charCode = a.charCode || a.keyCode;
    else if (a.keyCode && a.keyCode === 46) this.keyCode = 127;
    else this.keyCode = a.keyCode;
    this.clientX = a.clientX;
    this.clientY = a.clientY;
    this.ctrlKey = a.ctrlKey;
    this.target = a.target ? a.target : a.srcElement;
    if (this.target) {
        var b = Sys.UI.DomElement.getLocation(this.target);
        this.offsetX = typeof a.offsetX !== "undefined" ? a.offsetX : window.pageXOffset + (a.clientX || 0) - b.x;
        this.offsetY = typeof a.offsetY !== "undefined" ? a.offsetY : window.pageYOffset + (a.clientY || 0) - b.y
    }
    this.screenX = a.screenX;
    this.screenY = a.screenY;
    this.shiftKey = a.shiftKey;
    this.type = a.type
};
Sys.UI.DomEvent.prototype = {
    preventDefault: function() {
        if (this.rawEvent.preventDefault) this.rawEvent.preventDefault();
        else if (window.event) window.event.returnValue = false
    },
    stopPropagation: function() {
        if (this.rawEvent.stopPropagation) this.rawEvent.stopPropagation();
        else if (window.event) window.event.cancelBubble = true
    }
};
Sys.UI.DomEvent.registerClass("Sys.UI.DomEvent");
var $addHandler = Sys.UI.DomEvent.addHandler = function(a, d, e) {
    if (!a._events) a._events = {};
    var c = a._events[d];
    if (!c) a._events[d] = c = [];
    var b;
    if (a.addEventListener) {
        b = function(b) {
            return e.call(a, new Sys.UI.DomEvent(b))
        };
        a.addEventListener(d, b, false)
    } else if (a.attachEvent) {
        b = function() {
            return e.call(a, new Sys.UI.DomEvent(window.event))
        };
        a.attachEvent("on" + d, b)
    }
    c[c.length] = {
        handler: e,
        browserHandler: b
    }
},
    $addHandlers = Sys.UI.DomEvent.addHandlers = function(e, d, c) {
        for (var b in d) {
            var a = d[b];
            if (c) a = Function.createDelegate(c, a);
            $addHandler(e, b, a)
        }
    },
    $clearHandlers = Sys.UI.DomEvent.clearHandlers = function(a) {
        if (a._events) {
            var e = a._events;
            for (var b in e) {
                var d = e[b];
                for (var c = d.length - 1; c >= 0; c--) $removeHandler(a, b, d[c].handler)
            }
            a._events = null
        }
    },
    $removeHandler = Sys.UI.DomEvent.removeHandler = function(a, e, f) {
        var d = null,
        c = a._events[e],
        d = null;
        for (var b = 0, g = c.length; b < g; b++) if (c[b].handler === f) {
            d = c[b].browserHandler;
            break
        }
        if (a.removeEventListener) a.removeEventListener(e, d, false);
        else if (a.detachEvent) a.detachEvent("on" + e, d);
        c.splice(b, 1)
    };
Sys.IContainer = function() { };
Sys.IContainer.prototype = {};
Sys.IContainer.registerInterface("Sys.IContainer");
Sys._ScriptLoader = function() {
    this._scriptsToLoad = null;
    this._scriptLoadedDelegate = Function.createDelegate(this, this._scriptLoadedHandler)
};
Sys._ScriptLoader.prototype = {
    dispose: function() {
        this._stopLoading();
        if (this._events) delete this._events;
        this._scriptLoadedDelegate = null
    },
    loadScripts: function(a, c, d, b) {
        this._loading = true;
        this._allScriptsLoadedCallback = c;
        this._scriptLoadFailedCallback = d;
        this._scriptLoadTimeoutCallback = b;
        if (a > 0) this._timeoutCookie = window.setTimeout(Function.createDelegate(this, this._scriptLoadTimeoutHandler), a * 1000);
        this._loadScriptsInternal()
    },
    notifyScriptLoaded: function() {
        if (!this._loading) return;
        this._currentTask._notified++;
        if (Sys.Browser.agent === Sys.Browser.Safari) if (this._currentTask._notified === 1) window.setTimeout(Function.createDelegate(this, function() {
            this._scriptLoadedHandler(this._currentTask.get_scriptElement(), true)
        }), 0)
    },
    queueCustomScriptTag: function(a) {
        if (!this._scriptsToLoad) this._scriptsToLoad = [];
        Array.add(this._scriptsToLoad, a)
    },
    queueScriptBlock: function(a) {
        if (!this._scriptsToLoad) this._scriptsToLoad = [];
        Array.add(this._scriptsToLoad, {
            text: a
        })
    },
    queueScriptReference: function(a) {
        if (!this._scriptsToLoad) this._scriptsToLoad = [];
        Array.add(this._scriptsToLoad, {
            src: a
        })
    },
    _createScriptElement: function(c) {
        var a = document.createElement("SCRIPT");
        a.type = "text/javascript";
        for (var b in c) a[b] = c[b];
        return a
    },
    _loadScriptsInternal: function() {
        if (this._scriptsToLoad && this._scriptsToLoad.length > 0) {
            var b = Array.dequeue(this._scriptsToLoad),
                a = this._createScriptElement(b);
            if (a.text && Sys.Browser.agent === Sys.Browser.Safari) {
                a.innerHTML = a.text;
                delete a.text
            }
            if (typeof b.src === "string") {
                this._currentTask = new Sys._ScriptLoaderTask(a, this._scriptLoadedDelegate);
                this._currentTask.execute()
            } else {
                document.getElementsByTagName("HEAD")[0].appendChild(a);
                Sys._ScriptLoader._clearScript(a);
                this._loadScriptsInternal()
            }
        } else {
            var c = this._allScriptsLoadedCallback;
            this._stopLoading();
            if (c) c(this)
        }
    },
    _raiseError: function(a) {
        var c = this._scriptLoadFailedCallback,
            b = this._currentTask.get_scriptElement();
        this._stopLoading();
        if (c) c(this, b, a);
        else throw Sys._ScriptLoader._errorScriptLoadFailed(b.src, a)
    },
    _scriptLoadedHandler: function(a, b) {
        if (b && this._currentTask._notified) if (this._currentTask._notified > 1) this._raiseError(true);
        else {
            Array.add(Sys._ScriptLoader._getLoadedScripts(), a.src);
            this._currentTask.dispose();
            this._currentTask = null;
            this._loadScriptsInternal()
        } else this._raiseError(false)
    },
    _scriptLoadTimeoutHandler: function() {
        var a = this._scriptLoadTimeoutCallback;
        this._stopLoading();
        if (a) a(this)
    },
    _stopLoading: function() {
        if (this._timeoutCookie) {
            window.clearTimeout(this._timeoutCookie);
            this._timeoutCookie = null
        }
        if (this._currentTask) {
            this._currentTask.dispose();
            this._currentTask = null
        }
        this._scriptsToLoad = null;
        this._loading = null;
        this._allScriptsLoadedCallback = null;
        this._scriptLoadFailedCallback = null;
        this._scriptLoadTimeoutCallback = null
    }
};
Sys._ScriptLoader.registerClass("Sys._ScriptLoader", null, Sys.IDisposable);
Sys._ScriptLoader.getInstance = function() {
    var a = Sys._ScriptLoader._activeInstance;
    if (!a) a = Sys._ScriptLoader._activeInstance = new Sys._ScriptLoader;
    return a
};
Sys._ScriptLoader.isScriptLoaded = function(b) {
    var a = document.createElement("script");
    a.src = b;
    return Array.contains(Sys._ScriptLoader._getLoadedScripts(), a.src)
};
Sys._ScriptLoader.readLoadedScripts = function() {
    if (!Sys._ScriptLoader._referencedScripts) {
        var b = Sys._ScriptLoader._referencedScripts = [],
            c = document.getElementsByTagName("SCRIPT");
        for (i = c.length - 1; i >= 0; i--) {
            var d = c[i],
                a = d.src;
            if (a.length) if (!Array.contains(b, a)) Array.add(b, a)
        }
    }
};
Sys._ScriptLoader._clearScript = function(a) {
    if (!Sys.Debug.isDebug) a.parentNode.removeChild(a)
};
Sys._ScriptLoader._errorScriptLoadFailed = function(b, d) {
    var a;
    if (d) a = Sys.Res.scriptLoadMultipleCallbacks;
    else a = Sys.Res.scriptLoadFailed;
    var e = "Sys.ScriptLoadFailedException: " + String.format(a, b),
        c = Error.create(e, {
            name: "Sys.ScriptLoadFailedException",
            "scriptUrl": b
        });
    c.popStackFrame();
    return c
};
Sys._ScriptLoader._getLoadedScripts = function() {
    if (!Sys._ScriptLoader._referencedScripts) {
        Sys._ScriptLoader._referencedScripts = [];
        Sys._ScriptLoader.readLoadedScripts()
    }
    return Sys._ScriptLoader._referencedScripts
};
Sys._ScriptLoaderTask = function(b, a) {
    this._scriptElement = b;
    this._completedCallback = a;
    this._notified = 0
};
Sys._ScriptLoaderTask.prototype = {
    get_scriptElement: function() {
        return this._scriptElement
    },
    dispose: function() {
        if (this._disposed) return;
        this._disposed = true;
        this._removeScriptElementHandlers();
        Sys._ScriptLoader._clearScript(this._scriptElement);
        this._scriptElement = null
    },
    execute: function() {
        this._addScriptElementHandlers();
        document.getElementsByTagName("HEAD")[0].appendChild(this._scriptElement)
    },
    _addScriptElementHandlers: function() {
        this._scriptLoadDelegate = Function.createDelegate(this, this._scriptLoadHandler);
        if (Sys.Browser.agent !== Sys.Browser.InternetExplorer) {
            this._scriptElement.readyState = "loaded";
            $addHandler(this._scriptElement, "load", this._scriptLoadDelegate)
        } else $addHandler(this._scriptElement, "readystatechange", this._scriptLoadDelegate);
        this._scriptErrorDelegate = Function.createDelegate(this, this._scriptErrorHandler);
        $addHandler(this._scriptElement, "error", this._scriptErrorDelegate)
    },
    _removeScriptElementHandlers: function() {
        if (this._scriptLoadDelegate) {
            var a = this.get_scriptElement();
            if (Sys.Browser.agent !== Sys.Browser.InternetExplorer) $removeHandler(a, "load", this._scriptLoadDelegate);
            else $removeHandler(a, "readystatechange", this._scriptLoadDelegate);
            $removeHandler(a, "error", this._scriptErrorDelegate);
            this._scriptErrorDelegate = null;
            this._scriptLoadDelegate = null
        }
    },
    _scriptErrorHandler: function() {
        if (this._disposed) return;
        this._completedCallback(this.get_scriptElement(), false)
    },
    _scriptLoadHandler: function() {
        if (this._disposed) return;
        var a = this.get_scriptElement();
        if (a.readyState !== "loaded" && a.readyState !== "complete") return;
        var b = this;
        window.setTimeout(function() {
            b._completedCallback(a, true)
        },
        0)
    }
};
Sys._ScriptLoaderTask.registerClass("Sys._ScriptLoaderTask", null, Sys.IDisposable);
Sys.ApplicationLoadEventArgs = function(b, a) {
    Sys.ApplicationLoadEventArgs.initializeBase(this);
    this._components = b;
    this._isPartialLoad = a
};
Sys.ApplicationLoadEventArgs.prototype = {
    get_components: function() {
        return this._components
    },
    get_isPartialLoad: function() {
        return this._isPartialLoad
    }
};
Sys.ApplicationLoadEventArgs.registerClass("Sys.ApplicationLoadEventArgs", Sys.EventArgs);
Sys._Application = function() {
    Sys._Application.initializeBase(this);
    this._disposableObjects = [];
    this._components = {};
    this._createdComponents = [];
    this._secondPassComponents = [];
    this._unloadHandlerDelegate = Function.createDelegate(this, this._unloadHandler);
    this._loadHandlerDelegate = Function.createDelegate(this, this._loadHandler);
    Sys.UI.DomEvent.addHandler(window, "unload", this._unloadHandlerDelegate);
    Sys.UI.DomEvent.addHandler(window, "load", this._loadHandlerDelegate)
};
Sys._Application.prototype = {
    _creatingComponents: false,
    _disposing: false,
    get_isCreatingComponents: function() {
        return this._creatingComponents
    },
    add_load: function(a) {
        this.get_events().addHandler("load", a)
    },
    remove_load: function(a) {
        this.get_events().removeHandler("load", a)
    },
    add_init: function(a) {
        if (this._initialized) a(this, Sys.EventArgs.Empty);
        else this.get_events().addHandler("init", a)
    },
    remove_init: function(a) {
        this.get_events().removeHandler("init", a)
    },
    add_unload: function(a) {
        this.get_events().addHandler("unload", a)
    },
    remove_unload: function(a) {
        this.get_events().removeHandler("unload", a)
    },
    addComponent: function(a) {
        this._components[a.get_id()] = a
    },
    beginCreateComponents: function() {
        this._creatingComponents = true
    },
    dispose: function() {
        if (!this._disposing) {
            this._disposing = true;
            if (window.pageUnload) window.pageUnload(this, Sys.EventArgs.Empty);
            var c = this.get_events().getHandler("unload");
            if (c) c(this, Sys.EventArgs.Empty);
            var b = Array.clone(this._disposableObjects);
            for (var a = 0, e = b.length; a < e; a++) b[a].dispose();
            Array.clear(this._disposableObjects);
            Sys.UI.DomEvent.removeHandler(window, "unload", this._unloadHandlerDelegate);
            if (this._loadHandlerDelegate) {
                Sys.UI.DomEvent.removeHandler(window, "load", this._loadHandlerDelegate);
                this._loadHandlerDelegate = null
            }
            var d = Sys._ScriptLoader.getInstance();
            if (d) d.dispose();
            Sys._Application.callBaseMethod(this, "dispose")
        }
    },
    endCreateComponents: function() {
        var b = this._secondPassComponents;
        for (var a = 0, d = b.length; a < d; a++) {
            var c = b[a].component;
            Sys$Component$_setReferences(c, b[a].references);
            c.endUpdate()
        }
        this._secondPassComponents = [];
        this._creatingComponents = false
    },
    findComponent: function(b, a) {
        return a ? Sys.IContainer.isInstanceOfType(a) ? a.findComponent(b) : a[b] || null : Sys.Application._components[b] || null
    },
    getComponents: function() {
        var a = [],
            b = this._components;
        for (var c in b) a[a.length] = b[c];
        return a
    },
    initialize: function() {
        if (!this._initialized && !this._initializing) {
            this._initializing = true;
            window.setTimeout(Function.createDelegate(this, this._doInitialize), 0)
        }
    },
    notifyScriptLoaded: function() {
        var a = Sys._ScriptLoader.getInstance();
        if (a) a.notifyScriptLoaded()
    },
    registerDisposableObject: function(a) {
        if (!this._disposing) this._disposableObjects[this._disposableObjects.length] = a
    },
    raiseLoad: function() {
        var b = this.get_events().getHandler("load"),
            a = new Sys.ApplicationLoadEventArgs(Array.clone(this._createdComponents), !this._initializing);
        if (b) b(this, a);
        if (window.pageLoad) window.pageLoad(this, a);
        this._createdComponents = []
    },
    removeComponent: function(b) {
        var a = b.get_id();
        if (a) delete this._components[a]
    },
    unregisterDisposableObject: function(a) {
        if (!this._disposing) Array.remove(this._disposableObjects, a)
    },
    _addComponentToSecondPass: function(b, a) {
        this._secondPassComponents[this._secondPassComponents.length] = {
            component: b,
            references: a
        }
    },
    _doInitialize: function() {
        Sys._Application.callBaseMethod(this, "initialize");
        var a = this.get_events().getHandler("init");
        if (a) {
            this.beginCreateComponents();
            a(this, Sys.EventArgs.Empty);
            this.endCreateComponents()
        }
        this.raiseLoad();
        this._initializing = false
    },
    _loadHandler: function() {
        if (this._loadHandlerDelegate) {
            Sys.UI.DomEvent.removeHandler(window, "load", this._loadHandlerDelegate);
            this._loadHandlerDelegate = null
        }
        this.initialize()
    },
    _unloadHandler: function() {
        this.dispose()
    }
};
Sys._Application.registerClass("Sys._Application", Sys.Component, Sys.IContainer);
Sys.Application = new Sys._Application;
var $find = Sys.Application.findComponent;
Type.registerNamespace("Sys.Net");
Sys.Net.WebRequestExecutor = function() {
    this._webRequest = null;
    this._resultObject = null
};
Sys.Net.WebRequestExecutor.prototype = {
    get_webRequest: function() {
        return this._webRequest
    },
    _set_webRequest: function(a) {
        this._webRequest = a
    },
    get_started: function() {
        throw Error.notImplemented()
    },
    get_responseAvailable: function() {
        throw Error.notImplemented()
    },
    get_timedOut: function() {
        throw Error.notImplemented()
    },
    get_aborted: function() {
        throw Error.notImplemented()
    },
    get_responseData: function() {
        throw Error.notImplemented()
    },
    get_statusCode: function() {
        throw Error.notImplemented()
    },
    get_statusText: function() {
        throw Error.notImplemented()
    },
    get_xml: function() {
        throw Error.notImplemented()
    },
    get_object: function() {
        if (!this._resultObject) this._resultObject = Sys.Serialization.JavaScriptSerializer.deserialize(this.get_responseData());
        return this._resultObject
    },
    executeRequest: function() {
        throw Error.notImplemented()
    },
    abort: function() {
        throw Error.notImplemented()
    },
    getResponseHeader: function() {
        throw Error.notImplemented()
    },
    getAllResponseHeaders: function() {
        throw Error.notImplemented()
    }
};
Sys.Net.WebRequestExecutor.registerClass("Sys.Net.WebRequestExecutor");
window.XMLDOM = function(d) {
    if (!window.DOMParser) {
        var c = ["Msxml2.DOMDocument.3.0", "Msxml2.DOMDocument"];
        for (var b = 0; b < c.length; b++) try {
            var a = new ActiveXObject(c[b]);
            a.async = false;
            a.loadXML(d);
            a.setProperty("SelectionLanguage", "XPath");
            return a
        } catch (f) { }
        return null
    } else try {
        var e = new window.DOMParser;
        return e.parseFromString(d, "text/xml")
    } catch (f) {
        return null
    }
    return null
};
Sys.Net.XMLHttpExecutor = function() {
    Sys.Net.XMLHttpExecutor.initializeBase(this);
    var a = this;
    this._xmlHttpRequest = null;
    this._webRequest = null;
    this._responseAvailable = false;
    this._timedOut = false;
    this._timer = null;
    this._aborted = false;
    this._started = false;
    this._onReadyStateChange = function() {
        if (a._xmlHttpRequest.readyState === 4) {
            a._clearTimer();
            a._responseAvailable = true;
            a._webRequest.completed(Sys.EventArgs.Empty);
            if (a._xmlHttpRequest != null) {
                a._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
                a._xmlHttpRequest = null
            }
        }
    };
    this._clearTimer = function() {
        if (a._timer != null) {
            window.clearTimeout(a._timer);
            a._timer = null
        }
    };
    this._onTimeout = function() {
        if (!a._responseAvailable) {
            a._clearTimer();
            a._timedOut = true;
            a._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
            a._xmlHttpRequest.abort();
            a._webRequest.completed(Sys.EventArgs.Empty);
            a._xmlHttpRequest = null
        }
    }
};
Sys.Net.XMLHttpExecutor.prototype = {
    get_timedOut: function() {
        return this._timedOut
    },
    get_started: function() {
        return this._started
    },
    get_responseAvailable: function() {
        return this._responseAvailable
    },
    get_aborted: function() {
        return this._aborted
    },
    executeRequest: function() {
        this._webRequest = this.get_webRequest();
        var c = this._webRequest.get_body(),
            a = this._webRequest.get_headers();
        this._xmlHttpRequest = new XMLHttpRequest;
        this._xmlHttpRequest.onreadystatechange = this._onReadyStateChange;
        var e = this._webRequest.get_httpVerb();
        this._xmlHttpRequest.open(e, this._webRequest.getResolvedUrl(), true);
        if (a) for (var b in a) {
            var f = a[b];
            if (typeof f !== "function") this._xmlHttpRequest.setRequestHeader(b, f)
        }
        if (e.toLowerCase() === "post") {
            if (a === null || !a["Content-Type"]) this._xmlHttpRequest.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            if (!c) c = ""
        }
        var d = this._webRequest.get_timeout();
        if (d > 0) this._timer = window.setTimeout(Function.createDelegate(this, this._onTimeout), d);
        this._xmlHttpRequest.send(c);
        this._started = true
    },
    getResponseHeader: function(b) {
        var a;
        try {
            a = this._xmlHttpRequest.getResponseHeader(b)
        } catch (c) { }
        if (!a) a = "";
        return a
    },
    getAllResponseHeaders: function() {
        return this._xmlHttpRequest.getAllResponseHeaders()
    },
    get_responseData: function() {
        return this._xmlHttpRequest.responseText
    },
    get_statusCode: function() {
        return this._xmlHttpRequest.status
    },
    get_statusText: function() {
        return this._xmlHttpRequest.statusText
    },
    get_xml: function() {
        var a = this._xmlHttpRequest.responseXML;
        if (!a || !a.documentElement) {
            a = new XMLDOM(this._xmlHttpRequest.responseText);
            if (!a || !a.documentElement) return null
        } else if (navigator.userAgent.indexOf("MSIE") !== -1) a.setProperty("SelectionLanguage", "XPath");
        if (a.documentElement.namespaceURI === "http://www.mozilla.org/newlayout/xml/parsererror.xml" && a.documentElement.tagName === "parsererror") return null;
        if (a.documentElement.firstChild && a.documentElement.firstChild.tagName === "parsererror") return null;
        return a
    },
    abort: function() {
        if (this._aborted || this._responseAvailable || this._timedOut) return;
        this._aborted = true;
        this._clearTimer();
        if (this._xmlHttpRequest && !this._responseAvailable) {
            this._xmlHttpRequest.onreadystatechange = Function.emptyMethod;
            this._xmlHttpRequest.abort();
            this._xmlHttpRequest = null;
            var a = this._webRequest._get_eventHandlerList().getHandler("completed");
            if (a) a(this, Sys.EventArgs.Empty)
        }
    }
};
Sys.Net.XMLHttpExecutor.registerClass("Sys.Net.XMLHttpExecutor", Sys.Net.WebRequestExecutor);
Sys.Net._WebRequestManager = function() {
    this._this = this;
    this._defaultTimeout = 0;
    this._defaultExecutorType = "Sys.Net.XMLHttpExecutor"
};
Sys.Net._WebRequestManager.prototype = {
    add_invokingRequest: function(a) {
        this._get_eventHandlerList().addHandler("invokingRequest", a)
    },
    remove_invokingRequest: function(a) {
        this._get_eventHandlerList().removeHandler("invokingRequest", a)
    },
    add_completedRequest: function(a) {
        this._get_eventHandlerList().addHandler("completedRequest", a)
    },
    remove_completedRequest: function(a) {
        this._get_eventHandlerList().removeHandler("completedRequest", a)
    },
    _get_eventHandlerList: function() {
        if (!this._events) this._events = new Sys.EventHandlerList;
        return this._events
    },
    get_defaultTimeout: function() {
        return this._defaultTimeout
    },
    set_defaultTimeout: function(a) {
        this._defaultTimeout = a
    },
    get_defaultExecutorType: function() {
        return this._defaultExecutorType
    },
    set_defaultExecutorType: function(a) {
        this._defaultExecutorType = a
    },
    executeRequest: function(webRequest) {
        var executor = webRequest.get_executor();
        if (!executor) {
            var failed = false;
            try {
                var executorType = eval(this._defaultExecutorType);
                executor = new executorType
            } catch (a) {
                failed = true
            }
            webRequest.set_executor(executor)
        }
        if (executor.get_aborted()) return;
        var evArgs = new Sys.Net.NetworkRequestEventArgs(webRequest),
            handler = this._get_eventHandlerList().getHandler("invokingRequest");
        if (handler) handler(this, evArgs);
        if (!evArgs.get_cancel()) executor.executeRequest()
    }
};
Sys.Net._WebRequestManager.registerClass("Sys.Net._WebRequestManager");
Sys.Net.WebRequestManager = new Sys.Net._WebRequestManager;
Sys.Net.NetworkRequestEventArgs = function(a) {
    Sys.Net.NetworkRequestEventArgs.initializeBase(this);
    this._webRequest = a
};
Sys.Net.NetworkRequestEventArgs.prototype = {
    get_webRequest: function() {
        return this._webRequest
    }
};
Sys.Net.NetworkRequestEventArgs.registerClass("Sys.Net.NetworkRequestEventArgs", Sys.CancelEventArgs);
Sys.Net.WebRequest = function() {
    this._url = "";
    this._headers = {};
    this._body = null;
    this._userContext = null;
    this._httpVerb = null;
    this._executor = null;
    this._invokeCalled = false;
    this._timeout = 0
};
Sys.Net.WebRequest.prototype = {
    add_completed: function(a) {
        this._get_eventHandlerList().addHandler("completed", a)
    },
    remove_completed: function(a) {
        this._get_eventHandlerList().removeHandler("completed", a)
    },
    completed: function(b) {
        var a = Sys.Net.WebRequestManager._get_eventHandlerList().getHandler("completedRequest");
        if (a) a(this._executor, b);
        a = this._get_eventHandlerList().getHandler("completed");
        if (a) a(this._executor, b)
    },
    _get_eventHandlerList: function() {
        if (!this._events) this._events = new Sys.EventHandlerList;
        return this._events
    },
    get_url: function() {
        return this._url
    },
    set_url: function(a) {
        this._url = a
    },
    get_headers: function() {
        return this._headers
    },
    get_httpVerb: function() {
        if (this._httpVerb === null) {
            if (this._body === null) return "GET";
            return "POST"
        }
        return this._httpVerb
    },
    set_httpVerb: function(a) {
        this._httpVerb = a
    },
    get_body: function() {
        return this._body
    },
    set_body: function(a) {
        this._body = a
    },
    get_userContext: function() {
        return this._userContext
    },
    set_userContext: function(a) {
        this._userContext = a
    },
    get_executor: function() {
        return this._executor
    },
    set_executor: function(a) {
        this._executor = a;
        this._executor._set_webRequest(this)
    },
    get_timeout: function() {
        if (this._timeout === 0) return Sys.Net.WebRequestManager.get_defaultTimeout();
        return this._timeout
    },
    set_timeout: function(a) {
        this._timeout = a
    },
    getResolvedUrl: function() {
        return Sys.Net.WebRequest._resolveUrl(this._url)
    },
    invoke: function() {
        Sys.Net.WebRequestManager.executeRequest(this);
        this._invokeCalled = true
    }
};
Sys.Net.WebRequest._resolveUrl = function(b, a) {
    if (b && b.indexOf("://") !== -1) return b;
    if (!a || a.length === 0) {
        var c = document.getElementsByTagName("base")[0];
        if (c && c.href && c.href.length > 0) a = c.href;
        else a = document.URL
    }
    var d = a.indexOf("?");
    if (d !== -1) a = a.substr(0, d);
    a = a.substr(0, a.lastIndexOf("/") + 1);
    if (!b || b.length === 0) return a;
    if (b.charAt(0) === "/") {
        var e = a.indexOf("://"),
            g = a.indexOf("/", e + 3);
        return a.substr(0, g) + b
    } else {
        var f = a.lastIndexOf("/");
        return a.substr(0, f + 1) + b
    }
};
Sys.Net.WebRequest._createQueryString = function(d, b) {
    if (!b) b = encodeURIComponent;
    var a = new Sys.StringBuilder,
        f = 0;
    for (var c in d) {
        var e = d[c];
        if (typeof e === "function") continue;
        var g = Sys.Serialization.JavaScriptSerializer.serialize(e);
        if (f !== 0) a.append("&");
        a.append(c);
        a.append("=");
        a.append(b(g));
        f++
    }
    return a.toString()
};
Sys.Net.WebRequest._createUrl = function(a, b) {
    if (!b) return a;
    var d = Sys.Net.WebRequest._createQueryString(b);
    if (d.length > 0) {
        var c = "?";
        if (a && a.indexOf("?") !== -1) c = "&";
        return a + c + d
    } else return a
};
Sys.Net.WebRequest.registerClass("Sys.Net.WebRequest");
Sys.Net.WebServiceProxy = function() { };
Sys.Net.WebServiceProxy.prototype = {
    set_timeout: function(a) {
        this._timeout = a
    },
    get_timeout: function() {
        return this._timeout
    },
    set_defaultUserContext: function(a) {
        this._userContext = a
    },
    get_defaultUserContext: function() {
        return this._userContext
    },
    set_defaultSucceededCallback: function(a) {
        this._succeeded = a
    },
    get_defaultSucceededCallback: function() {
        return this._succeeded
    },
    set_defaultFailedCallback: function(a) {
        this._failed = a
    },
    get_defaultFailedCallback: function() {
        return this._failed
    },
    set_path: function(a) {
        this._path = a
    },
    get_path: function() {
        return this._path
    },
    _invoke: function(d, e, g, f, c, b, a) {
        if (c === null || typeof c === "undefined") c = this.get_defaultSucceededCallback();
        if (b === null || typeof b === "undefined") b = this.get_defaultFailedCallback();
        if (a === null || typeof a === "undefined") a = this.get_defaultUserContext();
        return Sys.Net.WebServiceProxy.invoke(d, e, g, f, c, b, a, this.get_timeout())
    }
};
Sys.Net.WebServiceProxy.registerClass("Sys.Net.WebServiceProxy");
Sys.Net.WebServiceProxy.invoke = function(k, a, j, d, i, c, f, h) {
    var b = new Sys.Net.WebRequest;
    b.get_headers()["Content-Type"] = "application/json; charset=utf-8";
    if (!d) d = {};
    var g = d;
    if (!j || !g) g = {};
    b.set_url(Sys.Net.WebRequest._createUrl(k + "/" + a, g));
    var e = null;
    if (!j) {
        e = Sys.Serialization.JavaScriptSerializer.serialize(d);
        if (e === "{}") e = ""
    }
    b.set_body(e);
    b.add_completed(l);
    if (h && h > 0) b.set_timeout(h);
    b.invoke();

    function l(d) {
        if (d.get_responseAvailable()) {
            var e = d.get_statusCode(),
                b = null;
            try {
                var j = d.getResponseHeader("Content-Type");
                if (j.startsWith("application/json")) b = d.get_object();
                else if (j.startsWith("text/xml")) b = d.get_xml();
                else b = d.get_responseData()
            } catch (m) { }
            var k = d.getResponseHeader("jsonerror"),
                g = k === "true";
            if (g) b = new Sys.Net.WebServiceError(false, b.Message, b.StackTrace, b.ExceptionType);
            if (e < 200 || e >= 300 || g) {
                if (c) {
                    if (!b || !g) b = new Sys.Net.WebServiceError(false, String.format(Sys.Res.webServiceFailedNoMsg, a), "", "");
                    b._statusCode = e;
                    c(b, f, a)
                }
            } else if (i) i(b, f, a)
        } else {
            var h;
            if (d.get_timedOut()) h = String.format(Sys.Res.webServiceTimedOut, a);
            else h = String.format(Sys.Res.webServiceFailedNoMsg, a);
            if (c) c(new Sys.Net.WebServiceError(d.get_timedOut(), h, "", ""), f, a)
        }
    }
    return b
};
Sys.Net.WebServiceProxy._generateTypedConstructor = function(a) {
    return function(b) {
        if (b) for (var c in b) this[c] = b[c];
        this.__type = a
    }
};
Sys.Net.WebServiceError = function(c, d, b, a) {
    this._timedOut = c;
    this._message = d;
    this._stackTrace = b;
    this._exceptionType = a;
    this._statusCode = -1
};
Sys.Net.WebServiceError.prototype = {
    get_timedOut: function() {
        return this._timedOut
    },
    get_statusCode: function() {
        return this._statusCode
    },
    get_message: function() {
        return this._message
    },
    get_stackTrace: function() {
        return this._stackTrace
    },
    get_exceptionType: function() {
        return this._exceptionType
    }
};
Sys.Net.WebServiceError.registerClass("Sys.Net.WebServiceError");
Type.registerNamespace("Sys.Services");
Sys.Services._ProfileService = function() {
    Sys.Services._ProfileService.initializeBase(this);
    this.properties = {}
};
Sys.Services._ProfileService.DefaultWebServicePath = "";
Sys.Services._ProfileService.prototype = {
    _defaultFailedCallback: null,
    _defaultLoadCompletedCallback: null,
    _defaultSaveCompletedCallback: null,
    _path: "",
    _timeout: 0,
    get_defaultFailedCallback: function() {
        return this._defaultFailedCallback
    },
    set_defaultFailedCallback: function(a) {
        this._defaultFailedCallback = a
    },
    get_defaultLoadCompletedCallback: function() {
        return this._defaultLoadCompletedCallback
    },
    set_defaultLoadCompletedCallback: function(a) {
        this._defaultLoadCompletedCallback = a
    },
    get_defaultSaveCompletedCallback: function() {
        return this._defaultSaveCompletedCallback
    },
    set_defaultSaveCompletedCallback: function(a) {
        this._defaultSaveCompletedCallback = a
    },
    get_path: function() {
        return this._path
    },
    set_path: function(a) {
        if (!a || !a.length) a = "";
        this._path = a
    },
    get_timeout: function() {
        return this._timeout
    },
    set_timeout: function(a) {
        this._timeout = a
    },
    load: function(b, d, e, f) {
        var c = {},
            a;
        if (!b) a = "GetAllPropertiesForCurrentUser";
        else {
            a = "GetPropertiesForCurrentUser";
            c = {
                properties: this._clonePropertyNames(b)
            }
        }
        this._invoke(this._get_path(), a, false, c, Function.createDelegate(this, this._onLoadComplete), Function.createDelegate(this, this._onLoadFailed), [d, e, f])
    },
    save: function(d, a, c, e) {
        var b = this._flattenProperties(d, this.properties);
        this._invoke(this._get_path(), "SetPropertiesForCurrentUser", false, {
            values: b
        },
        Function.createDelegate(this, this._onSaveComplete), Function.createDelegate(this, this._onSaveFailed), [a, c, e])
    },
    _clonePropertyNames: function(e) {
        var c = [],
            d = {};
        for (var b = 0; b < e.length; b++) {
            var a = e[b];
            if (!d[a]) {
                Array.add(c, a);
                d[a] = true
            }
        }
        return c
    },
    _flattenProperties: function(a, h, i) {
        var b = {},
            e, d;
        if (a && a.length === 0) return b;
        for (var c in h) {
            e = h[c];
            d = i ? i + "." + c : c;
            if (Sys.Services.ProfileGroup.isInstanceOfType(e)) {
                var g = this._flattenProperties(a, e, d);
                for (var f in g) {
                    var j = g[f];
                    b[f] = j
                }
            } else if (!a || Array.indexOf(a, d) !== -1) b[d] = e
        }
        return b
    },
    _get_path: function() {
        var a = this.get_path();
        if (!a.length) a = Sys.Services._ProfileService.DefaultWebServicePath;
        if (!a || !a.length) throw Error.invalidOperation(Sys.Res.servicePathNotSet);
        return a
    },
    _onLoadComplete: function(a, f, g) {
        if (typeof a !== "object") throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, g, "Object"));
        var d = this._unflattenProperties(a);
        for (var b in d) this.properties[b] = d[b];
        var c = f[0],
            e = c ? c : this._defaultLoadCompletedCallback;
        if (e) e(a.length, f[2], "Sys.Services.ProfileService.load")
    },
    _onLoadFailed: function(d, c) {
        var a = c[1],
            b = a ? a : this._defaultFailedCallback;
        if (b) b(d, c[2], "Sys.Services.ProfileService.load")
    },
    _onSaveComplete: function(d, c, f) {
        if (typeof d !== "number") throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, f, "Number"));
        var a = c[0],
            e = c[2],
            b = a ? a : this._defaultSaveCompletedCallback;
        if (b) b(d, e, "Sys.Services.ProfileService.save")
    },
    _onSaveFailed: function(e, c) {
        var a = c[1],
            d = c[2],
            b = a ? a : this._defaultFailedCallback;
        if (b) b(e, d, "Sys.Services.ProfileService.save")
    },
    _unflattenProperties: function(e) {
        var c = {},
            d, f, h = 0;
        for (var a in e) {
            h++;
            f = e[a];
            d = a.indexOf(".");
            if (d !== -1) {
                var g = a.substr(0, d);
                a = a.substr(d + 1);
                var b = c[g];
                if (!b || !Sys.Services.ProfileGroup.isInstanceOfType(b)) {
                    b = new Sys.Services.ProfileGroup;
                    c[g] = b
                }
                b[a] = f
            } else c[a] = f
        }
        e.length = h;
        return c
    }
};
Sys.Services._ProfileService.registerClass("Sys.Services._ProfileService", Sys.Net.WebServiceProxy);
Sys.Services.ProfileService = new Sys.Services._ProfileService;
Sys.Services.ProfileGroup = function(a) {
    if (a) for (var b in a) this[b] = a[b]
};
Sys.Services.ProfileGroup.registerClass("Sys.Services.ProfileGroup");
Sys.Services._AuthenticationService = function() {
    Sys.Services._AuthenticationService.initializeBase(this)
};
Sys.Services._AuthenticationService.DefaultWebServicePath = "";
Sys.Services._AuthenticationService.prototype = {
    _defaultFailedCallback: null,
    _defaultLoginCompletedCallback: null,
    _defaultLogoutCompletedCallback: null,
    _path: "",
    _timeout: 0,
    _authenticated: false,
    get_defaultFailedCallback: function() {
        return this._defaultFailedCallback
    },
    set_defaultFailedCallback: function(a) {
        this._defaultFailedCallback = a
    },
    get_defaultLoginCompletedCallback: function() {
        return this._defaultLoginCompletedCallback
    },
    set_defaultLoginCompletedCallback: function(a) {
        this._defaultLoginCompletedCallback = a
    },
    get_defaultLogoutCompletedCallback: function() {
        return this._defaultLogoutCompletedCallback
    },
    set_defaultLogoutCompletedCallback: function(a) {
        this._defaultLogoutCompletedCallback = a
    },
    get_isLoggedIn: function() {
        return this._authenticated
    },
    get_path: function() {
        return this._path
    },
    set_path: function(a) {
        if (!a || !a.length) a = "";
        this._path = a
    },
    get_timeout: function() {
        return this._timeout
    },
    set_timeout: function(a) {
        this._timeout = a
    },
    login: function(c, b, a, h, f, d, e, g) {
        this._invoke(this._get_path(), "Login", false, {
            userName: c,
            password: b,
            createPersistentCookie: a
        },
        Function.createDelegate(this, this._onLoginComplete), Function.createDelegate(this, this._onLoginFailed), [c, b, a, f, d, e, g])
    },
    logout: function(c, a, b, d) {
        this._invoke(this._get_path(), "Logout", false, {},
        Function.createDelegate(this, this._onLogoutComplete), Function.createDelegate(this, this._onLogoutFailed), [c, a, b, d])
    },
    _get_path: function() {
        var a = this.get_path();
        if (!a.length) a = Sys.Services._AuthenticationService.DefaultWebServicePath;
        if (!a || !a.length) throw Error.invalidOperation(Sys.Res.servicePathNotSet);
        return a
    },
    _onLoginComplete: function(f, c, g) {
        if (typeof f !== "boolean") throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, g, "Boolean"));
        var b = c[3],
            d = c[4],
            e = c[6],
            a = d ? d : this._defaultLoginCompletedCallback;
        if (f) {
            this._authenticated = true;
            if (a) a(true, e, "Sys.Services.AuthenticationService.login");
            if (typeof b !== "undefined" && b !== null) window.location.href = b
        } else if (a) a(false, e, "Sys.Services.AuthenticationService.login")
    },
    _onLoginFailed: function(d, c) {
        var a = c[5],
            b = a ? a : this._defaultFailedCallback;
        if (b) b(d, c[6], "Sys.Services.AuthenticationService.login")
    },
    _onLogoutComplete: function(g, a, f) {
        if (g !== null) throw Error.invalidOperation(String.format(Sys.Res.webServiceInvalidReturnType, f, "null"));
        var c = a[0],
            b = a[1],
            e = a[3],
            d = b ? b : this._defaultLogoutCompletedCallback;
        this._authenticated = false;
        if (d) d(null, e, "Sys.Services.AuthenticationService.logout");
        if (!c) window.location.reload();
        else window.location.href = c
    },
    _onLogoutFailed: function(d, c) {
        var a = c[2],
            b = a ? a : this._defaultFailedCallback;
        if (b) b(d, c[3], "Sys.Services.AuthenticationService.logout")
    },
    _setAuthenticated: function(a) {
        this._authenticated = a
    }
};
Sys.Services._AuthenticationService.registerClass("Sys.Services._AuthenticationService", Sys.Net.WebServiceProxy);
Sys.Services.AuthenticationService = new Sys.Services._AuthenticationService;
Type.registerNamespace("Sys.Serialization");
Sys.Serialization.JavaScriptSerializer = function() { };
Sys.Serialization.JavaScriptSerializer.registerClass("Sys.Serialization.JavaScriptSerializer");
Sys.Serialization.JavaScriptSerializer._stringRegEx = new RegExp('["\b\f\n\r\t\\\\\x00-\x1F]', "i");
Sys.Serialization.JavaScriptSerializer._serializeWithBuilder = function(b, a, h) {
    var c;
    switch (typeof b) {
        case "object":
            if (b) if (Array.isInstanceOfType(b)) {
                a.append("[");
                for (c = 0; c < b.length; ++c) {
                    if (c > 0) a.append(",");
                    Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(b[c], a)
                }
                a.append("]")
            } else {
                if (Date.isInstanceOfType(b)) {
                    a.append('"\\/Date(');
                    a.append(b.getTime());
                    a.append(')\\/"');
                    break
                }
                var e = [],
                i = 0;
                for (var g in b) {
                    if (g.startsWith("$")) continue;
                    e[i++] = g
                }
                if (h) e.sort();
                a.append("{");
                var j = false;
                for (c = 0; c < i; c++) {
                    var f = b[e[c]];
                    if (typeof f !== "undefined" && typeof f !== "function") {
                        if (j) a.append(",");
                        else j = true;
                        Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(e[c], a, h);
                        a.append(":");
                        Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(f, a, h)
                    }
                }
                a.append("}")
            } else a.append("null");
            break;
        case "number":
            if (isFinite(b)) a.append(String(b));
            else throw Error.invalidOperation(Sys.Res.cannotSerializeNonFiniteNumbers);
            break;
        case "string":
            a.append('"');
            if (Sys.Browser.agent === Sys.Browser.Safari || Sys.Serialization.JavaScriptSerializer._stringRegEx.test(b)) {
                var k = b.length;
                for (c = 0; c < k; ++c) {
                    var d = b.charAt(c);
                    if (d >= " ") {
                        if (d === "\\" || d === '"') a.append("\\");
                        a.append(d)
                    } else switch (d) {
                        case "\b":
                            a.append("\\b");
                            break;
                        case "\f":
                            a.append("\\f");
                            break;
                        case "\n":
                            a.append("\\n");
                            break;
                        case "\r":
                            a.append("\\r");
                            break;
                        case "\t":
                            a.append("\\t");
                            break;
                        default:
                            a.append("\\u00");
                            if (d.charCodeAt() < 16) a.append("0");
                            a.append(d.charCodeAt().toString(16))
                    }
                }
            } else a.append(b);
            a.append('"');
            break;
        case "boolean":
            a.append(b.toString());
            break;
        default:
            a.append("null");
            break
    }
};
Sys.Serialization.JavaScriptSerializer.serialize = function(b) {
    var a = new Sys.StringBuilder;
    Sys.Serialization.JavaScriptSerializer._serializeWithBuilder(b, a, false);
    return a.toString()
};
Sys.Serialization.JavaScriptSerializer.deserialize = function(data) {
    if (data.length === 0) throw Error.argument("data", Sys.Res.cannotDeserializeEmptyString);
    try {
        var exp = data.replace(new RegExp('(^|[^\\\\])\\"\\\\/Date\\((-?[0-9]+)\\)\\\\/\\"', "g"), "$1new Date($2)");
        return eval("(" + exp + ")")
    } catch (a) {
        throw Error.argument("data", Sys.Res.cannotDeserializeInvalidJson)
    }
};
Sys.CultureInfo = function(c, b, a) {
    this.name = c;
    this.numberFormat = b;
    this.dateTimeFormat = a
};
Sys.CultureInfo.prototype = {
    _getDateTimeFormats: function() {
        if (!this._dateTimeFormats) {
            var a = this.dateTimeFormat;
            this._dateTimeFormats = [a.MonthDayPattern, a.YearMonthPattern, a.ShortDatePattern, a.ShortTimePattern, a.LongDatePattern, a.LongTimePattern, a.FullDateTimePattern, a.RFC1123Pattern, a.SortableDateTimePattern, a.UniversalSortableDateTimePattern]
        }
        return this._dateTimeFormats
    },
    _getMonthIndex: function(a) {
        if (!this._upperMonths) this._upperMonths = this._toUpperArray(this.dateTimeFormat.MonthNames);
        return Array.indexOf(this._upperMonths, this._toUpper(a))
    },
    _getAbbrMonthIndex: function(a) {
        if (!this._upperAbbrMonths) this._upperAbbrMonths = this._toUpperArray(this.dateTimeFormat.AbbreviatedMonthNames);
        return Array.indexOf(this._upperMonths, this._toUpper(a))
    },
    _getDayIndex: function(a) {
        if (!this._upperDays) this._upperDays = this._toUpperArray(this.dateTimeFormat.DayNames);
        return Array.indexOf(this._upperDays, this._toUpper(a))
    },
    _getAbbrDayIndex: function(a) {
        if (!this._upperAbbrDays) this._upperAbbrDays = this._toUpperArray(this.dateTimeFormat.AbbreviatedDayNames);
        return Array.indexOf(this._upperAbbrDays, this._toUpper(a))
    },
    _toUpperArray: function(c) {
        var b = [];
        for (var a = 0, d = c.length; a < d; a++) b[a] = this._toUpper(c[a]);
        return b
    },
    _toUpper: function(a) {
        return a.split("\u00A0").join(" ").toUpperCase()
    }
};
Sys.CultureInfo._parse = function(b) {
    var a = Sys.Serialization.JavaScriptSerializer.deserialize(b);
    return new Sys.CultureInfo(a.name, a.numberFormat, a.dateTimeFormat)
};
Sys.CultureInfo.registerClass("Sys.CultureInfo");
Sys.CultureInfo.InvariantCulture = Sys.CultureInfo._parse('{"name":"","numberFormat":{"CurrencyDecimalDigits":2,"CurrencyDecimalSeparator":".","IsReadOnly":true,"CurrencyGroupSizes":[3],"NumberGroupSizes":[3],"PercentGroupSizes":[3],"CurrencyGroupSeparator":",","CurrencySymbol":"\u00A4","NaNSymbol":"NaN","CurrencyNegativePattern":0,"NumberNegativePattern":1,"PercentPositivePattern":0,"PercentNegativePattern":0,"NegativeInfinitySymbol":"-Infinity","NegativeSign":"-","NumberDecimalDigits":2,"NumberDecimalSeparator":".","NumberGroupSeparator":",","CurrencyPositivePattern":0,"PositiveInfinitySymbol":"Infinity","PositiveSign":"+","PercentDecimalDigits":2,"PercentDecimalSeparator":".","PercentGroupSeparator":",","PercentSymbol":"%","PerMilleSymbol":"\u2030","NativeDigits":["0","1","2","3","4","5","6","7","8","9"],"DigitSubstitution":1},"dateTimeFormat":{"AMDesignator":"AM","Calendar":{"MinSupportedDateTime":"@-62135568000000@","MaxSupportedDateTime":"@253402300799999@","AlgorithmType":1,"CalendarType":1,"Eras":[1],"TwoDigitYearMax":2029,"IsReadOnly":true},"DateSeparator":"/","FirstDayOfWeek":0,"CalendarWeekRule":0,"FullDateTimePattern":"dddd, dd MMMM yyyy HH:mm:ss","LongDatePattern":"dddd, dd MMMM yyyy","LongTimePattern":"HH:mm:ss","MonthDayPattern":"MMMM dd","PMDesignator":"PM","RFC1123Pattern":"ddd, dd MMM yyyy HH\':\'mm\':\'ss \'GMT\'","ShortDatePattern":"MM/dd/yyyy","ShortTimePattern":"HH:mm","SortableDateTimePattern":"yyyy\'-\'MM\'-\'dd\'T\'HH\':\'mm\':\'ss","TimeSeparator":":","UniversalSortableDateTimePattern":"yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'","YearMonthPattern":"yyyy MMMM","AbbreviatedDayNames":["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],"ShortestDayNames":["Su","Mo","Tu","We","Th","Fr","Sa"],"DayNames":["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],"AbbreviatedMonthNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthNames":["January","February","March","April","May","June","July","August","September","October","November","December",""],"IsReadOnly":true,"NativeCalendarName":"Gregorian Calendar","AbbreviatedMonthGenitiveNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthGenitiveNames":["January","February","March","April","May","June","July","August","September","October","November","December",""]}}');
if (typeof __cultureInfo === "undefined") var __cultureInfo = '{"name":"en-US","numberFormat":{"CurrencyDecimalDigits":2,"CurrencyDecimalSeparator":".","IsReadOnly":false,"CurrencyGroupSizes":[3],"NumberGroupSizes":[3],"PercentGroupSizes":[3],"CurrencyGroupSeparator":",","CurrencySymbol":"$","NaNSymbol":"NaN","CurrencyNegativePattern":0,"NumberNegativePattern":1,"PercentPositivePattern":0,"PercentNegativePattern":0,"NegativeInfinitySymbol":"-Infinity","NegativeSign":"-","NumberDecimalDigits":2,"NumberDecimalSeparator":".","NumberGroupSeparator":",","CurrencyPositivePattern":0,"PositiveInfinitySymbol":"Infinity","PositiveSign":"+","PercentDecimalDigits":2,"PercentDecimalSeparator":".","PercentGroupSeparator":",","PercentSymbol":"%","PerMilleSymbol":"\u2030","NativeDigits":["0","1","2","3","4","5","6","7","8","9"],"DigitSubstitution":1},"dateTimeFormat":{"AMDesignator":"AM","Calendar":{"MinSupportedDateTime":"@-62135568000000@","MaxSupportedDateTime":"@253402300799999@","AlgorithmType":1,"CalendarType":1,"Eras":[1],"TwoDigitYearMax":2029,"IsReadOnly":false},"DateSeparator":"/","FirstDayOfWeek":0,"CalendarWeekRule":0,"FullDateTimePattern":"dddd, MMMM dd, yyyy h:mm:ss tt","LongDatePattern":"dddd, MMMM dd, yyyy","LongTimePattern":"h:mm:ss tt","MonthDayPattern":"MMMM dd","PMDesignator":"PM","RFC1123Pattern":"ddd, dd MMM yyyy HH\':\'mm\':\'ss \'GMT\'","ShortDatePattern":"M/d/yyyy","ShortTimePattern":"h:mm tt","SortableDateTimePattern":"yyyy\'-\'MM\'-\'dd\'T\'HH\':\'mm\':\'ss","TimeSeparator":":","UniversalSortableDateTimePattern":"yyyy\'-\'MM\'-\'dd HH\':\'mm\':\'ss\'Z\'","YearMonthPattern":"MMMM, yyyy","AbbreviatedDayNames":["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],"ShortestDayNames":["Su","Mo","Tu","We","Th","Fr","Sa"],"DayNames":["Sunday","Monday","Tuesday","Wednesday","Thursday","Friday","Saturday"],"AbbreviatedMonthNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthNames":["January","February","March","April","May","June","July","August","September","October","November","December",""],"IsReadOnly":false,"NativeCalendarName":"Gregorian Calendar","AbbreviatedMonthGenitiveNames":["Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec",""],"MonthGenitiveNames":["January","February","March","April","May","June","July","August","September","October","November","December",""]}}';
Sys.CultureInfo.CurrentCulture = Sys.CultureInfo._parse(__cultureInfo);
delete __cultureInfo;
Sys.UI.Point = function(a, b) {
    this.x = a;
    this.y = b
};
Sys.UI.Point.registerClass("Sys.UI.Point");
Sys.UI.Bounds = function(c, d, b, a) {
    this.x = c;
    this.y = d;
    this.height = a;
    this.width = b
};
Sys.UI.Bounds.registerClass("Sys.UI.Bounds");
Sys.UI.DomElement = function() { };
Sys.UI.DomElement.registerClass("Sys.UI.DomElement");
Sys.UI.DomElement.addCssClass = function(a, b) {
    if (!Sys.UI.DomElement.containsCssClass(a, b)) if (a.className === "") a.className = b;
    else a.className += " " + b
};
Sys.UI.DomElement.containsCssClass = function(b, a) {
    return Array.contains(b.className.split(" "), a)
};
Sys.UI.DomElement.getBounds = function(a) {
    var b = Sys.UI.DomElement.getLocation(a);
    return new Sys.UI.Bounds(b.x, b.y, a.offsetWidth || 0, a.offsetHeight || 0)
};
var $get = Sys.UI.DomElement.getElementById = function(f, e) {
    if (!e) return document.getElementById(f);
    if (e.getElementById) return e.getElementById(f);
    var c = [],
        d = e.childNodes;
    for (var b = 0; b < d.length; b++) {
        var a = d[b];
        if (a.nodeType == 1) c[c.length] = a
    }
    while (c.length) {
        a = c.shift();
        if (a.id == f) return a;
        d = a.childNodes;
        for (b = 0; b < d.length; b++) {
            a = d[b];
            if (a.nodeType == 1) c[c.length] = a
        }
    }
    return null
};
switch (Sys.Browser.agent) {
    case Sys.Browser.InternetExplorer:
        Sys.UI.DomElement.getLocation = function Sys$UI$DomElement$getLocation(a) {
            if (a.self || a.nodeType === 9) return new Sys.UI.Point(0, 0);
            var d = a.getClientRects();
            if (!d || !d.length) return new Sys.UI.Point(0, 0);
            var e = a.ownerDocument.parentWindow,
            g = e.screenLeft - top.screenLeft - top.document.documentElement.scrollLeft + 2,
            h = e.screenTop - top.screenTop - top.document.documentElement.scrollTop + 2,
            c = e.frameElement || null;
            if (c) {
                var b = c.currentStyle;
                g += (c.frameBorder || 1) * 2 + (parseInt(b.paddingLeft) || 0) + (parseInt(b.borderLeftWidth) || 0) - a.ownerDocument.documentElement.scrollLeft;
                h += (c.frameBorder || 1) * 2 + (parseInt(b.paddingTop) || 0) + (parseInt(b.borderTopWidth) || 0) - a.ownerDocument.documentElement.scrollTop
            }
            var f = d[0];
            return new Sys.UI.Point(f.left - g, f.top - h)
        };
        break;
    case Sys.Browser.Safari:
        Sys.UI.DomElement.getLocation = function(c) {
            if (c.window && c.window === c || c.nodeType === 9) return new Sys.UI.Point(0, 0);
            var g = 0,
            h = 0,
            j = null,
            f = null,
            b;
            for (var a = c; a; j = a, (f = b, a = a.offsetParent)) {
                b = Sys.UI.DomElement._getCurrentStyle(a);
                var e = a.tagName;
                if ((a.offsetLeft || a.offsetTop) && (e !== "BODY" || (!f || f.position !== "absolute"))) {
                    g += a.offsetLeft;
                    h += a.offsetTop
                }
            }
            b = Sys.UI.DomElement._getCurrentStyle(c);
            var d = b ? b.position : null,
        k = d && d !== "static";
            if (!d || d !== "absolute") for (var a = c.parentNode; a; a = a.parentNode) {
                e = a.tagName;
                if (e !== "BODY" && e !== "HTML" && (a.scrollLeft || a.scrollTop)) {
                    g -= a.scrollLeft || 0;
                    h -= a.scrollTop || 0
                }
                b = Sys.UI.DomElement._getCurrentStyle(a);
                var i = b ? b.position : null;
                if (i && i === "absolute") break
            }
            return new Sys.UI.Point(g, h)
        };
        break;
    case Sys.Browser.Opera:
        Sys.UI.DomElement.getLocation = function(b) {
            if (b.window && b.window === b || b.nodeType === 9) return new Sys.UI.Point(0, 0);
            var d = 0,
            e = 0,
            i = null;
            for (var a = b; a; i = a, a = a.offsetParent) {
                var f = a.tagName;
                d += a.offsetLeft || 0;
                e += a.offsetTop || 0
            }
            var g = b.style.position,
            c = g && g !== "static";
            for (var a = b.parentNode; a; a = a.parentNode) {
                f = a.tagName;
                if (f !== "BODY" && f !== "HTML" && (a.scrollLeft || a.scrollTop) && (c && (a.style.overflow === "scroll" || a.style.overflow === "auto"))) {
                    d -= a.scrollLeft || 0;
                    e -= a.scrollTop || 0
                }
                var h = a && a.style ? a.style.position : null;
                c = c || h && h !== "static"
            }
            return new Sys.UI.Point(d, e)
        };
        break;
    default:
        Sys.UI.DomElement.getLocation = function(d) {
            if (d.window && d.window === d || d.nodeType === 9) return new Sys.UI.Point(0, 0);
            var e = 0,
            f = 0,
            i = null,
            h = null,
            b = null;
            for (var a = d; a; i = a, (h = b, a = a.offsetParent)) {
                var c = a.tagName;
                b = Sys.UI.DomElement._getCurrentStyle(a);
                if ((a.offsetLeft || a.offsetTop) && !(c === "BODY" && (!h || h.position !== "absolute"))) {
                    e += a.offsetLeft;
                    f += a.offsetTop
                }
                if (i !== null && b) {
                    if (c !== "TABLE" && c !== "TD" && c !== "HTML") {
                        e += parseInt(b.borderLeftWidth) || 0;
                        f += parseInt(b.borderTopWidth) || 0
                    }
                    if (c === "TABLE" && (b.position === "relative" || b.position === "absolute")) {
                        e += parseInt(b.marginLeft) || 0;
                        f += parseInt(b.marginTop) || 0
                    }
                }
            }
            b = Sys.UI.DomElement._getCurrentStyle(d);
            var g = b ? b.position : null,
        j = g && g !== "static";
            if (!g || g !== "absolute") for (var a = d.parentNode; a; a = a.parentNode) {
                c = a.tagName;
                if (c !== "BODY" && c !== "HTML" && (a.scrollLeft || a.scrollTop)) {
                    e -= a.scrollLeft || 0;
                    f -= a.scrollTop || 0;
                    b = Sys.UI.DomElement._getCurrentStyle(a);
                    e += parseInt(b.borderLeftWidth) || 0;
                    f += parseInt(b.borderTopWidth) || 0
                }
            }
            return new Sys.UI.Point(e, f)
        };
        break
}
Sys.UI.DomElement.removeCssClass = function(d, c) {
    var a = " " + d.className + " ",
        b = a.indexOf(" " + c + " ");
    if (b >= 0) d.className = (a.substr(0, b) + " " + a.substring(b + c.length + 1, a.length)).trim()
};
Sys.UI.DomElement.setLocation = function(b, c, d) {
    var a = b.style;
    a.position = "absolute";
    a.left = c + "px";
    a.top = d + "px"
};
Sys.UI.DomElement.toggleCssClass = function(b, a) {
    if (Sys.UI.DomElement.containsCssClass(b, a)) Sys.UI.DomElement.removeCssClass(b, a);
    else Sys.UI.DomElement.addCssClass(b, a)
};
Sys.UI.DomElement._getCurrentStyle = function(a) {
    var b = (a.ownerDocument ? a.ownerDocument : a.documentElement).defaultView;
    return b && a !== b && b.getComputedStyle ? b.getComputedStyle(a, null) : a.style
};
Sys.UI.Behavior = function(b) {
    Sys.UI.Behavior.initializeBase(this);
    this._element = b;
    var a = b._behaviors;
    if (!a) b._behaviors = [this];
    else a[a.length] = this
};
Sys.UI.Behavior.prototype = {
    _name: null,
    get_element: function() {
        return this._element
    },
    get_id: function() {
        var a = Sys.UI.Behavior.callBaseMethod(this, "get_id");
        if (a) return a;
        if (!this._element || !this._element.id) return "";
        return this._element.id + "$" + this.get_name()
    },
    get_name: function() {
        if (this._name) return this._name;
        var a = Object.getTypeName(this),
            b = a.lastIndexOf(".");
        if (b != -1) a = a.substr(b + 1);
        if (!this.get_isInitialized()) this._name = a;
        return a
    },
    set_name: function(a) {
        this._name = a
    },
    initialize: function() {
        Sys.UI.Behavior.callBaseMethod(this, "initialize");
        var a = this.get_name();
        if (a) this._element[a] = this
    },
    dispose: function() {
        Sys.UI.Behavior.callBaseMethod(this, "dispose");
        if (this._element) {
            var a = this.get_name();
            if (a) this._element[a] = null;
            Array.remove(this._element._behaviors, this);
            delete this._element
        }
    }
};
Sys.UI.Behavior.registerClass("Sys.UI.Behavior", Sys.Component);
Sys.UI.Behavior.getBehaviorByName = function(b, c) {
    var a = b[c];
    return a && Sys.UI.Behavior.isInstanceOfType(a) ? a : null
};
Sys.UI.Behavior.getBehaviors = function(a) {
    if (!a._behaviors) return [];
    return Array.clone(a._behaviors)
};
Sys.UI.Behavior.getBehaviorsByType = function(d, e) {
    var a = d._behaviors,
        c = [];
    if (a) for (var b = 0, f = a.length; b < f; b++) if (e.isInstanceOfType(a[b])) c[c.length] = a[b];
    return c
};
Sys.UI.VisibilityMode = function() {
    throw Error.notImplemented()
};
Sys.UI.VisibilityMode.prototype = {
    hide: 0,
    collapse: 1
};
Sys.UI.VisibilityMode.registerEnum("Sys.UI.VisibilityMode");
Sys.UI.Control = function(a) {
    Sys.UI.Control.initializeBase(this);
    this._element = a;
    a.control = this;
    this._oldDisplayMode = this._element.style.display;
    if (!this._oldDisplayMode || this._oldDisplayMode == "none") this._oldDisplayMode = ""
};
Sys.UI.Control.prototype = {
    _parent: null,
    _visibilityMode: Sys.UI.VisibilityMode.hide,
    get_element: function() {
        return this._element
    },
    get_id: function() {
        if (!this._element) return "";
        return this._element.id
    },
    set_id: function() {
        throw Error.invalidOperation(Sys.Res.cantSetId)
    },
    get_parent: function() {
        if (this._parent) return this._parent;
        else {
            var a = this._element.parentNode;
            while (a) {
                if (a.control) return a.control;
                a = a.parentNode
            }
            return null
        }
    },
    set_parent: function(a) {
        this._parent = a
    },
    get_visibilityMode: function() {
        return this._visibilityMode
    },
    set_visibilityMode: function(a) {
        if (this._visibilityMode !== a) {
            this._visibilityMode = a;
            if (this.get_visible() === false) if (this._visibilityMode === Sys.UI.VisibilityMode.hide) this._element.style.display = this._oldDisplayMode;
            else this._element.style.display = "none"
        }
        this._visibilityMode = a
    },
    get_visible: function() {
        return this._element.style.visibility != "hidden"
    },
    set_visible: function(a) {
        if (a != this.get_visible()) {
            this._element.style.visibility = a ? "visible" : "hidden";
            if (a || this._visibilityMode === Sys.UI.VisibilityMode.hide) this._element.style.display = this._oldDisplayMode;
            else this._element.style.display = "none"
        }
    },
    addCssClass: function(a) {
        Sys.UI.DomElement.addCssClass(this._element, a)
    },
    dispose: function() {
        Sys.UI.Control.callBaseMethod(this, "dispose");
        if (this._element) {
            this._element.control = undefined;
            delete this._element
        }
    },
    initialize: function() {
        Sys.UI.Control.callBaseMethod(this, "initialize");
        var a = this._element
    },
    onBubbleEvent: function() {
        return false
    },
    raiseBubbleEvent: function(b, c) {
        var a = this.get_parent();
        while (a) {
            if (a.onBubbleEvent(b, c)) return;
            a = a.get_parent()
        }
    },
    removeCssClass: function(a) {
        Sys.UI.DomElement.removeCssClass(this._element, a)
    },
    toggleCssClass: function(a) {
        Sys.UI.DomElement.toggleCssClass(this._element, a)
    }
};
Sys.UI.Control.registerClass("Sys.UI.Control", Sys.Component)
Sys.Res = {
    'argumentInteger': 'Value must be an integer.',
    'scriptLoadMultipleCallbacks': 'The script \'{0}\' contains multiple calls to Sys.Application.notifyScriptLoaded(). Only one is allowed.',
    'invokeCalledTwice': 'Cannot call invoke more than once.',
    'webServiceFailed': 'The server method \'{0}\' failed with the following error: {1}',
    'argumentType': 'Object cannot be converted to the required type.',
    'argumentNull': 'Value cannot be null.',
    'controlCantSetId': 'The id property can\'t be set on a control.',
    'formatBadFormatSpecifier': 'Format specifier was invalid.',
    'webServiceFailedNoMsg': 'The server method \'{0}\' failed.',
    'argumentDomElement': 'Value must be a DOM element.',
    'invalidExecutorType': 'Could not create a valid Sys.Net.WebRequestExecutor from: {0}.',
    'cannotCallBeforeResponse': 'Cannot call {0} when responseAvailable is false.',
    'actualValue': 'Actual value was {0}.',
    'enumInvalidValue': '\'{0}\' is not a valid value for enum {1}.',
    'scriptLoadFailed': 'The script \'{0}\' could not be loaded.',
    'parameterCount': 'Parameter count mismatch.',
    'cannotDeserializeEmptyString': 'Cannot deserialize empty string.',
    'formatInvalidString': 'Input string was not in a correct format.',
    'invalidTimeout': 'Value must be greater than or equal to zero.',
    'cannotAbortBeforeStart': 'Cannot abort when executor has not started.',
    'argument': 'Value does not fall within the expected range.',
    'cannotDeserializeInvalidJson': 'Cannot deserialize. The data does not correspond to valid JSON.',
    'invalidHttpVerb': 'httpVerb cannot be set to an empty or null string.',
    'nullWebRequest': 'Cannot call executeRequest with a null webRequest.',
    'eventHandlerInvalid': 'Handler was not added through the Sys.UI.DomEvent.addHandler method.',
    'cannotSerializeNonFiniteNumbers': 'Cannot serialize non finite numbers.',
    'argumentUndefined': 'Value cannot be undefined.',
    'webServiceInvalidReturnType': 'The server method \'{0}\' returned an invalid type. Expected type: {1}',
    'servicePathNotSet': 'The path to the web service has not been set.',
    'argumentTypeWithTypes': 'Object of type \'{0}\' cannot be converted to type \'{1}\'.',
    'cannotCallOnceStarted': 'Cannot call {0} once started.',
    'badBaseUrl1': 'Base URL does not contain ://.',
    'badBaseUrl2': 'Base URL does not contain another /.',
    'badBaseUrl3': 'Cannot find last / in base URL.',
    'setExecutorAfterActive': 'Cannot set executor after it has become active.',
    'paramName': 'Parameter name: {0}',
    'cannotCallOutsideHandler': 'Cannot call {0} outside of a completed event handler.',
    'format': 'One of the identified items was in an invalid format.',
    'assertFailedCaller': 'Assertion Failed: {0}\r\nat {1}',
    'argumentOutOfRange': 'Specified argument was out of the range of valid values.',
    'webServiceTimedOut': 'The server method \'{0}\' timed out.',
    'notImplemented': 'The method or operation is not implemented.',
    'assertFailed': 'Assertion Failed: {0}',
    'invalidOperation': 'Operation is not valid due to the current state of the object.',
    'breakIntoDebugger': '{0}\r\n\r\nBreak into debugger?'
};
if (typeof (Sys) !== 'undefined') Sys.Application.notifyScriptLoaded();
