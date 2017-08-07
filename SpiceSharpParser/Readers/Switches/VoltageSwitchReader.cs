using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switches
    /// </summary>
    public class VoltageSwitchReader : IReader
    {
        /// <summary>
        /// The last generated object
        /// </summary>
        public object Generated { get; private set; }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 's' && name.image[0] != 'S')
                return false;

            VoltageSwitch vsw = new VoltageSwitch(name.ReadWord());
            vsw.ReadNodes(parameters, 4);

            // Read the model
            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Model expected", false);
            vsw.Model = parameters[4].ReadModel<VoltageSwitchModel>(netlist);

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                string state = parameters[5].ReadWord();
                switch (state)
                {
                    case "on": vsw.SetOn(); break;
                    case "off": vsw.SetOff(); break;
                    default: throw new ParseException(parameters[5], "ON or OFF expected");
                }
            }

            netlist.Circuit.Components.Add(vsw);
            Generated = vsw;
            return true;
        }
    }
}
