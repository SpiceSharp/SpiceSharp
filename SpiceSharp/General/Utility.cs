using System.Globalization;

namespace SpiceSharp
{
    /// <summary>
    /// Some utility methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Gets or sets the separator used when combining strings.
        /// </summary>
        /// <value>
        /// The separator.
        /// </value>
        public static string Separator { get; set; } = "/";

        /// <summary>
        /// Format a string with the current culture.
        /// </summary>
        /// <param name="format">The formatting.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>The formatted string.</returns>
        public static string FormatString(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Combines a name with the specified appendix, using <see cref="Separator"/>.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="appendix">The appendix.</param>
        /// <returns></returns>
        public static string Combine(this string name, string appendix)
        {
            if (name == null)
                return appendix;
            return name + Separator + appendix;
        }
    }
}
