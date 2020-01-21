// Copied from: https://github.com/maxhauser/semver/tree/9398386011b84642e5da0c17b681ecfd6d79d81c
// Modified to work with .net framework 3.5 and removed build and prerelease parsing.
// Added method to compare versions by using the same syntax as nodejs does with package dependencies.

using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;

#if !NETSTANDARD
#endif

namespace ModAPI.SemanticVersioning
{
    /// <summary>
    /// A semantic version implementation.
    /// </summary>
    [Serializable]
    public sealed class SemVersion : IComparable<SemVersion>, IComparable, ISerializable
    {
        private static readonly Regex ParseEx =
            new Regex(@"^(?<major>\d+)" +
                      @"(?>\.(?<minor>\d+))?" +
                      @"(?>\.(?<patch>\d+))?$",
                RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture);

        /// <summary>
        /// Deserialize a <see cref="SemVersion"/>.
        /// </summary>
        /// <exception cref="ArgumentNullException">The <paramref name="info"/> parameter is null.</exception>
        private SemVersion(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            var semVersion = Parse(info.GetString("SemVersion"));
            Major = semVersion.Major;
            Minor = semVersion.Minor;
            Patch = semVersion.Patch;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion" /> class.
        /// </summary>
        /// <param name="major">The major version.</param>
        /// <param name="minor">The minor version.</param>
        /// <param name="patch">The patch version.</param>
        public SemVersion(int major, int minor = 0, int patch = 0)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        /// <summary>
        /// Constructs a new instance of the <see cref="SemVersion"/> class from
        /// a <see cref="System.Version"/>.
        /// </summary>
        /// <param name="version">The <see cref="Version"/> that is used to initialize
        /// the Major, Minor, and Patch.</param>
        /// <returns>A <see cref="SemVersion"/> with the same Major and Minor version.
        /// The Patch version will be the fourth part of the version number.</returns>
        public SemVersion(Version version)
        {
            if (version == null)
                throw new ArgumentNullException(nameof(version));

            Major = version.Major;
            Minor = version.Minor;

            if (version.Revision >= 0)
                Patch = version.Revision;
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/> equivalent.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="strict">If set to <see langword="true"/> minor and patch version are required,
        /// otherwise they are optional.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="version"/> has an invalid format.</exception>
        /// <exception cref="InvalidOperationException">The <paramref name="version"/> is missing Minor or Patch versions and <paramref name="strict"/> is <see langword="true"/>.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
        public static SemVersion Parse(string version, bool strict = false)
        {
            var match = ParseEx.Match(version);
            if (!match.Success)
                throw new ArgumentException("Invalid version.", nameof(version));

            var major = int.Parse(match.Groups["major"].Value, CultureInfo.InvariantCulture);

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success)
                minor = int.Parse(minorMatch.Value, CultureInfo.InvariantCulture);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no minor version given in strict mode)");

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
                patch = int.Parse(patchMatch.Value, CultureInfo.InvariantCulture);
            else if (strict)
                throw new InvalidOperationException("Invalid version (no patch version given in strict mode)");

            return new SemVersion(major, minor, patch);
        }

        /// <summary>
        /// Converts the string representation of a semantic version to its <see cref="SemVersion"/>
        /// equivalent and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="version">The version string.</param>
        /// <param name="semver">When the method returns, contains a <see cref="SemVersion"/> instance equivalent
        /// to the version string passed in, if the version string was valid, or <see langword="null"/> if the
        /// version string was not valid.</param>
        /// <param name="strict">If set to <see langword="true"/> minor and patch version are required,
        /// otherwise they are optional.</param>
        /// <returns><see langword="false"/> when a invalid version string is passed, otherwise <see langword="true"/>.</returns>
        public static bool TryParse(string version, out SemVersion semver, bool strict = false)
        {
            semver = null;
            if (version is null) return false;

            var match = ParseEx.Match(version);
            if (!match.Success) return false;

            if (!int.TryParse(match.Groups["major"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var major))
                return false;

            var minorMatch = match.Groups["minor"];
            int minor = 0;
            if (minorMatch.Success)
            {
                if (!int.TryParse(minorMatch.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out minor))
                    return false;
            }
            else if (strict) return false;

            var patchMatch = match.Groups["patch"];
            int patch = 0;
            if (patchMatch.Success)
            {
                if (!int.TryParse(patchMatch.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out patch))
                    return false;
            }
            else if (strict) return false;

            semver = new SemVersion(major, minor, patch);
            return true;
        }

        /// <summary>
        /// Checks whether two semantic versions are equal.
        /// </summary>
        /// <param name="versionA">The first version to compare.</param>
        /// <param name="versionB">The second version to compare.</param>
        /// <returns><see langword="true"/> if the two values are equal, otherwise <see langword="false"/>.</returns>
        public static bool Equals(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, versionB)) return true;
            if (versionA is null || versionB is null) return false;
            return versionA.Equals(versionB);
        }

