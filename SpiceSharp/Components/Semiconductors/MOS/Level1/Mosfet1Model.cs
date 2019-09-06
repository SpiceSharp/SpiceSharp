using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;
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
                {typeof(ModelTemperatureBehavior), e => new ModelTemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Mosfet1Model"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet1Model(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }
    }
}
