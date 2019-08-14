using System;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Waveforms
{
    /// <summary>
    /// Piecewise linear waveform.
    /// </summary>
    public class Pwl : Waveform
    {
        /// <summary>
        /// A definition of a line
        /// </summary>
        protected class LineDefinition
        {
            /// <summary>
            /// Gets or sets the slope of the line.
            /// </summary>
            public double A { get; set; }

            /// <summary>
            /// Gets or sets the intercept of the line.
            /// </summary>
            public double B { get; set; }
        }

        // Private variables
        private LineDefinition _lineDefinition;
        private bool _breakPointAdded;
        private long _currentLineIndex;
        private readonly long _pwlPoints;

        /// <summary>
        /// Creates a new instance of the <see cref="Pwl"/> class.
        /// </summary>
        /// <param name="times">Array of times.</param>
        /// <param name="voltages">Array of voltages.</param>
        public Pwl(double[] times, double[] voltages)
        {
            Times = times.ThrowIfEmpty(nameof(times));
            Voltages = voltages.ThrowIfNot(nameof(voltages), times.Length);

            _pwlPoints = Times.Length;
            for (var i = 1; i < _pwlPoints; i++)
            {
                if (Times[i-1] >= Times[i])
                    throw new ArgumentException("PWL - times array should contain monotonously increasing time points");
            }
        }

        /// <summary>
        /// Array of times.
        /// </summary>
        public double[] Times { get; }

        /// <summary>
        /// Array of voltages.
        /// </summary>
        public double[] Voltages { get; }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));

            if (simulation.Method.Time.Equals(0.0))
                Value = GetLineValue(0.0);

            if (simulation.Method is IBreakpoints method)
            {
                var breaks = method.Breakpoints;

                if (!_breakPointAdded && _currentLineIndex < _pwlPoints)
                {
                    double breakPointTime = Times[_currentLineIndex];
                    if (breakPointTime >= 0.0)
                    {
                        breaks.SetBreakpoint(breakPointTime);
                    }
                    _breakPointAdded = true;
                }
            }
        }

        /// <summary>
        /// Indicates a new timepoint is being probed.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Probe(TimeSimulation simulation)
        {
            simulation.ThrowIfNull(nameof(simulation));
            var time = simulation.Method.Time;
            Value = GetLineValue(time);
        }

        /// <summary>
        /// Resets the waveform.
        /// </summary>
        public void Reset()
        {
            _breakPointAdded = false;
            _currentLineIndex = 0;
        }

        /// <summary>
        /// Sets up the waveform.
        /// </summary>
        public override void Setup()
        {
            // Set value for time = 0.0
            Value = GetLineValue(0.0);
        }

        /// <summary>
        /// Gets the value of PWL for given time.
        /// </summary>
        /// <param name="time">Time.</param>
        /// <returns>
        /// PWL's value for given time.
        /// </returns>
        protected double GetLineValue(double time)
        {
            while (_currentLineIndex < _pwlPoints)
            {
                if (Times[_currentLineIndex] >= time)
                {
                    long prevLineIndex = _currentLineIndex - 1;
                    if (prevLineIndex >= 0)
                    {
                        if (_lineDefinition == null)
                        {
                            _lineDefinition = CreateLineParameters(Times[prevLineIndex], Times[_currentLineIndex], Voltages[prevLineIndex], Voltages[_currentLineIndex]);
                            _breakPointAdded = false;
                        }
                        return (_lineDefinition.A * time) + _lineDefinition.B;
                    }
                    else
                    {
                        return Voltages[0];
                    }
                }

                _breakPointAdded = false;
                _lineDefinition = null;
                _currentLineIndex++;
            }

            return Voltages[_pwlPoints - 1];
        }

        /// <summary>
        /// Calculate the slope and intercept of the line between two given points.
        /// </summary>
        /// <param name="x1">The first x-coordinate.</param>
        /// <param name="x2">The second x-coordinate.</param>
        /// <param name="y1">The first y-coordinate.</param>
        /// <param name="y2">The second y-coordinate.</param>
        /// <returns></returns>
        protected static LineDefinition CreateLineParameters(double x1, double x2, double y1, double y2)
        {
            double a = (y2 - y1) / (x2 - x1);
            return new LineDefinition()
            {
                A = a,
                B = y1 - (a * x1),
            };
        }
    }
}
