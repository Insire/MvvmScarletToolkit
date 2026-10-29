using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class ConverterMarkupExtensionTests : TraceTestBase
    {
        private static readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();

        [Fact]
        public void Converters_Without_Settings_Should_Share_One_Instance_Of_Their_Own_Type()
        {
            var first = new BooleanToVisibility().ProvideValue(_serviceProvider);
            var second = new BooleanToVisibility().ProvideValue(_serviceProvider);

            Assert.Multiple(() =>
            {
                Assert.IsType<BooleanToVisibility>(first);
                Assert.Same(first, second);
                Assert.IsType<InvertBooleanToVisibility>(new InvertBooleanToVisibility().ProvideValue(_serviceProvider));
            });
        }

        [Theory]
        [MemberData(nameof(ConvertersWithSettings))]
        public void Converters_With_Settings_Should_Provide_Themselves(MarkupExtension extension)
        {
            Assert.Same(extension, extension.ProvideValue(_serviceProvider));
        }

        public static TheoryData<MarkupExtension> ConvertersWithSettings => [new Flatten(), new ToCase()];

        [Fact]
        public void BooleanToVisibility_Should_Not_Be_Inverted()
        {
            var converter = (IValueConverter)new BooleanToVisibility().ProvideValue(_serviceProvider);

            Assert.Multiple(() =>
            {
                Assert.Equal(Visibility.Visible, converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture));
                Assert.Equal(Visibility.Hidden, converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture));
                Assert.Equal(Visibility.Hidden, converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void InvertBooleanToVisibility_Should_Be_Inverted()
        {
            var converter = (IValueConverter)new InvertBooleanToVisibility().ProvideValue(_serviceProvider);

            Assert.Multiple(() =>
            {
                Assert.Equal(Visibility.Hidden, converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture));
                Assert.Equal(Visibility.Visible, converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void Flatten_Should_Keep_Separator_Set_On_The_Extension()
        {
            var converter = (IValueConverter)new Flatten { Separator = "; " }.ProvideValue(_serviceProvider);

            Assert.Equal("a; b", converter.Convert(new[] { "a", " ", "b" }, typeof(string), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void Flatten_Instances_Should_Not_Share_Settings()
        {
            var first = (IValueConverter)new Flatten { Separator = "|" }.ProvideValue(_serviceProvider);
            var second = (IValueConverter)new Flatten().ProvideValue(_serviceProvider);

            Assert.Multiple(() =>
            {
                Assert.Equal("a|b", first.Convert(new[] { "a", "b" }, typeof(string), null, CultureInfo.InvariantCulture));
                Assert.Equal("a, b", second.Convert(new[] { "a", "b" }, typeof(string), null, CultureInfo.InvariantCulture));
            });
        }

        [Fact]
        public void ToCase_Should_Keep_Casing_Set_On_The_Extension()
        {
            var converter = (IValueConverter)new ToCase { Casing = CharacterCasing.Lower }.ProvideValue(_serviceProvider);

            Assert.Equal("mvvm", converter.Convert("MvVm", typeof(string), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void MultiConverterMarkupExtension_Should_Share_One_Instance()
        {
            var first = new MultiBooleanAndConverter().ProvideValue(_serviceProvider);

            Assert.IsType<MultiBooleanAndConverter>(first);
            Assert.Same(first, new MultiBooleanAndConverter().ProvideValue(_serviceProvider));
        }

        [Fact]
        public void RadioButtonCheckedConverter_ConvertBack_Should_Return_Parameter_When_Checked()
        {
            var converter = new RadioButtonCheckedConverter();

            Assert.Equal(DayOfWeek.Friday, converter.ConvertBack(true, typeof(DayOfWeek), DayOfWeek.Friday, CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(null)]
        public void RadioButtonCheckedConverter_ConvertBack_Should_Do_Nothing_When_Not_Checked(bool? value)
        {
            var converter = new RadioButtonCheckedConverter();

            Assert.Same(Binding.DoNothing, converter.ConvertBack(value, typeof(DayOfWeek), DayOfWeek.Friday, CultureInfo.InvariantCulture));
        }

        [Theory]
        [InlineData(typeof(IsNull))]
        [InlineData(typeof(IsNotNull))]
        public void Null_Checks_Should_Declare_Object_To_Bool_Conversion(Type converterType)
        {
            var attribute = (ValueConversionAttribute)Attribute.GetCustomAttribute(converterType, typeof(ValueConversionAttribute))!;

            Assert.Multiple(() =>
            {
                Assert.Equal(typeof(object), attribute.SourceType);
                Assert.Equal(typeof(bool), attribute.TargetType);
            });
        }

        [Fact]
        public Task Converters_Used_In_Xaml_Should_Keep_Their_Properties()
        {
            return StaRunner.Run(async () =>
            {
                const string xaml = """
                    <StackPanel xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                                xmlns:mvvm="http://SoftThorn.MvvmScarletToolkit.com/winfx/xaml/shared">
                        <TextBlock Text="{Binding Names, Converter={mvvm:Flatten Separator='; '}}" />
                        <TextBlock Text="{Binding Names, Converter={mvvm:Flatten}}" />
                        <TextBlock Visibility="{Binding IsVisible, Converter={mvvm:BooleanToVisibility}}" />
                    </StackPanel>
                    """;

                var panel = (StackPanel)XamlReader.Parse(xaml);
                panel.DataContext = new XamlViewModel();

                // bindings pick up the new DataContext on the dispatcher
                await Ui.Settle();

                var custom = (TextBlock)panel.Children[0];
                var defaults = (TextBlock)panel.Children[1];
                var visibility = (TextBlock)panel.Children[2];

                Assert.Multiple(() =>
                {
                    Assert.Equal("a; b", custom.Text);
                    Assert.Equal("a, b", defaults.Text);
                    Assert.Equal(Visibility.Visible, visibility.Visibility);
                });
            });
        }

        public sealed class XamlViewModel
        {
            public string[] Names { get; } = ["a", "b"];

            public bool IsVisible => true;
        }
    }
}
