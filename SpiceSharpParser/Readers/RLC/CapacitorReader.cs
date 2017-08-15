using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parameters;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read a capacitor
    /// </summary>
    public class CapacitorReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CapacitorReader() : base('c') { }

        /// <summary>
        /// Generate a capacitor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            Capacitor cap = new Capacitor(name);
            cap.ReadNodes(parameters, 2);

            // Search for a parameter IC, which is common for both types of capacitors
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i].kind == TokenConstants.ASSIGNMENT)
                {
                    AssignmentToken at = parameters[i] as AssignmentToken;
                    if (at.Name.image.ToLower() == "ic")
                    {
                        double ic = netlist.ParseDouble(at.Value);
                        cap.CAPinitCond.Set(ic);
                        parameters.RemoveAt(i);
                        break;
                    }
                }
            }

            // The rest is just dependent on the number of parameters
            if (parameters.Count == 3)
                cap.CAPcapac.Set(netlist.ParseDouble(parameters[2]));
            else
            {
                cap.SetModel(netlist.FindModel<CapacitorModel>(parameters[2]));
                switch (parameters[2].kind)
                {
                    case SpiceSharpParserConstants.WORD:
                    case SpiceSharpParserConstants.IDENTIFIER:
                        cap.SetModel(netlist.Path.FindModel<CapacitorModel>(parameters[2].image.ToLower()));
                        break;
                    default:
                        throw new ParseException(parameters[2], "Model name expected");
                }
                netlist.ReadParameters(cap, parameters, 2);
                if (!cap.CAPlength.Given)
                    throw new ParseException(parameters[1], "L needs to be specified", false);
            }

            return (ICircuitObject)cap;
        }
    }
}
