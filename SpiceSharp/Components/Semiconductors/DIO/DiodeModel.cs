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
                {typeof(ModelTemperatureBehavior), e => new ModelTemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DiodeModel"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public DiodeModel(string name) : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new ModelNoiseParameters());
        }
    }
}
