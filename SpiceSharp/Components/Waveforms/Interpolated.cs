using System;
using System.Collections.Generic;
using SpiceSharp.Diagnostics;
using MathNet.Numerics.Interpolation;

namespace SpiceSharp.Components
{
    /// <summary>
    /// An interpolated waveform
    /// </summary>
    public class Interpolated : Waveform<Interpolated>
    {
        /// <summary>
        /// Gets the list of time points
        /// </summary>
        public double[] Time { get; set; } = null;

        /// <summary>
        /// Gets the list of value points
        /// </summary>
        public double[] Value { get; set; } = null;

        /// <summary>
        /// Private variables
        /// </summary>
        private double pw = 0, per = 0;
        private int cindex = 0;
        private IInterpolation interpolation;

        /// <summary>
        /// Constructor
        /// </summary>
        public Interpolated() : base()
        {
        }

        /// <summary>
        /// Setup the interpolated waveform
        /// </summary>
        /// <param name="ckt"></param>
        public override void Setup(Circuit ckt)
        {
            if (Time == null || Time.Length < 2 || Value == null || Value.Length < 2)
                throw new CircuitException("No timepoints");

            // Sort the time points
            Array.Sort(Time, Value);

            // Create the linear interpolation
            interpolation = LinearSpline.Interpolate(Time, Value);

            per = Time[Time.Length - 1];
            pw = double.PositiveInfinity;
            for (int i = 1; i < Time.Length; i++)
                pw = Math.Min(pw, Time[i] - Time[i - 1]);
        }

        /// <summary>
        /// Calculate the value at a timepoint
        /// </summary>
        /// <param name="time">Timepoint</param>
        /// <returns></returns>
        public override double At(double time)
        {
            double basetime = 0.0;

            // Get the relative time
            if (time > per)
            {
                basetime = per * Math.Floor(time / per);
                time -= basetime;
            }
            if (time <= 0.0)
                time = 0.0;
            if (time > per)
                time = per;

            // Return the interpolated value
            return interpolation.Interpolate(time);
        }

        /// <summary>
        /// Accept the current timepoint
        /// </summary>
        /// <param name="ckt">The circuit</param>
        public override void Accept(Circuit ckt)
        {
            // Add a breakpoint
            if (ckt.Method == null)
                return;

            // Are we at a breakpoint?
            var method = ckt.Method;
            var breaks = method.Breaks;
            if (!method.Break)
                return;

            // Get the relative time
            double time = method.Time;
            double basetime = 0.0;
            if (time > per)
            {
                basetime = per * Math.Floor(time / per);
                time -= basetime;
            }
            if (time <= 0.0)
                time = 0.0;
            if (time > per)
                time = per;

            // Are we at the start of a breakpoint?
            double tol = 1e-7 * pw;
            
            // Are we at the start or very near the end?
            if (Math.Abs(time) <= tol)
            {
                cindex = 0;
                if (Time[cindex] == 0.0)
                    cindex++;
                breaks.SetBreakpoint(basetime + Time[cindex]);
            }
            else if (Math.Abs(time - per) <= tol)
            {
                cindex = 0;
                if (Time[cindex] == 0.0)
                    cindex++;
                breaks.SetBreakpoint(basetime + per + Time[cindex]);
            }
            else
            {
                if (Math.Abs(time - Time[cindex]) <= tol)
                {
                    if (cindex < Time.Length)
                    {
                        cindex++;
                        breaks.SetBreakpoint(basetime + Time[cindex]);
                    }
                }
            }
        }
    }
}
