using System;

namespace SpiceSharp.Components.Semiconductors
{
    /// <summary>
    /// A class with helper methods for semiconductor devices.
    /// </summary>
    public static class Semiconductor
    {
        /// <summary>
        /// Limit the per-iteration change of PN junction voltages
        /// Defined as DEVpnjlim in devsup.c
        /// </summary>
        /// <remarks>
        /// Taken from ngSpice, where it was fixed by Alan Gillespie's code.
        /// </remarks>
        /// <param name="newVoltage">The target voltage.</param>
        /// <param name="oldVoltage">The current voltage.</param>
        /// <param name="thermalVoltage">The thermal voltage.</param>
        /// <param name="criticalVoltage">The critical voltage.</param>
        /// <param name="limited">If <c>true</c>, the value was limited.</param>
        /// <returns>The new voltage value, limited if necessary.</returns>
        public static double LimitJunction(double newVoltage, double oldVoltage, double thermalVoltage, double criticalVoltage, ref bool limited)
        {
            if (newVoltage > criticalVoltage && Math.Abs(newVoltage - oldVoltage) > thermalVoltage + thermalVoltage)
            {
                if (oldVoltage > 0)
                {
                    double arg = (newVoltage - oldVoltage) / thermalVoltage;
                    if (arg > 0)
                        newVoltage = oldVoltage + thermalVoltage * (2 + Math.Log(arg - 2));
                    else
                        newVoltage = oldVoltage - thermalVoltage * (2 + Math.Log(2 - arg));
                }
                else
                    newVoltage = thermalVoltage * Math.Log(newVoltage / thermalVoltage);
                limited = true;
            }
            else
            {
                if (newVoltage < 0)
                {
                    double arg;
                    if (oldVoltage > 0)
                        arg = -oldVoltage - 1;
                    else
                        arg = 2 * oldVoltage - 1;

                    if (newVoltage < arg)
                    {
                        newVoltage = arg;
                        limited = true;
                    }
                }
            }
            return newVoltage;
        }
    }
}
