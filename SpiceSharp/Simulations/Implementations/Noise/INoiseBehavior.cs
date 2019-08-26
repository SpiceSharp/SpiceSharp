namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// A behavior that is used by <see cref="Noise" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.IBehavior" />
    public interface INoiseBehavior : IBehavior
    {
        /// <summary>
        /// Calculate the noise contributions.
        /// </summary>
        void Noise();
    }
}
