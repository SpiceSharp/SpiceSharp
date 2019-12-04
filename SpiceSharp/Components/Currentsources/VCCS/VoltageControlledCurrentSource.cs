using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [Pin(0, "G+"), Pin(1, "G-"), Pin(2, "VC+"), Pin(3, "VC-"), Connected()]
    public class VoltageControlledCurrentSource : Component
    {
        /// <summary>
        /// Private constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
        public const int VoltageControlledCurrentSourcePinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        public VoltageControlledCurrentSource(string name)
            : base(name, VoltageControlledCurrentSourcePinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlledCurrentSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled current source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="gain">The transconductance gain</param>
        public VoltageControlledCurrentSource(string name, string pos, string neg, string controlPos, string controlNeg, double gain)
            : base(name, VoltageControlledCurrentSourcePinCount)
        {
            Parameters.Add(new BaseParameters(gain));
            Connect(pos, neg, controlPos, controlNeg);
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name,
                LinkParameters ? Parameters : (IParameterSetDictionary)Parameters.Clone());
            behaviors.Parameters.CalculateDefaults();
            var context = new ComponentBindingContext(simulation, behaviors, MapNodes(simulation.Variables), null);
            behaviors
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
