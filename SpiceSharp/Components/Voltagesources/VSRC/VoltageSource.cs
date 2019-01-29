using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.VoltageSourceBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An independent voltage source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), IndependentSource]
    public class VoltageSource : Component
    {
        static VoltageSource()
        {
            RegisterBehaviorFactory(typeof(VoltageSource), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), name => new BiasingBehavior(name)},
                {typeof(FrequencyBehavior), name => new FrequencyBehavior(name)},
                {typeof(AcceptBehavior), name => new AcceptBehavior(name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSourcePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name</param>
        public VoltageSource(string name) : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            ParameterSets.Add(new CommonBehaviors.IndependentBaseParameters());
            ParameterSets.Add(new CommonBehaviors.IndependentFrequencyParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="dc">The DC value</param>
        public VoltageSource(string name, string pos, string neg, double dc)
            : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            ParameterSets.Add(new CommonBehaviors.IndependentBaseParameters(dc));
            ParameterSets.Add(new CommonBehaviors.IndependentFrequencyParameters());

            // Connect the device
            Connect(pos, neg);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="waveform">The waveform</param>
        public VoltageSource(string name, string pos, string neg, Waveform waveform) 
            : base(name, VoltageSourcePinCount)
        {
            // Register parameters
            ParameterSets.Add(new CommonBehaviors.IndependentBaseParameters(waveform));
            ParameterSets.Add(new CommonBehaviors.IndependentFrequencyParameters());

            // Connect the device
            Connect(pos, neg);
        }
    }
}
