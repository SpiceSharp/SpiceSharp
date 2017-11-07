using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A mutual inductance
    /// </summary>
    public class MutualInductance : CircuitComponent<MutualInductance>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static MutualInductance()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(MutualInductance), typeof(ComponentBehaviors.MutualInductanceLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(MutualInductance), typeof(ComponentBehaviors.MutualInductanceAcBehavior));
        }

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("k"), SpiceName("coefficient"), SpiceInfo("Mutual inductance", IsPrincipal = true)]
        public Parameter MUTcoupling { get; } = new Parameter();
        [SpiceName("inductor1"), SpiceInfo("First coupled inductor")]
        public CircuitIdentifier MUTind1 { get; set; }
        [SpiceName("inductor2"), SpiceInfo("Second coupled inductor")]
        public CircuitIdentifier MUTind2 { get; set; }

        /// <summary>
        /// The factor
        /// </summary>
        public double MUTfactor { get; internal set; }

        /// <summary>
        /// Private variables
        /// </summary>
        internal Inductor Inductor1, Inductor2;

        /// <summary>
        /// Matrix elements
        /// </summary>
        internal MatrixElement MUTbr1br2 { get; private set; }
        internal MatrixElement MUTbr2br1 { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the mutual inductance</param>
        public MutualInductance(CircuitIdentifier name) : base(name)
        {
            // Make sure mutual inductances are evaluated AFTER inductors
            Priority = -1;
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

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            MUTbr1br2 = matrix.GetElement(Inductor1.INDbrEq, Inductor2.INDbrEq);
            MUTbr2br1 = matrix.GetElement(Inductor2.INDbrEq, Inductor1.INDbrEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            MUTbr1br2 = null;
            MUTbr2br1 = null;
        }
    }
}
