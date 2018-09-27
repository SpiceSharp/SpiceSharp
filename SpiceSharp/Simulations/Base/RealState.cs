using System;
using SpiceSharp.Algebra;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A simulation state for simulations using real numbers.
    /// </summary>
    /// <seealso cref="State" />
    public class RealState : State
    {
        #region Simulation parameters

        /// <summary>
        /// Possible states of initialization
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

        // TODO: This should probably be separated.
        /// <summary>
        /// Possible states for solving using sparse matrices
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
        /// Possible domain types.
        /// </summary>
        public enum DomainType
        {
            /// <summary>
            /// No domain.
            /// </summary>
            None,

            /// <summary>
            /// The simulation uses the time domain
            /// </summary>
            Time,

            /// <summary>
            /// The simulation uses the frequency domain
            /// </summary>
            Frequency,

            /// <summary>
            /// The simulation uses the laplace domain (complex domain)
            /// </summary>
            Laplace
        }

        /// <summary>
        /// An extra conductance that can be added to all nodes to ground to aid convergence.
        /// </summary>
        /// <value>
        /// The conductance added on the diagonal.
        /// </value>
        public double DiagonalGmin { get; set; }

        /// <summary>
        /// Gets or sets the initialization flag.
        /// </summary>
        /// <value>
        /// The flag.
        /// </value>
        public InitializationStates Init { get; set; }

        /// <summary>
        /// Gets or sets flags for solving using sparse matrices.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public SparseStates Sparse { get; set; }

        /// <summary>
        /// Gets or sets the current domain for simulation.
        /// </summary>
        /// <value>
        /// The domain.
        /// </value>
        public DomainType Domain { get; set; }

        /// <summary>
        /// Gets or sets the flag for ignoring time-related effects. If true, each device should assume the circuit is not moving in time.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the simulation assumes a DC solution; otherwise, <c>false</c>.
        /// </value>
        public bool UseDc { get; set; }

        /// <summary>
        /// Gets or sets the flag for using initial conditions. If true, the operating point will not be calculated, and initial conditions will be used instead.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [use ic]; otherwise, <c>false</c>.
        /// </value>
        public bool UseIc { get; set; }

        /// <summary>
        /// The current minimum conductance parameter.
        /// This parameter is changed when doing GMIN stepping for aiding convergence.
        /// </summary>
        /// <value>
        /// The minimum conductance.
        /// </value>
        /// <remarks>
        /// Convergence is mainly an issue with semiconductor junctions, which often lead to exponential curves. Exponential dependencies
        /// are very harsh on convergence. A lower Gmin will cause iterations to converge faster, but to a (slightly) wrong value. By
        /// steadily relaxing this value back to 0 it is possible to progressively reach a solution without having non-convergence.
        /// </remarks>
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// The current source factor.
        /// This parameter is changed when doing source stepping for aiding convergence.
        /// </summary>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        public double SourceFactor { get; set; } = 1.0;

        /// <summary>
        /// Is the current iteration convergent?
        /// This parameter is used to communicate convergence.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the iterations converged; otherwise, <c>false</c>.
        /// </value>
        public bool IsConvergent { get; set; } = true;

        /// <summary>
        /// The current temperature for this circuit in Kelvin.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public double Temperature { get; set; } = 300.15;

        /// <summary>
        /// The nominal temperature for the circuit in Kelvin.
        /// Used by models as the default temperature where the parameters were measured.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        public double NominalTemperature { get; set; } = 300.15;

        /// <summary>
        /// Gets or sets a value indicating whether nodesets were applied.
        /// </summary>
        /// <value>
        ///   <c>true</c> if nodesets were applied; otherwise, <c>false</c>.
        /// </value>
        /// <remarks>
        /// Nodesets allow the simulator to start iterating to a solution using specified start
        /// values. If values are chosen close to the final solution, then convergence can be
        /// much faster.
        /// </remarks>
        public bool HadNodeSet { get; set; } = false;
        #endregion

        /// <summary>
        /// Gets the solver for solving linear systems of equations.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        public RealSolver Solver { get; } = new RealSolver();

        /// <summary>
        /// Gets the solution vector.
        /// </summary>
        /// <value>
        /// The solution.
        /// </value>
        public Vector<double> Solution { get; private set; }

        /// <summary>
        /// Gets the previous solution vector.
        /// </summary>
        /// <value>
        /// The old solution.
        /// </value>
        /// <remarks>
        /// This vector is needed for determining convergence.
        /// </remarks>
        public Vector<double> OldSolution { get; private set; }

        /// <summary>
        /// Setup the simulation state.
        /// </summary>
        /// <param name="nodes">The unknown variables for which the state is used.</param>
        /// <exception cref="ArgumentNullException">nodes</exception>
        public override void Setup(VariableSet nodes)
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

            base.Setup(nodes);
        }

        /// <summary>
        /// Unsetup the state.
        /// </summary>
        public override void Destroy()
        {
            Solution = null;
            OldSolution = null;
            base.Destroy();
        }

        /// <summary>
        /// Stores the solution.
        /// </summary>
        public void StoreSolution()
        {
            var tmp = OldSolution;
            OldSolution = Solution;
            Solution = tmp;
        }
    }
}
