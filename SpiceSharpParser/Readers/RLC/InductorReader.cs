using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read inductors
    /// </summary>
    public class InductorReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public InductorReader() : base('l') { }

        /// <summary>
        /// Generate an inductor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            Inductor ind = new Inductor(name);
            ind.ReadNodes(parameters, 2);

            // Read the value
            if (parameters.Count < 3)
                throw new ParseException(parameters[1], "Inductance expected", false);
            ind.INDinduct.Set(netlist.ParseDouble(parameters[2]));

            // Read initial conditions
            netlist.ReadParameters(ind, parameters, 3);
            return (ICircuitObject)ind;
        }
    }
}
