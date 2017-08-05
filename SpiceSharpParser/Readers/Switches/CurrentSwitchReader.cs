using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read current switches
    /// </summary>
    public class CurrentSwitchReader : IReader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'w' && name.image[0] != 'W')
                return false;

            CurrentSwitch csw = new CurrentSwitch(name.ReadWord());
            csw.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected", false);
                case 3: throw new ParseException(parameters[2], "Model expected", false);
            }

            csw.Set("control", parameters[2].ReadWord());
            csw.Model = parameters[3].ReadModel<CurrentSwitchModel>(netlist);

            // Optional on or off
            if (parameters.Count > 4)
            {
                string state = parameters[4].ReadWord();
                switch (state)
                {
                    case "on": csw.SetOn(); break;
                    case "off": csw.SetOff(); break;
                    default: throw new ParseException(parameters[4], "ON or OFF expected");
                }
            }

            netlist.Circuit.Components.Add(csw);
            return true;
        }
    }
}
