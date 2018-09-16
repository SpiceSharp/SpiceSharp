using SpiceSharp.Attributes;
using SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors2;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source with dynamic gain.
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), VoltageDriver(0, 1), Connected(0, 1)]
    public class VoltageControlledVoltageSource2 : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageControlledVoltageSourcePinCount = 2;
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltageSource2(Identifier name) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        public VoltageControlledVoltageSource2(Identifier name, Identifier pos, Identifier neg) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            Connect(pos, neg);
        }
    }
}
