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
            vsrc.ReadNodes(parameters, 2);

            // We can have a value or just DC
            string pvalue;
            for (int i = 2; i < parameters.Count; i++)
            {
                if (i == 2)
                {
                    // DC specification
                    if (parameters[i].TryReadLiteral("dc"))
                    {
                        i++;
                        vsrc.Set("dc", parameters[i].ReadValue());
                    }
                    else if (parameters[i].TryReadValue(out pvalue))
                        vsrc.Set("dc", pvalue);
                }

                // AC specification
                if (parameters[i].TryReadLiteral("ac"))
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
                if (parameters[i] is BracketToken)
                {
                    // Find the reader
                    var b = parameters[i] as BracketToken;
                    bool found = false;
                    if (!(b.Name is Token))
                        throw new ParseException(b.Name, "Waveform expected");
                    foreach (WaveformReader r in netlist.WaveformReaders)
                    {
                        if (r.Read(b.Name as Token, b.Parameters, netlist))
                        {
                            vsrc.Set("waveform", r.Current);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        throw new ParseException(b.Name, $"Unrecognized waveform \"{b.Name.Image()}\"");
                }
            }

            netlist.Circuit.Components.Add(vsrc);
            return true;
        }
    }
}
