using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// Configuration for a <see cref="Noise"/> analysis
    /// </summary>
    public class NoiseConfiguration : ParameterSet
    {
        /// <summary>
        /// Gets or sets the noise output node
        /// </summary>
        [PropertyName("output"), PropertyInfo("Noise output summation node")]
        public Identifier Output { get; set; }

        /// <summary>
        /// Gets or sets the noise output reference node
        /// </summary>
        [PropertyName("outputref"), PropertyInfo("Noise output reference node")]
        public Identifier OutputRef { get; set; }

        /// <summary>
        /// Gets or sets the name of the AC source used as input reference
        /// </summary>
        [PropertyName("input"), PropertyInfo("Name of the AC source used as input reference")]
        public Identifier Input { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public NoiseConfiguration()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="output">Output</param>
        /// <param name="reference">Output reference</param>
        /// <param name="input">Input</param>
        public NoiseConfiguration(Identifier output, Identifier reference, Identifier input)
        {
            Output = output;
            OutputRef = reference;
            Input = input;
        }
    }
}
