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
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            MutualInductance mut = new MutualInductance(name);
            switch (parameters.Count)
            {
                case 1: throw new ParseException(name, "Inductor name expected", false);
                case 2: throw new ParseException(parameters[0], "Inductor name expected", false);
                case 3: throw new ParseException(parameters[1], "Coupling factor expected", false);
            }

            // Read two inductors
            mut.Set("inductor1", parameters[0].ReadWord());
            mut.Set("inductor2", parameters[1].ReadWord());
            mut.Set("k", parameters[2].ReadValue());
            return mut;
        }
    }
}
