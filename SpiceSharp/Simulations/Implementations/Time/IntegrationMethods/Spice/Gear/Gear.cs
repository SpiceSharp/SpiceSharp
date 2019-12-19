namespace SpiceSharp.Simulations.IntegrationMethods
{
    /// <summary>
    /// A class that implements the Gear integration method.
    /// </summary>
    /// <seealso cref="SpiceMethod" />
    public partial class Gear : SpiceMethod
    {
        /// <summary>
        /// Creates an instance of the integration method for an associated <see cref="IBiasingSimulationState" />.
        /// </summary>
        /// <param name="simulation">The simulation that provides the biasing state.</param>
        /// <returns>
        /// The integration method.
        /// </returns>
        public override IIntegrationMethod Create(IStateful<IBiasingSimulationState> simulation)
            => new Instance(this, simulation);
    }
}
