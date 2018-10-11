using SpiceSharp.Attributes;
using SpiceSharp.Components.DelayBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This component can delay a voltage signal for a specified time. The delay
    /// only works unidirectional.
    /// </summary>
    /// <seealso cref="Component" />
    [Pin(0, "input"), Pin(1, "output"), VoltageDriver(2, 1), Connected(2, 1)]
    public class Delay : Component
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Delay"/> class.
        /// </summary>
        /// <param name="name">The name of the entity.</param>
        public Delay(string name)
            : base(name, 2)
        {
            // Add parameters
            ParameterSets.Add(new BaseParameters());

            // Add behavior factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Delay"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="input">The input node.</param>
        /// <param name="output">The output node.</param>
        /// <param name="delay">The delay in seconds.</param>
        public Delay(string name, string input, string output, double delay)
            : base(name, 2)
        {
            // Add parameters
            var ps = new BaseParameters();
            ps.TimeDelay.Value = delay;
            ParameterSets.Add(ps);
            
            // Add behavior factories
            Behaviors.Add(typeof(LoadBehavior), () => new LoadBehavior(Name));
            Behaviors.Add(typeof(TransientBehavior), () => new TransientBehavior(Name));
            Behaviors.Add(typeof(AcceptBehavior), () => new AcceptBehavior(Name));

            // Connect
            Connect(input, output);
        }
    }
}
