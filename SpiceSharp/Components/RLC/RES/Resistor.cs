using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.ResistorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A resistor
    /// </summary>
    [Pin(0, "R+"), Pin(1, "R-")]
    public class Resistor : Component
    {
        static Resistor()
        {
            RegisterBehaviorFactory(typeof(Resistor), new BehaviorFactoryDictionary
            {
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(NoiseBehavior), e => new NoiseBehavior(e.Name)},
                {typeof(TemperatureBehavior), e => new TemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int ResistorPinCount = 2;

        /// <summary>
        /// Create a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        public Resistor(string name) 
            : base(name, ResistorPinCount)
        {
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(string name, string pos, string neg, double res) 
            : base(name, ResistorPinCount)
        {
            ParameterSets.Add(new BaseParameters(res));
            Connect(pos, neg);
        }
    }
}
