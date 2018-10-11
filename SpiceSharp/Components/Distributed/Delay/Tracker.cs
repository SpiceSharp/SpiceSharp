using System;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.DelayBehaviors
{
    /// <summary>
    /// This class will track the value
    /// </summary>
    public class Tracker
    {
        private class Node
        {
            public double Time { get; set; }
            public double Value { get; set; }
            public Node Newer { get; set; }
            public Node Older { get; set; }
        }

        private Node _probe;
        private Node _oldest;
        private Node _reference;
        private readonly int _pos;
        private readonly double _delay;
        private double _relTol = 0.5, _absTol = 0.0;
        private bool _wasBreak;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tracker"/> class.
        /// </summary>
        /// <param name="positive">The positive.</param>
        /// <param name="delay">The delay.</param>
        public Tracker(int positive, double delay)
        {
            _delay = delay;
            _pos = positive;

            // Initialize our linked list with a base element at t=0 and one for probing
            _probe = new Node
            {
                Older = _oldest
            };
            _oldest = _probe;
            _reference = _oldest;
        }

        /// <summary>
        /// Updates the current probe
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void Probe(TimeSimulation simulation)
        {
            _probe.Value = simulation.RealState.Solution[_pos];
            _probe.Time = simulation.Method.Time;
        }

        /// <summary>
        /// Gets delayed value.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        /// <returns></returns>
        public double GetValue(TimeSimulation simulation)
        {
            var nowTime = simulation.Method.Time;
            var refTime = nowTime - _delay;
            
            // Find the points left and right for interpolation
            if (_reference.Time < refTime)
            {
                // Move forward if the next point is still before the lookup time
                while (_reference.Newer.Time < refTime)
                    _reference = _reference.Newer ?? throw new CircuitException("Non-causal system!");
            }
            else
            {
                // Move backward if the previous point is still after the lookup time
                while (_reference.Older != null && _reference.Time > refTime)
                    _reference = _reference.Older;
                if (_reference.Time > refTime)
                    return _reference.Value;
            }

            var tl = _reference.Time;
            var vl = _reference.Value;
            var tr = _reference.Newer.Time;
            var vr = _reference.Newer.Value;

            // Return the interpolated value
            return vl + (refTime - tl) / (tr - tl) * (vr - vl);
        }

        /// <summary>
        /// Accepts the solution.
        /// </summary>
        /// <param name="simulation">The simulation.</param>
        public void AcceptSolution(TimeSimulation simulation)
        {
            // If the simulation supports breakpoints, then add one if we have an inflection point at the input
            if (_probe != _oldest)
            {
                if (simulation.Method is IBreakpoints bpm)
                {
                    if (_wasBreak)
                    {
                        var center = _probe.Older;
                        var before = center.Older;

                        // Check if the inflection point crosses the threshold
                        var d1 = (_probe.Value - center.Value) / (_probe.Time - center.Time);
                        var d2 = (center.Value - before?.Value) / (center.Time - before?.Time) ?? 0.0;

                        var tol = _relTol * Math.Max(Math.Abs(d1), Math.Abs(d2)) + _absTol;
                        if (Math.Abs(d1 - d2) > tol)
                            bpm.Breakpoints.SetBreakpoint(center.Time + _delay);
                    }

                    _wasBreak = bpm.Break;
                }
            }

            var refTime = simulation.Method.Time - _delay;
            Node reuse = null;

            // Maintain the history such that we have exactly one point before the reference time
            while (_oldest.Newer != null && _oldest.Newer.Time <= refTime)
            {
                // Remove what we won't need anymore, but keep it for re-use
                reuse = _oldest;
                _oldest = _oldest.Newer;

                // Clear references
                reuse.Newer = null;
                reuse.Older = null;
            }

            // Add the accepted value
            var elt = reuse ?? new Node();
            elt.Older = _probe;
            _probe.Newer = elt;
            _probe = elt;
        }
    }
}
