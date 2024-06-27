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
    public partial class OctaveSweep : GeometricProgression
    {
        // ln(2)
        private const double LogOctave = 0.69314718055994530941723212145818;
        private double _r = 2.0;

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
                double n = Math.Log(Final / Initial) / Math.Log(_r);
                if (double.IsNaN(n) || double.IsInfinity(n))
                    throw new ArgumentException(Properties.Resources.Sweeps_Unreachable.FormatString(Final));
                return (int)Math.Round(n);
            }
        }

        /// <summary>
        /// Gets or sets the points per decade.
        /// </summary>
        /// <value>
        /// The points per decade.
        /// </value>
        [ParameterName("n"), ParameterName("steps"), ParameterInfo("The number of points per octave")]
        [GreaterThan(0)]
        public int PointsPerOctave
        {
            get => (int)Math.Round(LogOctave / Math.Log(_r));
            set => _r = Math.Exp(LogOctave / value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DecadeSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="pointsPerOctave">The number of points per decade.</param>
        public OctaveSweep(double initial, double final, int pointsPerOctave)
        {
            Initial = initial;
            Final = final;
            PointsPerOctave = pointsPerOctave;
        }
    }
}
