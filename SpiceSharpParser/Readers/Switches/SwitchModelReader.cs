using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="VoltageSwitchModel"/> and <see cref="CurrentSwitchModel"/> definitions.
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
        protected override CircuitObject GenerateModel(CircuitIdentifier name, string type)
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
