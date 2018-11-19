using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common transient behavior for MOS.
    /// </summary>
    /// <seealso cref="SpiceSharp.Behaviors.BaseTransientBehavior" />
    /// <seealso cref="SpiceSharp.Components.IConnectedBehavior" />
    public class TransientBehavior : ExportingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private BiasingBehavior _load;
        private TemperatureBehavior _temp;

        /// <summary>
        /// Gets or sets the bulk-drain capacitance.
        /// </summary>
        /// <value>
        /// The bulk-drain capacitance.
        /// </value>
        [ParameterName("cbd"), ParameterInfo("Bulk-Drain capacitance")]
        public double CapBd { get; protected set; }

        /// <summary>
        /// Gets or sets the bulk-source capacitance.
        /// </summary>
        /// <value>
        /// The bulk-source capacitance.
        /// </value>
        [ParameterName("cbs"), ParameterInfo("Bulk-Source capacitance")]
        public double CapBs { get; protected set; }

        /// <summary>
        /// State variables
        /// </summary>
        protected StateHistory VoltageGs { get; private set; }
        protected StateHistory VoltageDs { get; private set; }
        protected StateHistory VoltageBs { get; private set; }
        protected StateHistory CapGs { get; private set; }
        protected StateHistory CapGd { get; private set; }
        protected StateHistory CapGb { get; private set; }
        protected StateDerivative ChargeGs { get; private set; }
        protected StateDerivative ChargeGd { get; private set; }
        protected StateDerivative ChargeGb { get; private set; }
        protected StateDerivative ChargeBd { get; private set; }
        protected StateDerivative ChargeBs { get; private set; }

        /// <summary>
        /// Elements needed for transient behavior
        /// </summary>
        protected VectorElement<double> GatePtr { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name) : base(name)
        {
        }
        
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
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _load = provider.GetBehavior<BiasingBehavior>();
        }
        
        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="method"></param>
        public void CreateStates(IntegrationMethod method)
        {
			if (method == null)
				throw new ArgumentNullException(nameof(method));

            VoltageGs = method.CreateHistory();
            VoltageDs = method.CreateHistory();
            VoltageBs = method.CreateHistory();
            CapGs = method.CreateHistory();
            CapGd = method.CreateHistory();
            CapGb = method.CreateHistory();
            ChargeGs = method.CreateDerivative();
            ChargeGd = method.CreateDerivative();
            ChargeGb = method.CreateDerivative();
            ChargeBd = method.CreateDerivative();
            ChargeBs = method.CreateDerivative();
        }

        /// <summary>
        /// Gets DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double arg, sarg, sargsw;

            // Get voltages
            var vgs = _load.VoltageGs;
            var vds = _load.VoltageDs;
            var vbs = _load.VoltageBs;
            var vbd = _load.VoltageBd;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;
            var von = _mbp.MosfetType * _load.Von;
            var vdsat = _mbp.MosfetType * _load.SaturationVoltageDs;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var gateSourceOverlapCap = _mbp.GateSourceOverlapCapFactor * _bp.Width;
            var gateDrainOverlapCap = _mbp.GateDrainOverlapCapFactor * _bp.Width;
            var gateBulkOverlapCap = _mbp.GateBulkOverlapCapFactor * effectiveLength;
            var oxideCap = _mbp.OxideCapFactor * effectiveLength * _bp.Width;

            if (vbs < _temp.TempDepletionCap)
            {
                arg = 1 - vbs / _temp.TempBulkPotential;
                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(_mbp.BulkJunctionSideGradingCoefficient))
                {
                    if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5))
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    else
                        sarg = sargsw = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                }
                else
                {
                    if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5))
                        sarg = 1 / Math.Sqrt(arg);
                    else
                        sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    if (_mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                        sargsw = 1 / Math.Sqrt(arg);
                    else
                        sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                }
                ChargeBs.Current = _temp.TempBulkPotential * (_temp.CapBs * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBsSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
            }
            else
                ChargeBs.Current = _temp.F4S + vbs * (_temp.F2S + vbs * (_temp.F3S / 2));

            if (vbd < _temp.TempDepletionCap)
            {
                arg = 1 - vbd / _temp.TempBulkPotential;
                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5) && _mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                else
                {
                    if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5))
                        sarg = 1 / Math.Sqrt(arg);
                    else
                        sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    if (_mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                        sargsw = 1 / Math.Sqrt(arg);
                    else
                        sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                }
                ChargeBd.Current = _temp.TempBulkPotential * (_temp.CapBd * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBdSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
            }
            else
                ChargeBd.Current = _temp.F4D + vbd * (_temp.F2D + vbd * _temp.F3D / 2);

            /*
             * calculate meyer's capacitors
             */
            double icapgs, icapgd, icapgb;
            if (_load.Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd, von, vdsat,
                    out icapgs, out icapgd, out icapgb, _temp.TempPhi, oxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, von, vdsat,
                    out icapgd, out icapgs, out icapgb, _temp.TempPhi, oxideCap);
            }
            CapGs.Current = icapgs;
            CapGd.Current = icapgd;
            CapGb.Current = icapgb;
            var capgs = 2 * icapgs + gateSourceOverlapCap;
            var capgd = 2 * icapgd + gateDrainOverlapCap;
            var capgb = 2 * icapgb + gateBulkOverlapCap;

            ChargeGs.Current = capgs * vgs;
            ChargeGd.Current = capgd * vgd;
            ChargeGb.Current = capgb * vgb;

            // Store these voltages
            VoltageGs.Current = vgs;
            VoltageDs.Current = vds;
            VoltageBs.Current = vbs;
        }

        /// <summary>
        /// Allocate elements in the Y-matrix and Rhs-vector to populate during loading. Additional
        /// equations can also be allocated here.
        /// </summary>
        /// <param name="solver">The solver.</param>
        public void GetEquationPointers(Solver<double> solver)
        {
            if (solver == null)
                throw new ArgumentNullException(nameof(solver));
            GatePtr = solver.GetRhsElement(_load.GateNode);
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));
            double arg, sarg, sargsw;

            // Get voltages
            var vbd = _load.VoltageBd;
            var vbs = _load.VoltageBs;
            var vgs = _load.VoltageGs;
            var vds = _load.VoltageDs;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;
            var von = _mbp.MosfetType * _load.Von;
            var vdsat = _mbp.MosfetType * _load.SaturationVoltageDs;

            // Store these voltages
            VoltageGs.Current = vgs;
            VoltageDs.Current = vds;
            VoltageBs.Current = vbs;

            var gbd = 0.0;
            var cbd = 0.0;
            var gbs = 0.0;
            var cbs = 0.0;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var gateSourceOverlapCap = _mbp.GateSourceOverlapCapFactor * _bp.Width;
            var gateDrainOverlapCap = _mbp.GateDrainOverlapCapFactor * _bp.Width;
            var gateBulkOverlapCap = _mbp.GateBulkOverlapCapFactor * effectiveLength;
            var oxideCap = _mbp.OxideCapFactor * effectiveLength * _bp.Width;

            /*
            * now we do the hard part of the bulk - drain and bulk - source
            * diode - we evaluate the non - linear capacitance and
            * charge
            *
            * the basic equations are not hard, but the implementation
            * is somewhat long in an attempt to avoid log / exponential
            * evaluations
            */
            /*
            * charge storage elements
            *
            * .. bulk - drain and bulk - source depletion capacitances
            */
            if (vbs < _temp.TempDepletionCap)
            {
                arg = 1 - vbs / _temp.TempBulkPotential;
                /*
                * the following block looks somewhat long and messy,
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(_mbp.BulkJunctionSideGradingCoefficient))
                {
                    if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5))
                    {
                        sarg = sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        sarg = sargsw = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                }
                else
                {
                    if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5))
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (_mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                ChargeBs.Current = _temp.TempBulkPotential * (_temp.CapBs * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBsSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBs = _temp.CapBs * sarg + _temp.CapBsSidewall * sargsw;
            }
            else
            {
                ChargeBs.Current = _temp.F4S + vbs * (_temp.F2S + vbs * (_temp.F3S / 2));
                CapBs = _temp.F2S + _temp.F3S * vbs;
            }

            if (vbd < _temp.TempDepletionCap)
            {
                arg = 1 - vbd / _temp.TempBulkPotential;
                /*
                * the following block looks somewhat long and messy,
                * but since most users use the default grading
                * coefficients of .5, and sqrt is MUCH faster than an
                * Math.Exp(Math.Log()) we use this special case code to buy time.
                * (as much as 10% of total job time!)
                */
                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5) && _mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
                }
                else
                {
                    if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5))
                    {
                        sarg = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sarg = Math.Exp(-_mbp.BulkJunctionBotGradingCoefficient * Math.Log(arg));
                    }
                    if (_mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                    {
                        sargsw = 1 / Math.Sqrt(arg);
                    }
                    else
                    {
                        /* NOSQRT */
                        sargsw = Math.Exp(-_mbp.BulkJunctionSideGradingCoefficient * Math.Log(arg));
                    }
                }
                /* NOSQRT */
                ChargeBd.Current = _temp.TempBulkPotential * (_temp.CapBd * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBdSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBd = _temp.CapBd * sarg + _temp.CapBdSidewall * sargsw;
            }
            else
            {
                ChargeBd.Current = _temp.F4D + vbd * (_temp.F2D + vbd * _temp.F3D / 2);
                CapBd = _temp.F2D + vbd * _temp.F3D;
            }

            // integrate the capacitors and save results
            ChargeBd.Integrate();
            gbd += ChargeBd.Jacobian(CapBd);
            cbd += ChargeBd.Derivative;
            // TODO: The derivative of Qbd should be added to Cd (drain current). Figure out a way later.
            ChargeBs.Integrate();
            gbs += ChargeBs.Jacobian(CapBs);
            cbs += ChargeBs.Derivative;

            /*
             * calculate meyer's capacitors
             */
            double icapgs, icapgd, icapgb;
            if (_load.Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd, von, vdsat,
                    out icapgs, out icapgd, out icapgb, _temp.TempPhi, oxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, von, vdsat,
                    out icapgd, out icapgs, out icapgb, _temp.TempPhi, oxideCap);
            }
            CapGs.Current = icapgs;
            CapGd.Current = icapgd;
            CapGb.Current = icapgb;
            var vgs1 = VoltageGs[1];
            var vgd1 = vgs1 - VoltageDs[1];
            var vgb1 = vgs1 - VoltageBs[1];
            var capgs = CapGs.Current + CapGs[1] + gateSourceOverlapCap;
            var capgd = CapGd.Current + CapGd[1] + gateDrainOverlapCap;
            var capgb = CapGb.Current + CapGb[1] + gateBulkOverlapCap;

            ChargeGs.Current = (vgs - vgs1) * capgs + ChargeGs[1];
            ChargeGd.Current = (vgd - vgd1) * capgd + ChargeGd[1];
            ChargeGb.Current = (vgb - vgb1) * capgb + ChargeGb[1];

            ChargeGs.Integrate();
            var gcgs = ChargeGs.Jacobian(capgs);
            var ceqgs = ChargeGs.RhsCurrent(gcgs, vgs);
            ChargeGd.Integrate();
            var gcgd = ChargeGd.Jacobian(capgd);
            var ceqgd = ChargeGd.RhsCurrent(gcgd, vgd);
            ChargeGb.Integrate();
            var gcgb = ChargeGb.Jacobian(capgb);
            var ceqgb = ChargeGb.RhsCurrent(gcgb, vgb);

            // Load current vector
            var ceqbs = _mbp.MosfetType * (cbs - gbs * vbs);
            var ceqbd = _mbp.MosfetType * (cbd - gbd * vbd);
            var ptrs = _load.Pointers;
            GatePtr.Value -= _mbp.MosfetType * (ceqgs + ceqgb + ceqgd);
            ptrs.BulkPtr.Value -= ceqbs + ceqbd - _mbp.MosfetType * ceqgb;
            ptrs.DrainPrimePtr.Value += ceqbd + _mbp.MosfetType * ceqgd;
            ptrs.SourcePrimePtr.Value += ceqbs + _mbp.MosfetType * ceqgs;

            // Load Y-matrix
            ptrs.GateGatePtr.Value += gcgd + gcgs + gcgb;
            ptrs.BulkBulkPtr.Value += gbd + gbs + gcgb;
            ptrs.DrainPrimeDrainPrimePtr.Value += gbd + gcgd;
            ptrs.SourcePrimeSourcePrimePtr.Value += gbs + gcgs;
            ptrs.GateBulkPtr.Value -= gcgb;
            ptrs.GateDrainPrimePtr.Value -= gcgd;
            ptrs.GateSourcePrimePtr.Value -= gcgs;
            ptrs.BulkGatePtr.Value -= gcgb;
            ptrs.BulkDrainPrimePtr.Value -= gbd;
            ptrs.BulkSourcePrimePtr.Value -= gbs;
            ptrs.DrainPrimeGatePtr.Value -= gcgd;
            ptrs.DrainPrimeBulkPtr.Value -= gbd;
            ptrs.SourcePrimeGatePtr.Value -= gcgs;
            ptrs.SourcePrimeBulkPtr.Value -= gbs;
        }
    }
}
