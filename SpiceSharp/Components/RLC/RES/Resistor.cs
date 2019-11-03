using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
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
                {typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(INoiseBehavior), e => new NoiseBehavior(e.Name)},
                {typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int ResistorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public Resistor(string name) 
            : base(name, ResistorPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resistor"/> class.
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(string name, string pos, string neg, double res) 
            : base(name, ResistorPinCount)
        {
            Parameters.Add(new BaseParameters(res));
            Connect(pos, neg);
        }
    }
}
