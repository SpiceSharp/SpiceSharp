using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switch models
    /// </summary>
    public class VoltageSwitchModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VoltageSwitchModelReader() : base("sw") { }

        /// <summary>
        /// Generate a voltage switch model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override CircuitModel GenerateModel(string name) => new VoltageSwitchModel(name);
    }
}
