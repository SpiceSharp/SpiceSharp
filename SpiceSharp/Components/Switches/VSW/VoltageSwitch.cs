using SpiceSharp.Attributes;
using SpiceSharp.Components.SwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled switch
    /// </summary>
    [Pin(0, "S+"), Pin(1, "S-"), Pin(2, "SC+"), Pin(3, "SC-"), Connected(0, 1)]
    public class VoltageSwitch : Component
    {
        /// <summary>
        /// Set the model for the voltage-controlled switch
        /// </summary>
        public void SetModel(VoltageSwitchModel model) => Model = model;

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageSwitchPinCount = 4;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        public VoltageSwitch(string name) 
            : base(name, VoltageSwitchPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(BiasingBehavior), () => new BiasingBehavior(Name, new VoltageControlled()));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name, new VoltageControlled()));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name, new VoltageControlled()));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the voltage-controlled switch</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        public VoltageSwitch(string name, string pos, string neg, string controlPos, string controlNeg) 
            : base(name, VoltageSwitchPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(BiasingBehavior), () => new BiasingBehavior(Name, new VoltageControlled()));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name, new VoltageControlled()));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name, new VoltageControlled()));

            Connect(pos, neg, controlPos, controlNeg);
        }
    }
}
