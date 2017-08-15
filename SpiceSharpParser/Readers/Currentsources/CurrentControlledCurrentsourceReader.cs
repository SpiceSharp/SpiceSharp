using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

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
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            CurrentControlledCurrentsource cccs = new CurrentControlledCurrentsource(name);
            cccs.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected");
                case 3: throw new ParseException(parameters[2], "Value expected");
            }

            cccs.CCCScontName = parameters[2].image.ToLower();
            cccs.CCCScoeff.Set(netlist.ParseDouble(parameters[3]));
            return (ICircuitObject)cccs;
        }
    }
}
