using MvvmScarletToolkit.Test.Tracing.Otel.Fixtures;
using System.Diagnostics;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Attributes
{
    public abstract class TraceTestAttributeBase : BeforeAfterTestAttribute
    {
        internal static Activity? ActivityForTestRun = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity("TestRun");
    }
}
