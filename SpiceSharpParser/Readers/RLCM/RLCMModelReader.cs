using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="ResistorModel"/> and <see cref="CapacitorModel"/> definitions.
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
        protected override CircuitObject GenerateModel(CircuitIdentifier name, string type)
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
