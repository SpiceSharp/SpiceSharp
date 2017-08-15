using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read current switches
    /// </summary>
    public class CurrentSwitchReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentSwitchReader() : base('w') { }

        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            CurrentSwitch csw = new CurrentSwitch(name);
            csw.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected", false);
                case 3: throw new ParseException(parameters[2], "Model expected", false);
            }

            switch (parameters[2].kind)
            {
                case SpiceSharpParserConstants.WORD: csw.CSWcontName = parameters[2].image.ToLower(); break;
                default: throw new ParseException(parameters[2], "Voltage source name expected");
            }
            csw.SetModel(netlist.FindModel<CurrentSwitchModel>(parameters[3]));
            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters[4].image.ToLower())
                {
                    case "on": csw.SetOn(); break;
                    case "off": csw.SetOff(); break;
                    default: throw new ParseException(parameters[4], "ON or OFF expected");
                }
            }
            return (ICircuitObject)csw;
        }
    }
}
