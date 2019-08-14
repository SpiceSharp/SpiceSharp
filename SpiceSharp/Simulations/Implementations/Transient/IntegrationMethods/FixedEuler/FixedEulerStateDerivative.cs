namespace SpiceSharp.IntegrationMethods
{
    public partial class FixedEuler
    {
        /// <summary>
        /// A state that can be derived by the integration method
        /// </summary>
        /// <seealso cref="SpiceSharp.IntegrationMethods.StateDerivative" />
        protected class FixedEulerStateDerivative : StateDerivative
        {
            private readonly int _index;
            private readonly FixedEuler _method;
            private readonly History<IntegrationState> _states;

            /// <summary>
            /// Gets or sets the value of the state at the current timepoint.
            /// </summary>
            public override double Current
            {
                get => _states[0].State[_index];
                set => _states[0].State[_index] = value;
            }

            /// <summary>
            /// Gets the <see cref="System.Double"/> at the specified index.
            /// </summary>
            /// <param name="index">The index.</param>
            /// <returns></returns>
            public override double this[int index] => _states[index].State[_index];

            /// <summary>
            /// Gets the current derivative.
            /// </summary>
            public override double Derivative => _states[0].State[_index + 1];

            /// <summary>
            /// Calculate contribution to the jacobian matrix (or Y-matrix).
            /// </summary>
            /// <param name="derivative">Derivative of the state variable with respect to the unknown variable.</param>
            /// <returns>
            /// A value that can be added to the element in the Y-matrix.
            /// </returns>
            /// <remarks>
            /// The value returned by this method means that the state variable depends on the derivative of an unknown variable (eg.
            /// the voltage across a capacitor). <paramref name="derivative" /> is the derivative of the state variable w.r.t. the
            /// unknown variable.
            /// </remarks>
            public override double Jacobian(double derivative) => derivative * _method.Slope;

            /// <summary>
            /// Initializes a new instance of the <see cref="FixedEulerStateDerivative"/> class.
            /// </summary>
            /// <param name="method">The method.</param>
            public FixedEulerStateDerivative(FixedEuler method)
            {
                _method = method.ThrowIfNull(nameof(method));
                _index = method.StateManager.AllocateState(1);
                _states = method.IntegrationStates;
            }

            /// <summary>
            /// Calculate contribution to the rhs vector (right-hand side vector).
            /// </summary>
            /// <returns>
            /// A value that can be added to the element in the right-hand side vector.
            /// </returns>
            /// <remarks>
            /// The state variable is assumed to be linearly dependent of the unknown variables
            /// it is derived of. Ie. Q = dqdv * v (v is the unknown).
            /// </remarks>
            public override double RhsCurrent() =>
                _states[0].State[_index + 1] - _method.Slope * _states[0].State[_index];

            /// <summary>
            /// Calculates the derivative.
            /// </summary>
            public override void Integrate()
            {
                var current = _states[0].State;
                var previous = _states[1].State;

                current[_index + 1] = (current[_index] - previous[_index]) * _method.Slope;
            }
        }
    }
}
