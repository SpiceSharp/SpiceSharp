using SpiceSharp.Attributes;
using SpiceSharp.Diagnostics;

namespace SpiceSharp.Components.ISRC
{
    /// <summary>
    /// AC parameters for a <see cref="Currentsource"/>
    /// </summary>
    public class AcParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [NameAttribute("acmag"), InfoAttribute("A.C. magnitude value")]
        public Parameter ISRCacMag { get; } = new Parameter();
        [NameAttribute("acphase"), InfoAttribute("A.C. phase value")]
        public Parameter ISRCacPhase { get; } = new Parameter();
        [NameAttribute("ac"), InfoAttribute("A.C. magnitude, phase vector")]
        public void SetAc(double[] ac)
        {
            switch (ac.Length)
            {
                case 2: ISRCacPhase.Set(ac[1]); goto case 1;
                case 1: ISRCacMag.Set(ac[0]); break;
                case 0: ISRCacMag.Set(0.0); break;
                default:
                    throw new BadParameterException("ac");
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public AcParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mag">Magnitude</param>
        /// <param name="ph">Phase</param>
        public AcParameters(double mag, double ph)
        {
            ISRCacMag.Set(mag);
            ISRCacPhase.Set(ph);
        }
    }
}
