using SpiceSharp.Behaviors;
using SpiceSharp.Components.JFETBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Model for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Model" />
    public class JFETModel : Model
    {
        static JFETModel()
        {
            RegisterBehaviorFactory(typeof(JFETModel), new BehaviorFactoryDictionary
            {
                {typeof(ModelTemperatureBehavior), e => new ModelTemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JFETModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model.</param>
        public JFETModel(string name)
            : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new ModelNoiseParameters());
        }
    }
}
