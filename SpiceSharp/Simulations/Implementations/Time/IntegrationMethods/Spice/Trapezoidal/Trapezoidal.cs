using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A class that implements the trapezoidal integration method as
    /// implemented by Spice 3f5.
    /// </summary>
    /// <seealso cref="SpiceMethod" />
    [GeneratedParameters]
    public partial class Trapezoidal : SpiceMethod
    {
        /// <summary>
        /// Gets the xmu constant.
        /// </summary>
        /// <value>
        /// The xmu constant.
        /// </value>
        [ParameterName("xmu"), ParameterInfo("The xmu parameter.")]
        public double Xmu { get; set; } = 0.5;

        /// <summary>
        /// Creates an instance of the integration method for an associated <see cref="IBiasingSimulationState" />.
        /// </summary>
        /// <param name="state">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IBiasingSimulationState state) => new Instance(this, state);
    }
}
