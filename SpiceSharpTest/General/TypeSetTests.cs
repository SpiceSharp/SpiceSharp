using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.General;

namespace SpiceSharpTest.General
{
    [TestFixture]
    public class TypeSetTests
    {
        private interface IValue { }
        private class Value : IValue { }

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

        [Test]
        public void When_LastInheritedTypeIsRemoved_Expect_InterfaceLookupIsMissing()
        {
            var value = new Value();
            var set = new InheritedTypeSet<object> { value };

            Assert.That(set.Remove(value), Is.True);
            Assert.That(set.ContainsType<IValue>(), Is.False);
            Assert.That(set.TryGetValue<IValue>(out _), Is.False);
        }

        [Test]
        public void When_LastInterfaceTypeIsRemoved_Expect_InterfaceLookupIsMissing()
        {
            var value = new Value();
            var set = new InterfaceTypeSet<object> { value };

            Assert.That(set.Remove(value), Is.True);
            Assert.That(set.ContainsType<IValue>(), Is.False);
            Assert.That(set.TryGetValue<IValue>(out _), Is.False);
        }
    }
}