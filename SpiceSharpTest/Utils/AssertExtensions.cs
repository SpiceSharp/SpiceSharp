using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SpiceSharpTest.Utils
{
    public static class AssertExtensions
    {
        public static void AreEqualWithTol(this Assert assert, double expected, double actual, double relativeTol, double absoluteTol)
        {
            double tol = Math.Max(Math.Abs(actual), Math.Abs(expected)) * relativeTol + absoluteTol;
            Assert.AreEqual(expected, actual, tol);
        }
    }
}
