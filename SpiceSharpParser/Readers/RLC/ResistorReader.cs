using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a resistor
    /// </summary>
    public class ResistorReader : Reader
    {
        /// <summary>
        /// Read a resistor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="parameters">The resistor parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            // Test if we can read a resistor here
            if (name.image[0] != 'r' && name.image[0] != 'R')
                return false;

            Resistor res = new Resistor(name.ReadWord());
            res.ReadNodes(netlist, parameters, 2);

            // We have two possible formats:
            // Normal: RXXXXXXX N1 N2 VALUE
            if (parameters.Count == 3)
                res.Set("resistance", parameters[2].ReadValue());
            else
            {
                // Read the model
                res.Model = parameters[2].ReadModel<ResistorModel>(netlist);
                res.ReadParameters(parameters, 3);
                if (!res.RESlength.Given)
                    throw new ParseException(name, "L needs to be specified");
            }

            // Add the component
            netlist.Circuit.Components.Add(res);
            Generated = res;
            return true;
        }
    }
}
