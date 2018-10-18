using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet2Model"/>
    /// </summary>
    public class ModelBaseParameters : Common.ModelBaseParameters
    {
        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        public GivenParameter<double> Lambda { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the width effect on the threshold voltage.
        /// </summary>
        /// <value>
        /// The narrowing factor.
        /// </value>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        public GivenParameter<double> NarrowFactor { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the critical field for mobility degradation.
        /// </summary>
        /// <value>
        /// The critical field.
        /// </value>
        [ParameterName("ucrit"), ParameterInfo("Crit. field for mob. degradation")]
        public GivenParameter<double> CriticalField { get; } = new GivenParameter<double>(1e4);

        /// <summary>
        /// Gets the critical field exponent for mobility degradation.
        /// </summary>
        /// <value>
        /// The critical field exponent.
        /// </value>
        [ParameterName("uexp"), ParameterInfo("Crit. field exp for mob. deg.")]
        public GivenParameter<double> CriticalFieldExp { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the total channel charge coefficient.
        /// </summary>
        /// <value>
        /// The total channel charge coefficient.
        /// </value>
        [ParameterName("neff"), ParameterInfo("Total channel charge coeff.")]
        public GivenParameter<double> ChannelCharge { get; } = new GivenParameter<double>(1);

        /// <summary>
        /// Gets the fast surface state density.
        /// </summary>
        /// <value>
        /// The fast surface state density.
        /// </value>
        [ParameterName("nfs"), ParameterInfo("Fast surface state density")]
        public GivenParameter<double> FastSurfaceStateDensity { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the maximum drift velocity.
        /// </summary>
        /// <value>
        /// The maximum drift velocity.
        /// </value>
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        public GivenParameter<double> MaxDriftVelocity { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the junction depth.
        /// </summary>
        /// <value>
        /// The junction depth.
        /// </value>
        [ParameterName("xj"), ParameterInfo("Junction depth")]
        public GivenParameter<double> JunctionDepth { get; } = new GivenParameter<double>();

        /// <summary>
        /// Method for calculating the default values of derived parameters.
        /// </summary>
        /// <remarks>
        /// These calculations should be run whenever a parameter has been changed.
        /// </remarks>
        public override void CalculateDefaults()
        {
            // Set default oxide thickness
            if (!OxideThickness.Given)
                OxideThickness.RawValue = 1e-7;

            // Calculate base defaults
            base.CalculateDefaults();
        }
    }
}
