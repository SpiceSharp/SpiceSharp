using SpiceSharp.Components.LosslessTransmissionLineBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A lossless transmission line
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    public class LosslessTransmissionLine : Component
    {
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
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add behavior factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
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

            var bp = ParameterSets.Get<BaseParameters>();
            bp.Impedance = impedance;
            bp.Delay.Value = delay;
        }
    }
}
