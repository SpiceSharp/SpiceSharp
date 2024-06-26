using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level2
{
    /// <summary>
    /// Biasing behavior for a <see cref="Mosfet2" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    [BehaviorFor(typeof(Mosfet2)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
    [GeneratedParameters]
    public partial class Biasing : Temperature,
        IMosfetBiasingBehavior,
        IConvergenceBehavior
    {
        private readonly IIntegrationMethod _method;
        private readonly ITimeSimulationState _time;
        private readonly IIterationSimulationState _iteration;
        private readonly ElementSet<double> _elements;
        private readonly MosfetVariables<double> _variables;
        private readonly BiasingParameters _config;
        private readonly Contributions<double> _contributions = new();
        private readonly MosfetContributionEventArgs _args;
        private static readonly double[] _sig1 = [1.0, -1.0, 1.0, -1.0];
        private static readonly double[] _sig2 = [1.0, 1.0, -1.0, -1.0];

        /// <summary>
        /// The permittivity of silicon.
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// The maximum exponent argument.
        /// </summary>
        protected const double MaximumExponentArgument = 709.0;

        /// <inheritdoc/>
        TemperatureProperties IMosfetBiasingBehavior.Properties => Properties;

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
            _args = new MosfetContributionEventArgs(_contributions);
            context.TryGetState(out _time);
            context.TryGetState(out _method);
            _iteration = context.GetState<IIterationSimulationState>();
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
            double DrainSatCur, SourceSatCur;
            if ((Properties.TempSatCurDensity == 0) || (Parameters.DrainArea == 0) || (Parameters.SourceArea == 0))
            {
                DrainSatCur = Parameters.ParallelMultiplier * Properties.TempSatCur;
                SourceSatCur = Parameters.ParallelMultiplier * Properties.TempSatCur;
            }
            else
            {
                DrainSatCur = Parameters.ParallelMultiplier * Properties.TempSatCurDensity * Parameters.DrainArea;
                SourceSatCur = Parameters.ParallelMultiplier * Properties.TempSatCurDensity * Parameters.SourceArea;
            }
            double Beta = Properties.TempTransconductance * Parameters.Width * Parameters.ParallelMultiplier / Properties.EffectiveLength;

            // Get the current voltages
            Initialize(out double vgs, out double vds, out double vbs, out bool check);
            double vbd = vbs - vds;
            double vgd = vgs - vds;

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

            if (vds >= 0)
                Mode = 1;
            else
                Mode = -1;

            // An example out in the wild once-good, now-bad spaghetti code... Not touching this too much.
            {
                /* moseq2(vds,vbs,vgs,gm,gds,gmbs,qg,qc,qb,
                 *        cggb,cgdb,cgsb,cbgb,cbdb,cbsb)
                 */
                // Note:  cgdb, cgsb, cbdb, cbsb never used

                /*
                 *     this routine evaluates the drain current, its derivatives and
                 *     the charges associated with the gate, channel and bulk
                 *     for mosfets
                 *
                 */

                double arg;
                double sarg;
                double[] a4 = new double[4], b4 = new double[4], x4 = new double[8], poly4 = new double[8];
                double beta1;
                double dsrgdb;
                double d2sdb2;
                double barg;
                double d2bdb2;
                double factor;
                double dbrgdb;
                double eta;
                double vbin;
                double argd = 0.0;
                double args = 0.0;
                double argss;
                double argsd;
                double argxs = 0.0;
                double argxd = 0.0;
                double daddb2;
                double dasdb2;
                double dbargd;
                double dbargs;
                double dbxwd;
                double dbxws;
                double dgddb2;
                double dgddvb;
                double dgdvds;
                double gamasd;
                double xwd;
                double xws;
                double ddxwd;
                double gammad;
                double vth;
                double cfs;
                double cdonco;
                double xn = 0.0;
                double argg = 0.0;
                double vgst;
                double sarg3;
                double sbiarg;
                double dgdvbs;
                double body;
                double gdbdv;
                double dodvbs;
                double dodvds = 0.0;
                double dxndvd = 0.0;
                double dxndvb = 0.0;
                double udenom;
                double dudvgs;
                double dudvds;
                double dudvbs;
                double gammd2;
                double argv;
                double vgsx;
                double ufact;
                double ueff;
                double dsdvgs;
                double dsdvbs;
                double a1;
                double a3;
                double a;
                double b1;
                double b3;
                double b;
                double c1;
                double c;
                double d1;
                double fi;
                double p0;
                double p2;
                double p3;
                double p4;
                double p;
                double r3;
                double r;
                double ro;
                double s2;
                double s;
                double v1;
                double v2;
                double xv;
                double y3;
                double delta4;
                double xvalid = 0;
                double bsarg = 0;
                double dbsrdb;
                double bodys = 0;
                double gdbdvs = 0;
                double sargv;
                double xlfact;
                double dldsat;
                double xdv;
                double xlv;
                double vqchan;
                double dqdsat;
                double vl;
                double dfundg;
                double dfunds;
                double dfundb;
                double xls;
                double dldvgs;
                double dldvds;
                double dldvbs;
                double dfact;
                double clfact;
                double xleff;
                double deltal;
                double xwb;
                double vdson;
                double cdson;
                double didvds;
                double gdson;
                double gmw;
                double gbson;
                double expg;
                double xld;
                double xlamda = ModelParameters.Lambda;
                /* 'local' variables - these switch d & s around appropriately
                 * so that we don't have to worry about vds < 0
                 */
                double lvbs = Mode == 1 ? vbs : vbd;
                double lvds = Mode * vds;
                double lvgs = Mode == 1 ? vgs : vgd;
                double phiMinVbs = Properties.TempPhi - lvbs;
                double tmp; /* a temporary variable, not used for more than */
                /* about 10 lines at a time */
                int iknt;
                int jknt;
                int i;
                int j;

                double sphi;
                double sphi3;
                /*
                 *  compute some useful quantities
                 */

                if (lvbs <= 0.0)
                {
                    sarg = Math.Sqrt(phiMinVbs);
                    dsrgdb = -0.5 / sarg;
                    d2sdb2 = 0.5 * dsrgdb / phiMinVbs;
                }
                else
                {
                    sphi = Math.Sqrt(Properties.TempPhi);
                    sphi3 = Properties.TempPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / Properties.TempPhi);
                    tmp = sarg / sphi3;
                    dsrgdb = -0.5 * sarg * tmp;
                    d2sdb2 = -dsrgdb * tmp;
                }
                if ((lvbs - lvds) <= 0)
                {
                    barg = Math.Sqrt(phiMinVbs + lvds);
                    dbrgdb = -0.5 / barg;
                    d2bdb2 = 0.5 * dbrgdb / (phiMinVbs + lvds);
                }
                else
                {
                    sphi = Math.Sqrt(Properties.TempPhi);/* added by HT 050523 */
                    sphi3 = Properties.TempPhi * sphi;/* added by HT 050523 */
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / Properties.TempPhi);
                    tmp = barg / sphi3;
                    dbrgdb = -0.5 * barg * tmp;
                    d2bdb2 = -dbrgdb * tmp;
                }
                /*
                 *  calculate threshold voltage (von)
                 *     narrow-channel effect
                 */

                // XXX constant per device
                factor = 0.125 * ModelParameters.NarrowFactor * 2.0 * Math.PI * EpsilonSilicon / Properties.OxideCap * Properties.EffectiveLength;
                // XXX constant per device
                eta = 1.0 + factor;
                vbin = Properties.TempVbi * ModelParameters.MosfetType + factor * phiMinVbs;
                if ((ModelParameters.Gamma > 0.0) || (ModelParameters.SubstrateDoping > 0.0))
                {
                    xwd = ModelTemperature.Properties.Xd * barg;
                    xws = ModelTemperature.Properties.Xd * sarg;

                    // Short-channel effect with vds .ne. 0.0
                    argss = 0.0;
                    argsd = 0.0;
                    dbargs = 0.0;
                    dbargd = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                    if (ModelParameters.JunctionDepth > 0)
                    {
                        tmp = 2.0 / ModelParameters.JunctionDepth;
                        argxs = 1.0 + xws * tmp;
                        argxd = 1.0 + xwd * tmp;
                        args = Math.Sqrt(argxs);
                        argd = Math.Sqrt(argxd);
                        tmp = .5 * ModelParameters.JunctionDepth / Properties.EffectiveLength;
                        argss = tmp * (args - 1.0);
                        argsd = tmp * (argd - 1.0);
                    }
                    gamasd = ModelParameters.Gamma * (1.0 - argss - argsd);
                    dbxwd = ModelTemperature.Properties.Xd * dbrgdb;
                    dbxws = ModelTemperature.Properties.Xd * dsrgdb;
                    if (ModelParameters.JunctionDepth > 0)
                    {
                        tmp = 0.5 / Properties.EffectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        dasdb2 = -ModelTemperature.Properties.Xd * (d2sdb2 + dsrgdb * dsrgdb *
                            ModelTemperature.Properties.Xd / (ModelParameters.JunctionDepth * argxs)) /
                            (Properties.EffectiveLength * args);
                        daddb2 = -ModelTemperature.Properties.Xd * (d2bdb2 + dbrgdb * dbrgdb *
                            ModelTemperature.Properties.Xd / (ModelParameters.JunctionDepth * argxd)) /
                            (Properties.EffectiveLength * argd);
                        dgddb2 = -0.5 * ModelParameters.Gamma * (dasdb2 + daddb2);
                    }
                    dgddvb = -ModelParameters.Gamma * (dbargs + dbargd);
                    if (ModelParameters.JunctionDepth > 0)
                    {
                        ddxwd = -dbxwd;
                        dgdvds = -ModelParameters.Gamma * 0.5 * ddxwd / (Properties.EffectiveLength * argd);
                    }
                }
                else
                {
                    gamasd = ModelParameters.Gamma;
                    dgddvb = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                }
                double von = vbin + gamasd * sarg;
                vth = von;
                double vdsat = 0.0;
                if (ModelParameters.FastSurfaceStateDensity != 0.0 && Properties.OxideCap != 0.0)
                {
                    // XXX constant per model
                    cfs = Constants.Charge * ModelParameters.FastSurfaceStateDensity * 1e4 /*(cm**2/m**2)*/;
                    cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                    xn = 1.0 + cfs / Properties.OxideCap * Parameters.ParallelMultiplier * Parameters.Width * Properties.EffectiveLength + cdonco;

                    tmp = vt * xn;
                    von += tmp;
                    argg = 1.0 / tmp;
                    vgst = lvgs - von;
                }
                else
                {
                    vgst = lvgs - von;
                    if (lvgs <= vbin)
                    {
                        // Cutoff region
                        con.Ds.G = 0.0;
                        goto line1050;
                    }
                }

                /*
                 *  compute some more useful quantities
                 */

                sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                sbiarg = Math.Sqrt(Properties.TempBulkPotential);
                gammad = gamasd;
                dgdvbs = dgddvb;
                body = barg * barg * barg - sarg3;
                gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
                dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
                if (ModelParameters.FastSurfaceStateDensity == 0.0) goto line400;
                if (Properties.OxideCap == 0.0)
                    goto line410;
                dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2sdb2 + dgddb2 * sarg;
                dodvbs += vt * dxndvb;
                dxndvd = dgdvds * dsrgdb;
                dodvds = dgdvds * sarg + vt * dxndvd;
            /*
             *  evaluate effective mobility and its derivatives
             */
            line400:
                if (Properties.OxideCap <= 0.0)
                    goto line410;
                udenom = vgst;
                tmp = ModelParameters.CriticalField * 100 /* cm/m */ * EpsilonSilicon / ModelTemperature.Properties.OxideCapFactor;
                if (udenom <= tmp)
                    goto line410;
                ufact = Math.Exp(ModelParameters.CriticalFieldExp * Math.Log(tmp / udenom));
                ueff = ModelParameters.SurfaceMobility * 1e-4 /*(m**2/cm**2) */ * ufact;
                dudvgs = -ufact * ModelParameters.CriticalFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = ModelParameters.CriticalFieldExp * ufact * dodvbs / vgst;
                goto line500;
            line410:
                ufact = 1.0;
                ueff = ModelParameters.SurfaceMobility * 1e-4 /*(m**2/cm**2) */ ;
                dudvgs = 0.0;
                dudvds = 0.0;
                dudvbs = 0.0;
            /*
             *     evaluate saturation voltage and its derivatives according to
             *     grove-frohman equation
             */
            line500:
                vgsx = lvgs;
                gammad = gamasd / eta;
                dgdvbs = dgddvb;
                if (ModelParameters.FastSurfaceStateDensity != 0 && Properties.OxideCap != 0)
                {
                    vgsx = Math.Max(lvgs, von);
                }
                if (gammad > 0)
                {
                    gammd2 = gammad * gammad;
                    argv = (vgsx - vbin) / eta + phiMinVbs;
                    if (argv <= 0.0)
                    {
                        vdsat = 0.0;
                        dsdvgs = 0.0;
                        dsdvbs = 0.0;
                    }
                    else
                    {
                        arg = Math.Sqrt(1.0 + 4.0 * argv / gammd2);
                        vdsat = (vgsx - vbin) / eta + gammd2 * (1.0 - arg) / 2.0;
                        vdsat = Math.Max(vdsat, 0.0);
                        dsdvgs = (1.0 - 1.0 / arg) / eta;
                        dsdvbs = (gammad * (1.0 - arg) + 2.0 * argv / (gammad * arg)) /
                            eta * dgdvbs + 1.0 / arg + factor * dsdvgs;
                    }
                }
                else
                {
                    vdsat = (vgsx - vbin) / eta;
                    vdsat = Math.Max(vdsat, 0.0);
                    dsdvgs = 1.0;
                    dsdvbs = 0.0;
                }
                if (ModelParameters.MaxDriftVelocity > 0)
                {
                    /* 
                     *     evaluate saturation voltage and its derivatives 
                     *     according to baum's theory of scattering velocity 
                     *     saturation
                     */
                    v1 = (vgsx - vbin) / eta + phiMinVbs;
                    v2 = phiMinVbs;
                    xv = ModelParameters.MaxDriftVelocity * Properties.EffectiveLength / ueff;
                    a1 = gammad / 0.75;
                    b1 = -2.0 * (v1 + xv);
                    c1 = -2.0 * gammad * xv;
                    d1 = 2.0 * v1 * (v2 + xv) - v2 * v2 - 4.0 / 3.0 * gammad * sarg3;
                    a = -b1;
                    b = a1 * c1 - 4.0 * d1;
                    c = -d1 * (a1 * a1 - 4.0 * b1) - c1 * c1;
                    r = -a * a / 3.0 + b;
                    s = 2.0 * a * a * a / 27.0 - a * b / 3.0 + c;
                    r3 = r * r * r;
                    s2 = s * s;
                    p = s2 / 4.0 + r3 / 27.0;
                    p0 = Math.Abs(p);
                    p2 = Math.Sqrt(p0);
                    if (p < 0)
                    {
                        ro = Math.Sqrt(s2 / 4.0 + p0);
                        ro = Math.Log(ro) / 3.0;
                        ro = Math.Exp(ro);
                        fi = Math.Atan(-2.0 * p2 / s);
                        y3 = 2.0 * ro * Math.Cos(fi / 3.0) - a / 3.0;
                    }
                    else
                    {
                        p3 = -s / 2.0 + p2;
                        p3 = Math.Exp(Math.Log(Math.Abs(p3)) / 3.0);
                        p4 = -s / 2.0 - p2;
                        p4 = Math.Exp(Math.Log(Math.Abs(p4)) / 3.0);
                        y3 = p3 + p4 - a / 3.0;
                    }
                    iknt = 0;
                    a3 = Math.Sqrt(a1 * a1 / 4.0 - b1 + y3);
                    b3 = Math.Sqrt(y3 * y3 / 4.0 - d1);
                    for (i = 1; i <= 4; i++)
                    {
                        a4[i - 1] = a1 / 2.0 + _sig1[i - 1] * a3;
                        b4[i - 1] = y3 / 2.0 + _sig2[i - 1] * b3;
                        delta4 = a4[i - 1] * a4[i - 1] / 4.0 - b4[i - 1];
                        if (delta4 < 0)
                            continue;
                        iknt += 1;
                        tmp = Math.Sqrt(delta4);
                        x4[iknt - 1] = -a4[i - 1] / 2.0 + tmp;
                        iknt += 1;
                        x4[iknt - 1] = -a4[i - 1] / 2.0 - tmp;
                    }
                    jknt = 0;
                    for (j = 1; j <= iknt; j++)
                    {
                        if (x4[j - 1] <= 0)
                            continue;
                        /* XXX implement this sanely */
                        poly4[j - 1] = x4[j - 1] * x4[j - 1] * x4[j - 1] * x4[j - 1] + a1 * x4[j - 1] *
                            x4[j - 1] * x4[j - 1];
                        poly4[j - 1] = poly4[j - 1] + b1 * x4[j - 1] * x4[j - 1] + c1 * x4[j - 1] + d1;
                        if (Math.Abs(poly4[j - 1]) > 1.0e-6)
                            continue;
                        jknt += 1;
                        if (jknt <= 1)
                        {
                            xvalid = x4[j - 1];
                        }
                        if (x4[j - 1] > xvalid)
                            continue;
                        xvalid = x4[j - 1];
                    }
                    if (jknt > 0)
                    {
                        vdsat = xvalid * xvalid - phiMinVbs;
                    }
                }
                // Evaluate effective channel length and its derivatives
                if (lvds != 0.0)
                {
                    gammad = gamasd;
                    if ((lvbs - vdsat) <= 0)
                    {
                        bsarg = Math.Sqrt(vdsat + phiMinVbs);
                        dbsrdb = -0.5 / bsarg;
                    }
                    else
                    {
                        sphi = Math.Sqrt(Properties.TempPhi);/* added by HT 050523 */
                        sphi3 = Properties.TempPhi * sphi;/* added by HT 050523 */
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / Properties.TempPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    if (ModelParameters.MaxDriftVelocity <= 0)
                    {
                        if (ModelParameters.SubstrateDoping == 0.0 || xlamda > 0.0)
                        {
                            dldvgs = 0.0;
                            dldvds = 0.0;
                            dldvbs = 0.0;
                        }
                        else
                        {
                            argv = (lvds - vdsat) / 4.0;
                            sargv = Math.Sqrt(1.0 + argv * argv);
                            arg = Math.Sqrt(argv + sargv);
                            xlfact = ModelTemperature.Properties.Xd / (Properties.EffectiveLength * lvds);
                            xlamda = xlfact * arg;
                            dldsat = lvds * xlamda / (8.0 * sargv);
                            dldvgs = dldsat * dsdvgs;
                            dldvds = -xlamda + dldsat;
                            dldvbs = dldsat * dsdvbs;
                        }
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        xdv = ModelTemperature.Properties.Xd / Math.Sqrt(ModelParameters.ChannelCharge);
                        xlv = ModelParameters.MaxDriftVelocity * xdv / (2.0 * ueff);
                        vqchan = argv - gammad * bsarg;
                        dqdsat = -1.0 + gammad * dbsrdb;
                        vl = ModelParameters.MaxDriftVelocity * Properties.EffectiveLength;
                        dfunds = vl * dqdsat - ueff * vqchan;
                        dfundg = (vl - ueff * vdsat) / eta;
                        dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff *
                            (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                        dsdvgs = -dfundg / dfunds;
                        dsdvbs = -dfundb / dfunds;
                        if (ModelParameters.SubstrateDoping == 0.0 || xlamda > 0.0)
                        {
                            dldvgs = 0.0;
                            dldvds = 0.0;
                            dldvbs = 0.0;
                        }
                        else
                        {
                            argv = lvds - vdsat;
                            argv = Math.Max(argv, 0.0);
                            xls = Math.Sqrt(xlv * xlv + argv);
                            dldsat = xdv / (2.0 * xls);
                            xlfact = xdv / (Properties.EffectiveLength * lvds);
                            xlamda = xlfact * (xls - xlv);
                            dldsat /= Properties.EffectiveLength;
                            dldvgs = dldsat * dsdvgs;
                            dldvds = -xlamda + dldsat;
                            dldvbs = dldsat * dsdvbs;
                        }
                    }
                }
                else
                {
                    dldvgs = 0.0;
                    dldvds = 0.0;
                    dldvbs = 0.0;
                }
                /*
                 *     limit channel shortening at punch-through
                 */
                xwb = ModelTemperature.Properties.Xd * sbiarg;
                xld = Properties.EffectiveLength - xwb;
                clfact = 1.0 - xlamda * lvds;
                dldvds = -xlamda - dldvds;
                xleff = Properties.EffectiveLength * clfact;
                deltal = xlamda * lvds * Properties.EffectiveLength;
                if (ModelParameters.SubstrateDoping == 0.0) xwb = 0.25e-6;
                if (xleff < xwb)
                {
                    xleff = xwb / (1.0 + (deltal - xld) / xwb);
                    clfact = xleff / Properties.EffectiveLength;
                    dfact = xleff * xleff / (xwb * xwb);
                    dldvgs = dfact * dldvgs;
                    dldvds = dfact * dldvds;
                    dldvbs = dfact * dldvbs;
                }

                // Evaluate effective beta (effective kp)
                beta1 = Beta * ufact / clfact;

                // Test for mode of operation and branch appropriately
                gammad = gamasd;
                dgdvbs = dgddvb;
                if (lvds <= 1.0e-10)
                {
                    if (lvgs <= von)
                    {
                        if ((ModelParameters.FastSurfaceStateDensity == 0.0) || (Properties.OxideCap == 0.0))
                        {
                            con.Ds.G = 0.0;
                            goto line1050;
                        }

                        con.Ds.G = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg *
                            (lvgs - von));
                        goto line1050;
                    }

                    con.Ds.G = beta1 * (lvgs - vbin - gammad * sarg);
                    goto line1050;
                }

                if (ModelParameters.FastSurfaceStateDensity != 0 && Properties.OxideCap != 0)
                {
                    if (lvgs > von)
                        goto line900;
                }
                else
                {
                    if (lvgs > vbin)
                        goto line900;
                    goto doneval;
                }

                if (lvgs > von)
                    goto line900;

                // Subthreshold region
                if (vdsat <= 0)
                {
                    con.Ds.G = 0.0;
                    if (lvgs > vth) goto doneval;
                    goto line1050;
                }
                vdson = Math.Min(vdsat, lvds);
                if (lvds > vdsat)
                {
                    barg = bsarg;
                    body = bodys;
                    gdbdv = gdbdvs;
                }
                cdson = beta1 * ((von - vbin - eta * vdson * 0.5) * vdson - gammad * body / 1.5);
                didvds = beta1 * (von - vbin - eta * vdson - gammad * barg);
                gdson = -cdson * dldvds / clfact - beta1 * dgdvds * body / 1.5;
                if (lvds < vdsat)
                    gdson += didvds;
                gbson = -cdson * dldvbs / clfact + beta1 * (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
                if (lvds > vdsat)
                    gbson += didvds * dsdvbs;
                expg = Math.Exp(argg * (lvgs - von));
                con.Ds.C = cdson * expg;
                gmw = con.Ds.C * argg;
                Gm = gmw;
                if (lvds > vdsat)
                    Gm = gmw + didvds * dsdvgs * expg;
                tmp = gmw * (lvgs - von) / xn;
                con.Ds.G = gdson * expg - Gm * dodvds - tmp * dxndvd;
                Gmbs = gbson * expg - Gm * dodvbs - tmp * dxndvb;
                goto doneval;

            line900:
                if (lvds <= vdsat)
                {
                    // Linear region
                    con.Ds.C = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                    arg = con.Ds.C * (dudvgs / ufact - dldvgs / clfact);
                    Gm = arg + beta1 * lvds;
                    arg = con.Ds.C * (dudvds / ufact - dldvds / clfact);
                    con.Ds.G = arg + beta1 * (lvgs - vbin - eta * lvds - gammad * barg - dgdvds * body / 1.5);
                    arg = con.Ds.C * (dudvbs / ufact - dldvbs / clfact);
                    Gmbs = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
                }
                else
                {
                    // Saturation region
                    con.Ds.C = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = con.Ds.C * (dudvgs / ufact - dldvgs / clfact);
                    Gm = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    con.Ds.G = -con.Ds.C * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = con.Ds.C * (dudvbs / ufact - dldvbs / clfact);
                    Gmbs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvbs;
                }
                // Compute charges for "on" region
                goto doneval;
            // Finish special cases
            line1050:
                con.Ds.C = 0.0;
                Gm = 0.0;
                Gmbs = 0.0;
            // Finished
            doneval:
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

            // Right hand side contributions
            double xnrm, xrev;
            con.Bs.C = ModelParameters.MosfetType * (con.Bs.C - con.Bs.G * vbs);
            con.Bd.C = ModelParameters.MosfetType * (con.Bd.C - con.Bd.G * vbd);
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

            if (_iteration.Mode == IterationModes.Float || (_method != null && _method.BaseTime.Equals(0.0)) ||
                _iteration.Mode == IterationModes.Fix && !Parameters.Off)
            {
                // General iteration
                double s = _variables.SourcePrime.Value;
                vbs = ModelParameters.MosfetType * (_variables.Bulk.Value - s);
                vgs = ModelParameters.MosfetType * (_variables.Gate.Value - s);
                vds = ModelParameters.MosfetType * (_variables.DrainPrime.Value - s);

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

            double s = _variables.SourcePrime.Value;
            double vbs = ModelParameters.MosfetType * (_variables.Bulk.Value - s);
            double vgs = ModelParameters.MosfetType * (_variables.Gate.Value - s);
            double vds = ModelParameters.MosfetType * (_variables.DrainPrime.Value - s);
            double vbd = vbs - vds;
            double vgd = vgs - vds;
            double vgdo = Vgs - Vds;
            double delvbs = vbs - Vbs;
            double delvbd = vbd - Vbd;
            double delvgs = vgs - Vgs;
            double delvds = vds - Vds;
            double delvgd = vgd - vgdo;

            // these are needed for convergence testing
            // NOTE: Cd does not include contributions for transient simulations... Should check for a way to include them!
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

            /*
             *  check convergence
             */
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
