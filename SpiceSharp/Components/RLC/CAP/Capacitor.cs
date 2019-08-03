using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.CapacitorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [Pin(0, "C+"), Pin(1, "C-"), Connected]
    public class Capacitor : Component
    {
        static Capacitor()
        {
            RegisterBehaviorFactory(typeof(Capacitor), new BehaviorFactoryDictionary
            {
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(TemperatureBehavior), e => new TemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int CapacitorPinCount = 2;

        /// <summary>
        /// Creates a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(string name) : base(name, CapacitorPinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Capacitor"/> class.
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(string name, string pos, string neg, double cap) 
            : base(name, CapacitorPinCount)
        {
            ParameterSets.Add(new BaseParameters(cap));
            Connect(pos, neg);
        }
    }
}
