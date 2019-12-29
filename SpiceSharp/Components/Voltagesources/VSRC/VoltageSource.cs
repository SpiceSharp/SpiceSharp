using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CommonBehaviors;
using SpiceSharp.Components.VoltageSourceBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Validation;
using System.Linq;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), IndependentSource]
    public class VoltageSource : Component,
        IParameterized<IndependentSourceParameters>,
        IParameterized<IndependentSourceFrequencyParameters>,
        IRuleSubject
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public IndependentSourceParameters Parameters { get; } = new IndependentSourceParameters();

        /// <summary>
        /// Gets the frequency parameters.
        /// </summary>
        /// <value>
        /// The frequency parameters.
        /// </value>
        public IndependentSourceFrequencyParameters FrequencyParameters { get; } = new IndependentSourceFrequencyParameters();
        IndependentSourceFrequencyParameters IParameterized<IndependentSourceFrequencyParameters>.Parameters => FrequencyParameters;

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name</param>
        public VoltageSource(string name) 
            : base(name, VoltageSourcePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public VoltageSource(string name, string pos, string neg, double dc)
            : this(name)
        {
            Parameters.DcValue.Value = dc;
            Connect(pos, neg);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The waveform</param>
        public VoltageSource(string name, string pos, string neg, IWaveformDescription waveform) 
            : this(name)
        {
            Parameters.Waveform = waveform;
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
            var context = new ComponentBindingContext(this, simulation);
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
            var p = rules.GetParameterSet<ComponentValidationParameters>();
            var nodes = MapNodes(p.Variables);
            foreach (var rule in rules.GetRules<IConductiveRule>())
                rule.Apply(this, nodes.ToArray());
            foreach (var rule in rules.GetRules<IAppliedVoltageRule>())
                rule.Apply(this, nodes[0], nodes[1]);
        }
    }
}
