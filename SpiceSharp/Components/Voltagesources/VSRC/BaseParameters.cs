using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VSRC
{
    /// <summary>
    /// Parameters for a <see cref="Voltagesource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        public Waveform VSRCwaveform { get; set; }
        [NameAttribute("dc"), InfoAttribute("D.C. source value")]
        public Parameter VSRCdcValue { get; } = new Parameter();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dc">DC value</param>
        public BaseParameters(double dc)
        {
            VSRCdcValue.Set(dc);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="w">Waveform</param>
        public BaseParameters(Waveform w)
        {
            VSRCwaveform = w;
        }
    }
}
