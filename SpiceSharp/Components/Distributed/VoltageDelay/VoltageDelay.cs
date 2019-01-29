using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.DelayBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that will drive an output to a delayed input voltage.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), VoltageDriver(0, 1)]
    public class VoltageDelay : Component
    {
        static VoltageDelay()
        {
            RegisterBehaviorFactory(typeof(VoltageDelay), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)},
                {typeof(AcceptBehavior), e => new AcceptBehavior(e.Name)}
            });
        }

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
            ParameterSets.Add(new BaseParameters());
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
            ParameterSets.Get<BaseParameters>().Delay = delay;
            Connect(pos, neg, controlPos, controlNeg);
        }
    }
}
