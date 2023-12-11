using NUnit.Framework;
using System.Threading;
using System.Threading.Tasks;

namespace MvvmScarletToolkit.Tests
{
    public sealed class CommandBuilderExtensionTests
    {
        private static ScarletCommandBuilder _builder;

        [OneTimeSetUp]
        public static void Setup()
        {
            _builder = new ScarletCommandBuilder(Utils.GetTestDispatcher(), Utils.GetTestCommandManager(), Utils.GetTestExceptionHandler(), Utils.GetTestMessenger(), Utils.GetTestExitService(), Utils.GetTestEventManager(), Utils.TestBusyStackFactory);
        }

        [Test]
        public void Create_DoesReturnNewInstanceEveryTime()
        {
            var context = _builder.Create(() => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            var newContext = _builder.Create(() => Task.CompletedTask);
            Assert.Multiple(() =>
            {
                Assert.That(newContext, Is.Not.Null);

                Assert.That(ReferenceEquals(context, newContext), Is.False);
            });
        }

        [Test]
        public void Create_DoesReturnValidContextInstance()
        {
            // default object overloads:

            var context = _builder.Create(() => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create(() => Task.CompletedTask, () => true);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create((CancellationToken _) => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create((CancellationToken _) => Task.CompletedTask, () => true);
            Assert.That(context, Is.Not.Null);

            // generic overloads ignoring argument:

            context = _builder.Create<object>(() => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create<object>(() => Task.CompletedTask, () => true);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create<object>((CancellationToken _) => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create<object>((CancellationToken _) => Task.CompletedTask, () => true);
            Assert.That(context, Is.Not.Null);

            // generic overloads with argument:

            context = _builder.Create((object _) => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create((object _) => Task.CompletedTask, (object _) => true);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create((object _) => Task.CompletedTask, () => true);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create((object _, CancellationToken __) => Task.CompletedTask);
            Assert.That(context, Is.Not.Null);

            context = _builder.Create((object _, CancellationToken __) => Task.CompletedTask, (object _) => true);
            Assert.That(context, Is.Not.Null);
        }
    }
}
