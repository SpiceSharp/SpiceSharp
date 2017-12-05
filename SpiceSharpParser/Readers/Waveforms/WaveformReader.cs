using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Describes methods for reading a waveform. This is an abstract class.
    /// </summary>
    public abstract class WaveformReader : Reader
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private string[] keys;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="keys">The keys</param>
        public WaveformReader(string id, string[] keys)
            : base(StatementType.Waveform)
        {
            Identifier = id;
            this.keys = keys;
        }

        /// <summary>
        /// Create a new waveform
        /// </summary>
        /// <returns></returns>
        protected abstract Waveform Generate(string type);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The type of the waveform</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            Waveform w = Generate(type);

            if (st.Parameters.Count > keys.Length)
                throw new ParseException(st.Name, $"Too many arguments for waveform \"{st.Name.image}\"");

            var dict = ReaderExtension.GetParameters(w.GetType());
            for (int i = 0; i < st.Parameters.Count; i++)
            {
                dict[keys[i]].Set(w, netlist.ParseDouble(st.Parameters[i]));
            }

            Generated = w;
            return true;
        }
    }
}
