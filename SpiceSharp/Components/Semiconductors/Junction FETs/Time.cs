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

            _elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_gateNode, _drainPrimeNode),
                new MatrixLocation(_gateNode, _sourcePrimeNode),
                new MatrixLocation(_drainPrimeNode, _gateNode),
                new MatrixLocation(_sourcePrimeNode, _gateNode),
                new MatrixLocation(_gateNode, _gateNode),
                new MatrixLocation(_drainPrimeNode, _drainPrimeNode),
                new MatrixLocation(_sourcePrimeNode, _sourcePrimeNode)
            }, new[] { _gateNode, _drainPrimeNode, _sourcePrimeNode });
        }

        /// <inheritdoc/>
        void ITimeBehavior.InitializeStates()
        {
            var vgs = _time.UseIc && Parameters.InitialVgs.Given ?
                Parameters.InitialVgs.Value : Vgs;
            var vds = _time.UseIc && Parameters.InitialVds.Given ?
                Parameters.InitialVds.Value : -Vgd - Vgs;
            var vgd = vgs - vds;
            CalculateStates(vgs, vgd);
        }

        /// <inheritdoc/>
        protected override void Load()
        {
            base.Load();
            if (_time.UseDc)
                return;

            // Calculate the states
            var vgs = Vgs;
            var vgd = Vgd;
            CalculateStates(vgs, vgd);

            // Integrate and add contributions
            _qgs.Derive();
            var ggs = _qgs.GetContributions(CapGs).Jacobian;
            var cg = _qgs.Derivative;
            _qgd.Derive();
            var ggd = _qgd.GetContributions(CapGd).Jacobian;
            cg += _qgd.Derivative;
            var cd = -_qgd.Derivative;
            var cgd = _qgd.Derivative;

            var m = Parameters.ParallelMultiplier;
            ggd *= m;
            ggs *= m;
            var ceqgd = ModelParameters.JFETType * (cgd - ggd * vgd) * m;
            var ceqgs = ModelParameters.JFETType * (cg - cgd - ggs * vgs) * m;
            var cdreq = ModelParameters.JFETType * (cd + cgd) * m;

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
            var czgs = TempCapGs * Parameters.Area;
            var czgd = TempCapGd * Parameters.Area;
            var twop = TempGatePotential + TempGatePotential;
            var fcpb2 = CorDepCap * CorDepCap;
            var czgsf2 = czgs / ModelTemperature.F2;
            var czgdf2 = czgd / ModelTemperature.F2;
            if (vgs < CorDepCap)
            {
                var sarg = Math.Sqrt(1 - vgs / TempGatePotential);
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
                var sarg = Math.Sqrt(1 - vgd / TempGatePotential);
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
