using SpiceSharp.Simulations;
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
        public static T ThrowIfNotBound<T>(this T source, Behaviors.Behavior behavior) where T : class
        {
            if (source == null)
                throw new InstanceNotBoundException(behavior.Name);
            return source;
        }

        /// <summary>
        /// Checks the number of specified nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        /// <param name="count">The number of expected nodes.</param>
        public static void CheckNodes(this Variable[] nodes, int count)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));
            if (nodes.Length != count)
                throw new NodeMismatchException(count, nodes.Length);
            foreach (var node in nodes)
                node.ThrowIfNull(nameof(node));
        }
    }
}
