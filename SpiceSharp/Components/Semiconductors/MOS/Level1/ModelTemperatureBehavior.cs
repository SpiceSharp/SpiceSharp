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

            // Get parameters
            _mbp = provider.GetParameterSet<ModelBaseParameters>();
        }

        /// <summary>
        /// Do temperature-dependent calculations
        /// </summary>
        /// <param name="simulation">Base simulation</param>
        public override void Temperature(BaseSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            /* perform model defaulting */
            if (!_mbp.NominalTemperature.Given)
                _mbp.NominalTemperature.RawValue = simulation.RealState.NominalTemperature;

            Factor1 = _mbp.NominalTemperature / Circuit.ReferenceTemperature;
            VtNominal = _mbp.NominalTemperature * Circuit.KOverQ;
            var kt1 = Circuit.Boltzmann * _mbp.NominalTemperature;
            EgFet1 = 1.16 - 7.02e-4 * _mbp.NominalTemperature * _mbp.NominalTemperature / (_mbp.NominalTemperature + 1108);
            var arg1 = -EgFet1 / (kt1 + kt1) + 1.1150877 / (Circuit.Boltzmann * (Circuit.ReferenceTemperature + Circuit.ReferenceTemperature));
            PbFactor1 = -2 * VtNominal * (1.5 * Math.Log(Factor1) + Circuit.Charge * arg1);

            /* now model parameter preprocessing */

            if (!_mbp.OxideThickness.Given || _mbp.OxideThickness.Value.Equals(0))
            {
                OxideCapFactor = 0;
            }
            else
            {
                OxideCapFactor = 3.9 * 8.854214871e-12 / _mbp.OxideThickness;
                if (!_mbp.Transconductance.Given)
                {
                    if (!_mbp.SurfaceMobility.Given)
                    {
                        _mbp.SurfaceMobility.RawValue = 600;
                    }
                    _mbp.Transconductance.RawValue = _mbp.SurfaceMobility * OxideCapFactor * 1e-4;
                }
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
                            _mbp.Gamma.RawValue = Math.Sqrt(2 * 11.70 * 8.854214871e-12 * Circuit.Charge * _mbp.SubstrateDoping * 1e6) / OxideCapFactor;
                        }
                        if (!_mbp.Vt0.Given)
                        {
                            if (!_mbp.SurfaceStateDensity.Given)
                                _mbp.SurfaceStateDensity.RawValue = 0;
                            var vfb = wkfngs - _mbp.SurfaceStateDensity * 1e4 * Circuit.Charge / OxideCapFactor;
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
