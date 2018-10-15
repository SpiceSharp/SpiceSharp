using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DelayBehaviors
{
    public class BaseParameters : ParameterSet
    {
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay.")]
        public double Delay { get; set; }

        public double RelativeTolerance { get; set; }

        public double AbsoluteTolerance { get; set; }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <exception cref="CircuitException">Non-causal delay detected.</exception>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            if (Delay < 0.0)
                throw new CircuitException("Non-causal delay detected.");
        }
    }
}
