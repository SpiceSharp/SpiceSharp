namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Information for filling in the Jacobian matrix and right-hand-side vector.
    /// </summary>
    public readonly struct JacobianInfo
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

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override readonly bool Equals(object obj)
        {
            if (obj is JacobianInfo ji)
            {
                if (!Jacobian.Equals(ji.Jacobian))
                    return false;
                if (!Rhs.Equals(ji.Rhs))
                    return false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override readonly int GetHashCode() => (Jacobian.GetHashCode() * 13) ^ (Rhs.GetHashCode());

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(JacobianInfo left, JacobianInfo right) => left.Equals(right);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(JacobianInfo left, JacobianInfo right) => !left.Equals(right);
    }
}
