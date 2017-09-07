using System;
using System.Collections.Generic;

namespace SpiceSharp.IntegrationMethods
{
    /// <summary>
    /// A collection of breakpoints used for time-domain analysis.
    /// </summary>
    public class Breakpoints
    {
        /// <summary>
        /// The minimum space between two breakpoints
        /// </summary>
        public double MinBreak { get; set; } = 0.0;

        /// <summary>
        /// Gets the first breakpoint
        /// </summary>
        public double First { get; private set; } = 0.0;

        /// <summary>
        /// Gets the delta set by the first two breakpoints
        /// </summary>
        public double Delta { get; private set; } = double.PositiveInfinity;

        /// <summary>
        /// Private variables
        /// </summary>
        private List<double> bps = new List<double>() { 0.0, double.PositiveInfinity };

        /// <summary>
        /// Constructor
        /// </summary>
        public Breakpoints() { }

        /// <summary>
        /// Wrapper for CKTsetBreak in cktsetbk.c
        /// Add a breakpoint to the list
        /// </summary>
        /// <param name="bp"></param>
        public void SetBreakpoint(double bp)
        {
            // Insert
            for (int i = 0; i < bps.Count; i++)
            {
                // Same breakpoint, return without setting it
                if (Math.Abs(bps[i] - bp) <= MinBreak)
                    return;

                // Check if we need to insert the breakpoint here
                if (bp < bps[i])
                {
                    bps.Insert(i, bp);
                    if (i == 0)
                    {
                        First = bp;
                        Delta = First - bp;
                        return;
                    }
                    else
                    {
                        First = bps[0];
                        Delta = bps[1] - First;
                        return;
                    }
                }
            }

            // Since we got here, it just needs to be added to the end
            bps.Add(bp);
        }

        /// <summary>
        /// Clear breakpoint
        /// </summary>
        public void ClearBreakpoint()
        {
            // Remove the first item
            if (bps.Count > 2)
                bps.RemoveAt(0);
            else
            {
                bps[0] = bps[1];
                bps[1] = double.PositiveInfinity;
            }

            // Calculate the first breakpoint and the maximum delta
            First = bps[0];
            Delta = bps[1] - First;
        }

        /// <summary>
        /// Clear all breakpoints
        /// </summary>
        public void Clear()
        {
            bps.Clear();
            bps.Add(0.0);
            bps.Add(double.PositiveInfinity);
            First = 0.0;
            Delta = double.PositiveInfinity;
        }
    }
}
