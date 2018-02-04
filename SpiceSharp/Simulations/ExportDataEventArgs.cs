using System;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Exported simulation data. Can be used by simulations to pass exported simulation data as an event argument.
    /// </summary>
    public class ExportDataEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the state (null if not available)
        /// </summary>
        public RealState State { get; }

        /// <summary>
        /// Gets the complex state (null if not available)
        /// </summary>
        public ComplexState ComplexState { get; }

        /// <summary>
        /// Gets the integration method (null if not available)
        /// </summary>
        public IntegrationMethod Method { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        public ExportDataEventArgs(RealState state)
        {
            State = state;
            Method = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="method">Method</param>
        public ExportDataEventArgs(RealState state, IntegrationMethod method)
        {
            State = state;
            Method = method;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="complexState">Complex state</param>
        public ExportDataEventArgs(RealState state, ComplexState complexState)
        {
            State = state;
            ComplexState = complexState;
        }

        /// <summary>
        /// Gets the frequency
        /// </summary>
        /// <returns></returns>
        public double Frequency => ComplexState.Laplace.Imaginary / (2.0 * Math.PI);

        /// <summary>
        /// Gets the current time
        /// </summary>
        /// <returns></returns>
        public double Time => Method.Time;
    }
}
