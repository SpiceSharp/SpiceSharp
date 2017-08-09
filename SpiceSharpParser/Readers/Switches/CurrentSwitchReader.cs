using System.Collections.Generic;
using SpiceSharp.Components;

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

        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            CurrentSwitch csw = new CurrentSwitch(name);
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
            return csw;
        }
    }
}
