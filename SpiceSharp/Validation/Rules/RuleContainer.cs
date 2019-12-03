using SpiceSharp.General;
using System.Collections.Generic;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A container for validation rules.
    /// </summary>
    /// <seealso cref="IRuleContainer" />
    public class RuleContainer : InterfaceTypeDictionary<IRule>, IRuleContainer
    {
        /// <summary>
        /// Gets the configuration for the rules.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IParameterSetDictionary Configuration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleContainer"/> class.
        /// </summary>
        public RuleContainer()
        {
            Configuration = new ParameterSetDictionary();
        }

        /// <summary>
        /// Validates the specified validators.
        /// </summary>
        /// <param name="validators">The validators.</param>
        public void Validate(IEnumerable<IValidator> validators)
        {
            foreach (var rule in Values)
                rule.Setup(Configuration);
            foreach (var v in validators)
                v.Validate(this);
            foreach (var rule in Values)
                rule.Validate();
        }
    }
}
