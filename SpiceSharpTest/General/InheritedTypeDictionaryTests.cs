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
            var d = new InheritedTypeDictionary<IA, IA>();
            d.Add<A>(a);
            d.Add<B>(b);

            Assert.AreEqual(a, d.GetValue<A>());
            Assert.AreEqual(b, d.GetValue<B>());
            Assert.AreEqual(b, d.GetValue<IB>());
            Assert.Throws<AmbiguousTypeException>(() => d.GetValue<IA>());
        }
    }
}
