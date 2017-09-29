using SpiceSharp.Components;
using SpiceSharp.Circuits;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Reads <see cref="MESModel"/> definitions.
    /// </summary>
    public class MESModelReader : ModelReader
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MESModelReader()
            : base("nmf;pmf")
        {
        }

        /// <summary>
        /// Generate a new model
        /// </summary>
        /// <param name="name">Model name</param>
        /// <param name="type">Model type</param>
        /// <returns></returns>
        protected override ICircuitObject GenerateModel(string name, string type)
        {
            MESModel model = new MESModel(name);
            if (type == "nmf")
                model.SetNMF(true);
            else if (type == "pmf")
                model.SetPMF(true);
            return model;
        }
    }
}
