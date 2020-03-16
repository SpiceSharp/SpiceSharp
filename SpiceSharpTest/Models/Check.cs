using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Algebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace SpiceSharpTest.Models
{
    public static class Check
    {
        public static double RelativeTolerance = 1e-6;
        public static double AbsoluteTolerance = 1e-20;

        public static void Solver(ISparseSolver<double> solver, params double[] expected)
        {
            if (expected.Length != solver.Size * (solver.Size + 1))
                throw new ArgumentException("Wrong number of reference values. Solver has {0} elements, but {1} were given.".FormatString(solver.Size * (solver.Size + 1), expected.Length));
            var index = 0;
            for (var row = 0; row < solver.Size; row++)
            {
                for (var col = 0; col < solver.Size; col++)
                {
                    if (double.IsNaN(expected[index]))
                        Assert.AreEqual(null, solver.FindElement(row + 1, col + 1));
                    else
                        Double(expected[index], solver.GetElement(row + 1, col + 1).Value);
                    index++;
                }
                if (double.IsNaN(expected[index]))
                    Assert.AreEqual(null, solver.FindElement(row + 1));
                else
                    Double(expected[index], solver.GetElement(row + 1).Value);
                index++;
            }
        }
        public static void Solver(ISparseSolver<Complex> solver, params Complex[] expected)
        {
            if (expected.Length != solver.Size * (solver.Size + 1))
                throw new ArgumentException("Wrong number of reference values. Solver has {0} elements, but {1} were given.".FormatString(solver.Size * (solver.Size + 1), expected.Length));
            var index = 0;
            for (var row = 0; row < solver.Size; row++)
            {
                for (var col = 0; col < solver.Size; col++)
                {
                    if (double.IsNaN(expected[index].Real))
                        Assert.AreEqual(null, solver.FindElement(row + 1, col + 1));
                    else
                        Complex(expected[index], solver.GetElement(row + 1, col + 1).Value);
                    index++;
                }
                if (double.IsNaN(expected[index].Real))
                    Assert.AreEqual(null, solver.FindElement(row + 1));
                else
                    Complex(expected[index], solver.GetElement(row + 1).Value);
                index++;
            }
        }
        public static void Properties(object obj, IDictionary<string, double> expected)
        {
            var used = new HashSet<string>();

            // Use reflection to compare all their values
            foreach (var property in obj.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Where(x => x.PropertyType == typeof(double)))
            {
                var getter = property.GetGetMethod(true);
                if (getter != null && expected.TryGetValue(property.Name, out var expectedValue))
                {
                    used.Add(property.Name);
                    var actualValue = (double)getter.Invoke(obj, null);
                    Double(expectedValue, actualValue, "Mismatch for property '{0}'".FormatString(property.Name));
                }
            }

            // Make sure we tested all the properties
            Assert.AreEqual(expected.Count, used.Count);
        }

        public static void Double(double expected, double actual, string message = null)
        {
            var tol = Math.Max(Math.Abs(expected), Math.Abs(actual)) * RelativeTolerance + AbsoluteTolerance;
            Assert.AreEqual(expected, actual, tol, message);
        }
        public static void Complex(Complex expected, Complex actual, string message = null)
        {
            Double(expected.Real, actual.Real, message);
            Double(expected.Imaginary, actual.Imaginary, message);
        }
    }
}
