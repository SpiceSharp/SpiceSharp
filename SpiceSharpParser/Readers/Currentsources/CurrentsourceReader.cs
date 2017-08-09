using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read current sources
    /// </summary>
    public class CurrentsourceReader : Reader
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
            if (name.image[0] != 'i' && name.image[0] != 'I')
                return false;

            Currentsource isrc = new Currentsource(name.ReadWord());
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
                    object w = netlist.Readers.Read("waveform", b.Name as Token, b.Parameters, netlist);
                    isrc.Set("waveform", w);
                }
                else
                    throw new ParseException(parameters[i], "Unrecognized parameter");
            }

            netlist.Circuit.Components.Add(isrc);
            Generated = isrc;
            return true;
        }
    }
}
