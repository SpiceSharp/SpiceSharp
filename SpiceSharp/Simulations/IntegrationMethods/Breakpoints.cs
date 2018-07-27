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
        public double First { get; private set; }

        /// <summary>
        /// Gets the delta set by the first two breakpoints
        /// </summary>
        public double Delta { get; private set; } = double.PositiveInfinity;

        /// <summary>
        /// Private variables
        /// </summary>
        private readonly List<double> _bps = new List<double> { 0.0, double.PositiveInfinity };

        /// <summary>
        /// Wrapper for CKTsetBreak in cktsetbk.c
        /// Add a breakpoint to the list
        /// </summary>
        /// <param name="timePoint">Time point</param>
        public void SetBreakpoint(double timePoint)
        {
            // Insert
            for (var i = 0; i < _bps.Count; i++)
            {
                // Same breakpoint, return without setting it
                if (Math.Abs(_bps[i] - timePoint) <= MinBreak)
                    return;

                // Check if we need to insert the breakpoint here
                if (timePoint < _bps[i])
                {
                    _bps.Insert(i, timePoint);
                    if (i == 0)
                    {
                        First = timePoint;
                        Delta = First - timePoint;
                        return;
                    }

                    First = _bps[0];
                    Delta = _bps[1] - First;
                    return;
                }
            }

            // Since we got here, it just needs to be added to the end
            _bps.Add(timePoint);
        }

        /// <summary>
        /// Clear breakpoint
        /// </summary>
        public void ClearBreakpoint()
        {
            // Remove the first item
            if (_bps.Count > 2)
                _bps.RemoveAt(0);
            else
            {
                _bps[0] = _bps[1];
                _bps[1] = double.PositiveInfinity;
            }

            // Calculate the first breakpoint and the maximum delta
            First = _bps[0];
            Delta = _bps[1] - First;
        }

        /// <summary>
        /// Clear all breakpoints
        /// </summary>
        public void Clear()
        {
            _bps.Clear();
            _bps.Add(0.0);
            _bps.Add(double.PositiveInfinity);
            First = 0.0;
            Delta = double.PositiveInfinity;
        }
    }
}
