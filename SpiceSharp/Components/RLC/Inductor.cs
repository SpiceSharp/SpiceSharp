using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Circuits;
using SpiceSharp.Diagnostics;
using SpiceSharp.Parameters;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents an inductor
    /// </summary>
    public class Inductor : CircuitComponent
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
        public Parameter<double> INDinduct { get; } = new Parameter<double>();
        [SpiceName("ic"), SpiceInfo("Initial current through the inductor", Interesting = false)]
        public Parameter<double> INDinitCond { get; } = new Parameter<double>();
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
        public Inductor(string name) : base(name, 2) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the inductor</param>
        /// <param name="pos">The positive node</param>
        /// <param name="neg">The negative node</param>
        /// <param name="ind">The inductor</param>
        public Inductor(string name, string pos, string neg, double ind) : base(name, 2)
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
            var nodes = BindNodes(ckt, CircuitNode.NodeType.Current);
            INDposNode = nodes[0].Index;
            INDnegNode = nodes[1].Index;
            INDbrEq = nodes[2].Index;

            // Create 2 states
            INDstate = ckt.State.GetState(2);

            // Clear all events
            foreach (var inv in UpdateMutualInductance.GetInvocationList())
                UpdateMutualInductance -= (UpdateMutualInductanceEventHandler)inv;
        }

        /// <summary>
        /// Get the model for the inductor
        /// </summary>
        /// <returns></returns>
        public override CircuitModel GetModel() => null;

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
    }
}
