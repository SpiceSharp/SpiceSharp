namespace SpiceSharp.Components.Noise
{
    /// <summary>
    /// Thermal noise generator
    /// </summary>
    public class NoiseThermal : NoiseGenerator
    {
        /// <summary>
        /// Gets or sets the gain of the thermal noise
        /// The noise is 4 * k * T * G
        /// </summary>
        public double Conductance { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of the noise source</param>
        public NoiseThermal(string name, int a, int b) : base(name, a, b) { }

        /// <summary>
        /// Set the parameters for the thermal noise
        /// </summary>
        /// <param name="values">Values</param>
        public override void Set(params double[] values)
        {
            Conductance = values[0];
        }

        /// <summary>
        /// Calculate the noise quantity
        /// </summary>
        /// <param name="ckt">Circuit</param>
        /// <param name="param">The conductance of a resistor</param>
        /// <returns></returns>
        protected override double CalculateNoise(Circuit ckt)
        {
            var sol = ckt.State.Complex.Solution;
            var val = sol[NOISEnodes[0]] - sol[NOISEnodes[1]];
            double gain = val.Real * val.Real + val.Imaginary * val.Imaginary;
            return 4.0 * Circuit.CONSTBoltz * ckt.State.Temperature * Conductance * gain;
        }
    }
}
