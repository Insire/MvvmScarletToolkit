using Cake.Core;
using Cake.Frosting;

namespace Build.Tasks
{
    [IsDependentOn(typeof(PublishTestResults))]
    [IsDependentOn(typeof(HtmlReport))]
    [IsDependentOn(typeof(UploadCodecovReport))]
    public sealed class TestAndUploadReport : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            if (context.TestFailures.Count > 0)
            {
                throw new CakeException("Tests failed:" + Environment.NewLine + string.Join(Environment.NewLine, context.TestFailures));
            }
        }
    }
}
