using SpiceSharp.Components.IND;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Sparse;
using SpiceSharp.Simulations;
using SpiceSharp.IntegrationMethods;
using System;

namespace SpiceSharp.Behaviors.IND
{
    /// <summary>
    /// General behaviour for a <see cref="Components.Inductor"/>
    /// </summary>
    public class TransientBehavior : Behaviors.TransientBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        BaseParameters bp;
        LoadBehavior load;
        
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
        int INDbrEq;
        protected MatrixElement INDibrIbrptr { get; private set; }
        StateVariable INDflux;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Create export method
        /// </summary>
        /// <param name="property">Property</param>
        /// <returns></returns>
        public override Func<State, double> CreateExport(string property)
        {
            switch (property)
            {
                case "flux": return (State state) => INDflux.Value;
                default: return null;
            }
        }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            // Get parameters
            bp = provider.GetParameters<BaseParameters>();

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>();

            // Clear all events
            if (UpdateMutualInductance != null)
            {
                foreach (var inv in UpdateMutualInductance.GetInvocationList())
                    UpdateMutualInductance -= (UpdateMutualInductanceEventHandler)inv;
            }
        }

        /// <summary>
        /// Get matrix pointer
        /// </summary>
        /// <param name="matrix">Matrix</param>
        public override void GetMatrixPointers(Matrix matrix)
        {
            // Get current equation
            INDbrEq = load.INDbrEq;

            // Get matrix pointers
            INDibrIbrptr = matrix.GetElement(INDbrEq, INDbrEq);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            INDibrIbrptr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
            INDflux = states.Create();
        }

        /// <summary>
        /// Calculate DC states
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void GetDCstate(TimeSimulation sim)
        {
            // Get the current through
            if (bp.INDinitCond.Given)
                INDflux.Value = bp.INDinitCond * bp.INDinduct;
            else
                INDflux.Value = sim.Circuit.State.Solution[INDbrEq] * bp.INDinduct;
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

            // Initialize
            INDflux.Value = bp.INDinduct * state.Solution[INDbrEq];

            // Handle mutual inductances
            UpdateMutualInductances(ckt);

            // Finally load the Y-matrix
            var eq = INDflux.Integrate(bp.INDinduct);
            state.Rhs[INDbrEq] += eq.Ceq;
            INDibrIbrptr.Sub(eq.Geq);
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <param name="timestep">Timestep</param>
        public override void Truncate(ref double timestep)
        {
            INDflux.LocalTruncationError(ref timestep);
        }
    }
}
