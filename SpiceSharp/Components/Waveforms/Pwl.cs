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
        private LineDefinition lineDefinition = null;
        private bool breakPointsAdded = false;
        private long currentLineIndex = 0;
        private long pwlPoints = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="times">Array of times.</param>
        /// <param name="voltages">Array of voltages.</param>
        public Pwl(double[] times, double[] voltages)
        {
            Times = times ?? throw new ArgumentNullException(nameof(times));
            Voltages = voltages ?? throw new ArgumentNullException(nameof(voltages));

            if (Times.Length != Voltages.Length)
            {
                throw new ArgumentException("PWL - times array has different length than voltages array");
            }

            pwlPoints = Times.Length;

            if (pwlPoints == 0)
            {
                throw new ArgumentException("PWL - times array has zero points");
            }
        }

        /// <summary>
        /// Array of times.
        /// </summary>
        public double[] Times { get; private set; }

        /// <summary>
        /// Array of voltages.
        /// </summary>
        public double[] Voltages { get; private set; }

        /// <summary>
        /// Accepts the current timepoint.
        /// </summary>
        /// <param name="simulation">The time-based simulation</param>
        public override void Accept(TimeSimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            double time = simulation.Method.Time;

            if (simulation.Method.Time.Equals(0.0))
                Value = Voltages[0];

            if (simulation.Method is IBreakpoints method)
            {
                var breaks = method.Breakpoints;

                if (!breakPointsAdded)
                {
                    for (var i = 0; i < Times.Length; i++)
                    {
                        breaks.SetBreakpoint(Times[i]);
                    }

                    breakPointsAdded = true;
                }
            }
        }

        /// <summary>
        /// Indicates a new timepoint is being probed.
        /// </summary>
        /// <param name="simulation">The time-based simulation.</param>
        public override void Probe(TimeSimulation simulation)
        {
            var time = simulation.Method.Time;
            Value = GetLineValue(time);
        }

        /// <summary>
        /// Resets the waveform.
        /// </summary>
        public void Reset()
        {
            breakPointsAdded = false;
            currentLineIndex = 0;
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
            while (currentLineIndex < pwlPoints)
            {
                if (Times[currentLineIndex] >= time)
                {
                    long prevLineIndex = currentLineIndex - 1;
                    if (prevLineIndex >= 0)
                    {
                        if (lineDefinition == null)
                        {
                            lineDefinition = CreateLineParameters(Times[prevLineIndex], Times[currentLineIndex], Voltages[prevLineIndex], Voltages[currentLineIndex]);
                        }
                        return (lineDefinition.A * time) + lineDefinition.B;
                    }
                    else
                    {
                        return Voltages[0];
                    }
                }

                lineDefinition = null;
                currentLineIndex++;
            }

            return Voltages[pwlPoints - 1];
        }

        protected static LineDefinition CreateLineParameters(double x1, double x2, double y1, double y2)
        {
            double a = (y2 - y1) / (x2 - x1);
            return new LineDefinition()
            {
                A = a,
                B = y1 - (a * x1),
            };
        }

        protected class LineDefinition
        {
            public double A { get; set; }

            public double B { get; set; }
        }
    }
}
