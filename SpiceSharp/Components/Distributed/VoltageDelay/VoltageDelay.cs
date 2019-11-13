using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.DelayBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that will drive an output to a delayed input voltage.
    /// </summary>
    /// <seealso cref="Component" />
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), VoltageDriver(0, 1)]
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
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            var context = new ComponentBindingContext(simulation, behaviors, ApplyConnections(simulation.Variables), Model);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<ITimeBehavior>())
                behaviors.Add(new TransientBehavior(Name, context));
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (eb.Tracks<IBiasingBehavior>() && !behaviors.ContainsKey(typeof(IBiasingBehavior)))
                behaviors.Add(new BiasingBehavior(Name, context));
            if (eb.Tracks<IAcceptBehavior>())
                behaviors.Add(new AcceptBehavior(Name, context));
        }
    }
}
