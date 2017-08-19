using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp.Parser.Expressions;

namespace SpiceSharpTest
{
    [TestClass]
    public class SpiceExpressionTests
    {
        public static double Tolerance = 1e-18;

        [TestMethod]
        public void TestModifiers()
        {
            SpiceExpression e = new SpiceExpression();

            string[] test = new string[]
            {
                "1.1TB", "1.23g", "1.345megHz", "2.5678kOhm", "1.87654mV", "1.9u", "11.5n", "-18p", "25.5f"
            };
            double[] results = new double[]
            {
                1.1e12, 1.23e9, 1.345e6, 2.5678e3, 1.87654e-3, 1.9e-6, 11.5e-9, -18.0e-12, 25.5e-15
            };
            for (int i = 0; i < test.Length; i++)
                Assert.AreEqual(results[i], e.Parse(test[i]), Tolerance);
        }
    }
}
