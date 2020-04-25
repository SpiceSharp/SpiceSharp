using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using System.Collections.Generic;
using System.Numerics;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="FrequencySimulation" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class FrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation point should be exported.
        /// </summary>
        /// <value>
        ///   <c>true</c> if operating point information should be exported; otherwise, <c>false</c>.
        /// </value>
        public bool KeepOpInfo { get; set; } = false;

        /// <summary>
        /// Gets or sets the frequency points to be simulated.
        /// </summary>
        /// <value>
        /// The frequency points.
        /// </value>
        public IEnumerable<double> Frequencies { get; set; }

        /// <summary>
        /// Gets or sets the solver used to solve equations. If <c>null</c>, a default solver will be used.
        /// </summary>
        /// <value>
        /// The solver.
        /// </value>
        [ParameterName("complex.solver"), ParameterInfo("The solver used to solve equations.")]
        public ISparsePivotingSolver<Complex> Solver { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the simulation should be validated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the simulation should be validated; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("frequency.validate"), ParameterInfo("Flag indicating whether the simulation should validate the circuit before executing")]
        public bool Validate { get; set; } = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyParameters"/> class.
        /// Automatically specifies a sweep from 1Hz to 100Hz with 10 points per decade.
        /// </summary>
        public FrequencyParameters()
        {
            // Default frequency-sweep
            Frequencies = new DecadeSweep(1, 100, 10);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyParameters"/> class.
        /// </summary>
        /// <param name="frequencySweep">The frequency points.</param>
        public FrequencyParameters(IEnumerable<double> frequencySweep)
        {
            Frequencies = frequencySweep;
        }
    }
}
