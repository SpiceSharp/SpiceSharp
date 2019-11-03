using System;
using SpiceSharp.Entities;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using SpiceSharp.Algebra;

namespace SpiceSharp.Components.DiodeBehaviors
{
    /// <summary>
    /// DC biasing behavior for a <see cref="Diode" />.
    /// </summary>
    public class BiasingBehavior : TemperatureBehavior, IBiasingBehavior
    {
        /// <summary>
        /// Gets the positive internal node.
        /// </summary>
        public Variable PosPrime { get; private set; }

        /// <summary>
        /// Gets the matrix elements.
        /// </summary>
        /// <value>
        /// The matrix elements.
        /// </value>
        protected ElementSet<double> Elements { get; private set; }

        /// <summary>
        /// Gets the voltage.
        /// </summary>
        [ParameterName("v"), ParameterName("vd"), ParameterInfo("Diode voltage")]
        public double Voltage { get; private set; }

        /// <summary>
        /// Gets the current.
        /// </summary>
        [ParameterName("i"), ParameterName("id"), ParameterInfo("Diode current")]
        public double Current { get; protected set; }

        /// <summary>
        /// Gets the small-signal conductance.
        /// </summary>
        [ParameterName("gd"), ParameterInfo("Small-signal conductance")]
        public double Conductance { get; protected set; }

        /// <summary>
        /// Gets the power dissipated.
        /// </summary>
        [ParameterName("p"), ParameterName("pd"), ParameterInfo("Power")]
        public double GetPower() => Current * Voltage;

        private int _posNode, _negNode, _posPrimeNode;

        /// <summary>
        /// Initializes a new instance of the <see cref="BiasingBehavior"/> class.
        /// </summary>
        /// <param name="name">Name</param>
        public BiasingBehavior(string name) : base(name) { }

        /// <summary>
        /// Bind the behavior to a simulation.
        /// </summary>
        /// <param name="context">The binding context.</param>
        public override void Bind(BindingContext context)
        {
            base.Bind(context);

            var c = (ComponentBindingContext)context;
            c.Nodes.ThrowIfNot("nodes", 2);
            _posNode = BiasingState.Map[c.Nodes[0]];
            _negNode = BiasingState.Map[c.Nodes[1]];
            var variables = context.Variables;
            PosPrime = ModelParameters.Resistance > 0 ? 
                variables.Create(Name.Combine("pos"), VariableType.Voltage) :
                c.Nodes[0];
            _posPrimeNode = BiasingState.Map[PosPrime];

            // Get matrix elements
            Elements = new ElementSet<double>(BiasingState.Solver, new[] {
                new MatrixLocation(_posNode, _posNode),
                new MatrixLocation(_negNode, _negNode),
                new MatrixLocation(_posPrimeNode, _posPrimeNode),
                new MatrixLocation(_negNode, _posPrimeNode),
                new MatrixLocation(_posPrimeNode, _negNode),
                new MatrixLocation(_posNode, _posPrimeNode),
                new MatrixLocation(_posPrimeNode, _posNode)
            }, new[] { _negNode, _posPrimeNode });
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        void IBiasingBehavior.Load()
        {
            var state = BiasingState;
            double cd, gd;

            // Get the current voltages
            Initialize(out double vd, out bool check);

            /* 
             * this routine loads diodes for dc and transient analyses.
             */
            var csat = TempSaturationCurrent * BaseParameters.Area;
            var gspr = ModelTemperature.Conductance * BaseParameters.Area;

            // compute dc current and derivatives
            if (vd >= -3 * Vte)
            {
                // Forward bias
                var evd = Math.Exp(vd / Vte);
                cd = csat * (evd - 1) + BaseConfiguration.Gmin * vd;
                gd = csat * evd / Vte + BaseConfiguration.Gmin;
            }
            else if (!ModelParameters.BreakdownVoltage.Given || vd >= -TempBreakdownVoltage)
            {
                // Reverse bias
                var arg = 3 * Vte / (vd * Math.E);
                arg = arg * arg * arg;
                cd = -csat * (1 + arg) + BaseConfiguration.Gmin * vd;
                gd = csat * 3 * arg / vd + BaseConfiguration.Gmin;
            }
            else
            {
                // Reverse breakdown
                var evrev = Math.Exp(-(TempBreakdownVoltage + vd) / Vte);
                cd = -csat * evrev + BaseConfiguration.Gmin * vd;
                gd = csat * evrev / Vte + BaseConfiguration.Gmin;
            }

            // Check convergence
            if (state.Init != InitializationModes.Fix || !BaseParameters.Off)
            {
                if (check)
                    state.IsConvergent = false;
            }

            // Store for next time
            Voltage = vd;
            Current = cd;
            Conductance = gd;

            var cdeq = cd - gd * vd;
            Elements.Add(
                // Y-matrix
                gspr, gd, gd + gspr, -gd, -gd, -gspr, -gspr,
                // RHS vector
                cdeq, -cdeq);
        }

        /// <summary>
        /// Initialize the device based on the current iteration state.
        /// </summary>
        protected void Initialize(out double vd, out bool check)
        {
            var state = BiasingState;
            check = false;
            if (state.Init == InitializationModes.Junction)
            {
                vd = BaseParameters.Off ? 0.0 : TempVCritical;
            }
            else if (state.Init == InitializationModes.Fix && BaseParameters.Off)
            {
                vd = 0.0;
            }
            else
            {
                // Get voltage over the diode (without series resistance)
                vd = state.Solution[_posPrimeNode] - state.Solution[_negNode];

                // limit new junction voltage
                if (ModelParameters.BreakdownVoltage.Given && vd < Math.Min(0, -TempBreakdownVoltage + 10 * Vte))
                {
                    var vdtemp = -(vd + TempBreakdownVoltage);
                    vdtemp = Semiconductor.LimitJunction(vdtemp, -(Voltage + TempBreakdownVoltage), Vte, TempVCritical, ref check);
                    vd = -(vdtemp + TempBreakdownVoltage);
                }
                else
                {
                    vd = Semiconductor.LimitJunction(vd, Voltage, Vte, TempVCritical, ref check);
                }
            }
        }

        /// <summary>
        /// Check convergence for the diode
        /// </summary>
        /// <returns></returns>
        bool IBiasingBehavior.IsConvergent()
        {
            var state = BiasingState;
            var vd = state.Solution[_posPrimeNode] - state.Solution[_negNode];

            var delvd = vd - Voltage;
            var cdhat = Current + Conductance * delvd;
            var cd = Current;

            // check convergence
            var tol = BaseConfiguration.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(cd)) + BaseConfiguration.AbsoluteTolerance;
            if (Math.Abs(cdhat - cd) > tol)
            {
                state.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
