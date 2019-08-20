using System;
using System.Collections;
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

        /// <summary>
        /// Throws an exception if the object is null.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>The original object.</returns>
        public static T ThrowIfNull<T>(this T source, string name) where T : class
        {
            if (source == null)
                throw new ArgumentNullException(name);
            return source;
        }

        /// <summary>
        /// Throws an exception if the behavior is null saying that the behavior is not bound.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="behavior">The behavior that is supposed to be bound.</param>
        /// <returns></returns>
        public static T ThrowIfNotBound<T>(this T source, SpiceSharp.Behaviors.Behavior behavior) where T : class
        {
            if (source == null)
                throw new CircuitException("Behavior '{0}' is not bound to any simulation".FormatString(behavior.Name));
            return source;
        }

        /// <summary>
        /// Throws an exception if a collection is null or empty.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>The original collection.</returns>
        public static T ThrowIfEmpty<T>(this T source, string name) where T : ICollection
        {
            if (source == null)
                throw new ArgumentNullException(name);
            if (source.Count == 0)
                throw new ArgumentException("{0} contains no elements".FormatString(name));
            return source;
        }

        /// <summary>
        /// Throws an exception if a collection is null or does not contain a fixed amount of elements.
        /// </summary>
        /// <typeparam name="T">The collection type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="size">The original collection.</param>
        /// <returns></returns>
        public static T ThrowIfNot<T>(this T source, string name, int size) where T : ICollection
        {
            if (source == null)
                throw new ArgumentNullException(name);
            if (source.Count != size)
                throw new ArgumentException("{0} does not have {1} elements".FormatString(name, size));
            return source;
        }
    }
}
