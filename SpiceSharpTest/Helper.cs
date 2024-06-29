using NUnit.Framework;
using System;

namespace SpiceSharpTest
{
    public static class Helper
    {
        /// <summary>
        /// The default absolute tolerance.
        /// </summary>
        public const double DefaultAbsoluteTolerance = 1e-20;

        /// <summary>
        /// The default relative tolerance
        /// </summary>
        public const double DefaultRelativeTolerance = 1e-6;

        /// <summary>
        /// Calculates the allowed tolerance.
        /// </summary>
        /// <param name="reference">The reference value.</param>
        /// <param name="actual">The actual value.</param>
        /// <param name="relTol">The relative tolerance.</param>
        /// <param name="absTol">The abs tolerance.</param>
        /// <returns>The tolerance.</returns>
        public static double Tolerance(double reference, double actual, double relTol = DefaultRelativeTolerance, double absTol = DefaultAbsoluteTolerance)
        {
            return Math.Max(Math.Abs(reference), Math.Abs(actual)) * relTol + absTol;
        }

        /// <summary>
        /// Throw an exception if they aren't equal.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <param name="actual">The actual.</param>
        /// <param name="relTol">The relative tolerance.</param>
        /// <param name="absTol">The absolute tolerance.</param>
        public static void AreEqual(double reference, double actual, double relTol = DefaultRelativeTolerance, double absTol = DefaultAbsoluteTolerance)
        {
            Assert.That(actual, Is.EqualTo(reference).Within(Tolerance(reference, actual, relTol, absTol)));
        }
    }
}
