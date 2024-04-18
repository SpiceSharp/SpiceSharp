using SpiceSharp.Simulations;
using SpiceSharp.Simulations.IntegrationMethods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharp.Components
{
    public partial class Pwl
    {
        /// <summary>
        /// An instance of the <see cref="IWaveform"/> interface for a <see cref="Pwl"/>.
        /// </summary>
        /// <seealso cref="IWaveform"/>
        protected class Instance : IWaveform
        {
            private readonly Point[] _points;
            private readonly IIntegrationMethod _method;
            private Line _line;
            private int _index;

            /// <summary>
            /// A description of a line segment.
            /// </summary>
            protected class Line
            {
                private readonly double _m, _q;

                /// <summary>
                /// Initializes a new instance of the <see cref="Line"/> class.
                /// </summary>
                /// <param name="x1">The x-coordinate of the first point.</param>
                /// <param name="y1">The y-coordinate of the first point.</param>
                /// <param name="x2">The x-coordinate of the second point.</param>
                /// <param name="y2">The y-coordinate of the second point.</param>
                public Line(double x1, double y1, double x2, double y2)
                {
                    _m = (y2 - y1) / (x2 - x1);
                    if (_m.Equals(0.0))
                        _q = y1;
                    else
                        _q = y1 - (_m * x1);
                }

                /// <summary>
                /// Interpolates the line at the specified x-coordinate.
                /// </summary>
                /// <param name="x">The x-coordinate.</param>
                /// <returns>The value at the specified coordinate.</returns>
                public double At(double x)
                    => _m * x + _q;
            }

            /// <inheritdoc/>
            public double Value { get; private set; }

            /// <summary>
            /// Initializes a new instance of the <see cref="Instance"/> class.
            /// </summary>
            /// <param name="points">The points.</param>
            /// <param name="method">The integration method.</param>
            /// <exception cref="ArgumentException">Thrown if no points are specified, or if the time values are not monotonically increasing.</exception>
            public Instance(IEnumerable<Point> points, IIntegrationMethod method)
            {
                _method = method;
                _points = points.ThrowIfNull(nameof(points)).ToArray();
                if (_points.Length == 0)
                    throw new ArgumentException(Properties.Resources.Waveforms_Pwl_Empty);

                // Check monotonically increasing timepoints
                for (int i = 1; i < _points.Length; i++)
                {
                    if (_points[i - 1].Time >= _points[i].Time)
                        throw new ArgumentException(Properties.Resources.Waveforms_Pwl_NoIncreasingTimeValues);
                }
                _index = 0;

                Probe();
            }

            /// <inheritdoc/>
            public void Probe()
            {
                double time = _method?.Time ?? 0.0;

                // Find the line segment
                // The line segment is likely to be very close to the current segment.
                while (_index > 1 && _points[_index - 1].Time > time)
                {
                    _line = null;
                    _index--;
                }
                while (_index < _points.Length && time >= _points[_index].Time)
                {
                    _line = null;
                    _index++;
                }
                if (_line == null)
                {
                    if (_index == 0)
                        _line = new Line(
                            double.NegativeInfinity, _points[0].Value,
                            _points[0].Time, _points[0].Value);
                    else if (_index >= _points.Length)
                    {
                        _line = new Line(
                            double.NegativeInfinity, _points[_points.Length - 1].Value,
                            double.PositiveInfinity, _points[_points.Length - 1].Value
                            );
                    }
                    else if (time > _points[_index].Time)
                        _line = new Line(
                            _points[_index].Time, _points[_index].Value,
                            double.PositiveInfinity, _points[_index].Value);
                    else
                        _line = new Line(
                            _points[_index - 1].Time, _points[_index - 1].Value,
                            _points[_index].Time, _points[_index].Value);
                }

                Value = _line.At(time);
            }

            /// <inheritdoc/>
            public void Accept()
            {
                if (_method is IBreakpointMethod breakpoints)
                {
                    if (breakpoints.Break)
                    {
                        // Add the next point as a breakpoint
                        if (_index < _points.Length && _points[_index].Time > _method.Time)
                            breakpoints.Breakpoints.SetBreakpoint(_points[_index].Time);
                    }
                }
            }
        }
    }
}
