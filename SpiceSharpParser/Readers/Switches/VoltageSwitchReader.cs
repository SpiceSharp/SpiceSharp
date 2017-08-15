using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switches
    /// </summary>
    public class VoltageSwitchReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VoltageSwitchReader() : base('s') { }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            VoltageSwitch vsw = new VoltageSwitch(name);
            vsw.ReadNodes(parameters, 4);

            // Read the model
            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Model expected", false);
            vsw.SetModel(netlist.FindModel<VoltageSwitchModel>(parameters[4]));

            // Optional ON or OFF
            if (parameters.Count == 6)
            {
                switch (parameters[5].image.ToLower())
                {
                    case "on": vsw.SetOn(); break;
                    case "off": vsw.SetOff(); break;
                    default: throw new ParseException(parameters[5], "ON or OFF expected");
                }
            }
            return (ICircuitObject)vsw;
        }
    }
}
