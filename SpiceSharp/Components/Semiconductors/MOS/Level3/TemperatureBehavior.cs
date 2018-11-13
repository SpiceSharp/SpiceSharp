using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Temperature behavior for a <see cref="Mosfet3"/>
    /// </summary>
    public class TemperatureBehavior : Common.TemperatureBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TemperatureBehavior(string name) : base(name) { }

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
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");
        }
        
        /// <summary>
        /// Calculates the critical voltages.
        /// </summary>
        protected override void CalculateCriticalVoltages()
        {
            var vt = _bp.Temperature * Circuit.KOverQ;
            if (_mbp.JunctionSatCurDensity.Value <= 0 || _bp.DrainArea.Value <= 0 || _bp.SourceArea.Value <= 0)
            {
                SourceVCritical = DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * _mbp.JunctionSatCur));
            }
            else
            {
                DrainVCritical = vt * Math.Log(vt / (Circuit.Root2 * _mbp.JunctionSatCurDensity * _bp.DrainArea));
                SourceVCritical = vt * Math.Log(vt / (Circuit.Root2 * _mbp.JunctionSatCurDensity * _bp.SourceArea));
            }
        }

        /// <summary>
        /// Calculates the capacitances.
        /// </summary>
        protected override void CalculateCapacitanceFactors()
        {
            var arg = 1 - _mbp.ForwardCapDepletionCoefficient;
            var sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
            var sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
            F2D = CapBd * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionBotGradingCoefficient)) *
                  sarg / arg + CapBdSidewall * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionSideGradingCoefficient)) * sargsw /
                  arg;
            F3D = CapBd * _mbp.BulkJunctionBotGradingCoefficient * sarg / arg / _mbp.BulkJunctionPotential + CapBd *
                  _mbp.BulkJunctionSideGradingCoefficient * sargsw / arg / _mbp.BulkJunctionPotential;
            F4D = CapBd * _mbp.BulkJunctionPotential * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                  CapBdSidewall * _mbp.BulkJunctionPotential * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient) -
                  F3D / 2 * (TempDepletionCap * TempDepletionCap) - TempDepletionCap * F2D;

            arg = 1 - _mbp.ForwardCapDepletionCoefficient;
            sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
            sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
            F2S = CapBs * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionBotGradingCoefficient)) *
                  sarg / arg + CapBsSidewall * (1 - _mbp.ForwardCapDepletionCoefficient * (1 + _mbp.BulkJunctionSideGradingCoefficient)) * sargsw / arg;
            F3S = CapBs * _mbp.BulkJunctionBotGradingCoefficient * sarg / arg / _mbp.BulkJunctionPotential +
                  CapBsSidewall * _mbp.BulkJunctionSideGradingCoefficient * sargsw / arg / _mbp.BulkJunctionPotential;
            F4S = CapBs * _mbp.BulkJunctionPotential * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                  CapBsSidewall * _mbp.BulkJunctionPotential * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient) -
                  F3S / 2 * (TempBulkPotential * TempBulkPotential) - TempBulkPotential * F2S;
        }
    }
}
