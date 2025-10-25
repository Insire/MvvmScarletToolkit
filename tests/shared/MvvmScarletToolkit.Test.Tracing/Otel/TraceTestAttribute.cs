using System.Diagnostics;
using System.Reflection;

namespace MvvmScarletToolkit.Mediator.Tests.Otel
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class TraceTestAttribute : BaseTraceTestAttribute
    {
        public bool TracePerTest { get; set; }

        private Activity? activityForThisTest;

        public TraceTestAttribute(bool tracePerTest = false)
        {
            TracePerTest = tracePerTest;
        }

        public override void Before(MethodInfo methodUnderTest, IXunitTest test)
        {
            if (TracePerTest || ActivityForTestRun == null)
            {
                var testRunActivityLink = ActivityForTestRun == null ?
                    null :
                    new List<ActivityLink> { new ActivityLink(ActivityForTestRun.Context) };

                activityForThisTest = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity(
                    methodUnderTest.Name,
                    ActivityKind.Internal,
                    new ActivityContext(), links: testRunActivityLink);
            }
            else
            {
                activityForThisTest = OpenTelemetryMonitoredFixture.ActivitySource.StartActivity(methodUnderTest.Name, ActivityKind.Internal, ActivityForTestRun.Context);
            }

            base.Before(methodUnderTest, test);
        }

        public override void After(MethodInfo methodUnderTest, IXunitTest test)
        {
            activityForThisTest?.Stop();

            base.After(methodUnderTest, test);
        }
    }
}
