using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switches
    /// </summary>
    public class VoltageSwitchReader : Reader
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
            if (name.image[0] != 's' && name.image[0] != 'S')
                return false;

            VoltageSwitch vsw = new VoltageSwitch(name.image);
            ReadNodes(vsw, parameters, 4);

            // Read the model
            if (parameters.Count < 5)
                ThrowAfter(parameters[3], "Model expected");
            vsw.Model = ReadModel<VoltageSwitchModel>(parameters[4], netlist);

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                string state = ReadWord(parameters[5]).ToLower();
                switch (state)
                {
                    case "on": vsw.SetOn(); break;
                    case "off": vsw.SetOff(); break;
                    default: ThrowBefore(parameters[5], "On or off expected"); break;
                }
            }

            netlist.Circuit.Components.Add(vsw);
            return true;
        }
    }
}
