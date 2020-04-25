using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.InductorBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An inductor
    /// </summary>
    [Pin(0, "L+"), Pin(1, "L-"), VoltageDriver(0, 1)]
    public class Inductor : Component,
        IParameterized<BaseParameters>,
        IRuleSubject
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int InductorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(string name)
            : base(name, InductorPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="inductance">The inductance</param>
        public Inductor(string name, string pos, string neg, double inductance) 
            : this(name)
        {
            Parameters.Inductance = inductance;
            Connect(pos, neg);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new ComponentBindingContext(this, simulation, LinkParameters);
            behaviors
                .AddIfNo<ITimeBehavior>(simulation, () => new TimeBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new TemperatureBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }

        /// <summary>
        /// Applies the subject to any rules in the validation provider.
        /// </summary>
        /// <param name="rules">The provider.</param>
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
