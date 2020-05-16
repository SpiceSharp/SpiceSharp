using System.Collections.Generic;
using SpiceSharp.Algebra;
using SpiceSharp.ParameterSets;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="BiasingSimulation" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    public class BiasingParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets a value indicating whether the simulation should go straight to gmin stepping.
        /// </summary>
        /// <value>
        ///   <c>true</c> if gmin steping should be skipped; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("noopiter"), ParameterInfo("Skip immediately to gmin stepping.")]
        public bool NoOperatingPointIterations { get; set; } = false;

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
        public double Gmin { get; set; } = 1e-12;

        /// <summary>
        /// Gets or sets the number of steps to use when using gmin stepping to improve convergence.
        /// </summary>
        /// <value>
        /// The number of steps used for gmin stepping.
        /// </value>
        [ParameterName("gminsteps"), ParameterInfo("The number of steps used for gmin stepping.")]
        public int GminSteps { get; set; } = 10;

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
        public int SourceSteps { get; set; } = 10;

        /// <summary>
        /// Gets or sets the allowed relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance on solved quantities.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative error tolerance.")]
        public double RelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the absolute tolerance on voltages.
        /// </summary>
        /// <value>
        /// The allowed voltage tolerance.
        /// </value>
        [ParameterName("vntol"), ParameterInfo("The absolute voltage error tolerance.")]
        public double VoltageTolerance { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance on solved quantities.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute error tolerance.")]
        public double AbsoluteTolerance { get; set; } = 1e-12;

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
        /// Gets or sets the solver used to solve equations. If <c>null</c>, a default solver will be used.
        /// </summary>
        /// <value>
        /// The solver used by the simulation.
        /// </value>
        /// TODO: Is it ok to use a reference to a solver?
        [ParameterName("biasing.solver"), ParameterInfo("The solver used to solve equations.")]
        public ISparsePivotingSolver<double> Solver { get; set; }

        /// <summary>
        /// Gets or sets the (initial) temperature in Kelvin of the simulation.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        public double Temperature { get; set; } = 300.15;

        /// <summary>
        /// Gets or sets the (initial) temperature in degrees celsius of the simulation.
        /// </summary>
        /// <value>
        /// The temperature.
        /// </value>
        [ParameterName("temp"), ParameterName("temperature"), ParameterInfo("The temperature of the circuit in degrees Celsius.")]
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
        public double NominalTemperature { get; set; } = 300.15;

        /// <summary>
        /// Gets or sets the nominal temperature in degrees celsius.
        /// </summary>
        /// <value>
        /// The nominal temperature.
        /// </value>
        [ParameterName("tnom"), ParameterName("nominaltemperature"), ParameterInfo("The nominal temperature of the circuit in degrees Celsius")]
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
