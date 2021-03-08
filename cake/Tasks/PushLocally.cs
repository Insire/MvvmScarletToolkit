using Cake.Common;
using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Linq;

public sealed class PushLocally : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        var settings = new ProcessSettings()
            .UseWorkingDirectory(".")
            .WithArguments(builder => builder
                .AppendSwitchQuoted("nuget add source", Context.LocalNugetDirectoryPath)
                .AppendSwitch("--name", "Local"));

        context.StartProcess(context.Tools.Resolve("dotnet.exe"), settings);

        foreach (var path in context.GetFiles(Context.PackagePath + "/*.nupkg"))
        {
            settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("push")
                    .AppendSwitchQuoted("-source", Context.LocalNugetDirectoryPath)
                    .AppendQuoted(path.FullPath));

            context.StartProcess(context.Tools.Resolve("nuget.exe"), settings);
        }
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            && context.BuildSystem().IsLocalBuild
            && context.DirectoryExists(Context.LocalNugetDirectoryPath);
    }
}
