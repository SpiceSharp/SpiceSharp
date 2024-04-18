using System;
using System.Collections.Generic;

namespace SpiceSharp.Components.Distributed
{
    /// <summary>
    /// This class will keep track of one or more signals and can calculate the delayed version of it.
    /// </summary>
    public class DelayedSignal
    {
        /// <summary>
        /// A node used for our linked list
        /// </summary>
        private class Node
        {
            public double Time { get; set; }
            public double[] Values { get; }
            public Node Older { get; set; }
            public Node Newer { get; set; }

            public Node(int size)
            {
                Values = new double[size];
            }
        }

        // Private variables
        private Node _oldest, _probed, _reference;
        private readonly double[] _values;

        /// <summary>
        /// Gets the delay.
        /// </summary>
        public double Delay { get; }

        /// <summary>
        /// Gets the number of values stored by the delayed signal.
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Gets the current value with the specified index.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <param name="index">The index.</param>
        /// <returns>The value.</returns>
        public double this[int index] => _values[index];

        /// <summary>
        /// The derivative of the delayed signal with respect to the input.
        /// </summary>
        public double InputDerivative { get; private set; }

        /// <summary>
        /// Gets the values at the probed point.
        /// </summary>
        public IReadOnlyList<double> Values => _values;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedSignal"/> class.
        /// </summary>
        /// <param name="size">The number of elements to be stored.</param>
        /// <param name="delay">The amount of time to look back.</param>
        public DelayedSignal(int size, double delay)
        {
            Delay = delay;
            Size = size;
            _values = new double[size];
            if (delay <= 0)
                throw new ArgumentException(Properties.Resources.Delays_NonCausalDelay);

            // Setup our linked list
            _reference = _oldest = _probed = new Node(size)
            {
                Time = 0.0
            };
        }

        /// <summary>
        /// Probes the specified timepoint.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <param name="breakpoint">If <c>true</c>, interpolation will be linear. Else cubic interpolation will be used if possible.</param>
        public void Probe(double time, bool breakpoint)
        {
            double refTime = time - Delay;
            InputDerivative = 0.0;

            // Store the probe value
            _probed.Time = time;

            // Check that the time is increasing
            if (_probed.Older != null && _probed.Older.Time > time)
                throw new SpiceSharpException(
                    Properties.Resources.Delays_NonIncreasingTime.FormatString(_probed.Older.Time, time));

            // Move the reference to the closest delayed timepoint
            MoveReferenceCloseTo(refTime);

            // There is no point before the reference, assume constant
            if (refTime <= _reference.Time)
            {
                if (_reference.Older == null)
                {
                    for (int i = 0; i < Size; i++)
                        _values[i] = _reference.Values[i];
                    if (_reference == _probed)
                        InputDerivative = 1.0;
                }
                else
                {
                    double f1 = (refTime - _reference.Time) / (_reference.Older.Time - _reference.Time);
                    double f2 = (refTime - _reference.Older.Time) / (_reference.Time - _reference.Older.Time);
                    for (int i = 0; i < Size; i++)
                        _values[i] = f1 * _reference.Older.Values[i] + f2 * _reference.Values[i];
                    if (_reference == _probed)
                        InputDerivative = f2;
                }
            }
            else
            {
                // Do cubic interpolation if possible, else use linear interpolation
                if (breakpoint || _reference.Older == null)
                {
                    // Linear interpolation
                    double f1 = (refTime - _reference.Newer.Time) / (_reference.Time - _reference.Newer.Time);
                    double f2 = (refTime - _reference.Time) / (_reference.Newer.Time - _reference.Time);
                    for (int i = 0; i < Size; i++)
                        _values[i] = f1 * _reference.Values[i] + f2 * _reference.Newer.Values[i];
                    if (_reference.Newer == _probed)
                        InputDerivative = f2;
                }
                else
                {
                    // Cubic interpolation
                    double f1 = (refTime - _reference.Time) * (refTime - _reference.Newer.Time) /
                             (_reference.Older.Time - _reference.Time) /
                             (_reference.Older.Time - _reference.Newer.Time);
                    double f2 = (refTime - _reference.Older.Time) * (refTime - _reference.Newer.Time) /
                             (_reference.Time - _reference.Older.Time) / (_reference.Time - _reference.Newer.Time);
                    double f3 = (refTime - _reference.Older.Time) * (refTime - _reference.Time) /
                             (_reference.Newer.Time - _reference.Older.Time) /
                             (_reference.Newer.Time - _reference.Time);
                    for (int i = 0; i < Size; i++)
                    {
                        _values[i] = f1 * _reference.Older.Values[i] +
                                    f2 * _reference.Values[i] +
                                    f3 * _reference.Newer.Values[i];
                    }
                    if (_reference == _probed)
                        InputDerivative = f3;
                }
            }
        }

