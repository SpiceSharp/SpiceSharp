using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.DiodeBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A diode
    /// </summary>
    [Pin(0, "D+"), Pin(1, "D-")]
    public class Diode : Component
    {
        static Diode()
        {
            RegisterBehaviorFactory(typeof(Diode), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), name => new BiasingBehavior(name)},
                {typeof(TransientBehavior), name => new TransientBehavior(name)},
                {typeof(TemperatureBehavior), name => new TemperatureBehavior(name)},
                {typeof(FrequencyBehavior), name => new FrequencyBehavior(name)},
                {typeof(NoiseBehavior), name => new NoiseBehavior(name)}
            });
        }

        /// <summary>
        /// Set the model for the diode
        /// </summary>
        public void SetModel(DiodeModel model) => Model = model;

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int DiodePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Diode(string name) : base(name, DiodePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="anode">The anode.</param>
        /// <param name="cathode">The cathode.</param>
        /// <param name="model">The model.</param>
        public Diode(string name, string anode, string cathode, DiodeModel model)
            : this(name)
        {
            Connect(anode, cathode);
            Model = model;
        }
    }
}
