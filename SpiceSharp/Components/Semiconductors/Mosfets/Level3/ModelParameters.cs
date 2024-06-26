using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Mosfets.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet3Model" />
    /// </summary>
    /// <seealso cref="Mosfets.ModelParameters" />
    [GeneratedParameters]
    public partial class ModelParameters : Mosfets.ModelParameters, ICloneable<ModelParameters>
    {
        /// <summary>
        /// The possible versions used for the implementation.
        /// </summary>
        public enum Versions
        {
            /// <summary>
            /// The version as implemented by Spice 3f5.
            /// </summary>
            /// <remarks>
            /// Some small changes still apply vs Spice 3f5. Some small bugs were fixed, and
            /// some extra parameters will still apply (like <see cref="Parameters.ParallelMultiplier"/>),
            /// but the underlying model equations remain unchanged.
            /// </remarks>
            Spice3f5,

            /// <summary>
            /// The version as implemented by ngSpice.
            /// </summary>
            NgSpice
        }

        /// <summary>
        /// Gets or sets the version that needs to be used.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Versions Version { get; set; } = Versions.Spice3f5;

        /// <summary>
        /// Gets or sets a flag that uses legacy model for MOS3 if <c>true</c>.
        /// </summary>
        /// <value>
        /// Flag indicating whether or not the legacy model needs to be used.
        /// </value>
        public bool BadMos { get; set; }

        /// <summary>
        /// Gets or sets the drain-source voltage dependence of the threshold voltage.
        /// </summary>
        /// <value>
        /// The drain-source voltage dependence of the threshold voltage.
        /// </value>
        [ParameterName("eta"), ParameterInfo("Vds dependence of threshold voltage")]
        [Finite]
        private GivenParameter<double> _eta;

        /// <summary>
        /// Gets the kappa parameter.
        /// </summary>
        /// <value>
        /// The kappa-parameter.
        /// </value>
        [ParameterName("kappa"), ParameterInfo("Kappa")]
        [Finite]
        private GivenParameter<double> _kappa = new(0.2, false);

        /// <summary>
        /// Gets or sets the gate-source voltage dependence on mobility.
        /// </summary>
        /// <value>
        /// The gate-source voltage dependence on mobility.
        /// </value>
        [ParameterName("theta"), ParameterInfo("Vgs dependence on mobility")]
        [Finite]
        private GivenParameter<double> _theta;

        /// <summary>
        /// Gets or sets the fast surface state density.
        /// </summary>
        /// <value>
        /// The fast surface state density.
        /// </value>
        [ParameterName("nfs"), ParameterInfo("Fast surface state density")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _fastSurfaceStateDensity;

        /// <summary>
        /// Gets or sets the maximum drift velocity.
        /// </summary>
        /// <value>
        /// The maximum drift velocity.
        /// </value>
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        [Finite]
        private GivenParameter<double> _maxDriftVelocity;

        /// <summary>
        /// Gets or sets the junction depth.
        /// </summary>
        /// <value>
        /// The junction depth.
        /// </value>
        [ParameterName("xj"), ParameterInfo("Junction depth")]
        [GreaterThanOrEquals(0), Finite]
        private GivenParameter<double> _junctionDepth;

        /// <summary>
        /// Gets or sets the width effect on the threshold voltage.
        /// </summary>
        /// <value>
        /// The width effect on the threshold voltage.
        /// </value>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        [Finite]
        private double _delta;

        /// <summary>
        /// Gets or sets the length mask adjustment.
        /// </summary>
        /// <value>
        /// The length mask adjustment.
        /// </value>
        [ParameterName("xl"), ParameterInfo("Length mask adjustment", Units = "m")]
        [Finite]
        private double _lengthAdjust;

        /// <summary>
        /// Gets or sets the width narrowing due to diffusion.
        /// </summary>
        /// <value>
        /// The width narrowing.
        /// </value>
        [ParameterName("wd"), ParameterInfo("Width narrowing due to diffusion", Units = "m")]
        [Finite]
        private double _widthNarrow;

        /// <summary>
        /// Gets or sets the width mask adjustment.
        /// </summary>
        /// <value>
        /// The width mask adjustment.
        /// </value>
        [ParameterName("xw"), ParameterInfo("Width mask adjustment", Units = "m")]
        [Finite]
        private double _widthAdjust;

        /// <summary>
        /// Gets or sets the threshold voltage adjustment.
        /// </summary>
        /// <value>
        /// The threshold voltage adjustment.
        /// </value>
        [ParameterName("delvt0"), ParameterInfo("Threshold voltage adjust")]
        [Finite]
        private double _delVt0;

        /// <inheritdoc/>
        ModelParameters ICloneable<ModelParameters>.Clone()
            => (ModelParameters)Clone();
    }
}
