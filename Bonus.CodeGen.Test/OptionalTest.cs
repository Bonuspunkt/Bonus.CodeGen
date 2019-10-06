using Xunit;

namespace Bonus.CodeGen
{
    public class OptionalTest
    {
        [Fact]
        public void Set()
        {
            var optional  = new Optional<int>(1);
            Assert.Equal(1, optional.Value);
            Assert.True(optional .IsSet);
        }

        [Fact]
        public void Unset()
        {
            var optional = new Optional<int>();
            Assert.Equal(default, optional.Value);
            Assert.False(optional.IsSet);
        }
    }
}