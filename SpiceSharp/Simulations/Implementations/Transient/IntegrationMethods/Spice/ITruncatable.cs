namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// This interface indicates that a <see cref="StateDerivative"/> is capable of truncating the timestep.
    /// </summary>
    public interface ITruncatable
    {
        /// <summary>
        /// Truncates the current timestep.
        /// </summary>
        /// <returns>
        /// The maximum timestep allowed by this state.
        /// </returns>
        double Truncate();
    }
}
