using System;
using SpiceSharp.Behaviors;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

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
        protected StateDerivative Qgs { get; private set; }

        /// <summary>
        /// Gets the state tracking gate-drain charge.
        /// </summary>
        protected StateDerivative Qgd { get; private set; }

        /// <summary>
        /// Gets the G-S capacitance.
        /// </summary>
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the G-D capacitance.
        /// </summary>
        public double CapGd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransientBehavior"/> class.
        /// </summary>
        /// <param name="name">The identifier of the behavior.</param>
        /// <remarks>
        /// The identifier of the behavior should be the same as that of the entity creating it.
        /// </remarks>
        public TransientBehavior(string name) : base(name)
        {
        }

        /// <summary>
        /// Bind the behavior.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <param name="context">The context.</param>
        public override void Bind(Simulation simulation, BindingContext context)
        {
            base.Bind(simulation, context);

            var method = ((TimeSimulation)simulation).Method;
            Qgs = method.CreateDerivative();
            Qgd = method.CreateDerivative();
        }

        /// <summary>
        /// Calculates the state values from the current DC solution.
        /// </summary>
        /// <remarks>
        /// In this method, the initial value is calculated based on the operating point solution,
        /// and the result is stored in each respective <see cref="T:SpiceSharp.IntegrationMethods.StateDerivative" /> or <see cref="T:SpiceSharp.IntegrationMethods.StateHistory" />.
        /// </remarks>
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
            var ggs = Qgs.Jacobian(CapGs);
            var cg = Qgs.Derivative;
            Qgd.Integrate();
            var ggd = Qgd.Jacobian(CapGd);
            cg += Qgd.Derivative;
            var cd = -Qgd.Derivative;
            var cgd = Qgd.Derivative;

            var ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd);
            var ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs);
            var cdreq = ModelParameters.JFETType * (cd + cgd);
            GateNodePtr.Value += -ceqgs - ceqgd;
            DrainPrimeNodePtr.Value += -cdreq + ceqgd;
            SourcePrimeNodePtr.Value += cdreq + ceqgs;

            // Load Y-matrix
            GateDrainPrimePtr.Value += -ggd;
            GateSourcePrimePtr.Value += -ggs;
            DrainPrimeGatePtr.Value += -ggd;
            SourcePrimeGatePtr.Value += -ggs;
            GateGatePtr.Value += ggd + ggs;
            DrainPrimeDrainPrimePtr.Value += ggd;
            SourcePrimeSourcePrimePtr.Value += ggs;
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
                Qgs.Current = twop * czgs * (1 - sarg);
                CapGs = czgs / sarg;
            }
            else
            {
                Qgs.Current = czgs * F1 + czgsf2 *
                              (ModelTemperature.F3 * (vgs - CorDepCap) + (vgs * vgs - fcpb2) / (twop + twop));
                CapGs = czgsf2 * (ModelTemperature.F3 + vgs / twop);
            }

            if (vgd < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgd / TempGatePotential);
                Qgd.Current = twop * czgd * (1 - sarg);
                CapGd = czgd / sarg;
            }
            else
            {
                Qgd.Current = czgd * F1 + czgdf2 *
                              (ModelTemperature.F3 * (vgd - CorDepCap) + (vgd * vgd - fcpb2) / (twop + twop));
                CapGd = czgdf2 * (ModelTemperature.F3 + vgd / twop);
            }
        }
    }
}
