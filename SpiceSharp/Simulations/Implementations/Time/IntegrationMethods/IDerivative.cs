namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a state with a value that can also calculate the
    /// derivative with respect to time.
    /// </summary>
    public interface IDerivative
    {
        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        double Value { get; set; }

        /// <summary>
        /// Gets the current derivative.
        /// </summary>
        /// <value>
        /// The derivative.
        /// </value>
        double Derivative { get; }

        /// <summary>
        /// Gets a previous value. An index of 0 indicates the current value.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        double GetPreviousValue(int index);

        /// <summary>
        /// Gets a previous derivative. An index of 0 indicates the current value.
        /// </summary>
        /// <param name="index">The number of points to go back in time.</param>
        /// <returns>
        /// The previous derivative.
        /// </returns>
        double GetPreviousDerivative(int index);

        /// <summary>
        /// Gets the Y-matrix value and Rhs-vector contributions for the derived quantity.
        /// </summary>
        /// <param name="coefficient">The coefficient of the quantity that is derived.</param>
        /// <param name="currentValue">The current value of the derived state.</param>
        /// <returns>The information for filling in the Y-matrix and Rhs-vector.</returns>
        JacobianInfo GetContributions(double coefficient, double currentValue);

        /// <summary>
        /// Gets the Y-matrix value and Rhs-vector contributions for the derived quantity.
        /// The relationship is assumed to be linear.
        /// </summary>
        /// <param name="coefficient">The coefficient of the quantity that is derived.</param>
        /// <returns>The information for filling in the Y-matrix and Rhs-vector.</returns>
        JacobianInfo GetContributions(double coefficient);

        /// <summary>
        /// Integrates the state (calculates the derivative from the value).
        /// </summary>
        void Derive();
    }
}
