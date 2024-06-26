using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet2Model" />
    /// </summary>
    /// <seealso cref="Mosfets.ModelParameters" />
    [GeneratedParameters]
    public partial class ModelParameters : Mosfets.ModelParameters, ICloneable<ModelParameters>
    {
        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        /// <value>
        /// The channel length modulation parameter.
        /// </value>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _lambda;

        /// <summary>
        /// Gets or sets the width effect on the threshold voltage.
        /// </summary>
        /// <value>
        /// The width effect on the threshold voltage.
        /// </value>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        [Finite]
        private GivenParameter<double> _narrowFactor;

        /// <summary>
        /// Gets or sets the critical field for mobility degradation.
        /// </summary>
        /// <value>
        /// The critical field for mobility degradation.
        /// </value>
        [ParameterName("ucrit"), ParameterInfo("Crit. field for mob. degradation")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _criticalField = new(1e4, false);

        /// <summary>
        /// Gets or sets the critical field exponent for mobility degradation.
        /// </summary>
        /// <value>
        /// The critical field exponent for mobility degradation.
        /// </value>
        [ParameterName("uexp"), ParameterInfo("Crit. field exp for mob. deg.")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _criticalFieldExp;

        /// <summary>
        /// Gets the total channel charge coefficient.
        /// </summary>
        /// <value>
        /// The total channel charge coefficient.
        /// </value>
        [ParameterName("neff"), ParameterInfo("Total channel charge coeff.")]
        [GreaterThan(0), Finite]
        private GivenParameter<double> _channelCharge = new(1, false);

        /// <summary>
        /// Gets the fast surface state density.
        /// </summary>
        /// <value>
        /// The fast surface state density.
        /// </value>
        [ParameterName("nfs"), ParameterInfo("Fast surface state density")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _fastSurfaceStateDensity;

        /// <summary>
        /// Gets the maximum drift velocity.
        /// </summary>
        /// <value>
        /// The maximum drift velocity.
        /// </value>
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        [Finite]
        private GivenParameter<double> _maxDriftVelocity;

        /// <summary>
        /// Gets the junction depth.
        /// </summary>
        /// <value>
        /// The junction depth.
        /// </value>
        [ParameterName("xj"), ParameterInfo("Junction depth")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _junctionDepth;

        /// <inheritdoc/>
        ModelParameters ICloneable<ModelParameters>.Clone() => (ModelParameters)Clone();
    }
}
