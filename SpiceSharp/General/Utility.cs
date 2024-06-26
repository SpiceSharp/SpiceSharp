using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

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
        /// <returns>The original object.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="source"/> is <c>null</c>.</exception>
        public static T ThrowIfNull<T>(this T source, string name)
        {
            if (source == null)
                throw new ArgumentNullException(name);
            return source;
        }

        /// <summary>
        /// Throws an exception if the enumerable is null or empty.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <param name="source">The object.</param>
        /// <param name="name">The parameter name.</param>
        /// <returns>The original object.</returns>
        /// <exception cref="ArgumentException"><paramref name="source"/> is <c>null</c> or empty.</exception>
        public static IEnumerable<T> ThrowIfEmpty<T>(this IEnumerable<T> source, string name)
        {
            if (source == null || !source.Any())
                throw new ArgumentException(name + " cannot be null or empty.", name);
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
                string allowed = "{0}-{1}".FormatString(minimum, maximum);
                throw new ArgumentException(Properties.Resources.Parameters_ArgumentCountMismatch.FormatString(name, 0, allowed));
            }
            if (arguments.Length < minimum || arguments.Length > maximum)
            {
                string allowed = "{0}-{1}".FormatString(minimum, maximum);
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
            foreach (string node in nodes)
                node.ThrowIfNull(nameof(node));
            return nodes;
        }

