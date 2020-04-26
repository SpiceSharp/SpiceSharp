using SpiceSharp.Attributes;

namespace SpiceSharp.Components
{
    /// <summary>
    /// Base parameters for a <see cref="VoltageControlledCurrentSource"/>
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    public class VoltageControlledCurrentSourceParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the transconductance gain.
        /// </summary>
        /// <value>
        /// The transconductance gain..
        /// </value>
        [ParameterName("gain"), ParameterInfo("Transconductance of the source (gain)", Units = "\u03a9^-1")]
        public double Transconductance { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VoltageControlledCurrentSourceParameters"/> class.
        /// </summary>
        public VoltageControlledCurrentSourceParameters()
        {
        }
    }
}
