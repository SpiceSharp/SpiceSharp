using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.BipolarBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A bipolar junction transistor (BJT)
    /// </summary>
    [Pin(0, "Collector"), Pin(1, "Base"), Pin(2, "Emitter"), Pin(3, "Substrate")]
    public class BipolarJunctionTransistor : Component
    {
        static BipolarJunctionTransistor()
        {
            RegisterBehaviorFactory(typeof(BipolarJunctionTransistor), new BehaviorFactoryDictionary
            {
                {typeof(ITemperatureBehavior), e => new TemperatureBehavior(e.Name)},
                {typeof(IBiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(ITimeBehavior), e => new TransientBehavior(e.Name)},
                {typeof(IFrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(INoiseBehavior), e => new NoiseBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int BipolarJunctionTransistorPinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistor"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BipolarJunctionTransistor(string name) 
            : base(name, BipolarJunctionTransistorPinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BipolarJunctionTransistor"/> class.
        /// </summary>
        /// <param name="name">The name of the device.</param>
        /// <param name="c">The collector node.</param>
        /// <param name="b">The base node.</param>
        /// <param name="e">The emitter node.</param>
        /// <param name="s">The substrate node.</param>
        /// <param name="model">The model.</param>
        public BipolarJunctionTransistor(string name, string c, string b, string e, string s, string model)
            : this(name)
        {
            Connect(c, b, e, s);
            Model = model;
        }
    }
}
