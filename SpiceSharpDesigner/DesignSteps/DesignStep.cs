using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Designer
{
    public abstract class DesignStep
    {
        /// <summary>
        /// Gets or sets the method for applying a new input value
        /// </summary>
        public ApplyDesignInput Apply { get; set; } = null;

        /// <summary>
        /// Initialize the design step
        /// </summary>
        public ChangeCircuit Initialize { get; set; } = null;

        /// <summary>
        /// Finalize the design step
        /// </summary>
        public ChangeCircuit Finalize { get; set; } = null;

        /// <summary>
        /// Gets or sets the target measured value
        /// </summary>
        public double Target { get; set; } = 0.0;

        /// <summary>
        /// Minimum input value
        /// </summary>
        public double Minimum { get; set; } = 0.0;

        /// <summary>
        /// Maximum input value
        /// </summary>
        public double Maximum { get; set; } = 1e12;

        /// <summary>
        /// Gets the result
        /// </summary>
        public double Result { get; protected set; } = 0.0;

        /// <summary>
        /// Relative tolerance
        /// </summary>
        public double RelTol { get; set; } = 1e-6;

        /// <summary>
        /// Absolute tolerance
        /// </summary>
        public double AbsTol { get; set; } = 1e-12;

        /// <summary>
        /// Number of iterations
        /// </summary>
        public int Iterations { get; protected set; } = 0;

        /// <summary>
        /// Gets or sets the maximum number of iterations
        /// </summary>
        public int MaxIterations { get; set; } = 100;

        /// <summary>
        /// The class that will perform the measurement
        /// </summary>
        public Measurement Measurement { get; set; } = null;

        /// <summary>
        /// Execute the step
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Execute(Circuit ckt);
    }

    /// <summary>
    /// A method for applying changes to the circuit
    /// </summary>
    /// <param name="ckt">Circuit</param>
    public delegate void ChangeCircuit(Circuit ckt);

    /// <summary>
    /// Apply a value to the circuit
    /// </summary>
    /// <param name="ckt">Circuit</param>
    /// <param name="input">Input value</param>
    public delegate void ApplyDesignInput(DesignStep step, Circuit ckt, double input);
}
