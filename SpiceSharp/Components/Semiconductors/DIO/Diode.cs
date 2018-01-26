using SpiceSharp.Attributes;
using SpiceSharp.Components.DiodeBehaviors;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A diode
    /// </summary>
    [Pins("D+", "D-")]
    public class Diode : Component
    {
        /// <summary>
        /// Set the model for the diode
        /// </summary>
        public void SetModel(DiodeModel model) => Model = model;

        /// <summary>
        /// Extra variables
        /// </summary>
        public int PosNode { get; private set; }
        public int NegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int DiodePinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Diode(Identifier name) : base(name, DiodePinCount)
        {
            // Add parameters
            Parameters.Add(new BaseParameters());

            // Add factories
            AddFactory(typeof(LoadBehavior), () => new LoadBehavior(Name));
            AddFactory(typeof(TransientBehavior), () => new TransientBehavior(Name));
            AddFactory(typeof(TemperatureBehavior), () => new TemperatureBehavior(Name));
            AddFactory(typeof(FrequencyBehavior), () => new FrequencyBehavior(Name));
            AddFactory(typeof(NoiseBehavior), () => new NoiseBehavior(Name));
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="circuit">The circuit</param>
        public override void Setup(Circuit circuit)
        {
            var model = Model as DiodeModel;

            // Allocate nodes
            var nodes = BindNodes(circuit);
            PosNode = nodes[0].Index;
            NegNode = nodes[1].Index;
        }
    }
}
