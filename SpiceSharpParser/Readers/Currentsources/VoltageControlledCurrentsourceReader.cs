using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read voltage-controlled current sources
    /// </summary>
    public class VoltageControlledCurrentsourceReader : Reader
    {
        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameter</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'g' && name.image[0] != 'G')
                return false;

            VoltageControlledCurrentsource vccs = new VoltageControlledCurrentsource(name.image);
            ReadNodes(vccs, parameters, 4);

            if (parameters.Count < 5)
                ThrowAfter(parameters[3], "Value expected");
            vccs.Set("gain", ReadValue(parameters[4]));

            netlist.Circuit.Components.Add(vccs);
            return true;
        }
    }
}
