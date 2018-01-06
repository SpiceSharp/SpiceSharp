using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.VSW;
using SpiceSharp.Components.VSW;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class VoltageSwitchModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(Identifier name)
            : base(name)
        {
            // Add parameters
            Parameters.Register(new ModelBaseParameters());

            // Add factories
            AddFactory(typeof(ModelLoadBehavior), () => new ModelLoadBehavior(Name));
        }
    }
}
