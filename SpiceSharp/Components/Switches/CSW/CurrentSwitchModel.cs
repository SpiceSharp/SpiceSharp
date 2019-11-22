using SpiceSharp.Behaviors;
using SpiceSharp.Components.SwitchBehaviors;
using SpiceSharp.Entities;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="CurrentSwitch"/>
    /// </summary>
    public class CurrentSwitchModel : Model
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CurrentSwitchModel"/> class.
        /// </summary>
        /// <param name="name">The name of the model</param>
        public CurrentSwitchModel(string name) 
            : base(name, new ParameterSetDictionary(new InheritedTypeDictionary<IParameterSet>()))
        {
            Parameters.Add<ModelBaseParameters>(new CurrentModelParameters());
        }
    }
}
