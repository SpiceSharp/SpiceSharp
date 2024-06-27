using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Diodes
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Diode" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    [BehaviorFor(typeof(Diode)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Temperature,
        IBiasingBehavior,
        IConvergenceBehavior
    {
        private readonly IIterationSimulationState _iteration;

        /// <summary>
        /// The variables used by the behavior.
        /// </summary>
        protected readonly DiodeVariables<double> Variables;

        /// <summary>
        /// The matrix elements.
        /// </summary>
        protected readonly ElementSet<double> Elements;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Voltage/*'/>
        /// <remarks>
        /// If the series multiplier is set, then this voltage is the sum of all voltage drops over all the
        /// diodes in series.
        /// </remarks>
        [ParameterName("v"), ParameterName("vd"), ParameterInfo("The voltage across the internal diode")]
        public double Voltage => LocalVoltage * Parameters.SeriesMultiplier;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Current/*'/>
        /// <remarks>
        /// If the parallel multiplier is set, then this current is the sum of all currents through all the
        /// diodes in parallel.
        /// </remarks>
        [ParameterName("i"), ParameterName("id"), ParameterName("c"), ParameterInfo("The complex current through the diode")]
        public double Current => LocalCurrent * Parameters.ParallelMultiplier;

        /// <summary>
        /// Gets the small-signal conductance.
        /// </summary>
        /// <value>
        /// The small-signal conductance.
        /// </value>
        [ParameterName("gd"), ParameterInfo("Small-signal conductance")]
        public double Conductance => LocalConductance * Parameters.ParallelMultiplier;

        /// <include file='./Components/Common/docs.xml' path='docs/members[@name="biasing"]/Power/*'/>
        /// <remarks>
        /// The power does not take into account losses by parasitic series resistors.
        /// </remarks>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("The dissipated power")]
        public double Power => Current * Voltage;

        /// <summary>
        /// The voltage across a single diode (not including parallel or series multipliers).
        /// </summary>
        protected double LocalVoltage;

        /// <summary>
        /// The current through a single diode (not including parallel or series multipliers).
        /// </summary>
        protected double LocalCurrent;

        /// <summary>
        /// The conductance through a single diode (not including paralle or series multipliers).
        /// </summary>
        protected double LocalConductance;

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            context.Nodes.CheckNodes(2);

            var state = context.GetState<IBiasingSimulationState>();
            _iteration = context.GetState<IIterationSimulationState>();

            Variables = new DiodeVariables<double>(Name, state, context);
            Elements = new ElementSet<double>(state.Solver,
                Variables.GetMatrixLocations(state.Map),
                Variables.GetRhsIndicies(state.Map));
        }

        /// <inheritdoc/>
        protected virtual void Load()
        {
            double cd, gd;

            // Get the current voltage across (one diode).
            Initialize(out double vd, out bool check);

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            double csat = TempSaturationCurrent * Parameters.Area;
            double gspr = ModelTemperature.Conductance * Parameters.Area;

            // compute dc current and derivatives
            if (vd >= -3 * Vte)
            {
                // Forward bias
                double evd = Math.Exp(vd / Vte);
                cd = csat * (evd - 1) + BiasingParameters.Gmin * vd;
                gd = csat * evd / Vte + BiasingParameters.Gmin;
            }
            else if (!ModelParameters.BreakdownVoltage.Given || vd >= -TempBreakdownVoltage)
            {
                // Reverse bias
                double arg = 3 * Vte / (vd * Math.E);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + BiasingParameters.Gmin * vd;
                gd = csat * 3 * arg / vd + BiasingParameters.Gmin;
            }
            else
            {
                // Reverse breakdown
                double evrev = Math.Exp(-(TempBreakdownVoltage + vd) / Vte);
                cd = -csat * evrev + BiasingParameters.Gmin * vd;
                gd = csat * evrev / Vte + BiasingParameters.Gmin;
            }

            // Check convergence
            if (_iteration.Mode != IterationModes.Fix || !Parameters.Off)
            {
                if (check)
                    _iteration.IsConvergent = false;
            }

            // Store for next time
            LocalVoltage = vd;
            LocalCurrent = cd;
            LocalConductance = gd;

            double m = Parameters.ParallelMultiplier;
            double n = Parameters.SeriesMultiplier;

            double cdeq = cd - gd * vd;
            gd *= m / n;
            gspr *= m / n;
            cdeq *= m;
            Elements.Add(
                // Y-matrix
                gspr, gd, gd + gspr, -gd, -gd, -gspr, -gspr,
                // RHS vector
                cdeq, -cdeq);
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load() => Load();

        /// <summary>
        /// Initialize the device based on the current iteration state.
        /// </summary>
        protected void Initialize(out double vd, out bool check)
        {
            check = false;
            if (_iteration.Mode == IterationModes.Junction)
            {
                vd = Parameters.Off ? 0.0 : TempVCritical;
            }
            else if (_iteration.Mode == IterationModes.Fix && Parameters.Off)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diodes (without series resistance).
                vd = (Variables.PosPrime.Value - Variables.Negative.Value) / Parameters.SeriesMultiplier;

                // Limit new junction voltage.
                if (ModelParameters.BreakdownVoltage.Given && vd < Math.Min(0, -TempBreakdownVoltage + 10 * Vte))
                {
                    double vdtemp = -(vd + TempBreakdownVoltage);
                    vdtemp = Semiconductor.LimitJunction(vdtemp, -(LocalVoltage + TempBreakdownVoltage), Vte, TempVCritical, ref check);
                    vd = -(vdtemp + TempBreakdownVoltage);
                }
                else
                {
                    vd = Semiconductor.LimitJunction(vd, LocalVoltage, Vte, TempVCritical, ref check);
                }
            }
        }

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            double vd = (Variables.PosPrime.Value - Variables.Negative.Value) / Parameters.SeriesMultiplier;

            double delvd = vd - LocalVoltage;
            double cdhat = LocalCurrent + LocalConductance * delvd;
            double cd = LocalCurrent;

            // check convergence
            double tol = BiasingParameters.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + BiasingParameters.AbsoluteTolerance;
            if (Math.Abs(cdhat - cd) > tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
