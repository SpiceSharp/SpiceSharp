using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a resistor model
    /// </summary>
    public class ResistorModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResistorModelReader() : base("r") { }

        /// <summary>
        /// Generate a resistor model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override CircuitModel GenerateModel(string name) => new ResistorModel(name);
    }
}
