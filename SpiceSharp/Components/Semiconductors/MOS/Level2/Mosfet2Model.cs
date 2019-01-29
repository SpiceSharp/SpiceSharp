using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level2;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet2"/>
    /// </summary>
    public class Mosfet2Model : Model
    {
        static Mosfet2Model()
        {
            RegisterBehaviorFactory(typeof(Mosfet2Model), new BehaviorFactoryDictionary
            {
                {typeof(ModelTemperatureBehavior), name => new ModelTemperatureBehavior(name)}
            });
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet2Model(string name) : base(name)
        {
            // Add parameters
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }
    }
}
