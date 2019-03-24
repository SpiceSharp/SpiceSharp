using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.JFETBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A junction field-effect transistor.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    [Pin(0, "drain"), Pin(1, "gate"), Pin(2, "source")]
    public class JFET : Component
    {
        static JFET()
        {
            RegisterBehaviorFactory(typeof(JFET), new BehaviorFactoryDictionary
            {
                // Add behavior factories
                {typeof(TemperatureBehavior), e => new TemperatureBehavior(e.Name)},
                {typeof(BiasingBehavior), e => new BiasingBehavior(e.Name)},
                {typeof(FrequencyBehavior), e => new FrequencyBehavior(e.Name)},
                {typeof(TransientBehavior), e => new TransientBehavior(e.Name)}
            });
        }

        /// <summary>
        /// The number of pins on a JFET.
        /// </summary>
        public const int JFETPincount = 3;

        /// <summary>
        /// Initializes a new instance of the <see cref="JFET"/> class.
        /// </summary>
        /// <param name="name">The string of the entity.</param>
        public JFET(string name)
            : base(name, JFETPincount)
        {
            ParameterSets.Add(new BaseParameters());
        }
    }
}
