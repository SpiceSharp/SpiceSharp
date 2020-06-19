﻿using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Waveforms
{
    [TestFixture]
    public class PulseTests
    {
        [Test]
        public void When_PulseHasInvalidParameters_Expect_Exception()
        {
            // Negative rise time
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pulse(0, 1, 0, -1, 1, 2, 5).Create(null));
            // Negative fall time
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pulse(0, 1, 0, 1, -1, 2, 5).Create(null));
            // Negative pulse width
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pulse(0, 1, 0, 1, 1, -1, 5).Create(null));
            // Negative period
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pulse(0, 1, 0, 1, 1, 1, -1).Create(null));
            // Sum of times is higher than a period
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pulse(0, 1, 0, 1, 1, 1, 2).Create(null));
        }

        [Test]
        public void When_PulseBreakpoints_Expect_Reference()
        {
            var ckt = new Circuit(
                new VoltageSource("V1", "in", "0", new Pulse(0, 1, 0.2, 0.1, 0.1, 0.4, 1.0)));

            var tran = new Transient("tran", 0.1, 1.2);
            bool riseHit = false, risenHit = false, fallHit = false, fallenHit = false;

            tran.ExportSimulationData += (sender, args) =>
            {
                if (Math.Abs(args.Time - 0.2) < 1e-12)
                    riseHit = true;
                if (Math.Abs(args.Time - 0.3) < 1e-12)
                    risenHit = true;
                if (Math.Abs(args.Time - 0.7) < 1e-12)
                    fallHit = true;
                if (Math.Abs(args.Time - 0.8) < 1e-12)
                    fallenHit = true;
            };
            tran.Run(ckt);

            Assert.True(riseHit);
            Assert.True(risenHit);
            Assert.True(fallHit);
            Assert.True(fallenHit);
        }
    }
}
