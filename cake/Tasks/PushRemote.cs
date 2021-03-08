using Cake.Common;
using Cake.Common.Build;
using Cake.Common.IO;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using System.Linq;

public sealed class PushRemote : FrostingTask<Context>
{
    public override void Run(Context context)
    {
        foreach (var package in context.GetFiles(Context.PackagePath + "/*.nupkg"))
        {
            var settings = new ProcessSettings()
                .UseWorkingDirectory(".")
                .WithArguments(builder => builder
                    .Append("push")
                    .AppendQuoted(package.FullPath)
                    .AppendSwitchSecret("-apikey", context.EnvironmentVariable("NUGETORG_APIKEY"))
                    .AppendSwitchQuoted("-source", "https://api.nuget.org/v3/index.json")
                    .Append("-SkipDuplicate")
                    .AppendSwitch("-Verbosity", "detailed")
                );

            context.StartProcess(context.Tools.Resolve("nuget.exe"), settings);
        }
    }

    public override bool ShouldRun(Context context)
    {
        return base.ShouldRun(context)
            && context.BuildSystem().IsRunningOnAzurePipelines || context.BuildSystem().IsRunningOnAzurePipelinesHosted
            && !string.IsNullOrEmpty(context.EnvironmentVariable("NUGETORG_APIKEY"))
            && context.FileExists(context.Tools.Resolve("nuget.exe"));
    }
}
