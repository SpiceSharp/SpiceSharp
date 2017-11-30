using System;
using SpiceSharp.Diagnostics;
using SpiceSharp.Behaviors;
using SpiceSharp.Circuits;

namespace SpiceSharp.Components.ComponentBehaviors
{
    /// <summary>
    /// Noise behaviour for a <see cref="BSIM3v30"/>
    /// </summary>
    public class BSIM3v30NoiseBehavior : CircuitObjectBehaviorNoise
    {
        /// <summary>
        /// Noise generators by their index
        /// </summary>
        private const int BSIM3RDNOIZ = 0;
        private const int BSIM3RSNOIZ = 1;
        private const int BSIM3IDNOIZ = 2;
        private const int BSIM3FLNOIZ = 3;

        /// <summary>
        /// Noise generators
        /// </summary>
        public ComponentNoise BSIM3noise { get; } = new ComponentNoise(
            new Noise.NoiseThermal("rd", 0, 4),
            new Noise.NoiseThermal("rs", 2, 5),
            new Noise.NoiseShot("id", 4, 5),
            new Noise.NoiseGain("1overf", 4, 5)
            );

        /// <summary>
        /// Setup the behaviour
        /// </summary>
        /// <param name="component">Component</param>
        /// <param name="ckt">Circuit</param>
        public override void Setup(ICircuitObject component, Circuit ckt)
        {
            base.Setup(component, ckt);
            var bsim3 = ComponentTyped<BSIM3v30>();
            BSIM3noise.Setup(ckt,
                bsim3.BSIM3dNode,
                bsim3.BSIM3gNode,
                bsim3.BSIM3sNode,
                bsim3.BSIM3bNode,
                bsim3.BSIM3dNodePrime,
                bsim3.BSIM3sNodePrime
                );
        }

        /// <summary>
        /// Execute behaviour
        /// </summary>
        /// <param name="ckt"></param>
        public override void Noise(Circuit ckt)
        {
            var here = ComponentTyped<BSIM3v30>();
            var model = here.Model as BSIM3v30Model;
            var state = ckt.State;
            var noise = state.Noise;

            BSIM3noise.Generators[BSIM3RDNOIZ].Set(here.BSIM3drainConductance);
            BSIM3noise.Generators[BSIM3RSNOIZ].Set(here.BSIM3sourceConductance);

            // Calculate shot noise factor
            switch (model.BSIM3noiMod.Value)
            {
                case 1.0:
                case 3.0:
                    BSIM3noise.Generators[BSIM3IDNOIZ].Set(2.0 * Math.Abs(here.BSIM3gm + here.BSIM3gds + here.BSIM3gmbs) / 3.0);
                    break;

                case 2.0:
                case 4.0:
                    BSIM3noise.Generators[BSIM3IDNOIZ].Set(here.BSIM3ueff * Math.Abs(here.BSIM3qinv) / (here.pParam.BSIM3leff * here.pParam.BSIM3leff
                        + here.BSIM3ueff * Math.Abs(here.BSIM3qinv) * here.BSIM3rds));
                    break;

                case 5.0:
                case 6.0:
                    double vds = Math.Min(state.States[0][here.BSIM3states + BSIM3v30.BSIM3vds], here.BSIM3vdsat);
                    BSIM3noise.Generators[BSIM3IDNOIZ].Set((3.0 - vds / here.BSIM3vdsat) * Math.Abs(here.BSIM3gm + here.BSIM3gds + here.BSIM3gmbs) / 3.0);
                    break;

                default:
                    CircuitWarning.Warning(model, $"Invalid noise model {model.BSIM3noiMod.Value}");
                    break;
            }

            // Calculate flicker noise factor
            switch (model.BSIM3noiMod.Value)
            {
                case 1.0:
                case 4.0:
                case 5.0:
                    BSIM3noise.Generators[BSIM3FLNOIZ].Set(model.BSIM3kf * Math.Exp(model.BSIM3af * Math.Log(Math.Max(Math.Abs(here.BSIM3cd), 1e-38)))
                        / (Math.Pow(noise.Freq, model.BSIM3ef) * here.pParam.BSIM3leff * here.pParam.BSIM3leff * model.BSIM3cox));
                    break;
                case 2.0:
                case 3.0:
                case 6.0:
                    double vds = state.States[0][here.BSIM3states + BSIM3v24.BSIM3vds];
                    if (vds < 0.0)
                        vds = -vds;
                    double Ssi = StrongInversionNoiseEval(vds, model, here, noise.Freq, state.Temperature);
                    double T10 = model.BSIM3oxideTrapDensityA * 8.62e-5 * state.Temperature;
                    double T11 = here.pParam.BSIM3weff * here.pParam.BSIM3leff * Math.Pow(noise.Freq, model.BSIM3ef) * 4.0e36;
                    double Swi = T10 / T11 * here.BSIM3cd * here.BSIM3cd;
                    double T1 = Swi + Ssi;
                    if (T1 > 0.0)
                        BSIM3noise.Generators[BSIM3FLNOIZ].Set((Ssi * Swi) / T1);
                    else
                        BSIM3noise.Generators[BSIM3FLNOIZ].Set(0.0);
                    break;
            }

            // Evaluate noise sources
            BSIM3noise.Evaluate(ckt);
        }

