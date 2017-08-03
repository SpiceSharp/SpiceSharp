using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read current switches
    /// </summary>
    public class CurrentSwitchReader : Reader
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
            if (name.image[0] != 'w' && name.image[0] != 'W')
                return false;

            CurrentSwitch csw = new CurrentSwitch(name.image);
            ReadNodes(csw, parameters, 2);
            switch (parameters.Count)
            {
                case 2: ThrowAfter(parameters[1], "Voltage source expected"); break;
                case 3: ThrowAfter(parameters[2], "Model expected"); break;
            }

            csw.Set("control", ReadWord(parameters[2]));
            csw.Model = ReadModel<CurrentSwitchModel>(parameters[3], netlist);

            // Optional on or off
            if (parameters.Count > 4)
            {
                string state = ReadWord(parameters[4]).ToLower();
                switch (state)
                {
                    case "on": csw.SetOn(); break;
                    case "off": csw.SetOff(); break;
                    default: ThrowBefore(parameters[4], "On or Off expected"); break;
                }
            }

            netlist.Circuit.Components.Add(csw);
            return true;
        }
    }
}
