using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Delay" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay.")]
        public double Delay { get; set; }

        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        public double RelativeTolerance { get; set; } = 1.0;

        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        public double AbsoluteTolerance { get; set; } = 1.0;

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
                throw new CircuitException("Non-causal delay {0:e3} detected. Delays should be larger than 0.".FormatString(Delay));
            if (RelativeTolerance <= 0.0)
                throw new CircuitException("Relative tolerance {0:e3} should be larger than 0.".FormatString(RelativeTolerance));
            if (AbsoluteTolerance <= 0.0)
                throw new CircuitException("Absolute tolerance {0:e3} should be larger than 0.".FormatString(AbsoluteTolerance));
        }
    }
}
