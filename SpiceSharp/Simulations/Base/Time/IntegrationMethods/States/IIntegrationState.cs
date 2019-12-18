namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Describes a general state with a history for a <see cref="IIntegrationMethod"/>.
    /// </summary>
    public interface IIntegrationState
    {
        /// <summary>
        /// Accepts the current point and moves on to the next.
        /// </summary>
        void Accept();
    }
}
