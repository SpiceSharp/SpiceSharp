using NUnit.Framework;
using SpiceSharp;

namespace SpiceSharpTest.General
{
    [TestFixture]
    public class StringIdentifierTests
    {
        [Test]
        public void When_StringIdentifier_Is_Created_From_String_Comparision_With_Strings_Test()
        {
            Identifier s = "abC";

            Assert.AreEqual(s, "AbC");
            Assert.AreEqual(s, "ABC");
            Assert.AreEqual(s, "abc");
        }

        [Test]
        public void When_StringIdentifier_Is_Created_From_String_Comparision_With_StringIdentifiers_Test()
        {
            Identifier s = "abC";

            Assert.AreEqual(s, new StringIdentifier("abC"));
            Assert.AreEqual(s, new StringIdentifier("ABC"));
            Assert.AreEqual(s, new StringIdentifier("abc"));
        }

        [Test]
        public void When_StringIdentifier_Is_Created_From_Constructor_Comparision_With_StringIdentifiers_Test()
        {
            Identifier s = new StringIdentifier("abC");

            Assert.AreEqual(s, new StringIdentifier("abC"));
            Assert.AreEqual(s, new StringIdentifier("ABC"));
            Assert.AreEqual(s, new StringIdentifier("abc"));
        }

        [Test]
        public void When_StringIdentifier_Is_Created_From_Constructor_CaseSensitive_Comparision_With_StringIdentifiers_Test()
        {
            Identifier s = new StringIdentifier("abC", true);

            Assert.AreEqual(s, new StringIdentifier("abC", true));
            Assert.AreNotEqual(s, new StringIdentifier("ABC", true));
            Assert.AreNotEqual(s, new StringIdentifier("abc", true));
        }
    }
}
