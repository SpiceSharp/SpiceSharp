using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors.MUT;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance
    /// </summary>
    public class MutualInductance : Component
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("inductor1"), SpiceInfo("First coupled inductor")]
        public Identifier MUTind1 { get; set; }
        [SpiceName("inductor2"), SpiceInfo("Second coupled inductor")]
        public Identifier MUTind2 { get; set; }

        /// <summary>
        /// Private variables
        /// </summary>
        public Inductor Inductor1 { get; private set; }
        public Inductor Inductor2 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(Identifier name) : base(name, 0)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;
            RegisterBehavior(new LoadBehavior(Name));
            RegisterBehavior(new AcBehavior());
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="ind1">Inductor 1</param>
        /// <param name="ind2">Inductor 2</param>
        /// <param name="coupling">Mutual inductance</param>
        public MutualInductance(Identifier name, Identifier ind1, Identifier ind2, double coupling)
            : base(name, 0)
        {
            // Register behaviors
            Priority = -1;
            RegisterBehavior(new LoadBehavior(Name));
            RegisterBehavior(new AcBehavior());

            // Connect
            MUTind1 = ind1;
            MUTind2 = ind2;
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
