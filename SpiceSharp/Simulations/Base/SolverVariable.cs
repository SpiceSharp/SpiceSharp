using SpiceSharp.Simulations.Variables;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An <see cref="IVariable{T}"/> that takes its value from an <see cref="ISolverSimulationState{T}"/>.
    /// </summary>
    /// <typeparam name="T">The base value type.</typeparam>
    /// <seealso cref="IVariable{T}" />
    public class SolverVariable<T> : IVariable<T>
    {
        private readonly ISolverSimulationState<T> _state;
        private readonly int _index;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public T Value => _state.Solution[_index];

        /// <inheritdoc/>
        public IUnit Unit { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SolverVariable{T}"/> class.
        /// </summary>
        /// <param name="state">The state where to find the solution of the variable.</param>
        /// <param name="name">The name of the variable.</param>
        /// <param name="index">The index of the variable.</param>
        /// <param name="unit">The unit of the variable.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="state"/> is <c>null</c>.</exception>
        public SolverVariable(ISolverSimulationState<T> state, string name, int index, IUnit unit)
        {
            _state = state.ThrowIfNull(nameof(state));
            Name = name;
            _index = index;
            Unit = unit;
        }

        /// <summary>
        /// Converts to string.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "{0} ({1})".FormatString(Name, Unit);
        }
    }
}
