using System;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.JFETBehaviors
{
    /// <summary>
    /// Transient behavior for a <see cref="JFET" />.
    /// </summary>
    public class TransientBehavior : BiasingBehavior, ITimeBehavior
    {
        /// <summary>
        /// Gets the state tracking gate-source charge.
        /// </summary>
        protected IDerivative Qgs { get; private set; }

        /// <summary>
        /// Gets the state tracking gate-drain charge.
        /// </summary>
        protected IDerivative Qgd { get; private set; }

        /// <summary>
        /// Gets the transient matrix elements.
        /// </summary>
        /// <value>
        /// The transient matrix elements.
        /// </value>
        protected ElementSet<double> TransientMatrixElements { get; private set; }

        /// <summary>
        /// Gets the G-S capacitance.
        /// </summary>
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the G-D capacitance.
        /// </summary>
        public double CapGd { get; private set; }

        private int _gateNode, _drainPrimeNode, _sourcePrimeNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="context">The context.</param>
        public TransientBehavior(string name, ComponentBindingContext context) : base(name, context)
        {
            var method = context.GetState<IIntegrationMethod>();
            Qgs = method.CreateDerivative();
            Qgd = method.CreateDerivative();

            _gateNode = BiasingState.Map[context.Nodes[1]];
            _drainPrimeNode = BiasingState.Map[DrainPrime];
            _sourcePrimeNode = BiasingState.Map[SourcePrime];
            TransientMatrixElements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_gateNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _sourcePrimeNode),
                new MatrixLocation(_drainPrimeNode, _gateNode),
                new MatrixLocation(_sourcePrimeNode, _gateNode),
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_drainPrimeNode, _drainPrimeNode),
                new MatrixLocation(_sourcePrimeNode, _sourcePrimeNode)
            }, new[] { _gateNode, _drainPrimeNode, _sourcePrimeNode });
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        void ITimeBehavior.InitializeStates()
        {
            var vgs = Vgs;
            var vgd = Vgd;
            CalculateStates(vgs, vgd);
        }

        /// <summary>
        /// Perform time-dependent calculations.
        /// </summary>
        void ITimeBehavior.Load()
        {
            // Calculate the states
            var vgs = Vgs;
            var vgd = Vgd;
            CalculateStates(vgs, vgd);

            // Integrate and add contributions
            Qgs.Integrate();
            var ggs = Qgs.GetContributions(CapGs).Jacobian;
            var cg = Qgs.Derivative;
            Qgd.Integrate();
            var ggd = Qgd.GetContributions(CapGd).Jacobian;
            cg += Qgd.Derivative;
            var cd = -Qgd.Derivative;
            var cgd = Qgd.Derivative;

            var ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd);
            var ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs);
            var cdreq = ModelParameters.JFETType * (cd + cgd);

            TransientMatrixElements.Add(
                // Y-matrix
                -ggd,
                -ggs,
                -ggd,
                -ggs,
                ggd + ggs,
                ggd,
                ggs,
                // RHS vector
                -ceqgs - ceqgd, 
                -cdreq + ceqgd, 
                cdreq + ceqgs);
        }

        /// <summary>
        /// Calculates the states.
        /// </summary>
        /// <param name="vgs">The VGS.</param>
        /// <param name="vgd">The VGD.</param>
        private void CalculateStates(double vgs, double vgd)
        {
            // Charge storage elements
            var czgs = TempCapGs * BaseParameters.Area;
            var czgd = TempCapGd * BaseParameters.Area;
            var twop = TempGatePotential + TempGatePotential;
            var fcpb2 = CorDepCap * CorDepCap;
            var czgsf2 = czgs / ModelTemperature.F2;
            var czgdf2 = czgd / ModelTemperature.F2;
            if (vgs < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / TempGatePotential);
                Qgs.Value = twop * czgs * (1 - sarg);
                CapGs = czgs / sarg;
            }
            else
            {
                Qgs.Value = czgs * F1 + czgsf2 *
                              (ModelTemperature.F3 * (vgs - CorDepCap) + (vgs * vgs - fcpb2) / (twop + twop));
                CapGs = czgsf2 * (ModelTemperature.F3 + vgs / twop);
            }

            if (vgd < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / TempGatePotential);
                Qgd.Value = twop * czgd * (1 - sarg);
                CapGd = czgd / sarg;
            }
            else
            {
                Qgd.Value = czgd * F1 + czgdf2 *
                              (ModelTemperature.F3 * (vgd - CorDepCap) + (vgd * vgd - fcpb2) / (twop + twop));
                CapGd = czgdf2 * (ModelTemperature.F3 + vgd / twop);
            }
        }
    }
}
