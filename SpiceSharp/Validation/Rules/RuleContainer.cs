using SpiceSharp.General;
using SpiceSharp.Validation.Rules;
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
        /// Gets a default rule container.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static RuleContainer Default
        {
            get
            {
                var result = new RuleContainer(
                    new FloatingNodeRule(),
                    new HasGroundRule(),
                    new HasIndependentSourceRule(),
                    new ShortCircuitRule(),
                    new VoltageLoopRule());
                result.Configuration.Add(new VariableParameters());
                return result;
            }
        }

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
        /// Initializes a new instance of the <see cref="RuleContainer" /> class.
        /// </summary>
        /// <param name="rules">The rules.</param>
        public RuleContainer(params IRule[] rules)
        {
            Configuration = new ParameterSetDictionary();
            foreach (var rule in rules)
                Add(rule);
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
