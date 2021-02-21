using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.VoltageControlledCurrentSources
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class Parameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the transconductance gain.
        /// </summary>
        /// <value>
        /// The transconductance gain..
        /// </value>
        [ParameterName("gain"), ParameterInfo("Transconductance of the source (gain)", Units = "\u03a9^-1")]
        public double Transconductance { get; set; }
    }
}
