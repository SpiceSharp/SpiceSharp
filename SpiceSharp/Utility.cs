using System;
using System.Globalization;

namespace SpiceSharp
{
    /// <summary>
    /// Some utility methods
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Format a string with the current culture
        /// </summary>
        /// <param name="format">The formatting string</param>
        /// <param name="args">The arguments</param>
        /// <returns></returns>
        public static string FormatString(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }
    }
}
