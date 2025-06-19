using Cake.Common;
using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build.Tasks
{
    public sealed class PushGithub : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            foreach (var package in context.GetFiles(context.PackagePath.FullPath + "/*.nupkg"))
            {
                var settings = new ProcessSettings()
                    .UseWorkingDirectory(".")
                    .WithArguments(builder => builder
                        .Append("nuget push")
                        .AppendQuoted(package.FullPath)

                        .AppendSwitchQuotedSecret("--api-key", context.EnvironmentVariable("GITHUB_APIKEY"))
                        .AppendSwitch("--source", "https://nuget.pkg.github.com/insire/index.json")
                        .Append("--skip-duplicate")
                    );

                context.StartProcess("dotnet", settings);
            }
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.BuildSystem().IsRunningOnAzurePipelines
                && !string.IsNullOrEmpty(context.EnvironmentVariable("GITHUB_APIKEY"))
                && !context.IsPublicRelease;
        }
    }
}
