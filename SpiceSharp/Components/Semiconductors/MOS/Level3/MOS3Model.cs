using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.Mosfet.Level3;
using SpiceSharp.Components.Mosfet.Level3;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS3"/>
    /// </summary>
    public class MOS3Model : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS3Model(Identifier name) : base(name)
        {
            // Add parameters
            Parameters.Register(new ModelBaseParameters());
            Parameters.Register(new ModelNoiseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
