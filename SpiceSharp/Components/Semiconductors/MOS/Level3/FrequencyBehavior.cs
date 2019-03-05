using System;
using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Algebra.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Frequency behavior for a <see cref="Mosfet3" />.
    /// </summary>
    /// <seealso cref="SpiceSharp.Components.MosfetBehaviors.Level3.BiasingBehavior" />
    /// <seealso cref="SpiceSharp.Behaviors.IFrequencyBehavior" />
    public class FrequencyBehavior : DynamicParameterBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Nodes
        /// </summary>
        protected MatrixElement<PreciseComplex> CDrainDrainPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CGateGatePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourceSourcePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBulkBulkPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CDrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CDrainDrainPrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CGateBulkPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CGateDrainPrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CGateSourcePrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourceSourcePrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBulkDrainPrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBulkSourcePrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CDrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CDrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CBulkGatePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CDrainPrimeGatePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourcePrimeGatePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<PreciseComplex> CDrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<PreciseComplex> CSourcePrimeDrainPrimePtr { get; private set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        public void InitializeParameters(FrequencySimulation simulation)
        {
            if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));
            CalculateBaseCapacitances();
            CalculateCapacitances(VoltageGs, VoltageDs, VoltageBs);
            CalculateMeyerCharges(VoltageGs, VoltageGs - VoltageDs);
        }

        /// <summary>
        /// Gets matrix pionters
        /// </summary>
        /// <param name="solver">Matrix</param>
        public void GetEquationPointers(Solver<PreciseComplex> solver)
        {
			if (solver == null)
				throw new ArgumentNullException(nameof(solver));

            // Get matrix pointers
            CDrainDrainPtr = solver.GetMatrixElement(DrainNode, DrainNode);
            CGateGatePtr = solver.GetMatrixElement(GateNode, GateNode);
            CSourceSourcePtr = solver.GetMatrixElement(SourceNode, SourceNode);
            CBulkBulkPtr = solver.GetMatrixElement(BulkNode, BulkNode);
            CDrainPrimeDrainPrimePtr = solver.GetMatrixElement(DrainNodePrime, DrainNodePrime);
            CSourcePrimeSourcePrimePtr = solver.GetMatrixElement(SourceNodePrime, SourceNodePrime);
            CDrainDrainPrimePtr = solver.GetMatrixElement(DrainNode, DrainNodePrime);
            CGateBulkPtr = solver.GetMatrixElement(GateNode, BulkNode);
            CGateDrainPrimePtr = solver.GetMatrixElement(GateNode, DrainNodePrime);
            CGateSourcePrimePtr = solver.GetMatrixElement(GateNode, SourceNodePrime);
            CSourceSourcePrimePtr = solver.GetMatrixElement(SourceNode, SourceNodePrime);
            CBulkDrainPrimePtr = solver.GetMatrixElement(BulkNode, DrainNodePrime);
            CBulkSourcePrimePtr = solver.GetMatrixElement(BulkNode, SourceNodePrime);
            CDrainPrimeSourcePrimePtr = solver.GetMatrixElement(DrainNodePrime, SourceNodePrime);
            CDrainPrimeDrainPtr = solver.GetMatrixElement(DrainNodePrime, DrainNode);
            CBulkGatePtr = solver.GetMatrixElement(BulkNode, GateNode);
            CDrainPrimeGatePtr = solver.GetMatrixElement(DrainNodePrime, GateNode);
            CSourcePrimeGatePtr = solver.GetMatrixElement(SourceNodePrime, GateNode);
            CSourcePrimeSourcePtr = solver.GetMatrixElement(SourceNodePrime, SourceNode);
            CDrainPrimeBulkPtr = solver.GetMatrixElement(DrainNodePrime, BulkNode);
            CSourcePrimeBulkPtr = solver.GetMatrixElement(SourceNodePrime, BulkNode);
            CSourcePrimeDrainPrimePtr = solver.GetMatrixElement(SourceNodePrime, DrainNodePrime);
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        /// <param name="simulation">The frequency simulation.</param>
        /// <exception cref="ArgumentNullException">simulation</exception>
        public void Load(FrequencySimulation simulation)
        {
            if (simulation == null)
				throw new ArgumentNullException(nameof(simulation));

            var cstate = simulation.ComplexState;
            int xnrm, xrev;

            if (Mode < 0)
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
            var effectiveLength = BaseParameters.Length - 2 * ModelParameters.LateralDiffusion;
            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * BaseParameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * BaseParameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * effectiveLength;

            // Meyer"s model parameters
            var capgs = CapGs + CapGs + gateSourceOverlapCap;
            var capgd = CapGd + CapGd + gateDrainOverlapCap;
            var capgb = CapGb + CapGb + gateBulkOverlapCap;
            var xgs = (decimal)capgs * cstate.Laplace.Imaginary;
            var xgd = (decimal)capgd * cstate.Laplace.Imaginary;
            var xgb = (decimal)capgb * cstate.Laplace.Imaginary;
            var xbd = (decimal)CapBd * cstate.Laplace.Imaginary;
            var xbs = (decimal)CapBs * cstate.Laplace.Imaginary;

            // Load Y-matrix
            CGateGatePtr.Value += new PreciseComplex(0.0, xgd + xgs + xgb);
            CBulkBulkPtr.Value += new PreciseComplex(CondBd + CondBs, xgb + xbd + xbs);
            CDrainPrimeDrainPrimePtr.Value += new PreciseComplex(DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs), xgd + xbd);
            CSourcePrimeSourcePrimePtr.Value += new PreciseComplex(SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs), xgs + xbs);
            CGateBulkPtr.Value -= new PreciseComplex(0.0, xgb);
            CGateDrainPrimePtr.Value -= new PreciseComplex(0.0, xgd);
            CGateSourcePrimePtr.Value -= new PreciseComplex(0.0, xgs);
            CBulkGatePtr.Value -= new PreciseComplex(0.0, xgb);
            CBulkDrainPrimePtr.Value -= new PreciseComplex(CondBd, xbd);
            CBulkSourcePrimePtr.Value -= new PreciseComplex(CondBs, xbs);
            CDrainPrimeGatePtr.Value += new PreciseComplex((xnrm - xrev) * Transconductance, -xgd);
            CDrainPrimeBulkPtr.Value += new PreciseComplex(-CondBd + (xnrm - xrev) * TransconductanceBs, -xbd);
            CSourcePrimeGatePtr.Value -= new PreciseComplex((xnrm - xrev) * Transconductance, xgs);
            CSourcePrimeBulkPtr.Value -= new PreciseComplex(CondBs + (xnrm - xrev) * TransconductanceBs, xbs);
            CDrainDrainPtr.Value += DrainConductance;
            CSourceSourcePtr.Value += SourceConductance;
            CDrainDrainPrimePtr.Value -= DrainConductance;
            CSourceSourcePrimePtr.Value -= SourceConductance;
            CDrainPrimeDrainPtr.Value -= DrainConductance;
            CDrainPrimeSourcePrimePtr.Value -= CondDs + xnrm * (Transconductance + TransconductanceBs);
            CSourcePrimeSourcePtr.Value -= SourceConductance;
            CSourcePrimeDrainPrimePtr.Value -= CondDs + xrev * (Transconductance + TransconductanceBs);
        }
    }
}
