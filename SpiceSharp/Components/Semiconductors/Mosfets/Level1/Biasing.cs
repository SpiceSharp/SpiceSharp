using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level1
{
    /// <summary>
    /// Biasing behavior for a <see cref="Mosfet1" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    [BehaviorFor(typeof(Mosfet1)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Temperature,
        IMosfetBiasingBehavior,
        IConvergenceBehavior
    {
        private readonly ITimeSimulationState _time;
        private readonly IIntegrationMethod _method;
        private readonly IIterationSimulationState _iteration;
        private readonly MosfetVariables<double> _variables;
        private readonly ElementSet<double> _elements;
        private readonly BiasingParameters _config;
        private readonly Contributions<double> _contributions = new();
        private readonly MosfetContributionEventArgs _args;

        /// <inheritdoc/>
        TemperatureProperties IMosfetBiasingBehavior.Properties => Properties;

        /// <summary>
        /// The maximum exponent argument
        /// </summary>
        protected const double MaximumExponentArgument = 709.0;

        /// <inheritdoc/>
        public event EventHandler<MosfetContributionEventArgs> UpdateContributions;

        /// <include file='../common/docs.xml' path='docs/members/DrainCurrent/*'/>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double Id { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkSourceCurrent/*'/>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double Ibs { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkDrainCurrent/*'/>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double Ibd { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/Transconductance/*'/>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Gm { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkSourceTransconductance/*'/>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double Gmbs { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/DrainSourceConductance/*'/>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double Gds { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkSourceConductance/*'/>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double Gbs { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkDrainConductance/*'/>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double Gbd { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/von/*'/>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        public double Von { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/SaturationVoltage/*'/>
        [ParameterName("vdsat"), ParameterInfo("Saturation drain-source voltage")]
        public double Vdsat { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/Mode/*'/>
        public double Mode { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/GateSourceVoltage/*'/>
        [ParameterName("vgs"), ParameterInfo("Gate-Source voltage")]
        public double Vgs { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/DrainSourceVoltage/*'/>
        [ParameterName("vds"), ParameterInfo("Drain-Source voltage")]
        public double Vds { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkSourceVoltage/*'/>
        [ParameterName("vbs"), ParameterInfo("Bulk-Source voltage")]
        public double Vbs { get; private set; }

        /// <include file='../common/docs.xml' path='docs/members/BulkDrainVoltage/*'/>
        [ParameterName("vbd"), ParameterInfo("Bulk-Drain voltage")]
        public double Vbd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="context"/> is <c>null</c>.</exception>
        public Biasing(IComponentBindingContext context)
            : base(context)
        {
            var state = context.GetState<IBiasingSimulationState>();
            _config = context.GetSimulationParameterSet<BiasingParameters>();
            _iteration = context.GetState<IIterationSimulationState>();
            _args = new MosfetContributionEventArgs(_contributions);
            context.TryGetState(out _time);
            context.TryGetState(out _method);
            Vdsat = 0;
            Von = 0;
            Mode = 1;
            _variables = new MosfetVariables<double>(context, state);

            // Get matrix pointers
            _elements = new ElementSet<double>(state.Solver,
                _variables.GetMatrixLocations(state.Map),
                _variables.GetRhsIndices(state.Map));
        }

        /// <inheritdoc/>
        void IBiasingBehavior.Load()
        {
            var con = _contributions;
            con.Reset();

            double vt = Constants.KOverQ * Parameters.Temperature;
            double m = Parameters.ParallelMultiplier;
            double DrainSatCur, SourceSatCur;
            if ((Properties.TempSatCurDensity == 0) || (Parameters.DrainArea == 0) || (Parameters.SourceArea == 0))
            {
                DrainSatCur = m * Properties.TempSatCur;
                SourceSatCur = m * Properties.TempSatCur;
            }
            else
            {
                DrainSatCur = Properties.TempSatCurDensity * m * Parameters.DrainArea;
                SourceSatCur = Properties.TempSatCurDensity * m  * Parameters.SourceArea;
            }
            double Beta = Properties.TempTransconductance * m * Parameters.Width / Properties.EffectiveLength;

            // Get the current voltages
            Initialize(out double vgs, out double vds, out double vbs, out bool check);
            double vbd = vbs - vds;
            double vgd = vgs - vds;

            /*
             * Bulk-source and bulk-drain diodes
             *   here we just evaluate the ideal diode current and the
             *   corresponding derivative (conductance).
             */
            if (vbs <= -3 * vt)
            {
                con.Bs.G = _config.Gmin;
                con.Bs.C = con.Bs.G * vbs - SourceSatCur;
            }
            else
            {
                double evbs = Math.Exp(Math.Min(MaximumExponentArgument, vbs / vt));
                con.Bs.G = SourceSatCur * evbs / vt + _config.Gmin;
                con.Bs.C = SourceSatCur * (evbs - 1) + _config.Gmin * vbs;
            }
            if (vbd <= -3 * vt)
            {
                con.Bd.G = _config.Gmin;
                con.Bd.C = con.Bd.G * vbd - DrainSatCur;
            }
            else
            {
                double evbd = Math.Exp(Math.Min(MaximumExponentArgument, vbd / vt));
                con.Bd.G = DrainSatCur * evbd / vt + _config.Gmin;
                con.Bd.C = DrainSatCur * (evbd - 1) + _config.Gmin * vbd;
            }
            // Now to determine whether the user was able to correctly identify the source and drain of his device
            if (vds >= 0)
                Mode = 1;
            else
                Mode = -1;

            {
                /*
                 *     this block of code evaluates the drain current and its
                 *     derivatives using the shichman-hodges model and the
                 *     charges associated with the gate, channel and bulk for
                 *     mosfets
                 *
                 */

                /* the following 4 variables are local to this code block until
                 * it is obvious that they can be made global
                 */
                double arg;
                double betap;
                double sarg;
                double vgst;

                if ((Mode > 0 ? vbs : vbd) <= 0)
                {
                    sarg = Math.Sqrt(Properties.TempPhi - (Mode > 0 ? vbs : vbd));
                }
                else
                {
                    sarg = Math.Sqrt(Properties.TempPhi);
                    sarg -= (Mode > 0 ? vbs : vbd) / (sarg + sarg);
                    sarg = Math.Max(0, sarg);
                }
                double von = (Properties.TempVbi * ModelParameters.MosfetType) + ModelParameters.Gamma * sarg;
                vgst = (Mode > 0 ? vgs : vgd) - von;
                double vdsat = Math.Max(vgst, 0);
                if (sarg <= 0)
                    arg = 0;
                else
                    arg = ModelParameters.Gamma / (sarg + sarg);
                if (vgst <= 0)
                {
                    // Cutoff region
                    con.Ds.C = 0;
                    Gm = 0;
                    con.Ds.G = 0;
                    Gmbs = 0;
                }
                else
                {
                    // Saturation region
                    betap = Beta * (1 + ModelParameters.Lambda * (vds * Mode));
                    if (vgst <= (vds * Mode))
                    {
                        con.Ds.C = betap * vgst * vgst * .5;
                        Gm = betap * vgst;
                        con.Ds.G = ModelParameters.Lambda * Beta * vgst * vgst * .5;
                        Gmbs = Gm * arg;
                    }
                    else
                    {
                        // Linear region
                        con.Ds.C = betap * (vds * Mode) *
                            (vgst - .5 * (vds * Mode));
                        Gm = betap * (vds * Mode);
                        con.Ds.G = betap * (vgst - (vds * Mode)) +
                                ModelParameters.Lambda * Beta *
                                (vds * Mode) *
                                (vgst - .5 * (vds * Mode));
                        Gmbs = Gm * arg;
                    }
                }

                // now deal with n vs p polarity
                Von = ModelParameters.MosfetType * von;
                Vdsat = ModelParameters.MosfetType * vdsat;
            }

            // COMPUTE EQUIVALENT DRAIN CURRENT SOURCE
            Gds = con.Ds.G;
            Id = Mode * con.Ds.C - con.Bd.C;

            Vbs = vbs;
            Vbd = vbd;
            Vgs = vgs;
            Vds = vds;

            // Update with time-dependent calculations
            UpdateContributions?.Invoke(this, _args);

            // Check convergence
            if (!Parameters.Off && _iteration.Mode != IterationModes.Fix)
            {
                if (check)
                    _iteration.IsConvergent = false;
            }

            Gbs = con.Bs.G;
            Ibs = con.Bs.C;
            Gbd = con.Bd.G;
            Ibd = con.Bd.C;

            // Right hand side vector contributions
            con.Bs.C = ModelParameters.MosfetType * (con.Bs.C - con.Bs.G * vbs);
            con.Bd.C = ModelParameters.MosfetType * (con.Bd.C - con.Bd.G * vbd);
            double xnrm, xrev;
            if (Mode >= 0)
            {
                xnrm = 1;
                xrev = 0;
                con.Ds.C = ModelParameters.MosfetType * (con.Ds.C - con.Ds.G * vds - Gm * vgs - Gmbs * vbs);
            }
            else
            {
                xnrm = 0;
                xrev = 1;
                con.Ds.C = -ModelParameters.MosfetType * (con.Ds.C - con.Ds.G * (-vds) - Gm * vgd - Gmbs * vbd);
            }

            _elements.Add(
                // Y-matrix
                Properties.DrainConductance,
                con.Gd.G + con.Gs.G + con.Gb.G,
                Properties.SourceConductance,
                con.Bd.G + con.Bs.G + con.Gb.G,
                Properties.DrainConductance + con.Ds.G + con.Bd.G + xrev * (Gm + Gmbs) + con.Gd.G,
                Properties.SourceConductance + con.Ds.G + con.Bs.G + xnrm * (Gm + Gmbs) + con.Gs.G,
                -Properties.DrainConductance,
                -con.Gb.G,
                -con.Gd.G,
                -con.Gs.G,
                -Properties.SourceConductance,
                -con.Gb.G,
                -con.Bd.G,
                -con.Bs.G,
                -Properties.DrainConductance,
                (xnrm - xrev) * Gm - con.Gd.G,
                -con.Bd.G + (xnrm - xrev) * Gmbs,
                -con.Ds.G - xnrm * (Gm + Gmbs),
                -(xnrm - xrev) * Gm - con.Gs.G,
                -Properties.SourceConductance,
                -con.Bs.G - (xnrm - xrev) * Gmbs,
                -con.Ds.G - xrev * (Gm + Gmbs),

                // Right hand side vector
                -ModelParameters.MosfetType * (con.Gs.C + con.Gb.C + con.Gd.C),
                -(con.Bs.C + con.Bd.C - ModelParameters.MosfetType * con.Gb.C),
                con.Bd.C - con.Ds.C + ModelParameters.MosfetType * con.Gd.C,
                con.Ds.C + con.Bs.C + ModelParameters.MosfetType * con.Gs.C
                );
        }

        /// <include file='../common/docs.xml' path='docs/methods/Initialize/*'/>
        protected void Initialize(out double vgs, out double vds, out double vbs, out bool check)
        {
            check = true;

            if (_iteration.Mode == IterationModes.Float || (_time != null && !_time.UseDc && _method != null && _method.BaseTime.Equals(0.0)) ||
                _iteration.Mode == IterationModes.Fix && !Parameters.Off)
            {
                // General iteration
                vbs = ModelParameters.MosfetType * (_variables.Bulk.Value - _variables.SourcePrime.Value);
                vgs = ModelParameters.MosfetType * (_variables.Gate.Value - _variables.SourcePrime.Value);
                vds = ModelParameters.MosfetType * (_variables.DrainPrime.Value - _variables.SourcePrime.Value);

                // now some common crunching for some more useful quantities
                double vbd = vbs - vds;
                double vgd = vgs - vds;
                double vgdo = Vgs - Vds;
                double von = ModelParameters.MosfetType * Von;

                /*
				 * limiting
				 * we want to keep device voltages from changing
				 * so fast that the exponentials churn out overflows
				 * and similar rudeness
				 */
                // NOTE: Spice 3f5 does not write out Vgs during DC analysis, so DEVfetlim may give different results in Spice 3f5
                if (Vds >= 0)
                {
                    vgs = Transistor.LimitFet(vgs, Vgs, von);
                    vds = vgs - vgd;
                    vds = Transistor.LimitVds(vds, Vds);
                }
                else
                {
                    vgd = Transistor.LimitFet(vgd, vgdo, von);
                    vds = vgs - vgd;
                    vds = -Transistor.LimitVds(-vds, -Vds);
                    vgs = vgd + vds;
                }

                check = false;
                if (vds >= 0)
                    vbs = Semiconductor.LimitJunction(vbs, Vbs, Properties.TempVt, SourceVCritical, ref check);
                else
                {
                    vbd = Semiconductor.LimitJunction(vbd, Vbd, Properties.TempVt, DrainVCritical, ref check);
                    vbs = vbd + vds;
                }
            }
            else
            {
                /* ok - not one of the simple cases, so we have to
				 * look at all of the possibilities for why we were
				 * called.  We still just initialize the three voltages
				 */
                if (_iteration.Mode == IterationModes.Junction && !Parameters.Off)
                {
                    vds = ModelParameters.MosfetType * Parameters.InitialVds;
                    vgs = ModelParameters.MosfetType * Parameters.InitialVgs;
                    vbs = ModelParameters.MosfetType * Parameters.InitialVbs;

                    // TODO: At some point, check what this is supposed to do
                    if (vds.Equals(0) && vgs.Equals(0) && vbs.Equals(0) && (_time == null || _time.UseDc || !_time.UseIc))
                    {
                        vbs = -1;
                        vgs = ModelParameters.MosfetType * Properties.TempVt0;
                        vds = 0;
                    }
                }
                else
                {
                    vbs = vgs = vds = 0;
                }
            }
        }

        /// <inheritdoc/>
        bool IConvergenceBehavior.IsConvergent()
        {
            double cdhat;

            double vs = _variables.SourcePrime.Value;
            double vbs = ModelParameters.MosfetType * (_variables.Bulk.Value - vs);
            double vgs = ModelParameters.MosfetType * (_variables.Gate.Value - vs);
            double vds = ModelParameters.MosfetType * (_variables.DrainPrime.Value - vs);
            double vbd = vbs - vds;
            double vgd = vgs - vds;
            double vgdo = Vgs - Vds;
            double delvbs = vbs - Vbs;
            double delvbd = vbd - Vbd;
            double delvgs = vgs - Vgs;
            double delvds = vds - Vds;
            double delvgd = vgd - vgdo;

            // these are needed for convergence testing
            if (Mode >= 0)
            {
                cdhat = Id - Gbd * delvbd + Gmbs * delvbs +
                    Gm * delvgs + Gds * delvds;
            }
            else
            {
                cdhat = Id - (Gbd - Gmbs) * delvbd -
                    Gm * delvgd + Gds * delvds;
            }
            double cbhat = Ibs + Ibd + Gbd * delvbd + Gbs * delvbs;

            // Check convergence
            double tol = _config.RelativeTolerance * Math.Max(Math.Abs(cdhat), Math.Abs(Id)) + _config.AbsoluteTolerance;
            if (Math.Abs(cdhat - Id) >= tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }

            tol = _config.RelativeTolerance * Math.Max(Math.Abs(cbhat), Math.Abs(Ibs + Ibd)) + _config.AbsoluteTolerance;
            if (Math.Abs(cbhat - (Ibs + Ibd)) > tol)
            {
                _iteration.IsConvergent = false;
                return false;
            }
            return true;
        }
    }
}
