using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.VoltageControlledCurrentSourceBehaviors2
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentSource"/>
    /// </summary>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Transconductance of the source (gain)")]
        public GivenParameter<Func<double>> Coefficient { get; } = new GivenParameter<Func<double>>();

        /// <summary>
        /// Constructor
        /// </summary>
        public BaseParameters()
        {
        }
    }
}
