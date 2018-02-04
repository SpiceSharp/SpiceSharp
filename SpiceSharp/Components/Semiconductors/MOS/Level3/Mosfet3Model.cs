using SpiceSharp.Components.MosfetBehaviors.Level3;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet3"/>
    /// </summary>
    public class Mosfet3Model : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet3Model(Identifier name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new ModelNoiseParameters());

            // Add factories
            Behaviors.Add(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="nmos">True for NMOS transistors, false for PMOS transistors</param>
        public Mosfet3Model(Identifier name, bool nmos) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters(nmos));
            ParameterSets.Add(new ModelNoiseParameters());

            // Add factories
            Behaviors.Add(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
