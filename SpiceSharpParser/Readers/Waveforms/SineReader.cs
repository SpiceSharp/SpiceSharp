using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers.Waveforms
{
    /// <summary>
    /// Reads <see cref="Sine"/> waveforms.
    /// </summary>
    public class SineReader : WaveformReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SineReader()
            : base("sine", new string[] { "vo", "va", "freq", "td", "theta" })
        {
        }

        /// <summary>
        /// Generate a new sine waveform
        /// </summary>
        /// <returns></returns>
        protected override IWaveform Generate(string type) => new Sine();
    }
}
