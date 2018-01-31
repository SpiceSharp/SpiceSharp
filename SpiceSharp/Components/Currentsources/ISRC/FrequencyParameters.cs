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
        [PropertyName("acmag"), PropertyInfo("A.C. magnitude value")]
        public Parameter ACMagnitude { get; } = new Parameter();
        [PropertyName("acphase"), PropertyInfo("A.C. phase value")]
        public Parameter ACPhase { get; } = new Parameter();
        [PropertyName("ac"), PropertyInfo("A.C. magnitude, phase vector")]
        public void SetAC(double[] ac)
        {
            if (ac == null)
                throw new ArgumentNullException(nameof(ac));
            switch (ac.Length)
            {
                case 2: ACPhase.Set(ac[1]); goto case 1;
                case 1: ACMagnitude.Set(ac[0]); break;
                case 0: ACMagnitude.Set(0.0); break;
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
            ACMagnitude.Set(magnitude);
            ACPhase.Set(phase);
        }
    }
}
