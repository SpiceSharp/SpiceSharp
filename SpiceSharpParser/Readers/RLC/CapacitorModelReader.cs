using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class capable of reading capacitor models
    /// </summary>
    public class CapacitorModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CapacitorModelReader() : base("c") { }

        /// <summary>
        /// Create a capacitor model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override CircuitModel GenerateModel(string name) => new CapacitorModel(name);
    }
}
