using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), VoltageDriver(0, 1), Connected(0, 1)]
    public class VoltageControlledVoltageSource : Component
    {
        static VoltageControlledVoltageSource()
        {
            RegisterBehaviorFactory(typeof(VoltageControlledVoltageSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageControlledVoltageSourcePinCount = 4;
        
        /// <summary>
        /// Creates a new instance of the <see cref="VoltageControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltageSource(string name) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="VoltageControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="gain">The voltage gain</param>
        public VoltageControlledVoltageSource(string name, string pos, string neg, string controlPos, string controlNeg, double gain) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            ParameterSets.Add(new BaseParameters(gain));
            Connect(pos, neg, controlPos, controlNeg);
        }
    }
}
