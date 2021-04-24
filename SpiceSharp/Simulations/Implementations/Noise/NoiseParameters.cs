using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="Noise"/> analysis.
    /// </summary>
    /// <seealso cref="ParameterSet"/>
    [GeneratedParameters]
    public partial class NoiseParameters : ParameterSet, ICloneable<NoiseParameters>
    {
        /// <summary>
        /// Gets or sets the noise output node name.
        /// </summary>
        /// <value>
        /// The noise output node name.
        /// </value>
        [ParameterName("output"), ParameterInfo("Noise output summation node")]
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets the noise output reference node name.
        /// </summary>
        /// <value>
        /// The noise output reference node name.
        /// </value>
        [ParameterName("outputref"), ParameterInfo("Noise output reference node")]
        public string OutputRef { get; set; }

        /// <summary>
        /// Gets or sets the name of the input source.
        /// </summary>
        /// <value>
        /// The name of the input source.
        /// </value>
        [ParameterName("input"), ParameterInfo("Source that acts as the input")]
        public string InputSource { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseParameters"/> class.
        /// </summary>
        public NoiseParameters()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseParameters"/> class.
        /// </summary>
        /// <param name="output">The output node name.</param>
        /// <param name="reference">The reference node name.</param>
        public NoiseParameters(string output, string reference)
        {
            Output = output;
            OutputRef = reference;
        }

        /// <inheritdoc/>
        public NoiseParameters Clone()
            => (NoiseParameters)MemberwiseClone();
    }
}
