using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.VoltageSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), IndependentSource]
    public class VoltageSource : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name</param>
        public VoltageSource(string name) : base(name, VoltageSourcePinCount)
        {
            Parameters.Add(new CommonBehaviors.IndependentSourceParameters());
            Parameters.Add(new CommonBehaviors.IndependentSourceFrequencyParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public VoltageSource(string name, string pos, string neg, double dc)
            : base(name, VoltageSourcePinCount)
        {
            Parameters.Add(new CommonBehaviors.IndependentSourceParameters(dc));
            Parameters.Add(new CommonBehaviors.IndependentSourceFrequencyParameters());
            Connect(pos, neg);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The waveform</param>
        public VoltageSource(string name, string pos, string neg, Waveform waveform) 
            : base(name, VoltageSourcePinCount)
        {
            Parameters.Add(new CommonBehaviors.IndependentSourceParameters(waveform));
            Parameters.Add(new CommonBehaviors.IndependentSourceFrequencyParameters());
            Connect(pos, neg);
        }

        /// <summary>
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            var context = new ComponentBindingContext(simulation, behaviors, ApplyConnections(simulation.Variables), Model);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<IAcceptBehavior>())
                behaviors.Add(new AcceptBehavior(Name, context));
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));

            if (eb.Tracks<IBiasingBehavior>() && !behaviors.ContainsKey(typeof(IBiasingBehavior)))
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
