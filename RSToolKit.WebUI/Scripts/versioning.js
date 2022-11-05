//! Versioning

var Versioning = {
    Version: new Version(1,0,0,0),
    ParseVersion: function (version) {
        /// <signature>
        /// <summary>Parses a string into a version object.</summary>
        /// <returns type="Version" />
        /// <param type="String" name="version">The version to compare in following format 'x.x.x.x'</param>
        /// </signature>
        /// <signature>
        /// <summary>Returns the object passed in.</summary>
        /// <returns type="Version" />
        /// <param type="Version" name="version">The version.</param>
        /// </signature>
        if (typeof (version) === 'object' && version.Major) {
            return version;
        } else if (typeof (version) === 'string') {
            var mjr, mnr, bld, itr = 0;
            if (!/\d+\.\d+\.\d+\.\d+/.test(version)) {
                return false;
            }
            var parseVersion = version.match(/(\d+)\.(\d+)\.(\d+)\.(\d+)/);
            if (parseVersion === null) {
                return false;
            }
            if (isNaN(parseVersion[1]) || isNaN(parseVersion[2] || isNaN(parseVersion[3]) || isNaN(parseVersion[4]))) {
                return new Version();
            }
            mjr = parseInt(parseVersion[1], 10);
            mnr = parseInt(parseVersion[2], 10);
            bld = parseInt(parseVersion[3], 10);
            itr = parseInt(parseVersion[4], 10);
            version = new Version(mjr, mnr, bld, itr);
            return version;
        } else {
            return new Version();
        }
    }
};

function Version(major, minor, build, iteration) {
    /// <signature>
    /// <returns type="Version" />
    /// <param name="major" type="Number">The major version number.</param>
    /// <param name="minor" type="Number">The minor version number.</param>
    /// <param name="build" type="Number">The build version number.</param>
    /// <param name="iteration" type="Number">The iteration version number.</param>
    /// <field name="Major" type="Number">The major version number.</field>
    /// <field name="Minor" type="Number">The minor version number.</field>
    /// <field name="Build" type="Number">The build version number.</field>
    /// <field name="Iteration" type="Number">The iteration version number.</field>
    var ver = this;
    this.Major = 0;
    this.Minor = 0;
    this.Build = 0;
    this.Iteration = 0;
    if (typeof (major) === 'number') {
        this.Major = major;
    }
    if (typeof (minor) === 'number') {
        this.Minor = minor;
    }
    if (typeof (build) === 'number') {
        this.Build = build;
    }
    if (typeof (iteration) === 'number') {
        this.Iteration = iteration;
    }
    this.toString = function () {
        /// <signature>
        /// <summary>Holds verioning information for a class</summary>
        /// <returns type="String" />
        /// </signature>
        return ver.Major + '.' + ver.Minor + '.' + ver.Build + '.' + ver.Iteration;
    };
    this.Equals = function (version) {
        /// <signature>
        /// <summary>Compares two versions to see if they are equal.</summary>
        /// <returns type="Boolean" />
        /// <param type="Version" name="version">The version to compare as a Version object.</param>
        /// </signature>
        /// <signature>
        /// <summary>Compares two versions.</summary>
        /// <returns type="Boolean" />
        /// <param type="String" name="version">The version to compare in following format 'x.x.x.x'</param>
        /// </signature>
        /// <signature>
        /// <summary>Compares two versions.</summary>
        /// <returns type="Boolean" />
        /// <param type="Object" name="version">Will always return false.</param>
        /// </signature>
        var vers = Versioning.ParseVersion(version);
        return ver.Major === vers.Major && ver.Minor === vers.Minor && ver.Build === vers.Build && ver.Iteration === vers.Iteration;
    };
    this.Compare = function (version) {
        /// <signature>
        /// <summary>Compares two versions. return 0 if equal, less than zero if this one is less than the passed parameter, and greater than 0 otherwise.</summary>
        /// <returns type="Number" />
        /// <param type="Version" name="version">The version to compare as a Version object.</param>
        /// </signature>
        /// <signature>
        /// <summary>Compares two versions. return 0 if equal, less than zero if this one is less than the passed parameter, and greater than 0 otherwise.</summary>
        /// <returns type="Number" />
        /// <param type="String" name="version">The version to compare in following format 'x.x.x.x'</param>
        /// </signature>
        /// <signature>
        var vers = Versioning.ParseVersion(version);
        if (ver.Major > vers.Major) {
            return 1;
        }
        if (ver.Major < vers.Major) {
            return -1;
        }
        if (ver.Minor > vers.Minor) {
            return 1;
        }
        if (ver.Minor < vers.Minor) {
            return -1;
        }
        if (ver.Build > vers.Build) {
            return 1;
        }
        if (ver.Build < vers.Build) {
            return -1;
        }
        if (ver.Iteration > vers.Iteration) {
            return 1;
        }
        if (ver.Iteration < vers.Iteration) {
            return -1;
        }
        return 0;
    };
}