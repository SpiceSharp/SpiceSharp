using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level1;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A Mosfet.
    /// Level 1, Shichman-Hodges.
    /// </summary>
    [Pin(0, "Drain"), Pin(1, "Gate"), Pin(2, "Source"), Pin(3, "Bulk"), Connected(0, 2), Connected(0, 3)]
    public class Mosfet1 : Component
    {
        static Mosfet1()
        {
            RegisterBehaviorFactory(typeof(Mosfet1), new BehaviorFactoryDictionary
            {
                {typeof(TemperatureBehavior), e => new TemperatureBehavior(e.Name)},
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(NoiseBehavior), e => new NoiseBehavior(e.Name)},
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int Mosfet1PinCount = 4;

        /// <summary>
        /// Creates a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1(string name) : base(name, Mosfet1PinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet1"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="d">The drain node.</param>
        /// <param name="g">The gate node.</param>
        /// <param name="s">The source node.</param>
        /// <param name="b">The bulk node.</param>
        /// <param name="model">The mosfet model.</param>
        public Mosfet1(string name, string d, string g, string s, string b, string model)
            : this(name)
        {
            Connect(d, g, s, b);
            Model = model;
        }
    }
}
