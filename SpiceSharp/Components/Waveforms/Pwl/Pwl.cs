using SpiceSharp.ParameterSets;
using SpiceSharp.Simulations;
using System;
using System.Collections.Generic;
using SpiceSharp.Attributes;
using System.Linq;
using SpiceSharp.Entities;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Piecewise linear waveform.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    /// <seealso cref="IWaveformDescription" />
    [GeneratedParameters]
    public partial class Pwl : ParameterSet,
        IWaveformDescription
    {
        /// <summary>
        /// Gets or sets the waveform points.
        /// </summary>
        /// <value>
        /// The waveform points
        /// </value>
        [ParameterName("points"), ParameterInfo("The points of the waveform")]
        public IEnumerable<Point> Points { get; set; }

        /// <summary>
        /// Sets the waveform points using a vector sequence of times and values.
        /// </summary>
        /// <param name="vector">The array of alternating timepoints and values.</param>
        [ParameterName("points"), ParameterInfo("The vector of time and point values")]
        public void SetPoints(params double[] vector)
        {
            vector.ThrowIfNull(nameof(vector));
            int n = Math.Max(vector.Length / 2, 2);
            vector.ThrowIfNotLength(nameof(vector), n * 2);
            Point[] arr = new Point[n];
            for (int i = 0; i < n; i++)
                arr[i] = new Point(vector[i * 2], vector[i * 2 + 1]);
            Points = arr;
        }

        /// <inheritdoc/>
        public IWaveform Create(IBindingContext context)
        {
            IIntegrationMethod method = null;
            context?.TryGetState<IIntegrationMethod>(out method);
            return new Instance(Points, method);
        }

        /// <summary>
        /// Returns a string that represents the current piece-wise linear waveform.
        /// </summary>
        /// <returns>
        /// A string that represents the current piece-wise linear waveform.
        /// </returns>
        public override string ToString()
        {
            if (Points != null)
                return "pwl({0})".FormatString(string.Join(", ", Points));
            return "pwl(null)";
        }

        /// <inheritdoc/>
        public IWaveformDescription Clone()
        {
            return new Pwl()
            {
                Points = (Point[])Points.ToArray().Clone()
            };
        }
    }
}