        /// <summary>
        /// Compares the specified versions.
        /// </summary>
        /// <param name="versionA">The first version to compare.</param>
        /// <param name="versionB">The second version to compare.</param>
        /// <returns>A signed number indicating the relative values of <paramref name="versionA"/> and <paramref name="versionB"/>.</returns>
        public static int Compare(SemVersion versionA, SemVersion versionB)
        {
            if (ReferenceEquals(versionA, versionB)) return 0;
            if (versionA is null) return -1;
            if (versionB is null) return 1;
            return versionA.CompareTo(versionB);
        }

        /// <summary>
        /// Make a copy of the current instance with changed properties.
        /// </summary>
        /// <param name="major">The value to replace the major version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="minor">The value to replace the minor version or <see langword="null"/> to leave it unchanged.</param>
        /// <param name="patch">The value to replace the patch version or <see langword="null"/> to leave it unchanged.</param>
        /// <returns>The new version object.</returns>
        /// <remarks>
        /// The change method is intended to be called using named argument syntax, passing only
        /// those fields to be changed.
        /// </remarks>
        /// <example>
        /// To change only the patch version:
        /// <code>version.Change(patch: 4)</code>
        /// </example>
        public SemVersion Change(int? major = null, int? minor = null, int? patch = null)
        {
            return new SemVersion(
                major ?? Major,
                minor ?? Minor,
                patch ?? Patch);
        }

        /// <summary>
        /// Gets the major version.
        /// </summary>
        /// <value>
        /// The major version.
        /// </value>
        public int Major { get; }

        /// <summary>
        /// Gets the minor version.
        /// </summary>
        /// <value>
        /// The minor version.
        /// </value>
        public int Minor { get; }

        /// <summary>
        /// Gets the patch version.
        /// </summary>
        /// <value>
        /// The patch version.
        /// </value>
        public int Patch { get; }

