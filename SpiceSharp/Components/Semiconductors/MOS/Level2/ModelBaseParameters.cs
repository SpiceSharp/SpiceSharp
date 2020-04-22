using SpiceSharp.Attributes;
using System;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// Base parameters for a <see cref="Mosfet2Model" />
    /// </summary>
    /// <seealso cref="Common.ModelBaseParameters" />
    [GeneratedParameters]
    public class ModelBaseParameters : Common.ModelBaseParameters
    {
        private GivenParameter<double> _junctionDepth;
        private GivenParameter<double> _fastSurfaceStateDensity;
        private GivenParameter<double> _channelCharge = new GivenParameter<double>(1, false);
        private GivenParameter<double> _criticalFieldExp;
        private GivenParameter<double> _criticalField = new GivenParameter<double>(1e4, false);

        /// <summary>
        /// Gets the channel length modulation parameter.
        /// </summary>
        [ParameterName("lambda"), ParameterInfo("Channel length modulation")]
        public GivenParameter<double> Lambda { get; set; }

        /// <summary>
        /// Gets the width effect on the threshold voltage.
        /// </summary>
        [ParameterName("delta"), ParameterInfo("Width effect on threshold")]
        public GivenParameter<double> NarrowFactor { get; set; }

        /// <summary>
        /// Gets the critical field for mobility degradation.
        /// </summary>
        [ParameterName("ucrit"), ParameterInfo("Crit. field for mob. degradation")]
        [GreaterThan(0)]
        public GivenParameter<double> CriticalField
        {
            get => _criticalField;
            set
            {
                Utility.GreaterThan(value, nameof(CriticalField), 0);
                _criticalField = value;
            }
        }

        /// <summary>
        /// Gets the critical field exponent for mobility degradation.
        /// </summary>
        [ParameterName("uexp"), ParameterInfo("Crit. field exp for mob. deg.")]
        [GreaterThanOrEquals(0)]
        public GivenParameter<double> CriticalFieldExp
        {
            get => _criticalFieldExp;
            set
            {
                Utility.GreaterThanOrEquals(value, nameof(CriticalFieldExp), 0);
                _criticalFieldExp = value;
            }
        }

        /// <summary>
        /// Gets the total channel charge coefficient.
        /// </summary>
        [ParameterName("neff"), ParameterInfo("Total channel charge coeff.")]
        [GreaterThan(0)]
        public GivenParameter<double> ChannelCharge
        {
            get => _channelCharge;
            set
            {
                Utility.GreaterThan(value, nameof(ChannelCharge), 0);
                _channelCharge = value;
            }
        }

        /// <summary>
        /// Gets the fast surface state density.
        /// </summary>
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
        /// Gets the maximum drift velocity.
        /// </summary>
        [ParameterName("vmax"), ParameterInfo("Maximum carrier drift velocity")]
        public GivenParameter<double> MaxDriftVelocity { get; set; }

        /// <summary>
        /// Gets the junction depth.
        /// </summary>
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
    }
}
