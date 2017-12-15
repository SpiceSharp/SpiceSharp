using SpiceSharp.Circuits;
using SpiceSharp.Behaviors.DIO;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A diode
    /// </summary>
    [SpicePins("D+", "D-")]
    public class Diode : Component
    {
        /// <summary>
        /// Set the model for the diode
        /// </summary>
        public void SetModel(DiodeModel model) => Model = model;

        /// <summary>
        /// Extra variables
        /// </summary>
        public int DIOposNode { get; internal set; }
        public int DIOnegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int DIOpinCount = 2;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the device</param>
        public Diode(Identifier name) : base(name, DIOpinCount)
        {
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new TemperatureBehavior());
            RegisterBehavior(new AcBehavior());
            RegisterBehavior(new NoiseBehavior());
            RegisterBehavior(new TruncateBehavior());
        }

        /// <summary>
        /// Setup the device
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var model = Model as DiodeModel;

            // Allocate nodes
            var nodes = BindNodes(ckt);
            DIOposNode = nodes[0].Index;
            DIOnegNode = nodes[1].Index;
        }
    }
}
