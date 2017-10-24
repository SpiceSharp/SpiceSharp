using System;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behavior for a <see cref="BSIM4v80"/>
    /// </summary>
    public class BSIM4v80NoiseBehavior : CircuitObjectBehaviorNoise
    {
        /// <summary>
        /// Nodes as used by BSIM4noise
        /// </summary>
        private const int BSIM4dNode = 0;
        private const int BSIM4gNodeExt = 1;
        private const int BSIM4sNode = 2;
        private const int BSIM4bNode = 3;
        private const int BSIM4dNodePrime = 4;
        private const int BSIM4gNodeMid = 5;
        private const int BSIM4gNodePrime = 6;
        private const int BSIM4sNodePrime = 7;
        private const int BSIM4bNodePrime = 8;
        private const int BSIM4sbNode = 9;
        private const int BSIM4dbNode = 10;

        /// <summary>
        /// Identifiers by their index in the noise generator list
        /// </summary>
        private const int BSIM4RDNOIZ = 0;
        private const int BSIM4RSNOIZ = 1;
        private const int BSIM4RGNOIZ = 2;
        private const int BSIM4RBPSNOIZ = 3;
        private const int BSIM4RBPDNOIZ = 4;
        private const int BSIM4RBPBNOIZ = 5;
        private const int BSIM4RBSBNOIZ = 6;
        private const int BSIM4RBDBNOIZ = 7;
        private const int BSIM4IDNOIZ = 8;
        private const int BSIM4FLNOIZ = 9;
        private const int BSIM4IGSNOIZ = 10;
        private const int BSIM4IGDNOIZ = 11;
        private const int BSIM4IGBNOIZ = 12;
        private const int BSIM4CORLNOIZ = 13;


        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise BSIM4noise { get; } = new ComponentNoise(
            new Noise.NoiseThermal("rd", BSIM4dNodePrime, BSIM4dNode), // Noise due to rd
            new Noise.NoiseThermal("rs", BSIM4sNodePrime, BSIM4sNode), // Noise due to rs
            new Noise.NoiseThermal("rg", BSIM4gNodePrime, BSIM4gNodeExt), // Noise due to rgeltd
            new Noise.NoiseThermal("rbps", BSIM4bNodePrime, BSIM4sbNode), // Noise due to rbps
            new Noise.NoiseThermal("rbpd", BSIM4bNodePrime, BSIM4dbNode), // Noise due to rbpd
            new Noise.NoiseThermal("rbpb", BSIM4bNodePrime, BSIM4bNode), // Noise due to rbpb
            new Noise.NoiseThermal("rbsb", BSIM4bNode, BSIM4sbNode), // Noise due to rbsb
            new Noise.NoiseThermal("rbdb", BSIM4bNode, BSIM4dbNode), // Noise due to rbdb
            new Noise.NoiseShot("id", BSIM4dNodePrime, BSIM4sNodePrime), // Noise due to id (for tnoiMod2: uncorrelated portion only)
            new Noise.NoiseGain("1overf", BSIM4dNodePrime, BSIM4sNodePrime), // Flicker (1/f) noise
            new Noise.NoiseShot("igs", BSIM4gNodePrime, BSIM4sNodePrime), // Shot noise due to igs
            new Noise.NoiseShot("igd", BSIM4gNodePrime, BSIM4dNodePrime), // Shot noise due to igd
            new Noise.NoiseShot("igb", BSIM4gNodePrime, BSIM4bNodePrime), // Shot noise due to igb
            new Noise.NoiseThermal2("corl", BSIM4dNodePrime, BSIM4sNodePrime, BSIM4gNodePrime, BSIM4sNodePrime) // Contribution of correlated drain and induced gate noise
            );

        /// <summary>
        /// Setup the behavior
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var bsim4 = ComponentTyped<BSIM4v80>();

            BSIM4noise.Setup(ckt,
                bsim4.BSIM4dNode,
                bsim4.BSIM4gNodeExt,
                bsim4.BSIM4sNode,
                bsim4.BSIM4bNode,
                bsim4.BSIM4dNodePrime,
                bsim4.BSIM4gNodeMid,
                bsim4.BSIM4gNodePrime,
                bsim4.BSIM4sNodePrime,
                bsim4.BSIM4bNodePrime,
                bsim4.BSIM4sbNode,
                bsim4.BSIM4dbNode
                );
        }

        /// <summary>
        /// Execute the behavior
        /// </summary>
        /// <param name="ckt">Circuit</param>
        public override void Execute(Circuit ckt)
        {
            var here = ComponentTyped<BSIM4v80>();
            var model = here.Model as BSIM4v80Model;
            var state = ckt.State;
            var noise = state.Noise;

            double T0, T1, T2, T3, T4, T5, T6, T7, T8, T10, T11;
            double Vds, Ssi, Swi;
            double tmp = 0.0, gdpr, gspr, npart_theta = 0.0, npart_beta = 0.0, igsquare, bodymode;

            /* tnoiMod=2 (v4.7) */
            double eta, Leff, Lvsat, gamma, delta, epsilon, GammaGd0 = 0.0;
            double npart_c, sigrat = 0.0, C0, omega, ctnoi = 0.0;

            if (model.BSIM4tnoiMod == 0)
            {
                if (model.BSIM4rdsMod == 0)
                {
                    gspr = here.BSIM4sourceConductance;
                    gdpr = here.BSIM4drainConductance;
                    if (here.BSIM4grdsw > 0.0)
                        tmp = 1.0 / here.BSIM4grdsw; /* tmp used below */
                    else
                        tmp = 0.0;
                }
                else
                {
                    gspr = here.BSIM4gstot;
                    gdpr = here.BSIM4gdtot;
                    tmp = 0.0;
                }
            }
            else if (model.BSIM4tnoiMod == 1)
            {
                T5 = here.BSIM4Vgsteff / here.BSIM4EsatL;
                T5 *= T5;
                npart_beta = model.BSIM4rnoia * (1.0 + T5
                       * model.BSIM4tnoia * here.pParam.BSIM4leff);
                npart_theta = model.BSIM4rnoib * (1.0 + T5
                                            * model.BSIM4tnoib * here.pParam.BSIM4leff);
                if (npart_theta > 0.9)
                    npart_theta = 0.9;
                if (npart_theta > 0.9 * npart_beta)
                    npart_theta = 0.9 * npart_beta; //4.6.2

                if (model.BSIM4rdsMod == 0)
                {
                    gspr = here.BSIM4sourceConductance;
                    gdpr = here.BSIM4drainConductance;
                }
                else
                {
                    gspr = here.BSIM4gstot;
                    gdpr = here.BSIM4gdtot;
                }

                if (state.States[0][here.BSIM4states + BSIM4v80.BSIM4vds] >= 0.0)
                    gspr = gspr * (1.0 + npart_theta * npart_theta * gspr / here.BSIM4IdovVds);
                else
                    gdpr = gdpr * (1.0 + npart_theta * npart_theta * gdpr / here.BSIM4IdovVds);
            }
            else
            {   /* tnoiMod=2 (v4.7) */

                if (model.BSIM4rdsMod == 0)
                {
                    gspr = here.BSIM4sourceConductance;
                    gdpr = here.BSIM4drainConductance;
                }
                else
                {
                    gspr = here.BSIM4gstot;
                    gdpr = here.BSIM4gdtot;
                }

            }

            BSIM4noise.Generators[BSIM4RDNOIZ].Set(gdpr);
            BSIM4noise.Generators[BSIM4RSNOIZ].Set(gspr);

            if (here.BSIM4rgateMod == 1)
                BSIM4noise.Generators[BSIM4RGNOIZ].Set(here.BSIM4grgeltd);
            else if (here.BSIM4rgateMod == 2)
            {
                T0 = 1.0 + here.BSIM4grgeltd / here.BSIM4gcrg;
                T1 = T0 * T0;
                BSIM4noise.Generators[BSIM4RGNOIZ].Set(here.BSIM4grgeltd / T1);
            }
            else if (here.BSIM4rgateMod == 3)
                BSIM4noise.Generators[BSIM4RGNOIZ].Set(here.BSIM4grgeltd);
            else
                BSIM4noise.Generators[BSIM4RGNOIZ].Set(0.0);

            bodymode = 5;
            if (here.BSIM4rbodyMod == 2)
            {
                if ((!model.BSIM4rbps0.Given) || (!model.BSIM4rbpd0.Given))
                    bodymode = 1;
                else if ((!model.BSIM4rbsbx0.Given && !model.BSIM4rbsby0.Given) || (!model.BSIM4rbdbx0.Given && !model.BSIM4rbdby0.Given))
                    bodymode = 3;
            }

            if (here.BSIM4rbodyMod.Value != 0.0)
            {
                if (bodymode == 5)
                {
                    BSIM4noise.Generators[BSIM4RBPSNOIZ].Set(here.BSIM4grbps);
                    BSIM4noise.Generators[BSIM4RBPDNOIZ].Set(here.BSIM4grbpd);
                    BSIM4noise.Generators[BSIM4RBPBNOIZ].Set(here.BSIM4grbpb);
                    BSIM4noise.Generators[BSIM4RBSBNOIZ].Set(here.BSIM4grbsb);
                    BSIM4noise.Generators[BSIM4RBDBNOIZ].Set(here.BSIM4grbdb);
                }
                if (bodymode == 3)
                {
                    BSIM4noise.Generators[BSIM4RBPSNOIZ].Set(here.BSIM4grbps);
                    BSIM4noise.Generators[BSIM4RBPDNOIZ].Set(here.BSIM4grbpd);
                    BSIM4noise.Generators[BSIM4RBPBNOIZ].Set(here.BSIM4grbpb);
                    BSIM4noise.Generators[BSIM4RBSBNOIZ].Set(0.0);
                    BSIM4noise.Generators[BSIM4RBDBNOIZ].Set(0.0);
                }
                if (bodymode == 1)
                {
                    BSIM4noise.Generators[BSIM4RBPBNOIZ].Set(here.BSIM4grbpb);
                    BSIM4noise.Generators[BSIM4RBPSNOIZ].Set(0.0);
                    BSIM4noise.Generators[BSIM4RBPDNOIZ].Set(0.0);
                    BSIM4noise.Generators[BSIM4RBSBNOIZ].Set(0.0);
                    BSIM4noise.Generators[BSIM4RBDBNOIZ].Set(0.0);
                }
            }
            else
            {
                BSIM4noise.Generators[BSIM4RBPBNOIZ].Set(0.0);
                BSIM4noise.Generators[BSIM4RBPSNOIZ].Set(0.0);
                BSIM4noise.Generators[BSIM4RBPDNOIZ].Set(0.0);
                BSIM4noise.Generators[BSIM4RBSBNOIZ].Set(0.0);
                BSIM4noise.Generators[BSIM4RBDBNOIZ].Set(0.0);
            }

            if (model.BSIM4tnoiMod == 2)
            {
                eta = 1.0 - here.BSIM4Vdseff * here.BSIM4AbovVgst2Vtm;
                T0 = 1.0 - eta;
                T1 = 1.0 + eta;
                T2 = T1 + 2.0 * here.BSIM4Abulk * model.BSIM4vtm / here.BSIM4Vgsteff;
                Leff = here.pParam.BSIM4leff;
                Lvsat = Leff * (1.0 + here.BSIM4Vdseff / here.BSIM4EsatL);
                T6 = Leff / Lvsat;
                /*Unwanted code for T5 commented*/
                /*T5 = here.BSIM4Vgsteff / here.BSIM4EsatL; 
                T5 = T5 * T5;
                */
                gamma = T6 * (0.5 * T1 + T0 * T0 / (6.0 * T2));
                T3 = T2 * T2;
                T4 = T0 * T0;
                T5 = T3 * T3;
                delta = (T1 / T3 - (5.0 * T1 + T2) * T4 / (15.0 * T5) + T4 * T4 / (9.0 * T5 * T2)) / (6.0 * T6 * T6 * T6);
                T7 = T0 / T2;
                epsilon = (T7 - T7 * T7 * T7 / 3.0) / (6.0 * T6);
                T8 = here.BSIM4Vgsteff / here.BSIM4EsatL;
                T8 *= T8;
                npart_c = model.BSIM4rnoic * (1.0 + T8
                        * model.BSIM4tnoic * Leff);
                ctnoi = epsilon / Math.Sqrt(gamma * delta)
                * (2.5316 * npart_c);

                npart_beta = model.BSIM4rnoia * (1.0 + T8
                    * model.BSIM4tnoia * Leff);
                npart_theta = model.BSIM4rnoib * (1.0 + T8
                    * model.BSIM4tnoib * Leff);
                gamma = gamma * (3.0 * npart_beta * npart_beta);
                delta = delta * (3.75 * npart_theta * npart_theta);

                GammaGd0 = gamma * here.BSIM4noiGd0;
                C0 = here.BSIM4Coxeff * here.pParam.BSIM4weffCV * here.BSIM4nf * here.pParam.BSIM4leffCV;
                T0 = C0 / here.BSIM4noiGd0;
                sigrat = T0 * Math.Sqrt(delta / gamma);
            }
            switch (model.BSIM4tnoiMod.Value)
            {
                case 0:
                    T0 = here.BSIM4ueff * Math.Abs(here.BSIM4qinv);
                    T1 = T0 * tmp + here.pParam.BSIM4leff * here.pParam.BSIM4leff;
                    BSIM4noise.Generators[BSIM4IDNOIZ].Set((T0 / T1) * model.BSIM4ntnoi);
                    break;
                case 1:
                    T0 = here.BSIM4gm + here.BSIM4gmbs + here.BSIM4gds;
                    T0 *= T0;
                    igsquare = npart_theta * npart_theta * T0 / here.BSIM4IdovVds;
                    T1 = npart_beta * (here.BSIM4gm + here.BSIM4gmbs) + here.BSIM4gds;
                    T2 = T1 * T1 / here.BSIM4IdovVds;
                    BSIM4noise.Generators[BSIM4IDNOIZ].Set(here.BSIM4sNodePrime, (T2 - igsquare));
                    break;
                case 2:
                    T2 = GammaGd0;
                    T3 = ctnoi * ctnoi;
                    T4 = 1.0 - T3;
                    BSIM4noise.Generators[BSIM4IDNOIZ].Set(here.BSIM4sNodePrime, T2 * T4);

                    /* Evaluate output noise due to two correlated noise sources */
                    omega = 2.0 * Math.PI * noise.Freq;
                    T5 = omega * sigrat;
                    T6 = T5 * T5;
                    T7 = T6 / (1.0 + T6);

                    if (here.BSIM4mode >= 0)
                        BSIM4noise.Generators[BSIM4CORLNOIZ].Set(T2 * T3, T2 * T7, 0.5 * Math.PI);
                    else
                    {
                        BSIM4noise.Generators[BSIM4CORLNOIZ].Set(T2 * T3, T2 * T7, 0.5 * Math.PI);
                    }
                    break;
            }

            BSIM4noise.Generators[BSIM4FLNOIZ].Set(0.0);

            switch (model.BSIM4fnoiMod.Value)
            {
                case 0:
                    BSIM4noise.Generators[BSIM4FLNOIZ].Set(model.BSIM4kf * Math.Exp(model.BSIM4af 
                        * Math.Log(Math.Max(Math.Abs(here.BSIM4cd), 1e-38))) / (Math.Pow(noise.Freq, model.BSIM4ef) 
                        * here.pParam.BSIM4leff * here.pParam.BSIM4leff * model.BSIM4coxe));
                    break;
                case 1:
                    Vds = state.States[0][here.BSIM4states + BSIM4v80.BSIM4vds];
                    if (Vds < 0.0)
                        Vds = -Vds;

                    Ssi = Eval1ovFNoise(Vds, model, here, noise.Freq, state.Temperature);
                    T10 = model.BSIM4oxideTrapDensityA * Circuit.CONSTBoltz * state.Temperature;
                    T11 = here.pParam.BSIM4weff * here.BSIM4nf * here.pParam.BSIM4leff 
                        * Math.Pow(noise.Freq, model.BSIM4ef) * 1.0e10 * here.BSIM4nstar * here.BSIM4nstar;
                    Swi = T10 / T11 * here.BSIM4cd * here.BSIM4cd;
                    T1 = Swi + Ssi;
                    if (T1 > 0.0)
                        BSIM4noise.Generators[BSIM4FLNOIZ].Set((Ssi * Swi) / T1);
                    else
                        BSIM4noise.Generators[BSIM4FLNOIZ].Set(0.0);
                    break;
            }

            if (here.BSIM4mode >= 0)
            {  
                // bugfix
                BSIM4noise.Generators[BSIM4IGSNOIZ].Set(here.BSIM4Igs + here.BSIM4Igcs);
                BSIM4noise.Generators[BSIM4IGDNOIZ].Set(here.BSIM4Igd + here.BSIM4Igcd);
            }
            else
            {
                BSIM4noise.Generators[BSIM4IGSNOIZ].Set(here.BSIM4Igs + here.BSIM4Igcd);
                BSIM4noise.Generators[BSIM4IGDNOIZ].Set(here.BSIM4Igd + here.BSIM4Igcs);
            }
            BSIM4noise.Generators[BSIM4IGBNOIZ].Set(here.BSIM4Igb);
        }

        /// <summary>
        /// Evaluate 1 over f noise
        /// </summary>
        /// <param name="Vds"></param>
        /// <param name="model"></param>
        /// <param name="here"></param>
        /// <param name="freq"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        public double Eval1ovFNoise(double Vds, BSIM4v80Model model, BSIM4v80 here, double freq, double temp)
        {
            double cd, esat, DelClm, EffFreq, N0, Nl, Leff, Leffsq;
            double T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, Ssi;

            cd = Math.Abs(here.BSIM4cd);
            Leff = here.pParam.BSIM4leff - 2.0 * model.BSIM4lintnoi;
            Leffsq = Leff * Leff;
            esat = 2.0 * here.BSIM4vsattemp / here.BSIM4ueff;
            if (model.BSIM4em <= 0.0) DelClm = 0.0; /* flicker noise modified -JX  */
            else
            {
                T0 = ((((Vds - here.BSIM4Vdseff) / here.pParam.BSIM4litl)
                       + model.BSIM4em) / esat);
                DelClm = here.pParam.BSIM4litl * Math.Log(Math.Max(T0, 1e-38));
                if (DelClm < 0.0) DelClm = 0.0;  /* bugfix */
            }
            EffFreq = Math.Pow(freq, model.BSIM4ef);
            T1 = Circuit.CHARGE * Circuit.CHARGE * Circuit.CONSTBoltz * cd * temp * here.BSIM4ueff;
            T2 = 1.0e10 * EffFreq * here.BSIM4Abulk * model.BSIM4coxe * Leffsq;
            N0 = model.BSIM4coxe * here.BSIM4Vgsteff / Circuit.CHARGE;
            Nl = model.BSIM4coxe * here.BSIM4Vgsteff
              * (1.0 - here.BSIM4AbovVgst2Vtm * here.BSIM4Vdseff) / Circuit.CHARGE;

            T3 = model.BSIM4oxideTrapDensityA
               * Math.Log(Math.Max(((N0 + here.BSIM4nstar) / (Nl + here.BSIM4nstar)), 1e-38));
            T4 = model.BSIM4oxideTrapDensityB * (N0 - Nl);
            T5 = model.BSIM4oxideTrapDensityC * 0.5 * (N0 * N0 - Nl * Nl);

            T6 = Circuit.CONSTBoltz * temp * cd * cd;
            T7 = 1.0e10 * EffFreq * Leffsq * here.pParam.BSIM4weff * here.BSIM4nf;
            T8 = model.BSIM4oxideTrapDensityA + model.BSIM4oxideTrapDensityB * Nl
               + model.BSIM4oxideTrapDensityC * Nl * Nl;
            T9 = (Nl + here.BSIM4nstar) * (Nl + here.BSIM4nstar);
            Ssi = T1 / T2 * (T3 + T4 + T5) + T6 / T7 * DelClm * T8 / T9;
            return Ssi;
        }
    }
}
