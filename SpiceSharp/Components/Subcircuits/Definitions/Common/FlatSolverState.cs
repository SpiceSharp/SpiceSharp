using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A solver simulation state.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public abstract class FlatSolverState<T, S> : ISolverSimulationState<T> where S : ISolverSimulationState<T> where T : IFormattable
    {
        private readonly string _name;
        private readonly Dictionary<string, string> _nodeMap;

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
        public ISparseSolver<T> Solver => Parent.Solver;

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
        /// Gets all shared variables.
        /// </summary>
        /// <value>
        /// The shared variables.
        /// </value>
        public IVariableSet<IVariable<T>> Variables => Parent.Variables;

        /// <summary>
        /// Initializes a new instance of the <see cref="FlatSolverState{T, S}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="nodes">The nodes.</param>
        /// <param name="parent">The parent.</param>
        protected FlatSolverState(string name, IEnumerable<Bridge<string>> nodes, S parent)
        {
            _name = name.ThrowIfNull(nameof(name));
            _nodeMap = new Dictionary<string, string>(parent.Variables.Comparer);
            Parent = parent;

            // We make sure that the ground node is never made local
            _nodeMap.Add(Constants.Ground, Constants.Ground);
            foreach (var node in nodes)
                _nodeMap.Add(node.Local, node.Global);
        }

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        public IVariable<T> GetSharedVariable(string name)
        {
            if (!_nodeMap.TryGetValue(name, out var mapped))
                mapped = _name.Combine(name);
            return Parent.GetSharedVariable(mapped);
        }

        /// <summary>
        /// Creates a variable that is private to whoever requested it. The factory will not shared this
        /// variable with anyone else, and the name is only used for display purposes.
        /// </summary>
        /// <param name="name">The name of the private variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <returns>
        /// The private variable.
        /// </returns>
        public IVariable<T> CreatePrivateVariable(string name, IUnit unit) => Parent.CreatePrivateVariable(_name.Combine(name), unit);

        IVariableSet IVariableFactory.Variables => Parent.Variables;
        IVariable IVariableFactory.GetSharedVariable(string name) => Parent.GetSharedVariable(name);
        IVariable IVariableFactory.CreatePrivateVariable(string name, IUnit unit) => Parent.CreatePrivateVariable(name, unit);
    }
}
