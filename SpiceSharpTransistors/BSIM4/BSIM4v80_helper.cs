using System;
using System.IO;
using SpiceSharp.Diagnostics;
using SpiceSharp.Components.Transistors;

namespace SpiceSharp.Components
{
    public partial class BSIM4v80 : CircuitComponent<BSIM4v80>
    {
        /// <summary>
        /// BSIM4NumFingerDiff
        /// </summary>
        private bool BSIM4NumFingerDiff(double nf, double minSD, out double nuIntD, out double nuEndD, out double nuIntS, out double nuEndS)
        {
            int NF;
            NF = (int)nf;
            if ((NF % 2) != 0)
            {
                nuEndD = nuEndS = 1.0;
                nuIntD = nuIntS = 2.0 * Math.Max((nf - 1.0) / 2.0, 0.0);
            }
            else
            {
                if (minSD == 1) /* minimize # of source */
                {
                    nuEndD = 2.0;
                    nuIntD = 2.0 * Math.Max((nf / 2.0 - 1.0), 0.0);
                    nuEndS = 0.0;
                    nuIntS = nf;
                }
                else
                {
                    nuEndD = 0.0;
                    nuIntD = nf;
                    nuEndS = 2.0;
                    nuIntS = 2.0 * Math.Max((nf / 2.0 - 1.0), 0.0);
                }
            }
            return true;
        }

        /// <summary>
        /// BSIM4PAeffGeo
        /// </summary>
        private bool BSIM4PAeffGeo(double nf, double geo, double minSD, double Weffcj, double DMCG, double DMCI, double DMDG, out double Ps, out double Pd, out double As, out double Ad)
        {
            double T0, T1, T2;
            double ADiso, ADsha, ADmer, ASiso, ASsha, ASmer;
            double PDiso, PDsha, PDmer, PSiso, PSsha, PSmer;
            double nuIntD = 0.0, nuEndD = 0.0, nuIntS = 0.0, nuEndS = 0.0;
            Ad = double.NaN;
            As = double.NaN;
            Pd = double.NaN;
            Ps = double.NaN;

            if (geo < 9) /* For geo = 9 and 10, the numbers of S/D diffusions already known */
                BSIM4NumFingerDiff(nf, minSD, out nuIntD, out nuEndD, out nuIntS, out nuEndS);

            T0 = DMCG + DMCI;
            T1 = DMCG + DMCG;
            T2 = DMDG + DMDG;

            PSiso = PDiso = T0 + T0 + Weffcj;
            PSsha = PDsha = T1;
            PSmer = PDmer = T2;

            ASiso = ADiso = T0 * Weffcj;
            ASsha = ADsha = DMCG * Weffcj;
            ASmer = ADmer = DMDG * Weffcj;

            switch (geo)
            {
                case 0:
                    Ps = nuEndS * PSiso + nuIntS * PSsha;
                    Pd = nuEndD * PDiso + nuIntD * PDsha;
                    As = nuEndS * ASiso + nuIntS * ASsha;
                    Ad = nuEndD * ADiso + nuIntD * ADsha;
                    break;
                case 1:
                    Ps = nuEndS * PSiso + nuIntS * PSsha;
                    Pd = (nuEndD + nuIntD) * PDsha;
                    As = nuEndS * ASiso + nuIntS * ASsha;
                    Ad = (nuEndD + nuIntD) * ADsha;
                    break;
                case 2:
                    Ps = (nuEndS + nuIntS) * PSsha;
                    Pd = nuEndD * PDiso + nuIntD * PDsha;
                    As = (nuEndS + nuIntS) * ASsha;
                    Ad = nuEndD * ADiso + nuIntD * ADsha;
                    break;
                case 3:
                    Ps = (nuEndS + nuIntS) * PSsha;
                    Pd = (nuEndD + nuIntD) * PDsha;
                    As = (nuEndS + nuIntS) * ASsha;
                    Ad = (nuEndD + nuIntD) * ADsha;
                    break;
                case 4:
                    Ps = nuEndS * PSiso + nuIntS * PSsha;
                    Pd = nuEndD * PDmer + nuIntD * PDsha;
                    As = nuEndS * ASiso + nuIntS * ASsha;
                    Ad = nuEndD * ADmer + nuIntD * ADsha;
                    break;
                case 5:
                    Ps = (nuEndS + nuIntS) * PSsha;
                    Pd = nuEndD * PDmer + nuIntD * PDsha;
                    As = (nuEndS + nuIntS) * ASsha;
                    Ad = nuEndD * ADmer + nuIntD * ADsha;
                    break;
                case 6:
                    Ps = nuEndS * PSmer + nuIntS * PSsha;
                    Pd = nuEndD * PDiso + nuIntD * PDsha;
                    As = nuEndS * ASmer + nuIntS * ASsha;
                    Ad = nuEndD * ADiso + nuIntD * ADsha;
                    break;
                case 7:
                    Ps = nuEndS * PSmer + nuIntS * PSsha;
                    Pd = (nuEndD + nuIntD) * PDsha;
                    As = nuEndS * ASmer + nuIntS * ASsha;
                    Ad = (nuEndD + nuIntD) * ADsha;
                    break;
                case 8:
                    Ps = nuEndS * PSmer + nuIntS * PSsha;
                    Pd = nuEndD * PDmer + nuIntD * PDsha;
                    As = nuEndS * ASmer + nuIntS * ASsha;
                    Ad = nuEndD * ADmer + nuIntD * ADsha;
                    break;
                case 9: /* geo = 9 and 10 happen only when nf = even */
                    Ps = PSiso + (nf - 1.0) * PSsha;
                    Pd = nf * PDsha;
                    As = ASiso + (nf - 1.0) * ASsha;
                    Ad = nf * ADsha;
                    break;
                case 10:
                    Ps = nf * PSsha;
                    Pd = PDiso + (nf - 1.0) * PDsha;
                    As = nf * ASsha;
                    Ad = ADiso + (nf - 1.0) * ADsha;
                    break;
                default:
                    CircuitWarning.Warning(this, $"Warning: Specified GEO = {geo} not matched");
                    break;
            }
            return true;
        }

