using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read voltage controlled voltage sources
    /// </summary>
    public class VoltageControlledVoltagesourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VoltageControlledVoltagesourceReader() : base('e') { }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            VoltageControlledVoltagesource vcvs = new VoltageControlledVoltagesource(name);
            vcvs.ReadNodes(parameters, 4);

            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Value expected");
            vcvs.Set("gain", parameters[4].ReadValue());
            return vcvs;
        }
    }
}
