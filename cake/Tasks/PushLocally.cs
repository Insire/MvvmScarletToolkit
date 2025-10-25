using Cake.Common;
using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.NuGet.Push;
using Cake.Common.Tools.DotNet.NuGet.Source;
using Cake.Frosting;

namespace Build.Tasks
{
    public sealed class PushLocally : FrostingTask<BuildContext>
    {
        private const string LocalNugetServer = @"http://localhost:5555/v3/index.json";

        public override void Run(BuildContext context)
        {
            if (!context.DotNetNuGetHasSource(LocalNugetServer))
            {
                context.DotNetNuGetAddSource("Local", new DotNetNuGetAddSourceSettings()
                {
                    Source = LocalNugetServer,
                    HandleExitCode = exitCode => true,
                });
            }

            foreach (var package in context.GetFiles(context.PackagePath.FullPath + "/*.nupkg"))
            {
                context.DotNetNuGetPush(package, new DotNetNuGetPushSettings()
                {
                    Source = LocalNugetServer,
                    ApiKey = context.EnvironmentVariable("LOCALNUGETSERVER_APIKEY")
                });
            }
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.BuildSystem().IsLocalBuild
                && !string.IsNullOrEmpty(context.EnvironmentVariable("LOCALNUGETSERVER_APIKEY"));
        }
    }
}
