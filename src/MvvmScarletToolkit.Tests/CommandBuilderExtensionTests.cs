using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    public sealed class CommandBuilderExtensionTests
    {
        private static ScarletCommandBuilder _builder;

        [OneTimeSetUp]
        public static void Setip()
        {
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Test]
        public void Create_DoesReturnNewInstanceEveryTime()
        {
            var context = _builder.Create(() => Task.CompletedTask);
            Assert.IsNotNull(context);

            var newContext = _builder.Create(() => Task.CompletedTask);
            Assert.IsNotNull(newContext);

            Assert.IsFalse(object.ReferenceEquals(context, newContext));
        }

        [Test]
        public void Create_DoesReturnValidContextInstance()
        {
            // default object overloads:

            var context = _builder.Create(() => Task.CompletedTask);
            Assert.IsNotNull(context);

            context = _builder.Create(() => Task.CompletedTask, () => true);
            Assert.IsNotNull(context);

            context = _builder.Create((CancellationToken token) => Task.CompletedTask);
            Assert.IsNotNull(context);

            context = _builder.Create((CancellationToken token) => Task.CompletedTask, () => true);
            Assert.IsNotNull(context);

            // generic overloads ignoring argument:

            context = _builder.Create<object>(() => Task.CompletedTask);
            Assert.IsNotNull(context);

            context = _builder.Create<object>(() => Task.CompletedTask, () => true);
            Assert.IsNotNull(context);

            context = _builder.Create<object>((CancellationToken token) => Task.CompletedTask);
            Assert.IsNotNull(context);

            context = _builder.Create<object>((CancellationToken token) => Task.CompletedTask, () => true);
            Assert.IsNotNull(context);

            // generic overloads with argument:

            context = _builder.Create<object>((object o) => Task.CompletedTask);
            Assert.IsNotNull(context);

            context = _builder.Create<object>((object o) => Task.CompletedTask, (object o) => true);
            Assert.IsNotNull(context);

            context = _builder.Create<object>((object o) => Task.CompletedTask, () => true);
            Assert.IsNotNull(context);

            context = _builder.Create<object>((object o, CancellationToken token) => Task.CompletedTask);
            Assert.IsNotNull(context);

            context = _builder.Create<object>((object o, CancellationToken token) => Task.CompletedTask, (object o) => true);
            Assert.IsNotNull(context);
        }
    }
}
