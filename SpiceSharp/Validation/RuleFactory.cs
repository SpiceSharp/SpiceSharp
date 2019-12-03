using System;
using System.Collections.Generic;
using SpiceSharp.Validation.Rules;

namespace SpiceSharp.Validation
{
    /// <summary>
    /// A factory for rules.
    /// </summary>
    /// <seealso cref="IRuleFactory" />
    public class RuleFactory : IRuleFactory
    {
        static RuleFactory()
        {
            Default = new RuleFactory(
                () => new FloatingNodeRule(),
                () => new HasGroundRule(),
                () => new HasIndependentSourceRule(),
                () => new ShortCircuitRule(),
                () => new VoltageLoopRule()
                );
            Default.Configuration.Add(new VariableParameters());
        }

        /// <summary>
        /// Gets a default implementation of a rule factory.
        /// </summary>
        /// <value>
        /// The default.
        /// </value>
        public static RuleFactory Default { get; }

        /// <summary>
        /// Gets the configuration for generating rules.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IParameterSetDictionary Configuration { get; }

        /// <summary>
        /// Gets the rule factories.
        /// </summary>
        /// <value>
        /// The rule factories.
        /// </value>
        public IList<Func<IRule>> RuleFactories { get; } = new List<Func<IRule>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleFactory"/> class.
        /// </summary>
        public RuleFactory()
        {
            Configuration = new ParameterSetDictionary();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleFactory"/> class.
        /// </summary>
        /// <param name="factories">The factories.</param>
        public RuleFactory(params Func<IRule>[] factories)
        {
            Configuration = new ParameterSetDictionary();
            foreach (var f in factories)
                RuleFactories.Add(f);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RuleFactory"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public RuleFactory(IParameterSetDictionary configuration)
        {
            Configuration = configuration.ThrowIfNull(nameof(configuration));
        }

        /// <summary>
        /// Creates a container for the rules.
        /// </summary>
        public IRuleContainer CreateRuleContainer()
        {
            var container = new RuleContainer();

            // Add the configurations to the container
            Configuration.CalculateDefaults();
            foreach (var ps in Configuration.Values)
                container.Configuration.Add(ps);

            // Create the rules
            foreach (var f in RuleFactories)
                container.Add(f());

            return container;
        }
    }
}
