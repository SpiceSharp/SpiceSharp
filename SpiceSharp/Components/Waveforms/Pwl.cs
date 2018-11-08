using System;
using System.Collections.Generic;
using SpiceSharp.IntegrationMethods;
using SpiceSharp.Simulations;

namespace SpiceSharp.Components.Waveforms
{
    public class Pwl : Waveform
    {
        public Pwl(double[] times, double[] voltages, double initialVoltage = 0.0)
        {
            if (times == null)
            {
                throw new NullReferenceException(nameof(times));
            }

            if (voltages == null)
            {
                throw new NullReferenceException(nameof(voltages));
            }

            if (times.Length != voltages.Length)
            {
                throw new InvalidOperationException("PWL - times array has different length than voltages array");
            }

            Points = CreatePoints(times, voltages, initialVoltage);
            LineParameters = CreateLineParameters(Points);
        }

        public List<Point> Points { get; }

        protected LineDefinition[] LineParameters { get; }

        public override void Accept(TimeSimulation simulation)
        {
            if (simulation == null)
            {
                throw new ArgumentNullException(nameof(simulation));
            }

            double time = simulation.Method.Time;
            if (time.Equals(0.0))
                Value = Points[0].Y;

            if (simulation.Method is IBreakpoints method)
            {
                var breaks = method.Breakpoints;
                if (!method.Break)
                    return;

                for (int i = 0; i < Points.Count; i++)
                {
                    breaks.SetBreakpoint(Points[i].X);
                }
                   
            }
        }

        public override void Probe(TimeSimulation simulation)
        {
            var time = simulation.Method.Time;

            Value = GetLineValue(Points, LineParameters, time);
        }

        public override void Setup()
        {
        }

        protected List<Point> CreatePoints(double[] times, double[] voltages, double initialVoltage)
        {
            var result = new List<Point>();

            for (var i = 0; i < times.Length; i++)
            {
                result.Add(new Point(times[i], voltages[i]));
            }

            if (times[0] != 0.0)
            {
                result.Insert(0, new Point(0.0, initialVoltage));
            }

            return result;
        }

        protected static double GetLineValue(List<Point> points, LineDefinition[] lines, double x)
        {
            int index = 0;

            while (index < points.Count && points[index].X < x)
            {
                index++;
            }

            if (index == points.Count)
            {
                return points[points.Count - 1].Y;
            }

            if (index == 0 && points[0].X > x)
            {
                return points[0].Y;
            }

            return (lines[index].A * x) + lines[index].B;
        }

        protected static LineDefinition[] CreateLineParameters(List<Point> points)
        {
            var result = new List<LineDefinition>();

            for (var i = 0; i < points.Count - 1; i++)
            {
                double x1 = points[i].X;
                double x2 = points[i + 1].X;
                double y1 = points[i].Y;
                double y2 = points[i + 1].Y;

                double a = (y2 - y1) / (x2 - x1);

                result.Add(new LineDefinition()
                {
                    A = a,
                    B = y1 - (a * x1),
                });
            }
            result.Insert(0, result[0]);
            result.Add(result[result.Count - 1]);
            return result.ToArray();
        }

       
        public class Point
        {
            public Point(double x, double y)
            {
                X = x;
                Y = y;
            }

            public double X { get; set; }

            public double Y { get; set; }
        }

        protected class LineDefinition
        {
            public double A { get; set; }

            public double B { get; set; }
        }
    }
}
