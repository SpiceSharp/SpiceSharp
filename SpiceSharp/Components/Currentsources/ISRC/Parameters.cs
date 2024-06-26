using SpiceSharp.Attributes;

namespace SpiceSharp.Components.CurrentSources
{
    /// <summary>
    /// Parameters for an independent current source.
    /// </summary>
    [GeneratedParameters]
    public partial class Parameters : CommonBehaviors.IndependentSourceParameters, ICloneable<Parameters>
    {
        /// <summary>
        /// Gets or sets the number of current sources in parallel.
        /// </summary>
        /// <value>
        /// The number of current sources in parallel.
        /// </value>
        [ParameterName("m"), ParameterInfo("Parallel multiplier")]
        [GreaterThanOrEquals(0), Finite]
        private double _parallelMultiplier = 1.0;

        /// <inheritdoc/>
        Parameters ICloneable<Parameters>.Clone() => (Parameters)base.Clone();
    }
}