        /// <summary>
        /// BSIM4RdseffGeo
        /// </summary>
        private bool BSIM4RdseffGeo(double nf, double geo, double rgeo, double minSD, double Weffcj, double Rsh, double DMCG, double DMCI, double DMDG, double Type, out double Rtot)
        {
            double Rint = 0.0, Rend = 0.0;
            double nuIntD = 0.0, nuEndD = 0.0, nuIntS = 0.0, nuEndS = 0.0;

            if (geo < 9) /* since geo = 9 and 10 only happen when nf = even */
            {
                BSIM4NumFingerDiff(nf, minSD, out nuIntD, out nuEndD, out nuIntS, out nuEndS);

                /* Internal S/D resistance -- assume shared S or D and all wide contacts */
                if (Type == 1)
                {
                    if (nuIntS == 0.0)
                        Rint = 0.0;
                    else
                        Rint = Rsh * DMCG / (Weffcj * nuIntS);
                }
                else
                {
                    if (nuIntD == 0.0)
                        Rint = 0.0;
                    else
                        Rint = Rsh * DMCG / (Weffcj * nuIntD);
                }
            }

            /* End S/D resistance  -- geo dependent */
            switch (geo)
            {
                case 0:
                    if (Type == 1)
                        BSIM4RdsEndIso(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndS, rgeo, 1, out Rend);
                    else
                        BSIM4RdsEndIso(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndD, rgeo, 0, out Rend);
                    break;
                case 1:
                    if (Type == 1)
                        BSIM4RdsEndIso(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndS, rgeo, 1, out Rend);
                    else
                        BSIM4RdsEndSha(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndD, rgeo, 0, out Rend);
                    break;
                case 2:
                    if (Type == 1)
                        BSIM4RdsEndSha(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndS, rgeo, 1, out Rend);
                    else
                        BSIM4RdsEndIso(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndD, rgeo, 0, out Rend);
                    break;
                case 3:
                    if (Type == 1)
                        BSIM4RdsEndSha(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndS, rgeo, 1, out Rend);
                    else
                        BSIM4RdsEndSha(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndD, rgeo, 0, out Rend);
                    break;
                case 4:
                    if (Type == 1)
                        BSIM4RdsEndIso(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndS, rgeo, 1, out Rend);
                    else
                        Rend = Rsh * DMDG / Weffcj;
                    break;
                case 5:
                    if (Type == 1)
                        BSIM4RdsEndSha(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndS, rgeo, 1, out Rend);
                    else
                        Rend = Rsh * DMDG / (Weffcj * nuEndD);
                    break;
                case 6:
                    if (Type == 1)
                        Rend = Rsh * DMDG / Weffcj;
                    else
                        BSIM4RdsEndIso(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndD, rgeo, 0, out Rend);
                    break;
                case 7:
                    if (Type == 1)
                        Rend = Rsh * DMDG / (Weffcj * nuEndS);
                    else
                        BSIM4RdsEndSha(Weffcj, Rsh, DMCG, DMCI, DMDG, nuEndD, rgeo, 0, out Rend);
                    break;
                case 8:
                    Rend = Rsh * DMDG / Weffcj;
                    break;
                case 9: /* all wide contacts assumed for geo = 9 and 10 */
                    if (Type == 1)
                    {
                        Rend = 0.5 * Rsh * DMCG / Weffcj;
                        if (nf == 2.0)
                            Rint = 0.0;
                        else
                            Rint = Rsh * DMCG / (Weffcj * (nf - 2.0));
                    }
                    else
                    {
                        Rend = 0.0;
                        Rint = Rsh * DMCG / (Weffcj * nf);
                    }
                    break;
                case 10:
                    if (Type == 1)
                    {
                        Rend = 0.0;
                        Rint = Rsh * DMCG / (Weffcj * nf);
                    }
                    else
                    {
                        Rend = 0.5 * Rsh * DMCG / Weffcj; ;
                        if (nf == 2.0)
                            Rint = 0.0;
                        else
                            Rint = Rsh * DMCG / (Weffcj * (nf - 2.0));
                    }
                    break;
                default:
                    CircuitWarning.Warning(this, $"Warning: Specified GEO = {geo} not matched");
                    break;
            }

            if (Rint <= 0.0)
                Rtot = Rend;
            else if (Rend <= 0.0)

                Rtot = Rint;
            else

                Rtot = Rint * Rend / (Rint + Rend);
            if (Rtot == 0.0)

                CircuitWarning.Warning(this, "Warning: Zero resistance returned from RdseffGeo");
            return true;
        }

