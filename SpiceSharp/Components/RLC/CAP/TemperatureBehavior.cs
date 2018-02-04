using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Capacitor"/>
    /// </summary>
    public class TemperatureBehavior : Behaviors.TemperatureBehavior
    {
        /// <summary>
        /// Necessary parameters and behaviors
        /// </summary>
        ModelBaseParameters mbp;
        BaseParameters bp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            bp = provider.GetParameterSet<BaseParameters>("entity");
            if (!bp.Capacitance.Given)
                mbp = provider.GetParameterSet<ModelBaseParameters>("model");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            if (!bp.Capacitance.Given)
            {
                if (mbp == null)
                    throw new CircuitException("No model specified");

                double width = bp.Width.Given ? bp.Width.Value : mbp.DefaultWidth.Value;
                bp.Capacitance.Value = mbp.JunctionCap *
                    (width - mbp.Narrow) *
                    (bp.Length - mbp.Narrow) +
                    mbp.JunctionCapSidewall * 2 * (
                    (bp.Length - mbp.Narrow) +
                    (width - mbp.Narrow));
            }
        }
    }
}
