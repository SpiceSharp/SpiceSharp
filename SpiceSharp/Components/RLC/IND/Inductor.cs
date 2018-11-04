using SpiceSharp.Attributes;
using SpiceSharp.Components.InductorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An inductor
    /// </summary>
    [Pin(0, "L+"), Pin(1, "L-")]
    public class Inductor : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int InductorPinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(string name)
            : base(name, InductorPinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add factories
            Behaviors.Add(typeof(BaseBehavior), Name);
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="inductance">The inductance</param>
        public Inductor(string name, string pos, string neg, double inductance) 
            : base(name, InductorPinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters(inductance));

            // Add factories
            Behaviors.Add(typeof(BaseBehavior), Name);
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));

            // Connect
            Connect(pos, neg);
        }
    }
}
