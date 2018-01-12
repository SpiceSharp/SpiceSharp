using SpiceSharp.Diagnostics;

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
        /// The order of the state variable (= number of derivatives)
        /// </summary>
        int order;

        /// <summary>
        /// Gets or sets the value of the state at the current timepoint
        /// </summary>
        public double Value
        {
            get => source.Values[index];
            set => source.Values[index] = value;
        }

        /// <summary>
        /// Gets the first order derivative at the current timepoint
        /// </summary>
        public double Derivative
        {
            get
            {
                if (order > 0)
                    return source.Values[index + 1];
                throw new CircuitException("Invalid order");
            }
        }

        /// <summary>
        /// Constructor
        /// This constructor should not be used, except for in the <see cref="StatePool"/> class.
        /// </summary>
        /// <param name="source">Pool of states instantiating the variable</param>
        /// <param name="index">The index/identifier of the state variable</param>
        /// <param name="order">The order of the state variable</param>
        internal StateVariable(StatePool source, int index, int order)
        {
            this.source = source;
            this.index = index;
            this.order = order;
        }

        /// <summary>
        /// Get the derivative of the state variable
        /// </summary>
        /// <param name="order">The order (first derivative by default)</param>
        /// <returns></returns>
        public double GetDerivative(int order = 1)
        {
            if (order < this.order)
                return source.Values[index + order];
            throw new CircuitException("Invalid order");
        }

        /// <summary>
        /// Get a value of the state variable in history
        /// </summary>
        /// <param name="history">Number of points to go back in history (last point by default)</param>
        /// <returns></returns>
        public double GetPreviousValue(int history = 1) => source.GetPreviousValue(index, history);

        /// <summary>
        /// Get the timestep (in history)
        /// </summary>
        /// <param name="history">Number of steps to go back in history (current timestep by default)</param>
        /// <returns></returns>
        public double GetTimestep(int history = 0) => source.GetTimestep(history);

        /// <summary>
        /// Integrate the state variable
        /// </summary>
        /// <returns>The last order derivative</returns>
        public void Integrate()
        {
            for (int i = 0; i < order; i++)
                source.Integrate(index + i);
        }

        /// <summary>
        /// Integrate the state variable
        /// This method will also calculate contributions for the Y-matrix and Rhs-vector
        /// </summary>
        /// <param name="dqdv">The derivative of the state variable w.r.t. a voltage across</param>
        /// <param name="v">The voltage across</param>
        /// <returns>The contributions to the Y-matrix and Rhs-vector</returns>
        /// <returns></returns>
        public IntegrationMethod.Result Integrate(double dqdv, double v)
        {
            IntegrationMethod.Result result = null;
            for (int i = 0; i < order; i++)
                result = source.Integrate(index + i, dqdv, v);
            return result;
        }

        /// <summary>
        /// Truncate the timestep based on the LTE (Local Truncation Error)
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public void LocalTruncationError(ref double timestep)
        {
            for (int i = 0; i < order; i++)
                source.LocalTruncationError(index + i, ref timestep);
        }
    }
}
