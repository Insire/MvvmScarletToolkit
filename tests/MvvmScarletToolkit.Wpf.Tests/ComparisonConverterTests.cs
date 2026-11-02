using System.Globalization;
using System.Windows;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ComparisonConverterTests : TraceTestBase
    {
        private static readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

        private static void WithCulture(string name, Action action)
        {
            var current = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(name);
                action();
            }
            finally
            {
                CultureInfo.CurrentCulture = current;
            }
        }

        [Theory]
        [InlineData((sbyte)6)]
        [InlineData((byte)6)]
        [InlineData((short)6)]
        [InlineData((ushort)6)]
        [InlineData(6)]
        [InlineData(6u)]
        [InlineData(6L)]
        [InlineData(6ul)]
        [InlineData(6f)]
        [InlineData(6d)]
        public void GreaterThan_Should_Compare_All_Numeric_Types(object value)
        {
            var converter = new GreaterThan(5);

            Assert.Multiple(() =>
            {
                Assert.Equal(true, converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
                Assert.Equal(false, new GreaterThan(6).Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
            });
        }

        [Theory]
        [InlineData((sbyte)4)]
        [InlineData((byte)4)]
        [InlineData((short)4)]
        [InlineData((ushort)4)]
        [InlineData(4)]
        [InlineData(4u)]
        [InlineData(4L)]
        [InlineData(4ul)]
        [InlineData(4f)]
        [InlineData(4d)]
        public void SmallerThan_Should_Compare_All_Numeric_Types(object value)
        {
            var converter = new SmallerThan(5);

            Assert.Multiple(() =>
            {
                Assert.Equal(true, converter.Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
                Assert.Equal(false, new SmallerThan(4).Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Compare_Decimals()
        {
            Assert.Multiple(() =>
            {
                Assert.Equal(true, new GreaterThan(5).Convert(5.5m, typeof(bool), null, CultureInfo.InvariantCulture));
                Assert.Equal(true, new SmallerThan(5).Convert(4.5m, typeof(bool), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Compare_Against_Zero_By_Default()
        {
            Assert.Multiple(() =>
            {
                Assert.Equal(true, new GreaterThan().Convert(1, typeof(bool), null, CultureInfo.InvariantCulture));
                Assert.Equal(true, new SmallerThan().Convert(-1, typeof(bool), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Prefer_A_Double_Parameter_Over_The_Value()
        {
            Assert.Multiple(() =>
            {
                Assert.Equal(false, new GreaterThan(5).Convert(6, typeof(bool), 10d, CultureInfo.InvariantCulture));
                Assert.Equal(false, new SmallerThan(5).Convert(4, typeof(bool), 1d, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Parse_A_String_Parameter()
        {
            Assert.Multiple(() =>
            {
                Assert.Equal(false, new GreaterThan(5).Convert(6, typeof(bool), "10", CultureInfo.InvariantCulture));
                Assert.Equal(false, new SmallerThan(5).Convert(4, typeof(bool), "1", CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Ignore_A_String_Parameter_That_Is_Not_A_Number()
        {
            Assert.Multiple(() =>
            {
                Assert.Equal(true, new GreaterThan(5).Convert(6, typeof(bool), "many", CultureInfo.InvariantCulture));
                Assert.Equal(true, new SmallerThan(5).Convert(4, typeof(bool), "few", CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Parse_ConverterParameters_Independent_Of_The_Culture()
        {
            // a ConverterParameter is written in xaml and doesn't depend on the machine. In de-DE the dot is a group separator, so "1.5" would be 15
            WithCulture("de-DE", () => Assert.Multiple(() =>
            {
                Assert.Equal(true, new GreaterThan().Convert(2d, typeof(bool), "1.5", CultureInfo.InvariantCulture));
                Assert.Equal(false, new SmallerThan().Convert(2d, typeof(bool), "1.5", CultureInfo.InvariantCulture));
            }));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("6")]
        [InlineData(true)]
        public void Converters_Should_Return_False_For_Anything_Else_Than_Numbers(object? value)
        {
            Assert.Multiple(() =>
            {
                Assert.Equal(false, new GreaterThan(5).Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
                Assert.Equal(false, new SmallerThan(5).Convert(value, typeof(bool), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Not_Convert_Back()
        {
            Assert.Multiple(() =>
            {
                Assert.Same(DependencyProperty.UnsetValue, new GreaterThan().ConvertBack(true, typeof(double), null, CultureInfo.InvariantCulture));
                Assert.Same(DependencyProperty.UnsetValue, new SmallerThan().ConvertBack(true, typeof(double), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Converters_Should_Provide_Themselves_To_Keep_Their_Value()
        {
            var greaterThan = new GreaterThan(1);
            var smallerThan = new SmallerThan(1);

            Assert.Multiple(() =>
            {
                Assert.Same(greaterThan, greaterThan.ProvideValue(_serviceProvider));
                Assert.Same(smallerThan, smallerThan.ProvideValue(_serviceProvider));
            });
        }
    }
}
