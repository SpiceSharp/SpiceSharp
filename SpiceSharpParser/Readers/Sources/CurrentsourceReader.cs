using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="Currentsource"/>, <see cref="CurrentControlledCurrentsource"/> and <see cref="VoltageControlledCurrentsource"/> components.
    /// </summary>
    public class CurrentsourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentsourceReader() : base("i;g;f") { }

        /// <summary>
        /// Generate a current source
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override Entity Generate(string type, Identifier name, List<Token> parameters, Netlist netlist)
        {
            switch (type)
            {
                case "i": return GenerateISRC(name, parameters, netlist);
                case "g": return GenerateVCCS(name, parameters, netlist);
                case "f": return GenerateCCCS(name, parameters, netlist);
            }
            return null;
        }

        /// <summary>
        /// Generate a current source
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected Entity GenerateISRC(Identifier name, List<Token> parameters, Netlist netlist)
        {
            Currentsource isrc = new Currentsource(name);
            isrc.ReadNodes(netlist.Path, parameters);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i].image.ToLower() == "dc")
                {
                    i++;
                    isrc.ISRCdcValue.Set(netlist.ParseDouble(parameters[i]));
                }
                else if (i == 2 && ReaderExtension.IsValue(parameters[i]))
                    isrc.ISRCdcValue.Set(netlist.ParseDouble(parameters[i]));

                // AC specification
                else if (parameters[i].image.ToLower() == "ac")
                {
                    i++;
                    isrc.ISRCacMag.Set(netlist.ParseDouble(parameters[i]));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && ReaderExtension.IsValue(parameters[i + 1]))
                    {
                        i++;
                        isrc.ISRCacPhase.Set(netlist.ParseDouble(parameters[i]));
                    }
                }

                // Waveforms
                else if (parameters[i].kind == BRACKET)
                {
                    // Find the reader
                    BracketToken bt = parameters[i] as BracketToken;
                    Statement st = new Statement(StatementType.Waveform, bt.Name, bt.Parameters);
                    isrc.ISRCwaveform = (Waveform)netlist.Readers.Read(st, netlist);
                }
                else
                    throw new ParseException(parameters[i], "Unrecognized parameter");
            }

            return isrc;
        }

        /// <summary>
        /// Generate a CCCS
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected Entity GenerateCCCS(Identifier name, List<Token> parameters, Netlist netlist)
        {
            CurrentControlledCurrentsource cccs = new CurrentControlledCurrentsource(name);
            cccs.ReadNodes(netlist.Path, parameters);
            switch (parameters.Count)
            {
                case 2: throw new ParseException(parameters[1], "Voltage source expected", false);
                case 3: throw new ParseException(parameters[2], "Value expected", false);
            }

            if (!ReaderExtension.IsName(parameters[2]))
                throw new ParseException(parameters[2], "Component name expected");
            cccs.CCCScontName = new Identifier(parameters[2].image);
            cccs.CCCScoeff.Set(netlist.ParseDouble(parameters[3]));
            return cccs;
        }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected Entity GenerateVCCS(Identifier name, List<Token> parameters, Netlist netlist)
        {
            VoltageControlledCurrentsource vccs = new VoltageControlledCurrentsource(name);
            vccs.ReadNodes(netlist.Path, parameters);

            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Value expected", false);
            vccs.VCCScoeff.Set(netlist.ParseDouble(parameters[4]));
            return vccs;
        }
    }
}
