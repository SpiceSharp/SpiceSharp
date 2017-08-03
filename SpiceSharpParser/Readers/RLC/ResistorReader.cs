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

            Resistor r = new Resistor(name.image);
            ReadNodes(r, parameters, 2);

            // We have two possible formats:
            // Normal: RXXXXXXX N1 N2 VALUE
            if (parameters.Count == 3)
                r.Set("resistance", ReadValue(parameters[2]));
            else
            {
                // Read the model
                r.Model = ReadModel<ResistorModel>(parameters[2], netlist);
                ReadParameters(r, parameters, 3);
                if (!r.RESlength.Given)
                    ThrowBefore(name, "L needs to be specified");
            }

            // Add the component
            netlist.Circuit.Components.Add(r);
            return true;
        }
    }
}
