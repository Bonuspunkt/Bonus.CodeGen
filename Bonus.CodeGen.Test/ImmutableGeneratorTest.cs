using Xunit;

namespace Bonus.CodeGen
{
    [GenerateImmutable]
    public partial class Generated
    {
        public int Number { get; }
        public string Text { get; }
    }

    public partial class ImmutableGeneratorTest
    {
        [Fact]
        public void VerifyGenerated()
        {
            var target = Generated.Create();
            Assert.Equal(default, target.Number);
            Assert.Equal(default, target.Text);

            var newTarget = target.With(number: 7, text: "test");
            Assert.Equal(7, newTarget.Number);
            Assert.Equal("test", newTarget.Text);
        }
    }


    [GenerateImmutable]
    public partial class GeneratedWithDefault
    {
        internal const int No = 5;
        internal const string Txt = "oh hi";

        public static readonly GeneratedWithDefault New = new GeneratedWithDefault(number: No, text: Txt);

        public int Number { get; }
        public string Text { get; }
    }

    partial class ImmutableGeneratorTest
    {
        [Fact]
        public void VerifyGeneratedWithDefault()
        {
            var target = GeneratedWithDefault.Create();
            Assert.Equal(GeneratedWithDefault.No, target.Number);
            Assert.Equal(GeneratedWithDefault.Txt, target.Text);
        }
    }
}
