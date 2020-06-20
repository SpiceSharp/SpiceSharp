using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;
using System;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="BiasingSimulation" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class BiasingParameters : ParameterSet
    {
        private double _nominalTemperature = 300.15;
        private double _temperature = 300.15;
        private double _absolutePivotThreshold = 1e-13;
        private double _relativePivotThreshold = 1e-3;
        private double _absoluteTolerance = 1e-12;
        private double _voltageTolerance = 1e-6;
        private double _relativeTolerance = 1e-3;
        private int _sourceSteps = 10;
        private int _gminSteps = 10;
        private double _gmin = 1e-12;

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
        public double Gmin
        {
            get => _gmin;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Gmin), 0);
                _gmin = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of steps to use when using gmin stepping to improve convergence.
        /// </summary>
        /// <value>
        /// The number of steps used for gmin stepping.
        /// </value>
        [ParameterName("gminsteps"), ParameterInfo("The number of steps used for gmin stepping.")]
        [GreaterThanOrEquals(0)]
        public int GminSteps
        {
            get => _gminSteps;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(GminSteps), 0);
                _gminSteps = value;
            }
        }

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
        public int SourceSteps
        {
            get => _sourceSteps;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SourceSteps), 0);
                _sourceSteps = value;
            }
        }

        /// <summary>
        /// Gets or sets the allowed relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance on solved quantities.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative error tolerance.")]
        [GreaterThan(0)]
        public double RelativeTolerance
        {
            get => _relativeTolerance;
            set
            {
                Utility.GreaterThan(value, nameof(RelativeTolerance), 0);
                _relativeTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the absolute tolerance on voltages.
        /// </summary>
        /// <value>
        /// The allowed voltage tolerance.
        /// </value>
        [ParameterName("vntol"), ParameterInfo("The absolute voltage error tolerance.")]
        [GreaterThanOrEquals(0)]
        public double VoltageTolerance
        {
            get => _voltageTolerance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(VoltageTolerance), 0);
                _voltageTolerance = value;
            }
        }

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance on solved quantities.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute error tolerance.")]
        [GreaterThanOrEquals(0)]
        public double AbsoluteTolerance
        {
            get => _absoluteTolerance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(AbsoluteTolerance), 0);
                _absoluteTolerance = value;
            }
        }

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
        public Dictionary<string, double> Nodesets { get; }

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
        public double RelativePivotThreshold
        {
            get => _relativePivotThreshold;
            set
            {
                Utility.GreaterThan(value, nameof(RelativePivotThreshold), 0);
                _relativePivotThreshold = value;
            }
        }

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
        public double AbsolutePivotThreshold
        {
            get => _absolutePivotThreshold;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(AbsolutePivotThreshold), 0);
                _absolutePivotThreshold = value;
            }
        }

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

        /// <summary>
        /// Gets or sets the (initial) temperature in Kelvin of the simulation.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        [GreaterThan(0)]
        public double Temperature
        {
            get => _temperature;
            set
            {
                Utility.GreaterThan(value, nameof(Temperature), 0);
                _temperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the (initial) temperature in degrees celsius of the simulation.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        [ParameterName("temp"), ParameterName("temperature"), DerivedProperty(), ParameterInfo("The temperature of the circuit in degrees Celsius.")]
        [GreaterThan(Constants.CelsiusKelvin)]
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
        public double NominalTemperature
        {
            get => _nominalTemperature;
            set
            {
                Utility.GreaterThan(value, nameof(NominalTemperature), 0);
                _nominalTemperature = value;
            }
        }

        /// <summary>
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        [ParameterName("tnom"), ParameterName("nominaltemperature"), DerivedProperty(), ParameterInfo("The nominal temperature of the circuit in degrees Celsius")]
        [GreaterThan(Constants.CelsiusKelvin)]
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
        public IEqualityComparer<string> NodeComparer { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingParameters"/> class.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public BiasingParameters(IEqualityComparer<string> comparer = null)
        {
            NodeComparer = comparer ?? EqualityComparer<string>.Default;
            Nodesets = new Dictionary<string, double>(comparer);
        }
    }
}
