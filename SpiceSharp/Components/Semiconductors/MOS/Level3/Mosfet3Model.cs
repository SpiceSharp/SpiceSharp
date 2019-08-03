using SpiceSharp.Behaviors;
using SpiceSharp.Components.MosfetBehaviors.Level3;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A model for a <see cref="Mosfet3"/>
    /// </summary>
    public class Mosfet3Model : Model
    {
        static Mosfet3Model()
        {
            RegisterBehaviorFactory(typeof(Mosfet3Model), new BehaviorFactoryDictionary
            {
                {typeof(ModelTemperatureBehavior), e => new ModelTemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Mosfet3Model"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet3Model(string name) : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters());
            ParameterSets.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }

        /// <summary>
        /// Creates a new instance of the <see cref="Mosfet3Model"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="nmos">True for NMOS transistors, false for PMOS transistors</param>
        public Mosfet3Model(string name, bool nmos) : base(name)
        {
            ParameterSets.Add(new ModelBaseParameters(nmos));
            ParameterSets.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }
    }
}
