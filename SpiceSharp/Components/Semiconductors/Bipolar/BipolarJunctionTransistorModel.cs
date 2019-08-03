using SpiceSharp.Behaviors;
using SpiceSharp.Components.BipolarBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="BipolarJunctionTransistor"/>
    /// </summary>
    public class BipolarJunctionTransistorModel : Model
    {
        static BipolarJunctionTransistorModel()
        {
            RegisterBehaviorFactory(typeof(BipolarJunctionTransistorModel), new BehaviorFactoryDictionary
            {
                {typeof(ModelTemperatureBehavior), e => new ModelTemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BipolarJunctionTransistorModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public BipolarJunctionTransistorModel(string name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new ModelNoiseParameters());
        }
    }
}
