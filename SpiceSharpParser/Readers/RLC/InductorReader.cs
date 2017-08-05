using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    public class InductorReader : IReader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'l' && name.image[0] != 'L')
                return false;

            Inductor ind = new Inductor(name.ReadWord());
            ind.ReadNodes(parameters, 2);

            // Read the value
            if (parameters.Count < 3)
                throw new ParseException(parameters[1], "Inductance expected", false);
            ind.Set("inductance", parameters[2].ReadValue());

            // Read initial conditions
            ind.ReadParameters(parameters, 3);

            // Success
            netlist.Circuit.Components.Add(ind);
            return true;
        }
    }
}
