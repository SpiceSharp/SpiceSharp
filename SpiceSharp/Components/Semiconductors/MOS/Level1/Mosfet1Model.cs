using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level1;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet1"/>
    /// </summary>
    public class Mosfet1Model : Model
    {
        static Mosfet1Model()
        {
            RegisterBehaviorFactory(typeof(Mosfet1Model), new BehaviorFactoryDictionary
            {
                {typeof(ModelTemperatureBehavior), name => new ModelTemperatureBehavior(name)}
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1Model(string name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }
    }
}
