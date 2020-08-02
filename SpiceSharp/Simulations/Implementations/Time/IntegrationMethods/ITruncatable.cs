namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// This interface indicates that the class is capable of truncating a timestep in some way.
    /// </summary>
    public interface ITruncatable
    {
        /// <summary>
        /// Truncates the current timestep.
        /// </summary>
        /// <returns>
        /// The maximum timestep allowed by this instance.
        /// </returns>
        double Truncate();
    }
}
