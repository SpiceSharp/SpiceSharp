using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Components;

using static SpiceSharp.Parser.SpiceSharpParserConstants;

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
            ResistorModel model = new ResistorModel(ReadIdentifier(name));
            ReadParameters(model, parameters);
            netlist.Circuit.Components.Add(model);
            return true;
        }
    }
}
