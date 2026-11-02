using OpenTelemetry;
using System.Diagnostics;

namespace MvvmScarletToolkit.Test.Tracing.Otel
{
    public sealed class TestRunSpanProcessor : BaseProcessor<Activity>
    {
        private readonly string _testRunId;

        public TestRunSpanProcessor(string testRunId)
        {
            _testRunId = testRunId;
        }

        public override void OnStart(Activity? data)
        {
            data?.SetTag("test.run_id", _testRunId);
        }
    }
}
