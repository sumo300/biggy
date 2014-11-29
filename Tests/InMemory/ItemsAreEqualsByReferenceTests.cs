using System;
using System.Linq;
using Biggy.Core.InMemory;
using NUnit.Framework;

namespace Tests.InMemory
{
    [TestFixture]
    [Category("In memory")]
    public sealed class ItemsAreEqualsByReferenceTests
    {
        [Test]
        public void InMemory_Compare_The_Same_Instance()
        {
            // Given
            var firstItem = new InstrumentDocuments
            {
                Id = "1",
                Category = "Cat1",
                Type = "Alpha",
            };
            var locator = new ItemsAreEqualsByReference<InstrumentDocuments>(firstItem);
            var secondItem = firstItem;

            // When
            var result = locator.IsMatch(secondItem);

            // Then
            Assert.IsTrue(result);
        }
    }
}