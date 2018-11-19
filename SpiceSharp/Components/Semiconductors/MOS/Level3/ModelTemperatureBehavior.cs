using System;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet3Model" />.
    /// </summary>
    public class ModelTemperatureBehavior : Common.ModelTemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Shared parameters
        /// </summary>
        [ParameterName("xd"), ParameterInfo("Depletion layer width")]
        public double CoefficientDepletionLayerWidth { get; protected set; }
        [ParameterName("alpha"), ParameterInfo("Alpha")]
        public double Alpha { get; protected set; }

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
            base.Setup(simulation, provider);
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>();
        }
        
        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
            base.Temperature(simulation);

            if (_mbp.SubstrateDoping.Given)
            {
                if (_mbp.SubstrateDoping * 1e6 /* (cm**3 / m**3) */ > 1.45e16)
                {
                    if (!_mbp.Phi.Given)
                    {
                        _mbp.Phi.RawValue = 2 * VtNominal * Math.Log(_mbp.SubstrateDoping * 1e6 /* (cm^3/m^3) */  / 1.45e16);
                        _mbp.Phi.RawValue = Math.Max(0.1, _mbp.Phi);
                    }
                    var fermis = _mbp.MosfetType * 0.5 * _mbp.Phi;
                    var wkfng = 3.2;
                    if (!_mbp.GateType.Given)
                        _mbp.GateType.RawValue = 1;
                    if (!_mbp.GateType.RawValue.Equals(0.0))
                    {
                        var fermig = _mbp.MosfetType * _mbp.GateType * 0.5 * EgFet1;
                        wkfng = 3.25 + 0.5 * EgFet1 - fermig;
                    }
                    var wkfngs = wkfng - (3.25 + 0.5 * EgFet1 + fermis);
                    if (!_mbp.Gamma.Given)
                    {
                        _mbp.Gamma.RawValue = Math.Sqrt(2 * EpsilonSilicon * Circuit.Charge * _mbp.SubstrateDoping * 1e6 /* (cm**3 / m**3) */) /
                                              _mbp.OxideCapFactor;
                    }
                    if (!_mbp.Vt0.Given)
                    {
                        if (!_mbp.SurfaceStateDensity.Given)
                            _mbp.SurfaceStateDensity.RawValue = 0;
                        var vfb = wkfngs - _mbp.SurfaceStateDensity * 1e4 * Circuit.Charge / _mbp.OxideCapFactor;
                        _mbp.Vt0.RawValue = vfb + _mbp.MosfetType * (_mbp.Gamma * Math.Sqrt(_mbp.Phi) + _mbp.Phi);
                    }

                    Alpha = (EpsilonSilicon + EpsilonSilicon) / (Circuit.Charge * _mbp.SubstrateDoping * 1e6 /* (cm**3 / m**3) */);
                    CoefficientDepletionLayerWidth = Math.Sqrt(Alpha);
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
