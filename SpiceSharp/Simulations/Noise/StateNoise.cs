using System;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that represents noise
    /// </summary>
    public class StateNoise
    {
        /// <summary>
        /// Private variables
        /// </summary>
        double gainsqinv, freq, lstFreq, lnLastFreq, delFreq, delLnFreq, lnFreq;

        /// <summary>
        /// Current frequency point
        /// </summary>
        public double Freq
        {
            get => freq;
            set
            {
                // Shift current frequency to last frequency
                lstFreq = freq;
                lnLastFreq = lnFreq;

                // Update new values
                freq = value;
                lnFreq = Math.Log(Math.Max(freq, 1e-38));

                // Delta
                delFreq = freq - lstFreq;
                delLnFreq = lnFreq - lnLastFreq;
            }
        }

        /// <summary>
        /// Get the frequency step
        /// </summary>
        public double DelFreq { get => delFreq; }

        /// <summary>
        /// Output referred noise
        /// </summary>
        public double outNoiz = 0.0;

        /// <summary>
        /// Input referred noise
        /// </summary>
        public double inNoise = 0.0;

        /// <summary>
        /// Output noise density
        /// </summary>
        public double outNdens = 0.0;

        /// <summary>
        /// Gets or sets the inverse squared gain
        /// </summary>
        public double GainSqInv
        {
            get => gainsqinv;
            set
            {
                gainsqinv = value;
                LnGainInv = Math.Log(value);
            }
        }

        /// <summary>
        /// Gets the logarithm of the gain squared
        /// </summary>
        public double LnGainInv { get; private set; } = 0.0;

        /// <summary>
        /// Initialize
        /// </summary>
        /// <param name="freq">Starting frequency</param>
        public void Initialize(double freq)
        {
            this.freq = freq;
            lstFreq = freq;

            outNoiz = 0.0;
            inNoise = 0.0;
        }
        
        /// <summary>
        /// This subroutine evaluate the integratl of the function
        /// NOISE = a * (FREQUENCY) ^ (EXPONENT)
        /// given two points from the curve. If EXPONENT is relatively close to 0, the noise is simply multiplied
        /// by the change in frequency.
        /// If it isn't, a more complicated expression must be used.
        /// Note that EXPONENT = -1 gives a different equation than EXPONENT != -1.
        /// </summary>
        /// <param name="noizDens">Noise density</param>
        /// <param name="lnNdens">Last noise density</param>
        /// <param name="lnNlstDens">Last log noise density</param>
        /// <returns></returns>
        public double Integrate(double noizDens, double lnNdens, double lnNlstDens)
        {
            double exponent = (lnNdens - lnNlstDens) / delLnFreq;
            if (Math.Abs(exponent) < 1e-10)
                return noizDens * delFreq;
            else
            {
                double a = Math.Exp(lnNdens - exponent * lnFreq);
                exponent += 1.0;
                if (Math.Abs(exponent) < 1e-10)
                    return a * (lnFreq - lnLastFreq);
                else
                    return a * (Math.Exp(exponent * lnFreq) - Math.Exp(exponent * lnLastFreq)) / exponent;
            }
        }
    }
}
