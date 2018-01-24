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

        /// <summary>
        /// Get the integration method if any
        /// </summary>
        public IntegrationMethod Method { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        public ExportDataEventArgs(State state, IntegrationMethod method = null)
        {
            State = state;
            Method = method;
        }

        /// <summary>
        /// Get the frequency
        /// </summary>
        /// <returns></returns>
        public double GetFrequency()
        {
            return State.Laplace.Imaginary / (2.0 * Math.PI);
        }

        /// <summary>
        /// Get the current time
        /// </summary>
        /// <returns></returns>
        public double GetTime()
        {
            return Method.Time;
        }
    }
    
    /// <summary>
    /// Delegate for exporting simulation data
    /// </summary>
    /// <param name="sender">The object sending the event</param>
    /// <param name="data">The simulation data</param>
    public delegate void ExportSimulationDataEventHandler(object sender, ExportDataEventArgs data);

    /// <summary>
    /// Delegate for finalizing a simulation export
    /// </summary>
    /// <param name="sender">The object sending the event</param>
    /// <param name="ckt">The simulation data</param>
    public delegate void FinalizeSimulationExportEventHandler(object sender, Circuit ckt);
}
