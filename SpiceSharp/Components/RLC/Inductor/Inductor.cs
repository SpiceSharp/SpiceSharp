using System;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An inductor
    /// </summary>
    [SpicePins("L+", "L-")]
    public class Inductor : CircuitComponent<Inductor>
    {
        /// <summary>
        /// Register default behaviours
        /// </summary>
        static Inductor()
        {
            Behaviors.Behaviors.RegisterBehavior(typeof(Inductor), typeof(ComponentBehaviors.InductorLoadBehavior));
            Behaviors.Behaviors.RegisterBehavior(typeof(Inductor), typeof(ComponentBehaviors.InductorAcBehavior));
        }

        /// <summary>
        /// Delegate for adding effects of a mutual inductance
        /// </summary>
        /// <param name="sender">The inductor that sends the request</param>
        /// <param name="ckt">The circuit</param>
        public delegate void UpdateMutualInductanceEventHandler(Inductor sender, Circuit ckt);

        /// <summary>
        /// An event that is called when mutual inductances need to be included
        /// </summary>
        public event UpdateMutualInductanceEventHandler UpdateMutualInductance;

        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("inductance"), SpiceInfo("Inductance of the inductor", IsPrincipal = true)]
        public Parameter INDinduct { get; } = new Parameter();
        [SpiceName("ic"), SpiceInfo("Initial current through the inductor", Interesting = false)]
        public Parameter INDinitCond { get; } = new Parameter();
        [SpiceName("flux"), SpiceInfo("Flux through the inductor")]
        public double GetFlux(Circuit ckt) => ckt.State.States[0][INDstate + INDflux];
        [SpiceName("v"), SpiceName("volt"), SpiceInfo("Terminal voltage of the inductor")]
        public double GetVolt(Circuit ckt) => ckt.State.States[0][INDstate + INDvolt];
        [SpiceName("i"), SpiceName("current"), SpiceInfo("Current through the inductor")]
        public double GetCurrent(Circuit ckt) => ckt.State.Real.Solution[INDbrEq];
        [SpiceName("p"), SpiceInfo("Instantaneous power dissipated by the inductor")]
        public double GetPower(Circuit ckt) => ckt.State.Real.Solution[INDbrEq] * ckt.State.States[0][INDstate + INDvolt];

        /// <summary>
        /// Nodes
        /// </summary>
        public int INDstate { get; internal set; }
        public int INDbrEq { get; internal set; }
        public int INDposNode { get; internal set; }
        public int INDnegNode { get; internal set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int INDflux = 0;
        public const int INDvolt = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(CircuitIdentifier name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="ind">The inductance</param>
        public Inductor(CircuitIdentifier name, CircuitIdentifier pos, CircuitIdentifier neg, double ind) : base(name)
        {
            Connect(pos, neg);
            INDinduct.Set(ind);
        }

        /// <summary>
        /// Setup the inductor
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Setup(Circuit ckt)
        {
            var nodes = BindNodes(ckt);
            INDposNode = nodes[0].Index;
            INDnegNode = nodes[1].Index;
            INDbrEq = CreateNode(ckt, Name.Grow("#branch"), CircuitNode.NodeType.Current).Index;

            // Create 2 states
            INDstate = ckt.State.GetState(2);

            // Clear all events
            foreach (var inv in UpdateMutualInductance.GetInvocationList())
                UpdateMutualInductance -= (UpdateMutualInductanceEventHandler)inv;
        }

        /// <summary>
        /// Update all mutual inductances
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public void UpdateMutualInductances(Circuit ckt)
        {
            UpdateMutualInductance?.Invoke(this, ckt);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            var method = ckt.Method;
            if (method != null && method.SavedTime == 0.0)
                ckt.State.CopyDC(INDstate + INDflux);
        }

        /// <summary>
        /// Truncate
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="timeStep">Timestep</param>
        public override void Truncate(Circuit ckt, ref double timeStep)
        {
            ckt.Method.Terr(INDstate + INDflux, ckt, ref timeStep);
        }
    }
}
