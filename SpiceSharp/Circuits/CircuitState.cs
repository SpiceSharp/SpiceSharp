using System;
using System.Numerics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// Container for the state of an electronic circuit.
    /// </summary>
    public class CircuitState
    {
        #region Simulation parameters
        /// <summary>
        /// Initialization flags
        /// </summary>
        public enum InitFlags
        {
            Init,
            InitFloat,
            InitJct,
            InitFix
        }

        public enum SparseFlags
        {
            NISHOULDREORDER = 0x01,
            NIDIDPREORDER = 0x100,
            NIACSHOULDREORDER = 0x10
        }

        /// <summary>
        /// All possible domain types
        /// </summary>
        public enum DomainTypes
        {
            None,
            Time,
            Frequency,
            Laplace
        }

        /// <summary>
        /// TEMPORARY: Pivot absolute tolerance
        /// </summary>
        public double PivotAbsTol { get; set; } = 1e-13;

        /// <summary>
        /// TEMPORARY: Pivot relative tolerance
        /// </summary>
        public double PivotRelTol { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the initialization flag
        /// </summary>
        public InitFlags Init { get; set; }

        /// <summary>
        /// Gets or sets the sparse matrix flags
        /// </summary>
        public SparseFlags Sparse { get; set; }

        /// <summary>
        /// Gets or sets the current domain for simulation
        /// </summary>
        public DomainTypes Domain { get; set; }

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
        public double SrcFact { get; set; } = 1.0;

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
        public bool HadNodeset { get; set; } = false;
        #endregion

        #region Simulation solutions

        /// <summary>
        /// Get the equation matrix
        /// </summary>
        public Matrix Matrix { get; private set; } = null;

        /// <summary>
        /// Gets the real part of the RHS vector
        /// </summary>
        public double[] Rhs { get; private set; } = null;

        /// <summary>
        /// Gets the imaginary part of the RHS vector
        /// </summary>
        public double[] iRhs { get; private set; } = null;

        /// <summary>
        /// Gets the (real part of the) solution vector
        /// </summary>
        public double[] Solution { get; private set; } = null;

        /// <summary>
        /// Gets the imaginary part of the solution vector
        /// </summary>
        public double[] iSolution { get; private set; } = null;

        /// <summary>
        /// Gets the old solution
        /// </summary>
        public double[] OldSolution { get; private set; } = null;

        /// <summary>
        /// Gets or sets the current laplace variable
        /// Using a purely imaginary variable here will give you the steady-state frequency response
        /// </summary>
        public Complex Laplace = new Complex();

        /// <summary>
        /// Noise state
        /// </summary>
        public CircuitStateNoise Noise { get; private set; } = null;

        /// <summary>
        /// Get the order of the matrix/vectors
        /// </summary>
        public int Order { get; private set; } = 0;

        /// <summary>
        /// The amount of states in the circuit
        /// </summary>
        public int NumStates { get; private set; } = 0;

        /// <summary>
        /// Get the states for this circuit
        /// Each element in the vector is used by circuit components to store their state
        /// </summary>
        public double[][] States { get; private set; } = null;

        /// <summary>
        /// True if already initialized
        /// </summary>
        public bool Initialized { get; private set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public CircuitState()
        {
            Matrix = new Matrix();
        }

        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="ckt"></param>
        public void Initialize(Circuit ckt)
        {
            // Initialize all matrices
            Order = ckt.Nodes.Count + 1;
            Rhs = new double[Order];
            iRhs = new double[Order];
            Solution = new double[Order];
            OldSolution = new double[Order];
            iSolution = new double[Order];

            Noise = new CircuitStateNoise();
            if (ckt.Method != null)
                ReinitStates(ckt.Method);
            else
                States = new double[2][];

            // Fill the states
            NumStates = Math.Max(NumStates, 1);
            for (int i = 0; i < States.Length; i++)
                States[i] = new double[NumStates];

            Initialized = true;
        }

        /// <summary>
        /// Reinitialize states for a method
        /// </summary>
        /// <param name="method">The integration method</param>
        public void ReinitStates(IntegrationMethods.IntegrationMethod method)
        {
            // Allocate states
            // States = new Vector<double>[method.MaxOrder + 2];
            States = new double[8][];
            NumStates = Math.Max(NumStates, 1);
            for (int i = 0; i < States.Length; i++)
                States[i] = new double[NumStates];
        }

        /// <summary>
        /// Destroy/clear the state
        /// </summary>
        public void Destroy()
        {
            Order = 0;
            NumStates = 0;
            Initialized = false;

            Rhs = null;
            iRhs = null;
            Solution = null;
            iSolution = null;
            Matrix = null;

            // Remove states
            States = null;
        }

        /// <summary>
        /// Store the solution stored in Rhs
        /// </summary>
        /// <param name="imaginary"></param>
        public void StoreSolution(bool imaginary = false)
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
            {
                Rhs[i] = 0;
                iRhs[i] = 0;
            }
            Matrix.Clear();
        }

        /// <summary>
        /// Reserve some states
        /// </summary>
        /// <param name="count">The amount of states to be reserved</param>
        /// <returns></returns>
        public int GetState(int count = 1)
        {
            int index = NumStates;
            NumStates += count;
            return index;
        }

        /// <summary>
        /// Shift all the states by one place
        /// The first element can then be used to calculate the new integrated value,
        /// but it does not guarantee the elements will be cleared
        /// </summary>
        public void ShiftStates()
        {
            // Reuse the last state vector to save memory and speed (garbage collection)
            var tmp = States[States.Length - 1];
            for (int i = States.Length - 1; i > 0; i--)
                States[i] = States[i - 1];
            States[0] = tmp;
        }

        /// <summary>
        /// Copy state 0 to all other states
        /// </summary>
        /// <param name="index"></param>
        public void CopyDC(int index)
        {
            for (int i = 1; i < States.Length; i++)
                States[i][index] = States[0][index];
        }
        #endregion
    }
}
