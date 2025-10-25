using MvvmScarletToolkit.Mediator.Tests.Otel;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Fixtures
{
    public sealed class OpenTelemetryMonitoredFixture : OpenTelemetryFixtureBase, IDisposable
    {
        public static ActivitySource ActivitySource { get; } = new ActivitySource(TracerName);

        private const string TracerName = "unittest-with-otel";
        private readonly TracerProvider _tracerProvider;

        public OpenTelemetryMonitoredFixture()
        {
            _tracerProvider = CreateTracerProvider(ResourceBuilder.CreateDefault().AddService("MvvmScarletToolkit"), TracerName);
        }

        public void Dispose()
        {
            BaseTraceTestAttribute.ActivityForTestRun?.Stop();
            _tracerProvider?.ForceFlush();
            _tracerProvider?.Dispose();
        }
    }
}
