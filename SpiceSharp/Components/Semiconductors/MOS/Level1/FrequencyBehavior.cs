using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.MosfetBehaviors.Level1
{
    /// <summary>
    /// Frequency behavior for a <see cref="Mosfet1" />.
    /// </summary>
    public class FrequencyBehavior : DynamicParameterBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex simulation state.
        /// </summary>
        /// <value>
        /// The complex simulation state.
        /// </value>
        protected ComplexSimulationState ComplexState { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public FrequencyBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.GetValue<ComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(GateNode, GateNode),
                new MatrixLocation(BulkNode, BulkNode),
                new MatrixLocation(DrainNodePrime, DrainNodePrime),
                new MatrixLocation(SourceNodePrime, SourceNodePrime),
                new MatrixLocation(GateNode, BulkNode),
                new MatrixLocation(GateNode, DrainNodePrime),
                new MatrixLocation(GateNode, SourceNodePrime),
                new MatrixLocation(BulkNode, GateNode),
                new MatrixLocation(BulkNode, DrainNodePrime),
                new MatrixLocation(BulkNode, SourceNodePrime),
                new MatrixLocation(DrainNodePrime, GateNode),
                new MatrixLocation(DrainNodePrime, BulkNode),
                new MatrixLocation(SourceNodePrime, GateNode),
                new MatrixLocation(SourceNodePrime, BulkNode),
                new MatrixLocation(DrainNode, DrainNode),
                new MatrixLocation(SourceNode, SourceNode),
                new MatrixLocation(DrainNode, DrainNodePrime),
                new MatrixLocation(SourceNode, SourceNodePrime),
                new MatrixLocation(DrainNodePrime, DrainNode),
                new MatrixLocation(DrainNodePrime, SourceNodePrime),
                new MatrixLocation(SourceNodePrime, SourceNode),
                new MatrixLocation(SourceNodePrime, DrainNodePrime));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexState = null;
            ComplexElements?.Destroy();
            ComplexElements = null;
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
            var cstate = ComplexState.ThrowIfNotBound(this);
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
            ComplexElements.Add(
                new Complex(0.0, xgd + xgs + xgb),
                new Complex(CondBd + CondBs, xgb + xbd + xbs),
                new Complex(DrainConductance + CondDs + CondBd + xrev * (Transconductance + TransconductanceBs), xgd + xbd),
                new Complex(SourceConductance + CondDs + CondBs + xnrm * (Transconductance + TransconductanceBs), xgs + xbs),
                -new Complex(0.0, xgb),
                -new Complex(0.0, xgd),
                -new Complex(0.0, xgs),
                -new Complex(0.0, xgb),
                -new Complex(CondBd, xbd),
                -new Complex(CondBs, xbs),
                new Complex((xnrm - xrev) * Transconductance, -xgd),
                new Complex(-CondBd + (xnrm - xrev) * TransconductanceBs, -xbd),
                -new Complex((xnrm - xrev) * Transconductance, xgs),
                -new Complex(CondBs + (xnrm - xrev) * TransconductanceBs, xbs),
                DrainConductance,
                SourceConductance,
                -DrainConductance,
                -SourceConductance,
                -DrainConductance,
                -CondDs - xnrm * (Transconductance + TransconductanceBs),
                -SourceConductance,
                -CondDs - xrev * (Transconductance + TransconductanceBs));
        }
    }
}
