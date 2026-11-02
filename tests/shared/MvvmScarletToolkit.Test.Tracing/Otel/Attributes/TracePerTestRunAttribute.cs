using MvvmScarletToolkit.Test.Tracing.Otel.Fixtures;
using System.Diagnostics;
using System.Reflection;

namespace MvvmScarletToolkit.Test.Tracing.Otel.Attributes
{
    public sealed class TracePerTestRunAttribute : BeforeAfterTestAttribute
    {
        private Activity? _activityForThisTest;
        internal static Activity? ActivityForTestRun = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity("TestRun");

        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            if (ActivityForTestRun == null && OpenTelemetryFixtureBase.IsEnabled)
            {
                throw new ArgumentNullException(nameof(ActivityForTestRun), "The test run Activity was null, and therefore can't be used");
            }

            _activityForThisTest = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity(methodUnderTest.Name, ActivityKind.Internal, ActivityForTestRun?.Context ?? default);

            base.Before(methodUnderTest, test);
        }

        public override void After(MethodInfo methodUnderTest, IXunitTest test)
        {
            _activityForThisTest?.Stop();
            base.After(methodUnderTest, test);
        }
    }
}
