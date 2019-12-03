namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface describing a factory for rules.
    /// </summary>
    public interface IRuleFactory
    {
        /// <summary>
        /// Gets the configuration for generating rules.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        IParameterSetDictionary Configuration { get; }

        /// <summary>
        /// Creates a container with rules.
        /// </summary>
        IRuleContainer CreateRuleContainer();
    }
}
