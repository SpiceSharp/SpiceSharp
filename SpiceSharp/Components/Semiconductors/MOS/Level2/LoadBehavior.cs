using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level2
{
    /// <summary>
    /// General behavior of a <see cref="Mosfet2"/>
    /// </summary>
    public class LoadBehavior : Common.LoadBehavior
    {
        /// <summary>
        /// Necessary behaviors and parameters
        /// </summary>
        private BaseParameters _bp;
        private ModelBaseParameters _mbp;
        private TemperatureBehavior _temp;
        private ModelTemperatureBehavior _modeltemp;

        /// <summary>
        /// Some signs used in the model
        /// </summary>
        private static readonly double[] Sig1 = { 1.0, -1.0, 1.0, -1.0 };
        private static readonly double[] Sig2 = { 1.0, 1.0, -1.0, -1.0 };

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name</param>
        public LoadBehavior(string name) : base(name) { }

        /// <summary>
        /// Setup behavior
        /// </summary>
        /// <param name="simulation">Simulation</param>
        /// <param name="provider">Data provider</param>
        public override void Setup(Simulation simulation, SetupDataProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));
            base.Setup(simulation, provider);

            // Get parameters
            _bp = provider.GetParameterSet<BaseParameters>();
            _mbp = provider.GetParameterSet<ModelBaseParameters>("model");

            // Get behaviors
            _temp = provider.GetBehavior<TemperatureBehavior>();
            _modeltemp = provider.GetBehavior<ModelTemperatureBehavior>("model");

            // Reset
            SaturationVoltageDs = 0;
            Von = 0;
            Mode = 1;
        }

        /// <summary>
        /// Unsetup the behavior
        /// </summary>
        /// <param name="simulation"></param>
        public override void Unsetup(Simulation simulation)
        {
            _bp = null;
            _mbp = null;
            _temp = null;
            _modeltemp = null;

            base.Unsetup(simulation);
        }

        /// <summary>
        /// Execute behavior
        /// </summary>
        protected override double Evaluate(double vgs, double vds, double vbs)
        {
            double von;
            double vdsat, cdrain = 0.0;

            var vt = Circuit.KOverQ * _bp.Temperature;
            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var beta = _temp.TempTransconductance * _bp.Width / effectiveLength;
            var oxideCap = _mbp.OxideCapFactor * effectiveLength * _bp.Width;
            
            {
                /*
				* this routine evaluates the drain current, its derivatives and
				* the charges associated with the gate, channel and bulk
				* for mosfets
				*
				*/

                double arg;
                double sarg;
                double[] a4 = new double[4], b4 = new double[4], x4 = new double[8], poly4 = new double[8];
                double dsrgdb, d2Sdb2;
                var sphi = 0.0; /* square root of phi */
                var sphi3 = 0.0; /* square root of phi cubed */
                double barg, d2Bdb2,
                    dbrgdb,
                    argd = 0.0, args = 0.0;
                double argxs = 0.0, argxd = 0.0;
                double dgddb2, dgddvb, dgdvds, gamasd;
                double xn = 0.0, argg = 0.0, vgst,
                    dodvds = 0.0, dxndvd = 0.0, dxndvb = 0.0,
                    dudvgs, dudvds, dudvbs;
                double argv,
                    ufact, ueff, dsdvgs, dsdvbs;
                double xvalid = 0.0, bsarg = 0.0;
                double bodys = 0.0, gdbdvs = 0.0;
                double dldvgs = 0.0, dldvds = 0.0, dldvbs = 0.0;
                double xlamda = _mbp.Lambda;
                /* 'local' variables - these switch d & s around appropriately
				 * so that we don't have to worry about vds < 0
				 */
                var lvbs = vbs;
                var lvds = vds;
                var lvgs = vgs;
                var phiMinVbs = _temp.TempPhi - lvbs;
                double tmp; /* a temporary variable, not used for more than */
                            /* about 10 lines at a time */

                /*
				* compute some useful quantities
				*/

                if (lvbs <= 0.0)
                {
                    sarg = Math.Sqrt(phiMinVbs);
                    dsrgdb = -0.5 / sarg;
                    d2Sdb2 = 0.5 * dsrgdb / phiMinVbs;
                }
                else
                {
                    sphi = Math.Sqrt(_temp.TempPhi);
                    sphi3 = _temp.TempPhi * sphi;
                    sarg = sphi / (1.0 + 0.5 * lvbs / _temp.TempPhi);
                    tmp = sarg / sphi3;
                    dsrgdb = -0.5 * sarg * tmp;
                    d2Sdb2 = -dsrgdb * tmp;
                }
                if (lvds - lvbs >= 0)
                {
                    barg = Math.Sqrt(phiMinVbs + lvds);
                    dbrgdb = -0.5 / barg;
                    d2Bdb2 = 0.5 * dbrgdb / (phiMinVbs + lvds);
                }
                else
                {
                    barg = sphi / (1.0 + 0.5 * (lvbs - lvds) / _temp.TempPhi);
                    tmp = barg / sphi3;
                    dbrgdb = -0.5 * barg * tmp;
                    d2Bdb2 = -dbrgdb * tmp;
                }
                /*
				* calculate threshold voltage (von)
				* narrow - channel effect
				*/

                /* XXX constant per device */
                var factor = 0.125 * _mbp.NarrowFactor * 2.0 * Math.PI * EpsilonSilicon / oxideCap * effectiveLength;
                /* XXX constant per device */
                var eta = 1.0 + factor;
                var vbin = _temp.TempVoltageBi * _mbp.MosfetType + factor * phiMinVbs;
                if (_mbp.Gamma > 0.0 || _mbp.SubstrateDoping > 0.0)
                {
                    var xwd = _modeltemp.Xd * barg;
                    var xws = _modeltemp.Xd * sarg;

                    /*
					* short - channel effect with vds .ne. 0.0
					*/

                    var argss = 0.0;
                    var argsd = 0.0;
                    var dbargs = 0.0;
                    var dbargd = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                    if (_mbp.JunctionDepth > 0)
                    {
                        tmp = 2.0 / _mbp.JunctionDepth;
                        argxs = 1.0 + xws * tmp;
                        argxd = 1.0 + xwd * tmp;
                        args = Math.Sqrt(argxs);
                        argd = Math.Sqrt(argxd);
                        tmp = .5 * _mbp.JunctionDepth / effectiveLength;
                        argss = tmp * (args - 1.0);
                        argsd = tmp * (argd - 1.0);
                    }
                    gamasd = _mbp.Gamma * (1.0 - argss - argsd);
                    var dbxwd = _modeltemp.Xd * dbrgdb;
                    var dbxws = _modeltemp.Xd * dsrgdb;
                    if (_mbp.JunctionDepth > 0)
                    {
                        tmp = 0.5 / effectiveLength;
                        dbargs = tmp * dbxws / args;
                        dbargd = tmp * dbxwd / argd;
                        var dasdb2 = -_modeltemp.Xd * (d2Sdb2 + dsrgdb * dsrgdb * _modeltemp.Xd / (_mbp.JunctionDepth * argxs)) / (effectiveLength *
                                                                                                                                      args);
                        var daddb2 = -_modeltemp.Xd * (d2Bdb2 + dbrgdb * dbrgdb * _modeltemp.Xd / (_mbp.JunctionDepth * argxd)) / (effectiveLength *
                                                                                                                                      argd);
                        dgddb2 = -0.5 * _mbp.Gamma * (dasdb2 + daddb2);
                    }
                    dgddvb = -_mbp.Gamma * (dbargs + dbargd);
                    if (_mbp.JunctionDepth > 0)
                    {
                        var ddxwd = -dbxwd;
                        dgdvds = -_mbp.Gamma * 0.5 * ddxwd / (effectiveLength * argd);
                    }
                }
                else
                {
                    gamasd = _mbp.Gamma;
                    dgddvb = 0.0;
                    dgdvds = 0.0;
                    dgddb2 = 0.0;
                }
                von = vbin + gamasd * sarg;
                var vth = von;
                vdsat = 0.0;
                if (!_mbp.FastSurfaceStateDensity.Value.Equals(0.0) && !oxideCap.Equals(0.0))
                {
                    /* XXX constant per model */
                    var cfs = Circuit.Charge * _mbp.FastSurfaceStateDensity * 1e4;
                    var cdonco = -(gamasd * dsrgdb + dgddvb * sarg) + factor;
                    xn = 1.0 + cfs / oxideCap * _bp.Width * effectiveLength + cdonco;
                    tmp = vt * xn;
                    von = von + tmp;
                    argg = 1.0 / tmp;
                    vgst = lvgs - von;
                }
                else
                {
                    vgst = lvgs - von;
                    if (lvgs <= von)
                    {
                        /*
						 * cutoff region
						 */
                        CondDs = 0.0;
                        goto line1050;
                    }
                }

                /*
				* compute some more useful quantities
				*/

                var sarg3 = sarg * sarg * sarg;
                /* XXX constant per model */
                var sbiarg = Math.Sqrt(_temp.TempBulkPotential);
                var gammad = gamasd;
                var dgdvbs = dgddvb;
                var body = barg * barg * barg - sarg3;
                var gdbdv = 2.0 * gammad * (barg * barg * dbrgdb - sarg * sarg * dsrgdb);
                var dodvbs = -factor + dgdvbs * sarg + gammad * dsrgdb;
                if (_mbp.FastSurfaceStateDensity.Value.Equals(0.0))
                    goto line400;
                if (oxideCap.Equals(0.0))
                    goto line410;
                dxndvb = 2.0 * dgdvbs * dsrgdb + gammad * d2Sdb2 + dgddb2 * sarg;
                dodvbs = dodvbs + vt * dxndvb;
                dxndvd = dgdvds * dsrgdb;
                dodvds = dgdvds * sarg + vt * dxndvd;
                /*
				* evaluate effective mobility and its derivatives
				*/
                line400:
                if (oxideCap <= 0.0) goto line410;
                var udenom = vgst;
                tmp = _mbp.CriticalField * 100 /*cm/m*/  * EpsilonSilicon / _mbp.OxideCapFactor;
                if (udenom <= tmp) goto line410;
                ufact = Math.Exp(_mbp.CriticalFieldExp * Math.Log(tmp / udenom));
                ueff = _mbp.SurfaceMobility * 1e-4 /* (m *  * 2 / cm *  * 2) */  * ufact;
                dudvgs = -ufact * _mbp.CriticalFieldExp / udenom;
                dudvds = 0.0;
                dudvbs = _mbp.CriticalFieldExp * ufact * dodvbs / vgst;
                goto line500;
                line410:
                ufact = 1.0;
                ueff = _mbp.SurfaceMobility * 1e-4; /* (m^2/cm^2) */
                dudvgs = 0.0;
                dudvds = 0.0;
                dudvbs = 0.0;
                /*
				 * Evaluate saturation voltage and its derivatives according to
				 * grove - frohman equation
				 */
                line500:
                var vgsx = lvgs;
                gammad = gamasd / eta;
                dgdvbs = dgddvb;
                if (!_mbp.FastSurfaceStateDensity.Value.Equals(0.0) && !oxideCap.Equals(0.0))
                {
                    vgsx = Math.Max(lvgs, von);
                }
                if (gammad > 0)
                {
                    var gammd2 = gammad * gammad;
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
                        dsdvbs = (gammad * (1.0 - arg) + 2.0 * argv / (gammad * arg)) / eta * dgdvbs + 1.0 / arg + factor * dsdvgs;
                    }
                }
                else
                {
                    vdsat = (vgsx - vbin) / eta;
                    vdsat = Math.Max(vdsat, 0.0);
                    dsdvgs = 1.0;
                    dsdvbs = 0.0;
                }
                if (_mbp.MaxDriftVelocity > 0)
                {
                    /*
					 * evaluate saturation voltage and its derivatives
					 * according to baum's theory of scattering velocity
					 * saturation
					 */
                    var v1 = (vgsx - vbin) / eta + phiMinVbs;
                    var v2 = phiMinVbs;
                    var xv = _mbp.MaxDriftVelocity * effectiveLength / ueff;
                    var a1 = gammad / 0.75;
                    var b1 = -2.0 * (v1 + xv);
                    var c1 = -2.0 * gammad * xv;
                    var d1 = 2.0 * v1 * (v2 + xv) - v2 * v2 - 4.0 / 3.0 * gammad * sarg3;
                    var a = -b1;
                    var b = a1 * c1 - 4.0 * d1;
                    var c = -d1 * (a1 * a1 - 4.0 * b1) - c1 * c1;
                    var r = -a * a / 3.0 + b;
                    var s = 2.0 * a * a * a / 27.0 - a * b / 3.0 + c;
                    var r3 = r * r * r;
                    var s2 = s * s;
                    var p = s2 / 4.0 + r3 / 27.0;
                    var p0 = Math.Abs(p);
                    var p2 = Math.Sqrt(p0);
                    double y3;
                    if (p < 0)
                    {
                        var ro = Math.Sqrt(s2 / 4.0 + p0);
                        ro = Math.Log(ro) / 3.0;
                        ro = Math.Exp(ro);
                        var fi = Math.Atan(-2.0 * p2 / s);
                        y3 = 2.0 * ro * Math.Cos(fi / 3.0) - a / 3.0;
                    }
                    else
                    {
                        var p3 = -s / 2.0 + p2;
                        p3 = Math.Exp(Math.Log(Math.Abs(p3)) / 3.0);
                        var p4 = -s / 2.0 - p2;
                        p4 = Math.Exp(Math.Log(Math.Abs(p4)) / 3.0);
                        y3 = p3 + p4 - a / 3.0;
                    }
                    var iknt = 0;
                    var a3 = Math.Sqrt(a1 * a1 / 4.0 - b1 + y3);
                    var b3 = Math.Sqrt(y3 * y3 / 4.0 - d1);
                    for (var i = 1; i <= 4; i++)
                    {
                        a4[i - 1] = a1 / 2.0 + Sig1[i - 1] * a3;
                        b4[i - 1] = y3 / 2.0 + Sig2[i - 1] * b3;
                        var delta4 = a4[i - 1] * a4[i - 1] / 4.0 - b4[i - 1];
                        if (delta4 < 0)
                            continue;
                        iknt = iknt + 1;
                        tmp = Math.Sqrt(delta4);
                        x4[iknt - 1] = -a4[i - 1] / 2.0 + tmp;
                        iknt = iknt + 1;
                        x4[iknt - 1] = -a4[i - 1] / 2.0 - tmp;
                    }
                    var jknt = 0;
                    for (var j = 1; j <= iknt; j++)
                    {
                        if (x4[j - 1] <= 0) continue;
                        /* XXX implement this sanely */
                        poly4[j - 1] = x4[j - 1] * x4[j - 1] * x4[j - 1] * x4[j - 1] + a1 * x4[j - 1] * x4[j - 1] * x4[j - 1];
                        poly4[j - 1] = poly4[j - 1] + b1 * x4[j - 1] * x4[j - 1] + c1 * x4[j - 1] + d1;
                        if (Math.Abs(poly4[j - 1]) > 1.0e-6)
                            continue;
                        jknt = jknt + 1;
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
                /*
				 * evaluate effective channel length and its derivatives
				 */
                if (!lvds.Equals(0.0))
                {
                    gammad = gamasd;
                    double dbsrdb;
                    if (lvbs - vdsat <= 0)
                    {
                        bsarg = Math.Sqrt(vdsat + phiMinVbs);
                        dbsrdb = -0.5 / bsarg;
                    }
                    else
                    {
                        bsarg = sphi / (1.0 + 0.5 * (lvbs - vdsat) / _temp.TempPhi);
                        dbsrdb = -0.5 * bsarg * bsarg / sphi3;
                    }
                    bodys = bsarg * bsarg * bsarg - sarg3;
                    gdbdvs = 2.0 * gammad * (bsarg * bsarg * dbsrdb - sarg * sarg * dsrgdb);
                    double xlfact;
                    double dldsat;
                    if (_mbp.MaxDriftVelocity <= 0)
                    {
                        if (_mbp.SubstrateDoping.Value.Equals(0.0))
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = (lvds - vdsat) / 4.0;
                        var sargv = Math.Sqrt(1.0 + argv * argv);
                        arg = Math.Sqrt(argv + sargv);
                        xlfact = _modeltemp.Xd / (effectiveLength * lvds);
                        xlamda = xlfact * arg;
                        dldsat = lvds * xlamda / (8.0 * sargv);
                    }
                    else
                    {
                        argv = (vgsx - vbin) / eta - vdsat;
                        var xdv = _modeltemp.Xd / Math.Sqrt(_mbp.ChannelCharge);
                        var xlv = _mbp.MaxDriftVelocity * xdv / (2.0 * ueff);
                        var vqchan = argv - gammad * bsarg;
                        var dqdsat = -1.0 + gammad * dbsrdb;
                        var vl = _mbp.MaxDriftVelocity * effectiveLength;
                        var dfunds = vl * dqdsat - ueff * vqchan;
                        var dfundg = (vl - ueff * vdsat) / eta;
                        var dfundb = -vl * (1.0 + dqdsat - factor / eta) + ueff * (gdbdvs - dgdvbs * bodys / 1.5) / eta;
                        dsdvgs = -dfundg / dfunds;
                        dsdvbs = -dfundb / dfunds;
                        if (_mbp.SubstrateDoping.Value.Equals(0.0))
                            goto line610;
                        if (xlamda > 0.0)
                            goto line610;
                        argv = lvds - vdsat;
                        argv = Math.Max(argv, 0.0);
                        var xls = Math.Sqrt(xlv * xlv + argv);
                        dldsat = xdv / (2.0 * xls);
                        xlfact = xdv / (effectiveLength * lvds);
                        xlamda = xlfact * (xls - xlv);
                        dldsat = dldsat / effectiveLength;
                    }
                    dldvgs = dldsat * dsdvgs;
                    dldvds = -xlamda + dldsat;
                    dldvbs = dldsat * dsdvbs;
                }

                // Edited to work
                goto line610_finish;
                line610:
                dldvgs = 0.0;
                dldvds = 0.0;
                dldvbs = 0.0;
                line610_finish:

                /*
				* limit channel shortening at punch - through
				*/
                var xwb = _modeltemp.Xd * sbiarg;
                var xld = effectiveLength - xwb;
                var clfact = 1.0 - xlamda * lvds;
                dldvds = -xlamda - dldvds;
                var xleff = effectiveLength * clfact;
                var deltal = xlamda * lvds * effectiveLength;
                if (_mbp.SubstrateDoping.Value.Equals(0.0))
                    xwb = 0.25e-6;
                if (xleff < xwb)
                {
                    xleff = xwb / (1.0 + (deltal - xld) / xwb);
                    clfact = xleff / effectiveLength;
                    var dfact = xleff * xleff / (xwb * xwb);
                    dldvgs = dfact * dldvgs;
                    dldvds = dfact * dldvds;
                    dldvbs = dfact * dldvbs;
                }
                /*
				 * evaluate effective beta (effective kp)
				 */
                var beta1 = beta * ufact / clfact;
                /*
				 * test for mode of operation and branch appropriately
				 */
                gammad = gamasd;
                dgdvbs = dgddvb;
                if (lvds <= 1.0e-10)
                {
                    if (lvgs <= von)
                    {
                        if (_mbp.FastSurfaceStateDensity.Value.Equals(0.0) || oxideCap.Equals(0.0))
                        {
                            CondDs = 0.0;
                            goto line1050;
                        }

                        CondDs = beta1 * (von - vbin - gammad * sarg) * Math.Exp(argg * (lvgs - von));
                        goto line1050;
                    }

                    CondDs = beta1 * (lvgs - vbin - gammad * sarg);
                    goto line1050;
                }

                if (lvgs > von)
                    goto line900;
                /*
				* subthreshold region
				*/
                if (vdsat <= 0)
                {
                    CondDs = 0.0;
                    if (lvgs > vth)
                        goto doneval;
                    goto line1050;
                }
                var vdson = Math.Min(vdsat, lvds);
                if (lvds > vdsat)
                {
                    barg = bsarg;
                    body = bodys;
                    gdbdv = gdbdvs;
                }
                var cdson = beta1 * ((von - vbin - eta * vdson * 0.5) * vdson - gammad * body / 1.5);
                var didvds = beta1 * (von - vbin - eta * vdson - gammad * barg);
                var gdson = -cdson * dldvds / clfact - beta1 * dgdvds * body / 1.5;
                if (lvds < vdsat)
                    gdson = gdson + didvds;
                var gbson = -cdson * dldvbs / clfact + beta1 * (dodvbs * vdson + factor * vdson - dgdvbs * body / 1.5 - gdbdv);
                if (lvds > vdsat)
                    gbson = gbson + didvds * dsdvbs;
                var expg = Math.Exp(argg * (lvgs - von));
                cdrain = cdson * expg;
                var gmw = cdrain * argg;
                Transconductance = gmw;
                if (lvds > vdsat)
                    Transconductance = gmw + didvds * dsdvgs * expg;
                tmp = gmw * (lvgs - von) / xn;
                CondDs = gdson * expg - Transconductance * dodvds - tmp * dxndvd;
                TransconductanceBs = gbson * expg - Transconductance * dodvbs - tmp * dxndvb;
                goto doneval;

                line900:
                if (lvds <= vdsat)
                {
                    /*
					* linear region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * lvds / 2.0) * lvds - gammad * body / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Transconductance = arg + beta1 * lvds;
                    arg = cdrain * (dudvds / ufact - dldvds / clfact);
                    CondDs = arg + beta1 * (lvgs - vbin - eta * lvds - gammad * barg - dgdvds * body / 1.5);
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    TransconductanceBs = arg - beta1 * (gdbdv + dgdvbs * body / 1.5 - factor * lvds);
                }
                else
                {
                    /*
					* saturation region
					*/
                    cdrain = beta1 * ((lvgs - vbin - eta * vdsat / 2.0) * vdsat - gammad * bodys / 1.5);
                    arg = cdrain * (dudvgs / ufact - dldvgs / clfact);
                    Transconductance = arg + beta1 * vdsat + beta1 * (lvgs - vbin - eta * vdsat - gammad * bsarg) * dsdvgs;
                    CondDs = -cdrain * dldvds / clfact - beta1 * dgdvds * bodys / 1.5;
                    arg = cdrain * (dudvbs / ufact - dldvbs / clfact);
                    TransconductanceBs = arg - beta1 * (gdbdvs + dgdvbs * bodys / 1.5 - factor * vdsat) + beta1 * (lvgs - vbin - eta * vdsat - gammad *
                        bsarg) * dsdvbs;
                }
                /*
				* compute charges for "on" region
				*/
                goto doneval;
                /*
				* finish special cases
				*/
                line1050:
                cdrain = 0.0;
                Transconductance = 0.0;
                TransconductanceBs = 0.0;
                /*
				* finished
				*/
            }

            doneval:
            Von = _mbp.MosfetType * von;
            SaturationVoltageDs = _mbp.MosfetType * vdsat;
            return cdrain;
        }
    }
}
