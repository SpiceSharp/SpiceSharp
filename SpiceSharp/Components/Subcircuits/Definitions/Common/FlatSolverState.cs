using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Variables;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Subcircuits
{
    /// <summary>
    /// A solver simulation state that maps the names of the variables such that they are
    /// identified as being internal if necessary.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <typeparam name="S">The parent simulation state type.</typeparam>
    /// <seealso cref="ISolverSimulationState{T}" />
    public class FlatSolverState<T, S> : SubcircuitSolverState<T, S>
        where S : ISolverSimulationState<T>
    {
        /// <inheritdoc/>
        public override ISparsePivotingSolver<T> Solver => Parent.Solver;

        /// <inheritdoc/>
        public override IVector<T> Solution => Parent.Solution;

        /// <inheritdoc/>
        public override IVariableMap Map => Parent.Map;

        /// <summary>
        /// Initializes a new instance of the <see cref="SubcircuitSolverState{T, S}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="nodes">The nodes.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> or <paramref name="parent"/> is <c>null</c>.</exception>
        protected FlatSolverState(string name, S parent, IEnumerable<Bridge<string>> nodes)
            : base(name, parent)
        {
            // Create aliases for each node
            if (nodes != null)
            {
                foreach (var bridge in nodes)
                {
                    var elt = Parent.GetSharedVariable(bridge.Global);
                    Add(bridge.Local, elt);
                }
            }
        }

        /// <inheritdoc/>
        public override IVariable<T> GetSharedVariable(string name)
        {
            // Don't make the ground node local
            if (Parent.Comparer.Equals(name, Constants.Ground))
                return Parent.GetSharedVariable(name);

            return Parent.GetSharedVariable(Name.Combine(name));
        }

        /// <inheritdoc/>
        public override IVariable<T> CreatePrivateVariable(string name, IUnit unit) => Parent.CreatePrivateVariable(Name.Combine(name), unit);
    }
}
