using System;
using System.Numerics;
using SpiceSharp.Algebra;
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
        protected MatrixElement<Complex> CDrainDrainPtr { get; private set; }
        protected MatrixElement<Complex> CGateGatePtr { get; private set; }
        protected MatrixElement<Complex> CSourceSourcePtr { get; private set; }
        protected MatrixElement<Complex> CBulkBulkPtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CDrainDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CGateBulkPtr { get; private set; }
        protected MatrixElement<Complex> CGateDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CGateSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CSourceSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CBulkDrainPrimePtr { get; private set; }
        protected MatrixElement<Complex> CBulkSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeSourcePrimePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeDrainPtr { get; private set; }
        protected MatrixElement<Complex> CBulkGatePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeGatePtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeSourcePtr { get; private set; }
        protected MatrixElement<Complex> CDrainPrimeBulkPtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeBulkPtr { get; private set; }
        protected MatrixElement<Complex> CSourcePrimeDrainPrimePtr { get; private set; }
        
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
            simulation.ThrowIfNull(nameof(simulation));
            CalculateBaseCapacitances();
            CalculateCapacitances(VoltageGs, VoltageDs, VoltageBs);
            CalculateMeyerCharges(VoltageGs, VoltageGs - VoltageDs);
        }

        /// <summary>
        /// Gets matrix pionters
        /// </summary>
        /// <param name="solver">Matrix</param>
        public void GetEquationPointers(Solver<Complex> solver)
        {
			solver.ThrowIfNull(nameof(solver));

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
            simulation.ThrowIfNull(nameof(simulation));

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
            var xgs = capgs * cstate.Laplace.Imaginary;
            var xgd = capgd * cstate.Laplace.Imaginary;
            var xgb = capgb * cstate.Laplace.Imaginary;
            var xbd = CapBd * cstate.Laplace.Imaginary;
            var xbs = CapBs * cstate.Laplace.Imaginary;

            // Load Y-matrix
            CGateGatePtr.Value += new Complex(0.0, xgd + xgs + xgb);
            CBulkBulkPtr.Value += new Complex(CondBd + CondBs, xgb + xbd + xbs);
            CDrainPrimeDrainPrimePtr.Value += new Complex(DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs), xgd + xbd);
            CSourcePrimeSourcePrimePtr.Value += new Complex(SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs), xgs + xbs);
            CGateBulkPtr.Value -= new Complex(0.0, xgb);
            CGateDrainPrimePtr.Value -= new Complex(0.0, xgd);
            CGateSourcePrimePtr.Value -= new Complex(0.0, xgs);
            CBulkGatePtr.Value -= new Complex(0.0, xgb);
            CBulkDrainPrimePtr.Value -= new Complex(CondBd, xbd);
            CBulkSourcePrimePtr.Value -= new Complex(CondBs, xbs);
            CDrainPrimeGatePtr.Value += new Complex((xnrm - xrev) * Transconductance, -xgd);
            CDrainPrimeBulkPtr.Value += new Complex(-CondBd + (xnrm - xrev) * TransconductanceBs, -xbd);
            CSourcePrimeGatePtr.Value -= new Complex((xnrm - xrev) * Transconductance, xgs);
            CSourcePrimeBulkPtr.Value -= new Complex(CondBs + (xnrm - xrev) * TransconductanceBs, xbs);
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
