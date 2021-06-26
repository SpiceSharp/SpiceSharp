using SpiceSharp.ParameterSets;

namespace SpiceSharpTest.DiodeBehaviors
{
    /// <summary>
    /// Parameters used for our diode model.
    /// </summary>
    public class DiodeParameters : ParameterSet<DiodeParameters>
    {
        /// <summary>
        /// Gets or sets the ideality factor eta.
        /// </summary>
        public double Eta { get; set; }

        /// <summary>
        /// Gets or sets the saturation current.
        /// </summary>
        public double Iss { get; set; }
    }
}
