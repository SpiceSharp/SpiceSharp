namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Interface that describes a class that keeps statistics.
    /// </summary>
    /// <typeparam name="S">The type of statistics.</typeparam>
    public interface IKeepsStatistics<S> where S : IStatistics
    {
        /// <summary>
        /// Gets the statistics.
        /// </summary>
        /// <value>
        /// The statistics.
        /// </value>
        S Statistics { get; }
    }
}
