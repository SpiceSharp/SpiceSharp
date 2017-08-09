using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read models for current switches
    /// </summary>
    public class CurrentSwitchModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentSwitchModelReader() : base("csw") { }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override CircuitModel GenerateModel(string name) => new CurrentSwitchModel(name);
    }
}
