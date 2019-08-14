namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A template a <see cref="StateHistory" /> that also can be derived.
    /// </summary>
    /// <seealso cref="StateHistory" />
    public abstract class StateDerivative : StateHistory
    {
        /// <summary>
        /// Gets the current derivative.
        /// </summary>
        public abstract double Derivative { get; }

        /// <summary>
        /// Calculate contribution to the jacobian matrix (or Y-matrix). 
        /// </summary>
        /// <param name="derivative">Derivative of the state variable with respect to the unknown variable.</param>
        /// <returns>
        /// A value that can be added to the element in the Y-matrix.
        /// </returns>
        /// <remarks>
        /// The value returned by this method means that the state variable depends on the derivative of an unknown variable (eg.
        /// the voltage across a capacitor). <paramref name="derivative"/> is the derivative of the state variable w.r.t. the 
        /// unknown variable.
        /// </remarks>
        public abstract double Jacobian(double derivative);

        /// <summary>
        /// Calculate contribution to the rhs vector (right-hand side vector).
        /// </summary>
        /// <param name="jacobianValue">The Jacobian matrix contribution.</param>
        /// <param name="currentValue">The current value of the unknown variable.</param>
        /// <returns>
        /// A value that can be added to the element in the right-hand side vector.
        /// </returns>
        /// <remarks>
        /// The state variable can be nonlinearly dependent of the unknown variables
        /// it is derived of.
        /// </remarks>
        public virtual double RhsCurrent(double jacobianValue, double currentValue) =>
            Derivative - jacobianValue * currentValue;

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
        public abstract double RhsCurrent();

        /// <summary>
        /// Calculates the derivative.
        /// </summary>
        public abstract void Integrate();
    }
}