        /// <summary>
        /// Calculate noise in strong inversion
        /// </summary>
        /// <param name="Vds"></param>
        /// <param name="model"></param>
        /// <param name="here"></param>
        /// <param name="freq"></param>
        /// <param name="temp"></param>
        /// <returns></returns>
        private double StrongInversionNoiseEval(double Vds, BSIM3v30Model model, BSIM3v30 here, double freq, double temp)
        {
            double cd, esat, DelClm, EffFreq, N0, Nl, Leff, Leffsq;
            double T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, Ssi;

            cd = Math.Abs(here.BSIM3cd);
            Leff = here.pParam.BSIM3leff - 2.0 * model.BSIM3lintnoi;
            Leffsq = Leff * Leff;
            esat = 2.0 * here.pParam.BSIM3vsattemp / here.BSIM3ueff;
            if (model.BSIM3em <= 0.0) DelClm = 0.0;
            else
            {
                T0 = ((((Vds - here.BSIM3Vdseff) / here.pParam.BSIM3litl)
                    + model.BSIM3em) / esat);
                DelClm = here.pParam.BSIM3litl * Math.Log(Math.Max(T0, 1e-38));
            }
            EffFreq = Math.Pow(freq, model.BSIM3ef);
            T1 = Circuit.CHARGE * Circuit.CHARGE * 8.62e-5 * cd * temp * here.BSIM3ueff;
            T2 = 1.0e8 * EffFreq * here.BSIM3Abulk * model.BSIM3cox * Leffsq;

            N0 = model.BSIM3cox * here.BSIM3Vgsteff / Circuit.CHARGE;
            Nl = model.BSIM3cox * here.BSIM3Vgsteff
                 * (1.0 - here.BSIM3AbovVgst2Vtm * here.BSIM3Vdseff) / Circuit.CHARGE;

            T3 = model.BSIM3oxideTrapDensityA
               * Math.Log(Math.Max(((N0 + 2.0e14) / (Nl + 2.0e14)), 1e-38));
            T4 = model.BSIM3oxideTrapDensityB * (N0 - Nl);
            T5 = model.BSIM3oxideTrapDensityC * 0.5 * (N0 * N0 - Nl * Nl);

            T6 = 8.62e-5 * temp * cd * cd;
            T7 = 1.0e8 * EffFreq * Leffsq * here.pParam.BSIM3weff;
            T8 = model.BSIM3oxideTrapDensityA + model.BSIM3oxideTrapDensityB * Nl
               + model.BSIM3oxideTrapDensityC * Nl * Nl;
            T9 = (Nl + 2.0e14) * (Nl + 2.0e14);

            Ssi = T1 / T2 * (T3 + T4 + T5) + T6 / T7 * DelClm * T8 / T9;
            return Ssi;
        }
    }
}
