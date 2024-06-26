using SpiceSharp.Entities;
using SpiceSharp.Simulations.Variables;
using System;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A few helper methods for validation.
    /// </summary>
    public static class Helpers
    {
        private static readonly Func<IRules> _defaultRules = () =>
        {
            return new Simulations.Biasing.Rules(
                new VariableFactory(),
                Constants.DefaultComparer);
        };

        /// <summary>
        /// Returns true if the specified entity collection is valid under the default rules.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>
        ///   <c>true</c> if the specified entities are valid; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsValid(this IEntityCollection entities)
            => Validate(entities).ViolationCount > 0;

        /// <summary>
        /// Validates the collection of entities using the default rules.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>
        /// The rules used to validate the collection.
        /// </returns>
        public static IRules Validate(this IEntityCollection entities)
        {
            var rules = _defaultRules();
            foreach (var entity in entities)
            {
                if (entity is IRuleSubject subject)
                    subject.Apply(rules);
            }
            return rules;
        }

        /// <summary>
        /// Validates the collection of entities using the specified rules.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <param name="rules">The rules.</param>
        /// <returns>
        /// The rules passed by the 
        /// </returns>
        public static IRules Validate(this IEntityCollection entities, IRules rules)
        {
            rules.ThrowIfNull(nameof(rules));
            foreach (var entity in entities)
            {
                if (entity is IRuleSubject subject)
                    subject.Apply(rules);
            }
            return rules;
        }
    }
}
