using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.SamplerBehaviors
{
    /// <summary>
    /// The parameters for a <see cref="Sampler"/>.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class Parameters : ParameterSet
    {
        /// <summary>
        /// Occurs when data can be exported.
        /// </summary>
        public event EventHandler<ExportDataEventArgs> ExportSimulationData;

        /// <summary>
        /// Gets or sets the time-points that need to be hit.
        /// </summary>
        /// <value>
        /// The time-points to be hit.
        /// </value>
        [ParameterName("points"), ParameterInfo("The points that need to be hit")]
        public IEnumerable<double> Points { get; set; }

        /// <summary>
        /// Gets or sets the minimum timestep that needs to be guaranteed by the sampler.
        /// </summary>
        /// <value>
        /// The minimum timestep.
        /// </value>
        [ParameterName("deltamin"), ParameterInfo("The minimum timestep that needs to be guaranteed by the sampler")]
        public double MinDelta { get; set; } = 1e-12;

        /// <summary>
        /// Calls the <see cref="ExportSimulationData"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The argument.</param>
        public void Export(object sender, ExportDataEventArgs args)
        {
            ExportSimulationData?.Invoke(sender, args);
        }
    }
}
