using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.InductorBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An inductor
    /// </summary>
    [Pin(0, "L+"), Pin(1, "L-")]
    public class Inductor : Component
    {
        static Inductor()
        {
            RegisterBehaviorFactory(typeof(Inductor), new BehaviorFactoryDictionary
            {
                {typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name)},
                {typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(ITimeBehavior), e => new TransientBehavior(e.Name)},
                {typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int InductorPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(string name)
            : base(name, InductorPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Inductor"/> class.
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="inductance">The inductance</param>
        public Inductor(string name, string pos, string neg, double inductance) 
            : base(name, InductorPinCount)
        {
            Parameters.Add(new BaseParameters(inductance));
            Connect(pos, neg);
        }
    }
}
