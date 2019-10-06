using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Bonus.CodeGen
{
    [GenerateEquatable]
    partial class Equatable
    {
        public int Number { get; set; }
        public string Text { get; set; }
        public IEnumerable<int> Numbers { get; set; }
    }

    public class EquatableGeneratorTest
    {
        [Fact]
        public void ImplementsIEquatable()
        {
            Assert.True(typeof(IEquatable<Equatable>).IsAssignableFrom(typeof(Equatable)));
        }

        [Fact]
        public void Test()
        {
            var target1 = new Equatable
            {
                Number = 128,
                Text = "2**7",
                Numbers = Enumerable.Range(0, 50)
            };
            var target2 = new Equatable
            {
                Number = 128,
                Text = "2**7",
                Numbers = Enumerable.Range(0, 50)
            };

            Assert.True(target1.Equals(target2));
        }
    }
}
