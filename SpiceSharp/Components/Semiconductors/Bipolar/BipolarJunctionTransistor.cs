using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.BipolarBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A bipolar junction transistor (BJT)
    /// </summary>
    [Pin(0, "Collector"), Pin(1, "Base"), Pin(2, "Emitter"), Pin(3, "Substrate")]
    public class BipolarJunctionTransistor : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int BipolarJunctionTransistorPinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistor"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BipolarJunctionTransistor(string name) 
            : base(name, BipolarJunctionTransistorPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistor"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="c">The collector node.</param>
        /// <param name="b">The base node.</param>
        /// <param name="e">The emitter node.</param>
        /// <param name="s">The substrate node.</param>
        /// <param name="model">The model.</param>
        public BipolarJunctionTransistor(string name, string c, string b, string e, string s, string model)
            : this(name)
        {
            Connect(c, b, e, s);
            Model = model;
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
            if (eb.Tracks<INoiseBehavior>())
                behaviors.Add(new NoiseBehavior(Name, context));
            else if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            if (eb.Tracks<ITimeBehavior>())
                behaviors.Add(new TransientBehavior(Name, context));

            if (eb.Tracks<IBiasingBehavior>())
            {
                if (!behaviors.ContainsKey(typeof(IBiasingBehavior)))
                    behaviors.Add(new BiasingBehavior(Name, context));
            }
            else if (eb.Tracks<ITemperatureBehavior>() && !behaviors.ContainsKey(typeof(ITemperatureBehavior)))
                behaviors.Add(new TemperatureBehavior(Name, context));
        }
    }
}
