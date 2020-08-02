using SpiceSharp.ParameterSets;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A configuration for a <see cref="Noise"/> analysis.
    /// </summary>
    public class NoiseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the noise output node name.
        /// </summary>
        [ParameterName("output"), ParameterInfo("Noise output summation node")]
        public string Output { get; set; }

        /// <summary>
        /// Gets or sets the noise output reference node name.
        /// </summary>
        [ParameterName("outputref"), ParameterInfo("Noise output reference node")]
        public string OutputRef { get; set; }

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
    }
}
