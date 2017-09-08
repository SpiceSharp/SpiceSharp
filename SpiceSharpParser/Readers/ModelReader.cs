using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Describes methods for reading a model definition.
    /// </summary>
    public abstract class ModelReader : Reader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="type">The type, multiple types are separated by a semicolon (;)</param>
        public ModelReader(string id) : base(StatementType.Model)
        {
            Identifier = id;
        }

        /// <summary>
        /// Generate a model of the right type
        /// </summary>
        /// <returns></returns>
        protected abstract ICircuitObject GenerateModel(string name, string type);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            var model = GenerateModel(st.Name.image.ToLower(), type);
            netlist.ReadParameters((IParameterized)model, st.Parameters);
            
            // Output
            netlist.Path.Objects.Add(model);
            Generated = model;
            return true;
        }
    }
}
