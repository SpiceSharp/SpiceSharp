using System;
using SpiceSharp.Attributes;

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
        /// The permittivity of silicon
        /// </summary>
        private const double _epsilonSilicon = 11.7 * 8.854214871e-12;

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
        /// <remarks>
        /// When setting the parameter, spice 3f5 would change the delta parameter, but when asking
        /// for it, Spice 3f5 would return the narrow factor instead. This behavior is copied here
        /// for compatibility.
        /// </remarks>
        [ParameterName("delta"), ParameterInfo("Width effect on the threshold voltage")]
        public double DeltaWidth
        {
            get => NarrowFactor;
            set => Delta = value;
        }

        /// <summary>
        /// Gets or sets the width effect on the threshold voltage.
        /// </summary>
        /// <value>
        /// The width effect on the threshold voltage.
        /// </value>
        [ParameterName("input_delta"), ParameterInfo("")]
        public double Delta { get; protected set; }

        /// <summary>
        /// Gets or sets the narrowing factor.
        /// </summary>
        /// <value>
        /// The narrowing factor.
        /// </value>
        public double NarrowFactor { get; set; }

        /// <inheritdoc/>
        public override void CalculateDefaults()
        {
            // Calculate base defaults
            base.CalculateDefaults();

            // Calculate the narrowing factor
            NarrowFactor = Delta * 0.5 * Math.PI * _epsilonSilicon / OxideCapFactor;
        }

        /// <inheritdoc/>
        protected override ICloneable Clone()
        {
            // We have some private/protected properties that need to be set manually.
            var result = (ModelParameters)base.Clone();

            // Set properties
            result.Delta = Delta;
            result.NarrowFactor = NarrowFactor; // Just to be sure, given the special nature of "delta"

            return result;
        }
    }
}
