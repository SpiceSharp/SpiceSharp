using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.Bipolar;
using SpiceSharp.Components.Bipolar;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BJT"/>
    /// </summary>
    public class BJTModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BJTModel(Identifier name) : base(name)
        {
            // Add parameters
            Parameters.Set(new ModelBaseParameters());
            Parameters.Set(new ModelNoiseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
