using Cake.Codecov;
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Frosting;

[Dependency(typeof(CoberturaReport))]
public sealed class UploadCodecovReport : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        context.Codecov(new[] { context.CoberturaResultFile.FullPath }, context.EnvironmentVariable("CODECOV_TOKEN"));
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            && context.FileExists(context.CoberturaResultFile)
            && context.BuildSystem().IsRunningOnAzurePipelinesHosted
            && !string.IsNullOrEmpty(context.EnvironmentVariable("CODECOV_TOKEN"));
    }
}
