using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a waveform
    /// </summary>
    public abstract class WaveformReader : Reader
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private string[] keys;
        private string id;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keys">The keys</param>
        public WaveformReader(string id, string[] keys)
            : base(StatementType.Waveform)
        {
            this.id = id;
            this.keys = keys;
        }

        /// <summary>
        /// Create a new waveform
        /// </summary>
        /// <returns></returns>
        protected abstract IWaveform Generate(string type);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The type of the waveform</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            IParameterized w = (IParameterized)Generate(type);

            if (st.Parameters.Count > keys.Length)
                throw new ParseException(st.Name, $"Too many arguments for waveform \"{st.Name.image}\"");
            for (int i = 0; i < st.Parameters.Count; i++)
                w.Set(keys[i], netlist.ParseDouble(st.Parameters[i]));

            Generated = w;
            return true;
        }
    }
}
