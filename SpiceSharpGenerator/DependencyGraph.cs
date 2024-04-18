using SpiceSharpGenerator.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpGenerator
{
    /// <summary>
    /// Describes a graph of dependencies that can then be ordered.
    /// </summary>
    /// <typeparam name="T">The value.</typeparam>
    public class DependencyGraph<T> where T : IEquatable<T>
    {
        private readonly HashSet<T> _nodes = [];
        private readonly HashSet<Tuple<T, T>> _edges = [];

        /// <summary>
        /// Adds an item to the dependency graph.
        /// </summary>
        /// <param name="item">The item.</param>
        public void Add(T item)
        {
            _nodes.Add(item);
        }

        /// <summary>
        /// Clears the graph of any dependencies.
        /// </summary>
        public void ClearDependencies()
        {
            _edges.Clear();
        }

        /// <summary>
        /// Clears the graph.
        /// </summary>
        public void Clear()
        {
            _nodes.Clear();
            _edges.Clear();
        }

        /// <summary>
        /// Gets whether a dependency exists.
        /// </summary>
        /// <param name="dependsOnItem">The item that depends on <paramref name="item"/>.</param>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     <c>true</c> if the dependency exists; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T dependsOnItem, T item)
        {
            var key = Tuple.Create(item, dependsOnItem);
            return _edges.Contains(key);
        }

        /// <summary>
        /// Determines whether the item depends on other items.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     <c>true</c> if the item depends on other items; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDependentOnOther(T item)
        {
            return _edges.Any(i => i.Item2.Equals(item));
        }

        /// <summary>
        /// Determines whether the item is depended on by other items.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        ///     <c>true</c> if there are items that depend on this item; otherwise, <c>false</c>.
        /// </returns>
        public bool IsDependedOn(T item)
        {
            return _edges.Any(i => i.Item1.Equals(item));
        }

        /// <summary>
        /// Gets the items that are dependent on <paramref name="item"/>.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The items that are dependent on <paramref name="item"/>.
        /// </returns>
        public IEnumerable<T> GetDependentOn(T item)
        {
            return _edges.Where(e => e.Item1.Equals(item)).Select(p => p.Item2);
        }

        /// <summary>
        /// Gets the items that <paramref name="item"/> depends on.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns>
        /// The items that <paramref name="item"/> depends on.
        /// </returns>
        public IEnumerable<T> GetDependencies(T item)
        {
            return _edges.Where(e => e.Item2.Equals(item)).Select(p => p.Item1);
        }

        /// <summary>
        /// Specifies a dependency.
        /// </summary>
        /// <param name="dependsOnItem">The item that depends on <paramref name="item"/>.</param>
        /// <param name="item">The item that is depended on.</param>
        public void MakeDependency(T dependsOnItem, T item)
        {
            if (!_nodes.Contains(dependsOnItem))
                Add(dependsOnItem);
            if (!_nodes.Contains(item))
                Add(item);
            _edges.Add(Tuple.Create(item, dependsOnItem));
        }

        /// <summary>
        /// Topological Sorting (Kahn's algorithm).
        /// </summary>
        /// <remarks>https://en.wikipedia.org/wiki/Topological_sorting</remarks>
        /// <returns>
        /// The elements in their sorted order.
        /// </returns>
        public IEnumerable<T> OrderByIndependentFirst()
        {
            // Set of all items that do not depend on anything else
            var s = new HashSet<T>(_nodes.Where(n => _edges.All(e => !e.Item2.Equals(n))));
            var edges = new HashSet<Tuple<T, T>>(_edges);

            // while S is non-empty do
            while (s.Any())
            {
                // Remove a node n from S
                var n = s.First();
                s.Remove(n);

                // We found another element!
                yield return n;

                // For each node m with an edge e from n to m do
                foreach (var e in _edges.Where(e => e.Item1.Equals(n)).ToList())
                {
                    var m = e.Item2;

                    // Remove edge e from the graph
                    edges.Remove(e);

                    // If m has no other incoming edges then
                    if (edges.All(me => !me.Item2.Equals(m)))
                    {
                        // Insert m into S
                        s.Add(m);
                    }
                }
            }

            // If there are still edges left, then we have a cyclic dependency
            if (edges.Any())
                throw new CyclicDependencyException();
        }
    }
}
