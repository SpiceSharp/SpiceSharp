using NUnit.Framework;
using SpiceSharp.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class AMTests
    {
        [Test]
        public void When_NegativeModulationFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AM(1, 0, -1, 1, 0, 0).Create(null));
        }

        [Test]
        public void When_NegativeCarrierFrequency_Expect_Exception()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new AM(1, 0, 1, -1, 0, 0).Create(null));
        }
    }
}
