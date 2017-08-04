using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a waveform
    /// </summary>
    public abstract class WaveformReader : Reader
    {
        /// <summary>
        /// The exported waveform
        /// </summary>
        public Waveform Current { get; private set; } = null;

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
        {
            this.id = id;
            this.keys = keys;
        }

        /// <summary>
        /// Create a new waveform
        /// </summary>
        /// <returns></returns>
        protected abstract Waveform Generate();

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The type of the waveform</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image.ToLower() != id)
                return false;
            Current = Generate();

            if (parameters.Count > keys.Length)
                throw new ParseException($"Error on line {name.beginLine}, column {name.beginColumn}: Too many parameters for waveform \"{name.image}\"");
            for (int i = 0; i < parameters.Count; i++)
                Current.Set(keys[i], parameters[i].ReadValue());

            return true;
        }
    }
}
