using NUnit.Framework;
using SpiceSharp.Simulations;
using System;
using System.Linq;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class OctaveSweepTests
    {
        [Test]
        public void When_Exponential_Expect_Reference()
        {
            var sweep = new OctaveSweep(0.1, 102.4, 1);
            double[] expected =
            [
                0.1,
                0.2,
                0.4,
                0.8,
                1.6,
                3.2,
                6.4,
                12.8,
                25.6,
                51.2,
                102.4
            ];
            int index = 0;
            foreach (double c in sweep)
                Assert.That(c, Is.EqualTo(expected[index++]).Within(1e-9));
            Assert.That(expected.Length, Is.EqualTo(index));
        }

        [Test]
        public void When_Decaying_Expect_Reference()
        {
            var sweep = new OctaveSweep(102.4, 0.1, 1);
            double[] expected =
            [
                102.4,
                51.2,
                25.6,
                12.8,
                6.4,
                3.2,
                1.6,
                0.8,
                0.4,
                0.2,
                0.1
            ];
            int index = 0;
            foreach (double c in sweep)
                Assert.That(c, Is.EqualTo(expected[index++]).Within(1e-9));
            Assert.That(expected.Length, Is.EqualTo(index));
        }

        [Test]
        public void When_ExponentialNegative_Expect_Reference()
        {
            var sweep = new OctaveSweep(-0.1, -102.4, 1);
            double[] expected =
            [
                -0.1,
                -0.2,
                -0.4,
                -0.8,
                -1.6,
                -3.2,
                -6.4,
                -12.8,
                -25.6,
                -51.2,
                -102.4
            ];
            int index = 0;
            foreach (double c in sweep)
                Assert.That(c, Is.EqualTo(expected[index++]).Within(1e-9));
            Assert.That(expected.Length, Is.EqualTo(index));
        }

        [Test]
        public void When_DecayingNegative_Expect_Reference()
        {
            var sweep = new OctaveSweep(-102.4, -0.1, 1);
            double[] expected =
            [
                -102.4,
                -51.2,
                -25.6,
                -12.8,
                -6.4,
                -3.2,
                -1.6,
                -0.8,
                -0.4,
                -0.2,
                -0.1
            ];
            int index = 0;
            foreach (double c in sweep)
                Assert.That(c, Is.EqualTo(expected[index++]).Within(1e-9));
            Assert.That(expected.Length, Is.EqualTo(index));
        }

        [Test]
        public void When_Invalid_Expect_Exception()
        {
            // Wrong signs
            Assert.Throws<ArgumentException>(() => new OctaveSweep(-1, 100, 2).ToArray());

            // Zero start
            Assert.Throws<ArgumentException>(() => new OctaveSweep(0, 10, 2).ToArray());

            // Zero end
            Assert.Throws<ArgumentException>(() => new OctaveSweep(1, 0, 2).ToArray());
        }
    }
}
