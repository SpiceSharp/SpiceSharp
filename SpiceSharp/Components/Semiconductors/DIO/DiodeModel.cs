using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.DIO;
using SpiceSharp.Components.DIO;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Diode"/>
    /// </summary>
    public class DiodeModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public DiodeModel(Identifier name) : base(name)
        {
            // Add parameters
            Parameters.Set(new ModelBaseParameters());
            Parameters.Set(new ModelNoiseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
