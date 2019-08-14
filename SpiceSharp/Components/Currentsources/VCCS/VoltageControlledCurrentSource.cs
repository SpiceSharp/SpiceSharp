using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), Connected(0, 1)]
    public class VoltageControlledCurrentSource : Component
    {
        static VoltageControlledCurrentSource()
        {
            RegisterBehaviorFactory(typeof(VoltageControlledCurrentSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Private constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int VoltageControlledCurrentSourcePinCount = 4;

        /// <summary>
        /// Creates a new instance of the <see cref="VoltageControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentSource(string name)
            : base(name, VoltageControlledCurrentSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="VoltageControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="gain">The transconductance gain</param>
        public VoltageControlledCurrentSource(string name, string pos, string neg, string controlPos, string controlNeg, double gain)
            : this(name)
        {
            ParameterSets.Add(new BaseParameters(gain));
            Connect(pos, neg, controlPos, controlNeg);
        }
    }
}
