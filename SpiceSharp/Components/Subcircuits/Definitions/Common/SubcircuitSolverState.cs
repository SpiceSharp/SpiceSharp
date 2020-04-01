using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A subcircuit simulation state with a solver.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <typeparam name="S">The parent simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract class SubcircuitSolverState<T, S> : ISolverSimulationState<T> where S : ISolverSimulationState<T> where T : IFormattable
    {
        /// <summary>
        /// The name of the subcircuit.
        /// </summary>
        protected readonly string Name;

        /// <summary>
        /// The parent simulation state.
        /// </summary>
        protected readonly S Parent;

        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public abstract ISparseSolver<T> Solver { get; }

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public abstract IVector<T> Solution { get; }

        /// <summary>
        /// Gets the map that maps variables to indices for the solver.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public abstract IVariableMap Map { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSolverState{T, S}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        protected SubcircuitSolverState(string name, S parent)
        {
            Name = name.ThrowIfNull(nameof(name));
            Parent = parent;
        }

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        public abstract IVariable<T> GetSharedVariable(string name);

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        public abstract IVariable<T> CreatePrivateVariable(string name, IUnit unit);

        /// <summary>
        /// Gets the comparer used for comparing variable names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => Parent.Comparer;

        /// <summary>
        /// Adds a variable to the dictionary.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="variable">The variable.</param>
        public void Add(string id, IVariable<T> variable)
        {
            // Add the variable but prefix our subcircuit name.
            Parent.Add(Name.Combine(id), variable);
        }

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        /// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
        /// </returns>
        public bool ContainsKey(string key)
        {
            // Always have the ground node global
            if (Comparer.Equals(key, Constants.Ground))
                return Parent.ContainsKey(key);

            return Parent.ContainsKey(Name.Combine(key));
        }

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the dictionary contains a variable that has the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(string key, out IVariable<T> value)
        {
            // Always have the ground node global
            if (Comparer.Equals(key, Constants.Ground))
                return Parent.TryGetValue(key, out value);
            return Parent.TryGetValue(Name.Combine(key), out value);
        }

        /// <summary>
        /// Gets the <see cref="IVariable{T}"/> with the specified key.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable{T}"/>.
        /// </value>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public IVariable<T> this[string key]
        {
            get
            {
                // Always have a global ground
                if (Comparer.Equals(key, Constants.Ground))
                    return Parent[key];
                return Parent[Name.Combine(key)];
            }
        }

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        public IEnumerable<string> Keys => Parent.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        public IEnumerable<IVariable<T>> Values => Parent.Values;

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => Parent.Count;

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<KeyValuePair<string, IVariable<T>>> GetEnumerator() => Parent.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
