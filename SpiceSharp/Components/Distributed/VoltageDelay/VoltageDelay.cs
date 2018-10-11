using SpiceSharp.Attributes;
using SpiceSharp.Components.DelayBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A component that will drive an output to a delayed input voltage.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.Component" />
    [Pin(0, "input"), Pin(1, "output")]
    public class VoltageDelay : Component
    {
        private const int VoltageDelayPinCount = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDelay"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public VoltageDelay(string name)
            : base(name, VoltageDelayPinCount)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add behaviors
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageDelay"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="input">The input node.</param>
        /// <param name="output">The output node.</param>
        /// <param name="delay"></param>
        public VoltageDelay(string name, string input, string output, double delay)
            : this(name)
        {
            ParameterSets.Get<BaseParameters>().Delay = delay;
            Connect(input, output);
        }
    }
}
