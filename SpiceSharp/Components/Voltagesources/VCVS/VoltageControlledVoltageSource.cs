using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Entities;
using SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A voltage-controlled current-source
    /// </summary>
    [Pin(0, "V+"), Pin(1, "V-"), Pin(2, "VC+"), Pin(3, "VC-"), VoltageDriver(0, 1), Connected(0, 1)]
    public class VoltageControlledVoltageSource : Component
    {
        /// <summary>
        /// Constants
        /// </summary>
        [ParameterName("pincount"), ParameterInfo("Number of pins")]
		public const int VoltageControlledVoltageSourcePinCount = 4;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        public VoltageControlledVoltageSource(string name) 
            : base(name, VoltageControlledVoltageSourcePinCount)
        {
            Parameters.Add(new BaseParameters());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlledVoltageSource"/> class.
        /// </summary>
        /// <param name="name">The name of the voltage-controlled voltage source</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="controlPos">The positive controlling node</param>
        /// <param name="controlNeg">The negative controlling node</param>
        /// <param name="gain">The voltage gain</param>
        public VoltageControlledVoltageSource(string name, string pos, string neg, string controlPos, string controlNeg, double gain) 
            : base(name, VoltageControlledVoltageSourcePinCount)
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
            var context = new ComponentBindingContext(simulation, behaviors, ApplyConnections(simulation.Variables), Model);
            if (simulation is IBehavioral<IFrequencyBehavior>)
                behaviors.Add(new FrequencyBehavior(Name, context));
            else if (simulation is IBehavioral<IBiasingBehavior>)
                behaviors.Add(new BiasingBehavior(Name, context));
        }
    }
}
