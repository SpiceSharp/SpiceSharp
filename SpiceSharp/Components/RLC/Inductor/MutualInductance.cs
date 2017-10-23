using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

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
            Behaviours.Behaviours.RegisterBehaviour(typeof(MutualInductance), typeof(ComponentBehaviours.MutualInductanceLoadBehaviour));
            Behaviours.Behaviours.RegisterBehaviour(typeof(MutualInductance), typeof(ComponentBehaviours.MutualInductanceAcBehaviour));
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
            // Do nothing
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Temperature(Circuit ckt)
        {
            // Do nothing
        }
    }
}
