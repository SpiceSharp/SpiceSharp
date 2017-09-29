using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="JFETModel" /> definitions.
    /// </summary>
    public class JFETModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public JFETModelReader()
            : base("njf;pjf")
        {
        }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected override ICircuitObject GenerateModel(string name, string type)
        {
            JFETModel model = new JFETModel(name);
            if (type == "njf")
                model.SetNJF(true);
            else if (type == "pjf")
                model.SetPJF(true);
            return model;
        }
    }
}
