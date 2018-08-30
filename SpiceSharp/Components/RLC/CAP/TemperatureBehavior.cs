using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.CapacitorBehaviors
{
    /// <summary>
    /// Temperature behavior for a <see cref="Capacitor"/>
    /// </summary>
    public class TemperatureBehavior : BaseTemperatureBehavior
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
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
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

                var width = _bp.Width.Given ? _bp.Width.Value : _mbp.DefaultWidth.Value;
                _bp.Capacitance.RawValue = _mbp.JunctionCap *
                    (width - _mbp.Narrow) *
                    (_bp.Length - _mbp.Narrow) +
                    _mbp.JunctionCapSidewall * 2 * (
                    _bp.Length - _mbp.Narrow +
                    (width - _mbp.Narrow));
            }
        }
    }
}
