using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Resistor, Inductor, Capacitor, Mutual inductance model readers
    /// Only resistors and capacitors can have models, the rest is skipped
    /// </summary>
    public class RLCMModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RLCMModelReader()
            : base("r;c")
        {
        }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name">The model name</param>
        /// <param name="type">The type</param>
        /// <returns></returns>
        protected override ICircuitObject GenerateModel(string name, string type)
        {
            switch (type)
            {
                case "r": return new ResistorModel(name);
                case "c": return new CapacitorModel(name);
            }
            return null;
        }
    }
}
