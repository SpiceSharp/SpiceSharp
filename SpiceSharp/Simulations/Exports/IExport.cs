namespace SpiceSharp.Simulations
{
    /// <summary>
    /// An interface that describes an exporter for simulation data.
    /// </summary>
    /// <typeparam name="T">The type of output data.</typeparam>
    public interface IExport<T>
    {
        /// <summary>
        /// Gets the current value from the simulation.
        /// </summary>
        /// <remarks>
        /// This property will return a default if there is nothing to extract.
        /// </remarks>
        T Value { get; }

        /// <summary>
        /// Destroys the export.
        /// </summary>
        void Destroy();
    }
}
