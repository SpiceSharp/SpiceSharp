using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
using SpiceSharp.Components.MosfetBehaviors.Level3;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A MOS3 Mosfet
    /// Level 3, a semi-empirical model(see reference for level 3).
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class Mosfet3 : Component
    {
        static Mosfet3()
        {
            RegisterBehaviorFactory(typeof(Mosfet3), new BehaviorFactoryDictionary
            {
                {typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name)},
                {typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(ITimeBehavior), e => new TransientBehavior(e.Name)},
                {typeof(INoiseBehavior), e => new NoiseBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int Mosfet3PinCount = 4;

        /// <summary>
        /// Creates a new instance of the <see cref="Mosfet3"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet3(string name) : base(name, Mosfet3PinCount)
        {
            Parameters.Add(new BaseParameters());
        }
    }
}
