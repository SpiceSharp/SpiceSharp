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
        [ParameterName("acmag"), ParameterInfo("A.C. magnitude value")]
        public GivenParameter<double> AcMagnitude { get; } = new GivenParameter<double>();

        /// <summary>
        /// Small-signal phase.
        /// </summary>
        [ParameterName("acphase"), ParameterInfo("A.C. phase value")]
        public GivenParameter<double> AcPhase { get; } = new GivenParameter<double>();

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
                    AcPhase.Value = ac[1];
                    goto case 1;
                case 1:
                    AcMagnitude.Value = ac[0];
                    break;
                case 0:
                    AcMagnitude.Value = 0.0;
                    break;
                default:
                    throw new BadParameterException("ac");
            }
        }

        /// <summary>
        /// Gets the phasor represented by the amplitude and phase.
        /// </summary>
        public Complex Phasor { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="IndependentSourceFrequencyParameters"/> class.
        /// </summary>
        public IndependentSourceFrequencyParameters()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IndependentSourceFrequencyParameters"/> class.
        /// </summary>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="phase">Phase</param>
        public IndependentSourceFrequencyParameters(double magnitude, double phase)
        {
            AcMagnitude.Value = magnitude;
            AcPhase.Value = phase;
        }

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        public override ParameterSet Clone()
        {
            var result = (IndependentSourceFrequencyParameters) base.Clone();
            result.Phasor = Phasor;
            return result;
        }

        /// <summary>
        /// Copy parameters.
        /// </summary>
        /// <param name="source">The source object.</param>
        public override void CopyFrom(ParameterSet source)
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
