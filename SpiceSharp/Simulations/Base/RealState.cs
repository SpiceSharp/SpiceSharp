using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Container for the state of an electronic circuit.
    /// </summary>
    public class RealState : State
    {
        #region Simulation parameters
        /// <summary>
        /// Initialization flags
        /// </summary>
        [Flags]
        public enum InitializationStates
        {
            /// <summary>
            /// Default state
            /// </summary>
            None,

            /// <summary>
            /// Indicates that nodes may still be everywhere, and a first solution should be calculated
            /// </summary>
            InitFloat,

            /// <summary>
            /// Indicates that PN junctions should be initialized to a specific voltage
            /// </summary>
            /// <remarks>PN junction often don't behave well in iterative methods. A good initial value can be critical.</remarks>
            InitJunction,

            /// <summary>
            /// Indicates that an initial iteration has been done and that we need to fix it to check for convergence
            /// </summary>
            InitFix,

            /// <summary>
            /// Indicates that we are switching from DC to time-domain analysis.
            /// This is the case when calculating the first nonzero timepoint in Transient analysis.
            /// </summary>
            InitTransient
        }

        /// <summary>
        /// Sparse matrix flags
        /// </summary>
        [Flags]
        public enum SparseStates
        {
            /// <summary>
            /// Indicates that the matrix should be reordered
            /// </summary>
            /// <remarks>Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.</remarks>
            ShouldReorder = 0x01,

            /// <summary>
            /// Indicates that the matrix is preordered
            /// </summary>
            /// <remarks>Preordering uses common observations in matrices for Modifed Nodal Analysis (MNA) to reorder the matrix before running any analysis.</remarks>
            DidPreorder = 0x100,

            /// <summary>
            /// Indicates that the matrix should be reordered for AC analysis
            /// </summary>
            /// <remarks>Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.</remarks>
            AcShouldReorder = 0x10
        }

        /// <summary>
        /// All possible domain types
        /// </summary>
        public enum DomainType
        {
            None,
            Time,
            Frequency,
            Laplace
        }

        /// <summary>
        /// Extra conductance that is added to all nodes to ground to aid convergence
        /// </summary>
        public double DiagonalGmin { get; set; }

        /// <summary>
        /// Gets or sets the initialization flag
        /// </summary>
        public InitializationStates Init { get; set; }

        /// <summary>
        /// Gets or sets the sparse matrix flags
        /// </summary>
        public SparseStates Sparse { get; set; }

        /// <summary>
        /// Gets or sets the current domain for simulation
        /// </summary>
        public DomainType Domain { get; set; }

        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects
        /// If true, each device should assume the circuit is in rest
        /// </summary>
        public bool UseDc { get; set; }

        /// <summary>
        /// Gets or sets the flag for using initial conditions
        /// If true, the operating point will not be calculated, and initial 
        /// conditions will be used instead.
        /// </summary>
        public bool UseIc { get; set; }

        /// <summary>
        /// The current Gmin parameter
        /// This parameter is changed when doing GMIN stepping for aiding convergence
        /// </summary>
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// The current source factor
        /// This parameter is changed when doing source stepping for aiding convergence
        /// </summary>
        public double SourceFactor { get; set; } = 1.0;

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence
        /// </summary>
        public bool IsConvergent { get; set; } = true;

        /// <summary>
        /// The temperature for this circuit
        /// </summary>
        public double Temperature { get; set; } = 300.15;

        /// <summary>
        /// The nominal temperature for the circuit
        /// Used for model parameters as the default
        /// </summary>
        public double NominalTemperature { get; set; } = 300.15;

        /// <summary>
        /// Were the nodeset values assigned?
        /// </summary>
        public bool HadNodeSet { get; set; } = false;
        #endregion

        /// <summary>
        /// Solver
        /// </summary>
        public RealSolver Solver { get; } = new RealSolver();

        /// <summary>
        /// Gets the real solution vector
        /// </summary>
        public Vector<double> Solution { get; private set; }

        /// <summary>
        /// Gets the old solution
        /// </summary>
        public Vector<double> OldSolution { get; private set; }

        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="nodes">Nodes</param>
        public override void Initialize(VariableSet nodes)
        {
            if (nodes == null)
                throw new ArgumentNullException(nameof(nodes));

            // Initialize all matrices
            Solution = new DenseVector<double>(Solver.Order);
            OldSolution = new DenseVector<double>(Solver.Order);

            // Initialize all states and parameters
            Init = InitializationStates.None;
            Sparse = SparseStates.ShouldReorder;
            Domain = DomainType.None;
            DiagonalGmin = 0;
            UseDc = true;
            UseIc = false;

            base.Initialize(nodes);
        }

        /// <summary>
        /// Destroy/clear the state
        /// </summary>
        public override void Destroy()
        {
            Solution = null;
            OldSolution = null;
            base.Destroy();
        }

        /// <summary>
        /// Store the solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = OldSolution;
            OldSolution = Solution;
            Solution = tmp;
        }
    }
}
