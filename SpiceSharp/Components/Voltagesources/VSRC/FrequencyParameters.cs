using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.VoltagesourceBehaviors
{
    /// <summary>
    /// Parameters for AC analysis
    /// </summary>
    public class FrequencyParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("acmag"), ParameterInfo("A.C. Magnitude")]
        public Parameter AcMagnitude { get; } = new Parameter();
        [ParameterName("acphase"), ParameterInfo("A.C. Phase")]
        public Parameter AcPhase { get; } = new Parameter();
        [ParameterName("ac"), ParameterInfo("A.C. magnitude, phase vector")]
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
    }
}
