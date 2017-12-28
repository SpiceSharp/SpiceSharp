namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Proxy class for extracting the right state variables and values
    /// </summary>
    public class StateVariable
    {
        /// <summary>
        /// The source of all variables
        /// </summary>
        StatePool source;

        /// <summary>
        /// The identifier/index for the state variable
        /// </summary>
        int index;

        /// <summary>
        /// Gets or sets the value of the state at the current timepoint
        /// </summary>
        public double Value
        {
            get => source.First.Values[index];
            set => source.First.Values[index] = value;
        }

        /// <summary>
        /// Gets or sets the derivative of the state at the current timepoint
        /// </summary>
        public double Derivative
        {
            get => source.First.Derivatives[index];
        }

        /// <summary>
        /// Constructor
        /// This constructor should not be used, except for in the <see cref="StatePool"/> class.
        /// </summary>
        /// <param name="source">Pool of states instantiating the variable</param>
        /// <param name="index">The index/identifier of the state variable</param>
        internal StateVariable(StatePool source, int index)
        {
            this.source = source;
            this.index = index;
        }

        /// <summary>
        /// Integrate the state variable
        /// </summary>
        /// <param name="cap">Capacitance</param>
        /// <returns></returns>
        public IntegrationMethod.Result Integrate(double cap) => source.Integrate(index, cap);
    }
}
