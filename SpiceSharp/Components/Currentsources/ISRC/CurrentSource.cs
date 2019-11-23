using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent current source
    /// </summary>
    [Pin(0, "I+"), Pin(1, "I-"), IndependentSource, Connected]
    public class CurrentSource : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current source</param>
        public CurrentSource(string name) 
            : base(name, CurrentSourcePinCount)
        {
            Parameters.Add(new CommonBehaviors.IndependentSourceParameters());
            Parameters.Add(new CommonBehaviors.IndependentSourceFrequencyParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public CurrentSource(string name, string pos, string neg, double dc)
            : base(name, CurrentSourcePinCount)
        {
            Parameters.Add(new CommonBehaviors.IndependentSourceParameters(dc));
            Parameters.Add(new CommonBehaviors.IndependentSourceFrequencyParameters());
            Connect(pos, neg);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The Waveform-object</param>
        public CurrentSource(string name, string pos, string neg, Waveform waveform)
            : base(name, CurrentSourcePinCount)
        {
            Parameters.Add(new CommonBehaviors.IndependentSourceParameters(waveform));
            Parameters.Add(new CommonBehaviors.IndependentSourceFrequencyParameters());
            Connect(pos, neg);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name,
                LinkParameters ? Parameters : (IParameterSetDictionary)Parameters.Clone());
            behaviors.Parameters.CalculateDefaults();
            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), null);
            behaviors
                .AddIfNo<IAcceptBehavior>(simulation, () => new AcceptBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
