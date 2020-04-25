using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.LosslessTransmissionLineBehaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A lossless transmission line
    /// </summary>
    /// <seealso cref="Component" />
    [Pin(0, "Pos1"), Pin(1, "Neg1"), Pin(2, "Pos2"), Pin(3, "Neg2"), Connected(0, 2), Connected(1, 3), VoltageDriver(0, 2), VoltageDriver(1, 3)]
    public class LosslessTransmissionLine : Component,
        IParameterized<BaseParameters>
    {
        /// <summary>
        /// Gets the parameter set.
        /// </summary>
        /// <value>
        /// The parameter set.
        /// </value>
        public BaseParameters Parameters { get; } = new BaseParameters();

        /// <summary>
        /// The number of pins for a lossless transmission line
        /// </summary>
        public const int LosslessTransmissionLinePinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="LosslessTransmissionLine"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public LosslessTransmissionLine(string name)
            : base(name, LosslessTransmissionLinePinCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LosslessTransmissionLine"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="pos1">The positive terminal on one side.</param>
        /// <param name="neg1">The negative terminal on one side.</param>
        /// <param name="pos2">The positive terminal on the other side.</param>
        /// <param name="neg2">The negative terminal on the other side.</param>
        public LosslessTransmissionLine(string name, string pos1, string neg1, string pos2, string neg2)
            : this(name)
        {
            Connect(pos1, neg1, pos2, neg2);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LosslessTransmissionLine"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <param name="pos1">The positive terminal on one side.</param>
        /// <param name="neg1">The negative terminal on one side.</param>
        /// <param name="pos2">The positive terminal on the other side.</param>
        /// <param name="neg2">The negative terminal on the other side.</param>
        /// <param name="impedance">The characteristic impedance.</param>
        /// <param name="delay">The delay.</param>
        public LosslessTransmissionLine(string name, string pos1, string neg1, string pos2, string neg2, double impedance, double delay)
            : this(name)
        {
            Connect(pos1, neg1, pos2, neg2);
            Parameters.Impedance = impedance;
            Parameters.Delay = delay;
        }

        /// <summary>
        /// Creates the behaviors for the specified simulation and registers them with the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            CalculateDefaults();
            var context = new ComponentBindingContext(this, simulation, LinkParameters);
            behaviors
                .AddIfNo<IAcceptBehavior>(simulation, () => new AcceptBehavior(Name, context))
                .AddIfNo<ITimeBehavior>(simulation, () => new TimeBehavior(Name, context))
                .AddIfNo<IFrequencyBehavior>(simulation, () => new FrequencyBehavior(Name, context))
                .AddIfNo<IBiasingBehavior>(simulation, () => new BiasingBehavior(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
