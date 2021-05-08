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
            var expected = new[]
            {
                0.1,
                1,
                10,
                100
            };
            var index = 0;
            foreach (var c in sweep)
                Assert.AreEqual(expected[index++], c, 1e-9);
            Assert.AreEqual(index, expected.Length);
        }

        [Test]
        public void When_Decaying_Expect_Reference()
        {
            var sweep = new DecadeSweep(100, 0.1, 1);
            var expected = new[]
            {
                100,
                10,
                1,
                0.1
            };
            var index = 0;
            foreach (var c in sweep)
                Assert.AreEqual(expected[index++], c, 1e-9);
            Assert.AreEqual(index, expected.Length);
        }

        [Test]
        public void When_ExponentialNegative_Expect_Reference()
        {
            var sweep = new DecadeSweep(-0.1, -100, 1);
            var expected = new[]
            {
                -0.1,
                -1,
                -10,
                -100
            };
            var index = 0;
            foreach (var c in sweep)
                Assert.AreEqual(expected[index++], c, 1e-9);
            Assert.AreEqual(index, expected.Length);
        }

        [Test]
        public void When_DecayingNegative_Expect_Reference()
        {
            var sweep = new DecadeSweep(-100, -0.1, 1);
            var expected = new[]
            {
                -100,
                -10,
                -1,
                -0.1
            };
            var index = 0;
            foreach (var c in sweep)
                Assert.AreEqual(expected[index++], c, 1e-9);
            Assert.AreEqual(index, expected.Length);
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
