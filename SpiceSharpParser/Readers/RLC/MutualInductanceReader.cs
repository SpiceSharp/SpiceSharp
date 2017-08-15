using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class MutualInductanceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MutualInductanceReader() : base('k') { }

        /// <summary>
        /// Generate a mutual inductance
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            MutualInductance mut = new MutualInductance(name);
            switch (parameters.Count)
            {
                case 0: throw new ParseException($"Inductor name expected for mutual inductance \"{name}\"");
                case 1: throw new ParseException(parameters[0], "Inductor name expected", false);
                case 2: throw new ParseException(parameters[1], "Coupling factor expected", false);
            }

            // Read two inductors
            mut.MUTind1 = parameters[0].image.ToLower();
            mut.MUTind2 = parameters[1].image.ToLower();
            mut.MUTcoupling.Set(netlist.ParseDouble(parameters[2]));
            return (ICircuitObject)mut;
        }
    }
}
