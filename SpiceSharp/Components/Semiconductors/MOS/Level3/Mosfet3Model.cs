using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
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
                {typeof(ITemperatureBehavior), e => new ModelTemperatureBehavior(e.Name)}
            });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet3Model"/> class.
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Mosfet3Model(string name) : base(name)
        {
            Parameters.Add(new ModelBaseParameters());
            Parameters.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Mosfet3Model"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="nmos">True for NMOS transistors, false for PMOS transistors</param>
        public Mosfet3Model(string name, bool nmos) : base(name)
        {
            Parameters.Add(new ModelBaseParameters(nmos));
            Parameters.Add(new MosfetBehaviors.Common.ModelNoiseParameters());
        }
    }
}
