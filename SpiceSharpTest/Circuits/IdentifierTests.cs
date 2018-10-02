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
        private class CaseInsensitiveComparer : IEqualityComparer<Identifier>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
            /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
            /// <returns>
            ///   <see langword="true" /> if the specified objects are equal; otherwise, <see langword="false" />.
            /// </returns>
            public bool Equals(Identifier x, Identifier y)
            {
                // Do case insensitive equality checking on the identifiers
                return StringComparer.OrdinalIgnoreCase.Equals(x?.ToString(), y?.ToString());
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            public int GetHashCode(Identifier obj)
            {
                // Make sure the hash code is case insensitive
                return obj.ToString().ToLowerInvariant().GetHashCode();
            }
        }


        [Test]
        public void When_CaseInsensitiveIdentifier_Expect_MatchedElement()
        {
            var ckt = new Circuit(new CaseInsensitiveComparer());
            ckt.Entities.Add(
                new Resistor("R1", "a", "b", 1.0e3),
                new Capacitor("C1", "b", "0", 1e-6)
                );

            Assert.AreNotEqual(null, ckt.Entities["r1"]);
        }
    }
}
