using System.Numerics;
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
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        /// <param name="context"></param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context) 
        {
            ComplexState = context.GetState<IComplexSimulationState>();
            _drainNode = ComplexState.Map[context.Nodes[0]];
            _gateNode = ComplexState.Map[context.Nodes[1]];
            _sourceNode = ComplexState.Map[context.Nodes[2]];
            _bulkNode = ComplexState.Map[context.Nodes[3]];
            _drainNodePrime = ComplexState.Map[DrainPrime];
            _sourceNodePrime = ComplexState.Map[SourcePrime];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_bulkNode, _bulkNode),
                new MatrixLocation(_drainNodePrime, _drainNodePrime),
                new MatrixLocation(_sourceNodePrime, _sourceNodePrime),
                new MatrixLocation(_gateNode, _bulkNode),
                new MatrixLocation(_gateNode, _drainNodePrime),
                new MatrixLocation(_gateNode, _sourceNodePrime),
                new MatrixLocation(_bulkNode, _gateNode),
                new MatrixLocation(_bulkNode, _drainNodePrime),
                new MatrixLocation(_bulkNode, _sourceNodePrime),
                new MatrixLocation(_drainNodePrime, _gateNode),
                new MatrixLocation(_drainNodePrime, _bulkNode),
                new MatrixLocation(_sourceNodePrime, _gateNode),
                new MatrixLocation(_sourceNodePrime, _bulkNode),
                new MatrixLocation(_drainNode, _drainNode),
                new MatrixLocation(_sourceNode, _sourceNode),
                new MatrixLocation(_drainNode, _drainNodePrime),
                new MatrixLocation(_sourceNode, _sourceNodePrime),
                new MatrixLocation(_drainNodePrime, _drainNode),
                new MatrixLocation(_drainNodePrime, _sourceNodePrime),
                new MatrixLocation(_sourceNodePrime, _sourceNode),
                new MatrixLocation(_sourceNodePrime, _drainNodePrime));
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
