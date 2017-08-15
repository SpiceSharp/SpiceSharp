using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read a voltage source
    /// </summary>
    public class VoltagesourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public VoltagesourceReader() : base('v') { }

        /// <summary>
        /// Generate
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override ICircuitObject Generate(string name, List<Token> parameters, Netlist netlist)
        {
            Voltagesource vsrc = new Voltagesource(name);
            vsrc.ReadNodes(parameters, 2);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i].image.ToLower() == "dc")
                {
                    i++;
                    vsrc.VSRCdcValue.Set(netlist.ParseDouble(parameters[i]));
                }
                else if (i == 2 && (parameters[i].kind == SpiceSharpParserConstants.VALUE || parameters[i].kind == SpiceSharpParserConstants.EXPRESSION))
                    vsrc.VSRCdcValue.Set(netlist.ParseDouble(parameters[i]));

                // AC specification
                else if (parameters[i].image.ToLower() == "ac")
                {
                    i++;
                    vsrc.VSRCacMag.Set(netlist.ParseDouble(parameters[i]));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && (parameters[i + 1].kind == SpiceSharpParserConstants.VALUE || parameters[i + 1].kind == SpiceSharpParserConstants.EXPRESSION))
                    {
                        i++;
                        vsrc.VSRCacPhase.Set(netlist.ParseDouble(parameters[i]));
                    }
                }

                // Waveforms
                else if (parameters[i].kind == TokenConstants.BRACKET)
                {
                    // Find the reader
                    var bt = parameters[i] as BracketToken;
                    Statement st = new Statement(StatementType.Waveform, bt.Name, bt.Parameters);
                    object w = netlist.Readers.Read(st, netlist);
                    vsrc.VSRCwaveform = (IWaveform)w;
                }
                else
                    throw new ParseException(parameters[i], "Unrecognized parameter");
            }
            return (ICircuitObject)vsrc;
        }
    }
}
