using System;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet3Model"/>
    /// </summary>
    public class ModelBaseParameters : Common.ModelBaseParameters
    {
        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        private const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// Parameters
        /// </summary>
        [ParameterName("eta"), ParameterInfo("Vds dependence of threshold voltage")]
        public GivenParameter<double> Eta { get; } = new GivenParameter<double>();

        /// <summary>
        /// Gets the kappa parameter.
        /// </summary>
        /// <value>
        /// The kappa parameter.
        /// </value>
        [ParameterName("kappa"), ParameterInfo("Kappa")]
        public GivenParameter<double> Kappa { get; } = new GivenParameter<double>(0.2);

        /// <summary>
        /// Gets the gate-source voltage dependence on mobility.
        /// </summary>
        /// <value>
        /// The gate-source voltage dependence.
        /// </value>
        [ParameterName("theta"), ParameterInfo("Vgs dependence on mobility")]
        public GivenParameter<double> Theta { get; } = new GivenParameter<double>();

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
        /// Gets or sets the width effect on the threshold voltage.
        /// </summary>
        /// <value>
        /// The narrowing factor.
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
        /// The delta.
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

        /// <summary>
        /// Constructor
        /// </summary>
        public ModelBaseParameters()
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="nmos">True for NMOS, false for PMOS</param>
        public ModelBaseParameters(bool nmos)
        {
            if (nmos)
                MosfetType = 1.0;
            else
                MosfetType = -1.0;
        }

        /// <summary>
        /// Calculate dependent parameters
        /// </summary>
        public override void CalculateDefaults()
        {
            // Set the default oxide thickness
            if (!OxideThickness.Given)
                OxideThickness.RawValue = 1e-7;

            // Calculate base defaults
            base.CalculateDefaults();

            // Calculate the narrowing factor
            NarrowFactor = Delta * 0.5 * Math.PI * EpsilonSilicon / OxideCapFactor;
        }

        /// <summary>
        /// Creates a deep clone of the parameter set.
        /// </summary>
        /// <returns>
        /// A deep clone of the parameter set.
        /// </returns>
        public override ParameterSet Clone()
        {
            // We have some private/protected properties that need to be set manually.
            var result = (ModelBaseParameters) base.Clone();

            // Set properties
            result.Delta = Delta;
            result.NarrowFactor = NarrowFactor; // Just to be sure, given the special nature of "delta"

            return result;
        }
    }
}
