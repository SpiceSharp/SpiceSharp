using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="DiodeModel"/> definitions.
    /// </summary>
    public class DiodeModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DiodeModelReader() : base("d") { }

        /// <summary>
        /// Generate a diode model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override CircuitObject GenerateModel(CircuitIdentifier name, string type) => new DiodeModel(name);
    }
}
