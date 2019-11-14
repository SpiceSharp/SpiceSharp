﻿using System;
using System.Numerics;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Frequency behavior for a <see cref="JFET" />.
    /// </summary>
    public class FrequencyBehavior : BiasingBehavior, IFrequencyBehavior
    {
        /// <summary>
        /// Gets the gate-source capacitance.
        /// </summary>
        [ParameterName("capgs"), ParameterInfo("Capacitance G-S")]
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the gate-drain capacitance.
        /// </summary>
        [ParameterName("capgd"), ParameterInfo("Capacitance G-D")]
        public double CapGd { get; private set; }

        /// <summary>
        /// Gets the complex matrix elements.
        /// </summary>
        /// <value>
        /// The complex matrix elements.
        /// </value>
        protected ElementSet<Complex> ComplexElements { get; private set; }

        /// <summary>
        /// Gets the complex state.
        /// </summary>
        /// <value>
        /// The complex state.
        /// </value>
        protected IComplexSimulationState ComplexState { get; private set; }

        private int _drainNode, _gateNode, _sourceNode, _drainPrimeNode, _sourcePrimeNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public FrequencyBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            ComplexState = context.GetState<IComplexSimulationState>();
            _drainNode = ComplexState.Map[context.Nodes[0]];
            _gateNode = ComplexState.Map[context.Nodes[1]];
            _sourceNode = ComplexState.Map[context.Nodes[2]];
            _drainPrimeNode = ComplexState.Map[DrainPrime];
            _sourcePrimeNode = ComplexState.Map[SourcePrime];
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(_drainNode, _drainNode),
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_sourceNode, _sourceNode),
                new MatrixLocation(_drainPrimeNode, _drainPrimeNode),
                new MatrixLocation(_sourcePrimeNode, _sourcePrimeNode),
                new MatrixLocation(_drainNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _sourcePrimeNode),
                new MatrixLocation(_sourceNode, _sourcePrimeNode),
                new MatrixLocation(_drainPrimeNode, _drainNode),
                new MatrixLocation(_drainPrimeNode, _gateNode),
                new MatrixLocation(_drainPrimeNode, _sourcePrimeNode),
                new MatrixLocation(_sourcePrimeNode, _gateNode),
                new MatrixLocation(_sourcePrimeNode, _sourceNode),
                new MatrixLocation(_sourcePrimeNode, _drainPrimeNode));
        }

        /// <summary>
        /// Initializes the parameters.
        /// </summary>
        void IFrequencyBehavior.InitializeParameters()
        {
            var vgs = Vgs;
            var vgd = Vgd;

            // Calculate charge storage elements
            var czgs = TempCapGs * BaseParameters.Area;
            var czgd = TempCapGd * BaseParameters.Area;
            var twop = TempGatePotential + TempGatePotential;
            var czgsf2 = czgs / ModelTemperature.F2;
            var czgdf2 = czgd / ModelTemperature.F2;
            if (vgs < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / TempGatePotential);
                CapGs = czgs / sarg;
            }
            else
                CapGs = czgsf2 * (ModelTemperature.F3 + vgs / twop);

            if (vgd < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / TempGatePotential);
                CapGd = czgd / sarg;
            }
            else
                CapGd = czgdf2 * (ModelTemperature.F3 + vgd / twop);
        }

        /// <summary>
        /// Load the Y-matrix and Rhs vector.
        /// </summary>
        void IFrequencyBehavior.Load()
        {
            var omega = ComplexState.ThrowIfNotBound(this).Laplace.Imaginary;

            var gdpr = ModelParameters.DrainConductance * BaseParameters.Area;
            var gspr = ModelParameters.SourceConductance * BaseParameters.Area;
            var gm = Gm;
            var gds = Gds;
            var ggs = Ggs;
            var xgs = CapGs * omega;
            var ggd = Ggd;
            var xgd = CapGd * omega;

            ComplexElements.Add(
                gdpr,
                new Complex(ggd + ggs, xgd + xgs),
                gspr,
                new Complex(gdpr + gds + ggd, xgd),
                new Complex(gspr + gds + gm + ggs, xgs),
                -gdpr,
                -new Complex(ggd, xgd),
                -new Complex(ggs, xgs),
                -gspr,
                -gdpr,
                new Complex(-ggd + gm, -xgd),
                (-gds - gm),
                -new Complex(ggs + gm, xgs),
                -gspr,
                -gds);
        }
    }
}
