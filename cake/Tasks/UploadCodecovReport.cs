using Cake.Codecov;
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Diagnostics;
using Cake.Common.IO;
using Cake.Core;
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
                Required = true,
                Verbose = true,
                WorkingDirectory = context.CoberturaResultsPath,
                DisableNetwork = true,
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
