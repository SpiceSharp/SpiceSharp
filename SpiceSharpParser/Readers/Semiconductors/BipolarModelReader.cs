using System;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read bipolar transistor models
    /// </summary>
    public class BipolarModelReader : ModelReader
    {
        /// <summary>
        /// Private variables
        /// </summary>
        private bool npn = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isNpn">True if it is parsing npn transistor, false for pnp transistors</param>
        public BipolarModelReader(bool isNpn)
            : base(isNpn ? "npn" : "pnp")
        {
            npn = isNpn;
        }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        protected override CircuitModel GenerateModel(string name)
        {
            BJTModel model = new BJTModel(name);
            if (npn)
                model.SetNPN(true);
            else
                model.SetPNP(true);
            return model;
        }
    }
}
