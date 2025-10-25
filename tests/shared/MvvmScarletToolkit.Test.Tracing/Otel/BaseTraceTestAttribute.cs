using MvvmScarletToolkit.Test.Tracing.Otel.Fixtures;
using System.Diagnostics;

namespace MvvmScarletToolkit.Mediator.Tests.Otel
{
    public abstract class BaseTraceTestAttribute : BeforeAfterTestAttribute
    {
        internal static Activity? ActivityForTestRun = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity("TestRun");
    }
}
