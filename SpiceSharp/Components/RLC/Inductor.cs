using System;
using SpiceSharp.Circuits;
using SpiceSharp.Parameters;
using System.Numerics;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents an inductor
    /// </summary>
    [SpiceNodes("L+", "L-")]
    public class Inductor : CircuitComponent<Inductor>
    {
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
        public int INDstate { get; private set; }
        public int INDbrEq { get; private set; }
        public int INDposNode { get; private set; }
        public int INDnegNode { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        private const int INDflux = 0;
        private const int INDvolt = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        public Inductor(string name) : base(name) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="ind">The inductance</param>
        public Inductor(string name, string pos, string neg, double ind) : base(name)
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
            INDbrEq = CreateNode(ckt, CircuitNode.NodeType.Current).Index;

            // Create 2 states
            INDstate = ckt.State.GetState(2);

            // Clear all events
            foreach (var inv in UpdateMutualInductance.GetInvocationList())
                UpdateMutualInductance -= (UpdateMutualInductanceEventHandler)inv;
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="ckt"></param>
        public override void Temperature(Circuit ckt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Load the inductor in the circuit
        /// </summary>
        /// <param name="ckt"></param>
        public override void Load(Circuit ckt)
        {
            var state = ckt.State;
            var rstate = state.Real;

            // Initialize
            if (state.UseIC && INDinitCond.Given)
                state.States[0][INDstate + INDflux] = INDinduct * INDinitCond;
            else
                state.States[0][INDstate + INDflux] = INDinduct * rstate.OldSolution[INDbrEq];

            // Handle mutual inductances
            UpdateMutualInductance?.Invoke(this, ckt);

            // Finally load the Y-matrix
            // Note that without an integration method, the result will be a short circuit
            if (ckt.Method != null)
            {
                var result = ckt.Method.Integrate(state, INDstate + INDflux, INDinduct);
                rstate.Rhs[INDbrEq] += result.Ceq;
                rstate.Matrix[INDbrEq, INDbrEq] -= result.Geq;
            }

            rstate.Matrix[INDposNode, INDbrEq] += 1;
            rstate.Matrix[INDnegNode, INDbrEq] -= 1;
            rstate.Matrix[INDbrEq, INDposNode] += 1;
            rstate.Matrix[INDbrEq, INDnegNode] -= 1;
        }

        /// <summary>
        /// Load the inductor for AC analysis
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void AcLoad(Circuit ckt)
        {
            var cstate = ckt.State.Complex;
            Complex val = cstate.Laplace * INDinduct.Value;

            cstate.Matrix[INDposNode, INDbrEq] += 1.0;
            cstate.Matrix[INDnegNode, INDbrEq] -= 1.0;
            cstate.Matrix[INDbrEq, INDnegNode] -= 1.0;
            cstate.Matrix[INDbrEq, INDposNode] += 1.0;
            cstate.Matrix[INDbrEq, INDbrEq] -= val;
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
    }
}
