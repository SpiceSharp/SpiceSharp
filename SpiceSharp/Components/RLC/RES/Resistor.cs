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
                {typeof(BiasingBehavior), name => new BiasingBehavior(name)},
                {typeof(FrequencyBehavior), name => new FrequencyBehavior(name)},
                {typeof(NoiseBehavior), name => new NoiseBehavior(name)},
                {typeof(TemperatureBehavior), name => new TemperatureBehavior(name)}
            });
        }

        /// <summary>
        /// Set the model for the resistor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(ResistorModel model) => Model = model;
        
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int ResistorPinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        public Resistor(string name) 
            : base(name, ResistorPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the resistor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="res">The resistance</param>
        public Resistor(string name, string pos, string neg, double res) 
            : base(name, ResistorPinCount)
        {
            // Register parameters
            ParameterSets.Add(new BaseParameters(res));

            // Connect
            Connect(pos, neg);
        }
    }
}
