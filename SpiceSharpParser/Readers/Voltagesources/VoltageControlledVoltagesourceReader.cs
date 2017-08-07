using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read voltage controlled voltage sources
    /// </summary>
    public class VoltageControlledVoltagesourceReader : IReader
    {
        /// <summary>
        /// The last generated object
        /// </summary>
        public object Generated { get; private set; }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            if (name.image[0] != 'e' && name.image[0] != 'E')
                return false;

            VoltageControlledVoltagesource vcvs = new VoltageControlledVoltagesource(name.ReadWord());
            vcvs.ReadNodes(parameters, 4);

            if (parameters.Count < 5)
                throw new ParseException(parameters[3], "Value expected");
            vcvs.Set("gain", parameters[4].ReadValue());

            netlist.Circuit.Components.Add(vcvs);
            Generated = vcvs;
            return true;
        }
    }
}
