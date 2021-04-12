using NUnit.Framework;
using SpiceSharp.Components;
using System;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class SFFMTests
    {
        [Test]
        public void When_NegativeCarrierFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SFFM(0, 1, -1, 1, 1).Create(null));
        }

        [Test]
        public void When_NegativeSignalFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new SFFM(0, 1, 1, 0, -1).Create(null));
        }
    }
}
