using SpiceSharp.ParameterSets;
using SpiceSharp.Attributes;

namespace SpiceSharp.Components.Switches
{
    /// <summary>
    /// Base parameters for a switch model.
    /// </summary>
    public partial class ModelParameters : ParameterSet<ModelParameters>
    {
        /// <summary>
        /// Gets the resistance parameter when closed.
        /// </summary>
        /// <value>
        /// The on resistance.
        /// </value>
        [ParameterName("ron"), ParameterInfo("Closed resistance")]
        [Finite]
        private double _onResistance = 1;

        /// <summary>
        /// Gets the resistance parameter when open.
        /// </summary>
        /// <value>
        /// The off resistance.
        /// </value>
        [ParameterName("roff"), ParameterInfo("Open resistance")]
        [Finite]
        private double _offResistance = 1e12;

        /// <summary>
        /// Gets the threshold parameter.
        /// </summary>
        /// <value>
        /// The threshold value.
        /// </value>
        public virtual double Threshold { get; set; }

        /// <summary>
        /// Gets the hysteresis parameter.
        /// </summary>
        /// <value>
        /// The hysteresis value.
        /// </value>
        public virtual double Hysteresis { get; set; }
    }
}
