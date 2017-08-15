using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

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
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            Resistor res = new Resistor(name);
            res.ReadNodes(parameters, 2);

            // We have two possible formats:
            // Normal: RXXXXXXX N1 N2 VALUE
            if (parameters.Count == 3)
                res.RESresist.Set(netlist.ParseDouble(parameters[2]));
            else
            {
                // Read the model
                res.SetModel(netlist.FindModel<ResistorModel>(parameters[2]));
                netlist.ReadParameters(res, parameters, 3);
                if (!res.RESlength.Given)
                    throw new ParseException(parameters[parameters.Count - 1], "L needs to be specified", false);
            }
            return (ICircuitObject)res;
        }
    }
}
