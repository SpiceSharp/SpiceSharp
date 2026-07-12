using NUnit.Framework;
using SpiceSharp.Components;
using SpiceSharp.Entities;
using System.Collections.Generic;

namespace SpiceSharpTest.Circuits
{
    [TestFixture]
    public class ConcurrentEntityCollectionTests
    {
        [Test]
        public void When_CopyingAndCloning_Expect_AllEntities()
        {
            var collection = new ConcurrentEntityCollection
            {
                new Resistor("R1", "in", "0", 1.0),
                new Resistor("R2", "out", "0", 2.0)
            };
            var entities = new IEntity[2];

            ((ICollection<IEntity>)collection).CopyTo(entities, 0);
            var clone = collection.Clone();

            Assert.That(entities, Has.None.Null);
            Assert.That(clone.Count, Is.EqualTo(2));
            Assert.That(clone.Contains("R1"), Is.True);
            Assert.That(clone.Contains("R2"), Is.True);
        }

        [Test]
        public void When_RemovalHandlerReadsCollection_Expect_NoLockRecursion()
        {
            var collection = new ConcurrentEntityCollection
            {
                new Resistor("R1", "in", "0", 1.0)
            };
            var observedCount = -1;
            var observedContains = true;
            collection.EntityRemoved += (_, args) =>
            {
                observedCount = collection.Count;
                observedContains = collection.Contains(args.Entity.Name);
            };

            Assert.That(collection.Remove("R1"), Is.True);
            Assert.That(observedCount, Is.Zero);
            Assert.That(observedContains, Is.False);
        }
    }
}