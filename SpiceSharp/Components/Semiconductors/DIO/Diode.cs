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
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)},
                {typeof(TemperatureBehavior), e => new TemperatureBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(NoiseBehavior), e => new NoiseBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int DiodePinCount = 2;

        /// <summary>
        /// Creates a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Diode(string name) : base(name, DiodePinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Diode"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="anode">The anode.</param>
        /// <param name="cathode">The cathode.</param>
        /// <param name="model">The model.</param>
        public Diode(string name, string anode, string cathode, string model)
            : this(name)
        {
            Connect(anode, cathode);
            Model = model;
        }
    }
}
