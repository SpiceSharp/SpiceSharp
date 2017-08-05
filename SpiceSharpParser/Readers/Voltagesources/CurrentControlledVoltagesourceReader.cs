using System.Collections.Generic;
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

            CurrentControlledVoltagesource ccvs = new CurrentControlledVoltagesource(name.ReadWord());
            ccvs.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected", false);
                case 3: throw new ParseException(parameters[2], "Value expected", false);
            }

            ccvs.Set("control", parameters[2].ReadWord());
            ccvs.Set("gain", parameters[2].ReadValue());

            netlist.Circuit.Components.Add(ccvs);
            return true;
        }
    }
}
