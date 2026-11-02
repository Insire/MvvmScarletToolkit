using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ValueConverterTests : TraceTestBase
    {
        private static readonly CultureInfo _culture = CultureInfo.InvariantCulture;

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
        [InlineData(true, "160", 160d)]
        [InlineData(true, 80d, 80d)]
        [InlineData(true, null, 0d)]
        [InlineData(false, "160", 0d)]
        [InlineData(null, "160", 0d)]
        public void BooleanToWidth_Should_Return_The_Parameter_When_True(object? value, object? parameter, double expected)
        {
            Assert.Equal(expected, new BooleanToWidth().Convert(value, typeof(double), parameter, _culture));
        }

        [Fact]
        public void BooleanToWidth_Should_Not_Convert_Back()
        {
            Assert.Same(Binding.DoNothing, new BooleanToWidth().ConvertBack(160d, typeof(bool), null, _culture));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("parameter")]
        public void DebugConverter_Should_Pass_Values_Through(object? parameter)
        {
            var converter = new DebugConverter();

            Assert.Multiple(() =>
            {
                Assert.Equal("value", converter.Convert("value", typeof(string), parameter, _culture));
                Assert.Equal("value", converter.ConvertBack("value", typeof(string), parameter, _culture));
            });
        }

        [Fact]
        public void IgnoreNullOrEmptyString_Should_Pass_Strings_Through()
        {
            Assert.Equal("value", new IgnoreNullOrEmptyString().Convert("value", typeof(string), null, _culture));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IgnoreNullOrEmptyString_Should_Filter_Null_And_Empty_Strings(string? value)
        {
            Assert.Same(DependencyProperty.UnsetValue, new IgnoreNullOrEmptyString().Convert(value, typeof(string), null, _culture));
        }

        [Fact]
        public void IgnoreNullOrEmptyString_Should_Filter_Anything_Else_Than_Strings()
        {
            Assert.Same(DependencyProperty.UnsetValue, new IgnoreNullOrEmptyString().Convert(1, typeof(string), null, _culture));
        }

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(true, false, false)]
        [InlineData(false, false, false)]
        public void MultiBooleanAndConverter_Should_Combine_All_Values(bool first, bool second, bool expected)
        {
            Assert.Equal(expected, new MultiBooleanAndConverter().Convert([first, second], typeof(bool), null!, _culture));
        }

        [Fact]
        public void MultiBooleanAndConverter_Should_Be_True_Without_Values()
        {
            Assert.Equal(true, new MultiBooleanAndConverter().Convert([], typeof(bool), null!, _culture));
        }

        [Fact]
        public void MultiBooleanAndConverter_Should_Be_False_When_A_Value_Is_Not_A_Boolean()
        {
            // bindings pass DependencyProperty.UnsetValue until they are resolved
            Assert.Equal(false, new MultiBooleanAndConverter().Convert([true, DependencyProperty.UnsetValue], typeof(bool), null!, _culture));
        }

        [Fact]
        public void MultiBooleanAndConverter_Should_Not_Convert_Back()
        {
            Assert.Equal([Binding.DoNothing], new MultiBooleanAndConverter().ConvertBack(true, [typeof(bool)], null!, _culture));
        }

        [Theory]
        [InlineData(0L, "0 B")]
        [InlineData(1023L, "1,023 B")]
        [InlineData(1024L, "1.00 KB")]
        [InlineData(1536L, "1.50 KB")]
        [InlineData(10240L, "10.0 KB")]
        [InlineData(1048576L, "1.00 MB")]
        [InlineData(1073741824L, "1.00 GB")]
        [InlineData(1099511627776L, "1.00 TB")]
        [InlineData(1125899906842624L, "1.00 PB")]
        [InlineData(1152921504606846976L, "1,024.0 PB")]
        public void ToFileSize_Should_Use_The_Largest_Fitting_Unit(long bytes, string expected)
        {
            WithCulture("en-US", () => Assert.Equal(expected, new ToFileSize().Convert(bytes, typeof(string), null, _culture)));
        }

        [Fact]
        public void ToFileSize_Should_Format_With_The_Current_Culture()
        {
            WithCulture("de-DE", () => Assert.Equal("1,50 KB", new ToFileSize().Convert(1536L, typeof(string), null, _culture)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("1024")]
        [InlineData(1024)]
        public void ToFileSize_Should_Be_Empty_For_Anything_Else_Than_Longs(object? value)
        {
            Assert.Equal(string.Empty, new ToFileSize().Convert(value, typeof(string), null, _culture));
        }

        [Fact]
        public void ToLocalTime_Should_Format_The_Local_Time_With_The_Current_Culture()
        {
            var time = new DateTime(2026, 11, 2, 12, 30, 0, DateTimeKind.Utc);

            WithCulture("de-DE", () => Assert.Equal(time.ToLocalTime().ToString("g", new CultureInfo("de-DE")), new ToLocalTime().Convert(time, typeof(string), null, _culture)));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("2026-11-02")]
        public void ToLocalTime_Should_Be_Empty_For_Anything_Else_Than_DateTimes(object? value)
        {
            Assert.Equal(string.Empty, new ToLocalTime().Convert(value, typeof(string), null, _culture));
        }
    }
}
