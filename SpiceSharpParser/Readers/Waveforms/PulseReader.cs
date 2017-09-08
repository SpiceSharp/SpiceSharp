using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers.Waveforms
{
    /// <summary>
    /// Reads <see cref="Pulse"/> waveforms.
    /// </summary>
    public class PulseReader : WaveformReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public PulseReader()
            : base("pulse", new string[] { "v1", "v2", "td", "tr", "tf", "pw", "per" })
        {
        }

        /// <summary>
        /// Generate a new pulse waveform
        /// </summary>
        /// <returns></returns>
        protected override IWaveform Generate(string type) => new Pulse();
    }
}
