using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class that can read a resistor model
    /// </summary>
    public class ResistorModelReader : Reader
    {
        /// <summary>
        /// Read a resistor model
        /// </summary>
        /// <param name="name">The name of the control model</param>
        /// <param name="parameters">The parameters</param>
        /// <param name="netlist">The netlist</param>
        /// <returns></returns>
        public override bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            // Create the model
            ResistorModel model = new ResistorModel(name.ReadIdentifier());
            model.ReadParameters(parameters);
            netlist.Circuit.Components.Add(model);
            Generated = model;
            return true;
        }
    }
}
