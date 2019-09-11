﻿using SpiceSharp.Algebra;
using SpiceSharp.Circuits;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;

namespace SpiceSharp.Components.ResistorBehaviors
{
    /// <summary>
    /// General behavior for <see cref="Resistor"/>
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the voltage across the resistor.
        /// </summary>
        [ParameterName("v"), ParameterInfo("Voltage")]
        public double GetVoltage() => BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode];

        /// <summary>
        /// Gets the current through the resistor.
        /// </summary>
        [ParameterName("i"), ParameterInfo("Current")]
        public double GetCurrent() => (BiasingState.ThrowIfNotBound(this).Solution[PosNode] - BiasingState.Solution[NegNode]) * Conductance;

        /// <summary>
        /// Gets the power dissipated by the resistor.
        /// </summary>
        [ParameterName("p"), ParameterInfo("Power")]
        public double GetPower()
        {
            BiasingState.ThrowIfNotBound(this);
            var v = BiasingState.Solution[PosNode] - BiasingState.Solution[NegNode];
            return v * v * Conductance;
        }

        /// <summary>
        /// Gets the positive node.
        /// </summary>
        protected int PosNode { get; private set; }

        /// <summary>
        /// Gets the negative node.
        /// </summary>
        protected int NegNode { get; private set; }

        /// <summary>
        /// Gets the (positive, positive) element.
        /// </summary>
        protected IMatrixElement<double> PosPosPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, negative) element.
        /// </summary>
        protected IMatrixElement<double> NegNegPtr { get; private set; }

        /// <summary>
        /// Gets the (positive, negative) element.
        /// </summary>
        protected IMatrixElement<double> PosNegPtr { get; private set; }

        /// <summary>
        /// Gets the (negative, positive) element.
        /// </summary>
        protected IMatrixElement<double> NegPosPtr { get; private set; }

        /// <summary>
        /// Creates a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            if (context is ComponentBindingContext cc)
            {
                PosNode = cc.Pins[0];
                NegNode = cc.Pins[1];
            }

            var solver = BiasingState.Solver;
            PosPosPtr = solver.GetMatrixElement(PosNode, PosNode);
            NegNegPtr = solver.GetMatrixElement(NegNode, NegNode);
            PosNegPtr = solver.GetMatrixElement(PosNode, NegNode);
            NegPosPtr = solver.GetMatrixElement(NegNode, PosNode);
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();

            PosPosPtr = null;
            NegNegPtr = null;
            PosNegPtr = null;
            NegPosPtr = null;
        }

        /// <summary>
        /// Load the Y-matrix and Rhs-vector.
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var conductance = Conductance;
            PosPosPtr.Value += conductance;
            NegNegPtr.Value += conductance;
            PosNegPtr.Value -= conductance;
            NegPosPtr.Value -= conductance;
        }

        /// <summary>
        /// Tests convergence at the device-level.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the device determines the solution converges; otherwise, <c>false</c>.
        /// </returns>
        bool IBiasingBehavior.IsConvergent() => true;
    }
}
