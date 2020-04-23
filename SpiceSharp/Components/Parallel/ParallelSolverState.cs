using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace SpiceSharp.Components.ParallelBehaviors
{
    /// <summary>
    /// An abstract class with a default implementation for parallel access to solvers
    /// in parallel components.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <typeparam name="S">The base simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract class ParallelSolverState<T, S> : ISolverSimulationState<T>
        where S : ISolverSimulationState<T>
    {
        private readonly ParallelSolver<T> _solver;

        /// <summary>
        /// The parent simulation state.
        /// </summary>
        protected readonly S Parent;

        /// <summary>
        /// Gets the <see cref="IVariable{Complex}"/> with the specified name.
        /// </summary>
        /// <value>
        /// The <see cref="IVariable{Complex}"/>.
        /// </value>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public IVariable<T> this[string name] => Parent[name];

        /// <summary>
        /// Gets the number of elements in the collection.
        /// </summary>
        public int Count => Parent.Count;

        /// <summary>
        /// Gets the comparer used for comparing variable names.
        /// </summary>
        /// <value>
        /// The comparer.
        /// </value>
        public IEqualityComparer<string> Comparer => Parent.Comparer;

        /// <summary>
        /// Gets an enumerable collection that contains the keys in the read-only dictionary.
        /// </summary>
        public IEnumerable<string> Keys => Parent.Keys;

        /// <summary>
        /// Gets an enumerable collection that contains the values in the read-only dictionary.
        /// </summary>
        public IEnumerable<IVariable<T>> Values => Parent.Values;

        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISparsePivotingSolver<T> Solver => _solver;

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IVector<T> Solution => Parent.Solution;

        /// <summary>
        /// Gets the map that maps variables to indices for the solver.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map => Parent.Map;

        /// <summary>
        /// Initializes a new instance of the <see cref="ParallelSolverState{T, S}"/> class.
        /// </summary>
        /// <param name="parent">The parent simulation state.</param>
        protected ParallelSolverState(S parent)
        {
            Parent = parent;
            _solver = new ParallelSolver<T>(parent.Solver);
        }

        /// <summary>
        /// Resets the elements.
        /// </summary>
        public void Reset() => _solver.Reset();

        /// <summary>
        /// Applies the changes to the common solver.
        /// </summary>
        public void Apply() => _solver.Apply();

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        public IVariable<T> GetSharedVariable(string name) => Parent.GetSharedVariable(name);

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        public IVariable<T> CreatePrivateVariable(string name, IUnit unit) => Parent.CreatePrivateVariable(name, unit);

        /// <summary>
        /// Adds a variable to the dictionary.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="variable">The variable.</param>
        public void Add(string id, IVariable<T> variable) => Parent.Add(id, variable);

        /// <summary>
        /// Determines whether the read-only dictionary contains an element that has the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <returns>
        /// true if the read-only dictionary contains an element that has the specified key; otherwise, false.
        /// </returns>
        public bool ContainsKey(string key) => Parent.ContainsKey(key);

        /// <summary>
        /// Gets the value that is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate.</param>
        /// <param name="value">When this method returns, the value associated with the specified key, if the key is found; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns>
        /// <c>true</c> if the dictionary contains a variable that has the specified key; otherwise, <c>false</c>.
        /// </returns>
        public bool TryGetValue(string key, out IVariable<T> value) => Parent.TryGetValue(key, out value);

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
