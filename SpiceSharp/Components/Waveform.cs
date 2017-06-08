using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharp.Components
{
    /// <summary>
    /// This class represents a waveform object
    /// </summary>
    public abstract class Waveform
    {

        /// <summary>
        /// Setup the waveform
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public abstract void Setup(Circuit ckt);

        /// <summary>
        /// Calculate the value of the waveform at a specific value
        /// </summary>
        /// <param name="time">The time point</param>
        /// <returns></returns>
        public abstract double At(double time);

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt"></param>
        public abstract void Accept(Circuit ckt);
    }
}
