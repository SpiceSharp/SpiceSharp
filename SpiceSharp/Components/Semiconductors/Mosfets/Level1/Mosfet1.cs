using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Mosfets;
using SpiceSharp.Components.Mosfets.Level1;
using SpiceSharp.Diagnostics;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A Level 1 Mosfet using the Shichman-Hodges model.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="Mosfets.Parameters"/>
    /// <seealso cref="IRuleSubject"/>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk")]
    [Connected(0, 2), Connected(0, 3)]
    public class Mosfet1 : Component,
        IParameterized<Parameters>,
        IRuleSubject
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// The pin count for mofsets.
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public Mosfet1(string name)
            : base(name, PinCount)
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

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, behaviors, LinkParameters);
            if (context.ModelBehaviors == null)
                throw new NoModelException(Name, typeof(Mosfet1Model));
            behaviors
                .AddIfNo<IBiasingBehavior>(simulation, () => new Biasing(Name, context))
                .AddIfNo<ITemperatureBehavior>(simulation, () => new Temperature(Name, context))

                // Small-signal behaviors are separate instances
                .AddIfNo<INoiseBehavior>(simulation, () => new Mosfets.Level1.Noise(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, context))

                // Time behaviors are separate instances
                .AddIfNo<ITimeBehavior>(simulation, () => new Time(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }

        /// <inheritdoc/>
        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();
            var nodes = Nodes.Select(name => p.Factory.GetSharedVariable(name)).ToArray();
            foreach (var rule in rules.GetRules<IConductiveRule>())
            {
                rule.AddPath(this, nodes[0], nodes[2], nodes[3]);
                rule.AddPath(this, ConductionTypes.Ac, nodes[0], nodes[1]); // Gate-source capacitance
                rule.AddPath(this, ConductionTypes.Ac, nodes[0], nodes[2]); // Gate-drain capacitance
            }
        }
    }
}
