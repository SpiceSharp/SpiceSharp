namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a state with a derivative that can also calculate the
    /// integrated value with respect tjo time.
    /// </summary>
    public interface IIntegral
    {
        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        double Value { get; }

        /// <summary>
        /// Gets or sets the current derivative.
        /// </summary>
        /// <value>
        /// The derivative.
        /// </value>
        double Derivative { get; set; }

        /// <summary>
        /// Gets a previous value. An index of 0 indicates the current value.
        /// </summary>
        /// <param name="index">The number of points to go back in time.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        double GetPreviousValue(int index);

        /// <summary>
        /// Gets a previous derivative. An index of 0 indicates the current derivative.
        /// </summary>
        /// <param name="index">The number of points to go back in time.</param>
        /// <returns>
        /// The previous value.
        /// </returns>
        double GetPreviousDerivative(int index);

        /// <summary>
        /// Gets the Y-matrix value and Rhs-vector contributions for the integrated quantity.
        /// </summary>
        /// <param name="coefficient">The coefficient of the quantity that is integrated.</param>
        /// <param name="currentValue">The current value of the integrated state.</param>
        /// <returns>The information for fijlling in the Y-matrix and Rhs-vector.</returns>
        JacobianInfo GetContributions(double coefficient, double currentValue);

        /// <summary>
        /// Gets the Y-matrix value and Rhs-vector contributions for the derived quantity.
        /// The relationship is assumed to be linear.
        /// </summary>
        /// <param name="coefficient">The coefficient of the quantity that is integrated.</param>
        /// <returns>The information for filling in the Y-matrix and Rhs-vector.</returns>
        JacobianInfo GetContributions(double coefficient);

        /// <summary>
        /// Integrates the state (calculates the value from the derivative).
        /// </summary>
        void Integrate();
    }
}
