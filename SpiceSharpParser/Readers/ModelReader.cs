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
        protected abstract Entity GenerateModel(Identifier name, string type);

        /// <summary>
        /// Read
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="parameters">Parameters</param>
        /// <param name="netlist">Netlist</param>
        /// <returns></returns>
        public override bool Read(string type, Statement st, Netlist netlist)
        {
            var model = GenerateModel(new Identifier(st.Name.image), type);
            netlist.ReadParameters(model, st.Parameters);
            
            // Output
            netlist.Circuit.Objects.Add(model);
            Generated = model;
            return true;
        }
    }
}
