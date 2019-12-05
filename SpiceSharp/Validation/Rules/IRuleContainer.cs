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
        /// Resets all the rules in the container.
        /// </summary>
        void Reset();

        /// <summary>
        /// Applies subjects to all the rules.
        /// </summary>
        /// <param name="subjects">The subjects.</param>
        void ApplySubjects(IEnumerable<IRuleSubject> subjects);

        /// <summary>
        /// Validates all rules in the container.
        /// </summary>
        void Validate();
    }
}
