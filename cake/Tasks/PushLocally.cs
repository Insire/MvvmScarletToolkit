using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Common.Tools.NuGet;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

public sealed class PushLocally : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        if (!context.NuGetHasSource(Context.LocalNugetDirectoryPath))
        {
            context.NuGetAddSource("Local", Context.LocalNugetDirectoryPath);
        }

        foreach (var package in context.GetFiles(Context.PackagePath + "/*.nupkg"))
        {
            context.NuGetPush(package, new Cake.Common.Tools.NuGet.Push.NuGetPushSettings()
            {
                Source = Context.LocalNugetDirectoryPath,
                SkipDuplicate = true,
            });
        }
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            && context.BuildSystem().IsLocalBuild
            && context.DirectoryExists(Context.LocalNugetDirectoryPath);
    }
}
