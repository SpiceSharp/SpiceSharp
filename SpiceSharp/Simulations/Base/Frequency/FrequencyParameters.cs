using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using System.Numerics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="FrequencySimulation" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class FrequencyParameters : ParameterSet, ICloneable<FrequencyParameters>
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
        /// Gets or sets the relative threshold for choosing a pivot.
        /// </summary>
        /// <value>
        /// The relative pivot threshold.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value is not greater than 0.
        /// </exception>
        [ParameterName("pivrel"), ParameterInfo("The relative threshold for validating pivots")]
        [GreaterThan(0)]
        private double _relativePivotThreshold = 1e-3;

        /// <summary>
        /// Gets or sets the absolute threshold for choosing a pivot.
        /// </summary>
        /// <value>
        /// The absolute pivot threshold.
        /// </value>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the value is negative.
        /// </exception>
        [ParameterName("pivtol"), ParameterInfo("The absolute threshold for validating pivots")]
        [GreaterThanOrEquals(0)]
        private double _absolutePivotThreshold = 1e-13;

        /// <summary>
        /// Creates solver used to solve equations.
        /// </summary>
        /// <returns>A solver that can be used to solve equations.</returns>
        public ISparsePivotingSolver<Complex> CreateSolver()
        {
            var solver = new SparseComplexSolver();
            solver.Parameters.AbsolutePivotThreshold = AbsolutePivotThreshold;
            solver.Parameters.RelativePivotThreshold = RelativePivotThreshold;
            return solver;
        }

        /// <inheritdoc/>
        public FrequencyParameters Clone()
            => (FrequencyParameters)MemberwiseClone();

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
