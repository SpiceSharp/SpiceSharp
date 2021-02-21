using SpiceSharp.Attributes;

namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A class that implements the Gear integration method.
    /// </summary>
    /// <seealso cref="SpiceMethod" />
    [GeneratedParameters]
    public partial class Gear : SpiceMethod
    {
        /// <summary>
        /// Creates an instance of the integration method.
        /// </summary>
        /// <param name="state">The biasing simulation state that will be used as a base.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IBiasingSimulationState state) => new Instance(this, state);
    }
}
