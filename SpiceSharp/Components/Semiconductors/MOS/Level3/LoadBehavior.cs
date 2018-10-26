using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.MosfetBehaviors.Level3
{
    /// <summary>
    /// General behavior for a <see cref="Mosfet3"/>
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
            double vdsat, cdrain;
            var vt = Circuit.KOverQ * _bp.Temperature;
            var effectiveLength = _bp.Length - 2 * _mbp.LateralDiffusion;
            var beta = _temp.TempTransconductance * _bp.Width / effectiveLength;
            var oxideCap = _mbp.OxideCapFactor * effectiveLength * _bp.Width;
            double von;

            {
                var coeff0 = 0.0631353e0;
                var coeff1 = 0.8013292e0;
                var coeff2 = -0.01110777e0;
                double phibs; /* phi - vbs */
                double sqphbs; /* square root of phibs */
                double dsqdvb; /*  */
                double arga, argb, argc;
                double dfsdvb;
                double dxndvb = 0.0, dvodvb = 0.0, dvodvd = 0.0,
                    dvsdvg, dvsdvb, dvsdvd, xn = 0.0;
                var onvdsc = 0.0;
                var fdrain = 0.0;
                double dfddvg = 0.0, dfddvb = 0.0,
                    dfddvd = 0.0,
                    delxl, dldvd,
                    ddldvg, ddldvd, ddldvb,
                    gds0 = 0.0;
                double fshort;

                /*
				 * reference cdrain equations to source and
				 * charge equations to bulk
				 */
                vdsat = 0.0;
                var oneoverxl = 1.0 / effectiveLength;
                var eta = _mbp.Eta * 8.15e-22 / (_mbp.OxideCapFactor * effectiveLength * effectiveLength * effectiveLength);
                /*
				* .....square root term
				*/
                if (vbs <= 0.0)
                {
                    phibs = _temp.TempPhi - vbs;
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    var sqphis = Math.Sqrt(_temp.TempPhi); /* square root of phi */
                    var sqphs3 = _temp.TempPhi * sqphis; /* square root of phi cubed */
                    sqphbs = sqphis / (1.0 + vbs / (_temp.TempPhi + _temp.TempPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }
                /*
				 * .....short channel effect factor
				 */
                if (_mbp.JunctionDepth > 0 && _modeltemp.CoefficientDepletionLayerWidth > 0.0)
                {
                    var wps = _modeltemp.CoefficientDepletionLayerWidth * sqphbs;
                    var oneoverxj = 1.0 / _mbp.JunctionDepth; /* 1 / junction depth */
                    var xjonxl = _mbp.JunctionDepth * oneoverxl; /* junction depth / effective length */
                    var djonxj = _mbp.LateralDiffusion * oneoverxj;
                    var wponxj = wps * oneoverxj;
                    var wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    var dwpdvb = _modeltemp.CoefficientDepletionLayerWidth * dsqdvb;
                    var dadvb = (coeff1 + coeff2 * (wponxj + wponxj)) * dwpdvb * oneoverxj;
                    var dbdvb = -argc * argc * (1.0 - argc) * dwpdvb / (argb * wps);
                    dfsdvb = -xjonxl * (dadvb * argb + arga * dbdvb);
                }
                else
                {
                    fshort = 1.0;
                    dfsdvb = 0.0;
                }
                /*
				 * .....body effect
				 */
                var gammas = _mbp.Gamma * fshort;
                var fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                var fbody = fbodys + _mbp.NarrowFactor / _bp.Width;
                var onfbdy = 1.0 / (1.0 + fbody);
                var dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                var qbonco = gammas * sqphbs + _mbp.NarrowFactor * phibs / _bp.Width;
                var dqbdvb = gammas * dsqdvb + _mbp.Gamma * dfsdvb * sqphbs - _mbp.NarrowFactor / _bp.Width;
                /*
				 * .....static feedback effect
				 */
                var vbix = _temp.TempVoltageBi * _mbp.MosfetType - eta * vds;
                /*
				 * .....threshold voltage
				 */
                var vth = vbix + qbonco;
                var dvtdvd = -eta;
                var dvtdvb = dqbdvb;
                /*
				 * .....joint weak inversion and strong inversion
				 */
                von = vth;
                if (_mbp.FastSurfaceStateDensity > 0.0)
                {
                    var csonco = Circuit.Charge * _mbp.FastSurfaceStateDensity * 1e4 /* (cm *  * 2 / m *  * 2) */  * effectiveLength * _bp.Width /
                                    oxideCap;
                    var cdonco = qbonco / (phibs + phibs);
                    xn = 1.0 + csonco + cdonco;
                    von = vth + vt * xn;
                    dxndvb = dqbdvb / (phibs + phibs) - qbonco * dsqdvb / (phibs * sqphbs);
                    dvodvd = dvtdvd;
                    dvodvb = dvtdvb + vt * dxndvb;
                }
                else
                {
                    /*
					 * .....cutoff region
					 */
                    if (vgs <= von)
                    {
                        cdrain = 0.0;
                        Transconductance = 0.0;
                        CondDs = 0.0;
                        TransconductanceBs = 0.0;
                        goto innerline1000;
                    }
                }
                /*
				 * .....device is on
				 */
                var vgsx = Math.Max(vgs, von);
                /*
				 * .....mobility modulation by gate voltage
				 */
                var onfg = 1.0 + _mbp.Theta * (vgsx - vth);
                var fgate = 1.0 / onfg;
                var us = _temp.TempSurfaceMobility * 1e-4 /*(m**2/cm**2)*/ * fgate;
                var dfgdvg = -_mbp.Theta * fgate * fgate;
                var dfgdvd = -dfgdvg * dvtdvd;
                var dfgdvb = -dfgdvg * dvtdvb;
                /*
				 * .....saturation voltage
				 */
                vdsat = (vgsx - vth) * onfbdy;
                if (_mbp.MaxDriftVelocity <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    var vdsc = effectiveLength * _mbp.MaxDriftVelocity / us;
                    onvdsc = 1.0 / vdsc;
                    arga = (vgsx - vth) * onfbdy;
                    argb = Math.Sqrt(arga * arga + vdsc * vdsc);
                    vdsat = arga + vdsc - argb;
                    var dvsdga = (1.0 - arga / argb) * onfbdy;
                    dvsdvg = dvsdga - (1.0 - vdsc / argb) * vdsc * dfgdvg * onfg;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - arga * dvsdga * dfbdvb;
                }
                /*
				 * .....current factors in linear region
				 */
                var vdsx = Math.Min(vds, vdsat);
                if (vdsx.Equals(0.0))
                    goto line900;
                var cdo = vgsx - vth - 0.5 * (1.0 + fbody) * vdsx;
                var dcodvb = -dvtdvb - 0.5 * dfbdvb * vdsx;
                /*
				 * .....normalized drain current
				 */
                var cdnorm = cdo * vdsx;
                Transconductance = vdsx;
                CondDs = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                TransconductanceBs = dcodvb * vdsx;
                /*
				 * .....drain current without velocity saturation effect
				 */
                var cd1 = beta * cdnorm;
                beta = beta * fgate;
                cdrain = beta * cdnorm;
                Transconductance = beta * Transconductance + dfgdvg * cd1;
                CondDs = beta * CondDs + dfgdvd * cd1;
                TransconductanceBs = beta * TransconductanceBs;
                /*
				 * .....velocity saturation factor
				 */
                if (_mbp.MaxDriftVelocity > 0.0)
                {
                    fdrain = 1.0 / (1.0 + vdsx * onvdsc);
                    var fd2 = fdrain * fdrain;
                    arga = fd2 * vdsx * onvdsc * onfg;
                    dfddvg = -dfgdvg * arga;
                    dfddvd = -dfgdvd * arga - fd2 * onvdsc;
                    dfddvb = -dfgdvb * arga;
                    /*
					 * .....drain current
					 */
                    Transconductance = fdrain * Transconductance + dfddvg * cdrain;
                    CondDs = fdrain * CondDs + dfddvd * cdrain;
                    TransconductanceBs = fdrain * TransconductanceBs + dfddvb * cdrain;
                    cdrain = fdrain * cdrain;
                }
                /*
				 * .....channel length modulation
				 */
                if (vds <= vdsat) goto line700;
                if (_mbp.MaxDriftVelocity <= 0.0) goto line510;
                if (_modeltemp.Alpha.Equals(0.0))
                    goto line700;
                var cdsat = cdrain;
                var gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                var gdoncd = gdsat / cdsat;
                var gdonfd = gdsat / (1.0 - fdrain);
                var gdonfg = gdsat * onfg;
                var dgdvg = gdoncd * Transconductance - gdonfd * dfddvg + gdonfg * dfgdvg;
                var dgdvd = gdoncd * CondDs - gdonfd * dfddvd + gdonfg * dfgdvd;
                var dgdvb = gdoncd * TransconductanceBs - gdonfd * dfddvb + gdonfg * dfgdvb;

                var emax = _mbp.Kappa * cdsat * oneoverxl / gdsat;
                var emoncd = emax / cdsat;
                var emongd = emax / gdsat;
                var demdvg = emoncd * Transconductance - emongd * dgdvg;
                var demdvd = emoncd * CondDs - emongd * dgdvd;
                var demdvb = emoncd * TransconductanceBs - emongd * dgdvb;

                arga = 0.5 * emax * _modeltemp.Alpha;
                argc = _mbp.Kappa * _modeltemp.Alpha;
                argb = Math.Sqrt(arga * arga + argc * (vds - vdsat));
                delxl = argb - arga;
                dldvd = argc / (argb + argb);
                var dldem = 0.5 * (arga / argb - 1.0) * _modeltemp.Alpha;
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
                line510:
                delxl = Math.Sqrt(_mbp.Kappa * (vds - vdsat) * _modeltemp.Alpha);
                dldvd = 0.5 * delxl / (vds - vdsat);
                ddldvg = 0.0;
                ddldvd = -dldvd;
                ddldvb = 0.0;
                /*
				 * .....punch through approximation
				 */
                line520:
                if (delxl > 0.5 * effectiveLength)
                {
                    delxl = effectiveLength - effectiveLength * effectiveLength / (4.0 * delxl);
                    arga = 4.0 * (effectiveLength - delxl) * (effectiveLength - delxl) / (effectiveLength * effectiveLength);
                    ddldvg = ddldvg * arga;
                    ddldvd = ddldvd * arga;
                    ddldvb = ddldvb * arga;
                    dldvd = dldvd * arga;
                }
                /*
				 * .....saturation region
				 */
                var dlonxl = delxl * oneoverxl;
                var xlfact = 1.0 / (1.0 - dlonxl);
                cdrain = cdrain * xlfact;
                var diddl = cdrain / (effectiveLength - delxl);
                Transconductance = Transconductance * xlfact + diddl * ddldvg;
                gds0 = CondDs * xlfact + diddl * ddldvd;
                TransconductanceBs = TransconductanceBs * xlfact + diddl * ddldvb;
                Transconductance = Transconductance + gds0 * dvsdvg;
                TransconductanceBs = TransconductanceBs + gds0 * dvsdvb;
                CondDs = gds0 * dvsdvd + diddl * dldvd;
                /*
				 * .....finish strong inversion case
				 */
                line700:
                if (vgs < von)
                {
                    /*
					 * .....weak inversion
					 */
                    var onxn = 1.0 / xn;
                    var ondvt = onxn / vt;
                    var wfact = Math.Exp((vgs - von) * ondvt);
                    cdrain = cdrain * wfact;
                    var gms = Transconductance * wfact;
                    var gmw = cdrain * ondvt;
                    Transconductance = gmw;
                    if (vds > vdsat)
                    {
                        Transconductance = Transconductance + gds0 * dvsdvg * wfact;
                    }
                    CondDs = CondDs * wfact + (gms - gmw) * dvodvd;
                    TransconductanceBs = TransconductanceBs * wfact + (gms - gmw) * dvodvb - gmw * (vgs - von) * onxn * dxndvb;
                }
                /*
				 * .....charge computation
				 */
                goto innerline1000;
                /*
				 * .....special case of vds = 0.0d0 */
                line900:
                beta = beta * fgate;
                cdrain = 0.0;
                Transconductance = 0.0;
                CondDs = beta * (vgsx - vth);
                TransconductanceBs = 0.0;
                if (_mbp.FastSurfaceStateDensity > 0.0 && vgs < von)
                {
                    CondDs *= Math.Exp((vgs - von) / (vt * xn));
                }
                innerline1000:;
                /*
				 * .....done
				 */
            }

            Von = _mbp.MosfetType * von;
            SaturationVoltageDs = _mbp.MosfetType * vdsat;
            return cdrain;
        }
    }
}
