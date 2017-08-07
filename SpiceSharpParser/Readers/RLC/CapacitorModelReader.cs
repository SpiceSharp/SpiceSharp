using System.Collections.Generic;
using SpiceSharp.Components;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// A class capable of reading capacitor models
    /// </summary>
    public class CapacitorModelReader : IReader
    {
        /// <summary>
        /// The last generated object
        /// </summary>
        public object Generated { get; private set; }

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameters"></param>
        /// <param name="netlist"></param>
        /// <returns></returns>
        public bool Read(Token name, List<object> parameters, Netlist netlist)
        {
            CapacitorModel model = new CapacitorModel(name.ReadIdentifier());
            model.ReadParameters(parameters);
            netlist.Circuit.Components.Add(model);
            Generated = model;
            return true;
        }
    }
}
