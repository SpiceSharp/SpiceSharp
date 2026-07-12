using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.General;

namespace SpiceSharpTest.General
{
    [TestFixture]
    public class TypeSetTests
    {
        private interface IValue { }

        [Test]
        public void When_InheritedTypeIsMissing_Expect_Exception()
        {
            var set = new InheritedTypeSet<object>();

            var exception = Assert.Throws<TypeNotFoundException>(() => set.GetValue<IValue>());
            Assert.That(exception.Type, Is.EqualTo(typeof(IValue)));
        }

        [Test]
        public void When_InterfaceTypeIsMissing_Expect_Exception()
        {
            var set = new InterfaceTypeSet<object>();

            var exception = Assert.Throws<TypeNotFoundException>(() => set.GetValue<IValue>());
            Assert.That(exception.Type, Is.EqualTo(typeof(IValue)));
        }
    }
}