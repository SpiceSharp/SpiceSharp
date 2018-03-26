using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations
{
    /// <summary>
    /// A class that describes a job
    /// </summary>
    public class SweepConfiguration
    {
        /// <summary>
        /// Starting value
        /// </summary>
        [ParameterName("start"), ParameterInfo("The starting value")]
        public double Start { get; set; }

        /// <summary>
        /// Ending value
        /// </summary>
        [ParameterName("stop"), ParameterInfo("The stopping value")]
        public double Stop { get; set; }

        /// <summary>
        /// Value step
        /// </summary>
        [ParameterName("step"), ParameterInfo("The step")]
        public double Step { get; set; }

        /// <summary>
        /// The name of the source being varied
        /// </summary>
        [ParameterName("source"), ParameterInfo("The name of the swept source")]
        public Identifier ComponentName { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">The name of the source to sweep</param>
        /// <param name="start">The starting value</param>
        /// <param name="stop">The stopping value</param>
        /// <param name="step">The step value</param>
        public SweepConfiguration(Identifier name, double start, double stop, double step)
        {
            ComponentName = name;
            Start = start;
            Stop = stop;
            Step = step;
        }
    }
}
