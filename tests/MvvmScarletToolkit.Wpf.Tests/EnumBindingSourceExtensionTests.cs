using System.ComponentModel;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class EnumBindingSourceExtensionTests : TraceTestBase
    {
        public enum Sample
        {
            Visible,

            [Browsable(false)]
            NotBrowsable,

            [EditorBrowsable(EditorBrowsableState.Never)]
            NeverEditorBrowsable,

            [Browsable(true)]
            [EditorBrowsable(EditorBrowsableState.Always)]
            ExplicitlyBrowsable,
        }

        private static Array Provide(Type type, bool filterNonBrowseables = false)
        {
            return (Array)new EnumBindingSourceExtension(type, filterNonBrowseables).ProvideValue(Substitute.For<IServiceProvider>());
        }

        [Fact]
        public void Should_Return_All_Values()
        {
            var values = Provide(typeof(Sample));

            Assert.IsType<Sample[]>(values);
            Assert.Equal([Sample.Visible, Sample.NotBrowsable, Sample.NeverEditorBrowsable, Sample.ExplicitlyBrowsable], values.Cast<Sample>());
        }

        [Fact]
        public void Should_Return_Leading_Null_For_Nullable_Enum()
        {
            var values = Provide(typeof(Sample?));

            Assert.IsType<Sample?[]>(values);
            Assert.Equal([null, Sample.Visible, Sample.NotBrowsable, Sample.NeverEditorBrowsable, Sample.ExplicitlyBrowsable], values.Cast<Sample?>());
        }

        [Fact]
        public void Should_Filter_Values_That_Are_Not_Browsable()
        {
            var values = Provide(typeof(Sample), filterNonBrowseables: true);

            Assert.IsType<Sample[]>(values);
            Assert.Equal([Sample.Visible, Sample.ExplicitlyBrowsable], values.Cast<Sample>());
        }

        [Fact]
        public void Should_Filter_Values_That_Are_Not_Browsable_For_Nullable_Enum()
        {
            var values = Provide(typeof(Sample?), filterNonBrowseables: true);

            Assert.Equal([null, Sample.Visible, Sample.ExplicitlyBrowsable], values.Cast<Sample?>());
        }

        [Fact]
        public void Should_Reject_Types_That_Are_No_Enums()
        {
            Assert.Throws<ArgumentException>(() => new EnumBindingSourceExtension(typeof(int)));
        }
    }
}
