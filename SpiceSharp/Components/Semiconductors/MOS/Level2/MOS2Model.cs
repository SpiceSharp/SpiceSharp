using SpiceSharp.Circuits;
using SpiceSharp.Components.Mosfet.Level2;
using SpiceSharp.Behaviors.Mosfet.Level2;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="MOS2"/>
    /// </summary>
    public class MOS2Model : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public MOS2Model(Identifier name) : base(name)
        {
            // Add parameters
            Parameters.Register(new ModelBaseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
