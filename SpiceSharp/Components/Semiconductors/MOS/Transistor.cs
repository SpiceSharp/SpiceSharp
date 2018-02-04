using System;

namespace SpiceSharp.Components.MosfetBehaviors
{
    /// <summary>
    /// A class with static methods for Mosfet transistors
    /// </summary>
    public static class Transistor
    {
        /// <summary>
        /// Static variables
        /// </summary>
        public const double MaximumExponentArgument = 709.0;
        public const double EpsilonSilicon = 11.7 * 8.854214871e-12;
        public const double MaximumExponent = 5.834617425e14;
        public const double MinimumExponent = 1.713908431e-15;
        public const double ExponentThreshold = 34.0;
        public const double SmoothingFactor = 0.1;
        public const double EpsilonOxide = 3.453133e-11;
        // public static double EPSSI = 1.03594e-10; Duplicate of EpsilonSilicon
        // public static double Charge_q = 1.60219e-19; Duplicate of Circuit.Charge
        public const double MeterMicron = 1.0e6;
        // public static double Kb = 1.3806226e-23; Duplicate to Circuit.Boltzmann
        // public static double KboQ = 8.617087e-5;  /* Kb / q  where q = 1.60219e-19 */ Duplicate of Circuit.KOverQ
        public const double Delta = 1.0E-9;
        public const double Delta1 = 0.02;
        public const double Delta2 = 0.02;
        public const double Delta3 = 0.02;
        public const double Delta4 = 0.02;
        public const double Epsilon0 = 8.85418e-12;
        // public const double MM = 3; Seems to be unused

        public const double MaximumLongExponent = 2.688117142e+43;
        public const double MinimumLongExponent = 3.720075976e-44;
        public const double LongExponentThreshold = 100.0;

        /// <summary>
        /// Limiting function FET
        /// </summary>
        /// <param name="newVoltage">New voltage</param>
        /// <param name="oldVoltage">Olt voltage</param>
        /// <param name="threshold">Threshold</param>
        /// <returns></returns>
        public static double LimitFet(double newVoltage, double oldVoltage, double threshold)
        {
            double vtsthi;
            double vtstlo;
            double vtox;
            double delv;
            double vtemp;

            vtsthi = Math.Abs(2 * (oldVoltage - threshold)) + 2;
            vtstlo = vtsthi / 2 + 2;
            vtox = threshold + 3.5;
            delv = newVoltage - oldVoltage;

            if (oldVoltage >= threshold)
            {
                if (oldVoltage >= vtox)
                {
                    if (delv <= 0)
                    {
                        /* going off */
                        if (newVoltage >= vtox)
                        {
                            if (-delv > vtstlo)
                            {
                                newVoltage = oldVoltage - vtstlo;
                            }
                        }
                        else
                        {
                            newVoltage = Math.Max(newVoltage, threshold + 2);
                        }
                    }
                    else
                    {
                        /* staying on */
                        if (delv >= vtsthi)
                        {
                            newVoltage = oldVoltage + vtsthi;
                        }
                    }
                }
                else
                {
                    /* middle region */
                    if (delv <= 0)
                    {
                        /* decreasing */
                        newVoltage = Math.Max(newVoltage, threshold - .5);
                    }
                    else
                    {
                        /* increasing */
                        newVoltage = Math.Min(newVoltage, threshold + 4);
                    }
                }
            }
            else
            {
                /* off */
                if (delv <= 0)
                {
                    if (-delv > vtsthi)
                    {
                        newVoltage = oldVoltage - vtsthi;
                    }
                }
                else
                {
                    vtemp = threshold + .5;
                    if (newVoltage <= vtemp)
                    {
                        if (delv > vtstlo)
                        {
                            newVoltage = oldVoltage + vtstlo;
                        }
                    }
                    else
                    {
                        newVoltage = vtemp;
                    }
                }
            }
            return (newVoltage);
        }

