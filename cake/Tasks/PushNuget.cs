using Cake.Common;
using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build
{
    public sealed class PushNuget : FrostingTask<BuildContext>
    {
        public override void Run(BuildContext context)
        {
            foreach (var package in context.GetFiles(context.PackagePath.FullPath + "/*.nupkg"))
            {
                context.NuGetPush(package, new Cake.Common.Tools.NuGet.Push.NuGetPushSettings()
                {
                    ApiKey = context.EnvironmentVariable("NUGETORG_APIKEY"),
                    Source = "https://api.nuget.org/v3/index.json",
                    SkipDuplicate = true,
                });
            }
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.BuildSystem().IsRunningOnAzurePipelines
                && !string.IsNullOrEmpty(context.EnvironmentVariable("NUGETORG_APIKEY"))
                && context.IsPublicRelease;
        }
    }
}
