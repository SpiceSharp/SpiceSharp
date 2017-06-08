using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SpiceSharp.Circuits
{
    /// <summary>
    /// This class represents the state of the circuit
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
            InitFix,
            InitPred
        }

        /// <summary>
        /// Gets or sets the current initialization flag
        /// </summary>
        public InitFlags Init { get; set; }

        /// <summary>
        /// Gets or sets the current mode
        /// This depends on the type of simulation currently performed
        /// </summary>
        public int Mode { get; set; } = 0;

        /// <summary>
        /// True if the initial conditions should be used
        /// </summary>
        public bool UseIC { get; set; } = false;

        /// <summary>
        /// True if the current simulation is solving a DC solution
        /// </summary>
        public bool IsDc { get; set; } = false;

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
        public Vector<double>[] States { get; private set; } = null;

        /// <summary>
        /// Get the equation matrix
        /// </summary>
        public Matrix<double> Matrix { get; private set; } = null;

        /// <summary>
        /// Get the right-hand side vector
        /// </summary>
        public Vector<double> Rhs { get; private set; } = null;

        /// <summary>
        /// Get the current solution
        /// </summary>
        public Vector<double> Solution { get; private set; } = null;

        /// <summary>
        /// Get the previous solution
        /// Can be used for checking convergence
        /// </summary>
        public Vector<double> OldSolution { get; private set; } = null;

        /// <summary>
        /// True if already initialized
        /// </summary>
        public bool Initialized { get; private set; } = false;

        /// <summary>
        /// Initialize the state
        /// </summary>
        /// <param name="ckt"></param>
        public void Initialize(Circuit ckt)
        {
            // Initialize all matrices
            Order = ckt.Nodes.Count;
            Matrix = new SparseMatrix(Order + 1);
            Rhs = new SparseVector(Order + 1);
            Solution = new DenseVector(Order + 1);
            OldSolution = new DenseVector(Order + 1);

            // Allocate states
            if (ckt.Method != null)
                States = new Vector<double>[ckt.Method.MaxOrder + 2];
            else
                States = new Vector<double>[2];
            NumStates = Math.Max(NumStates, 1);
            for (int i = 0; i < States.Length; i++)
                States[i] = new DenseVector(NumStates);

            Initialized = true;
        }

        /// <summary>
        /// Destroy the state
        /// </summary>
        public void Destroy()
        {
            Order = 0;
            NumStates = 0;
            Initialized = false;

            // Remove all matrices
            Matrix = null;
            Rhs = null;
            Solution = null;
            OldSolution = null;

            // Remove states
            States = null;
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
            Vector<double> tmp = States[States.Length - 1];
            for (int i = States.Length - 2; i >= 0; i--)
                States[i + 1] = States[i];
            States[0] = tmp;
        }

        /// <summary>
        /// Solve the matrix equations
        /// </summary>
        public void Solve()
        {
            // All indices at 0 are the ground node
            // We remove these rows/columns because they will lead to a singular matrix
            var m = Matrix.RemoveRow(0).RemoveColumn(0);
            var b = Rhs.SubVector(1, Rhs.Count - 1);

            // Create a new solution vector of the original size
            if (Solution == null)
                Solution = new DenseVector(Rhs.Count);

            // Fill the 1-N elements with the solution
            var sol = m.Solve(b);
            Solution[0] = 0.0;
            Solution.SetSubVector(1, Solution.Count - 1, sol);
        }

        /// <summary>
        /// Clear the Matrix and Rhs vector
        /// </summary>
        public void Clear()
        {
            Matrix.Clear();
            Rhs.Clear();
        }

        /// <summary>
        /// Store the current solution
        /// </summary>
        public void StoreSolution()
        {
            var tmp = OldSolution;
            OldSolution = Solution;
            Solution = tmp;
        }
        #endregion
    }
}
