using System;
using SpiceSharp.Sparse;

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
            Init,

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
            ACShouldReorder = 0x10
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
        /// TEMPORARY: Pivot absolute tolerance
        /// </summary>
        public double PivotAbsoluteTolerance { get; set; } = 1e-13;

        /// <summary>
        /// TEMPORARY: Pivot relative tolerance
        /// </summary>
        public double PivotRelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Extra conductance that is added to all nodes to ground to aid convergence
        /// </summary>
        public double DiagonalGmin { get; set; } = 0;

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
        /// Gets or sets the flag for calculating small signal parameters
        /// If false, small signal parameters are not calculated
        /// </summary>
        public bool UseSmallSignal { get; set; }

        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects
        /// If true, each device should assume the circuit is in rest
        /// </summary>
        public bool UseDC { get; set; }

        /// <summary>
        /// Gets or sets the flag for using initial conditions
        /// If true, the operating point will not be calculated, and initial 
        /// conditions will be used instead.
        /// </summary>
        public bool UseIC { get; set; } = false;

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
        public bool IsCon { get; set; } = true;

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
        /// Get the equation matrix
        /// </summary>
        public Matrix Matrix { get; private set; } = null;

        /// <summary>
        /// Get the real right-hand-side vector
        /// </summary>
        public RealSolution Rhs { get; private set; } = null;

        /// <summary>
        /// Get the real solution vector
        /// </summary>
        public RealSolution Solution { get; private set; } = null;

        /// <summary>
        /// Gets the old solution
        /// </summary>
        public RealSolution OldSolution { get; private set; } = null;

        /// <summary>
        /// Get the order of the matrix/vectors
        /// </summary>
        public int Order { get; private set; } = 0;

        /// <summary>
        /// Constructor
        /// </summary>
        public RealState()
        {
            Matrix = new Matrix();
        }

        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="circuit"></param>
        public override void Initialize(Circuit circuit)
        {
            if (circuit == null)
                throw new ArgumentNullException(nameof(circuit));

            // Initialize all matrices
            Order = circuit.Nodes.Count + 1;
            Rhs = new RealSolution(Order);
            Solution = new RealSolution(Order);
            OldSolution = new RealSolution(Order);

            base.Initialize(circuit);
        }

        /// <summary>
        /// Destroy/clear the state
        /// </summary>
        public override void Destroy()
        {
            Order = 0;
            Rhs = null;
            Solution = null;
            Matrix = null;

            base.Destroy();
        }

        /// <summary>
        /// Store the solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = Rhs;
            Rhs = OldSolution;
            OldSolution = Solution;
            Solution = tmp;
        }

        /// <summary>
        /// Clear the matrix and Rhs vector
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < Order; i++)
                Rhs[i] = 0;
            Matrix.Clear();
        }
    }
}