#if DEBUG
        /// <summary>
        /// Prints a solver matrix and RHS vector to a string.
        /// </summary>
        /// <typeparam name="T">The state base type.</typeparam>
        /// <param name="state">The state.</param>
        /// <returns>The string.</returns>
        public static string Print<T>(this ISolverSimulationState<T> state)
        {
            var writer = new StringWriter();

            // Make a list of all our variables and initialize the column widths
            int n = state.Solver.Size;
            string[] variables = new string[n];
            string[] elements = new string[n * (n + 1)];
            int[] columnWidths = new int[n + 1];
            int leadWidth = 0;
            foreach (var p in state.Map)
            {
                int index = p.Value - 1;
                if (index < 0)
                    continue; // Ground node
                if (p.Key.Unit == Units.Volt)
                    variables[index] = @"V({0})".FormatString(p.Key.Name);
                else if (p.Key.Unit == Units.Ampere)
                    variables[index] = @"I({0})".FormatString(p.Key.Name);
                else
                    variables[index] = @"?({0})".FormatString(p.Key.Name);
                columnWidths[index] = Math.Max(variables[index].Length + 1, 6);
                leadWidth = Math.Max(leadWidth, columnWidths[index]);
            }
            columnWidths[n] = 6;

            // Determine the elements formatting and the widths
            for (int row = 0; row < n; row++)
            {
                // Matrix elements
                for (int col = 0; col < n; col++)
                {
                    int index = row * (n + 1) + col;
                    var elt = state.Solver.FindElement(new MatrixLocation(row + 1, col + 1));
                    if (elt is null)
                        elements[index] = ".";
                    else if (elt.Value is IFormattable formattable)
                        elements[index] = formattable.ToString("g6", CultureInfo.InvariantCulture);
                    else
                        elements[index] = elt.Value.ToString();
                    columnWidths[col] = Math.Max(columnWidths[col], elements[index].Length + 1);
                }

                // RHS element
                {
                    int index = row * (n + 1) + n;
                    var elt = state.Solver.FindElement(row + 1);
                    if (elt is null)
                        elements[index] = ".";
                    else if (elt.Value is IFormattable formattable)
                        elements[index] = formattable.ToString("g6", CultureInfo.InvariantCulture);
                    else
                        elements[index] = elt.Value.ToString();
                    columnWidths[n] = Math.Max(columnWidths[n], elements[index].Length + 1);
                }
            }

            // Write our column headers
            writer.Write(new string(' ', leadWidth));
            for (int i = 0; i < n; i++)
                writer.Write($"{{0,{columnWidths[i]}}}".FormatString(variables[i]));
            writer.WriteLine();

            // Write every row
            for (int row = 0; row < n; row++)
            {
                writer.Write($"{{0,{leadWidth}}}".FormatString(variables[row]));
                for (int col = 0; col <= n; col++)
                    writer.Write($"{{0,{columnWidths[col]}}}".FormatString(elements[row * (n + 1) + col]));
                writer.WriteLine();
            }

            return writer.ToString();
        }

        /// <summary>
        /// Prints a solver solution to a string.
        /// </summary>
        /// <typeparam name="T">The state base type.</typeparam>
        /// <param name="state">The state.</param>
        /// <returns>The string.</returns>
        public static string PrintSolution<T>(this ISolverSimulationState<T> state)
        {
            var writer = new StringWriter();
            int n = state.Solver.Size;
            string[] names = new string[n + 1];
            string[] values = new string[n + 1];
            int nameWidth = 6;
            foreach (var p in state.Map)
            {
                names[p.Value] = p.Key.Name;
                nameWidth = Math.Max(nameWidth, p.Key.Name.Length);
                var value = state.Solution[p.Value];
                if (value is null)
                    values[p.Value] = ".";
                else if (value is IFormattable formattable)
                    values[p.Value] = formattable.ToString("g6", CultureInfo.InvariantCulture);
                else
                    values[p.Value] = value.ToString();
            }

            for (int i = 0; i <= n; i++)
                writer.WriteLine($"{{0,{nameWidth}}} {{1}}".FormatString(names[i], values[i]));
            return writer.ToString();
        }

        /// <summary>
        /// Prints a solver to a string, but represents the reordered matrix.
        /// </summary>
        /// <typeparam name="T">The solver simulation state base type.</typeparam>
        /// <param name="state">The state.</param>
        /// <returns>Returns the string that represents the reordered solver matrix.</returns>
        public static string PrintReordered<T>(this ISolverSimulationState<T> state)
        {
            var writer = new StringWriter();

            // Make a list of all our variables and initialize the column widths
            int n = state.Solver.Size;
            string[] col_variables = new string[n];
            string[] row_variables = new string[n];
            string[] elements = new string[n * (n + 1)];
            int[] columnWidths = new int[n + 1];
            int leadWidth = 0;
            foreach (var p in state.Map)
            {
                // Get the index to show here
                var loc = state.Solver.ExternalToInternal(new(p.Value, p.Value));
                if (loc.Row > 0)
                {
                    if (p.Key.Unit == Units.Volt)
                        row_variables[loc.Row - 1] = @"V({0})".FormatString(p.Key.Name);
                    else if (p.Key.Unit == Units.Ampere)
                        row_variables[loc.Row - 1] = @"I({0})".FormatString(p.Key.Name);
                    else
                        row_variables[loc.Row - 1] = @"?({0})".FormatString(p.Key.Name);
                    leadWidth = Math.Max(leadWidth, row_variables[loc.Row - 1].Length + 1);
                }
                if (loc.Column > 0)
                {
                    if (p.Key.Unit == Units.Volt)
                        col_variables[loc.Column - 1] = @"V({0})".FormatString(p.Key.Name);
                    else if (p.Key.Unit == Units.Ampere)
                        col_variables[loc.Column - 1] = @"I({0})".FormatString(p.Key.Name);
                    else
                        col_variables[loc.Column - 1] = @"?({0})".FormatString(p.Key.Name);
                    columnWidths[loc.Column - 1] = Math.Max(col_variables[loc.Column - 1].Length + 1, 6);
                }
            }
            columnWidths[n] = 6;

            // Determine the elements formatting and the widths
            for (int row = 0; row < n; row++)
            {
                // Matrix elements
                for (int col = 0; col < n; col++)
                {
                    int index = row * (n + 1) + col;
                    var loc = new MatrixLocation(row + 1, col + 1);
                    loc = state.Solver.InternalToExternal(loc);
                    var elt = state.Solver.FindElement(loc);
                    if (elt is null)
                        elements[index] = ".";
                    else if (elt.Value is IFormattable formattable)
                        elements[index] = formattable.ToString("g6", CultureInfo.InvariantCulture);
                    else
                        elements[index] = elt.Value.ToString();
                    columnWidths[col] = Math.Max(columnWidths[col], elements[index].Length + 1);
                }

                // RHS element
                {
                    int index = row * (n + 1) + n;
                    var loc = new MatrixLocation(row + 1, 0);
                    loc = state.Solver.InternalToExternal(loc);
                    var elt = state.Solver.FindElement(loc.Row);
                    if (elt is null)
                        elements[index] = ".";
                    else if (elt.Value is IFormattable formattable)
                        elements[index] = formattable.ToString("g6", CultureInfo.InvariantCulture);
                    else
                        elements[index] = elt.Value.ToString();
                    columnWidths[n] = Math.Max(columnWidths[n], elements[index].Length + 1);
                }
            }

            // Write our column headers
            writer.Write(new string(' ', leadWidth));
            for (int i = 0; i < n; i++)
                writer.Write($"{{0,{columnWidths[i]}}}".FormatString(col_variables[i]));
            writer.WriteLine();

            // Write every row
            for (int row = 0; row < n; row++)
            {
                writer.Write($"{{0,{leadWidth}}}".FormatString(row_variables[row]));
                for (int col = 0; col <= n; col++)
                    writer.Write($"{{0,{columnWidths[col]}:g}}".FormatString(elements[row * (n + 1) + col]));
                writer.WriteLine();
            }

            return writer.ToString();
        }
#endif
    }
}
