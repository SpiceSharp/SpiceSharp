using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.JFETs
{
    /// <summary>
    /// Transient behavior for a <see cref="JFET" />.
    /// </summary>
    /// <seealso cref="Biasing"/>
    /// <seealso cref="ITimeBehavior"/>
    [BehaviorFor(typeof(JFET)), AddBehaviorIfNo(typeof(ITimeBehavior))]
    public class Time : Biasing,
        ITimeBehavior
    {
        private readonly int _gateNode, _drainPrimeNode, _sourcePrimeNode;
        private readonly ITimeSimulationState _time;
        private readonly ElementSet<double> _elements;
        private readonly IDerivative _qgs, _qgd;

        /// <summary>
        /// Gets the gate-source capacitance.
        /// </summary>
        /// <value>
        /// The gate-source capacitance.
        /// </value>
        public double CapGs { get; private set; }

        /// <summary>
        /// Gets the gate-drain capacitance.
        /// </summary>
        /// <value>
        /// The gate-drain capacitance.
        /// </value>
        public double CapGd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Time"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Time(IComponentBindingContext context) : base(context)
        {
            _time = context.GetState<ITimeSimulationState>();
            var method = context.GetState<IIntegrationMethod>();
            _qgs = method.CreateDerivative();
            _qgd = method.CreateDerivative();

            _gateNode = BiasingState.Map[BiasingState.GetSharedVariable(context.Nodes[1])];
            _drainPrimeNode = BiasingState.Map[DrainPrime];
            _sourcePrimeNode = BiasingState.Map[SourcePrime];

            _elements = new ElementSet<double>(BiasingState.Solver, [
                new MatrixLocation(_gateNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _sourcePrimeNode),
                new MatrixLocation(_drainPrimeNode, _gateNode),
                new MatrixLocation(_sourcePrimeNode, _gateNode),
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_drainPrimeNode, _drainPrimeNode),
                new MatrixLocation(_sourcePrimeNode, _sourcePrimeNode)
            ], [_gateNode, _drainPrimeNode, _sourcePrimeNode]);
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            double vgs = _time.UseIc && Parameters.InitialVgs.Given ?
                Parameters.InitialVgs.Value : Vgs;
            double vds = _time.UseIc && Parameters.InitialVds.Given ?
                Parameters.InitialVds.Value : -Vgd - Vgs;
            double vgd = vgs - vds;
            CalculateStates(vgs, vgd);
        }

        /// <inheritdoc/>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;

            // Calculate the states
            double vgs = Vgs;
            double vgd = Vgd;
            CalculateStates(vgs, vgd);

            // Integrate and add contributions
            _qgs.Derive();
            double ggs = _qgs.GetContributions(CapGs).Jacobian;
            double cg = _qgs.Derivative;
            _qgd.Derive();
            double ggd = _qgd.GetContributions(CapGd).Jacobian;
            cg += _qgd.Derivative;
            double cd = -_qgd.Derivative;
            double cgd = _qgd.Derivative;

            double m = Parameters.ParallelMultiplier;
            ggd *= m;
            ggs *= m;
            double ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd) * m;
            double ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs) * m;
            double cdreq = ModelParameters.JFETType * (cd + cgd) * m;

            _elements.Add(
                // Y-matrix
                -ggd,
                -ggs,
                -ggd,
                -ggs,
                (ggd + ggs),
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
        /// <param name="vgs">The gate-source voltage.</param>
        /// <param name="vgd">The gate-drain voltage.</param>
        private void CalculateStates(double vgs, double vgd)
        {
            // Charge storage elements
            double czgs = TempCapGs * Parameters.Area;
            double czgd = TempCapGd * Parameters.Area;
            double twop = TempGatePotential + TempGatePotential;
            double fcpb2 = CorDepCap * CorDepCap;
            double czgsf2 = czgs / ModelTemperature.F2;
            double czgdf2 = czgd / ModelTemperature.F2;
            if (vgs < CorDepCap)
            {
                double sarg = Math.Sqrt(1 - vgs / TempGatePotential);
                _qgs.Value = twop * czgs * (1 - sarg);
                CapGs = czgs / sarg;
            }
            else
            {
                _qgs.Value = czgs * F1 + czgsf2 *
                              (ModelTemperature.F3 * (vgs - CorDepCap) + (vgs * vgs - fcpb2) / (twop + twop));
                CapGs = czgsf2 * (ModelTemperature.F3 + vgs / twop);
            }

            if (vgd < CorDepCap)
            {
                double sarg = Math.Sqrt(1 - vgd / TempGatePotential);
                _qgd.Value = twop * czgd * (1 - sarg);
                CapGd = czgd / sarg;
            }
            else
            {
                _qgd.Value = czgd * F1 + czgdf2 *
                              (ModelTemperature.F3 * (vgd - CorDepCap) + (vgd * vgd - fcpb2) / (twop + twop));
                CapGd = czgdf2 * (ModelTemperature.F3 + vgd / twop);
            }
        }
    }
}