        /// <summary>
        /// Returns the <see cref="string" /> equivalent of this version.
        /// </summary>
        /// <returns>
        /// The <see cref="string" /> equivalent of this version.
        /// </returns>
        public override string ToString()
        {
            // Assume all separators (".."), at most 2 extra chars
            var estimatedLength = 2 + Major.Digits() + Minor.Digits() + Patch.Digits();
            var version = new StringBuilder(estimatedLength);
            version.Append(Major);
            version.Append('.');
            version.Append(Minor);
            version.Append('.');
            version.Append(Patch);

            return version.ToString();
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        /// other object.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="obj" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="obj" />.
        ///  Greater than zero: This instance follows <paramref name="obj" /> in the sort order.
        /// </returns>
        /// <exception cref="InvalidCastException">The <paramref name="obj"/> is not a <see cref="SemVersion"/>.</exception>
        public int CompareTo(object obj)
        {
            return CompareTo((SemVersion) obj);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates
        /// whether the current instance precedes, follows, or occurs in the same position in the sort order as the
        /// other object.
        /// </summary>
        /// <param name="other">An object to compare with this instance.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="other" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///  Greater than zero: This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareTo(SemVersion other)
        {
            return CompareByPrecedence(other);
        }

        /// <summary>
        /// Returns whether two semantic versions have the same precedence. Versions
        /// that differ only by build metadata have the same precedence.
        /// </summary>
        /// <param name="other">The semantic version to compare to.</param>
        /// <returns><see langword="true"/> if the version precedences are equal.</returns>
        public bool PrecedenceMatches(SemVersion other)
        {
            return CompareByPrecedence(other) == 0;
        }
        
        /// <summary>
        /// Returns whether this version matches the specified version.
        /// The format (loosely) matches the dependency format specified here: https://docs.npmjs.com/about-semantic-versioning
        /// It supports ^ and ~ prefix, and replacing a version number with x.
        /// </summary>
        /// <param name="version">The dependant version.</param>
        /// <returns><see langword="true"/> if the version matches the specified range.</returns>
        public bool DependencyMatches(string version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            
            char firstChar = version[0];
            if (firstChar == '~' || firstChar == '^')
            {
                version = version.Substring(1);
            }
            
            string[] groups = version.Split('.');

            if (groups.Length == 0)
                return true;

            if (groups.Length >= 1)
            {
                var major = groups[0].ToLowerInvariant();

                if (major == "x")
                    return true;

                if (!int.TryParse(major, out int intMajor))
                    return false;

                if (Major != intMajor)
                    return false;

                if (groups.Length == 1)
                    return true;
            }

            if (groups.Length >= 2)
            {
                var minor = groups[1].ToLowerInvariant();

                if (minor == "x")
                    return true;

                if (!int.TryParse(minor, out int intMinor))
                    return false;

                if (firstChar == '^')
                {
                    if (Minor < intMinor)
                        return false;
                }
                else
                {
                    if (Minor != intMinor)
                        return false;
                }

                if (groups.Length == 2 || firstChar == '^')
                    return true;
            }

            if (groups.Length >= 3)
            {
                var patch = groups[2].ToLowerInvariant();

                if (patch == "x")
                    return true;

                if (!int.TryParse(patch, out int intPatch))
                    return false;

                if (firstChar == '~')
                {
                    if (Patch < intPatch)
                        return false;
                }
                else
                {
                    if (Patch != intPatch)
                        return false;
                }

                if (groups.Length == 3)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Compares two semantic versions by precedence as defined in the SemVer spec. Versions
        /// that differ only by build metadata have the same precedence.
        /// </summary>
        /// <param name="other">The semantic version.</param>
        /// <returns>
        /// A value that indicates the relative order of the objects being compared.
        /// The return value has these meanings:
        ///  Less than zero: This instance precedes <paramref name="other" /> in the sort order.
        ///  Zero: This instance occurs in the same position in the sort order as <paramref name="other" />.
        ///  Greater than zero: This instance follows <paramref name="other" /> in the sort order.
        /// </returns>
        public int CompareByPrecedence(SemVersion other)
        {
            if (other is null)
                return 1;

            var r = Major.CompareTo(other.Major);
            if (r != 0) return r;

            r = Minor.CompareTo(other.Minor);
            if (r != 0) return r;

            return Patch.CompareTo(other.Patch);
        }

        private static int CompareComponent(string a, string b, bool nonemptyIsLower = false)
        {
            var aEmpty = string.IsNullOrEmpty(a);
            var bEmpty = string.IsNullOrEmpty(b);
            if (aEmpty && bEmpty)
                return 0;

            if (aEmpty)
                return nonemptyIsLower ? 1 : -1;
            if (bEmpty)
                return nonemptyIsLower ? -1 : 1;

            var aComps = a.Split('.');
            var bComps = b.Split('.');

            var minLen = Math.Min(aComps.Length, bComps.Length);
            for (int i = 0; i < minLen; i++)
            {
                var ac = aComps[i];
                var bc = bComps[i];
                var aIsNum = int.TryParse(ac, out var aNum);
                var bIsNum = int.TryParse(bc, out var bNum);
                int r;
                if (aIsNum && bIsNum)
                {
                    r = aNum.CompareTo(bNum);
                    if (r != 0) return r;
                }
                else
                {
                    if (aIsNum)
                        return -1;
                    if (bIsNum)
                        return 1;
                    r = string.CompareOrdinal(ac, bc);
                    if (r != 0)
                        return r;
                }
            }

            return aComps.Length.CompareTo(bComps.Length);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <see langword="true"/> if the specified <see cref="object" /> is equal to this instance, otherwise <see langword="false"/>.
        /// </returns>
        /// <exception cref="InvalidCastException">The <paramref name="obj"/> is not a <see cref="SemVersion"/>.</exception>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            var other = (SemVersion) obj;

            return Major == other.Major
                   && Minor == other.Minor
                   && Patch == other.Patch;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                // TODO verify this. Some versions start result = 17. Some use 37 instead of 31
                int result = Major.GetHashCode();
                result = result * 31 + Minor.GetHashCode();
                result = result * 31 + Patch.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Populates a <see cref="SerializationInfo"/> with the data needed to serialize the target object.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> to populate with data.</param>
        /// <param name="context">The destination (see <see cref="SerializationInfo"/>) for this serialization.</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            info.AddValue("SemVersion", ToString());
        }

        /// <summary>
        /// Implicit conversion from <see cref="string"/> to <see cref="SemVersion"/>.
        /// </summary>
        /// <param name="version">The semantic version.</param>
        /// <returns>The <see cref="SemVersion"/> object.</returns>
        /// <exception cref="ArgumentNullException">The <paramref name="version"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">The version number has an invalid format.</exception>
        /// <exception cref="OverflowException">The Major, Minor, or Patch versions are larger than <code>int.MaxValue</code>.</exception>
        public static implicit operator SemVersion(string version)
        {
            return Parse(version);
        }

        /// <summary>
        /// Compares two semantic versions for equality.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator ==(SemVersion left, SemVersion right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Compares two semantic versions for inequality.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is not equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator !=(SemVersion left, SemVersion right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >(SemVersion left, SemVersion right)
        {
            return Compare(left, right) > 0;
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is greater than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator >=(SemVersion left, SemVersion right)
        {
            return Equals(left, right) || Compare(left, right) > 0;
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <(SemVersion left, SemVersion right)
        {
            return Compare(left, right) < 0;
        }

        /// <summary>
        /// Compares two semantic versions.
        /// </summary>
        /// <param name="left">The left value.</param>
        /// <param name="right">The right value.</param>
        /// <returns>If left is less than or equal to right <see langword="true"/>, otherwise <see langword="false"/>.</returns>
        public static bool operator <=(SemVersion left, SemVersion right)
        {
            return Equals(left, right) || Compare(left, right) < 0;
        }
    }
}