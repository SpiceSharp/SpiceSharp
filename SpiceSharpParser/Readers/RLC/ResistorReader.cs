using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a resistor
    /// </summary>
    public class ResistorReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ResistorReader() : base('r') { }

        /// <summary>
        /// Generate a resistor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            Resistor res = new Resistor(name);
            res.ReadNodes(parameters, 2);

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
            return res;
        }
    }
}
