using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read current-controlled voltage sources
    /// </summary>
    public class CurrentControlledVoltagesourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentControlledVoltagesourceReader() : base('h') { }

        /// <summary>
        /// Generate a CCVS
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            CurrentControlledVoltagesource ccvs = new CurrentControlledVoltagesource(name);
            ccvs.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected", false);
                case 3: throw new ParseException(parameters[2], "Value expected", false);
            }

            ccvs.Set("control", parameters[2].ReadWord());
            ccvs.Set("gain", parameters[2].ReadValue());
            return ccvs;
        }
    }
}
