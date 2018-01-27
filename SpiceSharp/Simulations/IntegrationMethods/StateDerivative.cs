namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Proxy class for extracting the right state variables and values
    /// </summary>
    public class StateDerivative : StateHistory
    {
        /// <summary>
        /// Gets the first order derivative at the current timepoint
        /// </summary>
        public double Derivative => Source.Values[Index + 1];

        /// <summary>
        /// Constructor
        /// This constructor should not be used, except for in the <see cref="StatePool"/> class.
        /// </summary>
        /// <param name="source">Pool of states instantiating the variable</param>
        /// <param name="index">The index/identifier of the state variable</param>
        internal StateDerivative(StatePool source, int index) : base(source, index)
        {
        }

        /// <summary>
        /// Integrate the state variable
        /// </summary>
        /// <returns>The last order derivative</returns>
        public void Integrate() => Source.Integrate(Index);

        /// <summary>
        /// Calculate contribution to the jacobian matrix (or Y-matrix). 
        /// Using this means the state variable depends on the derivative of an unknown variable (eg.
        /// the voltage across a capacitor). <paramref name="dqdv"/> is the derivative of the state
        /// variable w.r.t. the unknown variable.
        /// </summary>
        /// <param name="dqdv">Derivative of the state variable w.r.t. the unknown variable</param>
        /// <returns></returns>
        public double Jacobian(double dqdv)
        {
            return dqdv * Source.Method.Slope;
        }

        /// <summary>
        /// Calculate contribution to the rhs vector (right-hand side vector).
        /// The state variable can be nonlinearly dependent of the unknown variables
        /// it is derived of.
        /// </summary>
        /// <param name="geq">Jacobian matrix contribution</param>
        /// <param name="v">The value of the unknown variable</param>
        /// <returns></returns>
        public double Current(double geq, double v)
        {
            return Source.Values[Index + 1] - geq * v;
        }

        /// <summary>
        /// Calculate contribution to the rhs vector (right-hand side vector).
        /// The state variable is assumed to be linearly dependent of the unknown variables
        /// it is derived of. Ie. Q = dqdv * v (v is the unknown)
        /// variable.
        /// </summary>
        /// <returns></returns>
        public double Current()
        {
            return Source.Values[Index + 1] - Source.Method.Slope * Source.Values[Index];
        }

        /// <summary>
        /// Truncate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public void LocalTruncationError(ref double timestep) => Source.LocalTruncationError(Index, ref timestep);
    }
}
