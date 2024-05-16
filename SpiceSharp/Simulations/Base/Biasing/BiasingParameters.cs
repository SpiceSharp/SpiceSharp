using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="BiasingSimulation" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public partial class BiasingParameters : ParameterSet, ICloneable<BiasingParameters>
    {
        /// <summary>
        /// Gets or sets a value indicating whether the simulation should go straight to gmin stepping.
        /// </summary>
        /// <value>
        ///   <c>true</c> if gmin steping should be skipped; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("noopiter"), ParameterInfo("Skip immediately to gmin stepping.")]
        public bool NoOperatingPointIterate { get; set; } = false;

        /// <summary>
        /// Gets or sets the minimum conductance.
        /// </summary>
        /// <value>
        /// The gmin value.
        /// </value>
        /// <remarks>
        /// Convergence is mainly an issue with semiconductor junctions, which often lead to exponential curves. Exponential dependencies
        /// are very harsh on convergence. A lower Gmin will cause iterations to converge faster, but to a (slightly) wrong value. By
        /// steadily relaxing this value back to 0 it is possible to progressively reach a solution without having non-convergence.
        /// </remarks>
        [ParameterName("gmin"), ParameterInfo("A minimum conductance for helping convergence.")]
        [GreaterThanOrEquals(0)]
        private double _gmin = 1e-12;

        /// <summary>
        /// Gets or sets the number of steps to use when using gmin stepping to improve convergence.
        /// </summary>
        /// <value>
        /// The number of steps used for gmin stepping.
        /// </value>
        [ParameterName("gminsteps"), ParameterInfo("The number of steps used for gmin stepping.")]
        [GreaterThanOrEquals(0)]
        private int _gminSteps = 10;

        /// <summary>
        /// Gets or sets the number of steps when using source stepping to improve convergence.
        /// </summary>
        /// <value>
        /// The number of steps for source-stepping.
        /// </value>
        /// <remarks>
        /// In source stepping, all sources are considered to be at 0 which has typically only one single solution (all nodes and
        /// currents are 0V and 0A). By increasing the source factor in small steps, it is possible to progressively reach a solution
        /// without having non-convergence.
        /// </remarks>
        [ParameterName("sourcesteps"), ParameterInfo("The number of steps used for source stepping.")]
        [GreaterThanOrEquals(0)]
        private int _sourceSteps = 10;

        /// <summary>
        /// Gets or sets the allowed relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance on solved quantities.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative error tolerance.")]
        [GreaterThan(0)]
        private double _relativeTolerance = 1e-3;

        /// <summary>
        /// Gets or sets the absolute tolerance on voltages.
        /// </summary>
        /// <value>
        /// The allowed voltage tolerance.
        /// </value>
        [ParameterName("vntol"), ParameterInfo("The absolute voltage error tolerance.")]
        [GreaterThanOrEquals(0)]
        private double _voltageTolerance = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance on solved quantities.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute error tolerance.")]
        [GreaterThanOrEquals(0)]
        private double _absoluteTolerance = 1e-12;

        /// <summary>
        /// Gets or sets the maximum number of iterations for operating point simulation.
        /// </summary>
        /// <value>
        /// The maximum number of iterations for DC solutions.
        /// </value>
        [ParameterName("itl1"), ParameterName("dciter"), ParameterInfo("The DC iteration limit.")]
        public int DcMaxIterations { get; set; } = 100;

        /// <summary>
        /// Gets the nodesets.
        /// </summary>
        /// <value>
        /// The nodesets.
        /// </value>
        /// <remarks>
        /// Nodesets allow specifying a value for a node that the simulator will use in its first
        /// iteration (or it will at least try to approach it). If you know an approximate solution
        /// to any voltage node, you can improve convergence by specifying it on this dictionary.
        /// </remarks>
        public Dictionary<string, double> Nodesets { get; private set; } = [];

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
        public ISparsePivotingSolver<double> CreateSolver()
        {
            var solver = new SparseRealSolver();
            solver.Parameters.AbsolutePivotThreshold = AbsolutePivotThreshold;
            solver.Parameters.RelativePivotThreshold = RelativePivotThreshold;
            return solver;
        }

        /// <inheritdoc/>
        public BiasingParameters Clone()
        {
            var clone = (BiasingParameters)MemberwiseClone();
            clone.Nodesets = [];
            foreach (var pair in Nodesets)
                clone.Nodesets.Add(pair.Key, pair.Value);
            return clone;
        }

        /// <summary>
        /// Gets or sets the (initial) temperature in Kelvin of the simulation.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        [GreaterThan(0)]
        private double _temperature = 300.15;

        /// <summary>
        /// Gets or sets the (initial) temperature in degrees celsius of the simulation.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        [ParameterName("temp"), ParameterName("temperature"), ParameterInfo("The temperature of the circuit in degrees Celsius.")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin)]
        public double TemperatureCelsius
        {
            get => Temperature - Constants.CelsiusKelvin;
            set => Temperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets the nominal temperature in Kelvin.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        [GreaterThan(0)]
        private double _nominalTemperature = 300.15;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        [ParameterName("tnom"), ParameterName("nominaltemperature"), ParameterInfo("The nominal temperature of the circuit in degrees Celsius")]
        [DerivedProperty, GreaterThan(-Constants.CelsiusKelvin)]
        public double NominalTemperatureCelsius
        {
            get => NominalTemperature - Constants.CelsiusKelvin;
            set => NominalTemperature = value + Constants.CelsiusKelvin;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the simulation should be validated.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the simulation should be validated; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("biasing.validate"), ParameterInfo("Flag indicating whether the simulation should validate the circuit before executing")]
        public bool Validate { get; set; } = true;

        /// <summary>
        /// Gets or sets the comparer used for node names.
        /// </summary>
        /// <value>
        /// The comparer use for nodes.
        /// </value>
        public IEqualityComparer<string> NodeComparer { get; set; } = Constants.DefaultComparer;
    }
}
