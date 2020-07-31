using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.LosslessTransmissionLines;
using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A lossless transmission line
    /// </summary>
    /// <seealso cref="Component" />
    /// <seealso cref="IParameterized{P}"/>
    [Pin(0, "Pos1"), Pin(1, "Neg1"), Pin(2, "Pos2"), Pin(3, "Neg2")]
    [Connected(0, 2), Connected(1, 3), VoltageDriver(0, 2), VoltageDriver(1, 3)]
    public partial class LosslessTransmissionLine : Component,
        IParameterized<Parameters>
    {
        /// <inheritdoc/>
        public Parameters Parameters { get; } = new Parameters();

        /// <summary>
        /// The number of pins for a lossless transmission line
        /// </summary>
        public const int PinCount = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="LosslessTransmissionLine"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public LosslessTransmissionLine(string name)
            : base(name, PinCount)
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
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
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="name"/> is <c>null</c>.</exception>
        public LosslessTransmissionLine(string name, string pos1, string neg1, string pos2, string neg2, double impedance, double delay)
            : this(name)
        {
            Connect(pos1, neg1, pos2, neg2);
            Parameters.Impedance = impedance;
            Parameters.Delay = delay;
        }

        /// <inheritdoc/>
        public override void CreateBehaviors(ISimulation simulation)
        {
            var behaviors = new BehaviorContainer(Name);
            Parameters.CalculateDefaults();
            var context = new ComponentBindingContext(this, simulation, behaviors, LinkParameters);
            behaviors.Build(simulation, context)
                .AddIfNo<IAcceptBehavior>(context => new Accept(Name, context))
                .AddIfNo<ITimeBehavior>(context => new Time(Name, context))
                .AddIfNo<IFrequencyBehavior>(context => new Frequency(Name, context))
                .AddIfNo<IBiasingBehavior>(context => new Biasing(Name, context));
            simulation.EntityBehaviors.Add(behaviors);
        }
    }
}
