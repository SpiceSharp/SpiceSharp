using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;
using SpiceSharp.Simulations.Sweeps;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// This class implements a sweep with a number of points per decade.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    [GeneratedParameters]
    public partial class DecadeSweep : GeometricProgression
    {
        // ln(10)
        private const double LogDecade = 2.3025850929940456840179914546844;
        private double _r = 10.0;

        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        /// <value>
        /// The initial frequency value.
        /// </value>
        [ParameterName("start"), ParameterName("initial"), ParameterInfo("The initial frequency of the sweep.")]
        public double Initial { get; set; }

        /// <summary>
        /// The final frequency of the sweep.
        /// </summary>
        /// <value>
        /// The final frequency value.
        /// </value>
        [ParameterName("stop"), ParameterName("final"), ParameterInfo("The final frequency of the sweep.")]
        public double Final { get; set; }

        /// <inheritdoc/>
        protected override double A => Initial;

        /// <inheritdoc/>
        protected override double R => _r;

        /// <inheritdoc/>
        protected override int N
        {
            get
            {
                if (Final.Equals(0.0) || Initial.Equals(0.0))
                    throw new ArgumentException(Properties.Resources.Sweeps_ZeroTarget);
                if (Final > 0 && Initial < 0 || Final < 0 && Initial > 0)
                    throw new ArgumentException(Properties.Resources.Sweeps_Unreachable.FormatString(Final));
                double n = Math.Log(Final / Initial) / Math.Log(_r);
                return (int)Math.Round(n);
            }
        }

        /// <summary>
        /// Gets or sets the points per decade.
        /// </summary>
        /// <value>
        /// The points per decade.
        /// </value>
        [ParameterName("n"), ParameterName("steps"), ParameterInfo("The number of points per decade")]
        [GreaterThan(0)]
        public int PointsPerDecade
        {
            get => (int)Math.Round(LogDecade / Math.Log(_r));
            set => _r = Math.Exp(LogDecade / value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecadeSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="pointsPerDecade">The number of points per decade.</param>
        public DecadeSweep(double initial, double final, int pointsPerDecade)
        {
            Initial = initial;
            Final = final;
            PointsPerDecade = pointsPerDecade;
        }
    }
}
