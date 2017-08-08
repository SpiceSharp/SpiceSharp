using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read inductors
    /// </summary>
    public class InductorReader : Reader
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
            if (name.image[0] != 'l' && name.image[0] != 'L')
                return false;

            Inductor ind = new Inductor(name.ReadWord());
            ind.ReadNodes(netlist, parameters, 2);

            // Read the value
            if (parameters.Count < 3)
                throw new ParseException(parameters[1], "Inductance expected", false);
            ind.Set("inductance", parameters[2].ReadValue());

            // Read initial conditions
            ind.ReadParameters(parameters, 3);

            // Success
            netlist.Circuit.Components.Add(ind);
            Generated = ind;
            return true;
        }
    }
}
