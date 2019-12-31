using SpiceSharp.Entities;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A few helper methods for validation.
    /// </summary>
    public static class Helpers
    {
        private static Func<IRules> _defaultRules = () => new Simulations.Biasing.Rules(new VariableSet());

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
        /// Violationses the specified entities.
        /// </summary>
        /// <param name="entities">The entities.</param>
        /// <returns>
        /// The rule violations detected by the default rules.
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
    }
}
