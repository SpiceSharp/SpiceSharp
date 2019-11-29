using SpiceSharp.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that implements a linear sweep.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    public class LinearSweep : IEnumerable<double>
    {
        /// <summary>
        /// Gets or sets the initial.
        /// </summary>
        /// <value>
        /// The initial frequency value.
        /// </value>
        [ParameterName("start"), ParameterName("initial"), ParameterInfo("The initial frequency of the sweep.")]
        public double Initial
        {
            get => _initial;
            set
            {
                if (value < 0)
                    throw new BadParameterException(nameof(Initial), value, Properties.Resources.Simulations_Frequency_InitialFrequencyTooSmall2);
                _initial = value;
            }
        }
        private double _initial;

        /// <summary>
        /// The final frequency of the sweep.
        /// </summary>
        /// <value>
        /// The final frequency value.
        /// </value>
        [ParameterName("stop"), ParameterName("final"), ParameterInfo("The final frequency of the sweep.")]
        public double Final
        {
            get => _final;
            set
            {
                if (value < 0)
                    throw new BadParameterException(nameof(Final), value, Properties.Resources.Simulations_Frequency_FinalFrequencyTooSmall2);
                _final = value;
            }
        }
        private double _final;

        /// <summary>
        /// Gets or sets the number of points.
        /// </summary>
        /// <value>
        /// The number of points.
        /// </value>
        public int Points
        {
            get => _points;
            set
            {
                if (value < 1)
                    throw new BadParameterException(nameof(Points), value, Properties.Resources.Simulations_Frequency_PointsTooSmall);
                _points = value;
            }
        }
        private int _points;

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="points">The number of points.</param>
        public LinearSweep(double initial, double final, int points)
        {
            Initial = initial;
            Final = final;
            Points = points;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinearSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="delta">The step size.</param>
        public LinearSweep(double initial, double final, double delta)
        {
            Initial = initial;

            // Calculate the number of points to be used
            Points = (int)(Math.Floor((final - Initial) / delta));
            Final = Initial + Points * delta;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<double> GetEnumerator()
        {
            for (var i = 0; i < Points; i++)
            {
                var f = (double)i / (Points - 1);
                yield return Initial * (1 - f) + Final * f;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
