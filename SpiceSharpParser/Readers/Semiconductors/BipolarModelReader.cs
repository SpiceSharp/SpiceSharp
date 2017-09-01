using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read bipolar transistor models
    /// </summary>
    public class BipolarModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isNpn">True if it is parsing npn transistor, false for pnp transistors</param>
        public BipolarModelReader()
            : base("npn;pnp")
        {
        }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override ICircuitObject GenerateModel(string name, string type)
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
