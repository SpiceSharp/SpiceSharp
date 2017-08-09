using System;
using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read current-controlled current sources
    /// </summary>
    public class CurrentControlledCurrentsourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentControlledCurrentsourceReader() : base('f') { }

        /// <summary>
        /// Generate a CCCS
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            CurrentControlledCurrentsource cccs = new CurrentControlledCurrentsource(name);
            cccs.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected");
                case 3: throw new ParseException(parameters[2], "Value expected");
            }

            cccs.Set("control", parameters[2].ReadWord());
            cccs.Set("gain", parameters[3].ReadValue());
            return cccs;
        }
    }
}
