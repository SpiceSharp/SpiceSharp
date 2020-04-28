﻿using System;

namespace SpiceSharp.Components.MosfetBehaviors
{
    /// <summary>
    /// A class with static methods for Mosfet transistors
    /// </summary>
    public static class Transistor
    {
        /// <summary>
        /// Limiting function FET
        /// </summary>
        /// <remarks>
        /// Update taken from ngSpice, which was fixed by Alan Gillespie's code.
        /// </remarks>
        /// <param name="newVoltage">New voltage</param>
        /// <param name="oldVoltage">Olt voltage</param>
        /// <param name="threshold">Threshold</param>
        /// <returns></returns>
        public static double LimitFet(double newVoltage, double oldVoltage, double threshold)
        {
            var vtstlo = Math.Abs(oldVoltage - threshold) + 1;
            var vtsthi = 2 * vtstlo + 2;
            var vtox = threshold + 3.5;
            var delv = newVoltage - oldVoltage;

            if (oldVoltage >= threshold)
            {
                if (oldVoltage >= vtox)
                {
                    if (delv <= 0)
                    {
                        // going off
                        if (newVoltage >= vtox)
                        {
                            if (-delv > vtstlo)
                                newVoltage = oldVoltage - vtstlo;
                        }
                        else
                            newVoltage = Math.Max(newVoltage, threshold + 2);
                    }
                    else
                    {
                        // staying on
                        if (delv >= vtsthi)
                            newVoltage = oldVoltage + vtsthi;
                    }
                }
                else
                {
                    // middle region
                    newVoltage = delv <= 0 ? Math.Max(newVoltage, threshold - 0.5) : Math.Min(newVoltage, threshold + 4);
                }
            }
            else
            {
                // off
                if (delv <= 0)
                {
                    if (-delv > vtsthi)
                    {
                        newVoltage = oldVoltage - vtsthi;
                    }
                }
                else
                {
                    var vtemp = threshold + .5;
                    if (newVoltage <= vtemp)
                    {
                        if (delv > vtstlo)
                            newVoltage = oldVoltage + vtstlo;
                    }
                    else
                        newVoltage = vtemp;
                }
            }
            return newVoltage;
        }

        /// <summary>
        /// Limiting function VDS
        /// </summary>
        /// <param name="newVoltage">New voltage</param>
        /// <param name="oldVoltage">Old voltage</param>
        /// <returns>The limited vds</returns>
        public static double LimitVds(double newVoltage, double oldVoltage)
        {
            if (oldVoltage >= 3.5)
            {
                if (newVoltage > oldVoltage)
                    newVoltage = Math.Min(newVoltage, 3 * oldVoltage + 2);
                else if (newVoltage < 3.5)
                    newVoltage = Math.Max(newVoltage, 2);
            }
            else
                newVoltage = newVoltage > oldVoltage ? Math.Min(newVoltage, 4) : Math.Max(newVoltage, -.5);
            return newVoltage;
        }

        /// <summary>
        /// QMeyer method for calculating capacitances
        /// </summary>
        /// <param name="vgs">Gate-source voltage</param>
        /// <param name="vgd">Gate-drain voltage</param>
        /// <param name="von">Von</param>
        /// <param name="vdsat">Saturation voltage</param>
        /// <param name="capGs">Gate-source capacitance</param>
        /// <param name="capGd">Gate-drain capacitance</param>
        /// <param name="capGb">Gate-bulk capacitance</param>
        /// <param name="phi">Phi</param>
        /// <param name="cox">Cox</param>
        public static void MeyerCharges(double vgs, double vgd, double von, double vdsat, out double capGs, out double capGd, out double capGb, double phi, double cox)
        {
            var vgst = vgs - von;
            if (vgst <= -phi)
            {
                capGb = cox / 2;
                capGs = 0;
                capGd = 0;
            }
            else if (vgst <= -phi / 2)
            {
                capGb = -vgst * cox / (2 * phi);
                capGs = 0;
                capGd = 0;
            }
            else if (vgst <= 0)
            {
                capGb = -vgst * cox / (2 * phi);
                capGs = vgst * cox / (1.5 * phi) + cox / 3;
                capGd = 0;
            }
            else
            {
                var vds = vgs - vgd;
                if (vdsat <= vds)
                {
                    capGs = cox / 3;
                    capGd = 0;
                    capGb = 0;
                }
                else
                {
                    var vddif = 2.0 * vdsat - vds;
                    var vddif1 = vdsat - vds;
                    var vddif2 = vddif * vddif;
                    capGd = cox * (1.0 - vdsat * vdsat / vddif2) / 3;
                    capGs = cox * (1.0 - vddif1 * vddif1 / vddif2) / 3;
                    capGb = 0;
                }
            }
        }
    }
}
