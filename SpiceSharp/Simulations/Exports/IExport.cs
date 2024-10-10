namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes an exporter for simulation data.
    /// </summary>
    /// <typeparam name="T">The type of output data.</typeparam>
    public interface IExport<T>
    {
        /// <summary>
        /// Gets or sets the simulation that the export applies to.
        /// </summary>
        ISimulation Simulation { get; set; }

        /// <summary>
        /// Gets the current value from the simulation.
        /// </summary>
        /// <remarks>
        /// This property will return a default if there is nothing to extract.
        /// </remarks>
        T Value { get; }

        /// <summary>
        /// Returns true if the export is currently valid.
        /// </summary>
        bool IsValid { get; }
    }
}