        /// <summary>
        /// Limiting function PN
        /// </summary>
        /// <param name="newVoltage">New voltage</param>
        /// <param name="oldVoltage">Old voltage</param>
        /// <param name="thermalVoltage">Thermal voltage</param>
        /// <param name="criticalVoltage">Critical voltage</param>
        /// <param name="check">Checking variable</param>
        /// <returns></returns>
        public static double LimitJunction(double newVoltage, double oldVoltage, double thermalVoltage, double criticalVoltage, ref int check)
        {
            double arg;

            if ((newVoltage > criticalVoltage) && (Math.Abs(newVoltage - oldVoltage) > (thermalVoltage + thermalVoltage)))
            {
                if (oldVoltage > 0)
                {
                    arg = 1 + (newVoltage - oldVoltage) / thermalVoltage;
                    if (arg > 0)
                    {
                        newVoltage = oldVoltage + thermalVoltage * Math.Log(arg);
                    }
                    else
                    {
                        newVoltage = criticalVoltage;
                    }
                }
                else
                {
                    newVoltage = thermalVoltage * Math.Log(newVoltage / thermalVoltage);
                }
                check = 1;
            }
            else
            {
                check = 0;
            }
            return (newVoltage);
        }

        /// <summary>
        /// Limiting function VDS
        /// </summary>
        /// <param name="newVoltage">New voltage</param>
        /// <param name="oldVoltage">Old voltage</param>
        /// <returns></returns>
        public static double LimitVoltageDS(double newVoltage, double oldVoltage)
        {
            if (oldVoltage >= 3.5)
            {
                if (newVoltage > oldVoltage)
                {
                    newVoltage = Math.Min(newVoltage, (3 * oldVoltage) + 2);
                }
                else
                {
                    if (newVoltage < 3.5)
                    {
                        newVoltage = Math.Max(newVoltage, 2);
                    }
                }
            }
            else
            {
                if (newVoltage > oldVoltage)
                {
                    newVoltage = Math.Min(newVoltage, 4);
                }
                else
                {
                    newVoltage = Math.Max(newVoltage, -.5);
                }
            }
            return (newVoltage);
        }

        /// <summary>
        /// QMeyer method for calculating capacitances
        /// </summary>
        /// <param name="vgs">Gate-source voltage</param>
        /// <param name="vgd">Gate-drain voltage</param>
        /// <param name="von">Von</param>
        /// <param name="vdsat">Saturation voltage</param>
        /// <param name="capGS">Gate-source capacitance</param>
        /// <param name="capGD">Gate-drain capacitance</param>
        /// <param name="capGB">Gate-bulk capacitance</param>
        /// <param name="phi">Phi</param>
        /// <param name="cox">Cox</param>
        public static void MeyerCharges(double vgs, double vgd, double von, double vdsat, out double capGS, out double capGD, out double capGB, double phi, double cox)
        {
            double vds;
            double vddif;
            double vddif1;
            double vddif2;
            double vgst;

            vgst = vgs - von;
            if (vgst <= -phi)
            {
                capGB = cox / 2;
                capGS = 0;
                capGD = 0;
            }
            else if (vgst <= -phi / 2)
            {
                capGB = -vgst * cox / (2 * phi);
                capGS = 0;
                capGD = 0;
            }
            else if (vgst <= 0)
            {
                capGB = -vgst * cox / (2 * phi);
                capGS = vgst * cox / (1.5 * phi) + cox / 3;
                capGD = 0;
            }
            else
            {
                vds = vgs - vgd;
                if (vdsat <= vds)
                {
                    capGS = cox / 3;
                    capGD = 0;
                    capGB = 0;
                }
                else
                {
                    vddif = 2.0 * vdsat - vds;
                    vddif1 = vdsat - vds/*-1.0e-12*/;
                    vddif2 = vddif * vddif;
                    capGD = cox * (1.0 - vdsat * vdsat / vddif2) / 3;
                    capGS = cox * (1.0 - vddif1 * vddif1 / vddif2) / 3;
                    capGB = 0;
                }
            }
        }
    }
}
