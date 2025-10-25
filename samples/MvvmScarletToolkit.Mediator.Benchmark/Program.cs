using BenchmarkDotNet.Running;

namespace MvvmScarletToolkit.Mediator.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
            => BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
