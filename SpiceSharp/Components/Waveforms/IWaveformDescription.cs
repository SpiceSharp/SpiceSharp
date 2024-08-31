using SpiceSharp.Entities;
using SpiceSharp.ParameterSets;

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
        /// <param name="context">The binding context.</param>
        /// <returns>
        /// The waveform instance.
        /// </returns>
        IWaveform Create(IBindingContext context);
    }
}
