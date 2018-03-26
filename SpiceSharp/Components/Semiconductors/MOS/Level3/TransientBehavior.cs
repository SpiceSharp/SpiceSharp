using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet3"/>
    /// </summary>
    public class TransientBehavior : BaseTransientBehavior, IConnectedBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private LoadBehavior _load;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Nodes
        /// </summary>
        private int _drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime;
        protected MatrixElement<double> DrainDrainPtr { get; private set; }
        protected MatrixElement<double> GateGatePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePtr { get; private set; }
        protected MatrixElement<double> BulkBulkPtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateBulkPtr { get; private set; }
        protected MatrixElement<double> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<double> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<double> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<double> BulkDrainPrimePtr { get; private set; }
        protected MatrixElement<double> BulkSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<double> BulkGatePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<double> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<double> DrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<double> SourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<double> SourcePrimeDrainPrimePtr { get; private set; }
        protected VectorElement<double> GatePtr { get; private set; }
        protected VectorElement<double> BulkPtr { get; private set; }
        protected VectorElement<double> DrainPrimePtr { get; private set; }
        protected VectorElement<double> SourcePrimePtr { get; private set; }

        /// <summary>
        /// States
        /// </summary>
        private StateHistory _vgs;
        private StateHistory _vds;
        private StateHistory _vbs;
        private StateHistory _capgs;
        private StateHistory _capgd;
        private StateHistory _capgb;
        private StateDerivative _qgs;
        private StateDerivative _qgd;
        private StateDerivative _qgb;
        private StateDerivative _qbd;
        private StateDerivative _qbs;

        /// <summary>
        /// Shared parameters
        /// </summary>
        [ParameterName("cbd"), ParameterInfo("Bulk-Drain capacitance")]
        public double CapBd { get; protected set; }
        [ParameterName("cbs"), ParameterInfo("Bulk-Source capacitance")]
        public double CapBs { get; protected set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public TransientBehavior(Identifier name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="provider">Data provider</param>
        public override void Setup(SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>("entity");
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>("entity");
            _load = provider.GetBehavior<LoadBehavior>("entity");
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");
        }

        /// <summary>
        /// Connect
        /// </summary>
        /// <param name="pins">Pins</param>
        public void Connect(params int[] pins)
        {
            if (pins == null)
                throw new ArgumentNullException(nameof(pins));
            if (pins.Length != 4)
                throw new CircuitException("Pin count mismatch: 4 pins expected, {0} given".FormatString(pins.Length));
            _drainNode = pins[0];
            _gateNode = pins[1];
            _sourceNode = pins[2];
            _bulkNode = pins[3];
        }

        /// <summary>
        /// Gets matrix pointers
        /// </summary>
        /// <param name="solver">Solver</param>
        public override void GetEquationPointers(Solver<double> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get extra equations
            _drainNodePrime = _load.DrainNodePrime;
            _sourceNodePrime = _load.SourceNodePrime;

            // Get matrix elements
            DrainDrainPtr = solver.GetMatrixElement(_drainNode, _drainNode);
            GateGatePtr = solver.GetMatrixElement(_gateNode, _gateNode);
            SourceSourcePtr = solver.GetMatrixElement(_sourceNode, _sourceNode);
            BulkBulkPtr = solver.GetMatrixElement(_bulkNode, _bulkNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(_drainNodePrime, _drainNodePrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(_sourceNodePrime, _sourceNodePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(_drainNode, _drainNodePrime);
            GateBulkPtr = solver.GetMatrixElement(_gateNode, _bulkNode);
            GateDrainPrimePtr = solver.GetMatrixElement(_gateNode, _drainNodePrime);
            GateSourcePrimePtr = solver.GetMatrixElement(_gateNode, _sourceNodePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(_sourceNode, _sourceNodePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(_bulkNode, _drainNodePrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(_bulkNode, _sourceNodePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(_drainNodePrime, _sourceNodePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(_drainNodePrime, _drainNode);
            BulkGatePtr = solver.GetMatrixElement(_bulkNode, _gateNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(_drainNodePrime, _gateNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(_sourceNodePrime, _gateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(_sourceNodePrime, _sourceNode);
            DrainPrimeBulkPtr = solver.GetMatrixElement(_drainNodePrime, _bulkNode);
            SourcePrimeBulkPtr = solver.GetMatrixElement(_sourceNodePrime, _bulkNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(_sourceNodePrime, _drainNodePrime);

            // Get rhs elements
            GatePtr = solver.GetRhsElement(_gateNode);
            BulkPtr = solver.GetRhsElement(_bulkNode);
            DrainPrimePtr = solver.GetRhsElement(_drainNodePrime);
            SourcePrimePtr = solver.GetRhsElement(_sourceNodePrime);
        }

        /// <summary>
        /// Unsetup
        /// </summary>
        public override void Unsetup()
        {
            // Remove references
            DrainDrainPtr = null;
            GateGatePtr = null;
            SourceSourcePtr = null;
            BulkBulkPtr = null;
            DrainPrimeDrainPrimePtr = null;
            SourcePrimeSourcePrimePtr = null;
            DrainDrainPrimePtr = null;
            GateBulkPtr = null;
            GateDrainPrimePtr = null;
            GateSourcePrimePtr = null;
            SourceSourcePrimePtr = null;
            BulkDrainPrimePtr = null;
            BulkSourcePrimePtr = null;
            DrainPrimeSourcePrimePtr = null;
            DrainPrimeDrainPtr = null;
            BulkGatePtr = null;
            DrainPrimeGatePtr = null;
            SourcePrimeGatePtr = null;
            SourcePrimeSourcePtr = null;
            DrainPrimeBulkPtr = null;
            SourcePrimeBulkPtr = null;
            SourcePrimeDrainPrimePtr = null;
        }

        /// <summary>
        /// Create states
        /// </summary>
        /// <param name="states">States</param>
        public override void CreateStates(StatePool states)
        {
			if (states == null)
				throw new ArgumentNullException(nameof(states));

            _vgs = states.CreateHistory();
            _vds = states.CreateHistory();
            _vbs = states.CreateHistory();
            _capgs = states.CreateHistory();
            _capgd = states.CreateHistory();
            _capgb = states.CreateHistory();
            _qgs = states.CreateDerivative();
            _qgd = states.CreateDerivative();
            _qgb = states.CreateDerivative();
            _qbd = states.CreateDerivative();
            _qbs = states.CreateDerivative();
        }

        /// <summary>
        /// Gets DC states
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double sargsw, capgs = 0.0, capgd = 0.0, capgb = 0.0;

            var vbs = _load.VoltageBs;
            var vgs = _load.VoltageGs;
            var vds = _load.VoltageDs;
            var vbd = vbs - vds;
            var vgd = vgs - vds;
            var vgb = vgs - vbs;
            var von = _mbp.MosfetType * _load.Von;
            var vdsat = _mbp.MosfetType * _load.SaturationVoltageDs;

            _vgs.Current = vgs;
            _vbs.Current = vbs;
            _vds.Current = vds;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var oxideCap = _modeltemp.OxideCapFactor * effectiveLength * _bp.Width;

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
                double arg = 1 - vbs / _temp.TempBulkPotential, sarg;
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
                /* NOSQRT */
                _qbs.Current = _temp.TempBulkPotential * (_temp.CapBs * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBsSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBs = _temp.CapBs * sarg + _temp.CapBsSidewall * sargsw;
            }
            else
            {
                _qbs.Current = _temp.F4S + vbs * (_temp.F2S + vbs * (_temp.F3S / 2));
                CapBs = _temp.F2S + _temp.F3S * vbs;
            }

            if (vbd < _temp.TempDepletionCap)
            {
                double arg = 1 - vbd / _temp.TempBulkPotential, sarg;
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
                _qbd.Current = _temp.TempBulkPotential * (_temp.CapBd * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBdSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBd = _temp.CapBd * sarg + _temp.CapBdSidewall * sargsw;
            }
            else
            {
                _qbd.Current = _temp.F4D + vbd * (_temp.F2D + vbd * _temp.F3D / 2);
                CapBd = _temp.F2D + vbd * _temp.F3D;
            }
            /* CAPZEROBYPASS */

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non - constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
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
            _capgs.Current = icapgs;
            _capgd.Current = icapgd;
            _capgb.Current = icapgb;

            /* TRANOP only */
            _qgs.Current = vgs * capgs;
            _qgd.Current = vgd * capgd;
            _qgb.Current = vgb * capgb;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double sargsw;

            var vbs = _load.VoltageBs;
            var vbd = _load.VoltageBd;
            var vgs = _load.VoltageGs;
            var vds = _load.VoltageDs;
            var vgd = _load.VoltageGs - _load.VoltageDs;
            var vgb = _load.VoltageGs - _load.VoltageBs;
            var von = _mbp.MosfetType * _load.Von;
            var vdsat = _mbp.MosfetType * _load.SaturationVoltageDs;

            _vds.Current = vds;
            _vbs.Current = vbs;
            _vgs.Current = vgs;

            double gbd = 0.0;
            double cbd = 0.0;
            double gbs = 0.0;
            double cbs = 0.0;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var gateSourceOverlapCap = _mbp.GateSourceOverlapCapFactor * _bp.Width;
            var gateDrainOverlapCap = _mbp.GateDrainOverlapCapFactor * _bp.Width;
            var gateBulkOverlapCap = _mbp.GateBulkOverlapCapFactor * effectiveLength;
            var oxideCap = _modeltemp.OxideCapFactor * effectiveLength * _bp.Width;

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
                double arg = 1 - vbs / _temp.TempBulkPotential, sarg;
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
                /* NOSQRT */
                _qbs.Current = _temp.TempBulkPotential * (_temp.CapBs * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBsSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBs = _temp.CapBs * sarg + _temp.CapBsSidewall * sargsw;
            }
            else
            {
                _qbs.Current = _temp.F4S + vbs * (_temp.F2S + vbs * (_temp.F3S / 2));
                CapBs = _temp.F2S + _temp.F3S * vbs;
            }

            if (vbd < _temp.TempDepletionCap)
            {
                double arg = 1 - vbd / _temp.TempBulkPotential, sarg;
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
                _qbd.Current = _temp.TempBulkPotential * (_temp.CapBd * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBdSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBd = _temp.CapBd * sarg + _temp.CapBdSidewall * sargsw;
            }
            else
            {
                _qbd.Current = _temp.F4D + vbd * (_temp.F2D + vbd * _temp.F3D / 2);
                CapBd = _temp.F2D + vbd * _temp.F3D;
            }
            /* CAPZEROBYPASS */

            /* (above only excludes tranop, since we're only at this
            * point if tran or tranop)
            */

            /* 
            * calculate equivalent conductances and currents for
            * depletion capacitors
            */

            /* integrate the capacitors and save results */
            _qbd.Integrate();
            gbd += _qbd.Jacobian(CapBd);
            cbd += _qbd.Derivative;
            _qbs.Integrate();
            gbs += _qbs.Jacobian(CapBs);
            cbs += _qbs.Derivative;

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non - constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
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
            _capgs.Current = icapgs;
            _capgd.Current = icapgd;
            _capgb.Current = icapgb;

            var vgs1 = _vgs[1];
            var vgd1 = vgs1 - _vds[1];
            var vgb1 = vgs1 - _vbs[1];
            var capgs = _capgs.Current + _capgs[1] + gateSourceOverlapCap;
            var capgd = _capgd.Current + _capgd[1] + gateDrainOverlapCap;
            var capgb = _capgb.Current + _capgb[1] + gateBulkOverlapCap;

            _qgs.Current = (vgs - vgs1) * capgs + _qgs[1];
            _qgd.Current = (vgd - vgd1) * capgd + _qgd[1];
            _qgb.Current = (vgb - vgb1) * capgb + _qgb[1];

            /* 
             * calculate equivalent conductances and currents for
             * meyer"s capacitors
             */
            _qgs.Integrate();
            var gcgs = _qgs.Jacobian(capgs);
            var ceqgs = _qgs.RhsCurrent(gcgs, vgs);
            _qgd.Integrate();
            var gcgd = _qgd.Jacobian(capgd);
            var ceqgd = _qgd.RhsCurrent(gcgd, vgd);
            _qgb.Integrate();
            var gcgb = _qgb.Jacobian(capgb);
            var ceqgb = _qgb.RhsCurrent(gcgb, vgb);

            // Load current vector
            var ceqbs = _mbp.MosfetType * (cbs - gbs * vbs);
            var ceqbd = _mbp.MosfetType * (cbd - gbd * vbd);
            GatePtr.Value -= _mbp.MosfetType * (ceqgs + ceqgb + ceqgd);
            BulkPtr.Value -= ceqbs + ceqbd - _mbp.MosfetType * ceqgb;
            DrainPrimePtr.Value += ceqbd + _mbp.MosfetType * ceqgd;
            SourcePrimePtr.Value += ceqbs + _mbp.MosfetType * ceqgs;

            // Load y matrix
            GateGatePtr.Value += gcgd + gcgs + gcgb;
            BulkBulkPtr.Value += gbd + gbs + gcgb;
            DrainPrimeDrainPrimePtr.Value += gbd + gcgd;
            SourcePrimeSourcePrimePtr.Value += gbs + gcgs;
            GateBulkPtr.Value -= gcgb;
            GateDrainPrimePtr.Value -= gcgd;
            GateSourcePrimePtr.Value -= gcgs;
            BulkGatePtr.Value -= gcgb;
            BulkDrainPrimePtr.Value -= gbd;
            BulkSourcePrimePtr.Value -= gbs;
            DrainPrimeGatePtr.Value += -gcgd;
            DrainPrimeBulkPtr.Value += -gbd;
            SourcePrimeGatePtr.Value += -gcgs;
            SourcePrimeBulkPtr.Value += -gbs;
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <returns>The timestep that satisfies the LTE</returns>
        public override double Truncate()
        {
            double timetmp = _qgs.LocalTruncationError();
            timetmp = Math.Min(timetmp, _qgd.LocalTruncationError());
            timetmp = Math.Min(timetmp, _qgb.LocalTruncationError());
            return timetmp;
        }
    }
}
