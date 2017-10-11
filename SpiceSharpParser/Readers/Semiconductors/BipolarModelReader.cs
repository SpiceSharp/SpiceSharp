using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="BJTModel"/> definitions.
    /// </summary>
    public class BipolarModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BipolarModelReader()
            : base("npn;pnp")
        {
        }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name">Model name</param>
        /// <param name="type">Model type</param>
        /// <returns></returns>
        protected override ICircuitObject GenerateModel(CircuitIdentifier name, string type)
        {
            BJTModel model = new BJTModel(name);
            if (type == "npn")
                model.SetNPN(true);
            else if (type == "pnp")
                model.SetPNP(true);
            return model;
        }
    }
}
