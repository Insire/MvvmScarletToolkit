using MvvmScarletToolkit.Wpf.Tests.Util;
using System.Windows.Controls;
using System.Windows.Input;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class SmallWpfTypesTests : TraceTestBase
    {
        [Fact]
        public Task BindingProxy_Should_Keep_Its_Data_When_Cloned()
        {
            return StaRunner.Run(() =>
            {
                var proxy = new BindingProxy { Data = "data" };

                Assert.Multiple(() =>
                {
                    Assert.Equal("data", proxy.Data);
                    Assert.Equal("data", ((BindingProxy)proxy.Clone()).Data);
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task FixedColumn_Should_Only_Apply_To_Columns_With_A_Width()
        {
            return StaRunner.Run(() =>
            {
                var plain = new GridViewColumn();
                var column = FixedColumn.ApplyWidth(new GridViewColumn(), 120);

                Assert.Multiple(() =>
                {
                    Assert.False(FixedColumn.IsFixedColumn(null));
                    Assert.False(FixedColumn.IsFixedColumn(plain));
                    Assert.Null(FixedColumn.GetFixedWidth(plain));

                    Assert.True(FixedColumn.IsFixedColumn(column));
                    Assert.Equal(120, FixedColumn.GetFixedWidth(column));
                    Assert.Equal(120, FixedColumn.GetWidth(column));
                });

                return Task.CompletedTask;
            });
        }

        [Fact]
        public void ToastServiceExtensions_Should_Show_A_Temporary_Toast_By_Default()
        {
            var service = Substitute.For<IToastService>();
            IToast? toast = null;
            service.When(p => p.Show(Arg.Any<IToast>())).Do(p => toast = p.Arg<IToast>());

            service.Show("title", "body", ToastType.Warning);

            var result = Assert.IsType<ToastViewModel>(toast);
            Assert.Multiple(() =>
            {
                Assert.Equal("title", result.Title);
                Assert.Equal("body", result.Body);
                Assert.Equal(ToastType.Warning, result.ToastType);
                Assert.False(result.IsPersistent);
            });
        }

        [Fact]
        public void ToastServiceExtensions_Should_Show_A_Persistent_Toast()
        {
            var service = Substitute.For<IToastService>();
            IToast? toast = null;
            service.When(p => p.Show(Arg.Any<IToast>())).Do(p => toast = p.Arg<IToast>());

            service.Show("title", "body", ToastType.Error, true);

            Assert.True(Assert.IsType<ToastViewModel>(toast).IsPersistent);
        }

        [Fact]
        public void StartProcessExtension_Should_Provide_The_Same_Command()
        {
            var extension = new StartProcessExtension();
            var serviceProvider = Substitute.For<IServiceProvider>();

            var command = extension.ProvideValue(serviceProvider);

            Assert.Multiple(() =>
            {
                Assert.IsAssignableFrom<ICommand>(command);
                Assert.Same(command, extension.ProvideValue(serviceProvider));
                Assert.Same(command, extension.Command);
            });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void StartProcessExtension_Should_Not_Start_Anything_Without_A_Url(string? url)
        {
            var extension = new StartProcessExtension { Url = url };
            var command = (ICommand)extension.ProvideValue(Substitute.For<IServiceProvider>());

            command.Execute(null);
        }
    }
}
