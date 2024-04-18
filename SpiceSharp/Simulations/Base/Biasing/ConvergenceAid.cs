using SpiceSharp.Algebra;
using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A template for aiding convergence.
    /// </summary>
    /// <remarks>
    /// The convergence aid will try to bring the solution of a variable as close
    /// as possible to the specified value. If this value is close to the final
    /// solution, then convergence can be achieved much faster.
    /// </remarks>
    public class ConvergenceAid
    {
        private readonly IBiasingSimulationState _state;
        private readonly int _index;
        private readonly Element<double> _diagonal, _rhs;

        /// <summary>
        /// The amount with which a value is forced to the convergence aid value.
        /// </summary>
        private const double _force = 1.0e10;

        /// <summary>
        /// Gets the variable.
        /// </summary>
        /// <value>
        /// The variable.
        /// </value>
        public IVariable Variable { get; }

        /// <summary>
        /// Gets the value for the convergence aid.
        /// </summary>
        public double Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvergenceAid"/> class.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="state">The biasing simulation state.</param>
        /// <param name="value">The value.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="variable"/> or <paramref name="state"/> is <c>null</c>.</exception>
        /// <exception cref="SpiceSharpException">Thrown if the variable for the convergence aid was not found.</exception>
        public ConvergenceAid(IVariable variable, IBiasingSimulationState state, double value)
        {
            Variable = variable.ThrowIfNull(nameof(variable));
            Value = value;

            _state = state.ThrowIfNull(nameof(state));
            if (!state.Map.Contains(variable))
                throw new SpiceSharpException(Properties.Resources.Simulations_ConvergenceAidVariableNotFound.FormatString(variable.Name));
            _index = state.Map[variable];
            _diagonal = _state.Solver.GetElement(new MatrixLocation(_index, _index));
            _rhs = !Value.Equals(0.0) ? _state.Solver.GetElement(_index) : _state.Solver.FindElement(_index);
            _state.Solution[_index] = Value;
        }

        /// <summary>
        /// Aids the convergence.
        /// </summary>
        public virtual void Aid()
        {
            // Clear the row
            bool hasOtherTypes = false;
            foreach (var v in _state.Map)
            {
                // If the variable is a current, then we can't just set it to 0... 
                if (v.Key.Unit != Units.Volt)
                    hasOtherTypes = true;
                else
                {
                    var elt = _state.Solver.FindElement(new MatrixLocation(_index, v.Value));
                    if (elt != null)
                        elt.Value = 0.0;
                }
            }

            // If there are current contributions, then we can't just hard-set the value
            if (hasOtherTypes)
            {
                _diagonal.Value = _force;
                if (_rhs != null)
                    _rhs.Value = _force * Value;
            }
            else
            {
                _diagonal.Value = 1.0;
                if (_rhs != null)
                    _rhs.Value = Value;
            }
        }
    }
}
