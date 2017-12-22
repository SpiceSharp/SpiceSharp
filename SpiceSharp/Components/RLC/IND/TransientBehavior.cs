using SpiceSharp.Components;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.IND
{
    /// <summary>
    /// General behaviour for a <see cref="Inductor"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior
    {
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
        public double GetCurrent(Circuit ckt) => ckt.State.Solution[INDbrEq];
        [SpiceName("p"), SpiceInfo("Instantaneous power dissipated by the inductor")]
        public double GetPower(Circuit ckt) => ckt.State.Solution[INDbrEq] * ckt.State.States[0][INDstate + INDvolt];

        /// <summary>
        /// Delegate for adding effects of a mutual inductance
        /// </summary>
        /// <param name="sender">The inductor that sends the request</param>
        /// <param name="ckt">The circuit</param>
        public delegate void UpdateMutualInductanceEventHandler(TransientBehavior sender, Circuit ckt);

        /// <summary>
        /// An event that is called when mutual inductances need to be included
        /// </summary>
        public event UpdateMutualInductanceEventHandler UpdateMutualInductance;

        /// <summary>
        /// Nodes
        /// </summary>
        public int INDstate { get; protected set; }
        public int INDbrEq { get; protected set; }
        public int INDposNode { get; protected set; }
        public int INDnegNode { get; protected set; }

        /// <summary>
        /// Matrix elements
        /// </summary>
        protected MatrixElement INDibrIbrptr { get; private set; }

        /// <summary>
        /// Constants
        /// </summary>
        public const int INDflux = 0;
        public const int INDvolt = 1;

        /// <summary>
        /// Constructor
        /// </summary>
        public TransientBehavior()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ind">Inductance</param>
        public TransientBehavior(double ind)
        {
            INDinduct.Set(ind);
        }

        /// <summary>
        /// Setup the load behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        /// <returns></returns>
        public override void Setup(Entity component, Circuit ckt)
        {
            var ind = component as Inductor;

            // Get behaviors
            var load = GetBehavior<LoadBehavior>(component);

            // Create branch equation
            INDbrEq = load.INDbrEq;

            // Create 2 states
            INDstate = ckt.State.GetState(2);

            // Get matrix elements
            var matrix = ckt.State.Matrix;
            INDibrIbrptr = matrix.GetElement(INDbrEq, INDbrEq);

            // Clear all events
            if (UpdateMutualInductance != null)
            {
                foreach (var inv in UpdateMutualInductance.GetInvocationList())
                    UpdateMutualInductance -= (UpdateMutualInductanceEventHandler)inv;
            }
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
        /// Execute behaviour
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            var ckt = sim.Circuit;
            var state = ckt.State;
            var rstate = state;

            // Initialize
            if (state.UseIC && INDinitCond.Given)
                state.States[0][INDstate + INDflux] = INDinduct * INDinitCond;
            else
                state.States[0][INDstate + INDflux] = INDinduct * rstate.Solution[INDbrEq];

            // Handle mutual inductances
            UpdateMutualInductances(ckt);

            // Finally load the Y-matrix
            // Note that without an integration method, the result will be a short circuit
            var result = ckt.Method.Integrate(state, INDstate + INDflux, INDinduct);
            rstate.Rhs[INDbrEq] += result.Ceq;
            INDibrIbrptr.Sub(result.Geq);
        }
    }
}
