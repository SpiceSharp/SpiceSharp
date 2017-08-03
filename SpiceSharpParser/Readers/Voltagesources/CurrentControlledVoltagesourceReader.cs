using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read current-controlled voltage sources
    /// </summary>
    public class CurrentControlledVoltagesourceReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'h' && name.image[0] != 'H')
                return false;

            CurrentControlledVoltagesource ccvs = new CurrentControlledVoltagesource(name.image);
            ReadNodes(ccvs, parameters, 2);

            // Read a voltage source name
            if (parameters.Count < 3)
                throw new ParseException($"Error on line {GetEndLine(parameters[1])}, column {GetEndColumn(parameters[1])}: Voltage source expected");
            ccvs.Set("control", ReadWord(parameters[2]));

            // Read the gain
            if (parameters.Count < 3)
                throw new ParseException($"Error on line {GetEndLine(parameters[2])}, column {GetEndColumn(parameters[2])}: Value expected");
            ccvs.Set("gain", ReadValue(parameters[2]));

            netlist.Circuit.Components.Add(ccvs);
            return true;
        }
    }
}
