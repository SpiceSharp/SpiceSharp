using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read a voltage source
    /// </summary>
    public class VoltagesourceReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'v' && name.image[0] != 'V')
                return false;

            Voltagesource vsrc = new Voltagesource(name.ReadWord());
            vsrc.ReadNodes(netlist, parameters, 2);

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
                    object w = netlist.Readers.Read("waveform", b.Name as Token, b.Parameters, netlist);
                    vsrc.Set("waveform", w);
                }
                else
                    throw new ParseException(parameters[i], "Unrecognized parameter");
            }

            Generated = vsrc;
            netlist.Circuit.Components.Add(vsrc);
            return true;
        }
    }
}
