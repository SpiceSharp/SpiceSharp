using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Diagnostics;
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
        private ModelBaseParameters _mbp;
        private BaseParameters _bp;

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
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            if (!_bp.Capacitance.Given)
                _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
            if (simulation == null)
                throw new ArgumentNullException(nameof(simulation));

            if (!_bp.Capacitance.Given)
            {
                if (_mbp == null)
                    throw new CircuitException("No model specified");

                double width = _bp.Width.Given ? _bp.Width.Value : _mbp.DefaultWidth.Value;
                _bp.Capacitance.Value = _mbp.JunctionCap *
                    (width - _mbp.Narrow) *
                    (_bp.Length - _mbp.Narrow) +
                    _mbp.JunctionCapSidewall * 2 * (
                    _bp.Length - _mbp.Narrow +
                    (width - _mbp.Narrow));
            }
        }
    }
}
