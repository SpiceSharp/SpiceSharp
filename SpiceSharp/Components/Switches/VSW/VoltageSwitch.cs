using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [Pin(0, "S+"), Pin(1, "S-"), Pin(2, "SC+"), Pin(3, "SC-"), Connected(0, 1)]
    public class VoltageSwitch : Component
    {
        static VoltageSwitch()
        {
            RegisterBehaviorFactory(typeof(VoltageSwitch), new BehaviorFactoryDictionary
            {
                // Add factories
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name, new VoltageControlled())},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name, new VoltageControlled())},
                {typeof(AcceptBehavior), e => new AcceptBehavior(e.Name, new VoltageControlled())}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSwitchPinCount = 4;

        /// <summary>
        /// Creates a new instance of the <see cref="VoltageSwitch"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(string name) 
            : base(name, VoltageSwitchPinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="VoltageSwitch"/> class.
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
    }
}
