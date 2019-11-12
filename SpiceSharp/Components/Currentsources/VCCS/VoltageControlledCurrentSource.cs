using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), Connected(0, 1)]
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
        /// Create one or more behaviors for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation for which behaviors need to be created.</param>
        /// <param name="entities">The other entities.</param>
        /// <param name="behaviors">A container where all behaviors are to be stored.</param>
        protected override void CreateBehaviors(ISimulation simulation, IEntityCollection entities, BehaviorContainer behaviors)
        {
            var context = new ComponentBindingContext(simulation, behaviors, ApplyConnections(simulation.Variables), null);
            if (simulation is IBehavioral<IFrequencyBehavior>)
                behaviors.Add(new FrequencyBehavior(Name, context));
            else if (simulation is IBehavioral<IBiasingBehavior>)
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
