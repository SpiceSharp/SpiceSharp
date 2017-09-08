using SpiceSharp.Simulations;

namespace SpiceSharp.Parser.Readers
{
    /// <summary>
    /// Describes a quantity that can be exported using simulated data.
    /// </summary>
    public abstract class Export
    {
        /// <summary>
        /// Extract the quantity from simulated data
        /// </summary>
        /// <param name="stream">Stream</param>
        /// <param name="ckt">Circuit</param>
        public abstract double Extract(SimulationData data);

        /// <summary>
        /// Get the type name
        /// </summary>
        public abstract string TypeName { get; }

        /// <summary>
        /// Get the name based on the properties
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the type of simulation that the export should act on
        /// eg. "tran", "dc", etc. Default is null (any simulation/optional)
        /// </summary>
        public string SimulationType { get; set; } = null;

        /// <summary>
        /// Override string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }
    }
}
