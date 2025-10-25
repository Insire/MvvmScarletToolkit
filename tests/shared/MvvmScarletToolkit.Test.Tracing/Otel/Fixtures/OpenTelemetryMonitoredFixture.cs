using MvvmScarletToolkit.Test.Tracing.Otel.Attributes;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Fixtures
{
    /// <summary>
    /// This class is created once per assembly
    /// </summary>
    public sealed class OpenTelemetryMonitoredFixture : OpenTelemetryFixtureBase, IDisposable
    {
        internal static ActivitySource ActivitySource { get; } = new ActivitySource(TracerName);

        private const string TracerName = "unittest-with-otel";
        private readonly TracerProvider _tracerProvider;

        public OpenTelemetryMonitoredFixture()
        {
            _tracerProvider = CreateTracerProvider(ResourceBuilder.CreateDefault().AddService("MvvmScarletToolkit"), TracerName);
        }

        public void Dispose()
        {
            TraceTestAttributeBase.ActivityForTestRun?.Stop();
            _tracerProvider?.ForceFlush();
            _tracerProvider?.Dispose();
        }
    }
}
