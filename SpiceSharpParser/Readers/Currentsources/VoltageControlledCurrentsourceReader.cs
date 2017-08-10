using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage-controlled current sources
    /// </summary>
    public class VoltageControlledCurrentsourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VoltageControlledCurrentsourceReader() : base('g') { }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            VoltageControlledCurrentsource vccs = new VoltageControlledCurrentsource(name);
            vccs.ReadNodes(parameters, 4);

            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Value expected", false);
            vccs.Set("gain", parameters[4].ReadValue());
            return vccs;
        }
    }
}
