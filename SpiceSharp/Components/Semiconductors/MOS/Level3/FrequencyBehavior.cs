using System.Numerics;
using SpiceSharp.Algebra;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// Frequency behavior for a <see cref="Mosfet3" />.
    /// </summary>
    public class FrequencyBehavior : DynamicParameterBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the external (drain, drain) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainDrainPtr { get; private set; }

        /// <summary>
        /// Gets the (gate, gate) element.
        /// </summary>
        protected MatrixElement<Complex> CGateGatePtr { get; private set; }

        /// <summary>
        /// Gets the external (source, source) element.
        /// </summary>
        protected MatrixElement<Complex> CSourceSourcePtr { get; private set; }

        /// <summary>
        /// Gets the (bulk, bulk) element.
        /// </summary>
        protected MatrixElement<Complex> CBulkBulkPtr { get; private set; }

        /// <summary>
        /// Gets the (drain, drain) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainPrimeDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (source, source) element.
        /// </summary>
        protected MatrixElement<Complex> CSourcePrimeSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external drain, drain) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (gate, bulk) element.
        /// </summary>
        protected MatrixElement<Complex> CGateBulkPtr { get; private set; }

        /// <summary>
        /// Gets the (gate, drain) element.
        /// </summary>
        protected MatrixElement<Complex> CGateDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (gate, source) element.
        /// </summary>
        protected MatrixElement<Complex> CGateSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (external source, source) element.
        /// </summary>
        protected MatrixElement<Complex> CSourceSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (bulk, drain) element.
        /// </summary>
        protected MatrixElement<Complex> CBulkDrainPrimePtr { get; private set; }

        /// <summary>
        /// Gets the (bulk, source) element.
        /// </summary>
        protected MatrixElement<Complex> CBulkSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (drain, source) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainPrimeSourcePrimePtr { get; private set; }

        /// <summary>
        /// Gets the (drain, external drain) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainPrimeDrainPtr { get; private set; }

        /// <summary>
        /// Gets the (bulk, gate) element.
        /// </summary>
        protected MatrixElement<Complex> CBulkGatePtr { get; private set; }

        /// <summary>
        /// Gets the (drain, gate) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainPrimeGatePtr { get; private set; }

        /// <summary>
        /// Gets the (source, gate) element.
        /// </summary>
        protected MatrixElement<Complex> CSourcePrimeGatePtr { get; private set; }

        /// <summary>
        /// Gets the (source, external source) element.
        /// </summary>
        protected MatrixElement<Complex> CSourcePrimeSourcePtr { get; private set; }

        /// <summary>
        /// Gets the (drain, bulk) element.
        /// </summary>
        protected MatrixElement<Complex> CDrainPrimeBulkPtr { get; private set; }

        /// <summary>
        /// Gets the (source, bulk) element.
        /// </summary>
        protected MatrixElement<Complex> CSourcePrimeBulkPtr { get; private set; }

        /// <summary>
        /// Gets the (source, drain) element.
        /// </summary>
        protected MatrixElement<Complex> CSourcePrimeDrainPrimePtr { get; private set; }

        // Cache
        private ComplexSimulationState _state;

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation"></param>
        /// <param name="context"></param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            _state = ((FrequencySimulation)simulation).ComplexState;
            var solver = _state.Solver;
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
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            _state = null;
            CDrainDrainPtr = null;
            CGateGatePtr = null;
            CSourceSourcePtr = null;
            CBulkBulkPtr = null;
            CDrainPrimeDrainPrimePtr = null;
            CSourcePrimeSourcePrimePtr = null;
            CDrainDrainPrimePtr = null;
            CGateBulkPtr = null;
            CGateDrainPrimePtr = null;
            CGateSourcePrimePtr = null;
            CSourceSourcePrimePtr = null;
            CBulkDrainPrimePtr = null;
            CBulkSourcePrimePtr = null;
            CDrainPrimeSourcePrimePtr = null;
            CDrainPrimeDrainPtr = null;
            CBulkGatePtr = null;
            CDrainPrimeGatePtr = null;
            CSourcePrimeGatePtr = null;
            CSourcePrimeSourcePtr = null;
            CDrainPrimeBulkPtr = null;
            CSourcePrimeBulkPtr = null;
            CSourcePrimeDrainPrimePtr = null;
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            CalculateBaseCapacitances();
            CalculateCapacitances(VoltageGs, VoltageDs, VoltageBs);
            CalculateMeyerCharges(VoltageGs, VoltageGs - VoltageDs);
        }

        /// <summary>
        /// Load the Y-matrix and right-hand side vector for frequency domain analysis.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var cstate = _state.ThrowIfNotBound(this);
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
