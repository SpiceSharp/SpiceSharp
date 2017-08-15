using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read bipolar transistors
    /// </summary>
    public class BipolarReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public BipolarReader() : base('q') { }

        /// <summary>
        /// Generate the bipolar transistor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            // I think the BJT definition is ambiguous (eg. QXXXX NC NB NE MNAME OFF can be either substrate = MNAME, model = OFF or model name = MNAME and transistor is OFF
            // We will only allow 3 terminals if there are only 4 parameters
            BJT bjt = new BJT(name);

            // Read the nodes
            if (parameters.Count <= 4)
                bjt.ReadNodes(parameters, 3);
            else
                bjt.ReadNodes(parameters, 4);

            if (parameters.Count == 3)
                throw new ParseException(parameters[2], "Model expected", false);
            if (parameters.Count == 4)
                bjt.SetModel(netlist.FindModel<BJTModel>(parameters[3]));
            else
                bjt.SetModel(netlist.FindModel<BJTModel>(parameters[4]));

            // Area
            if (parameters.Count > 5)
                bjt.BJTarea.Set(netlist.ParseDouble(parameters[5]));

            // ON/OFF
            if (parameters.Count > 6)
            {
                switch (parameters[6].image.ToLower())
                {
                    case "on": bjt.BJToff = false; break;
                    case "off": bjt.BJToff = true; break;
                    default: throw new ParseException(parameters[6], "ON or OFF expected");
                }
            }

            netlist.ReadParameters(bjt, parameters, 7);
            return (ICircuitObject)bjt;
        }
    }
}
