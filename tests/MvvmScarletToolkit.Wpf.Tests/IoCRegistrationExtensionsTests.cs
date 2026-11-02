using MvvmScarletToolkit.Wpf.Tests.Util;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace MvvmScarletToolkit.Wpf.Tests
{
    public sealed class IoCRegistrationExtensionsTests : TraceTestBase
    {
        private static FrameworkElement CreateControl(IContainer container)
        {
            var control = new Border();
            control.Resources.Register(container);

            return control;
        }

        private static IContainer CreateProvider(object? service)
        {
            var container = Substitute.For<IContainer, IServiceProvider>();
            ((IServiceProvider)container).GetService(typeof(string)).Returns(service);

            return container;
        }

        [Fact]
        public Task GetService_Should_Resolve_From_The_Registered_Container()
        {
            return StaRunner.Run(() =>
            {
                Assert.Equal("service", CreateControl(CreateProvider("service")).GetService<string>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task GetService_Should_Throw_Without_A_Registration_For_The_Service()
        {
            return StaRunner.Run(() =>
            {
                var control = CreateControl(CreateProvider(null));

                Assert.Throws<InvalidOperationException>(() => control.GetService<string>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task GetService_Should_Throw_When_The_Provider_Returns_Another_Type()
        {
            return StaRunner.Run(() =>
            {
                var control = CreateControl(CreateProvider(1));

                Assert.Throws<InvalidOperationException>(() => control.GetService<string>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task GetService_Should_Throw_When_The_Container_Is_No_ServiceProvider()
        {
            return StaRunner.Run(() =>
            {
                var control = CreateControl(Substitute.For<IContainer>());

                Assert.Throws<InvalidOperationException>(() => control.GetService<string>());

                return Task.CompletedTask;
            });
        }

        [Fact]
        public Task GetService_Should_Find_The_Container_Of_A_Parent()
        {
            return StaRunner.Run(() =>
            {
                var parent = new Border();
                parent.Resources.Register(CreateProvider("service"));
                var child = new TextBlock();
                parent.Child = child;

                Assert.Equal("service", child.GetService<string>());

                return Task.CompletedTask;
            });
        }
    }
}
