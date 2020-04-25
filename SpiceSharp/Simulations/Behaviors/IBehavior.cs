namespace SpiceSharp.Behaviors
{
    /// <summary>
    /// Contract for a behavior.
    /// </summary>
    /// <seealso cref="IExportPropertySet"/>
    /// <seealso cref="IParameterized"/>
    public interface IBehavior : IExportPropertySet, IParameterized
    {
        /// <summary>
        /// Gets the name of the behavior.
        /// </summary>
        /// <value>
        /// The name of the behavior.
        /// </value>
        /// <remarks>
        /// This is typically the name of the entity that created it.
        /// </remarks>
        string Name { get; }
    }
}
