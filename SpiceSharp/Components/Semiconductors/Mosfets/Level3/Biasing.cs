using SpiceSharp.Algebra;
using SpiceSharp.Attributes;
using SpiceSharp.Behaviors;
using SpiceSharp.Components.Semiconductors;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharp.Components.Mosfets.Level3
{
    /// <summary>
    /// Biasing behavior for a <see cref="Mosfet3" />.
    /// </summary>
    /// <seealso cref="Temperature"/>
    /// <seealso cref="IBiasingBehavior"/>
    /// <seealso cref="IConvergenceBehavior"/>
    [BehaviorFor(typeof(Mosfet3)), AddBehaviorIfNo(typeof(IBiasingBehavior))]
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

        /// <summary>
        /// The permittivity of silicon
        /// </summary>
        protected const double EpsilonSilicon = 11.7 * 8.854214871e-12;

        /// <summary>
        /// The maximum exponent argument.
        /// </summary>
        protected const double MaximumExponentArgument = 709.0;

        /// <inheritdoc/>
        Mosfets.TemperatureProperties IMosfetBiasingBehavior.Properties => Properties;

        /// <inheritdoc/>
        public event EventHandler<MosfetContributionEventArgs> UpdateContributions;

        /// <inheritdoc/>
        [ParameterName("id"), ParameterName("cd"), ParameterInfo("Drain current")]
        public double Id { get; private set; }

        /// <inheritdoc/>
        [ParameterName("ibs"), ParameterInfo("B-S junction current")]
        public double Ibs { get; private set; }

        /// <inheritdoc/>
        [ParameterName("ibd"), ParameterInfo("B-D junction current")]
        public double Ibd { get; private set; }

        /// <inheritdoc/>
        [ParameterName("gm"), ParameterInfo("Transconductance")]
        public double Gm { get; private set; }

        /// <inheritdoc/>
        [ParameterName("gmb"), ParameterName("gmbs"), ParameterInfo("Bulk-Source transconductance")]
        public double Gmbs { get; private set; }

        /// <inheritdoc/>
        [ParameterName("gds"), ParameterInfo("Drain-Source conductance")]
        public double Gds { get; private set; }

        /// <inheritdoc/>
        [ParameterName("gbs"), ParameterInfo("Bulk-Source conductance")]
        public double Gbs { get; private set; }

        /// <inheritdoc/>
        [ParameterName("gbd"), ParameterInfo("Bulk-Drain conductance")]
        public double Gbd { get; private set; }

        /// <inheritdoc/>
        [ParameterName("von"), ParameterInfo("Turn-on voltage")]
        public double Von { get; private set; }

        /// <inheritdoc/>
        [ParameterName("vdsat"), ParameterInfo("Saturation drain-source voltage")]
        public double Vdsat { get; private set; }

        /// <inheritdoc/>
        public double Mode { get; private set; } = 1;

        /// <inheritdoc/>
        [ParameterName("vgs"), ParameterInfo("Gate-Source voltage")]
        public double Vgs { get; private set; }

        /// <inheritdoc/>
        [ParameterName("vds"), ParameterInfo("Drain-Source voltage")]
        public double Vds { get; private set; }

        /// <inheritdoc/>
        [ParameterName("vbs"), ParameterInfo("Bulk-Source voltage")]
        public double Vbs { get; private set; }

        /// <inheritdoc/>
        [ParameterName("vbd"), ParameterInfo("Bulk-Drain voltage")]
        public double Vbd { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Biasing"/> class.
        /// </summary>
        /// <param name="context">The binding context.</param>
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

            /* first, we compute a few useful values - these could be
             * pre-computed, but for historical reasons are still done
             * here.  They may be moved at the expense of instance
             * size */

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
            double Beta = Properties.TempTransconductance * Parameters.ParallelMultiplier * Properties.EffectiveWidth / Properties.EffectiveLength;

            // Get the current voltages
            Initialize(out double vgs, out double vds, out double vbs, out bool check);
            double vbd = vbs - vds;
            double vgd = vgs - vds;

            if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
            {
                if (vbs <= -3 * Properties.TempVt)
                {
                    double arg = 3 * Properties.TempVt / (vbs * Math.E);
                    arg = arg * arg * arg;
                    con.Bs.C = -SourceSatCur * (1 + arg) + _config.Gmin * vbs;
                    con.Bs.G = SourceSatCur * 3 * arg / vbs + _config.Gmin;
                }
                else
                {
                    double evbs = Math.Exp(Math.Min(MaximumExponentArgument, vbs / Properties.TempVt));
                    con.Bs.G = SourceSatCur * evbs / Properties.TempVt + _config.Gmin;
                    con.Bs.C = SourceSatCur * (evbs - 1) + _config.Gmin * vbs;
                }
                if (vbd <= -3 * Properties.TempVt)
                {
                    double arg = 3 * Properties.TempVt / (vbd * Math.E);
                    arg = arg * arg * arg;
                    con.Bd.C = -DrainSatCur * (1 + arg) + _config.Gmin * vbd;
                    con.Bd.G = DrainSatCur * 3 * arg / vbd + _config.Gmin;
                }
                else
                {
                    double evbd = Math.Exp(Math.Min(MaximumExponentArgument, vbd / Properties.TempVt));
                    con.Bd.G = DrainSatCur * evbd / Properties.TempVt + _config.Gmin;
                    con.Bd.C = DrainSatCur * (evbd - 1) + _config.Gmin * vbd;
                }
            }
            else
            {
                if (vbs <= 0)
                {
                    con.Bs.G = SourceSatCur / Properties.TempVt;
                    con.Bs.C = con.Bs.G * vbs;
                    con.Bs.G += _config.Gmin;
                }
                else
                {
                    double evbs = Math.Exp(Math.Min(MaximumExponentArgument, vbs / Properties.TempVt));
                    con.Bs.G = SourceSatCur * evbs / Properties.TempVt + _config.Gmin;
                    con.Bs.C = SourceSatCur * (evbs - 1);
                }
                if (vbd <= 0)
                {
                    con.Bd.G = DrainSatCur / Properties.TempVt;
                    con.Bd.C = con.Bd.G * vbd;
                    con.Bd.G += _config.Gmin;
                }
                else
                {
                    double evbd = Math.Exp(Math.Min(MaximumExponentArgument, vbd / Properties.TempVt));
                    con.Bd.G = DrainSatCur * evbd / Properties.TempVt + _config.Gmin;
                    con.Bd.C = DrainSatCur * (evbd - 1);
                }
            }

            // Now to determine whether the user was able to correctly identify the source and drain of his device.
            if (vds >= 0)
                Mode = 1;
            else
                Mode = -1;

            {
                /*
				 * subroutine moseq3(vds,vbs,vgs,gm,gds,gmbs,
				 * qg,qc,qb,cggb,cgdb,cgsb,cbgb,cbdb,cbsb)
				 */

                /* this routine evaluates the drain current, its
				 * derivatives and the Constants.Charges associated with the
				 * gate, channel and bulk for mosfets based on
				 * semi-empirical equations */

                double coeff0 = 0.0631353e0;
                double coeff1 = 0.8013292e0;
                double coeff2 = -0.01110777e0;
                double oneoverxl;   /* 1/effective length */
                double eta; /* eta from model after length factor */
                double phibs;   /* phi - vbs */
                double sqphbs;  /* square root of phibs */
                double dsqdvb;  /*  */
                double sqphis;  /* square root of phi */
                double sqphs3;  /* square root of phi cubed */
                double wps;
                double oneoverxj;   /* 1/junction depth */
                double xjonxl;  /* junction depth/effective length */
                double djonxj;
                double wponxj;
                double arga;
                double argb;
                double argc;
                double dwpdvb;
                double dadvb;
                double dbdvb;
                double gammas;
                double fbodys;
                double fbody;
                double onfbdy;
                double qbonco;
                double vbix;
                double wconxj;
                double dfsdvb;
                double dfbdvb;
                double dqbdvb;
                double vth;
                double dvtdvb;
                double csonco;
                double cdonco;
                double dxndvb = 0.0;
                double dvodvb = 0.0;
                double dvodvd = 0.0;
                double vgsx;
                double dvtdvd;
                double onfg;
                double fgate;
                double us;
                double dfgdvg;
                double dfgdvd;
                double dfgdvb;
                double dvsdvg;
                double dvsdvb;
                double dvsdvd;
                double xn = 0.0;
                double vdsc;
                double onvdsc = 0.0;
                double dvsdga;
                double vdsx;
                double dcodvb;
                double cdnorm;
                double cdo;
                double cd1;
                double fdrain = 0.0;
                double fd2;
                double dfddvg = 0.0;
                double dfddvb = 0.0;
                double dfddvd = 0.0;
                double gdsat;
                double cdsat;
                double gdoncd;
                double gdonfd;
                double gdonfg;
                double dgdvg;
                double dgdvd;
                double dgdvb;
                double emax;
                double emongd;
                double demdvg;
                double demdvd;
                double demdvb;
                double delxl;
                double dldvd;
                double dldem;
                double ddldvg;
                double ddldvd;
                double ddldvb;
                double dlonxl;
                double xlfact;
                double diddl;
                double gds0 = 0.0;
                double emoncd;
                double ondvt;
                double onxn;
                double wfact;
                double gms;
                double gmw;
                double fshort;

                /*
				 *     reference con.Ds.C equations to source and
				 *     charge equations to bulk
				 */
                double vdsat = 0.0;
                oneoverxl = 1.0 / Properties.EffectiveLength;
                eta = ModelParameters.Eta * 8.15e-22 / (ModelTemperature.Properties.OxideCapFactor * Properties.EffectiveLength * Properties.EffectiveLength * Properties.EffectiveLength);

                // Square root term
                if ((Mode > 0 ? vbs : vbd) <= 0.0)
                {
                    phibs = Properties.TempPhi - (Mode > 0 ? vbs : vbd);
                    sqphbs = Math.Sqrt(phibs);
                    dsqdvb = -0.5 / sqphbs;
                }
                else
                {
                    sqphis = Math.Sqrt(Properties.TempPhi);
                    sqphs3 = Properties.TempPhi * sqphis;
                    sqphbs = sqphis / (1.0 + (Mode > 0 ? vbs : vbd) /
                             (Properties.TempPhi + Properties.TempPhi));
                    phibs = sqphbs * sqphbs;
                    dsqdvb = -phibs / (sqphs3 + sqphs3);
                }

                // Short channel effect factor
                if ((ModelParameters.JunctionDepth != 0.0) && (ModelTemperature.Properties.CoeffDepLayWidth != 0.0))
                {
                    wps = ModelTemperature.Properties.CoeffDepLayWidth * sqphbs;
                    oneoverxj = 1.0 / ModelParameters.JunctionDepth;
                    xjonxl = ModelParameters.JunctionDepth * oneoverxl;
                    djonxj = ModelParameters.LateralDiffusion * oneoverxj;
                    wponxj = wps * oneoverxj;
                    wconxj = coeff0 + coeff1 * wponxj + coeff2 * wponxj * wponxj;
                    arga = wconxj + djonxj;
                    argc = wponxj / (1.0 + wponxj);
                    argb = Math.Sqrt(1.0 - argc * argc);
                    fshort = 1.0 - xjonxl * (arga * argb - djonxj);
                    dwpdvb = ModelTemperature.Properties.CoeffDepLayWidth * dsqdvb;
                    dadvb = (coeff1 + coeff2 * (wponxj + wponxj)) * dwpdvb * oneoverxj;
                    dbdvb = -argc * argc * (1.0 - argc) * dwpdvb / (argb * wps);
                    dfsdvb = -xjonxl * (dadvb * argb + arga * dbdvb);
                }
                else
                {
                    fshort = 1.0;
                    dfsdvb = 0.0;
                }

                // Body effect
                gammas = ModelParameters.Gamma * fshort;
                fbodys = 0.5 * gammas / (sqphbs + sqphbs);
                fbody = fbodys + ModelTemperature.Properties.NarrowFactor / Properties.EffectiveWidth;
                onfbdy = 1.0 / (1.0 + fbody);
                dfbdvb = -fbodys * dsqdvb / sqphbs + fbodys * dfsdvb / fshort;
                qbonco = gammas * sqphbs + ModelTemperature.Properties.NarrowFactor * phibs / Properties.EffectiveWidth;
                dqbdvb = gammas * dsqdvb + ModelParameters.Gamma * dfsdvb * sqphbs - ModelTemperature.Properties.NarrowFactor / Properties.EffectiveWidth;

                // Static feedback effect
                vbix = Properties.TempVbi * ModelParameters.MosfetType - eta * (Mode * vds);

                // Threshold voltage
                vth = vbix + qbonco;
                dvtdvd = -eta;
                dvtdvb = dqbdvb;

                // Joint weak inversion and strong inversion
                double von = vth;
                if (ModelParameters.FastSurfaceStateDensity != 0.0)
                {
                    csonco = Constants.Charge * ModelParameters.FastSurfaceStateDensity *
                    1e4 /*(cm**2/m**2)*/ * Properties.EffectiveLength * Properties.EffectiveWidth *
                    Parameters.ParallelMultiplier / Properties.OxideCap;
                    cdonco = qbonco / (phibs + phibs);
                    xn = 1.0 + csonco + cdonco;
                    von = vth + Properties.TempVt * xn;
                    dxndvb = dqbdvb / (phibs + phibs) - qbonco * dsqdvb / (phibs * sqphbs);
                    dvodvd = dvtdvd;
                    dvodvb = dvtdvb + Properties.TempVt * dxndvb;
                }
                else
                {
                    // Cutoff region
                    if ((Mode > 0 ? vgs : vgd) <= von)
                    {
                        con.Ds.C = 0.0;
                        Gm = 0.0;
                        con.Ds.G = 0.0;
                        Gmbs = 0.0;
                        goto innerline1000;
                    }
                }

                // Device is on
                vgsx = Math.Max(Mode > 0 ? vgs : vgd, von);

                // Mobility modulation by gate voltage
                onfg = 1.0 + ModelParameters.Theta * (vgsx - vth);
                fgate = 1.0 / onfg;
                us = Properties.TempSurfaceMobility * 1e-4 /*(m**2/cm**2)*/ * fgate;
                dfgdvg = -ModelParameters.Theta * fgate * fgate;
                dfgdvd = -dfgdvg * dvtdvd;
                dfgdvb = -dfgdvg * dvtdvb;

                // Saturation voltage
                vdsat = (vgsx - vth) * onfbdy;
                if (ModelParameters.MaxDriftVelocity <= 0.0)
                {
                    dvsdvg = onfbdy;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - vdsat * dfbdvb * onfbdy;
                }
                else
                {
                    vdsc = Properties.EffectiveLength * ModelParameters.MaxDriftVelocity / us;
                    onvdsc = 1.0 / vdsc;
                    arga = (vgsx - vth) * onfbdy;
                    argb = Math.Sqrt(arga * arga + vdsc * vdsc);
                    vdsat = arga + vdsc - argb;
                    dvsdga = (1.0 - arga / argb) * onfbdy;
                    dvsdvg = dvsdga - (1.0 - vdsc / argb) * vdsc * dfgdvg * onfg;
                    dvsdvd = -dvsdvg * dvtdvd;
                    dvsdvb = -dvsdvg * dvtdvb - arga * dvsdga * dfbdvb;
                }

                // Current factors in linear region
                vdsx = Math.Min(Mode * vds, vdsat);
                if (vdsx == 0.0)
                    goto line900;
                cdo = vgsx - vth - 0.5 * (1.0 + fbody) * vdsx;
                dcodvb = -dvtdvb - 0.5 * dfbdvb * vdsx;

                // Normalized drain current
                cdnorm = cdo * vdsx;
                Gm = vdsx;

                if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                {
                    if ((Mode * vds) > vdsat)
                        con.Ds.G = -dvtdvd * vdsx;
                    else
                        con.Ds.G = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                }
                else
                    con.Ds.G = vgsx - vth - (1.0 + fbody + dvtdvd) * vdsx;
                Gmbs = dcodvb * vdsx;

                // Drain current without velocity saturation effect
                cd1 = Beta * cdnorm;
                Beta *= fgate;
                con.Ds.C = Beta * cdnorm;
                Gm = Beta * Gm + dfgdvg * cd1;
                con.Ds.G = Beta * con.Ds.G + dfgdvd * cd1;
                Gmbs *= Beta;
                if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                    Gmbs += dfgdvb * cd1;

                // Celocity saturation factor
                if (ModelParameters.MaxDriftVelocity > 0.0)
                {
                    fdrain = 1.0 / (1.0 + vdsx * onvdsc);
                    fd2 = fdrain * fdrain;
                    arga = fd2 * vdsx * onvdsc * onfg;
                    dfddvg = -dfgdvg * arga;

                    if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                    {
                        if ((Mode * vds) > vdsat)
                            dfddvd = -dfgdvd * arga;
                        else
                            dfddvd = -dfgdvd * arga - fd2 * onvdsc;
                    }
                    else
                        dfddvd = -dfgdvd * arga - fd2 * onvdsc;
                    dfddvb = -dfgdvb * arga;

                    // Drain current
                    Gm = fdrain * Gm + dfddvg * con.Ds.C;
                    con.Ds.G = fdrain * con.Ds.G + dfddvd * con.Ds.C;
                    Gmbs = fdrain * Gmbs + dfddvb * con.Ds.C;
                    con.Ds.C = fdrain * con.Ds.C;
                    Beta *= fdrain;
                }

                // Channel length modulation
                if ((Mode * vds) <= vdsat)
                {
                    if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                    {
                        if ((ModelParameters.MaxDriftVelocity > 0.0) || (ModelTemperature.Properties.Alpha == 0.0) || ModelParameters.BadMos)
                            goto line700;
                        else
                        {
                            arga = Mode * vds / vdsat;
                            delxl = Math.Sqrt(ModelParameters.Kappa * ModelTemperature.Properties.Alpha * vdsat / 8);
                            dldvd = 4 * delxl * arga * arga * arga / vdsat;
                            arga *= arga;
                            arga *= arga;
                            delxl *= arga;
                            ddldvg = 0.0;
                            ddldvd = -dldvd;
                            ddldvb = 0.0;
                            goto line520;
                        }
                    }
                    else
                        goto line700;
                }

                if (ModelParameters.MaxDriftVelocity <= 0.0)
                    goto line510;
                if (ModelTemperature.Properties.Alpha == 0.0)
                    goto line700;
                cdsat = con.Ds.C;
                gdsat = cdsat * (1.0 - fdrain) * onvdsc;
                gdsat = Math.Max(1.0e-12, gdsat);
                gdoncd = gdsat / cdsat;
                gdonfd = gdsat / (1.0 - fdrain);
                gdonfg = gdsat * onfg;
                dgdvg = gdoncd * Gm - gdonfd * dfddvg + gdonfg * dfgdvg;
                dgdvd = gdoncd * con.Ds.G - gdonfd * dfddvd + gdonfg * dfgdvd;
                dgdvb = gdoncd * Gmbs - gdonfd * dfddvb + gdonfg * dfgdvb;

                if (ModelParameters.BadMos)
                    emax = cdsat * oneoverxl / gdsat;
                else
                    emax = ModelParameters.Kappa * cdsat * oneoverxl / gdsat;
                emoncd = emax / cdsat;
                emongd = emax / gdsat;
                demdvg = emoncd * Gm - emongd * dgdvg;
                demdvd = emoncd * con.Ds.G - emongd * dgdvd;
                demdvb = emoncd * Gmbs - emongd * dgdvb;

                arga = 0.5 * emax * ModelTemperature.Properties.Alpha;
                argc = ModelParameters.Kappa * ModelTemperature.Properties.Alpha;
                argb = Math.Sqrt(arga * arga + argc * ((Mode * vds) - vdsat));
                delxl = argb - arga;

                if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                {
                    if (argb != 0.0)
                    {
                        dldvd = argc / (argb + argb);
                        dldem = 0.5 * (arga / argb - 1.0) * ModelTemperature.Properties.Alpha;
                    }
                    else
                    {
                        dldvd = 0.0;
                        dldem = 0.0;
                    }
                }
                else
                {
                    dldvd = argc / (argb + argb);
                    dldem = 0.5 * (arga / argb - 1.0) * ModelTemperature.Properties.Alpha;
                }
                ddldvg = dldem * demdvg;
                ddldvd = dldem * demdvd - dldvd;
                ddldvb = dldem * demdvb;
                goto line520;
            line510:
                if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                {
                    if (ModelParameters.BadMos)
                    {
                        delxl = Math.Sqrt(ModelParameters.Kappa * ((Mode * vds) - vdsat) * ModelTemperature.Properties.Alpha);
                        dldvd = 0.5 * delxl / ((Mode * vds) - vdsat);
                    }
                    else
                    {
                        delxl = Math.Sqrt(ModelParameters.Kappa * ModelTemperature.Properties.Alpha * ((Mode * vds) - vdsat + (vdsat / 8)));
                        dldvd = 0.5 * delxl / ((Mode * vds) - vdsat + (vdsat / 8));
                    }
                }
                else
                {
                    delxl = Math.Sqrt(ModelParameters.Kappa * ((Mode * vds) - vdsat) * ModelTemperature.Properties.Alpha);
                    dldvd = 0.5 * delxl / ((Mode * vds) - vdsat);
                }
                ddldvg = 0.0;
                ddldvd = -dldvd;
                ddldvb = 0.0;

            // Punch through approximation
            line520:
                if (delxl > (0.5 * Properties.EffectiveLength))
                {
                    delxl = Properties.EffectiveLength - (Properties.EffectiveLength * Properties.EffectiveLength / (4.0 * delxl));
                    arga = 4.0 * (Properties.EffectiveLength - delxl) * (Properties.EffectiveLength - delxl) / (Properties.EffectiveLength * Properties.EffectiveLength);
                    ddldvg *= arga;
                    ddldvd *= arga;
                    ddldvb *= arga;
                    dldvd *= arga;
                }

                // Saturation region
                dlonxl = delxl * oneoverxl;
                xlfact = 1.0 / (1.0 - dlonxl);

                if (ModelParameters.Version == ModelParameters.Versions.NgSpice)
                {
                    con.Ds.C *= xlfact;
                    diddl = con.Ds.C / (Properties.EffectiveLength - delxl);
                    Gm = Gm * xlfact + diddl * ddldvg;
                    Gmbs = Gmbs * xlfact + diddl * ddldvb;
                    gds0 = diddl * ddldvd;
                    Gm += gds0 * dvsdvg;
                    Gmbs += gds0 * dvsdvb;
                    con.Ds.G = con.Ds.G * xlfact + diddl * dldvd + gds0 * dvsdvd;
                    /*              con.Ds.G = (con.Ds.G*xlfact)+gds0*dvsdvd-
								   (cd1*ddldvd/(EffectiveLength*(1-2*dlonxl+dlonxl*dlonxl)));*/
                }
                else
                {
                    con.Ds.C *= xlfact;
                    diddl = con.Ds.C / (Properties.EffectiveLength - delxl);
                    Gm = Gm * xlfact + diddl * ddldvg;
                    gds0 = con.Ds.G * xlfact + diddl * ddldvd;
                    Gmbs = Gmbs * xlfact + diddl * ddldvb;
                    Gm += gds0 * dvsdvg;
                    Gmbs += gds0 * dvsdvb;
                    con.Ds.G = gds0 * dvsdvd + diddl * dldvd;
                }

            // Finish strong inversion case
            line700:
                if ((Mode > 0 ? vgs : vgd) < von)
                {
                    // Weak inversion
                    onxn = 1.0 / xn;
                    ondvt = onxn / Properties.TempVt;
                    wfact = Math.Exp(((Mode > 0 ? vgs : vgd) - von) * ondvt);
                    con.Ds.C *= wfact;
                    gms = Gm * wfact;
                    gmw = con.Ds.C * ondvt;
                    Gm = gmw;
                    if ((Mode * vds) > vdsat)
                    {
                        Gm += gds0 * dvsdvg * wfact;
                    }
                    con.Ds.G = con.Ds.G * wfact + (gms - gmw) * dvodvd;
                    Gmbs = Gmbs * wfact + (gms - gmw) * dvodvb - gmw * ((Mode > 0 ? vgs : vgd) - von) * onxn * dxndvb;
                }

                // Charge computation
                goto innerline1000;

            // Special case of vds = 0.0d0
            line900:
                Beta *= fgate;
                con.Ds.C = 0.0;
                Gm = 0.0;
                con.Ds.G = Beta * (vgsx - vth);
                Gmbs = 0.0;
                if ((ModelParameters.FastSurfaceStateDensity != 0.0) &&
                           ((Mode > 0 ? vgs : vgd) < von))
                {
                    con.Ds.G *= Math.Exp(((Mode > 0 ? vgs : vgd) - von) / (Properties.TempVt * xn));
                }
            innerline1000:;
                // Done
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
            if (!Parameters.Off || _iteration.Mode != IterationModes.Fix)
            {
                if (check)
                    _iteration.IsConvergent = false;
            }

            // Save things away for next time
            Gbd = con.Bd.G;
            Ibd = con.Bd.C;
            Gbs = con.Bs.G;
            Ibs = con.Bs.C;

            // Right hand side contributions
            double xnrm, xrev;
            con.Bs.C = ModelParameters.MosfetType * (con.Bs.C - (con.Bs.G - _config.Gmin) * vbs);
            con.Bd.C = ModelParameters.MosfetType * (con.Bd.C - (con.Bd.G - _config.Gmin) * vbd);
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
