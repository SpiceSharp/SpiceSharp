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
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            Voltagesource vsrc = new Voltagesource(name);
            vsrc.ReadNodes(parameters, 2);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i].TryReadLiteral("dc"))
                {
                    i++;
                    vsrc.Set("dc", parameters[i].ReadValue());
                }
                else if (i == 2 && parameters[i].TryReadValue(out string pvalue))
                    vsrc.Set("dc", pvalue);

                // AC specification
                else if (parameters[i].TryReadLiteral("ac"))
                {
                    i++;
                    vsrc.Set("acmag", parameters[i].ReadValue());

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && parameters[i + 1].TryReadValue(out pvalue))
                    {
                        i++;
                        vsrc.Set("acphase", pvalue);
                    }
                }

                // Waveforms
                else if (parameters[i].TryReadBracket(out BracketToken bt))
                {
                    // Find the reader
                    var b = parameters[i] as BracketToken;
                    if (!(b.Name is Token))
                        throw new ParseException(b.Name, "Waveform expected");
                    Statement st = new Statement(StatementType.Waveform, b.Name as Token, b.Parameters);
                    object w = netlist.Readers.Read(st, netlist);
                    vsrc.Set("waveform", w);
                }
                else
                    throw new ParseException(parameters[i], "Unrecognized parameter");
            }
            return vsrc;
        }
    }
}
