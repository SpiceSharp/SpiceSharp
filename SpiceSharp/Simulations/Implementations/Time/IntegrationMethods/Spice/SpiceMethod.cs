using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A default integration method as implemented by most Spice simulators.
    /// </summary>
    public abstract partial class SpiceMethod : VariableTimestepConfiguration
    {
        /// <summary>
        /// Gets or sets the initial timestep.
        /// </summary>
        /// <value>
        /// The initial timestep.
        /// </value>
        [ParameterName("step"), ParameterInfo("The initial timestep.")]
        public double InitialStep { get; set; }

        /// <summary>
        /// Gets or sets the absolute tolerance.
        /// </summary>
        /// <value>
        /// The absolute tolerance.
        /// </value>
        [ParameterName("abstol"), ParameterInfo("The absolute tolerance.")]
        public double AbsoluteTolerance { get; set; } = 1e-12;

        /// <summary>
        /// The tolerance on charge.
        /// </summary>
        /// <value>
        /// The charge tolerance.
        /// </value>
        [ParameterName("chgtol"), ParameterInfo("The charge tolerance.")]
        public double ChargeTolerance { get; set; } = 1e-14;

        /// <summary>
        /// Gets or sets the relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("reltol"), ParameterInfo("The relative tolerance.")]
        public double RelativeTolerance { get; set; } = 1e-3;

        /// <summary>
        /// Gets or sets the transient tolerance factor.
        /// </summary>
        /// <value>
        /// The transient tolerance factor.
        /// </value>
        [ParameterName("trtol"), ParameterInfo("The transient tolerance factor.")]
        public double TrTol { get; set; } = 7.0;

        /// <summary>
        /// The local truncation error relative tolerance.
        /// </summary>
        /// <value>
        /// The relative tolerance.
        /// </value>
        [ParameterName("ltereltol"), ParameterInfo("The local truncation error relative tolerance.")]
        public double LteRelTol { get; set; } = 1e-3;

        /// <summary>
        /// The local truncation error absolute tolerance.
        /// </summary>
        /// <value>
        /// The aboslute tolerance.
        /// </value>
        [ParameterName("lteabstol"), ParameterInfo("The local truncation error absolute tolerance.")]
        public double LteAbsTol { get; set; } = 1e-6;

        /// <summary>
        /// Gets or sets a value indicating whether node voltages should be used to truncate the timestep.
        /// </summary>
        /// <value>
        ///   <c>true</c> if node voltages are used; otherwise, <c>false</c>.
        /// </value>
        [ParameterName("truncnodes"), ParameterInfo("Flag that specifies whether or not node voltages should be used to truncate the timestep.")]
        public bool TruncateNodes { get; set; }
    }
}
