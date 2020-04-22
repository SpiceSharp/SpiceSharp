using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// Base parameters for a <see cref="Delay" />.
    /// </summary>
    /// <seealso cref="ParameterSet" />
    [GeneratedParameters]
    public class BaseParameters : ParameterSet
    {
        private double _absoluteTolerance = 1.0;
        private double _relativeTolerance = 1.0;
        private double _delay;

        /// <summary>
        /// Gets or sets the delay in seconds.
        /// </summary>
        [ParameterName("delay"), ParameterName("td"), ParameterInfo("The delay.", Units = "s")]
        [GreaterThanOrEquals(0)]
        public double Delay
        {
            get => _delay;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(Delay), 0);
                _delay = value;
            }
        }

        /// <summary>
        /// Gets or sets the relative tolerance to determine when a breakpoint (heavy nonlinear behavior occurs) needs to be added.
        /// </summary>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0)]
        public double RelativeTolerance
        {
            get => _relativeTolerance;
            set
            {
                Utility.GreaterThan(value, nameof(RelativeTolerance), 0);
                _relativeTolerance = value;
            }
        }

        /// Gets or sets the absolute tolerance to determine when a breakpoint (heavy nonlinear behavior occurs) needs to be added.
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance used to decide on adding a breakpoint.")]
        [GreaterThan(0)]
        public double AbsoluteTolerance
        {
            get => _absoluteTolerance;
            set
            {
                Utility.GreaterThan(value, nameof(AbsoluteTolerance), 0);
                _absoluteTolerance = value;
            }
        }

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        public override void CalculateDefaults()
        {
        }
    }
}
