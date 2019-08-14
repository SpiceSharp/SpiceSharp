using SpiceSharp.Attributes;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Delay" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.ParameterSet" />
    public class BaseParameters : ParameterSet
    {
        /// <summary>
        /// Gets or sets the delay in seconds.
        /// </summary>
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay.")]
        public double Delay { get; set; }

        /// <summary>
        /// Gets or sets the relative tolerance to determine when a breakpoint (heavy nonlinear behavior occurs) needs to be added.
        /// </summary>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        public double RelativeTolerance { get; set; } = 1.0;

        /// Gets or sets the absolute tolerance to determine when a breakpoint (heavy nonlinear behavior occurs) needs to be added.
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        public double AbsoluteTolerance { get; set; } = 1.0;

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
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
