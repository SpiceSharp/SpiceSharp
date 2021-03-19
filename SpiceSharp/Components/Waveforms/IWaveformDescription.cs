using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A waveform description.
    /// </summary>
    /// <seealso cref="IParameterSet" />
    public interface IWaveformDescription : IParameterSet, ICloneable<IWaveformDescription>
    {
        /// <summary>
        /// Creates a waveform instance for the specified simulation and entity.
        /// </summary>
        /// <param name="state">The time simulation state.</param>
        /// <returns>
        /// The waveform instance.
        /// </returns>
        IWaveform Create(IIntegrationMethod state);
    }
}
