namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// Indicates that a class can truncate a timestep
    /// </summary>
    public interface ITruncatable
    {
        /// <summary>
        /// Calculates a timestep to manage the truncation error
        /// </summary>
        /// <returns></returns>
        double Truncate();
    }
}
