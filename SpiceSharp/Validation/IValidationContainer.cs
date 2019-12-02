using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// An interface describing a collection of <see cref="IValidationRule"/>.
    /// </summary>
    public interface IValidationContainer : ICloneable
    {
        /// <summary>
        /// Gets the rules for validation.
        /// </summary>
        /// <value>
        /// The rules.
        /// </value>
        IEnumerable<IValidationRule> Rules { get; }

        /// <summary>
        /// Tries to get a validation rule from the container.
        /// </summary>
        /// <typeparam name="T">The base rule type.</typeparam>
        /// <param name="rule">The rule.</param>
        /// <returns>
        /// <c>true</c> if the rule has been found; otherwise <c>false</c>.
        /// </returns>
        bool TryGetRule<T>(out T rule) where T : IValidationRule;
    }
}
