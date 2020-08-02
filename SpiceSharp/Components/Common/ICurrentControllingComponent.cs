namespace SpiceSharp.Components
{
    /// <summary>
    /// Describes a component that is current-controlled.
    /// </summary>
    /// <seealso cref="IComponent" />
    public interface ICurrentControllingComponent : IComponent
    {
        /// <summary>
        /// Gets the name of the controlling source.
        /// </summary>
        /// <value>
        /// The controlling source.
        /// </value>
        public string ControllingSource { get; }
    }
}
