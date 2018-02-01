using SpiceSharp.Components.BipolarBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class BipolarJunctionTransistorModel : Model
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BipolarJunctionTransistorModel(Identifier name) : base(name)
        {
            // Add parameters
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new ModelNoiseParameters());

            // Add factories
            Behaviors.Add(typeof(ModelTemperatureBehavior), () => new ModelTemperatureBehavior(Name));
        }
    }
}
