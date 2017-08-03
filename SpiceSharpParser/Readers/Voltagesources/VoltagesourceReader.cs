using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            Voltagesource vsrc = new Voltagesource(name.image);
            ReadNodes(vsrc, parameters, 2);

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
                        vsrc.Set("dc", ReadValue(parameters[i]));
                    }
                    else if (TryReadValue(parameters[i], out pvalue))
                        vsrc.Set("dc", pvalue);
                }

                // AC specification
                if (TryReadLiteral(parameters[i], "ac"))
                {
                    i++;
                    vsrc.Set("acmag", ReadValue(parameters[i]));

                    // Look forward for one more value
                    if (i + 1 < parameters.Count && TryReadValue(parameters[i + 1], out pvalue))
                    {
                        i++;
                        vsrc.Set("acphase", pvalue);
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
                            vsrc.Set("waveform", r.Current);
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                        throw new ParseException($"Error on line {b.Name.beginLine}, column {b.Name.beginColumn}: Unrecognized waveform \"{b.Name.image}\"");
                }
            }

            netlist.Circuit.Components.Add(vsrc);
            return true;
        }
    }
}
