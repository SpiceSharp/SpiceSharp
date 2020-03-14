using System;
using System.Numerics;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CommonBehaviors
{
    /// <summary>
    /// AC parameters for an independent source.
    /// </summary>
    public class IndependentSourceFrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Small-signal magnitude.
        /// </summary>
        [ParameterName("acmag"), ParameterInfo("AC magnitude value")]
        public double AcMagnitude { get; set; }

        /// <summary>
        /// Small-signal phase.
        /// </summary>
        [ParameterName("acphase"), ParameterInfo("AC phase value")]
        public double AcPhase { get; set; }

        /// <summary>
        /// Sets the small-signal parameters of the source.
        /// </summary>
        /// <param name="ac">Parameters.</param>
        [ParameterName("ac"), ParameterInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            ac.ThrowIfNull(nameof(ac));
            switch (ac.Length)
            {
                case 2:
                    AcPhase = ac[1];
                    goto case 1;
                case 1:
                    AcMagnitude = ac[0];
                    break;
                case 0:
                    AcMagnitude = 0.0;
                    break;
                default:
                    throw new BadParameterException(nameof(ac));
            }
        }

        /// <summary>
        /// Gets the phasor represented by the amplitude and phase.
        /// </summary>
        public Complex Phasor { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndependentSourceFrequencyParameters"/> class.
        /// </summary>
        public IndependentSourceFrequencyParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IndependentSourceFrequencyParameters"/> class.
        /// </summary>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="phase">Phase</param>
        public IndependentSourceFrequencyParameters(double magnitude, double phase)
        {
            AcMagnitude = magnitude;
            AcPhase = phase;
        }

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        protected override ICloneable Clone()
        {
            var result = (IndependentSourceFrequencyParameters) base.Clone();
            result.Phasor = Phasor;
            return result;
        }

        /// <summary>
        /// Copy parameters.
        /// </summary>
        /// <param name="source">The source object.</param>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            Phasor = ((IndependentSourceFrequencyParameters)source).Phasor;
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            var phase = AcPhase * Math.PI / 180.0;
            Phasor = new Complex(
                AcMagnitude * Math.Cos(phase),
                AcMagnitude * Math.Sin(phase));
        }
    }
}
