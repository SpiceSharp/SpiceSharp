using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Simulations.Behaviors;

namespace SpiceSharp.Components.MosfetBehaviors.Common
{
    /// <summary>
    /// Common small-signal behavior for a MOS transistor.
    /// </summary>
    public class FrequencyBehavior : ExportingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Necessary behaviors
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private BiasingBehavior _load;
        private TemperatureBehavior _temp;

        /// <summary>
        /// Shared variables
        /// </summary>
        public double CapBs { get; protected set; }
        public double CapBd { get; protected set; }
        public double CapGs { get; protected set; }
        public double CapGd { get; protected set; }
        public double CapGb { get; protected set; }

        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<Complex> DrainDrainPtr { get; private set; }
        protected MatrixElement<Complex> GateGatePtr { get; private set; }
        protected MatrixElement<Complex> SourceSourcePtr { get; private set; }
        protected MatrixElement<Complex> BulkBulkPtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> GateBulkPtr { get; private set; }
        protected MatrixElement<Complex> GateDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> GateSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> SourceSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> BulkDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> BulkSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<Complex> BulkGatePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<Complex> DrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<Complex> SourcePrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

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
        /// Gets matrix pionters
        /// </summary>
        /// <param name="solver">Matrix</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get matrix pointers
            DrainDrainPtr = solver.GetMatrixElement(_load.DrainNode, _load.DrainNode);
            GateGatePtr = solver.GetMatrixElement(_load.GateNode, _load.GateNode);
            SourceSourcePtr = solver.GetMatrixElement(_load.SourceNode, _load.SourceNode);
            BulkBulkPtr = solver.GetMatrixElement(_load.BulkNode, _load.BulkNode);
            DrainPrimeDrainPrimePtr = solver.GetMatrixElement(_load.DrainNodePrime, _load.DrainNodePrime);
            SourcePrimeSourcePrimePtr = solver.GetMatrixElement(_load.SourceNodePrime, _load.SourceNodePrime);
            DrainDrainPrimePtr = solver.GetMatrixElement(_load.DrainNode, _load.DrainNodePrime);
            GateBulkPtr = solver.GetMatrixElement(_load.GateNode, _load.BulkNode);
            GateDrainPrimePtr = solver.GetMatrixElement(_load.GateNode, _load.DrainNodePrime);
            GateSourcePrimePtr = solver.GetMatrixElement(_load.GateNode, _load.SourceNodePrime);
            SourceSourcePrimePtr = solver.GetMatrixElement(_load.SourceNode, _load.SourceNodePrime);
            BulkDrainPrimePtr = solver.GetMatrixElement(_load.BulkNode, _load.DrainNodePrime);
            BulkSourcePrimePtr = solver.GetMatrixElement(_load.BulkNode, _load.SourceNodePrime);
            DrainPrimeSourcePrimePtr = solver.GetMatrixElement(_load.DrainNodePrime, _load.SourceNodePrime);
            DrainPrimeDrainPtr = solver.GetMatrixElement(_load.DrainNodePrime, _load.DrainNode);
            BulkGatePtr = solver.GetMatrixElement(_load.BulkNode, _load.GateNode);
            DrainPrimeGatePtr = solver.GetMatrixElement(_load.DrainNodePrime, _load.GateNode);
            SourcePrimeGatePtr = solver.GetMatrixElement(_load.SourceNodePrime, _load.GateNode);
            SourcePrimeSourcePtr = solver.GetMatrixElement(_load.SourceNodePrime, _load.SourceNode);
            DrainPrimeBulkPtr = solver.GetMatrixElement(_load.DrainNodePrime, _load.BulkNode);
            SourcePrimeBulkPtr = solver.GetMatrixElement(_load.SourceNodePrime, _load.BulkNode);
            SourcePrimeDrainPrimePtr = solver.GetMatrixElement(_load.SourceNodePrime, _load.DrainNodePrime);
        }
        
        /// <summary>
        /// Initialize AC parameters
        /// </summary>
        /// <param name="simulation"></param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            double sargsw;

            var vbs = _load.VoltageBs;
            var vbd = _load.VoltageBd;
            var vgs = _load.VoltageGs;
            var vds = _load.VoltageDs;
            var vgd = vgs - vds;
            var von = _mbp.MosfetType * _load.Von;
            var vdsat = _mbp.MosfetType * _load.SaturationVoltageDs;

            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
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
                double arg = 1 - vbs / _temp.TempBulkPotential, sarg;

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
                CapBs = _temp.CapBs * sarg + _temp.CapBsSidewall * sargsw;
            }
            else
            {
                CapBs = _temp.F2S + _temp.F3S * vbs;
            }

            if (vbd < _temp.TempDepletionCap)
            {
                double arg = 1 - vbd / _temp.TempBulkPotential, sarg;

                if (_mbp.BulkJunctionBotGradingCoefficient.Value.Equals(0.5) && _mbp.BulkJunctionSideGradingCoefficient.Value.Equals(0.5))
                {
                    sarg = sargsw = 1 / Math.Sqrt(arg);
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
                CapBd = _temp.CapBd * sarg + _temp.CapBdSidewall * sargsw;
            }
            else
                CapBd = _temp.F2D + vbd * _temp.F3D;

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
            CapGs = icapgs;
            CapGd = icapgd;
            CapGb = icapgb;
        }

        /// <summary>
        /// Execute behavior for AC analysis
        /// </summary>
        /// <param name="simulation">Frequency-based simulation</param>
        public void Load(FrequencySimulation simulation)
        {
			if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var cstate = simulation.ComplexState;
            int xnrm, xrev;

            if (_load.Mode < 0)
            {
                xnrm = 0;
                xrev = 1;
            }
            else
            {
                xnrm = 1;
                xrev = 0;
            }

            // Charge oriented model parameters
            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var gateSourceOverlapCap = _mbp.GateSourceOverlapCapFactor * _bp.Width;
            var gateDrainOverlapCap = _mbp.GateDrainOverlapCapFactor * _bp.Width;
            var gateBulkOverlapCap = _mbp.GateBulkOverlapCapFactor * effectiveLength;

            // Meyer"s model parameters
            var capgs = CapGs + CapGs + gateSourceOverlapCap;
            var capgd = CapGd + CapGd + gateDrainOverlapCap;
            var capgb = CapGb + CapGb + gateBulkOverlapCap;
            var xgs = capgs * cstate.Laplace.Imaginary;
            var xgd = capgd * cstate.Laplace.Imaginary;
            var xgb = capgb * cstate.Laplace.Imaginary;
            var xbd = CapBd * cstate.Laplace.Imaginary;
            var xbs = CapBs * cstate.Laplace.Imaginary;

            // Load Y-matrix
            GateGatePtr.Value += new Complex(0.0, xgd + xgs + xgb);
            BulkBulkPtr.Value += new Complex(_load.CondBd + _load.CondBs, xgb + xbd + xbs);
            DrainPrimeDrainPrimePtr.Value += new Complex(_temp.DrainConductance + _load.CondDs + _load.CondBd + xrev * (_load.Transconductance + _load.TransconductanceBs), xgd + xbd);
            SourcePrimeSourcePrimePtr.Value += new Complex(_temp.SourceConductance + _load.CondDs + _load.CondBs + xnrm * (_load.Transconductance + _load.TransconductanceBs), xgs + xbs);
            GateBulkPtr.Value -= new Complex(0.0, xgb);
            GateDrainPrimePtr.Value -= new Complex(0.0, xgd);
            GateSourcePrimePtr.Value -= new Complex(0.0, xgs);
            BulkGatePtr.Value -= new Complex(0.0, xgb);
            BulkDrainPrimePtr.Value -= new Complex(_load.CondBd, xbd);
            BulkSourcePrimePtr.Value -= new Complex(_load.CondBs, xbs);
            DrainPrimeGatePtr.Value += new Complex((xnrm - xrev) * _load.Transconductance, -xgd);
            DrainPrimeBulkPtr.Value += new Complex(-_load.CondBd + (xnrm - xrev) * _load.TransconductanceBs, -xbd);
            SourcePrimeGatePtr.Value -= new Complex((xnrm - xrev) * _load.Transconductance, xgs);
            SourcePrimeBulkPtr.Value -= new Complex(_load.CondBs + (xnrm - xrev) * _load.TransconductanceBs, xbs);
            DrainDrainPtr.Value += _temp.DrainConductance;
            SourceSourcePtr.Value += _temp.SourceConductance;
            DrainDrainPrimePtr.Value -= _temp.DrainConductance;
            SourceSourcePrimePtr.Value -= _temp.SourceConductance;
            DrainPrimeDrainPtr.Value -= _temp.DrainConductance;
            DrainPrimeSourcePrimePtr.Value -= _load.CondDs + xnrm * (_load.Transconductance + _load.TransconductanceBs);
            SourcePrimeSourcePtr.Value -= _temp.SourceConductance;
            SourcePrimeDrainPrimePtr.Value -= _load.CondDs + xrev * (_load.Transconductance + _load.TransconductanceBs);
        }
    }
}
