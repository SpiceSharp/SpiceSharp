using NUnit.Framework;
using SpiceSharp.Simulations;
using System;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class VariableMapTests
    {
        [Test]
        public void When_IndexIsMissing_Expect_Exception()
        {
            var map = new VariableMap(new Variable("0", null));

            Assert.Throws<ArgumentException>(() => _ = map[1]);
        }

        [Test]
        public void When_IndexAlreadyMapped_Expect_Exception()
        {
            var map = new VariableMap(new Variable("0", null));
            map.Add(new Variable("a", null), 1);

            Assert.Throws<ArgumentException>(() => map.Add(new Variable("b", null), 1));
            Assert.That(map[1].Name, Is.EqualTo("a"));
        }
    }
}