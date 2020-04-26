using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Validation;
using System.Linq;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source.
    /// </summary>
    /// <seealso cref="Component"/>
    /// <seealso cref="IParameterized{P}"/>
    /// <seealso cref="BaseParameters"/>
    /// <seealso cref="IRuleSubject"/>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component,
        IParameterized<BaseParameters>,
        IRuleSubject
    {
        /// <inheritdoc/>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// Gets or sets the name of the controlling entity.
        /// </summary>
        /// <value>
        /// The name of the controlling entity.
        /// </value>
        [ParameterName("control"), ParameterInfo("Name of the controlling entity/source")]
        public string ControllingSource { get; set; }

        /// <summary>
        /// The number of pins for a <see cref="CurrentControlledCurrentSource"/>.
        /// </summary>
		[ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int PinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public CurrentControlledCurrentSource(string name) 
            : base(name, PinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="voltageSource">The name of the voltage source.</param>
        /// <param name="gain">The current gain.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public CurrentControlledCurrentSource(string name, string pos, string neg, string voltageSource, double gain)
            : this(name)
        {
            Parameters.Coefficient = gain;
            Connect(pos, neg);
            ControllingSource = voltageSource;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new CurrentControlledBindingContext(this, simulation, ControllingSource, LinkParameters);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }

        void IRuleSubject.Apply(IRules rules)
        {
            var p = rules.GetParameterSet<ComponentRuleParameters>();
            var nodes = Nodes.Select(name => p.Factory.GetSharedVariable(name)).ToArray();
            foreach (var r in rules.GetRules<IConductiveRule>())
                r.AddPath(this, ConductionTypes.None, nodes[0], nodes[1]);
        }
    }
}
