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
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new ModelNoiseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="nmos">True for NMOS transistors, false for PMOS transistors</param>
        public MOS3Model(Identifier name, bool nmos) : base(name)
        {
            // Add parameters
            Parameters.Add(new ModelBaseParameters(nmos));
            Parameters.Add(new ModelNoiseParameters());

            // Add factories
            AddFactory(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
