using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

public sealed class PushLocally : FrostingTask<BuildContext>
{
    public override void Run(BuildContext context)
    {
        if (!context.NuGetHasSource(BuildContext.LocalNugetDirectoryPath))
        {
            context.NuGetAddSource("Local", BuildContext.LocalNugetDirectoryPath);
        }

        foreach (var package in context.GetFiles(BuildContext.PackagePath + "/*.nupkg"))
        {
            context.NuGetPush(package, new Cake.Common.Tools.NuGet.Push.NuGetPushSettings()
            {
                Source = BuildContext.LocalNugetDirectoryPath,
                SkipDuplicate = true,
            });
        }
    }

    public override bool ShouldRun(BuildContext context)
    {
        return base.ShouldRun(context)
            && context.BuildSystem().IsLocalBuild
            && context.DirectoryExists(BuildContext.LocalNugetDirectoryPath);
    }
}
