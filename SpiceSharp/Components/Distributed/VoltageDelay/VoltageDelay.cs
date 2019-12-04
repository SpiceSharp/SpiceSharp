using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.DelayBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that will drive an output to a delayed input voltage.
    /// </summary>
    /// <seealso cref="Component" />
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), Connected(0, 1), VoltageDriver(0, 1)]
    public class VoltageDelay : Component
    {
        /// <summary>
        /// The voltage delay pin count
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        private const int VoltageDelayPinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDelay"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public VoltageDelay(string name)
            : base(name, VoltageDelayPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDelay"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source.</param>
        /// <param name="pos">The positive node.</param>
        /// <param name="neg">The negative node.</param>
        /// <param name="controlPos">The positive controlling node.</param>
        /// <param name="controlNeg">The negative controlling node.</param>
        /// <param name="delay">The delay.</param>
        public VoltageDelay(string name, string pos, string neg, string controlPos, string controlNeg, double delay)
            : this(name)
        {
            Parameters.GetValue<BaseParameters>().Delay = delay;
            Connect(pos, neg, controlPos, controlNeg);
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
            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model);
            behaviors
                .AddIfNo<ITimeBehavior>(simulation, () => new TransientBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context))
                .AddIfNo<IAcceptBehavior>(simulation, () => new AcceptBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
