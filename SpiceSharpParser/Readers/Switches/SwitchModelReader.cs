using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switch models
    /// </summary>
    public class SwitchModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SwitchModelReader() : base("sw;csw") { }

        /// <summary>
        /// Generate a voltage switch model
        /// </summary>
        /// <param name="name">Name</param>
        /// <returns></returns>
        protected override ICircuitObject GenerateModel(string name, string type)
        {
            switch (type)
            {
                case "sw": return new VoltageSwitchModel(name);
                case "csw": return new CurrentSwitchModel(name);
            }
            return null;
        }
    }
}
