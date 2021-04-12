﻿using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledVoltageSources
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledVoltageSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the voltage gain.
        /// </summary>
        /// <value>
        /// The voltage gain.
        /// </value>
        [ParameterName("gain"), ParameterInfo("Voltage gain")]
        public double Coefficient { get; set; }
    }
}
