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

            Currentsource isrc = new Currentsource(name.image);
            ReadNodes(isrc, parameters, 2);

            // We can have a value or just DC
            string pvalue;
            for (int i = 2; i < parameters.Count; i++)
            {
                if (i == 2)
                {
                    // DC specification
                    if (TryReadLiteral(parameters[i], "dc"))
                    {
                        i++;
                        isrc.Set("dc", ReadValue(parameters[i]));
                    }
                    else if (TryReadValue(parameters[i], out pvalue))
                        isrc.Set("dc", pvalue);
                }

                // AC specification
                if (TryReadLiteral(parameters[i], "ac"))
                {
                    i++;
                    isrc.Set("acmag", ReadValue(parameters[i]));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && TryReadValue(parameters[i + 1], out pvalue))
                    {
                        i++;
                        isrc.Set("acphase", pvalue);
                    }
                }

                // Waveforms
                if (parameters[i] is SpiceSharpParser.Bracketed)
                {
                    // Find the reader
                    var b = parameters[i] as SpiceSharpParser.Bracketed;
                    bool found = false;
                    foreach (WaveformReader r in netlist.WaveformReaders)
                    {
                        if (r.Read(b.Name, b.Parameters, netlist))
                        {
                            isrc.Set("waveform", r.Current);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        ThrowBefore(b.Name, $"Unrecognized waveform \"{b.Name.image}\"");
                }
            }

            netlist.Circuit.Components.Add(isrc);
            return true;
        }
    }
}
