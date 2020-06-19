using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A waveform description.
    /// </summary>
    /// <seealso cref="IParameterSet" />
    public interface IWaveformDescription : IParameterSet
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
