using NUnit.Framework;
using SpiceSharp.General;

namespace SpiceSharpTest.General
{
    [TestFixture]
    public class InheritedTypeDictionaryTests
    {
        private interface IA { }
        private interface IB : IA { }
        private class A : IA { }
        private class B : A, IB { }

        [Test]
        public void When_Inheritance1_Expect_Reference()
        {
            var a = new A();
            var b = new B();
            var d = new InheritedTypeDictionary<object>();
            d.Add(typeof(A), a);
            d.Add(typeof(B), b);

            Assert.That(a, Is.EqualTo(d[typeof(A)]));
            Assert.That(b, Is.EqualTo(d[typeof(B)]));
            Assert.That(b, Is.EqualTo(d[typeof(IB)]));
            Assert.Throws<AmbiguousTypeException>(() => { object r = d[typeof(IA)]; });
        }
    }
}
