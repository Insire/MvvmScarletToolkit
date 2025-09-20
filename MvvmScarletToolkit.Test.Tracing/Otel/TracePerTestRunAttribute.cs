using System.Diagnostics;
using System.Reflection;

namespace MvvmScarletToolkit.Mediator.Tests.Otel
{
    public sealed class TracePerTestRunAttribute : BeforeAfterTestAttribute
    {
        private Activity? activityForThisTest;
        internal static Activity? ActivityForTestRun = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity("TestRun");

        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            if (ActivityForTestRun == null)
            {
                throw new ArgumentNullException(nameof(ActivityForTestRun), "The test run Activity was null, and therefore can't be used");
            }

            activityForThisTest = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity(methodUnderTest.Name, ActivityKind.Internal, ActivityForTestRun.Context);

            base.Before(methodUnderTest, test);
        }

        public override void After(MethodInfo methodUnderTest, IXunitTest test)
        {
            activityForThisTest?.Stop();
            base.After(methodUnderTest, test);
        }
    }
}
