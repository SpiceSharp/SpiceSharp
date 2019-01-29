using SpiceSharp.Behaviors;
using SpiceSharp.Components.DiodeBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Diode"/>
    /// </summary>
    public class DiodeModel : Model
    {
        static DiodeModel()
        {
            RegisterBehaviorFactory(typeof(DiodeModel), new BehaviorFactoryDictionary
            {
                {typeof(ModelTemperatureBehavior), name => new ModelTemperatureBehavior(name)}
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public DiodeModel(string name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new ModelNoiseParameters());
        }
    }
}
