using System;
using System.Collections.Generic;
using NUnit.Framework;
using SpiceSharp;
using SpiceSharp.Components;

namespace SpiceSharpTest.Circuits
{
    [TestFixture]
    public class IdentifierTests
    {
        protected class CaseInsensitiveIdentifier : Identifier
        {
            private readonly string _id;

            /// <summary>
            /// Initializes a new instance of the <see cref="CaseInsensitiveIdentifier"/> class.
            /// </summary>
            /// <param name="name">The name.</param>
            public CaseInsensitiveIdentifier(string name)
            {
                _id = name;
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public override int GetHashCode()
            {
                return _id?.ToLowerInvariant().GetHashCode() ?? 0;
            }

            /// <summary>
            /// Indicates whether the current object is equal to another object of the same type.
            /// </summary>
            /// <param name="other">An object to compare with this object.</param>
            /// <returns>
            /// <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
            /// </returns>
            public override bool Equals(Identifier other)
            {
                if (other is CaseInsensitiveIdentifier cii)
                    return _id.ToLowerInvariant().Equals(cii._id.ToLowerInvariant());
                return false;
            }

            /// <summary>
            /// Clones this identifier.
            /// </summary>
            /// <returns>
            /// The cloned identifier.
            /// </returns>
            public override Identifier Clone() => new CaseInsensitiveIdentifier(_id);
        }

        [Test]
        public void When_CaseInsensitiveIdentifier_Expect_MatchedElement()
        {
            // Make sure strings are converted to the right type
            Identifier.SetImplicitStringIdentifierType(typeof(CaseInsensitiveIdentifier));
            var ckt = new Circuit(
                new Resistor("R1", "a", "b", 1.0e3),
                new Capacitor("C1", "b", "0", 1e-6)
                );

            Assert.DoesNotThrow(() =>
            {
                var tmp = ckt.Entities["r1"];
            });

            // Revert for other tests - will go back to StringIdentifier
            Identifier.SetImplicitStringIdentifierType(null);
        }

        [Test]
        public void When_CaseSensitiveIdentifier_Expect_Exception()
        {
            // In this case we didn't change the implicit string type
            var ckt = new Circuit(
                new Resistor("R1", "a", "b", 1.0e3),
                new Capacitor("C1", "b", "0", 1e-6)
            );

            Assert.Throws<KeyNotFoundException>(() =>
            {
                var tmp = ckt.Entities["r1"];
            });
        }
    }
}
