using System;

namespace SpiceSharp.Components.Semiconductors
{
    /// <summary>
    /// A class with static methods for semiconductor devices
    /// </summary>
    public static class Semiconductor
    {
        /// <summary>
        /// Limit the per-iteration change of PN junction voltages
        /// Defined as DEVpnjlim in devsup.c
        /// </summary>
        /// <param name="newVoltage">New voltage</param>
        /// <param name="oldVoltage">Old voltage</param>
        /// <param name="thermalVoltage">Thermal voltage</param>
        /// <param name="criticalVoltage">Critical voltage</param>
        /// <returns></returns>
        public static double LimitJunction(double newVoltage, double oldVoltage, double thermalVoltage, double criticalVoltage, ref bool limited)
        {
            double arg;
            if (newVoltage > criticalVoltage && Math.Abs(newVoltage - oldVoltage) > thermalVoltage + thermalVoltage)
            {
                if (oldVoltage > 0)
                {
                    arg = 1 + (newVoltage - oldVoltage) / thermalVoltage;
                    if (arg > 0)
                        newVoltage = oldVoltage + thermalVoltage * Math.Log(arg);
                    else
                        newVoltage = criticalVoltage;
                }
                else
                    newVoltage = thermalVoltage * Math.Log(newVoltage / thermalVoltage);
                limited = true;
            }
            return newVoltage;
        }
    }
}
