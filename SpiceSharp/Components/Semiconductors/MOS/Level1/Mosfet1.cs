using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level1;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A Mosfet.
    /// Level 1, Shichman-Hodges.
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class Mosfet1 : Component,
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
		public const int Mosfet1PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1(string name) 
            : base(name, Mosfet1PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="d">The drain node.</param>
        /// <param name="g">The gate node.</param>
        /// <param name="s">The source node.</param>
        /// <param name="b">The bulk node.</param>
        /// <param name="model">The mosfet model.</param>
        public Mosfet1(string name, string d, string g, string s, string b, string model)
            : this(name)
        {
            Connect(d, g, s, b);
            Model = model;
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new ComponentBindingContext(this, simulation);
            if (context.ModelBehaviors == null)
                throw new NoModelException(Name);
            behaviors
                .AddIfNo<INoiseBehavior>(simulation, () => new NoiseBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new TimeBehavior(Name, context))
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
            {
                rule.AddPath(this, nodes[0], nodes[2], nodes[3]);
                rule.AddPath(this, ConductionTypes.Ac, nodes[0], nodes[1]); // Gate-source capacitance
            }
        }
    }
}
