using System;
using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.VoltagesourceModel
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
        public Parameter VSRCacMag { get; } = new Parameter();
        [PropertyName("acphase"), PropertyInfo("A.C. Phase")]
        public Parameter VSRCacPhase { get; } = new Parameter();
        [PropertyName("ac"), PropertyInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            if (ac == null)
                throw new ArgumentNullException(nameof(ac));

            switch (ac?.Length ?? -1)
            {
                case 2: VSRCacPhase.Set(ac[1]); goto case 1;
                case 1: VSRCacMag.Set(ac[0]); break;
                case 0: VSRCacMag.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }
    }
}
