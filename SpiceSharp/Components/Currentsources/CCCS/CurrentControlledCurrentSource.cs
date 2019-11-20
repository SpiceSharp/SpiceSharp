using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Components.CommonBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A current-controlled current source.
    /// </summary>
    [Pin(0, "F+"), Pin(1, "F-"), Connected(0, 0)]
    public class CurrentControlledCurrentSource : Component
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("control"), ParameterInfo("Name of the controlling source")]
        public string ControllingSource { get; set; }

        /// <summary>
        /// Constants
        /// </summary>
		[ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CurrentControlledCurrentSourcePinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        public CurrentControlledCurrentSource(string name) 
            : base(name, CurrentControlledCurrentSourcePinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the current controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="voltageSource">The name of the voltage source</param>
        /// <param name="gain">The current gain</param>
        public CurrentControlledCurrentSource(string name, string pos, string neg, string voltageSource, double gain)
            : this(name)
        {
            Parameters.GetValue<BaseParameters>().Coefficient.Value = gain;
            Connect(pos, neg);
            ControllingSource = voltageSource;
        }

        /// <summary>
        /// Creates the behaviors.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="behaviors">The entities.</param>
        public override void CreateBehaviors(ISimulation simulation, IBehaviorContainer behaviors)
        {
            base.CreateBehaviors(simulation, behaviors);

            var context = new ControlledBindingContext(simulation, behaviors, MapNodes(simulation.Variables), Model, ControllingSource);
            var eb = simulation.EntityBehaviors;
            if (eb.Tracks<IFrequencyBehavior>())
                behaviors.Add(new FrequencyBehavior(Name, context));
            else if (eb.Tracks<IBiasingBehavior>())
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
