using SpiceSharp.Components;

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
        public DiodeModelReader() : base("D") { }

        /// <summary>
        /// Generate a diode model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override CircuitModel GenerateModel(string name) => new DiodeModel(name);
    }
}
