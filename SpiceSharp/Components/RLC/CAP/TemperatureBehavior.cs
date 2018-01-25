using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.CAP;
using SpiceSharp.Simulations;

namespace SpiceSharp.Behaviors.CAP
{
    /// <summary>
    /// Temperature behavior for a <see cref="Components.Capacitor"/>
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
            if (!bp.CAPcapac.Given)
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

            if (!bp.CAPcapac.Given)
            {
                if (mbp == null)
                    throw new CircuitException("No model specified");

                double width = bp.CAPwidth.Given ? bp.CAPwidth.Value : mbp.CAPdefWidth.Value;
                bp.CAPcapac.Value = mbp.CAPcj *
                    (bp.CAPwidth - mbp.CAPnarrow) *
                    (bp.CAPlength - mbp.CAPnarrow) +
                    mbp.CAPcjsw * 2 * (
                    (bp.CAPlength - mbp.CAPnarrow) +
                    (bp.CAPwidth - mbp.CAPnarrow));
            }
        }
    }
}
