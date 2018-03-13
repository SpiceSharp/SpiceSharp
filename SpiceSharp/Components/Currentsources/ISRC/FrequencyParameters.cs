using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.CurrentsourceBehaviors
{
    /// <summary>
    /// AC parameters for a <see cref="CurrentSource"/>
    /// </summary>
    public class FrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("acmag"), PropertyInfo("A.C. magnitude value")]
        public Parameter AcMagnitude { get; } = new Parameter();
        [ParameterName("acphase"), PropertyInfo("A.C. phase value")]
        public Parameter AcPhase { get; } = new Parameter();
        [ParameterName("ac"), PropertyInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            if (ac == null)
                throw new ArgumentNullException(nameof(ac));
            switch (ac.Length)
            {
                case 2: AcPhase.Set(ac[1]); goto case 1;
                case 1: AcMagnitude.Set(ac[0]); break;
                case 0: AcMagnitude.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public FrequencyParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="magnitude">Magnitude</param>
        /// <param name="phase">Phase</param>
        public FrequencyParameters(double magnitude, double phase)
        {
            AcMagnitude.Set(magnitude);
            AcPhase.Set(phase);
        }
    }
}
