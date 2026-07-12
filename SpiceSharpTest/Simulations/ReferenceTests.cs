using NUnit.Framework;
using SpiceSharp.Simulations.Base;
using System.Collections.Generic;
using System.Linq;

namespace SpiceSharpTest.Simulations
{
    [TestFixture]
    public class ReferenceTests
    {
        [Test]
        public void When_ReferenceIsDefault_Expect_EmptyValueBehavior()
        {
            Reference value = default;
            Reference fromNullArray = (string[])null;
            Reference fromNullList = (List<string>)null;

            Assert.That(value.Length, Is.Zero);
            Assert.That(value, Is.EqualTo(new Reference()));
            Assert.That(value.GetHashCode(), Is.EqualTo(new Reference().GetHashCode()));
            Assert.That(value.ToString(), Is.Empty);
            Assert.That(value.ToArray(), Is.Empty);
            Assert.That(fromNullArray, Is.EqualTo(value));
            Assert.That(fromNullList, Is.EqualTo(value));
        }
    }
}