using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// A solver simulation state that only maps the names of the variables.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <typeparam name="S">The parent simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public class FlatSolverState<T, S> : SubcircuitSolverState<T, S> where S : ISolverSimulationState<T>
    {
        /// <summary>
        /// Gets the solver used to solve the system of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public override ISparsePivotingSolver<T> Solver => Parent.Solver;

        /// <summary>
        /// Gets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public override IVector<T> Solution => Parent.Solution;

        /// <summary>
        /// Gets the map that maps variables to indices for the solver.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public override IVariableMap Map => Parent.Map;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSolverState{T, S}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="nodes">The nodes.</param>
        protected FlatSolverState(string name, S parent, IEnumerable<Bridge<string>> nodes)
            : base(name, parent)
        {
            // Create aliases for each node
            foreach (var bridge in nodes)
            {
                var elt = Parent.GetSharedVariable(bridge.Global);
                Add(bridge.Local, elt);
            }
        }

        /// <summary>
        /// Gets a variable that can be shared with other behaviors by the factory. If another variable
        /// already exists with the same name, that is returned instead.
        /// </summary>
        /// <param name="name">The name of the shared variable.</param>
        /// <returns>
        /// The shared variable.
        /// </returns>
        public override IVariable<T> GetSharedVariable(string name)
        {
            // Don't make the ground node local
            if (Parent.Comparer.Equals(name, Constants.Ground))
                return Parent.GetSharedVariable(name);

            return Parent.GetSharedVariable(Name.Combine(name));
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
        public override IVariable<T> CreatePrivateVariable(string name, IUnit unit) => Parent.CreatePrivateVariable(Name.Combine(name), unit);
    }
}
