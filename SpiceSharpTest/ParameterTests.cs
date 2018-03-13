using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Attributes;

namespace SpiceSharpTest.Parameters
{
    /// <summary>
    /// Summary description for ParameterTests
    /// </summary>
    [TestFixture]
    public class ParameterTests
    {
        /// <summary>
        /// Example parameter class that contains parameters of every type
        /// </summary>
        public class ParameterExample : ParameterSet
        {
            [PropertyName("field1")]
            public double Field1;

            [PropertyName("property1")]
            public double Property1 { get; private set; }

            [PropertyName("property2")]
            public double Property2 { get; set; }

            [PropertyName("method1")]
            public void SetMethod1(double value) => Property1 = value;

            [PropertyName("method2")]
            public double GetMethod() => 1.0;

            [PropertyName("parameter1")]
            public Parameter Parameter1 { get; } = new Parameter();
        }

        [Test]
        public void When_SetterForField_Expect_DirectAccess()
        {
            var p = new ParameterExample();
            var setter = p.GetSetter("field1");
            setter(1.0);
            Assert.AreEqual(1.0, p.Field1, 1e-12);
            setter(10.0);
            Assert.AreEqual(10.0, p.Field1, 1e-12);
        }

        [Test]
        public void When_SetterForGetOnlyProperty_Expect_Null()
        {
            var p = new ParameterExample();
            Assert.AreEqual(null, p.GetSetter("property1"));
        }

        [Test]
        public void When_SetterForProperty_Expect_DirectAccess()
        {
            var p = new ParameterExample();
            var setter = p.GetSetter("property2");
            setter(1.0);
            Assert.AreEqual(1.0, p.Property2, 1e-12);
            setter(10.0);
            Assert.AreEqual(10.0, p.Property2, 1e-12);
        }

        [Test]
        public void When_SetterForMethod_Expect_DirectAccess()
        {
            var p = new ParameterExample();
            var setter = p.GetSetter("method1");
            setter(1.0);
            Assert.AreEqual(1.0, p.Property1, 1e-12);
            setter(10.0);
            Assert.AreEqual(10.0, p.Property1, 1e-12);
        }

        [Test]
        public void When_SetterForParameterProperty_Expect_DirectAccess()
        {
            var p = new ParameterExample();
            var setter = p.GetSetter("parameter1");
            Assert.AreEqual(false, p.Parameter1.Given);
            setter(1.0);
            Assert.AreEqual(1.0, p.Parameter1.Value, 1e-12);
            Assert.AreEqual(true, p.Parameter1.Given);
        }

        [Test]
        public void When_GetParameter_Expect_Parameter()
        {
            var p = new ParameterExample();
            var param = p.GetParameter("parameter1");
            Assert.AreEqual(p.Parameter1, param);
        }
    }
}
