using System;
using System.Collections.Generic;
using SpiceSharp.Components;
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
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            Capacitor cap = new Capacitor(name);
            cap.ReadNodes(parameters, 2);

            // Search for a parameter IC, which is common for both types of capacitors
            for (int i = 3; i < parameters.Count; i++)
            {
                if (parameters[i].TryReadAssignment(out string nn, out string nv))
                {
                    if (nn == "ic")
                    {
                        cap.Set("ic", nv);
                        parameters.RemoveAt(i);
                        break;
                    }
                }
            }

            // The rest is just dependent on the number of parameters
            if (parameters.Count == 3)
                cap.Set("capacitance", parameters[2].ReadValue());
            else
            {
                cap.Model = parameters[2].ReadModel<CapacitorModel>(netlist);
                cap.ReadParameters(parameters, 2);
                if (!cap.CAPlength.Given)
                    throw new ParseException(name, "L needs to be specified");
            }

            return cap;
        }
    }
}
