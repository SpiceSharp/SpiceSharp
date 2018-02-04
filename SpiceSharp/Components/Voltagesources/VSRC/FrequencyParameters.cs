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
        [PropertyName("acmag"), PropertyInfo("A.C. Magnitude")]
        public Parameter ACMagnitude { get; } = new Parameter();
        [PropertyName("acphase"), PropertyInfo("A.C. Phase")]
        public Parameter ACPhase { get; } = new Parameter();
        [PropertyName("ac"), PropertyInfo("A.C. magnitude, phase vector")]
        public void SetAC(double[] ac)
        {
            if (ac == null)
                throw new ArgumentNullException(nameof(ac));

            switch (ac?.Length ?? -1)
            {
                case 2: ACPhase.Set(ac[1]); goto case 1;
                case 1: ACMagnitude.Set(ac[0]); break;
                case 0: ACMagnitude.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }
    }
}
