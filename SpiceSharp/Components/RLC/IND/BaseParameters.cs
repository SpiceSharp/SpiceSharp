using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.InductorBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Inductor" />
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class BaseParameters : ParameterSet
    {
        private double _seriesMultiplier = 1.0;
        private double _parallelMultiplier = 1.0;
        private double _inductance;

        /// <summary>
        /// Gets the inductance parameter.
        /// </summary>
        [ParameterName("inductance"), ParameterInfo("Inductance of the inductor", Units = "H", IsPrincipal = true)]
        [GreaterThanOrEquals(0)]
        public double Inductance
        {
            get => _inductance;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Inductance), 0);
                _inductance = value;
            }
        }

        /// <summary>
        /// Gets the initial current parameter.
        /// </summary>
        [ParameterName("ic"), ParameterInfo("Initial current through the inductor", Units = "V", Interesting = false)]
        public GivenParameter<double> InitialCondition { get; set; }

        /// <summary>
        /// Gets or sets the parallel multiplier.
        /// </summary>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThan(0)]
        public double ParallelMultiplier
        {
            get => _parallelMultiplier;
            set
            {
                Utility.GreaterThan(value, nameof(ParallelMultiplier), 0);
                _parallelMultiplier = value;
            }
        }

        /// <summary>
        /// Gets or sets the series multiplier.
        /// </summary>
        /// <value>
        /// The series multiplier.
        /// </value>
        [ParameterName("n"), ParameterInfo("Series multiplier")]
        [GreaterThanOrEquals(0)]
        public double SeriesMultiplier
        {
            get => _seriesMultiplier;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(SeriesMultiplier), 0);
                _seriesMultiplier = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseParameters"/> class.
        /// </summary>
        /// <param name="inductance">Inductor</param>
        public BaseParameters(double inductance)
        {
            Inductance = inductance;
        }
    }
}
