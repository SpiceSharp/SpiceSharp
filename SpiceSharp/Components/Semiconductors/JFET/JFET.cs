using SpiceSharp.Attributes;
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
        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <param name="model">The model.</param>
        public void SetModel(JFETModel model) => Model = model;

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
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add behavior factories
            Behaviors.Add(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
        }
    }
}
