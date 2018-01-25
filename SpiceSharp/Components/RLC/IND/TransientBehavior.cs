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
        /// An event called when the flux can be updated
        /// Can be used by mutual inductances
        /// </summary>
        public event UpdateFluxEventHandler UpdateFlux;

        /// <summary>
        /// Nodes
        /// </summary>
        int INDbrEq;
        protected MatrixElement INDibrIbrptr { get; private set; }
        StateDerivative INDflux;

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
            bp = provider.GetParameterSet<BaseParameters>(0);

            // Get behaviors
            load = provider.GetBehavior<LoadBehavior>(0);

            // Clear all events
            if (UpdateFlux != null)
            {
                foreach (var inv in UpdateFlux.GetInvocationList())
                    UpdateFlux -= (UpdateFluxEventHandler)inv;
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
                INDflux.Value = sim.State.Solution[INDbrEq] * bp.INDinduct;
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="sim">Time-based simulation</param>
        public override void Transient(TimeSimulation sim)
        {
            var state = sim.State;

            // Initialize
            INDflux.Value = bp.INDinduct * state.Solution[INDbrEq];
            
            // Allow alterations of the flux
            if (UpdateFlux != null)
            {
                UpdateFluxEventArgs args = new UpdateFluxEventArgs(bp.INDinduct, state.Solution[INDbrEq], INDflux, state);
                UpdateFlux.Invoke(this, args);
            }

            // Finally load the Y-matrix
            INDflux.Integrate();
            state.Rhs[INDbrEq] += INDflux.Current();
            INDibrIbrptr.Sub(INDflux.Jacobian(bp.INDinduct));
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
