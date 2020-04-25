using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentControlledCurrentSourceBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="CurrentControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gain of the source.
        /// </summary>
        [ParameterName("gain"), ParameterInfo("Gain of the source")]
        public double Coefficient { get; set; }
    }
}
