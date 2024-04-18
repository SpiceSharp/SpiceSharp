using System;

namespace SpiceSharp.Components.Mosfets
{
    /// <summary>
    /// A class with static methods for Mosfet transistors.
    /// </summary>
    public static class Transistor
    {
        /// <summary>
        /// Limiting function for a FET.
        /// </summary>
        /// <remarks>
        /// Update taken from ngSpice, which was fixed by Alan Gillespie's code.
        /// </remarks>
        /// <param name="newVoltage">New voltage.</param>
        /// <param name="oldVoltage">Old voltage.</param>
        /// <param name="threshold">Threshold value.</param>
        /// <returns>The new voltage, limited if necessary.</returns>
        public static double LimitFet(double newVoltage, double oldVoltage, double threshold)
        {
            double vtstlo = Math.Abs(oldVoltage - threshold) + 1;
            double vtsthi = 2 * vtstlo + 2;
            double vtox = threshold + 3.5;
            double delv = newVoltage - oldVoltage;

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
                    double vtemp = threshold + .5;
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
        /// Limiting function for the drain-source voltage.
        /// </summary>
        /// <param name="newVoltage">The new voltage.</param>
        /// <param name="oldVoltage">The old voltage.</param>
        /// <returns>The new voltage, limited if necessary.</returns>
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
        /// QMeyer method for calculating capacitances.
        /// </summary>
        /// <param name="vgs">The gate-source voltage.</param>
        /// <param name="vgd">The gate-drain voltage.</param>
        /// <param name="von">The threshold voltage for switching on.</param>
        /// <param name="vdsat">The saturation voltage.</param>
        /// <param name="capGs">The gate-source capacitance.</param>
        /// <param name="capGd">The gate-drain capacitance.</param>
        /// <param name="capGb">The gate-bulk capacitance.</param>
        /// <param name="phi">The gate-bulk voltage.</param>
        /// <param name="cox">The oxide capacitance.</param>
        public static void MeyerCharges(double vgs, double vgd, double von, double vdsat, out double capGs, out double capGd, out double capGb, double phi, double cox)
        {
            double vgst = vgs - von;
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
                double vds = vgs - vgd;
                if (vdsat <= vds)
                {
                    capGs = cox / 3;
                    capGd = 0;
                    capGb = 0;
                }
                else
                {
                    double vddif = 2.0 * vdsat - vds;
                    double vddif1 = vdsat - vds;
                    double vddif2 = vddif * vddif;
                    capGd = cox * (1.0 - vdsat * vdsat / vddif2) / 3;
                    capGs = cox * (1.0 - vddif1 * vddif1 / vddif2) / 3;
                    capGb = 0;
                }
            }
        }
    }
}
