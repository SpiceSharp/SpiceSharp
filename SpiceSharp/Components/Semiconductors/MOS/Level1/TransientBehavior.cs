using System;
using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Transient behavior for a <see cref="Mosfet1"/>
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
        /// Shared parameters
        /// </summary>
        [ParameterName("cbd"), ParameterInfo("Bulk-Drain capacitance")]
        public double CapBd { get; protected set; }
        [ParameterName("cbs"), ParameterInfo("Bulk-Source capacitance")]
        public double CapBs { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        private int _drainNode, _gateNode, _sourceNode, _bulkNode, _sourceNodePrime, _drainNodePrime;
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
        /// State variables
        /// </summary>
        protected StateDerivative ChargeGs { get; private set; }
        protected StateDerivative ChargeGd { get; private set; }
        protected StateDerivative ChargeGb { get; private set; }
        protected StateDerivative ChargeBd { get; private set; }
        protected StateDerivative ChargeBs { get; private set; }
        protected StateHistory CapGs { get; private set; }
        protected StateHistory CapGd { get; private set; }
        protected StateHistory CapGb { get; private set; }
        protected StateHistory VoltageGs { get; private set; }
        protected StateHistory VoltageDs { get; private set; }
        protected StateHistory VoltageBs { get; private set; }

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

            // Get matrix pointers
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

            // Get rhs pointers
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

            ChargeGs = states.CreateDerivative();
            ChargeGd = states.CreateDerivative();
            ChargeGb = states.CreateDerivative();
            ChargeBd = states.CreateDerivative();
            ChargeBs = states.CreateDerivative();

            CapGs = states.CreateHistory();
            CapGd = states.CreateHistory();
            CapGb = states.CreateHistory();
            VoltageGs = states.CreateHistory();
            VoltageDs = states.CreateHistory();
            VoltageBs = states.CreateHistory();
        }

        /// <summary>
        /// Calculate DC state variables
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void GetDcState(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double arg, sarg, sargsw;

            // Get voltages
            double vbd = _load.VoltageBd;
            double vbs = _load.VoltageBs;
            double vgs = _load.VoltageGs;
            double vds = _load.VoltageDs;
            double vgd = vgs - vds;
            double vgb = vgs - vbs;

            double effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            double gateSourceOverlapCap = _mbp.GateSourceOverlapCapFactor * _bp.Width;
            double gateDrainOverlapCap = _mbp.GateDrainOverlapCapFactor * _bp.Width;
            double gateBulkOverlapCap = _mbp.GateBulkOverlapCapFactor * effectiveLength;
            double oxideCap = _modeltemp.OxideCapFactor * effectiveLength * _bp.Width;
            
            if (vbs < _temp.TempDepletionCap)
            {
                arg = 1 - vbs / _temp.TempBulkPotential;
                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(_mbp.BulkJunctionSideGradingCoefficient.Value))
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
                Transistor.MeyerCharges(vgs, vgd, _mbp.MosfetType * _load.Von, _mbp.MosfetType * _load.SaturationVoltageDs,
                    out icapgs, out icapgd, out icapgb,
                    _temp.TempPhi, oxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, _mbp.MosfetType * _load.Von, _mbp.MosfetType * _load.SaturationVoltageDs,
                    out icapgd, out icapgs, out icapgb,
                    _temp.TempPhi, oxideCap);
            }
            CapGs.Current = icapgs;
            CapGd.Current = icapgd;
            CapGb.Current = icapgb;
            var capgs = 2 * CapGs.Current + gateSourceOverlapCap;
            var capgd = 2 * CapGd.Current + gateDrainOverlapCap;
            var capgb = 2 * CapGb.Current + gateBulkOverlapCap;

            /* TRANOP only */
            ChargeGs.Current = vgs * capgs;
            ChargeGd.Current = vgd * capgd;
            ChargeGb.Current = vgb * capgb;

            // Store these voltages
            VoltageGs.Current = vgs;
            VoltageDs.Current = vds;
            VoltageBs.Current = vbs;
        }

        /// <summary>
        /// Transient behavior
        /// </summary>
        /// <param name="simulation">Time-based simulation</param>
        public override void Transient(TimeSimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double arg, sarg, sargsw;

            // Get voltages
            double vbd = _load.VoltageBd;
            double vbs = _load.VoltageBs;
            double vgs = _load.VoltageGs;
            double vds = _load.VoltageDs;
            double vgd = vgs - vds;
            double vgb = vgs - vbs;

            double effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            double gateSourceOverlapCap = _mbp.GateSourceOverlapCapFactor * _bp.Width;
            double gateDrainOverlapCap = _mbp.GateDrainOverlapCapFactor * _bp.Width;
            double gateBulkOverlapCap = _mbp.GateBulkOverlapCapFactor * effectiveLength;
            double oxideCap = _modeltemp.OxideCapFactor * effectiveLength * _bp.Width;

            double gbd = 0.0;
            double cbd = 0.0;
            double gbs = 0.0;
            double cbs = 0.0;

            // Store these voltages
            VoltageGs.Current = vgs;
            VoltageDs.Current = vds;
            VoltageBs.Current = vbs;

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
            /* CAPBYPASS */

            /* can't bypass the diode capacitance calculations */
            /* CAPZEROBYPASS */
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
                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(_mbp.BulkJunctionSideGradingCoefficient.Value))
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
                ChargeBs.Current = _temp.TempBulkPotential * (_temp.CapBs * (1 - arg * sarg) / (1 - _mbp.BulkJunctionBotGradingCoefficient) +
                    _temp.CapBsSidewall * (1 - arg * sargsw) / (1 - _mbp.BulkJunctionSideGradingCoefficient));
                CapBs = _temp.CapBs * sarg + _temp.CapBsSidewall * sargsw;
            }
            else
            {
                ChargeBs.Current = _temp.F4S + vbs * (_temp.F2S + vbs * (_temp.F3S / 2));
                CapBs = _temp.F2S + _temp.F3S * vbs;
            }

            /* can't bypass the diode capacitance calculations */

            /* CAPZEROBYPASS */
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
            // NOTE: The derivative of Qbd should be added to Cd (drain current). Figure out a way later.
            ChargeBs.Integrate();
            gbs += ChargeBs.Jacobian(CapBs);
            cbs += ChargeBs.Derivative;

            /* 
             * calculate meyer's capacitors
             */
            /* 
             * new cmeyer - this just evaluates at the current time, 
             * expects you to remember values from previous time
             * returns 1 / 2 of non-constant portion of capacitance
             * you must add in the other half from previous time
             * and the constant part
             */
            double icapgs, icapgd, icapgb;
            if (_load.Mode > 0)
            {
                Transistor.MeyerCharges(vgs, vgd,  _mbp.MosfetType * _load.Von, _mbp.MosfetType * _load.SaturationVoltageDs,
                    out icapgs, out icapgd, out icapgb,
                    _temp.TempPhi, oxideCap);
            }
            else
            {
                Transistor.MeyerCharges(vgd, vgs, _mbp.MosfetType * _load.Von, _mbp.MosfetType * _load.SaturationVoltageDs,
                    out icapgd, out icapgs, out icapgb,
                    _temp.TempPhi, oxideCap);
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


            /* NOTE: We can't reset derivatives!
            if (capgs == 0)
                state.States[0][States + Cqgs] = 0;
            if (capgd == 0)
                state.States[0][States + Cqgd] = 0;
            if (capgb == 0)
                state.States[0][States + Cqgb] = 0;
            */

            /* NOTE: The formula with the method.Slope is to make it work for nonlinear capacitances!
             * The correct formula is: ceq = dQ/dt - geq * vq where geq = slope * dQ/dvq
             * The formula in Spice 3f5 is: ceq = dQ/dt - slope * Q where it assumes a linear capacitance
            method.Integrate(state, out gcgs, out ceqgs, States + Qgs, capgs);
            method.Integrate(state, out gcgd, out ceqgd, States + Qgd, capgd);
            method.Integrate(state, out gcgb, out ceqgb, States + Qgb, capgb);
            ceqgs = ceqgs - gcgs * vgs + method.Slope * state.States[0][States + Qgs];
            ceqgd = ceqgd - gcgd * vgd + method.Slope * state.States[0][States + Qgd];
            ceqgb = ceqgb - gcgb * vgb + method.Slope * state.States[0][States + Qgb];
            */

            ChargeGs.Integrate();
            double gcgs = ChargeGs.Jacobian(capgs);
            double ceqgs = ChargeGs.RhsCurrent(gcgs, vgs);
            ChargeGd.Integrate();
            double gcgd = ChargeGd.Jacobian(capgd);
            double ceqgd = ChargeGd.RhsCurrent(gcgd, vgd);
            ChargeGb.Integrate();
            double gcgb = ChargeGb.Jacobian(capgb);
            double ceqgb = ChargeGb.RhsCurrent(gcgb, vgb);

            // Load current vector
            double ceqbs = _mbp.MosfetType * (cbs - gbs * vbs);
            double ceqbd = _mbp.MosfetType * (cbd - gbd * vbd);
            GatePtr.Value -= _mbp.MosfetType * (ceqgs + ceqgb + ceqgd);
            BulkPtr.Value -= ceqbs + ceqbd - _mbp.MosfetType * ceqgb;
            DrainPrimePtr.Value += ceqbd + _mbp.MosfetType * ceqgd;
            SourcePrimePtr.Value += ceqbs + _mbp.MosfetType * ceqgs;

            // Load Y-matrix
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
            DrainPrimeGatePtr.Value -= gcgd;
            DrainPrimeBulkPtr.Value -= gbd;
            SourcePrimeGatePtr.Value -= gcgs;
            SourcePrimeBulkPtr.Value -= gbs;
        }

        /// <summary>
        /// Truncate timestep
        /// </summary>
        /// <returns>The timestep that satisfies the LTE</returns>
        public override double Truncate()
        {
            double timetmp = ChargeGs.LocalTruncationError();
            timetmp = Math.Min(timetmp, ChargeGd.LocalTruncationError());
            timetmp = Math.Min(timetmp, ChargeGb.LocalTruncationError());
            return timetmp;
        }
    }
}
