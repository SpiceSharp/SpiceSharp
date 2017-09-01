using System;
using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage switches
    /// </summary>
    public class SwitchReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SwitchReader() : base("s;w") { }

        /// <summary>
        /// Generate a switch
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string type, string name, List<Token> parameters, Netlist netlist)
        {
            switch (type)
            {
                case "s": return GenerateVSW(name, parameters, netlist);
                case "w": return GenerateCSW(name, parameters, netlist);
            }
            return null;
        }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected ICircuitObject GenerateVSW(string name, List<Token> parameters, Netlist netlist)
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
                    case "on":
                        vsw.SetOn();
                        break;
                    case "off":
                        vsw.SetOff();
                        break;
                    default:
                        throw new ParseException(parameters[5], "ON or OFF expected");
                }
            }
            return vsw;
        }

        /// <summary>
        /// Generate a current switch
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected ICircuitObject GenerateCSW(string name, List<Token> parameters, Netlist netlist)
        {
            CurrentSwitch csw = new CurrentSwitch(name);
            csw.ReadNodes(parameters, 2);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected", false);
                case 3: throw new ParseException(parameters[2], "Model expected", false);
            }

            // Get the controlling voltage source
            switch (parameters[2].kind)
            {
                case WORD:
                    csw.CSWcontName = parameters[2].image.ToLower();
                    break;
                default:
                    throw new ParseException(parameters[2], "Voltage source name expected");
            }

            // Get the model
            csw.SetModel(netlist.FindModel<CurrentSwitchModel>(parameters[3]));

            // Optional on or off
            if (parameters.Count > 4)
            {
                switch (parameters[4].image.ToLower())
                {
                    case "on":
                        csw.SetOn();
                        break;
                    case "off":
                        csw.SetOff();
                        break;
                    default:
                        throw new ParseException(parameters[4], "ON or OFF expected");
                }
            }
            return csw;
        }
    }
}
