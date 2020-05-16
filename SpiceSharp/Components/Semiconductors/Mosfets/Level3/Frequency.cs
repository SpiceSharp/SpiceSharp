﻿using System.Numerics;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.Mosfets.Level3
{
    /// <summary>
    /// Small-signal behavior for a <see cref="Mosfet3" />.
    /// </summary>
    /// <seealso cref="Dynamic"/>
    /// <seealso cref="IFrequencyBehavior"/>
    public class Frequency : Dynamic,
        IFrequencyBehavior
    {
        private readonly ElementSet<Complex> _elements;
        private readonly IComplexSimulationState _complex;
        private readonly int _drainNode, _gateNode, _sourceNode, _bulkNode, _drainNodePrime, _sourceNodePrime;

        /// <summary>
        /// Gets the internal drain node.
        /// </summary>
        /// <value>
        /// The internal drain node.
        /// </value>
        protected new IVariable<Complex> DrainPrime { get; }

        /// <summary>
        /// Gets the internal source node.
        /// </summary>
        /// <value>
        /// The internal source node.
        /// </value>
        protected new IVariable<Complex> SourcePrime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Frequency"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public Frequency(string name, ComponentBindingContext context) : base(name, context)
        {
            _complex = context.GetState<IComplexSimulationState>();

            DrainPrime = _complex.GetSharedVariable(context.Nodes[0]);
            _drainNode = _complex.Map[DrainPrime];
            _gateNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[1])];
            SourcePrime = _complex.GetSharedVariable(context.Nodes[2]);
            _sourceNode = _complex.Map[SourcePrime];
            _bulkNode = _complex.Map[_complex.GetSharedVariable(context.Nodes[3])];

            // Add series drain node if necessary
            if (!ModelParameters.DrainResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.DrainSquares > 0)
                DrainPrime = _complex.CreatePrivateVariable(Name.Combine("drain"), Units.Volt);
            _drainNodePrime = _complex.Map[DrainPrime];

            // Add series source node if necessary
            if (!ModelParameters.SourceResistance.Equals(0.0) || !ModelParameters.SheetResistance.Equals(0.0) && Parameters.SourceSquares > 0)
                SourcePrime = _complex.CreatePrivateVariable(Name.Combine("source"), Units.Volt);
            _sourceNodePrime = _complex.Map[SourcePrime];

            _elements = new ElementSet<Complex>(_complex.Solver,
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

        void IFrequencyBehavior.InitializeParameters()
        {
            CalculateBaseCapacitances();
            CalculateCapacitances(VoltageDs, VoltageBs);
            CalculateMeyerCharges(VoltageGs, VoltageGs - VoltageDs);
        }

        void IFrequencyBehavior.Load()
        {
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
            var effectiveLength = Parameters.Length - 2 * ModelParameters.LateralDiffusion;
            var gateSourceOverlapCap = ModelParameters.GateSourceOverlapCapFactor * Parameters.Width;
            var gateDrainOverlapCap = ModelParameters.GateDrainOverlapCapFactor * Parameters.Width;
            var gateBulkOverlapCap = ModelParameters.GateBulkOverlapCapFactor * effectiveLength;

            // Meyer"s model parameters
            var capgs = CapGs + CapGs + gateSourceOverlapCap;
            var capgd = CapGd + CapGd + gateDrainOverlapCap;
            var capgb = CapGb + CapGb + gateBulkOverlapCap;
            var xgs = capgs * _complex.Laplace.Imaginary;
            var xgd = capgd * _complex.Laplace.Imaginary;
            var xgb = capgb * _complex.Laplace.Imaginary;
            var xbd = CapBd * _complex.Laplace.Imaginary;
            var xbs = CapBs * _complex.Laplace.Imaginary;

            // Load Y-matrix
            _elements.Add(
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