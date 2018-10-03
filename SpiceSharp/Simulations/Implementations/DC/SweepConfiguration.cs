using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes a sweep.
    /// </summary>
    public class SweepConfiguration
    {
        /// <summary>
        /// Gets or sets the starting value.
        /// </summary>
        /// <value>
        /// The starting value.
        /// </value>
        [ParameterName("start"), ParameterInfo("The starting value")]
        public double Start { get; set; }

        /// <summary>
        /// Gets or sets the final value.
        /// </summary>
        /// <value>
        /// The stopping value.
        /// </value>
        [ParameterName("stop"), ParameterInfo("The stopping value")]
        public double Stop { get; set; }

        /// <summary>
        /// Gets or sets the stepping value.
        /// </summary>
        /// <value>
        /// The stepping value.
        /// </value>
        [ParameterName("step"), ParameterInfo("The step")]
        public double Step { get; set; }

        /// <summary>
        /// The name of the source being varied.
        /// </summary>
        /// <value>
        /// The name of the source.
        /// </value>
        [ParameterName("source"), ParameterInfo("The name of the swept source")]
        public string ComponentName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SweepConfiguration"/> class.
        /// </summary>
        /// <param name="name">The name of the source to sweep.</param>
        /// <param name="start">The starting value.</param>
        /// <param name="stop">The stopping value.</param>
        /// <param name="step">The step value.</param>
        public SweepConfiguration(string name, double start, double stop, double step)
        {
            ComponentName = name;
            Start = start;
            Stop = stop;
            Step = step;
        }
    }
}
