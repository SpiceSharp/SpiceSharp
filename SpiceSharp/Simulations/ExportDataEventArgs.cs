using System;
using SpiceSharp.Circuits;
using SpiceSharp.IntegrationMethods;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Exported simulation data. Can be used by simulations to pass exported simulation data as an event argument.
    /// </summary>
    public class ExportDataEventArgs : EventArgs
    {
        /// <summary>
        /// Get the state
        /// </summary>
        public State State { get; }

        public ComplexState ComplexState { get; }

        /// <summary>
        /// Get the integration method if any
        /// </summary>
        public IntegrationMethod Method { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        public ExportDataEventArgs(State state)
        {
            State = state;
            Method = null;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="method">Method</param>
        public ExportDataEventArgs(State state, IntegrationMethod method)
        {
            State = state;
            Method = method;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="state">State</param>
        /// <param name="complexState">Complex state</param>
        public ExportDataEventArgs(State state, ComplexState complexState)
        {
            State = state;
            ComplexState = complexState;
        }

        /// <summary>
        /// Get the frequency
        /// </summary>
        /// <returns></returns>
        public double Frequency => ComplexState.Laplace.Imaginary / (2.0 * Math.PI);

        /// <summary>
        /// Get the current time
        /// </summary>
        /// <returns></returns>
        public double Time => Method.Time;
    }

    /// <summary>
    /// Delegate for finalizing a simulation export
    /// </summary>
    /// <param name="sender">The object sending the event</param>
    /// <param name="circuit">The simulation data</param>
    public delegate void FinalizeSimulationExportEventHandler(object sender, Circuit circuit);
}
