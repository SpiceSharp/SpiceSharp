using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="Noise"/> analysis.
    /// </summary>
    public class NoiseConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the noise output node identifier.
        /// </summary>
        [ParameterName("output"), ParameterInfo("Noise output summation node")]
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets the noise output reference node identifier.
        /// </summary>
        [ParameterName("outputref"), ParameterInfo("Noise output reference node")]
        public string OutputRef { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the AC source used as input reference.
        /// </summary>
        [ParameterName("input"), ParameterInfo("Name of the AC source used as input reference")]
        public string Input { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseConfiguration"/> class.
        /// </summary>
        public NoiseConfiguration()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NoiseConfiguration"/> class.
        /// </summary>
        /// <param name="output">The output node identifier.</param>
        /// <param name="reference">The reference node identifier.</param>
        /// <param name="input">The input source identifier.</param>
        public NoiseConfiguration(string output, string reference, string input)
        {
            Output = output;
            OutputRef = reference;
            Input = input;
        }
    }
}
