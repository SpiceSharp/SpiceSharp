﻿using System;
using System.Numerics;
using SpiceSharp.Sparse;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Container for the state of an electronic circuit.
    /// </summary>
    public class State
    {
        #region Simulation parameters
        /// <summary>
        /// Initialization flags
        /// </summary>
        [Flags]
        public enum InitFlags
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
            InitJct,

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
        public enum SparseFlags
        {
            /// <summary>
            /// Indicates that the matrix should be reordered
            /// </summary>
            /// <remarks>Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.</remarks>
            NISHOULDREORDER = 0x01,

            /// <summary>
            /// Indicates that the matrix is preordered
            /// </summary>
            /// <remarks>Preordering uses common observations in matrices for Modifed Nodal Analysis (MNA) to reorder the matrix before running any analysis.</remarks>
            NIDIDPREORDER = 0x100,

            /// <summary>
            /// Indicates that the matrix should be reordered for AC analysis
            /// </summary>
            /// <remarks>Pivoting is necessary to minimize numerical errors and to factorize a matrix using LU decomposition.</remarks>
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
        /// Extra conductance that is added to all nodes to ground to aid convergence
        /// </summary>
        public double DiagGmin = 0;

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
        /// Get the order of the matrix/vectors
        /// </summary>
        public int Order { get; private set; } = 0;

        /// <summary>
        /// True if already initialized
        /// </summary>
        public bool Initialized { get; private set; } = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public State()
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
            Initialized = true;
        }

        /// <summary>
        /// Destroy/clear the state
        /// </summary>
        public void Destroy()
        {
            Order = 0;
            Initialized = false;

            Rhs = null;
            iRhs = null;
            Solution = null;
            iSolution = null;
            Matrix = null;
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

            if (imaginary)
            {
                tmp = iRhs;
                iRhs = iSolution;
                iSolution = tmp;
            }
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
        #endregion
    }
}