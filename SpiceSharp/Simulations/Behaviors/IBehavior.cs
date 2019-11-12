namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Contract for a behavior.
    /// </summary>
    public interface IBehavior
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        string Name { get; }
    }
}
