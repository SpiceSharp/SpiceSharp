using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
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

            Inductor ind = new Inductor(name.image);
            ReadNodes(ind, parameters, 2);

            // Read the value
            if (parameters.Count < 3)
                throw new ParseException($"Error on line {GetBeginLine(name)}: Inductance expected");
            ind.Set("inductance", ReadValue(parameters[2]));

            // Read initial conditions
            ReadParameters(ind, parameters, 3);

            // Success
            netlist.Circuit.Components.Add(ind);
            return true;
        }
    }
}
