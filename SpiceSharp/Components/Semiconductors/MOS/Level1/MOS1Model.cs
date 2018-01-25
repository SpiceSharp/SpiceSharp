using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.Mosfet.Level1;
using SpiceSharp.Components.Mosfet.Level1;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS1"/>
    /// </summary>
    public class MOS1Model : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS1Model(Identifier name) : base(name)
        {
            // Add parameters
            Parameters.Set(new ModelBaseParameters());
            Parameters.Set(new ModelNoiseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
