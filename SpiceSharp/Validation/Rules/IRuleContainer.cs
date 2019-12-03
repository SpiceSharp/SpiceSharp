using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface describing a collection of <see cref="IRule"/>.
    /// </summary>
    public interface IRuleContainer : ITypeDictionary<IRule>
    {
        /// <summary>
        /// Gets the configuration for the rules.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        IParameterSetDictionary Configuration { get; }

        /// <summary>
        /// Validates the specified validators.
        /// </summary>
        /// <param name="validators">The validators.</param>
        void Validate(IEnumerable<IValidator> validators);
    }
}