        /// <summary>
        /// Sets the probed values.
        /// </summary>
        /// <param name="values">The values.</param>
        public void SetProbedValues(params double[] values)
        {
            values.ThrowIfNotLength(nameof(values), Size);
            for (int i = 0; i < Size; i++)
                _probed.Values[i] = values[i];
        }

        /// <summary>
        /// Accepts the last probed values.
        /// </summary>
        public void AcceptProbedValues()
        {
            double refTime = _probed.Time - Delay;

            // We need at least 2 nodes before the accepted timepoint in case the timestep is truncated
            var tmp = _oldest;
            while (tmp.Newer?.Newer != null && tmp.Newer.Newer.Time < refTime)
                tmp = tmp.Newer;

            // If the tooOld variable is not our oldest node, then we can move some nodes to the front
            // We do this to save some time having to allocate nodes
            if (tmp != _oldest)
            {
                // Place the older nodes immediately after the _probed node
                tmp.Older.Newer = _probed.Newer;
                if (_probed.Newer != null)
                    _probed.Newer.Older = tmp.Older;
                tmp.Older = null;

                _oldest.Older = _probed;
                _probed.Newer = _oldest;
                _oldest = tmp;

                // It is possible that our reference needs to be shifted too
                _reference = _oldest;
            }

            // We got rid of the nodes we don't need, advance the probed node
            _probed.Newer ??= new Node(Size)
                {
                    Older = _probed
                };
            _probed = _probed.Newer;
        }

        /// <summary>
        /// Get a tracked value.
        /// </summary>
        /// <param name="back">The number of points to go back in time.</param>
        /// <param name="index">The index.</param>
        /// <returns>The value in history.</returns>
        public double GetValue(int back, int index)
        {
            switch (back)
            {
                case 0: return _probed.Values[index];
                case 1: return _probed.Older.Values[index];
                default:
                    var elt = _probed;
                    for (int i = 0; i < back; i++)
                        elt = elt?.Older;
                    return elt?.Values[index] ?? _oldest.Values[index];
            }
        }

        /// <summary>
        /// Gets a tracked timepoint.
        /// </summary>
        /// <param name="back">The number of points to go back in time.</param>
        /// <returns>The time point in history.</returns>
        public double GetTime(int back)
        {
            switch (back)
            {
                case 0: return _probed.Time;
                case 1: return _probed.Older.Time;
                default:
                    var elt = _probed;
                    for (int i = 0; i < back; i++)
                        elt = elt?.Older;
                    return elt?.Time ?? double.NegativeInfinity;
            }
        }

        /// <summary>
        /// Moves the reference such that it has a point left and right of the specified time.
        /// </summary>
        /// <param name="time">The time.</param>
        private void MoveReferenceCloseTo(double time)
        {
            var r = _reference;
            double distance = Math.Abs(r.Time - time);
            bool hasMoved = false;

            // Move forward until the middle point is closest
            while (r.Newer != null && Math.Abs(r.Newer.Time - time) < distance)
            {
                r = r.Newer;
                distance = Math.Abs(r.Time - time);
                hasMoved = true;
            }

            // Move backward if not already moved forward
            if (!hasMoved)
            {
                while (r.Older != null && Math.Abs(r.Older.Time - time) < distance)
                {
                    r = r.Older;
                    distance = Math.Abs(r.Time - time);
                }
            }

            // Update
            _reference = r;
        }

        /// <summary>
        /// Clears any memory in the delayed signal.
        /// </summary>
        public void Clear()
        {
            // Setup our linked list
            _reference = _oldest = _probed = new Node(Size)
            {
                Time = 0.0
            };

            // Clear values
            for (int i = 0; i < Size; i++)
                _values[i] = 0.0;
        }
    }
}
