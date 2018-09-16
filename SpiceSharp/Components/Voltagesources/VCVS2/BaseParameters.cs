using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.VoltageControlledVoltageSourceBehaviors2
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledVoltageSource2"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Voltage gain")]
        public GivenParameter<Func<double>> Coefficient { get; } = new GivenParameter<Func<double>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters() { }
    }
}
