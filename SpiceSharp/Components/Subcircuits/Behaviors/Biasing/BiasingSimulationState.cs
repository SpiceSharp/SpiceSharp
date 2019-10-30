using SpiceSharp.Algebra;
using SpiceSharp.Simulations;
using System.Collections.Generic;

namespace SpiceSharp.Components.SubcircuitBehaviors
{
    /// <summary>
    /// An <see cref="IBiasingSimulationState"/> for using in <see cref="BiasingBehavior"/>.
    /// </summary>
    public abstract class BiasingSimulationState : IBiasingSimulationState
    {
        /// <summary>
        /// A pair of <see cref="Element{T}"/> instances.
        /// </summary>
        protected struct ElementPair
        {
            /// <summary>
            /// The parent element.
            /// </summary>
            public Element<double> Parent;

            /// <summary>
            /// The local element.
            /// </summary>
            public Element<double> Local;

            /// <summary>
            /// Initializes the structure.
            /// </summary>
            /// <param name="local">The local element.</param>
            /// <param name="parent">The parent element.</param>
            public ElementPair(Element<double> local, Element<double> parent)
            {
                Local = local;
                Parent = parent;
            }
        }

        /// <summary>
        /// Gets the parent <see cref="IBiasingSimulationState"/>.
        /// </summary>
        /// <value>
        /// The parent.
        /// </value>
        protected IBiasingSimulationState Parent { get; }

        /// <summary>
        /// Gets or sets the initialization flag.
        /// </summary>
        public InitializationModes Init => Parent.Init;

        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects. If true, each device should assume the circuit is not moving in time.
        /// </summary>
        public bool UseDc => Parent.UseDc;

        /// <summary>
        /// Gets or sets the flag for using initial conditions. If true, the operating point will not be calculated, and initial conditions will be used instead.
        /// </summary>
        public bool UseIc => Parent.UseIc;

        /// <summary>
        /// The current source factor.
        /// This parameter is changed when doing source stepping for aiding convergence.
        /// </summary>
        public double SourceFactor => Parent.SourceFactor;

        /// <summary>
        /// Gets or sets the a conductance that is shunted with PN junctions to aid convergence.
        /// </summary>
        public double Gmin => Parent.Gmin;

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        public bool IsConvergent
        {
            get => Parent.IsConvergent && _isConvergent;
            set => _isConvergent = value;
        }
        private bool _isConvergent;

        /// <summary>
        /// The current temperature for this circuit in Kelvin.
        /// </summary>
        public double Temperature { get; set; }

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        public double NominalTemperature => Parent.NominalTemperature;

        /// <summary>
        /// Gets or sets the solution.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public IVector<double> Solution { get; protected set; }

        /// <summary>
        /// Gets or sets the old solution.
        /// </summary>
        /// <value>
        /// The old solution.
        /// </value>
        public IVector<double> OldSolution { get; protected set; }

        /// <summary>
        /// Gets the sparse solver.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public ISparseSolver<double> Solver => LocalSolver;
        protected SparseRealSolver<SparseMatrix<double>, SparseVector<double>> LocalSolver { get; }

        /// <summary>
        /// Gets the variable to index map.
        /// </summary>
        /// <value>
        /// The map.
        /// </value>
        public IVariableMap Map { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingSimulationState"/> class.
        /// </summary>
        /// <param name="parent">The parent.</param>
        protected BiasingSimulationState(IBiasingSimulationState parent)
        {
            Parent = parent.ThrowIfNull(nameof(parent));
            LocalSolver = LUHelper.CreateSparseRealSolver();
            Map = new VariableMap(Parent.Map.Ground);
        }

        /// <summary>
        /// Notifies the state that these variables can be shared with other states.
        /// </summary>
        /// <param name="common">The common.</param>
        public abstract void ShareVariables(HashSet<Variable> common);

        /// <summary>
        /// Set up the simulation state for the simulation.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public virtual void Setup(ISimulation simulation)
        {
            Temperature = Parent.Temperature;
        }

        /// <summary>
        /// Destroys the simulation state.
        /// </summary>
        public virtual void Unsetup()
        {
        }

        /// <summary>
        /// Resets the biasing state for loading the local behaviors.
        /// </summary>
        public virtual void Reset()
        {
            Solver.Reset();
            _isConvergent = true;
        }

        /// <summary>
        /// Determines whether the state solution is convergent.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is convergent; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CheckConvergence();

        /// <summary>
        /// Apply changes locally.
        /// </summary>
        public abstract void ApplyAsynchroneously();

        /// <summary>
        /// Apply changes to the parent biasing state.
        /// </summary>
        public virtual void ApplySynchroneously()
        {
            Parent.IsConvergent &= _isConvergent;
        }
    }
}
