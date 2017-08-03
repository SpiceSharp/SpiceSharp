using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read current-controlled current sources
    /// </summary>
    public class CurrentControlledCurrentsourceReader : Reader
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
            if (name.image[0] != 'f' && name.image[0] != 'F')
                return false;

            CurrentControlledCurrentsource cccs = new CurrentControlledCurrentsource(name.image);
            ReadNodes(cccs, parameters, 2);
            switch (parameters.Count)
            {
                case 2: ThrowAfter(parameters[1], "Voltage source expected"); break;
                case 3: ThrowAfter(parameters[2], "Value expected"); break;
            }

            cccs.Set("control", ReadWord(parameters[2]));
            cccs.Set("gain", ReadValue(parameters[3]));

            netlist.Circuit.Components.Add(cccs);
            return true;
        }
    }
}
