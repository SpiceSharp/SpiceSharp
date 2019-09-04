using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class VoltageSwitchModel : Model
    {
        static VoltageSwitchModel()
        {
            RegisterBehaviorFactory(typeof(VoltageSwitchModel), new BehaviorFactoryDictionary
            {
                { typeof(CommonBehaviors.ModelParameterContainer), e => new CommonBehaviors.ModelParameterContainer(e.Name) }
            });
        }

        /// <summary>
        /// Creates a new instance of the <see cref="VoltageSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name)
            : base(name)
        {
            // Add parameters
            Parameters.Add(new VoltageModelParameters());
        }
    }
}
