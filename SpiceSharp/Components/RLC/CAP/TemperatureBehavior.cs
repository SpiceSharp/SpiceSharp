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
            bp = provider.GetParameterSet<BaseParameters>(0);
            if (!bp.Capacitance.Given)
                mbp = provider.GetParameterSet<ModelBaseParameters>(1);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="sim">Base simulation</param>
        public override void Temperature(BaseSimulation sim)
        {
			if (sim == null)
				throw new ArgumentNullException(nameof(sim));

            if (!bp.Capacitance.Given)
            {
                if (mbp == null)
                    throw new CircuitException("No model specified");

                double width = bp.Width.Given ? bp.Width.Value : mbp.DefWidth.Value;
                bp.Capacitance.Value = mbp.Cj *
                    (bp.Width - mbp.Narrow) *
                    (bp.Length - mbp.Narrow) +
                    mbp.Cjsw * 2 * (
                    (bp.Length - mbp.Narrow) +
                    (bp.Width - mbp.Narrow));
            }
        }
    }
}
