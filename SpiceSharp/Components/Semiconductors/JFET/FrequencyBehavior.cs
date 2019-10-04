using System;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="FrequencyBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public FrequencyBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            ComplexState = context.States.GetValue<IComplexSimulationState>();
            ComplexElements = new ElementSet<Complex>(ComplexState.Solver,
                new MatrixLocation(DrainNode, DrainNode),
                new MatrixLocation(GateNode, GateNode),
                new MatrixLocation(SourceNode, SourceNode),
                new MatrixLocation(DrainPrimeNode, DrainPrimeNode),
                new MatrixLocation(SourcePrimeNode, SourcePrimeNode),
                new MatrixLocation(DrainNode, DrainPrimeNode),
                new MatrixLocation(GateNode, DrainPrimeNode),
                new MatrixLocation(GateNode, SourcePrimeNode),
                new MatrixLocation(SourceNode, SourcePrimeNode),
                new MatrixLocation(DrainPrimeNode, DrainNode),
                new MatrixLocation(DrainPrimeNode, GateNode),
                new MatrixLocation(DrainPrimeNode, SourcePrimeNode),
                new MatrixLocation(SourcePrimeNode, GateNode),
                new MatrixLocation(SourcePrimeNode, SourceNode),
                new MatrixLocation(SourcePrimeNode, DrainPrimeNode));
        }

        /// <summary>
        /// Unbind the behavior.
        /// </summary>
        public override void Unbind()
        {
            base.Unbind();
            ComplexElements?.Destroy();
            ComplexElements = null;
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
