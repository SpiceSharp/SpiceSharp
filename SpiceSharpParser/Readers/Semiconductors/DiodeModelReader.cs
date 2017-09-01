using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read diode models
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
        protected override ICircuitObject GenerateModel(string name, string type) => new DiodeModel(name);
    }
}
