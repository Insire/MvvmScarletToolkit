using System.ComponentModel;

namespace MvvmScarletToolkit.Tests
{
    public sealed class EnumExtensionsTests : TraceTestBase
    {
        private enum Sample
        {
            [Description("first")]
            WithAttribute,
            WithoutAttribute,
        }

        [Fact]
        public void GetAttributeOfType_Should_Return_The_Attribute_Of_The_Value()
        {
            Assert.Equal("first", Sample.WithAttribute.GetAttributeOfType<DescriptionAttribute>()?.Description);
        }

        [Fact]
        public void GetAttributeOfType_Should_Return_Null_Without_The_Attribute()
        {
            Assert.Null(Sample.WithoutAttribute.GetAttributeOfType<DescriptionAttribute>());
        }

        [Fact]
        public void GetAttributeOfType_Should_Return_Null_For_An_Undefined_Value()
        {
            Assert.Null(((Sample)99).GetAttributeOfType<DescriptionAttribute>());
        }

        [Fact]
        public void GetAttributeOfType_With_Enum_Type_Should_Return_The_Attribute_Of_The_Value()
        {
            Assert.Equal("first", Sample.WithAttribute.GetAttributeOfType<Sample, DescriptionAttribute>()?.Description);
        }

        [Fact]
        public void GetAttributeOfType_With_Enum_Type_Should_Return_Null_Without_The_Attribute()
        {
            Assert.Null(Sample.WithoutAttribute.GetAttributeOfType<Sample, DescriptionAttribute>());
        }

        [Fact]
        public void GetAttributeOfType_With_Enum_Type_Should_Return_Null_For_An_Undefined_Value()
        {
            Assert.Null(((Sample)99).GetAttributeOfType<Sample, DescriptionAttribute>());
        }
    }
}
