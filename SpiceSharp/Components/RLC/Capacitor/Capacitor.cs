using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using SpiceSharp.Sparse;

namespace SpiceSharp.Components
{
    /// <summary>
    /// A capacitor
    /// </summary>
    [SpicePins("C+", "C-"), ConnectedPins()]
    public class Capacitor : CircuitComponent<Capacitor>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static Capacitor()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(Capacitor), typeof(ComponentBehaviors.CapacitorLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Capacitor), typeof(ComponentBehaviors.CapacitorAcBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Capacitor), typeof(ComponentBehaviors.CapacitorTemperatureBehavior));
        }

        /// <summary>
        /// Set the model for the capacitor
        /// </summary>
        /// <param name="model"></param>
        public void SetModel(CapacitorModel model) => Model = model;

        /// <summary>
        /// Capacitance
        /// </summary>
        [SpiceName("capacitance"), SpiceInfo("Device capacitance", IsPrincipal = true)]
        public Parameter CAPcapac { get; } = new Parameter();
        [SpiceName("ic"), SpiceInfo("Initial capacitor voltage", Interesting = false)]
        public Parameter CAPinitCond { get; } = new Parameter();
        [SpiceName("w"), SpiceInfo("Device width", Interesting = false)]
        public Parameter CAPwidth { get; } = new Parameter();
        [SpiceName("l"), SpiceInfo("Device length", Interesting = false)]
        public Parameter CAPlength { get; } = new Parameter();
        [SpiceName("i"), SpiceInfo("Device current")]
        public double GetCurrent(Circuit ckt) => ckt.State.States[0][CAPstate + CAPccap];
        [SpiceName("p"), SpiceInfo("Instantaneous device power")]
        public double GetPower(Circuit ckt) => ckt.State.States[0][CAPstate + CAPccap] * (ckt.State.Solution[CAPposNode] - ckt.State.Solution[CAPnegNode]);

        /// <summary>
        /// Nodes and states
        /// </summary>
        public int CAPstate { get; private set; }
        public int CAPposNode { get; private set; }
        public int CAPnegNode { get; private set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        internal MatrixElement CAPposPosptr { get; private set; }
        internal MatrixElement CAPnegNegptr { get; private set; }
        internal MatrixElement CAPposNegptr { get; private set; }
        internal MatrixElement CAPnegPosptr { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int CAPqcap = 0;
        public const int CAPccap = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name"></param>
        public Capacitor(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the capacitor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="cap">The capacitance</param>
        public Capacitor(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double cap) : base(name)
        {
            Connect(pos, neg);
            CAPcapac.Set(cap);
        }
        
        /// <summary>
        /// Setup the capacitor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            CAPposNode = nodes[0].Index;
            CAPnegNode = nodes[1].Index;

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            CAPposPosptr = matrix.GetElement(CAPposNode, CAPposNode);
            CAPnegNegptr = matrix.GetElement(CAPnegNode, CAPnegNode);
            CAPnegPosptr = matrix.GetElement(CAPnegNode, CAPposNode);
            CAPposNegptr = matrix.GetElement(CAPposNode, CAPnegNode);

            // Create to states for integration
            CAPstate = ckt.State.GetState(2);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Unsetup(Circuit ckt)
        {
            // Remove references
            CAPposPosptr = null;
            CAPnegNegptr = null;
            CAPnegPosptr = null;
            CAPposNegptr = null;
        }

        /// <summary>
        /// Accept a timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Copy DC states when accepting the first timepoint
            if (ckt.State.Init == CircuitState.InitFlags.InitTransient)
                ckt.State.CopyDC(CAPstate + CAPqcap);
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            ckt.Method.Terr(CAPstate + CAPqcap, ckt, ref timeStep);
        }
    }
}
