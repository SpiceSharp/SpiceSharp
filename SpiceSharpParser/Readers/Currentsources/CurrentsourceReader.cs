using System.Collections.Generic;
using SpiceSharp.Components;
using SpiceSharp.Parser.Readers.Extensions;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read current sources
    /// </summary>
    public class CurrentsourceReader : ComponentReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public CurrentsourceReader() : base('i') { }

        /// <summary>
        /// Generate a current source
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        protected override CircuitComponent Generate(string name, List<object> parameters, Netlist netlist)
        {
            Currentsource isrc = new Currentsource(name);
            isrc.ReadNodes(parameters, 2);

            // We can have a value or just DC
            for (int i = 2; i < parameters.Count; i++)
            {
                // DC specification
                if (i == 2 && parameters[i].TryReadLiteral("dc"))
                {
                    i++;
                    isrc.Set("dc", parameters[i].ReadValue());
                }
                else if (i == 2 && parameters[i].TryReadValue(out string pvalue))
                    isrc.Set("dc", pvalue);

                // AC specification
                else if (parameters[i].TryReadLiteral("ac"))
                {
                    i++;
                    isrc.Set("acmag", parameters[i].ReadValue());

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && parameters[i + 1].TryReadValue(out pvalue))
                    {
                        i++;
                        isrc.Set("acphase", pvalue);
                    }
                }

                // Waveforms
                else if (parameters[i].TryReadBracket(out BracketToken b))
                {
                    // Find the reader
                    if (!(b.Name is Token))
                        throw new ParseException(b.Name, "Waveform expected");
                    Statement st = new Statement(StatementType.Waveform, b.Name as Token, b.Parameters);
                    object w = netlist.Readers.Read(st, netlist);
                    isrc.Set("waveform", w);
                }
                else
                    throw new ParseException(parameters[i], "Unrecognized parameter");
            }

            return isrc;
        }
    }
}
