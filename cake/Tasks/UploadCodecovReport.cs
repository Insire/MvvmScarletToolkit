using Cake.Codecov;
using Cake.Common;
using Cake.Common.IO;
using Cake.Frosting;

namespace Build
{
    [IsDependentOn(typeof(CoberturaReport))]
    public sealed class UploadCodecovReport : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            var settings = new CodecovSettings()
            {
                Verbose = true,
                WorkingDirectory = context.CoberturaResultsPath,
                Files = new[] { context.CoberturaResultFile.FullPath },
                Token = context.EnvironmentVariable("CODECOV_TOKEN"),
            };

            context.Codecov(settings);
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.FileExists(context.CoberturaResultFile)
                && !string.IsNullOrEmpty(context.EnvironmentVariable("CODECOV_TOKEN"));
        }
    }
}
