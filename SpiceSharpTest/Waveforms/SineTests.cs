using NUnit.Framework;
using SpiceSharp.Components;
using System;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class SineTests
    {
        [Test]
        public void When_NegativeFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Sine(0, 1, -1).Create(null));
        }
    }
}
