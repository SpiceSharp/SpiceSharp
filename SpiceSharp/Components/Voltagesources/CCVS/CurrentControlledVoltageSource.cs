using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentControlledVoltageSources;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Validation;
using System.Linq;
using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled voltage source.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="CurrentControlledVoltageSources.Parameters"/>
    /// <seealso cref="IRuleSubject"/>
    [Pin(0, "H+"), Pin(1, "H-"), VoltageDriver(0, 1)]
    public class CurrentControlledVoltageSource : Component,
        IParameterized<Parameters>,
        IRuleSubject
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// Gets or sets the name of the controlling entity.
        /// </summary>
        /// <value>
        /// The name of the controlling entity.
        /// </value>
        [ParameterName("control"), ParameterInfo("Controlling voltage source")]
        public string ControllingName { get; set; }

        /// <summary>
        /// The pin count for current-controlled voltage sources.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int PinCount = 2;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled current source.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public CurrentControlledVoltageSource(string name) 
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current-controlled current source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="controllingSource">The controlling voltage source name.</param>
        /// <param name="gain">The transresistance (gain).</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public CurrentControlledVoltageSource(string name, string pos, string neg, string controllingSource, double gain) 
            : this(name)
        {
            Parameters.Coefficient = gain;
            Connect(pos, neg);
            ControllingName = controllingSource;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new CurrentControlledBindingContext(this, simulation, ControllingName, LinkParameters);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new Biasing(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }

        /// <inheritdoc/>
        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();
            var nodes = Nodes.Select(name => p.Factory.GetSharedVariable(name)).ToArray();
            foreach (var rule in rules.GetRules<IConductiveRule>())
                rule.AddPath(this, nodes[0], nodes[1]);
            foreach (var rule in rules.GetRules<IAppliedVoltageRule>())
                rule.Fix(this, nodes[0], nodes[1]);
        }
    }
}
