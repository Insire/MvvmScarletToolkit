using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build
{
    public sealed class PushLocally : FrostingTask<BuildContext>
    {
        private const string LocalNugetFolder = @"D:\Drop\NuGet";

        public override void Run(BuildContext context)
        {
            if (!context.NuGetHasSource(LocalNugetFolder))
            {
                context.NuGetAddSource("Local", LocalNugetFolder);
            }

            foreach (var package in context.GetFiles(context.PackagePath.FullPath + "/*.nupkg"))
            {
                context.NuGetPush(package, new Cake.Common.Tools.NuGet.Push.NuGetPushSettings()
                {
                    Source = LocalNugetFolder,
                });
            }
        }

        public override bool ShouldRun(BuildContext context)
        {
            return base.ShouldRun(context)
                && context.BuildSystem().IsLocalBuild
                && context.DirectoryExists(LocalNugetFolder);
        }
    }
}
