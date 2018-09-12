namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Proxy class for extracting the right state variables and values
    /// </summary>
    public abstract class StateDerivative : StateHistory
    {
        /// <summary>
        /// Gets the current derivative
        /// </summary>
        public abstract double Derivative { get; }

        /// <summary>
        /// Calculate contribution to the jacobian matrix (or Y-matrix). 
        /// Using this means the state variable depends on the derivative of an unknown variable (eg.
        /// the voltage across a capacitor). <paramref name="derivative"/> is the derivative of the state
        /// variable w.r.t. the unknown variable.
        /// </summary>
        /// <param name="derivative">Derivative of the state variable w.r.t. the unknown variable</param>
        /// <returns></returns>
        public abstract double Jacobian(double derivative);

        /// <summary>
        /// Calculate contribution to the rhs vector (right-hand side vector).
        /// The state variable can be nonlinearly dependent of the unknown variables
        /// it is derived of.
        /// </summary>
        /// <param name="jacobianValue">Jacobian matrix contribution</param>
        /// <param name="currentValue">The current value of the unknown variable</param>
        /// <returns></returns>
        public virtual double RhsCurrent(double jacobianValue, double currentValue) =>
            Derivative - jacobianValue * currentValue;

        /// <summary>
        /// Calculate contribution to the rhs vector (right-hand side vector).
        /// The state variable is assumed to be linearly dependent of the unknown variables
        /// it is derived of. Ie. Q = dqdv * v (v is the unknown)
        /// variable.
        /// </summary>
        /// <returns></returns>
        public abstract double RhsCurrent();

        /// <summary>
        /// Calculate the derivative
        /// </summary>
        public abstract void Integrate();
    }
}