        /// <summary>
        /// BSIM4RdsEndIso
        /// </summary>
        private bool BSIM4RdsEndIso(double Weffcj, double Rsh, double DMCG, double DMCI, double DMDG, double nuEnd, double rgeo, double Type, out double Rend)
        {
            Rend = double.NaN;
            if (Type == 1)
            {
                switch (rgeo)
                {
                    case 1:
                    case 2:
                    case 5:
                        if (nuEnd == 0.0)

                            Rend = 0.0;
                        else
                            Rend = Rsh * DMCG / (Weffcj * nuEnd);
                        break;
                    case 3:
                    case 4:
                    case 6:
                        if ((DMCG + DMCI) == 0.0)
                            CircuitWarning.Warning(this, "(DMCG + DMCI) can not be equal to zero");
                        if ((nuEnd == 0.0) || ((DMCG + DMCI) == 0.0))
                            Rend = 0.0;
                        else
                            Rend = Rsh * Weffcj / (3.0 * nuEnd * (DMCG + DMCI));
                        break;
                    default:

                        CircuitWarning.Warning(this, $"Warning: Specified RGEO = {rgeo} not matched");
                        break;
                }
            }
            else
            {
                switch (rgeo)
                {
                    case 1:
                    case 3:
                    case 7:
                        if (nuEnd == 0.0)
                            Rend = 0.0;
                        else
                            Rend = Rsh * DMCG / (Weffcj * nuEnd);
                        break;
                    case 2:
                    case 4:
                    case 8:
                        if ((DMCG + DMCI) == 0.0)
                            CircuitWarning.Warning(this, "(DMCG + DMCI) can not be equal to zero");
                        if ((nuEnd == 0.0) || ((DMCG + DMCI) == 0.0))
                            Rend = 0.0;
                        else
                            Rend = Rsh * Weffcj / (3.0 * nuEnd * (DMCG + DMCI));
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Warning: Specified RGEO = {rgeo} not matched");
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// BSIM4RdsEndSha
        /// </summary>
        private bool BSIM4RdsEndSha(double Weffcj, double Rsh, double DMCG, double DMCI, double DMDG, double nuEnd, double rgeo, double Type, out double Rend)
        {
            Rend = double.NaN;
            if (Type == 1)
            {
                switch (rgeo)
                {
                    case 1:
                    case 2:
                    case 5:
                        if (nuEnd == 0.0)
                            Rend = 0.0;
                        else
                            Rend = Rsh * DMCG / (Weffcj * nuEnd);
                        break;
                    case 3:
                    case 4:
                    case 6:
                        if (DMCG == 0.0)
                            CircuitWarning.Warning(this, "DMCG can not be equal to zero");
                        if (nuEnd == 0.0)
                            Rend = 0.0;
                        else
                            Rend = Rsh * Weffcj / (6.0 * nuEnd * DMCG);
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Warning: Specified RGEO = {rgeo} not matched");
                        break;
                }
            }
            else
            {
                switch (rgeo)
                {
                    case 1:
                    case 3:
                    case 7:
                        if (nuEnd == 0.0)
                            Rend = 0.0;
                        else
                            Rend = Rsh * DMCG / (Weffcj * nuEnd);
                        break;
                    case 2:
                    case 4:
                    case 8:
                        if (DMCG == 0.0)
                            CircuitWarning.Warning(this, "DMCG can not be equal to zero");
                        if (nuEnd == 0.0)
                            Rend = 0.0;
                        else
                            Rend = Rsh * Weffcj / (6.0 * nuEnd * DMCG);
                        break;
                    default:
                        CircuitWarning.Warning(this, $"Warning: Specified RGEO = {rgeo} not matched");
                        break;
                }
            }
            return true;
        }

        /// <summary>
        /// Dexp
        /// </summary>
        private double Dexp(double A)
        {
            if (A > Transistor.EXP_THRESHOLD)
            {
                return Transistor.MAX_EXP * (1.0 + A - Transistor.EXP_THRESHOLD);
            }
            else if (A < -Transistor.EXP_THRESHOLD)
            {
                return Transistor.MIN_EXP;
            }
            else
            {
                return Math.Exp(A);
            }
        }

        /// <summary>
        /// Dexp
        /// </summary>
        public void Dexp(double A, out double B, out double C)
        {
            if (A > Transistor.EXP_THRESHOLD)
            {
                B = Transistor.MAX_EXP * (1.0 + (A) - Transistor.EXP_THRESHOLD);
                C = Transistor.MAX_EXP;
            }
            else if (A < -Transistor.EXP_THRESHOLD)
            {
                B = Transistor.MIN_EXP;
                C = 0;
            }
            else
            {
                B = Math.Exp(A);
                C = B;
            }
        }

        /// <summary>
        /// BSIM4DioIjthVjmEval
        /// </summary>
        double BSIM4DioIjthVjmEval(double Nvtm, double Ijth, double Isb, double XExpBV)
        {
            double Tb, Tc, EVjmovNv;
            Tc = XExpBV;
            Tb = 1.0 + Ijth / Isb - Tc;
            EVjmovNv = 0.5 * (Tb + Math.Sqrt(Tb * Tb + 4.0 * Tc));
            return Nvtm * Math.Log(EVjmovNv);
        }

        /// <summary>
        /// BSIM4checkModel
        /// </summary>
        private bool BSIM4checkModel(Circuit ckt)
        {
            var model = Model as BSIM4v80Model;
            bool Fatal_Flag = false;
            using (StreamWriter sw = new StreamWriter("bsim4.out", true))
            {
                sw.WriteLine("BSIM4v80: Berkeley Short Channel IGFET Model-4");
                sw.WriteLine("Developed by Xuemei (Jane) Xi, Mohan Dunga, Prof. Ali Niknejad and Prof. Chenming Hu in 2003.");
                sw.WriteLine("");

                sw.WriteLine("++++++++++ BSIM4v80 PARAMETER CHECKING BELOW ++++++++++");

                if (Math.Abs(model.BSIM4version - 4.80) > 0.0001)
                {
                    sw.WriteLine("Warning: This model is BSIM4v80.8.0; you specified a wrong version number.");
                    CircuitWarning.Warning(this, "Warning: This model is BSIM4v80.8.0; you specified a wrong version number.");
                }

                sw.WriteLine($"Model = {model.Name}");

                if ((BSIM4rgateMod == 2) || (BSIM4rgateMod == 3))
                {
                    if ((BSIM4trnqsMod == 1) || (BSIM4acnqsMod == 1))
                    {
                        sw.WriteLine("Warning: You've selected both Rg and charge deficit NQS; select one only.");
                        CircuitWarning.Warning(this, "Warning: You've selected both Rg and charge deficit NQS; select one only.");
                    }
                }

                if (model.BSIM4toxe <= 0.0)
                {
                    sw.WriteLine($"Fatal: Toxe = {model.BSIM4toxe} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Toxe = {model.BSIM4toxe} is not positive.");
                    Fatal_Flag = true;
                }
                if (BSIM4toxp <= 0.0)
                {
                    sw.WriteLine($"Fatal: Toxp = {BSIM4toxp} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Toxp = {BSIM4toxp} is not positive.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4eot <= 0.0)
                {
                    sw.WriteLine($"Fatal: EOT = {model.BSIM4eot} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: EOT = {model.BSIM4eot} is not positive.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4epsrgate < 0.0)
                {
                    sw.WriteLine($"Fatal: Epsrgate = {model.BSIM4epsrgate} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Epsrgate = {model.BSIM4epsrgate} is not positive.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4epsrsub < 0.0)
                {
                    sw.WriteLine($"Fatal: Epsrsub = {model.BSIM4epsrsub} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Epsrsub = {model.BSIM4epsrsub} is not positive.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4easub < 0.0)
                {
                    sw.WriteLine($"Fatal: Easub = {model.BSIM4easub} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Easub = {model.BSIM4easub} is not positive.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4ni0sub <= 0.0)
                {
                    sw.WriteLine($"Fatal: Ni0sub = {model.BSIM4ni0sub} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Easub = {model.BSIM4ni0sub} is not positive.");
                    Fatal_Flag = true;
                }

                if (model.BSIM4toxm <= 0.0)
                {
                    sw.WriteLine($"Fatal: Toxm = {model.BSIM4toxm} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Toxm = {model.BSIM4toxm} is not positive.");
                    Fatal_Flag = true;
                }

                if (model.BSIM4toxref <= 0.0)
                {
                    sw.WriteLine($"Fatal: Toxref = {model.BSIM4toxref} is not positive.");
                    CircuitWarning.Warning(this, $"Fatal: Toxref = {model.BSIM4toxref} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4lpe0 < -pParam.BSIM4leff)
                {
                    sw.WriteLine($"Fatal: Lpe0 = {pParam.BSIM4lpe0} is less than -Leff.");
                    CircuitWarning.Warning(this, $"Fatal: Lpe0 = {pParam.BSIM4lpe0} is less than -Leff.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4lintnoi > pParam.BSIM4leff / 2)
                {
                    sw.WriteLine($"Fatal: Lintnoi = {model.BSIM4lintnoi} is too large - Leff for noise is negative.");
                    CircuitWarning.Warning(this, $"Fatal: Lintnoi = {model.BSIM4lintnoi} is too large - Leff for noise is negative.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4lpeb < -pParam.BSIM4leff)
                {
                    sw.WriteLine($"Fatal: Lpeb = {pParam.BSIM4lpeb} is less than -Leff.");
                    CircuitWarning.Warning(this, $"Fatal: Lpeb = {pParam.BSIM4lpeb} is less than -Leff.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4ndep <= 0.0)
                {
                    sw.WriteLine($"Fatal: Ndep = {pParam.BSIM4ndep} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Ndep = {pParam.BSIM4ndep} is not positive.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4phi <= 0.0)
                {
                    sw.WriteLine($"Fatal: Phi = {pParam.BSIM4phi} is not positive. Please check Phin and Ndep");
                    sw.WriteLine($"	   Phin = {pParam.BSIM4phin}  Ndep = {pParam.BSIM4ndep} ");
                    CircuitWarning.Warning(this, $"Fatal: Phi = {pParam.BSIM4phi} is not positive. Please check Phin and Ndep");
                    CircuitWarning.Warning(this, $"Phin = {pParam.BSIM4phin}  Ndep = {pParam.BSIM4ndep} ");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4nsub <= 0.0)
                {
                    sw.WriteLine($"Fatal: Nsub = {pParam.BSIM4nsub} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Nsub = {pParam.BSIM4nsub} is not positive.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4ngate < 0.0)
                {
                    sw.WriteLine($"Fatal: Ngate = {pParam.BSIM4ngate} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Ngate = {pParam.BSIM4ngate} Ngate is not positive.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4ngate > 1.0e25)
                {
                    sw.WriteLine($"Fatal: Ngate = {pParam.BSIM4ngate} is too high.");

                    CircuitWarning.Warning(this, $"Fatal: Ngate = {pParam.BSIM4ngate} Ngate is too high");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4xj <= 0.0)
                {
                    sw.WriteLine($"Fatal: Xj = {pParam.BSIM4xj} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Xj = {pParam.BSIM4xj} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4dvt1 < 0.0)
                {
                    sw.WriteLine($"Fatal: Dvt1 = {pParam.BSIM4dvt1} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Dvt1 = {pParam.BSIM4dvt1} is negative.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4dvt1w < 0.0)
                {
                    sw.WriteLine($"Fatal: Dvt1w = {pParam.BSIM4dvt1w} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Dvt1w = {pParam.BSIM4dvt1w} is negative.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4w0 == -pParam.BSIM4weff)
                {
                    sw.WriteLine("Fatal: (W0 + Weff) = 0 causing divided-by-zero.");

                    CircuitWarning.Warning(this, "Fatal: (W0 + Weff) = 0 causing divided-by-zero.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4dsub < 0.0)
                {
                    sw.WriteLine($"Fatal: Dsub = {pParam.BSIM4dsub} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Dsub = {pParam.BSIM4dsub} is negative.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4b1 == -pParam.BSIM4weff)
                {
                    sw.WriteLine("Fatal: (B1 + Weff) = 0 causing divided-by-zero.");

                    CircuitWarning.Warning(this, "Fatal: (B1 + Weff) = 0 causing divided-by-zero.");
                    Fatal_Flag = true;
                }
                if (BSIM4u0temp <= 0.0)
                {
                    sw.WriteLine($"Fatal: u0 at current temperature = {BSIM4u0temp} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: u0 at current temperature = {BSIM4u0temp} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4delta < 0.0)
                {
                    sw.WriteLine($"Fatal: Delta = {pParam.BSIM4delta} is less than zero.");

                    CircuitWarning.Warning(this, $"Fatal: Delta = {pParam.BSIM4delta} is less than zero.");
                    Fatal_Flag = true;
                }

                if (BSIM4vsattemp <= 0.0)
                {
                    sw.WriteLine($"Fatal: Vsat at current temperature = {BSIM4vsattemp} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Vsat at current temperature = {BSIM4vsattemp} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4pclm <= 0.0)
                {
                    sw.WriteLine($"Fatal: Pclm = {pParam.BSIM4pclm} is not positive.");

                    CircuitWarning.Warning(this, $"Fatal: Pclm = {pParam.BSIM4pclm} is not positive.");
                    Fatal_Flag = true;
                }

                if (pParam.BSIM4drout < 0.0)
                {
                    sw.WriteLine($"Fatal: Drout = {pParam.BSIM4drout} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Drout = {pParam.BSIM4drout} is negative.");
                    Fatal_Flag = true;
                }

                if (BSIM4nf < 1.0)
                {
                    sw.WriteLine($"Fatal: Number of finger = {BSIM4nf} is smaller than one.");
                    CircuitWarning.Warning(this, $"Fatal: Number of finger = {BSIM4nf} is smaller than one.");
                    Fatal_Flag = true;
                }

                if ((BSIM4sa > 0.0) && (BSIM4sb > 0.0) &&
                   ((BSIM4nf == 1.0) || ((BSIM4nf > 1.0) && (BSIM4sd > 0.0))))
                {
                    if (model.BSIM4saref <= 0.0)
                    {
                        sw.WriteLine($"Fatal: SAref = {model.BSIM4saref} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: SAref = {model.BSIM4saref} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4sbref <= 0.0)
                    {
                        sw.WriteLine($"Fatal: SBref = {model.BSIM4sbref} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: SBref = {model.BSIM4sbref} is not positive.");
                        Fatal_Flag = true;
                    }
                }

                if ((BSIM4l + model.BSIM4xl) <= model.BSIM4xgl)
                {
                    sw.WriteLine("Fatal: The parameter xgl must be smaller than Ldrawn+XL.");
                    CircuitWarning.Warning(this, "Fatal: The parameter xgl must be smaller than Ldrawn+XL.");
                    Fatal_Flag = true;
                }
                if (BSIM4ngcon < 1.0)
                {
                    sw.WriteLine("Fatal: The parameter ngcon cannot be smaller than one.");
                    CircuitWarning.Warning(this, "Fatal: The parameter ngcon cannot be smaller than one.");
                    Fatal_Flag = true;
                }
                if ((BSIM4ngcon != 1.0) && (BSIM4ngcon != 2.0))
                {
                    BSIM4ngcon.Value = 1.0;
                    sw.WriteLine("Warning: Ngcon must be equal to one or two; reset to 1.0.");
                    CircuitWarning.Warning(this, "Warning: Ngcon must be equal to one or two; reset to 1.0.");
                }

                if (model.BSIM4gbmin < 1.0e-20)
                {
                    sw.WriteLine($"Warning: Gbmin = {model.BSIM4gbmin} is too small.");
                    CircuitWarning.Warning(this, $"Warning: Gbmin = {model.BSIM4gbmin} is too small.");
                }

                /* Check saturation parameters */
                if (pParam.BSIM4fprout < 0.0)
                {
                    sw.WriteLine($"Fatal: fprout = {pParam.BSIM4fprout} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: fprout = {pParam.BSIM4fprout} is negative.");
                    Fatal_Flag = true;
                }
                if (pParam.BSIM4pdits < 0.0)
                {
                    sw.WriteLine($"Fatal: pdits = {pParam.BSIM4pdits} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: pdits = {pParam.BSIM4pdits} is negative.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4pditsl < 0.0)
                {
                    sw.WriteLine($"Fatal: pditsl = {model.BSIM4pditsl} is negative.");
                    CircuitWarning.Warning(this, $"Fatal: pditsl = {model.BSIM4pditsl} is negative.");
                    Fatal_Flag = true;
                }

                /* Check gate current parameters */
                if (model.BSIM4igbMod > 0)
                {
                    if (pParam.BSIM4nigbinv <= 0.0)
                    {
                        sw.WriteLine($"Fatal: nigbinv = {pParam.BSIM4nigbinv} is non-positive.");
                        CircuitWarning.Warning(this, $"Fatal: nigbinv = {pParam.BSIM4nigbinv} is non-positive.");
                        Fatal_Flag = true;
                    }
                    if (pParam.BSIM4nigbacc <= 0.0)
                    {
                        sw.WriteLine($"Fatal: nigbacc = {pParam.BSIM4nigbacc} is non-positive.");
                        CircuitWarning.Warning(this, $"Fatal: nigbacc = {pParam.BSIM4nigbacc} is non-positive.");
                        Fatal_Flag = true;
                    }
                }
                if (model.BSIM4igcMod > 0)
                {
                    if (pParam.BSIM4nigc <= 0.0)
                    {
                        sw.WriteLine($"Fatal: nigc = {pParam.BSIM4nigc} is non-positive.");
                        CircuitWarning.Warning(this, $"Fatal: nigc = {pParam.BSIM4nigc} is non-positive.");
                        Fatal_Flag = true;
                    }
                    if (pParam.BSIM4poxedge <= 0.0)
                    {
                        sw.WriteLine($"Fatal: poxedge = {pParam.BSIM4poxedge} is non-positive.");
                        CircuitWarning.Warning(this, $"Fatal: poxedge = {pParam.BSIM4poxedge} is non-positive.");
                        Fatal_Flag = true;
                    }
                    if (pParam.BSIM4pigcd <= 0.0)
                    {
                        sw.WriteLine($"Fatal: pigcd = {pParam.BSIM4pigcd} is non-positive.");
                        CircuitWarning.Warning(this, $"Fatal: pigcd = {pParam.BSIM4pigcd} is non-positive.");
                        Fatal_Flag = true;
                    }
                }

                /* Check capacitance parameters */
                if (pParam.BSIM4clc < 0.0)
                {
                    sw.WriteLine($"Fatal: Clc = {pParam.BSIM4clc} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Clc = {pParam.BSIM4clc} is negative.");
                    Fatal_Flag = true;
                }

                /* Check overlap capacitance parameters */
                if (pParam.BSIM4ckappas < 0.02)
                {
                    sw.WriteLine($"Warning: ckappas = {pParam.BSIM4ckappas} is too small. Set to 0.02");
                    CircuitWarning.Warning(this, $"Warning: ckappas = {pParam.BSIM4ckappas} is too small.");
                    pParam.BSIM4ckappas = 0.02;
                }
                if (pParam.BSIM4ckappad < 0.02)
                {
                    sw.WriteLine($"Warning: ckappad = {pParam.BSIM4ckappad} is too small. Set to 0.02");
                    CircuitWarning.Warning(this, $"Warning: ckappad = {pParam.BSIM4ckappad} is too small.");
                    pParam.BSIM4ckappad = 0.02;
                }

                if (model.BSIM4vtss < 0.0)
                {
                    sw.WriteLine($"Fatal: Vtss = {model.BSIM4vtss} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Vtss = {model.BSIM4vtss} is negative.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4vtsd < 0.0)
                {
                    sw.WriteLine($"Fatal: Vtsd = {model.BSIM4vtsd} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Vtsd = {model.BSIM4vtsd} is negative.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4vtssws < 0.0)
                {
                    sw.WriteLine($"Fatal: Vtssws = {model.BSIM4vtssws} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Vtssws = {model.BSIM4vtssws} is negative.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4vtsswd < 0.0)
                {
                    sw.WriteLine($"Fatal: Vtsswd = {model.BSIM4vtsswd} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Vtsswd = {model.BSIM4vtsswd} is negative.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4vtsswgs < 0.0)
                {
                    sw.WriteLine($"Fatal: Vtsswgs = {model.BSIM4vtsswgs} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Vtsswgs = {model.BSIM4vtsswgs} is negative.");
                    Fatal_Flag = true;
                }
                if (model.BSIM4vtsswgd < 0.0)
                {
                    sw.WriteLine($"Fatal: Vtsswgd = {model.BSIM4vtsswgd} is negative.");

                    CircuitWarning.Warning(this, $"Fatal: Vtsswgd = {model.BSIM4vtsswgd} is negative.");
                    Fatal_Flag = true;
                }


                if (model.BSIM4paramChk.Value == 1)
                {
                    /* Check L and W parameters */
                    if (pParam.BSIM4leff <= 1.0e-9)
                    {
                        sw.WriteLine($"Warning: Leff = {pParam.BSIM4leff} <= 1.0e-9. Recommended Leff >= 1e-8 ");

                        CircuitWarning.Warning(this, $"Warning: Leff = {pParam.BSIM4leff} <= 1.0e-9. Recommended Leff >= 1e-8 ");
                    }

                    if (pParam.BSIM4leffCV <= 1.0e-9)
                    {
                        sw.WriteLine($"Warning: Leff for CV = {pParam.BSIM4leffCV} <= 1.0e-9. Recommended LeffCV >=1e-8 ");

                        CircuitWarning.Warning(this, $"Warning: Leff for CV = {pParam.BSIM4leffCV} <= 1.0e-9. Recommended LeffCV >=1e-8 ");
                    }

                    if (pParam.BSIM4weff <= 1.0e-9)
                    {
                        sw.WriteLine($"Warning: Weff = {pParam.BSIM4weff} <= 1.0e-9. Recommended Weff >=1e-7 ");

                        CircuitWarning.Warning(this, $"Warning: Weff = {pParam.BSIM4weff} <= 1.0e-9. Recommended Weff >=1e-7 ");
                    }

                    if (pParam.BSIM4weffCV <= 1.0e-9)
                    {
                        sw.WriteLine($"Warning: Weff for CV = {pParam.BSIM4weffCV} <= 1.0e-9. Recommended WeffCV >= 1e-7 ");

                        CircuitWarning.Warning(this, $"Warning: Weff for CV = {pParam.BSIM4weffCV} <= 1.0e-9. Recommended WeffCV >= 1e-7 ");
                    }

                    /* Check threshold voltage parameters */
                    if (model.BSIM4toxe < 1.0e-10)
                    {
                        sw.WriteLine($"Warning: Toxe = {model.BSIM4toxe} is less than 1A. Recommended Toxe >= 5A");

                        CircuitWarning.Warning(this, $"Warning: Toxe = {model.BSIM4toxe} is less than 1A. Recommended Toxe >= 5A");
                    }
                    if (BSIM4toxp < 1.0e-10)
                    {
                        sw.WriteLine($"Warning: Toxp = {BSIM4toxp} is less than 1A. Recommended Toxp >= 5A");
                        CircuitWarning.Warning(this, $"Warning: Toxp = {BSIM4toxp} is less than 1A. Recommended Toxp >= 5A");
                    }
                    if (model.BSIM4toxm < 1.0e-10)
                    {
                        sw.WriteLine($"Warning: Toxm = {model.BSIM4toxm} is less than 1A. Recommended Toxm >= 5A");
                        CircuitWarning.Warning(this, $"Warning: Toxm = {model.BSIM4toxm} is less than 1A. Recommended Toxm >= 5A");
                    }

                    if (pParam.BSIM4ndep <= 1.0e12)
                    {
                        sw.WriteLine($"Warning: Ndep = {pParam.BSIM4ndep} may be too small.");

                        CircuitWarning.Warning(this, $"Warning: Ndep = {pParam.BSIM4ndep} may be too small.");
                    }
                    else if (pParam.BSIM4ndep >= 1.0e21)
                    {
                        sw.WriteLine($"Warning: Ndep = {pParam.BSIM4ndep} may be too large.");

                        CircuitWarning.Warning(this, $"Warning: Ndep = {pParam.BSIM4ndep} may be too large.");
                    }

                    if (pParam.BSIM4nsub <= 1.0e14)
                    {
                        sw.WriteLine($"Warning: Nsub = {pParam.BSIM4nsub} may be too small.");

                        CircuitWarning.Warning(this, $"Warning: Nsub = {pParam.BSIM4nsub} may be too small.");
                    }
                    else if (pParam.BSIM4nsub >= 1.0e21)
                    {
                        sw.WriteLine($"Warning: Nsub = {pParam.BSIM4nsub} may be too large.");

                        CircuitWarning.Warning(this, $"Warning: Nsub = {pParam.BSIM4nsub} may be too large.");
                    }

                    if ((pParam.BSIM4ngate > 0.0) &&
                        (pParam.BSIM4ngate <= 1.0e18))
                    {
                        sw.WriteLine($"Warning: Ngate = {pParam.BSIM4ngate} is less than 1.E18cm^-3.");

                        CircuitWarning.Warning(this, $"Warning: Ngate = {pParam.BSIM4ngate} is less than 1.E18cm^-3.");
                    }

                    if (pParam.BSIM4dvt0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Dvt0 = {pParam.BSIM4dvt0} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Dvt0 = {pParam.BSIM4dvt0} is negative.");
                    }

                    if (Math.Abs(1.0e-8 / (pParam.BSIM4w0 + pParam.BSIM4weff)) > 10.0)
                    {
                        sw.WriteLine("Warning: (W0 + Weff) may be too small.");

                        CircuitWarning.Warning(this, "Warning: (W0 + Weff) may be too small.");
                    }

                    /* Check subthreshold parameters */
                    if (pParam.BSIM4nfactor < 0.0)
                    {
                        sw.WriteLine($"Warning: Nfactor = {pParam.BSIM4nfactor} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Nfactor = {pParam.BSIM4nfactor} is negative.");
                    }
                    if (pParam.BSIM4cdsc < 0.0)
                    {
                        sw.WriteLine($"Warning: Cdsc = {pParam.BSIM4cdsc} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Cdsc = {pParam.BSIM4cdsc} is negative.");
                    }
                    if (pParam.BSIM4cdscd < 0.0)
                    {
                        sw.WriteLine($"Warning: Cdscd = {pParam.BSIM4cdscd} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Cdscd = {pParam.BSIM4cdscd} is negative.");
                    }
                    /* Check DIBL parameters */
                    if (BSIM4eta0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Eta0 = {BSIM4eta0} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Eta0 = {BSIM4eta0} is negative.");
                    }

                    /* Check Abulk parameters */
                    if (Math.Abs(1.0e-8 / (pParam.BSIM4b1 + pParam.BSIM4weff)) > 10.0)
                    {
                        sw.WriteLine("Warning: (B1 + Weff) may be too small.");

                        CircuitWarning.Warning(this, "Warning: (B1 + Weff) may be too small.");
                    }


                    /* Check Saturation parameters */
                    if (pParam.BSIM4a2 < 0.01)
                    {
                        sw.WriteLine($"Warning: A2 = {pParam.BSIM4a2} is too small. Set to 0.01.");

                        CircuitWarning.Warning(this, $"Warning: A2 = {pParam.BSIM4a2} is too small. Set to 0.01.");
                        pParam.BSIM4a2 = 0.01;
                    }
                    else if (pParam.BSIM4a2 > 1.0)
                    {
                        sw.WriteLine($"Warning: A2 = {pParam.BSIM4a2} is larger than 1. A2 is set to 1 and A1 is set to 0.");

                        CircuitWarning.Warning(this, $"Warning: A2 = {pParam.BSIM4a2} is larger than 1. A2 is set to 1 and A1 is set to 0.");
                        pParam.BSIM4a2 = 1.0;
                        pParam.BSIM4a1 = 0.0;
                    }

                    if (pParam.BSIM4prwg < 0.0)
                    {
                        sw.WriteLine($"Warning: Prwg = {pParam.BSIM4prwg} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: Prwg = {pParam.BSIM4prwg} is negative. Set to zero.");
                        pParam.BSIM4prwg = 0.0;
                    }

                    if (pParam.BSIM4rdsw < 0.0)
                    {
                        sw.WriteLine($"Warning: Rdsw = {pParam.BSIM4rdsw} is negative. Set to zero.");

                        CircuitWarning.Warning(this, $"Warning: Rdsw = {pParam.BSIM4rdsw} is negative. Set to zero.");
                        pParam.BSIM4rdsw = 0.0;
                        pParam.BSIM4rds0 = 0.0;
                    }

                    if (pParam.BSIM4rds0 < 0.0)
                    {
                        sw.WriteLine($"Warning: Rds at current temperature = {pParam.BSIM4rds0} is negative. Set to zero.");

                        CircuitWarning.Warning(this, $"Warning: Rds at current temperature = {pParam.BSIM4rds0} is negative. Set to zero.");
                        pParam.BSIM4rds0 = 0.0;
                    }

                    if (pParam.BSIM4rdswmin < 0.0)
                    {
                        sw.WriteLine($"Warning: Rdswmin at current temperature = {pParam.BSIM4rdswmin} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: Rdswmin at current temperature = {pParam.BSIM4rdswmin} is negative. Set to zero.");
                        pParam.BSIM4rdswmin = 0.0;
                    }

                    if (pParam.BSIM4pscbe2 <= 0.0)
                    {
                        sw.WriteLine($"Warning: Pscbe2 = {pParam.BSIM4pscbe2} is not positive.");
                        CircuitWarning.Warning(this, $"Warning: Pscbe2 = {pParam.BSIM4pscbe2} is not positive.");
                    }

                    if (pParam.BSIM4vsattemp < 1.0e3)
                    {
                        sw.WriteLine($"Warning: Vsat at current temperature = {pParam.BSIM4vsattemp} may be too small.");

                        CircuitWarning.Warning(this, $"Warning: Vsat at current temperature = {pParam.BSIM4vsattemp} may be too small.");
                    }

                    if ((model.BSIM4lambda.Given) && (pParam.BSIM4lambda > 0.0))
                    {
                        if (pParam.BSIM4lambda > 1.0e-9)
                        {
                            sw.WriteLine($"Warning: Lambda = {pParam.BSIM4lambda} may be too large.");

                            CircuitWarning.Warning(this, $"Warning: Lambda = {pParam.BSIM4lambda} may be too large.");
                        }
                    }

                    if ((model.BSIM4vtl.Given) && (pParam.BSIM4vtl > 0.0))
                    {
                        if (pParam.BSIM4vtl < 6.0e4)
                        {
                            sw.WriteLine($"Warning: Thermal velocity vtl = {pParam.BSIM4vtl} may be too small.");

                            CircuitWarning.Warning(this, $"Warning: Thermal velocity vtl = {pParam.BSIM4vtl} may be too small.");
                        }

                        if (pParam.BSIM4xn < 3.0)
                        {
                            sw.WriteLine($"Warning: back scattering coeff xn = {pParam.BSIM4xn} is too small.");

                            CircuitWarning.Warning(this, $"Warning: back scattering coeff xn = {pParam.BSIM4xn} is too small. Reset to 3.0 ");
                            pParam.BSIM4xn = 3.0;
                        }

                        if (model.BSIM4lc < 0.0)
                        {
                            sw.WriteLine($"Warning: back scattering coeff lc = {model.BSIM4lc} is too small.");

                            CircuitWarning.Warning(this, $"Warning: back scattering coeff lc = {model.BSIM4lc} is too small. Reset to 0.0");
                            pParam.BSIM4lc = 0.0;
                        }
                    }

                    if (pParam.BSIM4pdibl1 < 0.0)
                    {
                        sw.WriteLine($"Warning: Pdibl1 = {pParam.BSIM4pdibl1} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Pdibl1 = {pParam.BSIM4pdibl1} is negative.");
                    }
                    if (pParam.BSIM4pdibl2 < 0.0)
                    {
                        sw.WriteLine($"Warning: Pdibl2 = {pParam.BSIM4pdibl2} is negative.");

                        CircuitWarning.Warning(this, $"Warning: Pdibl2 = {pParam.BSIM4pdibl2} is negative.");
                    }

                    /* Check stress effect parameters */
                    if ((BSIM4sa > 0.0) && (BSIM4sb > 0.0) &&
                       ((BSIM4nf == 1.0) || ((BSIM4nf > 1.0) && (BSIM4sd > 0.0))))
                    {
                        if (model.BSIM4lodk2 <= 0.0)
                        {
                            sw.WriteLine($"Warning: LODK2 = {model.BSIM4lodk2} is not positive.");
                            CircuitWarning.Warning(this, $"Warning: LODK2 = {model.BSIM4lodk2} is not positive.");
                        }
                        if (model.BSIM4lodeta0 <= 0.0)
                        {
                            sw.WriteLine($"Warning: LODETA0 = {model.BSIM4lodeta0} is not positive.");

                            CircuitWarning.Warning(this, $"Warning: LODETA0 = {model.BSIM4lodeta0} is not positive.");
                        }
                    }

                    /* Check gate resistance parameters */
                    if (BSIM4rgateMod == 1)
                    {
                        if (model.BSIM4rshg <= 0.0)

                            CircuitWarning.Warning(this, "Warning: rshg should be positive for rgateMod = 1.");
                    }
                    else if (BSIM4rgateMod == 2)
                    {
                        if (model.BSIM4rshg <= 0.0)
                            CircuitWarning.Warning(this, "Warning: rshg <= 0.0 for rgateMod = 2.");
                        else if (pParam.BSIM4xrcrg1 <= 0.0)
                            CircuitWarning.Warning(this, "Warning: xrcrg1 <= 0.0 for rgateMod = 2.");
                    }
                    if (BSIM4rgateMod == 3)
                    {
                        if (model.BSIM4rshg <= 0.0)
                            CircuitWarning.Warning(this, "Warning: rshg should be positive for rgateMod = 3.");
                        else if (pParam.BSIM4xrcrg1 <= 0.0)
                            CircuitWarning.Warning(this, "Warning: xrcrg1 should be positive for rgateMod = 3.");
                    }

                    /* Check body resistance parameters */

                    if (model.BSIM4rbps0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBPS0 = {model.BSIM4rbps0 } is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBPS0 = {model.BSIM4rbps0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbpd0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBPD0 = {model.BSIM4rbpd0 } is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBPD0 = {model.BSIM4rbpd0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbpbx0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBPBX0 = {model.BSIM4rbpbx0} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBPBX0 = {model.BSIM4rbpbx0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbpby0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBPBY0 = {model.BSIM4rbpby0} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBPBY0 = {model.BSIM4rbpby0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbdbx0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBDBX0 = {model.BSIM4rbdbx0} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBDBX0 = {model.BSIM4rbdbx0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbdby0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBDBY0 = {model.BSIM4rbdby0} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBDBY0 = {model.BSIM4rbdby0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbsbx0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBSBX0 = {model.BSIM4rbsbx0} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBSBX0 = {model.BSIM4rbsbx0} is not positive.");
                        Fatal_Flag = true;
                    }
                    if (model.BSIM4rbsby0 <= 0.0)
                    {
                        sw.WriteLine($"Fatal: RBSBY0 = {model.BSIM4rbsby0} is not positive.");

                        CircuitWarning.Warning(this, $"Fatal: RBSBY0 = {model.BSIM4rbsby0} is not positive.");
                        Fatal_Flag = true;
                    }

                    /* Check capacitance parameters */
                    if (pParam.BSIM4noff < 0.1)
                    {
                        sw.WriteLine($"Warning: Noff = {pParam.BSIM4noff} is too small.");
                        CircuitWarning.Warning(this, $"Warning: Noff = {pParam.BSIM4noff} is too small.");
                    }

                    if (pParam.BSIM4voffcv < -0.5)
                    {
                        sw.WriteLine($"Warning: Voffcv = {pParam.BSIM4voffcv} is too small.");
                        CircuitWarning.Warning(this, $"Warning: Voffcv = {pParam.BSIM4voffcv} is too small.");
                    }
                    if (pParam.BSIM4moin < 5.0)
                    {
                        sw.WriteLine($"Warning: Moin = {pParam.BSIM4moin} is too small.");
                        CircuitWarning.Warning(this, $"Warning: Moin = {pParam.BSIM4moin} is too small.");
                    }
                    if (pParam.BSIM4moin > 25.0)
                    {
                        sw.WriteLine($"Warning: Moin = {pParam.BSIM4moin} is too large.");
                        CircuitWarning.Warning(this, $"Warning: Moin = {pParam.BSIM4moin} is too large.");
                    }
                    if (model.BSIM4capMod.Value == 2)
                    {
                        if (pParam.BSIM4acde < 0.1)
                        {
                            sw.WriteLine($"Warning:  Acde = {pParam.BSIM4acde} is too small.");

                            CircuitWarning.Warning(this, $"Warning: Acde = {pParam.BSIM4acde} is too small.");
                        }
                        if (pParam.BSIM4acde > 1.6)
                        {
                            sw.WriteLine($"Warning:  Acde = {pParam.BSIM4acde} is too large.");

                            CircuitWarning.Warning(this, $"Warning: Acde = {pParam.BSIM4acde} is too large.");
                        }
                    }

                    /* Check overlap capacitance parameters */
                    if (model.BSIM4cgdo < 0.0)
                    {
                        sw.WriteLine($"Warning: cgdo = {model.BSIM4cgdo} is negative. Set to zero.");

                        CircuitWarning.Warning(this, $"Warning: cgdo = {model.BSIM4cgdo} is negative. Set to zero.");
                        model.BSIM4cgdo.Value = 0.0;
                    }
                    if (model.BSIM4cgso < 0.0)
                    {
                        sw.WriteLine($"Warning: cgso = {model.BSIM4cgso} is negative. Set to zero.");

                        CircuitWarning.Warning(this, $"Warning: cgso = {model.BSIM4cgso} is negative. Set to zero.");
                        model.BSIM4cgso.Value = 0.0;
                    }
                    if (model.BSIM4cgbo < 0.0)
                    {
                        sw.WriteLine($"Warning: cgbo = {model.BSIM4cgbo} is negative. Set to zero.");

                        CircuitWarning.Warning(this, $"Warning: cgbo = {model.BSIM4cgbo} is negative. Set to zero.");
                        model.BSIM4cgbo.Value = 0.0;
                    }

                    /* v4.7 */
                    if (model.BSIM4tnoiMod.Value == 1 || model.BSIM4tnoiMod.Value == 2)
                    {
                        if (model.BSIM4tnoia < 0.0)
                        {

                            sw.WriteLine($"Warning: tnoia = {model.BSIM4tnoia} is negative. Set to zero.");

                            CircuitWarning.Warning(this, $"Warning: tnoia = {model.BSIM4tnoia} is negative. Set to zero.");
                            model.BSIM4tnoia.Value = 0.0;
                        }
                        if (model.BSIM4tnoib < 0.0)
                        {

                            sw.WriteLine($"Warning: tnoib = {model.BSIM4tnoib} is negative. Set to zero.");

                            CircuitWarning.Warning(this, $"Warning: tnoib = {model.BSIM4tnoib} is negative. Set to zero.");
                            model.BSIM4tnoib.Value = 0.0;
                        }
                        if (model.BSIM4rnoia < 0.0)
                        {

                            sw.WriteLine($"Warning: rnoia = {model.BSIM4rnoia} is negative. Set to zero.");

                            CircuitWarning.Warning(this, $"Warning: rnoia = {model.BSIM4rnoia} is negative. Set to zero.");
                            model.BSIM4rnoia.Value = 0.0;
                        }
                        if (model.BSIM4rnoib < 0.0)
                        {

                            sw.WriteLine($"Warning: rnoib = {model.BSIM4rnoib} is negative. Set to zero.");

                            CircuitWarning.Warning(this, $"Warning: rnoib = {model.BSIM4rnoib} is negative. Set to zero.");
                            model.BSIM4rnoib.Value = 0.0;
                        }
                    }

                    /* v4.7 */
                    if (model.BSIM4tnoiMod.Value == 2)
                    {
                        if (model.BSIM4tnoic < 0.0)
                        {

                            sw.WriteLine($"Warning: tnoic = {model.BSIM4tnoic} is negative. Set to zero.");

                            CircuitWarning.Warning(this, $"Warning: tnoic = {model.BSIM4tnoic} is negative. Set to zero.");
                            model.BSIM4tnoic.Value = 0.0;
                        }
                        if (model.BSIM4rnoic < 0.0)
                        {

                            sw.WriteLine($"Warning: rnoic = {model.BSIM4rnoic} is negative. Set to zero.");

                            CircuitWarning.Warning(this, $"Warning: rnoic = {model.BSIM4rnoic} is negative. Set to zero.");
                            model.BSIM4rnoic.Value = 0.0;
                        }
                    }

                    /* Limits of Njs and Njd modified in BSIM4v80.7 */
                    if (model.BSIM4SjctEmissionCoeff < 0.1)
                    {

                        sw.WriteLine($"Warning: Njs = {model.BSIM4SjctEmissionCoeff} is less than 0.1. Setting Njs to 0.1.");

                        CircuitWarning.Warning(this, $"Warning: Njs = {model.BSIM4SjctEmissionCoeff} is less than 0.1. Setting Njs to 0.1.");
                        model.BSIM4SjctEmissionCoeff.Value = 0.1;
                    }
                    else if (model.BSIM4SjctEmissionCoeff < 0.7)
                    {

                        sw.WriteLine($"Warning: Njs = {model.BSIM4SjctEmissionCoeff} is less than 0.7.");

                        CircuitWarning.Warning(this, $"Warning: Njs = {model.BSIM4SjctEmissionCoeff} is less than 0.7.");
                    }
                    if (model.BSIM4DjctEmissionCoeff < 0.1)
                    {

                        sw.WriteLine($"Warning: Njd = {model.BSIM4DjctEmissionCoeff} is less than 0.1. Setting Njd to 0.1.");

                        CircuitWarning.Warning(this, $"Warning: Njd = {model.BSIM4DjctEmissionCoeff} is less than 0.1. Setting Njd to 0.1.");
                        model.BSIM4DjctEmissionCoeff.Value = 0.1;
                    }
                    else if (model.BSIM4DjctEmissionCoeff < 0.7)
                    {

                        sw.WriteLine($"Warning: Njd = {model.BSIM4DjctEmissionCoeff} is less than 0.7.");

                        CircuitWarning.Warning(this, $"Warning: Njd = {model.BSIM4DjctEmissionCoeff} is less than 0.7.");
                    }

                    if (model.BSIM4njtsstemp < 0.0)
                    {
                        sw.WriteLine($"Warning: Njts = {model.BSIM4njtsstemp} is negative at temperature = {ckt.State.Temperature}.");

                        CircuitWarning.Warning(this, $"Warning: Njts = {model.BSIM4njtsstemp} is negative at temperature = {ckt.State.Temperature}.");
                    }
                    if (model.BSIM4njtsswstemp < 0.0)
                    {
                        sw.WriteLine($"Warning: Njtssw = {model.BSIM4njtsswstemp} is negative at temperature = {ckt.State.Temperature}.");

                        CircuitWarning.Warning(this, $"Warning: Njtssw = {model.BSIM4njtsswstemp} is negative at temperature = {ckt.State.Temperature}.");
                    }
                    if (model.BSIM4njtsswgstemp < 0.0)
                    {
                        sw.WriteLine($"Warning: Njtsswg = {model.BSIM4njtsswgstemp} is negative at temperature = {ckt.State.Temperature}.");

                        CircuitWarning.Warning(this, $"Warning: Njtsswg = {model.BSIM4njtsswgstemp} is negative at temperature = {ckt.State.Temperature}.");
                    }

                    if (model.BSIM4njtsd.Given && model.BSIM4njtsdtemp < 0.0)
                    {
                        sw.WriteLine($"Warning: Njtsd = {model.BSIM4njtsdtemp} is negative at temperature = {ckt.State.Temperature}.");

                        CircuitWarning.Warning(this, $"Warning: Njtsd = {model.BSIM4njtsdtemp} is negative at temperature = {ckt.State.Temperature}.");
                    }
                    if (model.BSIM4njtsswd.Given && model.BSIM4njtsswdtemp < 0.0)
                    {
                        sw.WriteLine($"Warning: Njtsswd = {model.BSIM4njtsswdtemp} is negative at temperature = {ckt.State.Temperature}.");

                        CircuitWarning.Warning(this, $"Warning: Njtsswd = {model.BSIM4njtsswdtemp} is negative at temperature = {ckt.State.Temperature}.");
                    }
                    if (model.BSIM4njtsswgd.Given && model.BSIM4njtsswgdtemp < 0.0)
                    {
                        sw.WriteLine($"Warning: Njtsswgd = {model.BSIM4njtsswgdtemp} is negative at temperature = {ckt.State.Temperature}.");

                        CircuitWarning.Warning(this, $"Warning: Njtsswgd = {model.BSIM4njtsswgdtemp} is negative at temperature = {ckt.State.Temperature}.");
                    }

                    if (model.BSIM4ntnoi < 0.0)
                    {
                        sw.WriteLine($"Warning: ntnoi = {model.BSIM4ntnoi} is negative. Set to zero.");
                        CircuitWarning.Warning(this, $"Warning: ntnoi = {model.BSIM4ntnoi} is negative. Set to zero.");
                        model.BSIM4ntnoi.Value = 0.0;
                    }

                    /* diode model */
                    if (model.BSIM4SbulkJctBotGradingCoeff >= 0.99)
                    {
                        sw.WriteLine($"Warning: MJS = {model.BSIM4SbulkJctBotGradingCoeff} is too big. Set to 0.99.");
                        CircuitWarning.Warning(this, $"Warning: MJS = {model.BSIM4SbulkJctBotGradingCoeff} is too big. Set to 0.99.");
                        model.BSIM4SbulkJctBotGradingCoeff.Value = 0.99;
                    }
                    if (model.BSIM4SbulkJctSideGradingCoeff >= 0.99)
                    {
                        sw.WriteLine($"Warning: MJSWS = {model.BSIM4SbulkJctSideGradingCoeff} is too big. Set to 0.99.");
                        CircuitWarning.Warning(this, $"Warning: MJSWS = {model.BSIM4SbulkJctSideGradingCoeff} is too big. Set to 0.99.");
                        model.BSIM4SbulkJctSideGradingCoeff.Value = 0.99;
                    }
                    if (model.BSIM4SbulkJctGateSideGradingCoeff >= 0.99)
                    {
                        sw.WriteLine($"Warning: MJSWGS = {model.BSIM4SbulkJctGateSideGradingCoeff} is too big. Set to 0.99.");
                        CircuitWarning.Warning(this, $"Warning: MJSWGS = {model.BSIM4SbulkJctGateSideGradingCoeff} is too big. Set to 0.99.");
                        model.BSIM4SbulkJctGateSideGradingCoeff.Value = 0.99;
                    }

                    if (model.BSIM4DbulkJctBotGradingCoeff >= 0.99)
                    {
                        sw.WriteLine($"Warning: MJD = {model.BSIM4DbulkJctBotGradingCoeff} is too big. Set to 0.99.");
                        CircuitWarning.Warning(this, $"Warning: MJD = {model.BSIM4DbulkJctBotGradingCoeff} is too big. Set to 0.99.");
                        model.BSIM4DbulkJctBotGradingCoeff.Value = 0.99;
                    }
                    if (model.BSIM4DbulkJctSideGradingCoeff >= 0.99)
                    {
                        sw.WriteLine($"Warning: MJSWD = {model.BSIM4DbulkJctSideGradingCoeff} is too big. Set to 0.99.");
                        CircuitWarning.Warning(this, $"Warning: MJSWD = {model.BSIM4DbulkJctSideGradingCoeff} is too big. Set to 0.99.");
                        model.BSIM4DbulkJctSideGradingCoeff.Value = 0.99;
                    }
                    if (model.BSIM4DbulkJctGateSideGradingCoeff >= 0.99)
                    {
                        sw.WriteLine($"Warning: MJSWGD = {model.BSIM4DbulkJctGateSideGradingCoeff} is too big. Set to 0.99.");
                        CircuitWarning.Warning(this, $"Warning: MJSWGD = {model.BSIM4DbulkJctGateSideGradingCoeff} is too big. Set to 0.99.");
                        model.BSIM4DbulkJctGateSideGradingCoeff.Value = 0.99;
                    }
                    if (model.BSIM4wpemod.Value == 1)
                    {
                        if (model.BSIM4scref <= 0.0)
                        {
                            sw.WriteLine($"Warning: SCREF = {model.BSIM4scref} is not positive. Set to 1e-6.");

                            CircuitWarning.Warning(this, $"Warning: SCREF = {model.BSIM4scref} is not positive. Set to 1e-6.");
                            model.BSIM4scref.Value = 1e-6;
                        }
                    }
                }/* loop for the parameter check for warning messages */
            }
            return Fatal_Flag;
        }

        /// <summary>
        /// BSIM4polyDepletion
        /// </summary>
        private bool BSIM4polyDepletion(double phi, double ngate, double epsgate, double coxe, double Vgs,
            out double Vgs_eff, out double dVgs_eff_dVg)
        {
            double T1, T2, T3, T4, T5, T6, T7, T8;

            /* Poly Gate Si Depletion Effect */
            if (ngate > 1.0e18 && ngate < 1.0e25 && Vgs > phi && epsgate != 0)
            {
                T1 = 1.0e6 * Circuit.CHARGE * epsgate * ngate / (coxe * coxe);
                T8 = Vgs - phi;
                T4 = Math.Sqrt(1.0 + 2.0 * T8 / T1);
                T2 = 2.0 * T8 / (T4 + 1.0);
                T3 = 0.5 * T2 * T2 / T1; /* T3 = Vpoly */
                T7 = 1.12 - T3 - 0.05;
                T6 = Math.Sqrt(T7 * T7 + 0.224);
                T5 = 1.12 - 0.5 * (T7 + T6);
                Vgs_eff = Vgs - T5;
                dVgs_eff_dVg = 1.0 - (0.5 - 0.5 / T4) * (1.0 + T7 / T6);
            }
            else
            {
                Vgs_eff = Vgs;
                dVgs_eff_dVg = 1.0;
            }
            return true;
        }
    }
}
