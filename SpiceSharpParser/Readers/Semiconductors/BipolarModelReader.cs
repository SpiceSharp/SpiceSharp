using System;
using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read bipolar transistor models
    /// </summary>
    public class BipolarModelReader : IReader
    {
        /// <summary>
        /// The last generated object
        /// </summary>
        public object Generated { get; private set; }

        /// <summary>
        /// Private variables
        /// </summary>
        private bool npn = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="isNpn">True if it is parsing npn transistor, false for pnp transistors</param>
        public BipolarModelReader(bool isNpn)
        {
            npn = isNpn;
        }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            BJTModel model = new BJTModel(name.ReadIdentifier());
            if (npn)
                model.SetNPN(true);
            else
                model.SetPNP(true);
            model.ReadParameters(parameters);
            netlist.Circuit.Components.Add(model);
            Generated = model;
            return true;
        }
    }
}
