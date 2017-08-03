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
                case 1: throw new ParseException($"Error on line {name.endLine}, column {name.endColumn}: Inductor name expected");
                case 2: throw new ParseException($"Error on line {GetEndLine(parameters[0])}, column {GetEndColumn(parameters[0])}: Inductor name expected");
                case 3: throw new ParseException($"Error on line {GetEndLine(parameters[0])}, column {GetEndColumn(parameters[0])}: Coupling factor expected");
            }

            // Read two inductors
            mut.Set("inductor1", ReadWord(parameters[0]));
            mut.Set("inductor2", ReadWord(parameters[1]));
            mut.Set("k", ReadValue(parameters[2]));

            netlist.Circuit.Components.Add(mut);
            return true;
        }
    }
}
