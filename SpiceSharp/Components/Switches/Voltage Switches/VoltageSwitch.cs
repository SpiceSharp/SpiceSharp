using SpiceSharp.ParameterSets;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Switches;
using SpiceSharp.Diagnostics;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System.Linq;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [Pin(0, "S+"), Pin(1, "S-"), Pin(2, "SC+"), Pin(3, "SC-"), Connected(0, 1)]
    public class VoltageSwitch : Component,
        IParameterized<Parameters>,
        IRuleSubject
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSwitchPinCount = 4;

        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(string name) 
            : base(name, VoltageSwitchPinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="model">The model.</param>
        public VoltageSwitch(string name, string pos, string neg, string controlPos, string controlNeg, string model) 
            : this(name)
        {
            Model = model;
            Connect(pos, neg, controlPos, controlNeg);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            var context = new ComponentBindingContext(this, simulation, LinkParameters);
            if (context.ModelBehaviors == null)
                throw new NoModelException(Name, typeof(VoltageSwitchModel));
            behaviors
                .AddIfNo<IAcceptBehavior>(simulation, () => new Accept(Name, context, new VoltageControlled(context)))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new Frequency(Name, context, new VoltageControlled(context)))
                .AddIfNo<IBiasingBehavior>(simulation, () => new Biasing(Name, context, new VoltageControlled(context)));
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
                rule.AddPath(this, nodes[0], nodes[1]);
                rule.AddPath(this, ConductionTypes.None, nodes[2], nodes[3]);
            }
        }
    }
}
