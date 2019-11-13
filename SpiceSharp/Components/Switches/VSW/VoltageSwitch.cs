using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [Pin(0, "S+"), Pin(1, "S-"), Pin(2, "SC+"), Pin(3, "SC-"), Connected(0, 1)]
    public class VoltageSwitch : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSwitchPinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(string name) 
            : base(name, VoltageSwitchPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        public VoltageSwitch(string name, string pos, string neg, string controlPos, string controlNeg) 
            : this(name)
        {
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
            if (eb.Tracks<IAcceptBehavior>())
                behaviors.Add(new AcceptBehavior(Name, context));
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (eb.Tracks<IBiasingBehavior>() && !behaviors.ContainsKey(typeof(IBiasingBehavior)))
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
