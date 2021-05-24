using System;
using System.Collections.Generic;
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
        public static string Separator { get; set; } = "/";

        /// <summary>
        /// Format a string using the current culture.
        /// </summary>
        /// <param name="format">The formatting.</param>
        /// <param name="args">The arguments.</param>
        /// <returns>
        /// The formatted string.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="format"/> is <c>null</c>.</exception>
        /// <exception cref="FormatException">
        /// Thrown if <paramref name="format"/> is invalid, or if the index of a format item is not higher than 0.
        /// </exception>
        public static string FormatString(this string format, params object[] args)
        {
            return string.Format(CultureInfo.CurrentCulture, format, args);
        }

        /// <summary>
        /// Combines a name with the specified appendix, using <see cref="Separator" />.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="appendix">The appendix.</param>
        /// <returns>
        /// The combined string.
        /// </returns>
        public static string Combine(this string name, string appendix)
        {
            if (name == null)
                return appendix;
            return name + Separator + appendix;
        }

        /// <summary>
        /// Throws an exception if the object is null.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>
        /// The original object.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static T ThrowIfNull<T>(this T source, string name)
        {
            if (source == null)
                throw new ArgumentNullException(name);
            return source;
        }

        /// <summary>
        /// Throws an exception if the array does not have the specified length.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="arguments">The array.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="expected">The number of expected elements.</param>
        /// <returns>
        /// The array.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Exepcted <paramref name="expected"/> arguments, but a different amount were given.
        /// </exception>
        public static T[] ThrowIfNotLength<T>(this T[] arguments, string name, int expected)
        {
            if (arguments == null)
                throw new ArgumentException(Properties.Resources.Parameters_ArgumentCountMismatch.FormatString(name, 0, expected));
            if (arguments.Length != expected)
                throw new ArgumentException(Properties.Resources.Parameters_ArgumentCountMismatch.FormatString(name, arguments.Length, expected));
            return arguments;
        }

        /// <summary>
        /// Throws an exception if the array does not have a length within range.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="arguments">The array.</param>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="minimum">The minimum amount of arguments.</param>
        /// <param name="maximum">The maximum amount of arguments.</param>
        /// <returns>
        /// The array.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Expected between <paramref name="minimum"/> and <paramref name="maximum"/> arguments, but a different amount were given.
        /// </exception>
        public static T[] ThrowIfNotLength<T>(this T[] arguments, string name, int minimum, int maximum)
        {
            if (arguments == null && minimum > 0)
            {
                var allowed = "{0}-{1}".FormatString(minimum, maximum);
                throw new ArgumentException(Properties.Resources.Parameters_ArgumentCountMismatch.FormatString(name, 0, allowed));
            }
            if (arguments.Length < minimum || arguments.Length > maximum)
            {
                var allowed = "{0}-{1}".FormatString(minimum, maximum);
                throw new ArgumentException(Properties.Resources.Parameters_ArgumentCountMismatch.FormatString(name, allowed));
            }
            return arguments;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than <paramref name="limit"/>.</exception>
        public static double GreaterThan(this double value, string name, double limit)
        {
            if (value <= limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotGreater.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than <paramref name="limit"/>.</exception>
        public static double LessThan(this double value, string name, double limit)
        {
            if (value >= limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotLess.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than or equal to <paramref name="limit"/>.</exception>
        public static double GreaterThanOrEquals(this double value, string name, double limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotGreaterOrEqual.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than or equal to the specified limit.</exception>
        public static double LessThanOrEquals(this double value, string name, double limit)
        {
            if (value > limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotLessOrEqual.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Specifies a lower limit for the value. If it is smaller, it is set to the limit value
        /// while raising a warning.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The limited value.</returns>
        public static double LowerLimit(this double value, object source, string name, double limit)
        {
            if (value < limit)
            {
                SpiceSharpWarning.Warning(source, Properties.Resources.Parameters_LowerLimitReached.FormatString(name, value, limit));
                value = limit;
            }
            return value;
        }

        /// <summary>
        /// Specifies an upper limit for the value. If it is larger, it is set to the limit value
        /// while raising a warning.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The limited value.</returns>
        public static double UpperLimit(this double value, object source, string name, double limit)
        {
            if (value > limit)
            {
                SpiceSharpWarning.Warning(source, Properties.Resources.Parameters_UpperLimitReached.FormatString(name, value, limit));
                value = limit;
            }
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than <paramref name="limit"/>.</exception>
        public static int GreaterThan(this int value, string name, int limit)
        {
            if (value <= limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotGreater.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than <paramref name="limit"/>.</exception>
        public static int LessThan(this int value, string name, int limit)
        {
            if (value >= limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotLess.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not greater than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not greater than or equal to <paramref name="limit"/>.</exception>
        public static int GreaterThanOrEquals(this int value, string name, int limit)
        {
            if (value < limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotGreaterOrEqual.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not less than or equal to the specified limit.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not less than or equal to the specified limit.</exception>
        public static int LessThanOrEquals(this int value, string name, int limit)
        {
            if (value > limit)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotLessOrEqual.FormatString(limit));
            return value;
        }

        /// <summary>
        /// Throws an exception if the value is not in the specified range.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name.</param>
        /// <param name="min">The minimum allowed value.</param>
        /// <param name="max">The maximum allowed value.</param>
        /// <returns>The original value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the value is not within bounds.</exception>
        public static int Between(this int value, string name, int min, int max)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(name, value, Properties.Resources.Parameters_NotWithinRange.FormatString(min, max));
            return value;
        }

        /// <summary>
        /// Specifies a lower limit for the value. If it is smaller, it is set to the limit value
        /// while raising a warning.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The limited value.</returns>
        public static int LowerLimit(this int value, object source, string name, int limit)
        {
            if (value < limit)
            {
                SpiceSharpWarning.Warning(source, Properties.Resources.Parameters_LowerLimitReached.FormatString(name, value, limit));
                value = limit;
            }
            return value;
        }

        /// <summary>
        /// Specifies an upper limit for the value. If it is larger, it is set to the limit value
        /// while raising a warning.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="source">The source.</param>
        /// <param name="name">The name.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>The limited value.</returns>
        public static int UpperLimit(this int value, object source, string name, int limit)
        {
            if (value > limit)
            {
                SpiceSharpWarning.Warning(source, Properties.Resources.Parameters_UpperLimitReached.FormatString(name, value, limit));
                value = limit;
            }
            return value;
        }

        /// <summary>
        /// Requires the value to be both a number and finite.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="name">The name of the property.</param>
        /// <returns>The value.</returns>
        public static double Finite(this double value, string name)
        {
            if (double.IsNaN(value))
                throw new ArgumentException(Properties.Resources.Parameters_IsNaN, name);
            if (double.IsInfinity(value))
                throw new ArgumentException(Properties.Resources.Parameters_Finite, name);
            return value;
        }

        /// <summary>
        /// Checks the number of specified nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="count">The number of expected nodes.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="nodes"/> or any of the node names in it is <c>null</c>.
        /// </exception>
        /// <exception cref="NodeMismatchException">The number of nodes in <paramref name="nodes"/> does not match <paramref name="count"/>.</exception>
        public static IReadOnlyList<string> CheckNodes(this IReadOnlyList<string> nodes, int count)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (nodes.Count != count)
                throw new NodeMismatchException(count, nodes.Count);
            foreach (var node in nodes)
                node.ThrowIfNull(nameof(node));
            return nodes;
        }
    }
}
