using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Temperature behavior for a <see cref="Model"/>
    /// </summary>
    public class ModelTemperatureBehavior : Common.ModelTemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public ModelTemperatureBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            base.Setup(simulation, provider);

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>();
        }

        /// <summary>
        /// Destroy the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public override void Unsetup(Simulation simulation)
        {
            _mbp = null;

            base.Unsetup(simulation);
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
            base.Temperature(simulation);

            if (_mbp.OxideThickness.Given && _mbp.OxideThickness > 0.0)
            {
                if (_mbp.SubstrateDoping.Given)
                {
                    if (_mbp.SubstrateDoping * 1e6 > 1.45e16)
                    {
                        if (!_mbp.Phi.Given)
                        {
                            _mbp.Phi.RawValue = 2 * VtNominal * Math.Log(_mbp.SubstrateDoping * 1e6 / 1.45e16);
                            _mbp.Phi.RawValue = Math.Max(.1, _mbp.Phi);
                        }

                        var fermis = _mbp.MosfetType * .5 * _mbp.Phi;
                        var wkfng = 3.2;
                        if (!_mbp.GateType.Given)
                            _mbp.GateType.RawValue = 1;
                        if (!_mbp.GateType.RawValue.Equals(0))
                        {
                            var fermig = _mbp.MosfetType * _mbp.GateType * .5 * EgFet1;
                            wkfng = 3.25 + .5 * EgFet1 - fermig;
                        }

                        var wkfngs = wkfng - (3.25 + .5 * EgFet1 + fermis);
                        if (!_mbp.Gamma.Given)
                        {
                            _mbp.Gamma.RawValue =
                                Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * _mbp.SubstrateDoping * 1e6) /
                                _mbp.OxideCapFactor;
                        }

                        if (!_mbp.Vt0.Given)
                        {
                            if (!_mbp.SurfaceStateDensity.Given)
                                _mbp.SurfaceStateDensity.RawValue = 0;
                            var vfb = wkfngs - _mbp.SurfaceStateDensity * 1e4 * Circuit.Charge / _mbp.OxideCapFactor;
                            _mbp.Vt0.RawValue = vfb + _mbp.MosfetType * (_mbp.Gamma * Math.Sqrt(_mbp.Phi) + _mbp.Phi);
                        }
                    }
                    else
                    {
                        _mbp.SubstrateDoping.RawValue = 0;
                        throw new CircuitException("{0}: Nsub < Ni".FormatString(Name));
                    }
                }
            }
        }
    }
}
