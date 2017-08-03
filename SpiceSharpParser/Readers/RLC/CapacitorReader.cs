using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read a capacitor
    /// </summary>
    public class CapacitorReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'c' && name.image[0] != 'C')
                return false;

            Capacitor cap = new Capacitor(name.image);
            ReadNodes(cap, parameters, 2);

            // Search for a parameter IC, which is common for both types of capacitors
            for (int i = 3; i < parameters.Count; i++)
            {
                string nn, nv;
                if (TryReadNamed(parameters[i], out nn, out nv))
                {
                    if (nn.ToLower() == "ic")
                    {
                        cap.Set("ic", nv);
                        parameters.RemoveAt(i);
                        break;
                    }
                }
            }

            // The rest is just dependent on the number of parameters
            if (parameters.Count == 3)
                cap.Set("capacitance", ReadValue(parameters[2]));
            else
            {
                cap.Model = ReadModel<CapacitorModel>(parameters[2], netlist);

                // Only parameters left
                bool hasL = false;
                for (int i = 3; i < parameters.Count; i++)
                {
                    string pname, pvalue;
                    ReadNamed(parameters[i], out pname, out pvalue);
                    cap.Set(pname.ToLower(), pvalue);
                    hasL |= pname.ToLower() == "l";
                }
                if (!hasL)
                    throw new ParseException($"Error at line {GetBeginLine(name)}: L needs to be specified for capacitor {GetImage(name)}");
            }

            netlist.Circuit.Components.Add(cap);
            return true;
        }
    }
}
