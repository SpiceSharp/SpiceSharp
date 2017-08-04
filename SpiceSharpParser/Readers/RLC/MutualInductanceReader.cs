using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Constructor
    /// </summary>
    public class MutualInductanceReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'k' && name.image[0] != 'K')
                return false;

            MutualInductance mut = new MutualInductance(name.image);
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

            netlist.Circuit.Components.Add(mut);
            return true;
        }
    }
}
