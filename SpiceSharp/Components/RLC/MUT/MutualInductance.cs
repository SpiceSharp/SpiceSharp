using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Components.MUT;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance
    /// </summary>
    public class MutualInductance : CircuitComponent
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("inductor1"), SpiceInfo("First coupled inductor")]
        public CircuitIdentifier MUTind1 { get; set; }
        [SpiceName("inductor2"), SpiceInfo("Second coupled inductor")]
        public CircuitIdentifier MUTind2 { get; set; }

        /// <summary>
        /// Private variables
        /// </summary>
        public Inductor Inductor1 { get; private set; }
        public Inductor Inductor2 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(CircuitIdentifier name) : base(name, 0)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;
            RegisterBehavior(new LoadBehavior());
            RegisterBehavior(new AcBehavior());
        }

        /// <summary>
        /// Setup the mutual inductance
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            // Get the inductors for the mutual inductance
            Inductor1 = ckt.Objects[MUTind1] as Inductor ?? throw new CircuitException($"{Name}: Could not find inductor '{MUTind1}'");
            Inductor2 = ckt.Objects[MUTind2] as Inductor ?? throw new CircuitException($"{Name}: Could not find inductor '{MUTind2}'");
        }
    }
}
