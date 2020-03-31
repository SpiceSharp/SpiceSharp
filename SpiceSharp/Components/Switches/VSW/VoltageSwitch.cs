using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [Pin(0, "S+"), Pin(1, "S-"), Pin(2, "SC+"), Pin(3, "SC-"), Connected(0, 1)]
    public class VoltageSwitch : Component,
        IParameterized<BaseParameters>,
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
        public BaseParameters Parameters { get; } = new BaseParameters();

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
            CalculateDefaults();
            var context = new ComponentBindingContext(this, simulation);
            if (context.ModelBehaviors == null)
                throw new ModelNotFoundException(Model);
            behaviors
                .AddIfNo<IAcceptBehavior>(simulation, () => new AcceptBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
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
