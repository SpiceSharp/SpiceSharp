using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// This class can read models for current switches
    /// </summary>
    public class CurrentSwitchModelReader : Reader
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
            CurrentSwitchModel model = new CurrentSwitchModel(ReadIdentifier(name));
            ReadParameters(model, parameters);
            netlist.Circuit.Components.Add(model);
            return true;
        }
    }
}
