using SpiceSharp.ParameterSets;

namespace SpiceSharp.Components.Mosfets.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet3Model" />
    /// </summary>
    /// <seealso cref="Mosfets.ModelParameters" />
    [GeneratedParameters]
    public class ModelParameters : Mosfets.ModelParameters
    {
        private GivenParameter<double> _junctionDepth;
        private GivenParameter<double> _fastSurfaceStateDensity;

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
        public GivenParameter<double> Eta { get; set; }

        /// <summary>
        /// Gets the kappa parameter.
        /// </summary>
        /// <value>
        /// The kappa-parameter.
        /// </value>
        [ParameterName("kappa"), ParameterInfo("Kappa")]
        public GivenParameter<double> Kappa { get; set; } = new GivenParameter<double>(0.2, false);

        /// <summary>
        /// Gets or sets the gate-source voltage dependence on mobility.
        /// </summary>
        /// <value>
        /// The gate-source voltage dependence on mobility.
        /// </value>
        [ParameterName("theta"), ParameterInfo("Vgs dependence on mobility")]
        public GivenParameter<double> Theta { get; set; }

        /// <summary>
        /// Gets or sets the fast surface state density.
        /// </summary>
        /// <value>
        /// The fast surface state density.
        /// </value>
        [ParameterName("nfs"), ParameterInfo("Fast surface state density")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> FastSurfaceStateDensity
        {
            get => _fastSurfaceStateDensity;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(FastSurfaceStateDensity), 0);
                _fastSurfaceStateDensity = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum drift velocity.
        /// </summary>
        /// <value>
        /// The maximum drift velocity.
        /// </value>
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        public GivenParameter<double> MaxDriftVelocity { get; set; }

        /// <summary>
        /// Gets or sets the junction depth.
        /// </summary>
        /// <value>
        /// The junction depth.
        /// </value>
        [ParameterName("xj"), ParameterInfo("Junction depth")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> JunctionDepth
        {
            get => _junctionDepth;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(JunctionDepth), 0);
                _junctionDepth = value;
            }
        }

        /// <summary>
        /// Gets or sets the width effect on the threshold voltage.
        /// </summary>
        /// <value>
        /// The width effect on the threshold voltage.
        /// </value>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        public double Delta { get; set; }

        /// <summary>
        /// Gets or sets the length mask adjustment.
        /// </summary>
        /// <value>
        /// The length mask adjustment.
        /// </value>
        [ParameterName("xl"), ParameterInfo("Length mask adjustment", Units = "m")]
        public double LengthAdjust { get; set; }

        /// <summary>
        /// Gets or sets the width narrowing due to diffusion.
        /// </summary>
        /// <value>
        /// The width narrowing.
        /// </value>
        [ParameterName("wd"), ParameterInfo("Width narrowing due to diffusion", Units = "m")]
        public double WidthNarrow { get; set; }

        /// <summary>
        /// Gets or sets the width mask adjustment.
        /// </summary>
        /// <value>
        /// The width mask adjustment.
        /// </value>
        [ParameterName("xw"), ParameterInfo("Width mask adjustment", Units = "m")]
        public double WidthAdjust { get; set; }

        /// <summary>
        /// Gets or sets the threshold voltage adjustment.
        /// </summary>
        /// <value>
        /// The threshold voltage adjustment.
        /// </value>
        [ParameterName("delvt0"), ParameterInfo("Threshold voltage adjust")]
        public double DelVt0 { get; set; }

        /// <inheritdoc/>
        protected override ICloneable Clone()
        {
            // We have some private/protected properties that need to be set manually.
            var result = (ModelParameters)base.Clone();
            result.Delta = Delta;
            return result;
        }

        /// <inheritdoc/>
        protected override void CopyFrom(ICloneable source)
        {
            base.CopyFrom(source);
            Delta = ((ModelParameters)source).Delta;
        }
    }
}
