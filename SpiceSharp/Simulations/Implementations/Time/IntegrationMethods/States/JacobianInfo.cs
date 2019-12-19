namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Information for filling in the Jacobian matrix and right-hand-side vector.
    /// </summary>
    public struct JacobianInfo
    {
        /// <summary>
        /// Gets the jacobian.
        /// </summary>
        /// <value>
        /// The jacobian.
        /// </value>
        public double Jacobian { get; }

        /// <summary>
        /// Gets the right-hand-side value.
        /// </summary>
        /// <value>
        /// The right-hand-side value.
        /// </value>
        public double Rhs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JacobianInfo"/> struct.
        /// </summary>
        /// <param name="jacobian">The jacobian.</param>
        /// <param name="rhs">The right-hand-side value.</param>
        public JacobianInfo(double jacobian, double rhs)
        {
            Jacobian = jacobian;
            Rhs = rhs;
        }
    }
}
