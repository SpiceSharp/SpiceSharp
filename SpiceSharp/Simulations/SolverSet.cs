using SpiceSharp.Algebra;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A collection of solvers.
    /// </summary>
    public class SolverSet : IEnumerable<Type>
    {
        private Dictionary<Type, object> _solvers = new Dictionary<Type, object>();

        /// <summary>
        /// Adds a solver to the collection.
        /// </summary>
        /// <typeparam name="T">The base type of the solver.</typeparam>
        /// <param name="solver">The solver.</param>
        public void Add<T>(SparseSolver<T> solver) where T : IFormattable, IEquatable<T>
        {
            _solvers.Add(typeof(T), solver.ThrowIfNull(nameof(solver)));
        }

        /// <summary>
        /// Removes a solver from the collection.
        /// </summary>
        /// <typeparam name="T">The base type of the solver.</typeparam>
        public void Remove<T>() where T : IFormattable, IEquatable<T>
        {
            _solvers.Remove(typeof(T));
        }

        /// <summary>
        /// Gets a <see cref="SparseSolver{T}"/> for a specified base type.
        /// </summary>
        /// <typeparam name="T">The base type of the solver.</typeparam>
        /// <returns>The solver.</returns>
        public SparseSolver<T> Get<T>() where T : IFormattable, IEquatable<T>
        {
            return (SparseSolver<T>)_solvers[typeof(T)];
        }

        /// <summary>
        /// Try getting a <see cref="SparseSolver{T}"/> for a specified base type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="solver">The solver.</param>
        /// <returns></returns>
        public bool TryGet<T>(out SparseSolver<T> solver) where T : IFormattable, IEquatable<T>
        {
            if (_solvers.TryGetValue(typeof(T), out var obj))
            {
                solver = (SparseSolver<T>)obj;
                return true;
            }

            solver = null;
            return false;
        }

        /// <summary>
        /// Determines whether this instance contains a solver with the specified base type.
        /// </summary>
        /// <typeparam name="T">The base type.</typeparam>
        /// <returns>
        ///   <c>true</c> if the set contains a solver; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains<T>() where T : IFormattable, IEquatable<T> => _solvers.ContainsKey(typeof(T));

        /// <summary>
        /// Clears all solvers.
        /// </summary>
        public void Clear() => _solvers.Clear();

        /// <summary>
        /// Returns an enumerator that iterates through the types of solvers in the set.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Type> GetEnumerator() => _solvers.Keys.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
