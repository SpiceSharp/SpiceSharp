using SpiceSharp.Attributes;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Class that describes a sweep with a number of points per octave.
    /// </summary>
    /// <seealso cref="IEnumerable{T}" />
    public class OctaveSweep : IEnumerable<double>
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
                if (value <= 0)
                    throw new BadParameterException(nameof(Initial), value, Properties.Resources.Simulations_Frequency_InitialFrequencyTooSmall);
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
                if (value <= 0)
                    throw new BadParameterException(nameof(Final), value, Properties.Resources.Simulations_Frequency_FinalFrequencyTooSmall);
            }
        }
        private double _final;

        /// <summary>
        /// Gets or sets the points per decade.
        /// </summary>
        /// <value>
        /// The points per decade.
        /// </value>
        public int PointsPerOctave
        {
            get => _ppo;
            set
            {
                if (value <= 0)
                    throw new BadParameterException(nameof(PointsPerOctave), value, Properties.Resources.Simulations_Frequency_PointsPerDecadeTooSmall);
                _ppo = value;
            }
        }
        private int _ppo = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="OctaveSweep"/> class.
        /// </summary>
        /// <param name="initial">The initial value.</param>
        /// <param name="final">The final value.</param>
        /// <param name="pointsPerOctave">The number of points per octave.</param>
        public OctaveSweep(double initial, double final, int pointsPerOctave)
        {
            Initial = initial;
            Final = final;
            PointsPerOctave = pointsPerOctave;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<double> GetEnumerator()
        {
            if (Final < Initial)
                throw new BadParameterException(nameof(Final), Final, Properties.Resources.Simulations_Frequency_FinalFrequencySmallerThanInitial);
            var delta = Math.Exp(Math.Log(2.0) / _ppo);
            double current = Initial;
            double stop = Final * Math.Sqrt(delta);
            while (current <= stop)
            {
                yield return current;
                current *= delta;
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
