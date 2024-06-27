using NUnit.Framework;
using SpiceSharp.Simulations;
using System;
using System.Linq;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class DecadeSweepTests
    {
        [Test]
        public void When_Exponential_Expect_Reference()
        {
            var sweep = new DecadeSweep(0.1, 100, 1);
            double[] expected =
            [
                0.1,
                1,
                10,
                100
            ];
            int index = 0;
            foreach (double c in sweep)
                Assert.That(c, Is.EqualTo(expected[index++]).Within(1e-9));
            Assert.That(expected.Length, Is.EqualTo(index));
        }

        [Test]
        public void When_Decaying_Expect_Reference()
        {
            var sweep = new DecadeSweep(100, 0.1, 1);
            double[] expected =
            [
                100,
                10,
                1,
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
            var sweep = new DecadeSweep(-0.1, -100, 1);
            double[] expected =
            [
                -0.1,
                -1,
                -10,
                -100
            ];
            int index = 0;
            foreach (double c in sweep)
                Assert.That(c, Is.EqualTo(expected[index++]).Within(1e-9));
            Assert.That(expected.Length, Is.EqualTo(index));
        }

        [Test]
        public void When_DecayingNegative_Expect_Reference()
        {
            var sweep = new DecadeSweep(-100, -0.1, 1);
            double[] expected =
            [
                -100,
                -10,
                -1,
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
            Assert.Throws<ArgumentException>(() => new DecadeSweep(-1, 100, 2).ToArray());

            // Zero start
            Assert.Throws<ArgumentException>(() => new DecadeSweep(0, 10, 2).ToArray());

            // Zero end
            Assert.Throws<ArgumentException>(() => new DecadeSweep(1, 0, 2).ToArray());
        }
    }
}
