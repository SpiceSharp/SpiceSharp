using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="VoltageSwitch"/>
    /// </summary>
    public class VoltageSwitchModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public VoltageSwitchModel(string name)
            : base(name, new ParameterSetDictionary(new InheritedTypeDictionary<IParameterSet>()))
        {
            Parameters.Add<ModelBaseParameters>(new VoltageModelParameters());
        }
    }
}
