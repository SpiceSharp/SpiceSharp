using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.VSRC
{
    /// <summary>
    /// Parameters for AC analysis
    /// </summary>
    public class AcParameters : Parameters
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [SpiceName("acmag"), SpiceInfo("A.C. Magnitude")]
        public Parameter VSRCacMag { get; } = new Parameter();
        [SpiceName("acphase"), SpiceInfo("A.C. Phase")]
        public Parameter VSRCacPhase { get; } = new Parameter();
        [SpiceName("ac"), SpiceInfo("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
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
