using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpiceSharp;
using SpiceSharp.Circuits;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using SpiceSharp.Diagnostics;

namespace SpiceSharpTest
{
    [TestClass]
    public class SpiceSharpTest
    {
        /// <summary>
        /// Calculate the error
        /// </summary>
        /// <param name="check">The value to check</param>
        /// <param name="expected">The expected value</param>
        /// <returns></returns>
        private double Error(double check, double expected)
        {
            double error = check - expected;
            if (expected != 0.0)
                error /= expected;
            return error;
        }
    }
}
