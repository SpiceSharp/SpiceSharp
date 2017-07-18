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
            InitFix
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
        /// Gets or sets the initialization flag
        /// </summary>
        public InitFlags Init { get; set; }

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
        /// The real state
        /// </summary>
        public CircuitStateReal Real { get; private set; } = null;

        /// <summary>
        /// The complex state
        /// </summary>
        public CircuitStateComplex Complex { get; private set; } = null;

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
            Order = ckt.Nodes.Count + 1;
            Real = new CircuitStateReal(Order);
            Complex = new CircuitStateComplex(Order);

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

            Real.Destroy();
            Complex.Destroy();

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
            var tmp = States[States.Length - 1];
            for (int i = States.Length - 1; i > 0; i--)
                States[i] = States[i - 1];
            // States[0] = tmp;
            States[0] = new DenseVector(States[1].Count);
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
