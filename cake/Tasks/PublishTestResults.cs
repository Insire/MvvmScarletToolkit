using Cake.Common.Build;
using Cake.Common.Build.AzurePipelines.Data;
using Cake.Common.IO;
using Cake.Frosting;

namespace Build.Tasks
{
    [IsDependentOn(typeof(Test))]
    public sealed class PublishTestResults : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var data = new AzurePipelinesPublishTestResultsData()
            {
                TestRunner = AzurePipelinesTestRunnerType.VSTest,
                TestResultsFiles = [.. context.GetFiles($"{context.TestResultsPath.FullPath}/**/*.trx")],
                MergeTestResults = true,
                Configuration = BuildContext.BuildConfiguration,
                TestRunTitle = $"Tests ({context.Environment.Platform.Family})",
            };

            context.AzurePipelines().Commands.PublishTestResults(data);
        }

        public override bool ShouldRun(BuildContext context)
        {
            return context.BuildSystem().IsRunningOnAzurePipelines
                && context.GetFiles($"{context.TestResultsPath.FullPath}/**/*.trx").Count > 0;
        }
    }
}
