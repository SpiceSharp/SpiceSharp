using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Parser.Expressions;

namespace SpiceSharpTest
{
    [TestClass]
    public class SpiceExpressionTests
    {
        public static double Tolerance = 1e-15;

        [TestMethod]
        public void TestModifiers()
        {
            string[] tests = new string[]
            {
                "1.1TB", "1.23g", "1.345megHz", "2.5678kOhm", "1.87654mV", "1.9u", "11.5n", "-18p", "25.5f"
            };
            double[] results = new double[]
            {
                1.1e12, 1.23e9, 1.345e6, 2.5678e3, 1.87654e-3, 1.9e-6, 11.5e-9, -18.0e-12, 25.5e-15
            };
            RunTests(tests, results);
        }

        [TestMethod]
        public void TestOperators()
        {
            SpiceExpression e = new SpiceExpression();

            string[] tests = new string[]
            {
                // Basic operators
                "2.345 + 2.5u", "2.345 - 2.5u",
                "-1.5u * 3k", "20meg / 1.9u",
                "10 % 3",
                "101 < 101", "101 <= 101",
                "102 > 101", "102 >= 101",
                "101u == 101e-6", "1 == 0",
                "101u != 101e-6", "1 != 0",
                "1 || 0", "0 || 0",
                "1 && 0", "1 && 1",
                "1.0 ? -1.0 : 2.0"
            };
            double[] results = new double[]
            {
                2.345 + 2.5e-6, 2.345 - 2.5e-6,
                -1.5e-6 * 3e3, 20e6 / 1.9e-6,
                10 % 3,
                0.0, 1.0,
                1.0, 1.0,
                1.0, 0.0,
                0.0, 1.0,
                1.0, 0.0,
                0.0, 1.0,
                -1.0
            };
            RunTests(tests, results);
        }

        [TestMethod]
        public void TestPrecedence()
        {
            SpiceExpression e = new SpiceExpression();

            string[] tests = new string[]
            {
                "1 + 2 * 3", "1 - 2 / 3",
                "2 / 3 / 4",
                "1 ? 2 ? 3 : 4 : 5 ? 6 : 7",
                "1 ? 2 > 3 : 4",
                "(1 + 2) * 3",
                "10.5g / ((1m + 2) / (10u - 2))"
            };
            double[] results = new double[]
            {
                1.0 + 2.0 * 3.0, 1.0 - 2.0 / 3.0,
                2.0 / 3.0 / 4.0,
                true ? true ? 3 : 4 : true ? 6 : 7,
                0.0,
                9.0,
                10.5e9 / ((1e-3 + 2) / (10e-6 - 2))
            };
            RunTests(tests, results);
        }

        [TestMethod]
        public void TestFunctions()
        {
            string[] tests = new string[]
            {
                "min(10, 20)", "max(10, 20)", "abs(-1u)", "sqrt(20.0)",
                "exp(20m)", "log(20m)", "log10(20m)", "pow(1.45, 1.8)",
                "cos(1.23)", "sin(-1.1)", "tan(1.345)",
                "cosh(1.23)", "sinh(-1.1)", "tanh(1.345)",
                "acos(0.78)", "asin(0.456)", "atan(0.46)",
                "atan2(1, 2)"
            };
            double[] results = new double[]
            {
                10.0, 20.0, 1e-6, Math.Sqrt(20.0),
                Math.Exp(20e-3), Math.Log(20e-3), Math.Log10(20e-3),
                Math.Pow(1.45, 1.8),
                Math.Cos(1.23), Math.Sin(-1.1), Math.Tan(1.345),
                Math.Cosh(1.23), Math.Sinh(-1.1), Math.Tanh(1.345),
                Math.Acos(0.78), Math.Asin(0.456), Math.Atan(0.46),
                Math.Atan2(1.0, 2.0)
            };
            RunTests(tests, results);
        }

        /// <summary>
        /// Run a series of expressions
        /// </summary>
        /// <param name="tests">Expressions</param>
        /// <param name="results">Expected results</param>
        private void RunTests(string[] tests, double[] results)
        {
            SpiceExpression e = new SpiceExpression();

            // Run tests
            for (int i = 0; i < tests.Length; i++)
            {
                double c = e.Parse(tests[i]);
                double error = c - results[i];
                if (results[i] != 0.0)
                    error /= results[i];
                Assert.AreEqual(error, 0.0, Tolerance);
            }
        }
    }
}
